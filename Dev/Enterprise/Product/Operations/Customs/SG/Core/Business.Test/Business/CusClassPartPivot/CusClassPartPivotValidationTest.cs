using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	internal class CusClassPartPivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCI_CC()
		{
			CusClassPartPivot.CI_CC = ZGuid.Empty;
			AssertEquals(true, CusClassPartPivot.HasNotifications());
			CusClassPartPivot.CI_CC = ZGuid.Invalid;
			AssertEquals(true, CusClassPartPivot.HasNotifications());
			var classification = Factory.New<Classification>();
			classification.CC_TariffNum = "6789";
			CusClassPartPivot.CI_CC = classification.PK;
			AssertEquals(false, CusClassPartPivot.HasNotifications());
		}

		public void TestCheckCI_TariffNumWarnsIncorrectTariff()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PartNum";
			var pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_OP = part.PK;
			pivot1.CI_RN_NKCountry = "SG";
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "6789";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			pivot1.CI_TariffNum = "6789";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "1234.56.78";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			pivot1.CI_TariffNum = "1234.56.78";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
			pivot1.CI_TariffNum = "";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, TariffValidator.TariffDoesNotExist.EnglishText);
		}

		CusClassPartPivot CusClassPartPivot
		{
			get
			{
				return cusClassPartPivot ?? (cusClassPartPivot = Factory.New<CusClassPartPivot>());
			}
		}

		CusClassPartPivot cusClassPartPivot;
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
