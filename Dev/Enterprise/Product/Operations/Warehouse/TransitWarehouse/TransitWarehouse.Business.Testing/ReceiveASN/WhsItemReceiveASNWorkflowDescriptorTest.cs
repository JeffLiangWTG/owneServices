using System.Linq;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	[TestedType(typeof(WhsItemReceiveASNWorkflowDescriptor))]
	class WhsItemReceiveASNWorkflowDescriptorTest : WorkflowDescriptorTestCase<WhsItemReceiveASNWorkflowDescriptor>
	{
		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Transit Receive ASN", WorkflowDescriptor.Description);
		}

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals(WorkflowDescriptors.TransitReceiveASN, WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDocumentBusinessContext

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.TransitReceiveASN, WorkflowDescriptor.DocumentBusinessContext.Single());
		}

		#endregion

		#region TestIncludeWorkflowTriggerActionXMLDebtorBalance

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
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

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestSupportedMessageRecipientParties

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.AirCargoResponsibleParty |
					MessageRecipientPartyType.Email;
			}
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
			var asn = (WhsItemReceiveASN)workflowProvider;

			if (partyTypeCode == MessageRecipientPartyTypeList.Codes.BookingParty)
			{
				var helper = new WhsTransitTestHelper(Factory);

				asn.BookingPartyDocAddress.E2_OA_Address = helper.CreateOrgHeaderAndSetupEDICommunications(WorkflowDescriptors.TransitReceiveASN).MainAddress.PK;
			}
		}

		#endregion

		#region GetParentsWithConfiguredOrganisationPartiesForTest

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<WhsItemReceiveASN>() };
		}

		#endregion
	}
}
