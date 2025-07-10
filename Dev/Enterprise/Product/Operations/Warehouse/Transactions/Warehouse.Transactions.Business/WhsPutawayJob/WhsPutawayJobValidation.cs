namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayJobValidation : AutoWhsPutawayJobValidation
	{
		public WhsPutawayJobValidation(AutoWhsPutawayJob parent)
			: base(parent)
		{
		}

		protected new WhsPutawayJob Parent
		{
			get { return (WhsPutawayJob)base.Parent; }
		}
	}
}
