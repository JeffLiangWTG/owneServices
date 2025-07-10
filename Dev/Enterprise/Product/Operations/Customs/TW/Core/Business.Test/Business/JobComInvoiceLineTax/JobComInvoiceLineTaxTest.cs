using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	sealed class JobComInvoiceLineTaxTest : Customs.Business.Testing.JobComInvoiceLineTaxTest
	{
		public void TestSetDefaultPaymentMethod()
		{
			var referenceDataHelper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeDTY = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateTypeCOM = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "COM");
			var refCusRateTypeSSG = referenceDataHelper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "SSG");
			var refAACusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "AA", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			var refBBCusProcedure = referenceDataHelper.CreateRefCusProcedure("TW", "IM", "BB", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "DTS", refCusRateTypeDTY.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "HWS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTA", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeCOM.PK);
			referenceDataHelper.LoadOrCreateNewCusRateCode(Factory, "SSG", refCusRateTypeSSG.PK);
			var startDate = ZDate.Today.AddMonths(-3);
			var endDate = ZDate.Today.AddMonths(3);
			referenceDataHelper.CreateTaxOrFee("DDF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			referenceDataHelper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate);
			refAACusProcedure.Attributes.AddNew("COMPaymentMethod", "CAS");
			refAACusProcedure.Attributes.AddNew("DTYPaymentMethod", "DEF");
			refAACusProcedure.Attributes.AddNew("SSGPaymentMethod", "CAS");
			refAACusProcedure.Attributes.AddNew("TATPaymentMethod", "DEF");
			refAACusProcedure.Attributes.AddNew("TPFPaymentMethod", "CAS");
			refAACusProcedure.Attributes.AddNew("VATPaymentMethod", "DEF");
			Factory.Save();
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK);
			var tradeGroupAllCountry = universalTestHelper.LoadOrCreateTradeGroup("TW", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupNZ = universalTestHelper.LoadOrCreateTradeGroup("TW", "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroupNZ, "NZ", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			var alcoholChildtariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff.PK, atTariffType.PK, "21039090202");
			var rate = universalTestHelper.CreateRate(alcoholChildtariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "150 * [LTR]");
			universalTestHelper.CreateCusApplicability(rate, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			invoiceLine.JI_PrimaryPreference = "PR1";
			invoiceLine.JI_CountryOfOrigin = "GB";
			invoiceLine.JI_Procedure = "AA";
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLineTax.JLT_Type = "AT";
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals("DEF", invoiceLineTax.JLT_MethodOfPayment);
			invoiceLineTax.JLT_MethodOfPayment = ZString.Empty;
			invoiceLineTax.JLT_Type = "CT";
			AssertEquals(ZString.Empty, invoiceLineTax.JLT_MethodOfPayment);
			invoiceLineTax.JLT_MethodOfPayment = "CAS";
			invoiceLineTax.JLT_Type = "AT";
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals("DEF", invoiceLineTax.JLT_MethodOfPayment);
			invoiceLine.JI_Procedure = "";
			AssertNullOrEmpty(invoiceLineTax.JLT_MethodOfPayment);
			invoiceLine.JI_Procedure = "AA";
			AssertEquals("DEF", invoiceLineTax.JLT_MethodOfPayment);
			invoiceLine.JI_Procedure = "BB";
			AssertNullOrEmpty(invoiceLineTax.JLT_MethodOfPayment);
		}

		public void TestJLT_TypeDescription()
		{
			var ssTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			ssTariffType.ZZI_Description = "Specifically Selected Goods and Services Tax";
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			AssertEquals("JLT_TypeDescription", ZString.Empty, invoiceLineTax.JLT_TypeDescription);
			invoiceLineTax.JLT_Type = "SS";
			AssertEquals("JLT_TypeDescription", "Specifically Selected Goods and Services Tax", invoiceLineTax.JLT_TypeDescription);
		}

		public void TestTariffDefaultingFromType()
		{
			var hsnTariffType = universalTestHelper.CreateTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var ssTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "SS");
			var ctTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "CT");
			Factory.Save();
			var tariff = universalTestHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "87031000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			universalTestHelper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "L*", tariff);
			universalTestHelper.CreateTariffAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, "T", tariff);
			Factory.Save();
			var passengerCarChildtariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "PASSENGERCAR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(passengerCarChildtariff.PK, hsnTariffType.PK, "87031000002");
			var sedanChildTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ssTariffType.PK, "SEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(sedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			var testSedanChildTariff = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, ctTariffType.PK, "TESTSEDAN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(testSedanChildTariff.PK, hsnTariffType.PK, "87031000002");
			Factory.Save();
			invoiceLine.JI_Tariff = "87031000002";
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "SS";
			AssertEquals(ZString.Empty, invoiceLineTax.JLT_Tariff);
			invoiceLineTax.JLT_Type = "CT";
			AssertEquals("TESTSEDAN", invoiceLineTax.JLT_Tariff);
		}

		public void TestQuantityAndQuantityUQWhenUpdateTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			var tariffType = helper.CreateTariffType("TW", "TXX");
			Factory.Save();
			var tariff1 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff1.PK, "CU2", "B");
			var tariff2 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff2.PK, "CU1", "KGM");
			helper.CreateTariffUOM(tariff2.PK, "CU2", "B");
			var tariff3 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000023", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff3.PK, "CU1", "KGM");
			helper.CreateTariffUOM(tariff3.PK, "CU2", "TNE");
			var tariff_1 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_1.PK, "CU1", "A");
			var tariff_2 = helper.CreateTariff("TW", tariffType.PK, "1000000022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_2.PK, "CU1", "B");
			var tariff_3 = helper.CreateTariff("TW", tariffType.PK, "1000000023", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_3.PK, "CU1", "C");
			var tariff_4 = helper.CreateTariff("TW", tariffType.PK, "1000000024", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_4.PK, "CU1", "TNE");
			Factory.Save();
			var header = Factory.New<JobDeclaration>().Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000021";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000021";
			AssertEquals("A", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(invoiceLine.JI_CustomsQuantity, invoiceLineTax.JLT_BaseQuantity);
			invoiceLineTax.JLT_Tariff = "1000000022";
			AssertEquals("B", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(invoiceLine.JI_CustomsSecondQuantity, invoiceLineTax.JLT_BaseQuantity);
			invoiceLineTax.JLT_Tariff = "1000000023";
			AssertEquals("C", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(100M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000022";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000024";
			AssertEquals("TNE", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(invoiceLine.JI_CustomsQuantity * 0.001M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0000000023";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000024";
			AssertEquals("TNE", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(invoiceLine.JI_CustomsSecondQuantity, invoiceLineTax.JLT_BaseQuantity);
		}

		public void TestSetDefaultQuantity()
		{
			invoiceLine.JI_CustomsUnitQty = "A";
			invoiceLine.JI_CustomsSecondUnitQty = "B";
			invoiceLine.JI_CustomsQuantity = 500M;
			invoiceLine.JI_CustomsSecondQuantity = 100M;
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_BaseQuantityUQ = "A";
			invoiceLineTax.SetDefaultQuantity(EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(invoiceLine));
			AssertEquals(invoiceLine.JI_CustomsQuantity, invoiceLineTax.JLT_BaseQuantity);
			invoiceLineTax.JLT_BaseQuantityUQ = "B";
			invoiceLineTax.SetDefaultQuantity(EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(invoiceLine));
			AssertEquals(invoiceLine.JI_CustomsSecondQuantity, invoiceLineTax.JLT_BaseQuantity);
			invoiceLineTax.JLT_BaseQuantityUQ = "C";
			invoiceLineTax.SetDefaultQuantity(EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(invoiceLine));
			AssertEquals(100M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLine.JI_CustomsUnitQty = "KGM";
			invoiceLineTax.JLT_BaseQuantityUQ = "TNE";
			invoiceLineTax.SetDefaultQuantity(EntryLineUniversalRate.GetUnitOfMeasureValueListByInvoiceLine(invoiceLine));
			AssertEquals(invoiceLine.JI_CustomsQuantity * 0.001M, invoiceLineTax.JLT_BaseQuantity);
		}

		public void TestUpdateTariffAndQuantityReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("TW", "HSN");
			var tariffType = helper.CreateTariffType("TW", "TXX");
			Factory.Save();
			var tariff1 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff1.PK, "CU1", "A");
			helper.CreateTariffUOM(tariff1.PK, "CU2", "B");
			var tariff2 = helper.CreateTariff("TW", hsnTariffType.PK, "0000000022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff2.PK, "CU1", "KGM");
			var tariff_1 = helper.CreateTariff("TW", tariffType.PK, "1000000021", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_1.PK, "CU1", "A");
			var tariff_2 = helper.CreateTariff("TW", tariffType.PK, "1000000022", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_2.PK, "CU1", "A1");
			var tariff_3 = helper.CreateTariff("TW", tariffType.PK, "1000000023", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_3.PK, "CU2", "A2");
			var tariff_4 = helper.CreateTariff("TW", tariffType.PK, "1000000024", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateTariffUOM(tariff_4.PK, "CU1", "TNE");
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLine.JI_Tariff = "0000000021";
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000022";
			AssertEquals("A1", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(ZDecimal.Zero, invoiceLineTax.JLT_BaseQuantity);
			Assert(!invoiceLineTax.JLT_BaseQuantityReadOnly);
			invoiceLineTax.JLT_BaseQuantity = 6M;
			AssertEquals(6M, invoiceLineTax.JLT_BaseQuantity);
			invoiceLineTax.JLT_Tariff = "1000000021";
			AssertEquals("A", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(ZDecimal.Zero, invoiceLineTax.JLT_BaseQuantity);
			Assert(invoiceLineTax.JLT_BaseQuantityReadOnly);
			invoiceLineTax.JLT_Tariff = "1000000023";
			AssertEquals(ZString.Empty, invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(ZDecimal.Zero, invoiceLineTax.JLT_BaseQuantity);
			Assert(invoiceLineTax.JLT_BaseQuantityReadOnly);
			invoiceLine.JI_Tariff = "0000000022";
			invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLineTax.JLT_Type = "TXX";
			invoiceLineTax.JLT_Tariff = "1000000024";
			AssertEquals("TNE", invoiceLineTax.JLT_BaseQuantityUQ);
			AssertEquals(ZDecimal.Zero, invoiceLineTax.JLT_BaseQuantity);
			Assert(invoiceLineTax.JLT_BaseQuantityReadOnly);
		}

		public void TestQuantityUQReadOnly()
		{
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			Assert(invoiceLineTax.JLT_BaseQuantityUQInfo.ReadOnly);
		}

		public void TestFormattedTariffRate()
		{
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var rateCode = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, "TAT", rateType.PK);
			var tradeGroupAllCountry = universalTestHelper.LoadOrCreateTradeGroup("TW", "ALL", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupNZ = universalTestHelper.LoadOrCreateTradeGroup("TW", "NZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroupNZ, "NZ", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			var alcoholChildtariff1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff1.PK, atTariffType.PK, "21039090201");
			var rate1 = universalTestHelper.CreateRate(alcoholChildtariff1, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "0.15 * VFD");
			universalTestHelper.CreateCusApplicability(rate1, tradeGroupNZ, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var alcoholChildtariff2 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff2.PK, atTariffType.PK, "21039090202");
			var rate2 = universalTestHelper.CreateRate(alcoholChildtariff2, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "150 * [LTR]");
			universalTestHelper.CreateCusApplicability(rate2, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var alcoholChildtariff3 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff3.PK, atTariffType.PK, "21039090203");
			var rate3 = universalTestHelper.CreateRate(alcoholChildtariff3, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: "other text");
			universalTestHelper.CreateCusApplicability(rate3, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			var alcoholChildtariff4 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "REPROCESSED4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.CreateTariffRelationship(alcoholChildtariff4.PK, atTariffType.PK, "21039090204");
			var rate4 = universalTestHelper.CreateRate(alcoholChildtariff4, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: null);
			universalTestHelper.CreateCusApplicability(rate4, tradeGroupAllCountry, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			invoiceLine.JI_CountryOfOrigin = "US";
			invoiceLineTax.JLT_Type = "AT";
			invoiceLineTax.JLT_Tariff = "REPROCESSED2";
			AssertEquals("150 * [LTR]", invoiceLineTax.FormattedTariffRate);
			invoiceLineTax.JLT_Tariff = "REPROCESSED3";
			AssertEquals("other text", invoiceLineTax.FormattedTariffRate);
			invoiceLineTax.JLT_Tariff = "REPROCESSED4";
			AssertEquals("0", invoiceLineTax.FormattedTariffRate);
			invoiceLineTax.JLT_Tariff = "REPROCESSED1";
			AssertEquals("", invoiceLineTax.FormattedTariffRate);
			invoiceLine.JI_CountryOfOrigin = "NZ";
			AssertEquals("0.15 * VFD", invoiceLineTax.FormattedTariffRate);
		}

		public void TestSupportsClone()
		{
			var tax = GetNewBusinessObject();
			Assert("Should support clone", tax.SupportsClone());
		}

		[ExpectNoExceptions]
		public void TestJLT_Type_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.JLT_TypeInfo, "Type", "The types of the duties other than tariffs.");
		}

		[ExpectNoExceptions]
		public void TestJLT_Tariff_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.JLT_TariffInfo, "Tariff", "The types of the goods belonging to the duties other than tariffs.");
		}

		[ExpectNoExceptions]
		public void TestJLT_MethodOfPayment_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.JLT_MethodOfPaymentInfo, "Payment Method", "The payment method for the duties other than tariffs.");
		}

		[ExpectNoExceptions]
		public void TestJLT_BaseQuantity_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.JLT_BaseQuantityInfo, "Quantity", "The quantity of specific tax rate for the duties other than tariffs.");
		}

		[ExpectNoExceptions]
		public void TestJLT_BaseQuantityUQ_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.JLT_BaseQuantityUQInfo, "UQ", "The UQ of specific tax rate for the duties other than tariffs.");
		}

		[ExpectNoExceptions]
		public void TestFormattedTariffRate_Caption()
		{
			var invoiceLineTax = Factory.New<JobComInvoiceLineTax>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(invoiceLineTax.FormattedTariffRateInfo, "Rate", "The Ad Valorem Tax rate for the duties other than tariffs.");
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceLineTax = invoiceLine.Taxes.AddNew();
			return invoiceLineTax;
		}

		UniversalReferenceTestDataHelper universalTestHelper;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			invoiceLine = (JobComInvoiceLine)Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
		}
	}
}
