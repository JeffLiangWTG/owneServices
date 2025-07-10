using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public abstract class RefTimeZoneRuleCollection : ActiveBusinessObjectCollection<RefTimeZoneRule>
	{
		public RefTimeZoneRuleCollection(DaylightSavingTimeZone parent) : base(parent)
		{
		}
	}
}
