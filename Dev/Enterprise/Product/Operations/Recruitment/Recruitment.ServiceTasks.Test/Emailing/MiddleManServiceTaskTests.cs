using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.EConversation.Business;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Registry;
using Enterprise.Recruitment.ServiceTasks.Emailing;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	[TestedType(typeof(MiddleManServiceTask))]
	sealed class MiddleManServiceTaskTests : ServiceTaskTestCase<MiddleManServiceTask>
	{
		IDisposable userContext;

		protected override void TearDownCore()
		{
			userContext.Dispose();
			base.TearDownCore();
		}

		public void TestCreateMailFilter()
		{
			var filter = MiddleManServiceTask.CreateMailFilter();

			AssertEquals("MMT", filter.Code);
			Assert("Filter should be enabled", filter.IsEnabled);
		}

		[UseSnapshotProtection]
		public void TestCreateMailFilter_AddressPredicate_CanSend()
		{
			var filter = MiddleManServiceTask.CreateMailFilter();
			var factory = new BusinessObjectFactory();

			var mailItem = factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_LastAttemptDateTime = ZDateTime.Now.AddDays(-1);

			var address = RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value;
			_ = mailItem.AddRecipientForUserCommunication(address);

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, address))
			{
				Assert("Filter should process emails sent to the middleman address", filter.CanProcess(mailItem));
			}
		}

		[UseSnapshotProtection]
		public void TestCreateMailFilter_AddressPredicate_CannotSend()
		{
			AssertCreateMailFilterAddressPredicateCannotSend("test@wisetechglobal.com");
			AssertCreateMailFilterAddressPredicateCannotSend("<test@wisetechglobal.com> [JerryTest]");
		}

		void AssertCreateMailFilterAddressPredicateCannotSend(string address)
		{
			var filter = MiddleManServiceTask.CreateMailFilter();
			var factory = new BusinessObjectFactory();
			var mailItem = factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_LastAttemptDateTime = ZDateTime.Now.AddDays(-1);
			_ = mailItem.AddRecipientForUserCommunication(address);

			AssertNotEquals(address, RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value);

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.Value))
			{
				Assert("Filter should not process emails not sent to the middleman address", !filter.CanProcess(mailItem));
			}
		}

		public void TestMiddleManServiceTask_RecModuleDisabled()
		{
			var methodInfo = typeof(MiddleManServiceTask).GetMethod(nameof(MiddleManServiceTask.CheckRecruitmentModuleIsEnabled));
			Assert("HostedServiceRequirement for Recruitment module is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("Recruitment module is disabled. This task will not run.", MiddleManServiceTask.CheckRecruitmentModuleIsEnabled());
			}
		}

		public void TestMiddleManServiceTask_EmptyMiddleManAddress()
		{
			var methodInfo = typeof(MiddleManServiceTask).GetMethod(nameof(MiddleManServiceTask.CheckMiddleManForwardingAddressIsSpecified));
			Assert("HostedServiceRequirement for middle man forwarding address is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("No middle-man address specified. This task will not run.", MiddleManServiceTask.CheckMiddleManForwardingAddressIsSpecified());
			}
		}

		public void TestMiddleManServiceTask_MailDocType()
		{
			var methodInfo = typeof(MiddleManServiceTask).GetMethod(nameof(MiddleManServiceTask.CheckReceivedMailDocType));
			Assert("HostedServiceRequirement for received mail doc type is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			using (RecruitmentDataRegistry.Instance.ReceivedMailDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("No document type to attach. This task will not run.", MiddleManServiceTask.CheckReceivedMailDocType());
			}
		}

		public void TestMiddleManServiceTask_FailedProcessingEmail()
		{
			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (RecruitmentDataRegistry.Instance.ReceivedMailDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.RefDocTypes.Email))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				//arrange
				var factory = new BusinessObjectFactory();

				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);

				//act
				var serviceTask = new MiddleManServiceTask { ServiceLogger = DummyLogger(out var logs) };
				serviceTask.RunTask(CancellationToken.None);

				factory.Save();

				//assert
				AssertContains("Error: Failed processing email from " + email.MI_From, logs.ToString());
			}
		}

		public void TestMiddleManServiceTask_FinishedProcessingEmail()
		{
			using (RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (RecruitmentDataRegistry.Instance.ReceivedMailDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.RefDocTypes.Email))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				//arrange
				var factory = new BusinessObjectFactory();
				var application = factory.NewWithValidTestData<HRJobApplication>();

				var participant = factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantID = ZGuid.Empty;
				participant.EmailAddress = "test@wisetechglobal.com";
				participant.JCP_ParticipantTableCode = string.Empty;

				var conversation = factory.NewWithPrimaryKey<JobConversation>(Guid.Parse("e5b050b1c5d04dba9becaf4c781bf7a2"));
				conversation.JCC_ParentTableCode = HRJobApplicationSchema.Constants.Prefix;
				conversation.JCC_ParentID = application.PK;
				conversation.Participants.Add(participant);

				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);
				var expected = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailAfterMMT(new BusinessObjectFactory(), false);

				factory.Save();

				var mailSender = new MockIMailSender();
				mailSender.SendAction = (mailItem) => { AssertMailItemsEqualByField(expected, mailItem); };

				using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
				{
					//act
					var serviceTask = new MiddleManServiceTask { ServiceLogger = DummyLogger(out var logs) };
					serviceTask.RunTask(CancellationToken.None);

					//assert
					AssertContains("Information: Finished processing email from " + email.MI_From, logs.ToString());
				}

				ObjectFactory.DisposeSubstitutions();
			}
		}

		[UseSnapshotProtection]
		public void TestProcessEmail()
		{
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				// arrange
				var factory = new BusinessObjectFactory();
				var application = factory.NewWithValidTestData<HRJobApplication>();

				var participant = factory.NewWithValidTestData<JobConversationParticipant>();
				participant.JCP_ParticipantID = ZGuid.Empty;
				participant.EmailAddress = "test@wisetechglobal.com";
				participant.JCP_ParticipantTableCode = string.Empty;

				var conversation = factory.NewWithPrimaryKey<JobConversation>(Guid.Parse("e5b050b1c5d04dba9becaf4c781bf7a2"));
				conversation.JCC_ParentTableCode = HRJobApplicationSchema.Constants.Prefix;
				conversation.JCC_ParentID = application.PK;
				conversation.Participants.Add(participant);

				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);
				var expected = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailAfterMMT(new BusinessObjectFactory(), false);

				factory.Save();

				var mailSender = new MockIMailSender();
				mailSender.SendAction = (mailItem) => { AssertMailItemsEqualByField(expected, mailItem); };

				using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
				{
					// act
					var result = EmailForwarderUtilities.ProcessEmail(email, DummyLogger(out var logs));

					// assert
					Assert("Email should have been processed", result);
					AssertContains("Successfully forwarded 1/1 emails.", logs.ToString());
				}

				ObjectFactory.DisposeSubstitutions();
			}
		}

		[UseSnapshotProtection]
		public void TestProcessEmail_NoJobConversation()
		{
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				// arrange
				var factory = new BusinessObjectFactory();
				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);

				factory.Save();

				using (ObjectFactory.Substitute<ISmtpSender>(new MockIMailSender()))
				{
					// act
					var result = EmailForwarderUtilities.ProcessEmail(email, DummyLogger(out var logs));

					// assert
					Assert("No job conversation means ProcessEmail should return false", !result);
					AssertContains("Unable to find a job conversation for this email.", logs.ToString());
				}

				ObjectFactory.DisposeSubstitutions();
			}
		}

		[UseSnapshotProtection]
		public void TestProcessEmail_NoParticipants()
		{
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				// arrange
				var factory = new BusinessObjectFactory();
				var application = factory.NewWithValidTestData<HRJobApplication>();
				var conversation = factory.NewWithPrimaryKey<JobConversation>(Guid.Parse("e5b050b1c5d04dba9becaf4c781bf7a2"));
				conversation.JCC_ParentTableCode = HRJobApplicationSchema.Constants.Prefix;
				conversation.JCC_ParentID = application.PK;

				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);

				factory.Save();

				using (ObjectFactory.Substitute<ISmtpSender>(new MockIMailSender()))
				{
					// act
					var result = EmailForwarderUtilities.ProcessEmail(email, DummyLogger(out var logs));

					// assert
					Assert("No participants means ProcessEmail should return false", !result);

					CombineAssertions(() =>
					{
						AssertContains(@"Debug: OtherParticipantsCount=0 ParticipantsCount=1 Original sender='Benjamin.Sutas@wisetechglobal.com'", logs.ToString());
						AssertContains(@"Debug: Name='Benjamin.Sutas' Subscribed=Y ParentIsActive=Y Email='Benjamin.Sutas@wisetechglobal.com'", logs.ToString());
						AssertContains(@"Error: Unable to find any participants to forward an email to.", logs.ToString());
					});
				}

				ObjectFactory.DisposeSubstitutions();
			}
		}

		[UseSnapshotProtection]
		public void TestProcessEmail_SomeForwardedEmailsFail()
		{
			using (RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "recruiting@wisetechglobal.com"))
			using (ZArchitecture.Environment.EnvProxy.Instance.SuspendBranchAccessError())
			{
				// arrange

				var factory = new BusinessObjectFactory();
				var application = factory.NewWithValidTestData<HRJobApplication>();

				var participant1 = factory.NewWithValidTestData<JobConversationParticipant>();
				participant1.JCP_ParticipantID = ZGuid.Empty;
				participant1.EmailAddress = "test@wisetechglobal.com";
				participant1.JCP_ParticipantTableCode = string.Empty;

				var participant2 = factory.NewWithValidTestData<JobConversationParticipant>();
				participant2.JCP_ParticipantID = ZGuid.Empty;
				participant2.EmailAddress = "test2@wisetechglobal.com";
				participant2.JCP_ParticipantTableCode = string.Empty;

				var conversation = factory.NewWithPrimaryKey<JobConversation>(Guid.Parse("e5b050b1c5d04dba9becaf4c781bf7a2"));
				conversation.JCC_ParentTableCode = HRJobApplicationSchema.Constants.Prefix;
				conversation.JCC_ParentID = application.PK;
				conversation.Participants.Add(participant1);
				conversation.Participants.Add(participant2);

				var email = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailBeforeMMT(factory, true);
				var expected = RecruitmentEmailDataHelpers.CreateEmail_FirstEmailAfterMMT(new BusinessObjectFactory(), false);

				factory.Save();

				var mailSender = new MockIMailSender();
				mailSender.SendAction =
					(mailItem) =>
					{
						if (mailItem.MailRecipients.ContainsRecipientWithEmail(participant2.EmailAddress))
						{
							mailSender.RejectedRecipients = new RejectedRecipientInfo[] { new RejectedRecipientInfo { Address = participant2.EmailAddress, ErrorCode = 123, ErrorMessage = "failed to send email" } };
						}
					};

				using (ObjectFactory.Substitute<ISmtpSender>(mailSender))
				{
					// act
					var result = EmailForwarderUtilities.ProcessEmail(email, DummyLogger(out var logs));

					// assert
					Assert("Partial forwarding success should return false from ProcessEmail", !result);
					AssertContains("Unable to send forwarded email: Subject=\"hihi\" From=\"recruiting@wisetechglobal.com\" To=\"<test2@wisetechglobal.com>\"", logs.ToString());
					AssertContains("Successfully forwarded 1/2 emails.", logs.ToString());

					AssertEquals("Unable to send email, had rejected recipients", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}

				ObjectFactory.DisposeSubstitutions();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void AssertMailItemsEqualByField(MailItem expected, MailItem actual)
		{
			CombineAssertions("MailItems were not equal", () =>
			{
				AssertEquals(nameof(expected.MI_Application), expected.MI_Application, actual.MI_Application);
				AssertEquals(nameof(expected.MI_Status), expected.MI_Status, actual.MI_Status);
				AssertEquals(nameof(expected.MI_Direction), expected.MI_Direction, actual.MI_Direction);
				AssertEquals(nameof(expected.MI_From), expected.MI_From, actual.MI_From);
				AssertEquals(nameof(expected.MI_ReplyTo), expected.MI_ReplyTo, actual.MI_ReplyTo);
				AssertEquals(nameof(expected.MI_Subject), expected.MI_Subject, actual.MI_Subject);
				AssertEquals(nameof(expected.MI_Body), expected.MI_Body, actual.MI_Body);
				AssertEquals(nameof(expected.MI_Header), expected.MI_Header, actual.MI_Header);
				AssertDateTimeWithinNSeconds(nameof(expected.MI_ReceivedDateTime), expected.MI_ReceivedDateTime, actual.MI_ReceivedDateTime, 5);
				AssertEquals(nameof(expected.MI_POP3UIDL), expected.MI_POP3UIDL, actual.MI_POP3UIDL);
				AssertEquals(nameof(expected.MI_Encoding), expected.MI_Encoding, actual.MI_Encoding);
				AssertEquals(nameof(expected.MI_ContentType), expected.MI_ContentType, actual.MI_ContentType);
			});
		}

		// copied from AssertDateTimeWithinOneSecond() in NUnitCore/Assertion.cs
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public static void AssertDateTimeWithinNSeconds(string message, ZDateTime expected, ZDateTime actual, int seconds)
		{
			if ((actual < expected.AddSeconds(-seconds)) || (actual > expected.AddSeconds(seconds)))
			{
				FailNotEquals(message, expected, actual, isHtmlMessage: false);
			}
			else
			{
				AssertionCount++;
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			userContext = ZArchitecture.Environment.EnvProxy.Instance.TemporaryServiceTaskContext("MMT", canRunInAnyBranch: true);

			RecruitmentDataRegistry.Instance.RecruitmentModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RecruitmentDataRegistry.Instance.MiddleMan_ForwardingAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RecruitmentDataHelpers.MiddleManAddressForTest);
			RecruitmentDataRegistry.Instance.ReceivedMailDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.RefDocTypes.Email);
		}

		ILogger DummyLogger(out StringBuilder logs)
		{
			var sb = logs = new StringBuilder();

			var logger = new DummyLogger();
			logger.OnLog += (o, e) => sb.AppendLine(e.Type + ": " + e.Message);
			return logger;
		}

		public void TestParseEmailAddress()
		{
			var a = "hello@gmail.com";
			var b = "recruiting@wisetechglobal.com";
			var c = "\"testrecruitment@gmail.com\" <testrecruitment@gmail.com>";
			var d = "\"an alias\" <something_@gmail.com>";
			var e = "<>";

			CombineAssertions(() =>
			{
				AssertEquals("hello@gmail.com", EmailForwarderUtilities.ParseEmailAddress(a));
				AssertEquals("recruiting@wisetechglobal.com", EmailForwarderUtilities.ParseEmailAddress(b));
				AssertEquals("testrecruitment@gmail.com", EmailForwarderUtilities.ParseEmailAddress(c));
				AssertEquals("something_@gmail.com", EmailForwarderUtilities.ParseEmailAddress(d));
				AssertEquals(null, EmailForwarderUtilities.ParseEmailAddress(e));
			});
		}
	}
}
