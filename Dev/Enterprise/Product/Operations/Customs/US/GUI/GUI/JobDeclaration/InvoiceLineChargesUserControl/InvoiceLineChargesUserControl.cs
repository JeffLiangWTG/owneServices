using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;
namespace Enterprise.Customs.US.GUI
{
	public partial class InvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			InitializeComponent();
		}

		public void AddOverrideCheckBoxes()
		{
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			zCheckBoxColumnStyleInfo1.Caption = "Override";
			zCheckBoxColumnStyleInfo1.ColumnName = JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge;
			ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo1, 63, true);
			zCheckBoxColumnStyleInfo1.IsVisible = false;

			zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ApportionedChargesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			zCheckBoxColumnStyleInfo1.Caption = "Override";
			zCheckBoxColumnStyleInfo1.ColumnName = JobComInvHeaderChargeSchema.Constants.J7_AdjustedCharge;
			ControlDpiScalingHelper.SetWidth(ref zCheckBoxColumnStyleInfo1, 63, true);
			zCheckBoxColumnStyleInfo1.IsVisible = false;
		}
	}
}
