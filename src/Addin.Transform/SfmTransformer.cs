using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mono.Addins;
using Palaso.Progress;
using Palaso.UI.WindowsForms.i8n;
using WeSay.AddinLib;
using WeSay.Foundation;

namespace Addin.Transform
{
	[Extension]
	public class SfmTransformer : LiftTransformer, IWeSayAddinHasSettings
	{
		private SfmTransformSettings _settings;

		public SfmTransformer()
		{
			_settings = new SfmTransformSettings();
		}

		public override string Name
		{
			get
			{
				return StringCatalog.Get("~Export To SFM");
			}
		}

		public override  string ShortDescription
		{
			get
			{
				return StringCatalog.Get("~Saves the lexicon in a form of standard format.");
			}
		}

		public override Image ButtonImage
		{
			get
			{
				return Resources.SfmTransformerButtonImage;
			}
		}

		/// <summary>
		/// this runs in a worker thread
		/// </summary>
		private static void OnDoGrepWork(object sender, DoWorkEventArgs args)
		{
			ProgressState progressState = (ProgressState)args.Argument;
			TransformWorkerArguments workerArguments = (TransformWorkerArguments)(progressState.Arguments);

			progressState.StatusLabel = "Converting to MDF...";
			progressState.NumberOfStepsCompleted++;
			//System.Threading.Thread.Sleep(100);//don't event see that message otherwise
			GrepFile(workerArguments.outputFilePath, args);
		}
		private static void GrepFile(string inputPath, DoWorkEventArgs args)
		{
			ProgressState progressState = (ProgressState)args.Argument;
			TransformWorkerArguments workerArguments = (TransformWorkerArguments)(progressState.Arguments);
			SfmTransformSettings sfmSettings = (SfmTransformSettings) workerArguments.postTransformArgument;

			string tempPath = inputPath + ".tmp";
			IEnumerable<SfmTransformSettings.ChangePair> pairs = sfmSettings.ChangePairs;
			using (StreamReader reader = File.OpenText(inputPath))
			{
				using (StreamWriter writer = new StreamWriter(tempPath))
				{
					while (!reader.EndOfStream)
					{
						string line = reader.ReadLine();
					   // if(line.StartsWith("\\lx"))
						{
							if(progressState.Cancel)
							{
								return;
							}
								//we don't have a way of knowing      progressState.NumberOfStepsCompleted = ;
						}
						foreach (SfmTransformSettings.ChangePair pair in pairs)
						{
							//this is super slow
							line = pair.regex.Replace(line, pair.to);
						}
						writer.WriteLine(line);
					}
					writer.Close();
				}
				reader.Close();
			}

			//System.Threading.Thread.Sleep(500);//don't event see that message otherwise
			progressState.StatusLabel = "Cleaning up...";
			progressState.NumberOfStepsCompleted++;
			File.Delete(inputPath);
			File.Move(tempPath, inputPath);//, backupPath);
			progressState.NumberOfStepsCompleted = progressState.TotalNumberOfSteps;
			System.Threading.Thread.Sleep(500);//don't event see that message otherwise
		}

		public override void Launch(Form parentForm, ProjectInfo projectInfo)
		{
			_settings.FillEmptySettingsWithGuesses(projectInfo);
			SetupPostTransformMethod(OnDoGrepWork, _settings, 10/*has some cushion*/);
			string output = TransformLift(projectInfo, "lift2sfm.xsl", "-sfm.txt");
			if (string.IsNullOrEmpty(output))
			{
				return; // get this when the user cancels
			}
			//GrepFile(output, _settings);

			if (_launchAfterTransform)
			{
				Process.Start(output);
			}
		}



		#region IWeSayAddinHasSettings Members

		public bool DoShowSettingsDialog(Form parentForm, ProjectInfo projectInfo)
		{
			SFMChangesDialog dlg = new SFMChangesDialog(_settings, projectInfo);
			return dlg.ShowDialog(parentForm) == DialogResult.OK;
		}

		public object Settings
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = (SfmTransformSettings)value;
			}
		}

		#endregion
	}
}
