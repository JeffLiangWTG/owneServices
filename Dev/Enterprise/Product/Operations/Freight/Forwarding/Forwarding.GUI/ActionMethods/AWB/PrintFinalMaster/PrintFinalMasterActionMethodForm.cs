using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class PrintFinalMasterActionMethodForm : ZChildForm
	{
		public PrintFinalMasterActionMethodForm(BulkConsolAWBActions businessEntity, string onErrorMessageError)
			: base(businessEntity)
		{
			InitializeComponent();
			MAWBPrintOptionsControl.HidePrintedDateDetails();
			CIMPOptionsControl.HideSentDateDetails();
			if (onErrorMessageError == AWBPrintSettings.Codes.Skip)
			{
				ShowExtraSkipOptions();
			}
		}

		void ShowExtraSkipOptions()
		{
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 323);
			AllowPrintingWithMsgErrorsCheckBox.Visible = true;
		}

		BulkConsolAWBActions AWBActions
		{
			get { return (BulkConsolAWBActions)BusinessEntity; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			AWBActions.ValidateAll();
			if (AWBActions.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}
	}
}
