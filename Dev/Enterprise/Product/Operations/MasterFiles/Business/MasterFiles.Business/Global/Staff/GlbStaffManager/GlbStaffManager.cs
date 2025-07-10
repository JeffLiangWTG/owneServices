using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffManager : AutoGlbStaffManager
	{
		public GlbStaffManager(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#region Properties

		[ReadOnlyMember(nameof(IsInDatabase))]
		[List("Lookups.StaffReportingRoles")]
		public override ZString GSM_ManagerType
		{
			get => base.GSM_ManagerType;
			set
			{
				base.GSM_ManagerType = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGSM_GS_Manager();
				}
			}
		}

		public bool IsDirectManager => GSM_ManagerType == DefaultStaffReportingRoles.Codes.DirectManager;

		public override ZDateTime GSM_EffectiveDate
		{
			get => base.GSM_EffectiveDate;
			set
			{
				base.GSM_EffectiveDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGSM_ManagerType();
					Validation.ValidateGSM_EndDate();
				}
			}
		}

		public override ZDateTime GSM_EndDate
		{
			get => base.GSM_EndDate;
			set
			{
				base.GSM_EndDate = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateGSM_ManagerType();
					Validation.ValidateGSM_EffectiveDate();
				}
			}
		}

		public bool IsCurrentManager => GSM_EffectiveDate.Date <= ZDateTime.Today.AddDays(1) && (GSM_EndDate.IsEmpty || GSM_EndDate >= ZDateTime.Today) && GSM_IsApproved;
		public bool IsFutureManager => GSM_EffectiveDate.Date > ZDateTime.Today && (GSM_EndDate.IsEmpty || GSM_EndDate > ZDateTime.Today);

		public bool HasCycle()
		{
			using (var command = Db.Connection.Command($@"SELECT * FROM ManagerTableCyclicalDependencyDetection(@loggedInStaff, @startTime, @endTime, @managerType)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, GSM_GS_Staff.ToGuid());
				command.AddParameterBasedOnDbColumn("@startTime", GSM_EffectiveDate, GlbStaffManagerSchema.GSM_EffectiveDate);
				command.AddParameterBasedOnDbColumn("@endTime", GSM_EndDate.IsEmpty ? new ZDateTime(2079, 6, 6) : GSM_EndDate, GlbStaffManagerSchema.GSM_EndDate);
				command.AddParameterBasedOnDbColumn("@managerType", GSM_ManagerType.ToString(), GlbStaffManagerSchema.GSM_ManagerType);

				return DataUtils.GetDataTableFromCommand(command).Rows.Count > 0;
			}
		}

		public bool SelfManaged => GSM_GS_Staff == GSM_GS_Manager;

		public bool MustBeCurrent { get; set; }

		#endregion

		public StaffReportingRole ReportingRole => (StaffReportingRole)SystemDataRegistry.Instance.StaffReportingRoles.Value.FindByCode(GSM_ManagerType);

		public bool CanAddRoleForStaff()
		{
			var result = false;

			if (ReportingRole != null)
			{
				result = ReportingRole.SharedRoleAllowed || !GetOtherManagersWithOverlappingPeriods().Any();
			}

			return result;
		}

		public GlbStaffManager[] GetOtherManagersWithOverlappingPeriods()
		{
			if (Staff == null || ReportingRole == null)
			{
				return Array.Empty<GlbStaffManager>();
			}

			return Staff.Managers.Where(x =>
			{
				var hasOverlappingEffectiveDates = false;
				if (x.PK != PK && x.GSM_ManagerType == GSM_ManagerType)
				{
					var endDate = GSM_EndDate.IsEmpty ? ZDateTime.MaxSmallDateTime : GSM_EndDate;
					var xEndDate = x.GSM_EndDate.IsEmpty ? ZDateTime.MaxSmallDateTime : x.GSM_EndDate;

					if (GSM_EffectiveDate <= xEndDate && endDate >= x.GSM_EffectiveDate)
					{
						hasOverlappingEffectiveDates = true;
					}
				}
				return hasOverlappingEffectiveDates;
			}).ToArray();
		}

		protected override bool SupportsCloneCore() => true;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			GSM_EffectiveDate = ZDateTime.Today;
		}
#endif
	}
}
