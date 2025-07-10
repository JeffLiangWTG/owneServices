using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	class StatementMessageLineCollection : NonPersistentBusinessObjectCollection<StatementMessageLine>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotSupportedException();
		}
	}
}
