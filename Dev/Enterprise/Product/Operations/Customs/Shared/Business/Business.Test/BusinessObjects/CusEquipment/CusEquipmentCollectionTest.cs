using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEquipmentCollection<CusEquipment>))]
	class CusEquipmentCollectionTest : ActiveBusinessObjectCollectionTestCase<CusEquipmentCollection<CusEquipment>>
	{
		protected override CusEquipmentCollection<CusEquipment> GetCollectionToTest()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			return new CusEquipmentCollection<CusEquipment>(declaration);
		}
	}
}
