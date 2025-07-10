using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public sealed class WhsInventoryHeldCodeInfoCollection : DataObjectInfoCollection<WhsInventoryHeldCodeInfo>
	{
		#region Constructors

		public WhsInventoryHeldCodeInfoCollection()
		{
		}

		public WhsInventoryHeldCodeInfoCollection(IEnumerable<WhsInventoryHeldCode> whsInventoryHeldCodes)
		{
			Add(new WhsInventoryHeldCodeInfo());
			foreach (var heldCode in whsInventoryHeldCodes)
			{
				Add(new WhsInventoryHeldCodeInfo(heldCode));
			}
		}

		#endregion
	}
}
