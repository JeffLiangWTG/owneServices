using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobDeclaration_PropertiesForMessageProcessorTest : TestCaseWithFactory
	{
		#region Tests for Properties

		public void TestLinkedObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;

			var bizObj = tempInterface.LinkedObject as JobDeclaration;
			AssertNotNull(bizObj);
			AssertEquals(declaration.PK, bizObj.PK);
		}

		public void TestEntrySummaryEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var bizObj = tempInterface.EntrySummaryEntry;
			AssertNotNull(bizObj);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK, bizObj.PK);
		}

		public void TestCargoReleaseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;

			var bo = declaration as IEntryHeaderParentBusinessObject;

			AssertEquals(entryHeader.PK, bo.CargoReleaseEntry.PK);
		}

		public void TestSimplifiedEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;

			var bizObj = tempInterface.SimplifiedEntry as CusEntryHeader;
			AssertNotNull(bizObj);
			AssertEquals(declaration.ActiveEntryHeaders.SimplifiedEntry.PK, bizObj.PK);
		}

		public void TestReferenceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;
			var entryNumber = Factory.New<CusEntryNumber>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "TEST00001";
			declaration.US_EntryFilerCode = "SV9";
			entryNumber.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			entryNumber.CE_ParentID = declaration.PK;
			entryNumber.CE_EntryNum = "TEST0001";
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			AssertEquals("TEST00001 / SV9-TEST000-1", tempInterface.ReferenceNumber);
		}

		public void TestRegistryCompanyPKAndRegistryBranchPK()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			declaration.JE_GB = branch.PK;

			AssertEquals(company.PK, tempInterface.RegistryCompanyPK);
			AssertEquals(branch.PK, tempInterface.RegistryBranchPK);
		}

		public void TestCusAgent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var tempInterface = declaration as IEntryHeaderParentBusinessObject;
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_Code = "STF";
			declaration.JE_GS_NKCusAgent = glbStaff.GS_Code;

			AssertEquals(glbStaff.GS_Code, tempInterface.CusAgent.GS_Code);
		}

		public void TestImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			declaration.JE_OH_Importer = org.PK;

			var bo = declaration as IEntryHeaderParentBusinessObject;

			AssertEquals(org.PK, bo.Importer.PK);
		}

		public void TestDispositionCodesInIEntryHeaderParentBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var message = mock.Object;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.CargoReleaseProcessingResults;
			message.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			declaration.Messages.Add(message);
			message.EM_MessageText =
"B011102113RR                                                            27444284" +
"R14701113 457149080111-35250130025143832 CMDUXIN DAN DONG        492  110708    " +
"R4            SZ1459194                           00000216CTNS CMDU             " +
"R5110308000805PAPERLESS                                                         " +
"R6FDA    110308000806FDA MAY PROCEED                                            " +
"R6FDA    110308000806FDA MAY PROCEED                  070011001THRU0011001      " +
"Y  1102113LS00002";

			Factory.Save();

			var bo = declaration as IEntryHeaderParentBusinessObject;

			AssertEquals(1, bo.DispositionCodes.Count);
			AssertEquals(CargoReleaseProcessingResultList.Descriptions.PaperlessEntry, bo.DispositionCodes[0].DispositionCodeDesc);
			AssertEquals("05", bo.DispositionCodes[0].US_Code);
		}

		public void TestReleaseStatusChangingSuspender()
		{
			var declaration = Factory.New<JobDeclaration>();

			var bo = declaration as IEntryHeaderParentBusinessObject;
			AssertNotNull(bo.ReleaseStatusChangingSuspender);
			AssertType<JobDeclaration.CargoReleaseEntryReleaseStatusChangingSuspender>(bo.ReleaseStatusChangingSuspender);
		}

		public void TestReleaseStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ReleaseStatus = ReleaseStatusList.Codes._01;

			var bo = declaration as IEntryHeaderParentBusinessObject;

			AssertEquals(ReleaseStatusList.Codes._01, bo.ReleaseStatus);
		}

		[TestDate(2019, 12, 11)]
		public void TestReleaseDateTime()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;

			var bo = declaration as IEntryHeaderParentBusinessObject;

			AssertEquals(ZDateTime.Now.ToString(), bo.ReleaseDateTime.ToString());
		}

		public void TestShouldUpdateDeclarationWithCargoReleaseResults()
		{
			var declaration = Factory.New<JobDeclaration>();
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var bo = declaration as IEntryHeaderParentBusinessObject;

			Assert(bo.ShouldUpdateDeclarationWithCargoReleaseResults);

			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, false);
			Assert(!bo.ShouldUpdateDeclarationWithCargoReleaseResults);
		}

		#endregion

		#region Tests for Functions without Returned Value

		public void TestUpdateQuotaStatus()
		{
			var declaration = Factory.New<JobDeclaration>();

			var bo = declaration as IEntryHeaderParentBusinessObject;
			bo.UpdateQuotaStatus(CargoReleaseProcessingResultList.Codes.ACASBillOnFile);

			AssertEquals(CargoReleaseProcessingResultList.Codes.ACASBillOnFile, declaration.US_QuotaStatus);
		}

		public void TestMarkAIIRequested()
		{
			var declaration = Factory.New<JobDeclaration>();

			var bo = declaration as IEntryHeaderParentBusinessObject;
			bo.MarkAIIRequested();

			Assert(declaration.US_IsAIIRequested);
		}

		public void TestUpdatemessageLinkedParentBOAfterReleasedAndLogs()
		{
			USCustomsDataRegistry.Instance.UpdateDeclarationWithCargoReleaseResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var declaration = GetDeclaration("71002057");
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = "TRK";

			var bill1 = declaration.Bills.AddNew();
			bill1.US_UI_NKBillIssuerSCAC = "C2";
			bill1.CU_BillNum = "PI3151202078";
			bill1.CU_BillType = "HB";
			bill1.CU_NoOfPacks = 10;
			bill1.CU_PackType = "AE";

			var bill2 = declaration.Bills.AddNew();
			bill2.US_UI_NKBillIssuerSCAC = "C2";
			bill2.CU_BillNum = "PI3151202079";
			bill2.CU_BillType = "HB";

			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.US_EntryDate = ZDateTime.Empty;
			declaration.JE_DateOfArrival = ZDateTime.Empty;

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var message = CreateStatusMessage(@"B004601267SO                                                                    
SO104601SV9  71002057 0147-299933300HLCUZIM TARRAGONA       40W  012216         
SO20CR B00046858                                                                
SO20CR B00046858                                                                
SO40RC2  PI3151202078                                      00004956BO   00004956
SO50110314151195BILL ARRIVED                            YCPA 90   1201161108    
SO40RC2  PI3151202079                                      00004957BO   00004957
SO50120116073851MANIFEST HOLD CBP                       YQTR 727  1201161108    
SO60012016120598RELEASED                                01201601                
SO70FDAFOO012016102201DATA UNDER PGA REVIEW         01  001001                  
SO7101166458126343012016102224                                                  
Y  4601267SO00000".Replace("\r\n", ""));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			bill1 = reLoadJob.Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == "PI3151202078");
			bill2 = reLoadJob.Bills.Cast<Bill>().FirstOrDefault(x => x.CU_BillNum == "PI3151202079");
			CombineAssertions(() =>
			{
				AssertEquals("US_SchDEntry", "4601", reLoadJob.US_SchDEntry);
				AssertEquals("US_UI_NKCarrierSCAC", "HLCU", reLoadJob.US_UI_NKCarrierSCAC);
				AssertEquals("JE_VoyageFlightNo", "40W", reLoadJob.JE_VoyageFlightNo);
				AssertEquals("JE_TotalNoOfPacks", 9913, reLoadJob.JE_TotalNoOfPacks);
				AssertEquals("JE_TotalNoOfPacksPackType", "BO", reLoadJob.JE_TotalNoOfPacksPackType);
				AssertEquals("US_EntryDate", new ZDateTime(2016, 1, 22), reLoadJob.US_EntryDate);
				AssertEquals("JE_DateOfArrival", new ZDateTime(2016, 12, 1), reLoadJob.JE_DateOfArrival);
				AssertEquals("US_SchDArrival", "1108", reLoadJob.US_SchDArrival);

				AssertEquals("bill1.CU_NoOfPacks is not updated for the original value is not zero", 10m, bill1.CU_NoOfPacks);
				AssertEquals("bill1.CU_PackType is not updated for the original value is not empty", "AE", bill1.CU_PackType);

				AssertEquals("bill2.CU_NoOfPacks is updated for the original value is zero", 4957m, bill2.CU_NoOfPacks);
				AssertEquals("bill2.CU_PackType is updated for the original value is empty", "BO", bill2.CU_PackType);

				var logs = declaration.Logs.GetAllLogs().ToArray<StmALog>();
				AssertEquals(8, logs.Length);
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "EDT" && x.SL_Reference.Contains("Entry lines merged")));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "EDT" && x.SL_Reference.Contains("Entry Number  was changed to 71002057")));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "WTA" && x.SL_Reference.Contains("BRK, global, system")));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "ADD" && x.SL_Reference == ZString.Empty));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "DEP" && x.SL_Reference == ZString.Empty));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "ARV" && x.SL_Reference == ZString.Empty));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "EDT" && x.SL_Reference == ZString.Empty));
				AssertEquals(1, logs.Count(x => x.SL_SE_NKEvent == "ATC"));
			});
		}

		public void TestAddOrUpdateCusDisposition()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B003901SV9SO                                                                    " +
					"SO103901SV9  71002057 0123-456789012CO                      3021 0909131        " +
					"SO20CR B00160703                                                                " +
					"SO40R    00591325206                                       00000150BL   00000000" +
					"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
					"SO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
					"SO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
					"SO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
					"Y  3901SV9SO00000                                                               ");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.EntryPGACusDispositions.Count, 2);
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "NHT" && x.CDI_Status == "07"));
			Assert(reLoadJob.EntryPGACusDispositions.Cast<CusDisposition>().Any(x => x.CDI_StatusKey == "FDA" && x.CDI_Status == "07"));
		}

		#endregion

		#region Tests for Functions with Returned Value

		public void TestGetFirstBillHasSameNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "TESTBILL";

			var bo = declaration as IEntryHeaderParentBusinessObject;
			AssertEquals(bill.PK, bo.GetFirstBillHasSameNumber("TESTBILL").PK);
		}

		public void TestGetUnableToDeactivateStatementLineRemarkIfNecessary()
		{
			var declaration = GetDeclaration("01000048");
			declaration.US_EntryFilerCode = "XJ5";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "01000048";
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			Factory.Save();

			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals("Precondition: Release Status should be 'Released'", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementNumber = "800400560";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "01000048";
			line.B3_EntryFilerCode = "XJ5";
			line.B3_Status = StatementLineStatusList.Codes.Active;

			var charge = line.Charges.AddNew();
			charge.B4_ChargeAmount = 15.94m;
			charge.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			var charge2 = line.Charges.AddNew();
			charge2.B4_ChargeAmount = 20m;
			charge2.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;

			line.B3_CustomsFeesTotal = 35.94m;
			statement.B2_StatementAmount = 35.94m;

			line.B3_Status = StatementLineStatusList.Codes.Active;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			var message6 = CreateStatusMessage("B001101SV9SO                                00                                  " +
									"SO101901XJ5  01000048 0123-456789012AL                      2246 021413         " +
									"SO20CR B00159843                                                                " +
									"SO40R    ALP31222406                                       00000600             " +
									"SO60022713162023ENTRY CANCELLED                                                 " +
									"Y  3910SV9SO00000");
			Factory.Save();

			DeclarationTestHelper.SetupForSendMessage();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>((EmailDef emailToMatched) => emailToMatched.Subject.Contains("ACE Cargo Release Status Response")));
			AssertNotNull(email);
			Assert("Should contains remarks.", email.Body.Contains("This entry has just been canceled, but it is on a statement"));
		}

		public void TestGetPGAEntryStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var pga01 = declaration.EntryPGACusDispositions.AddNew();
			var pga02 = declaration.EntryPGACusDispositions.AddNew();
			var pga03 = declaration.EntryPGACusDispositions.AddNew();

			pga01.CDI_StatusKey = "PGA01";
			pga02.CDI_StatusKey = "PGA02";
			pga03.CDI_StatusKey = "PGA03";

			pga01.CDI_Status = "01";
			pga02.CDI_Status = "02";
			pga03.CDI_Status = "04";

			var bo = declaration as IEntryHeaderParentBusinessObject;
			var dic = bo.GetPGAEntryStatus();

			AssertEquals(3, dic.Count);
			ZString status;
			Assert(dic.TryGetValue(pga01.CDI_StatusKey, out status));
			AssertEquals("01", status);
			Assert(dic.TryGetValue(pga02.CDI_StatusKey, out status));
			AssertEquals("02", status);
			Assert(dic.TryGetValue(pga03.CDI_StatusKey, out status));
			AssertEquals("04", status);
		}

		public void TestAddOGADispositionData()
		{
			var declaration = GetDeclaration("71002057");
			var message = CreateStatusMessage(
					"B001601SV9SO                                                                    " +
					"SO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
					"SO20CR 6NYC00663                                                                " +
					"SO40MACLU6A62S6680206                                                           " +
					"SO40HSSLLCHS222271                                         00000001     00000001" +
					"SO50051116121894BILL DEPARTED                                                   " +
					"SO60052616163799RELEASE SUSPENDED                                               " +
					"SO60052616163790UNDER CBP REVIEW                                                " +
					"SO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
					"SO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
					"SO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
					"SO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
					"Y  1601SV9SO00000");
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			var dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals(4, dispositionDataCount);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var outgoing = mock.Object;
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQuery;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_MessageNum = "HYEDUSCMT_159673";
			outgoing.EM_MessageText = "B013901SV9CQ                                               HYEDUSCMT_159673     WR1    SV9 71002057                                                    Y2       Y  3901SV9CQ00001";
			entry.Messages.Add(outgoing);

			var c1message = Factory.New<MQEDIMessage>();
			c1message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message.EM_MessageNum = "HYEDUSCMT_159673";
			c1message.EM_Status = EDIMessage.Status.Queued;
			c1message.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
									   "WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
									   "WO40MACLU6A62S6680206                                                           " +
									   "WO40HSSLLCHS222271                                         00000001     00000001" +
									   "WO50052716101095BILL ARRIVED                                                    " +
									   "WO60052716101098RELEASED                                05271601                " +
									   "WO60052716101001ONE USG                                                         " +
									   "WO70FDARAD012216122901DATA UNDER PGA REVIEW         01  002001                  " +
									   "WO70NHTOFF012216122901DATA UNDER PGA REVIEW         07  001001                  " +
									   "WO70NHTOFF012216122907MAY PROCEED                   07  002001                  " +
									   "WO70FDARAD012216122907MAY PROCEED                   07  001001                  " +
									   "Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals("The Count should not be increase", 4, dispositionDataCount);

			var c1message2 = Factory.New<MQEDIMessage>();
			c1message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			c1message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse;
			c1message2.EM_MessageNum = "HYEDUSCMT_159673";
			c1message2.EM_Status = EDIMessage.Status.Queued;
			c1message2.EM_MessageText = "B004701SV9C1                                               HTSLA1PRD_75845      " +
										"WO101601SV9  71002057 01001001-00050ACLUYORKTOWN EXPRESS    6A62 051316         " +
										"WO40MACLU6A62S6680206                                                           " +
										"WO40HSSLLCHS222271                                         00000001     00000001" +
										"WO50052716101095BILL ARRIVED                                                    " +
										"WO60052716101098RELEASED                                05271601                " +
										"WO60052716101001ONE USG                                                         " +
										"WO70NHTOFF012216123902                              02  002001                  " +
										"WO70FDARAD012216123902                              02  001001                  " +
										"Y  4701SV9C100006";
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			dispositionDataCount = reLoadJob.OGADispositionCodes.Count;
			AssertEquals("DispositionData Count should be increase", 6, dispositionDataCount);
		}

		#endregion

		JobDeclaration GetDeclaration(ZString entryNum)
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "TEST Importer";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_OH_Importer = importer.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.US_EnableCRL = true;
			dec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			dec.US_EntryFilerCode = "SV9";
			dec.US_EnableENS = false;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			dec.ImportEntryNumber = entryNum;
			Factory.Save();
			return dec;
		}

		MQEDIMessage CreateStatusMessage(ZString msgText)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = msgText;
			return message;
		}
	}
}
