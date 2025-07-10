using Enterprise.Customs.GUI;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class CommercialInvoiceDetailsLayout : IPanelLayoutProvider
	{
		public PanelLayout Layout => CommercialInvoice;

		public CommercialInvoiceDetailsLayout()
		{
			CommercialInvoice = CreateCommercialInvoiceDetailsLayouts();
		}

		PanelLayout CreateCommercialInvoiceDetailsLayouts()
		{
			var builder = new CommercialInvoiceDetailsLayoutBuilder<JobComInvoiceHeader>();
			var commonBag = builder.CommonBag;

			var noBag = InvoiceControlBag.Instance;
			builder.AddControlBag(noBag);

			builder.AddColumn();

			builder.Add(commonBag.InvoiceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(noBag.InvoiceDateDateEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceAmountConvertToLocalCurrencyControl, ControlWidthClass.Auto);
			builder.Add(noBag.ExchangeRatePlusFixedRateUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermsUserControl, ControlWidthClass.Auto);
			builder.Add(commonBag.IncoTermPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(commonBag.ValuationCodeDropEdit, ControlWidthClass.Auto);
			builder.Add(noBag.ValuationMethodDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.GrossWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.InvoiceCurrLandedCostExRateCalcEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NoOfPacksCalcDropEdit, ControlWidthClass.Auto);

			return builder.Build();
		}

		PanelLayout CommercialInvoice { get; }
	}
}
