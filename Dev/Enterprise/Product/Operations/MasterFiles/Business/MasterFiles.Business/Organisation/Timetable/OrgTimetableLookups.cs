using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgTimetableLookups : AutoOrgTimetableLookups
	{
		public OrgTimetableLookups(AutoOrgTimetable parent) : base(parent)
		{
		}

		public CodeDescriptionPairList Types
		{
			get
			{
				return Factory.GetCachedValue("OrgTimetableLookups.Types",
					() =>
					{
						var types = new CodeDescriptionPairList();
						types.AddPair(OrgTimetableType.Codes.Pickup, OrgTimetableType.Descriptions.Pickup);
						types.AddPair(OrgTimetableType.Codes.Deliver, OrgTimetableType.Descriptions.Deliver);
						return types;
					});
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
	}
}
