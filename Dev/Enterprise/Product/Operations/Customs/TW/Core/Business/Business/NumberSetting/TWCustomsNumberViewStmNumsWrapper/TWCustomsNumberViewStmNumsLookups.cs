using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business
{
	public class TWCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
	{
		public TWCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent)
			: base(parent)
		{ }

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue("TWCustomsNumberViewStmNumsLookups|TypeList", () =>
		 {
			 var list = new CodeDescriptionPairList();
			 list.AddPair(BaseEntryNumberGenerator.CustomsStmNumsType);
			 return list;
		 });
	}
}
