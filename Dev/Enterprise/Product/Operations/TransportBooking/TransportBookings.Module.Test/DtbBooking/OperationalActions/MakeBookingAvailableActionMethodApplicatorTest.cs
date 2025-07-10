using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(MakeBookingAvailableChangeActionMethodApplicator))]
	public class MakeBookingAvailableActionMethodApplicatorTest : BookingStatusChangeActionMethodApplicatorTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MakeBookingAvailableChangeActionMethodApplicator();
		}

		protected override IEnumerable<ZString> ExpectedStatusesToChangeFrom
		{
			get { return new List<ZString> { TransportStatuses.Codes.Incomplete, TransportStatuses.Codes.Quote, TransportStatuses.Codes.ActionRequired }; }
		}

		protected override ZString ExpectedInvalidStatusToChangeFrom
		{
			get { return TransportStatuses.Codes.Held; }
		}

		protected override ZString ExpectedStatusToChangeTo
		{
			get { return TransportStatuses.Codes.Available; }
		}
	}
}
