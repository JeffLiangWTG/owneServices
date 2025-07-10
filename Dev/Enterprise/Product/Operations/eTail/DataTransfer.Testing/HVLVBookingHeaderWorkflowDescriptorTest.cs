using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.eTail.DataTransfer.Testing
{
	[TestedType(typeof(HVLVBookingHeaderWorkflowDescriptor))]
	public class HVLVBookingHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<HVLVBookingHeaderWorkflowDescriptor>
	{
		public override void TestDescription() => AssertEquals("HVLV Booking Header", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("HVH", WorkflowDescriptor.Code);

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

		public override void TestSubTypes()
		{
			AssertEquals(1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Service Level", WorkflowDescriptor.SubTypeInformation[0].Description);
		}

		public override void TestSupportsWorkflowTemplates() => AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);

		public void TestSupportsUniversalTemplates() => AssertEquals(true, WorkflowDescriptor.SupportsUniversalTemplates);

		public override void TestSupportsEventTracking() => Assert(WorkflowDescriptor.SupportsEventTracking);

		public void TestSupportsHVLVPreScreening() => AssertEquals(true, WorkflowDescriptor.SupportsHVLVPreScreening);

		#region IEventSubscriptionAgent

		public void TestFireWorkflow_AIR()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertFireWorkflow(() => Factory.New<Customs.Business.CusMAWB>(),
					(parent, id) =>
					{
						var child = parent.ChildBills.AddNew();
						child.CS_HAWB = id;
						return child;
					},
					JobInvoicingConsumerTypes.CusMAWB.Code,
					WorkflowDescriptors.CustomsHouseAirCargoCode);
			}
		}

		public void TestFireWorkflow_SEA()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertFireWorkflow(() => Factory.New<CusSCAOceanBill>(),
					(parent, id) =>
					{
						var child = parent.HouseBills.AddNew();
						child.CA_HouseBill = id;
						return child;
					},
					WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode,
					WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode);
			}
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var processTask = bookingHeader.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTask.ProcessTaskNotifications.Add(action);
			Factory.Save();

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening;
			var resultProcessor = WorkflowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Processor for RunHVLVPreScreening should be HVLVPreScreeningProcessor", "HVLVPreScreeningProcessor", resultProcessor.GetType().Name);
		}

		void AssertFireWorkflow<TParent, TChild>(Func<TParent> createNewParent, Func<TParent, string, TChild> createNewChildWithId, string registrarDescriptor, string publisherDescriptor)
		{
			var header1 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment1_1 = header1.Consignments.AddNew();
			consignment1_1.HVC_WaybillNumber = "CONSIGN1_1";
			var consignment1_2 = header1.Consignments.AddNew();
			consignment1_2.HVC_WaybillNumber = "CONSIGN1_2";

			var header2 = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment2_1 = header2.Consignments.AddNew();
			consignment2_1.HVC_WaybillNumber = "CONSIGN2_1";
			var consignment2_2 = header2.Consignments.AddNew();
			consignment2_2.HVC_WaybillNumber = "CONSIGN2_2";

			Factory.Save();

			AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1_1", consignment1_1.HVC_ConsignmentId);
			AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN1_2", consignment1_2.HVC_ConsignmentId);
			AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2_1", consignment2_1.HVC_ConsignmentId);
			AssertEquals("Precondition: Consignment ID defaulted", "CONSIGN2_2", consignment2_2.HVC_ConsignmentId);

			var parent1 = createNewParent();
			var child1_1 = createNewChildWithId(parent1, "CONSIGN1_1");
			var child1_2 = createNewChildWithId(parent1, "CONSIGN1_2");

			var parent2 = Factory.New<Enterprise.Integration.Customs.Shared.ICusMAWB>();
			var child2_1 = createNewChildWithId(parent1, "CONSIGN2_1");
			var child2_2 = createNewChildWithId(parent1, "XXYYZZ");

			var subscription1 = CreateSubscription((parent1 as BusinessObject).PK, header1.PK, registrarDescriptor, publisherDescriptor);
			var subscription2 = CreateSubscription((parent2 as BusinessObject).PK, header2.PK, registrarDescriptor, publisherDescriptor);

			Factory.Save();

			IEventSubscriptionAgent workflowDescriptor = new HVLVBookingHeaderWorkflowDescriptor();
			var event1_1 = (child1_1 as BusinessObject).GetLogs().AddNew(AutoEvents.Departure);
			var event1_2 = (child1_2 as BusinessObject).GetLogs().AddNew(AutoEvents.Departure);
			var event2_1 = (child2_1 as BusinessObject).GetLogs().AddNew(AutoEvents.Departure);
			var event2_2 = (child2_2 as BusinessObject).GetLogs().AddNew(AutoEvents.Departure);

			var results = workflowDescriptor.GetLogParentsToFireWorkflowForEvent(subscription1, event2_1);
			Assert("Tried to fire event under a different subscription, no milestones should be fired", !results.Any());

			results = workflowDescriptor.GetLogParentsToFireWorkflowForEvent(subscription1, event1_1);
			AssertContainsExactElementsInAnyOrder("event1_1 fired", new[] { consignment1_1 }, results);

			results = workflowDescriptor.GetLogParentsToFireWorkflowForEvent(subscription1, event1_2);
			AssertContainsExactElementsInAnyOrder("event1_2 fired", new[] { consignment1_2 }, results);

			results = workflowDescriptor.GetLogParentsToFireWorkflowForEvent(subscription2, event2_1);
			AssertContainsExactElementsInAnyOrder("event2_1 fired", new[] { consignment2_1 }, results);

			results = workflowDescriptor.GetLogParentsToFireWorkflowForEvent(subscription2, event2_2);
			Assert("event2_2 not fired because ID numbers don't match", !results.Any());
		}

		StmEventSubscription CreateSubscription(ZGuid registrar, ZGuid agent, string registrarDescriptor, string publisherDescriptor)
		{
			var subscription = Factory.New<StmEventSubscription>();
			subscription.SES_SubscriberDescriptor = WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;
			subscription.SES_PublisherDescriptor = publisherDescriptor;
			subscription.SES_AgentDescriptor = WorkflowDescriptors.HVLVBookingHeaderWorkflowDescriptorCode;
			subscription.SES_RegistrarDescriptor = registrarDescriptor;
			subscription.SES_RegistrarParentId = registrar;
			subscription.SES_AgentParentId = agent;

			return subscription;
		}

		#endregion

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { BookingHeaderWithOrganisations };
		}

		HVLVBookingHeader BookingHeaderWithOrganisations => bookingHeaderWithOrganisations ?? (bookingHeaderWithOrganisations = CreateBookingHeaderWithOrganisations());
		HVLVBookingHeader bookingHeaderWithOrganisations;

		HVLVBookingHeader CreateBookingHeaderWithOrganisations()
		{
			var header = Factory.New<HVLVBookingHeader>();
			header.HVH_OA_BillToParty = ConsignorOrg.Addresses[0].PK;
			return header;
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.BillToParty |
					MessageRecipientPartyType.OrgProxy |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion
	}
}
