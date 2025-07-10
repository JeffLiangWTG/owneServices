using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Recruiter.Business.RecruiterDataRegistry;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(RecruiterDataRegistry))]
	sealed class RecruiterDataRegistryTest : RegistryItemSetTestCase<RecruiterDataRegistry>
	{
		public void TestExamMigrationStatus()
		{
			var item = ItemSet.ExamMigrationStatus;
			AssertEquals("ExamMigrationStatus", item.Name);
			AssertEquals("Exam Migration Status", item.Caption);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.ExamMigrationStatus.Options);
			AssertEquals(RegistryStorageFlags.System, item.Storage);
			AssertEquals(195, ItemSet.ExamMigrationStatus.DefaultValue.Count);
		}

		public void TestCertificateCodeSpecialtyMapping()
		{
			AssertEquals("Mappings between certificate codes and related specialization codes", ItemSet.CertificateCodeSpecialialisationMapping.Hint);
		}

		public void TestRefresherRenewPeriod()
		{
			AssertEquals("This registry allows you to set the number of months prior to the expiry of the current Refresher attempt in which a new subsequent refresher attempt will be created", ItemSet.RefresherAttemptGracePeriodMonths.Hint);
			AssertEquals(4, ItemSet.RefresherAttemptGracePeriodMonths.DefaultValue);
		}

		public void TestDocTypeCVCodeCaches()
		{
			AssertEquals("The default value is Resume", "RES", ItemSet.DocTypeCVCode);

			var factory = new BusinessObjectFactory();
			var doc = factory.NewWithValidTestData<RefDocType>();
			doc.RT_DocType = "RE2";
			factory.Save();

			ItemSet.DocTypeCV.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, doc.PK.ToGuid());
			AssertEquals("Should update to the new value", "RE2", Instance.DocTypeCVCode);
		}

		public void TestDaxtraResumeAcceptedFormats()
		{
			AssertEquals("Accepted file formats for the resumes parsed by Daxtra service.", ItemSet.DaxtraResumeAcceptedFormats.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraServiceUrl.Options);
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("PDF"));
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("DOC"));
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("DOCX"));
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("TXT"));
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("HTM"));
			Assert(ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.ContainsCode("HTML"));
			AssertEquals(6, ItemSet.DaxtraResumeAcceptedFormats.DefaultValue.Count);
		}

		public void TestDaxtraServiceUrl()
		{
			AssertEquals("A Daxtra service URL for resume parsing", ItemSet.DaxtraServiceUrl.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraServiceUrl.Options);
			AssertEquals("", ItemSet.DaxtraServiceUrl.DefaultValue);
		}

		public void TestDaxtraServiceAccountName()
		{
			AssertEquals("A Daxtra service Account Name for resume parsing", ItemSet.DaxtraAccountName.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraAccountName.Options);
			AssertEquals("", ItemSet.DaxtraAccountName.DefaultValue);
		}

		public void TestDaxtraSubjectRegex()
		{
			AssertEquals("A regular expression for extracting Job Opening from the email subject. Please use adTitle variable for the suggested Job Opening location.", ItemSet.DaxtraSubjectRegex.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraSubjectRegex.Options);
			AssertEquals("((Application for )(?<adTitle0>.*)( from .*))|((respond now:.*applied to )(?<adTitle1>.*)( on glassdoor))|((application received for )(?<adTitle2>.*))", ItemSet.DaxtraSubjectRegex.DefaultValue);
		}

		public void TestDaxtraTimeout()
		{
			AssertEquals("A timeout for the Daxtra web service call in seconds. Zero for the unlimited wait.", ItemSet.DaxtraTimeout.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraTimeout.Options);
			AssertEquals(30, ItemSet.DaxtraTimeout.DefaultValue);
		}

		public void TestDaxtraBatchSize()
		{
			AssertEquals("A number of files to send to Daxtra web service in one batch.", ItemSet.DaxtraBatchSize.Hint);
			AssertEquals(RegistryOptions.IsHidden, ItemSet.DaxtraBatchSize.Options);
			AssertEquals(100, ItemSet.DaxtraBatchSize.DefaultValue);
		}

		public void TestLoggingKafkaBrokers()
		{
			AssertEquals("Specifies the Kafka brokers for NLog Kafka target when logging parsing requests.", ItemSet.LoggingKafkaBrokers.Hint);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.LoggingKafkaBrokers.Options);
			AssertEquals("106-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,107-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,108-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,109-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044,110-cluster-kafka-q0-au2-brokers.kafka.wtg.ws:8044", ItemSet.LoggingKafkaBrokers.DefaultValue);
		}

		public void TestLoggingKafkaTopic_ProductionSystem() => LoggingKafkaTopic(true);

		public void TestLoggingKafkaTopic_NonProductionSystem() => LoggingKafkaTopic(false);

		public void LoggingKafkaTopic(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);

			AssertEquals("Specifies the Kafka topic for NLog Kafka target when logging parsing requests.", ItemSet.LoggingKafkaTopic.Hint);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.LoggingKafkaTopic.Options);

			var expectedTopic = isProductionSystem
				? "topic-au2-prod-hrm-logs-prod"
				: "topic-au1-test-hrm-logs-test";
			AssertEquals(expectedTopic, ItemSet.LoggingKafkaTopic.DefaultValue);
		}

		public void TestLoggingKafkaTopicUsername_ProductionSystem() => LoggingKafkaTopicUsername(true);

		public void TestLoggingKafkaTopicUsername_NonProductionSystem() => LoggingKafkaTopicUsername(false);

		public void LoggingKafkaTopicUsername(bool isProductionSystem)
		{
			var licenceType = isProductionSystem ? DatabaseTypes.Codes.Production : DatabaseTypes.Codes.Test;
			LicenceTypeChanger.SetSystemLicence(licenceType);

			AssertEquals("Specifies the Kafka topic username for logging parsing requests.", ItemSet.LoggingKafkaTopicUsername.Hint);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.LoggingKafkaTopicUsername.Options);

			var expectedTopic = isProductionSystem
				? "topic-au2-prod-hrm-logs-prod"
				: "topic-au1-test-hrm-logs-test";
			AssertEquals(expectedTopic, ItemSet.LoggingKafkaTopicUsername.DefaultValue);
		}

		public void TestLoggingKafkaTopicPassword()
		{
			AssertEquals("Specifies the Kafka topic password for logging parsing requests.", ItemSet.LoggingKafkaTopicPassword.Hint);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.LoggingKafkaTopicPassword.Options);
			AssertEquals(string.Empty, ItemSet.LoggingKafkaTopicPassword.DefaultValue);
		}

		public void TestLoggingMinLevel()
		{
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.LoggingMinLevel.Options);
			AssertEquals(NLog.LogLevel.Info.Name, ItemSet.LoggingMinLevel.DefaultValue);
			ItemSet.LoggingMinLevel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NLog.LogLevel.Debug.Name);
			AssertEquals(NLog.LogLevel.Debug.Name, ItemSet.LoggingMinLevel.Value);
		}

		public void TestApplicantEmailOptions()
		{
			string expectedBodyDefaultValue =
				"Thank you for your recent response to our advertised position. We have received a considerable number of applicants. At this stage we are in receipt of applications from candidates that more closely match our selection criteria, so we will not be proceeding with your application at this time." + System.Environment.NewLine + System.Environment.NewLine +
				"We will keep your application on file and should a suitable position become available, we will be in further contact." + System.Environment.NewLine + System.Environment.NewLine +
				"We thank you for your interest and wish you well in your job search." + System.Environment.NewLine + System.Environment.NewLine +
				"Yours faithfully,";

			AssertEquals("ApplicantEmailSubject.DefaultValue", "Thank you for coming", ItemSet.ApplicantEmailSubject.DefaultValue);
			AssertEquals("ApplicantEmailBody.DefaultValue", expectedBodyDefaultValue, ItemSet.ApplicantEmailBody.DefaultValue);

			ItemSet.ApplicantEmailSubject.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Subject");
			ItemSet.ApplicantEmailBody.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Body");

			AssertEquals("ApplicantEmailSubject.Value", "Subject", ItemSet.ApplicantEmailSubject.Value);
			AssertEquals("ApplicantEmailBody.Value", "Body", ItemSet.ApplicantEmailBody.Value);
		}

		public void TestRecruiterTestTypes()
		{
			RecruiterTestTypeCollection defaultValue = ItemSet.RecruiterTestTypes.DefaultValue;
			AssertEquals("defaultValue.Count", 1, defaultValue.Count);
			AssertEquals("defaultValue.Code", "STD", defaultValue[0].Code);
			AssertEquals("defaultValue.Description", "You can set these test types in the System Registry, under Recruiter/Recruiter Test Types", defaultValue[0].Description);
			AssertEquals("defaultValue.Category", string.Empty, defaultValue[0].Category);
			AssertEquals("defaultValue.NotificationType", RecruiterTestNotificationType.Codes.DoNotDeliver, defaultValue[0].NotificationType);
			AssertEquals("defaultValue.DocumentName", string.Empty, defaultValue[0].DocumentName);
			AssertEquals("defaultValue.TestNote", string.Empty, defaultValue[0].TestNote);

			RecruiterTestTypeCollection collection = new RecruiterTestTypeCollection();
			collection.Add("ABC", "ABC description", RecruiterTestCategory.Codes.Accreditation);
			collection.Add("fgb", "fgb description", RecruiterTestCategory.Codes.SkillTest, "test note for fgb");

			ItemSet.RecruiterTestTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			AssertEquals("Value.Count", 2, ItemSet.RecruiterTestTypes.Value.Count);

			AssertEquals("Value[0].Code", "ABC", ItemSet.RecruiterTestTypes.Value[0].Code);
			AssertEquals("Value[0].Description", "ABC description", ItemSet.RecruiterTestTypes.Value[0].Description);
			AssertEquals("Value[0].Category", RecruiterTestCategory.Codes.Accreditation, ItemSet.RecruiterTestTypes.Value[0].Category);
			AssertEquals("Value[0].NotificationType", RecruiterTestNotificationType.Codes.DoNotDeliver, ItemSet.RecruiterTestTypes.Value[0].NotificationType);
			AssertEquals("Value[0].DocumentName", string.Empty, ItemSet.RecruiterTestTypes.Value[0].DocumentName);
			AssertEquals("Value[0].TestNote", string.Empty, ItemSet.RecruiterTestTypes.Value[0].TestNote);

			AssertEquals("Value[1].Code", "fgb", ItemSet.RecruiterTestTypes.Value[1].Code);
			AssertEquals("Value[1].Description", "fgb description", ItemSet.RecruiterTestTypes.Value[1].Description);
			AssertEquals("Value[1].Category", RecruiterTestCategory.Codes.SkillTest, ItemSet.RecruiterTestTypes.Value[1].Category);
			AssertEquals("Value[0].NotificationType", RecruiterTestNotificationType.Codes.DoNotDeliver, ItemSet.RecruiterTestTypes.Value[1].NotificationType);
			AssertEquals("Value[0].DocumentName", string.Empty, ItemSet.RecruiterTestTypes.Value[1].DocumentName);
			AssertEquals("Value[1].TestNote", "test note for fgb", ItemSet.RecruiterTestTypes.Value[1].TestNote);
		}

		public void TestApplicationStatuses()
		{
			ApplicationStatusCollection defaultValue = ItemSet.ApplicationStatuses.DefaultValue;
			CodeDescriptionPairList list = defaultValue.AsCodeDescriptionPairList();
			AssertEquals("DefaultValue.Count", 8, list.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"INP\")", "In Progress", list.GetDescriptionFromCode("INP"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ST1\")", "Assessment Stage 1", list.GetDescriptionFromCode("ST1"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ST2\")", "Assessment Stage 2", list.GetDescriptionFromCode("ST2"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ST3\")", "Assessment Stage 3", list.GetDescriptionFromCode("ST3"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ST4\")", "Assessment Stage 4", list.GetDescriptionFromCode("ST4"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"DEF\")", "Deferred pending information", list.GetDescriptionFromCode("DEF"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"REJ\")", "Rejected", list.GetDescriptionFromCode("REJ"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"ACC\")", "Accepted", list.GetDescriptionFromCode("ACC"));

			ApplicationStatusCollection newCollection = new ApplicationStatusCollection();
			newCollection.AddPair("UNK", "Unknown");
			ItemSet.ApplicationStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newCollection);
			AssertEquals("Value.Count", 1, ItemSet.ApplicationStatuses.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"UNK\")", "Unknown", ItemSet.ApplicationStatuses.Value.AsCodeDescriptionPairList().GetDescriptionFromCode("UNK"));
		}

		public void TestAdPlacementPublicationsList()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.AdPlacementPublicationsList.DefaultValue;

			AssertEquals("DefaultValue.Count", 1, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"STD\")", "You can set these publication types in the System Registry, under Recruiter/Ad Placement Publications List", defaultValue.GetDescriptionFromCode("STD"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("NWS", "Newspaper");

			ItemSet.AdPlacementPublicationsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.AdPlacementPublicationsList.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"NWS\")", "Newspaper", ItemSet.AdPlacementPublicationsList.Value.GetDescriptionFromCode("NWS"));
		}

		public void TestInterviewStatuses()
		{
			ReadOnlyCodeDescriptionPairList defaultValue = ItemSet.InterviewStatuses.DefaultValue;

			AssertEquals("DefaultValue.Count", 3, defaultValue.Count);
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"NQR\")", "Not Required", defaultValue.GetDescriptionFromCode("NQR"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"BKD\")", "Booked", defaultValue.GetDescriptionFromCode("BKD"));
			AssertEquals("DefaultValue.GetDescriptionFromCode(\"CMP\")", "Completed", defaultValue.GetDescriptionFromCode("CMP"));

			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("FAI", "Failed");

			ItemSet.InterviewStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			AssertEquals("Value.Count", 1, ItemSet.InterviewStatuses.Value.Count);
			AssertEquals("Value.GetDescriptionFromCode(\"FAI\")", "Failed", ItemSet.InterviewStatuses.Value.GetDescriptionFromCode("FAI"));
		}

		public void TestAllowEditingLearningCentreTestResults()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AllowEditingLearningCentreTestResults.Storage);
			AssertEquals("Allow Editing of Learning Center Result", ItemSet.AllowEditingLearningCentreTestResults.Caption);
			AssertEquals("This will allow users to edit Learning Center results from the Job Applicant module.", ItemSet.AllowEditingLearningCentreTestResults.Hint);

			AssertEquals("Default value should be false", false, ItemSet.AllowEditingLearningCentreTestResults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.AllowEditingLearningCentreTestResults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ItemSet.AllowEditingLearningCentreTestResults.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLearningCentreUserRegistrationNotificationGroup()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LearningCentreUserRegistrationNotificationGroup.Storage);
			AssertEquals("Learning Center User Registration Notification Group", ItemSet.LearningCentreUserRegistrationNotificationGroup.Caption);
			AssertEquals("Group to be notified when a new user registration is made via Learning Center site.\r\nWhen anonymous registration is disabled, this group is responsible in approving new registrations.", ItemSet.LearningCentreUserRegistrationNotificationGroup.Hint);
			AssertEquals("Should not be cached", RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.LearningCentreUserRegistrationNotificationGroup.Options);

			AssertEquals("Default value should be empty", Guid.Empty, ItemSet.LearningCentreUserRegistrationNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			Guid newGuid = Guid.NewGuid();
			ItemSet.LearningCentreUserRegistrationNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals(newGuid, ItemSet.LearningCentreUserRegistrationNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestAllowAnonymousLearningCentreUserRegistration()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AllowAnonymousLearningCentreUserRegistration.Storage);
			AssertEquals("Allow Anonymous Learning Center User Registration", ItemSet.AllowAnonymousLearningCentreUserRegistration.Caption);
			AssertEquals("When disabled, new user registrations will have to be approved by the notification group before web access is allowed.", ItemSet.AllowAnonymousLearningCentreUserRegistration.Hint);
			AssertEquals("Should not be cached", RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.AllowAnonymousLearningCentreUserRegistration.Options);

			AssertEquals("Default value should be true", true, ItemSet.AllowAnonymousLearningCentreUserRegistration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			try
			{
				ItemSet.AllowAnonymousLearningCentreUserRegistration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Fail("RegistryValidationException should be thrown");
			}
			catch (RegistryValidationException ex)
			{
				AssertEquals("Please specify a value for \"Learning Center User Registration Notification Group\" before disabling the anonymous user registration", ex.Message);
				AssertEquals("Should still be true", true, ItemSet.AllowAnonymousLearningCentreUserRegistration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			}

			ItemSet.LearningCentreUserRegistrationNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			ItemSet.AllowAnonymousLearningCentreUserRegistration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ItemSet.AllowAnonymousLearningCentreUserRegistration.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLearningCentreSiteName()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LearningCentreSiteName.Storage);
			AssertEquals("Learning Center Site Name", ItemSet.LearningCentreSiteName.Caption);
			AssertEquals("Specify the site name for Learning Center", ItemSet.LearningCentreSiteName.Hint);
			AssertEquals("Should not be cached", RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.LearningCentreSiteName.Options);
			AssertEquals("Default value should be 'Learning Center'", "Learning Center", ItemSet.LearningCentreSiteName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.LearningCentreSiteName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Testing Centre");
			AssertEquals("Testing Centre", ItemSet.LearningCentreSiteName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestLearningCentreUserRegistrationPage()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LearningCentreRegistrationPage.Storage);
			AssertEquals("Learning Center User Registration Page", ItemSet.LearningCentreRegistrationPage.Caption);
			AssertEquals("Specify the URL of user registration page for Learning Center. Leave it blank to use the default page.", ItemSet.LearningCentreRegistrationPage.Hint);
			AssertEquals("Should not be cached", RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.LearningCentreRegistrationPage.Options);
			AssertEquals("Default value should be empty", "", ItemSet.LearningCentreRegistrationPage.DefaultValue);

			ItemSet.LearningCentreRegistrationPage.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://syd-wscw-1/my-account/register.shtml");
			AssertEquals("http://syd-wscw-1/my-account/register.shtml", ItemSet.LearningCentreRegistrationPage.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestCompulsoryJobSkillPK()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals("Compulsory Job Skill", ItemSet.CompulsoryJobSkillPK.Caption);
			AssertEquals("Specify a job skill code that is compulsory for all job applicants", ItemSet.CompulsoryJobSkillPK.Hint);
		}

		public void TestReminderEmailTemplate()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.ReminderEmailTemplate.Storage);
			AssertEquals("Reminder Email Template", ItemSet.ReminderEmailTemplate.Caption);
			AssertEquals("Specify the email template that will be sent out to the candidates when they have not yet completed the required tests", ItemSet.ReminderEmailTemplate.Hint);
			AssertEquals(typeof(DocHRJobApplicant), ItemSet.ReminderEmailTemplate.DefaultValue.DocSourceType);
		}

		public void TestTestExpiryPeriodInDays()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TestExpiryPeriodInDays.Storage);
			AssertEquals("Test Expiry Period in Days", ItemSet.TestExpiryPeriodInDays.Caption);
			AssertEquals("Specify the number of days a recruitment test has to be completed by", ItemSet.TestExpiryPeriodInDays.Hint);
			AssertEquals(7, ItemSet.TestExpiryPeriodInDays.DefaultValue);
		}

		public void TestLearningCenterCertificateDocumentMenuName()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.LearningCenterCertificateDocumentMenuName.Storage);
			AssertEquals("Learning Center Certificate Document Menu Name", ItemSet.LearningCenterCertificateDocumentMenuName.Caption);
			AssertEquals("Certificate Document", ItemSet.LearningCenterCertificateDocumentMenuName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			ItemSet.LearningCenterCertificateDocumentMenuName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "New Certificate Document");
			AssertEquals("New Certificate Document", ItemSet.LearningCenterCertificateDocumentMenuName.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestNewJobApplicationEmailTemplate()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.NewJobApplicationEmailTemplate.Storage);
			AssertEquals("New Job Application Email Template", ItemSet.NewJobApplicationEmailTemplate.Caption);
			AssertEquals("Specify the email template that will be sent out to the candidates when they apply for a position", ItemSet.NewJobApplicationEmailTemplate.Hint);
			AssertEquals(typeof(DocHRJobApplication), ItemSet.NewJobApplicationEmailTemplate.DefaultValue.DocSourceType);
		}

		public void TestNewJobApplicationNotificationEmailTemplate()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.NewJobApplicationNotificationEmailTemplate.Storage);
			AssertEquals("New Job Application Notification Email Template", ItemSet.NewJobApplicationNotificationEmailTemplate.Caption);
			AssertEquals("Specify the email template that will be sent out to the notification group when job applicants apply for a position.", ItemSet.NewJobApplicationNotificationEmailTemplate.Hint);
			AssertEquals(typeof(DocHRJobApplication), ItemSet.NewJobApplicationNotificationEmailTemplate.DefaultValue.DocSourceType);
		}

		public void TestNewJobApplicationNotificationGroup()
		{
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.NewJobApplicationNotificationGroup.Storage);
			AssertEquals("New Job Application Notification Group", ItemSet.NewJobApplicationNotificationGroup.Caption);
			AssertEquals("Group to be notified when job applicants apply for a position.", ItemSet.NewJobApplicationNotificationGroup.Hint);
			AssertEquals("Should not be cached", RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.NewJobApplicationNotificationGroup.Options);

			AssertEquals("Default value should be empty", Guid.Empty, ItemSet.NewJobApplicationNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			Guid newGuid = Guid.NewGuid();
			ItemSet.NewJobApplicationNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newGuid);
			AssertEquals(newGuid, ItemSet.NewJobApplicationNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestOnlineApplicationDocTypes()
		{
			AssertNotNull(ItemSet.OnlineApplicationDocTypes);
		}

		public void TestDocTypeCoverLetter()
		{
			var item = ItemSet.DocTypeCoverLetter;
			AssertNotNull(item);
			AssertEquals("Category", RawDataRegistry.Categories.Recruiter, item.Category);
			AssertEquals("Caption", "Document Type Cover Letter", item.Caption);
			AssertEquals("Hint", "Document Type for Applicant Cover Letter", item.Hint);
		}

		public void TestWorkPermitStatusList()
		{
			var item = ItemSet.WorkPermitStatusList;
			AssertEquals("Name", "WorkPermitStatus", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Recruiter, item.Category);
			AssertEquals("Caption", "Work Permit Status", item.Caption);
			AssertEquals("Hint", "The list of valid entries for work permit status.", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 5, item.DefaultValue.Count);
		}

		public void TestAvailabilities()
		{
			var item = ItemSet.AvailabilityList;
			AssertEquals("Name", "AvailabilityList", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Recruiter, item.Category);
			AssertEquals("Caption", "Availabilities", item.Caption);
			AssertEquals("Hint", "The list of valid entries for availabilities", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Bool Caption", "Enabled", ((CodeDescriptionBoolRegistryEditorInfo)item.EditorInfo).BoolColumnCaption);
			AssertEquals("Default Value", 4, item.DefaultValue.Count);
		}

		public void TestExamLandingPageFooterText()
		{
			var item = ItemSet.ExamLandingPageFooterText;
			AssertEquals("Name", "ExamLandingPageFooterText", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Recruiter, item.Category);
			AssertEquals("Caption", "Exam Landing Page Footer Text", item.Caption);
			AssertEquals("Hint", "Specifies the footer text for Exam", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertEquals("Default Value", "", item.DefaultValue.ToString());
		}

		public void TestExamLandingPageFooterTextGroupedExam()
		{
			var item = ItemSet.ExamLandingPageFooterTextGroupedExam;
			AssertEquals("Name", "ExamLandingPageFooterTextGroupedExam", item.Name);
			AssertEquals("Category", RawDataRegistry.Categories.Recruiter, item.Category);
			AssertEquals("Caption", "Exam Landing Page Footer Text - Grouped Exam", item.Caption);
			AssertEquals("Hint", "Specifies the footer text for Grouped Exam", item.Hint);
			AssertEquals("Flags", RegistryStorageFlags.System, item.Storage);
			AssertStartsWith("Default Value", @"<b>IMPORTANT:</b> Please read this before taking the exam. <br>* Results will only be recorded and submitted when a section has been completed.<br>* Do not press the back button between sections.<br> * Once a section has been submitted, the results cannot be altered. You can only retake a section once the exam’s resit period has elapsed. <br>", item.DefaultValue.ToString());
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems
		{
			get
			{
				yield return "DaxtraEnable";
				yield return "DaxtraServiceUrl";
				yield return "DaxtraAccountName";
				yield return "DaxtraSubjectRegex";
				yield return "DaxtraTimeout";
				yield return "DaxtraBatchSize";
				yield return "DaxtraResumeAcceptedFormats";
				yield return "DaxtraAutoParsingRules";
				yield return "LoggingKafkaBrokers";
				yield return "LoggingKafkaTopic";
				yield return "LoggingKafkaTopicUsername";
				yield return "LoggingKafkaTopicPassword";
				yield return "LoggingMinLevel";
				yield return "HackerRankPersonalAccessToken";
				yield return "TalogyClientId";
				yield return "TalogyClientSecret";
				yield return "TalogyGrantType";
				yield return "HREmailMailRetrievalProtocol";
				yield return "HREmailMailServer";
				yield return "HREmailMailServerPort";
				yield return "HREmailMailboxUserName";
				yield return "HREmailMailboxPassword";
				yield return "HREUseOAuth2";
				yield return "HREMs365OAuth2TenantId";
				yield return "HREMs365ApplicationId";
				yield return "HREMs365OAuth2Token";
				yield return "HREmailIMAPSecureConnectionType";
				yield return "HREmailPOP3SecureConnectionType";
				yield return "HREmailStartDaxtra";
			}
		}

		public void TestHREmailMailRetrievalProtocol()
		{
			AssertEquals("Protocol used to retrieve incoming mail", ItemSet.HREmailMailRetrievalProtocol.Hint);
			AssertEquals("Mail Retrieval Protocol", ItemSet.HREmailMailRetrievalProtocol.Caption);
			AssertEquals("POP3", ItemSet.HREmailMailRetrievalProtocol.DefaultValue);
		}

		public void TestHREmailMailServer()
		{
			AssertEquals("Mail Server", ItemSet.HREmailMailServer.Hint);
			AssertEquals("Mail Server", ItemSet.HREmailMailServer.Caption);
			AssertEquals("", ItemSet.HREmailMailServer.DefaultValue);
		}

		public void TestHREmailMailServerPort()
		{
			AssertEquals("Mail Server Port", ItemSet.HREmailMailServerPort.Hint);
			AssertEquals("Mail Server Port", ItemSet.HREmailMailServerPort.Caption);
			AssertEquals(110, ItemSet.HREmailMailServerPort.DefaultValue);
		}

		public void TestHREmailStartDaxtra()
		{
			AssertEquals("Determine start date for backlog HR emails to be parsed. If the emails were received prior to this date, the application won't automatically create job application and job applicant.", ItemSet.HREmailStartDaxtra.Hint);
			AssertEquals("Start date for HR emails processing", ItemSet.HREmailStartDaxtra.Caption);
			AssertEquals(DateTime.MinValue, ItemSet.HREmailStartDaxtra.DefaultValue);
		}

		public void TestHREmailMailboxUserName()
		{
			AssertEquals("e.g. user@example.com.", ItemSet.HREmailMailboxUserName.Hint);
			AssertEquals("Mailbox User Name", ItemSet.HREmailMailboxUserName.Caption);
			AssertEquals("", ItemSet.HREmailMailboxUserName.DefaultValue);
		}

		public void TestHREmailMailboxPassword()
		{
			AssertEquals("Mailbox Password", ItemSet.HREmailMailboxPassword.Hint);
			AssertEquals("Mailbox Password", ItemSet.HREmailMailboxPassword.Caption);
			AssertEquals("", ItemSet.HREmailMailboxPassword.DefaultValue);
		}

		public void TestHREUseOAuth2()
		{
			var item = ItemSet.HREUseOAuth2;

			AssertEquals("Name", "HREUseOAuth2", item.Name);
			AssertEquals("Category", Categories.Recruiter_EmailServiceTask_OAuth2, item.Category);
			AssertEquals("Caption", "Enable OAuth 2.0 authentication", item.Caption);
			AssertEquals("Hint", @"When enabled, OAuth 2.0 authentication will be used.", item.Hint);
			AssertEquals("DefaultValue", false, item.DefaultValue);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestHREMs365OAuth2TenantId()
		{
			var item = ItemSet.HREMs365OAuth2TenantId;
			AssertEquals("Name", "HREMs365OAuth2TenantId", item.Name);
			AssertEquals("Category", Categories.Recruiter_EmailServiceTask_OAuth2, item.Category);
			AssertEquals("Caption", "Tenant ID", item.Caption);
			AssertEquals("Hint", "The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used.", item.Hint);
			AssertEquals("DefaultValue", "", item.DefaultValue);
		}

		public void TestHREMs365ApplicationId()
		{
			var item = ItemSet.HREMs365ApplicationId;
			AssertEquals("Name", "HREMs365ApplicationId", item.Name);
			AssertEquals("Category", Categories.Recruiter_EmailServiceTask_OAuth2, item.Category);
			AssertEquals("Caption", "Application ID", item.Caption);
			AssertEquals("Hint", @"This is the Application ID registered in the Azure platform. It should be a unique identifier like 'acc8304d-88d3-4caa-8452-2be8061d7fba'.

Important: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the OAuth 2.0 authentication.", item.Hint);
			AssertEquals("DefaultValue", "", item.DefaultValue);
		}

		public void TestHREMs365OAuth2Token()
		{
			var item = ItemSet.HREMs365OAuth2Token;
			AssertEquals("Name", "HREMs365OAuth2Token", item.Name);
			AssertEquals("Category", Categories.Recruiter_EmailServiceTask_OAuth2, item.Category);
			AssertEquals("Caption", "OAuth 2.0 Access Token", item.Caption);
			AssertEquals("Hint", @"This setting stores the OAuth 2.0 Access Token that the HR Email Service Task will use during authentication.

Click on Grant Permissions to generate and store the OAuth 2.0 Access Token. If a token has already been generated, clicking on Grant Permissions will renew it. Click on the Clear button to clear the cached token.", item.Hint);

			AssertEquals("EmailType", EmailType.Incoming, item.EmailType);
			AssertEquals("HREMs365OAuth2TenantId", ItemSet.HREMs365OAuth2TenantId, item.Ms365OAuth2TenantId);
			AssertEquals("HREMs365ApplicationId", ItemSet.HREMs365ApplicationId, item.Ms365ApplicationIdForIncoming);
			AssertEquals("UseGraphApiForIncoming", null, item.UseGraphApiForIncoming);

			AssertNull("Default Token Identifier", item.DefaultValue.Identifier);
			AssertNull("Default Token User", item.DefaultValue.User);
			AssertNull("Default Token", item.DefaultValue.Token);

			var editorInfo = ItemSet.HREMs365OAuth2Token.EditorInfo as Ms365OAuth2TokenRegistryEditorInfo;
			AssertNotNull(editorInfo);
		}

		public void TestHREmailIMAPSecureConnectionType()
		{
			AssertEquals("The type of secure connection to use to connect to the IMAP mail server", ItemSet.HREmailIMAPSecureConnectionType.Hint);
			AssertEquals("IMAP Server Secure Connection", ItemSet.HREmailIMAPSecureConnectionType.Caption);
			AssertEquals("TLS", ItemSet.HREmailIMAPSecureConnectionType.DefaultValue);
		}

		public void TestHREmailPOP3SecureConnectionType()
		{
			AssertEquals("The type of secure connection to use to connect to the POP3 mail server", ItemSet.HREmailPOP3SecureConnectionType.Hint);
			AssertEquals("POP3 Server Secure Connection", ItemSet.HREmailPOP3SecureConnectionType.Caption);
			AssertEquals("NO", ItemSet.HREmailPOP3SecureConnectionType.DefaultValue);
		}

		public void TestReferringSourcesTypes()
		{
			AssertEquals("Referring Source Types", ItemSet.ReferringSourcesTypes.Caption);
			AssertEquals("The list of Referring Source Types.", ItemSet.ReferringSourcesTypes.Hint);
			AssertEquals(true, ItemSet.ReferringSourcesTypes.DefaultValue.Any());
			AssertContainsExactElementsInAnyOrder(new ReferringSourcesTypes().OfType<CodeDescriptionPair>().Select(x => $"{x.Code}|{x.MultilingualDescription}|True|Y"),
				ItemSet.ReferringSourcesTypes.DefaultValue.OfType<CodeDescriptionBool>().Select(x => $"{x.Code}|{x.Description}|{x.SystemDefined}|{x.Bool}"));
		}

		public void TestReferringStaffMembersToIgnoreGroup()
		{
			AssertEquals("Referring Staff Members to Ignore", ItemSet.ReferringStaffMembersToIgnoreGroup.Caption);
			AssertEquals("This allows internal staff members who work in the recruitment teams to forward emails to the HR Emails for registration but not be considered as a 'Referring Staff'.", ItemSet.ReferringStaffMembersToIgnoreGroup.Hint);
			AssertEquals(RegistryOptions.NotCached, RegistryOptions.NotCached & ItemSet.ReferringStaffMembersToIgnoreGroup.Options);
			AssertEquals(Guid.Empty, ItemSet.ReferringStaffMembersToIgnoreGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		public void TestReferringPartiesConfiguration()
		{
			AssertEquals("Recruitment Referring Parties", ItemSet.ReferringPartiesConfiguration.Caption);
			AssertEquals("The list of Recruitment Referring Parties.", ItemSet.ReferringPartiesConfiguration.Hint);
			AssertEquals(false, ItemSet.ReferringPartiesConfiguration.DefaultValue.Any());
		}

		public void TestHackerRankPersonalAccessToken()
		{
			AssertEquals("HackerRankPersonalAccessToken", ItemSet.HackerRankPersonalAccessToken.Name);
			AssertEquals("HackerRank Personal Access Token", ItemSet.HackerRankPersonalAccessToken.Caption);
			AssertEquals("Personal access token to use the HackerRank for Work API. Generated through HackerRank for Work account.", ItemSet.HackerRankPersonalAccessToken.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.HackerRankPersonalAccessToken.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.HackerRankPersonalAccessToken.Options);
			AssertEquals(string.Empty, ItemSet.HackerRankPersonalAccessToken.DefaultValue);
		}

		public void TestTalogyClientId()
		{
			AssertEquals("TalogyClientId", ItemSet.TalogyClientId.Name);
			AssertEquals("Talogy Client Id", ItemSet.TalogyClientId.Caption);
			AssertEquals("Client Id to use Talogy for Work API. Generated through Talogy for Work account.", ItemSet.TalogyClientId.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.TalogyClientId.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.TalogyClientId.Options);
			AssertEquals(string.Empty, ItemSet.TalogyClientId.DefaultValue);
		}

		public void TestTalogyClientSecret()
		{
			AssertEquals("TalogyClientSecret", ItemSet.TalogyClientSecret.Name);
			AssertEquals("Talogy Client Secret", ItemSet.TalogyClientSecret.Caption);
			AssertEquals("Client Secret to use Talogy for Work API. Generated through Talogy for Work account.", ItemSet.TalogyClientSecret.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.TalogyClientSecret.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.TalogyClientSecret.Options);
			AssertEquals(string.Empty, ItemSet.TalogyClientSecret.DefaultValue);
		}

		public void TestTalogyGrantType()
		{
			AssertEquals("TalogyGrantType", ItemSet.TalogyGrantType.Name);
			AssertEquals("Talogy Grant Type", ItemSet.TalogyGrantType.Caption);
			AssertEquals("Client Grant Type to use Talogy for Work API. Generated through Talogy for Work account.", ItemSet.TalogyGrantType.Hint);
			AssertEquals(RegistryStorageFlags.System, ItemSet.TalogyGrantType.Storage);
			AssertEquals(RegistryOptions.IsOnlyForSupport, ItemSet.TalogyGrantType.Options);
			AssertEquals("client_credentials", ItemSet.TalogyGrantType.DefaultValue);
		}
	}
}
