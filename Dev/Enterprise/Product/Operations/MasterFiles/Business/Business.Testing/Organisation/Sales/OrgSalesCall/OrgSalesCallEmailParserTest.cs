using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using MimeKit;
using MsgReader.Outlook;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.BounceEmailParser;
using static Enterprise.MasterFiles.Business.OrgSalesCallEmailParser;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesCallEmailParserTest : TestCaseWithFactory
	{
		static readonly string testDirectory = Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\MasterFiles\Business\MasterFiles.Business\Testing\");
		static readonly string sampleEmailTestFile = Path.Combine(testDirectory, "SampleEmail_ParsingOntoCommunication.eml");
		static readonly string sampleMsgTestFile = Path.Combine(testDirectory, "SampleMsg_ParsingOntoCommunication.msg");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseMsgFileWithNoHeaders()
		{
			var sampleMsg = Path.Combine(testDirectory, "SampleForTest - Accepted_CRM_ Standup.msg");
			var parser = new OrgSalesCallEmailParserForTesting(Factory.NewWithValidTestData<OrgSalesCall>(), new GUIProviderTest());
			AssertEquals(Result.NoMatchingContacts, parser.PopulateFromEmailFile(sampleMsg));
			AssertEquals("\r\n", parser.Parent.OQ_SalesCallNotes.ToUTF8());
			AssertEquals(new ZDateTime("2023-05-16 01:56:53.9149904"), parser.Parent.OQ_CallDate);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseMsgFileWithParagraphs()
		{
			var sampleMsgTestFileWithParagraphs = Path.Combine(testDirectory, "SampleMsg_ParsingParagraphs.msg");
			var parser = new OrgSalesCallEmailParserForTesting(Factory.NewWithValidTestData<OrgSalesCall>(), new GUIProviderTest());
			parser.PopulateFromEmailFile(sampleMsgTestFileWithParagraphs);
			AssertEquals("1\r\n2", parser.Parent.OQ_SalesCallNotes.ToUTF8());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseEmlFileMultipartAlternative()
		{
			var sampleEmlTestFileWithMultipartAlternative = Path.Combine(testDirectory, "SampleEML_MultipartAlternative.eml");
			var parser = new OrgSalesCallEmailParserForTesting(Factory.NewWithValidTestData<OrgSalesCall>(), new GUIProviderTest());
			parser.PopulateFromEmailFile(sampleEmlTestFileWithMultipartAlternative);
			AssertEquals("one two", parser.Parent.OQ_SalesCallNotes.ToUTF8());
		}

		public void TestNotEmailFormat()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			IGUIProvider provider = new GUIProviderTest();
			OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(salesCall, provider);

			ZString fileName1 = "1.txt";
			Result result1 = parser.PopulateFromEmailFile(fileName1);
			AssertEquals(Result.NotEmailFormat, result1);
		}

		public void TestInvalidEmailFormat()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			IGUIProvider provider = new GUIProviderTest();
			OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(salesCall, provider);

			ZString fileName1 = "1.msg";
			Result result1 = parser.PopulateFromEmailFile(fileName1);
			AssertEquals(Result.NotEmailFormat, result1);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseItemFromEmailFile()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "jason.zhu@wisetechglobal.com";
			Factory.Save();

			var message = MimeMessage.Load(sampleEmailTestFile);

			IGUIProvider provider = new GUIProviderTest();
			OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(communication, provider);
			Result result = parser.PopulateFromEmailFile(sampleEmailTestFile);

			AssertEquals(Result.Success, result);
			AssertEquals(new DateTime(2017, 07, 27, 06, 26, 03), parser.Parent.OQ_CallDate);
			AssertEquals(new ZString(message.Subject).SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength), parser.Parent.OQ_CallSummary);
			AssertEquals(ZBlob.FromUTF8(parser.GetPlainTextFromHtml(message.HtmlBody)), parser.Parent.OQ_SalesCallNotes);
			AssertEquals(orgContact.PK, parser.Parent.OQ_OC);
			AssertEquals(orgContact.OC_OH, parser.Parent.OQ_OH);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseItemFromMsgFile()
		{
			using (var message = new Storage.Message(sampleMsgTestFile))
			{
				var communication = Factory.NewWithValidTestData<OrgSalesCall>();
				OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();
				orgContact.OC_Email = "jason.zhu@wisetechglobal.com";
				Factory.Save();

				IGUIProvider provider = new GUIProviderTest();
				OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(communication, provider);

				Result result = parser.PopulateFromEmailFile(sampleMsgTestFile);

				AssertEquals(Result.Success, result);
				AssertEquals(new DateTime(2017, 07, 27, 20, 39, 44), parser.Parent.OQ_CallDate);
				AssertEquals(new ZString(message.Subject).SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength), parser.Parent.OQ_CallSummary);
				AssertEquals(ZBlob.FromUTF8(parser.GetPlainTextFromHtml(message.BodyHtml)), parser.Parent.OQ_SalesCallNotes);
				AssertEquals(orgContact.PK, parser.Parent.OQ_OC);
				AssertEquals(orgContact.OC_OH, parser.Parent.OQ_OH);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseItemWithCampaignsFromEmailFile()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();
			OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();

			var campaign = Factory.New<IGlbCompanyCampaign>();
			campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
			campaign.G0_CampaignID = "42";
			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = orgContact.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID] = orgContact.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode] = OrgContactSchema.Constants.Prefix;

			Factory.Save();

			var message = MimeMessage.Load(sampleEmailTestFile);
			message.Headers.Remove(BounceEmailConstants.BusinessEntityTableCodeKey);
			message.Headers.Remove(BounceEmailConstants.BusinessEntityIDKey);
			message.Headers.Add(BounceEmailConstants.BusinessEntityTableCodeKey, GlbCompanyCampaignItemSchema.Constants.Prefix);
			message.Headers.Add(BounceEmailConstants.BusinessEntityIDKey, campaignItem.PK.ToString());

			IGUIProvider provider = new GUIProviderTest();
			OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(communication, provider);
			Result result = parser.ParseMailItem(message);

			AssertEquals(Result.Success, result);
			AssertEquals(new DateTime(2017, 07, 27, 06, 26, 03), parser.Parent.OQ_CallDate);
			AssertEquals(new ZString(message.Subject).SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength), parser.Parent.OQ_CallSummary);
			AssertEquals(ZBlob.FromUTF8(parser.GetPlainTextFromHtml(message.HtmlBody)), parser.Parent.OQ_SalesCallNotes);
			AssertEquals(orgContact.PK, parser.Parent.OQ_OC);
			AssertEquals(orgContact.OC_OH, parser.Parent.OQ_OH);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestParseItemWithCampaignsFromMsgFile()
		{
			using (var message = new Storage.Message(sampleMsgTestFile))
			{
				var communication = Factory.NewWithValidTestData<OrgSalesCall>();
				OrgContact orgContact = Factory.NewWithValidTestData<OrgContact>();

				var campaign = Factory.New<IGlbCompanyCampaign>();
				campaign.G0_CampaignName = ZGuid.NewZGuid().ToString();
				campaign.G0_CampaignID = "42";
				BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
				campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
				campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = orgContact.PK;
				campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID] = orgContact.PK;
				campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode] = OrgContactSchema.Constants.Prefix;
				Factory.Save();

				message.Headers.UnknownHeaders.Add(BounceEmailConstants.BusinessEntityTableCodeKey, GlbCompanyCampaignItemSchema.Constants.Prefix);
				message.Headers.UnknownHeaders.Add(BounceEmailConstants.BusinessEntityIDKey, campaignItem.PK.ToString());

				IGUIProvider provider = new GUIProviderTest();
				OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(communication, provider);
				Result result = parser.ParseMailItem(message);

				AssertEquals(Result.Success, result);
				AssertEquals(new DateTime(2017, 07, 27, 20, 39, 44), parser.Parent.OQ_CallDate);
				AssertEquals(new ZString(message.Subject).SubstringSafe(0, OrgSalesCallSchema.OQ_CallSummary.MaxLength), parser.Parent.OQ_CallSummary);
				AssertEquals(ZBlob.FromUTF8(parser.GetPlainTextFromHtml(message.BodyHtml)), parser.Parent.OQ_SalesCallNotes);
				AssertEquals(orgContact.PK, parser.Parent.OQ_OC);
				AssertEquals(orgContact.OC_OH, parser.Parent.OQ_OH);
			}
		}

		public void TestParseItemWithMoreContacts()
		{
			var communication = Factory.NewWithValidTestData<OrgSalesCall>();

			OrgContact orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			string email1 = "jason.zhu@wisetechglobal.com";
			orgContact1.OC_Email = email1;
			orgContact1.OC_IsActive = false;

			OrgContact orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			string email2 = "Jason.Li@wisetechglobal.com";
			orgContact2.OC_Email = email2;
			orgContact2.OC_IsActive = true;

			Factory.Save();

			string[] emails = new string[] { email1, email2 };
			var contactQuery = new ZQuery(OrgContactSchema.OC_Email, emails);
			var contacts = new OrgContactCollection(Factory, contactQuery);
			contacts.Load();

			IGUIProvider provider = new GUIProviderTest();
			OrgSalesCallEmailParserForTesting parser = new OrgSalesCallEmailParserForTesting(communication, provider);
			OrgContact orgContact = parser.GetContactFromEmailAddresses(emails);

			AssertEquals(orgContact2.PK, orgContact.PK);
		}

		public void TestParseItemWithInvalidContentFromEmailFile()
		{
			using (var tempMsgFile = TempFile.NewWithExtension("msg"))
			using (var tempEmlFile = TempFile.NewWithExtension("eml"))
			{
				var fileData = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
				File.WriteAllBytes(tempMsgFile.Filename, fileData);
				File.WriteAllBytes(tempEmlFile.Filename, fileData);
				var parser = new OrgSalesCallEmailParserForTesting(Factory.NewWithValidTestData<OrgSalesCall>(), new GUIProviderTest());
				AssertEquals(Result.InvalidEmailFormat, parser.PopulateFromEmailFile(tempMsgFile.Filename));
				AssertEquals(Result.InvalidEmailFormat, parser.PopulateFromEmailFile(tempEmlFile.Filename));
			}
		}
	}
}
