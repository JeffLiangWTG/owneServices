namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsBusinessObjectLookupsTestCase : Environment.Business.Testing.WhsBusinessObjectLookupsTestCase
	{
		#region Properties

		WhsTestHelperFunctions helper;

		protected new WhsTestHelperFunctions Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new WhsTestHelperFunctions(Factory);
				}
				return helper;
			}
		}
		#endregion
	}
}
