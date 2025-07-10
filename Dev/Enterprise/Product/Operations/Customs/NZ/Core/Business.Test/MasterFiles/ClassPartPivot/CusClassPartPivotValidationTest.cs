using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;

namespace Enterprise.Customs.NZ.Business.MasterFiles.Testing
{
	class CusClassPartPivotValidationTest : Customs.Business.Testing.CusClassPartPivotValidationTest
	{
		public void TestValidateCI_ConcessionCode()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				UniversalTariffHelperTest.SetupTariffData(Factory);
				var pivot = Factory.New<CusClassPartPivot>();
				pivot.CI_TariffNum = "123456789";

				pivot.CI_ConcessionCode = "INVALID";
				AssertHasWarningContaining(pivot.CI_ConcessionCodeInfo, "Concession Code [INVALID] not recognized");
				pivot.CI_ConcessionCode = "100001A";
				AssertNoWarnings(pivot.CI_ConcessionCodeInfo);
			}
		}

		public void TestCI_PartsOfClassification()
		{
			NZCClassification classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "0000.00.00.01A";
			classification.U0_DateActiveFrom = ZDate.Today.AddDays(-1);
			classification.U0_DateActiveTo = ZDate.Today.AddDays(1);
			classification.U0_IsManual = false;
			Factory.Save();

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = "123";
			pivot.CI_PartsOfClassification = "0000.00.00.01A";
			pivot.CI_TariffNum = ZString.Empty;
			AssertHasMessageError(pivot.CI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);
			pivot.CI_CC = ZGuid.NewZGuid();
			AssertNoMessageError(pivot.CI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);
			AssertNoNotifications(pivot.CI_PartsOfClassificationInfo);
		}

		public void TestCheckCI_TariffNumCanBeEmpty()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_TariffNum = ZString.Empty;
			AssertHasError(pivot.CI_TariffNumInfo, Enterprise.Customs.Business.BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			pivot.CI_CC = ZGuid.NewZGuid();
			AssertNoError(pivot.CI_TariffNumInfo, Enterprise.Customs.Business.BaseCusClassPartPivotValidation.OneOfTariffOrClassificationIsMandatory);
			AssertNoNotifications(pivot.CI_TariffNumInfo);
		}

		public void TestCheckCI_TariffNumWarnsIncorrectTariff()
		{
			var classification = Factory.New<NZCClassification>();
			classification.U0_Tariff = "1234.56.78.90E";
			classification.U0_Description = "Stuff";
			classification.U0_DateActiveFrom = ZDateTime.MinSmallDateTimeValue;
			classification.U0_DateActiveTo = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PartNum";

			var pivot1 = Factory.New<CusClassPartPivot>();
			pivot1.CI_OP = part.PK;
			pivot1.CI_RN_NKCountry = "NZ";

			pivot1.CI_ChildType = Enterprise.Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "1234";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);

			pivot1.CI_ChildType = Enterprise.Customs.Business.ClassificationTypeList.Codes.HTE;
			pivot1.CI_TariffNum = "1234";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertHasMessageError(pivot1.CI_TariffNumInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);

			pivot1.CI_ChildType = Enterprise.Customs.Business.ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "1234.56.78 90E";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);

			pivot1.CI_ChildType = Enterprise.Customs.Business.ClassificationTypeList.Codes.HTE;
			pivot1.CI_TariffNum = "1234.56.78 90E";
			pivot1.Validation.ValidateCI_TariffNum();
			AssertNoMessageError(pivot1.CI_TariffNumInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);
		}
	}
}
