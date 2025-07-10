using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CountryStatesGlbHolidayEntry : NonPersistentBusinessObject
	{
		readonly CountryStatesGlbHolidayBizo parentEntry;

		public CountryStatesGlbHolidayEntry() { }
		public CountryStatesGlbHolidayEntry(CountryStatesGlbHolidayBizo parentEntry, ZGuid statePK, ZString stateName, ZBool isChecked)
		{
			using (SuspendSettingHasChangesIncludingChildren())
			{
				StatePK = statePK;
				StateName = stateName;
				IsChecked = isChecked;
				this.parentEntry = parentEntry;
			}
		}

		#region Properties
		public ZGuid? HolidayPK { get; set; }
		public ZGuid StatePK { get; set; }
		public ZString StateName
		{
			get
			{
				return stateName;
			}
			set
			{
				SetNonPersistentPropertyValue(StateNameInfo, ref stateName, value);
			}
		}
		ZString stateName;
		public ZPropertyInfo StateNameInfo
		{
			get { return GetZPropertyInfo(nameof(StateName)); }
		}
		public ZBool IsChecked
		{
			get
			{
				return isChecked;
			}
			set
			{
				SetNonPersistentPropertyValue(IsCheckedInfo, ref isChecked, value);
				if (!IsValidationSuspended && parentEntry != null && !parentEntry.isSelectAll)
				{
					parentEntry.Validation.ValidateAll();
				}
			}
		}
		ZBool isChecked;
		public ZPropertyInfo IsCheckedInfo
		{
			get { return GetZPropertyInfo(nameof(IsChecked)); }
		}

		#endregion
	}
}
