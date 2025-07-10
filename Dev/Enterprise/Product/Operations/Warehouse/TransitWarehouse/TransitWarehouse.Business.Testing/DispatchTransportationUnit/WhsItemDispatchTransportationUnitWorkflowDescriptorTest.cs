using System.Linq;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchTransportationUnitWorkflowDescriptor))]
	class WhsItemDispatchTransportationUnitWorkflowDescriptorTest : WhsItemHeaderWorkflowDescriptorTest<WhsItemDispatchTransportationUnitWorkflowDescriptor, WhsItemDispatchTransportationUnit>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Dispatch Transportation Unit", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitDispatchTransportationUnit, WorkflowDescriptor.Code);
		}

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override string WorkflowDescriptorType => WorkflowDescriptors.TransitDispatchTransportationUnit;

		protected override SchemaGuidColumn WarehouseColumnName => WhsItemDispatchTransportationUnitSchema.WDH_WW_Warehouse;

		#endregion

		#region TestIncludeWorkflowTriggerActionXMLDebtorBalance

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		#endregion

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult
		{
			get { return true; }
		}

		#endregion

		#region TestRequiresWarehouse

		protected override WarehouseCollectionType WarehouseTypeExpectedResult => WarehouseCollectionType.TransitWarehouse;

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			((IWhsWarehouseInternals)warehouse).CreateDefaultArea();
			var row = warehouse.Rows.AddNew();
			row.WR_Columns = 1;
			row.WR_Levels = 1;
			row.WR_Trays = 1;
			row.WR_Name = "A";
			((IWhsWarehouseInternals)warehouse).GenerateLocations();

			var header = Factory.NewWithValidTestData<WhsItemDispatchTransportationUnit>();
			header.WDH_WW_Warehouse = warehouse.PK;

			return new IWorkflowProvider[] { header };
		}

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitDispTranspUnt, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties =>
			MessageRecipientPartyType.Warehouse |
			MessageRecipientPartyType.OrgProxy |
			MessageRecipientPartyType.TransportCo |
			MessageRecipientPartyType.CustomsOutturnAgent |
			MessageRecipientPartyType.Forwarder |
			MessageRecipientPartyType.GateManagement |
			MessageRecipientPartyType.Email;

		#endregion

		#region TestSupportsSetFieldTriggerAction

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			var dtu = (WhsItemDispatchTransportationUnit)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				var helper = new WhsTransitTestHelper(Factory);

				var dll = helper.CreateDispatchLoadList("dll001", dtu.Warehouse.PK);
				helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

				var bookingParty = helper.CreateClient("ORG1", "Org1", "NJ", "JY");
				var dcn = helper.CreateDispatchConsignment("dcn001", dtu.Warehouse.PK, bookingParty);
				dcn.BookingPartyDocAddress.E2_OA_Address = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptors.TransitDispatchTransportationUnit).MainAddress.PK;

				var rcn = helper.CreateReceiveConsignment("rcn001", dtu.Warehouse.PK);
				var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rcn);
				var package = helper.PackingHelper.CreatePackage(packageJob, "pkg001", 1, "PLT");
				helper.CreatePackageState(package, TransitWarehouseStatuses.Codes.Booked, rcn, dispatchConsignment: dcn, dispatchLoadList: dll, location: dtu.Warehouse.DefaultLocation);

				Factory.Save();
			}
		}

		#endregion

		public void TestMessageRecipientPartyWhenPackageDCNIsEmpty()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var warehouse = helper.CreateWarehouse("TRW", true);

			var dtu = helper.CreateDispatchTransportationUnit("DTU001", warehouse.PK);
			var dll = helper.CreateDispatchLoadList("dll001", dtu.Warehouse.PK);
			helper.CreateDispatchDLLDTUPivot(dll.PK, dtu.PK);

			var bookingParty = helper.CreateClient("ORG1", "Org1", "NJ", "JY");
			var dcn = helper.CreateDispatchConsignment("dcn001", dtu.Warehouse.PK, bookingParty);
			dcn.BookingPartyDocAddress.E2_OA_Address = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptors.TransitDispatchTransportationUnit).MainAddress.PK;

			var rcn = helper.CreateReceiveConsignment("rcn001", dtu.Warehouse.PK);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			var packageWithoutDCN = helper.PackingHelper.CreatePackage(packageJob, "pkg001", 1, "PLT");
			helper.CreatePackageState(packageWithoutDCN, TransitWarehouseStatuses.Codes.Booked, rcn, dispatchLoadList: dll, location: dtu.Warehouse.DefaultLocation);
			var packageWithDCN = helper.PackingHelper.CreatePackage(packageJob, "pkg001", 1, "PLT");
			helper.CreatePackageState(packageWithDCN, TransitWarehouseStatuses.Codes.Booked, rcn, dispatchConsignment: dcn, dispatchLoadList: dll, location: dtu.Warehouse.DefaultLocation);

			Factory.Save();

			var descriptor = new WhsItemDispatchTransportationUnitWorkflowDescriptor();
			AssertEquals("Should not return error and get right number.", 1, descriptor.GetMessageRecipientParty(dtu, MessageRecipientPartyTypeList.Codes.Forwarder).Count());
		}

		#region TestSupportsSetFieldTriggerAction

		public void TestSupportsAutoRateCostsAndRevenue()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsAutoRateCostsAndRevenue);
		}

		#endregion
	}
}
