using System;
using System.Windows.Forms;
using WeSay.AddinLib;
using WeSay.Language;
using WeSay.Project;

namespace WeSay.CommonTools
{
    public partial class ActionsControl : UserControl, ITask
    {
        private bool _isActive;
        private bool _wasLoaded=false;

        public ActionsControl()
        {
            InitializeComponent();
       }

        #region ITask
        public void Activate()
        {
            if (!_wasLoaded)
            {
                _wasLoaded = true;
                AddAddin(new ComingSomedayAddin("Export To OpenOffice", ""));
            }
            _isActive = true;

        }


        private void AddAddin(IWeSayAddin addin)
        {
            ActionItemControl control = new ActionItemControl(addin);
            _addinsList.AddControlToBottom(control);
            control.Launch += new EventHandler(OnLaunchAction);
        }

        private void OnLaunchAction(object sender, EventArgs e)
        {
            IWeSayAddin addin = sender as IWeSayAddin;

            addin.Launch(Project.WeSayWordsProject.Project.ProjectDirectoryPath,
                                                Project.WeSayWordsProject.Project.PathToLiftFile);
        }

        public void Deactivate()
        {
            if(!IsActive)
            {
                throw new InvalidOperationException("Deactivate should only be called once after Activate.");
            }
           // this._vbox.Clear();
            _isActive = false;
        }

        public bool IsActive
        {
            get { return this._isActive; }
        }

        public string Label
        {
            get { return StringCatalog.Get("Actions"); }
        }

        public Control Control
        {
            get { return this; }
        }

        public bool IsPinned
        {
            get
            {
                return true;
            }
        }

        public string Status
        {
            get
            {
                return string.Empty;
            }
        }
        public string ExactStatus
        {
            get
            {
                return Status;
            }
        }

        public string Description
        {
            get
            {
                return StringCatalog.Get("Do other actions.");
            }
        }

        #endregion

    }
}
