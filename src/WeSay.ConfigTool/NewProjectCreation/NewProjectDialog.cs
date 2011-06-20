﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.WritingSystems;

namespace WeSay.ConfigTool.NewProjectCreation
{
	public partial class NewProjectDialog : Form
	{
		private readonly string _destinationDirectory;
		public string Iso639Code;
		public string LanguageName;


		public NewProjectDialog()
		{
			InitializeComponent();
		}

		public NewProjectDialog(string destinationDirectory)
		{
			_destinationDirectory = destinationDirectory;
			InitializeComponent();
			Icon = Application.OpenForms[0].Icon;
			btnOK.Enabled = false;
			_pathLabel.Text = "";
		}

		protected virtual bool EnableOK
		{
			get { return NameLooksOk && !string.IsNullOrEmpty(Iso639Code); }
		}

		protected void _textProjectName_TextChanged(object sender, EventArgs e)
		{
			btnOK.Enabled = EnableOK;
			UpdateMessage();
		}

		private void UpdateMessage()
		{
			if(NameLooksOk)
			{
				string[] dirs = PathToNewProjectDirectory.Split(Path.DirectorySeparatorChar);
				if (dirs.Length > 1)
				{
					string root = Path.Combine(dirs[dirs.Length - 3], dirs[dirs.Length - 2]);
					_pathLabel.Text = String.Format("Project will be created at: {0}",
													Path.Combine(root, dirs[dirs.Length - 1]));
				}

				_pathLabel.Invalidate();
				Debug.WriteLine(_pathLabel.Text);
				if (string.IsNullOrEmpty(Iso639Code))
				{
					_pathLabel.Text += Environment.NewLine + "Please choose a language to continue.";
				}
			}
			else
			{
				_pathLabel.Text = "Unable to create a new project there.";
			}
		}

		private bool NameLooksOk
		{
			get
			{
				//http://regexlib.com/Search.aspx?k=file+name
				//Regex legalFilePattern = new Regex(@"(.*?)");
				//               if (!(legalFilePattern.IsMatch(_textProjectName.Text)))
				//               {
				//                   return false;
				//               }

				if (_textProjectName.Text.Trim().Length < 1)
				{
					return false;
				}

				if (_textProjectName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
				{
					return false;
				}

				if (Directory.Exists(PathToNewProjectDirectory) || File.Exists(PathToNewProjectDirectory))
				{
					return false;
				}
				return true;
			}
		}

		public string PathToNewProjectDirectory
		{
			get { return Path.Combine(_destinationDirectory, _textProjectName.Text); }
		}

		protected void OnBtnOK_Click(object sender, EventArgs e)
		{
			ProjectName = _textProjectName.Text.Trim();
			DialogResult = DialogResult.OK;
			Close();
		}

		public string ProjectName
		{
			get;
			private set;
		}

		protected void OnBtnCancel_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void _chooseLanguageButton_Click(object sender, EventArgs e)
		{
			using (var dlg = new LookupISOCodeDialog())
			{
				if (DialogResult.OK != dlg.ShowDialog())
				{
					return;
				}
				_languageInfoLabel.Text = string.Format("{0} ({1})", dlg.ISOCodeAndName.Name, dlg.ISOCode);
				Iso639Code = dlg.ISOCodeAndName.Code;
				LanguageName = dlg.ISOCodeAndName.Name;
				if (_textProjectName.Text.Trim().Length == 0)
				{
					_textProjectName.Text = dlg.ISOCodeAndName.Name;
				}
				else
				{
					btnOK.Enabled = EnableOK;
					UpdateMessage();
				}
			}
		}
	}
}