using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class RoutingSupportProcessTaskTest<TJob> : ProcessTaskTest
			where TJob : BusinessObject, IRoutingSupport, IWorkflowProvider
	{
		protected abstract ZString ParentOrigin { get; set; }
		protected abstract ZString ParentDestination { get; set; }

		public void TestCanBeLinkedToTransport()
		{
			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();

			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("A milestone with an arrival / departure event can be linked to a transport", true, milestone.CanBeLinkedToTransport);
			milestone.TriggerConditions.TriggerEventCode = "";

			milestone.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ETA.Name;
			AssertEquals("A milestone with a transport trigger field can be linked to a transport", true, milestone.CanBeLinkedToTransport);

			milestone.TriggerConditions.TriggerFieldName = "xxx";
			AssertEquals("A milestone with no transport leg trigger field or transport linked event cannot be linked to a transport", false, milestone.CanBeLinkedToTransport);
		}

		#region Validation

		public void TestMilestoneAndWorkflowTriggerValidation()
		{
			ProcessTask task = WorkflowProvider.WorkflowItems.Tasks.AddNew();
			AssertEquals("Validation for task", typeof(ProcessTaskValidation), task.Validation.GetType());

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			AssertEquals("Validation for milestone", typeof(RoutingSupportMilestoneValidation), milestone.Validation.GetType());

			ProcessTask trigger = WorkflowProvider.WorkflowItems.Triggers.AddNew();
			AssertEquals("Validation for workflow trigger", typeof(RoutingSupportWorkflowTriggerValidation), trigger.Validation.GetType());
		}

		#endregion

		#region Estimated / Actual Dates

		public void TestP9_ActualDate_ReadOnlyWhenEventLinkedToSailingSchedule()
		{
			TJob job = this.Job;
			Origin.JA_RL_NKPortOfLoading = DestinationTransportLoadPort;
			Destination.JB_RL_NKPortOfDischarge = DestinationTransportDischargePort;
			OriginTransport.JW_JX = Sailing.PK;
			OriginTransport.JW_IsLinked = true;

			ProcessTask milestone = WorkflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			milestone.P9_ReferencedID = OriginTransport.PK;

			AssertEquals("Read only when transport linked to sailing", true, milestone.P9_ActualDateInfo.ReadOnly);
			AssertEquals("Read only when transport linked to sailing", true, milestone.P9_ActualDateInfo.ReadOnly);
			OriginTransport.JW_IsLinked = false;
			AssertEquals("Writable when transport not linked to sailing", false, milestone.P9_ActualDateInfo.ReadOnly);
			OriginTransport.Delete();
			AssertEquals("Writable when transport not linked to sailing", false, milestone.P9_ActualDateInfo.ReadOnly);
		}

		#endregion

		#region PortPair Property

		public void TestGetPortPair()
		{
			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			Transport originTransport = this.OriginTransport;
			Transport intermediateTransport = this.IntermediateTransport;
			Transport destinationTransport = this.DestinationTransport;

			milestone.P9_ReferencedID = originTransport.PK;
			AssertEquals(OriginTransportLoadPort + "->" + OriginTransportDischargePort, milestone.PortPair);
			milestone.P9_ReferencedID = intermediateTransport.PK;
			AssertEquals(IntermediateTransportLoadPort + "->" + IntermediateTransportDischargePort, milestone.PortPair);
			milestone.P9_ReferencedID = destinationTransport.PK;
			AssertEquals(DestinationTransportLoadPort + "->" + DestinationTransportDischargePort, milestone.PortPair);
		}

		public void TestSetPortPair()
		{
			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			Transport originTransport = this.OriginTransport;
			Transport intermediateTransport = this.IntermediateTransport;
			Transport destinationTransport = this.DestinationTransport;

			milestone.PortPair = OriginTransportLoadPort + " " + OriginTransportDischargePort;
			AssertEquals("Leg 1", originTransport.PK, milestone.P9_ReferencedID);
			milestone.PortPair = IntermediateTransportLoadPort + "->" + IntermediateTransportDischargePort;
			AssertEquals("Leg 2", intermediateTransport.PK, milestone.P9_ReferencedID);
			milestone.PortPair = DestinationTransportLoadPort + "\t-> " + DestinationTransportDischargePort;
			AssertEquals("Leg 3", destinationTransport.PK, milestone.P9_ReferencedID);
		}

		public void TestPortPairList()
		{
			Transport originTransport = this.OriginTransport;
			Transport intermediateTransport = this.IntermediateTransport;
			Transport destinationTransport = this.DestinationTransport;

			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			AssertEquals("3 legs in the list", 3, milestone.PortPairList.Count);
			AssertEquals("Leg 1", "SGSIN->MYPKG", milestone.PortPairList[0].Code);
			AssertEquals("Leg 2", "MYPKG->AUSYD", milestone.PortPairList[1].Code);
			AssertEquals("Leg 3", "AUSYD->USLAX", milestone.PortPairList[2].Code);
		}

		public void TestPortPairReadOnly()
		{
			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();

			milestone.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ATA.Name;
			AssertEquals("Transport leg may be entered with a transport field trigger", false, milestone.ReferenceCodeInfo.ReadOnly);
			milestone.TriggerConditions.TriggerFieldName = "";

			milestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
			AssertEquals("Transport leg may be entered for a departure event", false, milestone.ReferenceCodeInfo.ReadOnly);

			milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
			AssertEquals("Transport leg may be entered for an arrival event", false, milestone.ReferenceCodeInfo.ReadOnly);

			milestone.TriggerConditions.TriggerEventCode = Events.DeliveryOrderReceived.Code;
			AssertEquals("Transport leg read-only for any other condition", true, milestone.ReferenceCodeInfo.ReadOnly);
		}

		#endregion

		#region Estimate Defaulting when Linked

		public void TestExcludeEstimateDefaultingWhenTransportLinkedToSailing()
		{
			RoutingSupportProcessTask predecessor = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			RoutingSupportProcessTask milestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
			predecessor.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2005, 1, 1)));
			milestone.SetMilestoneScheduledDateForTest(new ZDateTimeOffset(new ZDateTime(2005, 2, 2)));

			milestone.P9_EstimatedDefaultFromPredecessor = predecessor.P9_Sequence;
			AssertEquals("Estimate defaulted when no transport", predecessor.P9_ScheduledDate, GetEstimateDefaultedFromDate(milestone).ToZDateTime());

			milestone.P9_ReferencedID = OriginTransport.PK;
			OriginTransport.JW_IsLinked = false;
			AssertEquals("Estimate defaulted when transport is not linked", predecessor.P9_ScheduledDate, GetEstimateDefaultedFromDate(milestone).ToZDateTime());

			OriginTransport.JW_IsLinked = true;
			AssertEquals("Estimate defaulted when transport is linked", predecessor.P9_ScheduledDate, GetEstimateDefaultedFromDate(milestone).ToZDateTime());
		}

		ZDateTimeOffset GetEstimateDefaultedFromDate(ProcessTask milestone)
		{
			return new ZDateTimeOffset(typeof(ProcessTask).InvokeMember("EstimateDefaultedFromDate", System.Reflection.BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, milestone, null));
		}

		#endregion

		public void TestEnsureTransportLegAttachedMethodUsingP9_SE_NKMilestoneEvent_Setter()
		{
			RoutingSupportProcessTaskForTest task = Factory.NewWithValidTestData<RoutingSupportProcessTaskForTest>();

			task.SetValueForGetTransportLegToAttach(null);
			task.TriggerConditions.TriggerEventCode = "ARV";
			AssertEquals(ZGuid.Empty, task.P9_ReferencedID);
			AssertEquals(ZString.Empty, task.P9_ReferencedTableCode);

			task.SetValueForGetTransportLegToAttach(OriginTransport);
			task.TriggerConditions.TriggerEventCode = "DEP";
			AssertEquals(OriginTransport.PK, task.P9_ReferencedID);
			AssertEquals(JobConsolTransportSchema.Constants.Prefix, task.P9_ReferencedTableCode);

			task.SetValueForGetTransportLegToAttach(null);
			task.TriggerConditions.TriggerEventCode = "ARV";
			AssertEquals(ZGuid.Empty, task.P9_ReferencedID);
			AssertEquals(ZString.Empty, task.P9_ReferencedTableCode);
		}

		public void TestEnsureTransportLegAttachedMethodUsingP9_TriggerField_Setter()
		{
			RoutingSupportProcessTaskForTest task = Factory.NewWithValidTestData<RoutingSupportProcessTaskForTest>();

			task.SetValueForGetTransportLegToAttach(null);
			task.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			AssertEquals(ZGuid.Empty, task.P9_ReferencedID);
			AssertEquals(ZString.Empty, task.P9_ReferencedTableCode);

			task.SetValueForGetTransportLegToAttach(OriginTransport);
			task.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATD;
			AssertEquals(OriginTransport.PK, task.P9_ReferencedID);
			AssertEquals(JobConsolTransportSchema.Constants.Prefix, task.P9_ReferencedTableCode);

			task.SetValueForGetTransportLegToAttach(null);
			task.TriggerConditions.TriggerFieldName = Transport.Schema.JW_ATA;
			AssertEquals(ZGuid.Empty, task.P9_ReferencedID);
			AssertEquals(ZString.Empty, task.P9_ReferencedTableCode);
		}

		public void TestDontLogRDDExceptionIfDurationCalculationIsDisabled()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false }))
			{
				var routingJob = Factory.New<TJob>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);
				exceptionType.WET_DefaultDurationHours = 0;
				Factory.Save();

				var exception = routingJob.WorkflowItems.Exceptions.AddNew();

				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(0, exception.P9_ExceptionDurationHours);
				AssertEquals(ZDateTimeOffset.Empty, exception.P9_ActualDateForBinding);

				exceptionType.WET_DefaultDurationHours = 50;
				Factory.Save();

				exception.ExceptionTypeCode = ZString.Empty;
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				var parent = exception.Parent;
				exception.Delete();

				if (parent is IStmALogParent logParent)
				{
					Assert(!logParent.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CalculateDeliveryDateWithExceptionsRequested.Code)).Any());
				}
			} 
		}

		#region Transports

		protected Transport OriginTransport
		{
			get
			{
				if (originTransport == null)
				{
					originTransport = Job.Transports.Count > 0 ? Job.Transports[0] : Job.Transports.AddNew();
					originTransport.JW_ETD = new ZDateTime(2014, 1, 1);
					originTransport.JW_ETA = new ZDateTime(2014, 2, 2);
					originTransport.JW_RL_NKLoadPort = OriginTransportLoadPort;
					originTransport.JW_RL_NKDiscPort = OriginTransportDischargePort;
				}
				return originTransport;
			}
		}
		Transport originTransport;

		protected Transport IntermediateTransport
		{
			get
			{
				if (intermediateTransport == null)
				{
					intermediateTransport = Job.Transports.AddNew();
					intermediateTransport.JW_ETD = new ZDateTime(2014, 3, 3);
					intermediateTransport.JW_ETA = new ZDateTime(2014, 4, 4);
					intermediateTransport.JW_RL_NKLoadPort = IntermediateTransportLoadPort;
					intermediateTransport.JW_RL_NKDiscPort = IntermediateTransportDischargePort;
				}
				return intermediateTransport;
			}
		}
		Transport intermediateTransport;

		protected Transport DestinationTransport
		{
			get
			{
				if (destinationTransport == null)
				{
					destinationTransport = Job.Transports.AddNew();
					destinationTransport.JW_ETD = new ZDateTime(2014, 5, 5);
					destinationTransport.JW_ETA = new ZDateTime(2014, 6, 6);
					destinationTransport.JW_RL_NKLoadPort = DestinationTransportLoadPort;
					destinationTransport.JW_RL_NKDiscPort = DestinationTransportDischargePort;
				}
				return destinationTransport;
			}
		}
		Transport destinationTransport;

		protected const string OriginTransportLoadPort = "SGSIN";
		protected const string OriginTransportDischargePort = "MYPKG";

		protected const string IntermediateTransportLoadPort = "MYPKG";
		protected const string IntermediateTransportDischargePort = "AUSYD";

		protected const string DestinationTransportLoadPort = "AUSYD";
		protected const string DestinationTransportDischargePort = "USLAX";

		#endregion

		#region Test Objects

		#region DepartureMilestone / ArrivalMilestone

		protected RoutingSupportProcessTask DepartureMilestone
		{
			get
			{
				if (departureMilestone == null)
				{
					departureMilestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					departureMilestone.TriggerConditions.TriggerEventCode = Events.Departure.Code;
				}
				return departureMilestone;
			}
		}
		RoutingSupportProcessTask departureMilestone;

		protected RoutingSupportProcessTask ArrivalMilestone
		{
			get
			{
				if (arrivalMilestone == null)
				{
					arrivalMilestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					arrivalMilestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				}
				return arrivalMilestone;
			}
		}
		RoutingSupportProcessTask arrivalMilestone;

		protected RoutingSupportProcessTask CargoAvailableMilestone
		{
			get
			{
				if (cargoAvailableMilestone == null)
				{
					cargoAvailableMilestone = (RoutingSupportProcessTask)WorkflowProvider.WorkflowItems.Milestones.AddNew();
					cargoAvailableMilestone.TriggerConditions.TriggerEventCode = Events.CargoAvailable.Code;
				}
				return cargoAvailableMilestone;
			}
		}
		RoutingSupportProcessTask cargoAvailableMilestone;

		#endregion

		#region Job

		protected TJob Job
		{
			get
			{
				if (job == null)
				{
					job = CreateNewJob();
					ParentOrigin = "SGSIN";
					ParentDestination = "USLAX";

					Transport originTransport = this.OriginTransport;
					Transport intermediateTransport = this.IntermediateTransport;
					Transport destinationTransport = this.DestinationTransport;

					for (int i = Job.Transports.Count - 1; i >= 0; i--)
					{
						if (Job.Transports[i] != originTransport &&
							Job.Transports[i] != intermediateTransport &&
							Job.Transports[i] != destinationTransport)
						{
							Job.Transports[i].Delete();
						}
					}
					AssertEquals(3, Job.Transports.Count);
				}
				return job;
			}
		}
		TJob job;

		protected IWorkflowProvider WorkflowProvider
		{
			get { return Job; }
		}

		#endregion

		#region Voyage & Sailing

		protected JobVoyage Voyage
		{
			get
			{
				if (voyage == null)
				{
					var vessel = Factory.NewWithValidTestData<RefVessel>();
					vessel.RV_Name = "Vessel";

					voyage = Factory.New<JobVoyage>();
					voyage.JV_RV_NKVessel = vessel.RV_FK;
					voyage.JV_VoyageFlight = "Voyage";
				}
				return voyage;
			}
		}
		JobVoyage voyage;

		protected VoyageOrigin Origin
		{
			get
			{
				if (origin == null || origin.IsDeleted)
				{
					origin = Voyage.Origins.AddNew();
				}
				return origin;
			}
		}
		VoyageOrigin origin;

		protected VoyageDestination Destination
		{
			get
			{
				if (destination == null || destination.IsDeleted)
				{
					destination = Voyage.Destinations.AddNew();
				}
				return destination;
			}
		}
		VoyageDestination destination;

		protected JobSailing Sailing
		{
			get
			{
				if (sailing == null)
				{
					sailing = Voyage.Sailings.AddNew();
					sailing.JX_JA = Origin.PK;
					sailing.JX_JB = Destination.PK;
				}
				return sailing;
			}
		}
		JobSailing sailing;

		#endregion

		#endregion

		#region Duration Exceptions

		static bool IsTypeDurationSupporter(Type type)
		{
			var supportedTypes = new Type[] { typeof(ICommonShipment), typeof(ICommonConsol), typeof(ICommonContainer) };
			return supportedTypes.Any(t => t.IsAssignableFrom(type));
		}

		[TestDate(2023, 10, 01, 10, 15, 20)]
		protected void TestExceptionTypeCodeChanged_UpdatesExceptionDurationHoursAndExceptionStartDate()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var routingJob = Factory.New<TJob>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);
				Factory.Save();

				exceptionType.WET_DefaultDurationHours = 0;
				Factory.Save();

				var exception = routingJob.WorkflowItems.Exceptions.AddNew();

				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(0, exception.P9_ExceptionDurationHours);
				AssertEquals(ZDateTimeOffset.Empty, exception.P9_ActualDateForBinding);

				exceptionType.WET_DefaultDurationHours = 50;
				Factory.Save();

				exception.ExceptionTypeCode = ZString.Empty;
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(50, exception.P9_ExceptionDurationHours);
				AssertEquals(ZDateTimeOffset.Now, exception.P9_ActualDateForBinding);
			}
		}

		[TestDate(2023, 10, 01, 10, 15, 20)]
		protected void TestCreateException_SetsDefaultLocation()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var routingJob = Factory.New<TJob>();
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
				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var exception = routingJob.WorkflowItems.Exceptions.AddNew();
					AssertEquals("CNSHA", exception.P9_RL_NKExceptionLocation);
				}

				branch.GB_RL_NKHomePort = "USLAX";
				Factory.Save();
				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var exception = routingJob.WorkflowItems.Exceptions.AddNew();
					AssertEquals("USLAX", exception.P9_RL_NKExceptionLocation);
				}
			}
		}

		[TestDate(2023, 10, 01, 10, 15, 20)]
		public void TestExceptionStartOrEndDateChanged_ExceptionDurationHoursChanged()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var routingJob = Factory.New<TJob>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);
				exceptionType.WET_DefaultDurationHours = 20;

				exceptionType.WET_UseStartEndToCalculateDuration = false;
				var exception = (RoutingSupportProcessTask)routingJob.WorkflowItems.Exceptions.AddNew();
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(20, exception.P9_ExceptionDurationHours);
				exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-1);
				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(2);
				AssertEquals(20, exception.P9_ExceptionDurationHours);

				exceptionType.WET_UseStartEndToCalculateDuration = true;
				exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-2);
				AssertEquals(96, exception.P9_ExceptionDurationHours);

				exceptionType.WET_DefaultDurationHours = 0;
				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(3);
				AssertEquals(120, exception.P9_ExceptionDurationHours);

				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(-10);
				AssertEquals("Duration should not change if date range is invalid", 120, exception.P9_ExceptionDurationHours);

				exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-2).AddMinutes(60);
				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(1);
				AssertEquals(71, exception.P9_ExceptionDurationHours);

				exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-2).AddMinutes(59);
				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(1);
				AssertEquals("Duration should be rounded up", 72, exception.P9_ExceptionDurationHours);

				exception.P9_ActualDateForBinding = ZDateTimeOffset.Now.AddDays(-2).AddMinutes(1);
				exception.P9_ExceptionEndDate = ZDateTimeOffset.Now.AddDays(1);
				AssertEquals("Duration should be rounded up", 72, exception.P9_ExceptionDurationHours);
			}
		}

		public void TestLogRevisedDeliveryDueDateIfNecessary_ExceptionDurationHoursChanged()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var routingJob = Factory.New<TJob>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);
				exceptionType.WET_DefaultDurationHours = 0;
				Factory.Save();

				var exception = (RoutingSupportProcessTask)routingJob.WorkflowItems.Exceptions.AddNew();

				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(0, exception.P9_ExceptionDurationHours);
				AssertEquals(ZDateTimeOffset.Empty, exception.P9_ActualDateForBinding);

				exceptionType.WET_DefaultDurationHours = 50;
				exception.ExceptionTypeCode = ZString.Empty;
				exception.ExceptionTypeCode = exceptionType.WET_Code;
				Factory.Save();

				AssertEquals("Precondition", 50, exception.P9_ExceptionDurationHours);

				var rddEvent = routingJob.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull("RDD event should be created on new exception", rddEvent);

				exception.P9_ExceptionDurationHours = 30;
				Factory.Save();
				rddEvent = routingJob.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull(rddEvent);
				AssertEquals($"|CHG=Exception Duration Hours Changed|JOB={exception.PK}", rddEvent.SL_Reference);

				exception.Delete();
				Factory.Save();
				rddEvent = routingJob.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNotNull(rddEvent);
				AssertEquals($"|CHG=Exception Deleted|JOB={exception.PK}", rddEvent.SL_Reference);
			}
		}

		public void TestLogRevisedDeliveryDueDateIfNecessary_DeletedAndNotIsInDatabase()
		{
			if (!IsTypeDurationSupporter(typeof(TJob)))
			{
				Assert(true);
				return;
			}

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				var routingJob = Factory.New<TJob>();
				var exceptionType = Factory.New<ProcessWorkflowExceptionType>();
				exceptionType.WET_Code = "WW1";
				exceptionType.WET_Description = nameof(exceptionType);
				exceptionType.WET_DefaultDurationHours = 0;
				Factory.Save();

				var exception = (RoutingSupportProcessTask)routingJob.WorkflowItems.Exceptions.AddNew();

				exception.ExceptionTypeCode = exceptionType.WET_Code;
				AssertEquals(0, exception.P9_ExceptionDurationHours);
				AssertEquals(ZDateTimeOffset.Empty, exception.P9_ActualDateForBinding);

				exceptionType.WET_DefaultDurationHours = 50;
				exception.ExceptionTypeCode = ZString.Empty;
				exception.ExceptionTypeCode = exceptionType.WET_Code;

				exception.Delete();
				Factory.Save();

				var rddEvent = routingJob.Logs.MostRecentLogByEventTime(Events.CalculateDeliveryDateWithExceptionsRequested);
				AssertNull("RDD event should not be created on deleted exception that was not already in database", rddEvent);
			}
		}

		#endregion

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		protected virtual TJob CreateNewJob()
		{
			return Factory.New<TJob>();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return WorkflowProvider.WorkflowItems.AddNew();
		}

		protected class RoutingSupportProcessTaskForTest : RoutingSupportProcessTask
		{
			public RoutingSupportProcessTaskForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			protected override Transport GetTransportLegToAttach()
			{
				return transportLegToAttach;
			}

			Transport transportLegToAttach;

			public void SetValueForGetTransportLegToAttach(Transport transportLeg)
			{
				transportLegToAttach = transportLeg;
			}

			public override IRoutingSupport Parent
			{
				get
				{
					if (parent == null)
					{
						parent = Factory.New<CommonShipment>();
					}

					return parent;
				}
			}

			CommonShipment parent;

			protected override Type TransportType
			{
				get { return typeof(Transport); }
			}

			protected override Type TransportParentType
			{
				get { return typeof(CommonShipment); }
			}
		}

		#endregion
	}
}
