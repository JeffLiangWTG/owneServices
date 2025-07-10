
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class RestrictedCodeLookups : Customs.Business.CusCodeDataLookups
	{
		public RestrictedCodeLookups(RestrictedCode parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList
		{
			get { return Factory.GetCachedValue<RestrictedCodeTypeList>(); }
		}

		public CodeDescriptionPairList RestrictedCodeList
		{
			get
			{
				switch (((RestrictedCode)Parent).CY_Code)
				{
					case RestrictedCodeTypeList.Codes.RestrictedSPI:
						return SPICompleteList.GetCachedList(Factory);
					case RestrictedCodeTypeList.Codes.RestrictedEntryType:
						return Factory.GetCachedValue<EntryTypeList>();
					default:
						return new CodeDescriptionPairList();
				}
			}
		}
	}
}
