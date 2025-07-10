using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AllocateWeightValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckNetWeight()
		{
			allocateWeight.NetWeight = -1;
			AssertHasError(allocateWeight.NetWeightInfo, "Net Weight cannot be negative.");
			allocateWeight.NetWeight = 1;
			AssertNoError(allocateWeight.NetWeightInfo, "Net Weight cannot be negative.");

			allocateWeight.NetWeight = 1234567890123456789;
			AssertHasErrorContaining(allocateWeight.NetWeightInfo, "the maximum value allowed for Net Weight is 999,999.999.");
			allocateWeight.NetWeight = 1;
			AssertNoErrorContaining(allocateWeight.NetWeightInfo, "the maximum value allowed for Net Weight is 999,999.999.");
		}

		public void TestCheckNetWeightUnit()
		{
			allocateWeight.NetWeightUnit = "AB";
			AssertHasError(allocateWeight.NetWeightUnitInfo, "Enter a valid Net Weight Unit.");
			allocateWeight.NetWeightUnit = "KG";
			AssertNoError(allocateWeight.NetWeightUnitInfo, "Enter a valid Net Weight Unit.");
		}

		public void TestCheckGrossWeight()
		{
			allocateWeight.GrossWeight = -1;
			AssertHasError(allocateWeight.GrossWeightInfo, "Gross Weight cannot be negative.");
			allocateWeight.GrossWeight = 1;
			AssertNoError(allocateWeight.GrossWeightInfo, "Gross Weight cannot be negative.");

			allocateWeight.GrossWeight = 1234567890123456789;
			AssertHasErrorContaining(allocateWeight.GrossWeightInfo, "the maximum value allowed for Gross Weight is 999,999.999.");
			allocateWeight.GrossWeight = 1;
			AssertNoErrorContaining(allocateWeight.GrossWeightInfo, "the maximum value allowed for Gross Weight is 999,999.999.");
		}

		public void TestCheckGrossWeightUnit()
		{
			allocateWeight.GrossWeightUnit = "AB";
			AssertHasError(allocateWeight.GrossWeightUnitInfo, "Enter a valid Gross Weight Unit.");
			allocateWeight.GrossWeightUnit = "KG";
			AssertNoError(allocateWeight.GrossWeightUnitInfo, "Enter a valid Gross Weight Unit.");
		}

		public void TestCheckMethod()
		{
			allocateWeight.AllocateWeightMethod = "AB";
			AssertHasError(allocateWeight.AllocateWeightMethodInfo, "Enter a valid Method.");
			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Price;
			AssertNoError(allocateWeight.AllocateWeightMethodInfo, "Enter a valid Method.");

			allocateWeight.AllocateWeightMethod = "";
			AssertHasError(allocateWeight.AllocateWeightMethodInfo, "Please enter a Method.");
			allocateWeight.AllocateWeightMethod = AllocateWeightMethodList.Codes.Quantity;
			AssertNoError(allocateWeight.AllocateWeightMethodInfo, "Please enter a Method.");
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();

			allocateWeight = new AllocateWeight(declaration.Factory, invoiceHeader.JobComInvoiceLines.Cast<BaseJobComInvoiceLine>());
		}

		AllocateWeight allocateWeight;

		#endregion
	}
}
