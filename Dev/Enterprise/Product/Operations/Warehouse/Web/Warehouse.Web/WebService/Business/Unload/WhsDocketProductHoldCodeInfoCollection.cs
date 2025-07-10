using System;
using System.Linq;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsDocketProductHoldCodeInfoCollection : DataObjectInfoCollection<WhsDocketProductHoldCodeInfo>
	{
		public WhsDocketProductHoldCodeInfoCollection()
		{
		}

		public WhsDocketProductHoldCodeInfoCollection(WhsDocketLineCollection lines)
		{
			var productGroupedLines = lines.GroupBy(l => l.ProductCode);
			foreach (var lineGroup in productGroupedLines)
			{
				var holdCodeInfo = new WhsDocketProductHoldCodeInfo(lineGroup.Key);
				foreach (var line in lineGroup)
				{
					holdCodeInfo.HoldCodes.Add(line.WE_WHC_NKCurrentInventoryHeldCode);
				}
				Add(holdCodeInfo);
			}
		}
	}
}
