using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	#region SuppressResourceStringsCheckRegion

	public sealed class EntryChargeTypeList : Registry.Business.Customs.EntryChargeTypeList
	{
		public static class Codes
		{
			public const string A10 = "A10";
			public const string A19 = "A19";
			public const string A20 = "A20";
			public const string A30 = "A30";
			public const string A40 = "A40";
			public const string A50 = "A50";
			public const string B10 = "B10";
			public const string B19 = "B19";
			public const string B29 = "B29";
			public const string B31 = "B31";
			public const string B32 = "B32";
			public const string B40 = "B40";
			public const string B49 = "B49";
			public const string B51 = "B51";
			public const string B52 = "B52";
			public const string B59 = "B59";
			public const string B60 = "B60";
			public const string B69 = "B69";
			public const string B79 = "B79";
			public const string B89 = "B89";
			public const string C10 = "C10";
			public const string C20 = "C20";
			public const string C21 = "C21";
			public const string C22 = "C22";
			public const string C23 = "C23";
			public const string C24 = "C24";
			public const string C25 = "C25";
			public const string C31 = "C31";
			public const string C32 = "C32";
			public const string C33 = "C33";
			public const string C34 = "C34";
			public const string D10 = "D10";
			public const string F10 = "F10";
			public const string F11 = "F11";
			public const string F12 = "F12";
			public const string F13 = "F13";
			public const string F14 = "F14";
			public const string F15 = "F15";
			public const string F16 = "F16";
			public const string F20 = "F20";
			public const string F21 = "F21";
			public const string F22 = "F22";
			public const string F23 = "F23";
			public const string F30 = "F30";
			public const string F31 = "F31";
			public const string F40 = "F40";
			public const string F50 = "F50";
			public const string F51 = "F51";
			public const string F52 = "F52";
			public const string F55 = "F55";
			public const string F56 = "F56";
			public const string F57 = "F57";
			public const string F58 = "F58";
			public const string F88 = "F88";
			public const string F99 = "F99";
			public const string X00 = "X00";
		}

		public static class Descriptions
		{
			public const string A10 = "進口稅";
			public const string A19 = "進口稅";
			public const string A20 = "平衡稅";
			public const string A30 = "反傾銷稅";
			public const string A40 = "報復關稅";
			public const string A50 = "額外關稅";
			public const string B10 = "貨物稅";
			public const string B19 = "貨物稅";
			public const string B29 = "進口商港建設費";
			public const string B31 = "菸酒稅";
			public const string B32 = "健康福利捐";
			public const string B40 = "營業稅";
			public const string B49 = "營業稅";
			public const string B51 = "進口推貿費";
			public const string B52 = "出口推貿費";
			public const string B59 = "進口推貿費";
			public const string B60 = "特種貨物及勞務稅";
			public const string B69 = "菸酒稅";
			public const string B79 = "健康福利捐";
			public const string B89 = "特種貨物及勞務稅";
			public const string C10 = "滯報費";
			public const string C20 = "滯納金";
			public const string C21 = "進口稅滯納金";
			public const string C22 = "貨物稅滯納金";
			public const string C23 = "營業稅滯納金";
			public const string C24 = "菸酒稅滯納金";
			public const string C25 = "特種貨物及勞務稅滯納金";
			public const string C31 = "進口稅利息";
			public const string C32 = "貨物稅利息";
			public const string C33 = "營業稅利息";
			public const string C34 = "特種貨物及勞務稅利息";
			public const string D10 = "押金(現金)";
			public const string F10 = "特別驗貨費";
			public const string F11 = "特別監視費";
			public const string F12 = "進口貨櫃加封費";
			public const string F13 = "出口重櫃加封費";
			public const string F14 = "零星加封費";
			public const string F15 = "保稅運貨工具加封費";
			public const string F16 = "押運費";
			public const string F20 = "簽證文件費";
			public const string F21 = "修改處理費";
			public const string F22 = "證照費";
			public const string F23 = "專責報關人員測驗合格證明書";
			public const string F30 = "影印費";
			public const string F31 = "資訊特別服務費";
			public const string F40 = "倉庫貯存費";
			public const string F50 = "鍵輸費";
			public const string F51 = "快速通關處理費";
			public const string F52 = "盤存特別處理費";
			public const string F55 = "保稅倉庫業務費";
			public const string F56 = "進出口貨棧業務費";
			public const string F57 = "貨櫃集散站業務費";
			public const string F58 = "助航服務費";
			public const string F88 = "規費滯納金";
			public const string F99 = "彙總規費";
			public const string X00 = "雜項收入";
		}

		public EntryChargeTypeList()
		{
			Add(Codes.A10, Descriptions.A10, true, "");
			Add(Codes.A19, Descriptions.A19, true, "");
			Add(Codes.A20, Descriptions.A20, true, "");
			Add(Codes.A30, Descriptions.A30, true, "");
			Add(Codes.A40, Descriptions.A40, true, "");
			Add(Codes.A50, Descriptions.A50, true, "");
			Add(Codes.B10, Descriptions.B10, true, "");
			Add(Codes.B19, Descriptions.B19, true, "");
			Add(Codes.B29, Descriptions.B29, true, "");
			Add(Codes.B31, Descriptions.B31, true, "");
			Add(Codes.B32, Descriptions.B32, true, "");
			Add(Codes.B40, Descriptions.B40, true, "");
			Add(Codes.B49, Descriptions.B49, true, "");
			Add(Codes.B51, Descriptions.B51, true, "");
			Add(Codes.B52, Descriptions.B52, true, "");
			Add(Codes.B59, Descriptions.B59, true, "");
			Add(Codes.B60, Descriptions.B60, true, "");
			Add(Codes.B69, Descriptions.B69, true, "");
			Add(Codes.B79, Descriptions.B79, true, "");
			Add(Codes.B89, Descriptions.B89, true, "");
			Add(Codes.C10, Descriptions.C10, true, "");
			Add(Codes.C20, Descriptions.C20, true, "");
			Add(Codes.C21, Descriptions.C21, true, "");
			Add(Codes.C22, Descriptions.C22, true, "");
			Add(Codes.C23, Descriptions.C23, true, "");
			Add(Codes.C24, Descriptions.C24, true, "");
			Add(Codes.C25, Descriptions.C25, true, "");
			Add(Codes.C31, Descriptions.C31, true, "");
			Add(Codes.C32, Descriptions.C32, true, "");
			Add(Codes.C33, Descriptions.C33, true, "");
			Add(Codes.C34, Descriptions.C34, true, "");
			Add(Codes.D10, Descriptions.D10, true, "");
			Add(Codes.F10, Descriptions.F10, true, "");
			Add(Codes.F11, Descriptions.F11, true, "");
			Add(Codes.F12, Descriptions.F12, true, "");
			Add(Codes.F13, Descriptions.F13, true, "");
			Add(Codes.F14, Descriptions.F14, true, "");
			Add(Codes.F15, Descriptions.F15, true, "");
			Add(Codes.F16, Descriptions.F16, true, "");
			Add(Codes.F20, Descriptions.F20, true, "");
			Add(Codes.F21, Descriptions.F21, true, "");
			Add(Codes.F22, Descriptions.F22, true, "");
			Add(Codes.F23, Descriptions.F23, true, "");
			Add(Codes.F30, Descriptions.F30, true, "");
			Add(Codes.F31, Descriptions.F31, true, "");
			Add(Codes.F40, Descriptions.F40, true, "");
			Add(Codes.F50, Descriptions.F50, true, "");
			Add(Codes.F51, Descriptions.F51, true, "");
			Add(Codes.F52, Descriptions.F52, true, "");
			Add(Codes.F55, Descriptions.F55, true, "");
			Add(Codes.F56, Descriptions.F56, true, "");
			Add(Codes.F57, Descriptions.F57, true, "");
			Add(Codes.F58, Descriptions.F58, true, "");
			Add(Codes.F88, Descriptions.F88, true, "");
			Add(Codes.F99, Descriptions.F99, true, "");
			Add(Codes.X00, Descriptions.X00, true, "");
		}

		public override string DutyCode => Core.Constants.Customs.CusEntryFeeTypes.DutyAmount;

		public override string TaxCode => ZString.Empty;

		#endregion
	}
}
