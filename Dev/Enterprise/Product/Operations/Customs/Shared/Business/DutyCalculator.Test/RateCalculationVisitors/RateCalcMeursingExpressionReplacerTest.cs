using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

sealed class RateCalcMeursingExpressionReplacerTest
{
	[TestedType(typeof(RateCalcMeursingExpressionReplacer<>))]
	sealed class RateCalcMeursingExpressionReplacerWithCusEntryLineTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entryLine = declaration.CustomsEntryHeaders.AddNew().AllEntryLines.AddNew();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When entity (Entity) is null", () => new RateCalcMeursingExpressionReplacer<CusEntryLine>(null, e => e.RandomLine));
				AssertExceptionThrown<ArgumentNullException>("When invoiceLineProviderFunction is null", () => new RateCalcMeursingExpressionReplacer<CusEntryLine>(entryLine, null));
				AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new RateCalcMeursingExpressionReplacer<CusEntryLine>(Factory.New<CusEntryLine>(), e => e.RandomLine));
			});
		}
	}

	[TestedType(typeof(RateCalcMeursingExpressionReplacer<>))]
	sealed class RateCalcMeursingExpressionReplacerWithInvoiceLineTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("When entity (BaseJobComInvoiceLine) is null", () => new RateCalcMeursingExpressionReplacer<BaseJobComInvoiceLine>(null, i => i));
				AssertExceptionThrown<ArgumentNullException>("When invoiceLineProviderFunction is null", () => new RateCalcMeursingExpressionReplacer<BaseJobComInvoiceLine>(invoiceLine, null));
				AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new RateCalcMeursingExpressionReplacer<BaseJobComInvoiceLine>(Factory.New<BaseJobComInvoiceLine>(), e => e));
			});
		}
	}

	[TestedType(typeof(RateCalcMeursingExpressionReplacer<>))]
	sealed class RateCalcMeursingExpressionReplacerPropertiesTest : TestCaseWithFactory
	{
		public void TestGetMeursingValueList_WhenCustomsFormulaIsEmpty()
		{
			var rateCalcMeursingExpressionReplacer = new RateCalcMeursingExpressionReplacer<CusEntryLine>(entryLine, e => e.RandomLine);

			CombineAssertions("When RateView Rate Formula is empty or null", () =>
			{
				AssertEquals("Result Count", 0, rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(null).Count);
				AssertEquals("Result Count", 0, rateCalcMeursingExpressionReplacer.ReplaceApplicableMeursingExpressions(Factory.New<RateView>()).Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<BaseJobDeclaration>();
			entryLine = declaration
				.CustomsEntryHeaders.AddNew()
				.MergedLines.AddNew();
		}

		BaseJobDeclaration declaration;
		CusEntryLine entryLine;
	}
}
