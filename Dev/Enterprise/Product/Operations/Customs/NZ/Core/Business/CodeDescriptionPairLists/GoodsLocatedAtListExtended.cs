using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business
{
	public class GoodsLocatedAtListForSeaImport : GoodsLocatedAtList
	{
		public new sealed class Codes : GoodsLocatedAtList.Codes
		{
			Codes() { }

			public const string DIS = "DIS";
			public const string DES = "DES";
		}

		public new sealed class Descriptions : GoodsLocatedAtList.Descriptions
		{
			Descriptions() { }

			public static MultilingualString DIS  { get { return ResString.GetMultilingualString("GoodsLocatedAtListForSeaImport|DIS",  "Place of Discharge"); } }
			public static MultilingualString DES  { get { return ResString.GetMultilingualString("GoodsLocatedAtListForSeaImport|DES",  "Final Destination"); } }
		}

		public GoodsLocatedAtListForSeaImport()
			: base()
		{
			AddPair(Codes.DIS, Descriptions.DIS);
			AddPair(Codes.DES, Descriptions.DES);
		}
	}

	public class GoodsLocatedAtListForSeaExport : GoodsLocatedAtList
	{
		public new sealed class Codes : GoodsLocatedAtList.Codes
		{
			Codes() { }

			public const string PC = "PC";
		}

		public new sealed class Descriptions : GoodsLocatedAtList.Descriptions
		{
			Descriptions() { }

			public static MultilingualString PC { get { return ResString.GetMultilingualString("GoodsLocatedAtListForSeaExport|PC", "Port Company"); } }
		}

		public GoodsLocatedAtListForSeaExport()
			: base()
		{
			AddPair(Codes.PC, Descriptions.PC);
		}
	}
}
