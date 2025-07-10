using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UpdateOrgPartRelationStrategy_OrgSupplierPartUpdate))]
	class UpdateOrgPartRelationStrategy_OrgSupplierPartUpdateTest : TestCaseWithFactory
	{
		public void TestUpdateOrgPartRelationStrategy_OrgSupplierPartUpdate()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "P1";
			product.OP_StockKeepingUnit = "UNT";

			var strategy =
				ObjectFactory.Get<IUpdateOrgPartRelationStrategy_OrgSupplierPartUpdate>() as
					IDeferTriggerConditionStrategy;

			AssertEquals("Should not defer on insertion of new record", false, strategy.ShouldDeferTrigger(product));

			Factory.Save();

			product.OP_StockKeepingUnit = "BOX";

			AssertEquals("Should defer on StockKeepingUnit update", true, strategy.ShouldDeferTrigger(product));

			Factory.Save();

			product.OP_Brand = "Test";
			AssertEquals("Should not defer on update of irrelevant column", false, strategy.ShouldDeferTrigger(product));
		}
	}
}
