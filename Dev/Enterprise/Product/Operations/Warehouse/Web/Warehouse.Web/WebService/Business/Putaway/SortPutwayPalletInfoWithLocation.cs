using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService.Business.Putaway
{
	public class SortPutwayPalletInfoWithLocation : SortByStandardPickingOrPutawayFields<(PutawayPalletInfo PalletInfo, WhsLocation Location)>
	{
		protected override IEnumerable<IComparer<(PutawayPalletInfo PalletInfo, WhsLocation Location)>> GetElementaryComparers()
		{
			return GetComparersForSortByLocationAndThenByProduct(l => l.Location, l => null, false)
			.Concat(new IComparer<(PutawayPalletInfo, WhsLocation)>[]
				{
					new ValueComparer(l => (ZString)l.PalletInfo.PalletID),
				});
		}
	}
}
