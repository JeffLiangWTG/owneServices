using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ETradeHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, Processor.ApplicationCode);
		}

		public void TestGetCorrectBranchPK()
		{
			var header = Factory.New<ETradeHeader>();
			var pk = ZGuid.NewZGuid();
			header.AMA_GB = pk;
			AssertEquals(pk, Processor.GetCorrectBranchPK((BusinessObject)header));
			AssertEquals(ZGuid.Empty, Processor.GetCorrectBranchPK(null));
		}

		ETradeMessageProcessorForTest Processor => processor ?? (processor = new ETradeMessageProcessorForTest(new LoggingInformation()));
		ETradeMessageProcessorForTest processor;

		#region Emails

		public void TestSendNoEmails()
		{
			ETradeSendNoEmails();
			ETradeImportDischargeListSendNoEmails();
			ETradeExportRegistrationNoSendNoEmails();
		}

		void ETradeExportRegistrationNoSendNoEmails()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULT";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010103";

			var message = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Response (Failure) for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for" + header.AMA_JobReference);
			AssertNull(email);
		}

		void ETradeSendNoEmails()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZX";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";

			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Response (Failure) for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for" + header.AMA_JobReference);
			AssertNull(email);
		}

		void ETradeImportDischargeListSendNoEmails()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZT";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Response (Failure) for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("NOE", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for" + header.AMA_JobReference);
			AssertNull(email);
		}

		public void TestSendEmailStaffMember()
		{
			ETradeSendEmailStaffMember();
			ETradeImportDischargeListSendEmailStaffMember();
			ETradeExportRegistrationNoSendEmailStaffMember();
		}

		void ETradeExportRegistrationNoSendEmailStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZA";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z4";
			staff1.GS_LoginName = "Z4";
			staff1.GS_EmailAddress = "test@test.mail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010103";
			var message = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", "TRE", messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void ETradeSendEmailStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZX";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z2";
			staff1.GS_LoginName = "Z2";
			staff1.GS_EmailAddress = "test@test.mail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";

			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void ETradeImportDischargeListSendEmailStaffMember()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZT";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z3";
			staff1.GS_LoginName = "Z3";
			staff1.GS_EmailAddress = "test@test.mail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessageForImportDischargeList(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Import Discharge List Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Import Discharge List Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		public void TestSendEmailNominatedGroup()
		{
			ETradeSendEmailNominatedGroup();
			ETradeImportDischargeLisSendEmailNominatedGroup();
			ETradeExportRegistrationNoSendEmailNominatedGroup();
		}

		void ETradeExportRegistrationNoSendEmailNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "X2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT3", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULF";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U4";
			staff1.GS_LoginName = "U4";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010103";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010103", "TRE", messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", "TRE", messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
			});
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeSendEmailNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULL";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U2";
			staff1.GS_LoginName = "U2";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
			});
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeImportDischargeLisSendEmailNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULT";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U3";
			staff1.GS_LoginName = "U3";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			SetupOriginalETradeMessageForImportDischargeList(header, messageText);
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Import Discharge List Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Import Discharge List Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
			});
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ENG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		public void TestSendEmailStaffMemberandNominatedGroup()
		{
			EtradeSendEmailStaffMemberandNominatedGroup();
			EtradeImportDischargeListSendEmailStaffMemberandNominatedGroup();
			EtradeExportRegistrationNoSendEmailStaffMemberandNominatedGroup();
		}

		void EtradeExportRegistrationNoSendEmailStaffMemberandNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "X2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT3", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULF";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U4";
			staff1.GS_LoginName = "U4";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010103";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals(2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void EtradeSendEmailStaffMemberandNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULL";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U2";
			staff1.GS_LoginName = "U2";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void EtradeImportDischargeListSendEmailStaffMemberandNominatedGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULT";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U3";
			staff1.GS_LoginName = "U3";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessageForImportDischargeList(header, messageText);
			Factory.Save();
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals("expected recipents", 2, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Import Discharge List Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Import Discharge List Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(2, email.Recipients.Count);
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[1].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		public void TestSendEmailStaffMemberOrNominatedGroupForGroup()
		{
			ETradeSendEmailStaffMemberOrNominatedGroupForGroup();
			ETradeImportDischargeListSendEmailStaffMemberOrNominatedGroupForGroup();
			ETradeExportRegistrationNoSendEmailStaffMemberOrNominatedGroupForGroup();
		}

		void ETradeExportRegistrationNoSendEmailStaffMemberOrNominatedGroupForGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "X2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT3", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULT";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U4";
			staff1.GS_LoginName = "U4";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010103";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", Staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeSendEmailStaffMemberOrNominatedGroupForGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULL";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U2";
			staff1.GS_LoginName = "U2";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeImportDischargeListSendEmailStaffMemberOrNominatedGroupForGroup()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "UL3";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U3";
			staff1.GS_LoginName = "U3";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Import Discharge List Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Import Discharge List Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		public void TestSendEmailStaffMemberOrNominatedGroupForStaff()
		{
			ETradeSendEmailStaffMemberOrNominatedGroupForStaff();
			ETradeImportDischargeListSendEmailStaffMemberOrNominatedGroupForStaff();
			ETradeExportRegistrationNoSendEmailStaffMemberOrNominatedGroupForStaff();
		}

		void ETradeSendEmailStaffMemberOrNominatedGroupForStaff()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULL";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U2";
			staff1.GS_LoginName = "U2";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeExportRegistrationNoSendEmailStaffMemberOrNominatedGroupForStaff()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "X2";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT3", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULT";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U4";
			staff1.GS_LoginName = "U4";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010103";
			header.AMA_OA_Carrier = orgAddress.PK;
			var message = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1700");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010103", TRMessageTypes.Codes.TRS, messageText, "1800");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Message", bodyText.Contains("Error Message"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		void ETradeImportDischargeListSendEmailStaffMemberOrNominatedGroupForStaff()
		{
			var orgProxy = Factory.New<OrgHeader>();
			orgProxy.OH_Code = "Z3";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_Address1 = "line";
			orgAddress.OA_OH = orgProxy.PK;
			var orgCusCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT3", Core.Constants.CountryCodes.Turkey);
			orgCusCode.OK_OA_PremisesAddress = orgAddress.PK;
			var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			company.GC_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULM";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U3";
			staff1.GS_LoginName = "U3";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessageForImportDischargeList(header, messageText);
			Factory.Save();
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				AssertEquals(1, email.Recipients.Count);
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, true));
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);
			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals(1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff1.GS_EmailAddress));
				Assert("Contains Error Code", bodyText.Contains("Error Code"));
				Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
				AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
			});
		}

		#endregion

		#region ProcessMessages

		public void TestProcessMessage()
		{
			ETradeProcessMessage();
			ETradeImportDischargeListProcessMessage();
			ETradeExportRegistrationNoProcessMessage();
		}

		void ETradeProcessMessage()
		{
			var messageText = TRMessageTestHelper.GetFileText("ETradeSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var parentNodeList = new List<ZString>() { "ETicaretSoapOut", "Record", "Sonuc" };
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/" };
			var xmlData = Enterprise.Customs.TR.Messaging.TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:ServisCevabiSorgulamaResponse/B:ServisCevabiSorgulamaResult", namespaceList);
			var tescilNo = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(xmlData, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010101";

			var message = CreateETradeMessage("ETG0010101", "TRE", messageText, "1500");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessage(header, messageText);
			Factory.Save();
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
				Assert("Contains Temporary Registration Number", bodyText.Contains("Temporary Registration Number"));
				Assert("Contains Temporary Registration Date", bodyText.Contains("Temporary Registration Date"));
				AssertEquals("Temporary Registration Number", tescilNo, header.TempRegNo);
				AssertEquals("Temporary Registration Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.TempRegNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.TRS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = TRMessageTestHelper.GetFileText("ETradeError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.ETrade.");
			var message2 = CreateETradeMessage("ETG0010101", "TRE", messageText, "1600");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			eTradeTemporaryRegistrationProcessor.ProcessMessage(message2);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Temporary Registration Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Contains Error Information", bodyText.Contains("Error Message"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
		}

		void ETradeExportRegistrationNoProcessMessage()
		{
			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");
			var header = Factory.New<ETradeHeader>();
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_JobReference = "ETG0010104";
			var message = CreateETradeMessage("ETG0010104", TRMessageTypes.Codes.TRS, messageText, "1900");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupETradeExportRegNoMessage(header, messageText);
			Factory.Save();
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, email.Recipients.Count);
				Assert(email.Recipients.Contains(Staff2.GS_EmailAddress));
				Assert("Contains Export Registration No Number", bodyText.Contains("Export Registration No Number"));
				Assert("Contains Export Registration No Date", bodyText.Contains("Export Registration No Date"));
				AssertEquals("Export Registration No Number", tescilNo, header.RegistrationNumber);
				AssertEquals("Export Registration No Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.RNS, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRS, true);
			var message2 = CreateETradeMessage("ETG0010104", TRMessageTypes.Codes.TRS, messageText, "1900");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			eTradeExportRegistrationNoMessageProcessor.ProcessMessage(message2);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Export Registration No Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", Staff2.GS_EmailAddress, email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Contains Error Code", bodyText.Contains("Error Code"));
			Assert("Contains Error Message", bodyText.Contains("Error Message"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
		}

		void ETradeImportDischargeListProcessMessage()
		{
			var messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD);
			var parentNodeList = new List<ZString>() { "Envelope", "Body", "ServisCevabiSorgulamaResponse", "ServisCevabiSorgulamaResult", "Hatalar", "Hata" };
			var tescilNo = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitNo");
			var tescilTarihi = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "KayitTarihi");

			var header = Factory.New<ETradeHeader>();
			header.AMA_JobReference = "ETG0010105";

			var message = CreateETradeMessage("ETG0010105", "TRD", messageText, "1505");
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			SetupOriginalETradeMessageForImportDischargeList(header, messageText);
			Factory.Save();
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response for " + header.AMA_JobReference);
			AssertNotNull(email);
			var bodyText = email.Body;
			CombineAssertions(() =>
			{
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", true, email.Recipients.Contains("Dummy9@dummy.com"));
				Assert("Contains Import Discharge List Number", bodyText.Contains("Import Discharge List Number"));
				Assert("Contains Import Discharge List Date", bodyText.Contains("Import Discharge List Date"));
				AssertEquals("Import Discharge List Number", tescilNo, header.DischargeRecordNo);
				AssertEquals("Import Discharge List Date", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.DischargeRecordNoDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", CustomsStatusList.Codes.DLS, header.RegistrationStatus);
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			messageText = TRMessageTestHelper.GetBodyText(TRMessageTypes.Codes.TRD, true);
			var message2 = CreateETradeMessage("ETG0010106", "TRD", messageText, "1605");
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
			eTradeImportDischargeListMessageProcessor.ProcessMessage(message2);

			email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "E-Trade Import Discharge List Message Status Response (Failure) for " + header.AMA_JobReference);
			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals("One recipient", 1, email.Recipients.Count);
				AssertEquals("Email Address", "Dummy9@dummy.com", email.Recipients[0].Email);
			});
			bodyText = email.Body;
			Assert("Contains Error Information", bodyText.Contains("Message Status Response (Failure) for"));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("AMA_MessageStatus", "ERR", header.AMA_MessageStatus);
		}

		ETradeEDIMessage CreateETradeMessage(ZString applicationReference, ZString messageType, ZString messageText, string messageNum)
		{
			var message = Factory.New<ETradeEDIMessage>();
			message.EM_ApplicationReference = applicationReference;
			message.EM_Status = EDIMessage.Status.Received;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = messageType;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum;
			return message;
		}

		#endregion

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			eTradeTemporaryRegistrationProcessor = new ETradeTemporaryRegistrationMessageProcessor(new LoggingInformation());
			eTradeExportRegistrationNoMessageProcessor = new ETradeExportRegistrationNoMessageProcessor(new LoggingInformation());
			eTradeImportDischargeListMessageProcessor = new ETradeImportDischargeListMessageProcessor(new LoggingInformation());
		}

		ETradeImportDischargeListMessageProcessor eTradeImportDischargeListMessageProcessor;
		ETradeTemporaryRegistrationMessageProcessor eTradeTemporaryRegistrationProcessor;
		ETradeExportRegistrationNoMessageProcessor eTradeExportRegistrationNoMessageProcessor;

		void SetupOriginalETradeMessage(ETradeHeader header, ZString messageText)
		{
			var message1 = Factory.New<ETradeEDIMessage>();
			message1.EM_SystemCreateUser = Staff1.GS_Code;
			message1.EM_ApplicationReference = "ETG0010101";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = TRMessageTypes.Codes.TRE;
			message1.EM_MessageText = messageText;
			message1.EM_MessageNum = "1500";
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			var message2 = Factory.New<ETradeEDIMessage>();
			message2.EM_SystemCreateUser = Staff2.GS_Code;
			message2.EM_ApplicationReference = "ETG0010102";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = TRMessageTypes.Codes.TRE;
			message2.EM_MessageText = messageText;
			message2.EM_MessageNum = "1600";
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
		}

		void SetupETradeExportRegNoMessage(ETradeHeader header, ZString messageText)
		{
			var message1 = Factory.New<ETradeEDIMessage>();
			message1.EM_SystemCreateUser = Staff1.GS_Code;
			message1.EM_ApplicationReference = "ETG0010103";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = TRMessageTypes.Codes.TRS;
			message1.EM_MessageText = messageText;
			message1.EM_MessageNum = "1700";
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			var message2 = Factory.New<ETradeEDIMessage>();
			message2.EM_SystemCreateUser = Staff2.GS_Code;
			message2.EM_ApplicationReference = "ETG0010104";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = TRMessageTypes.Codes.TRS;
			message2.EM_MessageText = messageText;
			message2.EM_MessageNum = "1900";
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
		}

		void SetupOriginalETradeMessageForImportDischargeList(ETradeHeader header, ZString messageText)
		{
			var message1 = Factory.New<ETradeEDIMessage>();
			message1.EM_SystemCreateUser = Staff1.GS_Code;
			message1.EM_ApplicationReference = "ETG0010105";
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message1.EM_Status = EDIMessageStatusList.Codes.Received;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_MessageType = TRMessageTypes.Codes.TRD;
			message1.EM_MessageText = messageText;
			message1.EM_MessageNum = "1505";
			message1.EM_LinkUniqueID = header.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			var message2 = Factory.New<ETradeEDIMessage>();
			message2.EM_SystemCreateUser = Staff2.GS_Code;
			message2.EM_ApplicationReference = "ETG0010106";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			message2.EM_Status = EDIMessageStatusList.Codes.Received;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = TRMessageTypes.Codes.TRD;
			message2.EM_MessageText = messageText;
			message2.EM_MessageNum = "1605";
			message2.EM_LinkUniqueID = header.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();
		}

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

		#endregion

	}

	class ETradeMessageProcessorForTest : ETradeMessageProcessor
	{
		public ETradeMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "ETrade Message Processor For Test";

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess) => ZString.Empty;

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => ZString.Empty;

		protected override bool ProcessMessageCore(ETradeEDIMessage message) => true;

		public new ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => base.GetCorrectBranchPK(linkedObject);
	}
}
