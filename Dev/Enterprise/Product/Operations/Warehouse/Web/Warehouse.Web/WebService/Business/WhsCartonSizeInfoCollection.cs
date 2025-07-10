using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public sealed class WhsCartonSizeInfoCollection : DataObjectInfoCollection<WhsCartonSizeInfo>
	{
		#region Constructors

		public WhsCartonSizeInfoCollection()
		{
		}

		public WhsCartonSizeInfoCollection(IEnumerable<WhsCartonSize> whsCartonSizes)
		{
			foreach (var size in whsCartonSizes)
			{
				Add(new WhsCartonSizeInfo(size));
			}
		}

		#endregion
	}
}
