using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlobalCreditGroupCollection))]
	sealed class GlobalCreditGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GlobalCreditGroupCollection(Factory);
		}

		public void TestOnlyReturnGlobalCredigGroupOrganisations()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAAAAA";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBBBBB";

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "CCCCCC";

			var orgMisc3 = Factory.NewWithValidTestData<OrgMiscServ>();
			orgMisc3.OM_OH = org3.PK;
			orgMisc3.OM_OH_ARGlobalCreditGroup = org1.PK;

			Factory.Save();

			var globalCreditGroupCollection = new GlobalCreditGroupCollection(Factory);
			AssertEquals("Collection Count", 0, globalCreditGroupCollection.Count);

			globalCreditGroupCollection.Load();
			Assert("Collection Count >0", globalCreditGroupCollection.Count > 0);
			Assert("Collection does not contain org2", !globalCreditGroupCollection.Contains(org2));
			Assert("Collection does contain org1", globalCreditGroupCollection.Contains(org1));
		}
	}
}
