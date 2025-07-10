using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class OrgSupplierPartFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForView()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TESTORG1";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "TESTORG2";
			Factory.Save();

			var collection = CreateCollectionToTest(Factory);
			var supplier1 = (OrgSupplierPart)collection.AddNew();
			supplier1.OP_PartNum = "111111";
			supplier1.OP_StockKeepingUnit = "";
			supplier1.RelatedOrganisations[0].OU_OH = org1.PK;
			var supplier2 = (OrgSupplierPart)collection.AddNew();
			supplier2.OP_PartNum = "222222";
			supplier2.OP_StockKeepingUnit = "";
			supplier2.RelatedOrganisations[0].OU_OH = org2.PK;
			Factory.Save();
			TestFetchForView(supplier1, supplier2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => new OrgSupplierPartCollection(factory);
	}
}
