using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCapabilityValidation : AutoGlbCapabilityValidation
	{
		public GlbCapabilityValidation(AutoGlbCapability parent)
			: base(parent)
		{
		}

		protected override void CheckG4_AutoAssignTasksAge()
		{
			if (Parent.G4_AllowTaskAutoAssignment && Parent.G4_AutoAssignTasksAge == ZDateTime.Empty)
			{
				Parent.G4_AutoAssignTasksAgeInfo.AddWarning(ResString.GetMultilingualString("C8DCE445-7C07-4BE9-B35A-AC2AAE0A312A", "No auto assign task age is set, release group auto assign task age will be applied if available."));
			}
			else if (Parent.G4_AllowTaskAutoAssignment && Parent.G4_AutoAssignTasksAge.IsValid && CheckZeroTime(Parent.G4_AutoAssignTasksAge))
			{
				Parent.G4_AutoAssignTasksAgeInfo.AddWarning(ResString.GetMultilingualString("F86E0E51-F891-4469-82C4-E44F1E8FD5DA", "000:00 indicates task will be assigned immediately after release."));
			}
		}

		internal static bool CheckZeroTime(ZDateTime dateTime)
		{
			return (dateTime.Hour + dateTime.Minute + dateTime.Minute + dateTime.Day + dateTime.Month) == 2;    //checking that the time is the moment a new year has begun
		}

		protected override void CheckG4_AutoAssignTasksAgeIsValidZDateTime()
		{
			var value = Parent.G4_AutoAssignTasksAgeInfo.Value;
			if (!value.IsEmpty && !value.IsValid)
			{
				var description = ResString.GetMultilingualString("F008D70C-1D8B-4B42-B0C2-80785A67782E", "Auto Assign Tasks Age. Correct format should be 000:00");
				var message = string.Format(TypeValidation.InvalidTypeMessage, description);
				Parent.G4_AutoAssignTasksAgeInfo.AddError(message);
			}
		}

		protected override void CheckG4_Code()
		{
			base.CheckG4_Code();
			MandatoryValidation.CheckEntered(Parent.G4_CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.G4_CodeInfo, Parent.Factory.Load<GlbCapability>(new ZQuery()));
		}

		protected override void CheckG4_Description()
		{
			base.CheckG4_Description();
			MandatoryValidation.CheckEntered(Parent.G4_DescriptionInfo);
		}
	}
}
