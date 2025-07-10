using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class CustomsChargeTypeList : Common.CustomsChargeTypeList
	{
		public new class Descriptions : Common.CustomsChargeTypeList.Descriptions
		{
			public new static MultilingualString ExWorks { get { return ResString.GetMultilingualString("TW|CustomsChargeTypeList|ExWorks", "Ex-Works Additions"); } }
		}
	}
}
