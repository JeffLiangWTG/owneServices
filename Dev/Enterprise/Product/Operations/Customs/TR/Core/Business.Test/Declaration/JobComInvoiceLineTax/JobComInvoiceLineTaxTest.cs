using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	sealed class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => tax;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => tax;
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => tax;

		public void TestDecimalPlaces()
		{
			CombineAssertions(() =>
			{
				AssertHasCustomAttribute<DecimalPlacesAttribute>(tax.GetType(), "JLT_BaseValue", false, attr => attr.DecimalPlaces == 2);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(tax.GetType(), "JLT_Rate", false, attr => attr.DecimalPlaces == 2);
				AssertHasCustomAttribute<DecimalPlacesAttribute>(tax.GetType(), "JLT_Amount", false, attr => attr.DecimalPlaces == 2);
			});
		}

		public void TestCalculatePercentage()
		{
			CombineAssertions(() =>
			{
				tax.JLT_BaseValue = 200m;
				tax.JLT_Rate = 15m;

				AssertEquals("JLT_Amount", 30m, tax.JLT_Amount);
				AssertEquals("JLT_BaseValue", 200m, tax.JLT_BaseValue);
				AssertEquals("JLT_Rate", 15m, tax.JLT_Rate);

				tax.JLT_Amount = 0m;
				tax.JLT_Rate = 10m;

				AssertEquals("JLT_Amount", 20m, tax.JLT_Amount);
				AssertEquals("JLT_BaseValue", 200m, tax.JLT_BaseValue);
				AssertEquals("JLT_Rate", 10m, tax.JLT_Rate);

				tax.JLT_BaseValue = 50m;
				tax.JLT_Amount = 2m;
				tax.JLT_Rate = 4m;

				AssertEquals("JLT_Amount", 2m, tax.JLT_Amount);
				AssertEquals("JLT_BaseValue", 50m, tax.JLT_BaseValue);
				AssertEquals("JLT_Rate", 4m, tax.JLT_Rate);

				tax.JLT_BaseValue = ZDecimal.Zero;
				tax.JLT_Amount = 4m;
				tax.JLT_Rate = 5m;

				AssertEquals("JLT_Amount", ZDecimal.Zero, tax.JLT_Amount);
			});
		}

		public void TestGetNewLookups() => AssertType<JobComInvoiceLineTaxLookups>(tax.Lookups);

		public void TestGetNewValidation() => AssertType<JobComInvoiceLineTaxValidation>(tax.Validation);

		public void TestJLT_TypeDescription()
		{
			AssertEquals("JLT_TypeDescription", ZString.Empty, tax.JLT_TypeDescription);
			tax.JLT_Type = "40";
			AssertEquals("JLT_TypeDescription", "Katma Değer Vergisi", tax.JLT_TypeDescription);
		}

		public void TestJLT_Type_Caption()
		{
			AssertEquals("Type", DataBoundResourceStrings.GetDataForProperty(tax.JLT_TypeInfo).Caption);
		}

		public void TestJLT_RateOverrideReasonCode_Caption()
		{
			AssertEquals("Action", DataBoundResourceStrings.GetDataForProperty(tax.JLT_RateOverrideReasonCodeInfo).Caption);
		}

		public void TestJLT_BaseValue_Caption()
		{
			AssertEquals("Base Amount", DataBoundResourceStrings.GetDataForProperty(tax.JLT_BaseValueInfo).Caption);
		}

		public void TestJLT_MethodOfCalculation_Caption()
		{
			AssertEquals("Method of Calculation", DataBoundResourceStrings.GetDataForProperty(tax.JLT_MethodOfCalculationInfo).Caption);
		}

		public void TestJLT_Rate_Caption()
		{
			AssertEquals("Tax Rate", DataBoundResourceStrings.GetDataForProperty(tax.JLT_RateInfo).Caption);
		}

		public void TestJLT_Amount_Caption()
		{
			AssertEquals("Total Amount", DataBoundResourceStrings.GetDataForProperty(tax.JLT_AmountInfo).Caption);
		}

		public void TestJLT_MethodOfPayment_Caption()
		{
			AssertEquals("Method of Payment", DataBoundResourceStrings.GetDataForProperty(tax.JLT_MethodOfPaymentInfo).Caption);
		}

		public void TestNationalType()
		{
			CombineAssertions(() =>
			{
				tax.NationalType = DeclarationHelper.NationalVatType.Code;
				AssertEquals("NationalType Should be readonly", false, tax.NationalTypeInfo.ReadOnly);
				AssertEquals("Duty Type", DataBoundResourceStrings.GetDataForProperty(tax.NationalTypeInfo).Caption);
				AssertEquals("NationalType", "40", tax.NationalType);
			});
		}

		public void TestJLT_Type()
		{
			CombineAssertions(() =>
			{
				tax.NationalType = DeclarationHelper.NationalVatType.Code;
				AssertEquals("JLT_Type", FeeTypeList.Codes.B00, tax.JLT_Type);

				tax.NationalType = "39";
				AssertEquals("JLT_Type", "39", tax.JLT_Type);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			tax = invoiceLine.Taxes.AddNew();
			tax.JLT_Type = "D10";
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceLineTax tax;
	}
}
