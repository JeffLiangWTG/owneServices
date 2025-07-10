using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Messaging.CUSCAR;
using Enterprise.Customs.ZA.Business.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.DocumentEngine;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes;
using static Enterprise.Integration.Customs.ASYCUDA;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;
using InboundInterchangeProcessor = Enterprise.Customs.ZA.Business.BatchProcessor.InboundInterchangeProcessor;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CUSRESMessageProcessorTest : TestCaseWithFactory
	{
		public void TestRecalculateAsycudaHeaderRegistrationNumberAfterBillsProcessed()
		{
			var manifestHeader = Factory.New<IAsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "NVC";
			manifestHeader.AMA_MasterBill = "MAN0123";
			manifestHeader.AMA_ClusterKey = 10;
			manifestHeader.AMA_ManifestType = "ALH";
			var bill1 = GetIAsycudaBill(manifestHeader.PK, manifestHeader.AMA_ClusterKey, "HB123");
			var bill2 = GetIAsycudaBill(manifestHeader.PK, manifestHeader.AMA_ClusterKey, "HB223");
			var bill3 = GetIAsycudaBill(manifestHeader.PK, manifestHeader.AMA_ClusterKey, "HB323");
			var outgoingMessage = GetOutgoingMessage((ManifestBase.AsycudaBill)bill1, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			outgoingMessage.EM_MessageNum = "202";
			Factory.Save();
			AssertRegistryStatusResult(bill1.PK, manifestHeader.PK, "8", ZString.Empty);

			bill1.ABL_BillStatus = ZString.Empty;
			bill2.ABL_BillStatus = "6";
			AssertRegistryStatusResult(bill1.PK, manifestHeader.PK, "6", ZString.Empty);

			bill3.ABL_BillStatus = "8";
			bill1.ABL_BillStatus = ZString.Empty;
			AssertRegistryStatusResult(bill1.PK, manifestHeader.PK, "6", "6");

			bill1.ABL_BillStatus = ZString.Empty;
			bill2.ABL_BillStatus = "8";
			AssertRegistryStatusResult(bill1.PK, manifestHeader.PK, "8", "8");

			bill3.ABL_BillStatus = "8";
			bill2.ABL_BillStatus = "39";
			bill1.ABL_BillStatus = ZString.Empty;
			manifestHeader.RegistrationStatus = ZString.Empty;
			Factory.Save();
			AssertRegistryStatusResult(bill1.PK, manifestHeader.PK, "8", "MULTIPLE");
		}

		void AssertRegistryStatusResult(ZGuid billPK, ZGuid headerPK, ZString expectBillStatus, ZString expectHeaderCountryStatus)
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var message = string.Format(CUSRES_GOVGIO_Response.Replace("\r\n", ""), expectBillStatus);
			var testMessage = GetMesssageForTest(message);

			var interchangeIn = SetInterchangeForMesssage(message, testMessage);
			interchangeIn.EI_HeaderText = $@"UNB+UNOB:4+SARSCART+00626166WTG::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++{SARSEDIMessage.MessageTypeNames.CUSRES_GIO}'";
			testMessage.EM_EI = interchangeIn.PK;
			Factory.Save();

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var savedManifestBill = newFactory.Load<IAsycudaBill>(billPK);

			AssertEquals("Bill Customs Status", expectBillStatus, savedManifestBill.ABL_BillStatus);
			var savedManifestCountry = newFactory.Load<IAsycudaManifestHeader>(headerPK);
			AssertEquals("Header Customs Status", expectHeaderCountryStatus, savedManifestCountry.RegistrationStatus);
		}

		public void TestUpdateRegistrationNumber()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "OUT";
			manifestHeader.AMA_MasterBill = "MAN0123";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HB234";

			var outgoingMessage = GetOutgoingMessage(manifestHeader, "UNH+7615+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+1D81F3A1A4804F11ABCD20EA0F05BF4F+4'DTM+137:20180718:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET:GABARONE'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+27+7615'", "7615", "CHG");
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageNum = "7615";
			Factory.Save();
			AssertUpdateRegistrationNumber(manifestHeader.PK, "8", 2, "CARN01111BJ7", ZString.Empty, "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+1D81F3A1A4804F11ABCD20EA0F05BF4F'DTM+178:20180522:102'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00626166'RFF+BH:THEONE'RFF+AAS:00655953ROADMAN1'DTM+137:20180521:102'RFF+ACD:7615'RFF+AFB:CARN01111BJ7'UNT+11+1'");

			outgoingMessage = GetOutgoingMessage(manifestHeader, "UNH+7616+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+4A51B82278DA4B718435F6EFC75DB009+4'DTM+137:20180523:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'TDT+20'RFF+ABT:00505655KOM20180523007139'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+29+7616'", "7616", "CHG");
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageNum = "7616";
			Factory.Save();
			AssertUpdateRegistrationNumber(manifestHeader.PK, "6", 4, "CARN01111BJ7", "UNB+UNOB:4+SARSCART+00626166TST::AAAAAAAAAAAAAABB:CORAS2+20180718:0409+8928++CUSRES-CUSCAR++1+GWWTGTEST+1'", "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+963+4A51B82278DA4B718435F6EFC75DB009'DTM+178:20180522:102'LOC+22+ ::ZZZ'GIS+6:120:ZZZ'NAD+AG+00626166'RFF+BH:THEONE'RFF+AAS:00655953ROADMAN1'DTM+137:20180521:102'RFF+ACD:7616'ERP+6:0'ERC+0034::ZZZ'FTX+AAO+++BOL LRN / MRN ENTRY where value=?'?'?: For Transit Road freight manifests: only ENTRY LRN is mandatory: : : 'UNT+13+1'");

			var manifestHeader1 = Factory.New<ZAManifest.IAsycudaManifestHeader>();
			manifestHeader1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			Factory.Save();

			outgoingMessage = GetOutgoingMessage(manifestHeader1 as ManifestBase.AsycudaManifestHeader, "UNH+7617+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+1AC0B4F143C841C6A0032F057E14D090+4'DTM+137:20180718:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET:GABARONE'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+27+7617'", "7617", "CHG");
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageNum = "7617";
			Factory.Save();
			AssertUpdateRegistrationNumber(manifestHeader1.PK, "8", "CARN01111BJ7", "UNB+UNOB:4+SARSCART+00626166TST::AAAAAAAAAAAAAABB:CORAS2+20180718:0509+8929++CUSRES-CUSCAR++1+GWWTGTEST+1'", "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+1AC0B4F143C841C6A0032F057E14D090'DTM+178:20180522:102'LOC+22+ ::ZZZ'GIS+8:120:ZZZ'NAD+AG+00626166'RFF+BH:THEONE'RFF+AAS:00655953ROADMAN1'DTM+137:20180521:102'RFF+ACD:7617'RFF+AFB:CARN01111BJ7'UNT+11+1'");

			outgoingMessage = GetOutgoingMessage(manifestHeader1 as ManifestBase.AsycudaManifestHeader, "UNH+7618+CUSCAR:D:16A:UN:RCG001'BGM+85:::RFM+4A51B82278DA4B718435F6EFC75DB009+4'DTM+137:20180523:102'DTM+136:20180522:102'RFF+ACW:EACBF377B6804C00BA276C8B9D382844'RFF+LO:MAN0000315'NAD+RL+00655953'NAD+MS+00505655TST'NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'TDT+20++3++00655953:172:20+++:::LIC                           'LOC+35+BW'LOC+36+ZA'LOC+17+KFN'DTM+132:20180522:102'GEI+5+24:71:ZZZ'GEI+5+DB:122:ZZZ'CNI+1+00655953ROADMAN1::::20180521'RFF+BM:THEONE:1'LOC+8+ZADUR'LOC+9+BWBBK'TDT+20'RFF+ABT:00505655KOM20180523007139'NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET'GID+1+5:BAG'FTX+AAA++9+DESC'MEA+AAE+AAB+KGM:5'PCI+24+GGG'UNT+29+7618'", "7618", "CHG");
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageNum = "7618";
			Factory.Save();
			AssertUpdateRegistrationNumber(manifestHeader1.PK, "6", "CARN01111BJ7", "UNB+UNOB:4+SARSCART+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20180718:0609+8929++CUSRES-CUSCAR++1+GWWTGTEST+1'", "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+963+4A51B82278DA4B718435F6EFC75DB009'DTM+178:20180522:102'LOC+22+ ::ZZZ'GIS+6:120:ZZZ'NAD+AG+00626166'RFF+BH:THEONE'RFF+AAS:00655953ROADMAN1'DTM+137:20180521:102'RFF+ACD:7618'ERP+6:0'ERC+0034::ZZZ'FTX+AAO+++BOL LRN / MRN ENTRY where value=?'?'?: For Transit Road freight manifests: only ENTRY LRN is mandatory: : : 'UNT+13+1'");
		}

		void AssertUpdateRegistrationNumberEntryNumber(ZString expectedStatus, ZString headerText, ZString message)
		{
			var testMessage = GetMesssageForTest(message);
			var interchangeIn = SetInterchangeForMesssage(message, testMessage);
			interchangeIn.EI_HeaderText = headerText;
			AssertEquals(expectedStatus, testMessage.CUSRESHelper.EntryStatus);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
		}

		void AssertUpdateRegistrationNumber(ZGuid manifestHeaderPK, ZString expectedStatus, ZInt expectedMessageCount, ZString expectedRegistrationNumber, ZString headerText, ZString message)
		{
			AssertUpdateRegistrationNumberEntryNumber(expectedStatus, headerText, message);

			var savedManifest = Factory.Load<AsycudaManifestHeader>(manifestHeaderPK);
			AssertEquals("Message Added", expectedMessageCount, savedManifest.Messages.Count);
			AssertEquals("Customs Status", expectedStatus, savedManifest.RegistrationStatus);
			AssertEquals("Cargo Manifest Number (RFF+AFB:)", expectedRegistrationNumber, savedManifest.RegistrationNumber);
		}

		void AssertUpdateRegistrationNumber(ZGuid manifestHeaderPK, ZString expectedStatus, ZString expectedRegistrationNumber, ZString headerText, ZString message)
		{
			AssertUpdateRegistrationNumberEntryNumber(expectedStatus, headerText, message);

			var savedManifest = Factory.Load<ZAManifest.IAsycudaManifestHeader>(manifestHeaderPK);
			AssertEquals("Cargo Manifest Number (RFF+AFB:)", expectedRegistrationNumber, savedManifest.RegistrationNumber);
		}

		[TestDate(2016, 06, 01, 01, 36, 09)]
		public void TestProcessGOVGIO()
		{
			AssertProcessGateInOutRESMessage(SARSEDIMessage.MessageTypeNames.CUSRES_GOVGIO);
		}

		[TestDate(2016, 06, 01, 01, 36, 09)]
		public void TestProcessGIO()
		{
			AssertProcessGateInOutRESMessage(SARSEDIMessage.MessageTypeNames.CUSRES_GIO);
		}

		void AssertProcessGateInOutRESMessage(string applicationReference)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "OUT";
			manifestHeader.AMA_MasterBill = "MAN0123";
			var bill = manifestHeader.Bills.AddNew();
			bill.ABL_BillNumber = "HB234";
			Factory.Save();

			var outgoingMessage = GetOutgoingMessage(manifestHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			outgoingMessage.EM_MessageNum = "202";
			Factory.Save();
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var message = string.Format(CUSRES_GOVGIO_Response.Replace("\r\n", ""), "6"); //Response 6 REJECT
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));

			var interchangeIn = SetInterchangeForMesssage(message, testMessage);
			interchangeIn.EI_InterchangeNum = "00000000000000000044";
			interchangeIn.EI_HeaderText = $@"UNB+UNOB:4+SARSCART+00626166WTG::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++{applicationReference}'";
			testMessage.EM_EI = interchangeIn.PK;
			Factory.Save();

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			var allMessages = logger.LogMessages.ToString();
			AssertNotContains("Unable to find the linked job", allMessages);
			AssertContains("Linking CUSRES Message: #IN1/00000000000000000044 to job: MAN0123", allMessages);

			var newFactory = new BusinessObjectFactory();
			var savedManifest = newFactory.Load<AsycudaManifestHeader>(manifestHeader.PK);

			AssertEquals("Message Added", 2, savedManifest.Messages.Count);
			var inMessage = savedManifest.Messages.Cast<EDIMessage>().FirstOrDefault(m => m.EM_MessageNum == "IN1");
			AssertNotNull("Message is associated with Manifest", inMessage);

			AssertContains("Entry Header Message", inMessage.EM_MessageInterpretation);
			AssertContains("Line number may not be 0", inMessage.EM_MessageInterpretation);

			AssertEquals("Customs Status", ZString.Empty, savedManifest.RegistrationStatus);
			AssertEquals("Cargo Manifest Number (RFF+AFB:)", ZString.Empty, savedManifest.RegistrationNumber);

			AssertEquals("Gate In/Out Message Customs Status", "6", savedManifest.GateInOutCustomsStatus);
		}

		[TestDate(2016, 06, 01, 01, 36, 09)]
		public void TestRollbackOfPermitTransactions()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("28");
			Factory.Save();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.Address1 = "IMPORTERADDR1";
			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			outgoingMessage.EM_MessageNum = "202";
			var permitHelper = new PermitTestDataHelper(Factory);
			var permit = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			_ = permitHelper.CreatePermitLineTransaction(permit, testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();
			AssertTransactionRolledbackWhenORGRejected(outgoingMessage, permitHelper);

			outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202", "203"), "203", "CNL");
			outgoingMessage.EM_MessageNum = "203";
			_ = permitHelper.CreatePermitLineTransaction(permit, testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();
			AssertTransactionRolledbackWhenCNLAccepted(outgoingMessage, permitHelper);
		}

		void AssertTransactionRolledbackWhenORGRejected(CUSRESEDIMessageForTest outgoingMessage, PermitTestDataHelper permitHelper)
		{
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var query = permitHelper.GetPermitLineTransactionQuery(testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			AssertNotNull("Transaction exists", new BusinessObjectFactory().LoadTop1<BaseCusPermitLineTransaction>(query));

			var message = GetTestMessageResNo("6"); //Response 6 REJECT
			var testMessage = GetMesssageForTest(message);
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Transaction Rolledback when ORG is Rejected", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				query = permitHelper.GetPermitLineTransactionQuery(testHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("One transaction for message (Status DEL)", 1, transactions.Length);
				AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
				AssertEquals("Status DEL", "DEL", transactions[0].CPL_TransactionStatus);
			});
		}

		void AssertTransactionRolledbackWhenCNLAccepted(CUSRESEDIMessageForTest outgoingMessage, PermitTestDataHelper permitHelper)
		{
			AssertEquals("203", outgoingMessage.EM_MessageNum);
			var query = permitHelper.GetPermitLineTransactionQuery(testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			AssertNotNull("Transaction exists", new BusinessObjectFactory().LoadTop1<BaseCusPermitLineTransaction>(query));

			var message = GetTestMessageResNo("28").Replace("202", "203"); //Response 28 CANCELLATION ACCEPTED
			var testMessage = GetMesssageForTest(message);
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Transaction Rolledback when CNL is Accepted", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				query = permitHelper.GetPermitLineTransactionQuery(testHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("One transaction for message", 2, transactions.Length);
				AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
				AssertEquals("DTI2014/7656 Cancelled Value = 50m", 50m, transactions[1].CPL_TranValue);
				AssertEquals("Status CON", "CON", transactions[1].CPL_TransactionStatus);
			});
		}

		[TestDate(2016, 06, 01, 01, 36, 09)]
		public void TestDuplicateSubmissionDoesNotRejectEntry()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("9");

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.MainAddress.Address1 = "IMPORTERADDR1";

			var startDate = new ZDateTime(2016, 01, 01);
			var endDate = new ZDateTime(2016, 12, 31);

			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			outgoingMessage.EM_MessageNum = "202";

			var permitHelper = new PermitTestDataHelper(Factory);
			var permit = permitHelper.CreatePermitHeader(importer.PK, "DTI2014/7656", startDate.Date, endDate.Date, Customs.Business.PermitQtyValIndicatorList.Codes.BTH, PermitTypeList.Codes.IMP, ZString.Empty, 1000m, 1000m);
			_ = permitHelper.CreatePermitLineTransaction(permit, testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA, -50m, -50m);
			Factory.Save();

			AssertNoTransactionRollbackWithNoRejectedEntries(outgoingMessage, permitHelper);
			AssertTransactionRolledbackWhenCNLAccepted(permitHelper);
		}

		void AssertNoTransactionRollbackWithNoRejectedEntries(CUSRESEDIMessageForTest outgoingMessage, PermitTestDataHelper permitHelper)
		{
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var query = permitHelper.GetPermitLineTransactionQuery(testHeader, outgoingMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
			AssertNotNull("Transaction exists", new BusinessObjectFactory().LoadTop1<BaseCusPermitLineTransaction>(query));

			var message = string.Format(TestMessage_FrequentSubmission.Replace("\r\n", ""));
			var testMessage = GetMesssageForTest(message);
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions("Transaction Rollback does not occur, as the Entries are not Rejected", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				query = permitHelper.GetPermitLineTransactionQuery(testHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("No rollback transactions for message. (1x outgoing)", 1, transactions.Length);
				AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
				AssertEquals("Status should be pending", "PND", transactions[0].CPL_TransactionStatus);
			});
		}

		void AssertTransactionRolledbackWhenCNLAccepted(PermitTestDataHelper permitHelper)
		{
			logger.ClearLogs();
			var message = GetTestMessageResNo("6"); //Response 6 REJECT
			var testMessage = GetMesssageForTest(message);
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions("Transaction Rolledback when CNL is Accepted", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				var query = permitHelper.GetPermitLineTransactionQuery(testHeader, testMessage, Customs.Business.PermitTransactionCategoryList.Codes.CUM, Customs.Business.PermitTransactionTypeList.Codes.TRA);
				var transactions = Factory.Load<BaseCusPermitLineTransaction>(query);
				AssertEquals("One transaction for message", 1, transactions.Length);
				AssertEquals("DTI2014/7656 Outgoing Value = -50m", -50m, transactions[0].CPL_TranValue);
				AssertEquals("Status Deleted", "DEL", transactions[0].CPL_TransactionStatus);
			});
		}

		public void TestNotFindingParent()
		{
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", ""), "IN1");
			testMessage.EM_MessageType = "RES";
			testMessage.EM_MessageSubType = "RES";

			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			testInterchange.EI_InterchangeNum = "INT123";
			Factory.Save();

			var proc = new ZACIncomingMessageProcessor(logger);
			proc.ExecuteBatch();
			testMessage.Reload();

			AssertEquals("DCD", testMessage.EM_Status);
			AssertContains("Log", @"Information: 	Unable to find the linked job for CUSRES Message: #IN1/INT123", logger.LogMessages.ToString());
		}

		public void TestAutoCreate_And_StoreManifestWithBarcode1()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("8");
			testHelper.CreateNewOrGetExistingCusCodeType(Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "Dsc.");
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			var manifestHeader = Factory.New<ZAManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = countryCode;
			manifestHeader.AMA_ApplicationCode = "NVC";
			manifestHeader.AMA_MasterBill = "MAN0123";
			manifestHeader.AMA_TransportMode = "ROA";
			manifestHeader.AMA_ManifestType = nameof(ManifestDocumentType.RFM);

			AssertIAddEntryDocsToEDocsAttributeNotExist(countryCode, manifestHeader);
			AssertIAddEntryDocsToEDocsAttributeExist(countryCode, manifestHeader);
		}

		void AssertIAddEntryDocsToEDocsAttributeNotExist(string countryCode, ZAManifest.IAsycudaManifestHeader manifestHeader)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var testMessage = GetMesssageForTest(CUSCAR_CUSRES_CODE8.Replace("\r\n", ""), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				AssertEquals("8", testMessage.CUSRESHelper.EntryStatus);
				var testInterchange = SetInterchangeForMesssage(CUSCAR_CUSRES_CODE8, testMessage);

				var baseHeader = manifestHeader as ManifestBase.AsycudaManifestHeader;
				var bill = baseHeader.Bills.AddNew();
				bill.ABL_BillNumber = "FAKE MANIFEST 2";
				AssertEquals("Prerequisite: AsycudaBill must have a manifiest header.", manifestHeader.PK, bill.Header.PK);

				var outgoingMessage = GetOutgoingMessage(baseHeader, CUSCAR_Message.Replace("\r\n", ""), "12336", "ORG");
				outgoingMessage.EM_MessageType = "CAR";
				outgoingMessage.EM_MessageNum = "12336";
				Factory.Save();

				var processor = new ZACIncomingMessageProcessor(logger);
				processor.ExecuteBatch();

				testMessage.Reload();

				var savedManifest = Factory.Load<ZAManifest.IAsycudaManifestHeader>(manifestHeader.PK);
				AssertEquals("Customs Status", "8", savedManifest.RegistrationStatus);
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(manifestHeader.PK, testMessage.EM_LinkUniqueID);
				AssertContains("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: MAN0123", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob)));
				logger.ClearLogs();
			}
		}

		void AssertIAddEntryDocsToEDocsAttributeExist(string countryCode, ZAManifest.IAsycudaManifestHeader manifestHeader)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
				cusCodeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus;
				cusCodeList.ZZD_Code = "8";
				cusCodeList.ZZD_Description = "Dsc.";
				cusCodeList.ZZD_CountryOrGrouping = "ZA";
				cusCodeList.ZZD_StartDate = ZDateTime.Today.AddMonths(-2);
				cusCodeList.ZZD_EndDate = ZDateTime.Today.AddMonths(2);

				var testMessage = GetMesssageForTest(CUSCAR_CUSRES_CODE8.Replace("\r\n", ""), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				AssertEquals("8", testMessage.CUSRESHelper.EntryStatus);
				var testInterchange = SetInterchangeForMesssage(CUSCAR_CUSRES_CODE8, testMessage);

				var attribute = Factory.New<ZZRefCusCodeListAttributeCombined>();
				attribute.ZZE_ZZD_CodeList = cusCodeList.PK;
				attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.IAddEntryDocsToEDocs;
				Factory.Save();
				cusCodeList.Attributes.Reload(true);

				var baseHeader = manifestHeader as ManifestBase.AsycudaManifestHeader;

				var bill = baseHeader.Bills.AddNew();
				bill.ABL_BillNumber = "FAKE MANIFEST 2";

				AssertEquals("Prerequisite: AsycudaBill must have a manifiest header.", manifestHeader.PK, bill.Header.PK);

				var outgoingMessage = GetOutgoingMessage(baseHeader, CUSCAR_Message.Replace("\r\n", ""), "12336", "ORG");
				outgoingMessage.EM_MessageType = "CAR";
				outgoingMessage.EM_MessageNum = "12336";

				testMessage.EM_Status = EDIMessage.Status.Queued;
				Factory.Save();

				var processor = new ZACIncomingMessageProcessor(logger);
				processor.ExecuteBatch();

				testMessage.Reload();

				var savedManifest = Factory.Load<ZAManifest.IAsycudaManifestHeader>(manifestHeader.PK);
				AssertEquals("Customs Status", "8", savedManifest.RegistrationStatus);

				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(manifestHeader.PK, testMessage.EM_LinkUniqueID);
				AssertContains("Log-PreProcess", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: MAN0123", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertContains("Log-Process", "Information: 	Auto created eDoc:ZA Manifest with Barcode for entry:F59D642456C94D9CA50B5E293C1ABC21.", logger.LogMessages.ToString());
				AssertEquals(1, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob)));
			}
		}

		public void TestAutoCreateSADDocuments()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("38");
			Factory.Save();

			AssertAutoCreateSADDocumentsRejection();
			AssertAutoCreateSADDocumentsOriginalEXP();
			AssertAutoCreateSADDocumentsChangeEXWNoWorksheer(testHelper);
			AssertAutoCreateSADDocumentsReplaceIMP();
			AssertAutoCreateSADDocumentsOutgoingMessageWrongMessageType();
			AssertAutoCreateSADDocumentsOutgoingMessageWrongMessageSubType();
		}

		void AssertAutoCreateSADDocumentsRejection()
		{
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			Factory.Save();

			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var allEDocs = testHeader.Declaration.DocManagerInfo.AllEDocs;
			AssertEquals(0, allEDocs.Count);
			AssertEquals(0, Factory.Load(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)).Length);

			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var message = GetTestMessageResNo("6"); //Response 6 REJECT
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			testHeader.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("No documents generated for not cleared status", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(0, allEDocs.Count);
				AssertEquals(0, (testHeader as IDocManagerSupport).DocManagerInfo.AllEDocs.Count);
				AssertEquals(0, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		void AssertAutoCreateSADDocumentsOriginalEXP()
		{
			var message = GetTestMessageResNo("1"); //Response 1 RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			testHeader.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Original Import", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(3, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		void AssertAutoCreateSADDocumentsChangeEXWNoWorksheer(ZAUniversalReferenceTestDataHelper testHelper)
		{
			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "38", "ZA", ZDateTime.Now, "false");
			var message = TestOutGoingMessage.Replace("\r\n", "").Replace("00002+9", "00002+4").Replace("CCI+++11:00", "CCI+++60:00");
			testHeader.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			var outgoingMessageCHG = GetOutgoingCUSDECMessageAndSave(testHeader, message, "203", "CHG");

			AssertEquals("203", outgoingMessageCHG.EM_MessageNum);

			message = GetTestMessageResNo("38").Replace("ACD:202", "ACD:203"); //Response 38 OFFLINE RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Change Export", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 38 - Offline Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:CHG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:CHG_VOC_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(5, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		void AssertAutoCreateSADDocumentsReplaceIMP()
		{
			var message = TestOutGoingMessage.Replace("\r\n", "").Replace("00002+9", "00002+5").Replace("CCI+++11:00", "CCI+++11:40");
			testHeader.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var outgoingMessageREP = GetOutgoingCUSDECMessageAndSave(testHeader, message, "204", "REP");

			AssertEquals("204", outgoingMessageREP.EM_MessageNum);

			message = GetTestMessageResNo("38").Replace("ACD:202", "ACD:204"); //Response 38 OFFLINE RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Replace ExBond", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 38 - Offline Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:REP_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:REP_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:REP_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(8, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		void AssertAutoCreateSADDocumentsOutgoingMessageWrongMessageType()
		{
			var message = TestOutGoingMessage.Replace("\r\n", "").Replace("00002+9", "00002+5").Replace("CCI+++11:00", "CCI+++11:40");
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var outgoingMessageWithWrongMessageType = GetOutgoingCUSDECMessageAndSave(testHeader, message, "205", "REP");
			outgoingMessageWithWrongMessageType.EM_MessageType = "RES";

			AssertEquals("205", outgoingMessageWithWrongMessageType.EM_MessageNum);

			message = GetTestMessageResNo("38").Replace("ACD:202", "ACD:205"); //Response 38 OFFLINE RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			testMessage.Validation.ValidateEM_MessageNum();
			CombineAssertions("Replace ExBond", () =>
			{
				AssertEquals("DCD", testMessage.EM_Status);
				AssertEquals("This CUSRES must appear in each CusEntryHeader corresponding to LRN", testMessage.EM_LinkedObject[CusEntryHeader.Schema.CH_BGMReference], testHeader.CH_BGMReference);
				Assert(testMessage.GetWarnings().Any(x => x.Message.Contains("Warning - EDI Message: Unable to link this CUSRES back to entry. CUSRES has been linked via LRN. This message has been discarded.")));

				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Unable to find the linked job for CUSRES Message: #IN1/{0}", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(8, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		void AssertAutoCreateSADDocumentsOutgoingMessageWrongMessageSubType()
		{
			var message = TestOutGoingMessage.Replace("\r\n", "").Replace("00002+9", "00002+5").Replace("CCI+++11:00", "CCI+++11:40");
			testHeader.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var outgoingMessageWithWrongMessageSubType = GetOutgoingCUSDECMessageAndSave(testHeader, message, "206", "REP");
			outgoingMessageWithWrongMessageSubType.EM_MessageSubType = "INV";

			AssertEquals("206", outgoingMessageWithWrongMessageSubType.EM_MessageNum);

			message = GetTestMessageResNo("38").Replace("ACD:202", "ACD:206"); //Response 38 OFFLINE RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Replace ExBond", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);

				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 38 - Offline Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:INV_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:INV_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(10, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		public void TestAutoCreateSADDocumentsWithNonSystemTemplate()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var message = GetTestMessageResNo("1"); //Response 1 RELEASE
			var testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();

			var allEDocs = testHeader.Declaration.DocManagerInfo.AllEDocs;
			AssertEquals(0, allEDocs.Count);
			AssertEquals(0, Factory.Load(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)).Length);

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Original Import", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(3, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});

			var documentFilter = new DocumentZQuery(BusinessContext.EDIMessage, "Customs Declaration Response");
			documentFilter.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			var documentCommand = Factory.LoadTop1<DocumentCommand>(documentFilter);
			documentCommand.SU_IsSystemDefined = false;

			message = GetTestMessageResNo("1"); //Response 1 RELEASE
			testMessage = GetMesssageForTest(message, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			testInterchange = SetInterchangeForMesssage(message, testMessage);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Original Import", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Error: 	Unable to create eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199. Object reference not set to an instance of an object.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertEquals(0, testHeader.Declaration.DocManagerInfo.AllEDocs.Count);
				AssertEquals(5, Factory.GetDatabaseCount(typeof(DocumentEngine.Scheduler.Business.StmPrintJob), new ZQuery(StmPrintJobSchema.SP_ParentGuid, testHeader.PK)));
			});
		}

		[TestDate(2016, 10, 10, 01, 36, 09)]
		public void TestDiscardedCusresMessage()
		{
			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", "00281124JSA20200223057020");
			outgoingMessage.MessageNumForTesting = "57081";

			var cusresMessage = TestMessageForDiscardedMessage.Replace("\r\n", "");

			var testInterchange = GetZACInterchange("INT1", EDIInterchange.Direction.Receive, EDIInterchange.Status.Acknowledged);
			testInterchange.EI_HeaderText = "UNB+UNOB:4+SARSDEC+99999999WTG::N6Q3M8H1Y1Q6P0G0:WTGAS2+20200223:1315+52137++CUSRES+++SAFJNBJNBWTG'";
			testInterchange.EI_BodyText = cusresMessage;
			testInterchange.EI_FooterText = "UNZ+1+52137'";
			Factory.Save();

			var testMessage = GetMesssageForTest(cusresMessage, "IN1");
			testInterchange.ContainedMessages.Add(testMessage);
			entryHeader1.MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			Factory.Save();

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions("Mismatching Interchange Recipient Code and Agent", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertContains("Processed CUSRES Message: #IN1/INT1 and did not update any job - the first 8 characters of the Interchange Recipient: 99999999WTG do not match the Agent Code: 00626166.", logger.LogMessages.ToString());
			});
		}

		public void TestUpdateEntryStatus_WhenInvalidAssessmentDate()
		{
			// prevent WhsOrderDataObjectReader.cs creating a loose mutex during ProcessBondedWarehouseOnFactorySaved.
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var helper = new ZAWhsDataTestHelper(Factory);
			helper.SetupCustomsData();
			var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.OutwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.OutwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
			entry.Declaration.JE_MessageType = ZAJobMessageTypeList.Codes.ExBond;
			entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;
			var entryInstruction = entry.EntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;

			AssertEquals("SupportsBondedWarehousing", expected: true, entry.SupportsBondedWarehousing);
			AssertEquals("IsInwardBondedWarehousingEnabled", expected: false, entry.IsInwardBondedWarehousingEnabled);
			AssertEquals("IsOutwardBondedWarehousingEnabled", expected: true, entry.IsOutwardBondedWarehousingEnabled);

			entry.CH_BGMReference = "00626166CLP20160426000199";

			var outgoingMessage = GetOutgoingMessage(entry, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			Factory.Save();

			AssertEquals("", entry.CH_Status);
			AssertEquals("", entry.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var testMessage = GetMesssageForTest(GetTestMessageResNo("6").Replace("20160401", ""), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(entry.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("6", entry.CH_EntryStatus);
				AssertEquals("", entry.CH_Status);
				AssertEquals("", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', BZA00001230", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject == "Entry Notification: 'Code 6 - Reject To Clearer', BZA00001230");
				AssertContains("Cannot cancel Stock Release due to the following errors.", email?.Body);
			});
		}

		public void TestUpdateEntryStatus()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("2");
			testHelper.CreateCustomsStatusCusCodeEntry("3");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("9");
			testHelper.CreateCustomsStatusCusCodeEntry("34");
			testHelper.CreateCustomsOfficeCusCodeEntry("JSA");

			testHelper.CreateCusCodeType("FAC", "Facility");
			var codeSACD = testHelper.CreateZACusCodeListEntry("FAC", "02", "SACD CAPE TOWN");
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("DepotType", "Desc.", "FAC", Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("DistrictOffices", "Desc.", "FAC", Core.Constants.CountryCodes.SouthAfrica);
			codeSACD.Attributes.AddNew("DepotType", "Containerised");
			codeSACD.Attributes.AddNew("DistrictOffices", "CTN");
			Factory.Save();

			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			AssertUpdateEntryStatusNOTWithUpdate();
			AssertUpdateEntryStatusNOTWithoutUpdate();
			AssertUpdateEntryStatusCLRWithUpdate(outgoingMessage);
			AssertUpdateEntryStatusEmailNotification();
			AssertUpdateEntryStatusCLRWithoutUpdate();
			AssertUpdateEntryStatusSTDWithUpdate();
			AssertUpdateEntryStatus6FrequentSubmission();
			AssertUpdateEntryStatusNormal6();
			AssertUpdateEntryStatusUnknown(testHelper);
		}

		void AssertUpdateEntryStatusNOTWithUpdate()
		{
			var testMessage = GetMesssageForTest(GetTestMessageResNo("9"), "IN1"); //Response 9 NOT (NOTification)
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("NOT with update", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("", testHeader.CH_EntryStatus);
				AssertEquals("", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 9 - Already on Customs system', B0000100X
", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusNOTWithoutUpdate()
		{
			logger.ClearLogs();
			var testMessage = GetMesssageForTest(GetTestMessageResNo("34"), "IN1"); //Response 34 - NOT (NOTification)
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("NOT without update", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("", testHeader.CH_EntryStatus);
				AssertEquals("", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Pay Info with the 'PEN' payment status added on Customs Entry: 00626166CLP20160426000199", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusCLRWithUpdate(CUSRESEDIMessageForTest outgoingMessage)
		{
			logger.ClearLogs();
			outgoingMessage.EM_Status = EDIMessage.Status.Queued; // Change status to reuse the outgoingMessage(Discarded status after receiving 9)
			Factory.Save();

			AssertEquals("202", outgoingMessage.EM_MessageNum);
			var time = ZDateTime.Now.AddMinutes(2);
			var testMessage = GetMesssageForTest(GetTestMessageResNo("1"), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0)); //Response 1 - CLR (Release)
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = GetTestMessageResNo("1");
			interchange.EI_InterchangeNum = "I456";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("CLR with update", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("1", testHeader.CH_EntryStatus);
				AssertEquals("Release", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/I456 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info payment status updated from 'PEN' to 'CLR' on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusEmailNotification()
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "Entry Notification: 'Code 1 - Release', B0000100X");
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "email1@wisetechglobal.com", email.Recipients[0].Email);
				var bodyText = email.Body;
				Assert("Contains Shipment Type", bodyText.Contains("<td>Shipment Type</td><td>IMP</td>"));
				Assert("Contains Customs Office", bodyText.Contains("<td>Customs Office</td><td>O.R. TAMBO INT AIRPORT</td>"));
				Assert("Contains Importer", bodyText.Contains("<td>Importer</td><td>SINGAPORE MRT LTD</td>"));
				Assert("Contains Main Supplier", bodyText.Contains("<td>Main Supplier</td><td>SINGAPORE XXX LTD</td>"));
				Assert("Contains Depot", bodyText.Contains("<td>Depot</td><td>SACD CAPE TOWN</td>"));
				Assert("Contains CPC", bodyText.Contains("<td>CPC</td><td>11</td>"));
				AssertNotNull("Not null", email.Attachments.Cast<AttachmentDef>().Select(o => o.DisplayName == "Customs Notification.pdf"));
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void AssertUpdateEntryStatusCLRWithoutUpdate()
		{
			logger.ClearLogs();
			var time = ZDateTime.Now.AddMinutes(-4);
			var testMessage = GetMesssageForTest(GetTestMessageResNo("3"), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0)); //Response 3 - CLR (Release)
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = GetTestMessageResNo("3");
			interchange.EI_InterchangeNum = "I789";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("CLR without update", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("1", testHeader.CH_EntryStatus);
				AssertEquals("Release", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/I789 to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 3 - Conditional Release', B0000100X

Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusSTDWithUpdate()
		{
			logger.ClearLogs();
			var time = ZDateTime.Now.AddMinutes(8);
			var testMessage = GetMesssageForTest(GetTestMessageResNo("2"), "IN1"); //Response 2 - STD (Standard)
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = GetTestMessageResNo("2");
			interchange.EI_InterchangeNum = "I123";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("STD with update", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("2", testHeader.CH_EntryStatus);
				AssertEquals("Stop/Detain", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/I123 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '2'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 2 - Stop/Detain', B0000100X", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatus6FrequentSubmission()
		{
			logger.ClearLogs();
			var time = ZDateTime.Now.AddMinutes(8);
			var testMessage = GetMesssageForTest(TestMessage_FrequentSubmission.Replace("\r\n", ""), "IN1");
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = TestMessage_FrequentSubmission.Replace("\r\n", "");
			interchange.EI_InterchangeNum = "999";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("6 of frequent submission", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("2", testHeader.CH_EntryStatus);
				AssertEquals("Stop/Detain", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/999 to job: 00626166CLP20160426000199
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusNormal6()
		{
			logger.ClearLogs();
			var time = ZDateTime.Now.AddMinutes(8);
			var testMessage = GetMesssageForTest(TestMessage_FrequentSubmission.Replace("\r\n", "").Replace("allocated", "xxx"), "IN1");
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = TestMessage_FrequentSubmission.Replace("\r\n", "");
			interchange.EI_InterchangeNum = "888";
			testMessage.EM_EI = interchange.PK;
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Normal 6", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("6", testHeader.CH_EntryStatus);
				AssertEquals("Reject To Clearer", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/888 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryStatusUnknown(ZAUniversalReferenceTestDataHelper testHelper)
		{
			logger.ClearLogs();
			var time = ZDateTime.Now.AddMinutes(8);
			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "A", "ZA", ZDateTime.Now, "true");

			var testMessage = GetMesssageForTest(GetTestMessageResNo("A"), "IN1"); //Response A - Unknown
			var interchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			interchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
			interchange.EI_BodyText = GetTestMessageResNo("A");
			interchange.EI_InterchangeNum = "XYZ";
			testMessage.EM_EI = interchange.PK;
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			CombineAssertions("Unknown status", () =>
			{
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("A", testHeader.CH_EntryStatus);
				AssertEquals("Unknown", testHeader.EntryHeaderStatusDescription);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/XYZ to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to 'A'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code A - ', B0000100X", logger.LogMessages.ToString());
			});
		}

		public void TestUpdateEntryStatusAfterAnotherMessageReceiving9Status()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			testHelper.CreateCustomsStatusCusCodeEntry("9");
			Factory.Save();

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders.First(x => x.CustomsProcedureCode == "11");
			entry.CH_Status = "AWA";
			entry.CH_BGMReference = "00626166CLP20160426000199";
			Factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			notification.NextAnswer = true;

			messageManager.SendMessages();
			var outgoingMessage = entry.LastSentCUSDECMessage;
			messageManager.SendMessages();
			var outgoingMessage2 = entry.LastSentCUSDECMessage;
			AssertNotEquals(outgoingMessage.EM_MessageNum, outgoingMessage2.EM_MessageNum);

			var testMessage0 = GetResMsgForTestUpdateEntryStatusAfterAnotherMessageReceiving9Status("9", outgoingMessage2.EM_MessageNum);
			var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
			Factory.Save();

			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
			AssertEquals(ZAMessage.Status.Discarded, outgoingMessage2.EM_Status);
			AssertEquals(entry.PK, testMessage0.EM_LinkUniqueID);
			AssertEquals("", entry.CH_EntryStatus);

			var testMessage1 = GetResMsgForTestUpdateEntryStatusAfterAnotherMessageReceiving9Status("7", outgoingMessage.EM_MessageNum);
			testInterchange = SetInterchangeForMesssage(testMessage1.EM_MessageText, testMessage1);
			testInterchange.EI_HeaderText = $"UNB+UNOB:4+SARSDEC+00626166WTG::A6E4W2U0E7I5S7F8:WTGAS2+{ZDateTime.Now.AddDays(2):yyyyMMdd}:1345+261++CUSRES+++IVADWTG'";
			Factory.Save();

			AssertEquals(outgoingMessage.EM_MessageNum, entry.LastSentCUSDECMessage.EM_MessageNum);
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage1);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
			AssertEquals(entry.PK, testMessage1.EM_LinkUniqueID);
			AssertEquals("7", entry.CH_EntryStatus);
		}

		[TestDate(2016, 10, 10, 01, 30, 01)]
		public void TestUpdateValuationDateOverride()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = _11;
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = _11;

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_InvoiceNumber = "INV001";
			invHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceNumber = "INV002";
			invHeader2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.ResumeApportionment();

			var invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;
			invLine1.JI_Procedure = invLine1.EntryInstruction.CEI_Style + _00;
			var invLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction2.PK;
			invLine2.JI_Procedure = invLine2.EntryInstruction.CEI_Style + _00;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			AssertEquals("Valuation Date Header 1", ZDateTime.Empty, invHeader1.JZ_ValuationDateOverride);
			AssertEquals("Valuation Date Header 2", ZDateTime.Empty, invHeader2.JZ_ValuationDateOverride);

			testHeader = invHeader1.FirstEntryHeader as CusEntryHeader;
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "202", "ORG");
			Factory.Save();
			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertEquals("JZ Valuation Date Header 1", ZDateTime.Empty, invHeader1.JZ_ValuationDateOverride);
				AssertEquals("CusEntryInstruction for JZ Valuation Date Header 1", valuationDate, invHeader1.EffectiveValuationDate);
				AssertEquals("CusEntryInstruction for JZ Valuation Date Header 1", valuationDate, (invHeader1.FirstEntryHeader.EntryInstruction as CusEntryInstruction).CEI_ExchangeRateDate);
				AssertEquals("Valuation Date Header 2 should not be update", ZDateTime.Empty, invHeader2.JZ_ValuationDateOverride);
				AssertEquals("CusEntryInstruction for JZ Valuation Date Header 2", ZDateTime.Today.AddDays(-1), invHeader2.EffectiveValuationDate);
				AssertEquals("CusEntryInstruction for JZ Valuation Date Header 2", ZDateTime.Empty, (invHeader2.FirstEntryHeader.EntryInstruction as CusEntryInstruction).CEI_ExchangeRateDate);
				AssertContains("Log Pre", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: TESTHeader1", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
				AssertContains("Log Pro", $@"Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201610100130 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201610100130 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201610100130 for entry:TESTHeader1.", logger.LogMessages.ToString());
			});
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault());
		}

		[TestDate(2016, 10, 10, 01, 36, 00)]
		public void TestUpdateAssessmentDate()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			AssertEquals("Assessment Date Instruction", ZDateTime.Empty, entryInstruction1.CEI_DateForDuty);
			AssertEquals("Assessment Date Instruction", ZDateTime.Empty, entryInstruction2.CEI_DateForDuty);

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_InvoiceNumber = "INV001";
			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceNumber = "INV002";

			var invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;
			var invLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction2.PK;

			var testEntryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			testEntryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var testEntryLine1 = testEntryHeader1.AllEntryLines.AddNew();
			invLine1.JI_CL = testEntryLine1.PK;

			var testEntryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			testEntryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			var testEntryLine2 = testEntryHeader1.AllEntryLines.AddNew();
			invLine2.JI_CL = testEntryLine1.PK;
			testEntryHeader1.CH_BGMReference = "TESTHeader1";

			var outgoingMessage = GetOutgoingMessage(testEntryHeader1, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testEntryHeader1.CH_BGMReference), "202", "ORG");
			Factory.Save();
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testEntryHeader1.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			testInterchange.EI_InterchangeNum = "INT1";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertEquals("Assessment Date Instruction", new ZDateTime(2016, 04, 01), entryInstruction1.CEI_DateForDuty);
				AssertEquals("Assessment Date Instruction", ZDateTime.Empty, entryInstruction2.CEI_DateForDuty);
				AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN1/INT1 to job: TESTHeader1
Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201610100136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201610100136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201610100136 for entry:TESTHeader1.
", logger.LogMessages.ToString());
				AssertNotNull(testMessage.Logs.Find(log => log.SL_Reference == $@"Assessment Date of Entry:{testEntryHeader1.CH_BGMReference} has been updated from '' to '01-Apr-2016 00:00:00'.").FirstOrDefault());
			});
		}

		[TestDate(2016, 10, 10, 01, 36, 09)]
		public void TestUpdateAssessmentDate_UsesTimestampFromCTRL()
		{
			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			var controlMessageText = string.Format("UNH+1+CONTRL:D:3:UN:CONTRL'UCI+460+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+{0}+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'", outgoingMessage.MessageNumForTesting);
			var controlInterchange1 = GetZACInterchange("00000000000000000031", EDIInterchange.Direction.Receive, EDIInterchange.Status.Acknowledged);
			controlInterchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160401:1455+1175++CONTRL+++GWWTGTEST+1'";
			controlInterchange1.EI_BodyText = controlMessageText;
			controlInterchange1.EI_FooterText = "UNZ+1+1175'";
			Factory.Save();

			var controlMessage = GetControlMessage(entryHeader1, EDIMessage.Status.Acknowledged);
			controlMessage.EM_MessageText = controlMessageText;
			controlMessage.MessageNumForTesting = "203";
			controlInterchange1.ContainedMessages.Add(controlMessage);

			entryHeader1.MessageStatus = ZAMessageStatusList.Codes.Acknowledged;
			Factory.Save();

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", entryHeader1.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Assessment Date on Instruction 1 is CONTRL Interchange Header DateTimeOfPreparation", new ZDateTime(2016, 04, 01, 14, 55, 0), entryHeader1.EntryInstruction.CEI_DateForDuty);
				AssertEquals("Assessment Date on Instruction 2 is not set", ZDateTime.Empty, entryHeader2.EntryInstruction.CEI_DateForDuty);

				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN1/{testInterchange.EI_InterchangeNum} to job: TESTHeader1
Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201610100136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201610100136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201610100136 for entry:TESTHeader1.
", logger.LogMessages.ToString());
			});
		}

		[TestDate(2016, 04, 01, 01, 36, 09)]
		public void TestUpdateAssessmentDate_UsesTimestampFromCUSDECwhenCTRLNotAccepted()
		{
			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			var controlInterchange1 = GetZACInterchange("00000000000000000031", EDIInterchange.Direction.Receive, EDIInterchange.Status.Acknowledged);
			controlInterchange1.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160401:1455+1175++CONTRL+++GWWTGTEST+1'";
			controlInterchange1.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+460+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+997+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			controlInterchange1.EI_FooterText = "UNZ+1+1175'";
			Factory.Save();

			var controlMessage = GetControlMessage(entryHeader1, EDIMessage.Status.Failed);
			controlMessage.MessageNumForTesting = "203";
			controlInterchange1.ContainedMessages.Add(controlMessage);

			entryHeader1.MessageStatus = ZAMessageStatusList.Codes.Error;
			Factory.Save();

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", entryHeader1.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			testInterchange.EI_InterchangeNum = "INT1";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Assessment Date on Instruction 1 is Original Message send time", new ZDateTime(2016, 04, 01, 01, 36, 09), entryHeader1.EntryInstruction.CEI_DateForDuty);
				AssertEquals("Assessment Date on Instruction 2 is not set", ZDateTime.Empty, entryHeader2.EntryInstruction.CEI_DateForDuty);

				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN1/INT1 to job: TESTHeader1
Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201604010136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201604010136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201604010136 for entry:TESTHeader1.
", logger.LogMessages.ToString());
			});
		}

		[TestDate(2016, 04, 01, 01, 36, 09)]
		public void TestUpdateAssessmentDate_UsesProcessingTimeWhenNoCTRLandCUSDECtooOld()
		{
			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			Factory.Save();

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", entryHeader1.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			testInterchange.EI_InterchangeNum = "INT1";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Assessment Date on Instruction 1 is Assessment Date with processing time", new ZDateTime(2016, 04, 01, 01, 36, 09), entryHeader1.EntryInstruction.CEI_DateForDuty);
				AssertEquals("Assessment Date on Instruction 2 is not set", ZDateTime.Empty, entryHeader2.EntryInstruction.CEI_DateForDuty);
				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN1/INT1 to job: TESTHeader1
Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201604010136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201604010136 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201604010136 for entry:TESTHeader1.
", logger.LogMessages.ToString());
			});
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdateAssessmentDate_UsesAssessmentDateWhenNoCTRLandCUSDECtooOldandProcessingDateNotMatch()
		{
			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			Factory.Save();

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", entryHeader1.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			testInterchange.EI_InterchangeNum = "INT1";
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Assessment Date on Instruction 1 is Assessment Date with processing time", new ZDateTime(2016, 04, 01, 00, 00, 00), entryHeader1.EntryInstruction.CEI_DateForDuty);
				AssertEquals("Assessment Date on Instruction 2 is not set", ZDateTime.Empty, entryHeader2.EntryInstruction.CEI_DateForDuty);
				var valuationDate = new ZDateTime(2016, 03, 30);
				AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN1/INT1 to job: TESTHeader1
Information: 	Entry Status of Entry:TESTHeader1 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', {declaration.JE_DeclarationReference}

Information: 	Exchange Rate Date of entry instruction linked to entry:TESTHeader1 has been updated to '30-Mar-16 00:00:00'.
Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: TESTHeader1
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_TESTHeader1_201604020304 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_WORKSHEET_TESTHeader1_201604020304 for entry:TESTHeader1.
Information: 	Auto created eDoc:ORG_SAD500_TESTHeader1_201604020304 for entry:TESTHeader1.
", logger.LogMessages.ToString());
			});
		}

		public void TestUpdateMRNNumberAndReleaseDateOnSaving()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHeader.CH_BGMReference = "TESTHeader1";
			_ = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "202", "ORG");
			Factory.Save();

			AssertOnlyUpdateStatusMRNNumber();
			AssertUpdateReleaseDate();
		}

		public void TestConcurrencyErrorInProcessMessage()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			{
				BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
				{
					var rows = ((IBusinessObjectFactoryInternals)factory).RowFactory.GetModifiedPersistentRowsInSaveOrder();
					var changedTables = new ChangedTableNames(rows.Select(x => x.Table.TableName).Distinct().ToList());
					if (changedTables.Contains(item: ProcessTasksSchema.Constants.TableName))
					{
						var dbTaskrow = ((IBusinessObjectFactoryInternals)factory).RowFactory.GetRow(ProcessTasksSchema.Constants.TableName, processTaskPk);
						AssertEquals("DB Value in DataBase", DataRowState.Modified, dbTaskrow.RowState);
						AssertEquals("DB Value Version", expected: true, actual: dbTaskrow.HasVersion(DataRowVersion.Original));
						((CargoWise.Integration.ITransactionParticipant)((IBusinessObjectFactoryInternals)factory).RowFactory).SaveInTransaction();
					}
				});

				var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
				testHelper.CreateCustomsStatusCusCodeEntry("1");
				testHeader.CH_BGMReference = "TESTHeader1";
				_ = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "202", "ORG");
				var group = Factory.New<GlbGroup>();
				group.GG_Code = "G1";
				Factory.Save();

				var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "IN1");
				testMessage.EM_MessageType = "RES";
				var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
				Factory.Save();
				PreAndProcessMessage(logger, testMessage);
				var utcDate = ZDateTime.UtcNow.AddDays(-1).ToSmallDateTimeFloor();
				var originalTask = new BusinessObjectFactory { RefreshEnabled = true }.Load<ProcessTask>(processTaskPk);
				originalTask.P9_CompletedTimeUtc = utcDate;
				originalTask.P9_ActualDateUtc = utcDate;
				originalTask.P9_GG_AssignedGroup = group.PK;
				originalTask.P9_ScheduledDate = utcDate;
				originalTask.P9_ScheduledDateUtc = utcDate;
				originalTask.P9_SystemLastEditTimeUtc = utcDate;
				originalTask.P9_SystemLastEditUser = "~BP";
				originalTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				testHeader.CH_ExitedStatus = ProcessTaskStatusCodeList.Codes.Closed;
				testHeader.CH_PaymentMethod = PaymentMethodCodeList.Codes.VATOnly;
				Factory.ChildFactories.Add(originalTask.Factory);
				var exceptionReporter = ExceptionReporterTestListener.Instance;
				try
				{
					BusinessObjectFactory.SaveTogether(Factory, originalTask.Factory);
				}
				catch (ZDataConcurrencyException ex)
				{
					AssertEquals("~ConcurrencyError~", ex.InnerException.Message);
					AssertEquals("DB Value in DataBase", System.Data.DataRowState.Modified, ex.Row.RowState);
					AssertEquals(1, exceptionReporter.Count);
					AssertContains("Should have error reported.", "Different Factories trying to save row in Factory.SaveTogether (ProcessTasks", exceptionReporter.Last().InnerException.Message);
					exceptionReporter.Clear();
				}
			}
		}

			void AssertOnlyUpdateStatusMRNNumber()
		{
			CombineAssertions("Only update Status and MRN", () =>
			{
				AssertEquals("", testHeader.CH_EntryStatus);
				var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "IN1");
				testMessage.EM_MessageType = "RES";
				var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
				Factory.Save();
				PreAndProcessMessage(logger, testMessage);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("1", testHeader.CH_EntryStatus);
				AssertEquals("JSA201603315000938", testHeader.MovementReferenceNumber);
			});
		}

		void AssertUpdateReleaseDate()
		{
			testHeader.CH_EntryStatus = "";
			Factory.Save();

			CombineAssertions("valid Header Text, Updating ReleaseDate", () =>
			{
				AssertEquals("", testHeader.CH_EntryStatus);
				var time = ZDateTime.Now.AddMinutes(2);
				var testMessage = Factory.New<CUSRESEDIMessageForTest>();
				testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				testMessage.EM_ApplicationCode = "ZAC";
				testMessage.EM_MessageType = "RES";
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_MessageText = TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference);
				testMessage.EM_MessageNum = "IN1";
				var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
				testInterchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
				testInterchange.EI_BodyText = TestMessage.Replace("\r\n", "");
				Factory.Save();
				PreAndProcessMessage(logger, testMessage);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("1", testHeader.CH_EntryStatus);
				AssertEquals("JSA201603315000938", testHeader.MovementReferenceNumber);
				AssertEquals(new ZDateTime(time.Year, time.Month, time.Day), testHeader.CH_EntryReleaseDate);
			});
		}

		public void TestGIS6WithMRNWontUpdateMRN()
		{
			testHeader.CH_BGMReference = "TESTHeader1";
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "202", "ORG");
			Factory.Save();
			testHeader.CH_EntryStatus = "";
			Factory.Save();

			CombineAssertions("valid Header Text, Updating ReleaseDate", () =>
			{
				AssertEquals("", testHeader.CH_EntryStatus);
				var time = ZDateTime.Now.AddMinutes(2);
				var testInterchange = Factory.New<EDIInterchangeForTest>();
				testInterchange.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
				testInterchange.EI_BodyText = TestGIS6MessageWithMRNNumber.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference);

				var testMessage = Factory.New<CUSRESEDIMessageForTest>();
				testMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				testMessage.EM_ApplicationCode = "ZAC";
				testMessage.EM_MessageType = "RES";
				testMessage.EM_Status = EDIMessage.Status.Queued;
				testMessage.EM_MessageText = TestGIS6MessageWithMRNNumber.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference);
				testMessage.EM_MessageNum = "IN1";
				testMessage.EM_EI = testInterchange.PK;
				Factory.Save();
				PreAndProcessMessage(logger, testMessage);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals("6", testHeader.CH_EntryStatus);
				AssertEquals(string.Empty, testHeader.MovementReferenceNumber);
				AssertEquals(ZDateTime.Empty, testHeader.CH_EntryReleaseDate);
			});
		}

		[TestDate(2017, 11, 2, 14, 8, 1)]
		public void TestDeleteEntryPayInfo_AlreadyOnCustomsSystem()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			Env.Registry.ZACustoms.SetIsTestMode(GlbBranch.CurrentBranch, value: true);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var testInst1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			testInst1.CEI_Style = "11";
			var testInvHeader = declaration.Invoices.AddNew();
			var testInvLine1 = testInvHeader.InvoiceLines.AddNew();
			testInvLine1.JI_CEI = testInst1.PK;
			testInvLine1.JI_Procedure = testInvLine1.EntryInstruction.CEI_Style + "00";
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			var entry = declaration.CustomsEntryHeaders.First(x => x.CustomsProcedureCode == "11");
			entry.CH_Status = "AWA";
			entry.CH_BGMReference = "00626166CLP20160426000199";

			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Accounting.Registry.Business.AccountingConfigurationRegistry.Instance.CustomsDefaultToCurrentLoginDept.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			notification.NextAnswer = true;
			messageManager.SendMessages();

			var outgoingMessageNum = ((CUSDECEDIMessage)entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC, EDIMessage.Direction.Transmit)).EM_MessageNum;
			AssertEntryPayInfo(entry, outgoingMessageNum);

			var message = $@"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199::00001'
DTM+132:20160504:102'
DTM+9:20160506050220:202'
TDT+20+QF234+4'
LOC+22+JHB'
LOC+14+A2'
GIS+6:120:ZZZ'
NAD+AG+00626166'
NAD+MS+TST'
RFF+AAS:08122222222'
DTM+137:20160424:102'
RFF+ACD:{outgoingMessageNum}'
RFF+ADE:8120060127'
ERP+1:0000'
ERC+0000'
FTX+AAO+++  Message ignored as a duplicate submiss:ion within allocated timeframe    '
UNT+18+1'";

			var testMessage0 = GetMesssageForTest(message.Replace("\r\n", ""), "IN0", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(message, testMessage0);
			testInterchange.EI_InterchangeNum = "INT123";
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
			AssertEquals(entry.PK, testMessage0.EM_LinkUniqueID);
			AssertEquals(0, testHeader.EntryPayInfos.Count);
			AssertEquals(0, testHeader.EntryInstruction.CaseNumbers.Cast<CaseNumber>().Count());
			AssertMultilineASCIIEquals("Log", $@"Information: 	Linking CUSRES Message: #IN0/INT123 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '9'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', {declaration.JE_DeclarationReference}

Information: 	Entry Pay Info with the 'PEN' payment status deleted on Customs Entry: 00626166CLP20160426000199", logger.LogMessages.ToString());
		}

		static void AssertEntryPayInfo(CusEntryHeader testHeader, ZString cusdecMessageNum)
		{
			AssertEquals(4, testHeader.EntryPayInfos.Count);

			CombineAssertions(() =>
			{
				var testPayInfoDty = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "DTY");
				AssertEquals("DTY_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 01), testPayInfoDty.C9_PaymentDate);
				AssertEquals("DTY_C9_CusResReceived", expected: true, testPayInfoDty.C9_CusResReceived);
				AssertEquals("DTY_C9_RemAdvReceived", expected: false, testPayInfoDty.C9_RemAdvReceived);
				AssertEquals("DTY_C9_PaymentParty", "F", testPayInfoDty.C9_PaymentParty);
				AssertEquals("DTY_C9_PaymentAmount", 0m, testPayInfoDty.C9_PaymentAmount);
				AssertEquals("DTY_C9_IsValid", expected: true, ((ILightValidationInternals)testPayInfoDty).IsValid);
				AssertEquals("DTY_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoDty.C9_IncomingPayResponseNo);
				AssertEquals("DTY_C9_PaymentStatus", CusEntryPayTypes.Pending, testPayInfoDty.C9_PaymentStatus);

				var testPayInfoVat = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "VAT");
				AssertEquals("VAT_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 01), testPayInfoVat.C9_PaymentDate);
				AssertEquals("VAT_C9_CusResReceived", expected: true, testPayInfoVat.C9_CusResReceived);
				AssertEquals("VAT_C9_RemAdvReceived", expected: false, testPayInfoVat.C9_RemAdvReceived);
				AssertEquals("VAT_C9_PaymentParty", "F", testPayInfoVat.C9_PaymentParty);
				AssertEquals("VAT_C9_PaymentAmount", 0m, testPayInfoVat.C9_PaymentAmount);
				AssertEquals("VAT_C9_IsValid", expected: true, ((ILightValidationInternals)testPayInfoVat).IsValid);
				AssertEquals("VAT_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoVat.C9_IncomingPayResponseNo);
				AssertEquals("VAT_C9_PaymentStatus", CusEntryPayTypes.Pending, testPayInfoVat.C9_PaymentStatus);

				var testPayInfoOth = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "OTH");
				AssertEquals("OTH_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 01), testPayInfoOth.C9_PaymentDate);
				AssertEquals("OTH_C9_CusResReceived", expected: true, testPayInfoOth.C9_CusResReceived);
				AssertEquals("OTH_C9_RemAdvReceived", expected: false, testPayInfoOth.C9_RemAdvReceived);
				AssertEquals("OTH_C9_PaymentParty", "C", testPayInfoOth.C9_PaymentParty);
				AssertEquals("OTH_C9_PaymentAmount", 0m, testPayInfoOth.C9_PaymentAmount);
				AssertEquals("OTH_C9_IsValid", expected: true, ((ILightValidationInternals)testPayInfoOth).IsValid);
				AssertEquals("OTH_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoOth.C9_IncomingPayResponseNo);
				AssertEquals("OTH_C9_PaymentStatus", CusEntryPayTypes.Pending, testPayInfoOth.C9_PaymentStatus);

				var testPayInfoPen = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().First(h => h.C9_TransactionType == "PEN");
				AssertEquals("PEN_C9_PaymentDate", new ZDateTime(2017, 11, 2, 14, 8, 01), testPayInfoPen.C9_PaymentDate);
				AssertEquals("PEN_C9_CusResReceived", expected: true, testPayInfoPen.C9_CusResReceived);
				AssertEquals("PEN_C9_RemAdvReceived", expected: false, testPayInfoPen.C9_RemAdvReceived);
				AssertEquals("PEN_C9_PaymentParty", "C", testPayInfoPen.C9_PaymentParty);
				AssertEquals("PEN_C9_PaymentAmount", 0m, testPayInfoPen.C9_PaymentAmount);
				AssertEquals("PEN_C9_IsValid", expected: true, ((ILightValidationInternals)testPayInfoPen).IsValid);
				AssertEquals("PEN_C9_IncommingPayResponseNo", cusdecMessageNum, testPayInfoPen.C9_IncomingPayResponseNo);
				AssertEquals("PEN_C9_PaymentStatus", CusEntryPayTypes.Pending, testPayInfoPen.C9_PaymentStatus);
				AssertEquals("PEN_C9_TransactionType should not have one Case Number with CaseType of ePP and CY_Data of ZString.Empty", 0, testHeader.EntryInstruction.CaseNumbers.Cast<CaseNumber>().Count(x => x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments && x.CY_Type == CusCodeDataTypeList.Codes.CaseNumber && x.CY_Data == ZString.Empty));
			});
		}

		public void TestUpdateEntryPayInfo()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			testHelper.CreateCustomsStatusCusCodeEntry("26");

			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			AssertUpdateEntryPayInfoNoPostingDate();
			AssertUpdateEntryNormalAddNewPayInfoNoValue();
			AssertUpdateEntryNormalAddNewPayInfo(testHelper);
			AssertUpdateEntryNoDuplicatedPayInfoAddedForSameOutgoingCUSDEC(testHelper);
			AssertUpdateEntryPayInfoAddedForAnotherOutgoingCUSDEC(testHelper);
			AssertUpdateEntryAddingNegativePayInfoOutgoingCUSDEC(testHelper);
			AssertUpdateEntryPayInfoNoPostingDateOutgoingCUSDEC(testHelper);
			AssertUpdateEntryPayInfoEntryStatusSeven(testHelper);
		}

		void AssertUpdateEntryPayInfoNoPostingDate()
		{
			CombineAssertions("No Posting Date - skip", () =>
			{
				var testMessage0 = GetMesssageForTest(TestMessage_NoPostingDate.Replace("\r\n", ""), "IN0", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(0, testHeader.EntryPayInfos.Count);

				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN0/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryNormalAddNewPayInfoNoValue()
		{
			CombineAssertions("Normal Add New PayInfo no value", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				var interchange1 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "789");
				var testMessage1 = GetMesssageForTest(TestMessage.Replace("\r\n", ""), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage1.EM_EI = interchange1.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage1);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
				AssertEquals(testHeader.PK, testMessage1.EM_LinkUniqueID);
				AssertEquals(4, testHeader.EntryPayInfos.Count);

				var testPayInfo1 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "DTY");
				AssertEquals("1_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo1.C9_PaymentDate);
				AssertEquals("1_C9_CusResReceived", expected: true, testPayInfo1.C9_CusResReceived);
				AssertEquals("1_C9_RemAdvReceived", expected: false, testPayInfo1.C9_RemAdvReceived);
				AssertEquals("1_C9_PaymentParty", "D", testPayInfo1.C9_PaymentParty);
				AssertEquals("1_C9_PaymentAmount", 0m, testPayInfo1.C9_PaymentAmount);
				AssertEquals("1_C9_IsValid", expected: true, (testPayInfo1 as ILightValidationInternals).IsValid);
				AssertEquals("1_C9_IncommingPayResponseNo", "202", testPayInfo1.C9_IncomingPayResponseNo);
				AssertEquals("1_C9_PaymentStatus", "CLR", testPayInfo1.C9_PaymentStatus);

				var testPayInfo2 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "VAT");
				AssertEquals("2_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo2.C9_PaymentDate);
				AssertEquals("2_C9_CusResReceived", expected: true, testPayInfo2.C9_CusResReceived);
				AssertEquals("2_C9_RemAdvReceived", expected: false, testPayInfo2.C9_RemAdvReceived);
				AssertEquals("2_C9_PaymentParty", "D", testPayInfo2.C9_PaymentParty);
				AssertEquals("2_C9_PaymentAmount", 0m, testPayInfo2.C9_PaymentAmount);
				AssertEquals("2_C9_IsValid", expected: true, (testPayInfo2 as ILightValidationInternals).IsValid);
				AssertEquals("2_C9_IncommingPayResponseNo", "202", testPayInfo2.C9_IncomingPayResponseNo);
				AssertEquals("2_C9_PaymentStatus", "CLR", testPayInfo2.C9_PaymentStatus);

				var testPayInfo3 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "OTH");
				AssertEquals("3_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo3.C9_PaymentDate);
				AssertEquals("3_C9_CusResReceived", expected: true, testPayInfo3.C9_CusResReceived);
				AssertEquals("3_C9_RemAdvReceived", expected: false, testPayInfo3.C9_RemAdvReceived);
				AssertEquals("3_C9_PaymentParty", "C", testPayInfo3.C9_PaymentParty);
				AssertEquals("3_C9_PaymentAmount", 0m, testPayInfo3.C9_PaymentAmount);
				AssertEquals("3_C9_IsValid", expected: true, (testPayInfo3 as ILightValidationInternals).IsValid);
				AssertEquals("3_C9_IncommingPayResponseNo", "202", testPayInfo3.C9_IncomingPayResponseNo);
				AssertEquals("3_C9_PaymentStatus", "CLR", testPayInfo3.C9_PaymentStatus);

				var testPayInfo4 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "PEN");
				AssertEquals("4_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo4.C9_PaymentDate);
				AssertEquals("4_C9_CusResReceived", expected: true, testPayInfo4.C9_CusResReceived);
				AssertEquals("4_C9_RemAdvReceived", expected: false, testPayInfo4.C9_RemAdvReceived);
				AssertEquals("4_C9_PaymentParty", "C", testPayInfo4.C9_PaymentParty);
				AssertEquals("4_C9_PaymentAmount", 0m, testPayInfo4.C9_PaymentAmount);
				AssertEquals("4_C9_IsValid", expected: true, (testPayInfo4 as ILightValidationInternals).IsValid);
				AssertEquals("4_C9_IncommingPayResponseNo", "202", testPayInfo4.C9_IncomingPayResponseNo);
				AssertEquals("4_C9_PaymentStatus", "CLR", testPayInfo4.C9_PaymentStatus);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/789 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryNormalAddNewPayInfo(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("Normal Add New PayInfo", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
				var outgoingMessage2 = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "201'"), "201");
				outgoingMessage2.CustomsDutyNoS1P2BBefore = 1m;
				outgoingMessage2.S1P2BDutyBefore = 2m;
				outgoingMessage2.ValueAddedTaxBefore = 3m;
				outgoingMessage2.CustomsDutyNoS1P2BAfter = 2m;
				outgoingMessage2.S1P2BDutyAfter = 3m;
				outgoingMessage2.ValueAddedTaxAfter = 4m;
				testHeader.AddMessage(outgoingMessage2);
				Factory.Save();

				var interchange1 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "ABC");
				var testMessage1 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "201'"), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage1.EM_EI = interchange1.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage1);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
				AssertEquals(testHeader.PK, testMessage1.EM_LinkUniqueID);
				AssertEquals(8, testHeader.EntryPayInfos.Count);

				var testPayInfo1 = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "DTY" && h.C9_IncomingPayResponseNo == "201");
				AssertEquals("1_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo1.C9_PaymentDate);
				AssertEquals("1_C9_CusResReceived", expected: true, testPayInfo1.C9_CusResReceived);
				AssertEquals("1_C9_RemAdvReceived", expected: false, testPayInfo1.C9_RemAdvReceived);
				AssertEquals("1_C9_PaymentParty", "D", testPayInfo1.C9_PaymentParty);
				AssertEquals("1_C9_PaymentAmount", 2m, testPayInfo1.C9_PaymentAmount);
				AssertEquals("1_C9_IsValid", expected: true, (testPayInfo1 as ILightValidationInternals).IsValid);
				AssertEquals("1_C9_IncommingPayResponseNo", "201", testPayInfo1.C9_IncomingPayResponseNo);
				AssertEquals("1_C9_PaymentStatus", "CLR", testPayInfo1.C9_PaymentStatus);

				var testPayInfo2 = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "VAT" && h.C9_IncomingPayResponseNo == "201");
				AssertEquals("2_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo2.C9_PaymentDate);
				AssertEquals("2_C9_CusResReceived", expected: true, testPayInfo2.C9_CusResReceived);
				AssertEquals("2_C9_RemAdvReceived", expected: false, testPayInfo2.C9_RemAdvReceived);
				AssertEquals("2_C9_PaymentParty", "D", testPayInfo2.C9_PaymentParty);
				AssertEquals("2_C9_PaymentAmount", 1m, testPayInfo2.C9_PaymentAmount);
				AssertEquals("2_C9_IsValid", expected: true, (testPayInfo2 as ILightValidationInternals).IsValid);
				AssertEquals("2_C9_IncommingPayResponseNo", "201", testPayInfo2.C9_IncomingPayResponseNo);
				AssertEquals("2_C9_PaymentStatus", "CLR", testPayInfo2.C9_PaymentStatus);

				var testPayInfo3 = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "OTH" && h.C9_IncomingPayResponseNo == "201");
				AssertEquals("3_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo3.C9_PaymentDate);
				AssertEquals("3_C9_CusResReceived", expected: true, testPayInfo3.C9_CusResReceived);
				AssertEquals("3_C9_RemAdvReceived", expected: false, testPayInfo3.C9_RemAdvReceived);
				AssertEquals("3_C9_PaymentParty", "C", testPayInfo3.C9_PaymentParty);
				AssertEquals("3_C9_PaymentAmount", 0m, testPayInfo3.C9_PaymentAmount);
				AssertEquals("3_C9_IsValid", expected: true, (testPayInfo3 as ILightValidationInternals).IsValid);
				AssertEquals("3_C9_IncommingPayResponseNo", "201", testPayInfo3.C9_IncomingPayResponseNo);
				AssertEquals("3_C9_PaymentStatus", "CLR", testPayInfo3.C9_PaymentStatus);

				var testPayInfo4 = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "PEN" && h.C9_IncomingPayResponseNo == "201");
				AssertEquals("4_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo4.C9_PaymentDate);
				AssertEquals("4_C9_CusResReceived", expected: true, testPayInfo4.C9_CusResReceived);
				AssertEquals("4_C9_RemAdvReceived", expected: false, testPayInfo4.C9_RemAdvReceived);
				AssertEquals("4_C9_PaymentParty", "C", testPayInfo4.C9_PaymentParty);
				AssertEquals("4_C9_PaymentAmount", 0m, testPayInfo4.C9_PaymentAmount);
				AssertEquals("4_C9_IsValid", expected: true, (testPayInfo4 as ILightValidationInternals).IsValid);
				AssertEquals("4_C9_IncommingPayResponseNo", "201", testPayInfo4.C9_IncomingPayResponseNo);
				AssertEquals("4_C9_PaymentStatus", "CLR", testPayInfo4.C9_PaymentStatus);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/ABC to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryNoDuplicatedPayInfoAddedForSameOutgoingCUSDEC(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("No Duplicated Pay Info Added For same outgoing CUSDEC", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "true");
				var interchange2 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "DEF");
				var testMessage2 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "201'"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage2.EM_EI = interchange2.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage2);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage2.EM_Status);
				AssertEquals(testHeader.PK, testMessage2.EM_LinkUniqueID);
				AssertEquals(8, testHeader.EntryPayInfos.Count);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN2/DEF to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoAddedForAnotherOutgoingCUSDEC(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("Pay Info Added For another outgoing CUSDEC", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
				var outgoingMessage2 = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "203'"), "203", "ORG");
				outgoingMessage2.CustomsDutyNoS1P2BBefore = 1m;
				outgoingMessage2.S1P2BDutyBefore = 2m;
				outgoingMessage2.ValueAddedTaxBefore = 3m;
				outgoingMessage2.CustomsDutyNoS1P2BAfter = 2m;
				outgoingMessage2.S1P2BDutyAfter = 3m;
				outgoingMessage2.ValueAddedTaxAfter = 4m;
				testHeader.AddMessage(outgoingMessage2);
				Factory.Save();

				var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "GHI");
				var testMessage3 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "203'"), "IN3", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage3.EM_EI = interchange3.PK;
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage3);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
				AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);
				AssertEquals(12, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "201" && x.C9_PaymentStatus == "CLR"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "203" && x.C9_PaymentStatus == "CLR"));

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN3/GHI to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryAddingNegativePayInfoOutgoingCUSDEC(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("Adding Negative PayInfo outgoing CUSDEC", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
				var outgoingMessage2 = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "204'"), "204", "ORG");
				outgoingMessage2.CustomsDutyNoS1P2BBefore = 2m;
				outgoingMessage2.S1P2BDutyBefore = 3m;
				outgoingMessage2.ValueAddedTaxBefore = 4m;
				outgoingMessage2.CustomsDutyNoS1P2BAfter = 1m;
				outgoingMessage2.S1P2BDutyAfter = 2m;
				outgoingMessage2.ValueAddedTaxAfter = 3m;
				outgoingMessage2.ProvisionalPaymentAmountAfter = -2m;
				outgoingMessage2.PenaltyAmountAfter = -1m;
				testHeader.AddMessage(outgoingMessage2);
				Factory.Save();

				var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "JKL");
				var testMessage3 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "204'"), "IN3", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage3.EM_EI = interchange3.PK;
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage3);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
				AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);
				AssertEquals(16, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "201"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "203"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "204" && x.C9_PaymentStatus == "CLR"));

				AssertEquals(-2m, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(h => h.C9_TransactionType == "DTY" && h.C9_IncomingPayResponseNo == "204").C9_PaymentAmount);
				AssertEquals(-1m, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(h => h.C9_TransactionType == "VAT" && h.C9_IncomingPayResponseNo == "204").C9_PaymentAmount);
				AssertEquals(-2m, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(h => h.C9_TransactionType == "OTH" && h.C9_IncomingPayResponseNo == "204").C9_PaymentAmount);
				AssertEquals(-1m, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(h => h.C9_TransactionType == "PEN" && h.C9_IncomingPayResponseNo == "204").C9_PaymentAmount);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN3/JKL to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoNoPostingDateOutgoingCUSDEC(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("no posting date won't add PayInfo outgoing CUSDEC", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
				var outgoingMessage2 = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "205'"), "205", "ORG");
				outgoingMessage2.CustomsDutyNoS1P2BBefore = 1m;
				outgoingMessage2.S1P2BDutyBefore = 2m;
				outgoingMessage2.ValueAddedTaxBefore = 3m;
				outgoingMessage2.CustomsDutyNoS1P2BAfter = 2m;
				outgoingMessage2.S1P2BDutyAfter = 3m;
				outgoingMessage2.ValueAddedTaxAfter = 4m;
				testHeader.AddMessage(outgoingMessage2);
				Factory.Save();

				var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""));
				var testMessage3 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "205'").Replace("DTM+202", "DTM+999"), "IN3");
				testMessage3.EM_EI = interchange3.PK;
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage3);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
				AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);
				AssertEquals(16, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "201"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "203"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "204"));
				AssertEquals(0, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "205"));
			});
		}

		void AssertUpdateEntryPayInfoEntryStatusSeven(ZAUniversalReferenceTestDataHelper testHelper)
		{
			CombineAssertions("EntryStatus 7 will also update Entry Pay Info", () =>
			{
				var time = ZDateTime.Now.AddMinutes(2);
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
				var outgoingMessage2 = Factory.NewWithValidTestData<CUSDECEDIMessageForTest>();
				outgoingMessage2.EM_ApplicationCode = "ZAC";
				outgoingMessage2.EM_MessageType = "DEC";
				outgoingMessage2.EM_MessageSubType = "ORG";
				outgoingMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage2.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage2.EM_LinkTable = testHeader.TableName;
				outgoingMessage2.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage2.MessageNumForTesting = "206";
				outgoingMessage2.CustomsDutyNoS1P2BBefore = 1m;
				outgoingMessage2.S1P2BDutyBefore = 2m;
				outgoingMessage2.ValueAddedTaxBefore = 3m;
				outgoingMessage2.CustomsDutyNoS1P2BAfter = 2m;
				outgoingMessage2.S1P2BDutyAfter = 3m;
				outgoingMessage2.ValueAddedTaxAfter = 4m;
				testHeader.AddMessage(outgoingMessage2);
				Factory.Save();

				var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""));
				var testMessage3 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("ACD:202'", "ACD:206'").Replace("GIS+1", "GIS+7"), "IN3");
				testMessage3.EM_EI = interchange3.PK;
				testHeader.CH_EntryStatus = "";
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage3);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
				AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);
				AssertEquals(20, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "201"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "203"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "204"));
				AssertEquals(0, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "205"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "206"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "206" && x.C9_PaymentDate == new ZDateTime(2016, 03, 31)));
			});
		}

		public void TestEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLines()
		{
			var invLine22 = invHeader2.InvoiceLines.AddNew();
			invLine22.JI_CEI = invLine21.EntryInstruction.PK;
			invLine22.JI_Procedure = invLine22.EntryInstruction.CEI_Style + "00";
			invLine22.JI_NewUsed = GoodsTypeList.Codes.S;
			declaration.DoMerge();
			Factory.Save();

			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("4");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			var incomingMessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00626166CLP20160426000199:0'DTM+132:20161023:102'DTM+202:20161006:102'TDT+20+PPG160002+1+++++A:::MAC ELXL6    SOPHIE'LOC+22+DBN::ZZZ'LOC+14+08::ZZZ'GIS+4:120:ZZZ:Y'EQD+CN+TGHU8625162'NAD+AG+00626166'RFF+AAS:MAC PPGPPA16001'DTM+137:20160923:102'RFF+ABT:DBN201610065000311'DTM+137:20161006:102'RFF+ACD:202'RFF+AAV:100754706'ERP+6:0'ERC+1::ZZZ'FTX+AAO+++DETAIN FOR PORT HEALTH'ERP+2:1'ERC+1204::ZZZ'FTX+AAO+++PPGR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:2'ERC+1204::ZZZ'FTX+AAO+++PPAR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=45000.00;!Expiry Date=2016/10/:08;'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754699;!DutyType=PPA;!PPAmount=75000.00;!Expiry Date=2016/10/:08;'ERP+2:3'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754699;!DutyType=PPA;!PPAmount=1101.00;!Expiry Date=2016/10/:08;'TAX+3+CUS:107:ZZZ'MOA+161:200000'CNT+7:10000.00'CNT+11:1'UNT+36+1'";
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			AssertEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesWontAddNewPayInfo(incomingMessageText);
			AssertTestEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesAddNew(incomingMessageText);
			AssertTestEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesUpdateExisting(incomingMessageText);
		}

		void AssertEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesWontAddNewPayInfo(ZString incomingMessageText)
		{
			CombineAssertions("GIS+6 won't add new PayInfo", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("GIS+4", "GIS+6"), "IN0", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(0, testHeader.EntryPayInfos.Count);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN0/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertTestEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesAddNew(ZString incomingMessageText)
		{
			CombineAssertions("adding new", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals("Should be 7", 7, testHeader.EntryPayInfos.Count);
				AssertEquals("Should be 4", 4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertEquals("Should be one provisional payment with amount of 45000", 1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 45000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696"));
				AssertEquals("Should be one provisional payment with amount of 75000", 1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 75000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertEquals("Should be one provisional payment with amount of 1101", 1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 1101 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertEquals("Should be one Case Number with CaseType of ePP and CY_Data of 100754696", 1, testHeader.EntryInstruction.CaseNumbers.Cast<CaseNumber>().Count(x => x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments && x.CY_Type == CusCodeDataTypeList.Codes.CaseNumber && x.CY_Data == "100754696"));
				AssertEquals("Should be one Case Number with CaseType of ePP and CY_Data of 100754699", 1, testHeader.EntryInstruction.CaseNumbers.Cast<CaseNumber>().Count(x => x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments && x.CY_Type == CusCodeDataTypeList.Codes.CaseNumber && x.CY_Data == "100754699"));
				AssertEquals("Should be 0 Case Number with CaseType of ePP and CY_Data of ZString.Empty", 0, testHeader.EntryInstruction.CaseNumbers.Cast<CaseNumber>().Count(x => x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments && x.CY_Type == CusCodeDataTypeList.Codes.CaseNumber && x.CY_Data == ZString.Empty));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPG' with Reference Number '100754696'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754699'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 3 of type 'PPA' with Reference Number '100754699'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertTestEntryPayInfosFromIncomingMessageWhenSameIncomingResponseNoIsUsedAcrossEntryLinesUpdateExisting(ZString incomingMessageText)
		{
			CombineAssertions("updating existing", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("45000", "55000").Replace("75000", "65000").Replace("1101", "1801").Replace("100754699", "100754698").Replace("2016/10", "2016/11"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(9, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 75000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 1101 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 55000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 65000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754698"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 1801 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754698"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754698'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 3 of type 'PPA' with Reference Number '100754698'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		public void TestUpdateEntryPayInfoFromIncomingMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("4");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			Factory.Save();

			var incomingMessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00626166CLP20160426000199:0'DTM+132:20161023:102'DTM+202:20161006:102'TDT+20+PPG160002+1+++++A:::MAC ELXL6    SOPHIE'LOC+22+DBN::ZZZ'LOC+14+08::ZZZ'GIS+4:120:ZZZ:Y'EQD+CN+TGHU8625162'NAD+AG+00626166'RFF+AAS:MAC PPGPPA16001'DTM+137:20160923:102'RFF+ABT:DBN201610065000311'DTM+137:20161006:102'RFF+ACD:202'RFF+AAV:100754706'ERP+6:0'ERC+1::ZZZ'FTX+AAO+++DETAIN FOR PORT HEALTH'ERP+2:1'ERC+1204::ZZZ'FTX+AAO+++PPGR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:2'ERC+1204::ZZZ'FTX+AAO+++PPAR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=45000.00;!Expiry Date=2016/10/:08;'ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754699;!DutyType=PPA;!PPAmount=75000.00;!Expiry Date=2016/10/:08;'TAX+3+CUS:107:ZZZ'MOA+161:200000'CNT+7:10000.00'CNT+11:1'UNT+36+1'";
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			AssertUpdateEntryPayInfoFromIncomingMessageWontAddNewPayInfo(incomingMessageText);
			AssertUpdateEntryPayInfoFromIncomingMessageAddNew(incomingMessageText);
			AssertUpdateEntryPayInfoFromIncomingMessageUpdateExisting(incomingMessageText);
		}

		void AssertUpdateEntryPayInfoFromIncomingMessageWontAddNewPayInfo(ZString incomingMessageText)
		{
			CombineAssertions("GIS+6 won't add new PayInfo", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("GIS+4", "GIS+6"), "IN0", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(0, testHeader.EntryPayInfos.Count);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN0/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoFromIncomingMessageAddNew(ZString incomingMessageText)
		{
			CombineAssertions("adding new", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(6, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 45000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 75000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPG' with Reference Number '100754696'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754699'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoFromIncomingMessageUpdateExisting(ZString incomingMessageText)
		{
			CombineAssertions("updating existing", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("45000", "55000").Replace("75000", "65000").Replace("100754699", "100754698").Replace("2016/10", "2016/11"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 75000 && x.C9_PaymentDate == new ZDateTime(2016, 10, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 55000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 65000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754698"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754698'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		public void TestUpdateEntryPayInfoStatusFromIncomingMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCustomsStatusCusCodeEntry("4");
			testHelper.CreateCustomsStatusCusCodeEntry("49");

			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, "Desc.", "CSTA", Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IsLiquidatedStatus, "Desc.", "CSTA", Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IUpdateProvPayLiquidationDate, "Desc.", "CSTA", Core.Constants.CountryCodes.SouthAfrica);

			var codeXX = testHelper.CreateZACusCodeListEntry("CSTA", "XX");
			codeXX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, "");
			codeXX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp, "true");
			var codeYY = testHelper.CreateZACusCodeListEntry("CSTA", "YY");
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, "");
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IsLiquidatedStatus, "");
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp, "true");
			var code45 = testHelper.CreateZACusCodeListEntry("CSTA", "45");
			code45.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvPayLiquidationDate, "");
			var code49 = testHelper.CreateZACusCodeListEntry("CSTA", "49");
			code49.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvPayLiquidationDate, "");
			Factory.Save();

			var incomingMessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00626166CLP20160426000199:0'DTM+132:20161023:102'DTM+202:20161006:102'TDT+20+PPG160002+1+++++A:::MAC ELXL6    SOPHIE'LOC+22+DBN::ZZZ'LOC+14+08::ZZZ'GIS+{STATUSCODE}:120:ZZZ:Y'EQD+CN+TGHU8625162'NAD+AG+00626166'RFF+AAS:MAC PPGPPA16001'DTM+137:20160923:102'RFF+ABT:DBN201610065000311'DTM+137:20161006:102'RFF+ACD:202'RFF+AAV:100754706'ERP+6:0'ERC+1::ZZZ'FTX+AAO+++DETAIN FOR PORT HEALTH'ERP+2:1'ERC+1204::ZZZ'FTX+AAO+++PPGR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:2'ERC+1204::ZZZ'FTX+AAO+++PPAR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'{PPINFOBODY}TAX+3+CUS:107:ZZZ'MOA+161:200000'CNT+7:10000.00'CNT+11:1'UNT+36+1'";
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			AssertUpdateEntryPayInfoStatusFromIncomingMessageWontAddNewPayInfoButCreateAddInfo(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageUpdatingExisting(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageLine2HeaderWontUpdateEntryInstruction(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderWontUpdateEntryInstructionIfTypeMismatch(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderUpdateEntryInstruction(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderLiquidatedStatusClearEntryInstruction(incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageUpdateWhereStatusCode("49", incomingMessageText);
			AssertUpdateEntryPayInfoStatusFromIncomingMessageUpdateWhereStatusCode("45", incomingMessageText);
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageWontAddNewPayInfoButCreateAddInfo(ZString incomingMessageText)
		{
			CombineAssertions("GIS+4 won't add new PayInfo, but will create AddInfo", () =>
			{
				var ppInfoBody = new ZString[] {
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=45000.00;!Expiry Date=2016/10/:08;'",
					"ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754699;!DutyType=PPA;!PPAmount=75000.00;!Expiry Date=2016/10/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "4").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(6, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPG' with Reference Number '100754696'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754699'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageUpdatingExisting(ZString incomingMessageText)
		{
			CombineAssertions("updating existing", () =>
			{
				var ppInfoBody = new ZString[] {
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=55000.00;!Expiry Date=2016/11/:08;'",
					"ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754698;!DutyType=PPA;!PPAmount=65000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "XX").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPA' with Reference Number '100754698'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageLine2HeaderWontUpdateEntryInstruction(ZString incomingMessageText)
		{
			CombineAssertions("line2 Header Level Type won't update EntryInstruction Value", () =>
			{
				testHeader.EntryInstruction.CEI_ProvisionalPaymentType = "PPE";
				testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount = 999m;
				var ppInfoBody = new ZString[] {
					"ERP+2:2'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754680;!DutyType=PPE;!PPAmount=45000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "XX").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN3", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(8, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals("PPE", testHeader.EntryInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(999m, testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN3/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 2 of type 'PPE' with Reference Number '100754680'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderWontUpdateEntryInstructionIfTypeMismatch(ZString incomingMessageText)
		{
			CombineAssertions("line1 Header Level Type won't update EntryInstruction Value if Type MisMatch", () =>
			{
				testHeader.EntryInstruction.CEI_ProvisionalPaymentType = "PPE";
				testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount = 999m;
				var ppInfoBody = new ZString[] {
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754680;!DutyType=PPU;!PPAmount=35000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "XX").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN4", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(9, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals("PPE", testHeader.EntryInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(999m, testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN4/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPU' with Reference Number '100754680'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderUpdateEntryInstruction(ZString incomingMessageText)
		{
			CombineAssertions("line1 Header Level Type update EntryInstruction Value", () =>
			{
				testHeader.EntryInstruction.CEI_ProvisionalPaymentType = "PPG";
				testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount = 999m;
				var ppInfoBody = new ZString[] {
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=25000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "XX").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN4", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(9, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals("PPG", testHeader.EntryInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(25000m, testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN4/{0} to job: 00626166CLP20160426000199
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been updated'.
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageLine1HeaderLiquidatedStatusClearEntryInstruction(ZString incomingMessageText)
		{
			CombineAssertions("line1 Header Level Type withliquidated status clear EntryInstruction Value", () =>
			{
				testHeader.EntryInstruction.CEI_ProvisionalPaymentType = "PPG";
				testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount = 999m;
				var ppInfoBody = new ZString[]
				{
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=15000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", "YY").Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN5", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(9, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(ZString.Empty, testHeader.EntryInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(0m, testHeader.EntryInstruction.CEI_ProvisionalPaymentAmount);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN5/{0} to job: 00626166CLP20160426000199
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been liquidated'.
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateEntryPayInfoStatusFromIncomingMessageUpdateWhereStatusCode(ZString statusCode, ZString incomingMessageText)
		{
			CombineAssertions($"Update Entry Pay Info where status code is {statusCode}", () =>
			{
				var ppInfoBody = new ZString[]
				{
					"ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=15000.00;!Expiry Date=2016/11/:08;'"
				};
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("{STATUSCODE}", statusCode).Replace("{PPINFOBODY}", ZString.Join(ppInfoBody)), "IN5", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);

				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				var entryPayInfos = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Where(x => x.C9_PaymentReference == "100754696");
				AssertEquals(new ZDate(2020, 02, 23), entryPayInfos.FirstOrDefault().C9_ReceiptDate);
			});
		}

		public void TestUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory);
			testHelper.CreateCustomsStatusCusCodeEntry("4");

			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, "Desc.", "CSTA", Core.Constants.CountryCodes.SouthAfrica);
			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IsLiquidatedStatus, "Desc.", "CSTA", Core.Constants.CountryCodes.SouthAfrica);
			var codeXX = testHelper.CreateZACusCodeListEntry("CSTA", "XX");
			codeXX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, ZString.Empty);
			codeXX.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp, "true");
			var codeYY = testHelper.CreateZACusCodeListEntry("CSTA", "YY");
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IUpdateProvisionalPaymentStatus, ZString.Empty);
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IsLiquidatedStatus, ZString.Empty);
			codeYY.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IMarkEntryPayInfoAwaitingResp, "true");
			Factory.Save();

			var incomingMessageText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00626166CLP20160426000199:0'DTM+132:20161023:102'DTM+202:20161006:102'TDT+20+PPG160002+1+++++A:::MAC ELXL6    SOPHIE'LOC+22+DBN::ZZZ'LOC+14+08::ZZZ'GIS+4:120:ZZZ:Y'EQD+CN+TGHU8625162'NAD+AG+00626166'RFF+AAS:MAC PPGPPA16001'DTM+137:20160923:102'RFF+ABT:DBN201610065000311'DTM+137:20161006:102'RFF+ACD:202'RFF+AAV:100754706'ERP+6:0'ERC+1::ZZZ'FTX+AAO+++DETAIN FOR PORT HEALTH'ERP+2:1'ERC+1204::ZZZ'FTX+AAO+++PPGR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:2'ERC+1204::ZZZ'FTX+AAO+++PPAR1 - Provisional Payment lodged in terms of Section 57A of the Cust:oms and Excise Act pending the investigation to the imposition of an a:nti- dumping countervailing or safeguard- duty on goods imported.'ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754696;!DutyType=PPG;!PPAmount=45000.00;!Expiry Date=2016/10/:08;'ERP+2:1'ERC+0000::ZZZ'FTX+AAO+++!PPNo=100754699;!DutyType=PPA;!PPAmount=75000.00;!Expiry Date=2016/10/:08;'TAX+3+CUS:107:ZZZ'MOA+161:200000'CNT+7:10000.00'CNT+11:1'UNT+36+1'";

			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
			Factory.Save();

			var testInstruction = testHeader.EntryInstruction;
			CombineAssertions("Pre-Condition", () =>
			{
				AssertEquals("", testHeader.CH_EntryStatus);
				AssertEquals("202", outgoingMessage.EM_MessageNum);
			});

			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageNoHeaderPPTypeUpdateButAddNewPayInfo(testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageMismatchWontUpdate(testHelper, testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdatingExisting(testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageNotUpdatingCaseNumberMismatch(testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateAgainMatchCaseNumber(testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateNonLiquidatedStatusWontChangePaymentInfoAsClosed(incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateLiquidatedStatusMarkPaymentInfoAsClosed(testHelper, testInstruction, incomingMessageText);
			AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageAddNewLiquidatedWithStatusMarkPaymentInfoAsClosed(incomingMessageText);
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageNoHeaderPPTypeUpdateButAddNewPayInfo(CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("GIS+4 EntryInstruction no Header PP Type, won't update, but just adding new on PayInfo", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText, "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();

				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(0m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(6, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202" && x.C9_PaymentStatus == "CLR"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Entry Pay Info with the 'CLR' payment status added on Customs Entry: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPG' with Reference Number '100754696'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPA' with Reference Number '100754699'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageMismatchWontUpdate(ZAUniversalReferenceTestDataHelper testHelper, CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("Type mis match won't update", () =>
			{
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "XX", "ZA", ZDateTime.Now, "true");
				testInstruction = testHeader.EntryInstruction;
				testInstruction.CEI_ProvisionalPaymentType = "PPR";
				testInstruction.CEI_ProvisionalPaymentAmount = 55m;
				Factory.Save();

				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("45000", "55000").Replace("75000", "65000").Replace("2016/10", "2016/11").Replace("GIS+4", "GIS+XX"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(55m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("PPR", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(6, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 55000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696" && x.C9_PaymentStatus == "XX"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 65000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699" && x.C9_PaymentStatus == "XX"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Provisional Payment Pay Info '100754699' of 'PPA' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdatingExisting(CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("Updating existing", () =>
			{
				testInstruction.CEI_ProvisionalPaymentType = "PPA";
				testInstruction.CEI_ProvisionalPaymentAmount = 55m;
				Factory.Save();

				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("45000", "55000").Replace("75000", "65000").Replace("2016/10", "2016/11").Replace("GIS+4", "GIS+XX"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(65000m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("PPA", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(6, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 55000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPG" && x.C9_PaymentReference == "100754696" && x.C9_PaymentStatus == "XX"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_PaymentAmount == 65000 && x.C9_PaymentDate == new ZDateTime(2016, 11, 08) && x.C9_TransactionType == "PPA" && x.C9_PaymentReference == "100754699" && x.C9_PaymentStatus == "XX"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been updated'.
Information: 	Provisional Payment Pay Info '100754699' of 'PPA' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageNotUpdatingCaseNumberMismatch(CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("Not updating due to case number mismatch", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("100754699", "10075469X").Replace("75000", "65000").Replace("2016/10", "2016/09").Replace("GIS+4", "GIS+XX"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(65000m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("PPA", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been updated'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPA' with Reference Number '10075469X'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateAgainMatchCaseNumber(CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("Update again with match case number", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("75000", "65111").Replace("2016/10", "2016/08"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(65111m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("PPA", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been updated'.
Information: 	Provisional Payment Pay Info '100754699' of 'PPA' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateNonLiquidatedStatusWontChangePaymentInfoAsClosed(ZString incomingMessageText)
		{
			CombineAssertions("Update with non-liquidatedStatus won't change the PaymentInfo as Closed", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("75000", "65113").Replace("2016/10", "2016/08"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699" && !x.C9_RemAdvReceived));
				testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699").C9_RemAdvReceived = true;
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699" && x.C9_RemAdvReceived));
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "10075469X"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				var target = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699");
				AssertEquals(65113m, target.C9_PaymentAmount);
				AssertEquals(expected: true, target.C9_RemAdvReceived);
				AssertEquals("XX", target.C9_PaymentStatus);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '4'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 4 - Detain Other (Other Government Agency - OGA)', B0000100X

Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been updated'.
Information: 	Provisional Payment Pay Info '100754699' of 'PPA' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Auto created eDoc:ORG_CUSNOTIFICATION_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_WORKSHEET_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.
Information: 	Auto created eDoc:ORG_SAD500_00626166CLP20160426000199_201609290105 for entry:00626166CLP20160426000199.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageUpdateLiquidatedStatusMarkPaymentInfoAsClosed(ZAUniversalReferenceTestDataHelper testHelper, CusEntryInstruction testInstruction, ZString incomingMessageText)
		{
			CombineAssertions("Update with LiquidatedStatus mark the PaymentInfo as Closed", () =>
			{
				testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "YY", "ZA", ZDateTime.Now, "true");
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("75000", "65112").Replace("2016/10", "2016/08").Replace("GIS+4", "GIS+YY"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				AssertEquals(0, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699" && !x.C9_RemAdvReceived));
				testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().First(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699").C9_RemAdvReceived = false;
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699" && !x.C9_RemAdvReceived));
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(0m, testInstruction.CEI_ProvisionalPaymentAmount);
				AssertEquals("", testInstruction.CEI_ProvisionalPaymentType);
				AssertEquals(7, testHeader.EntryPayInfos.Count);
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "10075469X"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				var target = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699");
				AssertEquals(65112m, target.C9_PaymentAmount);
				AssertEquals(expected: false, target.C9_RemAdvReceived);
				AssertEquals("YY", target.C9_PaymentStatus);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been liquidated'.
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been liquidated'.
Information: 	Provisional Payment Pay Info '100754699' of 'PPA' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.

", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		void AssertUpdateCusEntryInstructionEntryPayInfoStatusFromIncomingMessageAddNewLiquidatedWithStatusMarkPaymentInfoAsClosed(ZString incomingMessageText)
		{
			CombineAssertions("Add new Liquidated  with LiquidatedStatus mark the PaymentInfo as Closed", () =>
			{
				var testMessage0 = GetMesssageForTest(incomingMessageText.Replace("75000", "65114").Replace("2016/10", "2016/08").Replace("GIS+4", "GIS+YY").Replace("100754699", "100754698"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var testInterchange = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
				Factory.Save();
				logger.ClearLogs();
				AssertEquals(0, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754698"));
				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage0.EM_Status);
				AssertEquals(testHeader.PK, testMessage0.EM_LinkUniqueID);
				AssertEquals(8, testHeader.EntryPayInfos.Count);
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754698"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754699"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754696"));
				AssertEquals(1, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "10075469X"));
				AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				var target = testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().FirstOrDefault(x => x.C9_IncomingPayResponseNo != "202" && x.C9_PaymentReference == "100754698");
				AssertEquals(65114m, target.C9_PaymentAmount);
				AssertEquals(expected: false, target.C9_RemAdvReceived);
				AssertEquals("YY", target.C9_PaymentStatus);
				AssertMultilineASCIIEquals("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN2/{0} to job: 00626166CLP20160426000199
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been liquidated'.
Information: 	Provisional Payment Pay Info '100754696' of 'PPG' on Customs Entry: 00626166CLP20160426000199 on Line No: 1 has been updated'.
Information: 	Header Level Provisional Payment Pay Info of '11 - ' on Declaration B0000100X has been liquidated'.
Information: 	Provisional Payment Pay Info added on Customs Entry: 00626166CLP20160426000199 on Line No: 1 of type 'PPA' with Reference Number '100754698'.", testInterchange.EI_InterchangeNum), logger.LogMessages.ToString());
			});
		}

		public void TestProcessCaseNumber()
		{
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			var testMessage0 = GetMesssageForTest();
			testMessage0.EM_MessageNum = "IN0";
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0];

			var responseMessage = GetTestMessageResNo(CustomsStatus.SupportingDocsRequired);
			responseMessage = responseMessage.Replace("RFF+ACD:202'", "RFF+ACD:202'RFF+AAV:TESTCASE1'");
			testMessage0.EM_MessageText = responseMessage;
			_ = SetInterchangeForMesssage(responseMessage, testMessage0);
			Factory.Save();

			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			Factory.Save();

			var caseNumber = instruction.CaseNumbers[0];
			AssertNotNullOrEmpty(caseNumber.CY_Data);
			AssertEquals("Case Number added", "TESTCASE1", caseNumber.CY_Data);

			var time = ZDateTime.Now.AddMinutes(2);
			var interchange = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), responseMessage);
			testMessage0.EM_EI = interchange.PK;
			testMessage0.EM_MessageText = responseMessage;
			Factory.Save();

			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			Factory.Save();

			AssertEquals("Message count", 2, testHeader.Messages.Count);
			AssertEquals("No duplicate SupportingDocsRequired case numbers", 1, instruction.CaseNumbers.Cast<CaseNumber>().Count(x => x.CY_Code == CaseNumberTypeList.Codes.SupportingDocsRequired));
			AssertEquals("No ElectronicProvisionalPayments case numbers", expected: false, actual: instruction.CaseNumbers.Cast<CaseNumber>().Any(x => x.CY_Code == CaseNumberTypeList.Codes.ElectronicProvisionalPayments));
		}

		public void TestProcessCaseNumber_OtherStatus()
		{
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0];
			GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			var testMessage0 = GetMesssageForTest();
			testMessage0.EM_MessageNum = "IN0";

			var responseMessage = GetTestMessageResNo("78");
			responseMessage = responseMessage.Replace("RFF+ACD:202'", "RFF+ACD:202'RFF+AAV:TESTCASE1'");
			testMessage0.EM_MessageText = responseMessage;
			_ = SetInterchangeForMesssage(responseMessage, testMessage0);

			PreAndProcessMessage(logger, testMessage0);

			AssertEquals("CaseNumbers.Count after first message", 1, instruction.CaseNumbers.Count);
			AssertEquals("CaseNumbers[0].CY_Code after first message", CaseNumberTypeList.Codes.SupportingDocsRequired, instruction.CaseNumbers[0].CY_Code);
			AssertEquals("CaseNumbers[0].CY_Data after first message", "TESTCASE1", instruction.CaseNumbers[0].CY_Data);

			responseMessage = GetTestMessageResNo("79");
			responseMessage = responseMessage.Replace("RFF+ACD:202'", "RFF+ACD:202'RFF+AAV:TESTCASE1'");
			testMessage0.EM_MessageText = responseMessage;

			PreAndProcessMessage(logger, testMessage0);

			AssertEquals("CaseNumbers.Count after second message", 1, instruction.CaseNumbers.Count);
			AssertEquals("CaseNumbers[0].CY_Code after second message", CaseNumberTypeList.Codes.SupportingDocsRequired, instruction.CaseNumbers[0].CY_Code);
			AssertEquals("CaseNumbers[0].CY_Data after second message", "TESTCASE1", instruction.CaseNumbers[0].CY_Data);

			responseMessage = GetTestMessageResNo("80");
			responseMessage = responseMessage.Replace("RFF+ACD:202'", "RFF+ACD:202'RFF+AAV:TESTCASE2'");
			testMessage0.EM_MessageText = responseMessage;

			PreAndProcessMessage(logger, testMessage0);

			AssertEquals("CaseNumbers.Count after third message", 2, instruction.CaseNumbers.Count);
			AssertEquals("CaseNumbers[0].CY_Code after third message", CaseNumberTypeList.Codes.SupportingDocsRequired, instruction.CaseNumbers[0].CY_Code);
			AssertEquals("CaseNumbers[0].CY_Data after third message", "TESTCASE1", instruction.CaseNumbers[0].CY_Data);
			AssertEquals("CaseNumbers[1].CY_Code after third message", CaseNumberTypeList.Codes.SupportingDocsRequired, instruction.CaseNumbers[1].CY_Code);
			AssertEquals("CaseNumbers[1].CY_Data after third message", "TESTCASE2", instruction.CaseNumbers[1].CY_Data);
		}

		public void TestCustomsPrintIndicator()
		{
			_ = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();

			var testMessage1 = GetMesssageForTest(TestMessage.Replace("\r\n", ""), "IN0");
			_ = SetInterchangeForMesssage(TestMessage, testMessage1);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage1);
			Factory.Save();

			AssertEquals("Print Indicator should be 'N'", "N", testHeader.CH_RelPrintInd);

			var testMessage2 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("120:ZZZ:N", "120:ZZZ:Y"), "IN0");
			_ = SetInterchangeForMesssage(TestMessage, testMessage2);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage2);
			Factory.Save();

			AssertEquals("Print Indicator should be 'Y'", "Y", testHeader.CH_RelPrintInd);
		}

		public void TestPaymentDateWhenDTM202IsPresented()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();
			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			Factory.Save();
			var testMessage1 = GetMesssageForTest(TestMessage_StopDetain.Replace("\r\n", ""), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage1.EM_MessageText, testMessage1);
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage1);
			Factory.Save();

			AssertPaymentDateWhenDTM202IsPresentedTransactionType();

			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "false");
			var time = ZDateTime.Now.AddMinutes(2);
			testInterchange = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""));
			var outgoingMessage2 = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "203'"), "203", "ORG");
			testHeader.AddMessage(outgoingMessage2);
			Factory.Save();

			var testMessage2 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "203'"), "IN2");
			testMessage2.EM_EI = testInterchange.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage2);
			Factory.Save();

			AssertPaymentDateWhenDTM202IsPresentedTransactionTypeAndIncomingPayResponseNo();

			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "1", "ZA", ZDateTime.Now, "true");
			time = ZDateTime.Now.AddMinutes(2);
			testInterchange = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""));
			var outgoingMessage3 = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "204");
			Factory.Save();

			var testMessage3 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "204'").Replace("+202:", "+209:"), "IN2");
			testMessage3.EM_EI = testInterchange.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage2);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(8, testHeader.EntryPayInfos.Count);
				AssertEquals(4, testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"));
				AssertEquals(4, testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "203"));
				AssertEquals(0, testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "204"));
			});
		}

		void AssertPaymentDateWhenDTM202IsPresentedTransactionType()
		{
			CombineAssertions(() =>
			{
				var testPayInfo1 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "DTY");
				var testPayInfo2 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "VAT");
				var testPayInfo3 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "OTH");
				var testPayInfo4 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "PEN");
				AssertEquals("1_C9_PaymentDate", new ZDateTime(2016, 03, 28), testPayInfo1.C9_PaymentDate);
				AssertEquals("2_C9_PaymentDate", new ZDateTime(2016, 03, 28), testPayInfo2.C9_PaymentDate);
				AssertEquals("3_C9_PaymentDate", new ZDateTime(2016, 03, 28), testPayInfo2.C9_PaymentDate);
				AssertEquals("4_C9_PaymentDate", new ZDateTime(2016, 03, 28), testPayInfo2.C9_PaymentDate);
			});
		}

		void AssertPaymentDateWhenDTM202IsPresentedTransactionTypeAndIncomingPayResponseNo()
		{
			CombineAssertions(() =>
			{
				AssertEquals(8, testHeader.EntryPayInfos.Count);
				var testPayInfo1 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "DTY" && h.C9_IncomingPayResponseNo == "203");
				var testPayInfo2 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "VAT" && h.C9_IncomingPayResponseNo == "203");
				var testPayInfo3 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "OTH" && h.C9_IncomingPayResponseNo == "203");
				var testPayInfo4 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "PEN" && h.C9_IncomingPayResponseNo == "203");
				AssertEquals("1_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo1.C9_PaymentDate);
				AssertEquals("2_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo2.C9_PaymentDate);
				AssertEquals("3_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo2.C9_PaymentDate);
				AssertEquals("4_C9_PaymentDate", new ZDateTime(2016, 03, 31), testPayInfo2.C9_PaymentDate);
			});
		}

		public void TestUpdateEntryPayInfoForCashEntry()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			var time = ZDateTime.Now.AddMinutes(2);
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("GIS+D:134", "GIS+C:134").Replace("\r\n", ""), "202");
			outgoingMessage.CustomsDutyNoS1P2BBefore = 1m;
			outgoingMessage.CustomsDutyNoS1P2BAfter = 2m;
			outgoingMessage.ValueAddedTaxBefore = 3m;
			outgoingMessage.ValueAddedTaxAfter = 4m;
			outgoingMessage.S1P2BDutyBefore = 2m;
			outgoingMessage.S1P2BDutyAfter = 3m;
			outgoingMessage.PenaltyAmountBefore = 1m;
			outgoingMessage.PenaltyAmountAfter = 5m;
			outgoingMessage.ProvisionalPaymentAmountBefore = 1m;
			outgoingMessage.ProvisionalPaymentAmountAfter = 6m;
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);
			CombineAssertions("Add New PayInfo for Cash Entry", () =>
			{
				var interchange1 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "789");
				var testMessage1 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("DTM+202:20160331:102'", "").Replace("GIS+1:120", "GIS+7:120"), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage1.EM_EI = interchange1.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage1);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
				AssertEquals(testHeader.PK, testMessage1.EM_LinkUniqueID);
				AssertEquals(4, testHeader.EntryPayInfos.Count);

				var messageDate = testMessage1.EM_MessageDateTime.Date;
				var testPayInfo1 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "DTY");
				AssertUpdateEntryPayInfoForCashEntry("1", messageDate, testPayInfo1, 2m);
				var testPayInfo2 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "VAT");
				AssertUpdateEntryPayInfoForCashEntry("2", messageDate, testPayInfo2, 1m);
				var testPayInfo3 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "OTH");
				AssertUpdateEntryPayInfoForCashEntry("3", messageDate, testPayInfo3, 5m);
				var testPayInfo4 = testHeader.EntryPayInfos.OfType<CusEntryPayInfo>().FirstOrDefault(h => h.C9_TransactionType == "PEN");
				AssertUpdateEntryPayInfoForCashEntry("4", messageDate, testPayInfo4, 4m);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/789 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '7'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 7 - Ready For Cash Payment', B0000100X

Information: 	Entry Pay Info with the 'PEN' payment status added on Customs Entry: 00626166CLP20160426000199", logger.LogMessages.ToString());

				var interchange2 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "790");
				var testMessage2 = GetMesssageForTest(GetTestMessageResNo("6"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage2.EM_EI = interchange2.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage2);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage2.EM_Status);
				AssertEquals(testHeader.PK, testMessage2.EM_LinkUniqueID);
				AssertEquals("EntryPayInfo records deleted", 0, testHeader.EntryPayInfos.Count);
				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN2/790 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X

Information: 	Entry Pay Info with the 'PEN' payment status deleted on Customs Entry: 00626166CLP20160426000199", logger.LogMessages.ToString());

				var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "791");
				var testMessage3 = GetMesssageForTest(GetTestMessageResNo("6"), "IN3", new ZDateTime(2017, 5, 23, 1, 5, 0));
				testMessage3.EM_EI = interchange3.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage3);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
				AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);
				AssertEquals("No more EntryPayInfo records created", 0, testHeader.EntryPayInfos.Count);
			});
		}

		void AssertUpdateEntryPayInfoForCashEntry(ZString assertMessagePrefix, ZDate messageDate, CusEntryPayInfo cePayInfo, ZDecimal expectedPayAmount)
		{
			AssertEquals($"{assertMessagePrefix}_C9_PaymentDate", messageDate, cePayInfo.C9_PaymentDate);
			AssertEquals($"{assertMessagePrefix}_C9_CusResReceived", expected: true, cePayInfo.C9_CusResReceived);
			AssertEquals($"{assertMessagePrefix}_C9_RemAdvReceived", expected: false, cePayInfo.C9_RemAdvReceived);
			AssertEquals($"{assertMessagePrefix}_C9_PaymentParty", "C", cePayInfo.C9_PaymentParty);
			AssertEquals($"{assertMessagePrefix}_C9_PaymentAmount", expectedPayAmount, cePayInfo.C9_PaymentAmount);
			AssertEquals($"{assertMessagePrefix}_C9_IsValid", expected: true, (cePayInfo as ILightValidationInternals).IsValid);
			AssertEquals($"{assertMessagePrefix}_C9_IncomingPayResponseNo", "202", cePayInfo.C9_IncomingPayResponseNo);
			AssertEquals($"{assertMessagePrefix}_C9_PaymentStatus", CusEntryPayTypes.Pending, cePayInfo.C9_PaymentStatus);
		}

		public void TestUpdateEntryPayInfoForCashEntryWhenGettingResponseAfterRejected()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("6");
			testHelper.CreateCustomsStatusCusCodeEntry("7");
			var time = ZDateTime.Now.AddMinutes(2);
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("GIS+D:134", "GIS+C:134").Replace("\r\n", ""), "202");
			outgoingMessage.CustomsDutyNoS1P2BBefore = 1m;
			outgoingMessage.CustomsDutyNoS1P2BAfter = 2m;
			outgoingMessage.ValueAddedTaxBefore = 3m;
			outgoingMessage.ValueAddedTaxAfter = 4m;
			outgoingMessage.S1P2BDutyBefore = 2m;
			outgoingMessage.S1P2BDutyAfter = 3m;
			outgoingMessage.PenaltyAmountBefore = 1m;
			outgoingMessage.PenaltyAmountAfter = 5m;
			outgoingMessage.ProvisionalPaymentAmountBefore = 1m;
			outgoingMessage.ProvisionalPaymentAmountAfter = 6m;
			Factory.Save();

			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			CombineAssertions("Add New PayInfo for Cash Entry", () =>
			{
				var interchange2 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", ""), "790");
				var testMessage2 = GetMesssageForTest(GetTestMessageResNo("6"), "IN2", new ZDateTime(2016, 9, 29, 1, 5, 0));
				testMessage2.EM_EI = interchange2.PK;
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage2);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage2.EM_Status);
				AssertEquals(testHeader.PK, testMessage2.EM_LinkUniqueID);
				AssertEquals("No EntryPayInfo records created", 0, testHeader.EntryPayInfos.Count);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN2/790 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '6'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 6 - Reject To Clearer', B0000100X", logger.LogMessages.ToString());

				var testMessage1 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("DTM+202:20160331:102'", "").Replace("GIS+1:120", "GIS+7:120"), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
				var interchange1 = SetInterchangeForMesssage(testMessage1.EM_MessageText, testMessage1);
				interchange1.EI_HeaderText = string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm"));
				interchange1.EI_BodyText = TestMessage.Replace("\r\n", "");
				interchange1.EI_InterchangeNum = "789";
				Factory.Save();

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage1);
				Factory.Save();
				AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
				AssertEquals(testHeader.PK, testMessage1.EM_LinkUniqueID);
				AssertEquals("No EntryPayInfo records created", 0, testHeader.EntryPayInfos.Count);

				AssertMultilineASCIIEquals("Log", @"Information: 	Linking CUSRES Message: #IN1/789 to job: 00626166CLP20160426000199
Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '7'.
Information: 	Email sent.

SUBJECT: Entry Notification: 'Code 7 - Ready For Cash Payment', B0000100X", logger.LogMessages.ToString());
			});
		}

		public void TestUpdateEntryPayInfoFor26StatusCodeWhenReceiveReleaseMessage()
		{
			AssertUpdateEntryPayInfoWhenReceiveReleaseMessage("26");
		}

		public void TestUpdateEntryPayInfoFor2StatusCodeWhenReceiveReleaseMessage()
		{
			AssertUpdateEntryPayInfoWhenReceiveReleaseMessage("2");
		}

		public void AssertUpdateEntryPayInfoWhenReceiveReleaseMessage(string entryStatus)
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			testHelper.CreateCustomsStatusCusCodeEntry("2");
			testHelper.CreateCustomsStatusCusCodeEntry("13");
			testHelper.CreateCustomsStatusCusCodeEntry(entryStatus);

			var time = ZDateTime.Now.AddMinutes(2);
			var outgoingMessage = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "202");
			outgoingMessage.CustomsDutyNoS1P2BBefore = 1m;
			outgoingMessage.S1P2BDutyBefore = 2m;
			outgoingMessage.ValueAddedTaxBefore = 3m;
			outgoingMessage.CustomsDutyNoS1P2BAfter = 2m;
			outgoingMessage.S1P2BDutyAfter = 3m;
			outgoingMessage.ValueAddedTaxAfter = 4m;
			Factory.Save();
			AssertEquals("", testHeader.CH_EntryStatus);
			AssertEquals("202", outgoingMessage.EM_MessageNum);

			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "13", "ZA", ZDateTime.Now, "true");
			var interchange1 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), CUSRESMessageProcessorTest.GetTestMessageResNo("13"), "AAA");
			var testMessage1 = GetMesssageForTest(GetTestMessageResNo("13"), "202", new ZDateTime(2016, 9, 29, 1, 5, 0));
			testMessage1.EM_EI = interchange1.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage1);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage1.EM_Status);
			AssertEquals(testHeader.PK, testMessage1.EM_LinkUniqueID);
			AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "202"
			&& x.C9_PaymentStatus == CusEntryPayTypes.Pending && x.C9_PaymentDate == new ZDateTime(2016, 03, 31)));

			var outgoingMessage2 = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "8926'"), "8926");
			outgoingMessage2.CustomsDutyNoS1P2BBefore = 1m;
			outgoingMessage2.S1P2BDutyBefore = 2m;
			outgoingMessage2.ValueAddedTaxBefore = 3m;
			outgoingMessage2.CustomsDutyNoS1P2BAfter = 2m;
			outgoingMessage2.S1P2BDutyAfter = 3m;
			outgoingMessage2.ValueAddedTaxAfter = 4m;
			Factory.Save();
			AssertEquals("13", testHeader.CH_EntryStatus);
			AssertEquals("8926", outgoingMessage2.EM_MessageNum);

			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", entryStatus, "ZA", ZDateTime.Now, "true");
			var interchange3 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), CUSRESMessageProcessorTest.GetTestMessageResNo(entryStatus).Replace("202'", "8926'"), "BBB");
			var testMessage3 = GetMesssageForTest(GetTestMessageResNo(entryStatus).Replace("202'", "8926'"), "8926", new ZDateTime(2016, 9, 29, 1, 5, 0));
			testMessage3.EM_EI = interchange3.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage3);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage3.EM_Status);
			AssertEquals(testHeader.PK, testMessage3.EM_LinkUniqueID);

			testHelper.SetRefCusCodeListAttribute(Factory, "IMarkEntryPayInfoAwaitingResp", "37", "ZA", ZDateTime.Now, "true");
			var outgoingMessage3 = GetZAMessageForTest(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("202'", "9213'"), "9213");
			outgoingMessage3.CustomsDutyNoS1P2BBefore = 1m;
			outgoingMessage3.S1P2BDutyBefore = 2m;
			outgoingMessage3.ValueAddedTaxBefore = 3m;
			outgoingMessage3.CustomsDutyNoS1P2BAfter = 2m;
			outgoingMessage3.S1P2BDutyAfter = 3m;
			outgoingMessage3.ValueAddedTaxAfter = 4m;
			testHeader.AddMessage(outgoingMessage3);
			Factory.Save();

			var interchange4 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), CUSRESMessageProcessorTest.GetTestMessageResNo("37").Replace("202'", "9213'"), "CCC");
			var testMessage4 = GetMesssageForTest(GetTestMessageResNo("37").Replace("202'", "9213'"), "9213", new ZDateTime(2016, 9, 29, 1, 5, 0));
			testMessage4.EM_EI = interchange4.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage4);
			Factory.Save();

			var interchange5 = GetInterchange(string.Format(TestMessageHeader, time.ToString("yyyyMMdd"), time.ToString("HHmm")), TestMessage.Replace("\r\n", "").Replace("202'", "9213'"), "DDD");
			var testMessage5 = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("202'", "9213'"), "9213", new ZDateTime(2016, 9, 29, 1, 5, 0));
			testMessage5.EM_EI = interchange5.PK;
			Factory.Save();
			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage5);
			Factory.Save();
			AssertEquals(EDIMessage.Status.ProcessedOK, testMessage4.EM_Status);
			AssertEquals(testHeader.PK, testMessage4.EM_LinkUniqueID);
			AssertEquals(4, testHeader.EntryPayInfos.Cast<CusEntryPayInfo>().Count(x => x.C9_IncomingPayResponseNo == "9213" && x.C9_PaymentDate == new ZDateTime(2016, 03, 31) && x.C9_PaymentStatus == "CLR"));
		}

		[TestDate(1990, 06, 01, 01, 36, 09)]
		public void TestVOCValues()
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var helper = new ZAUniversalReferenceTestDataHelper(newFactory);
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateCustomsStatusCusCodeEntry("6");

			var declaration = helper.SetupDeclarationWithAllSchedules();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.CusLineTariffDetails.OfType<CusLineTariffDetail>().Where(x => x.BZ_Type.Left(1) > "2").DeleteAll();
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.PreferentialRate;
			invoiceLine.JI_ROOCert = "X";
			declaration.DoMerge();
			newFactory.Save();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			entry.CH_BGMReference = "00505655KFN20160801000996";
			newFactory.Save();

			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110.00m, 449.68m, 0);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);
			AssertEquals("entry.Messages.Count", 0, entry.Messages.Count);

			SendNewCUSDECMessage(entry, isChange: false);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");
			AssertEquals("entry.Messages.Count", 1, entry.Messages.Count);
			AssertXMLContains(@"GIS+C:134:ZZZ'", messageText);
			AssertXMLContains(@"MOA+38:1000'
MOA+40:1000'
TAX+1+VAT:107:ZZZ'
MOA+161:449.68'
TAX+1+1P1:107:ZZZ'
MOA+161:100.00'
TAX+1+12A:107:ZZZ'
MOA+161:110.00'
TAX+1+12B:107:ZZZ'
MOA+161:110.00'
TAX+1+13A:107:ZZZ'
MOA+161:132.00'
TAX+1+13B:107:ZZZ'
MOA+161:145.20'
TAX+1+13C:107:ZZZ'
MOA+161:159.72'
TAX+1+13D:107:ZZZ'
MOA+161:175.69'
TAX+1+15A:107:ZZZ'
MOA+161:193.26'
TAX+1+15B:107:ZZZ'
MOA+161:212.59'
TAX+1+2P1:107:ZZZ'
MOA+161:233.85'
TAX+1+2P2:107:ZZZ'
MOA+161:257.23'
TAX+1+2P3:107:ZZZ'
MOA+161:282.95'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1000'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:2112.49'
TAX+3+TVD:107:ZZZ'
MOA+161:449.68'
TAX+3+CUS:107:ZZZ'
MOA+161:1000'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
			AssertVOCBeforeValues(cusdecMessage, 0m, 0m, 0m, 0m, 0m, 0);

			AssertVOCValuesProcessCONTROLMessageShouldNotAffectVOCValues(entry);
			AssertVOCValuesProcessCUSRESRejectShouldNotCopyVOCValues(entry);
			AssertVOCValuesProcessCUSRESShouldNotCopyVOCValuesWithCONTROLRejectMessage(entry);
			AssertVOCValuesProcessCUSRESNonRejectShouldCopyVOCValues(entry);
			AssertVOCValuesIncreasingValuesShouldBeSentCorrectly(invoiceLine);
			AssertVOCValuesProcessCONTROLRejectMessageShouldNotCopyVOCValues(entry);
			AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues(entry);
			AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues(entry);
			AssertVOCValuesChangeEntryBeforeValuesShouldBeIncludedCorrectlyInMessage(entry);
			AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues1(entry);
			AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues1(entry);
			AssertVOCValuesWhenNoRefundSelectedMessageShouldContainCorrectValues(invoiceLine);
			AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues2(entry);
			AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues2(entry);
			AssertVOCValuesWhenMoreTaxAndDutyIsDueMessageShouldContainCorrectValues(invoiceLine);
			AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues3(entry);
			AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues3(entry);
			AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues4(entry);
			AssertVOCValuesProcessCUSRESCustomsClearedMessageShouldCopyVOCValues(entry);
		}

		void SendNewCUSDECMessage(CusEntryHeader entry, bool isChange = true)
		{
			var objectParent = new JobDeclarationMessageSendingObjectParent(entry.Declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			if (isChange)
			{
				var sendingObject = objectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			}
			messageManager.SendMessages();
			TestDateAttribute.AddSeconds(1);
		}

		CUSDECEDIMessage GetLastCUSDECMessage(CusEntryHeader entry) => entry.Messages.OfType<CUSDECEDIMessage>().Last();

		void CreateCONTROLResponseMessage(CusEntryHeader entry, ZString interchangeNumber, ZString outgoingMessageNumber, ZString incomingMessageNumber, bool isRejected = false)
		{
			var messageStatus = isRejected ? ActionCodedList.ThisLevelAndAllLowerLevelsRejected : ActionCodedList.ThisLevelAcknowledgedNextLowerLevelAcknowledgedIfNotExplicitlyRejected;
			var interchange = GetZACInterchange(entry.Factory, interchangeNumber.PadLeft(20, '0'), EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			interchange.EI_HeaderText = $"UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:1234+{incomingMessageNumber}++CONTRL+++GWWTGTEST+1'";
			interchange.EI_BodyText = $"UNH+1+CONTRL:D:3:UN:CONTRL'UCI+460+00505655TST::AAAAAAAAAAAAAABB:CORAS2+SARSDECT+7'UCM+{outgoingMessageNumber}+CUSDEC:D:96B:UN:ZZZ01+{messageStatus}'UNT+4+1'";
			interchange.EI_FooterText = $"UNZ+1+{incomingMessageNumber}'";
			var cusdecMessage = GetLastCUSDECMessage(entry);
			cusdecMessage.EM_MessageNum = outgoingMessageNumber;
			entry.Factory.Save();
		}

		void CreateCUSRESResponseMessage(CusEntryHeader entry, ZString interchangeNumber, ZString outgoingMessageNumber, ZString incomingMessageNumber, ZString responseStatusCode)
		{
			var interchange = GetZACInterchange(entry.Factory, interchangeNumber.PadLeft(20, '0'), EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			interchange.EI_HeaderText = $"UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160802:0122+{incomingMessageNumber}++EXPORT+++GWWTGTEST+1'";
			interchange.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655KFN20160801000996'DTM+178:20160801:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+{responseStatusCode}:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:20160801:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:{outgoingMessageNumber}'TAX+3+CUS:107:ZZZ'MOA+161:290'CNT+7:100.00'CNT+11:10'UNT+16+1'";
			interchange.EI_FooterText = $"UNZ+1+{incomingMessageNumber}'";
			entry.Factory.Save();
		}

		void RunMessageProcessors()
		{
			var processor = new InboundInterchangeProcessor(logger);
			var messageProcessor = new ZACIncomingMessageProcessor();
			messageProcessor.Logger = new LoggingInformation();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			TestDateAttribute.AddSeconds(1);
		}

		T ReloadFromNewFactory<T>(T businessObject) where T : BusinessObject => new BusinessObjectFactory().Load<T>(businessObject.PK);

		void AssertVOCValuesProcessCONTROLMessageShouldNotAffectVOCValues(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "31", "997", "1175");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);
		}

		void AssertVOCValuesProcessCUSRESRejectShouldNotCopyVOCValues(CusEntryHeader entry)
		{
			var newFactory = new BusinessObjectFactory();
			var cusresInterchange = GetZACInterchange(newFactory, "00000000000000000032", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1176++CUSRES-EXP-RA+++GWWTGTEST+1'";
			cusresInterchange.EI_BodyText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655KFN20160801000996:0'DTM+178:19900601:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:19900601:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:997'ERP+2:1'ERC+2108::ZZZ'FTX+AAO+++ FIELD(Warehousing details) DESCR(No matching previous declaration cou:ld be found for MRN KFN201606025000111)'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:100.00'CNT+11:10'UNT+19+1'";
			cusresInterchange.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);
		}

		void AssertVOCValuesProcessCUSRESShouldNotCopyVOCValuesWithCONTROLRejectMessage(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			SendNewCUSDECMessage(entry);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			CreateCONTROLResponseMessage(entry, "100", cusdecMessage.EM_MessageNum, "1180", isRejected: true);
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);

			SendNewCUSDECMessage(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);

			cusdecMessage = GetLastCUSDECMessage(entry);
			CreateCUSRESResponseMessage(entry, "101", cusdecMessage.EM_MessageNum, "1181", "6"); // Reject To Clearer
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 0m, 0m, 0m, 0m, 0m, 0);
		}

		void AssertVOCValuesProcessCUSRESNonRejectShouldCopyVOCValues(CusEntryHeader entry)
		{
			var newFactory = new BusinessObjectFactory();
			var cusresInterchange = GetZACInterchange(newFactory, "A32", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange.EI_BodyText = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655KFN20160801000996:0'DTM+178:19900601:102'DTM+202:19900601:102'TDT+20+SA234+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-44444444'DTM+137:19900601:102'RFF+ABT:JSA199006015000422'DTM+137:19900601:102'RFF+ACD:997'TAX+3+CUS:107:ZZZ'MOA+161:6000'CNT+7:10.00'CNT+11:1'UNT+19+1'";
			cusresInterchange.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
		}

		void AssertVOCValuesIncreasingValuesShouldBeSentCorrectly(JobComInvoiceLine invoiceLine)
		{
			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_LinePrice = 1500m;
			declaration = invoiceLine.Declaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			newFactory.Save();
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);

			SendNewCUSDECMessage(entry);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");
			AssertXMLContains(@"GIS+C:134:ZZZ'
FTX+LIN+++1'", messageText);
			AssertXMLContains(@"MOA+38:1500'
MOA+40:1500'
TAX+1+VAT:107:ZZZ'
MOA+161:674.66'
TAX+1+1P1:107:ZZZ'
MOA+161:150.00'
TAX+1+12A:107:ZZZ'
MOA+161:165.00'
TAX+1+12B:107:ZZZ'
MOA+161:165.00'
TAX+1+13A:107:ZZZ'
MOA+161:198.00'
TAX+1+13B:107:ZZZ'
MOA+161:217.80'
TAX+1+13C:107:ZZZ'
MOA+161:239.58'
TAX+1+13D:107:ZZZ'
MOA+161:263.54'
TAX+1+15A:107:ZZZ'
MOA+161:289.89'
TAX+1+15B:107:ZZZ'
MOA+161:318.88'
TAX+1+2P1:107:ZZZ'
MOA+161:350.77'
TAX+1+2P2:107:ZZZ'
MOA+161:385.85'
TAX+1+2P3:107:ZZZ'
MOA+161:424.43'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1500'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1056.25'
TAX+3+TVD:107:ZZZ'
MOA+161:224.98'
TAX+3+CUS:107:ZZZ'
MOA+161:1500'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1500m, 1500m, 3003.74m, 165m, 674.66m, 5);
			AssertVOCBeforeValues(cusdecMessage, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
		}

		void AssertVOCValuesProcessCONTROLRejectMessageShouldNotCopyVOCValues(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "33", "1026", "1214", isRejected: true);
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);

			SendNewCUSDECMessage(entry);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");

			AssertXMLContains(@"GIS+C:134:ZZZ'
FTX+LIN+++1'", messageText);
			AssertXMLContains(@"MOA+38:1500'
MOA+40:1500'
TAX+1+VAT:107:ZZZ'
MOA+161:674.66'
TAX+1+1P1:107:ZZZ'
MOA+161:150.00'
TAX+1+12A:107:ZZZ'
MOA+161:165.00'
TAX+1+12B:107:ZZZ'
MOA+161:165.00'
TAX+1+13A:107:ZZZ'
MOA+161:198.00'
TAX+1+13B:107:ZZZ'
MOA+161:217.80'
TAX+1+13C:107:ZZZ'
MOA+161:239.58'
TAX+1+13D:107:ZZZ'
MOA+161:263.54'
TAX+1+15A:107:ZZZ'
MOA+161:289.89'
TAX+1+15B:107:ZZZ'
MOA+161:318.88'
TAX+1+2P1:107:ZZZ'
MOA+161:350.77'
TAX+1+2P2:107:ZZZ'
MOA+161:385.85'
TAX+1+2P3:107:ZZZ'
MOA+161:424.43'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1500'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:1056.25'
TAX+3+TVD:107:ZZZ'
MOA+161:224.98'
TAX+3+CUS:107:ZZZ'
MOA+161:1500'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1500m, 1500m, 3003.74m, 165m, 674.66m, 5);
			AssertVOCBeforeValues(cusdecMessage, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
		}

		void AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "34", "1027", "1215");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
		}

		void AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "35", "1036", "1216", "39");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m, 5);
		}

		void AssertVOCValuesChangeEntryBeforeValuesShouldBeIncludedCorrectlyInMessage(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			entry.CIFValueBefore = 1200m;
			entry.CustomsValueBefore = 1200m;
			entry.CustomsDutyExcluding12BBefore = 3000m;
			entry.S1P2BDutyBefore = 150m;
			entry.ValueAddedTaxBefore = 600m;
			entry.Factory.Save();
			SendNewCUSDECMessage(entry);

			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");
			AssertXMLContains(@"GIS+C:134:ZZZ'
FTX+LIN+++1'", messageText);
			AssertXMLContains(@"MOA+38:1500'
MOA+40:1500'
TAX+1+VAT:107:ZZZ'
MOA+161:674.66'
TAX+1+1P1:107:ZZZ'
MOA+161:150.00'
TAX+1+12A:107:ZZZ'
MOA+161:165.00'
TAX+1+12B:107:ZZZ'
MOA+161:165.00'
TAX+1+13A:107:ZZZ'
MOA+161:198.00'
TAX+1+13B:107:ZZZ'
MOA+161:217.80'
TAX+1+13C:107:ZZZ'
MOA+161:239.58'
TAX+1+13D:107:ZZZ'
MOA+161:263.54'
TAX+1+15A:107:ZZZ'
MOA+161:289.89'
TAX+1+15B:107:ZZZ'
MOA+161:318.88'
TAX+1+2P1:107:ZZZ'
MOA+161:350.77'
TAX+1+2P2:107:ZZZ'
MOA+161:385.85'
TAX+1+2P3:107:ZZZ'
MOA+161:424.43'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1500'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:18.74'
TAX+3+TVD:107:ZZZ'
MOA+161:74.66'
TAX+3+CUS:107:ZZZ'
MOA+161:1500'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1500m, 1500m, 3003.74m, 165m, 674.66m, 5);
			AssertVOCBeforeValues(cusdecMessage, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues1(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "36", "1036", "1227");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues1(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "37", "1036", "1218", "39");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesWhenNoRefundSelectedMessageShouldContainCorrectValues(JobComInvoiceLine invoiceLine)
		{
			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_LinePrice = 1400m;
			declaration = invoiceLine.Declaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			newFactory.Save();
			AssertVOCAfterValues(entry, 1400m, 1400m, 2803.50m, 154m, 629.58m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);

			SendNewCUSDECMessage(entry);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");
			AssertXMLContains(@"GIS+C:134:ZZZ'
GIS+A:109:ZZZ'", messageText);
			AssertXMLContains(@"MOA+38:1400'
MOA+40:1400'
TAX+1+VAT:107:ZZZ'
MOA+161:629.58'
TAX+1+1P1:107:ZZZ'
MOA+161:140.00'
TAX+1+12A:107:ZZZ'
MOA+161:154.00'
TAX+1+12B:107:ZZZ'
MOA+161:154.00'
TAX+1+13A:107:ZZZ'
MOA+161:184.80'
TAX+1+13B:107:ZZZ'
MOA+161:203.28'
TAX+1+13C:107:ZZZ'
MOA+161:223.61'
TAX+1+13D:107:ZZZ'
MOA+161:245.97'
TAX+1+15A:107:ZZZ'
MOA+161:270.57'
TAX+1+15B:107:ZZZ'
MOA+161:297.62'
TAX+1+2P1:107:ZZZ'
MOA+161:327.39'
TAX+1+2P2:107:ZZZ'
MOA+161:360.12'
TAX+1+2P3:107:ZZZ'
MOA+161:396.14'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1400'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:192.50'
TAX+3+TVD:107:ZZZ'
MOA+161:29.58'
TAX+3+CUS:107:ZZZ'
MOA+161:1400'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1400m, 1400m, 2803.50m, 154m, 629.58m, 5);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues2(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "38", "1038", "1219");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1400m, 1400m, 2803.50m, 154m, 629.58m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues2(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "39", "1038", "1220", "39");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1400m, 1400m, 2803.50m, 154m, 629.58m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesWhenMoreTaxAndDutyIsDueMessageShouldContainCorrectValues(JobComInvoiceLine invoiceLine)
		{
			var newFactory = new BusinessObjectFactory();
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_LinePrice = 1500m;
			declaration = invoiceLine.Declaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			newFactory.Save();
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);

			SendNewCUSDECMessage(entry);
			var cusdecMessage = GetLastCUSDECMessage(entry);
			var messageText = cusdecMessage.EM_MessageText.Replace("'", "'\r\n");
			AssertXMLContains(@"GIS+C:134:ZZZ'
FTX+LIN+++1'", messageText);
			AssertXMLContains(@"MOA+38:1500'
MOA+40:1500'
TAX+1+VAT:107:ZZZ'
MOA+161:674.66'
TAX+1+1P1:107:ZZZ'
MOA+161:150.00'
TAX+1+12A:107:ZZZ'
MOA+161:165.00'
TAX+1+12B:107:ZZZ'
MOA+161:165.00'
TAX+1+13A:107:ZZZ'
MOA+161:198.00'
TAX+1+13B:107:ZZZ'
MOA+161:217.80'
TAX+1+13C:107:ZZZ'
MOA+161:239.58'
TAX+1+13D:107:ZZZ'
MOA+161:263.54'
TAX+1+15A:107:ZZZ'
MOA+161:289.89'
TAX+1+15B:107:ZZZ'
MOA+161:318.88'
TAX+1+2P1:107:ZZZ'
MOA+161:350.77'
TAX+1+2P2:107:ZZZ'
MOA+161:385.85'
TAX+1+2P3:107:ZZZ'
MOA+161:424.43'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:1500'
TAX+3+TRN:107:ZZZ'
MOA+161:0'
TAX+3+TDD:107:ZZZ'
MOA+161:18.74'
TAX+3+TVD:107:ZZZ'
MOA+161:74.66'
TAX+3+CUS:107:ZZZ'
MOA+161:1500'", messageText);
			AssertVOCAfterValues(cusdecMessage, 1500m, 1500m, 3003.74m, 165m, 674.66m, 5);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCONTROLMessageShouldNotCopyVOCValues3(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCONTROLResponseMessage(entry, "42", "1040", "1223");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues3(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "43", "1040", "1224", "39");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCUSRESNonCustomsClearedMessageShouldNotCopyVOCValues4(CusEntryHeader entry)
		{
			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "44", "1040", "1224", "13");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1200m, 1200m, 3000m, 150m, 600m, 5);
		}

		void AssertVOCValuesProcessCUSRESCustomsClearedMessageShouldCopyVOCValues(CusEntryHeader entry)
		{
			var newFactory = new BusinessObjectFactory();
			var testHelper = new ZAUniversalReferenceTestDataHelper(newFactory);
			testHelper.CreateCustomsStatusCusCodeEntry("4");
			var codeCSTA = testHelper.CreateZACusCodeListEntry("CSTA", "27");
			codeCSTA.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");
			newFactory.Save();

			entry = ReloadFromNewFactory(entry);
			CreateCUSRESResponseMessage(entry, "45", "1040", "1224", "27");
			RunMessageProcessors();

			entry = ReloadFromNewFactory(entry);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m, 5);
		}

		void SetupForAPPostingTesting(out BusinessObjectFactory factory, out CusEntryHeader entry, out JobDeclaration declaration, out InboundInterchangeProcessor processor, out ZACIncomingMessageProcessor messageProcessor, out MessageNotificationCollector_ForTest notificationCollector)
		{
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var startDate = new ZDateTime(2019, 9, 1);
			var endDate = new ZDateTime(2019, 9, 30);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateTypeDty = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, "DTY", "Duty");
			rateTypeDty.ZZR_IsPayable = true;
			Factory.Save();
			var rateCodeDty = helper.LoadOrCreateNewCusRateCode(Factory, "1P1", rateTypeDty.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "None", Core.Constants.CountryCodes.SouthAfrica, "ZA");
			helper.CreateTaxOrFee("VAT", 0.15, "ZA", startDate, endDate, "VAT Normal");
			Factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "123123123", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCodeDty.PK, startDate, endDate, "0.2 * VFD", preference.PK, "20%", "ZA");
			Factory.Save();

			_ = Factory.Load<Universal.Internal.RefCusRate>(tariff1Rate1.PK);
			Factory.Save();

			var tradeGroup = helper.CreateTradeGroup("ZA", "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup, "IT", startDate.Date, endDate.Date);
			helper.AddCountry(tradeGroup, "ZA", startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff1Rate1, tradeGroup, startDate, endDate);

			helper.CreateCustomsOfficeCusCodeEntry("JHB");
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateCustomsStatusCusCodeEntry("27");
			helper.CreateCustomsStatusCusCodeEntry("28");

			var codeCSTA = helper.CreateZACusCodeListEntry("CSTA", "27");
			codeCSTA.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");
			var codeCSTA2 = helper.CreateZACusCodeListEntry("CSTA", "28");
			helper.CreateCusCodeListAttribute(codeCSTA2.PK, RefCusCodeListAttributeTypes.Codes.ICustomsCancelled, "true");
			helper.CreateCusCodeListAttribute(codeCSTA2.PK, RefCusCodeListAttributeTypes.Codes.IPostCustomsAPInvoice, "true");

			helper.CreateOrFindExistingRefCusProcedure("ZA", "A", "11", "00", "", "Testing", "IMP", "");
			Factory.Save();

			var periods = new AccPeriodManagementCollection(Factory);
			var currentPeriod = periods.AddNew();
			currentPeriod.AM_StartDate = startDate;
			currentPeriod.AM_EndDate = endDate;
			currentPeriod.AM_Year = 2019;
			currentPeriod.AM_Period = 201909;
			currentPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
			testCreditor.MainAddress.Address1 = "TSTCRED";
			testCreditor.OH_IsCreditor = true;
			var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
			testSupplier.MainAddress.Address1 = "TSTSUP";
			var testImporter = Factory.NewWithValidTestData<OrgHeader>();
			testImporter.MainAddress.Address1 = "TSTIMP";
			var testAgent = Factory.NewWithValidTestData<OrgHeader>();
			testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
			testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "87654321", Core.Constants.CountryCodes.SouthAfrica);

			Factory.Save();

			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
			customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());

			GlbGroup group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			GlbGroupLink link = Factory.New<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			var collection = new CustomsDSBCreditorOverrideCollection();
			var creditor = collection.AddNew();
			creditor.DistrictOfficeCode = "JHB";
			creditor.CreditorPK = testCreditor.PK;
			ZACustomsRegistry.Instance.DSBCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			Factory.Save();

			notificationCollector = new MessageNotificationCollector_ForTest();
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			processor = new InboundInterchangeProcessor(logger);
			messageProcessor = new ZACIncomingMessageProcessor();
			messageProcessor.Logger = new LoggingInformation();

			var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB"));
			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
			Factory.Save();

			factory = new BusinessObjectFactory();

			declaration = factory.New<JobDeclaration>();
			var orgProxy = declaration.Branch.Company.OrgProxy;
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "00626166", Core.Constants.CountryCodes.SouthAfrica);

			factory.Save();

			var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
			var mapping1 = agentOfficeToCreditorMappings.AddNew();
			mapping1.OrganizationPK = orgProxy.PK;
			mapping1.CustomsOfficeCode = "JHB";
			mapping1.FinancialAccountNumber = "1234567890";
			mapping1.CreditorPK = testCreditor.PK;
			mapping1.ImporterPays = false;
			mapping1.AccountStartDay = 1;
			ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CustomsOffice = "JHB";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_OH_Supplier = testSupplier.PK;
			declaration.JE_OH_Importer = testImporter.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "INV0001";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.SouthAfrica;
			invoice.JZ_InvoiceAmount = 1000m;

			var line = invoice.InvoiceLines.AddNew();
			line.JI_Tariff = "123123123";
			line.JI_Procedure = "1100";
			line.JI_PrimaryPreference = "100";
			line.JI_CEI = entryInstruction.PK;
			line.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;
			line.JI_ZZF_NKTaxType = "VAT";
			line.JI_LinePrice = 1000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			entry = declaration.ActiveEntryHeaders[0];
			factory.Save();
			Assert("CH_BGMReference should contain a value", !entry.CH_BGMReference.IsEmpty);
		}

		[TestDate(2019, 9, 25, 10, 11, 12)]
		public void TestVOCValuesWithAPPosting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				SetupForAPPostingTesting(out var factory, out var entry, out var declaration, out var processor, out var messageProcessor, out var notificationCollector);

				AssertVOCValuesWithAPPostingSendInitialDec(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);
				AssertVOCValuesWithAPPostingSendPositiveVOC(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);
				AssertVOCValuesWithAPPostingSendNegativeVOC(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);
				AssertVOCValuesWithAPPostingSendPositiveVOCWithoutAPPostingSet(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);
				AssertVOCValuesWithAPPostingCancelDeclarationShouldReverseAllCharges(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);
			}
		}

		void AssertVOCValuesWithAPPostingSendInitialDec(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);
			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 1 Msg", 1, entry.Messages.Count);
			AssertNotNull("Declaration should have a job", declaration.Job);

			var outgoingMsg1 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg1.EM_MessageNum = "1001";
			outgoingMsg1.EM_Status = "SNT";

			var ctrlInt1 = GetZACInterchange(factory, "0000001", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt1.EI_ApplicationCode = "ZAC";
			ctrlInt1.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1111++CONTRL+++TESTINGWTG'";
			ctrlInt1.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+00626166WTG::N5L3X7O8Y2J5W7G5:WTGAS2+SARSDEC+7'UCM+1001+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt1.EI_FooterText = "UNZ+1+1111'";

			var cusInt1 = GetZACInterchange(factory, "0000002", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt1.EI_ApplicationCode = "ZAC";
			cusInt1.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1112++CUSRES+++TESTINGWTG'";
			cusInt1.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA051+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20190919:102'RFF+AAS:125-88155185'DTM+137:20190919:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1001'TAX+3+CUS:107:ZZZ'MOA+161:70561'CNT+7:1.00'CNT+11:1'UNT+21+1'";
			cusInt1.EI_FooterText = "UNZ+1+1112'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 3 Msgs", 3, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("First Charge", () =>
			{
				AssertEquals(1, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/CUSDSB", invoiceJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(395m), invoiceJob.Charges[0].JR_LocalSellAmt);
				AssertEquals(expected: true, invoiceJob.Charges[0].IsCostPosted);
			});
		}

		void AssertVOCValuesWithAPPostingSendPositiveVOC(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			declaration.Invoices[0].JZ_InvoiceAmount = 1500m;
			declaration.InvoiceLines[0].JI_LinePrice = 1500m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 4 Msgs", 4, entry.Messages.Count);

			var outgoingMsg2 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg2.EM_MessageNum = "1002";
			outgoingMsg2.EM_Status = "SNT";

			var ctrlInt2 = GetZACInterchange(factory, "0000003", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt2.EI_ApplicationCode = "ZAC";
			ctrlInt2.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1113++CONTRL+++TESTINGWTG'";
			ctrlInt2.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1002+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt2.EI_FooterText = "UNZ+1+1113'";

			var cusInt2 = GetZACInterchange(factory, "0000004", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt2.EI_ApplicationCode = "ZAC";
			cusInt2.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1114++CUSRES+++TESTINGWTG'";
			cusInt2.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA057+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+27:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20181127:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1002'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:1.00'CNT+11:1'UNT+20+1'";
			cusInt2.EI_FooterText = "UNZ+1+1114'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 6 Msgs", 6, entry.Messages.Count);

			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);
			CombineAssertions("Second Charge", () =>
			{
				AssertEquals(2, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/CUSDSB/1", invoiceJob.Charges[1].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(197.50m), invoiceJob.Charges[1].JR_LocalSellAmt);
				AssertEquals(expected: true, invoiceJob.Charges[1].IsCostPosted);
			});
		}

		void AssertVOCValuesWithAPPostingSendNegativeVOC(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			declaration.Invoices[0].JZ_InvoiceAmount = 1200m;
			declaration.InvoiceLines[0].JI_LinePrice = 1200m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 7 Msgs", 7, entry.Messages.Count);

			var outgoingMsg3 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg3.EM_MessageNum = "1003";
			outgoingMsg3.EM_Status = "SNT";

			var ctrlInt3 = GetZACInterchange(factory, "0000005", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt3.EI_ApplicationCode = "ZAC";
			ctrlInt3.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1115++CONTRL+++TESTINGWTG'";
			ctrlInt3.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1003+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt3.EI_FooterText = "UNZ+1+1115'";

			var cusInt3 = GetZACInterchange(factory, "0000006", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt3.EI_ApplicationCode = "ZAC";
			cusInt3.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1116++CUSRES+++TESTINGWTG'";
			cusInt3.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA057+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+27:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20181127:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1003'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:1.00'CNT+11:1'UNT+20+1'";
			cusInt3.EI_FooterText = "UNZ+1+1116'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 9 Msgs", 9, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("Third Charge", () =>
			{
				AssertEquals(3, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/CUSDSB/2", invoiceJob.Charges[2].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(-118.50m), invoiceJob.Charges[2].JR_LocalSellAmt);
				AssertEquals(expected: false, invoiceJob.Charges[2].IsCostPosted);
			});
		}

		void AssertVOCValuesWithAPPostingSendPositiveVOCWithoutAPPostingSet(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = false;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

			declaration.Invoices[0].JZ_InvoiceAmount = 1600m;
			declaration.InvoiceLines[0].JI_LinePrice = 1600m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 10 Msgs", 10, entry.Messages.Count);

			var outgoingMsg4 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg4.EM_MessageNum = "1004";
			outgoingMsg4.EM_Status = "SNT";

			var ctrlInt4 = GetZACInterchange(factory, "0000007", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt4.EI_ApplicationCode = "ZAC";
			ctrlInt4.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1117++CONTRL+++TESTINGWTG'";
			ctrlInt4.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1004+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt4.EI_FooterText = "UNZ+1+1117'";

			var cusInt4 = GetZACInterchange(factory, "0000008", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt4.EI_ApplicationCode = "ZAC";
			cusInt4.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1118++CUSRES+++TESTINGWTG'";
			cusInt4.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA057+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+27:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20181127:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1004'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:1.00'CNT+11:1'UNT+20+1'";
			cusInt4.EI_FooterText = "UNZ+1+1118'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 12 Msgs", 12, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("Replacement Third Charge", () =>
			{
				AssertEquals(3, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/CUSDSB/2", invoiceJob.Charges[2].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(39.50m), invoiceJob.Charges[2].JR_LocalSellAmt);
				AssertEquals(expected: false, invoiceJob.Charges[2].IsCostPosted);
			});
		}

		void AssertVOCValuesWithAPPostingCancelDeclarationShouldReverseAllCharges(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			factory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Cancellation;
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 13 Msgs", 13, entry.Messages.Count);

			var outgoingMsg5 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg5.EM_MessageNum = "1005";
			outgoingMsg5.EM_Status = "SNT";

			var ctrlInt5 = GetZACInterchange(factory, "0000009", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt5.EI_ApplicationCode = "ZAC";
			ctrlInt5.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1117++CONTRL+++TESTINGWTG'";
			ctrlInt5.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1005+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt5.EI_FooterText = "UNZ+1+1117'";

			var cusInt5 = GetZACInterchange(factory, "0000010", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt5.EI_ApplicationCode = "ZAC";
			cusInt5.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1118++CUSRES+++TESTINGWTG'";
			cusInt5.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA057+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+28:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20181127:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1005'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:1.00'CNT+11:1'UNT+20+1'";
			cusInt5.EI_FooterText = "UNZ+1+1118'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 15 Msgs", 15, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			AssertEquals("Should still have 3 charges but new values", 3, invoiceJob.Charges.Count); //Updates existing unposted-charge
			CombineAssertions("Cancellation Charge", () =>
			{
				AssertEquals("JHB201909180000002/CUSDSB/2", invoiceJob.Charges[2].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(-592.5), invoiceJob.Charges[2].JR_LocalSellAmt);
				AssertEquals(expected: false, invoiceJob.Charges[2].IsCostPosted);
			});
		}

		internal static ZString AmendmentGrantedNoPostingDateMessageBody(string reference) => $@"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+{reference}:0'
DTM+178:20190919:102'
TDT+20+BA057+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+L4::ZZZ'
GIS+27:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:12345678TSFO4216504'
DTM+137:20181127:102'
RFF+AAS:125-88155185'
DTM+137:20181127:102'
RFF+ABT:JHB201909180000002'
DTM+137:20190919:102'
RFF+ACD:1002'
TAX+3+CUS:107:ZZZ'
MOA+161:88518'
CNT+7:1.00'
CNT+11:1'
UNT+20+1'".Replace("\r\n", "");

		[TestDate(2019, 9, 25, 10, 11, 12)]
		public void TestAutoPostedBillingWithAmendment()
		{
			ZString InterchangeHeaderString(string responseToMessageNumber, string thisMessageNumber, string messageType) =>
				$"UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:{responseToMessageNumber}+{thisMessageNumber}++{messageType}+++TESTINGWTG'";
			ZString ControlResponseMessageBody() =>
				"UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1002+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ZString QuerySupportingDocumentsRequiredMessageBody(string reference) =>
				$"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{reference}:1'DTM+178:20190919:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A2::ZZZ'GIS+13:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20190919:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1002'RFF+AAV:185681385'ERP+1:0'ERC+0000::ZZZ'FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:0.50'CNT+11:1'UNT+24+1'";
			ZString SupportingDocumentsAcceptedMessageBody(string reference) =>
				$"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{reference}:1'DTM+178:20190919:102'TDT+20+SA123+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A2::ZZZ'GIS+33:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20190919:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1002'RFF+AAV:185681385'ERP+6:0'ERC+100::ZZZ'FTX+AAO+++00626166WTG-67196d8b-7e70-4eee-9789-dd5cf6eb3cea'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:0.50'CNT+11:1'UNT+24+1'";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				SetupForAPPostingTesting(out var factory, out var entry, out var declaration, out var processor, out var messageProcessor, out var notificationCollector);
				AssertVOCValuesWithAPPostingSendInitialDec(ref factory, ref entry, ref declaration, processor, messageProcessor, notificationCollector);

				declaration.Invoices[0].JZ_InvoiceAmount = 500m;
				declaration.InvoiceLines[0].JI_LinePrice = 500m;
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				factory.Save();

				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var sendingObject = objectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
				var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

				messageManager.SendMessages();
				factory.Save();

				factory = new BusinessObjectFactory();
				declaration = factory.Load<JobDeclaration>(declaration.PK);
				entry = factory.Load<CusEntryHeader>(entry.PK);
				AssertEquals("Entry should have 4 Msgs", 4, entry.Messages.Count);

				var outgoingMsg2 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				outgoingMsg2.EM_MessageNum = "1002";
				outgoingMsg2.EM_Status = "SNT";

				var ctrlInt2 = GetZACInterchange(factory, "0000003", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
				ctrlInt2.EI_ApplicationCode = "ZAC";
				ctrlInt2.EI_HeaderText = InterchangeHeaderString("1021", "1113", "CONTRL");
				ctrlInt2.EI_BodyText = ControlResponseMessageBody();
				ctrlInt2.EI_FooterText = "UNZ+1+1113'";

				var queryInt = GetZACInterchange(factory, "0000006", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
				queryInt.EI_ApplicationCode = "ZAC";
				queryInt.EI_HeaderText = InterchangeHeaderString("1021", "1116", "CUSRES");
				queryInt.EI_BodyText = QuerySupportingDocumentsRequiredMessageBody(entry.CH_BGMReference);
				queryInt.EI_FooterText = "UNZ+1+1116'";

				var docsInt = GetZACInterchange(factory, "0000007", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
				docsInt.EI_ApplicationCode = "ZAC";
				docsInt.EI_HeaderText = InterchangeHeaderString("1021", "1117", "CUSRES");
				docsInt.EI_BodyText = SupportingDocumentsAcceptedMessageBody(entry.CH_BGMReference);
				docsInt.EI_FooterText = "UNZ+1+1117'";

				var acceptedInt = GetZACInterchange(factory, "0000008", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
				acceptedInt.EI_ApplicationCode = "ZAC";
				acceptedInt.EI_HeaderText = InterchangeHeaderString("1021", "1118", "CUSRES");
				acceptedInt.EI_BodyText = AmendmentGrantedNoPostingDateMessageBody(entry.CH_BGMReference);
				acceptedInt.EI_FooterText = "UNZ+1+1118'";

				factory.Save();
				processor.ExecuteBatch();
				messageProcessor.ExecuteBatch();

				Factory.Save();

				new InvoicePostingAccountingIntegrator().IntegrateIfNecessary(
					new JobDeclarationIAccIntegrationDataProvider(
						ChargePosterBehaviours.AutoRateDSB | ChargePosterBehaviours.APPostDSB | ChargePosterBehaviours.PostNegativeCost,
						declaration.ActiveEntryHeaders.Select(x => x.PK), declaration.PK, true, Factory));

				factory = new BusinessObjectFactory();
				declaration = factory.Load<JobDeclaration>(declaration.PK);
				entry = factory.Load<CusEntryHeader>(entry.PK);
				AssertEquals("Entry should have 8 Msgs", 8, entry.Messages.Count);
				var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

				CombineAssertions("Adjustment Charge", () =>
				{
					AssertEquals(2, invoiceJob.Charges.Count);
					AssertEquals("JHB201909180000002/CUSDSB/1", invoiceJob.Charges[1].JR_APInvoiceNum);
					AssertEquals(new ZDecimal(-197.50m), invoiceJob.Charges[1].JR_LocalSellAmt);
					AssertEquals(expected: true, invoiceJob.Charges[1].IsCostPosted);
				});
			}
		}

		[TestDate(1990, 06, 01, 01, 36, 09)]
		public void TestVOCValues_ResetBeforeVOCValuesToLatestAcceptedAfterVOCValues()
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var helper = new ZAUniversalReferenceTestDataHelper(newFactory);
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateCustomsStatusCusCodeEntry("6");
			var declaration = helper.SetupDeclarationWithAllSchedules();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.CusLineTariffDetails.OfType<CusLineTariffDetail>().Where(x => x.BZ_Type.Left(1) > "2").DeleteAll();
			invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreference.PreferentialRate;
			invoiceLine.JI_ROOCert = "X";
			declaration.DoMerge();
			newFactory.Save();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			entry.CH_BGMReference = "00505655JSA19900601001245";
			newFactory.Save();
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110.00m, 449.68m);
			AssertVOCBeforeValues(entry, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);
			messageManager.SendMessages();
			var cusdecMessage1 = (CUSDECEDIMessage)entry.Messages[0];
			AssertVOCAfterValues(cusdecMessage1, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(cusdecMessage1, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero);

			var processor = new InboundInterchangeProcessor(logger);
			var messageProcessor = new ZACIncomingMessageProcessor { Logger = new LoggingInformation() };

			AssertVOCValues_ResetProcessCUSRESSNonRejectShouldCopyVOCValues(ref newFactory, ref entry, processor, messageProcessor);
			AssertVOCValues_ResetIncreasingValuesShouldBeSendCorrectly(ref newFactory, ref entry, ref invoiceLine);
			AssertVOCValues_ResetProcessCUSRESSNonRejectShouldCopyVOCValues1(ref newFactory, ref entry, processor, messageProcessor);
			AssertVOCValues_ResetIncreasingValuesShouldBeSendCorrectly1(ref newFactory, ref entry, ref invoiceLine);
			AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues(ref newFactory, ref entry, processor, messageProcessor);
			AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues1(ref newFactory, ref entry, processor, messageProcessor);
			AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues2(ref newFactory, ref entry, processor, messageProcessor);
		}

		void AssertVOCValues_ResetProcessCUSRESSNonRejectShouldCopyVOCValues(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor)
		{
			var cusdecMessage1 = (CUSDECEDIMessage)entry.Messages[0];
			var cusresInterchange2 = GetZACInterchange(newFactory, "00000000000000000A32", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange2.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20150801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange2.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA19900601001245:0'DTM+178:19900601:102'DTM+202:19900601:102'TDT+20+SA234+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-44444444'DTM+137:19900601:102'RFF+ABT:JSA199006015000422'DTM+137:19900601:102'RFF+ACD:{cusdecMessage1.EM_MessageNum}'TAX+3+CUS:107:ZZZ'MOA+161:6000'CNT+7:10.00'CNT+11:1'UNT+19+1'";
			cusresInterchange2.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			newFactory.Save();
			newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			entry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertVOCAfterValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
		}

		void AssertVOCValues_ResetIncreasingValuesShouldBeSendCorrectly(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, ref JobComInvoiceLine invoiceLine)
		{
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_LinePrice = 1500m;
			declaration = invoiceLine.Declaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			newFactory.Save();
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1000m, 1000m, 2002.49m, 110m, 449.68m);
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			messageManager.SendMessages();
			var cusdecMessage1 = (CUSDECEDIMessage)entry.Messages[0];
			var cusdecMessage2 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[1];
			cusdecMessage2.EM_SystemCreateTimeUtc = cusdecMessage1.EM_SystemCreateTimeUtc.AddHours(1);
			AssertVOCAfterValues(cusdecMessage2, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(cusdecMessage2, 1000m, 1000m, 2002.49m, 110m, 449.68m);
		}

		void AssertVOCValues_ResetProcessCUSRESSNonRejectShouldCopyVOCValues1(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor)
		{
			var cusdecMessage2 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[1];
			var cusresInterchange3 = GetZACInterchange(newFactory, "00000000000000000A33", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange3.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange3.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA19900601001245:0'DTM+178:19900601:102'DTM+202:19900601:102'TDT+20+SA234+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-44444444'DTM+137:19900601:102'RFF+ABT:JSA199006015000422'DTM+137:19900601:102'RFF+ACD:{cusdecMessage2.EM_MessageNum}'TAX+3+CUS:107:ZZZ'MOA+161:6000'CNT+7:10.00'CNT+11:1'UNT+19+1'";
			cusresInterchange3.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			newFactory.Save();
			newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			entry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertVOCAfterValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			AssertVOCBeforeValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
		}

		void AssertVOCValues_ResetIncreasingValuesShouldBeSendCorrectly1(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, ref JobComInvoiceLine invoiceLine)
		{
			invoiceLine = newFactory.Load<JobComInvoiceLine>(invoiceLine.PK);
			invoiceLine.JI_LinePrice = 3000m;
			declaration = invoiceLine.Declaration;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			entry = declaration.ActiveEntryHeaders[0];
			entry.CH_PaymentMethod = PaymentMethodCodeList.Codes.Cash;
			newFactory.Save();
			AssertVOCAfterValues(entry, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
			AssertVOCBeforeValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var notification = new MessageNotificationCollector_ForTest();
			var messageManager = new MessageManagers.MessageManager(objectParent, notification);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			messageManager.SendMessages();
			var cusdecMessage1 = (CUSDECEDIMessage)entry.Messages[0];
			var cusdecMessage3 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[2];
			cusdecMessage3.EM_SystemCreateTimeUtc = cusdecMessage1.EM_SystemCreateTimeUtc.AddHours(2);
			AssertVOCAfterValues(cusdecMessage3, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
			AssertVOCBeforeValues(cusdecMessage3, 1500m, 1500m, 3003.74m, 165m, 674.66m);
		}

		void AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor)
		{
			var cusdecMessage3 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[2];
			var cusresInterchange4 = GetZACInterchange(newFactory, "00000000000000000044", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange4.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20170801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange4.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA19900601001245:0'DTM+178:19900601:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:19900601:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:{cusdecMessage3.EM_MessageNum}'ERP+2:1'ERC+2108::ZZZ'FTX+AAO+++ FIELD(Warehousing details) DESCR(No matching previous declaration cou:ld be found for MRN KFN201606025000111)'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:100.00'CNT+11:10'UNT+19+1'";
			cusresInterchange4.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			entry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertVOCAfterValues(entry, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
			AssertVOCBeforeValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
		}

		void AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues1(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor)
		{
			var cusdecMessage3 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[2];
			var cusresInterchange5 = GetZACInterchange(newFactory, "00000000000000000045", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange5.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20160801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange5.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA19900601001245:0'DTM+178:19900601:102'DTM+202:19900601:102'TDT+20+SA234+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+A8::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:083-44444444'DTM+137:19900601:102'RFF+ABT:JSA199006015000422'DTM+137:19900601:102'RFF+ACD:{cusdecMessage3.EM_MessageNum}'TAX+3+CUS:107:ZZZ'MOA+161:6000'CNT+7:10.00'CNT+11:1'UNT+19+1'";
			cusresInterchange5.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			newFactory.Save();
			newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			entry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertVOCAfterValues(entry, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
			AssertVOCBeforeValues(entry, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
		}

		void AssertVOCValues_ResetProcessCUSRESRejectShouldResetVOCBeforeValues2(ref BusinessObjectFactory newFactory, ref CusEntryHeader entry, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor)
		{
			var cusdecMessage3 = entry.Messages.OfType<CUSDECEDIMessage>().ToArray()[2];
			var cusresInterchange6 = GetZACInterchange(newFactory, "00000000000000000046", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusresInterchange6.EI_HeaderText = "UNB+UNOB:4+SARSDECT+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20170801:0655+1176++CUSRES+++GWWTGTEST+1'";
			cusresInterchange6.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00505655JSA19900601001245:0'DTM+178:19900601:102'TDT+20+DE1455+3+++++:::DE1455'LOC+22+KFN::ZZZ'GIS+6:120:ZZZ:N'NAD+AG+00505655'RFF+AAS:BOL1608011455'DTM+137:19900601:102'RFF+UCN:6ZA011432336ZA01143233000000000004'RFF+ACD:{cusdecMessage3.EM_MessageNum}'ERP+2:1'ERC+2108::ZZZ'FTX+AAO+++ FIELD(Warehousing details) DESCR(No matching previous declaration cou:ld be found for MRN KFN201606025000111)'TAX+3+CUS:107:ZZZ'MOA+161:0'CNT+7:100.00'CNT+11:10'UNT+19+1'";
			cusresInterchange6.EI_FooterText = "UNZ+1+1176'";
			newFactory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			entry = newFactory.Load<CusEntryHeader>(entry.PK);
			AssertVOCAfterValues(entry, 3000m, 3000m, 6007.47m, 330m, 1349.18m);
			AssertVOCBeforeValues(entry, 1500m, 1500m, 3003.74m, 165m, 674.66m);
		}

		[TestDate(2019, 9, 25, 10, 11, 12)]
		public void TestVOCDutyValuesAutoPosted()
		{
			AccChargeCode createChargeCode(string code, string description, string chargeType)
			{
				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "UT" + code;
				chargeCode.AC_Desc = description;
				chargeCode.AC_ChargeType = chargeType;
				chargeCode.AC_MarginPercentage = 0m;
				chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
				return chargeCode;
			}

			var helper = new ZAUniversalReferenceTestDataHelper(Factory, false);
			var startDate = new ZDateTime(2019, 9, 1);
			var endDate = new ZDateTime(2019, 9, 30);
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Constants.CountryCodes.SouthAfrica, UniversalReferenceConstants.CusTariffCode.Schedule1Part1);
			var rateTypeDty = helper.CreateNewOrGetExistingRateType(Constants.CountryCodes.SouthAfrica, "DTY", "Duty");
			rateTypeDty.ZZR_IsPayable = true;
			rateTypeDty.ZZR_CustomsValueFormula = "CV";
			Factory.Save();
			var rateCodeDty = helper.LoadOrCreateNewCusRateCode(Factory, "1P1", rateTypeDty.PK);
			var preference = helper.CreatePreferenceForCountryAndGrouping("100", "None", Constants.CountryCodes.SouthAfrica, "ZA");
			_ = helper.CreateTaxOrFee("VAT", 0.15, "ZA", startDate, endDate, "VAT Normal");
			var chargeCodeDisbursementDEF = createChargeCode("DSB", "Customs Disbursements Default", Constants.ChargeType.Disbursement);
			var chargeCodeDisbursementDTY = createChargeCode("DTY", "Customs Disbursements Duty", Constants.ChargeType.Disbursement);
			var chargeCodeDisbursementVAT = createChargeCode("VAT", "Customs Disbursements VAT", Constants.ChargeType.Disbursement);
			Factory.Save();

			RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeCodeDisbursementDEF.PK.ToGuid());
			var entryChargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var entryChargeType1 = entryChargeTypesAndCodes.AddNew();
			entryChargeType1.ChargeType = Enterprise.Customs.Universal.Constants.RateTypes.Duty;
			entryChargeType1.AC_ChargeCode = chargeCodeDisbursementDTY.PK;
			var entryChargeType2 = entryChargeTypesAndCodes.AddNew();
			entryChargeType2.ChargeType = "VAT";
			entryChargeType2.AC_ChargeCode = chargeCodeDisbursementVAT.PK;
			RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, entryChargeTypesAndCodes);
			Factory.Save();

			var tariff1 = helper.CreateTariff(Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "123123123", startDate, endDate, taxOrFeeCode: "VAT");
			var tariff1Rate1 = helper.CreateRate(tariff1, rateCodeDty.PK, startDate, endDate, "0.2 * VFD", preference.PK, "20%", "ZA");
			_ = helper.CreateTariff(Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "321321321", startDate, endDate, taxOrFeeCode: "VAT");
			_ = helper.CreateRate(tariff1, rateCodeDty.PK, startDate, endDate, "0", preference.PK, "0%", "ZA");
			Factory.Save();

			_ = Factory.Load<Universal.Internal.RefCusRate>(tariff1Rate1.PK);
			Factory.Save();

			var tradeGroup = helper.CreateTradeGroup("ZA", "STANDARD", startDate, endDate);
			helper.AddCountry(tradeGroup, "IT", startDate.Date, endDate.Date);
			helper.AddCountry(tradeGroup, "ZA", startDate.Date, endDate.Date);
			helper.CreateCusApplicability(tariff1Rate1, tradeGroup, startDate, endDate);
			helper.CreateCustomsOfficeCusCodeEntry("JHB");
			helper.CreateCustomsStatusCusCodeEntry("1");
			helper.CreateCustomsStatusCusCodeEntry("27");
			var codeCSTA = helper.CreateZACusCodeListEntry("CSTA", "27");
			codeCSTA.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsCleared, "true");

			var proc = helper.CreateOrFindExistingRefCusProcedure("ZA", "A", "11", "00", "", "Testing", "IMP", "");
			proc.ZZ6_CalculateDuty = true;
			proc.ZZ6_CalculateVAT = true;
			Factory.Save();

			var periods = new AccPeriodManagementCollection(Factory);
			var currentPeriod = periods.AddNew();
			currentPeriod.AM_StartDate = startDate;
			currentPeriod.AM_EndDate = endDate;
			currentPeriod.AM_Year = 2019;
			currentPeriod.AM_Period = 201909;
			currentPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var testCreditor = Factory.NewWithValidTestData<OrgHeader>();
				testCreditor.MainAddress.Address1 = "TSTCRED";
				testCreditor.OH_IsCreditor = true;
				var testSupplier = Factory.NewWithValidTestData<OrgHeader>();
				testSupplier.MainAddress.Address1 = "TSTSUP";
				var testImporter = Factory.NewWithValidTestData<OrgHeader>();
				testImporter.MainAddress.Address1 = "TSTIMP";
				var testAgent = Factory.NewWithValidTestData<OrgHeader>();
				testAgent.CustomsCodes.AddNew("CDP", "51051342", "ZA");
				testAgent.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "87654321", Constants.CountryCodes.SouthAfrica);
				Factory.Save();

				var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
				var customsDefaultToCurrentLoginDeptRegistry = (BooleanRegistryItem)data.FindByName("CustomsDefaultToCurrentLoginDept");
				customsDefaultToCurrentLoginDeptRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var staff = Factory.LoadTop1<GlbStaff>(new ZQuery());
				var group = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
				var link = Factory.New<GlbGroupLink>();
				link.GK_GG = group.PK;
				link.GK_GS = staff.PK;
				Factory.Save();

				var option = new AccountingIntegrationOptions();
				option.EnableAccountingIntegration = true;
				option.APPostDSB = true;
				CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, option);

				var collection = new CustomsDSBCreditorOverrideCollection();
				var creditor = collection.AddNew();
				creditor.DistrictOfficeCode = "JHB";
				creditor.CreditorPK = testCreditor.PK;
				DataRegistry.Business.ZACustomsRegistry.Instance.DSBCreditors.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);
				Factory.Save();

				var notificationCollector = new MessageNotificationCollector_ForTest();
				var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
				var processor = new InboundInterchangeProcessor(logger);
				var messageProcessor = new ZACIncomingMessageProcessor();
				messageProcessor.Logger = new LoggingInformation();
				var ahhhShutUp = new SendsMessagesToCustomsShutterUpperer();

				var chargeCode = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB"));
				RatingDataRegistry.Instance.CustomsDisbursementChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
				RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCreditor.PK.ToGuid());
				Factory.Save();

				var factory = new BusinessObjectFactory();
				var declaration = factory.New<JobDeclaration>();
				var orgProxy = declaration.Branch.Company.OrgProxy;
				orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "12345678", Constants.CountryCodes.SouthAfrica);
				factory.Save();

				var agentOfficeToCreditorMappings = new FinancialAccountNumberPortMapCollection();
				var mapping1 = agentOfficeToCreditorMappings.AddNew();
				mapping1.OrganizationPK = orgProxy.PK;
				mapping1.CustomsOfficeCode = "JHB";
				mapping1.FinancialAccountNumber = "1234567890";
				mapping1.CreditorPK = testCreditor.PK;
				mapping1.ImporterPays = false;
				mapping1.AccountStartDay = 1;
				ZACustomsRegistry.Instance.FinancialAccountNumberPortMaps.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, agentOfficeToCreditorMappings);

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_CustomsOffice = "JHB";
				declaration.JE_TransportMode = "AIR";
				declaration.JE_OH_Supplier = testSupplier.PK;
				declaration.JE_OH_Importer = testImporter.PK;

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "11";

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV0001";
				invoice.JZ_RX_NKInvoice_Currency = Constants.CurrencyCodes.SouthAfrica;
				invoice.JZ_InvoiceAmount = 10000m;

				var line = invoice.InvoiceLines.AddNew();
				line.JI_Tariff = "321321321";
				line.JI_Procedure = "1100";
				line.JI_PrimaryPreference = "100";
				line.JI_CEI = entryInstruction.PK;
				line.JI_CountryOfOrigin = Constants.CountryCodes.Italy;
				line.JI_ZZF_NKTaxType = "VAT";
				line.JI_LinePrice = 10000m;

				declaration.DoMerge(ahhhShutUp);
				AssertEquals(1, declaration.ActiveEntryHeaders.Count);
				var entry = declaration.ActiveEntryHeaders[0];
				factory.Save();
				Assert("CH_BGMReference should contain a value", !entry.CH_BGMReference.IsEmpty);

				var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
				var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

				AssertVOCDutyValuesAutoPostedSendInitialDec(ref factory, ref entry, ref declaration, processor, messageProcessor, messageManager);
				AssertVOCDutyValuesAutoPostedIntroduceDuty(ref factory, ref entry, ref declaration, ahhhShutUp, processor, messageProcessor, notificationCollector);
			}
		}

		void AssertVOCDutyValuesAutoPostedSendInitialDec(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageManagers.MessageManager messageManager)
		{
			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 1 Msg", 1, entry.Messages.Count);
			AssertNotNull("Declaration should have a job", declaration.Job);

			var outgoingMsg1 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg1.EM_MessageNum = "1001";
			outgoingMsg1.EM_Status = "SNT";

			var ctrlInt1 = GetZACInterchange(factory, "0000001", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt1.EI_ApplicationCode = "ZAC";
			ctrlInt1.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1111++CONTRL+++TESTINGWTG'";
			ctrlInt1.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+00626166WTG::N5L3X7O8Y2J5W7G5:WTGAS2+SARSDEC+7'UCM+1001+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt1.EI_FooterText = "UNZ+1+1111'";

			var cusInt1 = GetZACInterchange(factory, "0000002", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt1.EI_ApplicationCode = "ZAC";
			cusInt1.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1112++CUSRES+++TESTINGWTG'";
			cusInt1.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA051+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+1:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20190919:102'RFF+AAS:125-88155185'DTM+137:20190919:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1001'TAX+3+CUS:107:ZZZ'MOA+161:70561'CNT+7:1.00'CNT+11:1'UNT+21+1'";
			cusInt1.EI_FooterText = "UNZ+1+1112'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 3 Msgs", 3, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("First Charge", () =>
			{
				AssertEquals(1, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/UTVAT", invoiceJob.Charges[0].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(1650m), invoiceJob.Charges[0].JR_LocalSellAmt);
				AssertEquals(expected: true, invoiceJob.Charges[0].IsCostPosted);
			});
		}

		void AssertVOCDutyValuesAutoPostedIntroduceDuty(ref BusinessObjectFactory factory, ref CusEntryHeader entry, ref JobDeclaration declaration, SendsMessagesToCustomsShutterUpperer ahhhShutUp, InboundInterchangeProcessor processor, ZACIncomingMessageProcessor messageProcessor, MessageNotificationCollector_ForTest notificationCollector)
		{
			declaration.InvoiceLines[0].JI_Tariff = "123123123";
			declaration.DoMerge(ahhhShutUp);
			factory.Save();

			var objectParent = new JobDeclarationMessageSendingObjectParent(declaration);
			var sendingObject = objectParent.SendingObjectsCollection[0];
			sendingObject.ShouldSend = true;
			sendingObject.MessageType = MessageSubTypeCodes.Codes.Change;
			var messageManager = new MessageManagers.MessageManager(objectParent, notificationCollector);

			messageManager.SendMessages();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 4 Msgs", 4, entry.Messages.Count);

			var outgoingMsg2 = entry.Messages.OfType<CUSDECEDIMessage>().Where(x => x.EM_MessageType == "DEC" && x.EM_Status == EDIMessage.Status.Queued && x.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Transmit).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
			outgoingMsg2.EM_MessageNum = "1002";
			outgoingMsg2.EM_Status = "SNT";

			var ctrlInt2 = GetZACInterchange(factory, "0000003", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			ctrlInt2.EI_ApplicationCode = "ZAC";
			ctrlInt2.EI_HeaderText = "UNB+UNOB:4+SARSCAR+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1113++CONTRL+++TESTINGWTG'";
			ctrlInt2.EI_BodyText = "UNH+1+CONTRL:D:3:UN:CONTRL'UCI+24687+12345678WTG::ABCDEFGHIJKLMNOP:WTGAS2+SARSDEC+7'UCM+1002+CUSDEC:D:96B:UN:ZZZ01+7'UNT+4+1'";
			ctrlInt2.EI_FooterText = "UNZ+1+1113'";

			var cusInt2 = GetZACInterchange(factory, "0000004", EDIInterchange.Direction.Receive, EDIInterchange.Status.Queued);
			cusInt2.EI_ApplicationCode = "ZAC";
			cusInt2.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::ABCDEFGHIJKLMNOP:WTGAS2+20190919:1021+1114++CUSRES+++TESTINGWTG'";
			cusInt2.EI_BodyText = $"UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+{entry.CH_BGMReference}:0'DTM+178:20190919:102'DTM+202:20190919:102'TDT+20+BA057+4+++++::: 'LOC+22+JSA::ZZZ'LOC+14+L4::ZZZ'GIS+27:120:ZZZ:N'NAD+AG+00626166'RFF+BH:12345678TSFO4216504'DTM+137:20181127:102'RFF+AAS:125-88155185'DTM+137:20181127:102'RFF+ABT:JHB201909180000002'DTM+137:20190919:102'RFF+ACD:1002'TAX+3+CUS:107:ZZZ'MOA+161:88518'CNT+7:1.00'CNT+11:1'UNT+20+1'";
			cusInt2.EI_FooterText = "UNZ+1+1114'";

			factory.Save();
			processor.ExecuteBatch();
			messageProcessor.ExecuteBatch();
			factory.Save();

			factory = new BusinessObjectFactory();
			declaration = factory.Load<JobDeclaration>(declaration.PK);
			entry = factory.Load<CusEntryHeader>(entry.PK);
			AssertEquals("Entry should have 6 Msgs", 6, entry.Messages.Count);
			var invoiceJob = Factory.Load<Accounting.Business.JobInvoicing.Job>(declaration.Job.PK);

			CombineAssertions("Duty should be posted", () =>
			{
				AssertEquals(3, invoiceJob.Charges.Count);
				AssertEquals("JHB201909180000002/UTDTY", invoiceJob.Charges[1].JR_APInvoiceNum);
				AssertEquals(new ZDecimal(2000m), invoiceJob.Charges[1].JR_LocalSellAmt);
				AssertEquals(expected: true, invoiceJob.Charges[1].IsCostPosted);
			});
		}

		static void AssertVOCAfterValues(CusEntryHeader entry, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDuty, ZDecimal s1p2BDuty, ZDecimal valueAddedTax, ZInt expectedCount)
		{
			AssertEquals("entry.VoucherOfCorrectionValueAfters.Count", expectedCount, entry.VoucherOfCorrectionValueAfters.Count);
			AssertVOCAfterValues(entry, cifValue, customsValue, customsDuty, s1p2BDuty, valueAddedTax);
		}

		static void AssertVOCAfterValues(CUSDECEDIMessage message, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDuty, ZDecimal s1p2BDuty, ZDecimal valueAddedTax, ZInt expectedCount)
		{
			AssertEquals("message.VoucherOfCorrectionValueAfters.Count", expectedCount, message.VoucherOfCorrectionValueAfters.Count);
			AssertVOCAfterValues(message, cifValue, customsValue, customsDuty, s1p2BDuty, valueAddedTax);
		}

		static void AssertVOCAfterValues(IVOCAfterValues vocValues, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDutyNoS1P2B, ZDecimal s1p2BDuty, ZDecimal valueAddedTax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IVOCAfterValues.CIFValue", cifValue, vocValues.CIFValue);
				AssertEquals("IVOCAfterValues.CustomsValue", customsValue, vocValues.CustomsValue);
				AssertEquals("IVOCAfterValues.CustomsDutyNoS1P2B", customsDutyNoS1P2B, vocValues.CustomsDutyNoS1P2B);
				AssertEquals("IVOCAfterValues.S1P2BDuty", s1p2BDuty, vocValues.S1P2BDuty);
				AssertEquals("IVOCAfterValues.ValueAddedTax", valueAddedTax, vocValues.ValueAddedTax);
			});
		}

		static void AssertVOCBeforeValues(CusEntryHeader entry, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDuty, ZDecimal s1p2BDuty, ZDecimal valueAddedTax, ZInt expectedCount)
		{
			AssertEquals("entry.VoucherOfCorrectionValueBefores.Count", expectedCount, entry.VoucherOfCorrectionValueBefores.Count);
			AssertVOCBeforeValues(entry, cifValue, customsValue, customsDuty, s1p2BDuty, valueAddedTax);
		}

		static void AssertVOCBeforeValues(CUSDECEDIMessage message, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDuty, ZDecimal s1p2BDuty, ZDecimal valueAddedTax, ZInt expectedCount)
		{
			AssertEquals("message.VoucherOfCorrectionValueBefores.Count", expectedCount, message.VoucherOfCorrectionValueBefores.Count);
			AssertVOCBeforeValues(message, cifValue, customsValue, customsDuty, s1p2BDuty, valueAddedTax);
		}

		static void AssertVOCBeforeValues(IVOCBeforeValues vocValues, ZDecimal cifValue, ZDecimal customsValue, ZDecimal customsDuty, ZDecimal s1p2BDuty, ZDecimal valueAddedTax)
		{
			CombineAssertions(() =>
			{
				AssertEquals("IVOCBeforeValues.CIFValue", cifValue, vocValues.CIFValue);
				AssertEquals("IVOCBeforeValues.CustomsValue", customsValue, vocValues.CustomsValue);
				AssertEquals("IVOCBeforeValues.CustomsDuty", customsDuty, vocValues.CustomsDutyNoS1P2B);
				AssertEquals("IVOCBeforeValues.S1P2BDuty", s1p2BDuty, vocValues.S1P2BDuty);
				AssertEquals("IVOCBeforeValues.ValueAddedTax", valueAddedTax, vocValues.ValueAddedTax);
			});
		}

		public void TestResetTargetEntryLineNumberAfterRejectedAndNotVOCEntry()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("6");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "11";
			declaration.Invoices.RemoveAll();
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "INV001";
			invHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.ResumeApportionment();
			invHeader.JobComInvoiceLines.RemoveAndDeleteAll();

			var invLine = invHeader.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = entryInstruction.PK;
			invLine.JI_Procedure = invLine.EntryInstruction.CEI_Style + "00";
			invLine.JI_TargetEntryLineNumber = 1;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			Factory.Save();

			var cusEntryHeader = declaration.ActiveEntryHeaders[0];
			cusEntryHeader.CH_BGMReference = "VICTEST";
			_ = GetOutgoingMessage(cusEntryHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", cusEntryHeader.CH_BGMReference), "202", "ORG");
			Factory.Save();

			var testMessage = GetMesssageForTest(GetTestMessageResNo("6").Replace("00626166CLP20160426000199", cusEntryHeader.CH_BGMReference), "IN1", new ZDateTime(2016, 9, 29));
			_ = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();

			AssertEquals("JobComInvoiceLine should have TargetEntryLineNumber=1", (ZShort)1, invLine.JI_TargetEntryLineNumber);

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			AssertEquals("JobComInvoiceLine should have TargetEntryLineNumber=1 after receive a rejected message but has VOC entry", (ZShort)1, invLine.JI_TargetEntryLineNumber);

			entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
			testMessage.EM_MessageText = GetTestMessageResNo("6").Replace("00626166CLP20160426000199", cusEntryHeader.CH_BGMReference).Replace("DTM+137:20160401:102'", "");
			testMessage.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();
			logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			PreAndProcessMessage(logger, testMessage);
			Factory.Save();
			AssertEquals("JobComInvoiceLine should have TargetEntryLineNumber=0 after receive a rejected message and not VOC entry", (ZShort)0, invLine.JI_TargetEntryLineNumber);
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestStatus_13_CusresMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("13");
			Factory.Save();

			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
			outgoingMessage.EM_MessageNum = "202";

			var testMessage0 = GetMesssageForTest(TestMessage_Status_13_CusresMessage.Replace("\r\n", ""), "IN0");
			_ = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
			AssertEquals("Prerequisite: No case numbers must exist when we start.", 0, testHeader.EntryInstruction.CaseNumbers.Count);

			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			AssertEquals("After processing, 1 case number is expected.", 1, testHeader.EntryInstruction.CaseNumbers.Count);

			var caseNumber = testHeader.EntryInstruction.CaseNumbers.ToArray()[0] as CaseNumber;
			Assert("CusCodeData object must not be null.", caseNumber != null);
			AssertEquals("SUP", caseNumber.CY_Code);
			AssertEquals("131313138", caseNumber.CY_Data);
			AssertEquals("CEI", caseNumber.CY_ParentTableCode);
			AssertEquals("CAS", caseNumber.CY_Type);

			AssertStatus_CusresMessage_EmailNotification("'Code 13 - Query - Supporting documents required', B0000100X");
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestStatus_14_CusresMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("14");
			Factory.Save();

			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_LinkUniqueID = testHeader.PK;
			outgoingMessage.EM_LinkTable = testHeader.TableName;
			outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
			outgoingMessage.EM_MessageNum = "202";

			var testMessage0 = GetMesssageForTest(TestMessage_Status_14_CusresMessage.Replace("\r\n", ""), "IN0");
			_ = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
			AssertEquals("Prerequisite: No case numbers must exist when we start.", 0, testHeader.EntryInstruction.CaseNumbers.Count);

			logger.ClearLogs();
			PreAndProcessMessage(logger, testMessage0);
			AssertEquals("After processing, 1 case number is expected.", 1, testHeader.EntryInstruction.CaseNumbers.Count);

			var caseNumber = testHeader.EntryInstruction.CaseNumbers.ToArray()[0] as CaseNumber;
			Assert("CusCodeData object must not be null.", caseNumber != null);
			AssertEquals("SUP", caseNumber.CY_Code);
			AssertEquals("105385028", caseNumber.CY_Data);
			AssertEquals("CEI", caseNumber.CY_ParentTableCode);
			AssertEquals("CAS", caseNumber.CY_Type);

			AssertStatus_CusresMessage_EmailNotification("'Code 14 - Notice To Upload Supporting Cargo Clearance / Customs Clearance Information', B0000100X");
		}

		void AssertStatus_CusresMessage_EmailNotification(string subject)
		{
			var subjectForEmail = $"Entry Notification: {subject}";
			var query = new ZQuery();
			query.AddToFilter(MailDBItemsSchema.MI_Subject, subjectForEmail);
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
			var email = Factory.LoadTop1<MailItem>(query);

			AssertNotNull("An e-mail must have been created.", email);
			AssertContains("email1@wisetechglobal.com", email.AllRecipients);
			AssertContains(subjectForEmail, email.MI_Body);
			AssertContains("Shipment Type", email.MI_Body);
			AssertContains("Customs Office", email.MI_Body);
			AssertContains("Importer", email.MI_Body);
			AssertContains("Main Supplier", email.MI_Body);
			AssertContains("Depot", email.MI_Body);
			AssertContains("CPC", email.MI_Body);
			AssertNotNull(email.MailAttachments);
			AssertEquals("Three e-mail attachments are expected.", 3, email.MailAttachments.Count);
		}

		public void TestStatus_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("14");
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var manifestHeader = Factory.New<ZAManifest.IAsycudaManifestHeader>();
				manifestHeader.AMA_RN_NKCountry = countryCode;
				manifestHeader.AMA_ApplicationCode = "OUT";
				manifestHeader.AMA_MasterBill = "MAN0123";

				var baseHeader = manifestHeader as ManifestBase.AsycudaManifestHeader;
				var bill = baseHeader.Bills.AddNew();
				bill.ABL_BillNumber = "HB234";

				var outgoingMessage = GetOutgoingMessage(baseHeader, TestOutgoingMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader.Replace("\r\n", ""), "7615", "CHG");
				outgoingMessage.EM_MessageType = "CAR";
				outgoingMessage.EM_MessageNum = "7615";

				var testMessage = GetMesssageForTest(TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader.Replace("\r\n", ""));
				AssertEquals("14", testMessage.CUSRESHelper.EntryStatus);
				var interchangeIn = SetInterchangeForMesssage(TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader, testMessage);

				PreAndProcessMessage(logger, testMessage);
				Factory.Save();

				var savedManifest = Factory.Load<ZAManifest.IAsycudaManifestHeader>(manifestHeader.PK);
				AssertEquals("Customs Status", "14", savedManifest.RegistrationStatus);

				var baseHeader2 = manifestHeader as ManifestBase.AsycudaManifestHeader;
				var savedAsycudaBill = baseHeader2.Bills[0];

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, baseHeader2.PK);
				AssertEquals("Message Added", 2, Factory.Load<EDIMessage>(query).Length);

				var providerHeader = (ICaseNumberCollectionProvider)savedManifest;
				var providerBill = (ICaseNumberCollectionProvider)savedAsycudaBill;
				AssertEquals("Must have 1 case number.", 1, providerHeader.CaseNumbers.Count);
				AssertEquals("Must have 0 case numbers.", 0, providerBill.CaseNumbers.Count);

				var caseNumber = providerHeader.CaseNumbers[0];
				AssertEquals("Store the case number from SARS.", "105394631", caseNumber.CY_Data);
				AssertEquals("CusCodeData.CY_Code", "SUP", caseNumber.CY_Code);
				AssertEquals("CusCodeData.CY_Type", "CAS", caseNumber.CY_Type);
				AssertEquals("CusCodeData.CY_ParentTableCode", "AMA", caseNumber.CY_ParentTableCode);
				AssertEquals("Link CusCodeData.CY_ParentID to correct PK (GUID)", savedManifest.PK, caseNumber.CY_ParentID);
			}
		}

		public void TestStatus_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("14");
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var manifestHeader = Factory.New<ZAManifest.IAsycudaManifestHeader>();
				manifestHeader.AMA_RN_NKCountry = countryCode;
				manifestHeader.AMA_ApplicationCode = "OUT";
				manifestHeader.AMA_MasterBill = "MAN0123";

				var baseHeader = manifestHeader as ManifestBase.AsycudaManifestHeader;
				var asycudaBill = baseHeader.Bills.AddNew();
				asycudaBill.ABL_BillNumber = "HB234";
				AssertEquals("Prerequisite: AsycudaBill must have a manifiest header.", manifestHeader.PK, asycudaBill.Header.PK);

				var outgoingMessage = GetOutgoingMessage(asycudaBill, TestOutgoingMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill.Replace("\r\n", ""), "7616", "CHG");
				outgoingMessage.EM_MessageType = "CAR";
				outgoingMessage.EM_MessageNum = "7616";

				var testMessage = GetMesssageForTest(TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill.Replace("\r\n", ""));
				AssertEquals("14", testMessage.CUSRESHelper.EntryStatus);

				var interchangeIn = SetInterchangeForMesssage(TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill, testMessage);
				PreAndProcessMessage(logger, testMessage);
				Factory.Save();

				var savedManifest = Factory.Load<ZAManifest.IAsycudaManifestHeader>(manifestHeader.PK);
				var baseHeader3 = manifestHeader as ManifestBase.AsycudaManifestHeader;
				var savedAsycudaBill = baseHeader3.Bills[0];
				AssertEquals("Prerequisite/Sanity Check: ", asycudaBill.PK, savedAsycudaBill.PK);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, savedAsycudaBill.PK);
				AssertEquals("Message Added", 2, Factory.Load<EDIMessage>(query).Length);
				AssertEquals("Customs Status", "14", savedAsycudaBill.ABL_BillStatus);

				var providerHeader = (ICaseNumberCollectionProvider)savedManifest;
				var providerBill = (ICaseNumberCollectionProvider)savedAsycudaBill;
				AssertEquals("Must have 0 case numbers.", 0, providerHeader.CaseNumbers.Count);
				AssertEquals("Must have 1 case number.", 1, providerBill.CaseNumbers.Count);

				var caseNumber = providerBill.CaseNumbers[0];
				AssertEquals("Store the case number from SARS.", "105394632", caseNumber.CY_Data);
				AssertEquals("CusCodeData.CY_Code", "SUP", caseNumber.CY_Code);
				AssertEquals("CusCodeData.CY_Type", "CAS", caseNumber.CY_Type);
				AssertEquals("CusCodeData.CY_ParentTableCode", "ABL", caseNumber.CY_ParentTableCode);
				AssertEquals("Link CusCodeData.CY_ParentID to correct PK (GUID)", savedAsycudaBill.PK, caseNumber.CY_ParentID);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdate_CusEntryPayInfo_PaymentDate_When_Status_8_CUSRES_Message()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("8");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var linkNo = "124";
				testHeader.EntryNumber = linkNo;

				var payInfo1 = testHeader.EntryPayInfos.AddNew();
				payInfo1.C9_PaymentAmount = 100m;
				payInfo1.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo1.C9_PaymentDate = new ZDateTime(1991, 01, 01);
				payInfo1.C9_IncomingPayResponseNo = linkNo;

				var payInfo2 = testHeader.EntryPayInfos.AddNew();
				payInfo2.C9_PaymentAmount = 200m;
				payInfo2.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo2.C9_PaymentDate = new ZDateTime(1992, 02, 02);
				payInfo2.C9_IncomingPayResponseNo = linkNo;

				var msgBelong = "Prerequisite: CusEntryPayInfo must belong to CusEntryHeader.";
				AssertEquals(msgBelong, linkNo, payInfo1.C9_IncomingPayResponseNo);
				AssertEquals(msgBelong, linkNo, payInfo2.C9_IncomingPayResponseNo);

				var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
				outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
				outgoingMessage.EM_MessageType = "DEC";
				outgoingMessage.EM_ApplicationCode = "ZAC";
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage.EM_LinkTable = testHeader.TableName;
				outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage.EM_MessageNum = "124";

				var testMessage0 = GetMesssageForTest(TestMessage_Update_CusEntryPayInfo_PaymentDate_When_Status_8_CUSRES_Message.Replace("\r\n", ""), "IN0");
				var testInterchange = SetInterchangeForMesssage(TestMessage_Update_CusEntryPayInfo_PaymentDate_When_Status_8_CUSRES_Message, testMessage0);

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);

				AssertEquals("AWR", payInfo1.C9_PaymentStatus);
				AssertEquals("AWR", payInfo2.C9_PaymentStatus);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo1.C9_PaymentDate);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo2.C9_PaymentDate);
				AssertEquals("124", payInfo1.C9_IncomingPayResponseNo);
				AssertEquals("124", payInfo2.C9_IncomingPayResponseNo);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdate_CusEntryPayInfo_When_Status_AWR_CUSRES_Message()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("8");
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var linkNo = "124";
				testHeader.EntryNumber = linkNo;

				var payInfo1 = testHeader.EntryPayInfos.AddNew();
				payInfo1.C9_PaymentAmount = 100m;
				payInfo1.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo1.C9_PaymentDate = new ZDateTime(1991, 01, 01);
				payInfo1.C9_IncomingPayResponseNo = linkNo;

				var payInfo2 = testHeader.EntryPayInfos.AddNew();
				payInfo2.C9_PaymentAmount = 200m;
				payInfo2.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo2.C9_PaymentDate = new ZDateTime(1992, 02, 02);
				payInfo2.C9_IncomingPayResponseNo = linkNo;

				var msgBelong = "Prerequisite: CusEntryPayInfo must belong to CusEntryHeader.";
				AssertEquals(msgBelong, linkNo, payInfo1.C9_IncomingPayResponseNo);
				AssertEquals(msgBelong, linkNo, payInfo2.C9_IncomingPayResponseNo);

				var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
				outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
				outgoingMessage.EM_MessageType = "DEC";
				outgoingMessage.EM_ApplicationCode = "ZAC";
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage.EM_LinkTable = testHeader.TableName;
				outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage.EM_MessageNum = "124";

				var testMessage0 = GetMesssageForTest(TestMessage_Update_CusEntryPayInfo_When_Status_AWR_CUSRES_Message.Replace("\r\n", ""), "IN0");
				var testInterchange = SetInterchangeForMesssage(TestMessage_Update_CusEntryPayInfo_When_Status_AWR_CUSRES_Message, testMessage0);

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);

				AssertEquals("AWR", payInfo1.C9_PaymentStatus);
				AssertEquals("AWR", payInfo2.C9_PaymentStatus);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo1.C9_PaymentDate);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo2.C9_PaymentDate);
				AssertEquals("124", payInfo1.C9_IncomingPayResponseNo);
				AssertEquals("124", payInfo2.C9_IncomingPayResponseNo);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdate_CusEntryPayInfo_When_Status_CLR_CUSRES_Message()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("8");
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var linkNo = "125";

				var payInfo3 = testHeader.EntryPayInfos.AddNew();
				payInfo3.C9_PaymentAmount = 300m;
				payInfo3.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo3.C9_PaymentDate = new ZDateTime(1991, 01, 01);
				payInfo3.C9_IncomingPayResponseNo = linkNo;

				var payInfo4 = testHeader.EntryPayInfos.AddNew();
				payInfo4.C9_PaymentAmount = 400m;
				payInfo4.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo4.C9_PaymentDate = new ZDateTime(1992, 02, 02);
				payInfo4.C9_IncomingPayResponseNo = linkNo;

				var msgBelong = "Prerequisite: CusEntryPayInfo must belong to CusEntryHeader.";
				AssertEquals(msgBelong, linkNo, payInfo3.C9_IncomingPayResponseNo);
				AssertEquals(msgBelong, linkNo, payInfo4.C9_IncomingPayResponseNo);

				AssertEquals(msgBelong, linkNo, payInfo3.C9_IncomingPayResponseNo);
				AssertEquals(msgBelong, linkNo, payInfo4.C9_IncomingPayResponseNo);

				var outgoingMessage2 = CreateOriginalMessageAndBusinessObjects();
				outgoingMessage2.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 04);
				outgoingMessage2.EM_MessageType = "DEC";
				outgoingMessage2.EM_ApplicationCode = "ZAC";
				outgoingMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage2.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage2.EM_LinkTable = testHeader.TableName;
				outgoingMessage2.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage2.EM_MessageNum = "125";

				var testMessage1 = GetMesssageForTest(TestMessage_Update_CusEntryPayInfo_When_Status_CLR_CUSRES_Message.Replace("\r\n", ""), "IN1");
				_ = SetInterchangeForMesssage(TestMessage_Update_CusEntryPayInfo_When_Status_CLR_CUSRES_Message, testMessage1);

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage1);

				AssertEquals("CLR", payInfo3.C9_PaymentStatus);
				AssertEquals("CLR", payInfo4.C9_PaymentStatus);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo3.C9_PaymentDate);
				AssertEquals(new ZDateTime(2019, 10, 02), payInfo4.C9_PaymentDate);
				AssertEquals("125", payInfo3.C9_IncomingPayResponseNo);
				AssertEquals("125", payInfo4.C9_IncomingPayResponseNo);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdate_CusEntryPayInfo_PaymentStatus_When_Status_1_CUSRES_Message()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var linkNo = "124";
				testHeader.EntryNumber = linkNo;

				var payInfo1 = testHeader.EntryPayInfos.AddNew();
				payInfo1.C9_PaymentAmount = 100m;
				payInfo1.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo1.C9_PaymentDate = new ZDateTime(1991, 01, 01);
				payInfo1.C9_IncomingPayResponseNo = linkNo;

				var payInfo2 = testHeader.EntryPayInfos.AddNew();
				payInfo2.C9_PaymentAmount = 200m;
				payInfo2.C9_PaymentStatus = CusEntryPayTypes.Pending;
				payInfo2.C9_PaymentDate = new ZDateTime(1992, 02, 02);
				payInfo2.C9_IncomingPayResponseNo = linkNo;

				var msgBelong = "Prerequisite: CusEntryPayInfo must belong to CusEntryHeader.";
				AssertEquals(msgBelong, linkNo, payInfo1.C9_IncomingPayResponseNo);
				AssertEquals(msgBelong, linkNo, payInfo2.C9_IncomingPayResponseNo);

				var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
				outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
				outgoingMessage.EM_MessageType = "DEC";
				outgoingMessage.EM_ApplicationCode = "ZAC";
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage.EM_LinkTable = testHeader.TableName;
				outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage.EM_MessageNum = "124";

				var testMessage0 = GetMesssageForTest(TestMessage_Update_CusEntryPayInfo_PaymentStatus_When_Status_1_CUSRES_Message.Replace("\r\n", ""), "IN0");
				var testInterchange = SetInterchangeForMesssage(TestMessage_Update_CusEntryPayInfo_PaymentStatus_When_Status_1_CUSRES_Message, testMessage0);
				testInterchange.EI_InterchangeNum = "INT1";

				logger.ClearLogs();
				PreAndProcessMessage(logger, testMessage0);

				AssertEquals("CLR", payInfo1.C9_PaymentStatus);
				AssertEquals("CLR", payInfo2.C9_PaymentStatus);
				AssertEquals(new ZDateTime(1991, 01, 01), payInfo1.C9_PaymentDate);
				AssertEquals(new ZDateTime(1992, 02, 02), payInfo2.C9_PaymentDate);
				AssertEquals("124", payInfo1.C9_IncomingPayResponseNo);
				AssertEquals("124", payInfo2.C9_IncomingPayResponseNo);

				var logText = logger.LogMessages.ToString();
				AssertContains("Log", "Information: 	Linking CUSRES Message: #IN0/INT1 to job: 00626166CLP20160426000199", logText);
				AssertContains("Log", "Information: 	Entry Status of Entry:00626166CLP20160426000199 has been updated to '1'.", logText);
				AssertContains("Log", "Information: 	Email sent.", logText);
				AssertContains("Log", "SUBJECT: Entry Notification: 'Code 1 - Release', B0000100X", logText);
				AssertContains("Log", "Information: 	Entry Pay Info payment status updated from 'PEN' to 'CLR' on Customs Entry: 00626166CLP20160426000199", logText);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestBondedWarehouseIncomingResponseMessage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			var customsStatusCode1 = testHelper.CreateCustomsStatusCusCodeEntry("1");

			testHelper.CreateNewOrGetExistingRefCusCodeListAttributeName("IUpdateBondedWhs", "Causes bonded warehouse inventory to be updated", "CSTA", Core.Constants.CountryCodes.SouthAfrica);
			customsStatusCode1.Attributes.AddNew("IUpdateBondedWhs", "");
			Factory.Save();

			var helper = new ZAWhsDataTestHelper(Factory);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				var entry = helper.GetNewEntryHeader(ZAJobMessageTypeList.Codes.Import, "BZA00001230", helper.InwardCusProcedure.ZZ6_ProcedureCode, "ENT3243", helper.InwardCusProcedure.ZZ6_PreviousProcedureCode, 100m);
				entry.CH_BGMReference = "00626166CLP20160426000199";
				entry.CH_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.InwardCreatedPending;

				var entryInstruction = entry.EntryInstruction;
				entryInstruction.CEI_OA_Warehouse = ZGuid.Empty;
				entryInstruction.CEI_Style = "DS";
				entryInstruction.CEI_Description = "HELLO";
				entryInstruction.CEI_Style = helper.InwardCusProcedure.ZZ6_ProcedureCode;

				AssertEquals("Prerequisite: Empty Entry Release date before Release ", ZDateTime.Empty, entry.CH_EntryReleaseDate);
				AssertEquals("Prerequisite: Must support Bonded Warehousing", expected: true, entry.SupportsBondedWarehousing);
				AssertEquals("Prerequisite: Must have Inward Bonded Warehousing Enabled", expected: true, entry.IsInwardBondedWarehousingEnabled);
				AssertEquals("Prerequisite: Must not have Outward Bonded Warehousing Enabled", expected: false, entry.IsOutwardBondedWarehousingEnabled);

				var outgoingMessage = GetOutgoingMessage(entry, TestOutGoingMessage.Replace("\r\n", ""), "202", "ORG");
				outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
				outgoingMessage.EM_MessageType = "DEC";
				outgoingMessage.EM_ApplicationCode = "ZAC";
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_LinkUniqueID = entry.PK;
				outgoingMessage.EM_LinkTable = entry.TableName;
				outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage.EM_MessageNum = "202";

				var interchangeIn = Factory.NewWithValidTestData<ZACInterchange>();
				interchangeIn.EI_InterchangeNum = "00000000000000000044";
				interchangeIn.EI_HeaderText = $@"UNB+UNOB:4+SARSCART+ZS::WTGTESTDEPOTSEA1:WTGAS2+20180511:0653+150++CUSRES'";

				var testMessage0 = GetMesssageForTest(TestMessage_BondedWarehouseIncomingResponseMessage.Replace("\r\n", ""), "IN0");
				testMessage0.EM_EI = interchangeIn.PK;
				var testInterchange = SetInterchangeForMesssage(TestMessage_BondedWarehouseIncomingResponseMessage, testMessage0);

				PreAndProcessMessage(logger, testMessage0);
				Factory.Save();

				var allMessages = logger.LogMessages.ToString();
				AssertNotContains("Error Processing Incoming EDI Message", allMessages);
				AssertNotContains("Data Context for the WarehouseCustomsEntry data object was empty", allMessages);
				AssertContains(string.Format("Linking CUSRES Message: #IN0/{0} to job: 00626166CLP20160426000199", testInterchange.EI_InterchangeNum), allMessages);
				AssertEquals("Entry Release date set from MRN DBN201910045000214", new ZDateTime(2019, 10, 04, 00, 00, 00), entry.CH_EntryReleaseDate);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestUpdate_CaseNumber_Date_When_Status_34_CUSRES_Message()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("34");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				var caseNumberProvider = (ICaseNumberCollectionProvider)testHeader.EntryInstruction;
				var caseNumber1 = caseNumberProvider.CaseNumbers.AddNew();
				caseNumber1.CY_Code = "SUP";
				caseNumber1.CY_Type = "CAS";
				caseNumber1.CY_Data = "111000444";
				var caseNumber2 = caseNumberProvider.CaseNumbers.AddNew();
				caseNumber2.CY_Code = "SUP";
				caseNumber2.CY_Type = "CAS";
				caseNumber2.CY_Data = "105389559";

				var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
				outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
				outgoingMessage.EM_MessageType = "DEC";
				outgoingMessage.EM_ApplicationCode = "ZAC";
				outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				outgoingMessage.EM_LinkUniqueID = testHeader.PK;
				outgoingMessage.EM_LinkTable = testHeader.TableName;
				outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
				outgoingMessage.EM_MessageNum = "124";

				var testMessage0 = GetMesssageForTest(TestMessage_Update_CaseNumber_Date_When_Status_34_CUSRES_Message.Replace("\r\n", ""), "IN0");
				var interchange = SetInterchangeForMesssage(TestMessage_Update_CaseNumber_Date_When_Status_34_CUSRES_Message, testMessage0);
				interchange.EI_SystemCreateTimeUtc = ZDateTime.Now;
				interchange.EI_IsActive = ZBool.True;
				interchange.EI_ApplicationCode = "ZAC";
				interchange.EI_InterchangeType = "ZAC";
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				interchange.EI_From = "SARS";
				interchange.EI_To = "WTG";
				interchange.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166::AAAAAAAAAAAAAABB:WTGAS2+190825:1224+2271++CUSRES+++WTG'";
				interchange.EI_BodyText = TestMessage_Update_CaseNumber_Date_When_Status_34_CUSRES_Message.Replace("\r\n", "");
				interchange.EI_FooterText = "UNZ+1+2271'";
				interchange.EI_InterchangeNum = "INT1";

				var unbTimeStamp = new ZDateTime(2019, 08, 25, 12, 24, 0);
				AssertEquals("Prerequisite: Valid UNB Date", unbTimeStamp, testMessage0.PreparationDate);
				PreAndProcessMessage(logger, testMessage0);
				AssertEquals(ZDateTime.Empty, caseNumber1.CY_Date);
				AssertEquals(unbTimeStamp, caseNumber2.CY_Date);

				var logText = logger.LogMessages.ToString();
				AssertContains("Log", "Information: 	Linking CUSRES Message: #IN0/INT1 to job: 00626166CLP20160426000199", logText);

				caseNumber2.CY_Date = ZDateTime.Empty;
				interchange.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166::AAAAAAAAAAAAAABB:WTGAS2+20190825:1224+2271++CUSRES+++WTG'";
				AssertEquals("Prerequisite: Valid UNB Date", unbTimeStamp, testMessage0.PreparationDate);
				PreAndProcessMessage(logger, testMessage0);
				AssertEquals(ZDateTime.Empty, caseNumber1.CY_Date);
				AssertEquals(unbTimeStamp, caseNumber2.CY_Date);
			}
		}

		[TestDate(2016, 04, 02, 03, 04, 05)]
		public void TestStatus_8_CusresCalinf_Message_for_JobVoyage()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("8");
			Factory.Save();

			var countryCode = Core.Constants.CountryCodes.SouthAfrica;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var jobVoyage = GetJobVoyageForTest();
				var outgoingMessage = GetOutgoingMessage(jobVoyage, TestOutgoingMessage_JobVoyage.Replace("\r\n", ""), "15", "ORG");
				outgoingMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CALINF;
				outgoingMessage.EM_MessageNum = "15";
				outgoingMessage.EM_Status = "SNT";
				AssertEquals("Prerequisite: EDIMessage.EM_LinkTable", "JobVoyage", outgoingMessage.EM_LinkTable);
				AssertEquals("Prerequisite: EDIMessage.EM_LinkUniqueID", jobVoyage.PK, outgoingMessage.EM_LinkUniqueID);
				Factory.Save();

				var calinfMessage = Factory.Load<CALINFEDIMessage>(outgoingMessage.PK);
				AssertEquals("Prerequisite: calinfMessage.ParentMessageNumber", "8875A18DBCA44B0486F8AEEA0F3242BD", calinfMessage.ParentMessageNumber);

				var commonAccessReference = outgoingMessage.PK.ToString().Replace("-", "").ToUpper();
				var testMessageText = TestMessage_PreProcessMessage_JobVoyage.Replace("8875A18DBCA44B0486F8AEEA0F3242BD", commonAccessReference);
				var testMessage = GetMesssageForTest(testMessageText.Replace("\r\n", ""));
				AssertEquals("Prerequisite: EM_MessageType", "RES", testMessage.EM_MessageType);
				AssertEquals("Prerequisite: Entry Status", "8", testMessage.CUSRESHelper.EntryStatus);

				var interchangeIn = SetInterchangeForMesssage(testMessageText, testMessage);
				PreAndProcessMessage(logger, testMessage);
				Factory.Save();

				var cusresMessage = Factory.Load<EDIMessage>(testMessage.PK);
				AssertEquals("Field: EDIMessage.EM_LinkTable", "JobVoyage", cusresMessage.EM_LinkTable);
				AssertEquals("Field: EDIMessage.EM_LinkUniqueID", jobVoyage.PK, cusresMessage.EM_LinkUniqueID);
				AssertEquals("Field: EDIMessage.EM_Status", EDIMessage.Status.ProcessedOK, cusresMessage.EM_Status);

				var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, jobVoyage.PK);
				var totalMessageCount = Factory.Load<EDIMessage>(query).Length;
				AssertEquals("Message Added", 2, totalMessageCount);
			}
		}

		public void TestPreProcessMessage_EntryHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var testHeader = declaration.CustomsEntryHeaders.AddNew();
			testHeader.CH_BGMReference = "TESTHeader1";

			var outgoingMessage = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "202", "ORG");
			Factory.Save();

			var testMessage = GetMesssageForTest(TestMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", testHeader.CH_BGMReference), "IN1");
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();

			new CUSRESMessageProcessor(logger).PreProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, testMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_JobVoyage()
		{
			var jobVoyage = GetJobVoyageForTest();
			var outgoingMessage = GetOutgoingMessage(jobVoyage, TestOutgoingMessage_JobVoyage.Replace("\r\n", ""), "15", "ORG");
			outgoingMessage.EM_MessageType = SARSEDIMessage.MessageTypes.CALINF;
			outgoingMessage.EM_MessageNum = "15";
			outgoingMessage.EM_Status = "SNT";
			AssertEquals("Prerequisite: EDIMessage.EM_LinkTable", "JobVoyage", outgoingMessage.EM_LinkTable);
			AssertEquals("Prerequisite: EDIMessage.EM_LinkUniqueID", jobVoyage.PK, outgoingMessage.EM_LinkUniqueID);
			Factory.Save();

			var commonAccessReference = outgoingMessage.PK.ToString().Replace("-", "").ToUpper();
			var testMessageText = TestMessage_PreProcessMessage_JobVoyage.Replace("8875A18DBCA44B0486F8AEEA0F3242BD", commonAccessReference);
			var testMessage = GetMesssageForTest(testMessageText.Replace("\r\n", ""));
			AssertEquals("Prerequisite: EM_MessageType", "RES", testMessage.EM_MessageType);
			AssertEquals("Prerequisite: Entry Status", "8", testMessage.CUSRESHelper.EntryStatus);

			var interchangeIn = SetInterchangeForMesssage(testMessageText, testMessage);
			Factory.Save();
			new CUSRESMessageProcessor(logger).PreProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				AssertEquals(jobVoyage.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(JobVoyage.Schema.TableName, testMessage.EM_LinkTable.ToString());
				AssertEquals(Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, testMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_AsycudaManifestHeader()
		{
			var testHeader = Factory.New<ZAManifest.IAsycudaManifestHeader>();
			testHeader.AMA_RN_NKCountry = Constants.CountryCodes.SouthAfrica;
			testHeader.AMA_JobReference = "MAN0000315";
			var outgoingMessageText = @"UNH+7615+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+00505655JSA20191021011281+4'
DTM+137:20180718:102'
DTM+136:20180522:102'
RFF+ACW:EACBF377B6804C00BA276C8B9D382844'
RFF+LO:MAN0000315'
NAD+RL+00655953'
NAD+MS+00505655TST'
NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'
TDT+20++3++00655953:172:20+++:::LIC                           '
LOC+35+BW'
LOC+36+ZA'
LOC+17+KFN'
DTM+132:20180522:102'
GEI+5+24:71:ZZZ'
GEI+5+DB:122:ZZZ'
CNI+1+00655953ROADMAN1::::20180521'
RFF+BM:THEONE:1'
LOC+8+ZADUR'
LOC+9+BWBBK'
NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'
NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET:GABARONE'
GID+1+5:BAG'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:5'
PCI+24+GGG'
UNT+27+7615'";

			var outgoingMessage = GetOutgoingMessage(testHeader as ManifestBase.AsycudaManifestHeader, outgoingMessageText.Replace("\r\n", ""), "7615", "CHG");
			outgoingMessage.EM_MessageType = "CAR";
			outgoingMessage.EM_MessageNum = "7615";

			var testMessage = GetMesssageForTest(TestMessage_PreProcessMessage_AsycudaManifestHeader.Replace("\r\n", ""));
			var interchangeIn = SetInterchangeForMesssage(TestMessage_PreProcessMessage_AsycudaManifestHeader, testMessage);
			Factory.Save();
			new CUSRESMessageProcessor(logger).PreProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				AssertEquals(testHeader.PK, testMessage.EM_LinkUniqueID);
				AssertEquals(AsycudaManifestHeader.Schema.TableName, testMessage.EM_LinkTable.ToString());
				AssertEquals(Messaging.Integration.EDIMessageStatusList.Codes.PreProcessedOK, testMessage.EM_Status);
			});
		}

		public void TestPreProcessMessage_NoMatch()
		{
			var msgText = TestMessage.Replace("\r\n", "");
			var testMessage = GetMesssageForTest(msgText);
			var interchangeIn = SetInterchangeForMesssage(msgText, testMessage);
			Factory.Save();
			new CUSRESMessageProcessor(logger).PreProcessMessage(testMessage);

			CombineAssertions(() =>
			{
				AssertNull(testMessage.EM_LinkedObject);
			});
		}

		public void TestPreAndProcessMessage_NoRFFACDMatchDefaultToLRN()
		{
			var outgoingMessage0 = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage0.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			outgoingMessage0.EM_MessageType = "DEC";
			outgoingMessage0.EM_ApplicationCode = "ZAC";
			outgoingMessage0.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage0.EM_MessageNum = "IN1";
			outgoingMessage0.MessageNumForTesting = "IN1";

			var testMessage0 = GetMesssageForTest(TestMessage_PreAndProcessMessage_NoRFFACDMatchDefaultToLRN.Replace("\r\n", ""), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			_ = SetInterchangeForMesssage(testMessage0.EM_MessageText, testMessage0);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage0);
			testMessage0.Validation.ValidateEM_MessageNum();
			CombineAssertions(() =>
			{
				AssertContains("Unable to find the linked job for", logger.LogMessages.ToString());
				Assert(testMessage0.GetWarnings().Any(x => x.Message.Contains("Warning - EDI Message: Unable to link this CUSRES back to entry. CUSRES has been linked via LRN. This message has been discarded.")));
				AssertEquals("This CUSRES must appear in each CusEntryHeader corresponding to LRN", testMessage0.EM_LinkedObject[CusEntryHeader.Schema.CH_BGMReference], testHeader.CH_BGMReference);
				AssertEquals(testMessage0.EM_Status, ZAMessage.Status.Discarded);
			});
			logger.LogMessages.Clear();

			var outgoingMessage = CreateOriginalMessageAndBusinessObjects();
			outgoingMessage.EM_MessageDateTime = new ZDateTime(2016, 03, 30, 01, 02, 03);
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			entryHeader1.CH_BGMReference = "00626166CLP20160426000199";
			outgoingMessage.EM_MessageText = TestOutGoingMessage.Replace("\r\n", "");
			outgoingMessage.EM_MessageNum = "IN1";
			outgoingMessage.MessageNumForTesting = "IN1";

			var testMessage = GetMesssageForTest(TestMessage_PreAndProcessMessage_NoRFFACDMatchDefaultToLRN.Replace("\r\n", ""), "IN1", new ZDateTime(2016, 9, 29, 1, 5, 0));
			var testInterchange = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();
			PreAndProcessMessage(logger, testMessage);
			testMessage.Validation.ValidateEM_MessageNum();
			CombineAssertions(() =>
			{
				Assert(!testMessage.GetWarnings().Any(x => x.Message.Contains("Warning - EDI Message: Unable to link this CUSRES back to entry. CUSRES has been linked via LRN. This message has been discarded.")));
				AssertContains("Log", string.Format(@"Information: 	Linking CUSRES Message: #IN1/{0} to job: {1}", testInterchange.EI_InterchangeNum, entryHeader1.CH_BGMReference), logger.LogMessages.ToString());
				AssertContains("Log", string.Format(@"Information: 	Entry Status of Entry:{0} has been updated to '13'.", entryHeader1.CH_BGMReference), logger.LogMessages.ToString());
				AssertEquals(testMessage.EM_Status, ZAMessage.Status.ProcessedOK);
			});
			logger.LogMessages.Clear();
		}

		public void TestPreProcessingRequired()
		{
			var processor = new CUSRESMessageProcessor(logger);
			AssertEquals(expected: true, processor.RequiresPreProcessing);
		}

		public void TestProcessOtherGovernmentAgencies()
		{
			_ = GetOutgoingMessage(testHeader, TestOutGoingMessage.Replace("\r\n", ""), "IL2023040428");
			var testMessage = GetMesssageForTest();
			testMessage.EM_MessageNum = "IN0";
			testMessage.EM_MessageText = TestMessage_OGA_Declaration_Amendment_Request.Replace("\r\n", "");
			_ = SetInterchangeForMesssage(testMessage.EM_MessageText, testMessage);
			Factory.Save();

			PreAndProcessMessage(logger, testMessage);
			Factory.Save();

			var caseNumber = ((IEnumerable<CaseNumber>)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions[0].CaseNumbers).FirstOrDefault(x => x.CY_Code == CaseNumberTypeList.Codes.OtherGovernmentAgencies);
			AssertEquals("OGA type", CaseNumberTypeList.Codes.OtherGovernmentAgencies, caseNumber?.CY_Code);
			AssertEquals("OGA case number", "151516758", caseNumber.CY_Data);
		}

		void PreAndProcessMessage(LoggingInformation logger, EDIMessage testMessage)
		{
			var p = new CUSRESMessageProcessor(logger);
			p.PreProcessMessage(testMessage);
			p.ProcessMessage(testMessage);
		}

		ZAMessageForTest GetZAMessageForTest(BusinessObject parent, ZString messageText, ZString messageNum)
		{
			var zaMessage = Factory.NewWithValidTestData<ZAMessageForTest>();
			zaMessage.EM_ApplicationCode = "ZAC";
			zaMessage.EM_MessageType = "DEC";
			zaMessage.EM_MessageSubType = "ORG";
			zaMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			zaMessage.EM_LinkUniqueID = parent.PK;
			zaMessage.EM_LinkTable = parent.TableName;
			zaMessage.EM_MessageText = messageText;
			zaMessage.MessageNumForTesting = messageNum;
			return zaMessage;
		}

		ZACInterchange GetZACInterchange(ZString interchangeNum, ZString receiveTransmit, ZString status)
		{
			return GetZACInterchange(Factory, interchangeNum, receiveTransmit, status);
		}

		ZACInterchange GetZACInterchange(BusinessObjectFactory factory, ZString interchangeNum, ZString receiveTransmit, ZString status)
		{
			var zaInterchange = factory.NewWithValidTestData<ZACInterchange>();
			zaInterchange.EI_From = "SARS";
			zaInterchange.EI_To = "TEST";
			zaInterchange.EI_InterchangeType = "ZAC";
			zaInterchange.EI_InterchangeNum = interchangeNum;
			zaInterchange.EI_ReceiveTransmit = receiveTransmit;
			zaInterchange.EI_Status = status;
			return zaInterchange;
		}

		CUSRESEDIMessageForTest CreateOriginalMessageAndBusinessObjects()
		{
			var testHelper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			testHelper.CreateCustomsStatusCusCodeEntry("1");
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;

			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			AssertEquals("Assessment Date Instruction", ZDateTime.Empty, entryInstruction1.CEI_DateForDuty);
			AssertEquals("Assessment Date Instruction", ZDateTime.Empty, entryInstruction2.CEI_DateForDuty);

			var invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_InvoiceNumber = "INV001";
			var invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceNumber = "INV002";

			var invLine1 = invHeader1.JobComInvoiceLines.AddNew();
			invLine1.JI_CEI = entryInstruction1.PK;

			var invLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invLine2.JI_CEI = entryInstruction2.PK;

			entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
			var testEntryLine1 = entryHeader1.AllEntryLines.AddNew();
			invLine1.JI_CL = testEntryLine1.PK;
			var testEntryLine2 = entryHeader1.AllEntryLines.AddNew();
			invLine2.JI_CL = testEntryLine2.PK;
			entryHeader1.CH_BGMReference = "TESTHeader1";

			entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;

			var outgoingMessage = GetOutgoingMessage(entryHeader1, TestOutGoingMessage.Replace("\r\n", "").Replace("00626166CLP20160426000199", entryHeader1.CH_BGMReference), "202", "ORG");

			return outgoingMessage;
		}

		CUSRESEDIMessageForTest GetMesssageForTest()
		{
			var message = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_ApplicationCode = "ZAC";
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		CUSRESEDIMessageForTest GetMesssageForTest(ZString messageText)
		{
			var message = GetMesssageForTest();
			message.EM_MessageText = messageText;
			return message;
		}

		CUSRESEDIMessageForTest GetMesssageForTest(ZString messageText, ZString messageNum)
		{
			var message = GetMesssageForTest(messageText);
			message.EM_MessageNum = messageNum;
			return message;
		}

		CUSRESEDIMessageForTest GetMesssageForTest(ZString messageText, ZString messageNum, ZDateTime messageCreateTimeUtc)
		{
			var message = GetMesssageForTest(messageText, messageNum);
			message.EM_SystemCreateTimeUtc = messageCreateTimeUtc;
			return message;
		}

		CUSRESEDIMessageForTest GetResMsgForTestUpdateEntryStatusAfterAnotherMessageReceiving9Status(ZString status, ZString messageNum)
		{
			return GetMesssageForTest(GetTestMessageResNo(status).Replace("202'", messageNum + "'"), "IN0", new ZDateTime(2016, 9, 29, 1, 5, 0));
		}

		EDIInterchangeForTest GetInterchange(ZString headerText, ZString bodyText)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchangeForTest>();
			interchange.EI_HeaderText = headerText;
			interchange.EI_BodyText = bodyText;
			return interchange;
		}

		EDIInterchangeForTest GetInterchange(ZString headerText, ZString bodyText, ZString interchangeNum)
		{
			var interchange = GetInterchange(headerText, bodyText);
			interchange.EI_InterchangeNum = interchangeNum;
			return interchange;
		}

		EDIInterchangeForTest SetInterchangeForMesssage(ZString messageText, CUSRESEDIMessageForTest testMessage)
		{
			var testInterchange = Factory.NewWithValidTestData<EDIInterchangeForTest>();
			testInterchange.EI_From = "SARS";
			testInterchange.EI_To = "TEST";
			testInterchange.EI_InterchangeType = "ZAC";
			testInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			testInterchange.EI_Status = EDIInterchange.Status.Acknowledged;
			testInterchange.EI_HeaderText = "UNB+UNOB:4+SARSDEC+00626166WTG::N6Q3M8H1Y1Q6P0G0:WTGAS2+20200223:1315+52137++CUSRES+++SAFJNBJNBWTG'";
			testInterchange.EI_FooterText = "UNZ+1+52137'";
			testInterchange.EI_BodyText = messageText;
			testInterchange.ContainedMessages.Add(testMessage);
			return testInterchange;
		}

		CUSRESEDIMessageForTest GetOutgoingMessage(BusinessObject parent, ZString messageText, ZString messageNum)
		{
			var outgoingMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			outgoingMessage.EM_ApplicationCode = "ZAC";
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingMessage.EM_LinkUniqueID = parent.PK;
			outgoingMessage.EM_LinkTable = parent.TableName;
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.MessageNumForTesting = messageNum;
			return outgoingMessage;
		}

		CUSRESEDIMessageForTest GetOutgoingMessage(BusinessObject parent, ZString messageText, ZString messageNum, ZString messageSubType)
		{
			var outgoingMessage = GetOutgoingMessage(parent, messageText, messageNum);
			outgoingMessage.EM_MessageSubType = messageSubType;
			return outgoingMessage;
		}

		CUSDECEDIMessage GetOutgoingCUSDECMessageAndSave(BusinessObject parent, ZString messageText, ZString messageNum, ZString messageSubType)
		{
			var outgoingMessage = Factory.NewWithValidTestData<CUSDECEDIMessage>();
			outgoingMessage.EM_MessageType = "DEC";
			outgoingMessage.EM_MessageSubType = messageSubType;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			outgoingMessage.EM_LinkUniqueID = parent.PK;
			outgoingMessage.EM_LinkTable = parent.TableName;
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.EM_MessageNum = messageNum;
			testHeader.Declaration.ResetMessageTypeChangeLogs();
			Factory.Save();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			return outgoingMessage;
		}

		CUSRESEDIMessageForTest GetControlMessage(BusinessObject parent, ZString status)
		{
			var controlMessage = Factory.NewWithValidTestData<CUSRESEDIMessageForTest>();
			controlMessage.EM_ApplicationCode = "ZAC";
			controlMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			controlMessage.EM_MessageType = "CTL";
			controlMessage.EM_Status = status;
			controlMessage.EM_LinkUniqueID = parent.PK;
			controlMessage.EM_LinkTable = parent.TableName;
			return controlMessage;
		}

		JobVoyage GetJobVoyageForTest()
		{
			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_VoyageFlight = "VF123";
			jobVoyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			return jobVoyage;
		}

		IAsycudaBill GetIAsycudaBill(ZGuid manifestHeader, ZInt manifestHeaderClusterKey, ZString billNumber)
		{
			var bill = Factory.New<IAsycudaBill>();
			bill.ABL_BillNumber = billNumber;
			bill.ABL_AMA = manifestHeader;
			bill.ABL_ClusterKey = manifestHeaderClusterKey;
			return bill;
		}

		CusEntryHeader entryHeader1;
		CusEntryHeader entryHeader2;
		CusEntryHeader testHeader;
		JobDeclaration declaration;
		JobComInvoiceHeader invHeader1;
		JobComInvoiceHeader invHeader2;
		JobComInvoiceLine invLine11;
		JobComInvoiceLine invLine21;
		LoggingInformationForTesting logger;
		Guid processTaskPk;

		protected override void SetUp()
		{
			CustomsDataRegistry.Instance.CreditCheckOnMessageSend.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = "IMP";
			declaration.JE_DeclarationReference = "B0000100X";
			declaration.JE_LocationOfGoods = "01";

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "~~";
			broker.GS_FullName = "BROKER NAME";
			broker.GS_EmailAddress = "email1@wisetechglobal.com";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			var entryInstruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_Style = "11";
			var entryInstruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = "20";

			invHeader1 = declaration.Invoices.AddNew();
			invHeader1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invLine11 = invHeader1.JobComInvoiceLines.AddNew();
			invLine11.JI_CEI = entryInstruction1.PK;
			invLine11.JI_Procedure = invLine11.EntryInstruction.CEI_Style + "00";
			invLine11.JI_NewUsed = GoodsTypeList.Codes.S;
			invHeader2 = declaration.Invoices.AddNew();
			invHeader2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invLine21 = invHeader2.JobComInvoiceLines.AddNew();
			invLine21.JI_CEI = entryInstruction1.PK;
			invLine21.JI_Procedure = invLine21.EntryInstruction.CEI_Style + "21";

			var invLine22 = invHeader2.JobComInvoiceLines.AddNew();
			invLine22.JI_CEI = entryInstruction1.PK;
			invLine22.JI_Procedure = invLine22.EntryInstruction.CEI_Style + "00";
			invLine22.JI_NewUsed = GoodsTypeList.Codes.S;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			BusinessObjectFactory.SetOnFactorySaveInTransactionForTest(factory =>
			{
				var rows = ((IBusinessObjectFactoryInternals)factory).RowFactory.GetModifiedPersistentRowsInSaveOrder();
				var processTaskRows = rows.Select(x => x).Where(y => y.Table.TableName == ProcessTasksSchema.Constants.TableName);
				foreach (var row in processTaskRows)
				{
					var pkName = row.Table.PrimaryKey[0];
					var index = row.Table.Columns.IndexOf(pkName);
					var rowPK = (Guid)row.ItemArray[index];
					processTaskPk = rowPK;
				}
			});
			Factory.Save();

			AssertEquals(1, declaration.ActiveEntryHeaders.Count);

			testHeader = declaration.ActiveEntryHeaders[0];
			testHeader.CH_BGMReference = "00626166CLP20160426000199";
			testHeader.CH_EntryNumber = 1;

			logger = (LoggingInformationForTesting)InboundInterchangeProcessorTest.GetNewLoggerForTesting();
		}

		#region TestMessages
		internal const string TestMessage_NoPostingDate = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+20+1'";

		const string TestMessageHeader = @"UNB+UNOB:4+SARSDECT+00626166TST::AAAAAAAAAAAAAABB:CORAS2+{0}:{1}+247++EXPORT+++GWWTGTEST+1''";

		internal const string TestMessageForDiscardedMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00281124JSA20200223057020:0'
DTM+178:20200222:102'
DTM+202:20200223:102'
TDT+20+CX749+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+62::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00281124SZX249002'
DTM+137:20200221:102'
RFF+AAS:160-21089666'
DTM+137:20200221:102'
RFF+ABT:JSA202002235125188'
DTM+137:20200223:102'
RFF+ACD:57081'
TAX+3+CUS:107:ZZZ'
MOA+161:479644'
CNT+7:230.00'
CNT+11:13'
UNT+21+1'";

		internal const string TestMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";

		internal static string GetTestMessageResNo(string customsStatus) => string.Format(TestMessageResNo.Replace("\r\n", ""), customsStatus);
		internal const string TestMessageResNo = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+{0}:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";

		const string TestMessage_StopDetain = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20160331:102'
DTM+202:20160328:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+2:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:202'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";

		const string TestMessage_FrequentSubmission = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199::00001'
DTM+132:20160504:102'
DTM+9:20160506033012:202'
TDT+20+QF234+4'
LOC+22+JHB'
LOC+14+22'
GIS+6:120:ZZZ'
NAD+AG+00626166'
NAD+MS+TST'
RFF+BH:HOUSEBILL'
DTM+137:20160423:102'
RFF+AAS:081-22222222'
DTM+137:20160424:102'
RFF+ACD:202'
ERP+1:0000'
ERC+0000'
FTX+AAO+++Message ignored as a duplicate submission within allocated timeframe'
UNT+19+1'";

		const string TestMessage_PreAndProcessMessage_NoRFFACDMatchDefaultToLRN = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20191003:102'
TDT+20+VOY1+1+++++:::SAFMONAT     SA SEDERBERG'
LOC+22+DBN::ZZZ'
LOC+14+J3::ZZZ'
GIS+13:120:ZZZ:N'
EQD+CN+ANLU1234560'
NAD+AG+00626166'
RFF+AAS:    MSC TIONA BL578439534'
DTM+137:20191003:102'
RFF+ABT:DBN201910045000214'
DTM+137:20191004:102'
RFF+ACD:202'
RFF+AAV:131313138'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:92281'
CNT+7:600.00'
CNT+11:1'
UNT+23+1'";

		const string TestMessage_PreProcessMessage_AsycudaManifestHeader = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655JSA20191021011281:0'
DTM+178:20191021:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+62::ZZZ'
GIS+14:120:ZZZ:N'
NAD+AG+00626166'
RFF+AAS:083-32154323'
DTM+137:20191020:102'
RFF+ABT:JSA201910215000501'
DTM+137:20191021:102'
RFF+ACD:7615'
RFF+AAV:105394631'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:166'
CNT+7:1.13'
CNT+11:16'
UNT+22+1'";

		const string TestMessage_BondedWarehouseIncomingResponseMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20191003:102'
TDT+20+VOY1+1+++++:::SAFMONAT     SA SEDERBERG'
LOC+22+DBN::ZZZ'
LOC+14+J3::ZZZ'
GIS+1:120:ZZZ:N'
EQD+CN+ANLU1234560'
NAD+AG+00626166'
RFF+AAS:    MSC TIONA BL578439534'
DTM+137:20191003:102'
RFF+ABT:DBN201910045000214'
DTM+137:20191004:102'
RFF+ACD:202'
RFF+AAV:105385028'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:92281'
CNT+7:600.00'
CNT+11:1'
UNT+23+1'";

		const string TestMessage_Status_13_CusresMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20191003:102'
TDT+20+VOY1+1+++++:::SAFMONAT     SA SEDERBERG'
LOC+22+DBN::ZZZ'
LOC+14+J3::ZZZ'
GIS+13:120:ZZZ:N'
EQD+CN+ANLU1234560'
NAD+AG+00626166'
RFF+AAS:    MSC TIONA BL578439534'
DTM+137:20191003:102'
RFF+ABT:DBN201910045000214'
DTM+137:20191004:102'
RFF+ACD:202'
RFF+AAV:131313138'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:92281'
CNT+7:600.00'
CNT+11:1'
UNT+23+1'";

		const string TestMessage_Status_14_CusresMessage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20191003:102'
TDT+20+VOY1+1+++++:::SAFMONAT     SA SEDERBERG'
LOC+22+DBN::ZZZ'
LOC+14+J3::ZZZ'
GIS+14:120:ZZZ:N'
EQD+CN+ANLU1234560'
NAD+AG+00626166'
RFF+AAS:    MSC TIONA BL578439534'
DTM+137:20191003:102'
RFF+ABT:DBN201910045000214'
DTM+137:20191004:102'
RFF+ACD:202'
RFF+AAV:105385028'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:92281'
CNT+7:600.00'
CNT+11:1'
UNT+23+1'";

		const string TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655JSA20191021011281:0'
DTM+178:20191021:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+62::ZZZ'
GIS+14:120:ZZZ:N'
NAD+AG+00626166'
RFF+AAS:083-32154323'
DTM+137:20191020:102'
RFF+ABT:JSA201910215000501'
DTM+137:20191021:102'
RFF+ACD:7615'
RFF+AAV:105394631'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:166'
CNT+7:1.13'
CNT+11:16'
UNT+22+1'";

		const string TestMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00505655JSA20191021011281:0'
DTM+178:20191021:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+62::ZZZ'
GIS+14:120:ZZZ:N'
NAD+AG+00626166'
RFF+AAS:083-32154323'
DTM+137:20191020:102'
RFF+ABT:JSA201910215000501'
DTM+137:20191021:102'
RFF+ACD:7616'
RFF+AAV:105394632'
ERP+1:0'
ERC+0000::ZZZ'
FTX+AAO+++PLEASE SUBMIT YOUR SUPPORTING DOCUMENTS ELECTRONICALLY'
TAX+3+CUS:107:ZZZ'
MOA+161:166'
CNT+7:1.13'
CNT+11:16'
UNT+22+1'";

		const string TestMessage_Update_CusEntryPayInfo_PaymentDate_When_Status_8_CUSRES_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+178:20191002:102'
DTM+202:20191002:102'
TDT+20+QR1363+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+70::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00101566CTU82509780'
RFF+AAS:157-68964711'
DTM+137:20190929:102'
RFF+ACD:124'
ERP+1:0'
ERC+1150::ZZZ'
FTX+AAO+++Billing?: Credit Limit Check Failed'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:166.00'
CNT+11:10'
UNT+21+1'";

		const string TestMessage_Update_CusEntryPayInfo_When_Status_AWR_CUSRES_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+178:20191002:102'
DTM+202:20191002:102'
TDT+20+QR1363+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+70::ZZZ'
GIS+8:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00101566CTU82509780'
RFF+AAS:157-68964711'
DTM+137:20190929:102'
RFF+ACD:124'
ERP+1:0'
ERC+1150::ZZZ'
FTX+AAO+++Billing?: Credit Limit Check Failed'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:166.00'
CNT+11:10'
UNT+21+1'";

		const string TestMessage_Update_CusEntryPayInfo_When_Status_CLR_CUSRES_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+178:20191002:102'
DTM+202:20191002:102'
TDT+20+QR1363+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+70::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00101566CTU82509780'
RFF+AAS:157-68964711'
DTM+137:20190929:102'
RFF+ACD:125'
ERP+1:0'
ERC+1150::ZZZ'
FTX+AAO+++Billing?: Credit Limit Check Failed'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:166.00'
CNT+11:10'
UNT+21+1'";

		const string TestMessage_Update_CusEntryPayInfo_PaymentStatus_When_Status_1_CUSRES_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+178:20191002:102'
TDT+20+QR1363+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+70::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00101566CTU82509780'
RFF+AAS:157-68964711'
DTM+137:20190929:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+ACD:124'
ERP+1:0'
ERC+1150::ZZZ'
FTX+AAO+++Billing?: Credit Limit Check Failed'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:166.00'
CNT+11:10'
UNT+22+1'";

		const string TestMessage_Update_CaseNumber_Date_When_Status_34_CUSRES_Message = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+178:20191002:102'
DTM+202:20191011:102'
TDT+20+SQ478+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A1::ZZZ'
GIS+34:120:ZZZ:N'
NAD+AG+00626166'
RFF+AAS:618-70437581'
DTM+137:20191001:102'
RFF+ABT:JSA201910115000442'
DTM+137:20191011:102'
RFF+ACD:124'
RFF+AAV:105389559'
TAX+3+CUS:107:ZZZ'
MOA+161:1000'
CNT+7:1000.00'
CNT+11:1'
UNT+20+1'";

		const string TestMessage_PreProcessMessage_JobVoyage = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+8875A18DBCA44B0486F8AEEA0F3242BD'
TDT+20+0028W+1+++++:::BERLIN BRIDGE'
LOC+22+ ::ZZZ'
GIS+8:120:ZZZ'
NAD+AG+00000000'
UNT+7+1'";

		const string TestMessage_OGA_Declaration_Amendment_Request = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20200602:102'
TDT+20+202A+2+++++:::OCENVRIS8 MAERSK LUZ'
LOC+22+MSB::ZZZ'
LOC+14+RB::ZZZ'
GIS+74:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:206245667937 7938'
DTM+137:20200507:102'
RFF+AAS: SAFM910721020'
DTM+137:20200507:102'
RFF+ABT:MSB202307185000219'
DTM+137:20230718:102'
RFF+ACD:IL2023040428'
RFF+AAV:151516651'
ERP+6:0'
ERC+3::ZZZ'
FTX+AAO+++DETAIN FOR PLANT INSPECTION'
ERP+6:0'
ERC+100::ZZZ'
FTX+AAO+++Please Declare the correct amount'
ERP+6:0'
ERC+0000::ZZZ'
FTX+AAO+++OGA Case=151516758'
TAX+3+CUS:107:ZZZ'
MOA+161:3000'
CNT+7:14473.00'
CNT+11:2'
UNT+30+1'";

		const string TestOutgoingMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaManifestHeader = @"UNH+7615+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+00505655JSA20191021011281+4'
DTM+137:20180718:102'
DTM+136:20180522:102'
RFF+ACW:EACBF377B6804C00BA276C8B9D382844'
RFF+LO:MAN0000315'
NAD+RL+00655953'
NAD+MS+00505655TST'
NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'
TDT+20++3++00655953:172:20+++:::LIC                           '
LOC+35+BW'
LOC+36+ZA'
LOC+17+KFN'
DTM+132:20180522:102'
GEI+5+24:71:ZZZ'
GEI+5+DB:122:ZZZ'
CNI+1+00655953ROADMAN1::::20180521'
RFF+BM:THEONE:1'
LOC+8+ZADUR'
LOC+9+BWBBK'
NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'
NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET:GABARONE'
GID+1+5:BAG'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:5'
PCI+24+GGG'
UNT+27+7615'";

		const string TestOutgoingMessage_Status_14_CusresCuscar_Message_with_CaseNumber_for_AsycudaBill = @"UNH+7615+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+00505655JSA20191021011281+4'
DTM+137:20180718:102'
DTM+136:20180522:102'
RFF+ACW:EACBF377B6804C00BA276C8B9D382844'
RFF+LO:MAN0000315'
NAD+RL+00655953'
NAD+MS+00505655TST'
NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2019041820160201:MHDSVRZAZAZA'
TDT+20++3++00655953:172:20+++:::LIC                           '
LOC+35+BW'
LOC+36+ZA'
LOC+17+KFN'
DTM+132:20180522:102'
GEI+5+24:71:ZZZ'
GEI+5+DB:122:ZZZ'
CNI+1+00655953ROADMAN1::::20180521'
RFF+BM:THEONE:1'
LOC+8+ZADUR'
LOC+9+BWBBK'
NAD+CN++DIGICORE ELECTRONICS (PTY) LTD:37 HILLCLIMB ROAD:DURBAN::3608'
NAD+CZ++TIM EXPORT BLNS GROUP:1 FAR STREET:GABARONE'
GID+1+5:BAG'
FTX+AAA++9+DESC'
MEA+AAE+AAB+KGM:5'
PCI+24+GGG'
UNT+27+7616'";

		const string TestOutgoingMessage_JobVoyage = @"UNH+79+CALINF:D:16A:UN:RCG001'
BGM+96:::SCH+8875A18DBCA44B0486F8AEEA0F3242BD+9'
DTM+137:202007061655:203'
NAD+MS+12345678::ZZZ'
TDT+20+V-123+1++:172:20+++:103::SA SEDERBERG'
RFF+ACL:V-123'
LOC+5+DEHAM:139:6'
DTM+136:202003271010:203'
LOC+153+ZADUR:139:6'
DTM+132:202003271010:203'
UNT+11+79'";

		internal const string TestOutGoingMessage = @"UNH+202+CUSDEC:D:96B:UN:ZZZ01'
BGM+929+00626166CLP20160426000199::00002+9'
CST++A:117:ZZZ'
LOC+14+02::ZZZ
LOC+35+HK::5'
LOC+36+SG::5'
LOC+96+CLP::ZZZ'
LOC+9+HKHKG::5'
GIS+D:134:ZZZ'
FTX++++7'
RFF+AAS:HOUSE BILL NO'
DTM+137:20160305:102'
RFF+ACD:202'
PAC+5'
PCI++ASDFWQERQWER'
PCI++WQERASDFASDFWQERQWERASDFWQERQWERASD:FWQERQWERASDFWQERQWER'
PCI++A123456'
PCI++01ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:02ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:03ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:04ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:05ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:06ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:07ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:08ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:09ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567:10ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567'
TDT+20++4+++++:::AIR'
DOC+380+INVH2FOB'
DTM+3:20160415:102'
NAD+IM+++SINGAPORE MRT LTD+300 BISHAN ROAD SINGAPORE 179102 RE:P. OF SINGAPORE+++179102'
NAD+EX+++SINGAPORE XXX LTD+300 BISHAN ROAD SINGAPORE 179102 RE:P. OF SINGAPORE+++179102'
NAD+AG+00626166'
NAD+MS+DPC'
UNS+D'
CST+0001+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-1'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:41.00'
NAD+WH'
MOA+38:544570.00'
MOA+40:544570.00'
TAX+1+DTY:107:ZZZ'
MOA+161:163371.00'
CST+0002+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-2'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:42.00'
NAD+WH'
MOA+38:1089140.00'
MOA+40:1089140.00'
TAX+1+DTY:107:ZZZ'
MOA+161:326742.00'
CST+0003+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-3'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+AU'
MEA+AAR++NO:43.00'
NAD+WH'
MOA+38:217828.00'
MOA+40:217828.00'
TAX+1+DTY:107:ZZZ'
MOA+161:65348.40'
CST+0004+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-4'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:44.00'
NAD+WH'
MOA+38:326742.00'
MOA+40:326742.00'
TAX+1+DTY:107:ZZZ'
MOA+161:98022.60'
CST+0005+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-5'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:45.00'
NAD+WH'
MOA+38:2178.00'
MOA+40:2178.00'
TAX+1+DTY:107:ZZZ'
MOA+161:653.40'
CST+0006+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-6'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:46.00'
NAD+WH'
MOA+38:109.00'
MOA+40:109.00'
TAX+1+DTY:107:ZZZ'
MOA+161:32.70'
CST+0007+84501230:108:ZZZ+100'
FTX+AAA+++OF A DRY LINEN CAPACITY NOT EXCEEDING 7 KG DESC2-7'
FTX+ACB+++NUIN'
FTX+CCI+++11:00:::N'
LOC+27+CN'
MEA+AAR++NO:47.00'
NAD+WH'
MOA+38:1.00'
MOA+40:1.00'
TAX+1+DTY:107:ZZZ'
MOA+161:0.30'
UNS+S'
TAX+3+CIF:107:ZZZ'
MOA+161:2252386.16'
TAX+3+TDD:107:ZZZ'
MOA+161:654170.40'
TAX+3+CUS:107:ZZZ'
MOA+161:2180568.00'
UNT+109+202'
";

		internal const string TestGIS6MessageWithMRNNumber = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:1'
DTM+178:20160816:102'
TDT+20+SA594+0+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A8::ZZZ'
GIS+6:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00426789HB2'
DTM+137:20160816:102'
RFF+AAS:000-6191388232'
DTM+137:20160816:102'
RFF+ABT:JSA201608165000417'
RFF+ACD:202'
ERP+1:0'
ERC+1121::ZZZ'
FTX+AAO+++ FIELD(Payment Code) DESCR(Invalid Payment Method Code V supplied, VAT: and other Duties must be declared.)'
ERP+1:0'
ERC+1060::ZZZ'
FTX+AAO+++ FIELD(Total VAT Due) DESCR(Trader calculated sum of duty tax fee diff:erences 770.00 does not match system calculated sum of duty tax fee di:fferences 154.00)'
ERP+1:0'
ERC+8463::ZZZ'
FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Field must be empty)'
ERP+1:0'
ERC+1173::ZZZ'
FTX+AAO+++ FIELD(Depot/Terminal Code) DESCR(Invalid Transport Code/Removal Trans:port Code for Depot)'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:10.00'
CNT+11:1'
UNT+31+1'
";

		internal const string CUSRES_GOVGIO_Message = "UNH+1+CUSRES:D:96B:UN:ZZZ01'BGM+962+00626166CLP20160426000199::00001'DTM+9:20160513143147:202'TDT+20'LOC+22+BBR'GIS+6:120:ZZZ'NAD+AG+00505655'NAD+MS+TST'RFF+BH:00505655'RFF+AAS:HENRYMASTER1'RFF+ACD:35'ERP+1:0000'ERC+0000'FTX+AAO+++Line number may not be 0'UNT+15+1'";

		internal const string CUSRES_GOVGIO_Response = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00626166CLP20160426000199:0'
DTM+132:20160331:102'
DTM+202:20160331:102'
TDT+20+SA123+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A2::ZZZ'
GIS+{0}:120:ZZZ:N'
NAD+AG+00626166'
RFF+BH:00626166HAWB123654'
DTM+137:20160331:102'
RFF+AAS:083-01203226'
DTM+137:20160331:102'
RFF+ABT:JSA201603315000938'
DTM+137:20160401:102'
RFF+AFB:JSA2016'
DTM+137:20160401:102'
RFF+ACD:202'
ERP+1:0000'
ERC+0000'
FTX+AAO+++Line number may not be 0'
TAX+3+CUS:107:ZZZ'
MOA+161:2000'
CNT+7:120.00'
CNT+11:10'
UNT+21+1'";

		internal const string CUSCAR_Message = @"UNH+12336+CUSCAR:D:16A:UN:RCG001'
BGM+85:::RFM+F59D642456C94D9CA50B5E293C1ABC21+9'
DTM+137:20200305:102'
RFF+LO:MAN0000616'
NAD+RL+FH000800'
NAD+MS+00505655TST'
NAD+DR+12321321321321321:A++DOKTER:DOKTER:7110285071080:2020041820160201:MHDSVRZAZAZA'
TDT+20++3++FH000800:172:20+++:::FAKE'
LOC+35+ZA'
LOC+36+BW'
LOC+17+KFN'
DTM+132:20200304:102'
GEI+5+22:71:ZZZ'
GEI+5+LB:122:ZZZ'
CNI+1+FH000800FAKE MANIFEST 2::::20200303'
RFF+BM:BILL1:1'
LOC+8+BWDUK'
LOC+9+ZABFN'
TDT+20'
RFF+ABT:00505655KFN20200304012306'
RFF+UCN:FAKEUCR'
NAD+CN++SPAKOOS INVESTMENTS PTY LTD:MMOPANE BLOCK:GABORONE:GA:0001'
NAD+CZ++SAMSOMITE:68 OLD MAIN ROAD KLOOF WARRA:DURBAN::4000'
GID+1+6:BOT'
FTX+AAA++9+5 BOTTLES'
MEA+AAE+AAW+MTQ:0'
MEA+AAE+AAB+KGM:50'
PCI+24+LEAKING'
UNT+29+12336'";

		internal const string CUSCAR_CUSRES_CODE8 = @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+F59D642456C94D9CA50B5E293C1ABC21'
TDT+20+NOT SUPPLIED+3+++++:::FAKE'
LOC+22+BFN::ZZZ'
GIS+8:120:ZZZ'
NAD+AG+00000000'
RFF+BH:BILL1'
RFF+AAS:FH000800FAKE MANIFEST 2'
DTM+137:20200303:102'
RFF+ACD:12336'
RFF+AFB:CARN01118MLL'
UNT+11+1'";
		#endregion
	}

	sealed class EDIInterchangeForTest : ZACInterchange
	{
		public EDIInterchangeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			EI_ApplicationCode = "ZAC";
			EI_InterchangeType = "ZAC";
			EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			EI_From = "ZACustoms";
			EI_To = "HYEDZACMT";
			EI_Status = EDIMessage.Status.Queued;
			EI_InterchangeNum = "" + RandomInstance.NextDouble();
			EI_ServerID = 0;
			EI_RetryCount = 0;
			EI_SystemCreateTimeUtc = ZDateTime.Now;
			EI_SystemCreateUser = "~BP";
			EI_SystemLastEditTimeUtc = ZDateTime.Now;
			EI_SystemLastEditUser = "~BP";
			EI_SessionGUID = new ZGuid();
			EI_IsActive = true;
		}

		public Random RandomInstance
		{
			get { return randomNum ??= new Random(); }
		}
		[ThreadStatic]
		static Random randomNum;
	}
}
