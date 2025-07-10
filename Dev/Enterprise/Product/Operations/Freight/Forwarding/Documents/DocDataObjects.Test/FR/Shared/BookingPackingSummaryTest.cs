using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(BookingPackingSummary))]
	sealed class BookingPackingSummaryTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BookingPackingSummary(new ZGuid());
		}
	}
}
