using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.MX;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.MX.Testing
{
	[TestedType(typeof(HouseAirwayBillRateLine))]
	sealed class HouseAirwayBillRateLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new HouseAirwayBillRateLine();
	}
}
