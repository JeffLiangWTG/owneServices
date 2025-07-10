using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondEquipmentCollection))]
	class CusInBondEquipmentCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondEquipmentCollection>
	{
		protected override CusInBondEquipmentCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			return new CusInBondEquipmentCollection(header);
		}
	}
}
