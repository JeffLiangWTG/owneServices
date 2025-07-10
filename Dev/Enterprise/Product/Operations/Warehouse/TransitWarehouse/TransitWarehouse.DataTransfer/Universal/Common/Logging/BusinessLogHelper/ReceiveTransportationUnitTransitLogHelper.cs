using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	[CodeAlive("This class will be used in future workitems")]
	public class ReceiveTransportationUnitTransitLogHelper : TransitLogTableHelper<WhsItemReceiveTransportationUnit, TransitLogColumnIDs.RTUColumn>
	{
		protected override ZString GetValue(WhsItemReceiveTransportationUnit rtu, TransitLogColumnIDs.RTUColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.RTUColumn.RTU:
					return rtu?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.RTUColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.RTUColumn.RTU:
					return Res.GetString("0a01a042-cf2e-4300-ac81-e913cd879fec", "RTU");
				default:
					return "";
			}
		}
	}
}
