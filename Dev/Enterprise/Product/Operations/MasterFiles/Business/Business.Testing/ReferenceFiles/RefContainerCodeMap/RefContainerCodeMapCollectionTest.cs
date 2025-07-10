using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainerCodeMapCollection))]
	sealed class RefContainerCodeMapCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new RefContainerCodeMapCollection(Factory.NewWithValidTestData<RefContainer>());
		}

		#endregion

		public void TestRelationship()
		{
			var xxxContainer = Factory.NewWithValidTestData<RefContainer>();
			xxxContainer.RC_Code = "XXX";

			var testContainer = Factory.NewWithValidTestData<RefContainer>();
			testContainer.RC_Code = "CD1";

			Factory.NewWithValidTestData<RefContainer>().RC_Code = "CD2";

			var testCodeMap1 = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testCodeMap1.RCM_RN_NKCountry = "US";
			testCodeMap1.RCM_Usage = "";
			testCodeMap1.RCM_RC_Container = testContainer.PK;
			testCodeMap1.RCM_Code = "US1";

			var testCodeMap2 = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testCodeMap2.RCM_RN_NKCountry = "CN";
			testCodeMap2.RCM_Usage = "CUS";
			testCodeMap2.RCM_RC_Container = testContainer.PK;
			testCodeMap2.RCM_Code = "CN1";

			var testCodeMapX = Factory.NewWithValidTestData<RefContainerCodeMap>();
			testCodeMapX.RCM_RN_NKCountry = "CN";
			testCodeMapX.RCM_Usage = "CIQ";
			testCodeMapX.RCM_RC_Container = xxxContainer.PK;
			testCodeMapX.RCM_Code = "CN2";

			Factory.Save();

			var loadedContainer = new BusinessObjectFactory().Load<RefContainer>(testContainer.PK);
			AssertEquals(2, loadedContainer.CodeMapCollection.Count);
			Assert(loadedContainer.CodeMapCollection.Cast<RefContainerCodeMap>().Any(map => map.RCM_Code == "US1" && map.RCM_Usage == "" && map.RCM_RN_NKCountry == "US" && map.RCM_RC_Container == testContainer.PK));
			Assert(loadedContainer.CodeMapCollection.Cast<RefContainerCodeMap>().Any(map => map.RCM_Code == "CN1" && map.RCM_Usage == "CUS" && map.RCM_RN_NKCountry == "CN" && map.RCM_RC_Container == testContainer.PK));
		}

		public void TestAllowNew()
		{
			var testContainer = Factory.NewWithValidTestData<RefContainer>();
			Assert(testContainer.CodeMapCollection.AllowNew);
		}
	}
}
