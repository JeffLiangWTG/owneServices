using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityGroupPivot : AutoGlbCapabilityGroupPivot
	{
		public GlbCapabilityGroupPivot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ZDateTimeDurationValue]
		[ResourceStringData("GlbCapabilityGroupPivot.GGC_AutoAssignTasksAge", Caption = "Auto Assign Tasks Age")]
		[ReadOnlyMember(nameof(GGC_AutoAssignTasksAge_ReadOnly))]
		public override ZDateTime GGC_AutoAssignTasksAge
		{
			get => base.GGC_AutoAssignTasksAge;
			set => base.GGC_AutoAssignTasksAge = value.ConvertToDurationBasedDate(GGC_AutoAssignTasksAgeInfo);
		}

		bool GGC_AutoAssignTasksAge_ReadOnly => !GGC_AllowTaskAutoAssignment;

		[ResourceStringData("GlbCapabilityGroupPivot.GGC_AllowTaskAutoAssignment", Caption = "Auto Assign Tasks")]
		public override ZBool GGC_AllowTaskAutoAssignment
		{
			get => base.GGC_AllowTaskAutoAssignment;
			set
			{
				base.GGC_AllowTaskAutoAssignment = value;

				if (!value && (GGC_AutoAssignTasksAge.IsEmpty || !GGC_AutoAssignTasksAge.IsValid))
				{
					GGC_AutoAssignTasksAge = new ZInt(0).GetDateTimeFromMinutes();
				}
			}
		}

		[ResourceStringData("GlbCapabilityGroupPivot.GGC_CapabilityStartableWorkflowLimit", Caption = "Startable Workflow Limit")]
		public override ZShort GGC_CapabilityStartableWorkflowLimit
		{
			get => base.GGC_CapabilityStartableWorkflowLimit;
			set => base.GGC_CapabilityStartableWorkflowLimit = value;
		}

		[ResourceStringData("GlbCapabilityGroupPivot.GGC_GG_Group", Caption = "Group Code")]
		public override ZGuid GGC_GG_Group
		{
			get => base.GGC_GG_Group;
			set => base.GGC_GG_Group = value;
		}

		[ResourceStringData("GlbCapabilityGroupPivot.GroupName", Caption = "Group Name")]
		public ZString GroupName => Group != null ? Group.GG_Desc : ZString.Empty;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			var row = ((IBusinessObjectInternals)this).Row;
			row[GlbCapabilityGroupPivotSchema.Constants.GGC_AutoAssignTasksAge] = new ZInt(0).GetDateTimeFromMinutes();
		}
	}
}
