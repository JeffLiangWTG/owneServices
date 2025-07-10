using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public abstract class BaseShipmentConsumerType : JobInvoicingConsumerType
	{
		protected BaseShipmentConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override bool IsTransportModeSupported
		{
			get { return true; }
		}

		public override bool IsDirectionSupported
		{
			get { return true; }
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}

		public override bool ShouldCreateCostJRJ(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool ShouldCreateSellJRJ(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool ShouldCreateAccruals(IJobInvoicingPlugIn host, string invoiceType) => IsSupportInvoicingPlugIn(host);

		public override bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType) => IsSupportInvoicingPlugIn(host);

		public override bool ProfitLossApplicable(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool CreditStatusApplicable(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool InvoicingPrintingApplicable(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool AllowRevenuePosting(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override bool AllowCostPosting(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);

		public override ResourceString MenuName(IJobInvoicingPlugIn host)
		{
			return IsSupportInvoicingPlugIn(host) ? base.MenuName(host) : ResString.GetMultilingualString("Accounting.QuoteMenuName", "&Quote Charges");
		}

		public override string DisplayName(IJobInvoicingPlugIn host)
		{
			return IsSupportInvoicingPlugIn(host) ? base.DisplayName(host) : Res.GetString("2419F823-4649-48C4-B90B-CC666926F3A6", "Quote Charges");
		}

		protected bool IsSupportInvoicingPlugIn(IJobInvoicingPlugIn host) => !host?.InvoicingSupporter.IsBookingWithQuote ?? false;
	}
}
