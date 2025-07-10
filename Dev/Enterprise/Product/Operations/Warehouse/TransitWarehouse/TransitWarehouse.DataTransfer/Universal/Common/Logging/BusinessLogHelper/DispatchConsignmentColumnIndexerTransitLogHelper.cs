using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Transit.Business;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	public class DispatchConsignmentColumnIndexerTransitLogHelper : TransitLogTableHelper<IColumnIndexer, TransitLogColumnIDs.DCNIndexerColumn>
	{
		protected readonly UniversalObjectFactory Factory;
		public DispatchConsignmentColumnIndexerTransitLogHelper(UniversalObjectFactory factory)
		{
			Factory = factory;
		}

		protected override ZString GetValue(IColumnIndexer dcn, TransitLogColumnIDs.DCNIndexerColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.DCNIndexerColumn.DCN:
					return DCNColumnIndexerHelper.GetFormattedReference(Factory, dcn);
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.DCNIndexerColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.DCNIndexerColumn.DCN:
					return Res.GetString("859f9fe5-4419-480c-9036-664d8a8694f5", "DCN");
				default:
					return "";
			}
		}
	}
}
