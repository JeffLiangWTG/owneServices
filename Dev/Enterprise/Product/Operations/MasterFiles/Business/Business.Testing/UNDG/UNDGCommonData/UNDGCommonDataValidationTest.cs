using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGCommonDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestLanguage()
		{
			UNDGCommonData data = Factory.New<UNDGCommonData>();
			data.DC_Descriptor = "AAA";
			data.DC_Type = UNDGCommonDataLookups.TypeConstants.SpecialProvisions;
			data.DC_Language = Core.Constants.Languages.Gujarati;
			data.RunPreSaveValidation();
			AssertNoErrors(data);

			data.DC_Language = Core.Constants.Languages.English;
			data.RunPreSaveValidation();
			AssertHasErrors(data.DC_LanguageInfo);

			Factory.Save();
			data.DC_Language = Core.Constants.Languages.Czech;
			data.DC_Language = Core.Constants.Languages.English;
			data.RunPreSaveValidation();
			AssertNoErrors(data);
		}
	}
}
