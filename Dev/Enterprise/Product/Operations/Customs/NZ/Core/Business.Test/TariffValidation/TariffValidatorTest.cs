using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MasterFiles;

namespace Enterprise.Customs.NZ.Business.TariffValidation.Testing
{
	using System;
	using System.Linq;
	using Enterprise.Customs.NZ.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.Universal.Testing;

	public class TariffValidatorTest : TestCaseWithFactory
	{
		public void TestAdditionalNotInDateStringTurnsUpWhereItShould()
		{
			InvoiceLine.JI_Tariff = TariffOutOfDate.U0_Tariff;
			AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorAdditionalNotInDateString);
			InvoiceLine.JI_Tariff = TariffNotInForceYet.U0_Tariff;
			AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorAdditionalNotInDateString);
			InvoiceLine.JI_Tariff = TariffValid.U0_Tariff;
			AssertNoMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorAdditionalNotInDateString);
		}

		public void TestCheckJI_Tariff()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				InvoiceLine.JI_Tariff = ZString.Empty;
				AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeMissing);
				AssertEquals("Should only be one MessageError when JI_Tariff is empty", 1, InvoiceLine.JI_TariffInfo.GetMessageErrors().Take(2).Count());
				InvoiceLine.JI_Tariff = TariffOutOfDate.U0_Tariff;
				AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeOutOfDate);
				InvoiceLine.JI_Tariff = TariffNotInForceYet.U0_Tariff;
				AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeNotInForceYet);
				InvoiceLine.JI_Tariff = TariffCodeValidStructureButNotOnFile;
				AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeNotOnFile);
				InvoiceLine.JI_Tariff = TariffCodeInvalidStructure;
				AssertHasMessageErrorContaining(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);
				InvoiceLine.JI_Tariff = TariffValid.U0_Tariff;
				AssertNoMessageErrors(InvoiceLine.JI_TariffInfo);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var tariff = UniversalTariffHelperTest.SetupTariffData(Factory);
				tariff.ZZ1_StartDate = new ZDateTime(2005, 1, 1);
				tariff.ZZ1_EndDate = new ZDateTime(2005, 12, 1);

				InvoiceLine.JI_Tariff = ZString.Empty;
				AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeMissing);
				AssertEquals("Should only be one MessageError when JI_Tariff is empty", 1, InvoiceLine.JI_TariffInfo.GetMessageErrors().Take(2).Count());
				InvoiceLine.JI_Tariff = "123";
				AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorInvalidTariffCode);
				InvoiceLine.JI_Tariff = "123456789";
				AssertNoMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorInvalidTariffCode);
			}
		}

		public void TestExciseTariff()
		{
			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			InvoiceLine.JI_Tariff = "99.20.20L";
			AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeIsExciseForImpOrExp);

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			InvoiceLine.JI_Tariff = "99.20.20L";
			AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeIsExciseForImpOrExp);

			InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Excise;
			InvoiceLine.JI_Tariff = "99.20.20L";
			AssertNoMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeIsExciseForImpOrExp);
			AssertNoMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);

			InvoiceLine.JI_Tariff = "9900.20.20L";
			AssertHasMessageError(InvoiceLine.JI_TariffInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);
		}

		public void TestPermitCodeValidationOnCheckJI_Tariff()
		{
			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes02Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			InvoiceLine.JI_Tariff = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes04Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			InvoiceLine.JI_Tariff = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes07Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			InvoiceLine.JI_Tariff = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes10Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			InvoiceLine.JI_Tariff = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			InvoiceLine.JI_Tariff = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			PermitCode permitOnLine = InvoiceLine.PermitCodes.AddNew();
			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes14Digit;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);
			InvoiceLine.PermitCodes.RemoveAndDeleteAll();

			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			PermitCode permitOnDec = InvoiceLine.PermitCodes.AddNew();
			InvoiceLine.JI_Tariff = TariffCodeRequiringPermitCodes14Digit;
			AssertNoWarnings(InvoiceLine.JI_TariffInfo);
			InvoiceLine.PermitCodes.RemoveAndDeleteAll();
			AssertHasWarningContaining(InvoiceLine.JI_TariffInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
		}

		public void TestCheckZN_PartsOfClassification()
		{
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				InvoiceLine.JI_PartsOfClassification = TariffManual.U0_Tariff;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);

				InvoiceLine.JI_PartsOfClassification = ZString.Empty;
				AssertNoNotifications(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = TariffManual.U0_Tariff;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfPleaseEnterATariffCode);

				InvoiceLine.JI_PartsOfClassification = TariffValid.U0_Tariff;
				AssertNoNotifications(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = TariffValid.U0_Tariff;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);

				InvoiceLine.JI_PartsOfClassification = ZString.Empty;
				AssertNoNotifications(InvoiceLine.JI_PartsOfClassificationInfo);

				Declaration.JE_DateOfArrival = new ZDateTime(2001, 12, 12);
				InvoiceLine.JI_Tariff = TariffValid.U0_Tariff;
				InvoiceLine.JI_PartsOfClassification = ZString.Empty;
				AssertNoNotifications(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = TariffManual.U0_Tariff;
				InvoiceLine.JI_PartsOfClassification = TariffValid.U0_Tariff;
				AssertHasMessageErrorContaining(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorTariffCodeNotInForceYet);
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				var rateType = helper.CreateNewOrGetExistingRateType("NZ", "DTY");
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", rateType.PK);
				var preference = helper.CreatePreferenceForCountry("N", "UnQualify", "NZ");
				var tariff1 = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate1 = helper.CreateRate(tariff1, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0", preference.PK, "Manual calculation required");
				var tariff2 = helper.CreateTariff("NZ", tariffType.PK, "987654321", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				var rate2 = helper.CreateRate(tariff2, rateCode.PK, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "0", preference.PK);

				InvoiceLine.JI_Tariff = "";
				InvoiceLine.Declaration.JE_DateOfArrival = ZDateTime.Today;
				InvoiceLine.JI_PartsOfClassification = tariff1.ZZ1_TariffCode;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);

				InvoiceLine.JI_PartsOfClassification = ZString.Empty;
				AssertNoMessageErrors(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfPleaseEnterATariffCode);

				InvoiceLine.JI_PartsOfClassification = tariff2.ZZ1_TariffCode;
				AssertNoMessageErrors(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);

				InvoiceLine.JI_PartsOfClassification = ZString.Empty;
				AssertNoMessageErrors(InvoiceLine.JI_PartsOfClassificationInfo);

				InvoiceLine.JI_Tariff = "";
				InvoiceLine.JI_PartsOfClassificationForTest = "INVALID";
				AssertHasMessageError(InvoiceLine.JI_PartsOfClassificationInfo, TariffValidator.MessageErrorInvalidTariffCode);
			}
		}

		public void TestCheckCC_TariffNum()
		{
			Classification.CC_TariffNum = ZString.Empty;
			AssertHasError(Classification.CC_TariffNumInfo, TariffValidator.MessageErrorTariffCodeMissing);
			AssertEquals("Should only be one Error when CC_TariffNum is empty", 1, Classification.CC_TariffNumInfo.GetErrors().Take(2).Count());
			Classification.CC_TariffNum = TariffOutOfDate.U0_Tariff;
			AssertHasMessageErrorContaining(Classification.CC_TariffNumInfo, TariffValidator.MessageErrorTariffCodeOutOfDate);
			Classification.CC_TariffNum = TariffCodeValidStructureButNotOnFile;
			AssertHasMessageErrorContaining(Classification.CC_TariffNumInfo, TariffValidator.MessageErrorTariffCodeNotOnFile);
			Classification.CC_TariffNum = TariffCodeInvalidStructure;
			AssertHasMessageErrorContaining(Classification.CC_TariffNumInfo, TariffValidator.MessageErrorTariffCodeInvalidLength);
			Classification.CC_TariffNum = TariffValid.U0_Tariff;
			AssertNoMessageErrors(Classification.CC_TariffNumInfo);
		}

		public void TestPermitCodeValidationOnCheckCC_TariffNum()
		{
			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes02Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			Classification.CC_TariffNum = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(Classification.CC_TariffNumInfo);

			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes04Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			Classification.CC_TariffNum = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(Classification.CC_TariffNumInfo);

			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes07Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			Classification.CC_TariffNum = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(Classification.CC_TariffNumInfo);

			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes10Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			Classification.CC_TariffNum = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(Classification.CC_TariffNumInfo);

			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			Classification.CC_TariffNum = TariffCodeRequiringNoPermitCodes;
			AssertNoWarnings(Classification.CC_TariffNumInfo);

			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes14Digit;
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
			PermitCode permitOnLine = Classification.PermitCodes.AddNew();
			Classification.CC_TariffNum = TariffCodeRequiringPermitCodes14Digit;
			AssertNoWarnings(Classification.CC_TariffNumInfo);
			Classification.PermitCodes.RemoveAndDeleteAll();
			AssertHasWarningContaining(Classification.CC_TariffNumInfo, TariffValidator.WarningPermitCodeMayBeRequiredForThisTariff);
		}

		public void TestPartsOfTariffValidationWhenMainTariffIsAddedThenRemoved()
		{
			Classification.CC_PartsOfClassification = "8547.10.00.00G";
			AssertHasMessageError(Classification.CC_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);
			Classification.CC_TariffNum = "8547.10.00.00G";
			AssertHasMessageError(Classification.CC_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);
			Classification.CC_TariffNum = "";
			AssertHasMessageError(Classification.CC_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfCannotUsePartsOfTariffCodeHere);
		}

		public void TestEnteringNormalCodeInPartsOfFieldWithNoTariffInMainCodeErrors()
		{
			Classification.CC_PartsOfClassification = TariffValid.U0_Tariff;
			AssertHasMessageError(Classification.CC_PartsOfClassificationInfo, TariffValidator.MessageErrorPartsOfDontNeedATariffCodeHere);
		}

		#region Implementation
		NZCClassification CreateNZCClassification(ZString tariffCode, ZDateTime dateActiveFrom, ZDateTime dateActiveTo, ZBool isManual)
		{
			NZCClassification result = Factory.New<NZCClassification>();
			result.U0_Tariff = tariffCode;
			result.U0_DateActiveFrom = dateActiveFrom;
			result.U0_DateActiveTo = dateActiveTo;
			result.U0_IsManual = isManual;
			return result;
		}

		#region TariffValid
		NZCClassification TariffValid
		{
			get
			{
				if (fTariffValid == null)
				{
					fTariffValid = CreateNZCClassification("0000.00.00.01A", dateForDutyRate.AddDays(-1), dateForDutyRate.AddDays(1), isManual: false);
				}
				return fTariffValid;
			}
		}
		NZCClassification fTariffValid;
		#endregion

		#region TariffOutOfDate
		NZCClassification TariffOutOfDate
		{
			get
			{
				if (fTariffOutOfDate == null)
				{
					fTariffOutOfDate = CreateNZCClassification("0000.00.00.02B", dateForDutyRate.AddDays(-5), dateForDutyRate.AddDays(-1), isManual: false);
				}
				return fTariffOutOfDate;
			}
		}
		NZCClassification fTariffOutOfDate;
		#endregion

		#region TariffNotInForceYet
		NZCClassification TariffNotInForceYet
		{
			get
			{
				if (fTariffNotInForceYet == null)
				{
					fTariffNotInForceYet = CreateNZCClassification("0000.00.00.03C", dateForDutyRate.AddDays(1), dateForDutyRate.AddDays(5), isManual: false);
				}
				return fTariffNotInForceYet;
			}
		}
		NZCClassification fTariffNotInForceYet;

		#endregion

		#region TariffManual
		NZCClassification TariffManual
		{
			get
			{
				if (fTariffManual == null)
				{
					fTariffManual = CreateNZCClassification("0000.00.00.05E", dateForDutyRate.AddDays(-1), dateForDutyRate.AddDays(1), isManual: true);
				}
				return fTariffManual;
			}
		}
		NZCClassification fTariffManual;
		#endregion

		public const string TariffCodeValidStructureButNotOnFile = "0000.00.00.00K";
		public const string TariffCodeInvalidStructure = "0000.00.00";

		public const string TariffCodeInvalidExcise = "99.10.25G";
		public const string TariffCodeNotInForceInTheTestSystem = "8479.89.00.43J";

		public const string TariffCodeRequiringPermitCodes02Digit = "0301.10.00.00B"; // 03
		public const string TariffCodeRequiringPermitCodes04Digit = "0307.10.00.02H"; // 0307
		public const string TariffCodeRequiringPermitCodes07Digit = "1605.20.01.00K"; // 1605.20
		public const string TariffCodeRequiringPermitCodes10Digit = "0307.39.00.01H"; // 0307.39.00
		public const string TariffCodeRequiringPermitCodes14Digit = "0307.99.19.00D"; // 0307.99.19.00D
		public const string TariffCodeRequiringNoPermitCodes = "3605.00.00.01E";

		ZDateTime dateForDutyRate;

		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclaration.New(Factory);
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					fDeclaration.JE_DateOfArrival = new ZDateTime(2005, 7, 11);
					dateForDutyRate = fDeclaration.DateForDutyRate;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
		#endregion

		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				}
				return fInvoiceHeader;
			}
		}
		JobComInvoiceHeader fInvoiceHeader;
		#endregion

		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;
		#endregion

		#region Classification
		CusClassification Classification
		{
			get
			{
				if (fClassification == null)
				{
					fClassification = Factory.New<CusClassification>();
					dateForDutyRate = fClassification.DateForDutyRate;
				}
				return fClassification;
			}
		}
		CusClassification fClassification;
		#endregion
		#endregion
	}
}
