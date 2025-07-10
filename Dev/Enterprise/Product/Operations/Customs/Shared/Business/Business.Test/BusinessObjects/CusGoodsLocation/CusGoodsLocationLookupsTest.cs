using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var list = lookups.QualifierList;
			CombineAssertions(() =>
			{
				AssertType<CusGoodsLocationQualifierList>("Type", list);
				AssertSame("Cached", list, lookups.QualifierList);
			});
		}

		public void TestTypeList()
		{
			var list = lookups.TypeList;
			CombineAssertions(() =>
			{
				AssertType<CusGoodsLocationTypeList>("Type", list);
				AssertSame("Cached", list, lookups.TypeList);
			});
		}

		public void TestLocationUseList()
		{
			var list = lookups.LocationUseList;
			CombineAssertions(() =>
			{
				AssertType<CusGoodsLocationUseList>("Type", list);
				AssertSame("Cached", list, lookups.LocationUseList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cusGoodsLocation = Factory.New<CusGoodsLocation>();
			lookups = cusGoodsLocation.Lookups;
		}
		CusGoodsLocationLookups lookups;
	}
}
