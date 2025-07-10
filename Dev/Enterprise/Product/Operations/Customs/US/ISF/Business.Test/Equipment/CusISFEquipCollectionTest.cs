using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(CusISFEquipCollection))]
	sealed class CusISFEquipCollectionTest : ActiveBusinessObjectCollectionTestCase<CusISFEquipCollection>
	{
		public void TestIndexer_EquipmentNumber()
		{
			var header = Factory.New<CusISFHeader>();
			var equipments = header.Equipments;
			var container1 = equipments.AddNew();
			container1.BE_ContainerNum = "TURE123456";
			var container2 = equipments.AddNew();
			container2.BE_ContainerNum = "TURE654321";
			AssertEquals(container1, equipments["TURE123456"]);
			AssertEquals(container2, equipments["TURE654321"]);
			AssertNull(equipments["TURE654XZZ"]);
			AssertNull(equipments[ZString.Empty]);
		}

		protected override CusISFEquipCollection GetCollectionToTest() => new CusISFEquipCollection(Factory.New<CusISFHeader>());
	}
}
