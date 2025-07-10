using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed partial class CommonInvoiceLineDetailsUserControl : ZUserControl
	{
		public CommonInvoiceLineDetailsUserControl()
		{
			InitializeComponent();
			InitializeLongTextControlBindings();
			InitializeTariffFindBox();
			InitializeLongWithDescriptionTariffFindBox();
			InitializeLongFormattedWithDescriptionTariffFindBox();
		}

		void InitializeLongFormattedWithDescriptionTariffFindBox()
		{
			FormattedWithDescriptionTariffFindBox.GetEffectiveDate = () => GetInvoiceLine(FormattedWithDescriptionTariffFindBox)?.EffectiveAssessmentDate ?? ZDateTime.Today;
			FormattedWithDescriptionTariffFindBox.GetTariffType = () => GetInvoiceLine(FormattedWithDescriptionTariffFindBox)?.UniversalTariffType ?? ZString.Empty;
			FormattedWithDescriptionTariffFindBox.GetDataGrouping = () => GetInvoiceLine(FormattedWithDescriptionTariffFindBox)?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
			FormattedWithDescriptionTariffFindBox.GetCountryCode = () => GetInvoiceLine(FormattedWithDescriptionTariffFindBox)?.CustomsCountryCode;
		}

		void InitializeLongWithDescriptionTariffFindBox()
		{
			WithDescriptionTariffFindBox.GetEffectiveDate = () => GetInvoiceLine(WithDescriptionTariffFindBox)?.EffectiveAssessmentDate ?? ZDateTime.Today;
			WithDescriptionTariffFindBox.GetTariffType = () => GetInvoiceLine(WithDescriptionTariffFindBox)?.UniversalTariffType ?? ZString.Empty;
			WithDescriptionTariffFindBox.GetDataGrouping = () => GetInvoiceLine(WithDescriptionTariffFindBox)?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
			WithDescriptionTariffFindBox.GetCountryCode = () => GetInvoiceLine(WithDescriptionTariffFindBox)?.CustomsCountryCode;
		}

		void InitializeTariffFindBox()
		{
			TariffFindBox.GetEffectiveDate = () => GetInvoiceLine(TariffFindBox)?.EffectiveAssessmentDate ?? ZDateTime.Today;
			TariffFindBox.GetTariffType = () => GetInvoiceLine(TariffFindBox)?.UniversalTariffType ?? ZString.Empty;
			TariffFindBox.GetDataGrouping = () => GetInvoiceLine(TariffFindBox)?.GetDefaultDataGroupingCode(DefaultDataGroupingType.Tariff) ?? ZString.Empty;
			TariffFindBox.GetCountryCode = () => GetInvoiceLine(TariffFindBox)?.CustomsCountryCode;
		}

		void InitializeLongTextControlBindings()
		{
			BindingSource.SetBindingMember(DescriptionLongTextControl, BaseJobComInvoiceLine.Schema.JI_Description);
			BindingSource.SetBindingMember(UnicodeDescriptionLongTextControl, BaseJobComInvoiceLine.Schema.JI_NDescription);
		}
		BaseJobComInvoiceLine GetInvoiceLine(Universal.GUI.TariffFindBox tariffFindBox) => ((DynamicLayoutPanel)tariffFindBox.Parent).CurrentDataItem as BaseJobComInvoiceLine;
	}
}
