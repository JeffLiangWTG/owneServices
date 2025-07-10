using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class DefaultOrgTimetableLookups : ZLookups
	{
		public DefaultOrgTimetableLookups(DefaultOrgTimetable parent) : base(parent)
		{
		}

		CodeDescriptionPairList types;
		public CodeDescriptionPairList Types
		{
			get
			{
				if (types == null)
				{
					types = new CodeDescriptionPairList();
					types.AddPair(OrgTimetableType.Codes.Pickup, OrgTimetableType.Descriptions.Pickup);
					types.AddPair(OrgTimetableType.Codes.Deliver, OrgTimetableType.Descriptions.Deliver);
				}
				return types;
			}
		}

		CodeDescriptionPairList days;
		public CodeDescriptionPairList Days
		{
			get
			{
				if (days == null)
				{
					days = new DayOfWeekCodeList();
				}
				return days;
			}
		}

		#region Implementation

		protected new DefaultOrgTimetable Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (DefaultOrgTimetable)base.Parent; }
		}

		#endregion
	}
}
