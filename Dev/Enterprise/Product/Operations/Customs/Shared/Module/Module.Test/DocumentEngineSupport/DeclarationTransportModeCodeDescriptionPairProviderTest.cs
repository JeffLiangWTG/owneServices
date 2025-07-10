using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	sealed class DeclarationTransportModeCodeDescriptionPairProviderTest : DeclarationLookupCodeDescriptionPairProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new DeclarationTransportModeCodeDescriptionPairProvider();

		protected override CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups) => lookups.TransportTypeList;
	}
}
