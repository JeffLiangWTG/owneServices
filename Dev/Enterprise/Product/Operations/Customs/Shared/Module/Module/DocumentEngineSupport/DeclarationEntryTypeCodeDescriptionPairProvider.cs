using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationEntryTypeCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationEntryTypeCodeDescriptionPairProvider
	{
		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.EntryTypeList;
		}
	}
}
