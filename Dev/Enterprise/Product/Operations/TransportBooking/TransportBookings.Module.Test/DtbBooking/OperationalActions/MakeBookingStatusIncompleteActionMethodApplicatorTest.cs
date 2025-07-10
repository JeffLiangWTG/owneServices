using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(MakeBookingStatusIncompleteActionMethodApplicator))]
	public class MakeBookingStatusIncompleteActionMethodApplicatorTest : BookingStatusChangeActionMethodApplicatorTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MakeBookingStatusIncompleteActionMethodApplicator();
		}

		protected override IEnumerable<ZString> ExpectedStatusesToChangeFrom
		{
			get { return new List<ZString> { TransportStatuses.Codes.Held }; }
		}

		protected override ZString ExpectedInvalidStatusToChangeFrom
		{
			get { return TransportStatuses.Codes.ActionRequired; }
		}

		protected override ZString ExpectedStatusToChangeTo
		{
			get { return TransportStatuses.Codes.Incomplete; }
		}
	}
}
