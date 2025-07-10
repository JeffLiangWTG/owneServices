using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class LCGSTBackRoundingCalculatorTest : TestCaseWithFactory
	{
		public void TestGetBackRoundedGSTAmount()
		{
			var declaration = Factory.New<TestDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 60m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10m;

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 20m;

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 30m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1.11m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine.PK).Round(2));
			AssertEquals(2.22m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine2.PK).Round(2));
			AssertEquals(3.34m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine3.PK).Round(2));

			AssertEquals(3.34m, ((MasterFiles.Business.IUltimateDistributee)invoiceLine3).GSTVATAmount.Round(2));

			invoice.JZ_InvoiceAmount = 70m;
			invoiceLine3.JI_LinePrice = 40m;
			var gst = invoiceLine3.CusEntryLine.Fees.GetOrAddFeeByFeeType(declaration.GSTOrVATCode);
			gst.CF_IsLandedCostOnly = true;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1.11m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine.PK).Round(2));
			AssertEquals(2.22m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine2.PK).Round(2));
			AssertEquals(4.45m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine3.PK).Round(2));

			AssertEquals(4.45m, ((MasterFiles.Business.IUltimateDistributee)invoiceLine3).GSTVATAmount.Round(2));

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2.22m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine.PK).Round(2));
			AssertEquals(4.44m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine2.PK).Round(2));
			AssertEquals(8.89m, declaration.LCGSTBackRoundingCalculator.GetBackRoundedGSTAmount(invoiceLine3.PK).Round(2));

			AssertEquals(8.89m, ((MasterFiles.Business.IUltimateDistributee)invoiceLine3).GSTVATAmount.Round(2));
		}

		class TestDeclaration : BaseJobDeclaration
		{
			public TestDeclaration(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override MergeManager GetMergeManager()
			{
				return new TestMergeManager(this);
			}
		}

		class TestMergeManager : MergeManager
		{
			public TestMergeManager(TestDeclaration declaration)
				: base(declaration)
			{
			}

			protected override LineMerger GetNewLineMergerCore()
			{
				return new TestLineMerger((TestDeclaration)Declaration);
			}
		}

		class TestLineMerger : LineMerger
		{
			public TestLineMerger(TestDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void CalculateDuties()
			{
				base.CalculateDuties();
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					foreach (CusEntryLine entryLine in entry.MergedLines)
					{
						entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount).CF_ChargeAmount = new ZDecimal(entryLine.CL_CustomsValue * 0.1111m).Round(2);
						entryLine.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.VAT).CF_ChargeAmount = new ZDecimal(entryLine.CL_CustomsValue * 0.2222m).Round(2);
					}
				}
			}
		}
	}
}
