using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class OperatorTransactionSelectionCollection : NonPersistentBusinessObjectCollection<OperatorTransactionSelection>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject() => new OperatorTransactionSelection();

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;
	}
}
