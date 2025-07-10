using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.Business.EntrySummaryPrinting;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Business.MessageBuilders.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using DbConnection = CargoWise.Data.DbConnection;
using IBillDetails = Enterprise.Customs.US.Business.MessageBuilders.IBillDetails;
using ICusEntryLine = Enterprise.Customs.US.Business.MessageBuilders.ICusEntryLine;
using TimeZoneInfo = System.TimeZoneInfo;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusEntryHeader))]
	sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
	{
		public void TestIsSelfCertification()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EntryFilerCode = "XCd";
			dec.US_EnableENS = true;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_DisclaimSanctions = true;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry as ICusEntryHeader;
			AssertEquals(true, entryHeader.IsSelfCertification);

			invoiceLine.US_DisclaimSanctions = false;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(false, entryHeader.IsSelfCertification);
			var invoiceLine1 = dec.InvoiceLines.AddNew();
			var fishing = invoiceLine1.FishingInformations.AddNew();
			fishing.US_MethodOfHarvest = "Test";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(true, entryHeader.IsSelfCertification);

			invoiceLine1.FishingInformations.RemoveAndDeleteAll();
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(false, entryHeader.IsSelfCertification);
			var invoiceLine2 = dec.InvoiceLines.AddNew();
			var mining = invoiceLine2.MiningInformations.AddNew();
			mining.CountryOfMining = "CA";
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(true, entryHeader.IsSelfCertification);
		}

		public void TestICusDispositionParentMembers()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var iAESCusDispositionParent = entryHeader as ICusDispositionParent;
			AssertEquals("ParentTableCode", CusEntryHeaderSchema.Constants.Prefix, iAESCusDispositionParent.ParentTableCode);
			AssertEquals("CollectionMaster", entryHeader.PK, iAESCusDispositionParent.CollectionMaster.PK);
		}

		public void TestAESCusDispositions()
		{
			var entryHeader = Factory.New<CusEntryHeader>();
			var disposition = entryHeader.AESCusDispositions.AddNew();

			AssertEquals(typeof(AESCusDispositionCollection), entryHeader.AESCusDispositions.GetType());
			AssertEquals(1, entryHeader.AESCusDispositions.Count);
			AssertEquals(false, disposition.IsDeleted);

			entryHeader.Delete();
			AssertEquals(0, entryHeader.AESCusDispositions.Count);
			AssertEquals(true, disposition.IsDeleted);
		}

		[ExpectNoExceptions]
		public void TestAllAddInfoColumnsAreInModelView()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			ModelViewTestHelper.AssertAllAddInfoColumnsAreInModelView(cusEntryHeader,
				"USCusEntryHeader",
				schemaTypeName: nameof(AutoCusEntryHeader.Schema));
		}

		public void TestInBondNumberIsAssignCorrectly()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableENS = true;
				declaration.US_EnableINB = true;
				declaration.AllocateEntryNumber("12345678");
				var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
				inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				inBondHeader.FillInInBondNumberIfInBond();
				AssertNotEquals("", inBondHeader.EntryNumber);
				AssertNotEquals("12345678", inBondHeader.EntryNumber);
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				entryHeader.FillInFormalEntryNumber();
				AssertEquals("12345678", entryHeader.EntryNumber);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestGetFromJobBranchCurrentTime()
		{
			var company1 = Factory.New<GlbCompany>();
			company1.GC_Code = "AAA";

			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			branch1.GB_Code = "CCC";
			branch1.GB_GC = company1.PK;
			branch1.GB_RL_NKHomePort = "USPHL";
			branch2.GB_Code = "DDD";
			branch2.GB_RL_NKHomePort = "USLAX";
			branch2.GB_GC = company1.PK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "TTT";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				declaration.JE_GB = branch1.PK;

				var localTimeBranch1 = ZDateTime.Now;
				var utcTimeNow = DateTime.UtcNow;
				var centralimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				var centralTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow, centralimeInfo);
				AssertEquals("Branch1 Time Zone", centralTimeZone.TimeOfDay.Hours, localTimeBranch1.TimeOfDay.Hours);

				using (Env.SetTemporaryUserContext(company1.PK.ToGuid(), branch2.PK.ToGuid(), department.PK.ToGuid()))
				{
					var localTimeBranch2 = ZDateTime.Now;
					var utcTimeNow2 = DateTime.UtcNow;

					var log = entry.LogManager.AddALogIfNecessary("", ImportMessageStatusList.Codes.ClearDepartureOriginal, new ImportMessageStatusList());
					AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log.SL_Reference);

					var pacificTimeInfo = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
					var pacificTimeZone = TimeZoneInfo.ConvertTimeFromUtc(utcTimeNow2, pacificTimeInfo);
					AssertEquals("Branch2 Time Zone", pacificTimeZone.TimeOfDay.Hours, localTimeBranch2.TimeOfDay.Hours);

					AssertEquals("Wrote by Job Branch Time", localTimeBranch1.TimeOfDay.Hours, log.SL_EventTime.TimeOfDay.Hours);
				}
			}
		}

		[TestDate(2012, 4, 1)]
		public void TestReportLimitHasReachedIfNeededForBranch()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "bob@where.com";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "!2s";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "3$3";
			staff2.GS_LoginName = "32!";
			staff2.GS_EmailAddress = "joe@who.com";
			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "4$4";
			staff3.GS_LoginName = "95!";
			staff3.GS_EmailAddress = "jay@how.com";
			Factory.Save();
			USCustomsDataRegistry.Instance.NumberRangeLimitWarningGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new GroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, group.PK));
			var connection = ((IDbConnected)Factory).Connection;
			connection.BeginTransaction();
			try
			{
				DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
				var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var branchNumberFountain = Env.NumberFountains.USInBondNumberFountain(GlbBranch.CurrentBranch.PK.ToGuid());
				branchNumberFountain.SetNext(Factory, (long)(numberRange.LastNumber - 101));
				var dateTime = ZDateTime.Now.AddHours(-1).ToDateTime();
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.US_EnableINB = true;
				var inBondHeader = declaration.CustomsEntryHeaders.AddNew();
				inBondHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				inBondHeader.FillInInBondNumberIfInBond();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
				AssertMultilineASCIIEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') is running out.\r\nThere are only 101 numbers remaining.\r\nYou will need to prepare to allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
				AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
				AssertEquals(2, email.Recipients.Count);
				AssertEquals("Email contains " + staff.GS_EmailAddress, true, email.Recipients.Contains(staff.GS_EmailAddress));
				AssertEquals("Email contains " + staff2.GS_EmailAddress, true, email.Recipients.Contains(staff2.GS_EmailAddress));
				while (branchNumberFountain.GetNext(Factory) != numberRange.LastNumber)
				{
				}

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				inBondHeader.EntryNumber = ZString.Empty;
				inBondHeader.FillInInBondNumberIfInBond();
				AssertEquals("Should not run as it's not the right time", 0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				staff.GS_EmailAddress = ZString.Empty;
				GlbStaff.CurrentUser.GS_EmailAddress = ZString.Empty;
				Factory.Save();
				inBondHeader.EntryNumber = ZString.Empty;
				inBondHeader.FillInInBondNumberIfInBond();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("Subject", "INBOND NUMBER RANGE LIMIT WARNING", email.Subject);
				AssertMultilineASCIIEquals("Body", string.Format("The In-Bond Number Range set up for branch ('{0} - {1}') has run out.\r\nPlease allocate a new number range.", GlbBranch.CurrentBranch.GB_Code, GlbBranch.CurrentBranch.GB_BranchName), email.Body);
				AssertEquals(ZDateTime.Now.AddDays(1), DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.GetValueWithoutFallback(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("Email", staff2.GS_EmailAddress, email.Recipients[0].Email);
				staff2.GS_EmailAddress = ZString.Empty;
				Factory.Save();
				DataRegistry.Business.USCustomsDataRegistry.Instance.NextInBondNumberLimitWarningReportRun.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, dateTime);
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				inBondHeader.EntryNumber = ZString.Empty;
				inBondHeader.FillInInBondNumberIfInBond();
				AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestRefreshHasBeenLodgedAtCustoms()
		{
			var fac1 = new BusinessObjectFactory();
			var fac2 = new BusinessObjectFactory();

			var declaration = fac1.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			fac1.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals(false, entry.HasBeenLodgedAtCustoms);

			var newEntry = fac2.Load<CusEntryHeader>(entry.PK);
			newEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			fac2.RefreshEnabled = false;
			fac2.Save();

			declaration.ReloadExistingDataRelatedToImportEntryNumberAllocation();
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);
		}

		public void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var msgAttachee = declaration.ActiveEntryHeaders.EntrySummaryEntry as IMessageAttacheeWithCBPSenderReference;
			AssertEquals(Core.Constants.TransportModes.Sea, msgAttachee.TransportMode);
		}

		public void TestIsEBondMessageAttacher()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			Assert("Should be false as the entry doesn't have any EBond messages.", !ensEntry.IsEBondMessageAttacher);
			Assert("Should be false as the entry is not Entry Summary.", !seEntry.IsEBondMessageAttacher);

			var messageForEntrySummaryEntry = Factory.New<EBondEDIMessage>();
			messageForEntrySummaryEntry.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			messageForEntrySummaryEntry.EM_LinkUniqueID = ensEntry.PK;

			var messageForSimplifiedEntry = Factory.New<EBondEDIMessage>();
			messageForSimplifiedEntry.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			messageForSimplifiedEntry.EM_LinkUniqueID = ensEntry.PK;

			Assert("Should be true as the entry is ENS and has at least one EBond message.", ensEntry.IsEBondMessageAttacher);
			Assert("Should be false as the entry is not Entry Summary.", !seEntry.IsEBondMessageAttacher);
		}

		public void TestPTTClearedAndValid()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZPTTStatus = FTZMessageStatusList.Codes.ClearPermitToTransfer;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var poaDoc = carrier.RequiredDocuments.AddNew();
			poaDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.US_F_OH_PTTCarrier = carrier.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals(true, declaration.IsPTTClearedAndValid);
			AssertEquals("PTT ClearedAndValid of EntryHeader should be the same as Dec.IsPTTClearAndValid", entryHeader.IsPTTClearedAndValid, declaration.IsPTTClearedAndValid);
		}

		public void TestCanSendOriginalForACEEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Assert(ensEntry.CanSendOriginal);
			Assert(ensEntry.CanSendWithdrawal);
		}

		public void TestCanSendOriginalForACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			Assert(!seEntry.CanSendOriginal);
			Assert(seEntry.CanSendWithdrawal);

			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Assert(!seEntry.CanSendOriginal);
			Assert(seEntry.CanSendWithdrawal);

			declaration.US_EnableCRL = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert(!seEntry.CanSendOriginal);
			Assert(seEntry.CanSendWithdrawal);
		}

		public void TestGetLatestENSMessageForDeletion()
		{
			var declaration = GetDeclarationWithENSMessageAccepted();
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "71019689";

			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Delete).PopulateMessage();
			outgoingMessage.EM_MessageNum = "~1500";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			outgoingMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;

			var messageBlock = (AENS10)outgoingMessage.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			var ensReceived = entry.LastENSAcceptedMessageBlock;
			AssertEquals(messageBlock.EntryFilerCode, ensReceived.EntryFilerCode);
			AssertEquals(messageBlock.EntryNumber, ensReceived.EntryNumber);
			AssertEquals(messageBlock.EntryTypeCode, ensReceived.EntryTypeCode);
		}

		public void TestGetLatestCargoReleaseMessageForDeletion()
		{
			var declaration = GetDeclarationWithSEMessageAccepted();
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, declaration.US_EntryType);

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_ReasonCode = "01";
			sendingAction.US_SE_MultipleDispositionsIndic = true;
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			var builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(sendingAction));
			var outgoingMessage = builder.PopulateMessage();
			var messageBlock = (ASESE10)outgoingMessage.MessageBlock.MessageBlocks.Find(x => x is ASESE10);
			AssertNotEquals(declaration.US_EntryType, messageBlock.EntryType);
			AssertEquals(EntryTypeList.Codes.ConsumptionFreeDutiable, messageBlock.EntryType);

			var ensReceived = declaration.ActiveEntryHeaders.SimplifiedEntry.LastCRAcceptedMessageBlock;
			AssertNotNull(ensReceived);
		}

		public void TestLastCRAcceptedMessageBlock()
		{
			var declaration = GetDeclarationWithSEMessageAccepted();
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_ReasonCode = "01";
			sendingAction.US_SE_MultipleDispositionsIndic = true;
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			var builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(sendingAction));
			var outgoingMessage = builder.PopulateMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_196572";

			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196572";
			rcvMessage.EM_MessageText =
@"B001101SV9SX                                               HYEDUSCMT_196571     " +
"SE10ASV9  71032807 11EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST0802186                                        00000010     N       " +
"SE20CR B00173079                                                                " +
"SE9001   SE DATA REJECTED                                                       " +
"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			seEntry.Messages.Add(rcvMessage);

			AssertEquals("01", seEntry.LastCRAcceptedMessageBlock.EntryType);
		}

		[TestDate(2021, 6, 1)]
		public void TestUS_PSDAcceptedCopyFromLatestENSMessage()
		{
			var request = new AutoSendStatementDateChangeRequest();
			request.OverrideAllOrByOrganisation = "ALL";

			var statementData = new DefaultStatementPrintDate();
			statementData.DoDefaultPrelimStatementPrintDate = true;
			statementData.NumberOfDays = 9;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			USCustomsDataRegistry.Instance.DefaultPrelimStatementPrintDate.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, statementData);
			USCustomsDataRegistry.Instance.AutoSendSDCR.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, request);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_DeclarationReference = "B00181007";

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			declaration.BrokerToPayIndicator = YesNoDefaultList.Codes.No;
			declaration.US_PeriodicStatementMM = "06";
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2021, 06, 10);

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			ensEntry.EntryNumber = "73009357";
			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(ensEntry, true, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_MessageNum = "HYEDUSCMT_210391";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);

			var blockAENS10 = (AENS10)outgoingMessage.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			AssertEquals("Should be from declaration.US_PreliminaryStatementPrintDate", new ZDateTime(2021, 06, 10), blockAENS10.PreliminaryStatementPrintDate);
			AssertEquals("Should be empty", ZDateTime.Empty, declaration.US_PSDAccepted);

			var soRcvMessage = CreateStatusMessage("B001101SV9SO                                00                                  " +
					"SO101901SV9  73009357 0123-456789012AL                      2246 021413         " +
					"SO20CR B00181007                                                                " +
					"SO40R    ALP31222406                                       00000600             " +
					"SO50022713162694BILL DEPARTED                                                   " +
					"SO60061421233198RELEASED                                06022101                " +
					"Y  1101SV9SO00000");

			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var block60 = (ASESSO60)soRcvMessage.MessageBlock.MessageBlocks.Find(x => x is ASESSO60);
			AssertNotNull(block60);
			AssertEquals("block60.ReleaseDate", new ZDateTime(2021, 06, 02), block60.ReleaseDate);

			var reLoadDec = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Based on ReleaseDate (+9 working days)", new ZDateTime(2021, 06, 15), reLoadDec.US_PreliminaryStatementPrintDate);

			var messageText =
"B001101SV9AX                                               HYEDUSCMT_210391     " +
"E0 SUMMRY 000001 REF ID: SV9 73009357 B00181007                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  73009357     B00181007   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  73009357     B00181007   " +
"Y  1101SV9AX00005";

			var axRcvMessage = CreateResponseMessage(ensEntry, "HYEDUSCMT_210391", messageText, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ZDateTime.Now.AddHours(-2));
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			reLoadDec = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Should copy from the last entry summary message we sent to customs", new ZDateTime(2021, 06, 10), reLoadDec.US_PSDAccepted);

			var statementUpdateMessage = (MQEDIMessage)reLoadDec.Messages.Find(new ZQuery(EDIMessageSchema.EM_MessageSubType, EM_MessageSubTypeList.Codes.StatementUpdateMessage))[0];
			AssertNotNull(statementUpdateMessage);
			var astuh = statementUpdateMessage.MessageBlock.MessageBlocks.OfType<ASTUH>().FirstOrDefault();
			AssertEquals("PSD sent in SU", new ZDate(2021, 06, 15), astuh.PreliminaryStatementPrintDate);
		}

		public void TestLastENSAcceptedMessageBlock()
		{
			var declaration = GetDeclarationWithENSMessageAccepted();

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.EntryNumber = "71019689";

			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_MessageNum = "~1600";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			outgoingMessage.EM_ReceiveTransmit = CBPEDIMessage.Direction.Transmit;

			var messageBlock = (AENS10)outgoingMessage.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			AssertEquals("21", messageBlock.EntryTypeCode);

			var incomingMessage = CreateResponseMessage(entry, "~1600", MQEDIMessageTest.RejectedER, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ZDateTime.Now);
			entry.Messages.Add(incomingMessage);

			var ensReceived = entry.LastENSAcceptedMessageBlock;
			AssertEquals("01", ensReceived.EntryTypeCode);
		}

		public void TestDoNotDeactivateExportEntriesEvenIfRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.US_HazardousCargo = "Y";
			invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.US_HazardousCargo = "N";
			invoice2.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, declaration.ActiveEntryHeaders.Count);
			Factory.Save();

			var entry1 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.InvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(invoice => invoice == invoice1) != null);
			var entry2 = declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(entry => entry.InvoiceHeaders.Cast<JobComInvoiceHeader>().FirstOrDefault(invoice => invoice == invoice2) != null);

			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.Error;
			invoice1.Delete();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("should not delete an entry that has been sent to Customs and rejected", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Only one active entry", 1, declaration.ActiveEntryHeaders.Count);
		}

		public void TestDutyAmountValidation()
		{
			var declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var invoice = declaration.Invoices.AddNew();
			declaration.US_EntryFilerCode = "XJ5";

			var originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;

			var charge1 = originalEntry.OriginalCharges.AddNew();
			charge1.CY_Code = Core.Constants.USCustoms.FeeCodes.Duty;
			var charge2 = originalEntry.OriginalCharges.AddNew();
			charge2.CY_Code = Core.Constants.USCustoms.FeeCodes.ReconciliationInterest;

			Factory.Save();

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 3000m;

			invoiceLine2.US_R_OrigCV = invoiceLine2.JI_LinePrice;
			invoiceLine2.US_R_OrigTariff = invoiceLine2.JI_Tariff;
			invoiceLine2.US_R_OrigFirstQty = invoiceLine2.JI_CustomsQuantity;

			invoiceLine.US_R_OrigDuty = 12m;
			invoiceLine2.US_R_OrigDuty = 34m;

			declaration.CalculateDutyFeesForChangedEntries();

			AssertEquals("Charges Count should be 2", 2, originalEntry.OriginalCharges.Count);
			var cYAmount = originalEntry.GetTotalOriginalCustomsFeesFromFeeCode(Core.Constants.USCustoms.FeeCodes.Duty);
			AssertEquals("Duty Amount", 46m, cYAmount);
		}

		public void TestIControllerIDProviderMembers()
		{
			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			IControllerIDProvider provider = entry;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclarationPluggedIntoShipment, provider.ControllerID);
			AssertEquals("BusinessObjectPK", dec.PK.ToGuid(), provider.BusinessObjectPK);
		}

		public void TestPGADataCorrections()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EntryFilerCode = "XCd";
			dec.US_EnableENS = true;

			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.APHISHeaders.AddNew();

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.APHISHeaders.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = dec.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals(8, entryHeader.PGADataCorrections.Count);
			AssertEquals(4, invoiceLine.CusEntryLine.PGADataCorrections.Count());
			AssertEquals(4, secondaryLine.CusEntryLine.PGADataCorrections.Count());
		}

		public void TestFallbackToDeclarationBrokerWhenCurrentUserIsBatchProcessor()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "ABC";
			broker.GS_FullName = "Abigail C";
			broker.GS_LoginName = "ABC";

			var broker2 = Factory.New<GlbStaff>();
			broker2.GS_Code = "DE";
			broker2.GS_FullName = "Darren E";
			broker2.GS_LoginName = "DE";

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			dec.JE_GS_NKCusAgent = "ABC";

			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("fall back to declaration broker", "Abigail C", entry.DeclarantName);
			}

			using (Env.SetTemporaryUserContext("DE", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Should take the current user as the registry is turned off", "Darren E", entry.DeclarantName);
			}

			dec.JE_GS_NKCusAgent = "";//null
			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Should print empty rather than ~BP", "", entry.DeclarantName);
			}
		}

		public void TestHasMultiExportDates()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.US_EntryFilerCode = "XXX";
			dec.US_EnableENS = true;
			dec.Invoices.AddNew();
			var invoiceLine = dec.InvoiceLines.AddNew();
			invoiceLine.US_DateOfExport = ZDateTime.BrettsBirthday;
			var invoiceLine2 = dec.InvoiceLines.AddNew();
			invoiceLine2.US_DateOfExport = ZDateTime.BrettsBirthday;
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = dec.CustomsEntryHeaders[0];
			Assert(!entry.HasMultiExportDates);

			invoiceLine2.US_DateOfExport = ZDateTime.BrettsBirthday.AddDays(1);
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert(entry.HasMultiExportDates);
		}

		public void TestICusAddInfoTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Integration.Customs.ICusAddInfoTypeSupporter supporter = entry;
			supporter.AssertType(typeof(US7501DocPrinting), CusAddInfoTypeAttribute.Codes.US7501DocPrinting);
			supporter.AssertType(typeof(PSCExplanationCusAddInfo), CusCodeDataTypeList.Codes.PSCReasonCodes);
			supporter.AssertType(null, "ZZ!");

			var printingData = entry.US7501DocPrintingData.AddNew();
			printingData.US_AVCases = "1";
			var pscExplanation = Factory.New<PSCExplanationCusAddInfo>();
			pscExplanation.B7_ParentID = entry.PK;
			pscExplanation.B7_ParentTableCode = entry.TablePrefix;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			CusAddInfo addInfo = newFactory.Load<CusAddInfo>(printingData.PK);
			AssertEquals(typeof(US7501DocPrinting), addInfo.GetType());
			addInfo = newFactory.Load<CusAddInfo>(pscExplanation.PK);
			AssertEquals(typeof(PSCExplanationCusAddInfo), addInfo.GetType());
		}

		public void TesICusCodeDataTypeSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			Integration.Customs.ICusCodeDataTypeSupporter supporter = entry;
			supporter.AssertType(typeof(PSCReasonCusCodeData), CusCodeDataTypeList.Codes.PSCReasonCodes);
			supporter.AssertType(typeof(ReconEntryOriginalCharge), CusCodeDataTypeList.Codes.ReconEntryOriginalCharge);
			supporter.AssertType(typeof(ReconRefundedCharge), CusCodeDataTypeList.Codes.ReconRefundedCharge);
			supporter.AssertType(null, "ZZ!");

			var pscReasonCodes = Factory.New<PSCReasonCusCodeData>();
			pscReasonCodes.CY_Code = "BD";
			pscReasonCodes.CY_ParentID = entry.PK;
			pscReasonCodes.CY_ParentTableCode = entry.TablePrefix;

			var charge = Factory.New<ReconEntryOriginalCharge>();
			charge.CY_Code = "DD";
			charge.CY_ParentID = entry.PK;
			charge.CY_ParentTableCode = entry.TablePrefix;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var codeData = newFactory.Load<CusCodeData>(pscReasonCodes.PK);
			AssertEquals(typeof(PSCReasonCusCodeData), codeData.GetType());
			codeData = newFactory.Load<CusCodeData>(charge.PK);
			AssertEquals(typeof(ReconEntryOriginalCharge), codeData.GetType());
		}

		public void TestDeclarantName()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;

			var dec = Factory.New<JobDeclaration>();
			var entry = dec.CustomsEntryHeaders.AddNew();
			AssertEquals("DeclarantName should print current logged in user", Env.CurrentUser.FullName, entry.DeclarantName);

			using (Env.SetTemporaryUserContext(User.ServiceUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Test Precondition: IsBatchProcessor", true, Env.CurrentUser.IsBatchProcessor);
				AssertEquals("DeclarantName should print blank when current logged in user is the Batch Processor", ZString.Empty, entry.DeclarantName);
			}

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "INC";
			broker.GS_FullName = "IAN CHEN";
			broker.GS_LoginName = "INC";
			dec.JE_GS_NKCusAgent = "INC";

			var broker2 = Factory.New<GlbStaff>();
			broker2.GS_Code = "INT";
			broker2.GS_FullName = "IAN TEST";
			broker2.GS_LoginName = "INT";

			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, broker2.PK.ToGuid());
			USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("DeclarantName should print from registry", broker2.GS_FullName, entry.DeclarantName);

			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals("DeclarantName should print from broker in declaration", broker.GS_FullName, entry.DeclarantName);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AssertEquals("DeclarantName should print from current user", GlbStaff.CurrentUser.GS_FullName, entry.DeclarantName);
		}

		public void TestLogEntryHeaderDeletionWhenIsShouldNotBeDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			var primeEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryHasBeenLodgedAtCustomsMock = Factory.NewMoq<CusEntryHeader>();
			entryHasBeenLodgedAtCustomsMock.Setup(m => m.HasBeenLodgedAtCustoms).Returns(true);
			var entryHasBeenLodgedAtCustoms = entryHasBeenLodgedAtCustomsMock.Object;
			entryHasBeenLodgedAtCustoms.CH_JE = declaration.PK;
			PopulateEntry(entryHasBeenLodgedAtCustoms, "ProtestID=34", "BGM1", primeEntry.PK, "ES1", ZDateTime.BrettsBirthday.AddMonths(-1), (short)2, "MT1", "ST1", 10m);

			var entryIsWaitingForResponseMock = Factory.NewMoq<CusEntryHeader>();
			entryIsWaitingForResponseMock.Setup(m => m.IsWaitingForResponse).Returns(true);
			var entryIsWaitingForResponse = entryIsWaitingForResponseMock.Object;
			entryIsWaitingForResponse.CH_JE = declaration.PK;
			PopulateEntry(entryIsWaitingForResponse, "CRLCertStatus=D", "BGM2", primeEntry.PK, "ES2", ZDateTime.BrettsBirthday.AddMonths(-2), (short)2, "MT2", "ST2", 12m);

			var entryHasBeenWithdrawnAndIsWaitingForResponseMock = Factory.NewMoq<CusEntryHeader>();
			entryHasBeenWithdrawnAndIsWaitingForResponseMock.Setup(m => m.IsWaitingForResponse).Returns(true);
			entryHasBeenWithdrawnAndIsWaitingForResponseMock.Setup(m => m.HasBeenWithdrawn).Returns(true);
			var entryHasBeenWithdrawnAndIsWaitingForResponse = entryHasBeenWithdrawnAndIsWaitingForResponseMock.Object;
			entryHasBeenWithdrawnAndIsWaitingForResponse.CH_JE = declaration.PK;
			PopulateEntry(entryHasBeenWithdrawnAndIsWaitingForResponse, "CWOStatus=DE", "BGM3A", primeEntry.PK, "ES3", ZDateTime.BrettsBirthday.AddMonths(-3), (short)2, "MT3", "ST3", 13m);

			var entryHasBeenWithdrawnAndLodgedMock = Factory.NewMoq<CusEntryHeader>();
			entryHasBeenWithdrawnAndLodgedMock.Setup(m => m.IsWaitingForResponse).Returns(true);
			entryHasBeenWithdrawnAndLodgedMock.Setup(m => m.HasBeenWithdrawn).Returns(true);
			var entryHasBeenWithdrawnAndLodged = entryHasBeenWithdrawnAndLodgedMock.Object;
			entryHasBeenWithdrawnAndLodged.CH_JE = declaration.PK;
			PopulateEntry(entryHasBeenWithdrawnAndLodged, "DestinationState=CA", "BGM3B", primeEntry.PK, "ES3", ZDateTime.BrettsBirthday.AddMonths(-3), (short)2, "MT3", "ST3", 13m);

			var entryMock = Factory.NewMoq<CusEntryHeader>();
			var entry = entryMock.Object;
			entry.CH_JE = declaration.PK;
			PopulateEntry(entry, "SchDEntry=3432", "BGM4", primeEntry.PK, "ES4", ZDateTime.BrettsBirthday.AddMonths(-4), (short)2, "MT4", "ST4", 14m);

			Factory.Save();
			var deletedEntryHeaderNoteQuery = new ZQuery(StmNoteSchema.ST_ParentID, declaration.PK);
			deletedEntryHeaderNoteQuery.AddToFilter(StmNoteSchema.ST_Description, "Deleted Entry Header");
			deletedEntryHeaderNoteQuery.AddToFilter(StmNoteSchema.ST_Table, declaration.TableName);
			var notes = Factory.Load<HiddenStmNote>(deletedEntryHeaderNoteQuery);
			AssertEquals(0, notes.Length);

			var entryHasBeenLodgedAtCustomsLine1 = entryHasBeenLodgedAtCustoms.MergedLines[0];
			var entryHasBeenLodgedAtCustomsLine2 = entryHasBeenLodgedAtCustoms.MergedLines[1];
			entryHasBeenLodgedAtCustoms.Delete();
			var entryIsWaitingForResponseLine1 = entryIsWaitingForResponse.MergedLines[0];
			var entryIsWaitingForResponseLine2 = entryIsWaitingForResponse.MergedLines[1];
			entryIsWaitingForResponse.Delete();
			entryHasBeenWithdrawnAndIsWaitingForResponse.Delete();
			entryHasBeenWithdrawnAndLodged.Delete();
			entry.Delete();
			Factory.Save();

			notes = Factory.Load<HiddenStmNote>(deletedEntryHeaderNoteQuery);
			AssertEquals(2, notes.Length);
			var note1 = notes[0];
			var note2 = notes[1];
			if (note1.ST_NoteDataAsText.Contains(entryIsWaitingForResponse.PK.ToString()))
			{
				note1 = notes[1];
				note2 = notes[0];
			}
			AssertContains("CallStack:\r\n   at Enterprise.Customs.US.Business.CusEntryHeader.LogEntryHeaderDeletionWhenIsShouldNotBeDeleted(JobDeclaration declaration, String key, String messagePrefix)", note1.ST_NoteDataAsText);
			AssertContains("CH_PK=" + entryHasBeenLodgedAtCustoms.PK.ToString(), note1.ST_NoteDataAsText);
			AssertContains("CH_AddInfo=ProtestID=34", note1.ST_NoteDataAsText);
			AssertContains("CH_BGMReference=BGM1", note1.ST_NoteDataAsText);
			AssertContains("CH_CH_PrimeEntry=" + primeEntry.PK.ToString(), note1.ST_NoteDataAsText);
			AssertContains("CL_PK=" + entryHasBeenLodgedAtCustomsLine1.PK.ToString(), note1.ST_NoteDataAsText);
			AssertContains("CL_CH=" + entryHasBeenLodgedAtCustoms.PK.ToString(), note1.ST_NoteDataAsText);
			AssertContains("CL_Description=LINE1", note1.ST_NoteDataAsText);
			AssertContains("CL_LineNumber=1", note1.ST_NoteDataAsText);
			AssertContains("CL_PK=" + entryHasBeenLodgedAtCustomsLine2.PK.ToString(), note1.ST_NoteDataAsText);
			AssertContains("CL_Description=LINE2", note1.ST_NoteDataAsText);
			AssertContains("CL_LineNumber=2", note1.ST_NoteDataAsText);

			AssertContains("CallStack:\r\n   at Enterprise.Customs.US.Business.CusEntryHeader.LogEntryHeaderDeletionWhenIsShouldNotBeDeleted(JobDeclaration declaration, String key, String messagePrefix)", note2.ST_NoteDataAsText);
			AssertContains("CH_PK=" + entryIsWaitingForResponse.PK.ToString(), note2.ST_NoteDataAsText);
			AssertContains("CH_AddInfo=CRLCertStatus=D", note2.ST_NoteDataAsText);
			AssertContains("CH_BGMReference=BGM2", note2.ST_NoteDataAsText);
			AssertContains("CH_CH_PrimeEntry=" + primeEntry.PK.ToString(), note2.ST_NoteDataAsText);
			AssertContains("CL_PK=" + entryIsWaitingForResponseLine1.PK.ToString(), note2.ST_NoteDataAsText);
			AssertContains("CL_CH=" + entryIsWaitingForResponse.PK.ToString(), note2.ST_NoteDataAsText);
			AssertContains("CL_Description=LINE1", note2.ST_NoteDataAsText);
			AssertContains("CL_LineNumber=1", note2.ST_NoteDataAsText);
			AssertContains("CL_PK=" + entryIsWaitingForResponseLine2.PK.ToString(), note2.ST_NoteDataAsText);
			AssertContains("CL_Description=LINE2", note2.ST_NoteDataAsText);
			AssertContains("CL_LineNumber=2", note2.ST_NoteDataAsText);

			if (ErrorReporter.HasBeenReported("Deleting US Customs Pending CusEntryHeader"))
			{
				ErrorReporter.Clear();
			}
		}

		public void TestTotalNonSecondaryInvoiceLinesCount()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine.AddSecondaryInvoiceLine();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("NonSecondaryInvoiceLinesCount", 1, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalNonSecondaryInvoiceLinesCount);
		}

		public void TestHMFDeminimisRuleForACSAndACE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.US_SPI = "AU";//MPF exempt

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("HMF DeMinimis Rule in ACE", 0m, entry.HMFAmountForEntry);
			AssertEquals("HMF amount should be cleared", 0m, invoiceLine.CusEntryLine.HMFAmount);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("HMF DeMinimis Rule applies", 0m, entry.HMFAmountForEntry);
			AssertNotEquals("HMF amount should not be cleared", 0m, invoiceLine.CusEntryLine.HMFAmount);
		}

		public void TestHMFDeminimisRuleWhenADDExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 855m;
			invoiceLine.US_SPI = "KR";//MPF exempt
			invoiceLine.JI_Tariff = "7210706060";
			invoiceLine.US_ADDCaseNo = "A580816002";
			invoiceLine.US_CVDCaseNo = "C580208000";

			if (invoiceLine.CountervailingDutyCase == null)
			{
				var cCase = Factory.New<USCACCase>();
				cCase.U5_CaseNumber = "C580208000";
				cCase.U5_ISOCountryCode = "KR";
				cCase.U5_RelatedCaseNumber = "A580816000";
				cCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
				cCase.U5_CaseStatusDate = ZDateTime.BrettsBirthday;

				USCACCaseBondCash bondCash = cCase.BondCashIndicators.AddNew();
				bondCash.U8_Indicator = BondCashIndicatorList.Codes.Cash;
				bondCash.U8_InactivatedDate = ZDateTime.Empty;
				bondCash.U8_EffectiveDate = invoiceLine.DateForADD_CVD;

				var rate = cCase.CaseRates.AddNew();
				rate.U6_AdValoremRate = 0.0115m;
				rate.U6_EffectiveDate = ZDateTime.Today;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotEquals("HMF DeMinimis Rule Should not apply", 0m, entry.HMFAmountForEntry);
		}

		public void TestSetAllocateEntryNumberOnSaving()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var inBondEntry = declaration.ActiveEntryHeaders.AddNew();
			inBondEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			Factory.Save();
			inBondEntry.EntryNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(ZString.Empty, inBondEntry.EntryNumber);
			Factory.Save();
			AssertEquals(ZString.Empty, inBondEntry.EntryNumber);
			inBondEntry.SetAllocateInbondNumberOnSaving();
			Factory.Save();
			AssertNotEquals(ZString.Empty, inBondEntry.EntryNumber);
			var number = inBondEntry.EntryNumber;
			Factory.Save();
			AssertEquals(number, inBondEntry.EntryNumber);
			inBondEntry.SetAllocateInbondNumberOnSaving();
			Factory.Save();
			AssertEquals(number, inBondEntry.EntryNumber);
		}

		[TestDate(2009, 6, 1)]
		public void TestRecordPayableAmountFromMessageOnSuccessfulResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Amount Payable", 1474.73m, entry.TotalAmountPayable);

			var outgoingMessage = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			outgoingMessage.EM_MessageNum = "~1500";
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 1, 1, 1, 1);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals("Still waiting", 0, entry.EntryPayInfos.Count);

			var incomingMessage = CreateResponseMessage(entry, "~1500", RejectedER, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, new ZDateTime(2010, 1, 1, 1, 1, 1));
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			AssertEquals("Rejected", 0, entry.EntryPayInfos.Count);

			var outgoingMessage2 = new EntrySummaryMessageBuilder(entry, UpdateActionCode.Add, false).PopulateMessage();
			outgoingMessage2.EM_MessageNum = "~1501";
			outgoingMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 1, 1, 2, 1);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals("Still waiting", 0, entry.EntryPayInfos.Count);

			invoiceLine.JI_LinePrice = 20000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("Total Amount Payable is now different", 1474.73m, entry.TotalAmountPayable);

			var incomingMessage2 = CreateResponseMessage(entry, "~1501", AcceptedER, ApplicationIdentifierCodeList.Codes.EntrySummaryResponse, new ZDateTime(2010, 1, 1, 1, 2, 1));
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("Accepted now", 1, entry.EntryPayInfos.Count);
			AssertEquals("Amount should be populated from message", 1474.73m, entry.EntryPayInfos[0].C9_PaymentAmount);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			AssertEquals("Still now withdrawn", 1, entry.EntryPayInfos.Count);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			AssertEquals("withdrawn, therefore the customs payment amount should be deleted", 0, entry.EntryPayInfos.Count);
		}

		public void TestEmailRecipientsWithSOSE()
		{
			GlbStaff currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "testcuruser@abc.com";

			var emailAddress = "abc@abc.au";
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "TSB"));
			if (staff == null)
			{
				staff = Factory.New<GlbStaff>();
				staff.GS_FullName = "Test Broker";
				staff.GS_Code = "TSB";
				staff.GS_EmailAddress = emailAddress;
			}

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			dec.US_EnableENS = true;
			dec.US_EnableCRL = true;
			dec.US_EntryFilerCode = "SV9";
			dec.US_FDAContactEmail = emailAddress;
			dec.JE_GS_NKCusAgent = staff.GS_Code;
			dec.JE_SystemCreateUser = currentStaff.GS_Code;
			dec.ImportEntryNumber = "71020679";

			var entrySE = dec.CustomsEntryHeaders.AddNew();
			entrySE.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entrySE.EntryNumber = "71020679";
			AssertNotNull(dec.ActiveEntryHeaders.SimplifiedEntry);

			var entryENS = dec.CustomsEntryHeaders.AddNew();
			entryENS.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryENS.EntryNumber = "71020679";
			AssertNotNull(dec.ActiveEntryHeaders.EntrySummaryEntry);

			var aeMessage = CreateMessage(EDIMessage.Direction.Transmit, ACEApplicationIdentifierCodeList.Codes.EntrySummary); // AE
			aeMessage.EM_MessageNum = "~170855";
			entryENS.Messages.Add(aeMessage);
			Factory.Save();

			var soMessage = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus); // SO
			soMessage.EM_MessageText = SOMessage;
			soMessage.EM_MessageNum = "~170855";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertEquals("testcuruser@abc.com", email.Recipients[0].Email);
		}

		[TestDate(2009, 6, 1)]
		public void TestRecordPayableAmountFromACEMessageOnSuccessfulResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Amount Payable", 1474.73m, entry.TotalAmountPayable);

			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_MessageNum = "~1500";
			outgoingMessage.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 1, 1, 1, 1);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals("Still waiting", 0, entry.EntryPayInfos.Count);

			var messageText =
"B003902SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022098 B00153984                                 " +
"E1 F198   INITIAL PAY TYP CANNOT BE INDIVD PAYMENTSV9  70022098     B00153984   " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E1 F577   SOLD TO PARTY MISSING-REQ'D FOR TYPE    SV9  70022098     B00153984   " +
"E0 TARIFF 000001 REF ID: 8471704065                                             " +
"E1 F492   FCC 740 MAY BE REQUIRED                 SV9  70022098     B00153984   " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022098     B00153984   " +
"E1RF998   TRANSACTION DATA REJECTED               SV9  70022098     B00153984   " +
"Y  3902SV9AX00008";

			var incomingMessage = CreateResponseMessage(entry, "~1500", messageText, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, new ZDateTime(2010, 1, 1, 1, 1, 1));
			entry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			AssertEquals("Rejected", 0, entry.EntryPayInfos.Count);

			var outgoingMessage2 = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage2.EM_MessageNum = "~1501";
			outgoingMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2010, 1, 1, 1, 2, 1);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals("Still waiting", 0, entry.EntryPayInfos.Count);

			invoiceLine.JI_LinePrice = 20000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals("Total Amount Payable is now different", 1474.73m, entry.TotalAmountPayable);

			messageText =
"B001101SV9AX                                               ~15000               " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005";

			var incomingMessage2 = CreateResponseMessage(entry, "~1501", messageText, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, new ZDateTime(2010, 1, 1, 1, 2, 1));
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("Accepted now", 1, entry.EntryPayInfos.Count);
			AssertEquals("Amount should be populated from message", 1474.73m, entry.EntryPayInfos[0].C9_PaymentAmount);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			AssertEquals("Still now withdrawn", 1, entry.EntryPayInfos.Count);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			AssertEquals("withdrawn, therefore the customs payment amount should be deleted", 0, entry.EntryPayInfos.Count);
		}

		public void TestHasMixedRelationshipIndicators()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2922292700";
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5911900040";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("HasMixedRelationshipIndicators", false, entry.HasMixedRelationshipIndicators);

			JobComInvoiceLine invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2106903800";
			invoiceLine3.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			Factory.Save();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("HasMixedRelationshipIndicators", true, entry.HasMixedRelationshipIndicators);
		}

		public void TestMixedRelationshipCS00249648()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 100;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 200;
			invoiceHeader2.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 300;
			invoiceHeader3.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2922292700";
			invoiceLine1.JI_JZ = invoiceHeader1.PK;
			invoiceLine1.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5911900040";
			invoiceLine2.JI_JZ = invoiceHeader2.PK;
			invoiceLine2.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2208.60.2000";
			invoiceLine3.JI_JZ = invoiceHeader3.PK;
			invoiceLine3.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("HasMixedRelationshipIndicator needs to recognise when there is no difference at line level when invoice headers are mixed - eg 1 invoice header is blank", false, entry.HasMixedRelationshipIndicators);
		}

		public void TestMixedRelationshipLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_InvoiceAmount = 100;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 200;
			invoiceHeader2.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			var invoiceHeader3 = declaration.Invoices.AddNew();
			invoiceHeader3.JZ_InvoiceAmount = 300;

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "2922292700";
			invoiceLine1.JI_JZ = invoiceHeader1.PK;
			invoiceLine1.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "5911900040";
			invoiceLine2.JI_JZ = invoiceHeader2.PK;
			invoiceLine2.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2208.60.2000";
			invoiceLine3.JI_JZ = invoiceHeader3.PK;
			invoiceLine3.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			var invoiceLine4 = declaration.InvoiceLines.AddNew();
			invoiceLine4.JI_Tariff = "2922292700";
			invoiceLine4.JI_JZ = invoiceHeader3.PK;
			invoiceLine4.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			var invoiceLine5 = declaration.InvoiceLines.AddNew();
			invoiceLine5.JI_Tariff = "5911900040";
			invoiceLine5.JI_JZ = invoiceHeader3.PK;
			invoiceLine5.US_TransactionsRelated = YesNoDefaultList.Codes.No;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("HasMixedRelationshipIndicator needs to recognise the difference at line level", true, entry.HasMixedRelationshipIndicators);
		}

		public void TestDiscardMessagesWhenDeleted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertNotNull(crlEntry);

			var message = new CargoReleaseMessageBuilder(crlEntry, UpdateActionCode.Delete, false).PopulateMessage();
			AssertEquals(crlEntry, message.EM_LinkedObject);
			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Assert(crlEntry.HasBeenWithdrawn);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Assert(declaration.IsBorderMovement);

			var bcrEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals(crlEntry, bcrEntry);

			Factory.Save();

			Assert(crlEntry.IsActive);
			Assert(!crlEntry.IsDeleted);
		}

		public void TestQueueMessagesAndAttachToDeclarationWhenRejected()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertNotNull(ensEntry);

			var message = new EntrySummaryMessageBuilder(ensEntry, UpdateActionCode.Add, false).PopulateMessage();
			AssertEquals(ensEntry, message.EM_LinkedObject);
			ensEntry.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryDelete;
			Assert(!ensEntry.IsOKToBeDeactivated);

			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			AssertExceptionThrown(typeof(ApplicationException), () =>
			{
				try
				{
					declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				}
				catch
				{
					declaration.UnlockDoMergeMutex();
					throw;
				}
			});

			AssertEquals(ensEntry, message.EM_LinkedObject);
			Assert(ensEntry.Messages.Contains(message));
			Assert(!ensEntry.IsDeleted);
			AssertEquals("messages are marked as discarded", EDIMessage.Status.Queued, message.EM_Status);
			if (ErrorReporter.LastMessageReported == "Deleting CusEntryHeader with messages")
			{
				ErrorReporter.Clear();
			}
		}

		public void TestICusEntryHeaderContainersAndUltimateConsignee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX879489";

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX879488";

			OrgHeader ultimateConsignee = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("Two IContainers", 2, new List<IContainer>(((ICusEntryHeader)entry).Containers).Count);
			AssertEquals(ultimateConsignee, ((ICusEntryHeader)entry).UltimateConsignee);
		}

		public void TestIRegistryAccessingSupporterMembers()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_GB = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entry.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entry.RegistryBranchPK);

			declaration.JE_GB = branch.PK;
			AssertEquals(company.PK.ToGuid(), entry.RegistryCompanyPK);
			AssertEquals(branch.PK.ToGuid(), entry.RegistryBranchPK);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entry.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entry.RegistryBranchPK);

			entry.CH_JE = ZGuid.Empty;
			AssertEquals(GlbCompany.CurrentCompany.PK.ToGuid(), entry.RegistryCompanyPK);
			AssertEquals(GlbBranch.CurrentBranch.PK.ToGuid(), entry.RegistryBranchPK);
		}

		public void TestIMessageResponseNotificatorMembers()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "Z8";
			staff.GS_EmailAddress = "dummy@email.com";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var header = declaration.CustomsEntryHeaders.AddNew();
			IMessageResponseNotificator notificator = header;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
			declaration.JE_GS_NKCusAgent = "Z8";
			AssertEquals("dummy@email.com", notificator.GetFallbackEmailAddressRecipient());
			var declaration2 = Factory.New<JobDeclaration>();
			header.CH_JE = declaration2.PK;
			AssertEquals(ZString.Empty, notificator.GetFallbackEmailAddressRecipient());
		}

		public void TestDoNotGenerateEntryNumberForExternalBrokerJobs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			AssertEquals("PreCondition", true, declaration.IsImportByExternalBroker);

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			AssertEquals("PreCondition", true, declaration.IsInDatabase);
			AssertEquals("No entry number has been generated", ZString.Empty, declaration.ActiveEntryHeaders.EntrySummaryEntry.EntryNumber);
		}

		[TestDate(2009, 6, 1)]
		public void TestGetGrandTotalFeeWhenRoundingIsAnIssue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 14831.74m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 999.42m;
			invoiceLine.JI_Tariff = "1";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 2587.12m;
			invoiceLine2.JI_Tariff = "2";

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 4897.25m;
			invoiceLine3.JI_Tariff = "2";

			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_LinePrice = 1515.11m;
			invoiceLine4.JI_Tariff = "3";

			JobComInvoiceLine invoiceLine5 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_LinePrice = 4832.24m;
			invoiceLine5.JI_Tariff = "3";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("MPF", 2.10m, invoiceLine.CusEntryLine.MPFAmount);
			AssertEquals("MPF", 15.72m, invoiceLine2.CusEntryLine.MPFAmount);
			AssertEquals("MPF", 13.33m, invoiceLine4.CusEntryLine.MPFAmount);

			//it used to be calculated as 31.14m which is the sum of figures before rounding.
			AssertEquals("Total MPF is calculated after each MPF is rounded", 31.15m, declaration.CustomsEntryHeaders[0].MPFAmountForEntry);

			AssertEquals("GrandTotalFee declared to Customs should be a sum of rounded figure when calculated during merge", 31.15m, ((ICusEntryHeader)declaration.CustomsEntryHeaders[0]).GrandTotalFee);
		}

		public void TestGetTotalChargeValueFor()
		{
			ReconDeclaration declaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			declaration.US_EntryFilerCode = "XJ5";

			ReconOriginalEntryHeader originalEntry = declaration.OriginalEntries.AddNew();
			originalEntry.US_R_DutyRateDate = ZDateTime.Today;
			originalEntry.US_R_CalcOrigDuty = true;

			JobComInvoiceHeader invoice = originalEntry.Invoice;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;

			invoiceLine.US_R_OrigCV = invoiceLine.JI_LinePrice;
			invoiceLine.US_R_OrigTariff = invoiceLine.JI_Tariff;
			invoiceLine.US_R_OrigFirstQty = invoiceLine.JI_CustomsQuantity;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3201.90.1000";
			invoiceLine2.JI_CustomsQuantity = 50m;
			invoiceLine2.JI_LinePrice = 3000m;

			invoiceLine2.US_R_OrigCV = invoiceLine2.JI_LinePrice;
			invoiceLine2.US_R_OrigTariff = invoiceLine2.JI_Tariff;
			invoiceLine2.US_R_OrigFirstQty = invoiceLine2.JI_CustomsQuantity;

			declaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("Nothing more to pay", 0m, declaration.ReconEntry.CH_TotalPaid);

			invoiceLine.JI_LinePrice = 2000m;//refund due
			invoiceLine2.JI_LinePrice = 2000m;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.CalculateDutyFeesForChangedEntries();
			AssertEquals("refund is due", -30m, declaration.ReconEntry.CH_TotalPaid);

			ICustomsCharges customsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration);

			AssertEquals("No billable charges", 0, customsCharges.GetCustomsCharges(null).Length);
		}

		[TestDate(2009, 1, 1)]
		public void TestGetTotalChargeValueForDeferredTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 6000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204215030";
			invoiceLine.JI_CustomsQuantity = 540m;
			invoiceLine.JI_LinePrice = 1500m;
			invoiceLine.US_SPI = "AU";// no MPF is payable

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty + Tax is payable", 186.66m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);

			ICustomsCharges customsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var charges = customsCharges.GetCustomsCharges(null);
			AssertEquals("Two billable charges", 2, charges.Length);
			AssertEquals(34.02m, charges[0].Amount);
			AssertEquals(true, charges[0].IsPaidByBroker);

			AssertEquals(152.64m, charges[1].Amount);
			AssertEquals(true, charges[0].IsPaidByBroker);

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty is payable", 34.02m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);

			charges = customsCharges.GetCustomsCharges(null);
			AssertEquals("Two billable charges", 2, charges.Length);
			AssertEquals(34.02m, charges[0].Amount);
			AssertEquals(true, charges[0].IsPaidByBroker);
			AssertEquals(152.64m, charges[1].Amount);
			AssertEquals(false, charges[1].IsPaidByBroker);

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.BulkLiquorDeferred;
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203000090";
			invoiceLine2.JI_CustomsQuantity = 5000m;
			invoiceLine2.JI_LinePrice = 15000m;
			invoiceLine2.US_SPI = "GB";
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Duty is payable", 218.16m, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_TotalPaid);

			customsCharges = ServiceLocator.GetService<ICustomsCharges>(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			charges = customsCharges.GetCustomsCharges(null);
			AssertEquals("Three billable charges", 3, charges.Length);
			var charge499 = charges.First(x => x.Description.Contains("499"));
			var chargeExs = charges.First(x => x.Description.Contains("Excise"));
			var chargeDty = charges.First(x => x.Description.Contains("Duty"));
			AssertEquals(34.02m, chargeDty.Amount);
			AssertEquals("Merchandise Processing Fee for 2203000090", 31.50m, charge499.Amount);
			AssertEquals(152.64m, chargeExs.Amount);
		}

		public void TestFTZWarehousePackageQtyTotalWhenMergedByTariff()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "OO11";
			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "O212";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "Test@#";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = importer.PK;
			relation.OU_Relationship = "OWN";
			product.OP_StockKeepingUnit = "UNT";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";

			var classification = Factory.NewWithValidTestData<CusClassification>();
			var aPivot = Factory.New<CusClassPartPivot>();
			aPivot.CI_CC = classification.PK;
			aPivot.CI_OP = product.PK;
			aPivot.CI_PartPivotUOM = "NO";
			classification.CC_TariffNum = "4415103003";
			classification.CC_LookupCode = "GRANNY";
			classification.CC_ClassificationType = "BTH";

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Tes2@#";
			var relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = importer2.PK;
			relation2.OU_Relationship = "OWN";
			product2.OP_StockKeepingUnit = "BOX";
			product2.OP_NetWeight = 0.1m;
			product2.OP_WeightUQ = "T";

			var classification2 = Factory.NewWithValidTestData<CusClassification>();
			var aPivot2 = Factory.New<CusClassPartPivot>();
			aPivot2.CI_CC = classification.PK;
			aPivot2.CI_OP = product.PK;
			aPivot2.CI_PartPivotUOM = "KGM";
			classification2.CC_TariffNum = "2215102002";
			classification2.CC_LookupCode = "NANNY";
			classification2.CC_ClassificationType = "BTH";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 20.51m;
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_BondedWhsQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "PCS";
			invoiceLine.US_ZoneStatus = "N";
			invoiceLine.JI_Weight = 20.5;
			invoiceLine.JI_WeightUQ = "KG";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_BondedWhsQuantity = 7m;
			invoiceLine2.JI_LinePrice = 33m;
			invoiceLine2.JI_OP = product.PK;
			invoiceLine2.JI_InvoiceUQ = "PCS";
			invoiceLine2.US_ZoneStatus = "N";
			invoiceLine2.JI_Weight = 33.0m;
			invoiceLine2.JI_WeightUQ = "KG";

			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 50.61m;
			invoiceLine3.JI_OP = product2.PK;
			invoiceLine3.JI_InvoiceUQ = "NO";
			AssertEquals("UQ loaded from Part", "BOX", invoiceLine3.PartStockTakeUnit);
			invoiceLine3.JI_BondedWhsQuantity = 9m;
			invoiceLine3.JI_BondedWhsUnitQty = "PCS";
			invoiceLine3.JI_Weight = 44.5m;
			invoiceLine3.JI_WeightUQ = "KG";

			AssertEquals("Manually changed", "PCS", invoiceLine3.JI_BondedWhsUnitQty);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Lines merged by Tariff", 2, declaration.ActiveEntryHeaders[0].MergedLines.Count);

			var ftzLine1 = declaration.ActiveEntryHeaders[0].MergedLines[0];
			AssertEquals("FTZ entry line number 1", (ZShort)1, ftzLine1.CL_LineNumber);
			AssertEquals("FTZ entry Line WarehousePkg.Qty", "12 PCS", ftzLine1.FTZWarehousePackageQtyAndUnit);
			AssertEquals("FTZ entry Line GrossWeightAndUnit", "54 KG", ftzLine1.FTZWeightAndUnit);

			var ftzLine2 = declaration.ActiveEntryHeaders[0].MergedLines[1];
			AssertEquals("FTZ entry Line2 WarehousePkg.Qty", "9 NO", ftzLine2.FTZWarehousePackageQtyAndUnit);
			AssertEquals("FTZ entry Line2 GrossWeightAndUnit", "45 KG", ftzLine2.FTZWeightAndUnit);

			AssertEquals("FTZ entry Total should be 12 PCS + 9 NO", (ZString)"12 PCS\r\n9 NO\r\n", declaration.ActiveEntryHeaders[0].FTZWarehousePackageTotalQuantityAndUnit);
			AssertEquals("FTZ GrossWeight Total should be 99 KG.", "99 KG\r\n", declaration.ActiveEntryHeaders[0].FTZBox19TotalGrossWeight);
		}

		public void TestFTZ214DocumentPrintMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "00011", "Test Foreign Loading Port", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "0012", "Test US Port of Arrival", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateRefSysConfigType("CBP214ED", "Expiration Date to print on CBP 214", "Expiration Date to print on CBP 214");
			refHelper.CreateRefSysConfig("CBP214ED", "2/28/2022", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateRefSysConfigType("CBP214FRD", "Revision Date to print on the first page of CBP 214", "Revision Date to print on the first page of CBP 214");
			refHelper.CreateRefSysConfig("CBP214FRD", "3/22", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateRefSysConfigType("CBP214CRD", "Revision Date to print on the continuation page of CBP 214", "Revision Date to print on the continuation page of CBP 214");
			refHelper.CreateRefSysConfig("CBP214CRD", "7/21", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));

			var impT2 = Factory.New<OrgHeader>();
			impT2.OH_Code = "I#2";

			var impT = Factory.New<OrgHeader>();
			impT.OH_Code = "I#T";
			impT.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			impT.MiscServ.OM_IMPartAttrib1Type = "NON";
			impT.CompanyData.OB_IMUsedBondedWhs = true;
			impT.OH_IsWarehouseClient = true;

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "~@T2";
			product.OP_StockKeepingUnit = "CS";
			product.OP_NetWeight = 0.1m;
			product.OP_WeightUQ = "T";
			product.RelatedOrganisations.AddOrganisationIfNotExist(impT.PK, OrgPartRelation.RelationshipTypes.Owner);

			var product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "~@T3";
			product2.OP_StockKeepingUnit = "UNT";
			product2.OP_NetWeight = 0.1m;
			product2.OP_WeightUQ = "T";
			product2.RelatedOrganisations.AddOrganisationIfNotExist(impT2.PK, OrgPartRelation.RelationshipTypes.Owner);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.FTZAdmissionNumber = "2140000|14|00000002";
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_SchDEntry = "3901";
			declaration.JE_ExportDate = new ZDateTime(2014, 2, 28);
			declaration.JE_DateOfArrival = new ZDateTime(2014, 3, 1);
			declaration.US_EntryDate = new ZDateTime(2014, 3, 2);
			declaration.JE_PrimaryITNumber = "IT1";
			declaration.US_ITDate = new ZDateTime(2014, 3, 14);
			declaration.JE_MasterBillIssuerSCAC = "SC1";
			declaration.JE_MasterBill = "MB2";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 3, 15);

			declaration.US_SchDLoading = "00011";
			declaration.US_SchDArrival = "0012";

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code1 = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "E1E2", "FRANK T. ZANDERHOF'S FTZ FACILITY", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, "02");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityAddress, "1819 W MARBLE STREET NE");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City, "SAN ANTONIO");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.State, "TX");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ZIPCode, "983456789");
			Factory.Save();

			declaration.US_US_NKLocationOfGoods = "E1E2";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "9876543210";
			tariff.UE_DateFrom = new ZDateTime(2009, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2099, 12, 31);
			tariff.UE_Unit1 = "KG";
			tariff.UE_Unit2 = "L";

			Factory.Save();

			var header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INVOICE 1";
			header1.JZ_RX_NKInvoice_Currency = "USD";
			header1.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			var line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_OP = product.PK;
			line1.JI_InvoiceQuantity = 1.0m;
			line1.JI_InvoiceUQ = "PK";
			line1.JI_Weight = 1.5m;
			line1.JI_WeightUQ = "KG";
			line1.US_UC_NKCountryOfOrigin = "FR";
			line1.JI_LinePrice = 1.0m;
			line1.JI_LineNo = 1;
			line1.JI_Tariff = "9876543210";
			line1.JI_CustomsQuantity = 1.0m;
			line1.JI_CustomsSecondQuantity = 1.15m;
			line1.JI_Description = "GOODS" + System.Environment.NewLine + "1";
			line1.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			line1.JI_BondedWhsQuantity = 8m;
			var fee1 = line1.FeeCusCodes.AddNew();
			fee1.CY_IsOverridden = true;
			fee1.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee1.CY_FeeAmount = 0.25m;

			var line2 = header1.JobComInvoiceLines.AddNew();
			line2.JI_OP = product.PK;
			line2.JI_LinePrice = 2.0m;
			line2.JI_LineNo = 2;
			line2.JI_InvoiceQuantity = 2.0m;
			line2.JI_InvoiceUQ = "PK";
			line2.JI_Weight = 2.4m;
			line2.JI_WeightUQ = "KG";
			line2.JI_Tariff = "9876543211";
			line2.JI_CustomsQuantity = 2.0m;
			line2.US_UC_NKCountryOfOrigin = "AU";
			line2.JI_Description = "GOODS 2";
			line2.JI_BondedWhsQuantity = 2m;
			line2.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			var fee2 = line2.FeeCusCodes.AddNew();
			fee2.CY_IsOverridden = true;
			fee2.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee2.CY_FeeAmount = 0.25m;

			var header2 = declaration.Invoices.AddNew();
			header2.JZ_InvoiceNumber = "INVOICE 2";
			header2.JZ_RX_NKInvoice_Currency = "USD";
			header2.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			var line3 = header2.JobComInvoiceLines.AddNew();
			line3.JI_OP = product2.PK;
			line3.JI_LinePrice = 3.0m;
			line3.JI_LineNo = 1;
			line3.JI_InvoiceQuantity = 3.0m;
			line3.JI_InvoiceUQ = "CS";
			line3.JI_Weight = 3.0m;
			line3.JI_WeightUQ = "KG";
			line3.JI_Tariff = "9876543212";
			line3.JI_CustomsQuantity = 3.0m;
			line3.US_UC_NKCountryOfOrigin = "NZ";
			line3.JI_Description = "GOODS 3";
			line3.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			line3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			line3.JI_BondedWhsQuantity = 3m;
			var fee3 = line3.FeeCusCodes.AddNew();
			fee3.CY_IsOverridden = true;
			fee3.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee3.CY_FeeAmount = 0.25m;

			var line4 = header2.JobComInvoiceLines.AddNew();
			line4.JI_LinePrice = 4.0m;
			line4.JI_LineNo = 2;
			line4.JI_InvoiceQuantity = 4.0m;
			line4.JI_InvoiceUQ = "PK";
			line4.JI_Weight = 4.0m;
			line4.JI_WeightUQ = "KG";
			line4.JI_Tariff = "9876543213";
			line4.JI_CustomsQuantity = 4.25m;
			line4.US_UC_NKCountryOfOrigin = "JP";
			line4.JI_Description = "GOODS 4";
			line4.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			line4.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			line4.JI_ParentID = line3.PK;
			line3.JI_BondedWhsQuantity = 5m;
			var fee4 = line4.FeeCusCodes.AddNew();
			fee4.CY_IsOverridden = true;
			fee4.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			fee4.CY_FeeAmount = 0.25m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			AssertEquals("FTZZone", "2140000", declaration.FTZEntry.FTZZone);
			AssertEquals("FTZCompanyName", "FRANK T. ZANDERHOF'S FTZ FACILITY", declaration.FTZEntry.FTZCompanyName);
			AssertEquals("FTZAddress", "1819 W MARBLE STREET NE", declaration.FTZEntry.FTZAddress);
			AssertEquals("FTZCity", "SAN ANTONIO", declaration.FTZEntry.FTZCity);
			AssertEquals("FTZState", "TX", declaration.FTZEntry.FTZState);
			AssertEquals("FTZZIPCode", "983456789", declaration.FTZEntry.FTZZIPCode);
			AssertEquals("FTZBillOfLading", "SC1MB2", declaration.FTZEntry.FTZBillOfLading);
			AssertEquals("FTZDeclarationDate", new ZDateTime(2014, 3, 15), declaration.AdmissionMsgLastAcceptedDate);
			AssertEquals("FTZITDate", new ZDateTime(2014, 3, 14), declaration.FTZEntry.FTZITDate);
			AssertEquals("FTZITNumber", "0012 Test US Port of Arrival", declaration.FTZEntry.FTZITFromPort);

			AssertEquals("FTZTotalHMF", 1.0m, declaration.FTZEntry.FTZTotalHMF);
			AssertEquals("FTZTotalEnteredValue", 7.0m, declaration.FTZEntry.FTZTotalEnteredValue);

			AssertEquals("FTZForeignPortOfLading", "00011 Test Foreign Loading Port", declaration.FTZEntry.FTZForeignPortOfLading);
			AssertEquals("FTZPortOfUnlading", "0012 Test US Port of Arrival", declaration.FTZEntry.FTZPortOfUnlading);
			AssertEquals("FTZImportDate", new ZDateTime(2014, 3, 1), declaration.FTZEntry.FTZImportDate);
			AssertEquals("FTZExportDate", new ZDateTime(2014, 2, 28), declaration.FTZEntry.FTZExportDate);

			AssertEquals("FTZZoneStatusN", "x", declaration.FTZEntry.FTZZoneStatusN);
			AssertEquals("FTZZoneStatusP", "", declaration.FTZEntry.FTZZoneStatusP);
			AssertEquals("FTZZoneStatusZ", "", declaration.FTZEntry.FTZZoneStatusZ);
			AssertEquals("FTZZoneStatusD", "", declaration.FTZEntry.FTZZoneStatusD);

			header1.US_ZoneStatus = ZoneStatusList.Codes.Domestic;
			header2.US_ZoneStatus = ZoneStatusList.Codes.Domestic;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			AssertEquals("FTZZoneStatusN", "", declaration.FTZEntry.FTZZoneStatusN);
			AssertEquals("FTZZoneStatusP", "", declaration.FTZEntry.FTZZoneStatusP);
			AssertEquals("FTZZoneStatusZ", "", declaration.FTZEntry.FTZZoneStatusZ);
			AssertEquals("FTZZoneStatusD", "x", declaration.FTZEntry.FTZZoneStatusD);

			AssertEquals("CBP214ExpirationDate", "2/28/2022", declaration.FTZEntry.CBP214ExpirationDate);
			AssertEquals("CBP214RevisionDateForFirstPage", "3/22", declaration.FTZEntry.CBP214RevisionDateForFirstPage);
			AssertEquals("CBP214RevisionDateForContinuationPage", "7/21", declaration.FTZEntry.CBP214RevisionDateForContinuationPage);

			AssertEquals("FTZForeignPortOfLading", "00011 Test Foreign Loading Port", declaration.FTZEntry.FTZForeignPortOfLading);
			AssertEquals("FTZPortOfUnlading", "0012 Test US Port of Arrival", declaration.FTZEntry.FTZPortOfUnlading);
			AssertEquals("FTZImportDate", new ZDateTime(2014, 3, 1), declaration.FTZEntry.FTZImportDate);
			AssertEquals("FTZExportDate", new ZDateTime(2014, 2, 28), declaration.FTZEntry.FTZExportDate);

			line1.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			AssertEquals("FTZZoneStatusN", "x", declaration.FTZEntry.FTZZoneStatusN);
			AssertEquals("FTZZoneStatusP", "", declaration.FTZEntry.FTZZoneStatusP);
			AssertEquals("FTZZoneStatusZ", "", declaration.FTZEntry.FTZZoneStatusZ);
			AssertEquals("FTZZoneStatusD", "x", declaration.FTZEntry.FTZZoneStatusD);

			AssertEquals("FTZForeignPortOfLading", "00011 Test Foreign Loading Port", declaration.FTZEntry.FTZForeignPortOfLading);
			AssertEquals("FTZPortOfUnlading", "0012 Test US Port of Arrival", declaration.FTZEntry.FTZPortOfUnlading);
			AssertEquals("FTZImportDate", new ZDateTime(2014, 3, 1), declaration.FTZEntry.FTZImportDate);
			AssertEquals("FTZExportDate", new ZDateTime(2014, 2, 28), declaration.FTZEntry.FTZExportDate);

			line1.US_ZoneStatus = ZoneStatusList.Codes.ZoneRestricted;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			AssertEquals("FTZZoneStatusN", "", declaration.FTZEntry.FTZZoneStatusN);
			AssertEquals("FTZZoneStatusP", "", declaration.FTZEntry.FTZZoneStatusP);
			AssertEquals("FTZZoneStatusZ", "x", declaration.FTZEntry.FTZZoneStatusZ);
			AssertEquals("FTZZoneStatusD", "x", declaration.FTZEntry.FTZZoneStatusD);

			line1.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			AssertEquals("FTZZoneStatusN", "", declaration.FTZEntry.FTZZoneStatusN);
			AssertEquals("FTZZoneStatusP", "x", declaration.FTZEntry.FTZZoneStatusP);
			AssertEquals("FTZZoneStatusZ", "", declaration.FTZEntry.FTZZoneStatusZ);
			AssertEquals("FTZZoneStatusD", "x", declaration.FTZEntry.FTZZoneStatusD);

			line1.US_SchDLoading = "60267";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("FTZForeignPortOfLading", "MULTI (SEE ITEM 15)", declaration.FTZEntry.FTZForeignPortOfLading);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.ZoneToZone;
			declaration.JE_PrimaryITNumber = ZString.Empty;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("FTZForeignPortOfLading", "MULTI (SEE ITEM 15)", declaration.FTZEntry.FTZForeignPortOfLading);
			AssertEquals("FTZPortOfUnlading", "0012 Test US Port of Arrival", declaration.FTZEntry.FTZPortOfUnlading);
			AssertEquals("FTZImportDate", new ZDateTime(2014, 3, 1), declaration.FTZEntry.FTZImportDate);
			AssertEquals("FTZExportDate", new ZDateTime(2014, 2, 28), declaration.FTZEntry.FTZExportDate);

			AssertEquals("FTZITDate", ZDateTime.Empty, declaration.FTZEntry.FTZITDate);
			AssertEquals("FTZITFromPort", ZString.Empty, declaration.FTZEntry.FTZITFromPort);

			AssertEquals("FTZImportingVesselOtherCarrier", "ZONE TRANSFER", declaration.FTZEntry.FTZImportingVesselOtherCarrier);

			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "LUCY TANIA";
			vessel.RV_RN_NKCountryOfReg = "IE";

			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Name = "INTERFLUG";
			carrier.UI_Code = "IF";

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "HB1";
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.US_UI_NKBillIssuerSCAC = "IF";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "MB2";
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.US_UI_NKBillIssuerSCAC = "IF";

			header1.JZ_CU_RelatedHouseBill = bill.PK;
			header2.JZ_CU_RelatedHouseBill = bill2.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = "LUCY TANIA";
			AssertEquals("FTZImportingVesselOtherCarrier", "LUCY TANIA, IE", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("FTZImportingVesselOtherCarrier", "RAILROAD", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("FTZImportingVesselOtherCarrier", "TRUCK", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("FTZImportingVesselOtherCarrier", "PIPELINE", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			AssertEquals("FTZImportingVesselOtherCarrier", "OTHER", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			AssertEquals("FTZImportingVesselOtherCarrier", "MAIL", declaration.FTZEntry.FTZImportingVesselOtherCarrier);
			AssertEquals("FTZBillOfLading", ZString.Empty, declaration.FTZEntry.FTZBillOfLading);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "123";
			declaration.US_UI_NKCarrierSCAC = "IF";
			AssertEquals("FTZImportingVesselOtherCarrier", "INTERFLUG", declaration.FTZEntry.FTZImportingVesselOtherCarrier);

			var ftzLine1 = declaration.FTZEntry.MergedLines[0];
			var ftzLine2 = declaration.FTZEntry.MergedLines[1];
			var ftzLine3 = declaration.FTZEntry.MergedLines[2];
			var ftzLine4 = declaration.FTZEntry.MergedLines[3];

			AssertEquals("FTZInvoice", "INVOICE 1", ftzLine1.InvoiceNumber);
			AssertEquals("FTZInvoice", "INVOICE 1", ftzLine2.InvoiceNumber);
			AssertEquals("FTZInvoice", "INVOICE 2", ftzLine3.InvoiceNumber);
			AssertEquals("FTZInvoice", "INVOICE 2", ftzLine4.InvoiceNumber);

			AssertEquals("FTZCountryOfOrigin", "FR", ((IFTZLine)ftzLine1).CountryOfOrigin);
			AssertEquals("FTZCountryOfOrigin", "AU", ((IFTZLine)ftzLine2).CountryOfOrigin);
			AssertEquals("FTZCountryOfOrigin", "NZ", ((IFTZLine)ftzLine3).CountryOfOrigin);
			AssertEquals("FTZCountryOfOrigin", "JP", ((IFTZLine)ftzLine4).CountryOfOrigin);

			AssertEquals("FTZDescription", "GOODS 1", ftzLine1.FTZDescription);
			AssertEquals("FTZDescription", "GOODS 2", ftzLine2.FTZDescription);
			AssertEquals("FTZDescription", "GOODS 3", ftzLine3.FTZDescription);
			AssertEquals("FTZDescription", "GOODS 4", ftzLine4.FTZDescription);

			AssertEquals("FTZWarehouseQtyAndUnit", "8 PK", ftzLine1.FTZWarehousePackageQtyAndUnit);
			AssertEquals("FTZWarehouseQtyAndUnit", "2 PK", ftzLine2.FTZWarehousePackageQtyAndUnit);
			AssertEquals("FTZWarehouseQtyAndUnit", "5 CS", ftzLine3.FTZWarehousePackageQtyAndUnit);
			AssertEquals("Total Warehouse quantity and Unit", "10 PK\r\n5 CS\r\n", declaration.FTZEntry.FTZWarehousePackageTotalQuantityAndUnit);
			AssertEquals("Total Quantity and Unit", "1 KG\r\n", declaration.FTZEntry.FTZBox18TotalQuantity);
			AssertEquals("Total Weight and Unit", "11 KG\r\n", declaration.FTZEntry.FTZBox19TotalGrossWeight);
			AssertEquals("Total Customs Value", 7.0m, declaration.FTZEntry.FTZBox20TotalAggrCharges);

			AssertEquals("FTZTariff", "9876.54.3210 (Zone P)", ftzLine1.FTZTariff);
			AssertEquals("FTZTariff", "9876.54.3211 (Zone D)", ftzLine2.FTZTariff);
			AssertEquals("FTZTariff", "9876.54.3212 (Zone D)", ftzLine3.FTZTariff);
			AssertEquals("FTZTariff", "9876.54.3213 (Zone D)", ftzLine4.FTZTariff);

			AssertEquals("FTZQuantity", "1.00 KG", ftzLine1.FTZQuantityAndUnit);
			AssertEquals("FTZQuantity", "2.00 ", ftzLine2.FTZQuantityAndUnit);
			AssertEquals("FTZQuantity", "3.00 ", ftzLine3.FTZQuantityAndUnit);
			AssertEquals("FTZQuantity", "4.25 ", ftzLine4.FTZQuantityAndUnit);

			AssertEquals("FTZSecondQuantity", "1.15 L", ftzLine1.FTZSecondQuantityAndUnit);
			AssertEquals("FTZSecondQuantity", "", ftzLine2.FTZSecondQuantityAndUnit);
			AssertEquals("FTZSecondQuantity", "", ftzLine3.FTZSecondQuantityAndUnit);
			AssertEquals("FTZSecondQuantity", "", ftzLine4.FTZSecondQuantityAndUnit);

			AssertEquals("FTZWeight", "2 KG", ftzLine1.FTZWeightAndUnit);
			AssertEquals("FTZWeight", "2 KG", ftzLine2.FTZWeightAndUnit);
			AssertEquals("FTZWeight", "3 KG", ftzLine3.FTZWeightAndUnit);
			AssertEquals("FTZWeight", "4 KG", ftzLine4.FTZWeightAndUnit);

			AssertEquals("FTZCustomsValue", 1.0m, ftzLine1.FTZCustomsValue);
			AssertEquals("FTZCustomsValue", 2.0m, ftzLine2.FTZCustomsValue);
			AssertEquals("FTZCustomsValue", 0.0m, ftzLine3.FTZCustomsValue);
			AssertEquals("FTZCustomsValue", 4.0m, ftzLine4.FTZCustomsValue);

			declaration.Packages.RemoveAndDeleteAll();

			bill.CU_NoOfPacks = 7.0m;
			bill.CU_PackType = "AE";
			bill2.CU_NoOfPacks = 8.0m;
			bill2.CU_PackType = "AM";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			declaration.FTZ214PrintCollection.PopulateElements();
			var ftzEntryLine1 = declaration.FTZ214PrintCollection[0];
			var ftzEntryLine2 = declaration.FTZ214PrintCollection[1];

			AssertEquals("FTZPackQuantityAndType", "7 AE", ftzEntryLine1.FTZPackQuantityAndType);
			AssertEquals("FTZPackQuantityAndType", "7 AE", ftzEntryLine2.FTZPackQuantityAndType);

			AssertEquals("FTZBillAndITNumber", "MB: SC1\r\nHB: IFHB1", ftzEntryLine1.FTZBillAndITNumber);
			AssertEquals("FTZBillAndITNumber", "MB: SC1\r\nHB: IFHB1", ftzEntryLine2.FTZBillAndITNumber);

			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.OH_FullName = "ORG1";
			var docAddress = declaration.WarehouseDocAddress;
			docAddress.OrganisationPK = org.PK;
			Factory.Save();
			declaration.FTZEntry.CalculateFTZZoneStatuses();

			docAddress.E2_Address1 = "docAddress1";
			docAddress.E2_Address2 = "docAddress2";
			docAddress.E2_City = "docCity";
			docAddress.E2_State = "docState";
			docAddress.E2_Postcode = "docPos";
			AssertEquals("FTZCompanyName", org.OH_FullName, declaration.FTZEntry.FTZCompanyName);
			AssertEquals("FTZAddress", docAddress.E2_Address1 + " " + docAddress.E2_Address2, declaration.FTZEntry.FTZAddress);
			AssertEquals("FTZCity", docAddress.E2_City, declaration.FTZEntry.FTZCity);
			AssertEquals("FTZState", docAddress.E2_State, declaration.FTZEntry.FTZState);
			AssertEquals("FTZZIPCode", docAddress.E2_Postcode, declaration.FTZEntry.FTZZIPCode);
		}

		public void TestIsPTTWIthoutException()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.US_F_PTTWOExc = true;
			var header = declaration.Invoices.AddNew();
			header.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("PTTWithoutException", declaration.US_F_PTTWOExc, entry.IsPTTWithoutException);
		}

		public void TestFTZ214DoucmnetBox29to31()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "XXX";
			broker1.GS_FullName = "No Need Unit Test";
			broker1.SignatureImage = new Bitmap(2, 1);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, broker1.PK.ToGuid());

			var header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INVOICE 1";
			header1.JZ_RX_NKInvoice_Currency = "USD";

			var line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1.0m;

			ICustomsBrokerDetails brokerDetails = line1;
			AssertEquals(declaration.BranchIAddressDetails, brokerDetails.Address);

			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.OH_FullName = "OR#1";
			org.OH_IsBroker = true;
			declaration.WarehouseDocAddress.OrganisationPK = org.PK;

			var message = CreateMessage(EDIMessage.Direction.Receive, ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			declaration.Messages.Add(message);
			message.EM_SystemCreateTimeUtc = new ZDateTime(2012, 04, 02, 22, 58, 00);
			message.EM_MessageText =
"B013910SV9NF                                               HYEDUSCMT_117429     " +
"90D153000112000000652501N                                                       " +
"91B6 2  XXXZBLFEB0902A                     1204022258                           " +
"9502115 ZONE CONCURRENCE DATA ACCEPTED                                          " +
"Y  3910SV9NF00001";

			declaration.Messages.Add(message);

			declaration.FTZConcurrenceStatus = FTZMessageStatusList.Codes.ClearConcurrence;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("FTZAdmissionStatusDate", true, declaration.FTZEntry.IsFTZOrgBroker);
			AssertEquals("FTZAdmissionStatusDate", true, declaration.FTZEntry.IsConcurrenceCleared);
			AssertEquals("FTZAdmissionStatusDate", true, declaration.FTZEntry.IsConcurrenceClearedAndIsFTZOrgBroker);

			var entry = declaration.CustomsEntryHeaders[0];
			AssertNotNull("Print Signature on the document.", entry.ConcurrenceFTZBrokerSignatureImage);
			AssertEquals("No Need Unit Test", entry.FTZDeclarantName);
			AssertEquals("EDI CUSTOMS BROKERS AS ATTY-IN-FACT", entry.FTZBrokerTitle);

			org.OH_IsBroker = false;
			Factory.Save();

			AssertEquals("FTZAdmissionStatusDate", false, declaration.FTZEntry.IsFTZOrgBroker);
			AssertEquals("FTZAdmissionStatusDate", true, declaration.FTZEntry.IsConcurrenceCleared);
			AssertEquals("FTZAdmissionStatusDate", false, declaration.FTZEntry.IsConcurrenceClearedAndIsFTZOrgBroker);
		}

		public void TestFTZ214DocumentBox12()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PrimaryITNumber = "8888884";
			declaration.US_ITDate = new ZDateTime(2023, 11, 01);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			JobComInvoiceHeader header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INVOICE 1";
			header1.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1.0m;
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.OH_FullName = "OR#1";
			org.OH_IsBroker = true;
			declaration.WarehouseDocAddress.OrganisationPK = org.PK;
			declaration.DeliveryOrPickupCartageCoPK = org.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("Company Full Name when ITNumber and IT Date entered", "OR#1", declaration.FTZEntry.InBondCarrier);
			declaration.US_ITDate = ZDateTime.Empty;
			AssertEquals("IT Date is empty", "", declaration.FTZEntry.InBondCarrier);
			declaration.US_ITDate = new ZDateTime(2023, 11, 02);
			declaration.JE_PrimaryITNumber = "";
			AssertEquals("ITNumber is empty", "", declaration.FTZEntry.InBondCarrier);
		}

		public void TestFTZ214DocumentPrintDisposition()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var messagePlus = Factory.New<MQEDIMessageForTesting>();
			MQEDIMessage result = messagePlus;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			MQEDIMessage message = messagePlus;
			declaration.Messages.Add(message);
			message.EM_MessageText =
"B003910SV9NF                                               HYEDUSCMT_118672     " +
"90A153000112000000702501N                                                       " +
"91B4 1  15300011200000070                  1403251330                           " +
"95     FTZ PAPERLESS ADMISSION                                                  " +
"Y  3910SV9NF00003";

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			message.EM_LinkUniqueID = declaration.PK;

			JobComInvoiceHeader header1 = declaration.Invoices.AddNew();
			header1.JZ_InvoiceNumber = "INVOICE 1";
			header1.JZ_RX_NKInvoice_Currency = "USD";
			JobComInvoiceLine line1 = header1.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1.0m;

			messagePlus = Factory.New<MQEDIMessageForTesting>();
			result = messagePlus;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			result.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			message = messagePlus;

			declaration.Messages.Add(message);
			message.EM_MessageText =
"B003910SV9NF                                               HYEDUSCMT_118672     " +
"90A153000112000000702501N                                                       " +
"91B5 1  15300011200000070                  1403251331                           " +
"95     FTZ ADMISSION DOCS REQD                                                  " +
"Y  3910SV9NF00003";
			AssertEquals(2, declaration.FTZDispositionCodes.Count);
			AssertEquals(DispositionList.Codes.B5, declaration.FTZDispositionCodes[1].US_Code);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("FTZAdmissionStatusDate", new ZDateTime(2014, 3, 25, 13, 31, 0), declaration.FTZEntry.FTZAdmissionStatusDate);
			AssertEquals("FTZAdmissionStatusMessage", "B5  FTZ Admission Documents Required", declaration.FTZEntry.FTZAdmissionStatusMessage);
		}

		public void TestImportingVesselName()
		{
			const string FTZForTest = "FTZ001A";
			const string VesselNameForTest = "SomeVessel";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MasterBill = "123456";
			declaration.US_FTZNo = FTZForTest;
			declaration.JE_VesselName = VesselNameForTest;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1211.90.9180";
			invoiceLine.JI_LinePrice = 250m;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.JI_LinePrice = 1m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			AssertEquals(ZString.Empty, entry.ImportingVesselName);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VesselName = VesselNameForTest;
			AssertEquals(VesselNameForTest, entry.ImportingVesselName);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VesselName = VesselNameForTest;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			AssertEquals("ImportingVesselName for 06", ZString.Empty, entry.ImportingVesselName);
			AssertEquals("ImportFTZNumber", FTZForTest, entry.ImportFTZNumber);
		}

		public void TestIsNonAMS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "MB1234456";
			declaration.JE_HouseBill = "HB34192314";

			var invoice = declaration.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			Assert(!entry.IsNonAMS);

			declaration.US_NonAMS = true;
			Assert(entry.IsNonAMS);

			declaration.US_NonAMS = false;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			Assert(entry.IsNonAMS);
		}

		[TestDate(2017, 12, 1)]
		public void TestBuildEmpty89EvenIfNoFeeExists()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 251.00m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "1211.90.9180";
			invoiceLine.JI_LinePrice = 250m;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.JI_LinePrice = 1m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			IDutyDataLineHeader iHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("BuildEmpty89EvenIfNoFeeExists", false, entry.BuildEmpty89EvenIfNoFeeExists);
			AssertEquals("GrandTotalFee", 25.31m, iHeader.FeeAndCharges.GetGrandTotalFee());

			invoiceLine.US_SPI = "AU";//MPF exempt therefore HMF becomes exempt
			invoiceLine2.US_SPI = "AU";//MPF exempt therefore HMF becomes exempt

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("BuildEmpty89EvenIfNoFeeExists", true, entry.BuildEmpty89EvenIfNoFeeExists);
			AssertEquals("GrandTotalFee", 0m, iHeader.FeeAndCharges.GetGrandTotalFee());
		}

		[TestDate(2009, 6, 1)]
		public void TestRequiredCodesWhenAmountIsEmpty()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 12000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_Tariff = "0804.40.0010";//AVOCADOS - Required
			invoiceLine.US_UC_NKCountryOfExport = "AU";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "AU";//MPF is exempt
									  //No quantities are set on purpose
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICusEntryLine entryLine = invoiceLine.CusEntryLine;
			List<IFee> fees = new List<IFee>(entryLine.Fees);
			AssertEquals(1, fees.Count);
			AssertEquals("Avocado fee is required", Core.Constants.USCustoms.FeeCodes.Avocado, fees[0].Code);
			AssertEquals("Avocado fee is required", 0m, fees[0].Amount);

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, false);
			MQEDIMessage message = builder.PopulateMessage();

			MessageBuildingBlocks.Input.ENS62 ens62 = (MessageBuildingBlocks.Input.ENS62)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "62");
			AssertNotNull(ens62);
			AssertEquals(0m, ens62.UserFeeAmount);
			AssertEquals(Core.Constants.USCustoms.FeeCodes.Avocado, ens62.ClassCode.ToString());

			MessageBuildingBlocks.Input.ENS89 ens89 = (MessageBuildingBlocks.Input.ENS89)message.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "89");
			AssertNotNull(ens89);
			AssertEquals(true, ens89.IsEmpty);
		}

		public void TestIReconOriginalChargeParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			IReconOriginalChargeParent entry = declaration.CustomsEntryHeaders.AddNew();
			var iHeader = (IDutyDataLineHeader)entry;

			AssertEquals(declaration.CustomsEntryHeaders[0], entry.ParentAsBusinessObject);
		}

		[TestDate(2023, 10, 25)]
		public void TestFeeAndChargeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			IReconOriginalChargeParent entry = declaration.CustomsEntryHeaders.AddNew();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AccountingClassFeeCode, ZDateTime.Now);

			AssertEquals(codes.Length + 3, entry.FeeAndChargeList.Count);

			AssertEquals("Duty", entry.FeeAndChargeList.GetDescriptionFromCode("DTY"));
			AssertEquals("Interest Amount For Reconciliation Summary", entry.FeeAndChargeList.GetDescriptionFromCode("ARS"));
			AssertEquals("MPF As Calculated And Unadjusted", entry.FeeAndChargeList.GetDescriptionFromCode("MPC"));
		}

		public void TestReconEntriesAreNotDeactivatedOnMerge()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			ReconOriginalEntryHeader originalEntry = reconDec.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice = originalEntry.Invoice;
			invoice.JobComInvoiceLines.AddNew();

			ReconOriginalEntryHeader originalEntry2 = reconDec.OriginalEntries.AddNew();

			reconDec.CalculateDutyFeesForChangedEntries();
			AssertEquals("Still active", true, originalEntry.GetWrappedEntry().IsActive);
			AssertEquals("Still active", true, originalEntry2.GetWrappedEntry().IsActive);
			AssertEquals("Still active", true, reconDec.ReconEntry.GetEntry().IsActive);
		}

		public void TestHasBeenLodgedAtCustoms()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());

			var reconEntry = reconDec.ReconEntry.GetEntry();
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconOriginal;
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconDelete;
			AssertEquals(true, reconEntry.HasBeenLodgedAtCustoms);

			reconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconDelete;
			AssertEquals(false, reconEntry.HasBeenLodgedAtCustoms);
			AssertEquals(true, reconEntry.HasBeenWithdrawn);
		}

		public void TestReconIssue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			ICusEntryHeader iEntry = entry;

			declaration.US_NAFTAReconIndicator = true;
			AssertEquals(true, iEntry.NAFTAReconciliation);

			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			AssertEquals("002", iEntry.OtherReconciliationIndicator);
		}

		public void TestTaxAmounts()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.DistilledSpirits, 1m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.OtherExcise, 2m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco, 4m);
			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Wines, 8m);

			AssertEquals("DistilledSpiritsTax", 1m, entry.DistilledSpiritsTax);
			AssertEquals("OtherExciseTax", 2m, entry.OtherExciseTax);
			AssertEquals("TobaccoTax", 4m, entry.TobaccoTax);
			AssertEquals("WinesTax", 8m, entry.WinesTax);

			AssertEquals("TotalEstimatedTax", 15m, entry.TotalEstimatedTax);
			AssertEquals("TotalIRTTaxes", 15m, entry.TotalIRTTaxes);
		}

		public void TestSorghumFee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			entry.Charges.SetAmount(Core.Constants.USCustoms.FeeCodes.Sorghum, 1m);
			AssertEquals(1m, entry.SorghumFee);
		}

		public void TestReconOriginalEntry()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();

			JobDeclaration declaration = reconDeclaration.ReconWrappedJobDeclaration;
			CusEntryHeader reconEntry = reconOriginalEntry.GetWrappedEntry();

			AssertNotNull("reconEntry.ReconOriginalEntry", reconEntry.ReconOriginalEntry);

			ErrorReporter.Clear();
			reconEntry.ReconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();

			try
			{
				AssertEquals("developers exception thrown to warn. The wrapping object should wrap an instance of entry.", "You are not supposed to set ReconOriginalEntry more than once.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestIDutyDataLineHeader()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_SchDEntry = "3901";
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_MonthlyFiling = true;
			declaration.US_PayableMPF = 300.12m;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			IDutyDataLineHeader ientry = entry;

			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			CusEntryLine entryLine3 = entry.MergedLines.AddNew();
			entryLine3.US_CL_ParentLine = entryLine2.PK;
			AssertEquals("PreCondition:IsSecondaryTariffLine", true, entryLine3.IsSecondaryTariffLine);
			AssertEquals("DutyDataLines including secondary tariff line", 3, new List<IEntryLineOrInvoiceLineDutyData>(ientry.DutyDataLines).Count);

			AssertEquals("IsInformalFeeApplicable", false, ientry.IsInformalFeeApplicable);
			AssertEquals("OverridenTotalMPFPayable", 300.12m, ientry.OverridenTotalMPFPayable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			AssertEquals("IsInformalFee not Applicable as currently Mail", false, ientry.IsInformalFeeApplicable);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("IsInformalFeeApplicable as currently Mail", true, ientry.IsInformalFeeApplicable);
		}

		[TestDate(2017, 12, 1)]
		public void TestIDutyDataLineHeaderCharge()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);

			IDutyDataLineHeader ientry = declaration.CustomsEntryHeaders[0];
			AssertNotNull(ientry.FeeAndCharges);

			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals("current total duty", 45m, declaration.CustomsEntryHeaders[0].TotalDutyAmount);
			AssertEquals("Dutialbe mail fee applicable", 5m, ientry.FeeAndCharges.GetFeeOrChargeAmount(Core.Constants.USCustoms.FeeCodes.DutiableMail));
		}

		public void TestIDutyDataLineHeaderDeleteDetachedEntryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			line1.JI_CL = entryLine1.PK;
			line2.JI_CL = entryLine1.PK;

			CusEntryLine entryLine2 = entry.MergedLines.AddNew();

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryLine entryLine3 = entry2.MergedLines.AddNew();

			IDutyDataLineHeader ientry = entry;
			ientry.DeleteDetachedEntryLines();

			AssertEquals("entryLine1.IsDeleted", false, entryLine1.IsDeleted);
			AssertEquals("entryLine2.IsDeleted", true, entryLine2.IsDeleted);
			AssertEquals("entryLine3.IsDeleted", false, entryLine3.IsDeleted);
		}

		public void TestShouldAddLogForMessageStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("one log has been added", 1, entry.Logs.GetAllLogs().Count);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			entry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals("Clear Reconciliation Original log has been added", 1, entry.Logs.GetAllLogs().Count);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal;
			AssertEquals("one log has been added", 1, entry.Logs.GetAllLogs().Count);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			AssertEquals("one log has been added", 1, entry.Logs.GetAllLogs().Count);
		}

		public void TestShouldAddLogForReconciliationMessageStatus()
		{
			var reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconOriginal;
			AssertEquals("Log added, because Declaration rejected", 1, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);
			AssertEquals("Log event description", "Declaration Rejected", reconDeclaration.ReconEntry.Logs.GetAllLogs()[0].SL_EventDescription);
			AssertEquals("Log reference", "ERO", reconDeclaration.ReconEntry.Logs.GetAllLogs()[0].SL_Reference);

			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconDelete;
			AssertEquals("new Log for rejection added", 2, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);
			AssertEquals("Log event description", "Declaration Rejected", reconDeclaration.ReconEntry.Logs.GetAllLogs()[1].SL_EventDescription);
			AssertEquals("Log reference", "ERD", reconDeclaration.ReconEntry.Logs.GetAllLogs()[1].SL_Reference);

			reconDeclaration = new DeclarationTestHelper().GetDutiableReconDeclaration(Factory);
			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			AssertEquals("Log not added, because we are not tracking awaiting status", 0, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);

			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconDelete;
			AssertEquals("Log not added, because we are not tracking awaiting status", 0, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);

			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			AssertEquals("CRO Log should be added for Entry", 1, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);
			AssertEquals("Customs Cleared Log should be added for Declaration", "Customs Cleared", reconDeclaration.Logs.GetAllLogs()[0].SL_EventDescription);
			AssertEquals("Customs Cleared Log Reference", "Reconciliation", reconDeclaration.Logs.GetAllLogs()[0].SL_Reference);

			reconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconReplace;
			AssertEquals("No new logs added, because status from 'Clear' to 'Clear'", 1, reconDeclaration.ReconEntry.Logs.GetAllLogs().Count);
			AssertEquals("Log event description", "Customs Entry Status", reconDeclaration.ReconEntry.Logs.GetAllLogs()[0].SL_EventDescription);
		}

		public void TestUniqueReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_BGMReference = "79834789";
			AssertEquals("UniqueReference", "79834789", entry.UniqueReference);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "89045798";
			AssertEquals("UniqueReference", "XJ589045798", entry.UniqueReference);
		}

		public void TestUltimateConsigneeNumberForCargoRelease()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "1");

			OrgHeader ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "2");

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICargoReleaseCusEntryHeader entry = invoiceLine.CusEntryLine.Header;

			AssertEquals("PreCondition", false, declaration.HasLineLevelUltimateConsignees);
			AssertEquals("1", entry.UltimateConsigneeNumber);

			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			AssertEquals("PreCondition", true, declaration.HasLineLevelUltimateConsignees);
			AssertEquals("", entry.UltimateConsigneeNumber);
		}

		public void TestICargoManifestStatusQueryData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_DeclarationReference = "B89345789";
			declaration.US_EntryFilerCode = "ABC";

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "123456";

			ICargoManifestStatusQueryData queryData = entry;
			AssertEquals("Reference", "123456", queryData.HumanFriendlyReference);
			AssertEquals("EntryOrInBondNumber", "123456", queryData.EntryOrInBondNumber);
			AssertEquals("ControllerID", ControllerIDs.Customs.JobDeclaration, queryData.ControllerID);
			AssertEquals("", queryData.MasterAirWayBillNumber);
			AssertEquals("", queryData.HouseAirWayBillNumber);
			AssertEquals("", queryData.BillIssuerCode);
			AssertEquals("", queryData.BillNumber);
			AssertEquals("JobReferenceNumber", "B89345789", queryData.JobReferenceNumber);
			AssertEquals("BusinessObjectPK", declaration.PK, queryData.BusinessObjectPK);

			EDIMessage message = Factory.New<EDIMessage>();
			queryData.LinkToMessage(message);
			AssertEquals(true, entry.Messages.Contains(message));
			AssertEquals(entry, message.EM_LinkedObject);
			AssertEquals(CargoManifestQueryActionType.Entry, queryData.QueryActionType);
			AssertEquals(CusEntryHeaderSchema.Constants.Prefix, queryData.TableCode);
		}

		public void TestTotalCustomsValueOfLinesWithMPF()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 4000.25m;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000.75m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("all lines are MPF-applicable", 10001m, declaration.CustomsEntryHeaders[0].TotalCustomsValueOfLinesWithMPF);

			invoiceLine2.US_SPI = "AU";//MPF exempt
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("one line is MPF-exempt", 4000.25m, declaration.CustomsEntryHeaders[0].TotalCustomsValueOfLinesWithMPF);
		}

		public void TestPayableBondedADD_CVD()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A570904014";
			addCase1.U5_ISOCountryCode = "CN";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "38021000";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 0.70m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			USCACCase addCase2 = Factory.New<USCACCase>();
			addCase2.U5_CaseNumber = "A533838000";
			addCase2.U5_ISOCountryCode = "IN";
			addCase2.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase2.CaseTariffs.AddNew().U9_TariffNumber = "3204179040";
			var rate2 = addCase2.CaseRates.AddNew();
			rate2.U6_AdValoremRate = 0.27m;
			rate2.U6_EffectiveDate = ZDateTime.Today;

			USCACCase cvdCase = Factory.New<USCACCase>();
			cvdCase.U5_CaseNumber = "C533839001";
			cvdCase.U5_ISOCountryCode = "IN";
			cvdCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			cvdCase.CaseTariffs.AddNew().U9_TariffNumber = "3204179040";
			var cvdRate = cvdCase.CaseRates.AddNew();
			cvdRate.U6_AdValoremRate = 0.35m;
			cvdRate.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 4000m;
			invoiceLine1.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine1.US_ADDCaseNo = "A570904014";
			invoiceLine1.US_IsBondedADD = true;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 6000m;
			invoiceLine2.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine2.US_ADDCaseNo = "A533838000";
			invoiceLine2.US_CVDCaseNo = "C533839001";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);

			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("CVD Duty payable", 2100.00m, entry.PayableCVDDuty);
			AssertEquals("ADD duty payable", 1620.00m, entry.PayableADDDuty);
			AssertEquals("ADD duty bonded ", 2800.00m, entry.BondedADDDuty);
			AssertEquals("Bonded ADD Duty Indicator", true, entry.BondedADDIndicator);
			AssertEquals("Bonded CVD Duty Indicator", false, entry.BondedCVDIndicator);

			invoiceLine1.US_IsBondedADD = false;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals("CVD Duty payable", 2100.00m, entry.PayableCVDDuty);
			AssertEquals("ADD duty payable", 4420.00m, entry.PayableADDDuty);
			AssertEquals("Bonded ADD Duty Indicator", false, entry.BondedADDIndicator);
			AssertEquals("Bonded CVD Duty Indicator", false, entry.BondedCVDIndicator);
		}

		public void TestIsPaid()
		{
			CusStatementHeader header1 = Factory.New<CusStatementHeader>();
			header1.B2_PaymentStatus = PaymentStatusList.Codes.PaymentAuthorizationAccepted;
			CusStatementLine line1 = header1.StatementLines.AddNew();
			line1.B3_EntryFilerCode = "XXX";
			line1.B3_EntryNum = "123";

			CusStatementHeader header2 = Factory.New<CusStatementHeader>();
			header2.B2_PaymentStatus = PaymentStatusList.Codes.PaymentInProgress;
			CusStatementLine line2 = header2.StatementLines.AddNew();
			line2.B3_EntryFilerCode = "XXY";
			line2.B3_EntryNum = "123";

			Factory.Save();

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XXX";
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "123";
			AssertEquals("IsPaid", true, entry.IsPaidViaStatement);

			declaration.US_EntryFilerCode = "XXY";
			AssertEquals("IsPaid", false, entry.IsPaidViaStatement);
		}

		[TestDate(2008, 3, 25)]
		public void TestICusEntryHeaderFees()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3302.10.50 00";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			List<IFee> fees = new List<IFee>(entry.Fees);

			AssertEquals("Fees for entry do not include excise taxes", 1, fees.Count);
			AssertEquals("MPF amount", 25m, fees[0].Amount);
			AssertEquals("Grand Total Fee", 25m, entry.GrandTotalFee);
			AssertEquals("TotalEstimatedTax", 0m, entry.TotalEstimatedTax);
			AssertEquals("Total Duty", 2740m, entry.TotalEstimatedDuty);
			AssertEquals("Total Paid including duty+tax+Fees", 2765m, declaration.CustomsEntryHeaders[0].CH_TotalPaid);

			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.JI_CustomsQuantity = 15000m;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			fees = new List<IFee>(entry.Fees);
			AssertEquals("Two elements in the invoiceLine.Fees collection", 2, invoiceLine.FeeCusCodes.Count);
			AssertEquals("However entry.Fees returns only one fee: MPF", 1, fees.Count);
			AssertEquals("MPF amount", 25m, fees[0].Amount);
			AssertEquals("Grand Total Fee", 25m, entry.GrandTotalFee);
			AssertEquals("TotalEstimatedTax", 2300.84m, entry.TotalEstimatedTax);
			AssertEquals("Total Duty", 0m, entry.TotalEstimatedDuty);
			AssertEquals("Total Paid including duty+tax+Fees", 2325.84m, declaration.CustomsEntryHeaders[0].CH_TotalPaid);
		}

		[TestDate(2017, 12, 1)]
		public void TestGrandTotalOtherRevenueAmount()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Cofee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;
			Factory.Save();

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "00001111";
			tariff2.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff2.UE_ShortDescription = "Wines Test";
			tariff2.UE_Unit1 = "KG";

			var dutyRate2 = tariff2.DutyRates.AddNew();
			dutyRate2.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate2.UD_TaxFeeFlag = "1";
			dutyRate2.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate2.UD_TaxFeeSpecificRate = 0.0851m;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			declaration.US_EntryFilerCode = "XJ5";
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = tariff2.UE_Tariff;
			invoiceLine2.JI_CustomsQuantity = 840m;
			invoiceLine2.JI_LinePrice = 2000m;
			invoiceLine2.JI_Weight = 3285.59m;
			invoiceLine2.JI_WeightUQ = "KG";
			invoiceLine2.JI_CustomsSecondQuantity = 3285.59m;

			declaration.US_SchDEntry = "1234";
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var iCusEntryHeader = (ICusEntryHeader)entry;
			AssertEquals(420m, iCusEntryHeader.GrandTotalOtherRevenueAmount);
			AssertEquals("GrandTotalFee should not contain coffee fee", 30m, iCusEntryHeader.GrandTotalFee);
			AssertEquals("TotalAmountPayable should contain coffee fee", 521.480m, entry.TotalAmountPayable);
		}

		public void TestIRTTaxesWhenSpiritsTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 9659m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.30.3030";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.US_UC_NKCountryOfExport = "GB";
			invoiceLine.JI_LinePrice = 4947m;
			invoiceLine.JI_CustomsQuantity = 194;
			invoiceLine.JI_CustomsUnitQty = "PFL";

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2208.30.3030";
			invoiceLine2.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine2.US_UC_NKCountryOfExport = "GB";
			invoiceLine2.JI_LinePrice = 4712m;
			invoiceLine2.JI_CustomsQuantity = 77;
			invoiceLine2.JI_CustomsUnitQty = "PFL";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			AssertEquals("TotalEstimatedTax", 966.48m, entry.TotalEstimatedTax);
			AssertEquals("TotalIRTTaxes", 966.48m, entry.TotalIRTTaxes);
		}

		public void TestIsTemporaryImportationBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("IsTemporaryImportationBond", false, entry.IsTemporaryImportationBond);

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			AssertEquals("IsTemporaryImportationBond", true, entry.IsTemporaryImportationBond);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("IsTemporaryImportationBond", false, entry.IsTemporaryImportationBond);
		}

		public void TestIsExhibition()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("IsExhibition", false, entry.IsExhibition);

			declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			AssertEquals("IsExhibition", true, entry.IsExhibition);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("IsExhibition", false, entry.IsExhibition);
		}

		[TestDate(2008, 08, 21)]
		public void TestExhibitionEstimatedDutiesIfEntryHadBeenForConsumption()
		{
			CreateExhibitionDeclaration();
			CusEntryHeader entry = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Exhibition estimated duty - Parent/Child", 697.60m, entry.ExhibitionEstimatedDutiesIfEntryHadBeenForConsumption);

			CreateExhibitionDeclaration2();
			CusEntryHeader entry2 = Declaration.CustomsEntryHeaders[0];
			AssertEquals("Exhibition estimated duty", 45m, entry2.ExhibitionEstimatedDutiesIfEntryHadBeenForConsumption);
		}

		public void TestIsBorderCargoRelease()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(true, entry.IsBorderCargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(false, entry.IsBorderCargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entry.IsBorderCargoRelease);
		}

		public void TestIsCargoRelease()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(false, entry.IsCargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(true, entry.IsCargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entry.IsCargoRelease);
		}

		public void TestIsSimplifiedEntry()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			AssertEquals(true, entry.IsACECargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(false, entry.IsACECargoRelease);
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entry.IsACECargoRelease);
		}

		public void TestFormalEntryNumberGenerationForReconIsDelayedUntilMessageIsGenerated()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			AssertNotNull(reconDeclaration.ReconEntry);

			Factory.Save();
			AssertEquals("No entry number is generated yet", true, reconDeclaration.ReconEntryNumber.IsEmpty);

			new ReconMessageManager(new ReconDeclarationIReconciliation(reconDeclaration), UpdateActionCode.Add).PopulateMessage();
			Factory.Save();

			AssertEquals("One message should have been generated", 1, reconDeclaration.Messages.Count);
			AssertEquals("Entry number generation is initiated by MQEDIMessage", false, reconDeclaration.ReconEntryNumber.IsEmpty);
			AssertEquals(true, reconDeclaration.Messages[0].EM_MessageText.Contains(reconDeclaration.ReconEntryNumberWithEntryFilerCode));
		}

		public void TestReconEntryNumberWithEntryFilerCode()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ReconDeclaration reconDeclaration = new ReconDeclaration(declaration);
			reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			AssertEquals(ZString.Empty, reconDeclaration.ReconEntryNumberWithEntryFilerCode);

			new ReconMessageManager(new ReconDeclarationIReconciliation(reconDeclaration), UpdateActionCode.Add).PopulateMessage();
			Factory.Save();
			AssertEquals("entry number is generated and shows the full entry filer + entry number", 11, reconDeclaration.ReconEntryNumberWithEntryFilerCode.Length);
		}

		public void TestFormalEntryNumber()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(100212);

			var existingEntryNumber = GetEntryNumber("0123456");
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.US_EntryFilerCode = "XJ5";

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = existingEntryNumber;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();

			AssertEquals("entry1.EntryNumber", existingEntryNumber, entry1.EntryNumber);
			AssertEquals("entry2.EntryNumber", GetEntryNumber("0100212"), entry2.EntryNumber);

			companyStmNums.SetNextNumber(123456);
			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.US_EntryFilerCode = "XJ5";
			var entry3 = declaration2.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			AssertEquals("entry3.EntryNumber", GetEntryNumber("0123457"), entry3.EntryNumber);
		}

		public void TestFormalEntryNumber_BranchLevel()
		{
			var branchStmNums = DeclarationTestHelper.SetupBranchSpecificFormalEntryNumber("XJ5");
			long nextBranchNumber = (long)(branchStmNums.SN_MinimumValue + 10);
			branchStmNums.SetNextNumber(nextBranchNumber);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			AssertEquals("entry1.EntryNumber", GetEntryNumber((branchStmNums.SN_MinimumValue + 10).ToString().PadLeft(7, '0')), entry1.EntryNumber);

			branchStmNums.SetNextNumber((long)(branchStmNums.SN_MinimumValue + 12));
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			AssertEquals("entry2.EntryNumber", GetEntryNumber((branchStmNums.SN_MinimumValue + 12).ToString().PadLeft(7, '0')), entry2.EntryNumber);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.ImportEntryNumber = "12345670";
			var entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entry3Line = entry3.MergedLines.AddNew();
			invoiceLine.JI_CL = entry3Line.PK;
			Factory.Save();
			AssertEquals("entry3.EntryNumber", "12345670", entry3.EntryNumber);
		}

		public void TestFormalEntryNumber_SetFromPreAllocatedNumber()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(100212);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.ImportEntryNumber = "0123456789";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			Factory.Save();

			AssertEquals("EntryNumber", "0123456789", entry.EntryNumber);
		}

		public void TestProcessingDistrictPortCode()
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "8888");

			GlbBranch anotherBranch = Factory.New<GlbBranch>();
			anotherBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			anotherBranch.GB_Code = "~12";
			anotherBranch.GB_RL_NKHomePort = "USLAX";
			Factory.Save();

			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, anotherBranch.PK.ToGuid(), Guid.Empty, "1111");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = anotherBranch.PK;
			var jobheader = new JobHeader.Loader(declaration).TryCreate();
			jobheader.JH_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("ProcessingDistrictPortCode", "1111", declaration.ProcessingDistrictPort);

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("ProcessingDistrictPortCode", "1111", entry.ProcessingDistrictPort);

			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("ProcessingDistrictPortCode", "8888", declaration.ProcessingDistrictPort);
			AssertEquals("ProcessingDistrictPortCode", "8888", entry.ProcessingDistrictPort);
		}

		public void TestPaymentRelatedFields()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_ClientBranchDesignation = "A";
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.BrettsBirthday;
			declaration.US_PeriodicStatementMM = "01";

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, entry.US_PaymentType);
			AssertEquals("A", entry.US_ClientBranchDesignation);
			AssertEquals(ZDateTime.BrettsBirthday, entry.US_PreliminaryStatementPrintDate);
			AssertEquals("PeriodicStatementMonth", "01", entry.US_PeriodicStatementMM);

			ICusEntryHeader entryHeaderI = entry;
			AssertEquals(PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate, entryHeaderI.PaymentTypeIndicator.ToString());
			AssertEquals("A", entryHeaderI.ClientBranchDesignation);
			AssertEquals(ZDateTime.BrettsBirthday, entryHeaderI.PreliminaryStatementPrintDate);
			AssertEquals("PeriodicStatementMonth", "01", entryHeaderI.PeriodicStatementMonth);
		}

		/// <summary>
		/// Tobacco Excise Tax
		/// </summary>
		[TestDate(2009, 6, 1)]
		public void TestTotalAmountPayableAndExciseTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.US_SPI = "";//for the sake of this test. tested and passed
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;

			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Total Duty Amount", 3275.4m, entry.TotalDutyAmount);
			AssertEquals("Total payble includes everything (even excise tax)", 5134.9m, entry.TotalAmountPayable);
		}

		[TestDate(2009, 6, 1)]
		public void TestTotalAmountPayableForWarehouseEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;

			AssertEquals("Total MPF Fee", 31.5m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Total HMF Fee", 18.75m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Total Duty Amount", 3275.4m, entry.TotalDutyAmount);
			AssertEquals("Total payble for warehouse entry should only include HMF is applicable", 18.75m, entry.TotalAmountPayable);

			AssertEquals("TotalAmountForBondCalculationUse should return all values that would be payable if not a warehouse entry", 5153.65m, entry.TotalAmountForBondCalculationUse);
		}

		[TestDate(2014, 6, 5)]
		public void TestTotalAmountPayableForReWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 15000m;

			invoiceLine.JI_Tariff = "2402103030";
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 1360m;

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			var entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total MPF Fee", 51.96m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals("Total HMF Fee", 18.75m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.HMF));
			AssertEquals("Total Excise Fee", 1828m, entry.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.Tobacco));
			AssertEquals("Total Duty Amount", 3275.40m, entry.TotalDutyAmount);
			AssertEquals("Total payble for re-warehouse entry should include nothing. HMF is already paid on at the time of 21", 0m, entry.TotalAmountPayable);
			AssertEquals("TotalAmountForBondCalculationUse should return all values that would be payable if not a rewarehouse entry", 5174.11m, entry.TotalAmountForBondCalculationUse);
		}

		[TestDate(2008, 3, 25)]
		public void TestAccountingIntegration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201.90.1000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("EntryChargeTypeList", typeof(Registry.Business.Customs.US.EntryChargeTypeList), entry.EntryChargeTypeList.GetType());

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals(true, entry.IsFeePaidByBroker("", "", null));

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			AssertEquals(false, entry.IsFeePaidByBroker("", "", null));

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(entry);

			CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);
			AssertEquals("three charges expected", 3, charges.Length);
			var dutyCharge = charges.First(x => x.Description.Contains("Duty"));
			AssertEquals("Duty autorated", 45m, dutyCharge.Amount);
			AssertEquals("Duty is paid by broker", true, dutyCharge.IsPaidByBroker);
			var charge499 = charges.First(x => x.Description.Contains("499"));
			AssertEquals("MPF autorated: takes Entry's one as there is a minimum and maximum", 25m, charge499.Amount);
			AssertEquals("MPF is paid by broker", true, charge499.IsPaidByBroker);
			var charge501 = charges.First(x => x.Description.Contains("501"));
			AssertEquals("HMF autorated: takes Entry's one", 3.75m, charge501.Amount);
			AssertEquals("HMF is paid by broker", true, charge501.IsPaidByBroker);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			charges = customsChargeGetter.GetCustomsCharges(null);
			AssertEquals("One charge expected", 1, charges.Length);
			AssertEquals("PreCondition: the only amount is HMF", 3.75m, charges[0].Amount);
			AssertEquals("HMF should be paid by brokers", true, charges[0].IsPaidByBroker);
		}

		[TestDate(2008, 3, 25)]
		public void TestAccountingIntegrationForTax()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(declaration.CustomsEntryHeaders[0]);
			CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);

			AssertEquals("two charges expected", 2, charges.Length);

			AssertEquals("PreCondition:First amount is MPF", 25m, charges[0].Amount);
			AssertEquals("MPF should be paid by brokers", true, charges[0].IsPaidByBroker);
			AssertEquals("PreCondition:Second amount is Tax", 2300.84m, charges[1].Amount);
			AssertEquals("Tax should be paid by brokers:Not deferred", true, charges[1].IsPaidByBroker);
			AssertEquals("Excise Tax Payable", charges[1].Description);

			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			charges = customsChargeGetter.GetCustomsCharges(null);

			AssertEquals("two charges expected", 2, charges.Length);

			AssertEquals("PreCondition:First amount is MPF", 25m, charges[0].Amount);
			AssertEquals("MPF should be paid by brokers", true, charges[0].IsPaidByBroker);
			AssertEquals("PreCondition:Second amount is Tax", 2300.84m, charges[1].Amount);
			AssertEquals("Tax should be paid by brokers", false, charges[1].IsPaidByBroker);
			AssertEquals("Excise Tax Deferred", charges[1].Description);
		}

		[TestDate(2009, 6, 1)]
		public void TestAccountingIntegrationForEntryType21WithTax()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(declaration.CustomsEntryHeaders[0]);
			CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);

			AssertEquals("One charge expected", 1, charges.Length);

			AssertEquals("PreCondition:First amount is HMF", 12.5m, charges[0].Amount);
			AssertEquals("HMF should be paid by brokers", true, charges[0].IsPaidByBroker);
		}

		public void TestRepopulateEntrySubmittedDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			declaration.ActiveEntryHeaders.EntrySummaryEntry.PopulateEntrySubmittedDateIfRequired();

			Assert(declaration.JE_EntrySubmittedDate.IsValid);
			Assert(declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_EntrySubmittedDate.IsValid);

			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			var invoice = reconDeclaration.OriginalEntries[0].Invoice;
			var reconInvoiceLine = invoice.InvoiceLines.AddNew();
			var entry = reconDeclaration.ReconEntry.GetEntry();
			entry.PopulateEntrySubmittedDateIfRequired();
			Assert(reconDeclaration.ReconWrappedJobDeclaration.JE_EntrySubmittedDate.IsValid);
			Assert(entry.CH_EntrySubmittedDate.IsValid);
		}

		public void TestRepopulateEntrySubmittedDateForCargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			declaration.ActiveEntryHeaders.CargoReleaseEntry.PopulateEntrySubmittedDateIfRequired();

			Assert("This should be populated only when formal entries are sent", !declaration.JE_EntrySubmittedDate.IsValid);
			Assert(declaration.ActiveEntryHeaders.CargoReleaseEntry.CH_EntrySubmittedDate.IsValid);
		}

		public void TestRepopulateEntrySubmittedDateForExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			declaration.ActiveEntryHeaders[0].PopulateEntrySubmittedDateIfRequired();

			Assert(declaration.JE_EntrySubmittedDate.IsValid);
			Assert(declaration.ActiveEntryHeaders[0].CH_EntrySubmittedDate.IsValid);
		}

		public void TestDoNotRepopulateEntrySubmittedDateWhenExists()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			declaration.JE_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;

			declaration.ActiveEntryHeaders.EntrySummaryEntry.PopulateEntrySubmittedDateIfRequired();

			AssertEquals(ZDateTime.BrettsBirthday, declaration.JE_EntrySubmittedDate);
			AssertEquals(ZDateTime.BrettsBirthday, declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_EntrySubmittedDate);
		}

		public void TestCurrencyConverterForUSExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportDate = new ZDateTime(2005, 8, 1);
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew().JI_LinePrice = 800m;
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var merger = new LineMerger(declaration);
			merger.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders[0];
			AssertEquals("CurrencyConverter.DateForRate", new ZDateTime(2005, 8, 1), entryHeader.CurrencyConverter.DateForRate);
		}

		public void TestGetBusinessLayerNotificationsToAddToWrapper()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			var entry = Factory.New<CusEntryHeaderForTesting>();
			entry.GetNewValidationReturns = new CusEntryHeaderValidationForTest(entry);
			entry.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(entry);

			entry.CH_EntryStatus = "A";

			IEnumerable<INotification> notifications = ((IMessageAttacheeInDeclaration)entry).GetBusinessLayerNotificationsToAddToWrapper();
			ZString result = notifications.GetWarnings().ToUniqueMessageListString();
			AssertEquals("should have a warning", true, result.Contains("AAAAA_BBBBB"));
		}

		public void TestICusEntryHeaderEntryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine1.PK, invoiceLine2.JI_ParentID);

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = invoiceLine3.PK;

			JobComInvoiceLine invoiceLine5 = invoiceLine2.AddSecondaryInvoiceLine();
			AssertEquals("PreCondition:JI_ParentID is set", invoiceLine2.PK, invoiceLine5.JI_ParentID);
			Assert(invoiceLine5.IsSetVLine);

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders[0];

			List<ICusEntryLine> entryLines = new List<ICusEntryLine>(((ICusEntryHeader)entry).EntryLines);
			AssertEquals("Three entry lines are expected", 3, entryLines.Count);
			AssertEquals("entryLine1 is contained", true, entryLines.Contains(invoiceLine1.CusEntryLine));
			AssertEquals("entryLine2 is contained", true, entryLines.Contains(invoiceLine2.CusEntryLine));
			AssertEquals("entryLine3 is contained", true, entryLines.Contains(invoiceLine3.CusEntryLine));
			AssertEquals("entryLine4 is not contained", false, entryLines.Contains(invoiceLine4.CusEntryLine));
			AssertEquals("entryLine5 is not contained", false, entryLines.Contains(invoiceLine5.CusEntryLine));
		}

		public void TestRelatedEntries()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.CH_CH_PrimeEntry = ensEntry.PK;

			var inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;

			var bcrEntry = declaration.CustomsEntryHeaders.AddNew();
			bcrEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			bcrEntry.CH_CH_PrimeEntry = ensEntry.PK;

			AssertEquals("RelatedCRLEntry", crlEntry, ensEntry.RelatedCRLEntry);
			AssertEquals("IsRelatedTo", true, ensEntry.IsRelatedTo(crlEntry));
			AssertEquals("RelatedENSEntry", ensEntry, crlEntry.RelatedENSEntry);
			AssertEquals("IsRelatedTo", true, crlEntry.IsRelatedTo(ensEntry));

			AssertEquals("RelatedENSEntry", null, inbEntry.RelatedENSEntry);
			AssertEquals("RelatedCRLEntry", null, inbEntry.RelatedCRLEntry);
			AssertEquals("IsRelatedTo", false, inbEntry.IsRelatedTo(ensEntry));
			AssertEquals("IsRelatedTo", false, inbEntry.IsRelatedTo(crlEntry));

			AssertEquals("RelatedBCREntry", bcrEntry, ensEntry.RelatedBCREntry);
			AssertEquals("IsRelatedTo", true, ensEntry.IsRelatedTo(bcrEntry));
			AssertEquals("RelatedENSEntry", ensEntry, bcrEntry.RelatedENSEntry);
			AssertEquals("IsRelatedTo", true, bcrEntry.IsRelatedTo(ensEntry));

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var aceCrEntry = declaration.CustomsEntryHeaders.AddNew();
			aceCrEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			aceCrEntry.CH_CH_PrimeEntry = ensEntry.PK;
			AssertEquals("RelatedCRLEntry", aceCrEntry, ensEntry.RelatedCRLEntry);
			AssertEquals("IsRelatedTo", true, ensEntry.IsRelatedTo(aceCrEntry));
		}

		public void TestEffectiveHasCargoReleaseCertified()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			CusEntryHeader crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.CH_CH_PrimeEntry = ensEntry.PK;

			AssertEquals("HasCargoReleaseCertified", false, crlEntry.EffectiveHasCargoReleaseBeenCertified);
			AssertEquals("HasCargoReleaseCertified", false, ensEntry.EffectiveHasCargoReleaseBeenCertified);

			ensEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals("EffectiveHasCargoReleaseBeenCertified", true, crlEntry.EffectiveHasCargoReleaseBeenCertified);
			AssertEquals("EffectiveHasCargoReleaseBeenCertified", true, ensEntry.EffectiveHasCargoReleaseBeenCertified);

			ensEntry.US_CRLCertStatus = "";
			AssertEquals("HasCargoReleaseCertified", false, crlEntry.EffectiveHasCargoReleaseBeenCertified);
			AssertEquals("HasCargoReleaseCertified", false, ensEntry.EffectiveHasCargoReleaseBeenCertified);

			crlEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals("EffectiveHasCargoReleaseBeenCertified", true, crlEntry.EffectiveHasCargoReleaseBeenCertified);
			AssertEquals("EffectiveHasCargoReleaseBeenCertified", true, ensEntry.EffectiveHasCargoReleaseBeenCertified);
		}

		public void TestMergeWithoutInvoiceLinesForInBond()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableINB = true;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;

			declaration.DoMerge();
			AssertEquals("One inbond entry is created", 1, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			declaration.DoMerge();
			AssertEquals(entry, declaration.CustomsEntryHeaders[0]);
		}

		public void TestHasActiveMessages()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(false, entry.HasActiveTransactionsWithCustoms);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			AssertEquals(true, entry.HasActiveTransactionsWithCustoms);
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			AssertEquals(true, entry.HasActiveTransactionsWithCustoms);
		}

		public void TestCanSendOriginal()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals("CanSendOriginal", true, entry.CanSendOriginal);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			AssertEquals("CanSendOriginal while it is waiting for response", true, entry.CanSendOriginal);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals(false, entry.CanSendOriginal);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("CanSendOriginal", true, entry.CanSendOriginal);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("CanSendOriginal", true, entry.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals("CanSendWithdrawal", false, entry.CanSendWithdrawal);

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			AssertEquals("CanSendWithdrawal", false, entry.CanSendWithdrawal);
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			Factory.Save();
			AssertEquals("CanSendWithdrawal", true, entry.CanSendWithdrawal);
		}

		public void TestIsRelevantFor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));

			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.BorderCargoRelease));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.CargoRelease));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.EntrySummary));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondDeparture));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.InBondUpdate));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.Export));
			AssertEquals(false, entry.IsRelevantFor(ImportMessageStatusList.MessageType.ACECargoRelease));
			AssertEquals(true, entry.IsRelevantFor(ImportMessageStatusList.MessageType.TemporaryImportationBond));
		}

		public void TestDeactivatedEntriesDeletedIfThereIsNoTransactions()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			AssertEquals("PreCondition:No transactions with customs", false, entry.HasTransactionsWithCustoms);
			entry.IsActive = false;
			AssertEquals("entry is deleted", true, entry.IsDeleted);
		}

		public void TestHasTransactionsWithCustoms()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			AssertEquals("No transactions with customs", false, entry.HasTransactionsWithCustoms);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete;
			Factory.Save();
			AssertEquals("Has transactions with customs", true, entry.HasTransactionsWithCustoms);
			AssertEquals("IsWaitingForResponse", true, entry.IsWaitingForResponse);
			AssertEquals("HasBeenWithdrawn", false, entry.HasBeenWithdrawn);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			AssertEquals("Has transactions with customs", true, entry.HasTransactionsWithCustoms);
			AssertEquals("IsWaitingForResponse", false, entry.IsWaitingForResponse);
			AssertEquals("HasBeenWithdrawn", false, entry.HasBeenWithdrawn);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryDelete;
			Factory.Save();
			AssertEquals("Has transactions with customs", true, entry.HasTransactionsWithCustoms);
			AssertEquals("IsWaitingForResponse", false, entry.IsWaitingForResponse);
			AssertEquals("HasBeenWithdrawn", true, entry.HasBeenWithdrawn);
		}

		public void TestIsCurrentlyWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("No transactions yet with customs", false, entry.IsCurrentlyWithdrawn);

			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			AssertEquals("Original cleared", false, entry.IsCurrentlyWithdrawn);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			Factory.Save();
			AssertEquals("Replacement Cleared", false, entry.IsCurrentlyWithdrawn);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingDeleteResponse;
			Factory.Save();
			AssertEquals("Waiting on Withdrawal", false, entry.IsCurrentlyWithdrawn);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			Factory.Save();
			AssertEquals("HasBeenWithdrawn", true, entry.IsCurrentlyWithdrawn);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			Factory.Save();
			AssertEquals("Entry sent again as new SED", false, entry.IsCurrentlyWithdrawn);
		}

		public void TestSettingCH_StatusSetNotRequireToBeReportToCustomsForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.US_ShouldBeReportToCustoms = true;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals("IsWaitingForResponse", false, entry.IsWaitingForResponse);
			AssertEquals("US_ShouldBeReportToCustoms", true, entry.US_ShouldBeReportToCustoms);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			AssertEquals("IsWaitingForResponse", true, entry.IsWaitingForResponse);
			AssertEquals("US_ShouldBeReportToCustoms", false, entry.US_ShouldBeReportToCustoms);
		}

		public void TestSettingCH_StatusSetNotRequireToBeReportToCustomsForImport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.US_ShouldBeReportToCustoms = true;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			AssertEquals("IsWaitingForResponse", false, entry.IsWaitingForResponse);
			AssertEquals("US_ShouldBeReportToCustoms", true, entry.US_ShouldBeReportToCustoms);
			entry.CH_Status = ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal;
			AssertEquals("SetAsNotRequireToBeReportToCustoms should not have been called for non-export", true, entry.US_ShouldBeReportToCustoms);
		}

		[TestDate(2010, 07, 21, 14, 54, 33)]
		public void TestBGMReferenceSuffixGeneration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592NYK2010";

			for (int i = 0; i < 15; i++)    //create 15 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			CusEntryHeader entry6 = declaration.ActiveEntryHeaders[5];
			CusEntryHeader entry10 = declaration.ActiveEntryHeaders[9];
			CusEntryHeader entry11 = declaration.ActiveEntryHeaders[10];
			CusEntryHeader entry12 = declaration.ActiveEntryHeaders[11];
			CusEntryHeader entry13 = declaration.ActiveEntryHeaders[12];
			CusEntryHeader entry15 = declaration.ActiveEntryHeaders[14];

			AssertEquals("entry1 CH_BGMReference", "B000017592NYK2010", entry1.CH_BGMReference);
			AssertEquals("entry6 CH_BGMReference", "E0010072114543304", entry6.CH_BGMReference);
			AssertEquals("entry10 CH_BGMReference", "E0010072114543308", entry10.CH_BGMReference);
			AssertEquals("entry11 CH_BGMReference should generate into Alpa suffix range", "E0010072114543309", entry11.CH_BGMReference);
			AssertEquals("entry12 CH_BGMReference should generate into Alpa suffix range", "E001007211454330A", entry12.CH_BGMReference);
			AssertEquals("entry13 CH_BGMReference should generate into Alpa suffix range", "E001007211454330B", entry13.CH_BGMReference);
			AssertEquals("entry15 CH_BGMReference should generate into Alpa suffix range", "E001007211454330D", entry15.CH_BGMReference);
		}

		[TestDate(2010, 07, 21, 14, 54, 33)]
		public void TestBGMReferenceSuffixGenerationWithStdJob()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B000017592";

			for (int i = 0; i < 15; i++)    //create 15 entries
			{
				JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				JobComInvoiceLine line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			Factory.Save();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader entry1 = declaration.ActiveEntryHeaders[0];
			CusEntryHeader entry6 = declaration.ActiveEntryHeaders[5];
			CusEntryHeader entry10 = declaration.ActiveEntryHeaders[9];
			CusEntryHeader entry11 = declaration.ActiveEntryHeaders[10];
			CusEntryHeader entry12 = declaration.ActiveEntryHeaders[11];
			CusEntryHeader entry13 = declaration.ActiveEntryHeaders[12];
			CusEntryHeader entry15 = declaration.ActiveEntryHeaders[14];

			AssertEquals("entry1 CH_BGMReference", "B000017592", entry1.CH_BGMReference);
			AssertEquals("entry6 CH_BGMReference", "B00001759205", entry6.CH_BGMReference);
			AssertEquals("entry10 CH_BGMReference", "B00001759209", entry10.CH_BGMReference);
			AssertEquals("entry11 CH_BGMReference should generate into Alpa suffix range", "B0000175920A", entry11.CH_BGMReference);
			AssertEquals("entry12 CH_BGMReference should generate into Alpa suffix range", "B0000175920B", entry12.CH_BGMReference);
			AssertEquals("entry13 CH_BGMReference should generate into Alpa suffix range", "B0000175920C", entry13.CH_BGMReference);
			AssertEquals("entry15 CH_BGMReference should generate into Alpa suffix range", "B0000175920E", entry15.CH_BGMReference);
		}

		public void TestCH_BGM_ReferenceMaxLength()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var aestirEntry = exportDeclaration.CustomsEntryHeaders.AddNew();
			aestirEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("CH_BGMReferenceInfo.MaxLength for Export job with AESTIR messaging must be 17", 17, aestirEntry.CH_BGMReferenceInfo.MaxLength);

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			importDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var importEntry = exportDeclaration.CustomsEntryHeaders.AddNew();
			importEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("CH_BGMReferenceInfo.MaxLength for Import job should be standard base value (35)", 35, importEntry.CH_BGMReferenceInfo.MaxLength);
		}

		public void TestCH_BGM_ReferenceReadOnly()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var aestirEntry = exportDeclaration.CustomsEntryHeaders.AddNew();
			aestirEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			aestirEntry.US_SendReplace = false;
			AssertEquals("CH_BGMReferenceInfo.ReadOnly should be true if US_SendReplace is false", true, aestirEntry.CH_BGMReferenceInfo.ReadOnly);

			aestirEntry.US_SendReplace = true;
			AssertEquals("CH_BGMReferenceInfo.ReadOnly should be false if US_SendReplace is true", false, aestirEntry.CH_BGMReferenceInfo.ReadOnly);
		}

		public void TestUS_SendReplace_ReadOnly()
		{
			var exportDeclaration = Factory.New<JobDeclaration>();
			exportDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var aestirEntry = exportDeclaration.CustomsEntryHeaders.AddNew();
			aestirEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("Pre-condition: No transactions with customs", false, aestirEntry.HasTransactionsWithCustoms);
			AssertEquals("US_SendReplace_ReadOnly should be false when no transactions have been sent to Customs", false, aestirEntry.US_SendReplaceInfo.ReadOnly);

			aestirEntry.CH_Status = AESDirectCustomsEntryStatus.Codes.Error;
			AssertEquals("Pre-condition: No transactions with customs - Transactions must be cleared types", false, aestirEntry.HasTransactionsWithCustoms);
			AssertEquals("US_SendReplace_ReadOnly should be false when no transactions have been sent to Customs", false, aestirEntry.US_SendReplaceInfo.ReadOnly);

			aestirEntry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			AssertEquals("Pre-condition: Has transactions with customs", true, aestirEntry.HasTransactionsWithCustoms);
			AssertEquals("US_SendReplace_ReadOnly should be true when transactions have been sent to Customs", true, aestirEntry.US_SendReplaceInfo.ReadOnly);
		}

		public void TestSEDString()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var generator = new AESInputBlockControlGenerator(entry);
			generator.AddMessageBlocks(new AESTIRMessageBlockBuilder(entry).Build(UpdateActionCode.Add));
			AssertMultilineASCIIEquals("SEDString", generator.Serialise(true), entry.SEDString);
		}

		[TestDate(2010, 07, 19, 15, 35, 49)]
		public void TestSEDStringOnResendAfterWithdrawal()
		{
			JobDeclaration declarationAESTIR = Factory.New<JobDeclaration>();
			declarationAESTIR.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationAESTIR.JE_DeclarationReference = "S00005000LAX2010";
			CusEntryHeader entryAESTIR = declarationAESTIR.CustomsEntryHeaders.AddNew();
			entryAESTIR.CH_BGMReference = "E0010071915354900";
			entryAESTIR.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entryAESTIR.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			Factory.Save();
			AESInputBlockControlGenerator generator = new AESInputBlockControlGenerator(entryAESTIR);
			AssertContains("SEDString", "Shipment Reference Number (15-31)                :E0010071915354900", entryAESTIR.SEDString);
			AssertContains("SEDString", "Shipment Filing Action Request Indicator (32-32) :A", entryAESTIR.SEDString);

			generator.AddMessageBlocks(new AESTIRMessageBlockBuilder(entryAESTIR).Build(UpdateActionCode.Add));
			AssertMultilineASCIIEquals("SEDString", generator.Serialise(true), entryAESTIR.SEDString);
		}

		[TestDate(2010, 07, 19, 15, 35, 49)]
		public void TestSEDStringOnResendAfterWithdrawalWithStdJobNumber()
		{
			var declarationAESTIR = Factory.New<JobDeclaration>();
			declarationAESTIR.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationAESTIR.JE_DeclarationReference = "S00005000";
			var entryAESTIR = declarationAESTIR.CustomsEntryHeaders.AddNew();
			entryAESTIR.CH_BGMReference = "E0010071915354900";  // As same reference cannot be used for re-sending after a cancel, the BGM Reference will generate using the Brokerid & DateTime format
			entryAESTIR.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entryAESTIR.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			Factory.Save();
			var generator = new AESInputBlockControlGenerator(entryAESTIR);
			AssertContains("SEDString", "Shipment Reference Number (15-31)                :E0010071915354900", entryAESTIR.SEDString);
			AssertContains("SEDString", "Shipment Filing Action Request Indicator (32-32) :A", entryAESTIR.SEDString);

			generator.AddMessageBlocks(new AESTIRMessageBlockBuilder(entryAESTIR).Build(UpdateActionCode.Add));
			AssertMultilineASCIIEquals("SEDString", generator.Serialise(true), entryAESTIR.SEDString);
		}

		public void TestUS_SendWithdrawn()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.US_SendWithdrawn = true;
			AssertEquals(false, entry.US_SendWithdrawn);
			AssertEquals(true, entry.US_SendWithdrawnInfo.ReadOnly);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			Factory.Save();

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			entry.US_SendWithdrawn = true;
			AssertEquals(true, entry.US_SendWithdrawn);
			AssertEquals(false, entry.US_SendWithdrawnInfo.ReadOnly);
		}

		public void TestSEDStringOnResendAfterDeletionOfInvoiceThatWasSent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_DeclarationReference = "B00001759";

			for (int i = 0; i < 3; i++) //create 3 entries
			{
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "Inv-" + i.ToString();
				invoice.US_ImportEntryNo = invoice.JZ_InvoiceNumber;  // make merge key different
				invoice.JZ_CU_RelatedHouseBill = ZGuid.Empty;
				var line = invoice.InvoiceLines.AddNew();
				line.US_MarksAndNumbers = invoice.JZ_InvoiceNumber;
			}

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			AssertEquals("Should be 3 Entry's on this declaration", 3, declaration.ActiveEntryHeaders.Count);

			var entry1 = declaration.ActiveEntryHeaders[0];
			var entry2 = declaration.ActiveEntryHeaders[1];
			var entry3 = declaration.ActiveEntryHeaders[2];

			AssertEquals("entry1 CH_BGMReference", "B00001759", entry1.CH_BGMReference);
			AssertEquals("entry2 CH_BGMReference", "B0000175901", entry2.CH_BGMReference);
			AssertEquals("entry3 CH_BGMReference", "B0000175902", entry3.CH_BGMReference);

			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry3.CH_Status = AESDirectCustomsEntryStatus.Codes.DeleteSEDClear;

			declaration.Invoices[2].Delete();
			declaration.Invoices[0].Delete();
			AssertEquals("Should only be 1 Invoice now on this declaration", 1, declaration.Invoices.Count);
			AssertEquals("Invoice remaining should be Invoice 2", "INV-1", declaration.Invoices[0].JZ_InvoiceNumber);

			var invoice4 = declaration.Invoices.AddNew();
			invoice4.JZ_InvoiceNumber = "Inv-4";
			invoice4.US_ImportEntryNo = invoice4.JZ_InvoiceNumber;  // make merge key different
			var invoice4line = invoice4.InvoiceLines.AddNew();
			invoice4line.US_MarksAndNumbers = invoice4.JZ_InvoiceNumber;

			declaration.DoMerge();
			AssertEquals("Should now be 2 Entry's again on this declaration", 2, declaration.ActiveEntryHeaders.Count);

			CusEntryHeader entry4;
			if (declaration.ActiveEntryHeaders[0].RandomHeader.JZ_InvoiceNumber == "INV-1")
			{
				entry2 = declaration.ActiveEntryHeaders[0];
				entry4 = declaration.ActiveEntryHeaders[1];
			}
			else
			{
				entry4 = declaration.ActiveEntryHeaders[0];
				entry2 = declaration.ActiveEntryHeaders[1];
			}

			AssertEquals("entry2 CH_BGMReference should not be changed", "B0000175901", entry2.CH_BGMReference);
			AssertEquals("entry4 CH_BGMReference should pick up from Last Saved BGM reference, not re-use deleted invoice references", "B0000175903", entry4.CH_BGMReference);
		}

		public void TestIAESTIRMessageAttacheeMembers()
		{
			var helper = new DeclarationTestHelper(Factory);
			var supplier = helper.CreateOrganisation("SUPPLIER", "USLAX");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "126597687", Core.Constants.CountryCodes.UnitedStates);

			var importer = helper.CreateOrganisation("IMPORTER", "AUSYD");
			importer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "96532144875", Core.Constants.CountryCodes.UnitedStates);

			var forwarder = helper.CreateOrganisation("FORWARDER", "PRGUY");
			forwarder.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "63265985468", Core.Constants.CountryCodes.UnitedStates);

			var intermConsignee = helper.CreateOrganisation("INTERM CONSIGNEE", "AUMEL");
			intermConsignee.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "326569469", Core.Constants.CountryCodes.UnitedStates);

			var shippingLine = helper.CreateOrganisation("SHIPPING LINE", "NZAKL");
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AACP", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_Consignee = intermConsignee.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_VesselName = "APL VESSEL TESTING";
			declaration.JE_VoyageFlightNo = "E123";
			declaration.JE_MasterBill = "MB5698463112";
			declaration.JE_HouseBill = "HB5698463112";
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalWeight = 150m;
			declaration.JE_TotalVolume = 13m;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
			declaration.US_StateOfOrigin = USStatesList.Codes.California;
			declaration.JE_ExportDate = new ZDateTime(2009, 12, 13);
			declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			declaration.US_ImportEntryNo = "56846684";
			declaration.US_ForeignTradeZone = "FZX23";
			declaration.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			declaration.US_TransportReference = "TRNREF23423";
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;
			declaration.US_FirstPortOfCallCity = "SYDNEY";
			declaration.US_RN_NKFirstPortOfCallCountry = Core.Constants.CountryCodes.Australia;
			declaration.US_SchDLoading = DeclarationTestHelper.USLAXScheduleDOrK;
			declaration.US_SchDExport = DeclarationTestHelper.USCHIScheduleDOrK;
			declaration.US_SchDArrival = DeclarationTestHelper.AUSYDScheduleDOrK;
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX432890";
			container1.CO_Seal = "SL23";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX968546";
			container2.CO_Seal = "SL69";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoice.US_UltimateConsigneeType = UltimateConsigneeTypeList.Codes.GovernmentEntity;

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 145m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 46m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			invoiceLine1.US_DDTCITARExemptionNo = "123.11B";
			invoiceLine1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCQuantity = 142m;
			invoiceLine1.US_DDTCRegistrationNo = "REG123";
			invoiceLine1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.BulletsRounds;
			invoiceLine1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ClassifiedArticlesTechnicalDataAndDefenseServicesNotOtherwiseEnumerated;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2020202020";
			invoiceLine2.JI_Description = "DESCRIPTION 2";
			invoiceLine2.US_ExportCode = ExportInformationCodeList.Codes.CH;
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine2.US_LicenseNo = "LCN5678";
			invoiceLine2.JI_Weight = 250m;
			invoiceLine2.JI_CustomsQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Packs;
			invoiceLine2.JI_CustomsSecondQuantity = 60m;
			invoiceLine2.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pieces;
			invoiceLine2.US_ECCN = "EC34";
			invoiceLine2.JI_LinePrice = 2600m;
			invoiceLine2.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			invoiceLine2.US_IsUsedVehicle = true;
			invoiceLine2.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine2.US_VehicleID = "VID12";
			invoiceLine2.US_VehicleTitleNo = "VTitleNo";
			invoiceLine2.US_VehicleTitleState = "MA";

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			var entry = declaration.ActiveEntryHeaders[0];
			IAESTIRMessageAttachee attachee = entry;
			IAESTIRTransportationDetail transportDetail = entry;

			AssertEquals("RelatedCompanyIndicator", YesNoDefaultList.Codes.Yes, attachee.RelatedCompanyIndicator);
			AssertEquals("ModeOfTransportationCodeMOT", "11", attachee.ModeOfTransportationCodeMOT);
			AssertEquals("CountryOfUltimateDestinationCode", "AU", attachee.CountryOfUltimateDestinationCode);
			AssertEquals("USStateOfOriginCode", "CA", attachee.USStateOfOriginCode);
			AssertEquals("CarrierIDSCACIATA", "AACP", attachee.CarrierIDSCACIATA);
			AssertEquals("ShipmentReferenceNumber", entry.CH_BGMReference, attachee.ShipmentReferenceNumber);
			AssertEquals("ConveyanceNameCarrierName", "APL VESSEL TESTING", attachee.ConveyanceNameCarrierName);
			AssertEquals("FilingOptionIndicator", AESCommodityFilingOptionList.Codes._2Predeparture, attachee.FilingOptionIndicator);
			AssertEquals("AEIFilingType", ZString.Empty, attachee.AEIFilingType);
			AssertEquals("PortOfUnladingCode", DeclarationTestHelper.AUSYDScheduleDOrK, attachee.PortOfUnladingCode);
			AssertEquals("PortOfExportationCode", DeclarationTestHelper.USCHIScheduleDOrK, attachee.PortOfExportationCode);
			AssertEquals("EstimatedDateOfExport", new ZDate(2009, 12, 13), attachee.EstimatedDateOfExport);
			AssertEquals("HazardousMaterialIndicatorHAZMAT", YesNoDefaultList.Codes.Yes, attachee.HazardousMaterialIndicatorHAZMAT);
			AssertEquals("InbondCode", InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, attachee.InbondCode);
			AssertEquals("EntryNumber", "56846684", attachee.EntryNumber);
			AssertEquals("ForeignTradeZoneIdentifier", "FZX23", attachee.ForeignTradeZoneIdentifier);
			AssertEquals("RoutedExportTransactionIndicator", YesNoDefaultList.Codes.Yes, attachee.RoutedExportTransactionIndicator);
			var transportationDetails = new List<IAESTIRTransportationDetail>(attachee.TransportationDetails);
			AssertEquals("TransportationDetails", 3, transportationDetails.Count);
			AssertEquals(transportDetail, transportationDetails[0]);
			AssertTransportDetail(transportationDetails[0], "", "", "TRNREF23423");
			AssertTransportDetail(transportationDetails[1], "CRUX432890", "SL23", "");
			AssertTransportDetail(transportationDetails[2], "CRUX968546", "SL69", "");

			AssertAESParty(attachee.USPPI, invoice.Supplier, invoice.USPPIDocAddress, "12659768700", AESConstants.IDTypes.EmployerIdentificationNumber, "CITY", USStatesList.Codes.California, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("USPPIIRSNumber", "", attachee.USPPIIRSNumber);
			AssertEquals("USPPIIRSIDType", "", attachee.USPPIIRSIDType);
			AssertAESParty(attachee.ForwardingAgent, declaration.Forwarder, null, "63265985468", AESConstants.IDTypes.DUNS, "CITY", Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertAESParty(attachee.UltimateConsignee, invoice.Importer, invoice.UltimateConsigneeDocAddress, "96532144875", AESConstants.IDTypes.DUNS, "CITY", "", Core.Constants.CountryCodes.Australia);
			AssertEquals("Ultimate Consignee Type", UltimateConsigneeTypeList.Codes.GovernmentEntity, attachee.UltimateConsigneeType);
			AssertEquals("IsSoldEnRoute", ZBool.False, attachee.IsSoldEnRoute);
			AssertEquals("CityOfFirstPortOfCall", "SYDNEY", attachee.CityOfFirstPortOfCall);
			AssertEquals("CountryOfFirstPortOfCall", "AU", attachee.CountryOfFirstPortOfCall);
			AssertAESParty(attachee.IntermediateConsignee, invoice.Consignee, invoice.IntermediateConsigneeDocAddress, "32656946900", AESConstants.IDTypes.DUNS, "CITY", "", Core.Constants.CountryCodes.Australia);
			var commodityLineItems = new List<IAESTIRCommodityLineItem>(attachee.CommodityLineItems);
			AssertEquals("CommodityLineItems", 2, commodityLineItems.Count);
			AssertEquals(entry.MergedLines[0], commodityLineItems[0]);
			AssertEquals(entry.MergedLines[1], commodityLineItems[1]);

			supplier.CustomsCodes.DeleteAll();
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "12345678901", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPIIRSNumber", "", attachee.USPPIIRSNumber);
			AssertEquals("USPPIIRSIDType", "", attachee.USPPIIRSIDType);

			invoice.USPPIDocAddress.E2_AddressOverride = true;
			invoice.USPPIDocAddress.E2_GovRegNumType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			invoice.USPPIDocAddress.E2_GovRegNum = "12345678901";
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPIIRSNumber", "", attachee.USPPIIRSNumber);
			AssertEquals("USPPIIRSIDType", "", attachee.USPPIIRSIDType);

			supplier.CustomsCodes.DeleteAll();
			invoice.USPPIDocAddress.E2_AddressOverride = false;
			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "12345678901", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPI ID TYPE", "D", attachee.USPPI.PartyIDType);
			AssertEquals("USPPI ID", "12345678901", attachee.USPPI.PartyID);

			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.PassportID, "12345678902", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPI ID TYPE", "T", attachee.USPPI.PartyIDType);
			AssertEquals("USPPI ID", "12345678902", attachee.USPPI.PartyID);

			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, "12345678903", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPI ID TYPE", "T", attachee.USPPI.PartyIDType);
			AssertEquals("USPPI ID", "12345678903", attachee.USPPI.PartyID);

			supplier.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12345678905", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			attachee = declaration.ActiveEntryHeaders[0];
			AssertEquals("USPPI ID TYPE", "E", attachee.USPPI.PartyIDType);
			AssertEquals("USPPI ID", "12345678905", attachee.USPPI.PartyID);
		}

		public void TestDataFromLatestClearMessage()
		{
			var helper = new DeclarationTestHelper(Factory);
			var supplier = helper.CreateOrganisation("BOB THE BUILDER", "USLAX");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, "126597687", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_RN_NKCountryOfDestination = "SG";
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_VesselName = "ABC VESSEL";
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;
			declaration.US_SchDArrival = "63592";
			declaration.US_SchDExport = "2709";
			declaration.US_DateOfExport = new ZDateTime(2007, 11, 2);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry.CH_BGMReference = "BHK3234232";
			var entryLine = entry.MergedLines.AddNew();

			var outgoingMessage1 = entry.Messages.AddNew(typeof(MQEDIMessage));
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage1.EM_MessageText =
"B  A0612456712E          US EXPORTER NAME 1                                     " +
"SC1Y11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 Y    " +
"ES1974 A  SHIPMENT ADDED                          X20111130000105               " +
"Y  A0612456712E          US EXPORTER NAME 1";
			outgoingMessage1.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(10);
			outgoingMessage1.EM_MessageNum = "MSG1";

			var incomingMessage1 = entry.Messages.AddNew(typeof(MQEDIMessage));
			incomingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage1.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage1.EM_MessageText =
"B  B0612456712E          US EXPORTER NAME 2                                     " +
"SC1Y11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 Y    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  B0612456712E          US EXPORTER NAME 2";
			incomingMessage1.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(2);
			incomingMessage1.EM_MessageNum = "MSG2";

			var incomingMessage2 = entry.Messages.AddNew(typeof(MQEDIMessage));
			incomingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage2.EM_MessageText =
"B  C0612456712E          US EXPORTER NAME 3                                     " +
"SC1Y11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 Y    " +
"ES1974 A  SHIPMENT ADDED                          X20111130000105               " +
"Y  C0612456712E          US EXPORTER NAME 3";
			incomingMessage2.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(3);
			incomingMessage2.EM_MessageNum = "MSG3";

			var incomingMessage3 = entry.Messages.AddNew(typeof(MQEDIMessage));
			incomingMessage3.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage3.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage3.EM_MessageText =
"B  D0612456712E          US EXPORTER NAME 4                                     " +
"SC1Y12NZNYXXXAS40002510        ABAI WHO YUN HE         4 60167270120071101 Y    " +
"ES1974 A  SHIPMENT ADDED                          X20111130000105               " +
"Y  D0612456712E          US EXPORTER NAME 4";
			incomingMessage3.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(4);
			incomingMessage3.EM_MessageNum = "MSG4";

			var incomingMessage4 = entry.Messages.AddNew(typeof(MQEDIMessage));
			incomingMessage4.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsExport;
			incomingMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage4.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipmentResponse;
			incomingMessage4.EM_MessageText =
"B  E0612456712E          US EXPORTER NAME 5                                     " +
"SC1Y11AUCAUNKNS40002509        ABAI YUN HE               60267270420081101 Y    " +
"ES1066  F FILING OPT IND MUST BE 2 OR 4                                         " +
"ES1970 RF SHIPMENT REJECTED; RESOLVE & RETRANSMIT                               " +
"Y  E0612456712E          US EXPORTER NAME 5";
			incomingMessage4.EM_SystemCreateTimeUtc = ZDateTime.BrettsBirthday.AddMinutes(5);
			incomingMessage4.EM_MessageNum = "MSG5";

			entry.US_IsDeactivated = true;
			AssertEquals(true, entry.HasBeenLodgedAtCustoms);
			AssertEquals(false, entry.HasBeenWithdrawn);

			IAESTIRMessageAttachee attachee = entry;
			invoiceLine.JI_CL = ZGuid.Empty;
			var usppi = attachee.USPPI;
			AssertEquals("D0612456712", usppi.PartyID);
			AssertEquals("E", usppi.PartyIDType);
			AssertEquals("US EXPORTER NAME 4", usppi.PartyName);
			AssertEquals("Y", attachee.RelatedCompanyIndicator);
			AssertEquals("12", attachee.ModeOfTransportationCodeMOT);
			AssertEquals("NZ", attachee.CountryOfUltimateDestinationCode);
			AssertEquals("NY", attachee.USStateOfOriginCode);
			AssertEquals("XXXA", attachee.CarrierIDSCACIATA);
			AssertEquals("S40002510", attachee.ShipmentReferenceNumber);
			AssertEquals("BAI WHO YUN HE", attachee.ConveyanceNameCarrierName);
			AssertEquals("4", attachee.FilingOptionIndicator);
			AssertEquals("60167", attachee.PortOfUnladingCode);
			AssertEquals("2701", attachee.PortOfExportationCode);
			AssertEquals(new ZDateTime(2007, 11, 1), attachee.EstimatedDateOfExport);
			AssertEquals("Y", attachee.HazardousMaterialIndicatorHAZMAT);

			invoiceLine.JI_CL = entryLine.PK;
			entryLine.InvoiceLines.Load();
			usppi = attachee.USPPI;
			AssertEquals("12659768700", usppi.PartyID);
			AssertEquals("T", usppi.PartyIDType);
			AssertEquals("BOB THE BUILDER", usppi.PartyName);
			AssertEquals("N", attachee.RelatedCompanyIndicator);
			AssertEquals("", attachee.ModeOfTransportationCodeMOT);
			AssertEquals(Core.Constants.CountryCodes.Singapore, attachee.CountryOfUltimateDestinationCode);
			AssertEquals("CA", attachee.USStateOfOriginCode);
			AssertEquals(DeclarationTestHelper.ShippingLineSCAC, attachee.CarrierIDSCACIATA);
			AssertEquals("BHK3234232", attachee.ShipmentReferenceNumber);
			AssertEquals("ABC VESSEL", attachee.ConveyanceNameCarrierName);
			AssertEquals(AESCommodityFilingOptionList.Codes._2Predeparture, attachee.FilingOptionIndicator);
			AssertEquals("63592", attachee.PortOfUnladingCode);
			AssertEquals("2709", attachee.PortOfExportationCode);
			AssertEquals(new ZDateTime(2007, 11, 2), attachee.EstimatedDateOfExport);
			AssertEquals("N", attachee.HazardousMaterialIndicatorHAZMAT);

			invoice.Delete();
			usppi = attachee.USPPI;
			AssertEquals("D0612456712", usppi.PartyID);
			AssertEquals("E", usppi.PartyIDType);
			AssertEquals("US EXPORTER NAME 4", usppi.PartyName);
			AssertEquals(ZString.Empty, usppi.ContactFirstName);
			AssertEquals(ZString.Empty, usppi.ContactMiddleInitial);
			AssertEquals(ZString.Empty, usppi.ContactLastName);
			AssertEquals(ZString.Empty, usppi.AddressLine1);
			AssertEquals(ZString.Empty, usppi.AddressLine2);
			AssertEquals(ZString.Empty, usppi.ContactPhoneNumber);
			AssertEquals(ZString.Empty, usppi.City);
			AssertEquals(ZString.Empty, usppi.StateCode);
			AssertEquals(ZString.Empty, usppi.CountryCode);
			AssertEquals(ZString.Empty, usppi.PostalCode);
		}

		public void TestDeriveExportDeclarationStatusIsCalledOnSaving()
		{
			var declaration = Factory.New<JobDeclarationForTesting>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry1 = Factory.New<CusEntryHeader>();
			declaration.CustomsEntryHeaders.Add(entry1);
			Factory.Save();
			AssertGreaterThanOrEqualTo("DeriveExportDeclarationStatus should have been called at least once.", 1, declaration.DeriveExportDeclarationStatusCallCount);
			entry1.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.ReplacementSEDRequired;
			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			declaration.DeriveExportDeclarationStatusCallCount = 0;
			Factory.Save();
			AssertGreaterThanOrEqualTo("DeriveExportDeclarationStatus should have been called at least once.", 1, declaration.DeriveExportDeclarationStatusCallCount);

			entry1.CH_EntryStatus = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			declaration.DeriveExportDeclarationStatusCallCount = 0;
			Factory.Save();
			AssertGreaterThanOrEqualTo("DeriveExportDeclarationStatus should have been called at least once.", 1, declaration.DeriveExportDeclarationStatusCallCount);

			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			declaration.DeriveExportDeclarationStatusCallCount = 0;
			Factory.Save();
			AssertGreaterThanOrEqualTo("DeriveExportDeclarationStatus should have been called at least once.", 1, declaration.DeriveExportDeclarationStatusCallCount);

			entry1.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			declaration.DeriveExportDeclarationStatusCallCount = 0;
			Factory.Save();
			AssertEquals("DeriveExportDeclarationStatus should not have been called.", 0, declaration.DeriveExportDeclarationStatusCallCount);

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			declaration.DeriveExportDeclarationStatusCallCount = 0;
			Factory.Save();
			AssertGreaterThanOrEqualTo("DeriveExportDeclarationStatus should have been called at least once.", 1, declaration.DeriveExportDeclarationStatusCallCount);
		}

		public void TestHasMultipleECCN()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("HasMultipleECCN", entry.MergedLines.HasMultipleECCN, entry.HasMultipleECCN);
		}

		public void TestHasMultipleLicenseDetails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("HasMultipleLicenseDetails", entry.MergedLines.HasMultipleLicenseDetails, entry.HasMultipleLicenseDetails);
		}

		public void TestHas98130075Articles()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "98130075";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Has98130075Articles", true, entry.Has98130075Articles);
		}

		public void TestIsForeignTradeZoneRequired()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			invoice.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertEquals("IsForeignTradeZoneRequired", true, entry.IsForeignTradeZoneRequired);

			invoice.US_InbondType = InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal;
			AssertEquals("IsForeignTradeZoneRequired", true, entry.IsForeignTradeZoneRequired);

			invoice.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;
			AssertEquals("IsForeignTradeZoneRequired", false, entry.IsForeignTradeZoneRequired);
		}

		public void TestIsActive()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("isActive", true, entry.IsActive);

			entry.CH_Status = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertEquals("isActive", true, entry.IsActive);
		}

		public void TestWithdrawalClearLogsCancelEachOther()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertEquals("HasWithdrawalLog - should have cancelled clear logs", true, entry.LogManager.HasAWithdrawnLog);
			AssertEquals("HasAClearLog", false, entry.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("HasWithdrawalLog - should have cancelled withdrawn logs", false, entry.LogManager.HasAWithdrawnLog);
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));
		}

		public void TestClearEntryNumberForNonDBEntriesWhenSaveFails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.AllocateEntryNumber("");

			CusEntryHeaderThrowingExceptionAfterOnSavingForTest entry = Factory.New<CusEntryHeaderThrowingExceptionAfterOnSavingForTest>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("entry is not in db", false, entry.IsInDatabase);
			AssertEquals("entry number is removed as save failed", true, entry.EntryNumber.IsEmpty);
			AssertEquals("entry number is removed as save failed", true, declaration.ImportEntryNumber.IsEmpty);

			entry.ShouldThrowException = false;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("entry is in db", true, entry.IsInDatabase);
			AssertEquals("entry number is assigned", false, entry.EntryNumber.IsEmpty);

			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("entry number is not removed as it was assigned before this transaction", false, entry.EntryNumber.IsEmpty);
		}

		public void TestDoNotClearEntryNumberIfCusEntryNumIsAlreadySavedSuccessfully()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "ABC";
			declaration.AllocateEntryNumber("");
			Factory.Save();
			AssertEquals("Saved", true, declaration.IsInDatabase);
			AssertNotEquals("Entry number is assigned when saved", ZString.Empty, declaration.ImportEntryNumber);

			CusEntryHeaderThrowingExceptionAfterOnSavingForTest entry = Factory.New<CusEntryHeaderThrowingExceptionAfterOnSavingForTest>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }
			AssertNotEquals("entry number is removed as save failed", ZString.Empty, entry.EntryNumber);
			AssertNotEquals("entry number is removed as save failed", ZString.Empty, declaration.ImportEntryNumber);

			entry.ShouldThrowException = false;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("Saved successfully. Entry is in db", true, entry.IsInDatabase);
			AssertEquals("entry number is assigned", false, entry.EntryNumber.IsEmpty);

			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("entry number is not removed as it was assigned before this transaction", false, entry.EntryNumber.IsEmpty);
		}

		public void TestDoNotClearEntryNumberEvenWhenSaveFailsForOnesThatAreNotGeneratedFromNumberFountain()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "ABC";
			declaration.ImportEntryNumber = "123456";
			Factory.Save();
			AssertEquals("Saved", true, declaration.IsInDatabase);

			CusEntryHeaderThrowingExceptionAfterOnSavingForTest entry = Factory.New<CusEntryHeaderThrowingExceptionAfterOnSavingForTest>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.CustomsEntryHeaders.Add(entry);
			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }
			AssertEquals("entry number should not be cleared", "123456", entry.EntryNumber);
			AssertEquals("entry number should not be cleared", "123456", declaration.ImportEntryNumber);

			entry.ShouldThrowException = false;
			Factory.Save();
			AssertEquals("Saved successfully. Entry is in db", true, entry.IsInDatabase);
			AssertEquals("entry number should not be cleared", "123456", entry.EntryNumber);
			AssertEquals("entry number should not be cleared", "123456", declaration.ImportEntryNumber);

			entry.ShouldThrowException = true;
			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("entry number should not be cleared", "123456", entry.EntryNumber);
			AssertEquals("entry number should not be cleared", "123456", declaration.ImportEntryNumber);
		}

		public void TestResetUS_AESBGMRefLastUsedToOriginalWhenSaveFailsDueToConcurrencyIssue()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			DeclarationTestHelper.SetEntryFilerIDDetails("364331434", Registry.Business.Customs.US.AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber);

			var declaration1 = factory1.New<JobDeclaration>();
			declaration1.JE_DeclarationReference = "B00000000";
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration1.JE_ApplicationCode = "BLT";
			declaration1.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var header1 = declaration1.Invoices.AddNew();
			var line1 = declaration1.InvoiceLines.AddNew();
			factory1.Save();

			AESMessageSender sender = new AESMessageSender(declaration1);
			sender.OnPrepare += new AESMessageSender.PrepareEventHandler(delegate { return; });
			sender.SendMessage();
			AssertEquals(1, declaration1.CustomsEntryHeaders.Count);

			var declaration2 = factory2.Load<JobDeclaration>(declaration1.PK);
			declaration2.InvoiceLines.FirstOrDefault().Delete();
			factory2.Save();
			AssertEquals("B00000000", declaration1.GetAddInfo().US_AESBGMRefLastUsed);

			try
			{
				factory1.Save();
			}
			catch
			{
			}
			var line2 = declaration1.InvoiceLines.AddNew();
			sender.SendMessage();
			try
			{
				factory1.Save();
			}
			catch
			{
			}
			AssertEquals(ZString.Empty, declaration1.GetAddInfo().US_AESBGMRefLastUsed);
		}

		public void TestResetUS_CRLCertStatusToOriginalWhenSaveFails()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			CusEntryHeaderThrowingExceptionAfterOnSavingForTest entry = Factory.New<CusEntryHeaderThrowingExceptionAfterOnSavingForTest>();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.CustomsEntryHeaders.Add(entry);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			entry.ShouldThrowException = false;
			entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			Factory.Save();
			AssertEquals("PreCondition", true, entry.IsInDatabase);

			entry.ShouldThrowException = true;
			entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;

			try
			{
				Factory.Save();
			}
			catch (ApplicationException) { }

			AssertEquals("US_CRLCertStatus is reset to its original value", CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, entry.US_CRLCertStatus);
		}

		public void TestAddLogsForInBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			AssertEquals(true, declaration.IsInBond);

			ImportMessageStatusList list = new ImportMessageStatusList();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertEquals("IsStatusClear", false, list.IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", false, entry.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("IsStatusClear", true, list.IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { ImportMessageStatusList.Codes.ClearDepartureOriginal }));

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			AssertEquals("IsStatusClear", false, list.IsStatusClear(entry.CH_Status));
			AssertEquals("HasAWithdrawnLog", false, entry.LogManager.HasAWithdrawnLog);

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertEquals("HasAWithdrawnLog", true, entry.LogManager.HasAWithdrawnLog);
		}

		public void TestIsClearedEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;
			AssertEquals(true, declaration.IsInBond);

			ImportMessageStatusList list = new ImportMessageStatusList();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			SetStatusAndSave(entry, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertEquals("Entry is not Cleared", false, entry.IsClearedEntry);

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("Entry is Cleared", true, entry.IsClearedEntry);

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.ErrorEntrySummaryReplace);
			AssertEquals("Entry is not Cleared", false, entry.IsClearedEntry);

			SetStatusAndSave(entry, ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings);
			AssertEquals("Entry is Cleared", true, entry.IsClearedEntry);
		}

		public void TestAddLogsForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			SetStatusAndSave(entry, AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse);
			AssertEquals("IsStatusClear", false, new AESDirectCustomsEntryStatus().IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", false, entry.LogManager.HasAClearLog(new string[] { AESDirectCustomsEntryStatus.Codes.OriginalSEDClear }));

			SetStatusAndSave(entry, AESDirectCustomsEntryStatus.Codes.OriginalSEDClear);
			AssertEquals("IsStatusClear", true, new AESDirectCustomsEntryStatus().IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { AESDirectCustomsEntryStatus.Codes.OriginalSEDClear }));

			SetStatusAndSave(entry, AESDirectCustomsEntryStatus.Codes.AwaitingReplacementResponse);
			AssertEquals("IsStatusClear", false, new AESDirectCustomsEntryStatus().IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { AESDirectCustomsEntryStatus.Codes.OriginalSEDClear }));

			SetStatusAndSave(entry, AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear);
			AssertEquals("IsStatusClear", true, new AESDirectCustomsEntryStatus().IsStatusClear(entry.CH_Status));
			AssertEquals("HasAClearLog", true, entry.LogManager.HasAClearLog(new string[] { AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear }));
		}

		public void TestEntryNumberType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_EnableINB = true;
			CusEntryHeader entry1 = declaration.CustomsEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(ZString.Empty, entry1.EntryNumber);
			AssertNull(entry1.CusEntryNumber);
			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(ZString.Empty, entry2.EntryNumber);
			AssertNull(entry2.CusEntryNumber);
			CusEntryHeader entry3 = declaration.CustomsEntryHeaders.AddNew();
			entry3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(ZString.Empty, entry3.EntryNumber);
			AssertNull(entry3.CusEntryNumber);
			CusEntryHeader entry4 = declaration.CustomsEntryHeaders.AddNew();
			entry4.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(ZString.Empty, entry4.EntryNumber);
			AssertNull(entry4.CusEntryNumber);
			CusEntryHeader entry5 = declaration.CustomsEntryHeaders.AddNew();
			entry5.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals(ZString.Empty, entry5.EntryNumber);
			AssertNull(entry5.CusEntryNumber);
			declaration.ImportEntryNumber = "23342323";
			AssertEquals("23342323", entry1.EntryNumber);
			AssertEquals("23342323", entry2.EntryNumber);
			AssertEquals("23342323", entry3.EntryNumber);
			AssertEquals(ZString.Empty, entry4.EntryNumber);
			AssertEquals(ZString.Empty, entry5.EntryNumber);
			AssertEquals(declaration.ENSEntryNumber, entry1.CusEntryNumber);
			AssertEquals(declaration.ENSEntryNumber, entry2.CusEntryNumber);
			AssertEquals(declaration.ENSEntryNumber, entry3.CusEntryNumber);
			AssertNull(entry4.CusEntryNumber);
			AssertNull(entry5.CusEntryNumber);
			entry1.EntryNumber = "52342223";
			AssertEquals("52342223", declaration.ImportEntryNumber);
			AssertEquals("52342223", entry2.EntryNumber);
			AssertEquals("52342223", entry3.EntryNumber);
			AssertEquals(ZString.Empty, entry4.EntryNumber);
			AssertEquals(ZString.Empty, entry5.EntryNumber);
			entry4.EntryNumber = "4621890712";
			AssertEquals(declaration.INBEntryNumber, entry4.CusEntryNumber);
			entry5.EntryNumber = "43521412";
			AssertEquals(entry5.CH_MessageType, entry5.CusEntryNumber.CE_EntryType);
			AssertEquals(entry5.PK, entry5.CusEntryNumber.CE_ParentID);
		}

		public void TestExportingCarrierWhenNoShippingLineEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VesselName = "HYOGO MARU";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("When sea declaration, ExportingCarrier should pick up the vessel name if the shippingLine is not entered.", declaration.JE_VesselName, entry.ExportingCarrier);

			var carrier = Factory.NewWithValidTestData<USCarrierCombined>();
			carrier.UI_Code = "STRK";
			carrier.UI_Name = "Stark Airlines";
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = "STRK";
			AssertEquals("When air declaration, ExportingCarrier should pick up the airline from SCAC if the shippingLine is not entered.", "Stark Airlines", entry.ExportingCarrier);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = "STRK";
			AssertEquals("When RAIL declaration, ExportingCarrier should pick up company from SCAC if the shippingLine is not entered.", "Stark Airlines", entry.ExportingCarrier);

			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_CarrierName = "LITTLE-KNOWN COMPANY";
			AssertEquals("When RAIL declaration, ExportingCarrier should pick up 'Company Name' property if the shippingLine is not entered and SCAC is unknown.", "LITTLE-KNOWN COMPANY", entry.ExportingCarrier);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			declaration.US_UI_NKCarrierSCAC = "STRK";
			AssertEquals("When TRUCK declaration, ExportingCarrier should pick up company from SCAC if the shippingLine is not entered.", "Stark Airlines", entry.ExportingCarrier);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Truck;
			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.US_CarrierName = "LITTLE-KNOWN COMPANY";
			AssertEquals("When TRUCK declaration, ExportingCarrier should pick up 'Company Name' property if the shippingLine is not entered and SCAC is unknown.", "LITTLE-KNOWN COMPANY", entry.ExportingCarrier);
		}

		public void TestExportingCarrierShouldNotBeAvailableFor()
		{
			var helper = new DeclarationTestHelper(Factory);
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			airline.RM_AirlineName1 = "TESTING AIRLINE";
			airline.RM_TwoCharacterCode = "TA";
			airline.RM_ThreeLetterCode = "";
			helper.ShippingLine.MiscServ.OM_RM_Airline = airline.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_VesselName = "VESSEL";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var list = new TransportTypeList();
			foreach (var transportMode in new[] { TransportTypeList.Codes.Air, TransportTypeList.Codes.BorderWaterBorne, TransportTypeList.Codes.Rail, TransportTypeList.Codes.Sea, TransportTypeList.Codes.Truck })
			{
				list.RemoveCode(transportMode);
				declaration.JE_TransportMode = transportMode;
				declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
				declaration.JE_VesselName = "VESSEL";
				AssertNotEquals("Transport Mode " + transportMode, ZString.Empty, entry.ExportingCarrier);
			}
			foreach (ICodeDescription pair in list)
			{
				declaration.JE_TransportMode = pair.Code;
				declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
				declaration.JE_VesselName = "VESSEL";
				AssertEquals("Transport Mode " + pair.Code, ZString.Empty, entry.ExportingCarrier);
			}
		}

		public void TestProperties()
		{
			var helper = new DeclarationTestHelper(Factory);
			var airline = Factory.NewWithValidTestData<RefAirline>();
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "081";
			airline.RM_AirlineName1 = "TESTING AIRLINE";
			airline.RM_TwoCharacterCode = "TA";
			airline.RM_ThreeLetterCode = "";
			helper.ShippingLine.MiscServ.OM_RM_Airline = airline.PK;

			var exportDate = new ZDateTime(2006, 10, 23, 3, 42, 23);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ExportDate = exportDate;
			declaration.JE_RL_NKPortOfLoading = helper.USLAX.Code;
			declaration.JE_RL_NKFinalDestination = helper.AUBNE.Code;
			declaration.US_SchDExport = "2704";
			declaration.US_SchDArrival = "60267";
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.JE_MasterBill = "08123212111";
			declaration.JE_HouseBill = "H2342323232";
			declaration.US_TransportReference = "BKÏ3 4Ù23"; // invalid characters should be stripped
			declaration.US_StateOfOrigin = "NY";
			declaration.US_ForeignTradeZone = "FXT";
			declaration.US_ImportEntryNo = "Ent12";
			declaration.US_InbondType = InbondTypeList.Codes.IEWarehouseWithdrawal;
			declaration.US_RN_NKCountryOfDestination = ZString.Empty;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.CustomsEntryHeaders[0];
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.NewZealand;
			AssertEquals("StateOfOrigin", "NY", entry.StateOfOrigin);
			AssertEquals("ForeignTradeZone", "FXT", entry.ForeignTradeZone);
			AssertEquals("PortOfExportInUSFormat", "2704", entry.PortOfExportInUSFormat);
			AssertEquals("CountryOfUltimateDestination", Core.Constants.CountryCodes.NewZealand, entry.CountryOfUltimateDestination);
			AssertEquals("PortOfArrivalInUSFormat", "60267", entry.PortOfArrivalInUSFormat);
			AssertEquals("ExportDate", exportDate.Date, entry.ExportDate);
			AssertEquals("ModeOfTransport", declaration.JE_Calc_USTransportMode, entry.ModeOfTransport);
			AssertEquals("CarrierCode", declaration.CarrierCode, entry.CarrierCode);
			AssertEquals("TransportationReferenceNumber", "BK3 423", entry.TransportationReferenceNumber);
			AssertEquals("ExportingCarrier", helper.VesselWithCountry.RV_Code, entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", helper.VesselWithCountry.RV_RN_NKCountryOfReg, entry.VesselCountryOfRegistration);
			AssertEquals("IsTransactionsRelated", false, entry.IsTransactionsRelated);
			AssertEquals("IsHazardousCargo", false, entry.IsHazardousCargo);
			AssertEquals("IsRoutedTransaction", false, entry.IsRoutedTransaction);
			AssertEquals("ImportEntryNumber", "Ent12", entry.ImportEntryNumber);
			AssertEquals("InbondType", InbondTypeList.Codes.IEWarehouseWithdrawal, entry.InbondType);

			declaration.US_RN_NKCountryOfDestination = ZString.Empty;
			invoice.US_StateOfOrigin = "WA";
			invoice.US_ForeignTradeZone = "ZZZ";
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoice.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			invoice.US_RoutedTransaction = YesNoDefaultList.Codes.No;
			invoice.US_ImportEntryNo = "L453s";
			invoice.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			AssertEquals("StateOfOrigin", "WA", entry.StateOfOrigin);
			AssertEquals("ForeignTradeZone", "ZZZ", entry.ForeignTradeZone);
			AssertEquals("PortOfExportInUSFormat", "2704", entry.PortOfExportInUSFormat);
			AssertEquals("CountryOfUltimateDestination", helper.AUBNE.RL_RN_NKCountryCode, entry.CountryOfUltimateDestination);
			AssertEquals("PortOfArrivalInUSFormat", "60267", entry.PortOfArrivalInUSFormat);
			AssertEquals("ExportDate", exportDate.Date, entry.ExportDate);
			AssertEquals("ModeOfTransport", declaration.JE_Calc_USTransportMode, entry.ModeOfTransport);
			AssertEquals("CarrierCode", declaration.CarrierCode, entry.CarrierCode);
			AssertEquals("TransportationReferenceNumber", "BK3 423", entry.TransportationReferenceNumber);
			AssertEquals("ExportingCarrier", helper.VesselWithCountry.RV_Code, entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", helper.VesselWithCountry.RV_RN_NKCountryOfReg, entry.VesselCountryOfRegistration);
			AssertEquals("IsTransactionsRelated", true, entry.IsTransactionsRelated);
			AssertEquals("IsHazardousCargo", true, entry.IsHazardousCargo);
			AssertEquals("IsRoutedTransaction", false, entry.IsRoutedTransaction);
			AssertEquals("ImportEntryNumber", "L453s", entry.ImportEntryNumber);
			AssertEquals("InbondType", InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, entry.InbondType);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			AssertEquals("CarrierCode", airline.RM_TwoCharacterCode, entry.CarrierCode);
			AssertEquals("ExportingCarrier", airline.RM_AirlineName1, entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", "", entry.VesselCountryOfRegistration);
			AssertEquals("TransportationReferenceNumber", "BK3 423", entry.TransportationReferenceNumber);

			airline.RM_ThreeLetterCode = "TAR";
			declaration.JE_MasterBill = "";
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			AssertEquals("CarrierCode", airline.RM_TwoCharacterCode, entry.CarrierCode);
			AssertEquals("ExportingCarrier", airline.RM_AirlineName1, entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", "", entry.VesselCountryOfRegistration);
			AssertEquals("TransportationReferenceNumber", "H2342323232", entry.TransportationReferenceNumber);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			AssertEquals("CarrierCode", declaration.CarrierCode, entry.CarrierCode);
			AssertEquals("ExportingCarrier", helper.ShippingLine.OH_FullName, entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", "", entry.VesselCountryOfRegistration);
			AssertEquals("TransportationReferenceNumber", "H2342323232", entry.TransportationReferenceNumber);

			declaration.JE_TransportMode = declaration.TransportModeRoadCodeForTesting;
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			AssertEquals("CarrierCode", declaration.CarrierCode, entry.CarrierCode);
			AssertEquals("ExportingCarrier", "", entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", "", entry.VesselCountryOfRegistration);
			AssertEquals("TransportationReferenceNumber", "H2342323232", entry.TransportationReferenceNumber);

			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertEquals("CarrierCode", "", entry.CarrierCode);
			AssertEquals("ExportingCarrier", "", entry.ExportingCarrier);
			AssertEquals("VesselCountryOfRegistration", "", entry.VesselCountryOfRegistration);

			declaration.US_UI_NKCarrierSCAC = "UNKN";
			declaration.JE_TransportMode = declaration.TransportModeRailCodeForTesting;
			declaration.US_CarrierName = "LITTLE-KNOWN COMPANY";
			declaration.JE_OH_ShippingLine = ZGuid.Empty;
			AssertEquals("ExportingCarrier", "LITTLE-KNOWN COMPANY", entry.ExportingCarrier);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_VesselName = helper.VesselWithCountry.RV_Code;
			declaration.JE_OH_ShippingLine = helper.ShippingLine.PK;
			declaration.US_SchDArrival = "60267";
			AssertEquals("PortOfArrivalInUSFormat", "60267", entry.PortOfArrivalInUSFormat);

			declaration.US_EnableAII = true;
			Assert(entry.IsElectronicInvoicing);
			declaration.US_EnableAII = false;
			declaration.US_IsInvoiceByRequest = true;
			Assert(entry.IsElectronicInvoicing);
			declaration.US_IsInvoiceByRequest = false;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert(entry.IsElectronicInvoicing);
			declaration.US_EntryMode = ZString.Empty;
			Assert(!entry.IsElectronicInvoicing);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert(!entry.IsElectronicInvoicing);
		}

		public void TestCountryOfUltimateDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.UnitedStates;

			var org01 = CreateOrgForAddressTest("org01", "adr01", "adr02", "city01", "state01", "0001");
			org01.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			org01.OH_RL_NKClosestPort = "USTES";

			var org02 = CreateOrgForAddressTest("org02", "adr01", "adr02", "city02", "state02", "0002");
			org02.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			org02.OH_RL_NKClosestPort = "CATES";

			var entry01 = declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();

			AssertEquals("Entry's Ultimate Destination Country is equal to Declaration's Destination Country", Core.Constants.CountryCodes.UnitedStates, entry01.CountryOfUltimateDestination);

			var invoice01 = declaration.Invoices.AddNew();
			var invoice02 = declaration.Invoices.AddNew();
			var line01 = invoice01.InvoiceLines.AddNew();
			var line02 = invoice02.InvoiceLines.AddNew();
			invoice01.JZ_OH_Buyer = org01.PK;
			invoice02.JZ_OH_Buyer = org02.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry02 = (CusEntryHeader)invoice01.FirstEntryHeader;
			var entry03 = (CusEntryHeader)invoice02.FirstEntryHeader;
			Factory.Save();

			AssertEquals("US", org01.CountryCode, entry02.CountryOfUltimateDestination);
			AssertEquals("CA", org02.CountryCode, entry03.CountryOfUltimateDestination);
		}

		public void TestFlightNo()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "QF220";

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			ICusEntryHeader cusEntry = entry;
			AssertEquals("Flight no should strip Carrier prefix", "220", cusEntry.VoyageNumber);

			declaration.JE_VoyageFlightNo = "235B";
			AssertEquals("Flight no", "235B", cusEntry.VoyageNumber);

			declaration.JE_VoyageFlightNo = "UA7354C";
			AssertEquals("Flight no", "7354C", cusEntry.VoyageNumber);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_VoyageFlightNo = "16E";
			AssertEquals("Voyage no", "16E", cusEntry.VoyageNumber);

			declaration.JE_VoyageFlightNo = "SE520";
			AssertEquals("Voyage no", "SE520", cusEntry.VoyageNumber);
		}

		public void TestEntryNumberAndUS_XTNIsReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			AssertEquals("entry.EntryNumberInfo.ReadOnly", false, entry.EntryNumberInfo.ReadOnly);
			AssertEquals("entry.US_XTNInfo.ReadOnly", false, entry.US_XTNInfo.ReadOnly);

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.ReplacementSEDClear;
			AssertEquals("entry.EntryNumberInfo.ReadOnly", true, entry.EntryNumberInfo.ReadOnly);
			AssertEquals("entry.US_XTNInfo.ReadOnly", true, entry.US_XTNInfo.ReadOnly);

			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			entry2.CH_Status = AESDirectCustomsEntryStatus.Codes.NotSent;
			entry2.US_IsDeactivated = true;
			AssertEquals("entry2.EntryNumberInfo.ReadOnly", true, entry2.EntryNumberInfo.ReadOnly);
			AssertEquals("entry2.US_XTNInfo.ReadOnly", true, entry2.US_XTNInfo.ReadOnly);
		}

		public void TestMessageStatusDescriptionForExport()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			AssertEquals("CH_Status", AESDirectCustomsEntryStatus.Codes.NotSent, entry.CH_Status);
			AssertEquals("MessageStatusDesc", AESDirectCustomsEntryStatus.Descriptions.NotSent, entry.MessageStatusDescription);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			AssertEquals("MessageStatusDesc", AESDirectCustomsEntryStatus.Descriptions.AwaitingOriginalResponse, entry.MessageStatusDescription);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			AssertEquals("MessageStatusDesc", AESDirectCustomsEntryStatus.Descriptions.OriginalSEDClear, entry.MessageStatusDescription);
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.Error;
			AssertEquals("MessageStatusDesc", AESDirectCustomsEntryStatus.Descriptions.Error, entry.MessageStatusDescription);
		}

		public void TestEntryNumberAssignmentOnSaving()
		{
			var companyStmNums = DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			companyStmNums.SetNextNumber(100212);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableINB = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;

			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			InBondNumberSetting inBondSetting = InBondNumberSetting.New(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));
			ZString nextNumber = inBondSetting.CurrentNextNumber.ToString().PadLeft(8, '0');
			nextNumber = nextNumber + InBondNumberCheckDigitCalculator.GetCheckDigit(nextNumber);

			AssertEquals("entry number", "", entry.EntryNumber);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("entry number is assigned for inbond", nextNumber, entry.EntryNumber);

			declaration.US_EnableINB = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			nextNumber = GetEntryNumber("0100212");

			AssertEquals("entry number", "", entry.EntryNumber);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			Factory.Save();
			AssertEquals("entry number is assigned for entry summary", nextNumber, entry.EntryNumber);
		}

		public void TestWarehouseEntryNumber()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSEntryNumber = "123";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.MergedLines.Add(entryLine);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("123", ((ICusEntryHeader)entryHeader).WarehouseEntryNumber);
		}

		public void TestDistrictPortCodeOfWarehouseEntry()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSDistrictPortCode = "123";
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader.MergedLines.Add(entryLine);
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("123", ((ICusEntryHeader)entryHeader).DistrictPortCodeOfWarehouseEntry);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals("123", ((ICusEntryHeader)entryHeader).DistrictPortCodeOfWarehouseEntry);
		}

		public void TestIsConsigneeNameAddressUsed()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(false, ((ICargoReleaseCusEntryHeader)entry).IsConsigneeNameAddressUsed);

			entry.US_UseConsigneeNameAddress = true;
			AssertEquals(true, ((ICargoReleaseCusEntryHeader)entry).IsConsigneeNameAddressUsed);
		}

		public void TestEncryptedUltimateConsigneeNumber()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";

			OrgCusCode ultimateConsigneeECNCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeECNCode.OK_CodeType = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
			ultimateConsigneeECNCode.OK_CustomsRegNo = "-T16WHSL5CXR";

			entry.Declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("-T16WHSL5CXR", ((ICargoReleaseCusEntryHeader)entry).UltimateConsigneeNumber);

			OrgCusCode ultimateConsigneeEINCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ultimateConsigneeEINCode.OK_CustomsRegNo = "088888-44444";
			AssertEquals("088888-44444", ((ICargoReleaseCusEntryHeader)entry).UltimateConsigneeNumber);
		}

		public void TestImporterCustomsRegNo()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_FullName = "Importer";
			OrgCusCode importerEINCode = importer.CustomsCodes.AddNew();
			importerEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			importerEINCode.OK_CustomsRegNo = "75-2221134";
			declaration.JE_OH_Importer = importer.PK;

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			OrgCusCode ultimateConsigneeEINCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ultimateConsigneeEINCode.OK_CustomsRegNo = "12-3456789";
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			importerOfRecord.OH_FullName = "Importer Of Record";
			OrgCusCode importerOfRecordEINCode = importerOfRecord.CustomsCodes.AddNew();
			importerOfRecordEINCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			importerOfRecordEINCode.OK_CustomsRegNo = "093801-04778";
			declaration.IOROrgPK = importerOfRecord.PK;

			AssertEquals("UltimateConsignee Customs number", "12-3456789", entry.Declaration.UltimateConsigneeCustomsClientNumber);
			AssertEquals("Importer number", "75-2221134", entry.Declaration.ImporterCustomsClientNumber);
		}

		public void TestEffectiveUltimateConsigneeCustomsRegNo()
		{
			var declaration = SetUpMergedInvoices();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var entry = declaration.CustomsEntryHeaders[0];

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			var ultimateConsigneeEINCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ultimateConsigneeEINCode.OK_CustomsRegNo = "12-3456789";

			declaration.IOROrgPK = ultimateConsignee.PK;
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;

			AssertEquals("ultimateConsignee is Importer of Record", USConstants.Same, entry.EffectiveUltimateConsigneeCustomsRegNo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals("ultimateConsignee for FTZ(06) entry type", "12-3456789", entry.EffectiveUltimateConsigneeCustomsRegNo);

			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			importerOfRecord.OH_FullName = "IOR";
			declaration.IOROrgPK = importerOfRecord.PK;

			AssertEquals("Ultimate Consignee for FTZ(06) entry type", "12-3456789", entry.EffectiveUltimateConsigneeCustomsRegNo);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("Ultimate Consignee differs from Importer of Record : EIN", "12-3456789", entry.EffectiveUltimateConsigneeCustomsRegNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			var ultimateConsigneeSSNCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeSSNCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			ultimateConsigneeSSNCode.OK_CustomsRegNo = "123-12-1234";

			AssertEquals("Ultimate Consignee differs from Importer of Record : SSN", "123-12-1234", entry.EffectiveUltimateConsigneeCustomsRegNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			var ultimateConsigneeCBPCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeCBPCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			ultimateConsigneeCBPCode.OK_CustomsRegNo = "YYDDPP-44999";

			AssertEquals("Ultimate Consignee differs from Importer of Record : CBP", "YYDDPP-44999", entry.EffectiveUltimateConsigneeCustomsRegNo);
		}

		public void TestUltimateConsigneeAddressDetails()
		{
			var declaration = SetUpMergedInvoices();
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(null, entry.EffectiveUltimateConsigneeWrapperAddressDetails);

			var ultimateConsignee = CreateOrgForAddressTest("Ultimate Consignee", "UC Addr 1", "UC Addr 2", "UC City", "", "UC PC");
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("Ultimate Consignee", entry.EffectiveUltimateConsigneeWrapperAddressDetails.CompanyName);
			var assertionMessage = " from Main Address";
			AssertEquals("Address 1" + assertionMessage, "UC Addr 1", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1);
			AssertEquals("Address 2" + assertionMessage, "UC Addr 2", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine2);
			AssertEquals("City" + assertionMessage, "UC City", entry.EffectiveUltimateConsigneeWrapperAddressDetails.City);
			AssertEquals("State" + assertionMessage, "", entry.EffectiveUltimateConsigneeWrapperAddressDetails.State);
			AssertEquals("Post Code" + assertionMessage, "UC PC", entry.EffectiveUltimateConsigneeWrapperAddressDetails.PostCode);
			AssertEquals("Ultimate consignee address details - Line 1", "UC Addr 1", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1);
			AssertEquals("Ultimate consignee address details - Line 2", "UC Addr 2", entry.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string", "UC City   UC PC", entry.EffectiveUltimateConsigneeCityStatePostCodeCountry);

			AddCustomsAddress(ultimateConsignee, "Customs Address Line 1", "Customs Address Line 2", "Michigan City", "IL", "59650");
			AssertEquals("Ultimate Consignee", entry.EffectiveUltimateConsigneeWrapperAddressDetails.CompanyName);
			assertionMessage = " should comes from Customs Address";
			AssertEquals("Address 1" + assertionMessage, "Customs Address Line 1", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1);
			AssertEquals("Address 2" + assertionMessage, "Customs Address Line 2", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine2);
			AssertEquals("City" + assertionMessage, "Michigan City", entry.EffectiveUltimateConsigneeWrapperAddressDetails.City);
			AssertEquals("State" + assertionMessage, "IL", entry.EffectiveUltimateConsigneeWrapperAddressDetails.State);
			AssertEquals("Post Code" + assertionMessage, "59650", entry.EffectiveUltimateConsigneeWrapperAddressDetails.PostCode);
			AssertEquals("Ultimate consignee address details - Line 1", "Customs Address Line 1", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1);
			AssertEquals("Ultimate consignee address details - Line 2", "Customs Address Line 2", entry.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string", "Michigan City   IL   59650   US", entry.EffectiveUltimateConsigneeCityStatePostCodeCountry);

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.FillWithValidTestData();
			ultimateConsignee2.OH_FullName = "Ultimate Consignee 2";

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			AssertEquals("Multiple consignees - should still return Declaration Ultimate Consignee", "Ultimate Consignee", entry.EffectiveUltimateConsigneeWrapperAddressDetails.CompanyName);
			assertionMessage = "Ultimate consignee address details should still return Declaration Ultimate Consignee details when multiple consignees exist";
			AssertEquals(assertionMessage, "Customs Address Line 1", entry.EffectiveUltimateConsigneeWrapperAddressDetails.AddressLine1);
			AssertEquals(assertionMessage, "Customs Address Line 2", entry.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals(assertionMessage, "Michigan City   IL   59650   US", entry.EffectiveUltimateConsigneeCityStatePostCodeCountry);

			var customsAddress = ultimateConsignee.Addresses.AddressesOfType(OrgConstants.AddressType.CustomsAddressOfRecord)[0];
			customsAddress.OA_Address2 = ZString.Empty;
			AssertEquals("EffectiveUltimateConsigneeAddressLine2 should now show city state postcode line", "Michigan City   IL   59650   US", entry.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string should be blank as has been move up 1 line", "", entry.EffectiveUltimateConsigneeCityStatePostCodeCountry);

			customsAddress.OA_Address2 = "Address line 2";
			customsAddress.OA_City = "";
			customsAddress.OA_PostCode = "59650";
			customsAddress.OA_State = "";
			AssertEquals("Address Line 2", "Address line 2", entry.EffectiveUltimateConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string when no city or state should be trimmed start",
						"59650   US", entry.EffectiveUltimateConsigneeCityStatePostCodeCountry);
		}

		[TestDate(2015, 01, 26)]
		public void TestElectedEntryDate()
		{
			Declaration.US_EntryDate = new ZDateTime(2008, 10, 01);
			Declaration.US_EntryDateElectionCode = "";
			AssertEquals("Elected entry date should be blank", "", EntryHeader.ElectedEntryDate);

			Declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.ArrivalDate;
			AssertEquals("Elected entry date should be arrival date", "10/01/2008", EntryHeader.ElectedEntryDate);

			Declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			AssertEquals("Elected entry date should be blank - no presentation date value", "", EntryHeader.ElectedEntryDate);

			Declaration.US_PresentationDate = new ZDateTime(2008, 10, 04);
			AssertEquals("Elected entry date should be presentation date", "10/04/2008", EntryHeader.ElectedEntryDate);

			Declaration.US_EntryDateElectionCode = "";
			Declaration.US_EstimatedEntryDate = new ZDateTime(2014, 03, 18);
			AssertEquals("Estimated entry date - When no specific selection of date is requested, print release date otherwise print estimated entry date.", "03/18/2014", EntryHeader.ElectedEntryDate);
			Declaration.JE_EntryAuthorisationDate = new ZDateTime(2014, 03, 19);
			AssertEquals("Release date - When no specific selection of date is requested, print release date otherwise print estimated entry date.", "03/19/2014", EntryHeader.ElectedEntryDate);

			Declaration.US_PresentationDate = ZDateTime.Empty;
			Declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			AssertEquals("Elected entry date for 'W' should be defaulted from Estimated Entry Date", "03/18/2014", EntryHeader.ElectedEntryDate);

			Declaration.US_PresentationDate = ZDateTime.Today.AddDays(2);
			AssertEquals("Elected entry date", "01/28/2015", EntryHeader.ElectedEntryDate);
		}

		public void TestIsRemoteLocationFiling()
		{
			CusEntryHeader entry = Factory.New<CusEntryHeader>();
			Assert("IsRemoteLocationFiling should be false because declaration is null here", !entry.IsRemoteLocationFiling);

			EntryHeader.Declaration.US_EntryMode = "";
			Assert("Precondition: !Declaration.IsRemoteLocationFiling", !EntryHeader.Declaration.IsRemoteLocationFiling);
			Assert("IsRemoteLocationFiling should be false because Declaration.IsRemoteLocationFiling is false", !EntryHeader.IsRemoteLocationFiling);

			EntryHeader.Declaration.US_EntryMode = EntryModeList.Codes.RLF;
			Assert("Precondition: Declaration.IsRemoteLocationFiling", EntryHeader.Declaration.IsRemoteLocationFiling);
			Assert("IsRemoteLocationFiling should be true because Declaration.IsRemoteLocationFiling is true", EntryHeader.IsRemoteLocationFiling);
		}

		[TestDate(2016, 12, 1)]
		public void TestFTZAdmissionNumberForFTZ214Document()
		{
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			AssertEquals("Validation Mode", true, declaration.IsFTZAdmissionValidationMode);
			AssertEquals("Entry Filer should be set by default", "SV9", declaration.US_EntryFilerCode);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;

			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "654-73-1975");

			declaration.FTZZoneID = "1530A01";
			declaration.FTZControlNumber = "00000045";
			AssertEquals("ForeignTradeZone AdmissionNumber", "1530A01|16|00000045", declaration.FTZAdmissionNumber);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryHeader = declaration.FTZEntry;
			AssertEquals("FTZ AdmissionNumberCusEntry", declaration.FTZAdmissionNumber, entryHeader.FTZAdmissionNumber);
			AssertEquals("Zone ID allow 9 Characters", "1530A01", entryHeader.FTZZone);
			declaration.FTZZoneID = "222333444555";
			AssertEquals("Zone ID allow 9 Characters", "222333444", entryHeader.FTZZone);
		}

		public void TestIsQuotaOrVisaEntryType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(false, entryHeader.IsQuotaOrVisaEntryType);

			declaration.US_EntryType = EntryTypeList.Codes.InformalQuotaVisa;
			AssertEquals(true, entryHeader.IsQuotaOrVisaEntryType);
		}

		public void TestStatusNeedsRecalculation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew(typeof(EDIMessage)).EM_Status = "RCV";
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(true, ((IStatusNeedsRecalculationProvider)entry).StatusNeedsRecalculation);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(true, ((IStatusNeedsRecalculationProvider)entry).StatusNeedsRecalculation);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(true, ((IStatusNeedsRecalculationProvider)entry).StatusNeedsRecalculation);

			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(false, ((IStatusNeedsRecalculationProvider)entry).StatusNeedsRecalculation);
		}

		public void TestICusEntryHeaderStateOfDestination()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.US_DestinationState = "IL";
			AssertEquals("StateOfDestination", "IL", ((ICusEntryHeader)entry).StateOfDestination);
		}

		public void TestICusEntryHeaderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = "ACS";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 153m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Declaration reference", declaration.JE_DeclarationReference, ensEntry.BrokerReferenceNumber);

			declaration.US_BRDRefNo = "B123456789";
			AssertEquals("Declaration reference", "123456789", ensEntry.BrokerReferenceNumber);

			var iACECusEntryHeader = (IACECusEntryHeader)ensEntry;
			AssertEquals("BondWaivedOrNoBond", true, iACECusEntryHeader.BondWaivedOrNoBond);
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("BondWaivedOrNoBond", false, iACECusEntryHeader.BondWaivedOrNoBond);
			declaration.US_BondWaiverCode = BondWaiverReasonCodeList.Codes._995;
			AssertEquals("BondWaivedOrNoBond", true, iACECusEntryHeader.BondWaivedOrNoBond);
			AssertEquals("BondWaiverReasonCode", BondWaiverReasonCodeList.Codes._995, iACECusEntryHeader.BondWaiverReasonCode);

			declaration.US_BondWaiverCode = ZString.Empty;
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			AssertEquals("BondWaivedOrNoBond", true, iACECusEntryHeader.BondWaivedOrNoBond);
			AssertEquals("BondWaiverReasonCode", ZString.Empty, iACECusEntryHeader.BondWaiverReasonCode);

			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit;
			AssertEquals("SplitShipmentReleaseCode", "", iACECusEntryHeader.SplitShipmentReleaseCode);

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit;
			AssertEquals("SplitShipmentReleaseCode", "2", iACECusEntryHeader.SplitShipmentReleaseCode);

			declaration.JE_MasterBill = "TestM1";
			declaration.PrimaryMasterBill.US_SESplitShip = true;
			AssertEquals("SplitShipment", true, iACECusEntryHeader.IsSplitShipment);
			AssertEquals("TotalValueOfEntrySummary", 153m, iACECusEntryHeader.TotalValueOfEntrySummary);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			invoiceLine.JI_LinePrice = ZDecimal.Zero;
			declaration.US_EstEnteredValue = 180;
			AssertEquals("TotalValueOfEntrySummary", 153m, iACECusEntryHeader.TotalValueOfEntrySummary);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			AssertEquals("TotalValueOfEntrySummary", 153m, iACECusEntryHeader.TotalValueOfEntrySummary);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = false;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			AssertEquals("CR Certification indicator", false, iACECusEntryHeader.IsACECargoReleaseCertification);

			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			AssertEquals("CR Certification indicator", true, iACECusEntryHeader.IsACECargoReleaseCertification);

			declaration.US_EnableCRL = true;
			AssertEquals("Precondition", CargoReleaseTypeList.Codes.SE, declaration.US_CargoReleaseType);
			AssertEquals("CR Certification indicator", true, iACECusEntryHeader.IsACECargoReleaseCertification);

			declaration.US_PGAExpeditedRelease = true;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.ACE_FDALines.AddNew();
			declaration.PGAFlags.RefreshInvoiceLinesWithPGAIndicators();
			AssertEquals("PGAExpeditedReleaseIndicator", "Y", iACECusEntryHeader.PGAExpeditedReleaseIndicator);

			declaration.US_PGAExpeditedRelease = false;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			invoiceLine.US_NMFS370Ind = OGAIndicatorList.Codes.Declared;
			AssertEquals("PGAExpeditedReleaseIndicator", "F", iACECusEntryHeader.PGAExpeditedReleaseIndicator);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
			AssertEquals("PGAExpeditedReleaseIndicator", "Y", iACECusEntryHeader.PGAExpeditedReleaseIndicator);
		}

		public void TestLowestBillDetailsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var master1 = declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";
			var master2 = declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			var house11 = master1.ChildBills.AddNew();
			house11.CU_BillNum = "HB11";
			house11.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var house21 = master2.ChildBills.AddNew();
			house21.CU_BillNum = "HB21";
			house21.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var house12 = master1.ChildBills.AddNew();
			house12.CU_BillNum = "HB12";
			house12.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];

			var bills = new List<IBillDetails>();
			bills.AddRange(((ICusEntryHeader)ensEntry).LowestBillDetails);

			AssertEquals(3, bills.Count);
			AssertEquals("MB1", bills[0].MasterBillNumber);
			AssertEquals("HB11", bills[0].HouseBillNumber);

			AssertEquals("MB1", bills[1].MasterBillNumber);
			AssertEquals("HB12", bills[1].HouseBillNumber);

			AssertEquals("MB2", bills[2].MasterBillNumber);
			AssertEquals("HB21", bills[2].HouseBillNumber);

			bills = new List<IBillDetails>();
			bills.AddRange(((IACECargoReleaseHeader)ensEntry).LowestBillDetails);
			AssertEquals(3, bills.Count);
			AssertEquals("MB1", bills[0].MasterBillNumber);
			AssertEquals("HB11", bills[0].HouseBillNumber);

			AssertEquals("MB1", bills[1].MasterBillNumber);
			AssertEquals("HB12", bills[1].HouseBillNumber);

			AssertEquals("MB2", bills[2].MasterBillNumber);
			AssertEquals("HB21", bills[2].HouseBillNumber);
		}

		public void TestLowestBillDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			var master1 = declaration.Bills.AddNew();
			master1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master1.CU_BillNum = "MB1";

			var master2 = declaration.Bills.AddNew();
			master2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			master2.CU_BillNum = "MB2";
			var house2 = master2.ChildBills.AddNew();
			house2.CU_BillNum = "HOUSEBILL2";
			house2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "V123456";
			house2.ITAndSplitDetails.AddNew().US_ITNumber = "12345678";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var ensEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];

			var bills = new List<IBillDetails>();
			bills.AddRange(((ICusEntryHeader)ensEntry).LowestBillDetails);
			AssertEquals(3, bills.Count);
			AssertEquals("MB1", bills[0].MasterBillNumber);
			AssertEquals("", bills[0].ITNumber);

			AssertEquals("MB2", bills[1].MasterBillNumber);
			AssertEquals("HOUSEBILL2", bills[1].HouseBillNumber);
			AssertEquals("V123456", bills[1].ITNumber);

			AssertEquals("MB2", bills[2].MasterBillNumber);
			AssertEquals("HOUSEBILL2", bills[2].HouseBillNumber);
			AssertEquals("12345678", bills[2].ITNumber);

			bills = new List<IBillDetails>();
			bills.AddRange(((IACECargoReleaseHeader)ensEntry).LowestBillDetails);
			AssertEquals(3, bills.Count);
			AssertEquals("MB1", bills[0].MasterBillNumber);
			AssertEquals("", bills[0].ITNumber);

			AssertEquals("MB2", bills[1].MasterBillNumber);
			AssertEquals("HOUSEBILL2", bills[1].HouseBillNumber);
			AssertEquals("V123456", bills[1].ITNumber);

			AssertEquals("MB2", bills[2].MasterBillNumber);
			AssertEquals("HOUSEBILL2", bills[2].HouseBillNumber);
			AssertEquals("12345678", bills[2].ITNumber);

			var house3 = master2.ChildBills.AddNew();
			house3.CU_BillNum = "HOUSEBILL3";
			house3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			var split1 = house3.ITAndSplitDetails.AddNew();
			split1.US_FlightNumber = "405J";
			split1.US_CarrierCode = "D0";
			var split2 = house3.ITAndSplitDetails.AddNew();
			split2.US_FlightNumber = "606JA";
			split2.US_CarrierCode = "D1";

			var seBills = new List<IBillDetails>();
			seBills.AddRange(((IACECargoReleaseHeader)ensEntry).LowestBillDetails);
			AssertEquals(4, seBills.Count);
			AssertEquals("MB1", seBills[0].MasterBillNumber);
			AssertEquals("", seBills[0].ITNumber);

			AssertEquals("MB2", seBills[1].MasterBillNumber);
			AssertEquals("HOUSEBILL2", seBills[1].HouseBillNumber);
			AssertEquals("V123456", seBills[1].ITNumber);

			AssertEquals("MB2", seBills[2].MasterBillNumber);
			AssertEquals("HOUSEBILL2", seBills[2].HouseBillNumber);
			AssertEquals("12345678", seBills[2].ITNumber);

			AssertEquals("MB2", seBills[3].MasterBillNumber);
			AssertEquals("HOUSEBILL3", seBills[3].HouseBillNumber);
			AssertEquals(ZString.Empty, seBills[3].ITNumber);
			AssertEquals(2, seBills[3].ConveyanceOrSplitDetails.Count());
		}

		public void TestHumanFriendlyReference()
		{
			AssertEquals(EntryHeader.EntryNumber, MessageAttacheeInDeclaration.HumanFriendlyReference);
		}

		public void TestIMessageAttacheeBranch()
		{
			AssertEquals(Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK), MessageAttacheeInDeclaration.Branch);
		}

		public void TestCH_EntryReleaseDate()
		{
			ErrorReporter.Clear();
			EntryHeader.CH_EntryReleaseDate = ZDateTime.Today;
			try
			{
				AssertEquals("developers exception thrown to warn.", "CH_EntryReleaseDate should not be used in US Customs. For setting Release Date please use Declaration > JE_EntryAuthorisationDate.", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestRecordTypeDescription()
		{
			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(MessageAttacheeRecordTypeDescriptions.CargoRelease, MessageAttacheeInDeclaration.RecordTypeDescription);

			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			AssertEquals(MessageAttacheeRecordTypeDescriptions.CargoRelease, MessageAttacheeInDeclaration.RecordTypeDescription);

			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals(MessageAttacheeRecordTypeDescriptions.Entry, MessageAttacheeInDeclaration.RecordTypeDescription);

			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals(MessageAttacheeRecordTypeDescriptions.InBond, MessageAttacheeInDeclaration.RecordTypeDescription);

			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			AssertEquals(MessageAttacheeRecordTypeDescriptions.SimplifiedEntry, MessageAttacheeInDeclaration.RecordTypeDescription);
		}

		public void TestUniqueAgricultureLicenseNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = "1402103030";
			line1.US_AgricultureLicNo = "LIC123";
			JobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = "2402103030";
			line2.US_AgricultureLicNo = "LIC456";
			JobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_Tariff = "3402103030";
			line3.US_AgricultureLicNo = "LIC123";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Unique Agriculture License Number", USConstants.SeeAttachedIndicator, entry.UniqueAgricultureLicenseNumber);

			line2.US_AgricultureLicNo = "LIC123";
			declaration.DoMerge();
			AssertEquals("Unique Agriculture License Number", "LIC123", entry.UniqueAgricultureLicenseNumber);

			line3.US_AgricultureLicNo = "";
			declaration.DoMerge();
			AssertEquals("Unique Agriculture License Number", "LIC123", entry.UniqueAgricultureLicenseNumber);
		}

		public void TestPrimaryITNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.ITNumber = "IT123";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CusEntryHeader entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Primary IT Number", "IT123", entry.PrimaryITNumber);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			bill1.ITNumber = "IT124";
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			bill2.ITNumber = "IT125";
			Assert(declaration.MergeManager.RequiresMerge);
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Primary IT Number", "MULTI", entry.PrimaryITNumber);
		}

		public void TestUniqueCountryOfOrigin()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Unique Country of Origin", "HK", entry.UniqueCountryOfOrigin);

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			CusEntryLine entryLine3 = entry.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfOrigin = "HK";
			JobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			line2.US_UC_NKCountryOfOrigin = "CN";
			JobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;
			line3.US_UC_NKCountryOfOrigin = "HK";
			AssertEquals("Unique Country of Origin", USConstants.MultipleValueIndicator, entry.UniqueCountryOfOrigin);

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entry.MergedLines.AddNew();
			entryLine2 = entry.MergedLines.AddNew();
			entryLine3 = entry.MergedLines.AddNew();

			invoice1 = declaration.Invoices.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfOrigin = "JP";
			line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			line2.US_UC_NKCountryOfOrigin = "KR";
			line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;
			line3.US_UC_NKCountryOfOrigin = "JP";
			AssertEquals("Unique Country of Origin", USConstants.MultipleValueIndicator, entry.UniqueCountryOfOrigin);
		}

		public void TestUniqueCountryOfExport()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Unique Country of Export", "HK", entry.UniqueCountryOfExport);

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			CusEntryLine entryLine3 = entry.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfExport = "HK";
			JobComInvoiceLine line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			line2.US_UC_NKCountryOfExport = "CN";
			JobComInvoiceLine line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;
			line3.US_UC_NKCountryOfExport = "HK";
			AssertEquals("Unique Country of Export", USConstants.MultipleValueIndicator, entry.UniqueCountryOfExport);

			declaration = Factory.New<JobDeclaration>();
			entry = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entry.MergedLines.AddNew();
			entryLine2 = entry.MergedLines.AddNew();
			entryLine3 = entry.MergedLines.AddNew();

			invoice1 = declaration.Invoices.AddNew();
			line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfExport = "JP";
			line2 = invoice1.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;
			line2.US_UC_NKCountryOfExport = "KR";
			line3 = invoice1.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;
			line3.US_UC_NKCountryOfExport = "JP";
			AssertEquals("Unique Country of Export", USConstants.MultipleValueIndicator, entry.UniqueCountryOfExport);
		}

		public void TestImporterAddressDetails()
		{
			var declaration = SetUpMergedInvoices();
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(null, entry.ImporterWrapperAddressDetails);

			var importer = CreateOrgForAddressTest("Importer", "adr1", "adr2", "cty", "ST", "1000");
			entry.RandomHeader.JZ_OH_Buyer = importer.PK;
			AssertEquals("Importer", entry.ImporterWrapperAddressDetails.CompanyName);
			var assertionMessage = "If no Customs Address - should be fallback to Main Address - ";
			AssertEquals(assertionMessage + "Address 1", "adr1", entry.ImporterWrapperAddressDetails.AddressLine1);
			AssertEquals(assertionMessage + "Address 2", "adr2", entry.ImporterWrapperAddressDetails.AddressLine2);
			AssertEquals(assertionMessage + "City", "cty", entry.ImporterWrapperAddressDetails.City);
			AssertEquals(assertionMessage + "State", "ST", entry.ImporterWrapperAddressDetails.State);
			AssertEquals(assertionMessage + "State", "1000", entry.ImporterWrapperAddressDetails.PostCode);
			AssertEquals(assertionMessage + "Importer Address line 2 should show address line", "adr2", entry.ImporterAddressLine2);
			AssertEquals(assertionMessage + "Importer city / state / postcode / country string",
						"cty   ST   1000   ", entry.ImporterCityStatePostCodeCountry);

			AddCustomsAddress(importer, "Importer Customs Address Line 1", "Importer Customs Address Line 2", "Michigan City", "IL", "59650");
			AssertEquals("Importer", entry.ImporterWrapperAddressDetails.CompanyName);
			assertionMessage = "Org has Customs Address, address details should be from Customs Adr - ";
			AssertEquals(assertionMessage + "Address 1", "Importer Customs Address Line 1", entry.ImporterWrapperAddressDetails.AddressLine1);
			AssertEquals(assertionMessage + "Address 2", "Importer Customs Address Line 2", entry.ImporterWrapperAddressDetails.AddressLine2);
			AssertEquals(assertionMessage + "City", "Michigan City", entry.ImporterWrapperAddressDetails.City);
			AssertEquals(assertionMessage + "State", "IL", entry.ImporterWrapperAddressDetails.State);
			AssertEquals(assertionMessage + "State", "59650", entry.ImporterWrapperAddressDetails.PostCode);
			AssertEquals(assertionMessage + "Importer Address line 2 should show address line", "Importer Customs Address Line 2", entry.ImporterAddressLine2);
			AssertEquals(assertionMessage + "Importer city / state / postcode / country string",
						"Michigan City   IL   59650   US", entry.ImporterCityStatePostCodeCountry);

			var importerOfRecord = CreateOrgForAddressTest("IOR Company", "adr1", "adr2", "cty", "ST", "1000");
			AddCustomsAddress(importerOfRecord, "Line 1", "Line 2", "Los Angeles California", "CA", "32000");

			declaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals("Importer of Record should be used if present", "IOR Company", entry.ImporterWrapperAddressDetails.CompanyName);
			AssertEquals("Importer Address line 2 should show address line", "Line 2", entry.ImporterAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string",
						"Los Angeles California   CA   32000   US", entry.ImporterCityStatePostCodeCountry);

			var customsAddr = importerOfRecord.Addresses.AddressesOfType(OrgConstants.AddressType.CustomsAddressOfRecord)[0];
			customsAddr.OA_Address2 = "";
			AssertEquals("IOR Address line 2 should show now show city state postcode line", "Los Angeles California   CA   32000   US", entry.ImporterAddressLine2);
			AssertEquals("IOR city / state / postcode / country string should now be blank as details have been move up 1 line ",
						ZString.Empty, entry.ImporterCityStatePostCodeCountry);

			customsAddr.OA_Address2 = "Line 2";
			customsAddr.OA_City = "";
			customsAddr.OA_PostCode = "32000";
			customsAddr.OA_State = "CA";
			AssertEquals("IOR Address line 2 should show addr. line 2", "Line 2", entry.ImporterAddressLine2);
			AssertEquals("IOR city / state / postcode / country string should be trimmed at the front", "CA   32000   US", entry.ImporterCityStatePostCodeCountry);

			customsAddr.OA_IsActive = false;
			var newAddress = importerOfRecord.Addresses.AddNew();
			newAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			newAddress.Address1 = "Poop";
			AssertEquals("Poop", entry.ImporterWrapperAddressDetails.AddressLine1);
		}

		public void TestCBPF4811ReferenceNumber()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			OrgHeader notifyParty = Factory.New<OrgHeader>();
			notifyParty.FillWithValidTestData();
			notifyParty.OH_FullName = "Importer";
			OrgCusCode notifyPartyCode = notifyParty.CustomsCodes.AddNew();
			notifyPartyCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			notifyPartyCode.OK_CustomsRegNo = "333444-33";

			var ior = Factory.New<OrgHeader>();
			ior.OH_FullName = "Importer Of Record";
			var iorCode = ior.CustomsCodes.AddNew();

			declaration.JE_OH_NotifyParty = notifyParty.PK;
			declaration.IOROrgPK = ior.PK;
			declaration.IORWrapper.ZO_NPID = "3333";
			AssertEquals("333444-33", entry.CBPF4811ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAndDeleteAll();
			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "CBN");
			AssertEquals("CBN", entry.CBPF4811ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAndDeleteAll();
			notifyParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "SSN");
			AssertEquals("SSN", entry.CBPF4811ReferenceNumber);

			notifyParty.CustomsCodes.RemoveAndDeleteAll();
			AssertEquals(ZString.Empty, entry.CBPF4811ReferenceNumber);

			declaration.JE_OH_NotifyParty = ZGuid.Empty;
			AssertEquals("3333", entry.CBPF4811ReferenceNumber);
		}

		public void TestFormattedEntryNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "ABC";
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.EntryNumber = "12345678";

			AssertEquals("Formatted Entry No", "ABC-1234567-8", entry.FormattedEntryNumber);
		}

		public void TestTotalEnteredValue()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceAmount = 15000m;
			invoice1.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice1.JZ_InvoiceNumber = "Inv-001";

			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 10000m;
			AssertEquals(10000m, invoiceLine1.JI_CustomsValue);

			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 5000m;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 3000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_InvoiceNumber = "Inv-002";

			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 3000m;

			AssertEquals("Invoice 1 entered total", 15000m, invoice1.InvoiceLineTotal);
			AssertEquals("Invoice 2 entered total", 3000m, invoice2.InvoiceLineTotal);

			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();

			CusEntryHeader ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Total Entered Value", 18000m, ensEntry.TotalEnteredValue);
			CusEntryHeader crlEntry = declaration.ActiveEntryHeaders.CargoReleaseEntry;
			AssertEquals("Total Entered Value", 18000m, crlEntry.TotalEnteredValue);

			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			AssertEquals(0m, invoiceLine1.JI_CustomsValue);
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(5000m, invoiceLine1.JI_CustomsValue);

			declaration.DoMerge();
			AssertEquals("Total Entered Value should exclude X lines", 8000m, ensEntry.TotalEnteredValue);
			AssertEquals("Total Entered Value should exclude X lines", 8000m, crlEntry.TotalEnteredValue);
		}

		public void TestTotalEnteredValueForDDP()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 11215.8m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceNumber = "Inv-001";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;

			var charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = "LCH";
			charge.J7_Amount = 12;
			charge.J7_RX_NKCurrency = "USD";
			charge.J7_IsIncludedInITOT = true;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = false;

			var charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = "OFT";
			charge2.J7_Amount = 250;
			charge2.J7_RX_NKCurrency = "USD";
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_IsDutiable = true;
			charge2.J7_IsGSTApplicable = true;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0304.41.0010";
			invoiceLine.JI_LinePrice = 6411.20m;
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_UC_NKCountryOfOrigin = "XQ";
			invoiceLine.US_SPI = "CA";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0302.14.0003";
			invoiceLine2.JI_LinePrice = 4804.60m;
			invoiceLine2.US_UC_NKCountryOfExport = "CA";
			invoiceLine2.US_UC_NKCountryOfOrigin = "XQ";
			invoiceLine2.US_SPI = "CA";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			AssertEquals("JI_CustomsValue", 6404.34m, invoiceLine.JI_CustomsValue);
			AssertEquals("JI_CustomsValue", 4799.46m, invoiceLine2.JI_CustomsValue);

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(sendingAction));
			var message = builder.PopulateMessage();
			var messageBlock = (ASESE10)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE10);
			AssertEquals("Total Entered Value", 11204m, messageBlock.EstimatedEntryValue);
		}

		public void TestLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Uninitialised declaration", "", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.US_US_NKLocationOfGoods = "FRM2";
			AssertEquals("Warehouse does not have FIRMS", "FRM2", ((ICusEntryHeader)entry).LocationOfGoods);

			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			declaration.WarehouseAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "FRM1");
			AssertEquals("Warehouse has FIRMS", "FRM1", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals("Warehouse has FIRMS", "FRM1", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals("Warehouse has FIRMS", "FRM1", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("Location of Goods should be the FIRMS code from bonded warehouse/FTZ if entered, otherwise from the front tab", "FRM1", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals("Location of Goods should be the FIRMS code from bonded warehouse/FTZ if entered, otherwise from the front tab", "FRM1", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.WarehouseDocAddress.OrganisationPK = ZGuid.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			AssertEquals("Location of Goods should be the FIRMS code from bonded warehouse/FTZ if entered, otherwise from the front tab", "FRM2", ((ICusEntryHeader)entry).LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals("Location of Goods should be the FIRMS code from bonded warehouse/FTZ if entered, otherwise from the front tab", "FRM2", ((ICusEntryHeader)entry).LocationOfGoods);
		}

		public void TestLocationOfGoodsAndName()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOCA", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.US_US_NKLocationOfGoods = "LOCA";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("LOCA/Location", entry.LocationOfGoods.ZZD_Code + "/" + entry.LocationOfGoods.ZZD_Description);
		}

		public void TestFIRMSAttributes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "LOCA", "Location", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityAddress, "111");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City, "222");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.State, "333");
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.ZIPCode, "444");
			Factory.Save();

			declaration.US_US_NKLocationOfGoods = "LOCA";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			AssertEquals("111", entry.FIRMSAddress);
			AssertEquals("222", entry.FIRMSCity);
			AssertEquals("333", entry.FIRMSState);
			AssertEquals("444", entry.FIRMSZIPCode);
		}

		public void TestMPFAndHMFChargesAmountForEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("EntryChargeTypeList", typeof(Registry.Business.Customs.US.EntryChargeTypeList), entry.EntryChargeTypeList.GetType());

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(entry);
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing].C1_ChargeAmount = 25m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.HMF].C1_ChargeAmount = 30m;

			CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);
			AssertEquals("two charges expected", 2, charges.Length);
			AssertEquals("MPF", 25m, charges[0].Amount);
			AssertEquals("HMF", 30m, charges[1].Amount);
			AssertEquals("MPF for entry", 25m, entry.MPFAmountForEntry);
			AssertEquals("HMF for entry", 30m, entry.HMFAmountForEntry);
		}

		public void TestOnlyHMFForWareHouseEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(entry);
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing].C1_ChargeAmount = 25m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.HMF].C1_ChargeAmount = 30m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseInformal].C1_ChargeAmount = 2m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.Duty].C1_ChargeAmount = 300m;

			CombineAssertions(() =>
			{
				CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);
				AssertEquals("one charges expected", 1, charges.Length);
				AssertEquals("HMF", 30m, charges[0].Amount);
				AssertEquals("MPF for entry", 25m, entry.MPFAmountForEntry);
				AssertEquals("HMF for entry", 30m, entry.HMFAmountForEntry);
				AssertEquals("Informal for entry", 2m, entry.InformalFee);
				AssertEquals("Duty for entry", 300m, entry.Duty);
			});
		}

		public void TestOnlyHMFForReWareHouseEntry()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(entry);
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing].C1_ChargeAmount = 25m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.HMF].C1_ChargeAmount = 30m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseInformal].C1_ChargeAmount = 2m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.Duty].C1_ChargeAmount = 300m;

			CombineAssertions(() =>
			{
				CustomsCharge[] charges = customsChargeGetter.GetCustomsCharges(null);
				AssertEquals("one charges expected", 1, charges.Length);
				AssertEquals("HMF", 30m, charges[0].Amount);
				AssertEquals("MPF for entry", 25m, entry.MPFAmountForEntry);
				AssertEquals("HMF for entry", 30m, entry.HMFAmountForEntry);
				AssertEquals("Duty for entry", 2m, entry.InformalFee);
				AssertEquals("Duty for entry", 300m, entry.Duty);
			});
		}

		public void TestOnlyHMFForOtherWareHouseEntryTypes()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			ICustomsCharges customsChargeGetter = ServiceLocator.GetService<ICustomsCharges>(entry);
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing].C1_ChargeAmount = 25m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.HMF].C1_ChargeAmount = 30m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.MerchandiseSurcharge].C1_ChargeAmount = 2m;
			entry.Charges[Core.Constants.USCustoms.FeeCodes.Duty].C1_ChargeAmount = 300m;

			CombineAssertions(() =>
			{
				declaration.US_EntryType = EntryTypeList.Codes.WarehouseFTZ;
				AssertEquals(EntryTypeList.Descriptions.WarehouseFTZ, 1, customsChargeGetter.GetCustomsCharges(null).Length);

				declaration.US_EntryType = EntryTypeList.Codes.TradeFair;
				AssertEquals(EntryTypeList.Descriptions.TradeFair, 1, customsChargeGetter.GetCustomsCharges(null).Length);

				declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
				AssertEquals(EntryTypeList.Descriptions.TemporaryImportationBond, 1, customsChargeGetter.GetCustomsCharges(null).Length);

				declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
				AssertEquals(EntryTypeList.Descriptions.PermanentExhibition, 1, customsChargeGetter.GetCustomsCharges(null).Length);

				declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
				AssertEquals("entry type other than warehouse", 4, customsChargeGetter.GetCustomsCharges(null).Length);
			});
		}

		public void TestCustomsClearedEventIsNotAddedForNonExport()
		{
			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration1.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration1.US_EnableENS = true;
			declaration1.US_EnableAII = true;
			declaration1.US_EnableCRL = true;

			declaration1.Invoices.AddNew();
			declaration1.InvoiceLines.AddNew();
			declaration1.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration2.Invoices.AddNew();
			declaration2.InvoiceLines.AddNew();
			declaration2.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			CusEntryHeader formalEntry = declaration1.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary)[0];
			CusEntryHeader cargoReleaseEntry = declaration1.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0];
			CusEntryHeader exportEntry = declaration2.ActiveEntryHeaders[0];

			AssertNull("No Customs Cleared log", formalEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", cargoReleaseEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", exportEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", declaration1.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", declaration2.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Export Customs Cleared log", declaration2.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));

			formalEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			cargoReleaseEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			exportEntry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;

			Factory.Save();
			AssertNull("No Customs Cleared log", formalEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", cargoReleaseEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", exportEntry.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNotNull("Export Customs Cleared log", exportEntry.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
			AssertNull("No Customs Cleared log", declaration1.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNull("No Customs Cleared log", declaration2.Logs.MostRecentLogByEventTime(Events.CustomsCleared));
			AssertNotNull("Export Customs Cleared log", declaration2.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared));
		}

		public void TestBillAndTariffLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.EntryNumber = "65000111";

			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_AdValoremTariff = "111111";

			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "222222";

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "GP891456";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_HouseBill = "HB12453";
			houseBill1.ITNumber = "65000111";

			AssertEquals(3, entry.BillAndTariffLines.Count);
			AssertEquals(typeof(FlattenEntryLineAndBillForImmediateDelivery), entry.BillAndTariffLines.TypeOfElements);
			AssertEquals("I", entry.BillAndTariffLines[0].ItBlAwbCode);
			AssertEquals("65000111", entry.BillAndTariffLines[0].ItBlAwbNumber);
			AssertEquals("M", entry.BillAndTariffLines[1].ItBlAwbCode);
			AssertEquals("GP891456", entry.BillAndTariffLines[1].ItBlAwbNumber);
			AssertEquals("H", entry.BillAndTariffLines[2].ItBlAwbCode);
			AssertEquals("HB12453", entry.BillAndTariffLines[2].ItBlAwbNumber);

			AssertEquals("I", entry.ItBlAwbCode1);
			AssertEquals("65000111", entry.ItBlAwbNumber1);
			AssertEquals("M", entry.ItBlAwbCode2);
			AssertEquals("GP891456", entry.ItBlAwbNumber2);
			AssertEquals("H", entry.ItBlAwbCode3);
			AssertEquals("HB12453", entry.ItBlAwbNumber3);
		}

		public void TestBillAndTariffLinesWhenSameHB()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.EntryNumber = "65000111";

			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_AdValoremTariff = "111111";

			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "222222";

			Bill masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_MasterBill = "00188855476";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill1.PK;
			houseBill1.CU_HouseBill = "112233";
			houseBill1.CU_NoOfPacks = 5m;
			houseBill1.CU_PackType = "PC";

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_MasterBill = "00188855465";

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill2.PK;
			houseBill2.CU_HouseBill = "112233";
			houseBill2.CU_NoOfPacks = 10m;
			houseBill2.CU_PackType = "BX";

			AssertEquals(typeof(FlattenEntryLineAndBillForImmediateDelivery), entry.BillAndTariffLines.TypeOfElements);
			AssertEquals(4, entry.BillAndTariffLines.Count);
			AssertEquals("M", entry.BillAndTariffLines[0].ItBlAwbCode);
			AssertEquals("00188855476", entry.BillAndTariffLines[0].ItBlAwbNumber);
			AssertEquals("H", entry.BillAndTariffLines[1].ItBlAwbCode);
			AssertEquals("112233", entry.BillAndTariffLines[1].ItBlAwbNumber);
			AssertEquals("5 PC", entry.ManifestQuantityAndUQ2_3461);
			AssertEquals("M", entry.BillAndTariffLines[2].ItBlAwbCode);
			AssertEquals("00188855465", entry.BillAndTariffLines[2].ItBlAwbNumber);
			AssertEquals("H", entry.BillAndTariffLines[3].ItBlAwbCode);
			AssertEquals("112233", entry.BillAndTariffLines[3].ItBlAwbNumber);
			AssertEquals("10 BX", entry.ManifestQuantityAndUQ4_3461);

			AssertEquals("M", entry.ItBlAwbCode1);
			AssertEquals("00188855476", entry.ItBlAwbNumber1);
			AssertEquals("H", entry.ItBlAwbCode2);
			AssertEquals("112233", entry.ItBlAwbNumber2);
			AssertEquals("M", entry.ItBlAwbCode3);
			AssertEquals("00188855465", entry.ItBlAwbNumber3);
			AssertEquals("H", entry.ItBlAwbCode4);
			AssertEquals("112233", entry.ItBlAwbNumber4);
		}

		public void TestAdditionalBills()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("AdditionalBills", "", entry.AdditionalBills);
		}

		public void TestAdditionalTariffs()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("AdditionalTariffs", "", entry.AdditionalTariffs);
		}

		public void TestBillAndTariffLinesContinuation()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entry.EntryNumber = "65000111";

			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			CusEntryLine entryLine3 = entry.MergedLines.AddNew();
			CusEntryLine entryLine4 = entry.MergedLines.AddNew();
			CusEntryLine entryLine5 = entry.MergedLines.AddNew();
			CusEntryLine entryLine6 = entry.MergedLines.AddNew();
			CusEntryLine entryLine7 = entry.MergedLines.AddNew();
			CusEntryLine entryLine8 = entry.MergedLines.AddNew();
			CusEntryLine entryLine9 = entry.MergedLines.AddNew();
			CusEntryLine entryLine10 = entry.MergedLines.AddNew();
			entryLine1.CL_AdValoremTariff = "111111";
			entryLine2.CL_AdValoremTariff = "222222";
			entryLine3.CL_AdValoremTariff = "33333";
			entryLine4.CL_AdValoremTariff = "44444";
			entryLine5.CL_AdValoremTariff = "5555";
			entryLine6.CL_AdValoremTariff = "66666";
			entryLine7.CL_AdValoremTariff = "77777";
			entryLine8.CL_AdValoremTariff = "88888";
			entryLine9.CL_AdValoremTariff = "99999";
			entryLine10.CL_AdValoremTariff = "101010";

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_MasterBill = "GP891456";

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill.PK;
			houseBill1.CU_HouseBill = "HB1";

			Bill subHouseBill1 = declaration.Bills.AddNew();
			subHouseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill1.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill1.CU_HouseBill = "SH1";
			subHouseBill1.ITNumber = "65000111";

			Bill subHouseBill2 = declaration.Bills.AddNew();
			subHouseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill2.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill2.CU_HouseBill = "SH2";

			Bill subHouseBill3 = declaration.Bills.AddNew();
			subHouseBill3.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill3.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill3.CU_HouseBill = "SH3";

			Bill subHouseBill4 = declaration.Bills.AddNew();
			subHouseBill4.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill4.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill4.CU_HouseBill = "SH4";

			Bill subHouseBill5 = declaration.Bills.AddNew();
			subHouseBill5.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill5.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill5.CU_HouseBill = "SH5";

			Bill subHouseBill6 = declaration.Bills.AddNew();
			subHouseBill6.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill6.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill6.CU_HouseBill = "SH6";

			Bill subHouseBill7 = declaration.Bills.AddNew();
			subHouseBill7.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill7.CU_CU_ParentBill = houseBill1.PK;
			subHouseBill7.CU_HouseBill = "SH7";

			AssertEquals(typeof(FlattenEntryLineAndBillForImmediateDelivery), entry.BillAndTariffLines.TypeOfElements);
			AssertEquals(10, entry.BillAndTariffLines.Count);
			AssertEquals("Order is important. Must be I, M, H, S (if required)", "I", entry.BillAndTariffLines[0].ItBlAwbCode);
			AssertEquals("65000111", entry.BillAndTariffLines[0].ItBlAwbNumber);
			AssertEquals("M", entry.BillAndTariffLines[1].ItBlAwbCode);
			AssertEquals("GP891456", entry.BillAndTariffLines[1].ItBlAwbNumber);
			AssertEquals("H", entry.BillAndTariffLines[2].ItBlAwbCode);
			AssertEquals("HB1", entry.BillAndTariffLines[2].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[3].ItBlAwbCode);
			AssertEquals("SH1", entry.BillAndTariffLines[3].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[4].ItBlAwbCode);
			AssertEquals("SH2", entry.BillAndTariffLines[4].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[5].ItBlAwbCode);
			AssertEquals("SH3", entry.BillAndTariffLines[5].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[6].ItBlAwbCode);
			AssertEquals("SH4", entry.BillAndTariffLines[6].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[7].ItBlAwbCode);
			AssertEquals("SH5", entry.BillAndTariffLines[7].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[8].ItBlAwbCode);
			AssertEquals("SH6", entry.BillAndTariffLines[8].ItBlAwbNumber);
			AssertEquals("S", entry.BillAndTariffLines[9].ItBlAwbCode);
			AssertEquals("SH7", entry.BillAndTariffLines[9].ItBlAwbNumber);

			AssertEquals("AdditionalBills", 10, entry.BillAndTariffLines.ITBLAWBToPrint);
			AssertEquals("AdditionalTariffs", 10, entry.BillAndTariffLines.UniqueTariffLines);

			AssertEquals("AdditionalBills", "Additional Bills", entry.AdditionalBills);
			AssertEquals("AdditionalTariffs", "ADD'L", entry.AdditionalTariffs);

			AssertEquals("I", entry.ItBlAwbCode1);
			AssertEquals("65000111", entry.ItBlAwbNumber1);
			AssertEquals("GP891456", entry.ItBlAwbNumber2);
			AssertEquals("HB1", entry.ItBlAwbNumber3);
			AssertEquals("SH1", entry.ItBlAwbNumber4);
			AssertEquals("SH2", entry.ItBlAwbNumber5);
			AssertEquals("SH3", entry.ItBlAwbNumber6);
			AssertEquals("SH4", entry.ItBlAwbNumber7);
			AssertEquals("SH5", entry.ItBlAwbNumber8);
		}

		public void TestCertificationDate()
		{
			AssertEquals(ZDate.Today, EntryHeader.CertificationDate);
		}

		public void TestArrivalDate()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(ZDate.Empty, entry.ArrivalDate);

			ZDateTime testDate = new ZDateTime(2010, 05, 27);
			declaration.US_EntryDate = testDate;
			AssertEquals(testDate, entry.ArrivalDate);
		}

		public void TestStatusDescription()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("", entry.StatusDescription);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.EntryDocumentsRequired, ZDateTime.Today);
			AssertEquals(CargoReleaseProcessingResultList.Descriptions.EntryDocumentsRequired, entry.StatusDescription);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.EntryCancelled, ZDateTime.Today.AddDays(1));
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PendingCBPReview, ZDateTime.Today.AddDays(2));
			AssertEquals(CargoReleaseProcessingResultList.Descriptions.PendingCBPReview, entry.StatusDescription);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.ManifestHoldAgriculture, ZDateTime.Today.AddDays(5));
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today.AddDays(5));
			AssertEquals(CargoReleaseProcessingResultList.Descriptions.CondReleaseGenExam + "\n" + CargoReleaseProcessingResultList.Descriptions.ManifestHoldAgriculture, entry.StatusDescription);
		}

		public void TestElectronicEntryReleaseNotificationLine1()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("", entry.ElectronicEntryReleaseNotificationLine1);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today);
			AssertEquals("", entry.ElectronicEntryReleaseNotificationLine1);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PaperlessEntry, ZDateTime.Today.AddDays(1));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today.AddDays(2));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.US_EntryMode = "";
			declaration.US_IsInvoiceByRequest = true;
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today.AddDays(2));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.US_IsInvoiceByRequest = false;
			declaration.US_EnableAII = true;
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today.AddDays(2));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseSpecDocReview, ZDateTime.Today.AddDays(3));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.CondReleaseGenExam, ZDateTime.Today.AddDays(4));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.EntryDetained, ZDateTime.Today.AddDays(5));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.OverrideToGeneral, ZDateTime.Today.AddDays(6));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.EntryDocumentsRequired, ZDateTime.Today.AddDays(7));
			AssertEquals("", entry.ElectronicEntryReleaseNotificationLine1);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.OverrideToGeneral, ZDateTime.Today.AddDays(8));
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.ReleaseRemovedFurtherDocReviewRequired, ZDateTime.Today.AddDays(8));
			AssertEquals("", entry.ElectronicEntryReleaseNotificationLine1);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.OverrideToGeneral, ZDateTime.Today.AddDays(9));
			var releaseDateUpdate = declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate, ZDateTime.Today.AddDays(9));
			releaseDateUpdate.US_ReleaseOrigin = ReleaseOriginCodeList.Codes.ReleaseDateRemoved;
			AssertEquals("", entry.ElectronicEntryReleaseNotificationLine1);

			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.OverrideToGeneral, ZDateTime.Today.AddDays(10));
			Assert(entry.ElectronicEntryReleaseNotificationLine1.StartsWith("ELECTRONIC ENTRY RELEASE NOTIFICATION"));
		}

		[TestDate(2009, 2, 22, 10, 11, 0)]
		public void TestElectronicEntryReleaseNotificationLine3()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PaperlessEntry, ZDateTime.Now);
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate, ZDateTime.Now);

			AssertEquals("Release Date: 02/22/09 10:11   EDI CUSTOMS BROKERS", entry.ElectronicEntryReleaseNotificationLine3);
		}

		[TestDate(2011, 8, 12, 10, 15, 0)]
		public void TestElectronicEntryReleaseNotificationLine3HasSameCompanyName()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PaperlessEntry, ZDateTime.Now);
			declaration.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.ReleaseDateUpdate, ZDateTime.Now);

			AssertEquals("Release Date: 08/12/11 10:15   EDI CUSTOMS BROKERS", entry.ElectronicEntryReleaseNotificationLine3);
			AssertEquals("EDI CUSTOMS BROKERS", entry.BranchName);

			var customsAddrOfRecord = Factory.Load<OrgHeader>(declaration.Branch.GB_OH_OrgProxy);
			customsAddrOfRecord.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
			customsAddrOfRecord.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals("Name should now default from Branch Proxy - Customs Address of Record", "Release Date: 08/12/11 10:15   Customs Brokers and Logistics Worldwide", entry.ElectronicEntryReleaseNotificationLine3);
			AssertEquals("Name should now default from Branch Proxy - Customs Address of Record", "Customs Brokers and Logistics Worldwide", entry.BranchName);
		}

		public void TestBoxNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("", entry.BoxNumber);

			var boxNoCollection = USCustomsDataRegistry.Instance.BoxNumbers.GetValueWithoutFallback(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty);
			BoxNumber boxNo = boxNoCollection.AddNew();
			BoxNumber boxNoDetails1 = boxNoCollection[0];
			boxNoDetails1.TransportMode = BoxNoTransportModeList.Codes.ALL;
			boxNoDetails1.BoxNo = "756";
			USCustomsDataRegistry.Instance.BoxNumbers.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, boxNoCollection);
			AssertEquals("756", entry.BoxNumber);

			BoxNumber boxNo2 = boxNoCollection.AddNew();
			BoxNumber boxNoDetails2 = boxNoCollection[1];
			boxNoDetails2.TransportMode = BoxNoTransportModeList.Codes.SEA;
			boxNoDetails2.BoxNo = "228";
			USCustomsDataRegistry.Instance.BoxNumbers.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, boxNoCollection);

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("228", entry.BoxNumber);
		}

		public void TestSingleTransBond()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.US_SuretyCode = "795";
			AssertEquals("", entry.SingleTransBond);

			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("X 795", entry.SingleTransBond);
		}

		public void TestIsAttorneyInFact()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(true, entry.IsAttorneyInFact);

			USCustomsDataRegistry.Instance.IsAttorneyInFact.SetValue(declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, entry.IsAttorneyInFact);
		}

		//CS00092174 - this value relates to the 3461 entry having been certified, not if the company is Certified to deal with ABI.
		public void TestABICertifiedText()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			CusEntryHeader crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.CH_CH_PrimeEntry = ensEntry.PK;

			AssertEquals(ZString.Empty, ensEntry.ABICertifiedText);
			AssertEquals(ZString.Empty, crlEntry.ABICertifiedText);

			ensEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals("HasCargoReleaseBeenCertified", true, declaration.HasCargoReleaseBeenCertified);
			AssertEquals("ABI Certified", ensEntry.ABICertifiedText);
			AssertEquals("ABI Certified", crlEntry.ABICertifiedText);

			ensEntry.US_CRLCertStatus = "";
			AssertEquals(ZString.Empty, ensEntry.ABICertifiedText);

			crlEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals("ABI Certified", crlEntry.ABICertifiedText);

			JobDeclaration bcrDeclaration = Factory.New<JobDeclaration>();
			bcrDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			bcrDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			bcrDeclaration.US_EnableCRL = true;

			CusEntryHeader bcrEntry = bcrDeclaration.CustomsEntryHeaders.AddNew();
			bcrEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			AssertEquals(ZString.Empty, bcrEntry.ABICertifiedText);

			bcrEntry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals("ABI Certified", bcrEntry.ABICertifiedText);
		}

		public void TestEntryNumberFormattedForImmediateDeliveryDoc()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "12345654";
			AssertEquals("12345654", entry.EntryNumberFormattedForImmediateDeliveryDoc);

			entry.EntryNumber = "02347654";
			AssertEquals("2347654", entry.EntryNumberFormattedForImmediateDeliveryDoc);
		}

		public void TestBox29ExamSite()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Box29 Other Data Exam Site", "", entry.Box29ExamSite);

			var referenceTesthelper = new UniversalReferenceTestDataHelper(Factory);
			referenceTesthelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			referenceTesthelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "N598", "WANDO TERMINAL", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var examSite = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);

			declaration.US_US_NKCentralizedExamSite = examSite.ZZD_Code;
			AssertEquals("Box29 Other Data", "Examination Site: " + examSite.ZZD_Code + " " + examSite.ZZD_Description, entry.Box29ExamSite);
		}

		public void TestBox29OtherData()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Box29 Other Data", "", entry.Box29OtherData);

			declaration.US_Box29Text = "Box 29 Other Data";
			AssertEquals("Box29 Other Data", "Box 29 Other Data\r\n\r\n", entry.Box29OtherData);

			declaration.US_Box29Text = "";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU1234567";

			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "OCLU8839485";

			AssertEquals("Box29 Other Data", "Containers: OCLU1234567, OCLU8839485", entry.Box29OtherData);

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			CusEntryLine entryLine1 = entry.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.LaceyActLines.AddNew();
			invoiceLine2.LaceyActLines.AddNew();

			declaration.IOROrgPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.IOR.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CTPAT, "12345");

			AssertEquals("Box29 Other Data", @"PPQ 505-ABI
CTPAT CERTIFIED
Containers: OCLU1234567, OCLU8839485", entry.Box29OtherData);
		}

		public void TestBox29OtherDataWithAdjustments()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Box29 Other Data", "", entry.Box29OtherData);

			declaration.US_Box29Text = @"TRANSFER BY: ABC TRUCKING
ENTRY BOND [ ] CARRIER BOND [ ] CHL BOND [ ]
DFS BOND [ ]
PAIRED CITIES REQ: YES [ ]  NO [X]";
			AssertEquals("Box29 Other Data", "TRANSFER BY: ABC TRUCKING\r\nENTRY BOND [ ] CARRIER BOND [ ] CHL BOND [ ]\r\nDFS BOND [ ]\r\nPAIRED CITIES REQ: YES [ ]  NO [X]\r\n\r\n", entry.Box29OtherData);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			for (int i = 1; i < 22; i++)
			{
				CreateTestContainer(declaration, i);
			}

			AssertEquals("Box29 Other Data with some containers", "TRANSFER BY: ABC TRUCKING\r\nENTRY BOND [ ] CARRIER BOND [ ] CHL BOND [ ]\r\nDFS BOND [ ]\r\nPAIRED CITIES REQ: YES [ ]  NO [X]\r\n\r\nContainers: OCLU1234501, OCLU1234502, OCLU1234503, OCLU1234504, OCLU1234505, OCLU1234506, OCLU1234507, OCLU1234508, OCLU1234509, OCLU1234510, OCLU1234511, OCLU1234512, OCLU1234513, OCLU1234514, OCLU1234515, OCLU1234516, OCLU1234517, OCLU1234518, OCLU1234519, OCLU1234520, OCLU1234521", entry.Box29OtherData);

			CreateTestContainer(declaration, 1);

			AssertEquals("Adding 1 more container will cause containers to print on overflow page not in Box29 data", "TRANSFER BY: ABC TRUCKING\r\nENTRY BOND [ ] CARRIER BOND [ ] CHL BOND [ ]\r\nDFS BOND [ ]\r\nPAIRED CITIES REQ: YES [ ]  NO [X]\r\n\r\n", entry.Box29OtherData);
		}

		public void TestBox29OtherDataWithAdjustmentsIfNormalCasePrinted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("Box29 Other Data", "", entry.Box29OtherData);

			declaration.US_Box29Text = @"Transfer by: ABC Trucking
Entry Bond [ ] Carrier Bond [ ] CHL Bond [ ]
DFS Bond [ ]
Paired cities req: Yes [ ]  No [X]";
			AssertEquals("Box29 Other Data", "Transfer by: ABC Trucking\r\nEntry Bond [ ] Carrier Bond [ ] CHL Bond [ ]\r\nDFS Bond [ ]\r\nPaired cities req: Yes [ ]  No [X]\r\n\r\n", entry.Box29OtherData);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			for (int i = 1; i < 22; i++)
			{
				CreateTestContainer(declaration, i);
			}

			AssertEquals("Box29 Other Data with some containers", "Transfer by: ABC Trucking\r\nEntry Bond [ ] Carrier Bond [ ] CHL Bond [ ]\r\nDFS Bond [ ]\r\nPaired cities req: Yes [ ]  No [X]\r\n\r\nContainers: OCLU1234501, OCLU1234502, OCLU1234503, OCLU1234504, OCLU1234505, OCLU1234506, OCLU1234507, OCLU1234508, OCLU1234509, OCLU1234510, OCLU1234511, OCLU1234512, OCLU1234513, OCLU1234514, OCLU1234515, OCLU1234516, OCLU1234517, OCLU1234518, OCLU1234519, OCLU1234520, OCLU1234521", entry.Box29OtherData);

			CreateTestContainer(declaration, 1);

			AssertEquals("Adding 1 more container should still cause containers to print on overflow page not in Box29 data", "Transfer by: ABC Trucking\r\nEntry Bond [ ] Carrier Bond [ ] CHL Bond [ ]\r\nDFS Bond [ ]\r\nPaired cities req: Yes [ ]  No [X]\r\n\r\n", entry.Box29OtherData);
		}

		public void TestBox29OtherDataWithLargCharAdjustments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			declaration.US_Box29Text = @"iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii";

			for (int i = 1; i < 27; i++)
			{
				CreateTestContainer(declaration, i);
			}

			AssertEquals("Box29 Other Data with some containers", "iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii\r\n\r\nContainers: OCLU1234501, OCLU1234502, OCLU1234503, OCLU1234504, OCLU1234505, OCLU1234506, OCLU1234507, OCLU1234508, OCLU1234509, OCLU1234510, OCLU1234511, OCLU1234512, OCLU1234513, OCLU1234514, OCLU1234515, OCLU1234516, OCLU1234517, OCLU1234518, OCLU1234519, OCLU1234520, OCLU1234521, OCLU1234522, OCLU1234523, OCLU1234524, OCLU1234525, OCLU1234526", entry.Box29OtherData);

			declaration.US_Box29Text = @"gmqwwwwwwwwwwiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii";
			AssertEquals("large characters will cause containers to print on overflow page not in Box29 data", "gmqwwwwwwwwwwiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii\r\n\r\n", entry.Box29OtherData);

			declaration.US_Box29Text = @"GMQWWWWWWWWWWIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII";
			AssertEquals("large characters will cause containers to print on overflow page not in Box29 data", "GMQWWWWWWWWWWIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII\r\n\r\n", entry.Box29OtherData);
		}

		public void TestContainersContinued()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();

			var examSite = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today);
			Declaration.US_Box29IncludeContainers = true;

			declaration.US_Box29Text = "TEST";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;

			for (int i = 1; i < 43; i++)
			{
				CreateTestContainer(declaration, i);
			}

			AssertEquals("no overflow container count", 0, entry.ContainersContinued.Count);
			AssertEquals("Box29 Other Data with containers", "TEST\r\n\r\nContainers: OCLU1234501, OCLU1234502, OCLU1234503, OCLU1234504, OCLU1234505, OCLU1234506, OCLU1234507, OCLU1234508, OCLU1234509, OCLU1234510, OCLU1234511, OCLU1234512, OCLU1234513, OCLU1234514, OCLU1234515, OCLU1234516, OCLU1234517, OCLU1234518, OCLU1234519, OCLU1234520, OCLU1234521, OCLU1234522, OCLU1234523, OCLU1234524, OCLU1234525, OCLU1234526, OCLU1234527, OCLU1234528, OCLU1234529, OCLU1234530, OCLU1234531, OCLU1234532, OCLU1234533, OCLU1234534, OCLU1234535, OCLU1234536, OCLU1234537, OCLU1234538, OCLU1234539, OCLU1234540, OCLU1234541, OCLU1234542", entry.Box29OtherData);

			CreateTestContainer(declaration, 1);

			AssertEquals("Containers should overflow to continuation page with 4 lines of container data", 6, entry.ContainersContinued.Count);
			AssertEquals("Box29 Other Data with too many containers now prints on overflow page not in Box 29", "TEST\r\n\r\n", entry.Box29OtherData);

			declaration.US_Box29Text = "";
			AssertEquals("Containers when no examination line can again print in Box 29 field", 0, entry.ContainersContinued.Count);
			AssertEquals("Box29 Other Data with containers when no examination site", "Containers: OCLU1234501, OCLU1234502, OCLU1234503, OCLU1234504, OCLU1234505, OCLU1234506, OCLU1234507, OCLU1234508, OCLU1234509, OCLU1234510, OCLU1234511, OCLU1234512, OCLU1234513, OCLU1234514, OCLU1234515, OCLU1234516, OCLU1234517, OCLU1234518, OCLU1234519, OCLU1234520, OCLU1234521, OCLU1234522, OCLU1234523, OCLU1234524, OCLU1234525, OCLU1234526, OCLU1234527, OCLU1234528, OCLU1234529, OCLU1234530, OCLU1234531, OCLU1234532, OCLU1234533, OCLU1234534, OCLU1234535, OCLU1234536, OCLU1234537, OCLU1234538, OCLU1234539, OCLU1234540, OCLU1234541, OCLU1234542, OCLU1234501", entry.Box29OtherData);
		}

		public void TestPPQForm368MarksBillOfLadingAndContainerNumber()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("PPQForm368MarksBillOfLadingAndContainerNumber", "", entry.PPQForm368MarksBillOfLadingAndContainerNumber);

			declaration.US_PPQForm368Box13A = "Box 13 Other Data";
			AssertEquals("PPQForm368MarksBillOfLadingAndContainerNumber", "Box 13 Other Data", entry.PPQForm368MarksBillOfLadingAndContainerNumber);
		}

		public void TestPPQForm368QuantityAndNetWeight()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("PPQForm368QuantityAndNetWeight", "", entry.PPQForm368QuantityAndNetWeight);

			declaration.US_PPQForm368Box13B = "Box 13 Other Data";
			AssertEquals("PPQForm368QuantityAndNetWeight", "Box 13 Other Data", entry.PPQForm368QuantityAndNetWeight);
		}

		public void TestPPQForm368Commodity()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("PPQForm368Commodity", "", entry.PPQForm368Commodity);

			declaration.US_PPQForm368Box13C = "Box 13 Other Data";
			AssertEquals("PPQForm368Commodity", "Box 13 Other Data", entry.PPQForm368Commodity);
		}

		public void TestExportUltimateConsignee()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Buyer = org.PK;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(invoice.US_ExportUltimateConsignee, entry.ExportUltimateConsignee);
			AssertEquals(org.PK, entry.ExportUltimateConsignee.ZO_OH_Organisation);
		}

		public void TestImporterOfRecordNumberForDocument_CusEntryHeader()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;

			var importer = Factory.New<OrgHeader>();
			Declaration.IOROrgPK = importer.PK;
			AssertEquals("Nothing print cuz importer has no register number", ZString.Empty, EntryHeader.ImporterOfRecordNumberForDocument);

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "987");
			AssertEquals("EIN should be printed", "987", EntryHeader.ImporterOfRecordNumberForDocument);

			importer.CustomsCodes.DeleteAll();

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123");
			AssertEquals("SSN should not be printed", ZString.Empty, EntryHeader.ImporterOfRecordNumberForDocument);

			Declaration.PrintSocialSecurityNumberOnDocument = true;
			AssertEquals("SSN should be printed", "123", EntryHeader.ImporterOfRecordNumberForDocument);
		}

		public void TestIntermediateConsignee()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Consignee = org.PK;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(invoice.US_IntermediateConsignee, entry.IntermediateConsignee);
			AssertEquals(org.PK, entry.IntermediateConsignee.ZO_OH_Organisation);
		}

		public void TestBranchPhone()
		{
			Declaration.Branch.OrgProxy.MainAddress.OA_Phone = "ORG PHONE";
			Declaration.Branch.OrgProxy.MainAddress.OA_Fax = "ORG FAX";
			AssertEquals("PHONE: ORG PHONE  FAX: ORG FAX", EntryHeader.BranchPhone);

			Declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			Declaration.Branch.GB_Phone = "BRANCH PHONE";
			Declaration.Branch.GB_Fax = "BRANCH FAX";
			AssertEquals("PHONE: BRANCH PHONE  FAX: BRANCH FAX", EntryHeader.BranchPhone);
		}

		public void TestBox27PhoneFax()
		{
			Declaration.Branch.OrgProxy.MainAddress.OA_Phone = "ORG PHONE";
			Declaration.Branch.OrgProxy.MainAddress.OA_Fax = "ORG FAX";
			AssertEquals("ORG PHONE  FAX: ORG FAX", EntryHeader.Box27PhoneFax);

			Declaration.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			Declaration.Branch.GB_Phone = "BRANCH PHONE";
			Declaration.Branch.GB_Fax = "BRANCH FAX";
			AssertEquals("BRANCH PHONE  FAX: BRANCH FAX", EntryHeader.Box27PhoneFax);
		}

		public void TestBranchAddress()
		{
			Declaration.Branch.OrgProxy.MainAddress.OA_Address1 = "Address Line 1";
			Declaration.Branch.OrgProxy.MainAddress.OA_Address2 = "Address Line 2";
			Declaration.Branch.OrgProxy.MainAddress.OA_City = "Nashville";
			Declaration.Branch.OrgProxy.MainAddress.OA_State = "NC";
			Declaration.Branch.OrgProxy.MainAddress.OA_RL_NKRelatedPortCode = "USXAZ";
			Declaration.Branch.OrgProxy.MainAddress.OA_PostCode = "37211";
			AssertEquals("Branch address should include state", "Address Line 1, Address Line 2 Nashville, NC 37211", EntryHeader.BranchAddress);

			Declaration.Branch.OrgProxy.MainAddress.OA_State = "";
			AssertEquals("Branch address when state not entered", "Address Line 1, Address Line 2 Nashville 37211", EntryHeader.BranchAddress);

			AssertEquals("Branch street address", "Address Line 1, Address Line 2", EntryHeader.BranchStreetAddress);
			Declaration.Branch.OrgProxy.MainAddress.OA_Address2 = "";
			AssertEquals("Branch street address", "Address Line 1", EntryHeader.BranchStreetAddress);

			var customsAddrOfRecord = Factory.Load<OrgHeader>(Declaration.Branch.GB_OH_OrgProxy);
			customsAddrOfRecord.Addresses[0].OA_CompanyNameOverride = "Customs Brokers and Logistics Worldwide";
			customsAddrOfRecord.Addresses[0].OA_Address1 = "Nashville Branch Office";
			customsAddrOfRecord.Addresses[0].OA_Address2 = "115 JonesTown Street";
			customsAddrOfRecord.Addresses[0].OA_City = "Nashville";
			customsAddrOfRecord.Addresses[0].OA_PostCode = "55555";
			customsAddrOfRecord.Addresses[0].OA_State = "NC";
			customsAddrOfRecord.Addresses[0].OA_RL_NKRelatedPortCode = "USRXC";
			customsAddrOfRecord.Addresses[0].OA_Phone = "+1 22 456 789";
			customsAddrOfRecord.Addresses[0].OA_Fax = "+1 22 456 799";
			customsAddrOfRecord.Addresses[0].AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			AssertEquals("Address should now default from Branch Proxy - Customs Address of Record", "Nashville Branch Office, 115 JonesTown Street Nashville, NC 55555", EntryHeader.BranchAddress);
			AssertEquals("Phone/Fax details should now default from Branch Proxy - Customs Address of Record", "PHONE: +1 22 456 789  FAX: +1 22 456 799", EntryHeader.BranchPhone);
		}

		public void TestTotalPayableADD()
		{
			USCACCase addCase1 = Factory.New<USCACCase>();
			addCase1.U5_CaseNumber = "A570204006";
			addCase1.U5_ISOCountryCode = "CN";
			addCase1.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8205595510";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "82014060";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8465960015";
			addCase1.CaseTariffs.AddNew().U9_TariffNumber = "8205203000";
			var rate1 = addCase1.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.89m;
			rate1.U6_EffectiveDate = ZDateTime.Today;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 24000m;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8205.20.3000";   // .4c per unit + 6.1% = 24000 * .061 = $1464 (this is sent)
			line2.JI_Description = "line with the highest duty rate";
			line2.JI_CustomsQuantity = 0m;
			line2.JI_LinePrice = 16000m;
			line2.US_ADDCaseNo = "A570204006";
			line2.US_ADDDepositValue = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (24000 * .054) = $1296
			line3.JI_Description = "line with the lower duty rate";
			line3.JI_CustomsQuantity = 0m;//due to this
			line3.JI_LinePrice = 8000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			ICusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("Payable ADD", 30240.00m, entry.PayableADDDuty);
			AssertEquals("Total ADD", 30240.00m, declaration.CustomsEntryHeaders[0].TotalAntidumpingDuty);
		}

		public void TestHasVisaOrQuotaLines()
		{
			JobDeclaration declaration = CreateVisaDeclaration();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals("pre-condition: VisaOrQuotaLinesMerchandiseValue", 3066m, entry.MergedLines.VisaOrQuotaLinesMerchandiseValue);
			AssertEquals("HasVisaOrQuotaLines", true, entry.HasVisaOrQuotaLines);
		}

		public void TestIUSCustomsChargeEntry()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);

			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2009, 6, 2);
			Factory.Save();//To generate an entry number

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			statement.B2_DueDate = new ZDateTime(2009, 6, 1);
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;

			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			statementLine.B3_EntryFilerCode = "XJ5";

			Factory.Save();//To be able to load using db only query

			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CH_Status = "CCO";
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2009, 6, 2);
			IUSCustomsChargeEntry chargeEntry = entryHeader;

			AssertEquals("Entry FilerCode", "XJ5", chargeEntry.EntryFilerCode);
			AssertEquals("EntryNumber", declaration.ImportEntryNumber, chargeEntry.EntryNumber);
			AssertEquals("Payment Type", PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter, chargeEntry.PaymentType);
			AssertEquals("DueDate is 10 business days after (in this case) release date", new ZDate(2009, 6, 16), chargeEntry.DueDate);
		}

		public void TestIsPaidByImporterOrBroker()
		{
			JobDeclaration declaration = new DeclarationTestHelper().GetMergedDutiableDeclaration(Factory);
			declaration.US_EntryFilerCode = "XJ5";
			Factory.Save();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			IUSCustomsChargeEntry chargeEntry = declaration.CustomsEntryHeaders[0];

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("IsPaidByBroker", false, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", false, chargeEntry.IsPaidByImporter);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("IsPaidByBroker", false, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", true, chargeEntry.IsPaidByImporter);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			AssertEquals("IsPaidByBroker", true, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", false, chargeEntry.IsPaidByImporter);

			CusStatementHeader statement = Factory.New<CusStatementHeader>();
			CusStatementLine statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			statementLine.B3_EntryFilerCode = "XJ5";
			AssertEquals("PreCondition", entry.Declaration, statementLine.Declaration);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			AssertEquals("IsPaidByBroker", true, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", false, chargeEntry.IsPaidByImporter);

			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			AssertEquals("IsPaidByBroker", true, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", false, chargeEntry.IsPaidByImporter);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			statement.B2_PaymentParty = PaymentPartyList.Codes.Broker;
			AssertEquals("IsPaidByBroker", false, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", true, chargeEntry.IsPaidByImporter);

			statement.B2_PaymentParty = PaymentPartyList.Codes.Importer;
			AssertEquals("IsPaidByBroker", false, entry.IsPaidByBroker);
			AssertEquals("IsPaidByImporter", true, chargeEntry.IsPaidByImporter);
		}

		public void TestICBPEDIMessageMessageTextNumberPlaceHolderFillerMembers()
		{
			DeclarationTestHelper.SetupBranchSpecificFormalEntryNumber("XJ5");
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			DbConnection connection = ((IDbConnected)Factory).Connection;
			try
			{
				connection.BeginTransaction();
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				declaration.US_EntryFilerCode = "XJ5";
				CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
				MQEDIMessage message = Factory.New<MQEDIMessage>();
				string messageData = "INBONDNUMBERPLACEHOLDER:{1}{0}USENTRYFILERENTRYNUMBERPLACEHOLDER:{2}{0}USENTRYNUMBERPLACEHOLDER:{3}END";
				string defaultMessageText = string.Format(messageData, System.Environment.NewLine, MQEDIMessage.InBondNumberPlaceHolder, MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, MQEDIMessage.USEntryNumberPlaceHolder);
				message.EM_MessageText = defaultMessageText;
				ICBPEDIMessageMessageTextNumberPlaceHolderFiller filler = entry;
				AssertEquals("", entry.EntryNumber);
				filler.Fill(message);
				AssertNotEquals("", entry.EntryNumber);
				ZString inBondNumber = entry.EntryNumber;
				AssertEquals(string.Format(messageData, System.Environment.NewLine, entry.EntryNumber.PadRight(12), MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, MQEDIMessage.USEntryNumberPlaceHolder), message.EM_MessageText);

				entry.EntryNumber = ZString.Empty;
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
				message.EM_MessageText = defaultMessageText;
				filler.Fill(message);
				AssertNotEquals("", entry.EntryNumber);
				AssertNotEquals(inBondNumber, entry.EntryNumber);
				AssertEquals(string.Format(messageData, System.Environment.NewLine, MQEDIMessage.InBondNumberPlaceHolder, "XJ5" + entry.EntryNumber, entry.EntryNumber), message.EM_MessageText);
				ZString reconEntryNumber = entry.EntryNumber;

				entry.EntryNumber = ZString.Empty;
				entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				message.EM_MessageText = defaultMessageText;
				filler.Fill(message);
				AssertNotEquals("", entry.EntryNumber);
				AssertNotEquals(inBondNumber, entry.EntryNumber);
				AssertNotEquals(reconEntryNumber, entry.EntryNumber);
				AssertEquals(string.Format(messageData, System.Environment.NewLine, MQEDIMessage.InBondNumberPlaceHolder, MQEDIMessage.USEntryFilerEntryNumberPlaceHolder, entry.EntryNumber), message.EM_MessageText);
				ZString ensEntryNumber = entry.EntryNumber;
			}
			finally
			{
				connection.RollbackTransaction();
			}
		}

		public void TestProcessingDistrictPortForBCR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2786", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2720", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var coll = new EntryProcessingPortsMappingCollection();
			var mapping = coll.AddNew();
			mapping.EntryPort = "2786";
			mapping.ProcessingPort = "2720";

			DataRegistry.Business.USCustomsDataRegistry.Instance.EntryProcessingPortMappings.SetValue(Declaration.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, coll);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_SchDEntry = "2786";
			AssertEquals("Recondition: declaration Processing District Port", "2720", declaration.ProcessingDistrictPort);

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Cargo Release Entry", true, entry.IsCargoRelease);
			AssertEquals("Should be the same as declaration Processing District Port", "2720", entry.ProcessingDistrictPort);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.US_SchDEntry = "3409";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.BCR;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Border Cargo Release Entry", true, entry.IsBorderCargoRelease);
			AssertEquals("Should be the same as declaration US_SchDEntry port", "3409", entry.ProcessingDistrictPort);
		}

		public void TestPSCReasonCodes()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var pscReasonCode = Factory.New<PSCReasonCusCodeData>();
			pscReasonCode.CY_ParentID = entry.PK;
			pscReasonCode.CY_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;
			pscReasonCode.CY_Data = "Some data";

			Factory.Save();

			AssertEquals("Load the same object saved", pscReasonCode, entry.PSCReasonCodes);
		}

		public void TestConsolidatedFilterCodeAndEntryNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_DeclarationReference = "B00100001";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration2.JE_DeclarationReference = "B00100002";
			declaration2.US_EntryFilerCode = "SV9";
			declaration2.DecEntryNumber = "12345678";

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration3.US_ConsolidatedJobNumber = "B00100001";
			var entry = declaration3.CustomsEntryHeaders.AddNew();
			AssertEquals("ConsolidatedJobNumber", ZString.Empty, ((IACECargoReleaseHeader)entry).ConsolidatedFilterCodeAndEntryNumber);

			declaration3.US_ConsolidatedJobNumber = "B00100002";
			entry = declaration3.CustomsEntryHeaders.AddNew();
			AssertEquals("ConsolidatedJobNumber", "SV912345678", ((IACECargoReleaseHeader)entry).ConsolidatedFilterCodeAndEntryNumber);
		}

		public void TestPSCExplanation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var pscExplanation = Factory.New<PSCExplanationCusAddInfo>();

			pscExplanation.B7_ParentID = entry.PK;
			pscExplanation.B7_ParentTableCode = CusEntryHeaderSchema.Constants.Prefix;

			Factory.Save();

			AssertEquals("Load the same object saved", pscExplanation, entry.PSCExplanation);
		}

		public void TestFDARecapDocumentProperties()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "~US";
			usCompany.GC_Name = "US TEST COMPANY, Inc";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = usCompany.Branches.AddNew();
			usBranch.GB_Code = "~US";
			Factory.Save();

			var declaration = DeclarationTestHelper.GetMergedDeclarationWithFDALines(Factory);
			declaration.JE_GB = usBranch.PK;

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			AssertEquals(4, entry.FDARecapLines.Count);
			AssertEquals("E123", entry.VoyageNumber);
			AssertEquals("APL VESSEL TESTING", entry.VesselName);
			AssertEquals(ZDateTime.Today, entry.ReleaseDate);
			AssertEquals(FDAEntryLevelDispositionCodeList.Codes._02 + "-" + FDAEntryLevelDispositionCodeList.Descriptions._02, entry.FDAStatus);
			AssertEquals("US TEST COMPANY, Inc", entry.BrokerName);
		}

		public void TestLiquidationDate()
		{
			var liquidation = Factory.New<CusLiquidation>();
			liquidation.B8_SystemCreateDate = new ZDateTime(2009, 11, 21);
			liquidation.B8_LiquidationDate = new ZDateTime(2009, 11, 20);
			liquidation.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code04;
			Declaration.Liquidations.Add(liquidation);

			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_SystemCreateDate = new ZDateTime(2009, 11, 24);
			liquidation2.B8_LiquidationDate = new ZDateTime(2009, 11, 23);
			liquidation2.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code01;
			Declaration.Liquidations.Add(liquidation2);

			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_SystemCreateDate = new ZDateTime(2009, 11, 22);
			liquidation3.B8_LiquidationDate = new ZDateTime(2009, 11, 21);
			Declaration.Liquidations.Add(liquidation3);

			AssertEquals("Liquidation date should comes from most recent create date", new ZDateTime(2009, 11, 23), EntryHeader.LiquidationDate);
			AssertEquals("Liquidation type should comes from most recent liquidation", LiquidationTypeCodeList.Codes.Code01, EntryHeader.LiquidationType);
		}

		public void TestILiquidationProviderMembers()
		{
			var entry = Factory.New<CusEntryHeader>();
			var liquidationProvider = (ILiquidationProvider)entry;
			liquidationProvider.SetAnticipatedLiquidatedDuty(100m);
			liquidationProvider.SetAnticipatedLiquidationDate(new ZDateTime(2019, 07, 10));
			AssertEquals(100m, entry.US_ALDuty);
			AssertEquals(new ZDateTime(2019, 07, 10), entry.US_ALDate);
		}

		public void TestPTTCarrier()
		{
			var pptOrg = Factory.New<OrgHeader>();
			pptOrg.OH_Code = "PPTOrg";
			pptOrg.OH_FullName = "Test PTT Org";
			pptOrg.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123910-00001");
			pptOrg.Addresses.AddNewMainAddress();

			Factory.Save();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			Declaration.DeliveryOrPickupCartageCoPK = pptOrg.PK;

			AssertEquals("PPTOrg in Box 44", pptOrg.OH_FullName, EntryHeader.FTZCartman);
		}

		public void TestIsConsumptionFTZEntryType()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EnableENS = true;
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("IsConsumptionFTZEntryType", true, EntryHeader.IsConsumptionFTZEntryType);
			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			AssertEquals("IsConsumptionFTZEntryType", false, EntryHeader.IsConsumptionFTZEntryType);
			EntryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			AssertEquals("IsConsumptionFTZEntryType", true, EntryHeader.IsConsumptionFTZEntryType);
			Declaration.US_EntryType = EntryTypeList.Codes.Baggage;
			AssertEquals("IsConsumptionFTZEntryType", false, EntryHeader.IsConsumptionFTZEntryType);
			Declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			AssertEquals("IsConsumptionFTZEntryType", true, EntryHeader.IsConsumptionFTZEntryType);
		}

		public void TestPermitClosedWhenEntrySummaryIsAccepted()
		{
			// - create FTZ declaration
			var declaration = JobDeclarationTest.CreateFTZWeeklyEstimateDeclaration(Factory, "40000007");

			// - create permit
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			AssertEquals(1, declaration.FindRelatedPermits().Count);
			var permit = declaration.FindRelatedPermits()[0];

			// - create entry summary
			declaration.US_EnableENS = true;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			// - permit is not closed yet
			AssertEquals(false, permit.CPH_IsClosed);

			// - permit is closed when Entry Summary is accepted
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;

			// - permit is closed
			AssertEquals(true, permit.CPH_IsClosed);
		}

		public void TestPermitClosedWhenCargoReleaseDeletionIsAccepted()
		{
			// - create FTZ declaration
			var declaration = JobDeclarationTest.CreateFTZWeeklyEstimateDeclaration(Factory, "40000007");

			// - create permit
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2017, 10, 14);
			Factory.Save();
			AssertEquals(1, declaration.FindRelatedPermits().Count);
			var permit = declaration.FindRelatedPermits()[0];

			// - permit is not closed yet
			AssertEquals(false, permit.CPH_IsClosed);

			// - permit is closed when CargoRelease deletion is accepted
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseDelete;

			// - permit is closed
			AssertEquals(true, permit.CPH_IsClosed);
		}

		public void TestMarkActionsTaken()
		{
			var declaration = GetFormalJobDeclaration();
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			LinkMessageForJobDeclaration(declaration.ActiveEntryHeaders.EntrySummaryEntry);
			AssertEquals(1, declaration.ENSStatusNotifications.Count);
			SetENSStatusNotification(declaration.ENSStatusNotifications[0]);
			AssertMarkActionsTaken(declaration);

			var reconDeclaration = GetReconJobDeclaration();
			AssertNotNull(reconDeclaration.ActiveEntryHeaders.ReconciliationEntry);
			LinkMessageForJobDeclaration(reconDeclaration.ActiveEntryHeaders.ReconciliationEntry);
			AssertEquals(1, reconDeclaration.ENSStatusNotifications.Count);
			SetENSStatusNotification(reconDeclaration.ENSStatusNotifications[0]);
			AssertMarkActionsTaken(reconDeclaration);
		}

		public void TestUnableToDisableENSWhenReject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			header.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryDelete;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorEntrySummaryReplace;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorElectronicInvoiceDelete;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorElectronicInvoiceOriginal;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorElectronicInvoiceReplace;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseDelete;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseReplace;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ErrorACECargoReleaseUpdate;
			Assert(!header.IsOKToBeDeactivated);

			header.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Assert(header.IsOKToBeDeactivated);

			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			Assert(header.IsOKToBeDeactivated);
		}

		public void TestIACECargoReleaseHeaderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount2 = 100m;
			declaration.US_BondProducerAccNo2 = "897423";
			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123910-00001");
			declaration.IOROrgPK = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			DeclarationTestHelper.SetupOrganizationsForACEInvoice(invoiceHeader, Factory);
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.ACECargoRelease)[0];
			var iHeader = (IACECargoReleaseHeader)seEntry;
			AssertEquals("ImporterOfRecordNumber", "123910-00001", iHeader.ImporterOfRecordNumber);
			AssertEquals("ImporterOfRecordType", EntityIdentifierQualifierList.Codes.CBPAssignedNumber, iHeader.ImporterOfRecordType);
			AssertEquals("Bills - only lowest bills without children", 3, iHeader.LowestBillDetails.Count());
			AssertEquals("Entities: Manufacturer, Ultimate Consignee, Seller, Sold To Party, Exporter, Shipper, Distributor, Packager", 9, iHeader.Entities.Count());
			AssertEquals("", iHeader.RailReferenceNumber);
			AssertEquals("ADDCVDBondType", BondTypeList.Codes.SingleTransactionBond, iHeader.ADDCVDBondType);
			AssertEquals("ADDCVDSingleTransactionBondAmount", 100m, iHeader.ADDCVDSingleTransactionBondAmount);
			AssertEquals("ADDCVDSingleTransactionBondAccNo", "897423", iHeader.ADDCVDSingleTransactionBondAccNo);
			AssertEquals("EstimatedDateOfArrivalForEntryType86", ZDateTime.Empty, iHeader.EstimatedDateOfArrivalForEntryType86);

			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(Common.UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN, "12345678");
			AssertEquals("12345678", iHeader.RailReferenceNumber);

			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.InvoiceLines.AddNew();
			invoiceHeader2.JZ_OA_ConsigneeAddress = invoiceHeader.JZ_OA_ConsigneeAddress;
			invoiceHeader2.JZ_OA_ManufacturerAddress = invoiceHeader.JZ_OA_ManufacturerAddress;
			invoiceHeader2.JZ_OA_SellerAddress = invoiceHeader.JZ_OA_SellerAddress;
			invoiceHeader2.JZ_OA_SoldToPartyAddress = invoiceHeader.JZ_OA_SoldToPartyAddress;
			invoiceHeader2.JZ_OA_ShipToPartyAddress = invoiceHeader.JZ_OA_ShipToPartyAddress;
			invoiceHeader2.JZ_OA_ExporterAddress = invoiceHeader.JZ_OA_ExporterAddress;
			invoiceHeader2.JZ_OA_ShipperAddress = invoiceHeader.JZ_OA_ShipperAddress;
			invoiceHeader2.JZ_OA_DistributorAddress = invoiceHeader.JZ_OA_DistributorAddress;
			invoiceHeader2.JZ_OA_PackagerAddress = invoiceHeader.JZ_OA_PackagerAddress;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Entities: Manufacturer, Ultimate Consignee, Seller, Sold To Party, Exporter, Shipper, Distributor, Packager", 9, iHeader.Entities.Count());
			AssertEquals(invoiceHeader.ConsigneeOrgAddress.OH_FullName, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.Consignee).CompanyName);
			AssertEquals(invoiceHeader.Seller.OH_FullName, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.SellingParty).CompanyName);
			AssertEquals(invoiceHeader.SoldToParty.OH_FullName, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.BuyingParty).CompanyName);
			AssertEquals(invoiceHeader.ManufacturerAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.ManufacturerSupplier).CompanyName);
			AssertEquals(invoiceHeader.ShipToPartyAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.ShipToParty).CompanyName);
			AssertEquals(invoiceHeader.ExporterAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.Exporter).CompanyName);
			AssertEquals(invoiceHeader.ShipperAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.Shipper).CompanyName);
			AssertEquals(invoiceHeader.DistributorAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.Distributor).CompanyName);
			AssertEquals(invoiceHeader.PackagerAddress.EffectiveCompanyNameTruncated, iHeader.Entities.FirstOrDefault(x => x.EntityCode == EntityCodeList.Codes.Packager).CompanyName);

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer2.OH_FullName = "Importer2";
			importer2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");
			invoiceHeader2.JZ_OA_ConsigneeAddress = importer2.MainAddress.PK;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Test Manufacturer 2";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.MainAddress.OA_Address1 = "Address1";
			invoiceHeader2.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller2";
			seller.OH_Code = "SE" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_SellerAddress = seller.MainAddress.PK;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "Sold To Party 2";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXGGG";
			invoiceHeader2.JZ_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var shipToParty = Factory.New<OrgHeader>();
			shipToParty.OH_FullName = "SHIP TO PARTY 2";
			shipToParty.OH_Code = "SIP" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_ShipToPartyAddress = shipToParty.MainAddress.PK;

			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "EXPORTER 2";
			exporter.OH_Code = "EXP" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_ExporterAddress = exporter.MainAddress.PK;

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_FullName = "SHIPPER 2";
			shipper.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_ShipperAddress = shipper.MainAddress.PK;

			var distributor = Factory.New<OrgHeader>();
			distributor.OH_FullName = "DISTRIBUTOR 2";
			distributor.OH_Code = "EXP" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_DistributorAddress = distributor.MainAddress.PK;

			var packager = Factory.New<OrgHeader>();
			packager.OH_FullName = "PACKAGER 2";
			packager.OH_Code = "PKG" + new Random().Next(1000000).ToString();
			invoiceHeader2.JZ_OA_PackagerAddress = packager.MainAddress.PK;
			AssertEquals("Two Invoice Headers with different parties, cannot use Random Header, entities will be sent on line level", 0, iHeader.Entities.Count());
			AssertEquals("CBPBondedWarehouse", ZString.Empty, iHeader.CBPBondedWarehouseFIRMS);

			declaration.US_US_NKLocationOfGoods = "FRM2";
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.US_WHSEntryFilerCode = "HO1";
			declaration.US_WHSEntryNumber = "7009005";
			AssertEquals("CBPBondedWarehouse", ZString.Empty, iHeader.CBPBondedWarehouseFIRMS);
			AssertEquals("Location of Goods", "FRM2", iHeader.LocationOfGoods);
			AssertEquals("EntryFilerCodeOfWarehouseEntry", "HO1", iHeader.EntryFilerCodeOfWarehouseEntry);
			AssertEquals("WarehouseEntryNumber", "7009005", iHeader.WarehouseEntryNumber);

			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();

			var iACECusEntryHeader = (IACECusEntryHeader)seEntry;
			declaration.IOROrgPK = org.PK;
			declaration.IORWrapper.ZO_KnwImpInd = "Y";
			AssertEquals("KnownImporterIndicator", true, iACECusEntryHeader.KnownImporterIndicator);

			declaration.IORWrapper.ZO_KnwImpInd = "N";
			AssertEquals("KnownImporterIndicator", false, iACECusEntryHeader.KnownImporterIndicator);

			declaration.US_ExpConsign = "Y";
			AssertEquals("IsExpressConsignment", true, iHeader.IsExpressConsignment);

			declaration.US_DomesticCargo = true;
			AssertEquals("IsDomestiCargo", true, iHeader.IsDomesticCargo);

			var warehouse = Factory.New<OrgHeader>();
			warehouse.FillWithValidTestData();
			declaration.WarehouseDocAddress.OrganisationPK = warehouse.PK;
			AssertEquals("CBPBondedWarehouse", ZString.Empty, iHeader.CBPBondedWarehouseFIRMS);
			AssertEquals("FirmsCodeForWarehousingEntry", "FRM2", iHeader.CurrentFirmsCodeForWarehousingEntry);
			AssertEquals("Location of Goods", "FRM2", iHeader.LocationOfGoods);

			declaration.WarehouseAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "N000");
			AssertEquals("CBPBondedWarehouse", "N000", iHeader.CBPBondedWarehouseFIRMS);
			AssertEquals("FirmsCodeForWarehousingEntry", "FRM2", iHeader.CurrentFirmsCodeForWarehousingEntry);
			AssertEquals("Location of Goods", "N000", iHeader.LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals("CBPBondedWarehouse", "N000", iHeader.CBPBondedWarehouseFIRMS);
			AssertEquals("FirmsCodeForWarehousingEntry", "FRM2", iHeader.CurrentFirmsCodeForWarehousingEntry);
			AssertEquals("Location of Goods", "N000", iHeader.LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			AssertEquals("CBPBondedWarehouse", ZString.Empty, iHeader.CBPBondedWarehouseFIRMS);
			AssertEquals("FirmsCodeForWarehousingEntry", "FRM2", iHeader.CurrentFirmsCodeForWarehousingEntry);
			AssertEquals("Location of Goods", "N000", iHeader.LocationOfGoods);

			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			AssertEquals("EstimatedDateOfArrivalForEntryType86", ZDateTime.BrettsBirthday, iHeader.EstimatedDateOfArrivalForEntryType86);
		}

		public void TestElectionCodeAndAssociatedDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.ACECargoRelease)[0];
			var iHeader = (IACECargoReleaseHeader)seEntry;
			AssertEquals("EntryDateElectionCode", ZString.Empty, iHeader.EntryDateElectionCode);
			AssertEquals("ElectedEntryDate", ZDate.Empty, iHeader.ElectedEntryDate);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate;
			declaration.US_PresentationDate = new ZDateTime(2014, 12, 02);
			AssertEquals("EntryDateElectionCode", EntryDateElectionCodeList.Codes.WeeklyEstimateFilingDate, iHeader.EntryDateElectionCode);
			AssertEquals("ElectedEntryDate", new ZDateTime(2014, 12, 02), iHeader.ElectedEntryDate);

			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.NonWeeklyEstimateFilingDate;
			AssertEquals("EntryDateElectionCode", ZString.Empty, iHeader.EntryDateElectionCode);
			AssertEquals("ElectedEntryDate", ZDateTime.Empty, iHeader.ElectedEntryDate);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			declaration.US_PresentationDate = new ZDateTime(2014, 12, 03);
			AssertEquals("EntryDateElectionCode", EntryDateElectionCodeList.Codes.PresentationDate, iHeader.EntryDateElectionCode);
			AssertEquals("ElectedEntryDate", new ZDateTime(2014, 12, 03), iHeader.ElectedEntryDate);
		}

		public override void TestSettingDateTimeFieldsWithInvalidDateDoesntCauseTheInvalidDateToGetDefaultedToOtherFields()
		{
			Assert(true);
		}

		public new void TestSettingValueCallsRefreshBinding()
		{
			Assert(true);
		}

		public void TestDeclarationSpecialInstructions()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			StmNote siNote = declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.SpecialInstructions.Description, "blah blah");
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CarrierBookingRequest.Description, "blah blah");
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.ContainerReleaseNote.Description, "blah blah");

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			StmNoteCollection specialInstructions = entry.DeclarationSpecialInstructions;

			AssertEquals("specialInstructions should contains only 1 Note", 1, specialInstructions.Count);
			AssertEquals("specialInstructions should contains siNote", siNote, specialInstructions[0]);
		}

		public void TestLocation()
		{
			const string AddressForTest1 = "6-A Podvysozkogo Street, of.107, Kiev 01103, UA";
			const string AddressForTest2 = "Unit 3a 72 O'Riordan Street Alexandria NSW 2015 AU";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_FCL_LCL_AIR = Enterprise.Core.Constants.ContainerModes.FCL;
			CusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_FCL_LCL_AIR = Enterprise.Core.Constants.ContainerModes.LCL;
			declaration.ContainerTerminalOperatorDocAddress.E2_Address1 = AddressForTest1;
			declaration.DepotDocAddress.E2_Address1 = AddressForTest2;

			AssertEquals("Location should be ContainerTerminalOperatorDocAddress", declaration.ContainerTerminalOperatorDocAddress.E2_Address1, entry.Location);

			container1.CO_FCL_LCL_AIR = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertEquals("Location should be DepotDocAddress", declaration.DepotDocAddress.E2_Address1, entry.Location);
		}

		public void TestSupplierContactPerson()
		{
			const string supplierContactNameForTest1 = "supplierContact1";
			const string supplierContactNameForTest2 = "supplierContact2";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("SupplierContactPerson should be empty because supplier is null here", ZString.Empty, entry.SupplierContactPerson);

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("SupplierContactPerson should be empty because no contacts were added", ZString.Empty, entry.SupplierContactPerson);

			OrgContact supplierContact1 = declaration.Supplier.Contacts.AddNew();
			supplierContact1.OC_ContactName = supplierContactNameForTest1;
			AssertEquals("SupplierContactPerson should be supplierContact1", supplierContactNameForTest1, entry.SupplierContactPerson);

			OrgContact supplierContact2 = declaration.Supplier.Contacts.AddNew();
			supplierContact2.OC_ContactName = supplierContactNameForTest2;
			AssertEquals("SupplierContactPerson should be supplierContact1 because SupplierContactPerson returns first contact from list", supplierContactNameForTest1, entry.SupplierContactPerson);
		}

		public void TestImporterDeliveryContactPerson()
		{
			const string importerContactNameForTest1 = "importerContact1";
			const string importerContactNameForTest2 = "importerContact2";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			AssertEquals("ImporterDeliveryContactPerson should be empty because ImporterDeliveryAddress.Organisation is null here", ZString.Empty, entry.ImporterDeliveryContactPerson);

			OrgHeader importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();

			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("ImporterDeliveryContactPerson should be empty because because no contacts were added", ZString.Empty, entry.ImporterDeliveryContactPerson);

			OrgContact importerContact1 = declaration.ImporterDeliveryAddress.Organisation.Contacts.AddNew();
			importerContact1.OC_ContactName = importerContactNameForTest1;
			AssertEquals("ImporterDeliveryContactPerson should be importerContact1", importerContactNameForTest1, entry.ImporterDeliveryContactPerson);

			OrgContact importerContact2 = declaration.ImporterDeliveryAddress.Organisation.Contacts.AddNew();
			importerContact2.OC_ContactName = importerContactNameForTest2;
			AssertEquals("ImporterDeliveryContactPerson should be importerContact1 because ImporterDeliveryContactPerson returns first contact from list", importerContactNameForTest1, entry.ImporterDeliveryContactPerson);
		}

		public void TestCreateDocPrintingDetails()
		{
			var declaration = CreateSimpleDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			Assert("Pre-condition: US7501DocPrintingData should be empty", entry.US7501DocPrintingData.Count == 0);
			AssertEquals(typeof(US7501DocPrintingCollection), entry.US7501DocPrintingData.GetType());

			entry.CreateDocPrintingDetails(new ZGuid());
			var entryLine = entry.MergedLines[0];
			AssertEquals("Date for Duty Calculation should be today by default", ZDateTime.Today, entryLine.DateForDutyCalculation);
			Assert("US7501DocPrintingData should have been created", entry.US7501DocPrintingData.Count > 0);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Doc Data - Date for Duty Calculation should be saved", ZDateTime.Today, docData.US_DutyDate);

			var entryDate = ZDateTime.Today.AddDays(1);
			declaration.US_EstimatedEntryDate = entryDate;
			entry.CreateDocPrintingDetails(new ZGuid());
			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Doc Data - Date for Duty Calculation should be saved", entryDate, docData.US_DutyDate);
		}

		[TestDate(2019, 02, 25)]
		public void TestPrintDutyAmountFromEntryLineFor9808003000FromMessage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice1 = declaration.Invoices.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.US_SupTariff = "9808003000";
			line1.JI_Tariff = "8457100075";
			line1.JI_LinePrice = 2000m;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entryHeader.CreateDocPrintingDetails(new ZGuid());
			Assert(entryHeader.US7501DocPrintingData.Count == 2);
			var docData = entryHeader.US7501DocPrintingData[1];
			AssertEquals(84m, docData.US_DutyAmount);
			AssertEquals("4.2%", docData.US_RateAsString);
			Assert(docData.US_IsAdditionalTotalPrintingDuty);
		}

		public void TestCreateDocPrintingDetailsForWatchEntry()
		{
			JobDeclaration declaration = CreateVisaDeclaration();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for entry line & 7 child lines", 8, entry.US7501DocPrintingData.Count);

			US7501DocPrinting docData = entry.US7501DocPrintingData[0];
			AssertEquals("Entry should have pro-rated summary", false, docData.US_ProRatedLine1.IsEmpty);
			AssertEquals("Pro-rated summary line 1", "9802.00.8068 (Free) 3066 / 9426 (Total Value) = 32.527%", docData.US_ProRatedLine1);
			AssertEquals("Pro-rated summary line 2", "32.527% x $796.25 (Total Duty Column 34) = $259.00", docData.US_ProRatedLine2);
			AssertEquals("Pro-rated summary line 3", "$796.25 - $259.00 = $537.25 (Total Duty Due)", docData.US_ProRatedLine3);

			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", 440m, docData.US_SecondaryLine1DutyAmount);
			AssertEquals("Secondary Line 3 Assembled Component Duty to print on 7501", 157.14m, docData.US_SecondaryLine3DutyAmount);
			AssertEquals("Secondary Line 5 Assembled Component Duty to print on 7501", 188.3m, docData.US_SecondaryLine5DutyAmount);
			AssertEquals("Secondary Line 7 Assembled Component Duty to print on 7501", 10.81m, docData.US_SecondaryLine7DutyAmount);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 Assembled Component Duty Rate to print on 7501", "44c/NO", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[2];
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[3];
			AssertEquals("Secondary Line 3 Assembled Component Duty Rate to print on 7501", "7.09%", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[4];
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[5];
			AssertEquals("Secondary Line 5 Assembled Component Duty Rate to print on 7501", "7.09%", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[6];
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[7];
			AssertEquals("Secondary Line 7 Assembled Component Duty Rate to print on 7501", "7.09%", docData.US_RateAsString);
		}

		public void TestCreateDocPrintingDetailsWhenTariffAppliesAAURule()
		{
			#region Setup tariffs

			USCTariffTest.CreateNewTariffIfNotExist(Factory, "8544300000", "7", 0.05m, "NO");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "99039406", "0", 0m, "");
			USCTariffTest.CreateNewTariffIfNotExist(Factory, "9802008069", "X", 0m, "");

			#endregion

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.VietNam;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.VietNam;
			invoiceLine.JI_FormattedTariff = "8544.30.0000";
			invoiceLine.SupTariffFormatted = "9903.94.06";
			invoiceLine.SupFormattedAdditionalTariff1 = "9802.00.8069";

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for entry line & 2 child lines", 3, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Duty Rate for tariff 9802008069 to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Duty Rate for tariff 99039406 to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[2];
			AssertEquals("Duty Rate for tariff 8544300000 to print on 7501", "5%", docData.US_RateAsString);
		}

		[TestDate(2010, 09, 28)]
		public void TestCottonFeeRateOnEnsemble()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0010";
			invoiceLine.JI_CustomsQuantity = 840.00000m;
			invoiceLine.JI_CustomsSecondQuantity = 3285.59m;

			var secondaryLine = invoiceLine.AddSecondaryInvoiceLine();
			secondaryLine.JI_Tariff = "6102.20.0020";
			secondaryLine.JI_CustomsQuantity = 840.00000m;
			secondaryLine.JI_CustomsSecondQuantity = 3285.59m;
			secondaryLine.JI_LinePrice = 5000m;

			Assert("PreCondition", !invoiceLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));
			Assert("PreCondition", secondaryLine.ImportTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Cotton));

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var action = new EntryHeaderMessageSendingAction(ensEntry, Common.US.ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original));
			action.US_SendMessage = true;
			var manager = new EntryHeaderSingleMessageManager(ensEntry, action);
			Enterprise.Messaging.Business.EDIMessage[] messages = manager.GenerateOriginalMessages(ensEntry);
			manager.OnOriginalSent();

			AssertEquals(1, messages.Length);
			AssertEquals("DocData generated", 2, ensEntry.US7501DocPrintingData.Count);

			var printFromMessage = new EntryMessageENS7501Print(ensEntry, (MQEDIMessage)messages[0], Factory.New<MQEDIMessage>(), null);
			AssertEquals("Entry print lines count", 1, printFromMessage.EntryPrintLines.Count);
			var entryLine = printFromMessage.EntryPrintLines[0];
			AssertEquals("Parent Tariff Number", "6104.22.0010", entryLine.FormattedTariff);
			AssertEquals("SecondaryLine1FormattedTariff", "6102.20.0020", entryLine.SecondaryLine1FormattedTariff);
			AssertEquals("Line Fee Desc", "056 056 DESC FROM DB", entryLine.LineFeeDescription);
			AssertEquals("Line Fee", 27.42m, entryLine.LineFeeAmount);
			AssertEquals("Line Fee Rate", "0.8345c/KG", entryLine.LineFeePercentAsString);
		}

		public void TestCreateDocPrintingDetailsWithAdValoremConversion()
		{
			CreateWatchWithRepairsDeclaration();
			var declaration = CreateWatchWithRepairsDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for entry line & 7 child lines", 8, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Entry should have Ad Valorem Calculation summary", false, docData.US_AVWatches.IsEmpty);
			AssertEquals("US_AVWatches: Total Watches", "1000 x $0.44 NO", docData.US_AVWatches);
			AssertEquals("US_AVWatchesDuty: Total Watches Duty", 440m, docData.US_AVWatchesDuty);
			AssertEquals("US_AVCases: Cases", "$2619 x 6%", docData.US_AVCases);
			AssertEquals("US_AVCasesDuty: Cases Duty", 157.14m, docData.US_AVCasesDuty);
			AssertEquals("US_AVBracelets: Bracelets", "$1345 x 14%", docData.US_AVBracelets);
			AssertEquals("US_AVBraceletsDuty: Bracelets Duty", 188.30m, docData.US_AVBraceletsDuty);
			AssertEquals("US_AVBatteries: Batteries", "$204 x 5.3%", docData.US_AVBatteries);
			AssertEquals("US_AVBatteriesDuty: Batteries Duty", 10.81m, docData.US_AVBatteriesDuty);
			AssertEquals("US_AVTotalDuty: Total Duty", 796.25m, docData.US_AVTotalDuty);
			AssertEquals("Ad Valorem Calculation summary line 2", "$796.25/$9426.00 (Total Entered Value) = 8.447%", docData.US_AVLine2);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 Component Duty Rate to print on 7501", "8.447%", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[2];
			AssertEquals("Secondary Line 2 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[3];
			AssertEquals("Secondary Line 3 Component Duty Rate to print on 7501", "8.447%", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[4];
			AssertEquals("Secondary Line 4 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[5];
			AssertEquals("Secondary Line 5 Component Duty Rate to print on 7501", "8.447%", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[6];
			AssertEquals("Secondary Line 6 Duty Rate to print on 7501", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[7];
			AssertEquals("Secondary Line 7 Component Duty Rate to print on 7501", "8.447%", docData.US_RateAsString);
		}

		public void TestMessageToPrintOn7501()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			StmNote messageToPrintNote = declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501.Description, "Dept. of Defense - Duty free claimed");

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			StmNoteCollection messageToPrintOn7501 = entry.MessageToPrintOn7501;

			AssertEquals("messageToPrintOn7501 should contain only 1 Note", 1, messageToPrintOn7501.Count);
			AssertEquals("messageToPrintOn7501 should contain messageToPrintNote", messageToPrintNote, messageToPrintOn7501[0]);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			messageToPrintOn7501 = entry.MessageToPrintOn7501;
			AssertEquals("messageToPrintOn7501 should not find note now as this is a declaration attached to a shipment", 0, messageToPrintOn7501.Count);

			var messageToPrintNoteFromShipment = shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.CustomsMessageToPrintOn7501.Description, "Dept. of Defense - Text from Shipment");
			messageToPrintOn7501 = entry.MessageToPrintOn7501;
			AssertEquals("messageToPrintOn7501 should find Note from Shipment as master", 1, messageToPrintOn7501.Count);
			AssertEquals("messageToPrintOn7501's PK should contain messageToPrintNoteFromShipment's PK", messageToPrintNoteFromShipment.PK, messageToPrintOn7501[0].PK);
		}

		public void TestUS_LineNoReflectsEntryLineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204.21.2000";// 19.8c/L

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201.10.0510";// 4.4c/KG

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var ensEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			// entry lines are loaded out of order: Order is not deterministic
			ensEntry.MergedLines.Remove(invoiceLine.CusEntryLine);
			ensEntry.MergedLines.Add(invoiceLine.CusEntryLine);

			AssertEquals("PreCondition", invoiceLine2.CusEntryLine, ensEntry.MergedLines[0]);
			AssertEquals("PreCondition", (short)2, invoiceLine2.CusEntryLine.CL_LineNumber);
			AssertEquals("PreCondition", invoiceLine.CusEntryLine, ensEntry.MergedLines[1]);
			AssertEquals("PreCondition", (short)1, invoiceLine.CusEntryLine.CL_LineNumber);

			var action = new EntryHeaderMessageSendingAction(ensEntry, Common.US.ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original));
			action.US_SendMessage = true;
			var manager = new EntryHeaderSingleMessageManager(ensEntry, action);
			Enterprise.Messaging.Business.EDIMessage[] messages = manager.GenerateOriginalMessages(ensEntry);
			manager.OnOriginalSent();

			AssertEquals(1, messages.Length);
			AssertEquals("DocData generated", 2, ensEntry.US7501DocPrintingData.Count);

			var printFromMessage = new EntryMessageENS7501Print(ensEntry, (MQEDIMessage)messages[0], Factory.New<MQEDIMessage>(), null);

			var entryLine1 = printFromMessage.EntryPrintLines[0];
			AssertEquals("19.8c/L", entryLine1.DutyPercentAsString);

			var entryLine2 = printFromMessage.EntryPrintLines[1];
			AssertEquals("4.4c/KG", entryLine2.DutyPercentAsString);
		}

		[TestDate(2011, 02, 07)]
		public void TestTIBShowsDutyRate()
		{
			var declaration = CreateTIBDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for TIB entry with 1 line (2 Tariffs)", 2, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Line Duty Rate", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 Assembled Component Duty to print on 7501", "5.8%", docData.US_RateAsString);
		}

		public void TestPrintTIBWithTwo9903()
		{
			var testHelper = new CombinedLinesHelperTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
			var testJob = testHelper.CombinedJob;
			testJob.US_EntryType = "23";
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine2 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			var invoiceLine3 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test9813Tariff.UE_Tariff;
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			invoiceLine2.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine3.JI_ParentID = invoiceLine1.PK;
			invoiceLine3.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038802Tariff.UE_Tariff;
			invoiceLine3.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine3.JI_LinePrice = 10000m;

			testJob.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CreateDocPrintingDetails(new ZGuid());

			AssertEquals(4, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Line Duty Rate", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 TIB Duty Rate to print on 7501", "40%", docData.US_RateAsString);
			AssertEquals("Secondary Line 1 TIB Duty Amount to print on 7501", 4000m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[2];
			AssertEquals("Secondary Line 2 TIB Duty Rate to print on 7501", "50%", docData.US_RateAsString);
			AssertEquals("Secondary Line 3 TIB Duty Amount to print on 7501", 5000m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[3];
			AssertEquals("Secondary Line 2 TIB Duty Rate to print on 7501", "70%", docData.US_RateAsString);
			AssertEquals("Secondary Line 3 TIB Duty Amount to print on 7501", 7000m, docData.US_SecondaryLineTIBCalculatedDutyAmount);
		}

		[TestDate(2011, 02, 07)]
		public void TestTIBShowsDutyAmountCalculated()
		{
			var declaration = CreateTIBDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for TIB entry with 1 line (2 Tariffs)", 2, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Line Duty Rate", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 TIB Duty Rate to print on 7501", "5.8%", docData.US_RateAsString);
			AssertEquals("Secondary Line 1 TIB Duty Amount to print on 7501", 580m, docData.US_SecondaryLineTIBCalculatedDutyAmount);
		}

		[TestDate(2001, 05, 11)]
		public void TestVLineTIBEntry()
		{
			var declaration = CreateVLineTIBDeclaration();
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CreateDocPrintingDetails(new ZGuid());
			AssertEquals("US7501DocPrintingData should have been created for TIB X/V set entry", 8, entry.US7501DocPrintingData.Count);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Line Duty Rate", "Free", docData.US_RateAsString);

			docData = entry.US7501DocPrintingData[1];
			AssertEquals("Secondary Line 1 TIB Duty Rate to print on 7501", "6.4%", docData.US_RateAsString);
			AssertEquals("Secondary Line 1 TIB Duty Amount to print on 7501", 320m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[2];
			AssertEquals("Line 2 TIB Duty Rate to print on 7501", "Free", docData.US_RateAsString);
			AssertEquals("Line 2 TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[3];
			AssertEquals("Line 2 Secondary Line TIB Duty Rate to print on 7501", "", docData.US_RateAsString);
			AssertEquals("Line 2 Secondary Line TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[4];
			AssertEquals("Line 3 TIB Duty Rate to print on 7501", "Free", docData.US_RateAsString);
			AssertEquals("Line 3 TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[5];
			AssertEquals("Line 3 Secondary Line TIB Duty Rate to print on 7501", "", docData.US_RateAsString);
			AssertEquals("Line 3 Secondary Line TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[6];
			AssertEquals("Line 4 TIB Duty Rate to print on 7501", "Free", docData.US_RateAsString);
			AssertEquals("Line 4 TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);

			docData = entry.US7501DocPrintingData[7];
			AssertEquals("Line 4 Secondary Line TIB Duty Rate to print on 7501", "", docData.US_RateAsString);
			AssertEquals("Line 4 Secondary Line TIB Duty Amount to print on 7501", 0m, docData.US_SecondaryLineTIBCalculatedDutyAmount);
		}

		public void TestBox1_ArrivalDate()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_DateOfArrival = new ZDateTime(2013, 06, 03);
			dec.US_EntryDate = new ZDateTime(2013, 06, 04);

			var entry = dec.CustomsEntryHeaders.AddNew();

			AssertEquals(new ZDateTime(2013, 06, 03), entry.Box1_ArrivalDate);
			dec.JE_PrimaryITNumber = "1234";
			AssertEquals(new ZDateTime(2013, 06, 04), entry.Box1_ArrivalDate);
		}

		public void TestBox8_ConsigneeNo()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			OrgCusCode ultimateConsigneeEINCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeEINCode.OK_CodeType = OrgCusCode.USACodeTypes.EmployerIdentificationNumber;
			ultimateConsigneeEINCode.OK_CustomsRegNo = "12-3456789";
			entry.RandomHeader.JZ_OH_Buyer = ultimateConsignee.PK;
			declaration.IOROrgPK = ultimateConsignee.PK;
			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("ultimateConsignee is Importer of Record", "12-3456789", entry.Box8_ConsigneeNo);

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			importerOfRecord.OH_FullName = "IOR";
			declaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals("Ultimate Consignee differs from Importer of Record : EIN", "12-3456789", entry.Box8_ConsigneeNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			OrgCusCode ultimateConsigneeSSNCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeSSNCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			ultimateConsigneeSSNCode.OK_CustomsRegNo = "123-12-1234";

			AssertEquals("Ultimate Consignee differs from Importer of Record : SSN (We don't print SSN on customs side)", "", entry.Box8_ConsigneeNo);

			declaration.PrintSocialSecurityNumberOnDocument = true;
			AssertEquals("Ultimate Consignee differs from Importer of Record : SSN (We print SSN on customs side through menu XXX (SSN))", "123-12-1234", entry.Box8_ConsigneeNo);

			ultimateConsignee.CustomsCodes.RemoveAll();
			OrgCusCode ultimateConsigneeCBPCode = ultimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeCBPCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			ultimateConsigneeCBPCode.OK_CustomsRegNo = "YYDDPP-44999";
			AssertEquals("Ultimate Consignee differs from Importer of Record : CBP", "YYDDPP-44999", entry.Box8_ConsigneeNo);
		}

		public void TestBox10_ConsigneeAddressDetails()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(null, entry.EffectiveUltimateConsigneeWrapperAddressDetails);

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			ultimateConsignee.MainAddress.OA_Address1 = "UC Addr 1";
			ultimateConsignee.MainAddress.OA_Address2 = "UC Addr 2";
			ultimateConsignee.MainAddress.OA_City = "UC City";
			ultimateConsignee.MainAddress.OA_PostCode = "UC PC";

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("Ultimate Consignee", entry.Box10_ConsigneeName);
			AssertEquals("Ultimate consignee address details", "UC Addr 1", entry.Box10_ConsigneeAddressLine1);
			AssertEquals("Ultimate consignee address details", "UC Addr 2", entry.Box10_ConsigneeAddressLine2);
			AssertEquals("Ultimate consignee address details", "UC City   UC PC", entry.Box10_ConsigneeCityStatePostCodeCountry);

			OrgHeader ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.FillWithValidTestData();
			ultimateConsignee2.OH_FullName = "Ultimate Consignee 2";

			JobComInvoiceHeader invoice4 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			AssertEquals("Multiple consignees", USConstants.MultipleValueIndicator, entry.Box8_ConsigneeNo);
			AssertEquals("Ultimate consignee address details should be blank when multiple consignees exist", "", entry.Box10_ConsigneeName);
			AssertEquals("Ultimate consignee address details should be blank when multiple consignees exist", "", entry.Box10_ConsigneeAddressLine1);
			AssertEquals("Ultimate consignee address details should be blank when multiple consignees exist", "", entry.Box10_ConsigneeAddressLine2);
			AssertEquals("Ultimate consignee address details should be blank when multiple consignees exist", "", entry.Box10_ConsigneeCityStatePostCodeCountry);
		}

		public void TestBox10_ConsigneeCityStatePostCodeCountry()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(null, entry.EffectiveUltimateConsigneeWrapperAddressDetails);

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.FillWithValidTestData();
			ultimateConsignee.OH_RL_NKClosestPort = "USCHI";
			ultimateConsignee.OH_FullName = "Ultimate Consignee";
			ultimateConsignee.MainAddress.OA_Address1 = "Address line 1";
			ultimateConsignee.MainAddress.OA_Address2 = "Address line 2";
			ultimateConsignee.MainAddress.OA_City = "Michigan City";
			ultimateConsignee.MainAddress.OA_PostCode = "59650";
			ultimateConsignee.MainAddress.OA_State = "IL";

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			AssertEquals("AddressLine2 should have address line", "Address line 2", entry.Box10_ConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string", "Michigan City   IL   59650   US", entry.Box10_ConsigneeCityStatePostCodeCountry);

			OrgHeader newConsignee = Factory.New<OrgHeader>();
			newConsignee.FillWithValidTestData();
			newConsignee.OH_RL_NKClosestPort = "USCHI";
			newConsignee.OH_FullName = "New Consignee";
			newConsignee.MainAddress.OA_Address1 = "One Address line only";
			newConsignee.MainAddress.OA_Address2 = "";
			newConsignee.MainAddress.OA_City = "Chicago";
			newConsignee.MainAddress.OA_PostCode = "59600";
			newConsignee.MainAddress.OA_State = "IL";

			declaration.JE_OA_ConsigneeAddress = newConsignee.MainAddress.PK;
			AssertEquals("AddressLine2 should now show city state postcode line", "Chicago   IL   59600   US", entry.Box10_ConsigneeAddressLine2);
			AssertEquals("Ultimate Consignee city / state / postcode / country string should be blank as has been move up 1 line", "", entry.Box10_ConsigneeCityStatePostCodeCountry);

			OrgHeader consigneeWithNoCity = Factory.New<OrgHeader>();
			consigneeWithNoCity.FillWithValidTestData();
			consigneeWithNoCity.OH_RL_NKClosestPort = "USCHI";
			consigneeWithNoCity.OH_FullName = "Ultimate Consignee";
			consigneeWithNoCity.MainAddress.OA_Address1 = "Address line 1";
			consigneeWithNoCity.MainAddress.OA_Address2 = "Address line 2";
			consigneeWithNoCity.MainAddress.OA_City = "";
			consigneeWithNoCity.MainAddress.OA_PostCode = "59650";
			consigneeWithNoCity.MainAddress.OA_State = "";

			declaration.JE_OA_ConsigneeAddress = consigneeWithNoCity.MainAddress.PK;
			AssertEquals("Ultimate Consignee city / state / postcode / country string when no city or state should be trimmed start", "59650   US", entry.Box10_ConsigneeCityStatePostCodeCountry);
		}

		public void TestShouldPrintBrokerSignature()
		{
			var dec = SetUpMergedInvoices();
			var entry = dec.CustomsEntryHeaders[0];

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, entry.ShouldPrintBrokerSignature);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, entry.ShouldPrintBrokerSignature);
		}

		public void TestBrokerSignature3461()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var dec = SetUpMergedInvoices();
			var entry = dec.CustomsEntryHeaders[0];

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "OOO";
			dec.JE_GS_NKCusAgent = broker.GS_Code;

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461);

			broker.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "III";
			broker1.SignatureImage = new Bitmap(2, 1);
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, broker1.PK.ToGuid());
			broker.SignatureImage = null;
			GlbStaff.CurrentUser.SignatureImage = null;
			AssertNotNull(entry.BrokerSignature3461);
		}

		public void TestBrokerSignature3461ElectronicRelease()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var dec = SetUpMergedInvoices();
			var entry = dec.CustomsEntryHeaders[0];
			var disposition = dec.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PaperlessEntry, ZDateTime.Today);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "OOO";
			dec.JE_GS_NKCusAgent = broker.GS_Code;

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			broker.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(entry.BrokerSignature3461ElectronicRelease);

			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, false);
			USCustomsDataRegistry.Instance.PrintBrokerSignatureOnEntryDocs.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, true);
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			GlbStaff.CurrentUser.SignatureImage = new Bitmap(1, 2);
			AssertNotNull(entry.BrokerSignature3461ElectronicRelease);

			dec.DispositionCodes.DeleteAll();
			AssertNull(entry.BrokerSignature3461ElectronicRelease);

			dec.DispositionCodes.AddNewIfNotExist(CargoReleaseProcessingResultList.Codes.PaperlessEntry, ZDateTime.Today);
			var broker1 = Factory.New<GlbStaff>();
			broker1.GS_Code = "III";
			broker1.SignatureImage = new Bitmap(2, 1);
			broker.SignatureImage = null;
			GlbStaff.CurrentUser.SignatureImage = null;
			USCustomsDataRegistry.Instance.PrintSignatureOnEntryDocsBroker.SetValue(dec.RegistryCompanyPK, Guid.Empty, Guid.Empty, broker1.PK.ToGuid());
			AssertNotNull(entry.BrokerSignature3461ElectronicRelease);
		}

		public void TestImporterNumberType()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.OH_FullName = "MR BOB";
			importerOfRecord.OH_RL_NKClosestPort = "AUSYD";
			importerOfRecord.MainAddress.OA_Address1 = "ADDRESS 1";
			importerOfRecord.MainAddress.OA_Address2 = "ADDRESS 2";
			importerOfRecord.MainAddress.OA_City = "SYDNEY";
			importerOfRecord.MainAddress.OA_State = "NSW";
			importerOfRecord.MainAddress.OA_PostCode = "2200";
			importerOfRecord.MainAddress.OA_Phone = "98774455";
			importerOfRecord.MainAddress.OA_Fax = "98774466";
			importerOfRecord.MainAddress.OA_Email = "who@what.where";
			var contact = importerOfRecord.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			var cusCode = importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			declaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals("X", entry.Box3IsIRS);
			AssertEquals(ZString.Empty, entry.Box3IsSSN);
			AssertEquals(ZString.Empty, entry.Box3IsCBP);
		}

		public void TestImporterSSNNumberType()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];

			OrgHeader importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.OH_FullName = "MR BOB";
			importerOfRecord.OH_RL_NKClosestPort = "AUSYD";
			importerOfRecord.MainAddress.OA_Address1 = "ADDRESS 1";
			importerOfRecord.MainAddress.OA_Address2 = "ADDRESS 2";
			importerOfRecord.MainAddress.OA_City = "SYDNEY";
			importerOfRecord.MainAddress.OA_State = "NSW";
			importerOfRecord.MainAddress.OA_PostCode = "2200";
			importerOfRecord.MainAddress.OA_Phone = "98774455";
			importerOfRecord.MainAddress.OA_Fax = "98774466";
			importerOfRecord.MainAddress.OA_Email = "who@what.where";
			var contact = importerOfRecord.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			var cusCode = importerOfRecord.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.SocialSecurityNumber, "123-33-5555");
			declaration.IOROrgPK = importerOfRecord.PK;
			AssertEquals(ZString.Empty, entry.Box3IsIRS);
			AssertEquals(ZString.Empty, entry.Box3IsSSN);
			AssertEquals(ZString.Empty, entry.Box3IsCBP);

			declaration.PrintSocialSecurityNumberOnDocument = true;
			AssertEquals(ZString.Empty, entry.Box3IsIRS);
			AssertEquals("X", entry.Box3IsSSN);
			AssertEquals(ZString.Empty, entry.Box3IsCBP);
		}

		public void TestBondType()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			AssertEquals("X", entry.IsSingleTransBond);
			AssertEquals(ZString.Empty, entry.IsContinuousBond);
			AssertEquals(ZString.Empty, entry.IsNoBondRequired);
		}

		public void TestTransportType()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("X", entry.IsAir);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("X", entry.IsSea);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("X", entry.IsRail);
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			AssertEquals("X", entry.IsTruck);
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			AssertEquals("X", entry.IsHandCarry);
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertEquals("X", entry.IsPipeline);
		}

		public void TestBills()
		{
			JobDeclaration declaration = SetUpMergedInvoices();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			var bill1 = declaration.Bills.AddNew();
			var bill2 = declaration.Bills.AddNew();
			AssertEquals(2, entry.Bills_3461.Count);
		}

		public void TestParties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);

			var importer2 = Factory.New<OrgHeader>();
			importer2.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");
			declaration.JE_OA_ConsigneeAddress = importer2.MainAddress.PK;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Test Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();

			var manufacturerAddress = manufacturer.Addresses.AddNew();
			manufacturerAddress.OA_Address1 = "new address";
			manufacturerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			declaration.JE_OA_ManufacturerAddress = manufacturerAddress.PK;

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Test Selling Party(Seller)";
			seller.OH_Code = "SE" + new Random().Next(1000000).ToString();
			declaration.JE_OA_SellerAddress = seller.MainAddress.PK;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "AUTO ELECTRICAL DISTRIBUTORS PTY LTD TEST TEST TES";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXGGG";
			soldToParty.MainAddress.OA_Address1 = "UNIT 1, 210 ROBINSON ROAD GEEBUN DOWNTOWN FOR TEST";
			soldToParty.MainAddress.OA_Address2 = "GEEBUNG, QLD GEEBUN DOWNTOWN FOR TEST GEEBUN DOWNT";
			soldToParty.MainAddress.OA_City = "GEEBUN CITY FOR LENGHT TE";
			soldToParty.MainAddress.OA_PostCode = "4034654521";
			declaration.JE_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123910-00001");
			declaration.IOROrgPK = importer.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			DeclarationTestHelper.SetupOrganizationsForACEInvoice(invoiceHeader, Factory);
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_OA_ConsigneeAddress = importer2.MainAddress.PK;
			invoiceLine.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			invoiceLine.JI_OA_Seller = seller.MainAddress.PK;
			invoiceLine.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = declaration.ActiveEntryHeaders.GetEntryWithType(Enterprise.Customs.Common.US.ImportMessageStatusList.MessageType.ACECargoRelease)[0];
			AssertEquals("Entities: Manufacturer, Ultimate Consignee, Seller, Sold To Party", 4, seEntry.Parties_3461.Count);
		}

		public void TestLinkedObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew() as ISimplifiedMessageLinkedObject;

			var bizObj = header.LinkedObject as CusEntryHeader;
			AssertNotNull(bizObj);
			AssertEquals(((CusEntryHeader)header).PK, bizObj.PK);
		}

		public void TestParentBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew() as ISimplifiedMessageLinkedObject;

			var bizObj = header.ParentBusinessObject as JobDeclaration;
			AssertNotNull(bizObj);
			AssertEquals(declaration.PK, bizObj.PK);
		}

		public void TestIsCargoReleaseBeingCertified()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			var bizObj = header as ISimplifiedMessageLinkedObject;
			Assert(!bizObj.IsCargoReleaseBeingCertified);

			header.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			Assert(bizObj.IsCargoReleaseBeingCertified);
		}

		public void TestIsFormalEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var bo = header as ISimplifiedMessageLinkedObject;
			Assert(bo.IsFormalEntry);
		}

		public void TestIsBorderCargoReleaseInISimplifiedMessageLinkedObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.BorderCargoRelease;
			var bo = header as ISimplifiedMessageLinkedObject;
			Assert(bo.IsBorderCargoRelease);
		}

		public void TestIsCargoReleaseInISimplifiedMessageLinkedObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			var bo = header as ISimplifiedMessageLinkedObject;
			Assert(bo.IsCargoRelease);
		}

		public void TestIsACECargoRelease()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			var bo = header as ISimplifiedMessageLinkedObject;
			Assert(bo.IsACECargoRelease);
		}

		public void TestClearCRLCerStatusFromAllHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			var header01 = declaration.ActiveEntryHeaders.AddNew();
			var header02 = declaration.ActiveEntryHeaders.AddNew();
			var header03 = declaration.ActiveEntryHeaders.AddNew();

			header.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			header01.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			header02.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			header03.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;

			var bo = header as ISimplifiedMessageLinkedObject;
			bo.ClearCRLCertStatusFromAllHeaders();
			AssertEquals(ZString.Empty, header.US_CRLCertStatus);
			AssertEquals(ZString.Empty, header01.US_CRLCertStatus);
			AssertEquals(ZString.Empty, header02.US_CRLCertStatus);
			AssertEquals(ZString.Empty, header03.US_CRLCertStatus);
		}

		public void TestDeactiveStatementLineIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImportEntryNumber = "TESTENTRY";
			header.EntryNumber = declaration.ImportEntryNumber;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_EntryFilerCode = declaration.EntryFilerCode;
			statement.B2_StatementNumber = declaration.ImportEntryNumber;
			statement.B2_Status = StatementHeaderStatusList.Codes.Preliminary;
			statement.B2_PaymentStatus = PaymentStatusList.Codes.PaymentFailed;
			statement.B2_GC = declaration.CompanyPK;

			var statementLine = Factory.New<CusStatementLine>();
			statementLine.B3_EntryFilerCode = declaration.EntryFilerCode;
			statementLine.B3_EntryNum = declaration.ImportEntryNumber;
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_B2 = statement.PK;

			var bo = header as ISimplifiedMessageLinkedObject;
			bo.DeactiveStatementLineIfRequired(new ZString[] { "A" }, "A");
			AssertEquals(StatementHeaderStatusList.Codes.Deleted, statement.B2_Status);
			AssertEquals(StatementLineStatusList.Codes.Deleted, statementLine.B3_Status);
		}

		[TestDate(2019, 12, 11)]
		public void TestAutoSendEntrySummaryQueryIfEligible()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			DataRegistry.Business.USCustomsDataRegistry.Instance.AutoQueryEntrySummaries.SetValue(declaration.RegistryCompanyPK, System.Guid.Empty, System.Guid.Empty, true);
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var message = new ACEEntrySummaryQueryMessageBuilder(header).PopulateMessage();
			message.EM_HeldUntilDate = ZDateTime.Now;

			var bo = header as ISimplifiedMessageLinkedObject;
			bo.AutoSendEntrySummaryQueryIfEligible();
			AssertEquals(ZDateTime.Now.AddDays(3).ToString(), message.EM_HeldUntilDate.ToString());
		}

		public void TestUpdateACEFDALineInfoIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71016297";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;

			var aceFDALine0 = invoiceLine.ACE_FDALines.AddNew();
			aceFDALine0.US_LineNo = 1;
			aceFDALine0.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var aceFDALine1 = invoiceLine.ACE_FDALines.AddNew();
			aceFDALine1.US_LineNo = 2;
			aceFDALine1.US_TrackingStatus = PGATrackingStatusList.Codes.Deleted;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CreateStatusMessage(
				"B001303SV9SO                                                                    " +
				"SO101303SV9  71016297 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
				"SO20CR B00000001                                                                " +
				"SO40MMAEU954607736                                                              " +
				"SO40HMAEU567902246                                         00000010     00000010" +
				"SO50102615084194BILL DEPARTED                                                   " +
				"SO70FDACOS081916144301DATA UNDER PGA REVIEW         041400011001              02" +
				"SO71                          107                                               " +
				"SO70FDACOS081916144301DATA UNDER PGA REVIEW         041400011002              02" +
				"SO71                          107                                               " +
				"Y  1303SV9SO00000");

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			AssertEquals(YesNoDefaultList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);
			aceFDALine0.Reload();
			AssertEquals(PGATrackingStatusList.Codes.ToBeUpdated, aceFDALine0.US_TrackingStatus);
			aceFDALine1.Reload();
			AssertEquals(PGATrackingStatusList.Codes.ToBeDeleted, aceFDALine1.US_TrackingStatus);
		}

		public void TestUpdateFWSLineInfoIfRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "71016297";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;

			var fwsLine0 = invoiceLine.FWSHeaders.AddNew();
			fwsLine0.US_LineNo = 1;
			fwsLine0.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			var fwsLine1 = invoiceLine.FWSHeaders.AddNew();
			fwsLine1.US_LineNo = 2;
			fwsLine1.US_TrackingStatus = PGATrackingStatusList.Codes.Added;
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			CreateStatusMessage(
				"B001303SV9SO                                                                    " +
				"SO101303SV9  71016297 0120-086695300MAEUMSC FLAMINIA        540W 102715         " +
				"SO20CR B00000001                                                                " +
				"SO40MMAEU954607736                                                              " +
				"SO40HMAEU567902246                                         00000010     00000010" +
				"SO50102615084194BILL DEPARTED                                                   " +
				"SO70FWSFWS081916144301DATA UNDER PGA REVIEW         041400011001              02" +
				"SO7104987654321098            107                                               " +
				"SO70FWSFWS081916144301DATA UNDER PGA REVIEW         041400011002              02" +
				"SO71                          107                           06123456789012345678" +
				"Y  1303SV9SO00000");

			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			fwsLine0.Reload();
			AssertEquals("", fwsLine0.US_ConfirmationNum);
			AssertEquals(PGATrackingStatusList.Codes.Added, fwsLine0.US_TrackingStatus);
			fwsLine1.Reload();
			AssertEquals("12345678901234", fwsLine1.US_ConfirmationNum);
			AssertEquals(PGATrackingStatusList.Codes.Added, fwsLine1.US_TrackingStatus);
		}

		public void TestCargoReleaseCertifiedStatus()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			header.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			AssertEquals(CargoReleaseCertificationStatusList.Codes.Certified, ((ISimplifiedMessageLinkedObject)header).CargoReleaseCertifiedStatus);

			header.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			AssertEquals(CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, ((ISimplifiedMessageLinkedObject)header).CargoReleaseCertifiedStatus);
		}

		public void TestGetMSCEventReferenceForCargoReleaseResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.ActiveEntryHeaders.AddNew();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			AssertEquals("SX", ((ISimplifiedMessageLinkedObject)header).GetMSCEventReferenceForCargoReleaseResponse(message));

			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus;
			AssertEquals("SO", ((ISimplifiedMessageLinkedObject)header).GetMSCEventReferenceForCargoReleaseResponse(message));
		}

		protected override bool RatesAreReciprocal => true;

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			CusEntryHeader result = (CusEntryHeader)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			result.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			return result;
		}

		protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection);

		protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

		protected override BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				JobDeclaration result = Factory.New<JobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				result.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
				result.US_EnableENS = true;
				return result;
			}
		}

		protected override void SetInvoicesToResultInTwoEntries(BaseJobComInvoiceLine line1, BaseJobComInvoiceLine line2)
		{
			line1.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			((JobComInvoiceLine)line1).InvoiceHeader.US_StateOfOrigin = "AZ";
			((JobComInvoiceLine)line2).InvoiceHeader.US_StateOfOrigin = "WA";
		}

		protected override void OverrideValuationDate(BaseJobComInvoiceHeader invoice, ZDateTime date)
		{
			base.OverrideValuationDate(invoice, date);
			((JobComInvoiceHeader)invoice).JobDeclaration.US_DateOfExport = date;
		}

		protected override void DoMerge(BaseJobDeclaration declaration)
		{
			((JobDeclaration)declaration).ImportEntryNumber = "1";
			base.DoMerge(declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		void PopulateEntry(CusEntryHeader entry, ZString addInfo, ZString bGMReference, ZGuid primeEntry, ZString entryStatus, ZDateTime entrySubmittedDate, ZShort highestLineNumber, ZString messageType, ZString status, ZDecimal totalPaid)
		{
			entry.CH_AddInfo = addInfo;
			entry.CH_BGMReference = bGMReference;
			entry.CH_CH_PrimeEntry = primeEntry;
			entry.CH_EntryStatus = entryStatus;
			entry.CH_EntrySubmittedDate = entrySubmittedDate;
			entry.CH_HighestLineNumber = highestLineNumber;
			entry.CH_MessageType = messageType;
			entry.CH_Status = status;
			entry.CH_TotalPaid = totalPaid;

			var entryLine1 = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine1.CL_CH = entry.PK;
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_Description = "LINE1";

			var entryLine2 = Factory.NewWithValidTestData<CusEntryLine>();
			entryLine2.CL_CH = entry.PK;
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_Description = "LINE2";
		}

		IMessageAttacheeInDeclaration MessageAttacheeInDeclaration => EntryHeader;

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		CusEntryHeader entryHeader;
		CusEntryHeader EntryHeader => entryHeader ?? (entryHeader = Declaration.CustomsEntryHeaders.AddNew());

		void SetStatusAndSave(CusEntryHeader entry, ZString status)
		{
			entry.CH_Status = status;
			entry.Factory.Save();
		}

		JobDeclaration SetUpMergedInvoices()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.DisableDefaultPackingInformation = true;

			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entry.MergedLines.AddNew();

			CusEntryHeader entry2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine2 = entry2.MergedLines.AddNew();

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			OrgHeader manufacturer = Factory.New<OrgHeader>();
			invoice1.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			manufacturer.OH_FullName = "Manufacturer";
			manufacturer.FillWithValidTestData();
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);

			JobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine.PK;
			line1.US_UC_NKCountryOfOrigin = "HK";
			line1.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			JobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine.PK;
			line2.US_UC_NKCountryOfOrigin = "HK";
			line2.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice3 = declaration.Invoices.AddNew();
			JobComInvoiceLine line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine2.PK;
			line3.US_UC_NKCountryOfOrigin = "HK";
			line3.US_UC_NKCountryOfExport = "HK";

			JobComInvoiceHeader invoice4 = declaration.Invoices.AddNew();
			JobComInvoiceLine line4 = invoice4.JobComInvoiceLines.AddNew();
			line4.JI_CL = entryLine2.PK;
			line4.US_UC_NKCountryOfOrigin = "HK";
			line4.US_UC_NKCountryOfExport = "HK";

			return declaration;
		}

		void CreateExhibitionDeclaration()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = Declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = Declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 6100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 24055m;
			invoiceLine1.JI_Tariff = "7326908587";
			invoiceLine1.JI_CustomsQuantity = 6100m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		void CreateExhibitionDeclaration2()
		{
			declaration = null;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Declaration.US_EntryType = EntryTypeList.Codes.PermanentExhibition;
			Declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.US_UC_NKCountryOfExport = "SG";
			invoice.US_UC_NKCountryOfOrigin = "SG";

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "3201901000";
			invoiceLine.JI_CustomsQuantity = 50m;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.US_SPI = "";

			Factory.Save();
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		JobDeclaration CreateSimpleDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "SIMPLE";
			invoiceHeader.JZ_InvoiceAmount = 5000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "3921902550";
			invoiceLine1.JI_CustomsQuantity = 1000m;
			invoiceLine1.JI_CustomsUnitQty = "NO";
			invoiceLine1.JI_LinePrice = 5000m;

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateVisaDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "WATCH";
			invoiceHeader.JZ_InvoiceAmount = 9426m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
			invoiceHeader.JZ_IncoTerm = "FOB";

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802008068";
				invoiceLine1.JI_InvoiceQuantity = 0m;
				invoiceLine1.JI_CustomsQuantity = 0m;
				invoiceLine1.US_98GoodsValue = 1852m;

				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802008068";
				childLine2.JI_InvoiceQuantity = 0m;
				childLine2.JI_CustomsQuantity = 0m;
				childLine2.US_98GoodsValue = 1010m;

				childLine2.JI_Tariff = "9102111020";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine4 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine4.US_SupTariff = "9802008068";
				childLine4.JI_InvoiceQuantity = 0m;
				childLine4.JI_CustomsQuantity = 0m;
				childLine4.US_98GoodsValue = 0m;

				childLine4.JI_Tariff = "9102111030";
				childLine4.JI_InvoiceQuantity = 1000m;
				childLine4.JI_InvoiceUQ = "NO";
				childLine4.JI_CustomsQuantity = 1000m;
				childLine4.JI_CustomsUnitQty = "NO";
				childLine4.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine6 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine6.US_SupTariff = "9802008068";
				childLine6.JI_InvoiceQuantity = 0m;
				childLine6.JI_CustomsQuantity = 0m;
				childLine6.US_98GoodsValue = 204m;

				childLine6.JI_Tariff = "9102111040";
				childLine6.JI_InvoiceQuantity = 1000m;
				childLine6.JI_InvoiceUQ = "NO";
				childLine6.JI_CustomsQuantity = 1000m;
				childLine6.JI_CustomsUnitQty = "NO";
				childLine6.JI_LinePrice = 0m;
			}

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateWatchWithRepairsDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_InvoiceNumber = "REPAIRED WATCH";
				invoiceHeader.JZ_InvoiceAmount = 9426m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
				invoiceHeader.JZ_RN_NKDefaultOrigin = "HK";
				invoiceHeader.JZ_IncoTerm = "FOB";

				JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
				invoiceLine1.US_SupTariff = "9802004040";
				invoiceLine1.US_98GoodsValue = 1852m;
				invoiceLine1.JI_Tariff = "9102111010";
				invoiceLine1.JI_InvoiceQuantity = 1000m;
				invoiceLine1.JI_InvoiceUQ = "NO";
				invoiceLine1.JI_CustomsQuantity = 1000m;
				invoiceLine1.JI_CustomsUnitQty = "NO";
				invoiceLine1.JI_LinePrice = 3406m;

				JobComInvoiceLine childLine1 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine1.US_SupTariff = "9802004040";
				childLine1.US_98GoodsValue = 1010m;
				childLine1.JI_Tariff = "9102111020";
				childLine1.JI_InvoiceQuantity = 1000m;
				childLine1.JI_InvoiceUQ = "NO";
				childLine1.JI_CustomsQuantity = 1000m;
				childLine1.JI_CustomsUnitQty = "NO";
				childLine1.JI_LinePrice = 1609m;

				JobComInvoiceLine childLine2 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine2.US_SupTariff = "9802004040";
				childLine2.JI_Tariff = "9102111030";
				childLine2.JI_InvoiceQuantity = 1000m;
				childLine2.JI_InvoiceUQ = "NO";
				childLine2.JI_CustomsQuantity = 1000m;
				childLine2.JI_CustomsUnitQty = "NO";
				childLine2.JI_LinePrice = 1345m;

				JobComInvoiceLine childLine3 = invoiceLine1.AddSecondaryInvoiceLine();
				childLine3.US_SupTariff = "9802004040";
				childLine3.US_98GoodsValue = 204m;
				childLine3.JI_Tariff = "9102111040";
				childLine3.JI_InvoiceQuantity = 1000m;
				childLine3.JI_InvoiceUQ = "NO";
				childLine3.JI_CustomsQuantity = 1000m;
				childLine3.JI_CustomsUnitQty = "NO";
				childLine3.JI_LinePrice = 0m;
			}

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateTIBDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 24055.10m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CN";
			invoiceHeader.JZ_IncoTerm = "FOB";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130030";
			invoiceLine1.JI_InvoiceQuantity = 100m;
			invoiceLine1.JI_InvoiceUQ = "KG";
			invoiceLine1.JI_LinePrice = 10000m;
			invoiceLine1.JI_Tariff = "3920995000";
			invoiceLine1.JI_CustomsQuantity = 10000m;
			invoiceLine1.JI_CustomsUnitQty = "KG";

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration CreateVLineTIBDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "TST-INV1";
			invoiceHeader.JZ_InvoiceAmount = 5000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			invoiceHeader.JZ_RN_NKDefaultOrigin = "CH";
			invoiceHeader.JZ_IncoTerm = "FOB";

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = "98130020";
			invoiceLine1.JI_LinePrice = 0m;
			invoiceLine1.JI_Tariff = "1902194000";
			invoiceLine1.JI_CustomsQuantity = 300m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			invoiceLine1.JI_Weight = 2500m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var secondaryLine1 = invoiceLine1.AddSecondaryInvoiceLine();
			secondaryLine1.US_SupTariff = "98130020";
			secondaryLine1.JI_LinePrice = 1300m;
			secondaryLine1.JI_Tariff = "0712311000";
			secondaryLine1.JI_CustomsQuantity = 300m;
			secondaryLine1.JI_CustomsUnitQty = "KG";
			secondaryLine1.JI_Weight = 650m;
			secondaryLine1.JI_WeightUQ = "KG";
			secondaryLine1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var secondaryLine2 = invoiceLine1.AddSecondaryInvoiceLine();
			secondaryLine2.US_SupTariff = "98130020";
			secondaryLine2.JI_LinePrice = 1300m;
			secondaryLine2.JI_Tariff = "2002908020";
			secondaryLine2.JI_CustomsQuantity = 300m;
			secondaryLine2.JI_CustomsUnitQty = "KG";
			secondaryLine2.JI_Weight = 650m;
			secondaryLine2.JI_WeightUQ = "KG";
			secondaryLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			var secondaryLine3 = invoiceLine1.AddSecondaryInvoiceLine();
			secondaryLine3.US_SupTariff = "98130020";
			secondaryLine3.JI_LinePrice = 2400m;
			secondaryLine3.JI_Tariff = "1902194000";
			secondaryLine3.JI_CustomsQuantity = 300m;
			secondaryLine3.JI_CustomsUnitQty = "KG";
			secondaryLine3.JI_Weight = 1200m;
			secondaryLine3.JI_WeightUQ = "KG";
			secondaryLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;

			Factory.Save();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		JobDeclaration GetDeclarationWithENSMessageAccepted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_DeclarationReference = "B00173079";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var outgoingMessage = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add).PopulateMessage();
			outgoingMessage.EM_MessageNum = "~1500";
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);

			var messageText =
"B001101SV9AX                                               ~1500                " +
"E0 SUMMRY 000001 REF ID: SV9 70022270 B00155595                                 " +
"E0 LINITM 000001 REF ID: 001                                                    " +
"E0 TARIFF 000001 REF ID: 4703110000                                             " +
"E1 W27C   *CENSUS* OR-LO VAL/QTY (1)              SV9  70022270     B00155595   " +
"E1AW995   SUMMARY HAS BEEN ADDED                  SV9  70022270     B00155595   " +
"Y  1101SV9AX00005";

			var incomingMessage2 = CreateResponseMessage(entry, "~1500", messageText, ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, ZDateTime.Now.AddHours(-2));
			entry.Messages.Add(incomingMessage2);
			return declaration;
		}

		JobDeclaration GetDeclarationWithSEMessageAccepted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208.60.2000";
			invoiceLine.JI_CustomsQuantity = 403m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_TaxApply = YesNoDefaultList.Codes.Yes;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_196571";
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, message.EM_MessageType);

			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196571";
			rcvMessage.EM_MessageText =
@"B001101SV9SX                                               HYEDUSCMT_196571     " +
"SE10ASV9  71032807 01EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST0802186                                        00000010     N       " +
"SE20CR B00173079                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			seEntry.Messages.Add(rcvMessage);

			return declaration;
		}

		ZString GetEntryNumber(string nextNum) => nextNum + EntryNumberCheckDigitCalculator.GetCheckDigit("XJ5", nextNum, 0);

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

		MQEDIMessage CreateResponseMessage(BusinessObject bizObj, ZString messageNum, ZString messageText, ZString messageType, ZDateTime createTime)
		{
			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = messageType;
			incomingMessage.EM_MessageNum = messageNum;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_SystemCreateTimeUtc = createTime;
			incomingMessage.EM_LinkedObject = bizObj;
			return incomingMessage;
		}

		OrgHeader CreateOrgForAddressTest(ZString name, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode)
		{
			var org = Factory.New<OrgHeader>();
			org.FillWithValidTestData();
			org.OH_FullName = name;
			org.MainAddress.OA_Address1 = adr1;
			org.MainAddress.OA_Address2 = adr2;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_State = state;
			org.MainAddress.OA_PostCode = postCode;
			return org;
		}

		void AddCustomsAddress(OrgHeader header, ZString adr1, ZString adr2, ZString city, ZString state, ZString postCode)
		{
			var impoterCustomsAddress = header.Addresses.AddNew();
			impoterCustomsAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
			impoterCustomsAddress.OA_Address1 = adr1;
			impoterCustomsAddress.OA_Address2 = adr2;
			impoterCustomsAddress.OA_City = city;
			impoterCustomsAddress.OA_State = state;
			impoterCustomsAddress.OA_PostCode = postCode;
		}

		void AssertTransportDetail(IAESTIRTransportationDetail transportDetail, ZString equipmentNumber, ZString sealNumber, ZString transportationReferenceNumber)
		{
			AssertEquals("EquipmentNumber", equipmentNumber, transportDetail.EquipmentNumber);
			AssertEquals("SealNumber", sealNumber, transportDetail.SealNumber);
			AssertEquals("TransportationReferenceNumber", transportationReferenceNumber, transportDetail.TransportationReferenceNumber);
		}

		void AssertAESParty(IAESTIRParty party, OrgHeader org, USOrganisationDocAddress docAddress, ZString partyID, ZString partyIDType, ZString city, ZString stateCode, ZString countryCode)
		{
			AssertEquals("PartyID", partyID, party.PartyID);
			AssertEquals("PartyIDType", partyIDType, party.PartyIDType);
			AssertEquals("PartyName", org.OH_FullName, party.PartyName);
			ZString[] partContactNames = new ZString[] { "", "" };
			if (org.Contacts.Count > 0)
			{
				partContactNames = org.Contacts[0].OC_ContactName.Split(' ');
			}

			AssertEquals(2, partContactNames.Length);
			AssertEquals("ContactFirstName", partContactNames[0], party.ContactFirstName);
			AssertEquals("ContactLastName", partContactNames[1], party.ContactLastName);

			AssertEquals("AddressLine1", org.MainAddress.OA_Address1.ToUpper(), party.AddressLine1);
			AssertEquals("AddressLine2", org.MainAddress.OA_Address2.ToUpper(), party.AddressLine2);
			AssertEquals("ContactPhoneNumber", docAddress?.E2_Phone ?? ZString.Empty, party.ContactPhoneNumber);
			AssertEquals("PostalCode", org.MainAddress.OA_PostCode.ToUpper(), party.PostalCode);
			AssertEquals("City", city, party.City);
			AssertEquals("StateCode", stateCode, party.StateCode);
			AssertEquals("CountryCode", countryCode, party.CountryCode);
		}

		void CreateTestContainer(JobDeclaration declaration, int containerSeq)
		{
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OCLU12345" + containerSeq.ToString().PadLeft(2, '0');
		}

		sealed class MQEDIMessageForTesting : MQEDIMessage
		{
			public MQEDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
			}
		}

		MQEDIMessage CreateMessage(string receiveTransmit, string applicationIdentifier)
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			MQEDIMessage result = mock.Object;
			result.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			result.EM_ReceiveTransmit = receiveTransmit;
			result.EM_MessageType = applicationIdentifier;
			result.EM_MessageText = @"B018888XJ5CI                                               ~15000               Y  8888XJ5CI00054";

			return result;
		}

		JobDeclaration GetFormalJobDeclaration()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			return declaration;
		}

		void LinkMessageForJobDeclaration(CusEntryHeader cusEntryHeader)
		{
			MQEDIMessage message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = cusEntryHeader;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message.EM_MessageText =
"B018888XJ5UC                                               34                   " +
"E171333   090110                                  XJ5  00000063                 " +
"E2  CHRIS SMITH                     5555555555  123456789012                    " +
"E3REQUESTED DOCUMENTS RECEIVED                                                  " +
"Y  8888XJ5UC00003";
		}

		void SetENSStatusNotification(ErrorsRecord errorsRecord)
		{
			var recordMessage = errorsRecord.message;
			errorsRecord.DispositionCode = ENSStatusDispositionCodeList._4;
			recordMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			recordMessage.EM_ApplicationReference = "4:ACTION REQUIRED";
		}

		void AssertMarkActionsTaken(JobDeclaration declaration)
		{
			var errorsRecordCount = getIncompleteENSStatusNotificationsCount(declaration);
			AssertEquals(1, errorsRecordCount);

			if (declaration.IsRecon)
			{
				errorsRecordCount = getIncompleteENSStatusNotificationsCount(declaration);
				AssertEquals(1, errorsRecordCount);
			}
			else
			{
				declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryReplace;

				errorsRecordCount = getIncompleteENSStatusNotificationsCount(declaration);
				AssertEquals(0, errorsRecordCount);
			}
		}

		JobDeclaration GetReconJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.ImportEntryNumber = "00000063";

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			new ReconDeclaration(declaration);

			return declaration;
		}

		int getIncompleteENSStatusNotificationsCount(JobDeclaration declaration)
		{
			return declaration.ENSStatusNotifications.Cast<ErrorsRecord>().Count(r => !r.message.ActionAuthorised && r.message.EM_ActionStatus == EM_ActionStatusList.Codes.Incomplete);
		}

		const string AcceptedER = "B018888XJ5ER                                               53972                E08888XJ5 70033754B00154240ACCEPTED - RECORDS REQUIRED             21808        E08888XJ5 70033754B00154240CERT-RELEASE CERTIFIED VIA SUMMARY      218082A5     Y  8888XJ5ER00002            000000143723";

		const string RejectedER = "B018888XJ5ER                                               27428                10A888891-01319900091-013199000                 9112408   XJ5 7000575221089  IL E108888XJ5 70005752   61821   SURETY REVOKED                           B001510819000000000000000000356630 00000000000000000000000000000250000000010000          E908888XJ5 70005752   52421   TRANSACTION DATA REJECTED                B00151081Y  8888XJ5ER00004            000000035663";

		const string SOMessage =
@"B001101SV9SO                                                                    " +
"SO101101SV9  71020679 0158-123456789                                            " +
"SO20CR B00166239                                                                " +
"SO40RAPLUMST051316                                         00000100     00000000" +
"SO50051316101491NO BILL MATCH                                                   " +
"SO70NHTREI051316101307MAY PROCEED                   07  00011001              01" +
"SO70APHAPL051316101307MAY PROCEED                   07  00021001              01" +
"Y  1101SV9SO00000";

		sealed class CusEntryHeaderForTesting : CusEntryHeader
		{
			public CusEntryHeaderForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Customs.Business.CusEntryHeaderValidation GetNewValidationReturns { get; set; }

			protected override Customs.Business.CusEntryHeaderValidation GetNewValidation()
			{
				return GetNewValidationReturns;
			}
		}

		sealed class CusEntryHeaderValidationForTest : CusEntryHeaderValidation
		{
			public CusEntryHeaderValidationForTest(CusEntryHeader entry)
				: base(entry)
			{
			}

			protected override void CheckCH_EntryStatus()
			{
				base.CheckCH_EntryStatus();
				if (Parent.CH_EntryStatus == "A")
				{
					Parent.CH_EntryStatusInfo.AddWarning("AAAAA_BBBBB");
				}
			}
		}

		sealed class CusEntryHeaderThrowingExceptionAfterOnSavingForTest : CusEntryHeader
		{
			public CusEntryHeaderThrowingExceptionAfterOnSavingForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool ShouldThrowException;

			public override void OnSaving()
			{
				base.OnSaving();
				if (ShouldThrowException)
				{
					throw new ApplicationException("intended");
				}
			}
		}

		sealed class JobDeclarationForTesting : JobDeclaration
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public int DeriveExportDeclarationStatusCallCount { get; set; }

			protected override void DeriveExportDeclarationStatus()
			{
				DeriveExportDeclarationStatusCallCount++;
			}
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override ZBool IsPTTCleared => true;
		}
	}
}
