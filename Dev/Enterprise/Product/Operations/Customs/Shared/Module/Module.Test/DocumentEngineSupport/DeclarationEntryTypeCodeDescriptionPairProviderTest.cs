using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using ICodeDescriptionPairListProvider = Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider;

namespace Enterprise.Customs.Module.Testing
{
	abstract class DeclarationEntryTypeCodeDescriptionPairProviderTest : DeclarationLookupCodeDescriptionPairProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new DeclarationMessageSubTypeCodeDescriptionPairProvider();

		protected override CodeDescriptionPairList GetSpecificLookupFromJobDecLookup(JobDeclarationFilterLookups lookups) => lookups.MessageSubTypeList();

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(CountryCode);
		}

		protected abstract string CountryCode { get; }
	}
}
