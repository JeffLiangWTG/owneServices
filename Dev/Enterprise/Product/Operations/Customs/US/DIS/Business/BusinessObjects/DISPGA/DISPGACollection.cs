using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISPGACollection : NonPersistentBusinessObjectCollection<DISPGA>
	{
		public DISPGACollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DISPGA(Factory);
		}
	}
}
