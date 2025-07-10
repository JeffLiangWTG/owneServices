using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	[CodeAlive("This class will be used in future workitems")]
	public class ReceiveConsignmentTransitLogHelper : TransitLogTableHelper<WhsItemReceiveConsignment, TransitLogColumnIDs.RCNColumn>
	{
		protected override ZString GetValue(WhsItemReceiveConsignment rcn, TransitLogColumnIDs.RCNColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.RCNColumn.RCN:
					return rcn?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.RCNColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.RCNColumn.RCN:
					return Res.GetString("2b9e5284-7b29-4cca-95de-37a6c2a14d84", "RCN");
				default:
					return "";
			}
		}
	}
}
