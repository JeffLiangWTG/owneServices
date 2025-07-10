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
	[TestedType(typeof(DynamicWorkOrderFilterBusinessObject))]
	public class DynamicWorkOrderFilterBusinessObjectTest : PickableDocketFilterBusinessObjectTest<DynamicWorkOrderFilterBusinessObject>
	{
		#region Overrides

		protected override bool SupportsContainerNoFilter => false;
		protected override bool SupportsCustomerReferenceFilter => false;
		protected override bool SupportsTransportReferenceFilter => false;

		public override void TestFilterDocketReferenceMatchesExternalReferenceEvenWithNoDocketReferences()
		{
			Assert("incomplete test", true);
		}

		protected override bool SupportsFilterForEnteredStatus => false;

		public override void TestFilterEntryKey()
		{
			Assert("Non-empty bonded entry keys are not supported on dynamic work orders (Constraint_CustomsSpecificFields)", true);
		}

		protected override ZString ExternalReferenceFieldName => "Dynamic Work Order No.";

		protected override CodeDescriptionPairList GetExpectedDocketStatus()
		{
			var status = base.GetExpectedDocketStatus();
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Putaway));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Held));
			status.RemoveAt(status.IndexOfCode(DocketStatus.Codes.Cancelled));
			return status;
		}

		#endregion

		#region Status Filter - UnfinalisedFilterType

		public void TestStatusesFilter_UNF_DoNotIncludeCancelledDocket()
		{
			var client = Helper.CreateClient("C1");
			var whs1 = Helper.CreateWarehouse("W1");
			var whs2 = Helper.CreateWarehouse("W2");
			var docket1 = Helper.CreateWhsDynamicWorkOrder(client, whs1, "D1");
			var docket2 = Helper.CreateWhsDynamicWorkOrder(client, whs2, "D2");

			Factory.Save();

			var pick = Helper.CreatePickNew();
			pick.WP_WW_Whs = whs2.PK;
			docket2.WD_WP = pick.PK;
			docket2.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket2.WD_FinalisedDate = ZDateTimeOffset.Today;

			Factory.Save();

			var docketCollection = GetDocketCollection();
			var filter = new DynamicWorkOrderFilterBusinessObject();
			var statusFilter = (ModuleTextFilter)filter[GetStatusFilterCaption()];
			docketCollection.AdditionalFilter = statusFilter.Query;
			AssertContainsExactElementsInAnyOrder("Pre-condition: no status filter property, should get every docket including finalised.", new[] { docket1, docket2 }, docketCollection);

			statusFilter.Property = (ZString)StatusFilterTypes.UnfinalisedFilterType;
			docketCollection.AdditionalFilter = statusFilter.Query;
			AssertContainsExactElementsInAnyOrder("status filter property is Un-Finalised, should not include finalised docket.", new[] { docket1 }, docketCollection);
		}

		#endregion

		#region Overrides

		public override void TestFilterByConsignee()
		{
			Assert("Dynamic Work Order does not support Filter by Consignee.", true);
		}

		protected override bool SupportsDeliveryRouteFilters => false;

		protected override bool SupportsAccountingFilters => false;

		protected override bool SupportsCustomAttribFilters => false;

		protected override WhsPickableDocket GetDocketForTransportCoFilterTesting(TestDataSimpleEnvironment data, string docketReference, OrgSupplierPart product, OrgHeader transportCo = null) => throw new System.NotSupportedException("Dynamic Work Order does not support Filter by Transport Co.");

		#endregion

		#region Implementation

		protected override CodeDescriptionPairList ValidOrderTypes => new DynamicWorkOrderType();

		protected override WhsDocket CreateDocket(OrgHeader org, WhsWarehouse whs, string @ref)
		{
			return Helper.CreateWhsDynamicWorkOrder(org, whs, @ref);
		}

		protected override WhsDocketLine CreateDocketLine(WhsDocket docket, OrgSupplierPart part, ZDecimal units)
		{
			return Helper.CreateWhsDynamicWorkOrderLine((WhsDynamicWorkOrder)docket, part, units);
		}

		protected override ActiveBusinessObjectCollection<WhsDocket> GetDocketCollection()
		{
			return new WhsDynamicWorkOrderCollection(Factory);
		}

		#endregion
	}
}
