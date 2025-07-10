namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsReceiveToPrintEventArgsTest : WhsDocketToPrintEventArgsTest
	{
		#region Implementation  

		protected override WhsDocket GetNewDocket()
		{
			return Factory.New<WhsReceive>();
		}

		protected override WhsDocketToPrintEventArgs GetNewDocketToPrintEventArgs(WhsDocketLabelControl docketDocument, Core.Constants.DataContext dataContext)
		{
			return new WhsReceiveToPrintEventArgs(docketDocument, dataContext);
		}

		protected override Core.Constants.DataContext GetDataContextToTest()
		{
			return Enterprise.Core.Constants.DataContext.WhsPalletIDLabels;
		}

		#endregion
	}
}
