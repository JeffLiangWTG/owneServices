using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.Recruitment.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Registry
{
	[TestedType(typeof(RecruitmentDataRegistry))]
	sealed class RecruitmentDataRegistryTest : RegistryItemSetTestCase<RecruitmentDataRegistry>
	{
		public void TestRecruitmentModuleEnabledDefaults()
		{
			AssertEquals("Enables access to the Recruitment module, at Maintain -> [H] Human Resources", ItemSet.RecruitmentModuleEnabled.Hint);
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.RecruitmentModuleEnabled.Options);
			AssertEquals(false, ItemSet.RecruitmentModuleEnabled.DefaultValue);
		}

		public void TestWorkItemTemplateProperties_Defaults()
		{
			var defaultValue = ItemSet.WorkItemTemplateProperties.DefaultValue;
			AssertEquals("defaultValue.Count", 0, defaultValue.Count);
		}

		public void TestCreatePersonAPIEnabled()
		{
			AssertEquals(false, ItemSet.CreatePersonAPIEnabled.DefaultValue);
			AssertEquals(RegistryOptions.IsOnlyEditableBySupportIfHosted, ItemSet.CreatePersonAPIEnabled.Options);
		}

		public void TestConvertApiSecretKeyDefaults()
		{
			AssertEquals("ConvertApi Secret API Key can be obtained at https://www.convertapi.com/a/signup", ItemSet.ConvertApiSecretKey.Hint);
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.ConvertApiSecretKey.Options);
			AssertEquals("", ItemSet.ConvertApiSecretKey.DefaultValue);
		}

		public void TestResumeBacklogConversionHighWaterMarkDefaults()
		{
			AssertEquals("How far back in time to parse applications that might have CVs/resumes attached but that haven't been converted to PDF", ItemSet.ResumeBacklogConversionHighWaterMark.Hint);
			AssertEquals(RegistryOptions.IsOnlyForController, ItemSet.ResumeBacklogConversionHighWaterMark.Options);
			AssertGreaterThan(ItemSet.ResumeBacklogConversionHighWaterMark.DefaultValue, DateTime.Now);
		}

		protected override IEnumerable<string> ConditionallyVisibleRegistryItems => new List<string>(base.ConditionallyVisibleRegistryItems)
		{
			"RecruitmentModuleEnabled",
			"ConvertApiSecretKey",
			"ResumeBacklogConversionHighWaterMark",
			"AutomatedRejection_EmailSignatureImage",
			"AutomatedRejection_EmailTemplate",
			"AutomatedRejection_LinkedInURL",
			"AutomatedRejection_DaysToDelaySendingEmail",
		};

		public void TestMiddleManSMTPServerPasswordHasNormalCharacterCasing()
		{
			var dataType = RecruitmentDataRegistry.Instance.MiddleMan_SMTPServerPassword.Inner.DataType;
			var stringStorage = (StringRegistryDataType)dataType;
			AssertEquals("String case data type wasn't 'Normal'", CharacterCase.Normal, stringStorage.CharacterCase);
		}

		public void TestAutoRejectionEmailTemplateExists()
		{
			AssertEquals("This text will be copied into an email that will be sent to a candidate informing them of application rejection.", ItemSet.AutomatedRejection_EmailTemplate.Hint);
			AssertEquals(RegistryOptions.Default, ItemSet.AutomatedRejection_EmailTemplate.Options);
			AssertEquals(RecruitmentDataRegistry.DefaultEmailBody, ItemSet.AutomatedRejection_EmailTemplate.DefaultValue.EmailBody);
			AssertEquals(RecruitmentDataRegistry.DefaultSubject, ItemSet.AutomatedRejection_EmailTemplate.DefaultValue.EmailSubject);
		}

		public void TestAutomatedRejectionEmailSignatureImage()
		{
			AssertEquals("Company logo image used in the signature of the automated rejection emails.", ItemSet.AutomatedRejection_EmailSignatureImage.Hint);
			AssertEquals(RegistryOptions.Default, ItemSet.AutomatedRejection_EmailSignatureImage.Options);
		}

		public void TestAutomatedRejectionLinkedInURL()
		{
			AssertEquals("The URL to insert into the rejection email that directs to the WTG LinkedIn page.", ItemSet.AutomatedRejection_LinkedInURL.Hint);
			AssertEquals(RegistryOptions.Default, ItemSet.AutomatedRejection_LinkedInURL.Options);
			AssertEquals(RecruitmentDataRegistry.DefaultLinkedInURL, ItemSet.AutomatedRejection_LinkedInURL.DefaultValue);
		}

		public void TestAutomatedRejectionDaysToDelaySendingEmail()
		{
			AssertEquals("The number of days to wait from rejecting a candidate to sending the rejection email. 0 days will be an immediate send.", ItemSet.AutomatedRejection_DaysToDelaySendingEmail.Hint);
			AssertEquals(RegistryOptions.Default, ItemSet.AutomatedRejection_DaysToDelaySendingEmail.Options);
			AssertEquals(RecruitmentDataRegistry.DefaultRejectionEmailSendDelay, ItemSet.AutomatedRejection_DaysToDelaySendingEmail.DefaultValue);
		}
	}
}
