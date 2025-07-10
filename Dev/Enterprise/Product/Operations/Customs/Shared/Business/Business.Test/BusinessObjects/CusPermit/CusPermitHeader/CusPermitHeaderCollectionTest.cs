using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPermitHeaderCollection))]
	sealed class CusPermitHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPermitHeaderCollection>
	{
		public void TestMatchesFilterCore()
		{
			var guarantee1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			guarantee1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;

			var permit1 = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			var permit2 = Factory.NewWithValidTestData<BaseCusPermitHeader>();

			Factory.Save();

			var coll = new CusPermitHeaderCollection(Factory);

			AssertCollectionContains(permit1, coll);
			AssertCollectionContains(permit2, coll);
			AssertCollectionNotContains(guarantee1, coll);

			coll.MatchesFilterDelegate = (BaseCusPermitHeader permitHeader) => permitHeader.PK == permit1.PK;
			coll.RefreshFromDb();

			AssertCollectionContains(permit1, coll);
			AssertCollectionNotContains(permit2, coll);
			AssertCollectionNotContains(guarantee1, coll);
		}

		protected override CusPermitHeaderCollection GetCollectionToTest()
		{
			return new CusPermitHeaderCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, CusPermitHeaderApplicationCodeList.Codes.Permit);
		}
	}
}
