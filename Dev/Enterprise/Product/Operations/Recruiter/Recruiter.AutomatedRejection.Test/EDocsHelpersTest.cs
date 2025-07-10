using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruiter.AutomatedRejection;
using Enterprise.Recruiter.Business;
using Enterprise.Recruiter.Business.Testing;
using Enterprise.Recruitment.Registry;
using MailManager;
using NUnit.Framework;

namespace Enterprise.Recruiter.Testing.AutomatedRejection
{
	sealed class EDocsHelpersTest : TestCase
	{
		public void TestNullIfEmpty()
		{
			CombineAssertions(() =>
			{
				AssertNull(EDocsHelpers.NullIfEmpty(null));
				AssertNull(EDocsHelpers.NullIfEmpty(string.Empty));
				AssertNotNull(EDocsHelpers.NullIfEmpty("hi"));
			});
		}

		[UseSnapshotProtection]

		public void TestSaveEmailToEdocsAndAddToConversation()
		{
			// arrange
			var factory = new BusinessObjectFactory();
			var mailItem = NewMailItem(factory);
			var application = AutomatedRejectionTestHelper.CreateApplication(factory, "Bobette Rossini");
			factory.Save();

			var conversation = application.EConversation.RootConversation;

			using (RecruitmentDataRegistry.Instance.ReceivedMailDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.RefDocTypes.Email))
			{
				// act
				var eDoc = EDocsHelpers.SaveEmailToEdocsAndAddToConversation(mailItem, conversation);

				// assert
				AssertEmailSavedToEDocs(application, mailItem, eDoc);
			}
		}

		public static void AssertEmailSavedToEDocs(HRJobApplication application, MailItem mailItem, IeDoc eDoc)
		{
			AssertNotNull(application);
			AssertNotNull(mailItem);
			AssertNotNull(eDoc);

			// assert Application EDocs contains the email
			var files = application.DocManagerInfo.Files;
			AssertEquals(1, files.Count);

			var localEDoc = files.GetFromUniqueKey(eDoc.UniqueKey.ToGuid());
			AssertEquals($"{mailItem.MI_Subject}.{EDocsHelpers.EmailFileExtension}", localEDoc.FileName);

			// assert conversation contains the message from the email
			var conversation = application.EConversation.RootConversation;
			Assert(conversation.AnyLocalMessageContains(mailItem.MI_Subject));
		}

		static MailItem NewMailItem(BusinessObjectFactory factory)
		{
			var mailItem = factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			return mailItem;
		}
	}
}
