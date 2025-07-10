namespace Enterprise.Customs.ZA.Business
{
	public class OperatorTransactionSelectionValidation : AutoOperatorTransactionSelectionValidation
	{
		public OperatorTransactionSelectionValidation(AutoOperatorTransactionSelection parent) : base(parent)
		{
		}

		protected override void CheckTransactionDateIsValidZDateTimeRange()
		{
		}

		protected override void CheckIntoBondDateIsValidZDateTimeRange()
		{
		}
	}
}
