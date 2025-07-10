using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.ZA.Business
{
	public class NatureList : ShipmentTypeList
	{
		public new class Codes : ShipmentTypeList.Codes
		{
			public const string FreightRemainingOnBoard = "FRB";
		}

		public new class Descriptions : ShipmentTypeList.Descriptions
		{
			public const string FreightRemainingOnBoard = "Freight Remaining on Board (57)";
		}

		public NatureList()
		{
			AddPair(Codes.FreightRemainingOnBoard, ResString.GetMultilingualString("48634974-040E-4915-AF80-BE3FBA89FD53", Descriptions.FreightRemainingOnBoard));
		}
	}
}
