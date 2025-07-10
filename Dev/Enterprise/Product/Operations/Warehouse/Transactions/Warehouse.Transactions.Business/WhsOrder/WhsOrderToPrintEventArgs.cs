namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderToPrintEventArgs : WhsDocketToPrintEventArgs
	{
		#region Constructor

		public WhsOrderToPrintEventArgs(WhsDocketLabelControl orderDocument, Core.Constants.DataContext dataContext)
			: base(orderDocument, dataContext)
		{
		}

		public WhsOrderToPrintEventArgs(WhsDocketsLabelControl docketsLabelControl, Core.Constants.DataContext dataContext)
			: base(dataContext)
		{
			this.DocketsLabelControl = docketsLabelControl;
			ContinueToPrint = DocketsLabelControl.Lines.TotalNumberOfLabels > 0;
		}

		#endregion

		public readonly WhsDocketsLabelControl DocketsLabelControl;
	}
}
