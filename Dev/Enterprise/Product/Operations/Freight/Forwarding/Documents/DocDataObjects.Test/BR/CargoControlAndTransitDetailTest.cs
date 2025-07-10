using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.BR.Testing
{
	[TestedType(typeof(CargoControlAndTransitDetail))]
	sealed class CargoControlAndTransitDetailTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var cctHouseAndTransitDetail = new CargoControlAndTransitDetail("zzz", "zzz", "CargoControlAndTransitHouseManifest");

			return cctHouseAndTransitDetail;
		}
	}
}
