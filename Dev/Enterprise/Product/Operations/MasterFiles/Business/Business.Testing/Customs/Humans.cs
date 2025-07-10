using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class Humans : NonPersistentBusinessObjectCollection<Human>
	{
		public Humans(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Human(Factory);
		}
	}
}
