using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class TariffValidatorTest : TestCaseWithFactory
	{
		public void TestInvalidTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "12345678";
			AssertHasMessageError("Tariff does not exist", invoiceLine.JI_TariffInfo, "The tariff code cannot be found in the customs tariff code list.");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Singapore, Constants.TariffTypes.HarmonizedSystem);
			Factory.Save();
			var testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "12345679", ZDateTime.MinSmallDateTimeValue, ZDateTime.Now.AddYears(-1), "Test Tariff - expired", compositeKey: "HC00000001");
			Factory.Save();
			var formatedTariff = new TariffFormatter().Format(testTariff.ZZ1_TariffCode);
			Factory.ClearQueryCache();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = "12345679";
			AssertHasMessageError("Tariff has expired", invoiceLine.JI_TariffInfo, "This Tariff is no longer valid for use as it has expired.");
			testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "21345678", ZDateTime.Now.AddYears(1), ZDateTime.Now.AddYears(5), "Test Tariff - not yet implemented", compositeKey: "HC00000003");
			Factory.Save();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = "21345678";
			AssertHasMessageError("Tariff has not yet come into implementation", invoiceLine.JI_TariffInfo, string.Format("This Tariff is not yet valid for use. It does not become active until {0}.", testTariff.ZZ1_StartDate.ToLongTimeString()));
			testTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Singapore, tariffType.PK, "34567812", ZDateTime.Now.AddYears(-1), ZDateTime.Now.AddYears(5), "Test Tariff - valid", compositeKey: "HC00000003");
			Factory.Save();
			invoiceLine.JI_Tariff = ZString.Empty;
			invoiceLine.JI_Tariff = "34567812";
			AssertEquals(false, invoiceLine.JI_TariffInfo.HasMessageErrors());
		}
	}
}
