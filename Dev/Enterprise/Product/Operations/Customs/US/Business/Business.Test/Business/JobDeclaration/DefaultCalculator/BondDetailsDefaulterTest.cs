using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	class BondDetailsDefaulterTest : TestCaseWithFactory
	{
		public void TestBondDetailsForTIBChapter98()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test99038001Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = 1000m;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.US_BondType = "9";
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("BondAmount should include 99038001(1000 * 40%) and 7601103000(1000 * 70%)", 2252m, testHelper.Charpter98Job.US_BondAmount);

			testHelper.ParentLine.JI_LinePrice = 500000m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryHeader = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBCharges - MPF", 497.99m, tibCalcs.TIBCharges);
		}

		public void TestGetBondDetailsForAccountNo()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			CusBondDetailCollection bondData = new CusBondDetailCollection(importer);
			CusBondDetail oneBondData = bondData.AddNew();
			oneBondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData.PW_BondAmount = 50000m;
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);
			oneBondData.PW_BondNumber = "123456";
			oneBondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData.PW_SuretyCode = "891";
			oneBondData.PW_BondFiledPort = "3901";

			oneBondData = bondData.AddNew();
			oneBondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData.PW_BondAmount = 60000m;
			oneBondData.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			oneBondData.PW_BondNumber = "234567";
			oneBondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData.PW_SuretyCode = "123";
			oneBondData.PW_BondFiledPort = "5980";
			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.IOROrgPK = importer.PK;

			BondDetailsDefaulter defaulter = new BondDetailsDefaulter();
			AssertNull(defaulter.GetBondDetailsForAccountNo(declaration, "98"));

			CusBondDetail defaultBondData = defaulter.GetBondDetailsForAccountNo(declaration, "123456");
			AssertEquals("BondAmount", 50000m, defaultBondData.PW_BondAmount);
			AssertEquals("Bond Number", "123456", defaultBondData.PW_BondNumber);
			AssertEquals("BondType", ImporterBondTypeList.Codes.ContinuousBond, defaultBondData.PW_BondType);
			AssertEquals("Surety Code", "891", defaultBondData.PW_SuretyCode);

			defaultBondData = defaulter.GetBondDetailsForAccountNo(declaration, "234567");
			AssertEquals("BondAmount", 60000m, defaultBondData.PW_BondAmount);
			AssertEquals("Bond Number", "234567", defaultBondData.PW_BondNumber);
			AssertEquals("BondType", ImporterBondTypeList.Codes.ContinuousBond, defaultBondData.PW_BondType);
			AssertEquals("Surety Code", "123", defaultBondData.PW_SuretyCode);
		}

		public void TestBondDetailsAreDefaultedFromIOR()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			CusBondDetailCollection bondData = new CusBondDetailCollection(importer);

			CusBondDetail oneBondData1 = bondData.AddNew();
			oneBondData1.PW_ActivityCode = ActivityCodeList.Codes._1a1;
			oneBondData1.PW_BondAmount = 60000m;
			oneBondData1.PW_BondEffectiveDate = new ZDateTime(2007, 2, 1);
			oneBondData1.PW_BondNumber = "12345678901";
			oneBondData1.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			oneBondData1.PW_SuretyCode = "123";
			oneBondData1.PW_BondFiledPort = "5980";

			CusBondDetail oneBondData = bondData.AddNew();
			oneBondData.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData.PW_BondAmount = 50000m;
			oneBondData.PW_BondEffectiveDate = new ZDateTime(2007, 4, 1);
			oneBondData.PW_BondNumber = "123456";
			oneBondData.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData.PW_SuretyCode = "891";
			oneBondData.PW_BondFiledPort = "3901";

			CusBondDetail oneBondData2 = bondData.AddNew();
			oneBondData2.PW_ActivityCode = ActivityCodeList.Codes._1;
			oneBondData2.PW_BondAmount = 50000m;
			oneBondData2.PW_BondEffectiveDate = new ZDateTime(2006, 4, 1);
			oneBondData2.PW_BondExpiryDate = new ZDateTime(2007, 1, 31);
			oneBondData2.PW_BondNumber = "345678";
			oneBondData2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			oneBondData2.PW_SuretyCode = "892";
			oneBondData2.PW_BondFiledPort = "3901";

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("BondAmount Defaulted", ZDecimal.Zero, declaration.US_BondAmount);
			AssertEquals("Bond Number", "123456", declaration.US_BondProducerAccNo);
			AssertEquals("BondType", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			AssertEquals("Surety Code", "891", declaration.US_SuretyCode);

			declaration.IOROrgPK = ZGuid.Empty;
			AssertEquals("BondAmount", 0m, declaration.US_BondAmount);
			AssertEquals("Bond Number", "123456", declaration.US_BondProducerAccNo);
			AssertEquals("BondType", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			AssertEquals("Surety Code", "891", declaration.US_SuretyCode);

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 1, 1);
			declaration.IOROrgPK = importer.PK;
			AssertEquals("BondAmount Defaulted", ZDecimal.Zero, declaration.US_BondAmount);
			AssertEquals("Bond Number", "345678", declaration.US_BondProducerAccNo);
			AssertEquals("BondType", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			AssertEquals("Surety Code", "892", declaration.US_SuretyCode);

			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 2, 1);
			declaration.IOROrgPK = ZGuid.Empty;
			declaration.US_BondType = ZString.Empty;
			declaration.IOROrgPK = importer.PK;
			AssertEquals("BondAmount Defaulted", 60000m, declaration.US_BondAmount);
			AssertEquals("Bond Number", "1234567890", declaration.US_BondProducerAccNo);
			AssertEquals("BondType", BondTypeList.Codes.SingleTransactionBond, declaration.US_BondType);
			AssertEquals("Surety Code", "123", declaration.US_SuretyCode);
		}

		public void TestOverrideObsoleteBondType()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			CusBondDetailCollection bondData = new CusBondDetailCollection(importer);

			// ContinuousBond 1/1/2008..1/1/2010
			CusBondDetail bond1 = bondData.AddNew();
			bond1.PW_ActivityCode = ActivityCodeList.Codes._1;
			bond1.PW_BondAmount = 50000m;
			bond1.PW_BondEffectiveDate = new ZDateTime(2008, 1, 1);
			bond1.PW_BondExpiryDate = new ZDateTime(2010, 1, 1);
			bond1.PW_BondNumber = "123456";
			bond1.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bond1.PW_SuretyCode = "891";
			bond1.PW_BondFiledPort = "3901";

			// SingleTransactionBond 1/1/2009..1/1/2011
			CusBondDetail bond2 = bondData.AddNew();
			bond2.PW_ActivityCode = ActivityCodeList.Codes._1a1;
			bond2.PW_BondAmount = 60000m;
			bond2.PW_BondEffectiveDate = new ZDateTime(2009, 1, 1);
			bond2.PW_BondExpiryDate = new ZDateTime(2011, 1, 1);
			bond2.PW_BondNumber = "12345678901";
			bond2.PW_BondType = ImporterBondTypeList.Codes.SingleTransactionBond;
			bond2.PW_SuretyCode = "892";
			bond2.PW_BondFiledPort = "5980";

			Factory.Save();

			JobDeclaration declarationEmptyBond = Factory.New<JobDeclaration>();
			declarationEmptyBond.US_EstimatedEntryDate = new ZDateTime(2007, 1, 1);
			declarationEmptyBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationEmptyBond.US_BondType = ZString.Empty;
			declarationEmptyBond.IOROrgPK = importer.PK;

			JobDeclaration declarationNoBond = Factory.New<JobDeclaration>();
			declarationNoBond.US_EstimatedEntryDate = new ZDateTime(2007, 1, 1);
			declarationNoBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationNoBond.US_BondType = BondTypeList.Codes.NoBondRequired;
			declarationNoBond.IOROrgPK = importer.PK;

			JobDeclaration declarationContinuousBond = Factory.New<JobDeclaration>();
			declarationContinuousBond.US_EstimatedEntryDate = new ZDateTime(2007, 1, 1);
			declarationContinuousBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationContinuousBond.US_BondType = BondTypeList.Codes.ContinuousBond;
			declarationContinuousBond.IOROrgPK = importer.PK;

			JobDeclaration declarationSingleTransactionBond = Factory.New<JobDeclaration>();
			declarationSingleTransactionBond.US_EstimatedEntryDate = new ZDateTime(2007, 1, 1);
			declarationSingleTransactionBond.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationSingleTransactionBond.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declarationSingleTransactionBond.IOROrgPK = importer.PK;

			BondDetailsDefaulter defaulter = new BondDetailsDefaulter();

			// check initial setup
			AssertEquals(ZString.Empty, declarationEmptyBond.US_BondType);
			AssertEquals(BondTypeList.Codes.NoBondRequired, declarationNoBond.US_BondType);
			AssertEquals(BondTypeList.Codes.ContinuousBond, declarationContinuousBond.US_BondType);
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals(ZString.Empty, declarationEmptyBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationNoBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationContinuousBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationSingleTransactionBond.US_SuretyCode);

			// 06/2008 - default to ContinuousBond (bond1)
			declarationEmptyBond.US_EstimatedEntryDate = new ZDateTime(2008, 6, 1);
			declarationNoBond.US_EstimatedEntryDate = new ZDateTime(2008, 6, 1);
			declarationContinuousBond.US_EstimatedEntryDate = new ZDateTime(2008, 6, 1);
			declarationSingleTransactionBond.US_EstimatedEntryDate = new ZDateTime(2008, 6, 1);

			defaulter.Default(declarationEmptyBond, declarationEmptyBond.US_BondType);
			defaulter.Default(declarationNoBond, declarationNoBond.US_BondType);
			defaulter.Default(declarationContinuousBond, declarationContinuousBond.US_BondType);
			defaulter.Default(declarationSingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals(BondTypeList.Codes.ContinuousBond, declarationEmptyBond.US_BondType);
			AssertEquals(BondTypeList.Codes.NoBondRequired, declarationNoBond.US_BondType);
			AssertEquals(BondTypeList.Codes.ContinuousBond, declarationContinuousBond.US_BondType);
			AssertEquals(BondTypeList.Codes.ContinuousBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals("891", declarationEmptyBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationNoBond.US_SuretyCode);
			AssertEquals("891", declarationContinuousBond.US_SuretyCode);
			AssertEquals("891", declarationSingleTransactionBond.US_SuretyCode);

			// 06/2009 - default to ContinuousBond (bond1) or SingleTransactionBond (bond2) depending on the original type
			declarationEmptyBond.US_BondType = ZString.Empty;
			declarationNoBond.US_BondType = BondTypeList.Codes.NoBondRequired;
			declarationContinuousBond.US_BondType = BondTypeList.Codes.ContinuousBond;
			declarationSingleTransactionBond.US_BondType = BondTypeList.Codes.SingleTransactionBond;

			declarationEmptyBond.US_EstimatedEntryDate = new ZDateTime(2009, 6, 1);
			declarationNoBond.US_EstimatedEntryDate = new ZDateTime(2009, 6, 1);
			declarationContinuousBond.US_EstimatedEntryDate = new ZDateTime(2009, 6, 1);
			declarationSingleTransactionBond.US_EstimatedEntryDate = new ZDateTime(2009, 6, 1);

			defaulter.Default(declarationEmptyBond, declarationEmptyBond.US_BondType);
			defaulter.Default(declarationNoBond, declarationNoBond.US_BondType);
			defaulter.Default(declarationContinuousBond, declarationContinuousBond.US_BondType);
			defaulter.Default(declarationSingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationEmptyBond.US_BondType);
			AssertEquals(BondTypeList.Codes.NoBondRequired, declarationNoBond.US_BondType);
			AssertEquals(BondTypeList.Codes.ContinuousBond, declarationContinuousBond.US_BondType);
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals("892", declarationEmptyBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationNoBond.US_SuretyCode);
			AssertEquals("891", declarationContinuousBond.US_SuretyCode);
			AssertEquals("892", declarationSingleTransactionBond.US_SuretyCode);

			// 06/2010 - default to SingleTransactionBond (bond2)
			declarationEmptyBond.US_BondType = ZString.Empty;
			declarationNoBond.US_BondType = BondTypeList.Codes.NoBondRequired;
			declarationContinuousBond.US_BondType = BondTypeList.Codes.ContinuousBond;
			declarationSingleTransactionBond.US_BondType = BondTypeList.Codes.SingleTransactionBond;

			declarationEmptyBond.US_EstimatedEntryDate = new ZDateTime(2010, 6, 1);
			declarationNoBond.US_EstimatedEntryDate = new ZDateTime(2010, 6, 1);
			declarationContinuousBond.US_EstimatedEntryDate = new ZDateTime(2010, 6, 1);
			declarationSingleTransactionBond.US_EstimatedEntryDate = new ZDateTime(2010, 6, 1);

			defaulter.Default(declarationEmptyBond, declarationEmptyBond.US_BondType);
			defaulter.Default(declarationNoBond, declarationNoBond.US_BondType);
			defaulter.Default(declarationContinuousBond, declarationContinuousBond.US_BondType);
			defaulter.Default(declarationSingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationEmptyBond.US_BondType);
			AssertEquals(BondTypeList.Codes.NoBondRequired, declarationNoBond.US_BondType);
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationContinuousBond.US_BondType);
			AssertEquals(BondTypeList.Codes.SingleTransactionBond, declarationSingleTransactionBond.US_BondType);

			AssertEquals("892", declarationEmptyBond.US_SuretyCode);
			AssertEquals(ZString.Empty, declarationNoBond.US_SuretyCode);
			AssertEquals("892", declarationContinuousBond.US_SuretyCode);
			AssertEquals("892", declarationSingleTransactionBond.US_SuretyCode);
		}

		public void TestDetermineBondAmountForSEB()
		{
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 5750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondAmount = 50000m;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount should not be calculated for Continuous Bond", 0m, declaration.US_BondAmount);

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount should be re-calculated for SingleTransactionBond", 17250m, declaration.US_BondAmount);

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 8500m;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount set for Manual Calculation", 8500m, declaration.US_BondAmount);
		}

		public void TestDetermineBondAmountWhenTIB()
		{
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 5750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.US_TIBMotorVehicles = Enterprise.Customs.Business.YesNoList.Codes.Yes;
			declaration.US_BondAmount = 50000m;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount should be minimum 100 for SingleTransactionBond", 100m, declaration.US_BondAmount);

			declaration.US_TIBMVNonConforming = true;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount should be re-calculated for SingleTransactionBond - Customs value * 3", 17250m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcWarehouse()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 56805m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.US_SupQty1 = 6100m;
			invoiceLine1.US_SupUQ1 = "KG";
			invoiceLine1.JI_Tariff = "5555555555";
			invoiceLine1.JI_InvoiceQuantity = 350m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 350m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5555555555";
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 350m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 32750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond should be total customs value plus all applicable duties, taxes & fees", 65062m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcReWarehouse()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 56805m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.US_SupQty1 = 6100m;
			invoiceLine1.US_SupUQ1 = "KG";
			invoiceLine1.JI_Tariff = "5555555555";
			invoiceLine1.JI_InvoiceQuantity = 350m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 350m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5555555555";
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 350m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 32750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond should be total customs value plus all applicable duties, taxes & fees", 65062m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcTradeFair()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.TradeFair;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 56805m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.US_SupQty1 = 6100m;
			invoiceLine1.US_SupUQ1 = "KG";
			invoiceLine1.JI_Tariff = "5555555555";
			invoiceLine1.JI_InvoiceQuantity = 350m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 350m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5555555555";
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 350m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 32750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond should be total customs value plus all applicable duties, taxes & fees", 100m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcWarehouseFTZ()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseFTZ;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 56805m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.US_SupQty1 = 6100m;
			invoiceLine1.US_SupUQ1 = "KG";
			invoiceLine1.JI_Tariff = "5555555555";
			invoiceLine1.JI_InvoiceQuantity = 350m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 350m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5555555555";
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 350m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 32750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond should be total customs value plus all applicable duties, taxes & fees", 65062m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcExWarehouseWithADD()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570504000";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.08m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalADDCVD;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 56805m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5555555555";
			invoiceLine2.JI_InvoiceQuantity = 350m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_CustomsQuantity = 350m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_LinePrice = 32750m;
			invoiceLine2.US_ADDCaseNo = "A570504000";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond for Ex-Warehouse entry should be 2 * duty including ADD & CVD if present plus all taxes & fees", 87184m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcDefault()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 32750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5555555555";
			invoiceLine.JI_InvoiceQuantity = 350m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 350m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 32750m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Default auto calculation of Bond should be total customs value plus all applicable duties, taxes & fees", 41007m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcDefaultWithADD()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570504000";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.08m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 32750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5555555555";
			invoiceLine.JI_InvoiceQuantity = 350m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 350m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 32750m;
			invoiceLine.US_ADDCaseNo = "A570504000";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Default auto calculation of Bond should be total customs value plus all applicable duties which includes ADD (35370) & CVD, taxes & fees", 76377m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcDefaultWithCVD()
		{
			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C533839001";
			cvdCase.U5_ISOCountryCode = "IN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = cvdCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 0.33m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 4000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.US_CVDCaseNo = "C533839001";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Default auto calculation of Bond should be total customs value plus all applicable duties which includes ADD & CVD (1980), taxes & fees", 12005m, declaration.US_BondAmount);
		}

		JobDeclaration SetupDataForBondCalculation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5750m;
			invoiceLine.JI_Tariff = "2208204000";

			return declaration;
		}

		public void TestBondCalcForFDA()
		{
			var declaration = SetupDataForBondCalculation();
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
			line.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for FDA goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);
		}

		public void TestBondCalcForEPA()
		{
			var declaration = SetupDataForBondCalculation();
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
			line.US_VNEInd = OGAIndicatorList.Codes.Declared;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for VNE goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);

			line.US_VNEInd = ZString.Empty;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated should be total customs value plus all applicable duties, taxes & fees", 5776m, declaration.US_BondAmount);

			line.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for TSCA goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);

			line.US_TSCAInd = ZString.Empty;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated should be total customs value plus all applicable duties, taxes & fees", 5776m, declaration.US_BondAmount);

			line.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for PST goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);

			line.US_PSTIndicator = ZString.Empty;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated should be total customs value plus all applicable duties, taxes & fees", 5776m, declaration.US_BondAmount);

			line.US_ODSInd = OGAIndicatorList.Codes.Declared;
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for ODS goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);
		}

		public void TestBondCalcForATF()
		{
			var declaration = SetupDataForBondCalculation();
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
			line.US_ATFInd = OGAIndicatorList.Codes.Declared;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for ATF goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);
		}

		public void TestBondCalcForCPSC()
		{
			var declaration = SetupDataForBondCalculation();
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
			line.US_CPSCInd = OGAIndicatorList.Codes.Declared;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for CPSC goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);
		}

		public void TestBondCalcForAMS()
		{
			var declaration = SetupDataForBondCalculation();
			var line = declaration.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault();
			line.US_AMSInd = OGAIndicatorList.Codes.Declared;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for AMS goods should be 3 x the entered value", 17250m, declaration.US_BondAmount);
		}

		public void TestBondCalcOtherMSC_OGA()
		{
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2718.85m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported subject to Other Agency requirements that could pose a threat to public health should also calculate at 3 x the entered value", 8157m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcQuotaVisa()
		{
			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5407820090";
			invoiceLine.JI_InvoiceQuantity = 3000m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_LinePrice = 10000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802.00.80";// should be null
			invoiceLine2.JI_InvoiceQuantity = 6100m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.US_98GoodsValue = 24055m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7326908587";
			invoiceLine3.JI_InvoiceQuantity = 4500m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_LinePrice = 17000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise subject to Quota and/or Visa requirements should calculate bond amount as 3 * the entered value of goods subject to those categores PLUS total entered value and ALL duties, taxes and fees which apply for the remainder of the merchandise.", 47493m, declaration.US_BondAmount);

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.DoMerge();
			AssertEquals("Entry Type is not quota so assume quota does not apply", 28985m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcExhibition1_Duty()
		{
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported for exhibition that would be dutiable in the normal course of events requires a bond equal to the estimated duty", 698m, declaration.US_BondAmount);
		}

		public void TestBondCalcExhibition2_Min()
		{
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_Tariff = "7326908587";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported for exhibition that would not be dutiable in the normal course of events requires a bond equal to $100 or the MPF calculated, whichever is greater", 100m, declaration.US_BondAmount);
		}

		[TestDate(2011, 09, 14)]
		public void TestBondCalcExhibition3_MPF()
		{
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 10000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008040";
			invoiceLine2.US_98GoodsValue = 24055m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7326908587";
			invoiceLine3.JI_LinePrice = 17000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(21m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine.CusEntryLine).Amount);
			AssertEquals(50.5155m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true)).Amount);
			AssertEquals(35.7m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine3.CusEntryLine).Amount);

			AssertEquals("This one should be MPF value", 108m, declaration.US_BondAmount);
		}

		[TestDate(2011, 11, 07)]
		public void TestBondCalcExhibition4_MPF()
		{
			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 10000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008040";
			invoiceLine2.US_98GoodsValue = 24055m;

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7326908587";
			invoiceLine3.JI_LinePrice = 17000m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			AssertEquals(34.64m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine.CusEntryLine).Amount);
			AssertEquals(83.326520m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine2.GetEntryLineFor(CusEntryHeaderMessageTypeList.Codes.EntrySummary, true)).Amount);
			AssertEquals(58.888m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine3.CusEntryLine).Amount);

			AssertEquals("This one should be MPF value", 177m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcTIB1()
		{
			JobDeclaration declaration = CreateTIBDeclaration();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount calculated for dutiable TIB - (duty + fees * 2)", 1497m, declaration.US_BondAmount);
		}

		[TestDate(2009, 04, 07)]
		public void TestBondCalcTIB2()
		{
			JobDeclaration declaration = CreateTIBWithExemptTariffsDeclaration();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount calculated for dutiable TIB - exception tariffs (duty + fees * 110%)", 371m, declaration.US_BondAmount);
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcForTIBWithCombinedTariffTypes()
		{
			JobDeclaration declaration = CreateTIBWithBothExemptAndStandardTariffsDeclaration();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("As standard duty applies to some lines, TIB Bond Calculation will be 200% for this entry ie: 2 x (697.6 (std duties) + 312 (exception duties) + 68.29 (fees))", 2156m, declaration.US_BondAmount);
		}

		[TestDate(2017, 12, 1)]
		public void TestBondCalcForTIBWhenOthewiseFreeOfDutyGetsMin()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported under TIB that would not be dutiable in the normal course of events requires a bond equal to $100 or the MPF calculated, whichever is greater", 100m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 0m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges - minimum MPF", 25m, tibCalcs.TIBCharges);
			AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge - minimum bond charge : (greater of $100 or mpf case)", 100m, tibCalcs.TIBBondCharge);
		}

		[TestDate(2012, 01, 23)]
		public void TestTIBCalculationsForEntrySummaryPrinting()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "A570504000";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.08m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";

			invoiceLine1.JI_Tariff = "3406000000";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.US_ADDCaseNo = "A570504000";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Anti Dumping Duty should be included in bond calculations. In this case 110% x (10830 + 25 (MPF))", 11919m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 10800.00m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges", 34.64m, tibCalcs.TIBCharges);
			AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge", 11918.104m, tibCalcs.TIBBondCharge);
		}

		[TestDate(2018, 12, 20)]
		public void TestTIBCalculationsForEntrySummaryPrintingWithInvaildTariffs()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;

			var invoice = declaration.Invoices.AddNew();
			var parentLine = invoice.JobComInvoiceLines.AddNew();
			parentLine.US_SupTariff = "99038001";
			parentLine.JI_Tariff = "7601103000";
			parentLine.JI_LinePrice = 1000m;
			var childLine = invoice.JobComInvoiceLines.AddNew();
			childLine.JI_ParentID = parentLine.PK;
			childLine.US_SupTariff = "9813005010";
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertNoExceptionThrown(() => new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader));
		}

		[TestDate(2008, 08, 21)]
		public void TestBondCalcForTIBWithADD()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForImport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				USCACCase addCase = Factory.New<USCACCase>();
				addCase.U5_CaseNumber = "A570504000";
				addCase.U5_ISOCountryCode = "CN";
				addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
				addCase.U5_CaseStatusDate = ZDateTime.Today;

				var rate = addCase.CaseRates.AddNew();
				rate.U6_AdValoremRate = 1.08m;
				rate.U6_EffectiveDate = ZDateTime.Today;

				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
				declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 2718.85m;
				invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

				JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "98130020";

				invoiceLine1.JI_Tariff = "3406000000";
				invoiceLine1.JI_CustomsQuantity = 10m;
				invoiceLine1.JI_CustomsUnitQty = "KG";
				invoiceLine1.JI_LinePrice = 10000m;
				invoiceLine1.US_ADDCaseNo = "A570504000";

				declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				AssertEquals("Anti Dumping Duty should be included in bond calculations. In this case 110% x (10800 + 25 (MPF))", 11908m, declaration.US_BondAmount);

				CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
				var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
				AssertEquals("TIBADDAmount", 10800m, tibCalcs.TIBADDAmount);
				AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
				AssertEquals("TIBCharges - MPF minimum value", 25m, tibCalcs.TIBCharges);
				AssertEquals("TIBChargesDesc", "(MPF - Minimum: 25.00)", tibCalcs.TIBChargesDesc);
				AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
				AssertEquals("TIBBondCharge", 11907.50m, tibCalcs.TIBBondCharge);
			}
		}

		public void TestBondCalcForTIBWithCVD()
		{
			USCACCase addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "C570942003";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
			addCase.U5_CaseStatusDate = ZDateTime.Today;

			var rate = addCase.CaseRates.AddNew();
			rate.U6_AdValoremRate = 1.63m;
			rate.U6_EffectiveDate = ZDateTime.Today;

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 4000m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3406000000";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CountryOfOrigin = "CN";
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.US_CVDCaseNo = "C570942003";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Countervailing Duty should be included in bond calculations (in this case CVD of 9772.2 + Fees of 34.64 x 200%)", 19630m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 0m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 9780m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges", 34.64m, tibCalcs.TIBCharges);
			AssertEquals("TIBChargesDesc", "(MPF: 34.64)", tibCalcs.TIBChargesDesc);
			AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge", 19629.28m, tibCalcs.TIBBondCharge);
		}

		[TestDate(2011, 09, 14)]
		public void TestBondCalcForTIBWhenOthewiseFreeOfDutyGetsDoubleMPF()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals(21m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine).Amount);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008040";
			invoiceLine2.JI_LinePrice = 24055m;
			AssertEquals(50.5155m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine2).Amount);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7326908587";
			invoiceLine3.JI_LinePrice = 17000m;
			AssertEquals(35.7m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine3).Amount);

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported under TIB that would not be dutiable in the normal course of events requires a bond equal to $100 or double the MPF calculated, whichever is greater. This one should be double the MPF value", 215m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 0m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges", 107.22m, tibCalcs.TIBCharges);
			AssertEquals("TIBChargesDesc", "(MPF: 107.22)", tibCalcs.TIBChargesDesc);
			AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge", 214.44m, tibCalcs.TIBBondCharge);
		}

		[TestDate(2011, 11, 07)]
		public void TestBondCalcForTIBWhenOthewiseFreeOfDutyGetsDoubleMPF2()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7326908587";
			invoiceLine.JI_LinePrice = 10000m;
			AssertEquals(34.64m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine).Amount);

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "9802008040";
			invoiceLine2.JI_LinePrice = 24055m;
			AssertEquals(83.32652m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine2).Amount);

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7326908587";
			invoiceLine3.JI_LinePrice = 17000m;
			AssertEquals(58.888m, new MPFCalculator(Factory, true).CalculateFee(invoiceLine3).Amount);

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Merchandise imported under TIB that would not be dutiable in the normal course of events requires a bond equal to $100 or double the MPF calculated, whichever is greater. This one should be double the MPF value", 354m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 0m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges", 176.85m, tibCalcs.TIBCharges);
			AssertEquals("TIBChargesDesc", "(MPF: 176.85)", tibCalcs.TIBChargesDesc);
			AssertEquals("TIBDuty", 0m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge", 353.7m, tibCalcs.TIBBondCharge);
		}

		public void TestBondCalcUnconditionallyFreeMerchandise()
		{
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.UFM;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 25750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 25750m;
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_FDAQty1 = 10000;
			invoiceLine.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Bond amount calculated for Unconditionally Free Merchandise should be 10% of the total entered value", 2575m, declaration.US_BondAmount);
		}

		public void TestBondCalcMinimumAmount()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "5555555555";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddYears(1);
			tariff.UE_Column1RateAdValorem = .25m;
			tariff.UE_Column1RateSpecific = .25m;
			tariff.UE_Unit1 = "KG";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.AdValorem;

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.DEF;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 32750m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "5555555555";
			invoiceLine.JI_InvoiceQuantity = 350m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsQuantity = 350m;
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_LinePrice = 25m;

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Default auto calculation of Bond should be $100 (minimum amount)", 100m, declaration.US_BondAmount);

			declaration.US_BondCalcCode = SEBCalculationList.Codes.UFM;
			AssertEquals("UFM calculation of Bond should be $100 (minimum amount)", 100m, declaration.US_BondAmount);

			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			AssertEquals("TIB calculation of Bond should be $100 (minimum amount)", 100m, declaration.US_BondAmount);

			declaration.US_BondCalcCode = SEBCalculationList.Codes.MSC;
			AssertEquals("MSC calculation of Bond should be $100 (minimum amount)", 100m, declaration.US_BondAmount);

			declaration.US_BondAmount = 0m;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			AssertEquals("No minimum for MAN - Entries for government has no bond amount", 0m, declaration.US_BondAmount);

			declaration.US_BondCalcCode = SEBCalculationList.Codes.EXH;
			AssertEquals("EXH calculation of Bond should be $100 (minimum amount)", 100m, declaration.US_BondAmount);
		}

		[TestDate(2008, 09, 11)]
		public void TestBondCalcWhereCottonFeeApplies()
		{
			JobDeclaration declaration = CreateTIBDeclarationWhereCottonFeeApplies();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount calculated for dutiable TIB - (duty 1161.69 + fees (MPF: 50.52) * 2)", 3325m, declaration.US_BondAmount);
		}

		[TestDate(2007, 3, 29)]
		public void TestBondCalcWhereBeefFeeApplies()
		{
			JobDeclaration declaration = CreateTIBDeclarationWhereBeefFeeApplies();
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;
			declaration.DefaultBondAmountForSingleTransactionBond();
			AssertEquals("Bond amount calculated for dutiable TIB - (duty 44 + fees (MPF: 50.52 Beef: 3.78) * 2)", 197m, declaration.US_BondAmount);
		}

		public void TestBondCalcForTIBGetsAllNonHMFFees()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.TIB;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2718.85m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine2.US_SupTariff = "98130030";
			invoiceLine2.JI_InvoiceQuantity = 1000m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 24055m;
			invoiceLine2.JI_Tariff = "0201300600";
			invoiceLine2.JI_CustomsQuantity = 1000m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];

			AssertEquals("Merchandise imported under TIB requires a bond equal to $100 or the Total of All Fees calculated, whichever is greater", 429m, declaration.US_BondAmount);

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entryHeader);
			AssertEquals("TIBADDAmount", 0m, tibCalcs.TIBADDAmount);
			AssertEquals("TIBCVDAmount", 0m, tibCalcs.TIBCVDAmount);
			AssertEquals("TIBCharges", 170.44m, tibCalcs.TIBCharges);
			AssertEquals("TIBChargesDesc", "(MPF: 166.65; 053 - 053 Desc from DB: 3.79)", tibCalcs.TIBChargesDesc);
			AssertEquals("TIBDuty", 44m, tibCalcs.TIBDuty);
			AssertEquals("TIBBondCharge", 428.88m, tibCalcs.TIBBondCharge);
		}

		public void TestBondCalcForTIBWith9813TariffOnSupAdditionalTariff()
		{
			#region Setup Tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8462220050", "7", 0.044m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030125", "7", 0.1m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "98130050", "0", 0m, "");

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Netherlands;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Netherlands;
			invoiceLine.JI_FormattedTariff = "8462.22.0050";
			invoiceLine.SupTariffFormatted = "9903.01.25";
			invoiceLine.SupFormattedAdditionalTariff1 = "9813.00.50";
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var tibCalcs = new BondDetailsDefaulter().TIBCalculationsForEntrySummaryPrinting(entry);
			AssertEquals("TIBDuty", 1440m, tibCalcs.TIBDuty);
		}

		public void TestBondCaclForLowValueEntries()
		{
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			var bondDetailCollection = new CusBondDetailCollection(importer);
			var bondDetail1 = bondDetailCollection.AddNew();
			bondDetail1.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondDetail1.PW_BondAmount = 50000m;
			bondDetail1.PW_BondEffectiveDate = ZDateTime.Today.AddDays(-3);
			bondDetail1.PW_BondNumber = "123456";
			bondDetail1.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondDetail1.PW_SuretyCode = "891";
			bondDetail1.PW_BondFiledPort = "3901";

			var bondDetail2 = bondDetailCollection.AddNew();
			bondDetail2.PW_ActivityCode = ActivityCodeList.Codes._1;
			bondDetail2.PW_BondAmount = 60000m;
			bondDetail2.PW_BondEffectiveDate = ZDateTime.Today.AddDays(10);
			bondDetail2.PW_BondNumber = "234567";
			bondDetail2.PW_BondType = ImporterBondTypeList.Codes.ContinuousBond;
			bondDetail2.PW_SuretyCode = "123";
			bondDetail2.PW_BondFiledPort = "5980";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.IOROrgPK = importer.PK;
			AssertEquals(ImporterBondTypeList.Codes.ContinuousBond, declaration.US_BondType);

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			AssertEquals(BondTypeList.Codes.NoBondRequired, declaration.US_BondType);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals(ImporterBondTypeList.Codes.ContinuousBond, declaration.US_BondType);
		}

		#region Implementation

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;

			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		JobDeclaration CreateTIBDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;

			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateTIBWithExemptTariffsDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 8464m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "GB";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130050";
			invoiceLine1.JI_InvoiceQuantity = 95m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 8000m;
			invoiceLine1.JI_Tariff = "8528723600";
			invoiceLine1.JI_CustomsQuantity = 10m;
			invoiceLine1.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 5m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 464m;
			invoiceLine2.JI_Tariff = "7318154000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateTIBWithBothExemptAndStandardTariffsDeclaration()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "COMBINED";
			invoiceHeader.JZ_InvoiceAmount = 32519.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.US_SupTariff = "98130050";
			invoiceLine2.JI_InvoiceQuantity = 95m;
			invoiceLine2.JI_InvoiceUQ = "KG";
			invoiceLine2.JI_LinePrice = 8000m;
			invoiceLine2.JI_CountryOfOrigin = "GB";
			invoiceLine2.JI_Tariff = "8528723600";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_CustomsUnitQty = "NO";

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.US_SupTariff = "98130050";
			invoiceLine3.JI_InvoiceQuantity = 5m;
			invoiceLine3.JI_InvoiceUQ = "KG";
			invoiceLine3.JI_LinePrice = 464m;
			invoiceLine3.JI_CountryOfOrigin = "GB";
			invoiceLine3.JI_Tariff = "7318154000";
			invoiceLine3.JI_CustomsQuantity = 50m;
			invoiceLine3.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateTIBDeclarationWhereCottonFeeApplies()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "6206900040"; // Cotton
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateTIBDeclarationWhereBeefFeeApplies()
		{
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 1000m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "0201300600"; // Beef
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		#endregion
	}
}
