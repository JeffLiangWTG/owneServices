using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CustomsRuleHelperTest : TestCaseWithFactory
	{
		public void TestValidateWithTariffRule()
		{
			var date = new ZDateTime(2023, 11, 30);

			var customsRule = Factory.New<CustomsRule>();
			customsRule.CPH_StartDate = date.Date.AddMonths(-1);
			customsRule.CPH_EndDate = date.Date.AddMonths(1);
			customsRule.CPH_PermitDescription = "TEST";
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DateOfFirstArrival = date;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var expected = string.Format(CustomsRuleHelper.TRFRuleMessageError, customsRule.HumanReadableName);
			invoiceLine.JI_Tariff = "3333333333";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);

			var trfRule = customsRule.Rules.AddNew();
			trfRule.CPR_RuleCode = CustomsRuleRuleCodeList.Codes.TariffNumber;
			trfRule.CPR_ValueFrom = "33";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "333";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333";
			Factory.Save();
			invoiceLine.JI_Tariff = "4444444444";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);

			trfRule.CPR_ValueFrom = "3333333332";
			trfRule.CPR_ValueTo = "3333333334";
			Factory.Save();
			invoiceLine.JI_Tariff = "3333333331";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333332";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333333";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333334";
			AssertHasMessageError(invoiceLine.JI_TariffInfo, expected);
			invoiceLine.JI_Tariff = "3333333335";
			AssertNoMessageError(invoiceLine.JI_TariffInfo, expected);
		}
	}
}
