using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using WTG.StaticAnalysis.Annotation;
using Res = Enterprise.Warehouse.Transit.DataTransfer.Universal.Res;

namespace Enterprise.Warehouse.Transit.DataTransfer
{
	[CodeAlive("This class will be used in future workitems")]
	public class ReceiveASNTransitLogHelper : TransitLogTableHelper<WhsItemReceiveASN, TransitLogColumnIDs.ASNColumn>
	{
		protected override ZString GetValue(WhsItemReceiveASN asn, TransitLogColumnIDs.ASNColumn column)
		{
			switch (column)
			{
				case TransitLogColumnIDs.ASNColumn.ASN:
					return asn?.FormattedReference ?? "";
				default:
					return "";
			}
		}

		protected override ZString GetHeader(TransitLogColumnIDs.ASNColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.ASNColumn.ASN:
					return Res.GetString("840ae08c-0195-42ed-905e-81d1e0ec54b3", "ASN");
				default:
					return "";
			}
		}
	}
}
