namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsBusinessObjectCollectionTestCase : Environment.Business.Testing.WhsBusinessObjectCollectionTestCase
	{
		protected new WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;
	}
}
