using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	sealed class BranchSwitcherBusinessObject : NonPersistentBusinessObject
	{
		public BranchSwitcherBusinessObject(GlbBranch oldBranch)
			: base(oldBranch.Factory)
		{
			originalBranchPKs = new List<ZGuid> { oldBranch.PK };
			replacementBranch = oldBranch.PK;
		}

		public BranchSwitcherBusinessObject(GlbCompany oldCompany)
			: base(oldCompany.Factory)
		{
			originalBranchPKs = oldCompany.Branches.GetPKs();
			replacementBranch = ZGuid.Empty;
		}

		readonly List<ZGuid> originalBranchPKs;

		[List(nameof(ReplacementBranchList))]
		public ZGuid ReplacementBranch
		{
			get => replacementBranch;
			set
			{
				if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
				{
					foreach (var task in StmServiceTasks.Cast<StmServiceTask>())
					{
						if (replacementBranch.IsEmpty || task.SST_GB_Branch == replacementBranch)
						{
							task.SST_GB_Branch = value;
						}
					}
				}
				else
				{
					foreach (var task in ServiceTasks.Cast<ServiceTaskSchedule>())
					{
						if (replacementBranch.IsEmpty || task.S5_GB == replacementBranch)
						{
							task.S5_GB = value;
						}
					}
				}

				foreach (var task in ScheduledReports.Cast<ScheduledReportForBranchChange>())
				{
					if (replacementBranch.IsEmpty || task.S5_GB == replacementBranch)
					{
						task.S5_GB = value;
					}
				}

				foreach (var staff in Staff)
				{
					if (replacementBranch.IsEmpty || staff.GS_GB_HomeBranch == replacementBranch)
					{
						staff.GS_GB_HomeBranch = value;
					}
				}

				SetNonPersistentPropertyValue(ReplacementBranchInfo, ref replacementBranch, value);
			}
		}
		ZGuid replacementBranch;

		public ZPropertyInfo ReplacementBranchInfo => GetZPropertyInfo(nameof(ReplacementBranch));

		public GlbBranchCollection ReplacementBranchList => replacementBranchList ?? (replacementBranchList = new GlbBranchCollection(Factory));
		GlbBranchCollection replacementBranchList;

		#region Service Tasks

		public ServiceTaskScheduleCollection ServiceTasks
		{
			get
			{
				if (serviceTasks == null)
				{
					serviceTasks = new ServiceTaskScheduleCollectionForBranchChange(Factory);

					if (!SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
					{
						if (originalBranchPKs.Any())
						{
							serviceTasks.LoadWithMoreFiltering(new ZQuery(StmScheduleTaskSchema.S5_GB,
								originalBranchPKs));
						}

						RegisterEditableChildObject(serviceTasks);
					}
				}
				return serviceTasks;
			}
		}
		ServiceTaskScheduleCollectionForBranchChange serviceTasks;

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ServiceTaskScheduleForBranchChange : ServiceTaskSchedule
		{
			public ServiceTaskScheduleForBranchChange(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override ZBool S5_IsActive
			{
				get { return base.S5_IsActive; }
				set
				{
					base.S5_IsActive = value;
					Validation.ValidateS5_GB();
				}
			}

			protected override StmScheduleTaskValidation GetNewValidation()
			{
				return new ServiceTaskScheduleForBranchChangeValidation(this);
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ServiceTaskScheduleCollectionForBranchChange : ServiceTaskScheduleCollection
		{
			public ServiceTaskScheduleCollectionForBranchChange(BusinessObjectFactory factory) : base(factory, RemoteStatus.WithoutStatus) { }

			protected override bool AllowNewCore => false;

			protected override bool AllowRemoveCore => false;

			public new ServiceTaskScheduleForBranchChange this[int i] => (ServiceTaskScheduleForBranchChange)Elements[i];

			public new ServiceTaskScheduleForBranchChange AddNew()
			{
				throw new NotSupportedException();
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ServiceTaskScheduleForBranchChangeValidation : ServiceTaskScheduleValidation
		{
			public ServiceTaskScheduleForBranchChangeValidation(ServiceTaskScheduleForBranchChange parent) : base(parent) { }

			protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			{
				// Separate validation for S5_GB
				return info.Name != StmScheduleTaskSchema.Constants.S5_GB && base.ShouldValidateFKToCancelledRecord(info);
			}

			protected override void CheckS5_GB()
			{
				base.CheckS5_GB();
				if (Parent.S5_IsActive && Parent.S5_GB.IsValid && Parent.Branch != null && !Parent.Branch.GB_IsActive)
				{
					Parent.S5_GBInfo.AddError(Res.GetString("e013cc52-d244-47d9-a98b-b368c2427859", "This branch is inactive."));
				}
			}
		}

		#endregion

		#region Stm Service Tasks

		public StmServiceTaskCollection StmServiceTasks
		{
			get
			{
				if (stmServiceTasks == null)
				{
					stmServiceTasks = new StmServiceTaskCollectionForBranchChange(Factory);

					if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
					{
						if (originalBranchPKs.Count > 0)
						{
							stmServiceTasks.LoadWithMoreFiltering(new ZQuery(StmServiceTaskSchema.SST_GB_Branch,
								originalBranchPKs));
						}

						RegisterEditableChildObject(stmServiceTasks);
					}
				}
				return stmServiceTasks;
			}
		}
		StmServiceTaskCollectionForBranchChange stmServiceTasks;

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class StmServiceTaskForBranchChange : StmServiceTask
		{
			public StmServiceTaskForBranchChange(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public override ZBool SST_Active
			{
				get { return base.SST_Active; }
				set
				{
					base.SST_Active = value;
					Validation.ValidateSST_Active();
				}
			}

			protected override StmServiceTaskValidation GetNewValidation()
			{
				return new StmServiceTaskForBranchChangeValidation(this);
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class StmServiceTaskCollectionForBranchChange : StmServiceTaskCollection
		{
			public StmServiceTaskCollectionForBranchChange(BusinessObjectFactory factory) : base(factory) { }

			protected override bool AllowNewCore => false;

			protected override bool AllowRemoveCore => false;

			public new StmServiceTaskForBranchChange this[int i] => (StmServiceTaskForBranchChange)Elements[i];

			public new StmServiceTaskForBranchChange AddNew()
			{
				throw new NotSupportedException();
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class StmServiceTaskForBranchChangeValidation : StmServiceTaskValidation
		{
			public StmServiceTaskForBranchChangeValidation(StmServiceTaskForBranchChange parent) : base(parent) { }

			protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			{
				// Separate validation for S5_GB
				return info.Name != StmScheduleTaskSchema.Constants.S5_GB && base.ShouldValidateFKToCancelledRecord(info);
			}

			protected override void CheckSST_Active()
			{
				if (Parent.SST_Active && Parent.SST_GB_Branch.IsValid && Parent.Branch != null && !Parent.Branch.GB_IsActive)
				{
					Parent.SST_GB_BranchInfo.AddError(Res.GetString("286F28C8-4419-49E2-A93E-38F08548B6B7", "This branch is inactive."));
				}
			}
		}

		#endregion

		#region Scheduled Reports

		public ReportScheduleTaskCollection ScheduledReports
		{
			get
			{
				if (scheduledReports == null)
				{
					scheduledReports = new ScheduledReportCollectionForBranchChange(Factory);

					if (originalBranchPKs.Any())
					{
						scheduledReports.LoadWithMoreFiltering(new ZQuery(StmScheduleTaskSchema.S5_GB, originalBranchPKs));
					}

					RegisterEditableChildObject(scheduledReports);
				}
				return scheduledReports;
			}
		}
		ScheduledReportCollectionForBranchChange scheduledReports;

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ScheduledReportForBranchChange : ReportScheduleTask
		{
			public ScheduledReportForBranchChange(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override bool S5_GB_ReadOnly => false;

			public override ZBool S5_IsActive
			{
				get { return base.S5_IsActive; }
				set
				{
					base.S5_IsActive = value;
					Validation.ValidateS5_GB();
				}
			}

			protected override StmScheduleTaskValidation GetNewValidation()
			{
				return new ScheduledReportForBranchChangeValidation(this);
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ScheduledReportCollectionForBranchChange : ReportScheduleTaskCollection
		{
			public ScheduledReportCollectionForBranchChange(BusinessObjectFactory factory) : base(factory) { }

			protected override bool AllowNewCore => false;

			protected override bool AllowRemoveCore => false;

			public new ScheduledReportForBranchChange this[int i] => (ScheduledReportForBranchChange)Elements[i];

			public new ScheduledReportForBranchChange AddNew()
			{
				throw new NotSupportedException();
			}
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
		class ScheduledReportForBranchChangeValidation : ReportScheduleTaskValidation
		{
			public ScheduledReportForBranchChangeValidation(ScheduledReportForBranchChange parent) : base(parent) { }

			protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
			{
				// Separate validation for S5_GB
				return info.Name != StmScheduleTaskSchema.Constants.S5_GB && base.ShouldValidateFKToCancelledRecord(info);
			}

			protected override void CheckS5_GB()
			{
				base.CheckS5_GB();
				if (Parent.S5_IsActive && Parent.S5_GB.IsValid && Parent.Branch != null && !Parent.Branch.GB_IsActive)
				{
					Parent.S5_GBInfo.AddError(Res.GetString("e013cc52-d244-47d9-a98b-b368c2427859", "This branch is inactive."));
				}
			}
		}

		#endregion

		#region Staff

		public GlbStaffCollection Staff
		{
			get
			{
				if (staff == null)
				{
					staff = new GlbStaffCollection(Factory);

					var listOfFoundStaffPKs = GetListOfStaffPKs();
					staff.AdditionalFilter = new ZQuery(GlbStaffSchema.PK, listOfFoundStaffPKs);

					RegisterEditableChildObject(staff);
				}

				return staff;
			}
		}
		GlbStaffCollection staff;

		List<ZGuid> GetListOfStaffPKs()
		{
			var listOfFoundStaffPKs = new GlbStaffCollection(Factory)
			{
				AdditionalFilter = new ZQuery(GlbStaffSchema.GS_GB_HomeBranch, originalBranchPKs)
			};
			return listOfFoundStaffPKs.Select(s => s.PK).ToList();
		}

		void AdditionalValidationForGlbStaff(GlbStaff staff)
		{
			staff.Validation.ValidateGS_GB_HomeBranch();
			if (staff.GS_IsActive && staff.GS_GB_HomeBranch.IsValid && staff.HomeBranch != null && !staff.HomeBranch.GB_IsActive)
			{
				staff.GS_GB_HomeBranchInfo.AddError(Res.GetString("DEDC5551-3422-4664-8C93-C11EDA24E465", "This branch is inactive."));
			}
		}

		#endregion

		public bool HasActiveSchedules
		{
			get
			{
				var hasActiveReports = ScheduledReports.Cast<ScheduledReportForBranchChange>().Any(r => r.S5_IsActive);

				return SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value
					? hasActiveReports || StmServiceTasks.Cast<StmServiceTask>().Any(t => t.SST_Active)
					: hasActiveReports || ServiceTasks.Cast<StmScheduleTask>().Any(t => t.S5_IsActive);
			}
		}

		public bool HasActiveStaff => Staff.Any(s => s.GS_IsActive);

		public void RefreshValidationOnBranches()
		{
			if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				foreach (var task in StmServiceTasks.Cast<StmServiceTask>())
				{
					PerformStmServiceTaskValidation(task);
				}

				foreach (var report in ScheduledReports.Cast<StmScheduleTask>())
				{
					PerformServiceTaskValidation(report);
				}
			}
			else
			{
				foreach (var task in ServiceTasks.Concat(ScheduledReports).Cast<StmScheduleTask>())
				{
					PerformServiceTaskValidation(task);
				}
			}

			foreach (var staff in Staff)
			{
				PerformStaffValidation(staff);
			}

			void PerformServiceTaskValidation(StmScheduleTask serviceTask)
			{
				serviceTask.Validation.ValidateS5_GB();

				if (serviceTask.S5_IsActiveInfo.HasChanges)
				{
					serviceTask.Validation.ValidateS5_IsActive();
				}
			}

			void PerformStmServiceTaskValidation(StmServiceTask stmServiceTask)
			{
				stmServiceTask.Validation.ValidateSST_GB_Branch();

				if (stmServiceTask.SST_GB_BranchInfo.HasChanges)
				{
					stmServiceTask.Validation.ValidateSST_Active();
				}
			}

			void PerformStaffValidation(GlbStaff staff)
			{
				staff.GS_GB_HomeBranchInfo.AdditionalValidation += delegate
					{ AdditionalValidationForGlbStaff(staff); };
				staff.Validation.ValidateGS_GB_HomeBranch();

				if (staff.GS_IsActiveInfo.HasChanges)
				{
					staff.Validation.ValidateGS_IsActive();
				}
			}
		}

		public void CancelBranchChanges()
		{
			if (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.Value)
			{
				foreach (var task in StmServiceTasks.Concat(ScheduledReports))
				{
					task.CancelChanges();
				}
			}
			else
			{
				foreach (var task in ServiceTasks.Concat(ScheduledReports))
				{
					task.CancelChanges();
				}
			}

			foreach (var staff in Staff)
			{
				staff.CancelChanges();
			}
		}
	}
}
