using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	[TestedType(typeof(BookingPackingLine))]
	sealed class BookingPackingLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BookingPackingLine(new ZGuid())
			{
				HarmonizedCodes = System.Array.Empty<HarmonizedCode>(),
				DangerousGoods = System.Array.Empty<DangerousGood>()
			};
		}
	}
}
