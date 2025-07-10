using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityGroupPivotValidation : AutoGlbCapabilityGroupPivotValidation
	{
		public GlbCapabilityGroupPivotValidation(AutoGlbCapabilityGroupPivot parent)
			: base(parent)
		{
		}

		protected override void CheckGGC_AutoAssignTasksAge()
		{
			base.CheckGGC_AutoAssignTasksAge();

			if (Parent.GGC_AllowTaskAutoAssignment)
			{
				if (Parent.GGC_AutoAssignTasksAge.IsValid && GlbCapabilityValidation.CheckZeroTime(Parent.GGC_AutoAssignTasksAge))
				{
					Parent.GGC_AutoAssignTasksAgeInfo.AddWarning(ResString.GetMultilingualString("4eea3e20-f5bf-406e-9efb-4132767c488d", "000:00 indicates task will be assigned immediately after release."));
				}
			}
		}

		protected override void CheckGGC_AutoAssignTasksAgeIsValidZDateTime()
		{
			base.CheckGGC_AutoAssignTasksAgeIsValidZDateTime();

			var value = Parent.GGC_AutoAssignTasksAgeInfo.Value;
			if (!value.IsEmpty && !value.IsValid)
			{
				var description = ResString.GetMultilingualString("f795fdc5-3d23-457d-9ad8-3536f79ce753", "Auto Assign Tasks Age. Correct format should be 000:00");
				var message = string.Format(TypeValidation.InvalidTypeMessage, description);
				Parent.GGC_AutoAssignTasksAgeInfo.AddError(message);
			}
		}

		protected override void CheckGGC_GG_Group()
		{
			base.CheckGGC_GG_Group();

			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.GGC_GG_GroupInfo, Parent.Factory.Load<GlbCapabilityGroupPivot>(new ZQuery()));
		}

		protected override void CheckGGC_GG_GroupIsNotEmpty()
		{
			if (Parent.GGC_AllowTaskAutoAssignment)
			{
				MandatoryValidation.CheckEntered(Parent.GGC_GG_GroupInfo);
			}
		}

		protected override void CheckGGC_GG_GroupIsValidZGuid()
		{
			base.CheckGGC_GG_GroupIsValidZGuid();

			var value = Parent.GGC_GG_GroupInfo.Value;
			if (!value.IsEmpty && !value.IsValid)
			{
				var description = ResString.GetMultilingualString("4f324d59-b9a6-4f62-b388-316cc92a34d1", "Group");
				var message = string.Format(TypeValidation.InvalidTypeMessage, description);
				Parent.GGC_GG_GroupInfo.AddError(message);
			}
		}
	}
}
