using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class DispatchLoadListTransitLogHelper : TransitLogTableHelper<WhsItemDispatchLoadList, TransitLogColumnIDs.LoadListColumn>
	{
		protected override ZString GetValue(WhsItemDispatchLoadList loadList, TransitLogColumnIDs.LoadListColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.LoadListColumn.LoadList:
					return loadList?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.LoadListColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.LoadListColumn.LoadList:
					return Res.GetString("2c3d516d-af10-4eef-a9d0-cff1806f552c", "Load List");
				default:
					return "";
			}
		}
	}
}
