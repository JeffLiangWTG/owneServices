using Enterprise.Customs.Common;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Module.Testing
{
	sealed class AdditionalReferenceNumberTypesPairProviderTest : CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
		}

		public void TestIsReturningCorrectCollection_DifferentCountries()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Iceland))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedArabEmirates))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), CusEntryNumLookups.GetAdditionalReferenceNumberTypes(GlbCompany.CurrentCompany.Country.Code));
			}
		}

		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider() => new AdditionalReferenceNumberTypesCodeDescriptionPairProvider();
	}
}
