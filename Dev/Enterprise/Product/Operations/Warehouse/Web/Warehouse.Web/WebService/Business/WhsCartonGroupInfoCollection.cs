using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public sealed class WhsCartonGroupInfoCollection : DataObjectInfoCollection<WhsCartonGroupInfo>
	{
		#region Constructors

		public WhsCartonGroupInfoCollection()
		{
		}

		public WhsCartonGroupInfoCollection(IEnumerable<WhsCartonGroup> whsCartonGroups)
		{
			foreach (var cartonGroup in whsCartonGroups)
			{
				Add(new WhsCartonGroupInfo(cartonGroup));
			}
		}

		#endregion
	}
}
