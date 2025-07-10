using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(AdjustmentFilterBusinessObject))]
	public class AdjustmentFilterBusinessObjectTest : DocketFilterBusinessObjectTest<AdjustmentFilterBusinessObject, WhsAdjustment>
	{
		#region TestAdjustmentReasonFilter

		public void TestAdjustmentReasonFilter()
		{
			var client = Helper.CreateClient();
			var product = Helper.CreateProduct(client, "BOX");
			var warehouse = Helper.CreateWarehouse("BIG WAREHOUSE", "A", 5, 10);
			Factory.Save();

			var adjustmentWithoutLines = Helper.CreateWhsAdjustment(client, warehouse, "ADJ1");
			var adjustmentWithDifferentReasonCodes = Helper.CreateWhsAdjustment(client, warehouse, "ADJ2");
			var adjustmentLine1 = Helper.CreateWhsAdjustmentLine(adjustmentWithDifferentReasonCodes, product, 10m, "A-1-1");
			adjustmentLine1.WE_ReasonCode = "SHR";
			var correctAdjustment = Helper.CreateWhsAdjustment(client, warehouse, "ADJ3");
			var adjustmentLine2 = Helper.CreateWhsAdjustmentLine(correctAdjustment, product, 10m, "A-1-1");
			adjustmentLine2.WE_ReasonCode = "STA";

			Factory.Save();

			var adjustmentReasonFilter = (ModuleTextBaseFilter)FilterStripBizO["Reason Code"];
			adjustmentReasonFilter.Property = "STA";
			AssertEquals("Reason Code", adjustmentReasonFilter.MultilingualDescription);
			AssertEquals(FilterCategories.NumbersAndReferences, adjustmentReasonFilter.Category);

			var adjustments = Factory.Load<WhsAdjustment>(adjustmentReasonFilter.Query);
			AssertEquals(1, adjustments.Length);
			AssertEquals(correctAdjustment.PK, adjustments[0].PK);

			adjustmentReasonFilter.Property = "";

			adjustments = Factory.Load<WhsAdjustment>(adjustmentReasonFilter.Query);
			AssertContainsExactElementsInAnyOrder(new WhsAdjustment[] { adjustmentWithoutLines, adjustmentWithDifferentReasonCodes, correctAdjustment }, adjustments);
		}

		#endregion

		#region TestAdjustmentTypeFilter

		public void TestAdjustmentTypeFilter()
		{
			var filter = (ModuleTextFilter)DocketFilter["Adjustment Type"];
			filter.IsActive = true;

			SetupTestData();
			Docket11.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			Docket12.WD_DocketSubType = AdjustmentType.Codes.Customs;
			Docket21.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			Docket22.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;

			filter.Property = AdjustmentType.Codes.Adjustment;
			DocketAssert(true, false, false, false);

			filter.Property = AdjustmentType.Codes.Customs;
			DocketAssert(false, true, false, false);

			filter.Property = AdjustmentType.Codes.InternalWarehouseAdjustment;
			DocketAssert(false, false, true, false);

			filter.Property = AdjustmentType.Codes.OwnershipAdjustment;
			DocketAssert(false, false, false, true);

			filter.Property = "JGU";
			DocketAssert(false, false, false, false);
		}

		public void TestAdjustmentTypeFilter_ValidAdjustmentTypes()
		{
			var filter = (ModuleTextFilter)DocketFilter["Adjustment Type"];
			filter.IsActive = true;

			AssertContainsExactElementsInAnyOrder(new AdjustmentType(), filter.List);
		}

		#endregion

		#region Overrides

		protected override bool HasAdditionalReferences => false;

		protected override bool SupportsContainerNoFilter => false;

		protected override bool SupportsCustomerReferenceFilter => false;

		protected override bool SupportsTransportReferenceFilter => false;

		#endregion

		#region WorkflowDescriptorCode

		protected override string WorkflowDescriptorCode => WorkflowDescriptors.WhsAdjustmentWorkflowDescriptorCode;

		#endregion

		#region Implementation

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsAdjustment(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			return Helper.CreateWhsAdjustmentLine((WhsAdjustment)docket, part, units, docket.Warehouse.Rows[0].Locations[0]);
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return new WhsAdjustmentCollection(Factory);
		}

		protected override bool SupportsFilterForErrorStatus => false;

		protected override WhsAdjustment GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null)
			=> throw new System.NotSupportedException("Adjustment does not support Filter by Transport Co.");

		#region Docket Status Filter

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = new DocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.New));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Picking));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.AttachedToPick));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			return status;
		}

		#endregion

		#endregion
	}
}
