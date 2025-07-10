using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(EDIMessageDeliveryContextSelector))]
	public class EDIMessageDeliveryContextSelectorTest : EnterpriseBusinessObjectTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
		}

		EDIMessageDeliveryContextSelector MakeContextSelector(string code, string processType, string description = "blah")
		{
			var selector = Factory.New<EDIMessageDeliveryContextSelector>();
			selector.ECS_Code = code;
			selector.ECS_Description = description;
			selector.ECS_ProcessType = processType;
			return selector;
		}

		EDIMessageDeliveryContextLine AddLine(EDIMessageDeliveryContextSelector selector, string type, string value, string desc = "")
		{
			var line = selector.Lines.AddNew();
			line.ECL_ContextType = type;
			line.ECL_Value = value;
			line.ECL_Description = desc;
			return line;
		}

		Forwarding.IForwardingShipment MakeTriggeredShipment(EDIMessageDeliveryContextSelector contextSelector, OrgHeader orgToUse, string triggerType)
		{
			var shipment = MakeShipment(contextSelector, orgToUse, triggerType);
			((IStmALogProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent00);
			return shipment;
		}

		Forwarding.IForwardingShipment MakeShipment(EDIMessageDeliveryContextSelector contextSelector, OrgHeader orgToUse, string triggerType)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = orgToUse.PK;

			((BusinessObject)shipment).FillWithValidTestData();
			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.P9_Description = "BLAH";
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = triggerType;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			notification.PQ_ECS_MessageDeliveryContextSelector = contextSelector.PK;
			return shipment;
		}

		OrgHeader MakeOrgWithCommunicationModes(string module, string fileFormat)
		{
			var orgToUse = Factory.NewWithValidTestData<OrgHeader>();
			AddCommunicationModeToOrg(orgToUse, module, fileFormat);
			return orgToUse;
		}

		void AddCommunicationModeToOrg(OrgHeader orgToUse, string module, string fileFormat)
		{
			var communicationsMode = orgToUse.EDICommunicationsModes.AddNew();
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = fileFormat;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Module = module;
		}

		#endregion

		public void TestDelete()
		{
			var selector = Factory.New<EDIMessageDeliveryContextSelector>();
			selector.ECS_Code = "ABC";
			selector.ECS_ProcessType = "ABC";
			selector.ECS_Description = "ABC";
			var line1 = selector.Lines.AddNew();
			var line2 = selector.Lines.AddNew();

			selector.Delete();
			AssertEquals(true, line1.IsDeleted);
			AssertEquals(true, line2.IsDeleted);
		}

		public void TestEndToEnd_XUE()
		{
			AssertEndToEndTest(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);
		}

		void AssertEndToEndTest(string triggerType, string fileFormat)
		{
			var selector = MakeContextSelector("AAA", "AAA");
			AddLine(selector, "TYPE", "<MACRO>");
			AddLine(selector, "Goods", "<JS_GoodsDescription>", "This is a description\"");
			AddLine(selector, "Ref", "<JS_UniqueConsignRef>", "This is a description");
			AddLine(selector, "Ref&<>", "BLAH \uFFFE&<", "& <>");// \uFFFE is a invalid xml char and gets removed by the xml write

			var orgToUse = MakeOrgWithCommunicationModes("SHP", fileFormat);
			var shipment = MakeTriggeredShipment(selector, orgToUse, triggerType);
			shipment.JS_GoodsDescription = "This & That <>";
			shipment.JS_UniqueConsignRef = "S00002220";
			Factory.Save();

			var logs = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(1, messages.Length);
			AssertContains(@"
    <ContextCollection>
      <Context>
        <Type>TYPE</Type>
        <Value></Value>
      </Context>
      <Context>
        <Type Description=""This is a description&quot;"">Goods</Type>
        <Value>This &amp; That &lt;&gt;</Value>
      </Context>
      <Context>
        <Type Description=""This is a description"">Ref</Type>
        <Value>S00002220</Value>
      </Context>
      <Context>
        <Type Description=""&amp; &lt;&gt;"">Ref&amp;&lt;&gt;</Type>
        <Value>BLAH &amp;&lt;</Value>
      </Context>
    </ContextCollection>",
				messages[0].EM_MessageText);

			AssertContains("Assert logs are here, full log text is tested in EDIMessageDeliveryContextApplicatorTest",
				"Warning: [WorkflowEventTrigger] [Default] Error evaluating Additional", logs);
		}

		public void TestTriggerActionsWithDuplicateMessageDeliverySelectors()
		{
			var selector = MakeContextSelector("AAA", "AAA");
			var selector2 = MakeContextSelector("BBB", "BBB", "BBB");
			AddLine(selector, "TYPE", "BLAH");
			AddLine(selector2, "TYPE", "BLAH");

			var orgToUse = MakeOrgWithCommunicationModes("SHP", WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML);
			var shipment = MakeShipment(selector, orgToUse, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent);

			var trigger = ((IWorkflowProvider)shipment).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent01Code;
			trigger.P9_Description = "BLAH";
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			action1.PQ_ECS_MessageDeliveryContextSelector = selector.PK;
			var action1duplicate = trigger.ProcessTaskNotifications.AddNew();
			action1duplicate.PQ_TriggerType = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			action1duplicate.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			action1duplicate.PQ_ECS_MessageDeliveryContextSelector = selector.PK;
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignor;
			action2.PQ_ECS_MessageDeliveryContextSelector = selector2.PK;

			((IStmALogProvider)shipment).Logs.AddNew(AutoEvents.CustomisableEvent01);

			Factory.Save();

			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			var messages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("action1Duplicate does not deliver a message as it is a duplicate of action1", 2, messages.Length);
		}
	}
}
