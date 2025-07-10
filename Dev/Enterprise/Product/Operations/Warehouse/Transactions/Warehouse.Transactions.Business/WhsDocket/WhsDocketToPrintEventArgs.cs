using System;

namespace Enterprise.Warehouse.Transactions.Business
{
	public abstract class WhsDocketToPrintEventArgs : EventArgs
	{
		#region Constructor

		protected WhsDocketToPrintEventArgs(WhsDocketLabelControl docketDocument, Core.Constants.DataContext dataContext)
		{
			this.DocketDocument = docketDocument;
			this.DataContext = dataContext;
			ContinueToPrint = true;
		}

		protected WhsDocketToPrintEventArgs(Core.Constants.DataContext dataContext)
		{
			this.DataContext = dataContext;
		}

		#endregion

		#region Implementation

		public readonly WhsDocketLabelControl DocketDocument;
		public readonly Core.Constants.DataContext DataContext;
		public bool ContinueToPrint;

		#endregion
	}
}
