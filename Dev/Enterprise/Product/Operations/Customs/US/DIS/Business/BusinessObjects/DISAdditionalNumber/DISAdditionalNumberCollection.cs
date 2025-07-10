using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.DIS.Business
{
	public class DISAdditionalNumberCollection : NonPersistentBusinessObjectCollection<DISAdditionalNumber>
	{
		public DISAdditionalNumberCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DISAdditionalNumber(Factory);
		}
	}
}
