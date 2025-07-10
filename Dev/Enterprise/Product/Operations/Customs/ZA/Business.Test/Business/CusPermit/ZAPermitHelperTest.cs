using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class ZAPermitHelperTest : TestCaseWithFactory
	{
		[TestDate(2016, 6, 01)]
		public void TestRollbackPermitTransactionsForMessage()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			var transaction = permitHelper.CreatePermitLineTransaction(permit1, entryHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			ZAPermitHelper.RollbackPermitTransactionsForMessage(entryHeader, incomingMessage, outgoingMessage, logger, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			Factory.Save();
			var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, incomingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("Two transactions for message (1x outoing + 1x rollback", 2, transactions.Length);
			AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
			AssertEquals("DTI2014/7656 Rollback Value = 50m", 50m, transactions[1].CPL_TranValue);
		}

		[TestDate(2016, 6, 01)]
		public void TestRollbackPermitTransactionsForEntry()
		{
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, entryHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -40m, -40m);
			var transaction2 = permitHelper.CreatePermitLineTransaction(permit1, entryHeader, outgoingMessage2, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -60m, -60m);
			Factory.Save();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			ZAPermitHelper.RollbackPermitTransactionsForEntry(entryHeader, incomingMessage, outgoingMessage2, logger, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			Factory.Save();
			var query = permitHelper.GetPermitLineTransactionQuery(entryHeader, incomingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
			AssertEquals("Two transactions for message (1x outgoing + 1x rollback", 2, transactions.Length);
			AssertEquals("DTI2014/7656 Outoing Value = 100m", -40m, transactions[0].CPL_TranValue);
			AssertEquals("DTI2014/7656 Rollback Value = 100m", 100m, transactions[1].CPL_TranValue);
		}

		public void TestGetPermitAppIdForMessage()
		{
			AssertEquals("ACD is OU1", "OU1", ZAPermitHelper.GetPermitAppIdForMessage(incomingMessage));
		}

		public void TestMatchEntry()
		{
			var permit = new PermitRecord { PermitHeader = null, Value = 12, Quantity = 0, Procedure = "TEST" };
			var permitHeader = Factory.New<CusPermitHeader>();
			var transaction = permitHeader.CusPermitLineTransactions.AddNew();
			Assert("Does not match", !transaction.MatchEntry(entryHeader, permit));
			transaction.CPL_TransactionType = PermitTransactionTypeList.Codes.TRA;
			transaction.CPL_Reference = PermitHelper.GetPermitReferenceForEntry(entryHeader);
			transaction.CPL_Comment = PermitHelper.GetPermitComment(entryHeader, permit);
			transaction.CPL_ReferenceNumberLine = PermitHelper.GetPermitReferenceNumberLineForEntry(entryHeader);
			Assert("Matches", transaction.MatchEntry(entryHeader, permit));
		}

		[TestDate(2016, 6, 01)]
		public void TestGetPermitRecords_UnitOfMeasureNotSetupOnPermit()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", startDate, endDate);
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "NO");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "LI");
			var entryLine1 = entryHeader.MergedLines.AddNew();
			var addInfoCodes = entryLine1.AdditionalInformationCodes;
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ImportPermitControl, "DTI2014/7656");
			var invoice = entryHeader.Declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Tariff = "991001";
			Factory.Save();
			var permitRecords = ZAPermitHelper.GetPermitRecords(entryHeader);
			Assert("UnitOfMeasureNotSetupOnPermit", permitRecords.Any(x => x.ErrorMessages.Contains(ZAPermitHelper.UnitOfMeasureNotSetupOnPermit(permit1.CPH_Number))));
		}

		[TestDate(2016, 6, 01)]
		public void TestGetPermitRecords_UnitOfMeasureNotValidForTariff()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariff1 = universalReferenceDataHelper.CreateRateViewWithTariffTypeAndRateType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1, "DTY", "991001");
			Factory.Save();
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "KG");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "NO");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "LI");
			Factory.Save();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 500m;
			var addInfoCodes = entryLine1.AdditionalInformationCodes;
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ImportPermitControl, "DTI2014/7656");
			var invoice = entryHeader.Declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Tariff = "991001";
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "LA";
			invoiceLine.JI_CustomsSecondQuantity = 40m;
			invoiceLine.JI_CustomsSecondUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 60m;
			invoiceLine.JI_CustomsThirdUnitQty = "LI";
			Factory.Save();
			var permitRecords = ZAPermitHelper.GetPermitRecords(entryHeader);
			Assert("UnitOfMeasureNotValidForTariff", permitRecords.Any(x => x.ErrorMessages.Contains(ZAPermitHelper.UnitOfMeasureNotValidForTariff(permit1.CPH_Number, permit1.CPH_UnitOfMeasure, "99100100"))));
		}

		[TestDate(2016, 6, 01)]
		public void TestGetPermitRecords()
		{
			var universalReferenceDataHelper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var tariffType1P1 = universalReferenceDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			Factory.Save();
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit1 = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			var permit2 = permitHelper.CreatePermitHeader(importer.PK, "RCC1", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, ZString.Empty, 1000m, 1000m);
			var permit3 = permitHelper.CreatePermitHeader(importer.PK, "RCC2", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.VAL, PermitTypeList.Codes.RCC, PermitSubTypeList.Codes.ACO, 1000m, 1000m);
			var permit4 = permitHelper.CreatePermitHeader(importer.PK, "DTI2017/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m, "NO");
			var transaction1 = permitHelper.CreatePermitLineTransaction(permit1, entryHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			var tariff1 = universalReferenceDataHelper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "991001", startDate, endDate);
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.AdditionalUOMType, "NO");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.StatisticalUOMType, "LA");
			universalReferenceDataHelper.CreateTariffUOM(tariff1, UniversalReferenceConstants.RefCusTariffUOMTypes.ClassificationUOMType, "LI");
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 500m;
			var addInfoCodes = entryLine1.AdditionalInformationCodes;
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC1");
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "123.45");
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, "RCC2");
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.RebateCreditValue, "543.21");
			addInfoCodes.AddNew(UniversalReferenceConstants.AdditionalInformation.ImportPermitControl, "DTI2017/7656");
			var invoice = entryHeader.Declaration.Invoices.AddNew();
			invoice.JZ_ValuationDateOverride = ZDateTime.Today;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Tariff = "991001";
			invoiceLine.JI_CustomsQuantity = 20m;
			invoiceLine.JI_CustomsUnitQty = "LA";
			invoiceLine.JI_CustomsSecondQuantity = 40m;
			invoiceLine.JI_CustomsSecondUnitQty = "NO";
			invoiceLine.JI_CustomsThirdQuantity = 60m;
			invoiceLine.JI_CustomsThirdUnitQty = "LI";
			Factory.Save();
			var permitRecords = ZAPermitHelper.GetPermitRecords(entryHeader);
			AssertEquals("4 Permit Records Created", 4, permitRecords.Count);
			AssertEquals("DTI2014/7656: Transaction Value", 0m, permitRecords.First(x => x.PermitHeader.CPH_Number == "DTI2014/7656").Value);
			AssertEquals("RCC1: Transaction Value", 123.45m, permitRecords.First(x => x.PermitHeader.CPH_Number == "RCC1").Value);
			AssertEquals("RCC2: Transaction Value", 543.21m, permitRecords.First(x => x.PermitHeader.CPH_Number == "RCC2").Value);
			AssertEquals("DTI2017/7656: Transaction Value", 500m, permitRecords.First(x => x.PermitHeader.CPH_Number == "DTI2017/7656").Value);
			AssertEquals("DTI2017/7656: Transaction Quantity (NO)", 40m, permitRecords.First(x => x.PermitHeader.CPH_Number == "DTI2017/7656").Quantity);
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = Factory.NewWithValidTestData<OrgHeader>();
			supplier = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "DBN201609231234567";
			entryHeader.CH_EntryNumber = 1;
			outgoingMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_MessageNum = "OU1";
			outgoingMessage.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("202", "OU1");
			outgoingMessage2 = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			outgoingMessage2.EM_LinkedObject = entryHeader;
			outgoingMessage2.EM_MessageNum = "OU2";
			outgoingMessage2.EM_MessageText = CUSRESMessageProcessorTest.TestOutGoingMessage.Replace("\r\n", "").Replace("202", "OU2");
			incomingMessage = Factory.NewWithValidTestData<CUSRESEDIMessage>();
			incomingMessage.EM_MessageNum = "NOTUSED";
			incomingMessage.EM_MessageText = CUSRESMessageProcessorTest.GetTestMessageResNo("").Replace("UNH+1", "UNH+IN1").Replace("ACD:202", "ACD:OU1");
		}

		OrgHeader importer;
		OrgHeader supplier;
		CusEntryHeader entryHeader;
		CUSDECEDIMessage outgoingMessage;
		CUSDECEDIMessage outgoingMessage2;
		CUSRESEDIMessage incomingMessage;
	}
}
