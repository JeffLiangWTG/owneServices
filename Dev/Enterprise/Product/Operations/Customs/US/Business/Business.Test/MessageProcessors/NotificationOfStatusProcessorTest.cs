using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class NotificationOfStatusProcessorTest : ABIProcessorTest<NotificationOfStatusProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestGetKeysForBlockingParallelProcessing_MissingHeader()
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageNum = "~15000";
			incomingMessage.EM_MessageText =
"B011101SV9NS                                                                    " +
"05LEVANT PRIDE           S12033001120430000000                                  " +
"3050DELU05908900                                    0000000001 1206042316DELU   " +
"4063333209855      3001                                                         " +
"50INBOND EXPORTED ON 120604 AT                                                  " +
"503001                                                                          " +
"60 BMSY6804982   BAM5789                                                        " +
"Y  1101SV9NS00006                                                               ";
			incomingMessage.EM_Status = MQEDIMessage.Status.Queued;
			AssertTestGetKeysForBlockingParallelProcessing(incomingMessage, ZString.Empty, null);
		}

		public void TestGetKeysForBlockingParallelProcessing_MultipleHeader()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";
			usBranch.GB_BranchName = "US Branch";
			usBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration1.JE_GB = usBranch.PK;
			var bill1 = declaration1.Bills.AddNew();
			bill1.CU_BillNum = "05908900";
			bill1.US_UI_NKBillIssuerSCAC = "DELU";
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.JE_GB = usBranch.PK;
			var bill2 = declaration2.Bills.AddNew();
			bill2.CU_BillNum = "05908900";
			bill2.US_UI_NKBillIssuerSCAC = "DELU";
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText =
"B011101SV9NS                                                                    " +
"05LEVANT PRIDE           S12033001120430000000                                  " +
"3050DELU05908900                                    0000000001 1206042316DELU   " +
"4063333209855      3001                                                         " +
"50INBOND EXPORTED ON 120604 AT                                                  " +
"503001                                                                          " +
"60 BMSY6804982   BAM5789                                                        " +
"Y  1101SV9NS00006                                                               ";
			message.EM_Status = MQEDIMessage.Status.Queued;
			AssertTestGetKeysForBlockingParallelProcessing(message, declaration1.JE_DeclarationReference, new string[] { declaration2.JE_DeclarationReference.ToString() });
		}

		public void TestGetKeysForBlockingParallelProcessing_OneHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "SV9";
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entry.PK;
			entryNumber.CE_EntryNum = "999999996";
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText =
"B011101SV9NS                                                                    " +
"1061999999996   390100000                                                       " +
"3054APLU001821004                                   0000000000 0612041631APLU   " +
"4061568741095      2704                                                         " +
"50INBOND DELETE                                                                 " +
"60 APZU4145927   4936655                                                        " +
"60 NOSU2405592   4936009                                                        " +
"Y  1101SV9NS00006                                                               ";
			message.EM_Status = MQEDIMessage.Status.Queued;
			AssertTestGetKeysForBlockingParallelProcessing(message, declaration.JE_DeclarationReference, new string[] { $"InBond:SV9-999999996|{GlbCompany.CurrentCompany.PK}" });
		}

		void AssertTestGetKeysForBlockingParallelProcessing(MQEDIMessage incomingMessage, string expectedJobNumber, string[] expectedAdditionalKeys)
		{
			var logger = new LoggingInformation();
			var processor = new NotificationOfStatusProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(incomingMessage, logger);
			var actualKeys = provider.GetSerializationKeysResult(incomingMessage, logger, actualMetaData.ReturnValue);
			CombineAssertions(() =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, expectedJobNumber)), actualMetaData);
				if (expectedAdditionalKeys == null)
				{
					AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
				}
				else
				{
					AssertArrayEqualsByElements("Keys", expectedAdditionalKeys, actualKeys.ReturnValue.Keys.ToArray());
				}
			});
		}

		public void TestDispositionList()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("267");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.ImportEntryNumber = "03050406";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05CPRS0241273375739213087130873801130326000000                                  ",
"304ACPRS130851952006                                0000000624 1304031006       ",
"400126703050406    3801                                                         ",
"60 EMHU00660766  0790930                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();
			var list = DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode);
			AssertEquals(list.Count, processor.DispositionList.Count);
		}

		public void TestEmailNotificationForRailInbond()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var shipment = Factory.New<Integration.Forwarding.IForwardingShipment>();
			var header = Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ParentID = ((BusinessObject)shipment).PK;
			header.BH_ParentTableCode = "JS";
			var movement = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			movement.BM_BH = header.PK;

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05KENCOR HEAVY HAUL LTD  040423401120823000000                                  ",
"3050KEQH04042                                       0000000002 1209131748KEQH   ",
"4062388915796      5301                                                         ",
"50INBOND EXPORTED ON 120913 AT                                                  ",
"505301                                                                          ",
"60 NC                                                                           ");
			processor.Process();

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Status Notification Response for Unknown/KEQH04042", email.Subject);
			AssertEquals("Message cannot be attached to job, because no block 10 with InBond number and import declaration with entry doesn't exists for this shipment",
				Guid.Empty, response.EM_LinkUniqueID);
		}

		public void TestMessageForRailShipment()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";
			usBranch.GB_BranchName = "US Branch";
			usBranch.GB_RL_NKHomePort = "USPT";
			usBranch.GB_Phone = "1234567890";
			usBranch.GB_State = "STAT";
			usBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "CPRS131482103785";
			consol.JK_RL_NKLoadPort = "CAONT";
			consol.JK_RL_NKDischargePort = "USDET";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "EMHU638491";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
			shipment.JS_UniqueConsignRef = "SLAXRI1300137464";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_JC = container1.PK;
			packLine1.JL_PackageCount = 1;

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;

			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_EntryFilerCode = "267";
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "131482103785";
			declaration.JE_MasterBillIssuerSCAC = "CPRS";
			declaration.JE_GB = usBranch.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.ImportEntryNumber = "03054911";
			Factory.Save();

			AssertEquals("Precondition: JS_HouseBill number should be empty", ZString.Empty, shipment.JS_HouseBill);
			AssertEquals("Precondition: declaration bills - should contains one MB", 1, declaration.Bills.Count);
			AssertEquals("Precondition: declaration master bill", "131482103785", declaration.Bills[0].CU_BillNum);
			AssertEquals("Precondition: declaration JE_MasterBillIssuerSCAC", "CPRS", declaration.JE_MasterBillIssuerSCAC);
			AssertEquals("Precondition: US_UI_NKBillIssuerSCAC", "CPRS", declaration.Bills[0].US_UI_NKBillIssuerSCAC);

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05CPRS0241303375811213151131513801130528000000                                  ",
"301ICPRS131482103785                                0000000026 1306011351       ",
"4000               3801                                                         ",
"50DETROIT,MI. 3801 HOLD FOR EXAM                                                ",
"50NO ENTRY                                                                      ",
"60 EMHU00638491  0790945                                                        ");
			processor.Process();

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("Status Notification Response for SLAXRI1300137464", email.Subject);
			AssertEquals(declaration.Bills[0].PK, response.EM_LinkUniqueID);
		}

		public void TestAttachStatusNotificationToJobByEntryNumber()
		{
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("267");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.ImportEntryNumber = "03050406";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05CPRS0241273375739213087130873801130326000000                                  ",
"304ACPRS130851952006                                0000000624 1304031006       ",
"400126703050406    3801                                                         ",
"60 EMHU00660766  0790930                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); }));
			AssertEquals("Status Notification Response for B00001000 / 267-0305040-6", email.Subject);
			AssertEquals("Message should be attached to declaration job", declaration.ActiveEntryHeaders.EntrySummaryEntry.PK, response.EM_LinkUniqueID);
		}

		public void TestAttachStatusNotificationToJobWhenNoEntryNumberInBlock40()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";

			var header = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header.BH_ParentID = declaration.PK;
			header.BH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var bill = (Customs.Business.CusInBondBill)header.Bills.AddNew();
			bill.B0_IssuerCode = "EGLV";
			bill.B0_MasterBillNumber = "020300060810";

			var movement = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveHeader>();
			movement.BM_BH = header.PK;
			movement.InBondNumber = "693000140";

			var moveDetail = Factory.New<Integration.Customs.US.InBond.ICusInBondMoveDetail>();
			moveDetail.B9_BM = movement.PK;
			moveDetail.B9_B0 = bill.PK;

			var header2 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			header2.BH_IsActive = false;

			var bill2 = (Customs.Business.CusInBondBill)header2.Bills.AddNew();
			bill2.B0_IssuerCode = "EGLV";
			bill2.B0_MasterBillNumber = "020300060810";

			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"3019EGLV020300060810                                0000000050 1304181143ODFL   ",
"4061693000140      2709                                                         ",
"50ACTUAL ARRIVAL AT 2709 130417                                                 ",
"60 EMCU6097560   EMCFGW0681                                                     ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); }));
			AssertEquals("Status Notification Response for B00001000 / 693000140", email.Subject);
			AssertEquals("Message should be attached to inbond job", movement.PK, response.EM_LinkUniqueID);
		}

		public void TestAttachStatusNotificationToJobByMasterBillNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "267";

			var inbond = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ParentID = declaration.PK;
			inbond.BH_ParentTableCode = declaration.TablePrefix;

			var bill1 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "130851952006";
			bill1.B0_IssuerCode = "CPRS";

			var moveHeader = inbond.MovementHeader;
			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "1";
			Factory.Save();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification;
			response.EM_Status = EDIMessage.Status.Queued;

			var processor = new NotificationOfStatusProcessor();
			processor.Message = response;

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05CPRS0241273375739213087130873801130326000000                                  ",
"3096CPRS130851952006                                0000000624 1303280841       ",
"4000               3801                                                         ",
"50TRAIN CONSIST AT 3801                                                         ",
"50CPRS0241273375739213087                                                       ",
"60 EMHU00660766  0790930                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); }));
			AssertEquals("Status Notification Response for B00001000 /", email.Subject);
			AssertEquals("Message should be attached to declaration job", moveHeader.PK, response.EM_LinkUniqueID);
		}

		protected override void EndToEndCore()
		{
			CreateTestData("001821004", "001821005", "999999996", "");

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061999999996   390100000                                                       ",
"3054APLU001821004                                   0000000000 0612041631APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			EmailDef email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); }));
			ZString body = email.Body;
			Assert("One email sent to users", body.Contains("INBOND DELETE"));
		}

		public void TestAttachToJobBackwardCompatibility()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.US_EntryFilerCode = "XJ5";

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = entry.PK;
			entryNumber.CE_EntryNum = "999999996";
			entryNumber.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNumber.CE_ParentTable = CusEntryHeaderSchema.Constants.TableName;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061999999996   390100000                                                       ",
"3054APLU001821004                                   0000000000 0612041631APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertEquals("Should be linked to declaration job - inbond entry", entry.PK, message.EM_LinkUniqueID);
		}

		public void TestGetEmailAddressToSendTo()
		{
			var group = Factory.New<GlbGroup>();
			var user1 = group.Staff.AddNew();
			user1.GS_Code = "US1";
			user1.GS_LoginName = "user1";
			user1.GS_EmailAddress = "user1@cargowise.com.au";
			var user2 = group.Staff.AddNew();
			user2.GS_Code = "US2";
			user2.GS_LoginName = "user2";
			user2.GS_EmailAddress = "user2@cargowise.com.au";
			Factory.Save();

			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK, false));

			CreateTestData("001821004", "001821005", "999999996", "");
			declaration.JE_GS_NKCusAgent = user2.GS_Code;

			var outgoing1 = Factory.New<MQEDIMessage>();
			outgoing1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing1.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			entry.Messages.Add(outgoing1);

			Factory.Save();

			var outgoing2 = Factory.New<MQEDIMessage>();
			outgoing2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing2.EM_MessageText = "A " + MQEDIMessage.MessageNumberPlaceHolder + " B";
			entry.Messages.Add(outgoing2);

			Factory.Save();

			// Process ingoing message
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061999999996   390100000                                                       ",
"3054APLU001821004                                   0000000000 0612041631APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(
				new AABIOutputB()
				{
					FilerCode = "XJ5"
				},
				new AABIOutputY()
				{
					FilerCode = "XJ5"
				});
			processor.Process();

			// Check that result were sent to the current user (sender of the last message to ABI)
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{
				return emailToMatched.Subject.Contains("Status Notification");
			}));
			AssertEquals(2, email.Recipients.Count);
			AssertEquals("Send Email to ABIGroup", true, email.Recipients.Contains("dong2@pretend.email.com"));
		}

		public void TestInvalidMessageFormatExceptionWhenThereIsNoINBNS10()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"3054APLU001821004                                   0000000000 1602040601APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertNotNull("An email with subject containing 'Status Notification' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); })));
		}

		public void TestInvalidMessageFormatExceptionWhenThereIsNoEntry()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061999999996   390100000                                                       ",
"3054APLU001821004                                   0000000000 1602040601APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertNotNull("An email with subject containing 'Status Notification' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); })));
		}

		public void TestInvalidMessageFormatExceptionWhenThereIsNoBill()
		{
			CreateTestData("", "", "999999996", "");

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061999999996   390100000                                                       ",
"3054APLU001821004                                   0000000000 1602040601APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertNotNull("An email with subject containing 'Status Notification' should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); })));
		}

		public void TestStatusNotificationForACEM1()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05LEVANT PRIDE           S12033001120430000000                                  ",
"3050XXXZS1211259                                    0000000001 1206042316XXXZ   ",
"4063333209855      3001                                                         ",
"50INBOND EXPORTED ON 120604 AT                                                  ",
"503001                                                                          ",
"60 BMSY6804982   BAM5789                                                        ");

			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "SV9" }, new AABIOutputY() { FilerCode = "SV9" });
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.Process();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification"); }));
			var body = email.Body;
			Assert("One email sent to users", body.Contains("INBOND EXPORTED"));
		}

		public void TestDoNotLinkToEmptyInBondNumber()
		{
			CreateTestData("001821004", "", "", "");
			declaration.US_EntryFilerCode = "XJ5";
			var cusEntryNumUS = Factory.New<CusEntryNumber>();
			cusEntryNumUS.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			cusEntryNumUS.CE_EntryNum = "";
			cusEntryNumUS.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			cusEntryNumUS.CE_ParentID = entry.PK;
			cusEntryNumUS.CE_ParentTable = entry.TableName;
			var cusEntryNumAU = Factory.New<CusEntryNumber>();
			cusEntryNumAU.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.InBond;
			cusEntryNumAU.CE_EntryNum = "";
			cusEntryNumAU.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			cusEntryNumAU.CE_ParentID = entry.PK;
			cusEntryNumAU.CE_ParentTable = entry.TableName;
			Factory.Save();
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1061            390100000                                                       ",
"3054APLU001821004                                   0000000000 0612041631APLU   ",
"4061568741095      2704                                                         ",
"50INBOND DELETE                                                                 ",
"60 APZU4145927   4936655                                                        ",
"60 NOSU2405592   4936009                                                        ");

			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();
			message.Reload();
			AssertEquals("EM_LinkTable", ZString.Empty, message.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, message.EM_LinkUniqueID);
		}

		public void TestStatusNotificationForLegacyEntry()
		{
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1062333209100   250697108                                                       ",
"3011TURBBSANOI101101                                0000000001 1203301524SCLS   ",
"4062333209100      2506                                                         ",
"50INBOND ARRIVED ON                                                             ",
"50120330                                                                        ",
"60 NC                                                                           ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertNotNull("An email object with subject containing the expected notification should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for Unknown/TURBBSANOI101101"); })));
			AssertNotNull("An email object with body containing the expected notification should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("Status Notification Response for Unknown/TURBBSANOI101101"); })));
		}

		public void TestStatusNotificationForAttacheeIndentifierIncludingSCAC()
		{
			CreateTestData("", "", "999999996", "");

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1062005449721   230455976                                                       ",
"306HJTWRJ304895890                                  0000000100 1103072118JTWR   ",
"4000               2709                                                         ",
"50DO NOT LOAD. INADEQUATE CARGO                                                 ",
"50DESCRIPTION. AMMEND AND RESUBM                                                ",
"60 MOLU0038948   SJ093984X                                                      ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			processor.Process();

			AssertNotNull("An email object with body containing the expected notification should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Body.Contains("Message for JTWRJ304895890"); })));
		}

		public void TestCastingIssue00869499()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";
			usBranch.GB_BranchName = "US Branch";
			usBranch.GB_RL_NKHomePort = "USPT";
			usBranch.GB_Phone = "1234567890";
			usBranch.GB_State = "STAT";
			usBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var amsHeader = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			amsHeader.BH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			amsHeader.BH_JobReference = "AMS0001";
			var amsBill = (Customs.Business.CusInBondBill)amsHeader.Bills.AddNew();
			amsBill.B0_IssuerCode = "EGLV";
			amsBill.B0_MasterBillNumber = "J304895890";
			var amsMovement = amsHeader.MovementHeader;
			var amsMoveDetail = amsMovement.MovementDetails.AddNew(amsBill.PK);

			var inbHeader = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbHeader.BH_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			inbHeader.BH_JobReference = "INB0001";
			var inbBill = (Customs.Business.CusInBondBill)inbHeader.Bills.AddNew();
			inbBill.B0_IssuerCode = "EGLV";
			inbBill.B0_MasterBillNumber = "J304895890";
			var inbMovement = inbHeader.MovementHeader;
			var inbMoveDetail = inbMovement.MovementDetails.AddNew(inbBill.PK);

			var declaration1 = factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_DeclarationReference = "B000001";
			declaration1.JE_MasterBill = "J304895891";
			declaration1.JE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			declaration1.JE_GB = usBranch.PK;

			var declaration2 = factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "B000002";
			declaration2.JE_MasterBill = "J304895891";
			declaration2.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration2.JE_MasterBillIssuerSCAC = "EGLV";
			declaration2.JE_GB = usBranch.PK;
			factory.Save();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"306HEGLVJ304895890                                  0000000100 1103072118JTWR   ",
"4000               2709                                                         ",
"50DO NOT LOAD. INADEQUATE CARGO                                                 ",
"50DESCRIPTION. AMMEND AND RESUBM                                                ",
"60 MOLU0038948   SJ093984X                                                      ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			AssertNotNull("An email object with body containing the expected notification should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for INB0001"); })));

			processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.Message = message;
			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1062005449721   230455976                                                       ",
"306HEGLVJ304895891                                  0000000100 1103072118JTWR   ",
"4000               2709                                                         ",
"50DO NOT LOAD. INADEQUATE CARGO                                                 ",
"50DESCRIPTION. AMMEND AND RESUBM                                                ",
"60 MOLU0038948   SJ093984X                                                      ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			AssertNotNull("An email object with body containing the expected notification should have been created.", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for B000002"); })));
		}

		public void TestMatchMultipleDeclarationBySCACAndMasterBillNumber()
		{
			var notificationForPM = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			notificationForPM.GS_EmailAddress = "test@cargowise.com";

			var caCompany = Factory.New<GlbCompany>();
			caCompany.GC_Code = "CAC";
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "CAN";
			caBranch.GB_BranchName = "CA Branch";
			caBranch.GB_RL_NKHomePort = "CAPT";
			caBranch.GB_Phone = "1234567890";
			caBranch.GB_State = "STAT";
			caBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "USB";
			usBranch.GB_BranchName = "US Branch";
			usBranch.GB_RL_NKHomePort = "USPT";
			usBranch.GB_Phone = "1234567890";
			usBranch.GB_State = "STAT";
			usBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var caDeclaration = factory.New<Customs.Business.BaseJobDeclaration>();
				var cabill = caDeclaration.Bills.AddNew();
				cabill.CU_BillNum = "05908900";
				cabill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				cabill.CU_MasterBill = "05908900";
				cabill.Declaration.JE_GB = caBranch.PK;
				cabill.Declaration.JE_DeclarationReference = "B0000011";

				var usDeclaration = factory.New<JobDeclaration>();
				var usbill = usDeclaration.Bills.AddNew();
				usbill.CU_BillNum = "05908900";
				usbill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				usbill.CU_MasterBill = "05908900";
				usbill.US_UI_NKBillIssuerSCAC = "DELU";
				usbill.Declaration.JE_GB = usBranch.PK;
				usbill.Declaration.JE_DeclarationReference = "B0000012";
				factory.Save();
			}

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05LEVANT PRIDE           S12033001120430000000                                  ",
"3050DELU05908900                                    0000000001 1206042316DELU   ",
"4063333209855      3001                                                         ",
"50INBOND EXPORTED ON 120604 AT                                                  ",
"503001                                                                          ",
"60 BMSY6804982   BAM5789                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "SV9" }, new AABIOutputY() { FilerCode = "SV9" });
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			AssertNotNull("Status Notification Response for B0000012", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for B0000012"); })));

			var prCompany = Factory.New<GlbCompany>();
			prCompany.GC_Code = "PRC";
			prCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			var prBranch = prCompany.Branches.AddNew();
			prBranch.GB_Code = "PRB";
			prBranch.GB_BranchName = "PR Branch";
			prBranch.GB_RL_NKHomePort = "PRPT";
			prBranch.GB_Phone = "1234567890";
			prBranch.GB_State = "STAT";
			prBranch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.PuertoRico;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.PuertoRico))
			{
				var usDeclaration = factory.New<JobDeclaration>();
				var usbill = usDeclaration.Bills.AddNew();
				usbill.CU_BillNum = "05908900";
				usbill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				usbill.CU_MasterBill = "05908900";
				usbill.US_UI_NKBillIssuerSCAC = "DELU";
				usbill.Declaration.JE_GB = usBranch.PK;
				usbill.Declaration.JE_DeclarationReference = "B0000013";
				factory.Save();
			}

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05LEVANT PRIDE           S12033001120430000000                                  ",
"3050DELU05908900                                    0000000001 1206042316DELU   ",
"4063333209855      3001                                                         ",
"50INBOND EXPORTED ON 120604 AT                                                  ",
"503001                                                                          ",
"60 BMSY6804982   BAM5789                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "SV9" }, new AABIOutputY() { FilerCode = "SV9" });
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			AssertNotNull("Status Notification Response for B0000012", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for B0000012"); })));
			AssertNotNull("Status Notification Response for B0000013", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for B0000013"); })));
		}

		public void TestMatchMultipleInBondBySCACAndMasterBillNumber()
		{
			var inbond1 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond1.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;

			var moveHeader1 = inbond1.MovementHeader;
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			((Integration.Customs.US.InBond.ICusInBondMoveHeader)moveHeader1).InBondNumber = "775365463";

			var bill1 = (Customs.Business.CusInBondBill)inbond1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "05908900";
			bill1.B0_IssuerCode = "DELU";

			var movementDetail1 = (CusInBondMoveDetail)moveHeader1.MovementDetails.AddNew();
			movementDetail1.B9_B0 = bill1.PK;
			movementDetail1.B9_SeqNo = "1";

			var inbond2 = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond2.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;

			var moveHeader2 = inbond2.MovementHeader;
			moveHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			((Integration.Customs.US.InBond.ICusInBondMoveHeader)moveHeader2).InBondNumber = "775365464";

			var bill2 = (Customs.Business.CusInBondBill)inbond2.Bills.AddNew();
			bill2.B0_MasterBillNumber = "05908900";
			bill2.B0_IssuerCode = "DELU";

			var movementDetail2 = (CusInBondMoveDetail)moveHeader2.MovementDetails.AddNew();
			movementDetail2.B9_B0 = bill2.PK;
			movementDetail2.B9_SeqNo = "2";

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"05LEVANT PRIDE           S12033001120430000000                                  ",
"3050DELU05908900                                    0000000001 1206042316DELU   ",
"4063333209855      3001                                                         ",
"50INBOND EXPORTED ON 120604 AT                                                  ",
"503001                                                                          ",
"60 BMSY6804982   BAM5789                                                        ");

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "SV9" }, new AABIOutputY() { FilerCode = "SV9" });
			AssertNoExceptionThrown(delegate
			{ processor.Process(); });

			AssertNotNull("Status Notification Response for 775365463", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for  / 775365463"); })));
			var inbond1Messages = ((IMessageAttacheeWithCBPSenderReference)movementDetail1.MoveHeader).Messages;
			AssertEquals(1, inbond1Messages.Count);
			var message1 = (MQEDIMessage)inbond1Messages.FirstOrDefault();
			AssertEquals(EDIMessage.Status.Queued, message1.EM_Status);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification, message1.EM_MessageSubType);

			AssertNotNull("Status Notification Response for 775365464", Env.OutgoingCustomsMailManager.EmailsCreated.Find(new Predicate<EmailDef>(delegate (EmailDef emailToMatched)
			{ return emailToMatched.Subject.Contains("Status Notification Response for  / 775365464"); })));
			var inbond2Messages = ((IMessageAttacheeWithCBPSenderReference)movementDetail2.MoveHeader).Messages;
			AssertEquals(1, inbond2Messages.Count);
			var message2 = (MQEDIMessage)inbond2Messages.FirstOrDefault();
			AssertEquals(EDIMessage.Status.Received, message2.EM_Status);
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification, message2.EM_MessageSubType);
		}

		public void TestDispositionCodesForMultipleBillOnAirBond()
		{
			var inbond = (Customs.Business.CusInBondHeader)Factory.New<Integration.Customs.US.InBond.ICusInBondHeader>();
			inbond.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.InBond;
			inbond.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;

			var moveHeader = inbond.MovementHeader;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			((Integration.Customs.US.InBond.ICusInBondMoveHeader)moveHeader).InBondNumber = "775365463";

			var bill1 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill1.B0_MasterBillNumber = "29714581571";
			bill1.B0_IssuerCode = "C1";
			bill1.B0_HouseBillNumber = "13321140223";
			var bill2 = (Customs.Business.CusInBondBill)inbond.Bills.AddNew();
			bill2.B0_MasterBillNumber = "29714581571";
			bill2.B0_IssuerCode = "C1";
			bill2.B0_HouseBillNumber = "13321140225";

			var moveDetail1 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail1.B9_SeqNo = "0001";
			var moveDetail2 = (CusInBondMoveDetail)moveHeader.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			moveDetail2.B9_SeqNo = "0002";
			Factory.Save();

			processor.SetBAndYBlock(new AABIOutputB() { FilerCode = "XJ5" }, new AABIOutputY() { FilerCode = "XJ5" });

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1062775365463   380180107                                                       ",
"301D297 14581571        013321140223                0000000067 2107281736EACQ   ",
"4062775365463      3901                                                         ",
"50EFL GLOBAL LLC                                                                ");

			processor.Process();

			AddMessageBlocksToProcessor(processor, ACEApplicationIdentifierCodeList.Codes.InbondStatusNotification,
"1062775365463   380100000                                                       ",
"3011297 14581571        013321140225                0000000067 2107291225EACQ   ",
"4062775365463      3801H856                                                     ");

			processor.Process();

			Factory.Save();

			var dispositionCodesForBill1 = new DispositionDataCollection(moveDetail1);
			dispositionCodesForBill1.Load();
			AssertEquals(1, dispositionCodesForBill1.Count);
			AssertEquals("1D", dispositionCodesForBill1[0].US_Code);

			var dispositionCodesForBill2 = new DispositionDataCollection(moveDetail2);
			dispositionCodesForBill2.Load();
			AssertEquals(1, dispositionCodesForBill2.Count);
			AssertEquals("11", dispositionCodesForBill2[0].US_Code);
		}

		#region Implementation

		NotificationOfStatusProcessor processor;
		MQEDIMessage message;
		JobDeclaration declaration;
		Bill bill;
		Bill bill2;
		CusEntryHeader entry;
		CusContainer container;

		void CreateTestData(ZString bill1Num, ZString bill2Num, ZString entryNum, ZString containerNum)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;

			if (!bill1Num.IsEmpty)
			{
				bill = declaration.Bills.AddNew();
				bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				bill.CU_MasterBill = bill1Num;
			}

			if (!bill2Num.IsEmpty)
			{
				bill2 = declaration.Bills.AddNew();
				bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
				bill2.CU_MasterBill = bill2Num;
			}

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.EntryNumber = entryNum;

			if (!containerNum.IsEmpty)
			{
				container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerNum;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			processor = new NotificationOfStatusProcessor();
			message = Factory.New<MQEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.Message = message;
		}

		#endregion
	}
}
