using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class CommonInvoiceLineCalculationsControlBag : ControlBag
	{
		public static CommonInvoiceLineCalculationsControlBag Instance => instanceLazy.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CommonInvoiceLineCalculationsControlBag> instanceLazy = new Lazy<CommonInvoiceLineCalculationsControlBag>(() => new CommonInvoiceLineCalculationsControlBag());

		CommonInvoiceLineCalculationsControlBag()
		{
			CurrentInvoiceLabel = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.CurrentInvoiceLabel));
			BalanceConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.BalanceConvertToLocalCurrencyControl));
			LinesEnteredConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.LinesEnteredConvertToLocalCurrencyControl));
			LinesTotalConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.LinesTotalConvertToLocalCurrencyControl));
			SummaryLabel = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.SummaryLabel));
			CIFConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.CIFConvertToLocalCurrencyControl));
			InsuranceInInvoiceCurrConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.InsuranceInInvoiceCurrConvertToLocalCurrencyControl));
			FreightInInvoiceCurrConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.FreightInInvoiceCurrConvertToLocalCurrencyControl));
			FOBConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.FOBConvertToLocalCurrencyControl));
			GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl));
			DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl = RegisterControl(nameof(CommonInvoiceLineCalculationsUserControl.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl));
		}

		protected override Control CreateTemplate() => new CommonInvoiceLineCalculationsUserControl();

		public ControlReference CurrentInvoiceLabel { get; }
		public ControlReference BalanceConvertToLocalCurrencyControl { get; }
		public ControlReference LinesEnteredConvertToLocalCurrencyControl { get; }
		public ControlReference LinesTotalConvertToLocalCurrencyControl { get; }
		public ControlReference SummaryLabel { get; }
		public ControlReference CIFConvertToLocalCurrencyControl { get; }
		public ControlReference InsuranceInInvoiceCurrConvertToLocalCurrencyControl { get; }
		public ControlReference FreightInInvoiceCurrConvertToLocalCurrencyControl { get; }
		public ControlReference FOBConvertToLocalCurrencyControl { get; }
		public ControlReference GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl { get; }
		public ControlReference DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl { get; }
	}
}
