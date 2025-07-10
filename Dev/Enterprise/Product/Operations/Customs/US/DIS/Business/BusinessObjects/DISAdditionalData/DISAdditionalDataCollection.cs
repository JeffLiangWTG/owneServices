using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISAdditionalDataCollection : NonPersistentBusinessObjectCollection<DISAdditionalData>
	{
		public DISAdditionalDataCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DISAdditionalData(Factory);
		}
	}
}
