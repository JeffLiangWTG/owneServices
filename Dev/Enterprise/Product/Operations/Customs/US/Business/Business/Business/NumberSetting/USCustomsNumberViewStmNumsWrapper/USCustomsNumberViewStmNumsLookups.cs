using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCustomsNumberViewStmNumsLookups : CustomsNumberViewStmNumsLookups
	{
		public USCustomsNumberViewStmNumsLookups(CustomsNumberViewStmNums parent)
			: base(parent)
		{ }

		public override CodeDescriptionPairList TypeList => Factory.GetCachedValue("WithoutINBTillRelease", () =>
		{
			var result = new NumberRangeTypeList();
			result.RemoveCode(NumberRangeTypeList.Codes.InBond);
			return result;
		});
	}
}
