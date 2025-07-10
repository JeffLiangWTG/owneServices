using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.SG.V4.Business.CMDMessaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.SG.V4.GUI.CMDMessaging
{
	public partial class GHACaptureForm : ZChildForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public GHACaptureForm()
			: base()
		{
			InitializeComponent();
		}

		public GHACaptureForm(GHACapture gHACapture)
			: base(gHACapture)
		{
			InitializeComponent();
		}

		public override string FormHeading
		{
			get { return ResString.GetMultilingualString("CMDMessaging|GHACaptureForm|FormHeading", "Select GHA"); }
		}

		#region OK / Cancel Button Clicks

		void OKBoundButton_Click(object sender, System.EventArgs e)
		{
			HandleOKButtonClick();
		}

		void CancelBoundButton_Click(object sender, System.EventArgs e)
		{
			HandleCancelButtonClick();
		}

		void HandleOKButtonClick()
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.Notifications.HasErrors())
			{
				this.DialogResult = DialogResult.OK;
				Close();
			}
		}

		void HandleCancelButtonClick()
		{
			((GHACapture)BusinessEntity).GHA = "";
			Close();
		}

		#endregion

		#region IAllowTabBackwardBetweenSomeOfMyChildren Members

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == oKBoundButton && previousControl == cancelBoundButton;
		}

		#endregion
	}
}
