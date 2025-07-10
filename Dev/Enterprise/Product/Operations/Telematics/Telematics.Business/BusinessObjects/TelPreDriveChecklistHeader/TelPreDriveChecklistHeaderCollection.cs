using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class TelPreDriveChecklistHeaderCollection : ActiveBusinessObjectCollection<TelPreDriveChecklistHeader>
	{
		public TelPreDriveChecklistHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
