using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module
{
	public class DeclarationMessageSubTypeCodeDescriptionPairProvider : DeclarationFilterLookupCodeDescriptionPairProvider, Integration.Customs.Shared.IDeclarationMessageSubTypeCodeDescriptionPairProvider
	{
		public override ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return FilterBizo.Lookups.MessageSubTypeList();
		}
	}
}
