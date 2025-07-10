using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

class UniversalRateCalcDataTest : TestCaseWithFactory
{
	public void TestCustomsValue()
	{
		PopulateExchangeRateData();
		var (entryLine, _) = GetNewEntryLine();

		entryLine.CL_CustomsValue = 1.0m;
		var universalRateCalcData = new UniversalRateCalcData(entryLine, Factory.New<RateView>());

		AssertEquals(2.0m, universalRateCalcData.CustomsValue);
	}

	public void TestDateOfValuation()
	{
		var (entryLine, _) = GetNewEntryLine();
		entryLine.RandomLine.EntryInstruction.CEI_DateForDuty = ZDateTime.Today.AddDays(2);
		entryLine.CL_CustomsValue = 1.0m;
		var universalRateCalcData = new UniversalRateCalcData(entryLine, Factory.New<RateView>());

		AssertEquals(ZDateTime.Today.AddDays(2), universalRateCalcData.DateOfValuation);
	}

	void PopulateExchangeRateData()
	{
		RefExchangeRate exchangeRate = RefExchangeRate.New(Factory);
		exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
		exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
		exchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate;
		exchangeRate.RE_SellRate = 2.0m;
		exchangeRate.RE_RX_NKExCurrency = CurrencyCodes.EuropeanUnion;
		exchangeRate.RE_GC = GlbCompany.CurrentCompany.PK;

		Factory.Save();
	}

	(Declaration.CusEntryLine, Declaration.JobDeclaration) GetNewEntryLine()
	{
		var declaration = Factory.NewWithValidTestData<Declaration.JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_DateForDuty = ZDateTime.Today;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = instruction.PK;
		declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		var entryHeader = declaration.CustomsEntryHeaders.First();
		return (entryHeader.AllEntryLines.First(), declaration);
	}
}
