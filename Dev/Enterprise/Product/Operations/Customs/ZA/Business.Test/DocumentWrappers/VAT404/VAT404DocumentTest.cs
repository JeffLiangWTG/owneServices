using System.IO;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.MessageManagers.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(VAT404Document))]
	sealed class VAT404DocumentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentMenuExists()
		{
			AssertNotNull(VAT404TestHelper.GetVAT404Document(Factory).VAT404ProofOfPaymentMenu);
		}

		public void TestDeliveryDocument()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			var document = VAT404TestHelper.GetVAT404Document(Factory);
			CombineAssertions(() =>
			{
				document.DeliveryContacts.RemoveAll();
				IMessageNotificationCollector notification = new MessageNotificationCollector_ForTest();
				document.DeliverDocument(notification);
				AssertEquals("Document for " + document.Importer.OH_Code + " not delivered: No valid recipient was found.", notification.Notifications.WarningNotificationsAsString());
				AssertEquals("", notification.Notifications.InformationNotificationsAsString());
				AssertEquals(0, Factory.Load<StmPrintJob>(new ZQuery()).Length);
			});
			CombineAssertions(() =>
			{
				var contact = document.DeliveryContacts.AddNew();
				contact.DeliveryMethod = "EML";
				contact.Email = "test@test.com";
				IMessageNotificationCollector notification = new MessageNotificationCollector_ForTest();
				document.DeliverDocument(notification);
				AssertEquals("", notification.Notifications.ErrorNotificationsAsString());
				AssertEquals("Document for " + document.Importer.OH_Code + " has been queued for Delivery", notification.Notifications.InformationNotificationsAsString());
				AssertEquals(1, Factory.Load<StmPrintJob>(new ZQuery()).Length);
			});
		}

		[TestDate(2019, 07, 04)]
		public void TestPreviewDocument()
		{
			var filter = new DocumentZQuery(BusinessContext.ZAProofOfPayment, "VAT 404 Proof Of Payment");
			var docMenu = Factory.LoadTop1<DocumentCommand>(filter);
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER";
			testContact.OC_Email = "mango@peeters.com";
			var cneOD = testContact.Documents.AddNew();
			cneOD.OD_DocumentGroup = "CNE";
			cneOD.OD_DeliverBy = "EML";
			cneOD.OD_AttachmentType = "PDF";
			cneOD.OD_OC = testContact.PK;
			cneOD.OD_FilterDirection = "ALL";
			cneOD.OD_FilterShipmentMode = "ALL";
			var cneCC = cneOD.CarbonCopyRecipients.AddNew();
			cneCC.ODR_EmailAddress = "django123@peeters.com";
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN001");
			entry.Declaration.JE_OH_Importer = testOrg.PK;
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			Factory.Save();
			var instruction = new VAT404DocumentInstruction(Factory);
			instruction.PerformSearch();
			var tester = instruction.VAT404Documents.FirstOrDefault() as VAT404Document;
			var existingFiles = new DirectoryInfo(EnvProxy.Instance.TempPath).GetFiles("*.xls").Select(x => x.Name).ToList();
			tester.PreviewDocument();
			var excelFiles = new DirectoryInfo(EnvProxy.Instance.TempPath).GetFiles("*.xls").Where(x => !existingFiles.Contains(x.Name)).ToArray();
			AssertEquals(1, excelFiles.Length);
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(excelFiles[0].FullName);
				var workSheet = excelInterface.WorkSheets[0];
				AssertContains("{C}-[LRN001]   {L}-[2019-07-04]   {O}-[B00001000]   {Q}-[12]", workSheet.ToString());
			}

			File.Delete(excelFiles[0].FullName);
		}

		public void TestDeliveryDocumentSPE()
		{
			var filter = new DocumentZQuery(BusinessContext.ZAProofOfPayment, "VAT 404 Proof Of Payment");
			var docMenu = Factory.LoadTop1<DocumentCommand>(filter);
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER";
			testContact.OC_Email = "mango@peeters.com";
			var cneOD = testContact.Documents.AddNew();
			cneOD.OD_DocumentGroup = "CNE";
			cneOD.OD_DeliverBy = "EML";
			cneOD.OD_AttachmentType = "PDF";
			cneOD.OD_OC = testContact.PK;
			cneOD.OD_FilterDirection = "ALL";
			cneOD.OD_FilterShipmentMode = "ALL";
			var cneCC = cneOD.CarbonCopyRecipients.AddNew();
			cneCC.ODR_EmailAddress = "django123@peeters.com";
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN001");
			entry.Declaration.JE_OH_Importer = testOrg.PK;
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			Factory.Save();
			var instruction = new VAT404DocumentInstruction(Factory);
			instruction.PerformSearch();
			var tester = instruction.VAT404Documents.FirstOrDefault() as VAT404Document;
			tester.DeliveryContacts[0].DeliveryMethod = "EML";
			tester.DeliveryContacts[0].DeliveryAddress = "7563@8956.com";
			CombineAssertions(() =>
			{
				AssertEquals(tester.DeliveryContacts[0].DeliveryAddress, "7563@8956.com");
				IMessageNotificationCollector notification = new MessageNotificationCollector_ForTest();
				tester.DeliverDocument(notification);
				AssertEquals(tester.DeliveryContacts[0].DeliveryAddress, "7563@8956.com");
			});
		}

		public void TestLoadingDeliveryContact()
		{
			var filter = new DocumentZQuery(BusinessContext.ZAProofOfPayment, "VAT 404 Proof Of Payment");
			var docMenu = Factory.LoadTop1<DocumentCommand>(filter);
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER1";
			testContact.OC_Email = "tester@tester.com";
			var cneOD = testContact.Documents.AddNew();
			cneOD.OD_DocumentGroup = "CNE";
			cneOD.OD_DeliverBy = "EML";
			cneOD.OD_AttachmentType = "PDF";
			cneOD.OD_OC = testContact.PK;
			cneOD.OD_FilterDirection = "ALL";
			cneOD.OD_FilterShipmentMode = "ALL";
			var cneCC = cneOD.CarbonCopyRecipients.AddNew();
			cneCC.ODR_EmailAddress = "CNE@CNE.COM";
			testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER2";
			testContact.OC_Email = "tester@tester.com";
			var acrOD = testContact.Documents.AddNew();
			acrOD.OD_DocumentGroup = "A/R";
			acrOD.OD_DeliverBy = "EML";
			acrOD.OD_AttachmentType = "PDF";
			acrOD.OD_OC = testContact.PK;
			acrOD.OD_FilterDirection = "ALL";
			acrOD.OD_FilterShipmentMode = "ALL";
			var acrCC = acrOD.CarbonCopyRecipients.AddNew();
			acrCC.ODR_EmailAddress = "acr@acr.COM";
			testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER3";
			testContact.OC_Email = "tester@tester.com";
			var allOD = testContact.Documents.AddNew();
			allOD.OD_DocumentGroup = "ALL";
			allOD.OD_DeliverBy = "EML";
			allOD.OD_AttachmentType = "PDF";
			allOD.OD_OC = testContact.PK;
			allOD.OD_FilterDirection = "ALL";
			allOD.OD_FilterShipmentMode = "ALL";
			var allCC = allOD.CarbonCopyRecipients.AddNew();
			allCC.ODR_EmailAddress = "all@all.COM";
			testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "TESTER4";
			testContact.OC_Email = "tester@tester.com";
			var docOD = testContact.Documents.AddNew();
			docOD.OD_SU_MenuItem = docMenu.PK;
			docOD.OD_DeliverBy = "EML";
			docOD.OD_AttachmentType = "PDF";
			docOD.OD_OC = testContact.PK;
			docOD.OD_FilterDirection = "ALL";
			docOD.OD_FilterShipmentMode = "ALL";
			var docCC = docOD.CarbonCopyRecipients.AddNew();
			docCC.ODR_EmailAddress = "doc@doc.COM";
			var entry = VAT404TestHelper.SetupEntry(Factory, "LRN001");
			entry.Declaration.JE_OH_Importer = testOrg.PK;
			VAT404TestHelper.AddPayInfo(entry, "VAT", 12m, "RCP01", ZDateTime.Today, ZDateTime.Today);
			Factory.Save();
			var instruction = new VAT404DocumentInstruction(Factory);
			instruction.PerformSearch();
			var tester = instruction.VAT404Documents.FirstOrDefault() as VAT404Document;
			CombineAssertions(() =>
			{
				AssertEquals(3, tester.DeliveryContacts.Count);
				Assert("Check Subscribing ALL", tester.DeliveryContacts.Cast<DocDeliveryContact>().Any(x => x.EmailCarbonCopyRecipientsAsString == "all@all.COM"));
				Assert("Check Subscribing Specific Document", tester.DeliveryContacts.Cast<DocDeliveryContact>().Any(x => x.EmailCarbonCopyRecipientsAsString == "doc@doc.COM"));
				Assert("Check Subscribing A/R", tester.DeliveryContacts.Cast<DocDeliveryContact>().Any(x => x.EmailCarbonCopyRecipientsAsString == "acr@acr.COM"));
				Assert("Check NOT Subscribing CNE", !tester.DeliveryContacts.Cast<DocDeliveryContact>().Any(x => x.EmailCarbonCopyRecipientsAsString == "CNE@CNE.COM"));
			});
		}

		protected override BusinessObject GetNewBusinessObject() => VAT404TestHelper.GetVAT404Document(Factory);
	}
}
