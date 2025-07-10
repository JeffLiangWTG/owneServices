using System;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocumentInventoryEventArgs : EventArgs
	{
		public WhsDocumentInventoryEventArgs(WhsDocumentInventoryOptions options)
		{
			this.options = options;
		}

		#region Options

		public WhsDocumentInventoryOptions Options
		{
			get { return options; }
		}

		readonly WhsDocumentInventoryOptions options;

		#endregion

		#region ContinueToPrint

		public ZBool ContinueToPrint
		{
			get { return continueToPrint; }
			set { continueToPrint = value; }
		}

		ZBool continueToPrint = true;

		#endregion
	}
}
