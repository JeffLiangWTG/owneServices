using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StationsCollection : NonPersistentBusinessObjectCollection<RadioStation>
	{
		public StationsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new RadioStation(Factory);
		}
	}
}
