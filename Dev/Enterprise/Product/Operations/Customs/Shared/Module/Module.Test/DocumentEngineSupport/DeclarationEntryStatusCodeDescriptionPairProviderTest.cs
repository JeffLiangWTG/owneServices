using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	sealed class DeclarationEntryStatusCodeDescriptionPairProviderTest : DeclarationLookupCodeDescriptionPairProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new DeclarationEntryStatusCodeDescriptionPairProvider();

		protected override CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups) => lookups.EntryStatusList();
	}
}
