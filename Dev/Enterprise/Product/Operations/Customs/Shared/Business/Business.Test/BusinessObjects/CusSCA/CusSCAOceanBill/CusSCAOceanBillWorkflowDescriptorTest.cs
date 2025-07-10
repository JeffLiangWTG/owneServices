using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusSCAOceanBillWorkflowDescriptor))]
	sealed class CusSCAOceanBillWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusSCAOceanBillWorkflowDescriptor>
	{
		public override void TestIEventPublisherPerformance()
		{
			Assert(true);
		}

		public void TestSRPWithResponsibleParty()
		{
			using (Factory.AddDisposableService())
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345", Core.Constants.CountryCodes.Australia);
				var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "SRP_DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = WorkflowDescriptor.Code;

				var oceanBill = (BaseCusSCAOceanBill)Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();
				oceanBill.CB_ResponsiblePartyID = "12345";
				var workflow = (IWorkflowProvider)oceanBill;

				var trigger = workflow.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Test";
				trigger.TriggerConditions.TriggerEventCode = Events.CargoReceivedAtDepotCode;
				trigger.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;

				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				notification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.SeaCargoResponsibleParty;

				oceanBill.Logs.AddNew(Events.CargoReceivedAtDepot);

				Factory.Save();

				var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				var triggerWTELogs = Factory.Load<StmALog>(query);
				var triggerWTELog = triggerWTELogs[0];

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(triggerWTELog, trigger));
				processor.Process(new NotificationBuffer());
				Factory.Save();

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, oceanBill.PK));
				AssertEquals("Message Sent", 1, messages.Length);
				AssertEquals("Correct Destination", "SRP_DummyDestination", messages[0].Interchange.EI_To);
			}
		}

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Sea Cargo", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", "SCR", WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			Assert("Requires Branch", WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert("Requires Client", WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert("Doesn't require Department", !WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert("Requires Port 1", WorkflowDescriptor.RequiresPort1);
			Assert("Requires Port 2", WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			AssertEquals("No subtypes", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestSupportsEventTracking()
		{
			Assert("Supports Event Tracking", WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestWorkflowProviderType()
		{
			var provider = (IWorkflowProvider)Factory.New<Integration.Customs.AU.ICusSCAOceanBill>();
			Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
			AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);

			provider = (IWorkflowProvider)Factory.New<Integration.Customs.NZ.ICusSCAOceanBill>();
			Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
			AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
		}

		#region IEventsPublisher

		public void TestGetSubscriptionsQuery()
		{
			var masters = new[]
			{
				Factory.New<TestCusSCAOceanBill>(),
				Factory.New<TestCusSCAOceanBill>(),
			};

			var houses = new[]
			{
				CreateCusSCAHouse(masters[0]),		//	houses[0]
				CreateCusSCAHouse(masters[0]),		//	houses[1]
				CreateCusSCAHouse(masters[1]),		//	houses[2]
				CreateCusSCAHouse(masters[1]),		//	houses[3]
			};

			var containers = new[]
			{
				CreateCusSCAContainer(masters[0]),
				CreateCusSCAContainer(masters[0]),
				CreateCusSCAContainer(masters[1]),
				CreateCusSCAContainer(masters[1]),
			};

			var pivots = new[]
			{
				CreateCusSCAPivot(houses[0], containers[0]),
				CreateCusSCAPivot(houses[1], containers[1]),
				CreateCusSCAPivot(houses[2], containers[2]),
				CreateCusSCAPivot(houses[3], containers[3]),
			};

			var subscriptions = new[]
			{
				CreateSubscription(masters[0], "AAA"),								//	subscriptions[0]
				CreateSubscription(masters[0], WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode),		//	subscriptions[1]
				CreateSubscription(masters[1], "AAA"),								//	subscriptions[2]
				CreateSubscription(masters[1], WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode),		//	subscriptions[3]
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

			containers[0].Logs.AddNew(Events.Arrival);
			Factory.Save();
			subscriptionsSubQuery = WorkflowDescriptor.GetSubscriptionsQuery();
			query = string.Format("SELECT DISTINCT SES_PK FROM dbo.StmALog JOIN ({0}) IQ (SES_PK, TargetID) on SL_Parent = TargetID", subscriptionsSubQuery);

			using (var command = Db.Connection.Command(query))
			{
				var actual = new[] { (Guid)command.ExecuteScalar() };
				var expected = new[] { subscriptions[1] };

				AssertContainsExactElementsInAnyOrder(expected, actual);
			}

			pivots[0].Logs.AddNew(Events.Arrival);
			Factory.Save();
			subscriptionsSubQuery = WorkflowDescriptor.GetSubscriptionsQuery();
			query = string.Format("SELECT DISTINCT SES_PK FROM dbo.StmALog JOIN ({0}) IQ (SES_PK, TargetID) on SL_Parent = TargetID", subscriptionsSubQuery);

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

		BaseCusSCAHouse CreateCusSCAHouse(BaseCusSCAOceanBill oceanBill)
		{
			var house = Factory.NewWithValidTestData<TestCusSCAHouse>();
			house.CA_CB = oceanBill.PK;
			return house;
		}

		BaseCusSCAContainer CreateCusSCAContainer(BaseCusSCAOceanBill oceanBill)
		{
			var container = Factory.NewWithValidTestData<TestCusSCAContainer>();
			container.CN_CB = oceanBill.PK;
			return container;
		}

		BaseCusSCAPivot CreateCusSCAPivot(BaseCusSCAHouse house, BaseCusSCAContainer container)
		{
			var pivot = Factory.NewWithValidTestData<TestCusSCAPivot>();
			pivot.CV_CA = house.PK;
			pivot.CV_CN = container.PK;
			return pivot;
		}

		#endregion

		new CusSCAOceanBillWorkflowDescriptor WorkflowDescriptor
		{
			get
			{
				return (CusSCAOceanBillWorkflowDescriptor)base.WorkflowDescriptor;
			}
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new[] { (IWorkflowProvider)Factory.New<Integration.Customs.AU.ICusSCAOceanBill>(), (IWorkflowProvider)Factory.New<Integration.Customs.NZ.ICusSCAOceanBill>() };
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy; }
		}
	}
}
