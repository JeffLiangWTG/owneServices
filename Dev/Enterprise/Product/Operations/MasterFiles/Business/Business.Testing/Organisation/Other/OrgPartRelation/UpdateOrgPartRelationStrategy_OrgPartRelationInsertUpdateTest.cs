using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate))]
	class UpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdateTest : TestCaseWithFactory
	{
		public void TestUpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "C1";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "P1";
			product.OP_StockKeepingUnit = "UNT";

			var strategy =
				ObjectFactory.Get<IUpdateOrgPartRelationStrategy_OrgPartRelationInsertUpdate>() as IDeferTriggerConditionStrategy;

			Factory.Save();

			var clientProductRelation = Factory.New<OrgPartRelation>();
			clientProductRelation.OU_OH = client.PK;
			clientProductRelation.OU_OP = product.PK;
			clientProductRelation.OU_Relationship = "OWN";
			clientProductRelation.OU_ClientUQ = "BOX";

			AssertEquals("Should defer on insertion of new record", true, strategy.ShouldDeferTrigger(clientProductRelation));

			Factory.Save();

			clientProductRelation.OU_ClientUQ = "UNT";
			AssertEquals("Should defer on OU_ClientUQ update", true, strategy.ShouldDeferTrigger(clientProductRelation));

			Factory.Save();

			clientProductRelation.OU_UnitPrice = 2m;
			AssertEquals("Should not defer on irrelevant column changes", false, strategy.ShouldDeferTrigger(clientProductRelation));
		}
	}
}
