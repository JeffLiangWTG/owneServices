using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefVesselZZ))]
	public class RefVesselZZTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookupVesselByCode()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			RefVesselZZ expectedVessel = Factory.New<RefVesselZZ>();
			expectedVessel.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel.ZZO_LloydsNumber = expectedVessel.ZZO_LloydsNumber;
			expectedVessel.ZZO_ZZZ_NKDataGrouping = "ZA";

			AssertEquals("Should have a valid RefVesselZZ obj", expectedVessel.PK, RefVesselZZ.LookupVesselByCode(expectedVessel.ZZO_Code, expectedVessel.ZZO_ZZZ_NKDataGrouping, Factory).PK);
			AssertEquals(null, RefVessel.LookupVesselByCode("COLUMBUS OLIVOS YY", Factory));
		}

		public void TestLookupVesselsByCode()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			RefVesselZZ expectedVessel = Factory.New<RefVesselZZ>();
			expectedVessel.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel.ZZO_LloydsNumber = expectedVessel.ZZO_LloydsNumber;
			expectedVessel.ZZO_ZZZ_NKDataGrouping = "ZA";

			var vessels = RefVesselZZ.LookupVesselsByCode(expectedVessel.ZZO_Code, expectedVessel.ZZO_ZZZ_NKDataGrouping, Factory);

			AssertEquals("Number of Vessels found with name: " + expectedVessel.ZZO_Code, 1, vessels.Length);
			AssertCollectionContains("Finds expectedVessel", expectedVessel, vessels);

			var expectedVessel2 = Factory.New<RefVesselZZ>();
			expectedVessel2.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel2.ZZO_RadioCallSign = "112233";
			expectedVessel.ZZO_LloydsNumber = expectedVessel.ZZO_LloydsNumber;
			expectedVessel.ZZO_ZZZ_NKDataGrouping = "ZA";
			vessels = RefVesselZZ.LookupVesselsByCode(expectedVessel2.ZZO_Code, expectedVessel2.ZZO_ZZZ_NKDataGrouping, Factory);
			AssertEquals("Number of Vessels found with name: " + expectedVessel2.ZZO_Code, 2, vessels.Length);
			AssertCollectionContains("Finds expectedVessel", expectedVessel, vessels);
			AssertCollectionContains("Finds expectedVessel", expectedVessel2, vessels);

			AssertEquals(null, RefVessel.LookupVesselByCode("COLUMBUS OLIVOS YY", Factory));
		}

		public void TestGetCachedVesselByCode()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			RefVesselZZ expectedVessel = Factory.New<RefVesselZZ>();
			expectedVessel.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel.ZZO_LloydsNumber = expectedVessel.ZZO_LloydsNumber;
			expectedVessel.ZZO_ZZZ_NKDataGrouping = "ZA";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			RefVesselZZ.GetCachedVesselByCode(expectedVessel.ZZO_Code, factory1);
			factory1.ClearQueryCache();
			RefVesselZZ.GetCachedVesselByCode(expectedVessel.ZZO_Code, factory1);
			factory1.ClearQueryCache();
			AssertEquals(1, factory1.DatabaseLoadCount);

			RefVesselZZ.LookupVesselByCode(expectedVessel.ZZO_Code, expectedVessel.ZZO_ZZZ_NKDataGrouping, factory1);
			AssertEquals(2, factory1.DatabaseLoadCount);

			var factory2 = new BusinessObjectFactory();
			RefVesselZZ.GetCachedVesselByCode(expectedVessel.ZZO_Code, factory2);
			AssertEquals("New factory, new load.", 1, factory2.DatabaseLoadCount);
		}

		public void TestGetCachedVesselsByCode()
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, Factory);
			RefVesselZZ expectedVessel = Factory.New<RefVesselZZ>();
			expectedVessel.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel.ZZO_LloydsNumber = expectedVessel.ZZO_LloydsNumber;
			expectedVessel.ZZO_ZZZ_NKDataGrouping = "ZA";
			expectedVessel.ZZO_RadioCallSign = "1";
			RefVesselZZ expectedVessel2 = Factory.New<RefVesselZZ>();
			expectedVessel2.ZZO_Code = "COLUMBUS OLIVOS ZZ";
			expectedVessel2.ZZO_LloydsNumber = expectedVessel2.ZZO_LloydsNumber;
			expectedVessel2.ZZO_ZZZ_NKDataGrouping = "ZA";
			expectedVessel2.ZZO_RadioCallSign = "2";
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			RefVesselZZ.GetCachedVesselsByCode(expectedVessel.ZZO_Code, factory1);
			factory1.ClearQueryCache();
			RefVesselZZ.GetCachedVesselsByCode(expectedVessel.ZZO_Code, factory1);
			factory1.ClearQueryCache();
			AssertEquals(1, factory1.DatabaseLoadCount);

			RefVesselZZ.LookupVesselsByCode(expectedVessel.ZZO_Code, expectedVessel.ZZO_ZZZ_NKDataGrouping, factory1);
			AssertEquals(2, factory1.DatabaseLoadCount);

			var factory2 = new BusinessObjectFactory();
			RefVesselZZ.GetCachedVesselsByCode(expectedVessel.ZZO_Code, factory2);
			AssertEquals("New factory, new load.", 1, factory2.DatabaseLoadCount);
		}

		public void TestHumanReadableNameCore()
		{
			var vessel = Factory.NewWithValidTestData<RefVesselZZ>();
			vessel.ZZO_Code = "ABC";
			vessel.ZZO_LloydsNumber = "12345";

			AssertEquals("Global Vessels - ABC - 12345", vessel.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			MasterFilesTestHelper.CheckDataGroupingAndCreateIfNeeded(Core.Constants.CountryCodes.SouthAfrica, factory);
			return base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
