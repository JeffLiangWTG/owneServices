using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(FlightDetail))]
	sealed class FlightDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var flightDetail = new FlightDetail("zzz");

			return flightDetail;
		}
	}
}
