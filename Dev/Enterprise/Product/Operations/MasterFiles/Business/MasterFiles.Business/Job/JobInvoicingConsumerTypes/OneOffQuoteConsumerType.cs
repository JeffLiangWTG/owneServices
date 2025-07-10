using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class OneOffQuoteConsumerType : JobInvoicingConsumerType
	{
		public OneOffQuoteConsumerType(string code, MultilingualString description)
			: base(code, description) { }

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.Quotations; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.Get<Enterprise.Integration.Rating.IRating>().QuoteType; }
		}

		public override bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType)
		{
			return false;
		}

		public override bool ShouldCreateAccruals(IJobInvoicingPlugIn host, string invoiceType)
		{
			return false;
		}

		public override bool ShouldCreateSellJRJ(IJobInvoicingPlugIn host) => false;

		public override bool ShouldCreateCostJRJ(IJobInvoicingPlugIn host) => false;

		public override bool InvoicingPrintingApplicable(IJobInvoicingPlugIn host)
		{
			return false;
		}

		public override bool CreditStatusApplicable(IJobInvoicingPlugIn host)
		{
			return false;
		}

		public override bool ProfitLossApplicable(IJobInvoicingPlugIn host)
		{
			return false;
		}

		public override bool OverseasAgentApplicable { get { return RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.Value; } }

		public override PostChargesAllowedInformation ShouldPostCharges(IJobInvoicingPlugIn host, string invoiceType, bool isForAPLine)
		{
			var reason = Res.GetString("687ad944-d808-4eea-94f6-f640fcb22512", "Spot Quotes/Quoted Bookings do not allow charges to be posted. Charges should be posted on the Shipment or standalone Booking only.");

			return new PostChargesAllowedInformation(false, reason);
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceForwarding; }
		}

		public override string DisplayName(IJobInvoicingPlugIn host)
		{
			return host.InvoicingSupporter.IsQuote ? Res.GetString("2419F823-4649-48C4-B90B-CC666926F3A6", "Quote Charges") : base.DisplayName(host);
		}

		public override ResourceString MenuName(IJobInvoicingPlugIn host)
		{
			return host.InvoicingSupporter.IsQuote ? ResString.GetMultilingualString("Accounting.QuoteMenuName", "&Quote Charges") : base.MenuName(host);
		}

		public override bool AllowCostPosting(IJobInvoicingPlugIn host)
		{
			return !host.InvoicingSupporter.IsQuote;
		}

		public override bool AllowRevenuePosting(IJobInvoicingPlugIn host)
		{
			return !host.InvoicingSupporter.IsQuote;
		}

		public override string RevenueChargeDescription(IJobInvoicingPlugIn host)
		{
			return host.InvoicingSupporter.IsQuote ? ResString.GetMultilingualString("Accounting.QuoteCharges", "Quote Charges") : base.RevenueChargeDescription(host);
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}

		public override bool IsTransportModeSupported
		{
			get { return true; }
		}

		public override bool IsDirectionSupported
		{
			get { return true; }
		}
	}
}
