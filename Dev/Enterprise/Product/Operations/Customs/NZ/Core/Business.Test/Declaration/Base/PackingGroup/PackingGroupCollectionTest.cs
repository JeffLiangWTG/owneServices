using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(PackingGroupCollection))]
	public class PackingGroupCollectionTest : Customs.Business.Testing.BasePackingGroupCollectionTest<PackingGroupCollection>
	{
		public void TestGetTypeOfElementsFromPK()
		{
			PackingGroupCollection collection = GetCollectionToTest();
			AssertEquals(typeof(PackingGroup), collection.GetTypeOfElementsFromPK(ZGuid.Empty));
		}

		protected override PackingGroupCollection GetCollectionToTest()
		{
			return new PackingGroupCollection((Bill)HouseBill);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			PackingGroup result = Factory.New<PackingGroup>();
			result.CR_CU_HouseBill = HouseBill.PK;
			return result;
		}
	}
}
