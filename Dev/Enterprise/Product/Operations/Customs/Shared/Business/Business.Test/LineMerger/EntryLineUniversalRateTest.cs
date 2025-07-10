using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(EntryLineUniversalRate))]
	public abstract class EntryLineUniversalRateAbstractTest<T> : TestCaseWithFactory
		where T : EntryLineUniversalRate
	{
		public void TestCountrySpecificValueList()
		{
			var entryLineUniversalRate = GetEntryLineUniversalRate();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder<(string uq, decimal qty)>(ExpectCountrySpecificValues, entryLineUniversalRate.CountrySpecificValueList.Select(a => (a.Key, a.Value)));
				AssertSame("dic cached", entryLineUniversalRate.AdditionalInformationList, entryLineUniversalRate.AdditionalInformationList);
			});
		}

		public void TestAdditionalInformationList()
		{
			var entryLineUniversalRate = GetEntryLineUniversalRate();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder(ExpectAdditionalInformationList, entryLineUniversalRate.AdditionalInformationList);
				AssertSame("list cached", entryLineUniversalRate.AdditionalInformationList, entryLineUniversalRate.AdditionalInformationList);
			});
		}

		public void TestMeursingExpressionList()
		{
			var entryLineUniversalRate = GetEntryLineUniversalRate();
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInExactOrder<(string code, string expression)>(ExpectedMeursingExpressionList, entryLineUniversalRate.MeursingExpressionList.Select(a => (a.Key, a.Value)));
				AssertSame("meursing expression list cached", entryLineUniversalRate.MeursingExpressionList, entryLineUniversalRate.MeursingExpressionList);
			});
		}

		protected abstract T GetEntryLineUniversalRate();

		protected virtual IEnumerable<(string uq, decimal qty)> ExpectCountrySpecificValues => Enumerable.Empty<(string uq, decimal qty)>();

		protected virtual IList<Tuple<string, string>> ExpectAdditionalInformationList => new List<Tuple<string, string>>();

		protected virtual IEnumerable<(string code, string expression)> ExpectedMeursingExpressionList => Enumerable.Empty<(string code, string expression)>();
	}

	[TestedType(typeof(EntryLineUniversalRate))]
	class EntryLineUniversalRateBaseOnlyTest : EntryLineUniversalRateAbstractTest<EntryLineUniversalRate>
	{
		public void TestCustomsValue()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			var entryLineUniversalRate = new EntryLineUniversalRate(entryLine, 100.0m);
			AssertEquals(100.0m, entryLineUniversalRate.CustomsValue);
		}

		[TestDate(2021, 9, 24)]
		public void TestDateOfValuation()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			var entryLineUniversalRate = new EntryLineUniversalRate(entryLine, new decimal(100.0));
			AssertEquals(new DateTime(2021, 9, 24), entryLineUniversalRate.DateOfValuation.Date);
		}

		public void TestCalculateValueForDuty()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine.CL_CustomsValue = 100.21m;
			var universalRate = new EntryLineUniversalRate(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("ValueForDuty = CL_CustomsValue", 100.21m, universalRate.ValueForDuty);
				universalRate.CustomsValueFormula = "CV / 2";
				AssertEquals("ValueForDuty = CL_CustomsValue / 2", 50.11m, universalRate.ValueForDuty);
				universalRate.CustomsValueFormula = "CV * 2";
				AssertEquals("ValueForDuty = CL_CustomsValue * 2", 200.42m, universalRate.ValueForDuty);
			});
		}

		public void TestUnitOfMeasureValueList()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine1.JI_CustomsUnitQty = "035";
			invoiceLine1.JI_CustomsQuantity = 22m;
			invoiceLine1.JI_CustomsSecondUnitQty = "001";
			invoiceLine1.JI_CustomsSecondQuantity = 33m;
			invoiceLine1.JI_CustomsThirdUnitQty = "002";
			invoiceLine1.JI_CustomsThirdQuantity = 44m;
			var universalRate = new EntryLineUniversalRate(entryLine);
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder<(string uq, decimal qty)>(new List<(string uq, decimal qty)> { ("035", 22m), ("001", 33m), ("002", 44m) }, universalRate.UnitOfMeasureValueList.Select(a => (a.Key, a.Value)));
				AssertSame("dic cached", universalRate.UnitOfMeasureValueList, universalRate.UnitOfMeasureValueList);
			});
		}

		protected override EntryLineUniversalRate GetEntryLineUniversalRate()
		{
			var entryLine = Factory.NewWithValidTestData<CusEntryLine>();
			return new EntryLineUniversalRate(entryLine);
		}
	}
}
