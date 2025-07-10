using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	[CodeAlive("This class will be used in future workitems")]
	public class DispatchConsignmentTransitLogHelper : TransitLogTableHelper<WhsItemDispatchConsignment, TransitLogColumnIDs.DCNColumn>
	{
		protected override ZString GetValue(WhsItemDispatchConsignment dcn, TransitLogColumnIDs.DCNColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.DCNColumn.DCN:
					return dcn?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.DCNColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.DCNColumn.DCN:
					return Res.GetString("912c5da6-1cdb-40c9-8ae5-fc1ff0fa4af8", "DCN");
				default:
					return "";
			}
		}
	}
}
