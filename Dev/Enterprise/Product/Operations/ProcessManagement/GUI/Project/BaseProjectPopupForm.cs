using System;
using System.Windows.Forms;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class BaseProjectPopupForm : ZChildForm
	{
		public BaseProjectPopupForm()
		{
			InitializeComponent();
		}

		public BaseProjectPopupForm(ProjectAction action)
			: base(action)
		{
			InitializeComponent();
			Action = action;
		}

		public ProjectAction Action
		{
			private set;
			get;
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public virtual void CloseButton_Click(object sender, EventArgs e)
		{
			Action.Synchronise();
			if (!Action.HasErrors)
			{
				DialogResult = DialogResult.OK;
			}
			else
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

#if DEBUG
		public ZButton CloseButton_Exposed
		{
			get { return CloseButton; }
		}
#endif
	}
}
