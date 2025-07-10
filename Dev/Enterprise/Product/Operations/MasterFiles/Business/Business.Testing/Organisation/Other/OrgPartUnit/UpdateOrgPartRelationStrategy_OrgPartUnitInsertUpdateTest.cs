using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate))]
	class UpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdateTest : TestCaseWithFactory
	{
		public void TestUpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate()
		{
			var client = Factory.New<OrgHeader>();
			client.OH_Code = "C1";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "P1";
			product.OP_StockKeepingUnit = "UNT";

			var clientProductRelation = Factory.New<OrgPartRelation>();
			clientProductRelation.OU_OH = client.PK;
			clientProductRelation.OU_OP = product.PK;
			clientProductRelation.OU_Relationship = "OWN";
			clientProductRelation.OU_ClientUQ = "BOX";

			Factory.Save();

			var partUnit = Factory.New<OrgPartUnit>();
			partUnit.OF_OP = product.PK;
			partUnit.OF_PackType = "UNT";
			partUnit.OF_ParentPackType = "CAS";
			partUnit.OF_QuantityInParent = 10m;

			var strategy =
				ObjectFactory.Get<IUpdateOrgPartRelationStrategy_OrgPartUnitInsertUpdate>() as IDeferTriggerConditionStrategy;

			AssertEquals("Should defer on insertion of new record", true, strategy.ShouldDeferTrigger(partUnit));

			Factory.Save();

			partUnit.OF_PackType = "BOX";
			AssertEquals("Should defer on OF_PackType update", true, strategy.ShouldDeferTrigger(partUnit));

			partUnit.OF_ParentPackType = "CAS";
			AssertEquals("Should defer on OF_ParentPackType update", true, strategy.ShouldDeferTrigger(partUnit));

			partUnit.OF_QuantityInParent = 2m;
			AssertEquals("Should defer on OF_QuantityInParent update", true, strategy.ShouldDeferTrigger(partUnit));

			Factory.Save();

			partUnit.OF_Depth = 6m;
			AssertEquals("Should not defer on irrelevant column changes", false, strategy.ShouldDeferTrigger(partUnit));
		}
	}
}
