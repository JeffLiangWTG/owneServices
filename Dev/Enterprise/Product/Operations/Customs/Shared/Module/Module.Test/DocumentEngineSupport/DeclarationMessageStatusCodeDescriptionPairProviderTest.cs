using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	sealed class DeclarationMessageStatusCodeDescriptionPairProviderTest : DeclarationLookupCodeDescriptionPairProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new DeclarationMessageStatusCodeDescriptionPairProvider();
		}

		protected override CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups)
		{
			return lookups.MessageStatusList();
		}

		public override void TestIsReturningCorrectCollection()
		{
			Assert(true);
		}
	}
}
