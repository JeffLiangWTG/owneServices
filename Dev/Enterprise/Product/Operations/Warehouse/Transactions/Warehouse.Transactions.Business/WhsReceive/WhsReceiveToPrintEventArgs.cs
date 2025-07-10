namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveToPrintEventArgs : WhsDocketToPrintEventArgs
	{
		#region Constructors

		public WhsReceiveToPrintEventArgs(WhsDocketLabelControl receiveDocument, Core.Constants.DataContext dataContext)
			: base(receiveDocument, dataContext)
		{
		}

		#endregion
	}
}
