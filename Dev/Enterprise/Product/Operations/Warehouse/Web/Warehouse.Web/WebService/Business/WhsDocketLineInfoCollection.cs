using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public sealed class WhsDocketLineInfoCollection : DataObjectInfoCollection<WhsDocketLineInfo>
	{
		#region Constructors

		public WhsDocketLineInfoCollection()
		{
		}

		#region Constructor for TransferLines

		public WhsDocketLineInfoCollection(IEnumerable<WhsTransferLine> whsTransferLines)
		{
			foreach (var line in whsTransferLines)
			{
				Add(new WhsDocketLineInfo(line));
			}
		}

		#endregion

		public WhsDocketLineInfoCollection(IEnumerable<WhsDocketLine> whsDocketLines)
		{
			foreach (var line in whsDocketLines)
			{
				Add(new WhsDocketLineInfo(line));
			}
		}

		#endregion
	}
}
