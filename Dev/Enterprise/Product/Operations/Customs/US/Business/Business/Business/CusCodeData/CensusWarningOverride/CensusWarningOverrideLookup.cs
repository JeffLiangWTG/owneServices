
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CensusWarningOverrideLookup : Customs.Business.CusCodeDataLookups
	{
		public CensusWarningOverrideLookup(CensusWarningOverride parent)
			: base(parent)
		{
		}

		public new CensusWarningOverride Parent
		{
			get { return (CensusWarningOverride)base.Parent; }
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<CensusWarningCodeList>(); }
		}

		public CodeDescriptionPairList CensusOverrideList
		{
			get
			{
				return Factory.GetCachedValue("CensusOverrideCodeList for " + Parent.CY_Code, delegate
					{
						return CensusOverrideCodeList.GetListFor(Parent.CY_Code);
					}
				);
			}
		}
	}
}
