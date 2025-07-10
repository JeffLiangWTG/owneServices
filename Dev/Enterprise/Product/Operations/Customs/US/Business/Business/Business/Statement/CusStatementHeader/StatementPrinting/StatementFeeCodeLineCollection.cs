using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class StatementFeeCodeLineCollection : NonPersistentBusinessObjectCollection<StatementFeeCodeLine>
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
