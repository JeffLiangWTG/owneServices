using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Integration.Customs.TR;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class SPTSBranchCustomsApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		#region Emails

		public void TestSPTSSendNoEmails()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZG";

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var message = CreateSPTSMessage(header, messageText);
			SetupOriginalSPTSMessage(header, messageText);

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			sPTSProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			sPTSProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			sPTSProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			sPTSProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			AssertNull(email);

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestSPTSSendEmailStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZG";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z5";
			staff1.GS_LoginName = "Z5";
			staff1.GS_EmailAddress = "test@test.mail.com";
			Factory.Save();

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var message = CreateSPTSMessage(header, messageText);
			SetupOriginalSPTSMessage(header, messageText);
			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("AMA_MessageStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestSPTSSendEmailNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "2";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULG";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U5";
			staff1.GS_LoginName = "U5";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var message = CreateSPTSMessage(header, messageText);
			SetupOriginalSPTSMessage(header, messageText);
			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		public void TestSPTSSendEmailStaffMemberAndNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "2";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULX";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U5";
			staff1.GS_LoginName = "U5";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			SetupOriginalSPTSMessage(header, messageText);
			var message = CreateSPTSMessage(header, messageText);
			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("expected recipents", 2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("expected recipents", 2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("expected recipents", 2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}
		public void TestSPTSSendEmailStaffMemberOrNominatedGroupForGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "2";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULX";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U5";
			staff1.GS_LoginName = "U5";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml"));
			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}
		public void TestSPTSSendEmailStaffMemberOrNominatedGroupForStaff()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "2";
			orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULX";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U5";
			staff1.GS_LoginName = "U5";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml"));
			SetupOriginalSPTSMessage(header, messageText);
			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		#endregion

		#region ProcessMessages

		public void TestT1PProcessMessage()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");
			var expectedInterpretation = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.htm");

			var message = CreateSPTSMessage(header, messageText);

			sPTSProcessor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("message.EM_MessageInterpretation", expectedInterpretation, message.EM_MessageInterpretation);
			});
		}

		public void TestT1PErrorMessage()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml");
			var expectedErrorMessage = TRMessageTestHelper.GetFileText("SPTS.SPTSError.htm");
			var message = CreateSPTSMessage(header, messageText);

			sPTSProcessor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
				AssertEquals(expectedErrorMessage, message.EM_MessageInterpretation);
			});
		}

		public void TestTSPErrorMessage()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			var expectedErrorMessage = TRMessageTestHelper.GetFileText("SPTS.TSPErrorResponse.htm");
			var message = CreateT1pIncomingMessage(header);

			sPTSProcessor.ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
				AssertEquals(expectedErrorMessage, message.EM_MessageInterpretation);
			});
		}

		public void TestSPTSProcessMessage()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "2020/00084/5";

			var messageText = TRMessageTestHelper.GetFileText("SPTS.SPTSSuccess.xml");

			var message = CreateSPTSMessage(header, messageText);
			SetupOriginalSPTSMessage(header, messageText);

			sPTSProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response for " + header.BH_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number:"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date:"));
				AssertEquals("RegistrationNumber", "20590100HT000198", header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime("20/08/2020 14:39:29").ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("EntryStatus", "REG", header.MessageStatus);
				AssertEquals("message.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			message = CreateSPTSMessage(header, TRMessageTestHelper.GetFileText("SPTS.SPTSError.xml"));
			sPTSProcessor.ProcessMessage(message);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message"));
				AssertEquals("EntryStatus", "ERR", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "SPTS Status Message Response (Failure) for " + header.BH_JobReference);
			CombineAssertions(() =>
			{
				AssertNull(email);
				AssertEquals("EntryStatus", "AWA", header.MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		EDIMessage CreateT1pIncomingMessage(ICusInBondSPTSHeader header)
		{
			var messageTrackingID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
			var guid = "b5d51907-d1bf-4852-91c6-5c33e535a167";
			var factory = Factory;
			var group = factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var tspOutgoingMessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingTest.xml");
			var tspOutgoingInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TSP, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, messageTrackingID, tspOutgoingMessageText);
			var tspOutgoingMessage = CreateMessage(factory, TRMessageTypes.Codes.TSP, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, CusInBondHeader.Schema.TableName, header.PK, tspOutgoingMessageText, tspOutgoingInterchange.PK);
			tspOutgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var tspIncomingMessageText = TRMessageTestHelper.GetFileText("SPTS.SPTSOutgoingResponse.xml");
			var tspIncomingInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TSP, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, messageTrackingID, tspIncomingMessageText);
			var tspIncomingMessage = CreateMessage(factory, TRMessageTypes.Codes.TSP, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, CusInBondHeader.Schema.TableName, header.PK, tspIncomingMessageText, tspIncomingInterchange.PK);
			tspIncomingMessage.EM_SystemCreateUser = staff.GS_Code;
			tspIncomingMessage.EM_ApplicationReference = guid;

			var t1pOutgoingMessageText = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml");
			var t1pOutgoingInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T1P, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, messageTrackingID, t1pOutgoingMessageText);
			var t1pOutgoingMessage = CreateMessage(factory, TRMessageTypes.Codes.T1P, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, CusInBondHeader.Schema.TableName, header.PK, t1pOutgoingMessageText, t1pOutgoingInterchange.PK);
			t1pOutgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			var t1pIncomingMessageText = TRMessageTestHelper.GetFileText("SPTS.TSPErrorResponse.xml");
			var t1pIncomingInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T1P, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, messageTrackingID, t1pIncomingMessageText);
			var t1pIncomingMessage = CreateMessage(factory, TRMessageTypes.Codes.T1P, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, CusInBondHeader.Schema.TableName, header.PK, t1pIncomingMessageText, t1pIncomingInterchange.PK);
			t1pIncomingMessage.EM_SystemCreateUser = staff.GS_Code;

			return t1pIncomingMessage;
		}

		SPTSMessage CreateSPTSMessage(ICusInBondSPTSHeader header, ZString messageText)
		{
			var message = Factory.New<SPTSMessage>();
			message.EM_ApplicationReference = header.BH_JobReference;
			message.EM_LinkTable = CusInBondHeader.Schema.TableName;
			message.EM_LinkUniqueID = header.PK;
			message.EM_MessageNum = "33";
			message.EM_MessageText = messageText;
			message.EM_MessageInterpretation = messageText;
			message.EM_MessageType = TRMessageTypes.Codes.TSP;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;

			Factory.Save();
			return message;
		}

		public static EDIMessage CreateMessage(BusinessObjectFactory factory, ZString messageType, ZString direction, ZString status, ZString linkTable, ZGuid linkID, ZString messageText, ZGuid interchangePK)
		{
			var message = factory.New<SPTSMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = direction;
			message.EM_Status = status;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_MessageText = messageText;
			message.EM_LinkTable = linkTable;
			message.EM_LinkUniqueID = linkID;
			message.EM_EI = interchangePK;

			return message;
		}

		public void TestUpdateEM_ApplicationReferenceWhenGuidHasValue()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "ULU-2019/00002379";
			var messageText = TRMessageTestHelper.GetFileText("ResponseWithGuidValue.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Common.");
			var message = CreateSPTSMessage(header, messageText);

			sPTSProcessor.ProcessMessage(message);

			var guid = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
			AssertEquals("EM_ApplicationReference should be equal to the Guid value", guid, message.EM_ApplicationReference);

			CombineAssertions("Message Interpretation", () =>
			{
				var expectedInterpretation = TRMessageTestHelper.GetFileText("SPTS.ResponseWithGuidValue.htm");
				AssertEquals("EM_MessageInterpretation", expectedInterpretation, message.EM_MessageInterpretation);
			});
		}

		[TestDate(2022, 09, 05)]
		public void TestCreateT1PMessageForErrorMessage()
		{
			var factory = Factory;
			var header = factory.New<ICusInBondSPTSHeader>();
			header.BH_JobReference = "ULU-2019/00002379";
			header.BH_GB = GlbBranch.CurrentBranch.PK;

			sPTSProcessor.ProcessMessage(CreateT1pIncomingMessage(header));

			var spts = factory.Load<ICusInBondSPTSHeader>(header.PK);
			var expectedFormettedMessageText = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml");

			CombineAssertions(() =>
			{
				AssertEquals("Messages.Count", 5, spts.Messages.Count);
				var message = (EDIMessage)spts.Messages[4];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "T1P", message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedFormettedMessageText, message.EM_MessageText);
			});
		}

		#endregion

		#region Setups

		void SetupOriginalSPTSMessage(ICusInBondSPTSHeader header, ZString messageText)
		{
			var message1 = Factory.New<SPTSMessage>();
			message1.EM_SystemCreateUser = Staff1.GS_Code;
			message1.EM_ApplicationReference = "2020/00084/5";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = TRMessageTypes.Codes.TSP;
			message1.EM_MessageText = messageText;
			message1.EM_MessageNum = "33";
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = CusInBondHeader.Schema.TableName;

			var message2 = Factory.New<SPTSMessage>();
			message2.EM_SystemCreateUser = Staff2.GS_Code;
			message2.EM_ApplicationReference = "2020/00084/5";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = TRMessageTypes.Codes.TSP;
			message2.EM_MessageText = messageText;
			message2.EM_MessageNum = "44";
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = CusInBondHeader.Schema.TableName;
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			sPTSProcessor = new SPTSBranchCustomsApplicationTypeMessageProcessor(new LoggingInformation());
		}
		SPTSBranchCustomsApplicationTypeMessageProcessor sPTSProcessor;

		#endregion

		#region Staffs

		GlbStaff Staff1 => staff1 ?? (staff1 = CreateStaff("S09", "S09", "Staff09", "Dummy9@dummy.com"));
		GlbStaff staff1;

		GlbStaff Staff2 => staff2 ?? (staff2 = CreateStaff("S08", "S08", "Staff08", "Dummy8@dummy.com"));
		GlbStaff staff2;

		GlbStaff CreateStaff(ZString code, ZString loginName, ZString fullName, ZString email)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = code;
			staff.GS_LoginName = loginName;
			staff.GS_FullName = fullName;
			staff.GS_EmailAddress = email;
			return staff;
		}

		#endregion

		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, sPTSProcessor.ApplicationCode);
		}

		public void TestGetCorrectBranchPK()
		{
			var header = Factory.New<ICusInBondSPTSHeader>();
			var pk = ZGuid.NewZGuid();
			header.BH_GB = pk;
			AssertEquals(pk, TestProcessor.GetCorrectBranchPK((BusinessObject)header));
			AssertEquals(ZGuid.Empty, TestProcessor.GetCorrectBranchPK(null));
		}

		SPTSBranchCustomsApplicationTypeMessageProcessorForTest TestProcessor => testProcessor ?? (testProcessor = new SPTSBranchCustomsApplicationTypeMessageProcessorForTest(new LoggingInformation()));
		SPTSBranchCustomsApplicationTypeMessageProcessorForTest testProcessor;
	}

	class SPTSBranchCustomsApplicationTypeMessageProcessorForTest : SPTSBranchCustomsApplicationTypeMessageProcessor
	{
		public SPTSBranchCustomsApplicationTypeMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "Manifest Message Processor For Test";

		public new ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, SPTSMessage message, bool isSuccess) => ZString.Empty;

		public new string MailSubject(IMessageAttachee messageAttacheeBO, SPTSMessage message) => ZString.Empty;

		public new bool ProcessMessageCore(SPTSMessage message) => true;

		public new ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => base.GetCorrectBranchPK(linkedObject);
	}
}

