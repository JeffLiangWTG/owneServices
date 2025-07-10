using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.CommercialInvoice
{
	public sealed class InvoiceHeaderDetailsControlBag : ControlBag
	{
		public static InvoiceHeaderDetailsControlBag Instance => instance ?? (instance = new InvoiceHeaderDetailsControlBag());

		[ThreadStatic]
		static InvoiceHeaderDetailsControlBag instance;

		public InvoiceHeaderDetailsControlBag() : base()
		{
			InvoiceNumberBoundTextBox = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.InvoiceNumberBoundTextBox));
			InvoiceDateEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.InvoiceDateEdit));
			InvoiceAmountCalcFindBox = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.InvoiceAmountCalcFindBox));
			InvoiceCurrExRateCalcEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.InvoiceCurrExRateCalcEdit));
			IncotermAndIncotermPlaceUserControl = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.IncotermAndIncotermPlaceUserControl));
			ValuationCodeDropEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.ValuationCodeDropEdit));
			GrossWeightCalcDropEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.NetWeightCalcDropEdit));
			InvoiceCurrLandedCostExRateCalcEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.InvoiceCurrLandedCostExRateCalcEdit));
			NoOfPacksCalcDropEdit = RegisterControl(nameof(InvoiceHeaderDetailsLayoutUserControl.NoOfPacksCalcDropEdit));
		}

		public ControlReference InvoiceNumberBoundTextBox { get; }
		public ControlReference InvoiceDateEdit { get; }
		public ControlReference InvoiceAmountCalcFindBox { get; }
		public ControlReference InvoiceCurrExRateCalcEdit { get; }
		public ControlReference IncotermAndIncotermPlaceUserControl { get; }
		public ControlReference ValuationCodeDropEdit { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference InvoiceCurrLandedCostExRateCalcEdit { get; }
		public ControlReference NoOfPacksCalcDropEdit { get; }

		protected override Control CreateTemplate() => new InvoiceHeaderDetailsLayoutUserControl();
	}
}
