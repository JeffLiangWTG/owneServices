using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class SupplierROOCalculatorTest : TestCaseWithFactory
	{
		public void TestLowValueSupplierIsNotReturned()
		{
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration - 1;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			OrgHeader[] headers = rooCalculator.Execute();
			AssertEquals(0, headers.Length);
		}

		public void TestNonEuroHighValueSupplierIsReturned()
		{
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_InvoiceAmount = 3000;
			var currencyUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			currencyUSD.ExchangeRates.DeleteAll();
			ZDateTime today = ZDateTime.Today;
			currencyUSD.SetCustomsRate(today, today, 2);
			invoice1.JZ_RX_NKInvoice_Currency = currencyUSD.RX_Code;
			invoice1.JZ_ValuationDateOverride = today;
			OrgHeader[] headers = rooCalculator.Execute();
			AssertEquals(0, headers.Length);
		}

		public void TestEuroHighValueSupplierIsReturned()
		{
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			OrgHeader[] headers = rooCalculator.Execute();
			AssertEquals(1, headers.Length);
		}

		public void TestAggregateValues()
		{
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration / 2;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_OH_Supplier = supplier1.PK;
			invoice2.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration / 2;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			OrgHeader[] headers = rooCalculator.Execute();
			AssertEquals(1, headers.Length);
			AssertSame(supplier1, headers[0]);
		}

		public void TestMultipleSuppliersAreReturned()
		{
			invoice1.JZ_OH_Supplier = supplier1.PK;
			invoice1.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_OH_Supplier = supplier2.PK;
			invoice2.JZ_InvoiceAmount = SupplierROOCalculator.MinimumRandForDeclaration;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			OrgHeader[] headers = rooCalculator.Execute();
			AssertEquals(2, headers.Length);
			if (supplier1.PK == headers[0].PK)
			{
				AssertEquals(supplier1.PK, headers[0].PK);
				AssertEquals(supplier2.PK, headers[1].PK);
			}
			else
			{
				AssertEquals(supplier2.PK, headers[0].PK);
				AssertEquals(supplier1.PK, headers[1].PK);
			}
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		OrgHeader supplier1;
		OrgHeader supplier2;
		SupplierROOCalculator rooCalculator;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice1 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoice2 = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			supplier1 = OrgHeader.New(Factory);
			supplier2 = OrgHeader.New(Factory);
			rooCalculator = new SupplierROOCalculator(declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders);
		}
	}
}
