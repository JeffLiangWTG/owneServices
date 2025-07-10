using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusVehicleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCVH_ModelYear_Mandatory()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "87032110";
			vehicle.Validation.ValidateCVH_ModelYear();
			AssertHasMessageErrorContaining("Tariff requires car details", vehicle.CVH_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_Tariff = ZString.Empty;
			vehicle.CVH_ModelYear = ZString.Empty;
			AssertNoMessageErrorContaining("Car Details not required", vehicle.CVH_ModelYearInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCVH_ModelYear_FutureYear()
	{
		const string dateInTheFutureMessage = "Date is in the future.";
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = "87032110";
			vehicle.CVH_ModelYear = "5000";
			AssertHasWarningContaining("Car detail required and date is in the future", vehicle.CVH_ModelYearInfo, dateInTheFutureMessage);

			vehicle.CVH_ModelYear = "2000";
			AssertNoWarningContaining("In the past", vehicle.CVH_ModelYearInfo, dateInTheFutureMessage);

			invoiceLine.JI_Tariff = ZString.Empty;
			vehicle.CVH_ModelYear = "5000";
			AssertHasWarningContaining("Car detail not required but date is in the future", vehicle.CVH_ModelYearInfo, dateInTheFutureMessage);

			vehicle.CVH_ModelYear = ZString.Empty;
			AssertNoWarningContaining("Car detail not required and date is empty", vehicle.CVH_ModelYearInfo, dateInTheFutureMessage);
		});
	}

	public void TestCheckCVH_ModelYear_InvalidStructure()
	{
		const string invalidYearMessage = "Invalid year entered. Valid year must be greater than 1900.";
		CombineAssertions(() =>
		{
			vehicle.CVH_ModelYear = "-200";
			AssertHasError("Negative Year", vehicle.CVH_ModelYearInfo, invalidYearMessage);

			vehicle.CVH_ModelYear = "ZADW";
			AssertHasError("Invalid Year", vehicle.CVH_ModelYearInfo, invalidYearMessage);

			vehicle.CVH_ModelYear = "1900";
			AssertHasError("Threshold year", vehicle.CVH_ModelYearInfo, invalidYearMessage);

			vehicle.CVH_ModelYear = "1901";
			AssertNoError("Valid starting year", vehicle.CVH_ModelYearInfo, invalidYearMessage);
		});
	}

	public void TestCheckCVH_VehicleIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			vehicle.CVH_VehicleIdentificationNumber = "1";
			AssertNoNotifications(vehicle.CVH_VehicleIdentificationNumberInfo);

			vehicle.CVH_VehicleIdentificationNumber = ZString.Empty;
			AssertNoNotifications(vehicle.CVH_VehicleIdentificationNumberInfo);

			invoiceLine.JI_Tariff = "87032110";
			vehicle.CVH_VehicleIdentificationNumber = "ABC123";
			AssertNoNotifications(vehicle.CVH_VehicleIdentificationNumberInfo);

			vehicle.CVH_VehicleIdentificationNumber = ZString.Empty;
			AssertHasWarningContaining(vehicle.CVH_VehicleIdentificationNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(vehicle.CVH_VehicleIdentificationNumberInfo, "'brak' will be used in the XML message.");
		});
	}

	public void TestCheckCVH_VehicleIdentificationNumberR432()
	{
		const string messageError = "(R432) Invalid VIN number (I, O, Q letters are forbidden)";
		invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		vehicle.CVH_VehicleIdentificationNumber = "VIN";

		CombineAssertions(() =>
		{
			AssertHasMessageError("Invalid VIN starts with I, O or Q", vehicle.CVH_VehicleIdentificationNumberInfo, messageError);

			vehicle.CVH_VehicleIdentificationNumber = "VAN";
			AssertNoMessageError("VIN is valid", vehicle.CVH_VehicleIdentificationNumberInfo, messageError);
		});
	}

	public void TestIsCarDetailsDataRequired()
	{
		var engine = vehicle.Engine;
		var invoiceLine = vehicle.InvoiceLine;
		invoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		CheckIsCarDetailsDataRequired(false, false, false, false, false);
		CheckIsCarDetailsDataRequired(true, false, false, false, false);
		CheckIsCarDetailsDataRequired(false, true, false, false, false);
		CheckIsCarDetailsDataRequired(false, false, true, false, false);
		CheckIsCarDetailsDataRequired(false, false, false, true, false);
		CheckIsCarDetailsDataRequired(false, false, false, false, true);

		void CheckIsCarDetailsDataRequired(bool insertCVH_ModelYear, bool insertCVH_VehicleIdentificationNumber,
			bool insertJI_MarkModel, bool insertCEG_EngineNumber, bool insertCEG_EngineType)
		{
			var shouldHaveMessageError = insertCVH_ModelYear || insertCVH_VehicleIdentificationNumber || insertJI_MarkModel || insertCEG_EngineNumber || insertCEG_EngineType;
			vehicle.CVH_ModelYear = insertCVH_ModelYear ? "A" : string.Empty;
			vehicle.CVH_VehicleIdentificationNumber = insertCVH_VehicleIdentificationNumber ? "A" : string.Empty;
			invoiceLine.JI_MarkModel = insertJI_MarkModel ? "A" : string.Empty;
			engine.CEG_EngineNumber = insertCEG_EngineNumber ? "A" : string.Empty;
			engine.CEG_EngineType = insertCEG_EngineType ? "A" : string.Empty;
			vehicle.Validation.ValidateAll();
			engine.Validation.ValidateAll();
			invoiceLine.PLValidationOrNull?.ValidateJI_MarkModel();

			CombineAssertions($"Setup : {insertCVH_ModelYear} {insertCVH_VehicleIdentificationNumber} {insertJI_MarkModel} {insertCEG_EngineNumber} {insertCEG_EngineType}", () =>
			{
				AssertEquals("IsCarDetailsDataRequired", shouldHaveMessageError, vehicle.IsCarDetailsDataRequired);

				CheckMustBeEnteredMessageError(!insertCVH_ModelYear && shouldHaveMessageError, vehicle.CVH_ModelYearInfo);
				CheckMustBeEnteredWarning(!insertCVH_VehicleIdentificationNumber && shouldHaveMessageError, vehicle.CVH_VehicleIdentificationNumberInfo);
				CheckMustBeEnteredMessageError(!insertJI_MarkModel && shouldHaveMessageError, invoiceLine.JI_MarkModelInfo);
				CheckMustBeEnteredWarning(!insertCEG_EngineNumber && shouldHaveMessageError, engine.CEG_EngineNumberInfo);
				CheckMustBeEnteredMessageError(!insertCEG_EngineType && shouldHaveMessageError, engine.CEG_EngineTypeInfo);
			});
		}

		void CheckMustBeEnteredMessageError(bool isRequired, ZPropertyInfo propertyInfo)
		{
			if (isRequired)
			{
				AssertHasMessageErrorContaining($"{propertyInfo.Name} should be required", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoMessageErrorContaining($"{propertyInfo.Name} should not be required", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		void CheckMustBeEnteredWarning(bool isRequired, ZPropertyInfo propertyInfo)
		{
			if (isRequired)
			{
				AssertHasWarningContaining($"{propertyInfo.Name} should be required", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			}
			else
			{
				AssertNoWarningContaining($"{propertyInfo.Name} should not be required", propertyInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		AddPLCodesForCarDetailsRequired();
		var declaration = Factory.New<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		vehicle = invoiceLine.FirstVehicle;
	}
	JobComInvoiceLine invoiceLine;
	CusVehicle vehicle;

	void AddPLCodesForCarDetailsRequired()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = PL.Business.UniversalReferenceConstants.RefCusCodeListType.Codes.CarDetailsRequiredCodes;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "PLCodesForCarDetails");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "87032110", "mark1,model1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}
}
