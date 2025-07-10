using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class ClassificationValidationTest : TestCaseWithFactory
	{
		public void TestCheckCC_TariffNumWarnsIncorrectTariff()
		{
			var classification = Factory.New<Classification>();
			classification.CC_TariffNum = "6789";
			classification.Validation.ValidateCC_TariffNum();
			AssertHasMessageError(classification.CC_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			classification.CC_TariffNum = "12345678";
			classification.Validation.ValidateCC_TariffNum();
			AssertNoMessageError(classification.CC_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			classification.CC_TariffNum = "";
			classification.Validation.ValidateCC_TariffNum();
			AssertNoMessageError(classification.CC_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(tariffType, "12345678");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper helper;
	}
}
