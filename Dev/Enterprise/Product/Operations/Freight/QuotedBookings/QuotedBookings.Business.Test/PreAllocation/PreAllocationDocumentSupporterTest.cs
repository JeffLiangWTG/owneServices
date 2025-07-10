using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	[TestedType(typeof(PreAllocationDocumentSupporter))]
	public class PreAllocationDocumentSupporterTest : DocumentSupporterTest
	{
		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return true;
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var booking = QuotedBooking.CreateNewBooking(Factory);
			var quotedBooking = QuotedBooking.New(ZGuid.Empty, booking.PK, Factory);
			return new PreAllocation(quotedBooking, PreAllocation.PreAllocationState.New);
		}
	}
}
