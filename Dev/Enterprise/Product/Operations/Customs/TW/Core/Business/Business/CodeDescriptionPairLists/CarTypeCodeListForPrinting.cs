using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CarTypeCodeListForPrinting : CodeDescriptionPairList
	{
		protected CarTypeCodeListForPrinting()
		{
			AddPair(Codes.A1, Descriptions.A1);
			AddPair(Codes.A2, Descriptions.A2);
			AddPair(Codes.B1, Descriptions.B1);
			AddPair(Codes.B2, Descriptions.B2);
			AddPair(Codes.C1, Descriptions.C1);
			AddPair(Codes.C2, Descriptions.C2);
			AddPair(Codes.D1, Descriptions.D1);
			AddPair(Codes.D2, Descriptions.D2);
			AddPair(Codes.E1, Descriptions.E1);
			AddPair(Codes.F1, Descriptions.F1);
			AddPair(Codes.F2, Descriptions.F2);
			AddPair(Codes.G1, Descriptions.G1);
			AddPair(Codes.G2, Descriptions.G2);
			AddPair(Codes.H1, Descriptions.H1);
			AddPair(Codes.J1, Descriptions.J1);
			AddPair(Codes.K1, Descriptions.K1);
		}

		public static CarTypeCodeListForPrinting Instance => instance ??= new CarTypeCodeListForPrinting();
		[ThreadStatic]
		static CarTypeCodeListForPrinting instance;

		public abstract class Codes
		{
			public const string A1 = "A1";
			public const string A2 = "A2";
			public const string B1 = "B1";
			public const string B2 = "B2";
			public const string C1 = "C1";
			public const string C2 = "C2";
			public const string D1 = "D1";
			public const string D2 = "D2";
			public const string E1 = "E1";
			public const string F1 = "F1";
			public const string F2 = "F2";
			public const string G1 = "G1";
			public const string G2 = "G2";
			public const string H1 = "H1";
			public const string J1 = "J1";
			public const string K1 = "K1";
		}

		public abstract class Descriptions
		{
			public static MultilingualString A1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|A1", "BUS"); } }
			public static MultilingualString A2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|A2", "VAN (PASSENGER CAR)"); } }
			public static MultilingualString B1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|B1", "TRUCK"); } }
			public static MultilingualString B2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|B2", "PICKUP"); } }
			public static MultilingualString C1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|C1", "LARGE WAGON"); } }
			public static MultilingualString C2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|C2", "SMALL WAGON"); } }
			public static MultilingualString D1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|D1", "LOANER BUS"); } }
			public static MultilingualString D2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|D2", "LOANER VAN"); } }
			public static MultilingualString E1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|E1", "CHASSIS CAR"); } }
			public static MultilingualString F1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|F1", "TRACTOR"); } }
			public static MultilingualString F2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|F2", "TRUCK TRACTOR"); } }
			public static MultilingualString G1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|G1", "SMALL SPECIAL VEHICLE"); } }
			public static MultilingualString G2 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|G2", "LARGE SPECIAL VEHICLE"); } }
			public static MultilingualString H1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|H1", "MOTORBIKE"); } }
			public static MultilingualString J1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|J1", "LIGHT/ HEAVY TRAILER"); } }
			public static MultilingualString K1 { get { return ResString.GetMultilingualString("CarTypeCodeListForPrinting|K1", "ELECTROMOTIVE BICYCLE"); } }
		}
	}
}
