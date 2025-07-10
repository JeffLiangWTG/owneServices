using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class WarehouseValidationHelperTest : TestCaseWithFactory
	{
		#region TestCheckWarehouseEligibility

		public void TestCheckWarehouseEligibility()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var whs1 = helper.CreateWarehouse("WH1");
			var whs2 = helper.CreateWarehouse("WH2");
			var contact = Factory.New<OrgContact>();

			helper.ProhibitWarehouseAccessForOrgContact(whs1, contact);

			var dummy = new DummyFilterBizO(Factory);

			dummy.DummyProperty = whs2.PK;
			WarehouseValidationHelper.CheckWarehouseEligibility(dummy.DummyPropertyInfo, contact);
			AssertNoErrors(dummy.DummyPropertyInfo);

			dummy.DummyProperty = whs1.PK;
			WarehouseValidationHelper.CheckWarehouseEligibility(dummy.DummyPropertyInfo, contact);
			AssertHasError("Should have an error.", dummy.DummyPropertyInfo, "You are not authorized for warehouse WH1 (WH1). Please contact your system administrator to request access rights.");
		}

		class DummyFilterBizO : NonPersistentBusinessObject, IObsoleteValidation
		{
			public ZGuid DummyProperty { get; set; }

			public ZPropertyInfo DummyPropertyInfo
			{
				get { return GetZPropertyInfo(nameof(DummyProperty)); }
			}

			public DummyFilterBizO(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override SchemaGuidColumn PKSchemaColumn
			{
				get { return null; }
			}
		}

		#endregion

	}
}
