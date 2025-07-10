using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobShipmentPreplanningProcessTask))]
	sealed class JobShipmentPreplanningProcessTaskTest : RoutingSupportProcessTaskTest<JobShipmentPreplanning>
	{
		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromETD()
		{
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETD;
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(1);

			OriginTransport.JW_ETD = new ZDateTime(2005, 1, 1);
			Factory.Save();
			AssertEquals("Estimated date populated", new ZDateTime(2005, 1, 2), milestone.P9_ScheduledDate.ToZDateTime());
		}

		[TestDate(2010, 1, 1)]
		public void TestEstimateDefaultedFromETA()
		{
			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.P9_EstimatedDefaultedFrom = ShipmentPreAdviceMilestoneEstimateDefaultedFromList.Codes.ETA;
			milestone.P9_EstimatedDefaultTimeDelta = new ZDateTime(2000, 1, 1).AddDays(1);

			DestinationTransport.JW_ETA = new ZDateTime(2005, 1, 1);
			Factory.Save();
			AssertEquals("Estimated date populated", new ZDateTime(2005, 1, 2), milestone.P9_ScheduledDate.ToZDateTime());
		}

		public void TestGetTransportLegToAttach()
		{
			AssertEquals("Departure milestone transport defaulted", OriginTransport, DepartureMilestone.Transport);
			AssertEquals("Arrival milestone transport defaulted", DestinationTransport, ArrivalMilestone.Transport);
		}

		public void TestMatchesTriggerAction_ForXMFTrigger()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			JobShipmentPreplanning preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_JS = shipment.PK;

			ProcessTask consolTrigger = ((IWorkflowProvider)consol).WorkflowItems.Triggers.AddNew();
			ProcessTask shipmentTrigger = shipment.WorkflowItems.Triggers.AddNew();
			ProcessTask preplanningTrigger = preplanning.WorkflowItems.Triggers.AddNew();

			AssertEquals(false, preplanningTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals(false, shipmentTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals(false, preplanningTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals(false, consolTrigger.MatchesTriggerAction(preplanningTrigger));

			ProcessTaskNotification consolNotification = consolTrigger.ProcessTaskNotifications.AddNew();
			consolNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification shipmentNotification = shipmentTrigger.ProcessTaskNotifications.AddNew();
			shipmentNotification.PQ_TriggerType = "XMF";
			ProcessTaskNotification preplanningNotification = preplanningTrigger.ProcessTaskNotifications.AddNew();
			preplanningNotification.PQ_TriggerType = "XMF";

			AssertEquals("Preadvice matches preadvice", true, preplanningTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals("Preadvice matches attached shipment", true, preplanningTrigger.MatchesTriggerAction(shipmentTrigger));
			AssertEquals("Shipment matches attached preadvice", true, shipmentTrigger.MatchesTriggerAction(preplanningTrigger));
			AssertEquals("Preadvice matches attached consol", true, preplanningTrigger.MatchesTriggerAction(consolTrigger));
			AssertEquals("Consol matches attached preadvice", true, consolTrigger.MatchesTriggerAction(preplanningTrigger));
		}

		public void TestCreateException_DoesNotSetDefaultLocation()
		{
			var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
			exceptionType.WET_Code = "WW1";
			exceptionType.WET_Description = nameof(exceptionType);

			var staff = Factory.New<GlbStaff>();
			staff.GS_GB_HomeBranch = Env.CurrentBranch.PK;
			staff.GS_GE_HomeDepartment = Env.CurrentDepartment.PK;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "XXX";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch);
			Factory.Save();

			branch.GB_RL_NKHomePort = "CNSHA";
			Factory.Save();

			var preadvice = Factory.New<JobShipmentPreplanning>();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var exception = (JobShipmentPreplanningProcessTask)preadvice.WorkflowItems.Exceptions.AddNew();
				AssertEquals(ZString.Empty, exception.P9_RL_NKExceptionLocation);
			}
		}

		#region Closing Order AllImportDocumentsReceived Exceptions

		public void TestClosingPreadviceAIDExceptionClosesOrderAIDException()
		{
			JobShipmentPreplanning preadvice = Factory.New<JobShipmentPreplanning>();
			ProcessTask preadviceException = preadvice.WorkflowItems.Exceptions.AddNew();
			preadviceException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			Order order = preadvice.Orders.AddNew();
			ProcessTask orderException = order.WorkflowItems.Exceptions.AddNew();
			orderException.TriggerConditions.TriggerEventCode = Events.AllImportDocumentsReceived.Code;

			AssertEquals("Order milestone not actioned initially", false, orderException.IsClosed);
			preadviceException.IsExceptionActioned = true;
			AssertEquals("Order milestone actioned", true, orderException.IsExceptionActioned);
		}

		#endregion

		#region Implementation

		protected override JobShipmentPreplanning CreateNewJob()
		{
			return Factory.NewWithValidTestData<JobShipmentPreplanning>();
		}

		protected override ZString ParentOrigin
		{
			get { return Job.EF_RL_NKPortLoad; }
			set { Job.EF_RL_NKPortLoad = value; }
		}

		protected override ZString ParentDestination
		{
			get { return Job.EF_RL_NKPortDisch; }
			set { Job.EF_RL_NKPortDisch = value; }
		}

		#endregion
	}
}
