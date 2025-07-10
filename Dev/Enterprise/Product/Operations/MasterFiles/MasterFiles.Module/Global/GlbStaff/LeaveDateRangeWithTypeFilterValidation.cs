using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class LeaveDateRangeWithTypeFilterValidation : ModuleFilterDateValidation
	{
		public LeaveDateRangeWithTypeFilterValidation(ModuleDateFilter parent) : base(parent)
		{
		}

		new LeaveDateRangeWithTypeFilter Parent => (LeaveDateRangeWithTypeFilter)base.Parent;

		#region ValidateWorkHolidayType

		public void ValidateWorkHolidayType()
		{
			ValidateCalculatedProperty(Parent.WorkHolidayTypeInfo);
		}

		protected void CheckWorkHolidayType()
		{
			ListValidation.ErrorIfInvalidCode(Parent.WorkHolidayTypeInfo);

			if (Parent.IsPropertySearchUsingHasNoDateEntered && !Parent.WorkHolidayType.IsEmpty)
			{
				Parent.WorkHolidayTypeInfo.AddError(Res.GetString("{C05F7A95-289C-4453-A915-3769576FA504}", "Leave type can not be used together with 'Has No Date'."));
			}
		}

		#endregion

		protected override void CheckPropertySearch()
		{
			base.CheckPropertySearch();

			if (Parent.IsPropertySearchUsingHasNoDateEntered && !Parent.WorkHolidayType.IsEmpty)
			{
				Parent.Validation.ValidateWorkHolidayType();
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateWorkHolidayType();
		}
	}
}
