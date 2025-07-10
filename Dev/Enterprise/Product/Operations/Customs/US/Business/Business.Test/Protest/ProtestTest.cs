using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Protest.Testing
{
	[TestedType(typeof(Protest))]
	sealed class ProtestTestCase : NonPersistentBusinessObjectTestCase
	{
		public void TestEnsureThatAddInfoDataIsSaved()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var protest = new Protest(declaration);
			protest.US_P_MerchandiseDesc = "TEST";
			protest.US_P_AddressTeam = "XYZ";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			protest = new Protest(declaration);
			AssertEquals("US_P_MerchandiseDesc", "TEST", protest.US_P_MerchandiseDesc);
			AssertEquals("US_P_AddressTeam", "XYZ", protest.US_P_AddressTeam);
		}

		public void TestMatchesFilter()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var protestDec = new Protest(declaration);
			Factory.Save();

			var dbOnlyQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			dbOnlyQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.MoreCodes.Protest);
			Assert(declaration.MatchesFilter(dbOnlyQuery));
			Assert(protestDec.MatchesFilter(dbOnlyQuery));
			dbOnlyQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, String.Empty);
			Assert(!declaration.MatchesFilter(dbOnlyQuery));
			Assert(!protestDec.MatchesFilter(dbOnlyQuery));
		}

		public void TestExceptionGeneratedForProtestDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var reconDeclaration = new ReconDeclaration(declaration);
			var mileStone = reconDeclaration.WorkflowItems.AddNew();
			mileStone.P9_Type = Core.Constants.Workflow.MilestoneType;
			mileStone.P9_ParentTableCode = "JE";
			mileStone.P9_ParentID = declaration.PK;
			mileStone.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today.AddDays(-1));
			mileStone.SetMilestoneExceptionAddedForTest(ZDateTimeOffset.Invalid);
			mileStone.P9_SE_NKExceptionEvent = "";
			Factory.Save();

			var type = Type.GetType("Enterprise.WorkflowManager.ServiceTasks.WorkflowExceptionGenerationProcessor, Enterprise.WorkflowManager.ServiceTasks", true);
			var obj = Activator.CreateInstance(type) as IProcessor;
			var notifications = new ZArchitecture.NotificationBuffer();

			AssertNoExceptionThrown("No exceptio should be thrown when processor is running", () =>
			{
				obj.Process(notifications);
			});
		}

		public void TestHumanReadableShortcutNameInProtest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var protestDec = new Protest(declaration);
			declaration.JE_DeclarationReference = "S005202392";
			Protest.Protestant.E2_CompanyName = "ORGFULLNAME";

			AssertEquals("Protest Job# ImportFullName", string.Format("Protest - {0}{1}", declaration.JE_DeclarationReference, protestDec.Protestant.E2_CompanyName.IsEmpty ? "" : " - " + protestDec.Protestant.E2_CompanyName), protestDec.HumanReadableShortcutName);
		}

		public void TestDeclarationPK()
		{
			var protest = new Protest(Declaration);
			AssertEquals(Declaration.PK, protest.PK);
		}

		public void TestJobStatus()
		{
			var protest = new Protest(Declaration);
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = Declaration.PK;
			Declaration.Job.JH_Status = JobHeaderStatus.Working.Code;
			AssertEquals(JobHeaderStatus.Working.Code, protest.JobStatus);
		}

		public void TestAllocationUseMutex()
		{
			var protest = new Protest(Declaration);
			Factory.Save();

			var cBPAssignedProtestNumberRefreshCount = 0;
			protest.US_P_CBPAssignedProtestNumberInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				cBPAssignedProtestNumberRefreshCount++;
			};

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var declarationInDiffFactory = newFactory.Load<JobDeclaration>(Declaration.PK);
			var protestInDiffFactory = new Protest(declarationInDiffFactory);
			IAllocateNumberSupporter supporterInDiffFactory = protestInDiffFactory;
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			supporterInDiffFactory.DoAllocate("12345678");
			AssertEquals("Allocated", "12345678", protestInDiffFactory.US_P_CBPAssignedProtestNumber);

			IAllocateNumberSupporter supporter = protest;
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			supporter.UnlockNumberAllocationMutex();
			AssertEquals(false, supporter.LockNumberAllocationMutex);
			AssertEquals(true, supporterInDiffFactory.LockNumberAllocationMutex);
			AssertEquals("", supporter.GetReasonToStopProceeding());

			newFactory.Save();
			AssertEquals(0, cBPAssignedProtestNumberRefreshCount);
			AssertEquals(Protest.Constants.CBPNumberAlreadyAllocated("12345678"), supporter.GetReasonToStopProceeding());
			AssertEquals("12345678", protest.US_P_CBPAssignedProtestNumber);
			AssertEquals(1, cBPAssignedProtestNumberRefreshCount);
			supporterInDiffFactory.UnlockNumberAllocationMutex();
		}

		public void TestSetDefaultValues()
		{
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "9898");
			USCustomsDataRegistry.Instance.EntryDeclarant.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SVA";
			staff.GS_EmailAddress = "dummy@email.com";

			var previousUser = GlbStaff.CurrentUser.GS_Code;
			var systemAccount = GlbStaff.CurrentUser.GS_IsSystemAccount;
			GlbStaff.CurrentUser.GS_Code = staff.GS_Code;
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;

			AssertEquals(ZString.Empty, Protest.US_P_AddressTeam);
			AssertEquals(TariffActCitationList.Codes.A_Section514, Protest.TariffActCitation);
			AssertEquals("9898", Protest.US_P_FilingDDPP);
			AssertEquals(JobMessageTypeList.MoreCodes.Protest, Declaration.JE_MessageType);
			AssertEquals("SVA", Protest.JE_GS_NKCusAgent);

			GlbStaff.CurrentUser.GS_Code = previousUser;
			GlbStaff.CurrentUser.GS_IsSystemAccount = systemAccount;
		}

		public void TestIAllocateEntryNumberSupporter()
		{
			var protest = new Protest(Declaration);
			var supporter = (IAllocateNumberSupporter)Protest;
			supporter.DoAllocate("342089");

			AssertEquals("342089", protest.US_P_CBPAssignedProtestNumber);

			AssertEquals(Protest.Constants.CBPNumberAlreadyAllocated("342089"), supporter.GetReasonToStopProceeding());
			AssertEquals("342089", supporter.GetExistingNumber());
			AssertNotNull(supporter.GetNewAllocateNumber());
		}

		public void TestProtestantRegistrationNumberResultIsDefaulted()
		{
			var organisation = Factory.New<OrgHeader>();
			Protest.Protestant.OrganisationPK = organisation.PK;
			Protest.Protestant.E2_AddressOverride = true;
			AssertEquals("", Protest.Protestant.E2_GovRegNum);

			organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-123456700");

			Protest.Protestant.E2_AddressOverride = false;
			Protest.Protestant.E2_AddressOverride = true;
			AssertEquals("12-123456700", Protest.Protestant.E2_GovRegNum);

			Protest.Protestant.E2_AddressOverride = false;
			AssertEquals("12-123456700", Protest.Protestant.E2_GovRegNum);
		}

		public void TestProtestantIDIsValidated()
		{
			Protest.Protestant.E2_AddressOverride = true;

			Protest.Protestant.E2_GovRegNum = "X";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "123";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "12-1234567AU";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "123-12-1234";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "20ABCD-12345";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "AAA123456";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "AAAA123456AAA";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "AAA123456AAA";
			AssertEquals(false, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "1234";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "12D3";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "123SDAFSDFAD";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "1-2-3";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());

			Protest.Protestant.E2_GovRegNum = "A-B-C";
			AssertEquals(true, Protest.Protestant.E2_GovRegNumInfo.HasMessageErrors());
		}

		public void TestProtestantCountryIsValidated()
		{
			Protest.Protestant.E2_AddressOverride = true;

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(false, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			AssertEquals(false, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;
			AssertEquals(false, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Chile;
			AssertEquals(true, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			AssertEquals(true, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.TariffActCitation = TariffActCitationList.Codes.C_Section520d;
			Protest.Protestant.Validation.ValidateE2_RN_NKCountryCode();
			AssertEquals(false, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			AssertEquals(true, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AssertEquals(true, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());

			Protest.Protestant.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AssertEquals(true, Protest.Protestant.E2_RN_NKCountryCodeInfo.HasMessageErrors());
		}

		public void TestJustificationNote()
		{
			string expected = "some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text some long text ";
			Protest.JustificationNote = "SOMETEXT";
			Factory.Save();
			Protest.JustificationNote = "";
			Factory.Save();
			Protest.JustificationNote = "TEST";
			Protest.JustificationNote = "";
			Protest.JustificationNote = expected;
			AssertEquals(9945, Protest.JustificationNote.Length);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<JobDeclaration>(Declaration.PK);
			var newProtest = new Protest(declaration);
			AssertEquals(expected, newProtest.JustificationNote);
		}

		public void TestLinkedEntries()
		{
			var linkedEntry = Protest.LinkedEntries.AddNew();
			linkedEntry.US_LE_EntryNumber = "TEST";
			AssertEquals("TEST", Protest.LinkedEntries[0].US_LE_EntryNumber);
		}

		public void TestValidation()
		{
			AssertNotNull(Protest.Validation);
		}

		public void TestLookups()
		{
			AssertNotNull(Protest.Lookups);
		}

		public void TestLogs()
		{
			Protest.Factory.Save();
			AssertEquals(1, Protest.GetLogs().GetAllLogs().Count);
		}

		public void TestNotes()
		{
			Protest.GetNotes().AddNew(true, "Test", "Note Text");
			Protest.Factory.Save();
			AssertEquals(1, Protest.GetNotes().GetAllNotes().Count);
		}

		public void TestMessages()
		{
			Protest.Messages.AddNew(typeof(EDIMessage));
			AssertEquals(1, Protest.Messages.Count);
		}

		public void TestIsInDatabase()
		{
			AssertEquals(false, Protest.IsInDatabase);
			Protest.Factory.Save();
			AssertEquals(true, Protest.IsInDatabase);
		}

		public void TestDelete()
		{
			var declarationPK = Declaration.PK;
			Protest.Factory.Save();
			Protest.Delete();
			Factory.Save();

			AssertNull(Factory.Load<Protest>(declarationPK));
		}

		public void TestIsSavedByFactory()
		{
			AssertEquals(true, Protest.IsSavedByFactory);
		}

		public void TestCanSendWithdrawal()
		{
			Protest.US_P_CBPAssignedProtestNumber = ZString.Empty;
			AssertEquals(false, Protest.CanSendWithdrawal);

			Protest.US_P_CBPAssignedProtestNumber = "200600314565";
			AssertEquals(true, Protest.CanSendWithdrawal);
		}

		public void TestIEDocsProviderMembers()
		{
			var eDocsProvider = (IEDocsProvider)GetNewBusinessObject();
			AssertEquals("EDocsProviderSupporter type", typeof(JobInvoicingEDocsProviderSupporter), eDocsProvider.GetEDocsProviderSupporter().GetType());
		}

		public void TestIEDocsPluginHostDeciderMembers()
		{
			var iEDocsPluginHostDecider = (IEDocsPluginHostDecider)GetNewBusinessObject();
			AssertEquals("HostBusinessEntity for Protest should be the wrapped Declaration", Declaration, iEDocsPluginHostDecider.HostBusinessEntity);
		}

		public void TestHasEntriesAreNotLiquidated()
		{
			var entry = Protest.LinkedEntries.AddNew();
			entry.US_LE_LiquidationDate = ZDateTime.Today;
			var entry2 = Protest.LinkedEntries.AddNew();
			AssertEquals(true, Protest.HasEntriesNotLiquidated);

			entry2.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-4);
			AssertEquals(false, Protest.HasEntriesNotLiquidated);
		}

		public void TestServiceLevel_CS00172151()
		{
			var serviceLevelCode = "STD";
			var serviceLevel = Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, serviceLevelCode);
			if (serviceLevel == null)
			{
				serviceLevel = Factory.NewWithValidTestData<RefServiceLevel>();
				serviceLevel.RS_Code = serviceLevelCode;
			}
			serviceLevel.RS_IsActive = false;
			Factory.Save();

			var declaration = Protest.Declaration;
			AssertEquals("Service Level should be empty on wrapped declaration", ZString.Empty, declaration.JE_RS_NKServiceLevel);

			declaration.RunPreSaveValidation();
			AssertNotContains("Error - JE_RS_NKServiceLevel: This Service is inactive - it may not be used", declaration.Notifications.ToUniqueMessageListString());
		}

		public void TestProtestStatusFields()
		{
			AssertEquals(ZString.Empty, Protest.ProtestStatus);
			AssertEquals(ZString.Empty, Protest.ProtestStatusDescription);
			AssertEquals(ZDateTime.Empty, Protest.US_P_StatusDate);
			AssertEquals(ZString.Empty, Protest.StatusDisposition);

			Declaration.JE_EntryStatus = ProtestStatusCodesList.Codes.P;
			AssertEquals("P", Protest.ProtestStatus);
			AssertEquals("Partly Denied", Protest.ProtestStatusDescription);

			Protest.US_P_StatusDate = ZDateTime.Now.Date;
			AssertEquals(ZDateTime.Now.Date, Protest.US_P_StatusDate);
			AssertEquals("Readonly in GUI", true, Protest.US_P_StatusDateInfo.ReadOnly);

			var message = Protest.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			message.EM_MessageText = "B015501125SS                                                                    P10530912150009B12O010691  21                                    O20120627      P11N        N        N        6S120120807                                       P12E0220120807AWAITING RECONSTRUC'D ENTRY SUMMARIES FROM THE PROTEST FILER      Y  5501125SS00003";

			message = Protest.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_MessageText = "B015501125SS                                                                    P10550112150033B12O008937  21R20120522                           A20120625      P11N        N        N        6PO20120625                                       P12E0420120629PROTEST DECIDED                                                   Y  5501125SS00003";
			AssertEquals("E04 PROTEST DECIDED", Protest.StatusDisposition);

			message = Protest.Messages.AddNew(typeof(MQEDIMessage));
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ProtestAutomaticNotificationandResponsetoFilerQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(2);
			message.EM_MessageText = "B015501125SS                                                                    P10180113150001B13O015104  11R20130114                           O20130114      P11N        N        N        44420130114                                       Y  5501125SS00002";
			AssertEquals(ZString.Empty, Protest.StatusDisposition);
		}

		public void TestRefundPartyId()
		{
			var declaration = Factory.New<JobDeclaration>();
			var protest = new Protest(declaration);
			AssertEquals("RefundPartyId (1)", ZString.Empty, protest.RefundPartyId);

			var refundParty = Factory.New<OrgHeader>();
			protest.RefundPartyAddress.OrganisationPK = refundParty.PK;

			AssertEquals("RefundPartyId (2)", ZString.Empty, protest.RefundPartyId);

			refundParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber, "22");
			AssertEquals("RefundPartyId (3)", ZString.Empty, protest.RefundPartyId);

			refundParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "333");
			AssertEquals("RefundPartyId (4)", "333", protest.RefundPartyId);
		}

		public void TestRefundParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var protest = new Protest(declaration);
			AssertEquals("RefundParty DocAddressType", DocAddressTypes.Codes.RefundParty, protest.RefundPartyAddress.E2_AddressType);
			Factory.Save();

			var refundPartyOrgHeader = Factory.New<OrgHeader>();
			var refundPartyOrgAddress = Factory.New<OrgAddress>();
			refundPartyOrgAddress.OA_OH = refundPartyOrgHeader.PK;
			protest.RefundPartyAddress.E2_OA_Address = refundPartyOrgAddress.PK;

			AssertEquals("RefundParty OrgHeader", refundPartyOrgHeader, protest.RefundParty);
		}

		public void TestRefundPartyAddressRequirement()
		{
			var declaration = Factory.New<JobDeclaration>();
			var protest = new Protest(declaration);
			protest.US_P_RefundCOPartyType = "T";
			protest.RefundPartyAddress.OrganisationPK = ZGuid.Empty;
			protest.RunPreSaveValidation();
			AssertEquals(true, protest.RefundPartyAddress.OrganisationPKInfo.HasMessageErrors());
		}

		public void TestProcessTaskLoadType()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var protest = new Protest(declaration);
			AssertEquals(false, declaration.IsInDatabase);
			AssertEquals(false, protest.IsInDatabase);

			var workitem = protest.WorkflowItems.AddNew();
			Factory.Save();

			var proLoaded = Factory.Load<JobDeclaration>(declaration.PK);
			var reLoadWorkItem = proLoaded.WorkflowItems.FindByPK(workitem.PK);
			AssertNotNull(reLoadWorkItem);
			AssertType<BaseJobDeclarationProcessTask<JobDeclaration>>(reLoadWorkItem);
		}

		public void TestIJobHeaderParent_AllowInvoiceDeletion()
		{
			IJobHeaderParent protest = Factory.New<Protest>();
			Assert(protest.AllowInvoiceDeletion);
		}

		public void TestPropertiesForDocumentPrinting()
		{
			Protest.JustificationNote = "Any authorized agent of any of the persons described above. 1. Name and address of the protestant; 2. The importer number of the protestant; 3. The number and date(s) of the entry(s); 4. The date of liquidation of the entry (or the date of a decision); 5. A specific description of the merchandise; 6. The nature of and justification for the objection set forth distinctly and specifically with respect to each category, claim, decision, or refusal; 7. The date of receipt and protest number of any protest previously filed that is the subject of a pending application for further review; and 8. If another party has not filed a timely protest, the surety's protest shall certify that the protest is not being filed collusively to extend another authorized person''s time to protest. 9. Whether accelerated disposition is being requested.";
			AssertEquals("Any authorized agent of any of the persons described above. 1. Name and address of the protestant; 2. The importer number of the protestant; 3. The number and date(s) of the entry(s); 4. The date of liquidation of the entry (or the date of a decision); 5. A specific description of the merchandise; 6. The nature of and justification for the objection set forth distinctly and specifically with respect to each category, claim, decision, or refusal; 7. The date of receipt and protest number of any protest previously filed that is the subject of a pending application for further review; and 8. If another party has not filed a timely protest, the surety's protest shall certify that the protest is ", Protest.JustificationArgumentsSection5);
			AssertEquals("not being filed collusively to extend another authorized person''s time to protest. 9. Whether accelerated disposition is being requested.", Protest.JustificationArgumentsContinuation);

			var cusAgentStaffMember = Factory.NewWithValidTestData<GlbStaff>();
			cusAgentStaffMember.GS_Code = "JLB";
			cusAgentStaffMember.GS_FullName = "JIMMY BEAN";
			cusAgentStaffMember.GS_WorkPhone = "+1 738 2956000";
			cusAgentStaffMember.GS_EmailAddress = "jimmy.beam@somecompany.somewhere";

			Protest.Declaration.JE_GS_NKCusAgent = cusAgentStaffMember.GS_Code;
			AssertNotNull(protest.Broker);

			var entry1 = Protest.LinkedEntries.AddNew();
			entry1.US_LE_EntryNumber = "XJ500000011";
			entry1.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-5);
			entry1.US_LE_PortCode = "8881";
			entry1.US_LE_EntryDate = ZDateTime.Today;

			var entry2 = Protest.LinkedEntries.AddNew();
			entry2.US_LE_EntryNumber = "XJ50000";
			entry2.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-10);
			entry2.US_LE_PortCode = "8882";
			entry2.US_LE_EntryDate = ZDateTime.Today.AddDays(-1);

			var entry3 = Protest.LinkedEntries.AddNew();
			entry3.US_LE_EntryNumber = "XJ500000022";
			entry3.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-20);
			entry3.US_LE_PortCode = "8883";
			entry3.US_LE_EntryDate = ZDateTime.Today.AddDays(-2);

			var entry4 = Protest.LinkedEntries.AddNew();
			entry4.US_LE_EntryNumber = "XJ500000033";
			entry4.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-30);
			entry4.US_LE_PortCode = "8884";
			entry4.US_LE_EntryDate = ZDateTime.Today.AddDays(-3);

			var entry5 = Protest.LinkedEntries.AddNew();
			entry5.US_LE_EntryNumber = "XJ500000044";
			entry5.US_LE_LiquidationDate = ZDateTime.Today.AddDays(-40);
			entry5.US_LE_PortCode = "8885";
			entry5.US_LE_EntryDate = ZDateTime.Today.AddDays(-4);

			AssertEquals("Liquidation Date for Entry 1", ZDateTime.Today.AddDays(-5), Protest.LinkedEntryLiquidationDate1);
			AssertEquals("Liquidation Date for Entry 2", ZDateTime.Today.AddDays(-10), Protest.LinkedEntryLiquidationDate2);
			AssertEquals("Liquidation Date for Entry 3", ZDateTime.Today.AddDays(-20), Protest.LinkedEntryLiquidationDate3);
			AssertEquals("Liquidation Date for Entry 4", ZDateTime.Today.AddDays(-30), Protest.LinkedEntryLiquidationDate4);
			AssertEquals("Liquidation Date for Entry 5", ZDateTime.Today.AddDays(-40), Protest.LinkedEntryLiquidationDate5);

			AssertEquals("Entry Date for Entry 1", ZDateTime.Today, Protest.LinkedEntryDate1);
			AssertEquals("Entry Date for Entry 2", ZDateTime.Today.AddDays(-1), Protest.LinkedEntryDate2);
			AssertEquals("Entry Date for Entry 3", ZDateTime.Today.AddDays(-2), Protest.LinkedEntryDate3);
			AssertEquals("Entry Date for Entry 4", ZDateTime.Today.AddDays(-3), Protest.LinkedEntryDate4);
			AssertEquals("Entry Date for Entry 5", ZDateTime.Today.AddDays(-4), Protest.LinkedEntryDate5);

			AssertEquals("Port for Entry 1", "8881", Protest.LinkedEntryPort1);
			AssertEquals("Port for Entry 2", "8882", Protest.LinkedEntryPort2);
			AssertEquals("Port for Entry 3", "8883", Protest.LinkedEntryPort3);
			AssertEquals("Port for Entry 4", "8884", Protest.LinkedEntryPort4);
			AssertEquals("Port for Entry 5", "8885", Protest.LinkedEntryPort5);

			AssertEquals("Filer Code for Entry 1", "XJ5", Protest.LinkedEntryFiler1);
			AssertEquals("Filer Code for Entry 2", ZString.Empty, Protest.LinkedEntryFiler2);
			AssertEquals("Filer Code for Entry 3", "XJ5", Protest.LinkedEntryFiler3);
			AssertEquals("Filer Code for Entry 4", "XJ5", Protest.LinkedEntryFiler4);
			AssertEquals("Filer Code for Entry 5", "XJ5", Protest.LinkedEntryFiler5);

			AssertEquals("Entry Number 1", "0000001", Protest.LinkedEntryNumTruncated1);
			AssertEquals("Entry Number 2", ZString.Empty, Protest.LinkedEntryNumTruncated2);
			AssertEquals("Entry Number 3", "0000002", Protest.LinkedEntryNumTruncated3);
			AssertEquals("Entry Number 4", "0000003", Protest.LinkedEntryNumTruncated4);
			AssertEquals("Entry Number 5", "0000004", Protest.LinkedEntryNumTruncated5);

			AssertEquals("Check Digit for Entry Number 1", "1", Protest.LinkedEntryNumCheckDigit1);
			AssertEquals("Check Digit for Entry Number 2", ZString.Empty, Protest.LinkedEntryNumCheckDigit2);
			AssertEquals("Check Digit for Entry Number 3", "2", Protest.LinkedEntryNumCheckDigit3);
			AssertEquals("Check Digit for Entry Number 4", "3", Protest.LinkedEntryNumCheckDigit4);
			AssertEquals("Check Digit for Entry Number 5", "4", Protest.LinkedEntryNumCheckDigit5);
		}

		protected override BusinessObject GetNewBusinessObject() => Protest;

		Protest protest;
		Protest Protest => protest ?? (protest = new Protest(Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
