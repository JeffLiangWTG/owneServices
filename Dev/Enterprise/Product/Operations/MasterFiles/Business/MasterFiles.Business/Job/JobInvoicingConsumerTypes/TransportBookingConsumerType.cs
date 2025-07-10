using System;
using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class TransportBookingConsumerType : JobInvoicingConsumerType
	{
		public TransportBookingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.DtbBooking; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IDtbBooking>(); }
		}

		public override bool AllowRevenuePosting(IJobInvoicingPlugIn host)
		{
			return false;
		}

		public override bool ShouldCreateWIPs(IJobInvoicingPlugIn host, string invoiceType)
		{
			return false;
		}

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

		public override PostChargesAllowedInformation ShouldPostCharges(IJobInvoicingPlugIn host, string invoiceType, bool isForAPLine)
		{
			if (isForAPLine)
			{
				return base.ShouldPostCharges(host, invoiceType, true);
			}
			var message = Res.GetString("12199aaf-6f98-4688-935d-64e34b34db51", "Transport Bookings do not allow revenue charges to be posted. Revenue rating and billing should be done on the individual Transport Consignment.");

			return new PostChargesAllowedInformation(false, message);
		}

		public override SecurityCheckpoint DistanceCalculationCheckpoint
		{
			get { return Env.Security.RoadDistanceCalculationServiceLocalTransport; } // change to transport booking
		}

		public override string DisplayName(IJobInvoicingPlugIn host)
		{
			return Res.GetString("1389F823-4649-48C4-B90B-CC666926F4B7", "Quote and Costing");
		}

		public override ResourceString MenuName(IJobInvoicingPlugIn host)
		{
			return ResString.GetMultilingualString("Accounting.QuoteAndCostingMenuName", "&Quote and Costing");
		}

		public override string RevenueChargeDescription(IJobInvoicingPlugIn host)
		{
			return ResString.GetMultilingualString("Accounting.QuoteCharges", "Quote Charges");
		}

		public override bool SupportsWiseRates
		{
			get { return true; }
		}
	}
}
