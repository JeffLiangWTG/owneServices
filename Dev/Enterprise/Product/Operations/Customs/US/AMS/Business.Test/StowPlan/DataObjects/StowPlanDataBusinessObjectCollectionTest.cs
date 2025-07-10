using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanDataBusinessObjectCollection<StowPlanShipmentData>))]
	class StowPlanDataBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StowPlanDataBusinessObjectCollection<StowPlanShipmentData>>
	{
		protected override StowPlanDataBusinessObjectCollection<StowPlanShipmentData> GetCollectionToTest()
		{
			return new StowPlanDataBusinessObjectCollection<StowPlanShipmentData>(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StowPlanShipmentData(Factory.New<BillOfLading>());
		}
	}
}
