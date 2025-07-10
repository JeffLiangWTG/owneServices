using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class ImportDetailsUserControl : ZUserControl
	{
		public ImportDetailsUserControl()
		{
			InitializeComponent();
			LabelCaptionRenderProvider.SetLabelCaptionVisible(TW_ImportTariffDescriptionTextBox, false);
		}

		internal void SetControlsVisibilityForRAPROR(JobComInvoiceLine invoiceLine)
		{
			var isRAPOrROR = invoiceLine?.IsRAPOrROR ?? false;
			var isCV = invoiceLine?.ShouldAllowOneTenthCVasRAPRORPrice ?? ZBool.False;
			TW_RAPRORPriceConvertToLocalCurrencyControl.Visible = isRAPOrROR;
			JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.Visible = isRAPOrROR;
			JI_UseOneTenthCVCheckBox.Visible = isCV;
			RAPRORGroupBox.Visible = isRAPOrROR;

			if (isRAPOrROR)
			{
				JI_Calc_RAPRORUnitPriceConvertToLocalCurrencyControl.UpdateCaption();
				TW_RAPRORPriceConvertToLocalCurrencyControl.UpdateCaption();
				RAPRORGroupBox.CaptionResourceString = invoiceLine.IsRAP ? Res.GetData("D4FBA24D-D17D-468C-9757-8787225E88D7", "Repair/Assembly/Processing")
					: Res.GetData("CA09ED25-0C46-431B-B48F-0C666297C742", "Rental/Royalty");
				RAPRORGroupBox.UpdateCaption();
			}
		}
	}
}
