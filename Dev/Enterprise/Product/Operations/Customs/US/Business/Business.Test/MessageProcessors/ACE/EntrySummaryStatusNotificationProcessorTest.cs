using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class EntrySummaryStatusNotificationProcessorTest : ABIProcessorTest<EntrySummaryStatusNotificationProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		public void TestKeysForBlockingParallelProcessing()
		{
			var declaration = GetMergedDeclaration("00000063");
			var message = GetMessageToProcess();
			var logger = new LoggingInformation();
			var processor = new EntrySummaryStatusNotificationProcessor();
			var provider = processor as IKeysForBlockingParallelProcessingProvider;
			var actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			var actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message without AESSE1", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.Empty), actualMetaData);
				AssertEquals(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder, actualKeys.ReturnValue.ResultType);
			});

			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";
			actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message link to Job", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(CusEntryHeader.Schema.TableName, declaration.ActiveEntryHeaders.EntrySummaryEntry.PK, GlbBranch.CurrentBranch.PK, declaration.JE_DeclarationReference)), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:XJ5-00000063|{GlbCompany.CurrentCompany.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});

			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000064                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";
			actualMetaData = provider.GetLinkedBusinessObjectMetaData(message, logger);
			actualKeys = provider.GetSerializationKeysResult(message, logger, actualMetaData.ReturnValue);
			CombineAssertions("Message can not find Job", () =>
			{
				AssertEquals("Meta data", ProcessingResult.New(LinkedBusinessObjectMetaData.New(ZString.Empty, ZGuid.Empty, ZGuid.Empty, $"Entry:XJ5-00000064|{GlbCompany.CurrentCompany.PK}")), actualMetaData);
				AssertArrayEqualsByElements("Keys", new[] { $"Entry:XJ5-00000064|{GlbCompany.CurrentCompany.PK}" }, actualKeys.ReturnValue.Keys.ToArray());
			});
		}

		protected override void EndToEndCore()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = GetMergedDeclaration("00000063");
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryNotification, message.EM_MessageSubType);
			AssertEquals(declaration.ActiveEntryHeaders.EntrySummaryEntry, message.EM_LinkedObject);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
			AssertNotNull("Subject should contain " + declaration.DeclarationReferenceAppendedByFormattedEntryNumber, email);
			AssertEquals("test@cargowise.com", email.Recipients[0].Email);

			message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E191333   090110        122012                    XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.Reload();
			AssertEquals(ZDateTime.Empty, entry.US_ALDate);

			message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E191333   090110        122012Y                   XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			entry.Reload();
			AssertEquals(ZDateTime.Empty, entry.US_ALDate);
		}

		public void TestMessageLinkedToReconAndEmailSent()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00000063";
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var recon = new ReconDeclaration(declaration);

			Factory.Save();

			AssertNotNull(recon.ReconEntry);

			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E171333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"Y  8888XJ5UC00003";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryNotification, message.EM_MessageSubType);
			AssertEquals(recon.ReconEntry.PK, message.EM_LinkedObject.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
			AssertNotNull("Subject should contain " + declaration.DeclarationReferenceAppendedByFormattedEntryNumber, email);
			AssertEquals("test@cargowise.com", email.Recipients[0].Email);
		}

		public void TestUCMessageIncludePGADispositions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "04", "DATA REJECTED PER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "122", "INVALID EXEMPTION (FME) QUALIFIER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "126", "INVALID FILER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "151", "MISSING DATA", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "152", "INVALID CANADIAN PROVINCE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = GetMergedDeclaration("00000063");
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E1P1333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"SO70FDAFOO121515102204DATA REJECTED PER PGA REVIEW040414001001THRU01012322 2    " +
				"SO7103CN1234567890112515153025151152153154155156157158159160                    " +
				"SO712 2222222222  0316150910  122126                                            " +
				"SO72COMMENT1.                                                                   " +
				"SO72COMMENT2.                                                                   " +
				"Y  8888XJ5UC00003";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			CombineAssertions(() =>
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
				var emailBody = email.Body;

				AssertContains("P DESC", emailBody);
				var newFactory = new BusinessObjectFactory();
				var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
				AssertEquals("PGA disposition codes processed", 1, declarationReloaded.OGADispositionCodes.Count);
				var disposition1 = declarationReloaded.OGADispositionCodes[0];
				AssertEquals("04", disposition1.US_Code);
				AssertEquals(new ZDateTime(2015, 12, 15, 10, 22, 00), disposition1.US_DispositionDate);
				AssertEquals("001", disposition1.US_OGADispositionBeginningCBPLine);
				AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);

				AssertEquals(2, disposition1.OGADispositionDetails.Count);
				AssertEquals("151", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
				AssertEquals("152", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
				AssertEquals("03", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
				AssertEquals("CN1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 11, 25, 15, 30, 25), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

				AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
				AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
				AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
				AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);
				AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);

				AssertContains(@"<th>Agency/Quota Indicator</th><th>Disposition Date</th><th>PGA Entry Status Desc</th><th>PGA Line Status Desc</th><th>CBP Line Status</th><th>Beg. CBP Line</th><th>Beg. Tariff</th><th>Beg. PGA Line</th><th>End PGA Line</th><th>End Tariff</th><th>End CBP Line</th><th>Document Type</th><th>PGA Entry Hold Type</th><th>Comment</th><th>Reasons</th></tr></thead><tr><td>FDA</td><td>15-Dec-15 10:22</td><td>04 DATA REJECTED PER PGA REVIEW</td><td>04 DATA REJECTED PER PGA REVIEW</td><td>04 DATA REJECTED PER PGA REVIEW</td><td>001</td><td>&nbsp;</td><td>001</td><td>123</td><td>&nbsp;</td><td>010</td><td>22 - EPA Form 3520-1</td><td>2 - Intensive or document required set directly by agency listed in positions 5-7</td><td>COMMENT1. COMMENT2.</td><td>MISSING DATA, INVALID CANADIAN PROVINCE, INVALID PRODUCT CODE, MISSING OR INVALID DATE, MISSING, INVALID, OR EXPIRED FWS IMPORT/EXPORT LICENSE NUMBER, INVALID FWS PORT, INACCURATE FWS INTENDED USE DESCRIPTION, MISSING CARRIER NAME, MISSING OR INVALID MODE OF TRANSPORTATION CODE, MISSING OR INACCURATE BONDED LOCATION FOR INSPECTION, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER</td>", emailBody);
			});
		}

		public void TestUCMessageIncludePGADispositions_01()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "04", "DATA REJECTED PER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "11", "INTENSIVE – EXAM/SAMPLE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "13", "EXAM- RESOLVED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "102", "REFUSED", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "103", "PARTIAL RELEASE AND REFUSE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "110", "MISMATCH IN REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "111", "CANCELLED MANUFACTURER FACILITY REGISTRATION", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "122", "INVALID EXEMPTION (FME) QUALIFIER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "126", "INVALID FILER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "151", "MISSING DATA", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "152", "INVALID CANADIAN PROVINCE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = GetMergedDeclaration("00000063");
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E1P1333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"SO70FDAFOO123013143913EPA DATA REVIEW             01041122223654THRU933301   201" +
				"SO7103CN1234567890112515153025151152153154155156157158159160                    " +
				"SO712 2222222222  0316150910  122126                                            " +
				"SO72COMMENT1.                                                                   " +
				"SO72COMMENT2.                                                                   " +
				"SO70EPAPST103013143911EPA DATA REVIEW             020112333B777THRU5556602   001" +
				"SO711 3333333333  0616150910  102103                                            " +
				"SO712 4444444444  0316150910  110111                                            " +
				"SO72COMMENT3.                                                                   " +
				"SO72COMMENT4.                                                                   " +
				"Y  8888XJ5UC00003";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			CombineAssertions(() =>
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
				var emailBody = email.Body;

				AssertContains("P DESC", emailBody);
				var newFactory = new BusinessObjectFactory();
				var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
				AssertEquals("PGA disposition codes processed", 2, declarationReloaded.OGADispositionCodes.Count);
				var disposition1 = declarationReloaded.OGADispositionCodes[0];
				AssertEquals("04", disposition1.US_Code);
				AssertEquals(new ZDateTime(2013, 12, 30, 14, 39, 00), disposition1.US_DispositionDate);
				AssertEquals("2222", disposition1.US_OGADispositionBeginningCBPLine);
				AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);

				AssertEquals(2, disposition1.OGADispositionDetails.Count);
				AssertEquals("151", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
				AssertEquals("152", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
				AssertEquals("03", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
				AssertEquals("CN1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 11, 25, 15, 30, 25), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

				AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
				AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
				AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
				AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);

				AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);
				var disposition2 = declarationReloaded.OGADispositionCodes[1];

				AssertEquals(new ZDateTime(2013, 10, 30, 14, 39, 00), disposition2.US_DispositionDate);
				AssertEquals("01", disposition2.US_Code);

				AssertEquals(2, disposition2.OGADispositionDetails.Count);
				AssertEquals("102", disposition2.OGADispositionDetails[0].US_LineSubReasonCode1);
				AssertEquals("103", disposition2.OGADispositionDetails[0].US_LineSubReasonCode2);
				AssertEquals("1", disposition2.OGADispositionDetails[0].US_ReferenceIDQualifier);
				AssertEquals("3333333333", disposition2.OGADispositionDetails[0].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 06, 16, 09, 10, 0), disposition2.OGADispositionDetails[0].US_ReceiptDateTime);

				AssertEquals("110", disposition2.OGADispositionDetails[1].US_LineSubReasonCode1);
				AssertEquals("111", disposition2.OGADispositionDetails[1].US_LineSubReasonCode2);
				AssertEquals("2", disposition2.OGADispositionDetails[1].US_ReferenceIDQualifier);
				AssertEquals("4444444444", disposition2.OGADispositionDetails[1].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition2.OGADispositionDetails[1].US_ReceiptDateTime);
				AssertEquals("COMMENT3. COMMENT4.", disposition2.US_Comment);
				AssertContains(@"<th>Agency/Quota Indicator</th><th>Disposition Date</th><th>PGA Entry Status Desc</th><th>PGA Line Status Desc</th><th>CBP Line Status</th><th>Beg. CBP Line</th><th>Beg. Tariff</th><th>Beg. PGA Line</th><th>End PGA Line</th><th>End Tariff</th><th>End CBP Line</th><th>Document Type</th><th>PGA Entry Hold Type</th><th>Comment</th><th>Reasons</th></tr></thead><tr><td>FDA</td><td>30-Dec-13 14:39</td><td>13 EXAM- RESOLVED</td><td>04 DATA REJECTED PER PGA REVIEW</td><td>01 DATA UNDER PGA REVIEW</td><td>2222</td><td>3</td><td>654</td><td>333</td><td>9</td><td>THRU</td><td>01 - Packing List</td><td>2 - Intensive or document required set directly by agency listed in positions 5-7</td><td>COMMENT1. COMMENT2.</td><td>MISSING DATA, INVALID CANADIAN PROVINCE, INVALID PRODUCT CODE, MISSING OR INVALID DATE, MISSING, INVALID, OR EXPIRED FWS IMPORT/EXPORT LICENSE NUMBER, INVALID FWS PORT, INACCURATE FWS INTENDED USE DESCRIPTION, MISSING CARRIER NAME, MISSING OR INVALID MODE OF TRANSPORTATION CODE, MISSING OR INACCURATE BONDED LOCATION FOR INSPECTION, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER</td></tr><tr><td>EPA</td><td>30-Oct-13 14:39</td><td>11 INTENSIVE – EXAM/SAMPLE</td><td>01 DATA UNDER PGA REVIEW</td><td>02 HOLD INTACT</td><td>333B</td><td>7</td><td>77T</td><td>566</td><td>5</td><td>HRU5</td><td>02 - Invoice</td><td>0 - </td><td>COMMENT3. COMMENT4.</td><td>REFUSED, PARTIAL RELEASE AND REFUSE, MISMATCH IN REGISTRATION, CANCELLED MANUFACTURER FACILITY REGISTRATION</td>", emailBody);
			});
		}

		public void TestUCMessageIncludePGADispositions_02()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "01", "DATA UNDER PGA REVIEW", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "02", "HOLD INTACT", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus, "08", "MOVE TO SECURE HLDNG FCLTY", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "122", "INVALID EXEMPTION (FME) QUALIFIER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "126", "INVALID FILER", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "151", "MISSING DATA", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineSubReason, "152", "INVALID CANADIAN PROVINCE", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = GetMergedDeclaration("00000063");
			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E1P1333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"SO70FDAFOO091416143401DATA UNDER PGA REVIEW       080203000120030101532022   202" +
				"SO7103CN1234567890112515153025151152153154155156157158159160                    " +
				"SO712 2222222222  0316150910  122126                                            " +
				"SO72COMMENT1.                                                                   " +
				"SO72COMMENT2.                                                                   " +
				"Y  8888XJ5UC00003";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			CombineAssertions(() =>
			{
				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
				var emailBody = email.Body;

				AssertContains("P DESC", emailBody);
				var newFactory = new BusinessObjectFactory();
				var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
				AssertEquals("PGA disposition codes processed", 1, declarationReloaded.OGADispositionCodes.Count);
				var disposition1 = declarationReloaded.OGADispositionCodes[0];
				AssertEquals("02", disposition1.US_Code);
				AssertEquals(new ZDateTime(2016, 9, 14, 14, 34, 00), disposition1.US_DispositionDate);
				AssertEquals("0001", disposition1.US_OGADispositionBeginningCBPLine);
				AssertEquals(OGADispositionSourceList.Codes.PGA, disposition1.US_Source);

				AssertEquals(2, disposition1.OGADispositionDetails.Count);
				AssertEquals("151", disposition1.OGADispositionDetails[0].US_LineSubReasonCode1);
				AssertEquals("152", disposition1.OGADispositionDetails[0].US_LineSubReasonCode2);
				AssertEquals("03", disposition1.OGADispositionDetails[0].US_ReferenceIDQualifier);
				AssertEquals("CN1234567890", disposition1.OGADispositionDetails[0].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 11, 25, 15, 30, 25), disposition1.OGADispositionDetails[0].US_ReceiptDateTime);

				AssertEquals("122", disposition1.OGADispositionDetails[1].US_LineSubReasonCode1);
				AssertEquals("126", disposition1.OGADispositionDetails[1].US_LineSubReasonCode2);
				AssertEquals("2", disposition1.OGADispositionDetails[1].US_ReferenceIDQualifier);
				AssertEquals("2222222222", disposition1.OGADispositionDetails[1].US_ReferenceID);
				AssertEquals(new ZDateTime(2015, 03, 16, 09, 10, 0), disposition1.OGADispositionDetails[1].US_ReceiptDateTime);
				AssertEquals("COMMENT1. COMMENT2.", disposition1.US_Comment);
				AssertContains(@"<th>Agency/Quota Indicator</th><th>Disposition Date</th><th>PGA Entry Status Desc</th><th>PGA Line Status Desc</th><th>CBP Line Status</th><th>Beg. CBP Line</th><th>Beg. Tariff</th><th>Beg. PGA Line</th><th>End PGA Line</th><th>End Tariff</th><th>End CBP Line</th><th>Document Type</th><th>PGA Entry Hold Type</th><th>Comment</th><th>Reasons</th></tr></thead><tr><td>FDA</td><td>14-Sep-16 14:34</td><td>01 DATA UNDER PGA REVIEW</td><td>02 HOLD INTACT</td><td>08 MOVE TO SECURE HLDNG FCLTY</td><td>0001</td><td>2</td><td>003</td><td>320</td><td>5</td><td>0101</td><td>22 - EPA Form 3520-1</td><td>2 - Intensive or document required set directly by agency listed in positions 5-7</td><td>COMMENT1. COMMENT2.</td><td>MISSING DATA, INVALID CANADIAN PROVINCE, INVALID PRODUCT CODE, MISSING OR INVALID DATE, MISSING, INVALID, OR EXPIRED FWS IMPORT/EXPORT LICENSE NUMBER, INVALID FWS PORT, INACCURATE FWS INTENDED USE DESCRIPTION, MISSING CARRIER NAME, MISSING OR INVALID MODE OF TRANSPORTATION CODE, MISSING OR INACCURATE BONDED LOCATION FOR INSPECTION, INVALID EXEMPTION (FME) QUALIFIER, INVALID FILER</td></tr>", emailBody);
			});
		}
		
		public void TestQuotaLineStatus()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			Factory.Save();

			var declaration = GetMergedDeclaration("00000063");
			declaration.JE_GS_NKCusAgent = staff.GS_Code;

			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333022090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"E4TA0Q01                                  100.25      KG 8          KG          " +
"E4TA1Q02                                  99          G  100.11     MG          " +
"E4TA2Q03                                  7841225213  OT 89         OT          " +
"E4TA3Q04                                  18          MC 877777.23  TL          " +
"E4TA4Q05                                  1.25        LT 78945614783T           " +
"Y  8888XJ5UC00003";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
			AssertNotNull("Subject should contain " + declaration.DeclarationReferenceAppendedByFormattedEntryNumber, email);
			AssertEquals("test@cargowise.com", email.Recipients[0].Email);
			var emailBody = email.Body;
			AssertContains("<td>Q01 - Quota Processed / Accepted</td>", emailBody);
#if NETFRAMEWORK
			AssertContains("<td>78,412,252.13 OT</td>", emailBody);
			AssertContains("<td>789,456,147.83 T</td>", emailBody);
#else
			AssertContains("<td>78,412,252.130 OT</td>", emailBody);
			AssertContains("<td>789,456,147.830 T</td>", emailBody);
#endif
			AssertContains("An Entry summary is rejected through Quota processing, please see Quota Details for status and proration/apportionment quantities. The filer has 2 working days to respond to a Quota rejection.", emailBody);
		}

		public void TestQuotaLineStoreInCusDisposition_UCMessage()
		{
			var declaration = GetMergedDeclaration("00000063");
			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333022090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"E4001Q01                                 000000010025KG 000000000800KG          " +
"Y  8888XJ5UC00003";
			Factory.Save();

			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			var expectedValue = new Dictionary<ZString, ZString[]>();
#if NETFRAMEWORK
			expectedValue.Add("Q01", new ZString[] { "1", "Q01", ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, "Requested Quota Qty: 100.25 KG  Reserved Quota Qty: 8.00 KG" });
#else
			expectedValue.Add("Q01", new ZString[] { "1", "Q01", ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification, "Requested Quota Qty: 100.250 KG  Reserved Quota Qty: 8.000 KG" });
#endif
			var factory2 = new BusinessObjectFactory();
			var reLoadJob = factory2.Load<JobDeclaration>(declaration.PK);
			var entryLoaded = factory2.Load<CusEntryHeader>(reLoadJob.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("Line1 CusDisposition count", 1, entryLoaded.MergedLines[0].QuotaDispositions.Count);
			AssertQuotaStoreInCusDisposition(entryLoaded.MergedLines[0], message, expectedValue);
		}

		public void TestSetEntryStatusToDispositionCodeRejectedForRecon()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~AA";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00000063";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var recon = new ReconDeclaration(declaration);

			Factory.Save();

			declaration.ActiveEntryHeaders.ReconciliationEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconReplace;

			var staff2 = Factory.New<GlbStaff>();
			staff2.FillWithValidTestData();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "test2@cargowise.com";

			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.ReconciliationEntry, false, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_SystemCreateUser = staff2.GS_Code;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333010090110        100110                    XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Entry Summary Status Notification"));
			AssertEquals("Should be addressed to the one who sent the last 7501", "test2@cargowise.com", email.Recipients[0].Email);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.ReconciliationEntry.PK);
			AssertEquals("entry status should have been updated", ReconMessageStatusList.Codes.ErrorReconReplace, entryLoaded.CH_Status);
			AssertEquals("Anticipated Liquidation Date should not be updated", ZDateTime.Empty, entryLoaded.US_ALDate);

			var messageLoaded = factory2.Load<MQEDIMessage>(message.PK);
			AssertEquals("EM_ApplicationReference should be populated with disposition code to easily search from module", "4:123456789012", messageLoaded.EM_ApplicationReference);

			AssertContains("Disposition date is included", new ZDate(2010, 10, 1).ToShortDateString(), email.Body);
			AssertNotContains("It should be visible only for disposition code 9", "Entry Summary Red-Lined(Liquidation Date unset)", email.Body);
			AssertContains("4 DESC", email.Body);
		}

		public void TestSetEntryStatusToDispositionCodeRejected()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~AA";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;

			var staff2 = Factory.New<GlbStaff>();
			staff2.FillWithValidTestData();
			staff2.GS_Code = "~BB";
			staff2.GS_EmailAddress = "test2@cargowise.com";

			declaration.JE_GS_NKCusAgent = staff2.GS_Code;

			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_SystemCreateUser = staff2.GS_Code;
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E141333010090110        100110                    XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains("Entry Summary Status Notification"));
			AssertEquals("Should be addressed to the one who sent the last 7501", "test2@cargowise.com", email.Recipients[0].Email);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.ErrorEntrySummaryReplace, entryLoaded.CH_Status);
			AssertEquals("Anticipated Liquidation Date should not be updated", ZDateTime.Empty, entryLoaded.US_ALDate);

			var messageLoaded = factory2.Load<MQEDIMessage>(message.PK);
			AssertEquals("EM_ApplicationReference should be populated with disposition code to easily search from module", "4:123456789012", messageLoaded.EM_ApplicationReference);

			AssertContains("Disposition date is included", new ZDate(2010, 10, 1).ToShortDateString(), email.Body);
			AssertNotContains("It should be visible only for disposition code 9", "Entry Summary Red-Lined(Liquidation Date unset)", email.Body);
			AssertContains("4 DESC", email.Body);

			message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E1R1333010090110        100110                    XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			factory2.Load<MQEDIMessage>(message.PK);
			AssertNotNull(Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Body.Contains("4 DESC")));
		}

		public void TestSetEntryStatusToDispositionCodeInactivated()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E151333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.ClearEntrySummaryDelete, entryLoaded.CH_Status);
			Assert(entryLoaded.HasBeenWithdrawn);
		}

		public void TestSetEntryStatusToDispositionCodeCancelled()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			MQEDIMessage message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.EntrySummaryCanceled, entryLoaded.CH_Status);
		}

		public void TestCancelledEntryWithPreliminaryStatement()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals("Precondition: Release Status should be 'Released'", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementNumber = "800400560";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "00000063";
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

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.EntrySummaryCanceled, entryLoaded.CH_Status);
			AssertEquals("Declaration Release Date should be cleared", ZDateTime.Empty, entryLoaded.Declaration.JE_EntryAuthorisationDate);
			AssertEquals("Declaration Release Status shoudl be empty", CRLReleaseStatusList.Codes.CAN, entryLoaded.Declaration.ReleaseStatus);

			line.Reload();
			AssertEquals(StatementLineStatusList.Codes.Deleted, line.B3_Status);
			statement.Reload();
			AssertEquals("If only one statement line, statement should have deleted status", StatementHeaderStatusList.Codes.Deleted, statement.B2_Status);

			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			line.B3_Status = StatementLineStatusList.Codes.Active;

			var line2 = statement.StatementLines.AddNew();
			line2.B3_EntryNum = "00000064";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_Status = StatementLineStatusList.Codes.Active;

			var charge3 = line2.Charges.AddNew();
			charge3.B4_ChargeAmount = 20.50m;
			charge3.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing;

			var charge4 = line.Charges.AddNew();
			charge4.B4_ChargeAmount = 10.50m;
			charge4.B4_ChargeType = Core.Constants.USCustoms.FeeCodes.HMF;

			line2.B3_CustomsFeesTotal = 31m;
			statement.B2_StatementAmount = 66.94m;

			message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			factory2 = new BusinessObjectFactory();
			entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.EntrySummaryCanceled, entryLoaded.CH_Status);
			line.Reload();
			AssertEquals(StatementLineStatusList.Codes.Deleted, line.B3_Status);
			line2.Reload();
			AssertEquals(StatementLineStatusList.Codes.Active, line2.B3_Status);
			statement.Reload();
			AssertEquals(StatementHeaderStatusList.Codes.Preliminary, statement.B2_Status);

			declaration.Logs.GetAllLogs().Load();
			AssertNotNull(declaration.Logs.MostRecentLogByEventTime(Events.AuthorisationWithdrawn, ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification + " - CAN"));
		}

		public void TestCancelledEntryWithFinalOrPaidStatement()
		{
			var declaration = GetMergedDeclaration("00000063");
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Today;
			AssertEquals("Precondition: Release Status should be 'Released'", CRLReleaseStatusList.Codes.REL, declaration.ReleaseStatus);

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			statement.B2_StatementNumber = "800400560";
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;

			var line = statement.StatementLines.AddNew();
			line.B3_EntryNum = "00000063";
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

			var message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			var factory2 = new BusinessObjectFactory();
			var entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.EntrySummaryCanceled, entryLoaded.CH_Status);
			AssertEquals("Declaration Release Date should be cleared", ZDateTime.Empty, entryLoaded.Declaration.JE_EntryAuthorisationDate);
			AssertEquals("Declaration Release Status shoudl be empty", CRLReleaseStatusList.Codes.CAN, entryLoaded.Declaration.ReleaseStatus);

			line.Reload();
			AssertEquals("Payment is in progress, statement line status should not be updated", StatementLineStatusList.Codes.Active, line.B3_Status);
			statement.Reload();
			AssertEquals("Payment is in progress, statement status should not be updated", StatementHeaderStatusList.Codes.Preliminary, statement.B2_Status);

			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			statement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement.B2_PaymentStatus = ZString.Empty;

			message = GetMessageToProcess();
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E161333010090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"Y  8888XJ5UC00002";

			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			message.Reload();
			AssertEquals(MQEDIMessage.Status.Received, message.EM_Status);

			factory2 = new BusinessObjectFactory();
			entryLoaded = factory2.Load<CusEntryHeader>(declaration.ActiveEntryHeaders.EntrySummaryEntry.PK);
			AssertEquals("entry status should have been updated", ImportMessageStatusList.Codes.EntrySummaryCanceled, entryLoaded.CH_Status);

			line.Reload();
			AssertEquals("This is a Final Statement, statement line status should not be updated", StatementLineStatusList.Codes.Active, line.B3_Status);
			statement.Reload();
			AssertEquals("This is a Final Statement, statement status should not be updated", StatementHeaderStatusList.Codes.Final, statement.B2_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("This entry has just been canceled, but it is on a statement, '800400560' which is paid or its ACH Authorization is in progress. System has not adjusted any records. Please follow it up with CBP."));
		}

		public void TestUCMessageLinkedToDrawback()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00000063";
			declaration.JE_GS_NKCusAgent = staffZ1.GS_Code;

			Factory.Save();

			USCustomsDataRegistry.Instance.ABIMessagesGroup.SetValue(Guid.Empty, declaration.RegistryBranchPK, Guid.Empty, new ManifestGroupNotification("ESG", group.PK, false));

			var message = GetMessageToProcess();
			message.EM_MessageText = "B018888XJ5UC                                                                    E122175   050611                                  XJ5  00000063     B00155797   E2                                                                              Y  1101SV9UC00002                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject.Contains(declaration.DeclarationReferenceAppendedByFormattedEntryNumber));
			AssertEquals("Entry Number printed", true, email.Body.Contains("XJ5-0000006-3"));

			var newFactory = new BusinessObjectFactory();
			var declarationReloaded = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals("Processed UC Message and Attached to Job Declaration", 1, declarationReloaded.Messages.Count);
		}

		public void TestSetEntryNumber()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, "UCDSP", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INoFurtherActionRequired, "INoFurtherActionRequired", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IActionRequiredDespiteActionID, "IActionRequiredDespiteActionID", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1", "1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "2", "2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3", "3 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4", "4 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "5", "5 DESC", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "6", "6 DESC", startDate, endDate);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "7", "7 DESC", startDate, endDate);
			var code8 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "8", "8 DESC", startDate, endDate);
			var code9 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "9", "9 DESC", startDate, endDate);
			var codeE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "E", "E DESC", startDate, endDate);
			var codeP = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "P", "P DESC", startDate, endDate);
			var codeQ = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Q", "Q DESC", startDate, endDate);
			var codeR = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "R", "R DESC", startDate, endDate);

			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName2.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, "Y");
			var attributeE1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeE.PK, attributeName2.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName1.ZXE_Name, "Y");
			var attribute71 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName1.ZXE_Name, "Y");
			var attribute81 = helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, attributeName1.ZXE_Name, "Y");
			var attributeQ1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeQ.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			Factory.Save();

			var message = GetMessageToProcess();
			message.EM_MessageText = "B001101SV9UC                                                                    E122175   050611                                  739  62027790     B00155797   E2                                                                              Y  1101SV9UC00002                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotNull(email);
			Assert(email.Body.Contains("<tr><td>Entry Number</td><td>739-6202779-0</td></tr>"));

			var declaration = GetMergedDeclaration("00000063");

			message = GetMessageToProcess();
			message.EM_MessageText = "B001101SV9UC                                                                    E122175   050611                                  XJ5  00000063     B00155797   E2                                                                              Y  1101SV9UC00002                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotNull(email);
			Assert(!email.Body.Contains("<tr><td>Entry Number</td><td>XJ5-0000006-3</td></tr>"));
			Assert(email.Subject.Contains("XJ5-0000006-3"));
		}

		public void TestEntrySummaryStatusNotificationUpdateUC4WithoutE2()
		{
			var declaration = GetMergedDeclaration("00000063");
			var messagewithoutE2 = GetMessageToProcess();
			messagewithoutE2.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E142171022032416                                  XJ5  00000063                 " +
				"E4001Q02   QUOTA APPORTIONED             000001700000NO 000001619047NO          " +
				"Y  8888XJ5UC00003";
			Factory.Save();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();
			messagewithoutE2.Reload();
			AssertEquals("4:ACTION REQUIRED", messagewithoutE2.EM_ApplicationReference);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, messagewithoutE2.EM_ActionStatus);
		}

		public void TestEntrySummaryStatusNotificationUpdateUC4WithE2WithoutActionID()
		{
			var declaration = GetMergedDeclaration("00000063");
			var messageWithoutActionID = GetMessageToProcess();
			messageWithoutActionID.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E142171022032416                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555                                  " +
				"E4001Q02   QUOTA APPORTIONED             000001700000NO 000001619047NO          " +
				"Y  8888XJ5UC00003";
			Factory.Save();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();
			messageWithoutActionID.Reload();
			AssertEquals("4:ACTION REQUIRED", messageWithoutActionID.EM_ApplicationReference);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, messageWithoutActionID.EM_ActionStatus);
		}

		public void TestEntrySummaryStatusNotificationUpdateWithDispositionE()
		{
			var declaration = GetMergedDeclaration("00000063");
			var messageWithActionID = GetMessageToProcess();
			messageWithActionID.EM_MessageText =
				"B018888XJ5UC                                                                    " +
				"E1E2001   060216                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                                                                 " +
				"Y  8888XJ5UC00002";
			Factory.Save();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();
			messageWithActionID.Reload();
			AssertEquals("E:ACTION REQUIRED", messageWithActionID.EM_ApplicationReference);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, messageWithActionID.EM_ActionStatus);
		}

		public void TestEntrySummaryStatusNotificationUpdateWithDisposition2()
		{
			var declaration = GetMergedDeclaration("00000063");
			var messageWithActionID = GetMessageToProcess();
			messageWithActionID.EM_MessageText =
				"B018888XJ5UC                                                                    " +
				"E122001   060216                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                                                                 " +
				"Y  8888XJ5UC00002";
			Factory.Save();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();
			messageWithActionID.Reload();
			AssertEquals("2:ACTION REQUIRED", messageWithActionID.EM_ApplicationReference);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, messageWithActionID.EM_ActionStatus);
		}

		public void TestEntrySummaryStatusNotificationUpdateWithManualRequest()
		{
			var declaration = GetMergedDeclaration("00000063");
			var messageWithActionID = GetMessageToProcess();
			messageWithActionID.EM_MessageText =
				"B018888XJ5UC                                                                    " +
				"E141001   060216                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                                                                 " +
				"Y  8888XJ5UC00002";
			Factory.Save();
			var processor = new ABIIncomingMessageProcessor();
			processor.ExecuteBatch();
			Factory.Save();
			messageWithActionID.Reload();
			AssertEquals("4:ACTION REQUIRED", messageWithActionID.EM_ApplicationReference);
			AssertEquals(EM_ActionStatusList.Codes.Incomplete, messageWithActionID.EM_ActionStatus);
		}

		public void TestNoExceptionThrownWithUS_PaperlessEntryNotInReconDeclarationShouldNotBeSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00000063";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var recon = new ReconDeclaration(declaration);
			Factory.Save();

			var message = GetMessageToProcess();
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
				"B018888XJ5UC                                               34                   " +
				"E131333   090110                                  XJ5  00000063                 " +
				"E2  CHRIS SMITH                     5555555555  123456789012                    " +
				"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
				"Y  8888XJ5UC00003";

			Factory.Save();

			var processor = new ABIIncomingMessageProcessor();
			AssertNoExceptionThrown(() => { processor.ExecuteBatch(); });
		}

		JobDeclaration GetMergedDeclaration(ZString entryNumber)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EntryFilerCode = "XJ5";
			dec.ImportEntryNumber = entryNumber;
			dec.JE_GS_NKCusAgent = staffZ1.GS_Code;

			dec.Invoices.AddNew();
			dec.InvoiceLines.AddNew();

			dec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			return dec;
		}

		MQEDIMessage GetMessageToProcess()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_Status = EDIMessage.Status.Queued;
			return message;
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ENSStatusDispositionCode, "UCDSP", dataGrouping.ZZZ_DataGrouping);
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.INoFurtherActionRequired, "INoFurtherActionRequired", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Enterprise.Customs.Universal.RefCusCodeListAttributeTypes.Codes.IActionRequiredDespiteActionID, "IActionRequiredDespiteActionID", codeType.ZZK_CodeType, Core.Constants.CountryCodes.UnitedStates);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "1", "1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "2", "2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3", "3 DESC", startDate, endDate);
			var code4 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "4", "4 DESC", startDate, endDate);
			var code5 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "5", "5 DESC", startDate, endDate);
			var code6 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "6", "6 DESC", startDate, endDate);
			var code7 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "7", "7 DESC", startDate, endDate);
			var code8 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "8", "8 DESC", startDate, endDate);
			var code9 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "9", "9 DESC", startDate, endDate);
			var codeE = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "E", "E DESC", startDate, endDate);
			var codeP = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "P", "P DESC", startDate, endDate);
			var codeQ = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Q", "Q DESC", startDate, endDate);
			var codeR = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "R", "R DESC", startDate, endDate);

			var attribute21 = helper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, attributeName2.ZXE_Name, "Y");
			var attribute41 = helper.CreateNewOrGetExistingCusCodeListAttribute(code4.PK, attributeName2.ZXE_Name, "Y");
			var attributeE1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeE.PK, attributeName2.ZXE_Name, "Y");
			var attribute61 = helper.CreateNewOrGetExistingCusCodeListAttribute(code6.PK, attributeName1.ZXE_Name, "Y");
			var attribute71 = helper.CreateNewOrGetExistingCusCodeListAttribute(code7.PK, attributeName1.ZXE_Name, "Y");
			var attribute81 = helper.CreateNewOrGetExistingCusCodeListAttribute(code8.PK, attributeName1.ZXE_Name, "Y");
			var attributeQ1 = helper.CreateNewOrGetExistingCusCodeListAttribute(codeQ.PK, attributeName1.ZXE_Name, "Y");
			Factory.Save();
		}
	}
}
