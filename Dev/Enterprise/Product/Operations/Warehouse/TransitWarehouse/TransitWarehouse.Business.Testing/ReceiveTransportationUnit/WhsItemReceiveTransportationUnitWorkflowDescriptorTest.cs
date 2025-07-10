using System.Linq;
using CargoWise.Definitions;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveTransportationUnitWorkflowDescriptor))]
	class WhsItemReceiveTransportationUnitWorkflowDescriptorTest : WhsItemHeaderWorkflowDescriptorTest<WhsItemReceiveTransportationUnitWorkflowDescriptor, WhsItemReceiveTransportationUnit>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Receive Transportation Unit", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitReceiveTransportationUnit, WorkflowDescriptor.Code);
		}

		#endregion

		#region WorkflowDescriptorType

		protected override string WorkflowDescriptorType => WorkflowDescriptors.TransitReceiveTransportationUnit;

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
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsItemReceiveTransportationUnit>() };
		}

		protected override SchemaGuidColumn WarehouseColumnName => WhsItemReceiveTransportationUnitSchema.WRH_WW_Warehouse;

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitRecTranspUnt, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

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
			var rtu = (WhsItemReceiveTransportationUnit)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Forwarder)
			{
				var helper = new WhsTransitTestHelper(Factory);

				var bookingParty = helper.CreateClient("ORG1", "Org1", "NJ", "JY");
				var transportCompany = helper.CreateClient("ORG2", "Org2", "NJ", "JY");
				var asn = helper.CreateReceiveASN("asn001", rtu.Warehouse.PK, bookingParty, transportCompany);
				asn.BookingPartyDocAddress.E2_OA_Address = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptors.TransitReceiveTransportationUnit).MainAddress.PK;
				helper.CreateReceiveASNRTUPivot(rtu.PK, asn.PK);
			}
		}

		#endregion

		#region TestSupportsSetFieldTriggerAction

		public void TestSupportsAutoRateCostsAndRevenue()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsAutoRateCostsAndRevenue);
		}

		#endregion
	}
}
