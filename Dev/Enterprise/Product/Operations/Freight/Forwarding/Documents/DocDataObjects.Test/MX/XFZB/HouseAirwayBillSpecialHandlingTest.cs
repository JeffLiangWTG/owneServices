using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.MX.Testing
{
	[TestedType(typeof(HouseAirwayBillSpecialHandling))]
	sealed class HouseAirwayBillSpecialHandlingTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new HouseAirwayBillSpecialHandling(0);
	}
}
