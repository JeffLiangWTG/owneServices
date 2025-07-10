using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	sealed class DeclarationMessageTypeCodeDescriptionPairProviderTest : DeclarationLookupCodeDescriptionPairProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new DeclarationMessageTypeCodeDescriptionPairProvider();
		}

		protected override CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups)
		{
			return lookups.MessageTypeList;
		}
	}
}
