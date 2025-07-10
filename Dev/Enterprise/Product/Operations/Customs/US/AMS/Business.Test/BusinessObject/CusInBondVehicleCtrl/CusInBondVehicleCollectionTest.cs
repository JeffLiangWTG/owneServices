using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondVehicleCollection))]
	sealed class CusInBondVehicleCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondVehicleCollection>
	{
		protected override CusInBondVehicleCollection GetCollectionToTest()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			return new CusInBondVehicleCollection(container);
		}
	}
}
