using CargoWise.Types;

namespace Enterprise.Customs.GUI
{
	public partial class GeneralCountryInvoiceLineUserControl : DeclarationInvoiceLineUserControl
	{
		public GeneralCountryInvoiceLineUserControl()
		{
			InitializeComponent();
			InvoiceLineUserControlHelper.SetTariffRelated(CustomsInvoiceLinesBoundGrid, Name, JI_TariffFindBox, GetCustomsCountryCode, GetDataGroupingForUniversalTariff, GetUniversalTariffType());
			JI_TariffFindBox.GetEffectiveDate = GetEffectiveAssessmentDateForUniversalTariff;
		}

		protected override ZBool DynamicLayoutApplied => ZBool.True;
	}
}
