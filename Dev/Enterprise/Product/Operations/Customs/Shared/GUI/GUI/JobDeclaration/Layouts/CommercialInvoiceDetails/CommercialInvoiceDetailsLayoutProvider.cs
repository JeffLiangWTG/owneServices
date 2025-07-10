using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	sealed class CommercialInvoiceDetailsLayoutProvider : IPanelLayoutProvider
	{
		PanelLayout IPanelLayoutProvider.Layout => PanelLayout;

		PanelLayout PanelLayout { get; }

		public CommercialInvoiceDetailsLayoutProvider()
		{
			PanelLayout = CreatePanelLayout();
		}

		PanelLayout CreatePanelLayout()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<BaseJobComInvoiceHeader>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.GroupInvoiceDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}
	}
}
