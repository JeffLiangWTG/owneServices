using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(InvoiceLineDutyCalculationStrategy))]
sealed class InvoiceLineDutyCalculationStrategyTest : TestCaseWithFactory
{
	public void TestConstructor()
		=> AssertExceptionThrown<ArgumentNullException>("Invoice line is null", () => new InvoiceLineDutyCalculationStrategy(null));

	public void TestCalculateExciseDuty()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);

		AssertEquals("[Pre-Condition]: Excise Tariff One and Rate MB200", data.ExciseTariffOne.PK, data.MB200Rate.ZZ2_ZZ1_Tariff);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

		var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "MB200";
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsSecondQuantity = 0.02;
		invoiceLine.JI_CustomsSecondUnitQty = "LTR";
		invoiceLine.JI_CustomsThirdQuantity = 13;
		invoiceLine.JI_CustomsThirdUnitQty = "NMB";
		var exciseDutyCalculator = new InvoiceLineDutyCalculationStrategy(invoiceLine);

		CombineAssertions("Excise duty calculator", () =>
		{
			AssertEquals("Single quantity", 87m, exciseDutyCalculator.Calculate());

			invoiceLine.JI_CustomsFourthQuantity = 8;
			invoiceLine.JI_CustomsFourthUnitQty = "NMB";
			AssertEquals("Multiple EXC quantities", 141m, exciseDutyCalculator.Calculate());
		});
	}

	public void TestCalculateExciseDutyWhenCalculateDutyIsFalse()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "77",
			previousProcedureCode: "77",
			concession: "777",
			description: "Description",
			shipmentType: "IMP",
			calculateDuty: false);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "7777";

		var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "MB200";
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = "KGM";

		var exciseDutyCalculator = new InvoiceLineDutyCalculationStrategy(invoiceLine);

		AssertEquals("No Duty Calculation when calculateDuty is false", 0m, exciseDutyCalculator.Calculate());
	}

	public void TestCalculateExciseDuty_DateForDuty()
	{
		var data = RateCalculatorDataHelper.SetupRefDataForDutyCalculation(Factory);

		AssertEquals("[Pre-Condition]: Excise Tariff One and Rate MB200", data.ExciseTariffOne.PK, data.MB200Rate.ZZ2_ZZ1_Tariff);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;

		var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_DateForDuty = ZDateTime.Now;
		invoiceLine.JI_CEI = entryInstruction.PK;

		AssertEquals("[Pre-Condition]: InvoiceLine uses CEI_DateForDuty", entryInstruction.CEI_DateForDuty, invoiceLine.EffectiveDateForDutyAndRate);

		invoiceLine.JI_PrimaryPreference = data.Preference.ZZS_Preference;
		invoiceLine.JI_Tariff = data.ExciseTariffOne.ZZ1_TariffCode;
		invoiceLine.JI_SupplementaryCode1 = "MB200";
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = "KGM";
		invoiceLine.JI_CustomsSecondQuantity = 0.02;
		invoiceLine.JI_CustomsSecondUnitQty = "LTR";
		invoiceLine.JI_CustomsThirdQuantity = 13;
		invoiceLine.JI_CustomsThirdUnitQty = "NMB";
		var exciseDutyCalculator = new InvoiceLineDutyCalculationStrategy(invoiceLine);

		CombineAssertions("Excise duty calculator", () =>
		{
			AssertEquals("Single quantity", 87m, exciseDutyCalculator.Calculate());

			invoiceLine.JI_CustomsFourthQuantity = 8;
			invoiceLine.JI_CustomsFourthUnitQty = "NMB";
			AssertEquals("Multiple EXC quantities", 141m, exciseDutyCalculator.Calculate());
		});
	}
}
