using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class DaylightSavingTimeZoneCollection : RefTimeZoneCollection
	{
		public DaylightSavingTimeZoneCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DaylightSavingTimeZone this[int index]
		{
			get { return (DaylightSavingTimeZone)base[index]; }
		}

		public new DaylightSavingTimeZone AddNew()
		{
			return (DaylightSavingTimeZone)base.AddNew();
		}
	}
}
