using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Tracking.Business
{
	public class PlannedVoyagesCollection : NonPersistentBusinessObjectCollection<PlannedVoyage>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PlannedVoyage();
		}
	}

	public class PlannedVoyage : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ZString VoyageType { get; set; }
		public ZString Vessel { get; set; }
		public ZString Voyage { get; set; }
		public ZDateTime ETD { get; set; }
		public ZDateTime ETA { get; set; }
	}
}
