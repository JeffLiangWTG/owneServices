using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.UniversalDataBuss.Integration.DataObjects;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(HouseAirCargoWorkflowDescriptor))]
	sealed class HouseAirCargoWorkflowDescriptorTest : WorkflowDescriptorTestCase<HouseAirCargoWorkflowDescriptor>
	{
		public void TestSupportsBufferManagement()
		{
			AssertEquals("HouseAirCargoWorkflowDescriptors should support BufferManagement.", true, WorkflowDescriptor.SupportsBufferManagement);
		}

		public override void TestWorkflowProviderType()
		{
			AssertWorkflowProviderType(Core.Constants.CountryCodes.Australia, ObjectFactory.GetType<Integration.Customs.AU.ICusHAWB>());
			AssertWorkflowProviderType(Core.Constants.CountryCodes.NewZealand, ObjectFactory.GetType<Integration.Customs.NZ.ICusHAWB>());
			AssertWorkflowProviderType(Core.Constants.CountryCodes.UnitedKingdom, ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICusHAWB>());

			AssertWorkflowProviderType(Core.Constants.CountryCodes.SouthAfrica, typeof(CusHAWB));
		}

		void AssertWorkflowProviderType(string countryCode, Type expectedHAWBType)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var provider = (IWorkflowProvider)Factory.New(expectedHAWBType);
				AssertEquals($"WorkflowProviderType should be {expectedHAWBType.ToString()}", expectedHAWBType, WorkflowDescriptor.WorkflowProviderType);
				AssertEquals($"Workflow Code is the same as the WorkflowDescriptor Code for {expectedHAWBType.ToString()}", WorkflowDescriptor.Code, provider.WorkflowType);
			}
		}

		public void TestControllerID()
		{
			AssertControllerID(Core.Constants.CountryCodes.Australia, ControllerIDs.Customs.AU.HouseAirCargo);
			AssertControllerID(Core.Constants.CountryCodes.NewZealand, ControllerIDs.Customs.NZ.ExpressECI);
			AssertControllerID(Core.Constants.CountryCodes.UnitedKingdom, ControllerIDs.Customs.GB.CcsukAirInventory);

			AssertControllerID(Core.Constants.CountryCodes.SouthAfrica, ControllerIDs.Customs.BaseAirCargo);
		}

		void AssertControllerID(string countryCode, ControllerID expectedControllerID)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				AssertEquals($"ControllerID for {expectedControllerID.ToString()}", expectedControllerID, WorkflowDescriptor.ControllerID);
			}
		}

		public void TestGetAdditionalWorkflowTriggerActionTypeList_SendWithdrawalMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var hawb = Factory.New<CusHAWB>();
				var task = ((IWorkflowProvider)hawb).WorkflowItems.Triggers.AddNew();
				AssertEquals(true, WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, hawb).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var hawb = Factory.New<CusHAWB>();
				var task = ((IWorkflowProvider)hawb).WorkflowItems.Triggers.AddNew();
				AssertEquals(false, WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, hawb).ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage));
			}
		}

		public void TestSendWithdrawalMessageProcessor_AU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var mawb = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				var hawbForWorkflow = (IWorkflowProvider)hawb;

				var trigger = hawbForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisationWithdrawnCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage;

				hawb.Logs.AddNew(Events.AuthorisationWithdrawn);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());

				hawb.Messages.Load();
				AssertEquals("Message Sent", 1, hawb.Messages.Count);
				AssertEquals("WDW", hawb.Messages[0].EM_MessageSubType);
			}
		}

		public void TestSendWithdrawalMessageProcessor_CN()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var mawb = (CusMAWB)Factory.New<Integration.Customs.AU.ICusMAWB>();
				var hawb = mawb.ChildBills.AddNew();
				var hawbForWorkflow = (IWorkflowProvider)hawb;

				var trigger = hawbForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisationWithdrawnCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage;

				hawb.Logs.AddNew(Events.AuthorisationWithdrawn);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				AssertNull(processor);
			}
		}

		public override void TestIEventPublisherPerformance()
		{
			Assert(true);
		}

		public void TestDeConsolidatorRecipientParty()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DDP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var orgCusCode = Factory.New<OrgCusCode>();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
				orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
				orgCusCode.OK_CustomsRegNo = "9999Z";
				orgCusCode.OK_OH = orgHeader.PK;

				var mawb = Factory.New<Integration.Customs.AU.ICusMAWB>() as CusMAWB;
				var hawb = mawb.ChildBills.AddNew();
				var hawbForWorkflow = (IWorkflowProvider)hawb;

				var underbond = Factory.New<Integration.Customs.AU.ICusUnderbond>() as CusUnderbond;
				underbond.C4_ParentID = mawb.PK;
				underbond.C4_ParentTableCode = mawb.TablePrefix;
				underbond.C4_MovementReason = "DCL";
				underbond.C4_DestinationPremiseID = "9999Z";

				var trigger = hawbForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoAvailableCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.DeConsolidator;

				mawb.Logs.AddNew(Events.CargoReceivedAtDepot);
				hawb.Logs.AddNew(Events.CargoAvailable);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				hawb.Messages.Load();
				AssertEquals("Message Sent", 1, hawb.Messages.Count);
				AssertEquals("Correct Destination", "DDP_DummyDestination", hawb.Messages[0].Interchange.EI_To);
			}
		}

		public void TestAirCargoResponsiblePartyRecipient()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "ARP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var mawb = Factory.New<Integration.Customs.AU.ICusMAWB>() as CusMAWB;
				mawb.CM_OH_ResponsibleParty = orgHeader.PK;
				var hawb = mawb.ChildBills.AddNew();
				var hawbForWorkflow = (IWorkflowProvider)hawb;

				var trigger = hawbForWorkflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoAvailableCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty;

				mawb.Logs.AddNew(Events.CargoReceivedAtDepot);
				hawb.Logs.AddNew(Events.CargoAvailable);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				hawb.Messages.Load();
				AssertEquals("Message Sent", 1, hawb.Messages.Count);
				AssertEquals("Correct Destination", "ARP_DummyDestination", hawb.Messages[0].Interchange.EI_To);
			}
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.CustomsHouseAirCargoCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "House Air Cargo", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("sub type", 1, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals("Origin", WorkflowDescriptor.Port1Name);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
			AssertEquals("Destination", WorkflowDescriptor.Port2Name);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CusHAWB, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestSupportsSetFieldTriggerAction()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsSetFieldTriggerAction(null, null));
		}

		#region IEventPublisher

		public void TestGetSubscriptionsQuery()
		{
			var masters = new[]
			{
				Factory.New<CusMAWB>(),
				Factory.New<CusMAWB>(),
			};

			var houses = new[]
			{
				CreateHAWB(masters[0]),		//	houses[0]
				CreateHAWB(masters[0]),		//	houses[1]
				CreateHAWB(masters[1]),		//	houses[2]
				CreateHAWB(masters[1]),		//	houses[3]
			};

			var subscriptions = new[]
			{
				CreateSubscription(masters[0], "AAA"),												//	subscriptions[0]
				CreateSubscription(masters[0], WorkflowDescriptors.CustomsHouseAirCargoCode),		//	subscriptions[1]
				CreateSubscription(masters[1], "AAA"),												//	subscriptions[2]
				CreateSubscription(masters[1], WorkflowDescriptors.CustomsHouseAirCargoCode),		//	subscriptions[3]
			};

			houses[0].Logs.AddNew(Events.Arrival);

			Factory.Save();

			var subscriptionsSubQuery = WorkflowDescriptor.GetSubscriptionsQuery();
			var query = string.Format("SELECT DISTINCT SES_PK FROM dbo.StmALog JOIN ({0}) IQ (SES_PK, TargetID) on SL_Parent = TargetID", subscriptionsSubQuery);

			using (var command = Db.Connection.Command(query))
			{
				var actual = new[] { (Guid)command.ExecuteScalar() };
				var expected = new[] { subscriptions[1] };

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}
		}

		Guid CreateSubscription(BusinessObject registrar, string publisherDescriptor)
		{
			var subscription = Factory.New<StmEventSubscription>();
			subscription.SES_RegistrarParentId = registrar.PK;
			subscription.SES_AgentDescriptor = "AAA";
			subscription.SES_PublisherDescriptor = publisherDescriptor;
			subscription.SES_RegistrarDescriptor = "DUM";
			subscription.SES_SubscriberDescriptor = "BBB";

			return subscription.PK.ToGuid();
		}

		CusHAWB CreateHAWB(CusMAWB mawb)
		{
			var hawb = Factory.NewWithValidTestData<CusHAWB>();
			hawb.CS_CM = mawb.PK;

			return hawb;
		}

		#endregion

		new HouseAirCargoWorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				return (HouseAirCargoWorkflowDescriptor)base.WorkflowDescriptor;
			}
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new[]
				{
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage,
						WorkflowTriggerActionTypeConstants.Descriptions.SendWithdrawalMessage)
				};
			}
		}

		protected override ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			switch (recipient)
			{
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return new ZString[] { ServiceCodesList.Codes.TransitWarehouseReceive };
				default:
					return base.ExpectedSupportedTriggerPartyServices(recipient);
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email |
					MessageRecipientPartyType.DeConsolidator |
					MessageRecipientPartyType.AirCargoResponsibleParty |
					MessageRecipientPartyType.ArrivalTransitWarehouse;
			}
		}

		protected override void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
			switch (partyTypeCode)
			{
				case MessageRecipientPartyTypeList.Codes.DeConsolidator:
					{
						var hawb = workflowProvider as CusHAWB;
						var mawb = hawb.MAWB;
						if (mawb == null)
						{
							mawb = hawb.Factory.New<CusMAWB>();
							hawb.CS_CM = mawb.PK;
						}
						var underbond = Factory.New<Integration.Customs.AU.ICusUnderbond>() as CusUnderbond;
						underbond.C4_ParentID = mawb.PK;
						underbond.C4_ParentTableCode = mawb.TablePrefix;
						underbond.C4_MovementReason = "DCL";
						underbond.C4_DestinationPremiseID = "9999Z";

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullNameTruncated + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
						orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "9999Z";
						orgCusCode.OK_OH = orgHeader.PK;
					}
					break;
				case MessageRecipientPartyTypeList.Codes.AirCargoResponsibleParty:
					{
						var hawb = workflowProvider as CusHAWB;
						var mawb = hawb.MAWB;
						if (mawb == null)
						{
							mawb = hawb.Factory.New<CusMAWB>();
							hawb.CS_CM = mawb.PK;
						}

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
						communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
						communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						communicationMode.EK_Destination = orgHeader.OH_FullNameTruncated + "@notificationemail.cargowise.com";
						communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
						communicationMode.EK_Module = WorkflowDescriptor.Code;

						mawb.CM_OH_ResponsibleParty = orgHeader.PK;
					}
					break;
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					{
						var hawb = workflowProvider as CusHAWB;
						var mawb = hawb.MAWB;
						if (mawb == null)
						{
							mawb = hawb.Factory.New<CusMAWB>();
							hawb.CS_CM = mawb.PK;
						}

						var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
						var orgMode = orgHeader.EDICommunicationsModes.AddNew();
						orgMode.EK_Module = WorkflowDescriptors.CustomsHouseAirCargoCode;
						orgMode.EK_FileFormat = "NTF";
						orgMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
						orgMode.EK_Destination = "@notificationemail.cargowise.com";

						var orgCusCode = Factory.New<OrgCusCode>();
						orgCusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
						orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
						orgCusCode.OK_CustomsRegNo = "41065894724";
						orgCusCode.OK_OH = orgHeader.PK;

						mawb.CM_OA_UnpackDepotAddress = orgHeader.MainAddress.PK;
					}
					break;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var auCompany = Factory.New<GlbCompany>();
			auCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var auBranch = auCompany.Branches.AddNew();
			var mawb = Factory.New<Integration.Customs.AU.ICusMAWB>() as CusMAWB;
			mawb.CM_GB = auBranch.PK;
			return new[] { (IWorkflowProvider)mawb.ChildBills.AddNew() };
		}

		protected override string TestingCountry => Core.Constants.CountryCodes.Australia;
	}
}
