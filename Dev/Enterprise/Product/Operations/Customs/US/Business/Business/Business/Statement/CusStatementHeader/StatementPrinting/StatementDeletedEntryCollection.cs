using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	class StatementDeletedEntryCollection : NonPersistentBusinessObjectCollection<StatementDeletedEntry>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}

		protected override bool AllowNewCore => false;
	}
}
