using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class InvoiceLineCustomsChargeTypeList : Customs.Business.CustomsChargeTypeList
	{
		public new class Codes : Common.CustomsChargeTypeList.Codes
		{
			public const string IntellectualValue = "INT";
		}

		public new class Descriptions : Common.CustomsChargeTypeList.Descriptions
		{
			public static MultilingualString IntellectualValue = ResString.GetMultilingualString("89E47594-6F64-4002-A20A-D8F7086DD8B2", "Intellectual Value");
		}
	}
}
