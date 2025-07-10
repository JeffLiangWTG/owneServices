using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(JobComInvoiceLine.RateSelectionCriteria))]
sealed class JobComInvoiceLineRateSelectionCriteriaTest : TestCaseWithFactory
{
	public void TestRateSelectionCriteriaType() => CombineAssertions(() =>
	{
		var line = Factory.New<JobComInvoiceLine>();
		AssertType<JobComInvoiceLine.RateSelectionCriteria>("DutyRateSelectionCriteria Type", line.DutyRateSelectionCriteria);
		AssertType<JobComInvoiceLine.RateSelectionCriteria>("AllApplicableRatesSelectionCriteria Type", line.AllApplicableRatesSelectionCriteria);
		AssertType<JobComInvoiceLine.RateSelectionCriteria>("ExciseRateSelectionCriteria Type", line.ExciseRateSelectionCriteria);
	});

	public void TestTradeGroupCountry() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		declaration.JE_GoodsDestination = "DE";
		var invoice = declaration.Invoices.AddNew();

		var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		line1.JI_CountryOfOrigin = "SE";
		var rateSelectionCriteria = CreateRateSelectionCriteria(line1);
		AssertEquals("When Import Declaration, TradeGroupCountry", "SE", rateSelectionCriteria.TradeGroupCountry);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		line2.JI_CountryOfOrigin = "CH";
		rateSelectionCriteria = CreateRateSelectionCriteria(line2);
		AssertEquals("When Export Declaration, TradeGroupCountry", "DE", rateSelectionCriteria.TradeGroupCountry);
	});

	public void TestAdditionalCodes() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();

		var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		var rateSelectionCriteria = CreateRateSelectionCriteria(line1);
		AssertContainsExactElementsInAnyOrder("When No Excise Codes are entered", Array.Empty<ZString>(), rateSelectionCriteria.AdditionalCodes);

		var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		line2.JI_SupplementaryCode1 = "CD1";
		line2.JI_SupplementaryCode2 = "CD2";
		line2.AdditionalSupplementaryCodes.AddNew().CY_Code = "CD3";
		rateSelectionCriteria = CreateRateSelectionCriteria(line2);
		AssertContainsExactElementsInAnyOrder("When Excise Codes are entered", new ZString[] { "CD1", "CD2", "CD3" }, rateSelectionCriteria.AdditionalCodes);
	});

	public void TestExciseRateSelectionCriteria_RateType() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();

		var line1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		AssertEquals("When Import Declaration, RateType", Constants.RateTypes.Excise, line1.ExciseRateSelectionCriteria.RateType);

		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		AssertEquals("When Export Declaration, RateType", Constants.RateTypes.ExportDuty, line2.ExciseRateSelectionCriteria.RateType);
	});

	public void TestRateSelectionCriteria_EffectiveDate()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
		var rateSelectionCriteria = CreateRateSelectionCriteria(invoiceLine);

		AssertEquals("RateSelectionCriteria.EffectiveDate is set to InvoiceLine.EffectiveDateForDutyAndRate", invoiceLine.EffectiveDateForDutyAndRate, rateSelectionCriteria.EffectiveDate);
	}

	static JobComInvoiceLine.RateSelectionCriteria CreateRateSelectionCriteria(JobComInvoiceLine line) => new(line, ZString.Empty, ZString.Empty);
}
