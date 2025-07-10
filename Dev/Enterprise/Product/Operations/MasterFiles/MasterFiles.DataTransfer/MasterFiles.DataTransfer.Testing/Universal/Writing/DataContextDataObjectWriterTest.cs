using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.CodeMapping;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	class DataContextDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestTriggerDateIsTriggeringEventDate()
		{
			var typeOfForwardingShipment = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipmentBO = Factory.New(typeOfForwardingShipment);
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			var logBO1 = shipmentBO.GetLogs().AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddHours(1), reference: "Test Ref"));
			var logBO2 = shipmentBO.GetLogs().AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddHours(2), reference: "Test Ref"));
			var logBO3 = shipmentBO.GetLogs().AddNew(new EventValue(Events.CustomisableEvent00, eventTime: ZDateTimeOffset.Now.AddHours(3), reference: "Test Ref"));

			var addedLogs = new List<StmALog>() { logBO1, logBO2, logBO3 };

			CombineAssertions(() =>
			{
				foreach (var log in addedLogs)
				{
					var queuedLog = new QueuedLogForTesting(log, trigger);
					queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
					queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", log.PK, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

					var dataSource = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2012_11) as UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
					var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => log));
					var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
					new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataSource, eventInfoProvider);

					AssertEquals(log.SL_EventTimeOffset, dataSource.Workflow.TriggerDate.GetValueOrDefault());
				}
			});
		}

		[TestDate(2011, 2, 1)]
		public void TestGetDataObject_2012_11()
		{
			var typeOfForwardingShipment = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipmentBO = Factory.New(typeOfForwardingShipment);
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "THE CHAMP IS HERE";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code; // The Famous "ATH".
			trigger.TriggerConditions.TriggerConditionValue = "BLOOBER";
			((IWorkflowProvider)shipmentBO).Logs.AddNew(Events.Authorised);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			var dataSource = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2012_11) as UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
			var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataSource, eventInfoProvider);

			CombineAssertions(delegate
			{
				AssertEquals("dataSource.Workflow.ActionPurpose.ToStringContents()", "APP - As Per Payload", dataSource.Workflow.ActionPurpose.ToStringContents());
				AssertEquals("dataSource.Workflow.Company.ToStringContents()", "EDI - Eagle Datamation International", dataSource.Workflow.Company.ToStringContents());
				AssertEquals("dataSource.Workflow.Company.Country.ToStringContents()", "AU - Australia", dataSource.Workflow.Company.Country.ToStringContents());
				AssertEquals("dataSource.DataSource.DataProvider.Code.Value", "EDIDATEDI", dataSource.DataSource.DataProvider.Code.Value);
				AssertEquals("dataSource.DataSource.DataProvider.Type.Value.ToString()", "EnterpriseID", dataSource.DataSource.DataProvider.Type.Value.ToString());
				AssertEquals("dataSource.Workflow.EventReference", "Test Ref", dataSource.Workflow.EventReference);
				AssertEquals("dataSource.Workflow.EventType.ToStringContents()", "ATH - Authorized", dataSource.Workflow.EventType.ToStringContents());
				AssertEquals("dataSource.Workflow.EventUser.ToStringContents()", "E - CargoWise Support", dataSource.Workflow.EventUser.ToStringContents());
				AssertEquals("dataSource.GetDataSources()", "ForwardingShipment [S00001010]", dataSource.GetDataSources());
				AssertEquals("dataSource.Workflow.TriggerCount", 1, dataSource.Workflow.TriggerCount);
				AssertEquals("dataSource.Workflow.TriggerDate", new ZDateTimeOffset(2010, 12, 25), dataSource.Workflow.TriggerDate);
				AssertEquals("dataSource.Workflow.TriggerDescription", "THE CHAMP IS HERE", dataSource.Workflow.TriggerDescription);
				AssertEquals("dataSource.Workflow.TriggerReference", "BLOOBER", dataSource.Workflow.TriggerReference);
				AssertEquals("dataSource.Workflow.TriggerType", TriggerType.Trigger, dataSource.Workflow.TriggerType);
				AssertEquals("dataSource.Workflow.RecipientRoleCollection", "FOR - Forwarder", string.Join("\r\n", dataSource.Workflow.RecipientRoleCollection.Select(o => o.Code + " - " + o.Description).ToArray()));
			});
		}

		[TestDate(2011, 2, 1)]
		public void TestLazyDataKeyGeneration_2012_11()
		{
			var inboundXml =
	$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>PO BOX 2623</Address1>
        <City>DUBAI</City>
        <CompanyName>AL TAYER TRENDS LLC</CompanyName>
        <Country>
          <Code>AE</Code>
        </Country>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>1450 REMINGTON ROAD</Address1>
        <City>BOLINGBROOK</City>
        <CompanyName>SCHWARTZ SUPPLY SOURCE</CompanyName>
        <Country>
          <Code>US</Code>
        </Country>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_MessageText = inboundXml;

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var codeMapper = new CodeMappingManager(xmlSessionTracker);
			var dataMessageFactory = message.GetUniversalDataMessageFactory();
			Assert(dataMessageFactory.TopLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out var topLevelDataObject));

			var universalFactory = new UniversalObjectFactory();
			var shipmentDataContextManager = (IShipmentDataContextManager)ObjectFactory.Get("ForwardingShipmentDataContextManager");

			topLevelDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.TPC } } });
			xmlSessionTracker.IndividualImportBegin(shipmentDataContextManager.DataContextType);
			Assert(shipmentDataContextManager.UseIncomingShipmentData(topLevelDataObject, xmlSessionTracker, universalFactory));
			xmlSessionTracker.IndividualImportEnd();

			var shipmentBO = (BusinessObject)universalFactory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery()).Single();
			AssertEquals(false, shipmentBO.IsInDatabase);

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "THE CHAMP IS HERE";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code; // The Famous "ATH".
			trigger.TriggerConditions.TriggerConditionValue = "BLOOBER";
			((IWorkflowProvider)shipmentBO).Logs.AddNew(Events.Authorised);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.TransportCo;

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			var dataContext = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2012_11) as UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
			var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataContext, eventInfoProvider);
			AssertEquals("Without a save of the import job, we should not have a shipment number", "ForwardingShipment []", dataContext.GetDataSources());

			universalFactory.SaveForTesting();
			dataContext = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2012_11) as UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;
			actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataContext, eventInfoProvider);
			AssertEquals("Once saved, the lazy evalutation should provide the correct shipment number", "ForwardingShipment [S00001000]", dataContext.GetDataSources());
		}

		[TestDate(2011, 2, 5)]
		public void TestGetDataObject_2011_11()
		{
			var typeOfForwardingShipment = ObjectFactory.GetType<Forwarding.IForwardingShipment>();
			var shipmentBO = Factory.New(typeOfForwardingShipment);
			shipmentBO[JobShipmentSchema.JS_UniqueConsignRef] = "S00001010";

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "THE CHAMP IS HERE";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code; // The Famous "ATH".
			trigger.TriggerConditions.TriggerConditionValue = "BLOOBER";
			trigger.P9_ScheduledDateForBinding = new ZDateTimeOffset(new ZDateTime(2011, 2, 5));
			((IWorkflowProvider)shipmentBO).Logs.AddNew(Events.Authorised);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			var dataSource = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11) as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataSource, eventInfoProvider);

			CombineAssertions(delegate
			{
				AssertEquals("dataSource.ActionPurpose.ToStringContents()", "APP - As Per Payload", dataSource.ActionPurpose.ToStringContents());
				AssertEquals("dataSource.Company.ToStringContents()", "EDI - Eagle Datamation International", dataSource.Company.ToStringContents());
				AssertEquals("dataSource.Company.Country.ToStringContents()", "AU - Australia", dataSource.Company.Country.ToStringContents());
				AssertEquals("dataSource.EnterpriseID", "EDI", dataSource.EnterpriseID);
				AssertEquals("dataSource.EventReference", "Test Ref", dataSource.EventReference);
				AssertEquals("dataSource.EventType.ToStringContents()", "ATH - Authorized", dataSource.EventType.ToStringContents());
				AssertEquals("dataSource.EventUser.ToStringContents()", "E - CargoWise Support", dataSource.EventUser.ToStringContents());
				AssertEquals("dataSource.GetDataSources()", "ForwardingShipment [S00001010]", dataSource.GetDataSources());
				AssertEquals("dataSource.ServerID", "DAT", dataSource.ServerID);
				AssertEquals("dataSource.TriggerCount", 1, dataSource.TriggerCount);
				AssertEquals("dataSource.TriggerDate", new ZDateTimeOffset(2010, 12, 25), dataSource.TriggerDate);
				AssertEquals("dataSource.TriggerDescription", "THE CHAMP IS HERE", dataSource.TriggerDescription);
				AssertEquals("dataSource.TriggerReference", "BLOOBER", dataSource.TriggerReference);
				AssertEquals("dataSource.TriggerType", TriggerType.Trigger, dataSource.TriggerType);
				AssertEquals("dataSource.RecipientRoleCollection", "ORP - Organization Proxy", string.Join("\r\n", dataSource.RecipientRoleCollection.Select(o => o.Code + " - " + o.Description).ToArray()));
			});
		}

		[TestDate(2011, 2, 1)]
		public void TestLazyDataKeyGeneration_2011_11()
		{
			var inboundXml =
	$@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>ForwardingShipment</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <IsForwardRegistered>true</IsForwardRegistered>
    <GoodsDescription>WINNER WINNER CHICKEN DINNER</GoodsDescription>
    <PortOfDischarge>
      <Code>AEJEA</Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>USORF</Code>
    </PortOfLoading>
    <ReleaseType>
      <Code>CSH</Code>
      <Description>Company/Cashier Check</Description>
    </ReleaseType>
    <ShipmentType>
      <Code>AGT</Code>
      <Description>Agent</Description>
    </ShipmentType>
    <TotalNoOfPacks>1</TotalNoOfPacks>
    <TotalNoOfPacksPackageType>
      <Code>PKG</Code>
      <Description>Package</Description>
    </TotalNoOfPacksPackageType>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <WayBillNumber>SHOWMETHEWUGGETS</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>ConsigneeDocumentaryAddress</AddressType>
        <Address1>PO BOX 2623</Address1>
        <City>DUBAI</City>
        <CompanyName>AL TAYER TRENDS LLC</CompanyName>
        <Country>
          <Code>AE</Code>
        </Country>
      </OrganizationAddress>
      <OrganizationAddress>
        <AddressType>ConsignorDocumentaryAddress</AddressType>
        <Address1>1450 REMINGTON ROAD</Address1>
        <City>BOLINGBROOK</City>
        <CompanyName>SCHWARTZ SUPPLY SOURCE</CompanyName>
        <Country>
          <Code>US</Code>
        </Country>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_MessageText = inboundXml;

			var xmlSessionTracker = new XmlSessionTracker(new SimpleLogger());
			var codeMapper = new CodeMappingManager(xmlSessionTracker);
			var dataMessageFactory = message.GetUniversalDataMessageFactory();
			Assert(dataMessageFactory.TopLevelDataObjectFactory.TryGetTopLevelDataObject(message, codeMapper, xmlSessionTracker, out var topLevelDataObject));

			var universalFactory = new UniversalObjectFactory();
			var shipmentDataContextManager = (IShipmentDataContextManager)ObjectFactory.Get("ForwardingShipmentDataContextManager");

			topLevelDataObject.DataContext.SetWorkflowInfo(new WorkflowInfo { RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.TPC } } });
			xmlSessionTracker.IndividualImportBegin(shipmentDataContextManager.DataContextType);
			Assert(shipmentDataContextManager.UseIncomingShipmentData(topLevelDataObject, xmlSessionTracker, universalFactory));
			xmlSessionTracker.IndividualImportEnd();

			var shipmentBO = (BusinessObject)universalFactory.BOFactory.Load<Forwarding.IForwardingShipment>(new ZQuery()).Single();
			AssertEquals(false, shipmentBO.IsInDatabase);

			var trigger = ((IWorkflowProvider)shipmentBO).WorkflowItems.AddNew();
			trigger.P9_Description = "THE CHAMP IS HERE";
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
			trigger.TriggerConditions.TriggerEventCode = Events.Authorised.Code; // The Famous "ATH".
			trigger.TriggerConditions.TriggerConditionValue = "BLOOBER";
			((IWorkflowProvider)shipmentBO).Logs.AddNew(Events.Authorised);

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.TransportCo;

			var logBO = shipmentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));

			var queuedLog = new QueuedLogForTesting(logBO, trigger);
			queuedLog.SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			queuedLog.SJ_Reference = string.Format("{0}|{1}|{2}", logBO.PK, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment);

			var dataContext = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11) as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			var actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataContext, eventInfoProvider);
			AssertEquals("Without a save of the import job, we should not have a shipment number", "ForwardingShipment []", dataContext.GetDataSources());

			universalFactory.SaveForTesting();
			dataContext = DataContextFactory.New(shipmentBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11) as UniversalDataBuss.DataObjects.Universal._2011_11.DataContext;
			actionWrapper = new ActionWrapper(action, shipmentBO, Lazy.Create<IStmALog>(() => logBO));
			eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, shipmentBO);
			new DataContextDataObjectWriter().PopulateDataObject(actionWrapper, dataContext, eventInfoProvider);
			AssertEquals("Once saved, the lazy evalutation should provide the correct shipment number", "ForwardingShipment [S00001000]", dataContext.GetDataSources());
		}
	}
}
