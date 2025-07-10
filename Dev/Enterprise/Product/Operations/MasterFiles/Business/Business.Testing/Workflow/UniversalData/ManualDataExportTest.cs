using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.MasterFiles.Business.UniversalData.Testing
{
	[TestedType(typeof(ManualDataExport))]
	public sealed class ManualDataExportTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		public void TestDontAllowSavingNonPersistentLog()
		{
			var log = Factory.New<NonPersistentStmALog>();
			AssertExceptionThrown<InvalidOperationException>(Factory.Save);
		}

		public void TestDispose()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			AssertNotNull("dataExport.WorkflowDescriptor before dispose", dataExport.WorkflowDescriptor);
			dataExport.Dispose();
			AssertNull("dataExport.WorkflowDescriptor after dispose", dataExport.WorkflowDescriptor);
		}

		public void TestSentDataDescription()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			CombineAssertions(delegate
			{
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				AssertEquals("dataExport.SentDataDescription", "Send XML Universal Event", dataExport.SendDataDescription);

				dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
				AssertEquals("dataExport.SentDataDescription", "Send XML Universal Shipment", dataExport.SendDataDescription);

				dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalTransaction);
				AssertEquals("dataExport.SentDataDescription", "Send XML Universal Transaction", dataExport.SendDataDescription);

				dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalSchedule);
				AssertEquals("dataExport.SentDataDescription", "Send XML Universal Schedule", dataExport.SendDataDescription);
			});
		}

		[TestDate(2011, 6, 1, 12, 34, 56)]
		public void TestCommunicationModeMatching()
		{
			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var tempCommunicationsMode = TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!", out var mode))
			{
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;
				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "JOB0001234";
				mode.EK_TransportMode = dummyBO.Z0_Code = "AIR";
				Factory.Save();

				var recipient = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				dataExport.OverrideRecipient(recipient);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
				dataExport.EventCode = Events.AuthorisedCode;
				dataExport.EventReference = "McLaren";

				CombineAssertions(delegate
				{
					var notifications = new NotificationsForTest();
					dataExport.SendData(notifications);
					AssertMultilineASCIIEquals("dataExport.SendData() notifications",
	@"Processing Dummy Business Object JOB0001234
Universal Event queued for sending to Organization [EDICUS].",
					notifications.Notifications);
				});

				IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertEquals("messages.Count", 1, messages.Length);
			}
		}

		[TestDate(2011, 6, 1, 12, 34, 56)]
		public void TestWillSendWithValidSetup()
		{
			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var tempCommunicationsMode = TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!", out var _))
			{
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;
				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "JOB0001234";
				Factory.Save();

				var recipient = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				dataExport.OverrideRecipient(recipient);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
				dataExport.EventCode = Events.AuthorisedCode;
				dataExport.EventReference = "McLaren";

				CombineAssertions(delegate
				{
					var notifications = new NotificationsForTest();
					dataExport.SendData(notifications);
					AssertMultilineASCIIEquals("dataExport.SendData() notifications",
	@"Processing Dummy Business Object JOB0001234
Universal Event queued for sending to Organization [EDICUS].",
					notifications.Notifications);
				});

				IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertEquals("messages.Count", 1, messages.Length);
				var message = messages[0];
				var interchange = message.Interchange;

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertContains("message.EM_MessageText", "UniversalEvent", message.EM_MessageText);

					AssertMultilineASCIIEquals("message.EM_MessageText", $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>DummyBusinessObject</Type>
          <Key>JOB0001234</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-06-01T12:34:56.000+00:00</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerReference>McLaren</TriggerReference>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>ORP</Code>
          <Description>Organization Proxy</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2011-06-01T12:34:56.000+00:00</EventTime>
    <EventType>ATH</EventType>
    <EventReference>McLaren</EventReference>
    <IsEstimate>false</IsEstimate>

    <AdditionalFieldsToUpdateCollection>
      <AdditionalFieldsToUpdate>
        <Type>DummyWithWorkflow.Z0_Description</Type>
        <Value>TEST DESCRIPTION</Value>
      </AdditionalFieldsToUpdate>
    </AdditionalFieldsToUpdateCollection>

    <ContextCollection>
      <Context>
        <Type>CubbyHouseBill</Type>
        <Value>WAWAWA342892382</Value>
      </Context>
      <Context>
        <Type>DummyZDecimal</Type>
        <Value>348.534</Value>
      </Context>
    </ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>
				".Trim(), message.EM_MessageText);
				});
			}
		}

		[TestDate(2011, 6, 1, 12, 34, 56)]
		public void TestWillSendWithValidSetup_AlternateRecipient()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			using (var tempCommunicationsMode = TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!", out var _))
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (Factory.AddDisposableService())
			{
				var dummyBO = Factory.New<DummyWithWorkflow>();
				dummyBO.Z0_Description = "JOB0001234";
				Factory.Save();

				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Broker;
				dataExport.RecipientPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				dataExport.EventCode = Events.AuthorisedCode;
				dataExport.EventReference = "McLaren";

				CombineAssertions(delegate
				{
					var notifications = new NotificationsForTest();
					dataExport.SendData(notifications);
					AssertMultilineASCIIEquals("dataExport.SendData() notifications",
	@"Processing Dummy Business Object JOB0001234
Universal Event sent internally for Organization [EDICUS].",
					notifications.Notifications);
				});

				IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertEquals("messages.Count", 1, messages.Length);
				var message = messages[0];

				CombineAssertions(delegate
				{
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
					AssertEquals("message.EM_ReceiveTransmit", "INT", message.EM_ReceiveTransmit);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Discarded, message.EM_Status);
					AssertContains("message.EM_MessageText", "UniversalEvent", message.EM_MessageText);

					AssertMultilineASCIIEquals("message.EM_MessageText", @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>DummyBusinessObject</Type>
          <Key>JOB0001234</Key>
        </DataSource>
      </DataSourceCollection>

      <Company>
        <Code>EDI</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Eagle Datamation International</Name>
      </Company>
      <DataProvider>EDIDATEDI</DataProvider>
      <EnterpriseID>EDI</EnterpriseID>
      <EventBranch>
        <Code>BNE</Code>
        <Name>BN - AUBNE</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ATH</Code>
        <Description>Authorized</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>DAT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2011-06-01T12:34:56.000+00:00</TriggerDate>
      <TriggerDescription></TriggerDescription>
      <TriggerReference>McLaren</TriggerReference>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>BRO</Code>
          <Description>Broker</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EventTime>2011-06-01T12:34:56.000+00:00</EventTime>
    <EventType>ATH</EventType>
    <EventReference>McLaren</EventReference>
    <IsEstimate>false</IsEstimate>

    <AdditionalFieldsToUpdateCollection>
      <AdditionalFieldsToUpdate>
        <Type>DummyWithWorkflow.Z0_Description</Type>
        <Value>TEST DESCRIPTION</Value>
      </AdditionalFieldsToUpdate>
    </AdditionalFieldsToUpdateCollection>

    <ContextCollection>
      <Context>
        <Type>CubbyHouseBill</Type>
        <Value>WAWAWA342892382</Value>
      </Context>
      <Context>
        <Type>DummyZDecimal</Type>
        <Value>348.534</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>
				".Trim(), message.EM_MessageText);
				});
			}
		}

		public void TestErrorsThatHappenWhenProcessingGetReported()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var tempCommunicationsMode = TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "", out var _))
			{
				var dummyBO = Factory.New<DummyWithWorkflow>();
				Factory.Save();

				var factory = new BusinessObjectFactory();
				using (factory.AddDisposableService())
				{
					var dataExport = new ManualDataExport(factory, dummyBO, UniversalDataType.UniversalEvent);
					dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
					dataExport.EventCode = Events.AuthorisedCode;

					var notifications = new NotificationsForTest();
					dataExport.SendData(notifications);
					factory.Save();
					CombineAssertions(delegate
					{
						var notificationsText = notifications.Notifications;
						var exceptionIndex = notificationsText.IndexOf("System.ArgumentException:", StringComparison.Ordinal);
						if (exceptionIndex >= 0)
						{
							notificationsText = notificationsText.Substring(0, exceptionIndex + "System.ArgumentException:".Length);
						}

#if NETFRAMEWORK
						AssertMultilineASCIIEquals("dataExport.SendData() notifications",
		@"Processing Dummy Business Object Default
Error sending Universal Event to Organization [EDICUS]:-

Error delivering message: Interchange cannot be empty
Parameter name: mode.
Delivery mode details:
Transport: EDP
File format: XUE
Purpose: 
Destination: 
Organization: EDICUS
File name: 

Exception detail:
System.ArgumentException:",
						notificationsText);
#else
						AssertMultilineASCIIEquals("dataExport.SendData() notifications",
		@"Processing Dummy Business Object Default
Error sending Universal Event to Organization [EDICUS]:-

Error delivering message: Interchange cannot be empty (Parameter 'mode').
Delivery mode details:
Transport: EDP
File format: XUE
Purpose: 
Destination: 
Organization: EDICUS
File name: 

Exception detail:
System.ArgumentException:",
						notificationsText);
#endif
					});

					IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
					AssertEquals("messages.Count", 0, messages.Length);
				}
			}
		}

		public void TestOverrideRecipient()
		{
			var recipient = Factory.New<OrgHeader>();
			recipient.OH_Code = "NEWORG";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			dataExport.OverrideRecipient(recipient);

			var notifications = new NotificationsForTest();
			dataExport.SendData(notifications);
			AssertMultilineASCIIEquals("dataExport.SendData() notifications", @"
No EDI Communications settings were found on the Recipient Organization [NEWORG].
Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
Failure was because:
Organization [NEWORG] for Company [EDI] has no matching Communication Modes.
".Trim(), notifications.Notifications);
		}

		public void TestInternallySendingForEvent()
		{
			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
				DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

				var dummyBO = Factory.New<DummyWithWorkflow>();
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
				dataExport.EventCode = Events.AuthorisedCode;

				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);
				AssertMultilineASCIIEquals("dataExport.SendData() notifications", string.Format(@"
Processing Dummy Business Object Default
Universal Event sent internally for Organization [{0}].
".Trim(), GlbCompany.CurrentCompany.OrgProxy.OH_Code), notifications.Notifications);
			}
		}

		public void TestInternallySendingForShipment()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.UseRealShipmentForInternalUniversalXMLSending = true;
			using (Factory.AddDisposableService())
			{
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
				dataExport.EventCode = Events.AuthorisedCode;

				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);
				AssertMultilineASCIIEquals("dataExport.SendData() notifications", string.Format(@"
Processing Dummy Business Object Default
Universal Shipment sent internally for Organization [{0}].
".Trim(), GlbCompany.CurrentCompany.OrgProxy.OH_Code), notifications.Notifications);
			}
		}

		public void TestWillTellTheUserWhenNoMessageSentBecauseNoMatchingEdiCommunicationsModeWasPresent()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			var recipient = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			dataExport.OverrideRecipient(recipient);
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			dataExport.EventCode = Events.AuthorisedCode;

			CombineAssertions(delegate
			{
				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);
				AssertMultilineASCIIEquals("dataExport.SendData() notifications",
@"No EDI Communications settings were found on the Recipient Organization [EDICUS].
Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
Failure was because:
Organization [EDICUS] for Company [EDI] has no matching Communication Modes.",
				notifications.Notifications);
			});

			IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
			AssertEquals("messages.Count", 0, messages.Length);
		}

		public void TestWillTellTheUserWhenNoMessageSentBecauseARecipientWasNotFound()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.NotifyParty;

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.NotifyParty;
			dataExport.EventCode = Events.AuthorisedCode;

			CombineAssertions(delegate
			{
				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);
				AssertMultilineASCIIEquals("dataExport.SendData() notifications",
@"You selected 'NFP' for the Recipient Type, but no Organization has been entered for this Recipient Type on Dummy Business Object Default.
Please enter an Organization for this Recipient Type, or select another Recipient Type.",
				notifications.Notifications);
			});

			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
			dataExport.RecipientType = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription("AirCargo Responsible Party");
			CombineAssertions(delegate
			{
				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);
				AssertMultilineASCIIEquals("dataExport.SendData() notifications",
@"You selected 'ARP' for the Recipient Type, but no Organization has been entered for this Recipient Type on Dummy Business Object Default.
Please enter an Organization for this Recipient Type, or select another Recipient Type.",
				notifications.Notifications);
			});

			IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
			AssertEquals("messages.Count", 0, messages.Length);
		}

		public void TestRecipientDetails()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			AssertEquals("dataExport.RecipientPKInfo.ReadOnly", true, dataExport.RecipientPKInfo.ReadOnly);
			AssertEquals("dataExport.RecipientTypeInfo.ReadOnly", true, dataExport.RecipientTypeInfo.ReadOnly);
			AssertEquals("dataExport.Calc_RecipientType", "", dataExport.Calc_RecipientType);
			AssertEquals("dataExport.RecipientType", "", dataExport.RecipientType);

			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertEquals("dataExport.RecipientPKInfo.ReadOnly", true, dataExport.RecipientPKInfo.ReadOnly);
			AssertEquals("dataExport.RecipientTypeInfo.ReadOnly", true, dataExport.RecipientTypeInfo.ReadOnly);
			AssertEquals("dataExport.Calc_RecipientType", MessageRecipientPartyTypeList.Codes.OrgProxy, dataExport.Calc_RecipientType);
			AssertEquals("dataExport.RecipientType", MessageRecipientPartyTypeList.Codes.OrgProxy, dataExport.RecipientType);

			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertEquals("dataExport.RecipientPKInfo.ReadOnly", false, dataExport.RecipientPKInfo.ReadOnly);
			AssertEquals("dataExport.RecipientTypeInfo.ReadOnly", false, dataExport.RecipientTypeInfo.ReadOnly);
			AssertEquals("dataExport.Calc_RecipientType", MessageRecipientPartyTypeList.SpecialCodes.Other, dataExport.Calc_RecipientType);
			AssertEquals("dataExport.RecipientType", MessageRecipientPartyTypeList.Codes.OrgProxy, dataExport.RecipientType);

			dataExport.RecipientPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.Codes.PickupAgent;
			AssertEquals("dataExport.RecipientPKInfo.ReadOnly", true, dataExport.RecipientPKInfo.ReadOnly);
			AssertEquals("dataExport.RecipientTypeInfo.ReadOnly", true, dataExport.RecipientTypeInfo.ReadOnly);
			AssertEquals("dataExport.Calc_RecipientType", MessageRecipientPartyTypeList.Codes.PickupAgent, dataExport.Calc_RecipientType);
			AssertEquals("dataExport.RecipientType", MessageRecipientPartyTypeList.Codes.PickupAgent, dataExport.RecipientType);
			AssertEquals("dataExport.RecipientPK", ZGuid.Empty, dataExport.RecipientPK);

			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.SpecialCodes.Other;
			dataExport.RecipientType = ZString.Empty;
			AssertHasErrorContaining(dataExport.RecipientTypeInfo, MandatoryValidation.MustBeEntered);
			dataExport.RecipientType = "@#@";
			AssertHasErrorContaining(dataExport.RecipientTypeInfo, ListValidation.InvalidCodeError);
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoErrors(dataExport.RecipientTypeInfo);

			dataExport.RecipientPK = ZGuid.Empty;
			AssertHasErrorContaining(dataExport.RecipientPKInfo, MandatoryValidation.MustBeEntered);
			dataExport.RecipientPK = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			AssertNoErrors(dataExport.RecipientPKInfo);

			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			dataExport.RecipientType = "@#@";
			AssertNoErrors(dataExport.RecipientTypeInfo);
			dataExport.RecipientPK = ZGuid.Empty;
			AssertNoErrors(dataExport.RecipientPKInfo);
		}

		public void TestRecipientServiceReadonly()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var shipmentDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			AssertEquals("RecipientService should be editable for a Universal Shipment Action.", false, shipmentDataExport.RecipientServiceInfo.ReadOnly);

			var eventDataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			AssertEquals("RecipientService should be readonly when NOT for a Universal Shipment Action.", true, eventDataExport.RecipientServiceInfo.ReadOnly);
		}

		public void TestValidation_RecipientType()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			var dummyBO = Factory.New<DummyWithWorkflow>();

			AssertRecipientType(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment));
			AssertRecipientType(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalTransaction));
			AssertRecipientType(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalSchedule));
		}

		void AssertRecipientType(ManualDataExport dataExport)
		{
			dataExport.Calc_RecipientType = "!?!";
			AssertHasErrors(dataExport.Calc_RecipientTypeInfo);
			dataExport.Calc_RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoErrors(dataExport.Calc_RecipientTypeInfo);
			dataExport.Calc_RecipientType = "";
			AssertHasErrors(dataExport.Calc_RecipientTypeInfo);
		}

		public void TestValidation_RecipientService()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC", "DEF" };

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
			dataExport.RecipientType = nameof(RecipientRoleType.ORP);
			dataExport.RecipientService = "!?!";
			AssertHasErrorContaining(dataExport.RecipientServiceInfo, ListValidation.InvalidCodeError);

			dataExport.RecipientService = "ABC";
			AssertNoErrors(dataExport.RecipientServiceInfo);

			dataExport.RecipientService = "DEF";
			AssertNoErrors(dataExport.RecipientServiceInfo);

			dataExport.RecipientService = "GHI";
			AssertHasErrorContaining(dataExport.RecipientServiceInfo, ListValidation.InvalidCodeError);

			dataExport.RecipientService = "";
			AssertHasErrorContaining(dataExport.RecipientServiceInfo, MandatoryValidation.MustBeEntered);

			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "", "ABC" };
			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.NotifyParty; // new cached list
			dataExport.ValidateRecipientService();
			AssertNoErrors("A TriggerPartyService is not mandatory if it supports BLANK.", dataExport.RecipientServiceInfo);

			var dataExport_Event = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			dataExport_Event.RecipientService = "ABC";
			AssertHasErrorContaining(dataExport_Event.RecipientServiceInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestValidation_EventCode()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();

			AssertEventCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment));
			AssertEventCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent), true);
			AssertEventCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalTransaction));
			AssertEventCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalSchedule));
		}

		void AssertEventCode(ManualDataExport dataExport, bool isEventCodeRequired = false)
		{
			dataExport.EventCode = "!?!";
			AssertHasErrors(dataExport.EventCodeInfo);
			dataExport.EventCode = "ATH";
			AssertNoErrors(dataExport.EventCodeInfo);
			dataExport.EventCode = "";
			if (isEventCodeRequired)
			{
				AssertHasErrors(dataExport.EventCodeInfo);
			}
			else
			{
				AssertNoErrors(dataExport.EventCodeInfo);
			}
		}

		public void TestValidation_PurposeCode()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;

			var dummyBO = Factory.New<DummyWithWorkflow>();

			AssertPurposeCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment));
			AssertPurposeCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalTransaction));
			AssertPurposeCode(new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalSchedule));
		}

		void AssertPurposeCode(ManualDataExport dataExport)
		{
			dataExport.PurposeCode = "!?!";
			AssertHasErrors(dataExport.PurposeCodeInfo);
			dataExport.PurposeCode = "INV";
			AssertNoErrors(dataExport.PurposeCodeInfo);
			dataExport.PurposeCode = "";
			AssertNoErrors(dataExport.PurposeCodeInfo);
		}

		public void TestSendMultiple_UniversalEvent()
		{
			using (Factory.AddDisposableService())
			{
				using (var tempCommunicationsMode = TemporaryCommunicationsModeOnCompanyOrgProxy(Factory, EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, "TARGETME!", out var _))
				using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
				{
					DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.OrgProxy;
					var dummy1 = Factory.New<DummyWithWorkflow>();
					dummy1.Z0_Description = "JOB0001";

					var dummy2 = Factory.New<DummyWithWorkflow>();
					dummy2.Z0_Description = "JOB0002";

					var dummy3 = Factory.New<DummyWithWorkflow>();
					dummy3.Z0_Description = "JOB0003";

					Factory.Save();

					var recipient = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					var dataExport = new ManualDataExport(Factory, new[] { dummy1, dummy2, dummy3 }, UniversalDataType.UniversalEvent);
					dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.OrgProxy;
					dataExport.EventCode = Events.AuthorisedCode;
					dataExport.OverrideRecipient(recipient);

					CombineAssertions(delegate
					{
						var notifications = new NotificationsForTest();
						dataExport.SendData(notifications);
						AssertMultilineASCIIEquals("dataExport.SendData() notifications",
		@"Processing Dummy Business Object JOB0001
Universal Event queued for sending to Organization [EDICUS].

Processing Dummy Business Object JOB0002
Universal Event queued for sending to Organization [EDICUS].

Processing Dummy Business Object JOB0003
Universal Event queued for sending to Organization [EDICUS].",
						notifications.Notifications);
					});
				}
			}
		}

		public void TestSendMultiple_UniversalShipment_EAdaptorInterface_WithRegistrySet()
		{
			using (Factory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://rickroll.com"))
			{
				TestSendMultiple_UniversalShipment_Core(
					EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface,
					(importBroker1,
						importBroker2,
						shipment1,
						shipment2,
						shipment3,
						shipment4,
						notifications) =>
					{
						AssertMultilineASCIIEquals("dataExport.SendData() notifications",
							@"Processing Shipment ONE, Shipment TWO, Shipment FOUR
Universal Shipment queued for sending to Organization [BROKERONE].

Processing Shipment THREE
Universal Shipment queued for sending to Organization [BROKERTWO].

", notifications.Notifications);

						var recipientsData = UniversalXmlWorkflowProcessorForTesting.RecipientsAndExportedData;

						AssertEquals("Exported in two batches", 2, recipientsData.Count);
						AssertContainsExactElementsInAnyOrder("Grouped by recipient", new[] { shipment1, shipment2, shipment4 },
							recipientsData[importBroker1]);
						AssertContainsExactElementsInAnyOrder("Grouped by recipient", new[] { shipment3 }, recipientsData[importBroker2]);
					});
			}
		}

		public void TestSendMultiple_UniversalShipment_EAdaptorInterface_WithoutRegistrySet()
		{
			TestSendMultiple_UniversalShipment_Core(
					EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface,
					(importBroker1,
					importBroker2,
					shipment1,
					shipment2,
					shipment3,
					shipment4,
					notifications) =>
				{
					AssertMultilineASCIIEquals("dataExport.SendData() notifications",
						@"eAdaptor Outbound Messaging has not been configured in the Registry.
Universal XML cannot be sent.
eAdaptor Outbound Messaging has not been configured in the Registry.
Universal XML cannot be sent.", notifications.Notifications);
					var recipientsData = UniversalXmlWorkflowProcessorForTesting.RecipientsAndExportedData;

					AssertEquals("Exported nothing", 0, recipientsData.Count);
				});
		}

		public void TestSendMultiple_UniversalShipment_EHubInterface()
		{
			TestSendMultiple_UniversalShipment_Core(
				EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				(importBroker1,
					importBroker2,
					shipment1,
					shipment2,
					shipment3,
					shipment4,
					notifications) =>
				{
					AssertMultilineASCIIEquals("dataExport.SendData() notifications",
						@"Processing Shipment ONE, Shipment TWO, Shipment FOUR
Universal Shipment queued for sending to Organization [BROKERONE].

Processing Shipment THREE
Universal Shipment queued for sending to Organization [BROKERTWO].

", notifications.Notifications);

					var recipientsData = UniversalXmlWorkflowProcessorForTesting.RecipientsAndExportedData;

					AssertEquals("Exported in two batches", 2, recipientsData.Count);
					AssertContainsExactElementsInAnyOrder("Grouped by recipient", new[] { shipment1, shipment2, shipment4 },
						recipientsData[importBroker1]);
					AssertContainsExactElementsInAnyOrder("Grouped by recipient", new[] { shipment3 }, recipientsData[importBroker2]);
				});
		}

		void TestSendMultiple_UniversalShipment_Core(string transportType, Action<OrgHeader, OrgHeader, IWorkflowProvider, IWorkflowProvider, IWorkflowProvider, IWorkflowProvider, NotificationsForTest> assert)
		{
			using (Factory.AddDisposableService())
			{
				Func<string, OrgHeader> createImportBroker = (code) =>
					{
						var importBroker = Factory.New<OrgHeader>();
						importBroker.OH_Code = code;

						var communicationsMode = importBroker.EDICommunicationsModes.AddNew();
						communicationsMode.EK_Module = "SHP";
						communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
						communicationsMode.EK_CommunicationsTransport = transportType;

						return importBroker;
					};

				var importBroker1 = createImportBroker("BROKERONE");
				var importBroker2 = createImportBroker("BROKERTWO");

				Func<string, OrgHeader, IWorkflowProvider> createShipment = (consignRef, broker) =>
				{
					var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as BusinessObject;
					shipment[JobShipmentSchema.JS_UniqueConsignRef] = consignRef;
					shipment[JobShipmentSchema.JS_OH_ImportBroker] = broker.PK;

					return shipment as IWorkflowProvider;
				};

				var shipment1 = createShipment("ONE", importBroker1);
				var shipment2 = createShipment("TWO", importBroker1);
				var shipment3 = createShipment("THREE", importBroker2);
				var shipment4 = createShipment("FOUR", importBroker1);

				var dataExport = new ManualDataExport(Factory, new[] { shipment1, shipment2, shipment3, shipment4 }, UniversalDataType.UniversalShipment);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Broker;

				AssertNoExceptionThrown("Batch sending for universal shipment should be supported", () => dataExport.SendData(new NotificationsForTest()));
				Factory.Save();

				var mock = new Mock<IUniversalXmlWorkflowProvider>();
				mock.Setup(mock => mock.GetUniversalXmlWorkflowProcessor(It.IsAny<ActionWrapper>(), (Integration.IMessageProcessorCommunicationModesResult)It.IsAny<IUniversalXmlCommunicationModeProvider>(), It.IsAny<Func<IDataWritingManager, ITopLevelDataObjectWriter>>(), It.IsAny<BusinessObject>(), It.IsAny<IEventInfo>(), It.IsAny<IXmlWriter>(), It.IsAny<IUniversalXmlSchema>()))
					.Returns((ActionWrapper action, IUniversalXmlCommunicationModeProvider modeProvider, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject businessObject, IEventInfo eventInfo, IXmlWriter xmlWriter, IUniversalXmlSchema schema) =>
					{
						return new UniversalXmlWorkflowProcessorForTesting(action, modeProvider, dataWriterGetter, businessObject, eventInfo, xmlWriter, schema);
					});

				using (ObjectFactory.Substitute(mock.Object))
				{
					var notifications = new NotificationsForTest();
					dataExport.SendData(notifications);
					Factory.Save();

					assert(importBroker1, importBroker2, shipment1, shipment2, shipment3, shipment4, notifications);

					UniversalXmlWorkflowProcessorForTesting.RecipientsAndExportedData.Clear();
				}
			}
		}

		public void TestExceptionInProcessingGetsErrorReport()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent, universalXmlWorkflowProcessorFactory: (dontCare, atAll) =>
			{
				return new UniversalXmlWorkflowProcessorForTesting(null, null, null, null, exceptionToThrowOnProcess: new InvalidOperationException("He's dead Jim"));
			});

			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
			dataExport.EventCode = Events.AuthorisedCode;
			var notifications = new NotificationsForTest();
			AssertExceptionThrown<InvalidOperationException>(() => dataExport.SendData(notifications));
			AssertEquals("Unhandled UniversalXMLWorkflow Exception", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestRetriableExceptionsArentErrorReported()
		{
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent, universalXmlWorkflowProcessorFactory: (dontCare, atAll) =>
			{
				return new UniversalXmlWorkflowProcessorForTesting(null, null, null, null, exceptionToThrowOnProcess: new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("He's dead Jim"), null, null), Factory));
			});

			dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
			dataExport.EventCode = Events.AuthorisedCode;
			var notifications = new NotificationsForTest();
			AssertExceptionThrown<ZSaveConcurrencyException>(() => dataExport.SendData(notifications));
			AssertNull("Should not report concurrency exceptions", ErrorReporter.LastExceptionReported);
			ErrorReporter.Clear();
		}

		public void TestSendDataWithResult()
		{
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			DummyWorkflowDescriptor.Instance.SupportedMessageRecipientPartiesExposed = MessageRecipientPartyType.Forwarder;

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.UseRealShipmentForInternalUniversalXMLSending = true;
			using (Factory.AddDisposableService())
			{
				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
				dataExport.RecipientType = MessageRecipientPartyTypeList.Codes.Forwarder;
				dataExport.EventCode = Events.AuthorisedCode;

				var notifications = new NotificationsForTest();
				var xmlEvents = dataExport.SendData(notifications);
				AssertEquals(1, xmlEvents.Count());
				AssertEquals(EDIMessageStatusList.Codes.Discarded, xmlEvents.Select(e => e.Context.ProcessingStatusCode).FirstOrDefault());
			}
		}

		public void TestSendDataWithResult_Failed()
		{
			var recipient = Factory.New<OrgHeader>();
			recipient.OH_Code = "NEWORG";

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalEvent);
			dataExport.OverrideRecipient(recipient);

			var notifications = new NotificationsForTest();
			var xmlEvents = dataExport.SendData(notifications);
			AssertMultilineASCIIEquals("dataExport.SendData() notifications", @"
No EDI Communications settings were found on the Recipient Organization [NEWORG].
Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
Failure was because:
Organization [NEWORG] for Company [EDI] has no matching Communication Modes.
".Trim(), notifications.Notifications);
			AssertEquals(0, xmlEvents.Count());
		}

		public void TestSendDataWithSpecifiedDataWriterGetter()
		{
			var recipient = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);

			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.UseRealShipmentForInternalUniversalXMLSending = true;

			using (Factory.AddDisposableService())
			{
				TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);

				var dataExport = new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment, dataWriterGetter: dataWritingManager => new DataObjectWriterForTesting());
				dataExport.OverrideRecipient(recipient);

				var notifications = new NotificationsForTest();
				dataExport.SendData(notifications);

				IEDIMessage[] messages = Factory.Load<IEDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.UniversalDataMessaging));
				AssertEquals("messages.Count", 1, messages.Length);

				AssertContains("EM_MessageText - AddInfoCollection", @"    <AddInfoCollection>
      <AddInfo>
        <Key>Company Name</Key>
        <Value>WiseTech Global</Value>
      </AddInfo>
    </AddInfoCollection>", messages[0].EM_MessageText);
			}
		}

		#region Implementation

		class UniversalXmlWorkflowProcessorForTesting : IUniversalXmlWorkflowProcessor, ISupportUniversalBatchExport
		{
			public UniversalXmlWorkflowProcessorForTesting(ActionWrapper action, IUniversalXmlCommunicationModeProvider communicationModes, Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter, BusinessObject exportedBO, IEventInfo eventInfo = null, IXmlWriter xmlWriter = null, IUniversalXmlSchema schema = null, Exception exceptionToThrowOnProcess = null)
			{
				if (communicationModes != null)
				{
					recipient = ((EDICommunicationsMode)communicationModes.CommunicationModes[0]).Organisation;
					RecipientsAndExportedData.Add(recipient, new List<BusinessObject>());
					RecipientsAndExportedData[recipient].Add(exportedBO);
				}

				this.exceptionToThrowOnProcess = exceptionToThrowOnProcess;
			}

			readonly OrgHeader recipient;
			readonly Exception exceptionToThrowOnProcess;

			void IProcessor.Process(INotifications notifications, CancellationToken token)
			{
			}

			void ISupportUniversalBatchExport.AddAnotherExportedBusinessObject(BusinessObject anotherExportedBO)
			{
				RecipientsAndExportedData[recipient].Add(anotherExportedBO);
			}

			void ISupportUniversalBatchExport.AddAnotherTopLevelDataObjectWriterAndXmlWriter(ITopLevelDataObjectWriter topLevelDataObjectWriter, IXmlWriter xmlWriter)
			{
			}

			public IXmlEventValueObject[] Process(INotifications notifications, CancellationToken cancellationToken)
			{
				if (exceptionToThrowOnProcess != null)
				{
					throw exceptionToThrowOnProcess;
				}

				return Array.Empty<IXmlEventValueObject>();
			}
			public MultilingualString GetConfigurationWarnings() => null;
			public void AddAdditionalTriggerParty(ZString partyCode, ZString otherCode) { }

			public static Dictionary<OrgHeader, List<BusinessObject>> RecipientsAndExportedData = new Dictionary<OrgHeader, List<BusinessObject>>();
		}

		public static IDisposable TemporaryCommunicationsModeOnCompanyOrgProxy(BusinessObjectFactory factory, ZString ediCommunicationsModeFileFormat, ZString destination)
		{
			return TemporaryCommunicationsModeOnCompanyOrgProxy(factory, ediCommunicationsModeFileFormat, destination, out var mode);
		}

		public static IDisposable TemporaryCommunicationsModeOnCompanyOrgProxy(BusinessObjectFactory factory, ZString ediCommunicationsModeFileFormat, ZString destination, out EDICommunicationsMode mode)
		{
			var orgProxy = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertNotNull("Precondition: must have a Company OrgProxy setup.", orgProxy);
			AssertEquals("Precondition: Company OrgProxy Code", "EDICUS", orgProxy.OH_Code);

			var tempCommunicationsMode = orgProxy.EDICommunicationsModes.AddNew();
			tempCommunicationsMode.EK_Module = "DUM";
			tempCommunicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			tempCommunicationsMode.EK_FileFormat = ediCommunicationsModeFileFormat;
			tempCommunicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			tempCommunicationsMode.EK_Destination = destination;
			factory.Save();
			mode = tempCommunicationsMode;
			return new DisposableAction(() =>
			{
				tempCommunicationsMode.Delete();
				factory.Save();
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var dummyBO = Factory.New<DummyWithWorkflow>();
			return new ManualDataExport(Factory, dummyBO, UniversalDataType.UniversalShipment);
		}

		class NotificationsForTest : INotifications
		{
			public string Notifications
			{
				get { return string.Join(System.Environment.NewLine, notifications); }
			}

			void INotifications.Add(INotification notification)
			{
				if (notification != null)
				{
					notifications.Add(notification.Message);
				}
			}

			readonly List<string> notifications = new List<string>();
		}

		class DataObjectWriterForTesting : ITopLevelDataObjectWriter
		{
			public DataObjectWriterForTesting()
			{
			}

			public DataContextType TopLevelDataContextType => DataContextType.ForwardingShipment;

			public ZString EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

			public ZString RootElementName => "UniversalShipment";

			public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
			{
				var dataContext = DataContextFactory.New(sourceBO.GetUniversalDataContextManager(), UniversalXmlInfo.Namespace_2011_11);
				var dataSource = dataContext.DataSourceCollection.First();

				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
				};
				universalShipment.SetAddInfoCollection(() => new List<AddInfo>() { new AddInfo() { Key = "Company Name", Value = "WiseTech Global" } });

				return universalShipment;
			}
		}

		#endregion
	}
}
