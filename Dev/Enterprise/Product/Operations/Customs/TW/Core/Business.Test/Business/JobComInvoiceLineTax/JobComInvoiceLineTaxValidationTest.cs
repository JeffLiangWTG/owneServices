using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceLineTaxValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJLT_Type()
		{
			universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = ZString.Empty;
			AssertHasErrorContaining(invoiceLineTax.JLT_TypeInfo, MandatoryValidation.MustBeEntered);
			invoiceLineTax.JLT_Type = "HSN";
			AssertHasMessageErrorContaining(invoiceLineTax.JLT_TypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoErrorContaining(invoiceLineTax.JLT_TypeInfo, MandatoryValidation.MustBeEntered);
			invoiceLineTax.JLT_Type = "SS";
			AssertNoMessageErrorContaining(invoiceLineTax.JLT_TypeInfo, ListValidation.InvalidCodeMessageError);
			var newInvoiceLineTax = invoiceLine.Taxes.AddNew();
			newInvoiceLineTax.JLT_Type = "SS";
			AssertHasMessageErrorContaining(newInvoiceLineTax.JLT_TypeInfo, ValidationConstants.InvoiceLineTax.DuplicateTariffTypeFound(newInvoiceLineTax.JLT_TypeDescription));
		}

		public void TestCheckJLT_Tariff()
		{
			var hsnTariffType = universalTestHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var ssTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var testTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "TST");
			Factory.Save();
			var tariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, testTariffType.PK, "12346578", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariff2 = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Tariff = "4566997";
			AssertNoMessageErrorContaining(invoiceLineTax.JLT_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLineTax.JLT_Tariff = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceLineTax.JLT_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLineTax.JLT_Tariff = "XXX";
			AssertHasMessageErrorContaining(invoiceLineTax.JLT_TariffInfo, ListValidation.InvalidCodeMessageError);
			invoiceLineTax.JLT_Type = "SS";
			invoiceLineTax.JLT_Tariff = "12346578";
			AssertHasMessageErrorContaining(invoiceLineTax.JLT_TariffInfo, ValidationConstants.InvoiceLineTax.TariffDoesNotBelongToType(invoiceLineTax.JLT_Tariff, invoiceLineTax.JLT_Type));
			var passengerCarChildtariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var passengerCarTariffRelationship = universalTestHelper.CreateTariffRelationship(passengerCarChildtariff.PK, hsnTariffType.PK, "87031000002");
			var sedanCarChildTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var testSedanChildTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, testTariffType.PK, "TESTSEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var sedanTariffRelationship = universalTestHelper.CreateTariffRelationship(testSedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			Factory.Save();
			invoiceLine.JI_Tariff = "87031000002";
			var invoiceLineTax2 = invoiceLine.Taxes.AddNew();
			invoiceLineTax2.JLT_Type = "SS";
			invoiceLineTax2.JLT_Tariff = "PASSENGERCAR";
			AssertNoMessageErrors(invoiceLineTax2.JLT_TariffInfo);
			invoiceLineTax2.JLT_Tariff = "SEDAN";
			AssertHasMessageErrorContaining(invoiceLineTax2.JLT_TariffInfo, ValidationConstants.InvoiceLineTax.ChildTariffDoesNotBelongToMainTariff(invoiceLineTax2.JLT_Tariff, invoiceLine.JI_Tariff));
			invoiceLineTax2.JLT_Tariff = "TESTSEDAN";
			AssertHasMessageErrorContaining(invoiceLineTax2.JLT_TariffInfo, ValidationConstants.InvoiceLineTax.TariffDoesNotBelongToType(invoiceLineTax2.JLT_Tariff, invoiceLineTax2.JLT_Type));
			invoiceLineTax2.JLT_Type = "TST";
			invoiceLineTax2.JLT_Tariff = "TESTSEDAN";
			AssertNoMessageErrors(invoiceLineTax2.JLT_TariffInfo);
		}

		public void TestCheckJLT_MethodOfPayment()
		{
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_MethodOfPayment = "ABC";
			AssertHasMessageErrorContaining(invoiceLineTax.JLT_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			invoiceLineTax.JLT_MethodOfPayment = "CAS";
			AssertNoMessageErrorContaining(invoiceLineTax.JLT_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation
		JobComInvoiceLine invoiceLine;
		UniversalReferenceTestDataHelper universalTestHelper;
		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
		}
		#endregion
	}
}
