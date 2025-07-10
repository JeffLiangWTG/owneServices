using System;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CertaintyLikertItemHelper
	{
		public ICodeDescription ToItem(ZByte percentage)
		{
			var index = percentage / 20;
			index = Math.Min(index, 4);
			index = Math.Max(index, 0);

			return CertaintyLikertItemList[index];
		}

		public ZByte ToPercentage(ZString itemCode)
		{
			if (itemCode.IsEmpty)
			{
				return 0;
			}
			else
			{
				var index = CertaintyLikertItemList.IndexOfCode(itemCode);
				if (index < 0)
				{ return 0; }
				else
				{
					return (byte)(index * 25);
				}
			}
		}

		CertaintyLikertItemList CertaintyLikertItemList
		{
			get { return certaintyLikertItemList ?? (certaintyLikertItemList = new CertaintyLikertItemList()); }
		}
		CertaintyLikertItemList certaintyLikertItemList;
	}
}
