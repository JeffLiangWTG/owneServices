using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
	[ThreadSafe]
	public static class TransitFormMenuItems
	{
		public static ZGuid CIN750InFromRCN => new ZGuid("C1617A2C-43C2-4A12-BE19-7D752D79A44C");
		public static ZGuid CIN750OutFromDCN => new ZGuid("5051605d-4cd0-42ec-8038-2d0b5791a111");

		public static ZGuid CIN750CorFromRCN => new ZGuid("C104C8AC-A700-4DB0-947D-BC2F373D9A98");

		public static ZGuid CIN750ConsFromDCN => new ZGuid("5bf8591c-f374-48e1-976a-8cb0d4152368");

		public static ZGuid CIN750DeconsFromDCN => new ZGuid("d6106852-c6ab-4a1f-be89-396555492582");
	}
}
