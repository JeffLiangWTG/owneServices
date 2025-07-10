using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Recruiter.Business.HRJobApplicationEmailParser;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationEmailParserTest : TestCaseWithFactory
	{
		public void TestTryExtractDataManuallyWorkaround()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var parser = GetParser<HRJobApplication>(true, false, false, true, true);
			parser.PopulateFromFile(CantReadEmailAddressPath);

			AssertNotNull(parser.Parent.Applicant);
			AssertEquals("chrismiles2156@gmail.com", parser.Parent.Applicant.HA_EmailAddress);
			AssertEquals("0435369239", parser.Parent.Applicant.HA_MobilePhone);
		}

		public void TestTryExtractDataManuallyWorkaround_ShouldNotOverridePhone()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var parser = GetParser<HRJobApplication>(true, false, false, true, false);
			parser.PopulateFromFile(CantReadEmailAddressPath);

			AssertNotNull(parser.Parent.Applicant);
			AssertEquals("chrismiles2156@gmail.com", parser.Parent.Applicant.HA_EmailAddress);
			AssertEquals("0499702888", parser.Parent.Applicant.HA_MobilePhone);
		}

		public void TestTryExtractDataManuallyWorkaround_ShouldNotOverrideEmail()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var parser = GetParser<HRJobApplication>(true, false, false, false, true);
			parser.PopulateFromFile(CantReadEmailAddressPath);

			AssertNotNull(parser.Parent.Applicant);
			AssertEquals("email@gmail.com", parser.Parent.Applicant.HA_EmailAddress);
			AssertEquals("0435369239", parser.Parent.Applicant.HA_MobilePhone);
		}

		public void TestErrorMessageOnException()
		{
			var parser = GetParser<HRJobApplication>(false);
			var result = parser.PopulateFromFile(EmptyMsgPath);
			Assert(result.ParseResult.HasFlag(ParseResult.InvalidFileFormat));
			AssertEquals("boom\r\n", result.Error);
		}

		public void TestPopulateFromFile_Email()
		{
			var parser = GetParser<HRJobApplication>();
			var result = parser.PopulateFromFile(EmptyMsgPath);
			AssertApplicant(parser.Parent);
			AssertEquals(ParseResult.None | ParseResult.IsEmail, result.ParseResult);
		}

		public void TestPopulateFromFile_Email_BadEml()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParser(application, new DaxtraResumeParser());
			var result = parser.PopulateFromFile(BadEmailPath);
			Assert(result.ParseResult.HasFlag(ParseResult.NoResumeParsed));
			AssertEquals("HRJobApplicationEmailParser.PopulateFromFile.FormatException", ErrorReporter.LastKeyReported);
			AssertContains("Exception parsing EML file: bad.eml", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPopulateFromFile_ReturnsError()
		{
			var mailItem = Factory.NewWithValidTestData<MailItem>();
			var parser = new HRJobApplicationEmailParserForTest(mailItem, new DaxtraResumeParser(), Factory, true);
			var result = parser.PopulateFromFile(BadEmailPath);
			AssertEquals("boom\r\n", result.Error);
			AssertEquals("HRJobApplicationEmailParserForTest.PopulateFromFile.FormatException", ErrorReporter.LastKeyReported);
			AssertContains("Exception parsing EML file: bad.eml", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPopulateFromFile_Resume_Pdf()
		{
			var parser = GetParser<HRJobApplication>();
			var emptyPdfPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.pdf", "empty.pdf");
			var result = parser.PopulateFromFile(emptyPdfPath);
			AssertApplicant(parser.Parent);
			AssertEquals(ParseResult.None, result.ParseResult);
		}

		public void TestPopulateFromFile_Resume_Doc()
		{
			var parser = GetParser<HRJobApplication>();
			var emptyDocPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.doc", "empty.doc");
			var result = parser.PopulateFromFile(emptyDocPath);
			AssertApplicant(parser.Parent);
			AssertEquals(ParseResult.None, result.ParseResult);
		}

		public void TestPopulateFromFile_Resume_Docx()
		{
			var parser = GetParser<HRJobApplication>();
			var emptyDocxPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.docx", "empty.docx");
			var result = parser.PopulateFromFile(emptyDocxPath);
			AssertApplicant(parser.Parent);
			AssertEquals(ParseResult.None, result.ParseResult);
		}

		public void TestResultCodes()
		{
			var parser = GetParser<HRJobApplication>();
			var result = parser.PopulateFromFile(@"c:\empty.jpg");
			Assert(result.ParseResult.HasFlag(ParseResult.BadFormat));

			var emptyZZZPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.zzz", "empty.zzz");
			result = parser.PopulateFromFile(emptyZZZPath);
			Assert(result.ParseResult.HasFlag(ParseResult.InvalidFileFormat));

			parser = GetParser<HRJobApplication>(false);
			result = parser.PopulateFromFile(EmptyMsgPath);
			Assert(result.ParseResult.HasFlag(ParseResult.InvalidFileFormat));
		}

		public void TestAssignJobOpening()
		{
			var parser = GetParser<HRJobApplication>();

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Joker";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Joker";
			opening.HV_CampaignStartDate = ZDateTime.Today.AddDays(-5);
			opening.HV_CampaignEndDate = ZDateTime.Today.AddDays(5);
			opening.HV_HJ_JobRole = jobRole.PK;

			var result = parser.PopulateFromFile(BatmanMsgPath, opening);
			AssertApplicant(parser.Parent);
			Assert(result.ParseResult.HasFlag(ParseResult.None));

			AssertEquals(opening.PK, parser.Parent.HP_HV);
		}

		public void TestMatchJobOpening()
		{
			var parser = GetParser<HRJobApplication>();

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Batman";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			opening.HV_CampaignEndDate = new ZDateTime(2019, 2, 20);
			opening.HV_HJ_JobRole = jobRole.PK;

			var result = parser.PopulateFromFile(BatmanMsgPath);
			AssertApplicant(parser.Parent);
			Assert(result.ParseResult.HasFlag(ParseResult.None));

			AssertEquals(ZGuid.Empty, parser.Parent.HP_HV);
		}

		public void TestMatchJobOpening_Disabled()
		{
			var parser = GetParser<HRJobApplication>(allowMatchJobOpening: false);

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Batman";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			opening.HV_CampaignEndDate = new ZDateTime(2019, 3, 1);
			opening.HV_HJ_JobRole = jobRole.PK;

			var result = parser.PopulateFromFile(BatmanMsgPath);
			AssertApplicant(parser.Parent);
			Assert(result.ParseResult.HasFlag(ParseResult.None));

			AssertEquals(ZGuid.Empty, parser.Parent.HP_HV);
		}

		public void TestMatchJobOpening_GracePeriod()
		{
			var parser = GetParser<HRJobApplication>();

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening = Factory.New<HRRecruitmentJobCampaign>();
			opening.HV_AdTitle = "Batman";
			opening.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			opening.HV_CampaignEndDate = new ZDateTime(2019, 3, 1);
			opening.HV_HJ_JobRole = jobRole.PK;

			var result = parser.PopulateFromFile(BatmanMsgPath);
			AssertApplicant(parser.Parent);
			Assert(result.ParseResult.HasFlag(ParseResult.None));

			AssertEquals(opening.PK, parser.Parent.HP_HV);
		}

		public void TestMatchApplicant()
		{
			const string mobileNoSpace = "0412345678";
			const string mobileWithSpace1 = "0412 345 678";
			const string mobileWithSpace2 = "04 12 34 56 78";

			var resume = new ApplicantResume { Mobile = mobileWithSpace1 };
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_MobilePhone = mobileNoSpace;

			var personApplicant = Factory.NewWithValidTestData<HRJobApplicant>();
			personApplicant.SetFromPerson(person);
			Factory.Save();

			person.ApplicantCollection.Load();
			AssertNotEquals(0, person.ApplicantCollection.Count);

			var applicant = MatchApplicant(Factory, resume);
			AssertNotNull(applicant);
			AssertEquals(mobileNoSpace, applicant.Mobile);

			resume.Mobile = mobileNoSpace;
			person.PER_MobilePhone = mobileWithSpace1;
			Factory.Save();
			person.ApplicantCollection.Load();
			applicant = MatchApplicant(Factory, resume);
			AssertNotNull(applicant);
			AssertEquals(mobileWithSpace1, applicant.Mobile);

			resume.Mobile = mobileWithSpace1;
			person.PER_MobilePhone = mobileWithSpace2;
			Factory.Save();
			person.ApplicantCollection.Load();
			applicant = MatchApplicant(Factory, resume);
			AssertNotNull(applicant);
			AssertEquals(mobileWithSpace2, applicant.Mobile);
		}

		public void TestPopulateFromFile_SetupReferringSource()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			Factory.Save();

			var configs = new ReferringPartyConfigurationCollection();
			var config1 = configs.AddNew();
			config1.Domain = "@wisetechglobal.com";
			config1.ReferringParty = "OH";
			config1.DefaultReferringSource = "AGT";
			config1.OrganizationPK = org.PK;
			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);

			var parser = GetParser<HRJobApplication>();
			parser.PopulateFromFile(EmptyMsgPath);
			AssertEquals("AGT", parser.Parent.HP_SourceType);
			AssertEquals(org.PK, parser.Parent.HP_OH_ReferringOrganisation);
		}

		public void TestTryExtractMissingData_NonHtmlBody_DoesNotHaveError()
		{
			void TestCase(ZString fileName, string domainName, OrgHeader org)
			{
				var configs = new ReferringPartyConfigurationCollection();
				var config1 = configs.AddNew();
				config1.Domain = domainName;
				config1.ReferringParty = "OH";
				config1.DefaultReferringSource = "AGT";
				config1.OrganizationPK = org.PK;
				RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);

				var parser = GetParser<HRJobApplication>(emptyEmail: true, emptyPhone: true);
				var result = parser.PopulateFromFile(fileName);
				Assert(result.Error.IsNullOrEmpty());
			}

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			Factory.Save();

			TestCase(PlaintextMsgPath, "@wisetechglobal.com", org);
			TestCase(PlaintextEmlPath, "@gmail.com", org);
		}

		public void TestPopulateFromMailItem_SetupReferringSource()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ORG_TEST1";
			Factory.Save();

			var configs = new ReferringPartyConfigurationCollection();
			var config1 = configs.AddNew();
			config1.Domain = "@wisetechglobal.com";
			config1.ReferringParty = "OH";
			config1.DefaultReferringSource = "AGT";
			config1.OrganizationPK = org.PK;
			RecruiterDataRegistry.Instance.ReferringPartiesConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, configs);

			using (var emptyMsgStream = resourceRetriever.Value.GetStream("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.msg"))
			using (var message = new MsgReader.Outlook.Storage.Message(emptyMsgStream, FileAccess.ReadWrite))
			{
				var parser = GetParser<HRJobApplication>();
				parser.PopulateFromMailItem(message);
				AssertEquals("AGT", parser.Parent.HP_SourceType);
				AssertEquals(org.PK, parser.Parent.HP_OH_ReferringOrganisation);
			}
		}

		public void TestOverlappingGracePeriodJobOpenings()
		{
			var parser = GetParser<HRJobApplication>();

			var jobRole = Factory.New<HRJobRole>();
			jobRole.HJ_JobTitle = "Batman";
			jobRole.HJ_JobRoleDescription = "some description";

			var opening1 = Factory.New<HRRecruitmentJobCampaign>();
			opening1.HV_AdTitle = "Batman";
			opening1.HV_CampaignStartDate = new ZDateTime(2019, 2, 1); //email date is 13.03.2019
			opening1.HV_CampaignEndDate = new ZDateTime(2019, 3, 10);
			opening1.HV_HJ_JobRole = jobRole.PK;

			var opening2 = Factory.New<HRRecruitmentJobCampaign>();
			opening2.HV_AdTitle = "Batman";
			opening2.HV_CampaignStartDate = new ZDateTime(2019, 3, 11); //email date is 13.03.2019
			opening2.HV_CampaignEndDate = new ZDateTime(2019, 4, 1);
			opening2.HV_HJ_JobRole = jobRole.PK;

			var result = parser.PopulateFromFile(BatmanMsgPath);
			AssertApplicant(parser.Parent);
			Assert(!result.ParseResult.HasFlag(ParseResult.MultipleJobOpeningsFound));

			AssertEquals(opening2.PK, parser.Parent.HP_HV);
		}

		public void TestPopulateFromMailItem_ShouldNotShowErrorMessageIfParsingSkipped()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
			{
				var application = Factory.New<HRJobApplication>();
				var parser = new HRJobApplicationEmailParser(application, new DaxtraResumeParser());
				var result = parser.PopulateFromFile(BadEmailPath);
				Assert(result.ParseResult.HasFlag(ParseResult.NoResumeParsed));
				AssertEquals("HRJobApplicationEmailParser.PopulateFromFile.FormatException", ErrorReporter.LastKeyReported);
				AssertContains("Exception parsing EML file: bad.eml", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = false;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);
			{
				var application = Factory.New<HRJobApplication>();
				var parser = new HRJobApplicationEmailParser(application, new DaxtraResumeParser());
				var result = parser.PopulateFromFile(BadEmailPath);
				Assert(!result.ParseResult.HasFlag(ParseResult.NoResumeParsed));
				AssertEquals("HRJobApplicationEmailParser.PopulateFromFile.FormatException", ErrorReporter.LastKeyReported);
				AssertContains("Exception parsing EML file: bad.eml", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		void AssertApplicant(HRJobApplication application)
		{
			var applicant = application.Applicant;
			AssertNotNull(applicant);

			AssertEquals(applicant.HA_FullName, "name");
			AssertEquals(applicant.HA_Gender, "M");
			AssertEquals(applicant.HA_RN_NKNationalityCodeISO, "AU");
			AssertEquals(applicant.HA_MobilePhone, "0499702888");
			AssertEquals(applicant.HA_UserAddress1, "address1");
			AssertEquals(applicant.HA_UserAddress2, "address2");
			AssertEquals(applicant.HA_City, "Miranda");
			AssertEquals(applicant.HA_Postcode, "2228");
			AssertEquals(applicant.HA_State, "NSW");
			AssertEquals(applicant.HA_RN_NKCountry, "AU");
			AssertEquals(applicant.HA_EmailAddress, "email@gmail.com");
			AssertEquals(applicant.HA_HomePhone, "0212345678");
		}

		public void TestProcessEmailWithoutAttachmentsWithoutEmailBody()
		{
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "FMT";
			docType.RT_Desc = "test type";

			using (RecruiterDataRegistry.Instance.DocTypeReferringSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docType.PK.ToGuid()))
			{
				var parser = GetParser<HRJobApplication>();
				var result = parser.PopulateFromFile(LinkedinNoAttachmentMsgPath);
				AssertNull(parser.Parent.Applicant);
			}
		}

		public void TestProcessEmailWithoutAttachmentsWithEmailBody()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "FMT";
			docType.RT_Desc = "test type";

			using (RecruiterDataRegistry.Instance.DocTypeReferringSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docType.PK.ToGuid()))
			{
				var parser = GetParser<HRJobApplication>();
				var result = parser.PopulateFromFile(LinkedinNoAttachmentMsgPath);
				AssertApplicant(parser.Parent);
				AssertEquals(ParseResult.None | ParseResult.IsEmail, result.ParseResult);
				var document = Factory.LoadTop1<HRJobApplicationDocument>(new ZQuery(HRJobApplicationDocumentSchema.HPD_HP, parser.Parent.PK));
				AssertEquals("Document should be saved as Referring Source", document.HPD_Type, ApplicationDocumentTypes.Codes.ReferringSource);
				AssertEquals("Should add email just once", 1, parser.Parent.DocManagerInfo.AllEDocs.Count);
				AssertEquals("DocTypeReferringSource", "FMT", parser.Parent.DocManagerInfo.AllEDocs[0].DocType);
			}
		}

		public void TestProcessEmailMalformedAttachment()
		{
			var rules = new EmailParsingRuleCollection();
			var rule = rules.AddNew();
			rule.ReferringPartyCode = "UNK";
			rule.AllowParseAttachments = rule.AllowFallbackToEmailBody = true;
			RecruiterDataRegistry.Instance.DaxtraAutoParsingRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rules);

			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "DOC";
			docType.RT_Desc = "test type";

			using (RecruiterDataRegistry.Instance.DocTypeReferringSource.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, docType.PK.ToGuid()))
			{
				var parser = GetParser<HRJobApplication>(true, true);
				var badattachmentMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.badattachment.msg", "badattachment.msg");

				var result = parser.PopulateFromFile(badattachmentMsgPath);
				AssertApplicant(parser.Parent);
				AssertNotNull(parser.LastData);
				AssertEquals(2, parser.LastData.Count);
				AssertEquals(true, parser.LastData[1].IsEmailWithoutAttachment);

				AssertEquals(ParseResult.None | ParseResult.IsEmail, result.ParseResult);
				var document = Factory.LoadTop1<HRJobApplicationDocument>(new ZQuery(HRJobApplicationDocumentSchema.HPD_HP, parser.Parent.PK));
				AssertEquals("Document should be saved as Referring Source", document.HPD_Type, ApplicationDocumentTypes.Codes.ReferringSource);
				AssertEquals("Should add email just once", 1, parser.Parent.DocManagerInfo.AllEDocs.Count);
				AssertEquals("DocTypeReferringSource", "DOC", parser.Parent.DocManagerInfo.AllEDocs[0].DocType);
			}
		}

		public void TestTryParseSenderEmailAddress()
		{
			ZString senderEmailAddress;
			AssertEquals(true, TryParseSenderEmailAddress(EmptyMsgPath, out senderEmailAddress));
			AssertEquals(true, senderEmailAddress.EndsWith("@wisetechglobal.com"));

			AssertEquals(false, TryParseSenderEmailAddress(BadEmailPath, out senderEmailAddress));
			AssertEquals("", senderEmailAddress);

			AssertEquals(true, TryParseSenderEmailAddress(BatmanMsgPath, out senderEmailAddress));
			AssertEquals(true, senderEmailAddress.EndsWith("@wisetechglobal.com"));
		}

		public void TestHomeAndMobilePhone()
		{
			var resume = new ApplicantResume();
			resume.Mobile = "123456";
			resume.HomePhone = "654321";

			var applicant = Factory.New<HRJobApplicant>();

			SetApplicantValues(resume, applicant);
			AssertEquals("123456", applicant.HA_MobilePhone);
			AssertEquals("654321", applicant.HA_HomePhone);

			resume.Mobile = "123456";
			resume.HomePhone = "";
			applicant.HA_MobilePhone = "";
			applicant.HA_HomePhone = "";
			SetApplicantValues(resume, applicant);
			AssertEquals("123456", applicant.HA_MobilePhone);
			AssertEquals("", applicant.HA_HomePhone);

			resume.Mobile = "";
			resume.HomePhone = "654321";
			applicant.HA_MobilePhone = "";
			applicant.HA_HomePhone = "";
			SetApplicantValues(resume, applicant);
			AssertEquals("654321", applicant.HA_MobilePhone);
			AssertEquals("", applicant.HA_HomePhone);
		}

		public void TestAllPropertiesWithPreceedingWhitespace()
		{
			var resume = new ApplicantResume();

			resume.EmailAddress = $"                                                                                                                                      " +
								  $"                                                                                                                                  test@wisetech.com";
			resume.Name = "                                                                                                                 test name";
			resume.Gender = "                                                                                                                 M";
			resume.Address1 = "                                                                                                                 test address1";
			resume.Mobile = "                                                                                                                 test mobile";
			resume.Address2 = "                                                                                                                 test address2";
			resume.State = "                                                                                                                 test state";
			resume.Country = "                                                                                                                 IN";
			resume.Nationality = "                                                                                                                 test nationality";
			resume.Postcode = "                                                                                                                 0000";
			resume.HomePhone = "                                                                                                                 test homephone";
			resume.City = "                                                                                                                 test city";

			var applicant = Factory.New<HRJobApplicant>();

			SetApplicantValues(resume, applicant);

			CombineAssertions(() =>
			{
				AssertEquals("test@wisetech.com", applicant.HA_EmailAddress);
				AssertEquals("test name", applicant.HA_FullName);
				AssertEquals("M", applicant.HA_Gender);
				AssertEquals("test address1", applicant.HA_UserAddress1);
				AssertEquals("test mobile", applicant.HA_MobilePhone);
				AssertEquals("test address2", applicant.HA_UserAddress2);
				AssertEquals("test state", applicant.HA_State);
				AssertEquals("IN", applicant.HA_RN_NKCountry);
				AssertEquals("test homephone", applicant.HA_HomePhone);
				AssertEquals("0000", applicant.HA_Postcode);
			});
		}

		public void TestResumeWithoutEmail()
		{
			var mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Application = ApplicationDocumentsUpdater.HREmailsServiceTaskCode;
			mailItem.MI_Status = MailStatus.Unprocessed;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_Subject = "Subject test";
			mailItem.MI_From = "test@wisetech.com";
			mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			mailItem.MI_Body = "body test";

			var newAttachment = mailItem.MailAttachments.AddNew();
			newAttachment.MA_FileName = "empty.pdf";
			newAttachment.MA_Data = resourceRetriever.Value.GetBytes("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.pdf");
			Factory.Save();

			var parser = new HRJobApplicationEmailParserForTest_EmptyEmail(mailItem, new DaxtraResumeParser(), Factory);
			AssertNull("Precondition", parser.Parent);
			parser.ParseMailItem(mailItem);
			Assert("Should be able to parse the resume without email", !parser.CurrentResultForTest.HasFlag(ParseResult.NoResumeParsed));
			AssertNull("Should not create application if the resume doesn't have email", parser.Parent);
			AssertEquals(MailStatus.Unprocessed, mailItem.MI_Status);
		}

		public void TestIsLikelyCoverLetter()
		{
			AssertEquals(true, IsLikelyCoverLetter("cover letter"));
			AssertEquals(true, IsLikelyCoverLetter("cover note"));
			AssertEquals(true, IsLikelyCoverLetter("cover letter.txt"));
			AssertEquals(true, IsLikelyCoverLetter("FW: cover letter.pdf"));
			AssertEquals(true, IsLikelyCoverLetter("RE: Anton's cover note"));
			AssertEquals(true, IsLikelyCoverLetter("coverletter"));
			AssertEquals(true, IsLikelyCoverLetter("12345_donantoniocoverletter"));
		}

		public void TestCreateApplicationAndApplicantWithoutMatchingEmailMobileHomePhone()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = "test1@wisetech.com";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2.HA_FullName = string.Empty;
			applicant2.HA_EmailAddress = "test2@wisetech.com";

			var applicant3 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant3.HA_FullName = "Test 3";
			applicant3.HA_EmailAddress = string.Empty;

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);

			AssertEquals(GlbPerson.EmptyFullName, application.Applicant.HA_FullName);
			AssertEquals("", application.Applicant.HA_EmailAddress);
		}

		public void TestLoadApplicant_MatchingEmail()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = "test1@wisetech.com";

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.EmailAddress = "test1@wisetech.com";
			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 1", application.Applicant.HA_FullName);
			AssertEquals("test1@wisetech.com", application.Applicant.HA_EmailAddress);
			AssertEquals(applicant1, application.Applicant);
		}

		public void TestLoadApplicant_MatchingMobile()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = string.Empty;
			applicant1.HA_MobilePhone = "+61 444 444 4444";
			Factory.Save();

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.Mobile = "+61 444 444 4444";
			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 1", application.Applicant.HA_FullName);
			AssertEquals("", application.Applicant.HA_EmailAddress);
			AssertEquals("+61 444 444 4444", application.Applicant.Mobile);
			AssertEquals(applicant1, application.Applicant);
		}

		public void TestLoadApplicant_MatchingHomePhone()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = string.Empty;
			applicant1.HA_HomePhone = "02 2222 2222";

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.HomePhone = "02 2222 2222";
			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 1", application.Applicant.HA_FullName);
			AssertEquals("", application.Applicant.HA_EmailAddress);
			AssertEquals("02 2222 2222", application.Applicant.HA_HomePhone);
			AssertEquals(applicant1, application.Applicant);
		}

		public void TestLoadApplicant_MatchingEmailMobile()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = "test1@wisetech.com";
			applicant1.HA_MobilePhone = "+61 444 444 4444";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2.HA_FullName = "Test 2";
			applicant2.HA_EmailAddress = "test2@wisetech.com";
			applicant2.HA_MobilePhone = "+61 555 555 5555";

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.EmailAddress = "test1@wisetech.com";
			resume.Mobile = "+61 555 555 5555";

			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 1", application.Applicant.HA_FullName);
			AssertEquals("test1@wisetech.com", application.Applicant.HA_EmailAddress);
			AssertEquals(applicant1, application.Applicant);
		}

		public void TestLoadApplicant_MatchingEmailHomePhone()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = "test1@wisetech.com";
			applicant1.HA_HomePhone = "02 2222 2222";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2.HA_FullName = "Test 2";
			applicant2.HA_EmailAddress = "test2@wisetech.com";
			applicant2.HA_HomePhone = "02 3333 3333";

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.EmailAddress = "test1@wisetech.com";
			resume.HomePhone = "03 3333 3333";

			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 1", application.Applicant.HA_FullName);
			AssertEquals("test1@wisetech.com", application.Applicant.HA_EmailAddress);
			AssertEquals(applicant1, application.Applicant);
		}

		public void TestLoadApplicant_MatchingMobileHomePhone()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = string.Empty;
			applicant1.HA_MobilePhone = "+61 444 444 4444";
			applicant1.HA_HomePhone = "02 2222 2222";

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant2.HA_FullName = "Test 2";
			applicant2.HA_EmailAddress = "test2@wisetech.com";
			applicant2.HA_MobilePhone = "+61 555 555 5555";
			applicant2.HA_HomePhone = "02 3333 3333";
			Factory.Save();

			var application = Factory.New<HRJobApplication>();
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			AssertNull(application.Applicant);

			var results = new List<ParseDataToBeProcessed>();
			var resume = new ApplicantResume();
			resume.EmailAddress = string.Empty;
			resume.Mobile = "+61 555 555 5555";
			resume.HomePhone = "02 2222 2222";

			results.Add(new ParseDataToBeProcessed(true, "test1.doc", null, false, resume));
			parser.ProcessParsedData_Exposed(results);
			AssertNotNull(application.Applicant);
			AssertEquals("Test 2", application.Applicant.HA_FullName);
			AssertEquals("test2@wisetech.com", application.Applicant.HA_EmailAddress);
			AssertEquals(applicant2, application.Applicant);
		}

		public void TestSetApplicantValues()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Test Name";
			applicant.HA_EmailAddress = "test1@wisetech.com";

			var resume = new ApplicantResume();
			resume.Name = "wrong name";
			resume.EmailAddress = "wrongemail@wisetech.com";
			resume.Mobile = "+61 456 411 411";
			resume.Country = "AU";

			AssertEquals("Test Name", applicant.HA_FullName);
			AssertEquals("test1@wisetech.com", applicant.HA_EmailAddress);
			Assert(string.IsNullOrEmpty(applicant.HA_MobilePhone));
			AssertEquals(null, applicant.Country);

			SetApplicantValues(resume, applicant);

			AssertEquals("Test Name", applicant.HA_FullName);
			AssertEquals("test1@wisetech.com", applicant.HA_EmailAddress);
			AssertEquals("+61 456 411 411", applicant.HA_MobilePhone);
			AssertEquals("AU", applicant.Country.Code);
		}

		public void TestProcessQueueFile()
		{
			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant1.HA_FullName = "Test 1";
			applicant1.HA_EmailAddress = "test1@wisetech.com";
			applicant1.HA_MobilePhone = string.Empty;

			var application = Factory.New<HRJobApplication>();
			application.HP_HA = applicant1.PK;
			var parser = new HRJobApplicationEmailParserForTest(application, new DaxtraResumeParser(), createApplicantWithEmptyEmail: true);
			parser.EnableCustomParseCore = true;

			AssertEquals(0, application.Documents.Count);
			AssertEquals("", application.Applicant.HA_MobilePhone);

			parser.ProcessQueueFile("Some Resume.docx", Array.Empty<byte>(), DateTime.Now);

			AssertEquals(1, application.Documents.Count);
			AssertEquals("Should not change fields with values, just fill the empty ones", "Test 1", application.Applicant.HA_FullName);
			AssertEquals("Should not change fields with values, just fill the empty ones", "test1@wisetech.com", application.Applicant.HA_EmailAddress);
			AssertEquals("+61 413444555", application.Applicant.HA_MobilePhone);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		string cantReadEmailAddressPath;
		string CantReadEmailAddressPath
		{
			get
			{
				if (string.IsNullOrEmpty(cantReadEmailAddressPath))
				{
					cantReadEmailAddressPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.cant-read-email-address.eml", "cant-read-email-address.eml");
				}
				return cantReadEmailAddressPath;
			}
		}

		string emptyMsgPath;
		string EmptyMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(emptyMsgPath))
				{
					emptyMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.empty.msg", "empty.msg");
				}
				return emptyMsgPath;
			}
		}

		string badEmailPath;
		string BadEmailPath
		{
			get
			{
				if (string.IsNullOrEmpty(badEmailPath))
				{
					badEmailPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.bad.eml", "bad.eml");
				}
				return badEmailPath;
			}
		}

		string batmanMsgPath;
		string BatmanMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(batmanMsgPath))
				{
					batmanMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.batman.msg", "batman.msg");
				}
				return batmanMsgPath;
			}
		}

		string linkedinNoAttachmentMsgPath;
		string LinkedinNoAttachmentMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(linkedinNoAttachmentMsgPath))
				{
					linkedinNoAttachmentMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.linkedinNoAttachment.msg", "linkedinNoAttachment.msg");
				}
				return linkedinNoAttachmentMsgPath;
			}
		}

		string plaintextMsgPath;
		string PlaintextMsgPath
		{
			get
			{
				if (string.IsNullOrEmpty(plaintextMsgPath))
				{
					plaintextMsgPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.plaintext_rubbish.msg", "plaintext_rubbish.msg");
				}
				return plaintextMsgPath;
			}
		}

		string plaintextEmlPath;
		string PlaintextEmlPath
		{
			get
			{
				if (string.IsNullOrEmpty(plaintextEmlPath))
				{
					plaintextEmlPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Recruiter.Business.Testing.Application.TestFiles.plaintext_eml.eml", "plaintext_eml.eml");
				}
				return plaintextEmlPath;
			}
		}

		HRJobApplicationEmailParserForTest GetParser<T>(bool noExceptions = true, bool failOnce = false, bool failedConversion = false, bool emptyEmail = false, bool emptyPhone = false, bool allowMatchJobOpening = true) where T : HRJobApplication
		{
			var application = Factory.New<T>();
			var resumeParser = new ApplicantResumeParserForTest(string.Empty, application.PK, noExceptions, failOnce, failedConversion, emptyEmail, emptyPhone);
			var parser = new HRJobApplicationEmailParserForTest(application, resumeParser, allowMatchJobOpening);
			return parser;
		}

		class HRJobApplicationEmailParserForTest : HRJobApplicationEmailParser
		{
			readonly bool blowup;

			public HRJobApplicationEmailParserForTest(HRJobApplication jobApplication, IApplicantResumeParser resumeParser, bool allowMatchJobOpening = true) : base(jobApplication, resumeParser, allowMatchJobOpening: allowMatchJobOpening)
			{
			}

			public HRJobApplicationEmailParserForTest(HRJobApplication jobApplication, IApplicantResumeParser resumeParser, bool createApplicantWithEmptyEmail, bool allowMatchJobOpening = true) : base(jobApplication, resumeParser, createApplicantWithEmptyEmail, allowMatchJobOpening: allowMatchJobOpening)
			{
			}

			public HRJobApplicationEmailParserForTest(MailItem mailItem, IApplicantResumeParser resumeParser, BusinessObjectFactory factory, bool blowup = false) : base(mailItem, resumeParser, factory)
			{
				this.blowup = blowup;
			}

			public List<ParseDataToBeProcessed> LastData { get; private set; }

			public bool EnableCustomParseCore { get; set; }

			protected override void ProcessParsedData(List<ParseDataToBeProcessed> results)
			{
				LastData = results;
				base.ProcessParsedData(results);
			}

			protected override List<ParseDataToBeProcessed> ParseMailItemCore(EmailData emailData, HRRecruitmentJobCampaign jobOpening, string filename = null)
			{
				if (blowup)
				{
					throw new Exception("boom");
				}
				else
				{
					return base.ParseMailItemCore(emailData, jobOpening, filename);
				}
			}

			public void ProcessParsedData_Exposed(List<ParseDataToBeProcessed> results)
			{
				base.ProcessParsedData(results);
			}

			protected override IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
			{
				if (EnableCustomParseCore)
				{
					var applicant = new ApplicantResume();
					applicant.Name = "Test FullName";
					applicant.EmailAddress = "test@wisetech.com";
					applicant.Mobile = "+61 413444555";
					var resume = new ApplicantResumeParseResult() { ParsedResume = applicant, Status = ApplicantResumeParseStatus.Success };

					if (resume.Status == ApplicantResumeParseStatus.Success)
					{
						var parsedResume = resume.ParsedResume;
						CreateDocument(documentType, parsedResume.ResumeXml);
					}

					return resume;
				}
				else
				{
					return base.ParseCore(filename, data, documentType);
				}
			}
		}

		sealed class HRJobApplicationEmailParserForTest_EmptyEmail : HRJobApplicationEmailParserForTest
		{
			public ParseResult CurrentResultForTest => CurrentResult;

			public HRJobApplicationEmailParserForTest_EmptyEmail(MailItem mailItem, IApplicantResumeParser resumeParser, BusinessObjectFactory factory)
				: base(mailItem, resumeParser, factory)
			{
			}

			protected override IApplicantResumeParseResult ParseCore(string filename, byte[] data, string documentType)
			{
				var applicant = new ApplicantResume();
				applicant.Name = "Test FullName";
				applicant.EmailAddress = string.Empty; // need to be empty for TestResumeWithoutEmail
				return new ApplicantResumeParseResult() { ParsedResume = applicant, Status = ApplicantResumeParseStatus.Success };
			}
		}
	}
}
