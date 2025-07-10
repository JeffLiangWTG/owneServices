using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(HoldBookingChangeActionMethodApplicator))]
	public class HoldBookingActionMethodApplicatorTest : BookingStatusChangeActionMethodApplicatorTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new HoldBookingChangeActionMethodApplicator();
		}

		protected override IEnumerable<ZString> ExpectedStatusesToChangeFrom
		{
			get { return new List<ZString> { TransportStatuses.Codes.Available }; }
		}

		protected override ZString ExpectedInvalidStatusToChangeFrom
		{
			get { return TransportStatuses.Codes.Incomplete; }
		}

		protected override ZString ExpectedStatusToChangeTo
		{
			get { return TransportStatuses.Codes.Held; }
		}
	}
}
