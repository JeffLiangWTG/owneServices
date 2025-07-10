using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommercialInvoiceDetailsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new CommercialInvoiceDetailsUserControl();

		[ThreadStatic]
		static CommercialInvoiceDetailsControlBag instance;

		public static CommercialInvoiceDetailsControlBag Instance => instance ?? (instance = new CommercialInvoiceDetailsControlBag());

		CommercialInvoiceDetailsControlBag()
		{
			InvoiceNumberTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceNumberTextBox));
			GroupInvoiceDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.GroupInvoiceDropEdit));
			InvoiceAmountConvertToLocalCurrencyControl = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceAmountConvertToLocalCurrencyControl));
			InvoiceCurrExRateCalcEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceCurrExRateCalcEdit));
			IncoTermsUserControl = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.IncoTermsUserControl));
			IncoTermPlaceTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.IncoTermPlaceTextBox));
			AdditionalTermsTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.AdditionalTermsTextBox));
			GrossWeightCalcDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.NetWeightCalcDropEdit));
			InvoiceCurrLandedCostExRateCalcEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceCurrLandedCostExRateCalcEdit));
			NoOfPacksCalcDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.NoOfPacksCalcDropEdit));
			InvoiceDateEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.InvoiceDateEdit));
			ValuationCodeDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.ValuationCodeDropEdit));
			UCRTextBox = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.UCRTextBox));
			ExporterAddressControl = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.ExporterAddressControl));
			IncoTermDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.IncoTermDropEdit));
			PaymentMethodDropEdit = RegisterControl(nameof(CommercialInvoiceDetailsUserControl.PaymentMethodDropEdit));
		}

		public ControlReference InvoiceNumberTextBox { get; }
		public ControlReference GroupInvoiceDropEdit { get; }
		public ControlReference InvoiceAmountConvertToLocalCurrencyControl { get; }
		public ControlReference InvoiceCurrExRateCalcEdit { get; }
		public ControlReference IncoTermsUserControl { get; }
		public ControlReference IncoTermPlaceTextBox { get; }
		public ControlReference AdditionalTermsTextBox { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference InvoiceCurrLandedCostExRateCalcEdit { get; }
		public ControlReference NoOfPacksCalcDropEdit { get; }
		public ControlReference InvoiceDateEdit { get; }
		public ControlReference ValuationCodeDropEdit { get; }
		public ControlReference UCRTextBox { get; }
		public ControlReference ExporterAddressControl { get; }
		public ControlReference IncoTermDropEdit { get; }
		public ControlReference PaymentMethodDropEdit { get; }
	}
}
