namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderLineLookups : WhsPickableDocketLineLookups
	{
		public WhsOrderLineLookups(WhsOrderLine parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsOrderLine Parent
		{
			get { return (WhsOrderLine)base.Parent; }
		}

		#endregion

		#region ExcludeProductsNotForResale

		protected override bool ExcludeProductsNotForResale
		{
			get { return true; }
		}

		#endregion
	}
}
