using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class CusEngineValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCEG_EngineType()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			engine.CEG_EngineType = "1";
			AssertHasMessageError(engine.CEG_EngineTypeInfo, ListValidation.InvalidCodeMessageError);

			engine.CEG_EngineType = ZString.Empty;
			AssertNoNotifications(engine.CEG_EngineTypeInfo);

			invoiceLine.JI_Tariff = "87032110";
			engine.CEG_EngineType = FuelTypeList.Codes.B;
			AssertNoNotifications(engine.CEG_EngineTypeInfo);

			engine.CEG_EngineType = "1";
			AssertHasMessageError(engine.CEG_EngineTypeInfo, ListValidation.InvalidCodeMessageError);

			engine.CEG_EngineType = ZString.Empty;
			AssertHasMessageErrorContaining(engine.CEG_EngineTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCEG_EngineNumber()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_Tariff = ZString.Empty;
			engine.CEG_EngineNumber = "1";
			AssertNoNotifications(engine.CEG_EngineNumberInfo);

			engine.CEG_EngineNumber = ZString.Empty;
			AssertNoNotifications(engine.CEG_EngineNumberInfo);

			invoiceLine.JI_Tariff = "87032110";
			engine.CEG_EngineNumber = "ABC123";
			AssertNoNotifications(engine.CEG_EngineNumberInfo);

			engine.CEG_EngineNumber = ZString.Empty;
			AssertHasWarningContaining(engine.CEG_EngineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(engine.CEG_EngineNumberInfo, "'brak' will be used in the XML message.");
		});
	}

	public void TestCheckCEG_CapacityCC()
	{
		CombineAssertions(() =>
		{
			engine.CEG_CapacityCC = -2000;
			AssertHasErrorContaining("Negative", engine.CEG_CapacityCCInfo, MandatoryValidation.ValueCannotBeNegative);

			engine.CEG_CapacityCC = 5000;
			AssertNoErrorContaining("Positive", engine.CEG_CapacityCCInfo, MandatoryValidation.ValueCannotBeNegative);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		AddPLCodesForCarDetailsRequired();
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		engine = invoiceLine.FirstVehicle.Engine;
	}
	JobComInvoiceLine invoiceLine;
	CusEngine engine;

	void AddPLCodesForCarDetailsRequired()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = PL.Business.UniversalReferenceConstants.RefCusCodeListType.Codes.CarDetailsRequiredCodes;
		helper.CreateNewOrGetExistingCusCodeType(codeType, "PLCodesForCarDetails");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Poland, codeType, "87032110", "mark1,model1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();
	}
}
