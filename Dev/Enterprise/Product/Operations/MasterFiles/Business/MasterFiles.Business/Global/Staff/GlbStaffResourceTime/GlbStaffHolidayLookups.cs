using Enterprise.Registry.Business;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayLookups : AutoGlbStaffHolidayLookups
	{
		public GlbStaffHolidayLookups(AutoGlbStaffHoliday parent)
			: base(parent)
		{
		}

		#region Record Types

		public static class RecordTypes
		{
			public const string Leave = "LEV";
			public const string TimeAllocation = "TIM";
		}

		#endregion

		#region Types

		public CodeDescriptionBoolCollection Types
		{
			get
			{
				if (fTypes == null)
				{
					fTypes = GetTypes();
				}

				return fTypes;
			}
		}

		CodeDescriptionBoolCollection fTypes;

		protected virtual CodeDescriptionBoolCollection GetTypes()
		{
			return new CodeDescriptionBoolCollection();
		}

		#endregion
	}
}
