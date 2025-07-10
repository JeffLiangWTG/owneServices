using System;
using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DataTransfer.MessageDelivery;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.HelperClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using IAUCusMAWB = Enterprise.Integration.Customs.AU.ICusMAWB;
using ICusUnderbond = Enterprise.Integration.Customs.AU.ICusUnderbond;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public abstract class JobConsolWorkflowDescriptorTestBase<TConsol, TWorkflowDescriptor> : WorkflowDescriptorTestCase<TWorkflowDescriptor>
		where TConsol : ForwardingConsol
		where TWorkflowDescriptor : JobConsolWorkflowDescriptor, new()
	{
		public override void TestID()
		{
			AssertEquals("Correct Code", JobInvoicingConsumerTypes.Consol.Code, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", JobInvoicingConsumerTypes.Consol.Description, WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("2 sub types", 2, WorkflowDescriptor.SubTypeInformation.Length);

			AssertEquals("Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[0].List);
			AssertEquals("Direction", WorkflowDescriptor.SubTypeInformation[1].Description);
			AssertNotNull(WorkflowDescriptor.SubTypeInformation[1].List);

			CodeDescriptionPairList transportModeList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[0].List;
			AssertEquals("", transportModeList[0].Code);
			AssertEquals("All", transportModeList[0].Description);

			var directionList = (CodeDescriptionPairList)WorkflowDescriptor.SubTypeInformation[1].List;
			AssertEquals("", directionList[0].Code);
			AssertEquals("All", directionList[0].Description);
			AssertEquals(DirectionsContext.Import, directionList[1].Code);
			AssertEquals("Import", directionList[1].Description);
			AssertEquals(DirectionsContext.Export, directionList[2].Code);
			AssertEquals("Export", directionList[2].Description);
			AssertEquals(DirectionsContext.Domestic, directionList[3].Code);
			AssertEquals("Domestic", directionList[3].Description);
			AssertEquals(DirectionsContext.CrossTrade, directionList[4].Code);
			AssertEquals("Cross Trade", directionList[4].Description);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#region Workflow Triggers

		protected override SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get
			{
				return new SchemaColumn[]
				{
					JobConsolTransportSchema.JW_RL_NKLoadPort,
					JobConsolTransportSchema.JW_RL_NKDiscPort,
					JobConsolTransportSchema.JW_Vessel,
					JobConsolTransportSchema.JW_VoyageFlight,
					JobConsolTransportSchema.JW_ETD,
					JobConsolTransportSchema.JW_ETA,
					JobConsolTransportSchema.JW_ATD,
					JobConsolTransportSchema.JW_ATA,
					JobConsolSchema.JK_MasterBillNum
				};
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				List<CodeDescriptionPair> list = new List<CodeDescriptionPair>();
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml, WorkflowTriggerActionTypeConstants.Descriptions.SendDescartesXml));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess, WorkflowTriggerActionTypeConstants.Descriptions.StartDestinationPortClearanceProcess));
				list.Add(new CodeDescriptionPair(ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document, ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Descriptions.SendCargoIMPPhase2Document));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendEmanifestCloseMessage));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills, WorkflowTriggerActionTypeConstants.Descriptions.SendAllEmanifestHouseBills));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentManifestXML));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalManifestEventXML));
				list.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage, WorkflowTriggerActionTypeConstants.Descriptions.ValidateAndSendAirCargoReportMessage));
				return list.ToArray();
			}
		}

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.DepartureTransitWarehouse:
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive, ServiceCodesList.Codes.TransitWarehouseDispatch, ServiceCodesList.Codes.TransitWarehouseReceiveAndDispatch };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.Consignee |
					MessageRecipientPartyType.Consignor |
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.Broker |
					MessageRecipientPartyType.ImportBroker |
					MessageRecipientPartyType.ExportBroker |
					MessageRecipientPartyType.ArrivalCTO |
					MessageRecipientPartyType.DepartureCTO |
					MessageRecipientPartyType.ArrivalContainerYard |
					MessageRecipientPartyType.DepartureContainerYard |
					MessageRecipientPartyType.PickupCartage |
					MessageRecipientPartyType.DeliveryCartage |
					MessageRecipientPartyType.ReceivingAgent |
					MessageRecipientPartyType.SendingAgent |
					MessageRecipientPartyType.ControllingAgent |
					MessageRecipientPartyType.ControllingCustomer |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.Carrier |
					MessageRecipientPartyType.DepartureCFS |
					MessageRecipientPartyType.ArrivalCFS |
					MessageRecipientPartyType.DeConsolidator |
					MessageRecipientPartyType.DepartureTransitWarehouse |
					MessageRecipientPartyType.ArrivalTransitWarehouse;
			}
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.Consol, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		#region GetTestParentsWithConfiguredOrganisationParties

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				ForwardingConsolWithConfiguredOrganisationParties,
			};
		}

		ForwardingConsol ForwardingConsolWithConfiguredOrganisationParties
		{
			get { return forwardingConsolWithOrganisationParties ?? (forwardingConsolWithOrganisationParties = GetConsolWithOrgInfo()); }
		}
		ForwardingConsol forwardingConsolWithOrganisationParties;

		/// <summary>
		/// Gets a ForwardingConsol with required test organisations attached to it
		/// </summary>
		ForwardingConsol GetConsolWithOrgInfo()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			// Receiving & Sending Agent
			consol.JK_OA_ReceivingForwarderAddress = ReceivingAgentOrg.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = SendingAgentOrg.MainAddress.PK;

			// Carrier
			consol.JK_OA_ShippingLineAddress = CarrierOrg.MainAddress.PK;

			// Departure & Arrival CTO
			consol.JK_OA_DepartureCTOAddress = DepartureCTOOrg.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = ArrivalCTOOrg.MainAddress.PK;

			// Departure & Arrival Container Yard
			consol.JK_OA_ContainerYardEmptyPickupAddress = DepartureContainerYardOrg.MainAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = ArrivalContainerYardOrg.MainAddress.PK;

			// Linked Shipment Parties
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			consol.Shipments.Add(shipment);

			// Departure & Arrival CFS
			consol.JK_OA_PackDepotAddress = DepartureCFSOrg.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = ArrivalCFSOrg.MainAddress.PK;

			// Consignee, Consignor, Controlling Customer & Broker
			shipment.ConsigneePK = ConsigneeOrg.PK;
			shipment.ConsignorPK = ConsignorOrg.PK;
			shipment.JS_OH_ExportBroker = BrokerOrg.PK;
			shipment.JS_OH_ImportBroker = BrokerOrg.PK;
			shipment.ControllingCustomerAddress.OrganisationPK = ControllingCustomerOrg.PK;
			shipment.ControllingAgentDocumentaryAddress.OrganisationPK = ControllingAgentOrg.PK;

			// Pickup & Delivery Cartage
			JobDocsAndCartage shipmentDocs = JobDocsAndCartage.GetOrCreateDocsAndCartageFromParent(shipment);
			shipmentDocs.PickupCartageCoPK = PickupCartageOrg.PK;
			shipmentDocs.DeliveryCartageCoPK = DeliveryCartageOrg.PK;

			Factory.Save();
			// BillToParty
			var job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = consol;
			job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;
			job.JH_OA_LocalChargesAddr = BillToPartyOrg.MainAddress.PK;
			Factory.Save();

			return consol;
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			base.SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(workflowProvider, partyTypeCode);
			switch (partyTypeCode)
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					{
						var consol = workflowProvider as ForwardingConsol;
						var mawb = Factory.New<IAUCusMAWB>();
						mawb.CM_JK = consol.PK;

						var underbond = Factory.New<ICusUnderbond>();
						underbond.LinkedObject = mawb as BusinessObject;
						underbond.C4_MovementReason = "DCL";
						underbond.C4_DestinationPremiseID = "9999Z";

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
						orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "9999Z";
						orgCusCode.OK_OH = orgHeader.PK;
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					{
						var consol = workflowProvider as ForwardingConsol;
						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

						var mawb = Factory.New<IAUCusMAWB>();
						mawb.CM_JK = consol.PK;
						mawb.CM_OH_ResponsibleParty = orgHeader.PK;

						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullName + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;
					}
					break;
			}
		}

		#endregion

		public void TestGetCorrectDeliveryModeForSimplifiedXMLTriggerAction()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			var communicationsMode = orgProxy.EDICommunicationsModes.AddNew();
			try
			{
				communicationsMode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
				communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
				communicationsMode.EK_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;
				communicationsMode.EK_Destination = Env.TempPath;
				communicationsMode.EK_Filename = "(*JobNumber*).xml";

				Factory.Save();

				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				ProcessTask processTask = consol.WorkflowItems.Triggers.AddNew();
				ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
				processTask.ProcessTaskNotifications.Add(action);

				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.AsPerPayload;

				IProcessor resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));

				Assert("Processor for Simplified XML should be ", resultProcessor is XmlMessageDeliver);

				var xmlProcessor = resultProcessor as XmlMessageDeliver;
				AssertNotNull("Processor should be XmlMessageDelivery", xmlProcessor);
				AssertEquals("Delivery Mode should be found", 1, xmlProcessor.Modes.CommunicationModes.Count);
				AssertEquals("Transport Mode", communicationsMode.EK_CommunicationsTransport, xmlProcessor.Modes.CommunicationModes[0].EK_CommunicationsTransport);
				AssertEquals("FileName", communicationsMode.EK_Filename, xmlProcessor.Modes.CommunicationModes[0].EK_Filename);
				AssertEquals("File Directory", communicationsMode.EK_Destination, xmlProcessor.Modes.CommunicationModes[0].EK_Destination);
			}
			finally
			{
				orgProxy.EDICommunicationsModes.RemoveAndDelete(communicationsMode);
				Factory.Save();
			}
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ProcessTask processTask = consol.WorkflowItems.Triggers.AddNew();
			ProcessTaskNotification action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = ForwardingShipmentWorkflowDescriptor.ForwardingShipmentWorkflowTriggerActionTypeConstants.Codes.SendCargoIMPPhase2Document;

			IProcessor resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendCargoIMPPhase2Document should be ConsolCargoImpPhase2MessageDelivery", resultProcessor is ConsolCargoImpPhase2MessageDelivery);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB;
			resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			Assert("Processor for SendSXMLWithAWB should be XmlMessageDelivery", resultProcessor is XmlMessageDeliver);
		}

		public void TestRepeatedTemplateApplicationOnConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			WorkflowItemCollectionView collection = new WorkflowTriggerCollectionView(consol.WorkflowItems);

			ProcessTask existingTrigger = collection.AddNew();
			existingTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			existingTrigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			Factory.Save();

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = consol.WorkflowItems.WorkflowType;

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			templateTrigger.TriggerConditions.TriggerEventCode = Events.Departure.Code;

			AssertNoExceptionThrown(() => collection.CreateItemsFromTemplate_ForTest(template));
		}

		#endregion

		#region Date Defaulting

		protected override IWorkflowProvider GetParentForGettingDateTimeOffset()
		{
			var consol = (ForwardingConsol)base.GetParentForGettingDateTimeOffset();

			consol.Transports.ArrivalTransport.JW_RL_NKLoadPort = "AUPER";
			consol.Transports.ArrivalTransport.JW_RL_NKDiscPort = "AUBNE";

			return consol;
		}

		protected override void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			var consol = (ForwardingConsol)workflowProvider;

			switch (dateTimeSourceType)
			{
				case ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD:
					consol.Transports.DepartureTransport.JW_ETD = localTime;
					break;

				case ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA:
					consol.Transports.ArrivalTransport.JW_ETA = localTime;
					break;

				case ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable:
					consol.Transports.ArrivalTransport.JW_TerminalAvailabilityDate = localTime;
					break;

				case ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage:
					consol.Transports.ArrivalTransport.JW_TerminalStorageDate = localTime;
					break;

				case ForwardingConsolEstimateDefaultedFromList.Codes.LCLAvailable:
					consol.Transports.ArrivalTransport.JW_DepotAvailabilityDate = localTime;
					break;

				case ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage:
					consol.Transports.ArrivalTransport.JW_DepotStorageDate = localTime;
					break;

				default:
					base.SetDateTimeSourcePropertyValue(workflowProvider, dateTimeSourceType, localTime);
					break;
			}
		}

		protected override TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			switch (dateTimeSourceType)
			{
				case ForwardingConsolEstimateDefaultedFromList.Codes.LoadingETD:
					return TimeSpan.FromHours(8);

				case ForwardingConsolEstimateDefaultedFromList.Codes.DischargeETA:
				case ForwardingConsolEstimateDefaultedFromList.Codes.FCLAvailable:
				case ForwardingConsolEstimateDefaultedFromList.Codes.FCLStorage:
				case ForwardingConsolEstimateDefaultedFromList.Codes.LCLAvailable:
				case ForwardingConsolEstimateDefaultedFromList.Codes.LCLStorage:
					return TimeSpan.FromHours(10); // All of these date/time sources relate to the arrival transport, whose destination is AUBNE

				default:
					return base.GetUtcOffsetForDateTimeSourceType(workflowProvider, dateTimeSourceType);
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			switch (table.TableName)
			{
				case JobConsolSchema.Constants.TableName:
					return Factory.New<ForwardingConsol>();

				default:
					return base.NewBusinessObjectInTable(table);
			}
		}

		#endregion
	}
}
