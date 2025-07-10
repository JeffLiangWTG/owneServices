using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemDispatchLoadListWorkflowDescriptor))]
	public class WhsItemDispatchLoadListWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsItemDispatchLoadListWorkflowDescriptor>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Dispatch Load List", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitDispatchLoadList, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestIncludeWorkflowTriggerActionXMLDebtorBalance

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		#endregion

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult => true;

		protected override WarehouseCollectionType WarehouseTypeExpectedResult => WarehouseCollectionType.TransitWarehouse;

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportsBufferManagement

		public void TestSupportsBufferManagement()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsBufferManagement);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsItemDispatchLoadList>() };

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitDspLoadList, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties =>
			MessageRecipientPartyType.Warehouse |
			MessageRecipientPartyType.OrgProxy |
			MessageRecipientPartyType.Email;

		#endregion

		#region SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);

			var loadList = (WhsItemDispatchLoadList)workflowProvider;
			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.Warehouse)
			{
				loadList.Warehouse.WW_OA_WarehouseAddress = CreateOrgHeaderAndSetupEDICommunications().MainAddress.PK;
			}
		}

		OrgHeader CreateOrgHeaderAndSetupEDICommunications()
		{
			var org = Factory.New<OrgHeader>();
			var orgMode = org.EDICommunicationsModes.AddNew();
			orgMode.EK_Module = WorkflowDescriptors.TransitDispatchLoadList;
			orgMode.EK_FileFormat = "NTF";
			orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
			orgMode.EK_Destination = "@notificationemail.cargowise.com";
			return org;
		}

		#endregion

		#region TestSupportsSetFieldTriggerAction

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		#endregion
	}
}
