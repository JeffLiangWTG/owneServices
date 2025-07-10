using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistEntryCollection : ActiveBusinessObjectCollection<TelPreDriveChecklistEntry>
	{
		public TelPreDriveChecklistEntryCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
