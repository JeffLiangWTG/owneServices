
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class RegoNumberLookups : Customs.Business.CusCodeDataLookups
	{
		public RegoNumberLookups(RegoNumber regoNumber)
			: base(regoNumber)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<RegoNumberCodeList>(); }
		}
	}
}
