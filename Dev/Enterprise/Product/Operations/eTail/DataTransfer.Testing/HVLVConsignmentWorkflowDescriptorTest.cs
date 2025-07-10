using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVConsignmentWorkflowDescriptor))]
	public class HVLVConsignmentWorkflowDescriptorTest : WorkflowDescriptorTestCase<HVLVConsignmentWorkflowDescriptor>
	{
		public override void TestID() => AssertEquals(WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode, WorkflowDescriptor.Code);

		public override void TestDescription() => AssertEquals("HVLV Consignment", WorkflowDescriptor.Description);

		public override void TestRequiresBranch() => AssertEquals(true, WorkflowDescriptor.RequiresBranch);

		public override void TestRequiresClient() => AssertEquals(true, WorkflowDescriptor.RequiresClient);

		public void TestClientName() => AssertEquals("eTailer", WorkflowDescriptor.ClientName);

		public override void TestRequiresDepartment() => AssertEquals(false, WorkflowDescriptor.RequiresDepartment);

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public void TestPort1Name() => AssertEquals("Dispatch UNLOCO", WorkflowDescriptor.Port1Name);

		public override void TestSupportsEventTracking() => AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);

		public override void TestSupportsWorkflowTemplates() => AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);

		public void TestSupportsHVLVPreScreening() => AssertEquals(false, WorkflowDescriptor.SupportsHVLVPreScreening);

		public override void TestSubTypes()
		{
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Service Level", WorkflowDescriptor.SubTypeInformation[0].Description);
		}

		public void TestGetWorkflowTriggerAction_ForCFDTriggerType_IsCreateStandAloneDeclarationProcessor()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var standAloneDeclarationTrigger = consignment.WorkflowItems.Triggers.AddNew();
			standAloneDeclarationTrigger.P9_Description = "Create Stand Alone Declaration Trigger";
			standAloneDeclarationTrigger.TriggerConditions.TriggerEventCode = AutoEvents.HVLVReadyCode;
			standAloneDeclarationTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

			var standAloneDeclarationTriggerAction = standAloneDeclarationTrigger.ProcessTaskNotifications.AddNew();
			standAloneDeclarationTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateStandAloneDeclaration;

			var hlrTriggerLog = consignment.Logs.AddNew(AutoEvents.HVLVReady);
			Factory.Save();

			var workflowProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(standAloneDeclarationTriggerAction, new QueuedLogForTesting(hlrTriggerLog, standAloneDeclarationTrigger));
			AssertType("Should return correct processor for CFD Trigger Action", typeof(CreateStandAloneDeclarationProcessor), workflowProcessor);
		}

		public void TestConsignorConsigneeRecipients()
		{
			var headerConsignor = Factory.New<OrgHeader>();
			headerConsignor.Addresses.AddNew();

			var shipmentConsignor1 = Factory.New<OrgHeader>();
			var shipmentConsignor2 = Factory.New<OrgHeader>();
			var shipmentConsignee1 = Factory.New<OrgHeader>();
			var shipmentConsignee2 = Factory.New<OrgHeader>();

			var shipment1 = Factory.New<ForwardingShipment>();
			var shipment2 = Factory.New<ForwardingShipment>();

			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = headerConsignor.Addresses[0].PK;

			var consignment = header.Consignments.AddNew();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment1.PK;
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment2.PK;

			shipment1.ConsignorDocumentaryAddress.OrganisationPK = shipmentConsignor1.PK;
			shipment1.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee1.PK;
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = shipmentConsignor1.PK;
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee1.PK;

			var cnrRecipient = WorkflowDescriptor.GetMessageRecipientParty(consignment, MessageRecipientPartyTypeList.Codes.Consignor).Single();
			var cneRecipient = WorkflowDescriptor.GetMessageRecipientParty(consignment, MessageRecipientPartyTypeList.Codes.Consignee).Single();

			AssertEquals("Only one distinct consignor on all shipments, should use this org", shipmentConsignor1, cnrRecipient.Party);
			AssertEquals("Only one distinct consignee on all shipments, should use this org", shipmentConsignee1, cneRecipient.Party);

			shipment2.ConsignorDocumentaryAddress.OrganisationPK = shipmentConsignor2.PK;
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee2.PK;

			cnrRecipient = WorkflowDescriptor.GetMessageRecipientParty(consignment, MessageRecipientPartyTypeList.Codes.Consignor).Single();
			cneRecipient = WorkflowDescriptor.GetMessageRecipientParty(consignment, MessageRecipientPartyTypeList.Codes.Consignee).SingleOrDefault();

			AssertEquals("Multiple shipment orgs, should use header org", headerConsignor, cnrRecipient.Party);
			AssertNull("Multiple shipment orgs, no recipient", cneRecipient);
		}

		public void TestDocumentBusinessContext()
		{
			AssertContainsExactElementsInAnyOrder(new[] { BusinessContext.HVLVConsignment }, WorkflowDescriptor.DocumentBusinessContext);
		}

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest() => new IWorkflowProvider[] { ConsignmentWithOrganisations };

		HVLVConsignment ConsignmentWithOrganisations => consignmentWithOrganisations ?? (consignmentWithOrganisations = CreateConsignmentWithOrganisations());
		HVLVConsignment consignmentWithOrganisations;

		HVLVConsignment CreateConsignmentWithOrganisations()
		{
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_OH_LastMileCarrier = Factory.NewWithValidTestData<OrgHeader>().PK;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.ConsigneePK = ConsigneeOrg.PK;
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;

			return consignment;
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var result = new List<CodeDescriptionPair>(base.ExpectedAdditionalWorkflowTriggerActionTypes);
				result.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateStandAloneDeclaration, WorkflowTriggerActionTypeConstants.Descriptions.CreateStandAloneDeclaration));
				return result.ToArray();
			}
		}

		#endregion
	}
}
