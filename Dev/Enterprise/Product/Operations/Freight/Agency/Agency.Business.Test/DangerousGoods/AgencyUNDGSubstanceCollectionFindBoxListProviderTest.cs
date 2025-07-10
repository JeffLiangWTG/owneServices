using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyUNDGSubstanceCollectionFindBoxListProviderTest : TestCaseWithFactory
	{
		public void TestNearestMatchCore()
		{
			var iatSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			iatSubstance.DG_Code = "ABC1";
			iatSubstance.DG_IsActive = false;

			var imoSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			imoSubstance.DG_Code = "ABC2";
			imoSubstance.DG_IsActive = false;

			var cfrSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			cfrSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			cfrSubstance.DG_Code = "ABC3";
			cfrSubstance.DG_IsActive = false;

			var collection = new AgencyUNDGSubstanceCollection(Factory, null, cfrShouldBeDefaulted: false);
			var provider = new AgencyUNDGSubstanceCollectionFindBoxListProviderForTest(collection);

			AssertEquals(("ABC", false), provider.NearestMatchCore("ABC", true));

			iatSubstance.DG_IsActive = true;
			AssertEquals(("ABC1", true), provider.NearestMatchCore("ABC", true));

			imoSubstance.DG_IsActive = true;
			AssertEquals(("ABC2", true), provider.NearestMatchCore("ABC", true));

			imoSubstance.DG_IsActive = false;
			iatSubstance.DG_IsActive = false;
			cfrSubstance.DG_IsActive = true;
			AssertEquals(("ABC3", true), provider.NearestMatchCore("ABC", true));

			imoSubstance.DG_IsActive = true;
			iatSubstance.DG_IsActive = true;
			cfrSubstance.DG_IsActive = true;
			AssertEquals(("ABC2", true), provider.NearestMatchCore("ABC", true));

			collection = new AgencyUNDGSubstanceCollection(Factory, null, cfrShouldBeDefaulted: true);
			provider = new AgencyUNDGSubstanceCollectionFindBoxListProviderForTest(collection);

			imoSubstance.DG_IsActive = false;
			iatSubstance.DG_IsActive = false;
			cfrSubstance.DG_IsActive = false;
			AssertEquals(("ABC", false), provider.NearestMatchCore("ABC", true));

			iatSubstance.DG_IsActive = true;
			AssertEquals(("ABC1", true), provider.NearestMatchCore("ABC", true));

			cfrSubstance.DG_IsActive = true;
			AssertEquals(("ABC3", true), provider.NearestMatchCore("ABC", true));

			imoSubstance.DG_IsActive = true;
			iatSubstance.DG_IsActive = false;
			cfrSubstance.DG_IsActive = false;
			AssertEquals(("ABC2", true), provider.NearestMatchCore("ABC", true));

			imoSubstance.DG_IsActive = true;
			iatSubstance.DG_IsActive = true;
			cfrSubstance.DG_IsActive = true;
			AssertEquals(("ABC3", true), provider.NearestMatchCore("ABC", true));
		}

		public void TestBizObjsFromCodeWithCompleteFilter()
		{
			var iatSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			iatSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			iatSubstance.DG_Code = "ABC1";
			iatSubstance.DG_IsActive = false;

			var collection = new AgencyUNDGSubstanceCollection(Factory, null, cfrShouldBeDefaulted: false);
			var provider = new AgencyUNDGSubstanceCollectionFindBoxListProviderForTest(collection);

			AssertContainsExactElementsInAnyOrder(new[] { iatSubstance }, provider.BizObjsFromCodeWithCompleteFilterForTest("ABC1").Cast<UNDGSubstance>().ToArray());

			var imoSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			imoSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			imoSubstance.DG_Code = "ABC1";
			imoSubstance.DG_IsActive = false;

			var cfrSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			cfrSubstance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR;
			cfrSubstance.DG_Code = "ABC1";
			cfrSubstance.DG_IsActive = false;

			AssertContainsExactElementsInAnyOrder(new[] { imoSubstance }, provider.BizObjsFromCodeWithCompleteFilterForTest("ABC1").Cast<UNDGSubstance>().ToArray());

			collection = new AgencyUNDGSubstanceCollection(Factory, null, cfrShouldBeDefaulted: true);
			provider = new AgencyUNDGSubstanceCollectionFindBoxListProviderForTest(collection);
			AssertContainsExactElementsInAnyOrder(new[] { cfrSubstance }, provider.BizObjsFromCodeWithCompleteFilterForTest("ABC1").Cast<UNDGSubstance>().ToArray());
		}

		#region Implementation

		class AgencyUNDGSubstanceCollectionFindBoxListProviderForTest : AgencyUNDGSubstanceCollectionFindBoxListProvider
		{
			public AgencyUNDGSubstanceCollectionFindBoxListProviderForTest(AgencyUNDGSubstanceCollection collection) : base(collection)
			{
			}

			public IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilterForTest(string code)
			{
				return base.BizObjsFromCodeWithCompleteFilter(code);
			}
		}

		#endregion
	}
}
