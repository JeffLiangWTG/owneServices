using System;
using CargoWise.Application;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class QuotedBookingConsumerType : BaseShipmentConsumerType
	{
		public QuotedBookingConsumerType(string code, MultilingualString description)
			: base(code, description)
		{
		}

		public override ControllerID ControllerID
		{
			get { return ControllerIDs.QuotedBookings; }
		}

		public override Type BizoType
		{
			get { return ObjectFactory.GetType<IQuotedBooking>(); }
		}

		public override bool OverseasAgentApplicable
		{
			get { return true; }
		}

		public override bool ShouldDisplayClientContractNumber(IJobInvoicingPlugIn host) => IsSupportInvoicingPlugIn(host);
	}
}
