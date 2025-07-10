using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	public sealed class InvoiceControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new InvoiceLayoutsUserControl();

		public static InvoiceControlBag Instance => instance ?? (instance = new InvoiceControlBag());

		[ThreadStatic]
		static InvoiceControlBag instance;

		InvoiceControlBag()
		{
			InvoiceDateDateEdit = RegisterControl(nameof(InvoiceLayoutsUserControl.InvoiceDateDateEdit));
			ValuationMethodDropEdit = RegisterControl(nameof(InvoiceLayoutsUserControl.ValuationMethodDropEdit));
			ExchangeRatePlusFixedRateUserControl = RegisterControl(nameof(InvoiceLayoutsUserControl.ExchangeRatePlusFixedRateUserControl));
		}

		public ControlReference InvoiceDateDateEdit { get; }
		public ControlReference ValuationMethodDropEdit { get; }
		public ControlReference ExchangeRatePlusFixedRateUserControl { get; }
	}
}
