using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	[CodeAlive("This class will be used in future workitems")]
	public class DispatchTransportationUnitTransitLogHelper : TransitLogTableHelper<WhsItemDispatchTransportationUnit, TransitLogColumnIDs.DTUColumn>
	{
		protected override ZString GetValue(WhsItemDispatchTransportationUnit dtu, TransitLogColumnIDs.DTUColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.DTUColumn.DTU:
					return dtu?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.DTUColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.DTUColumn.DTU:
					return Res.GetString("0eb2c38f-0f87-4ca0-9a64-e4a1a0417155", "DTU");
				default:
					return "";
			}
		}
	}
}
