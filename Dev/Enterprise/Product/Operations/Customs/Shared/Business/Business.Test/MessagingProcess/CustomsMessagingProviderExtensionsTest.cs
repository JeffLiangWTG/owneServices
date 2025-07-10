using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Environment;
using Moq;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	sealed class CustomsMessagingProviderExtensionsTest : TestCaseWithFactory
	{
		public void TestTestModeWarningValidation()
		{
			var messengers = new[] { customsMessenger };
			var provider = new CustomsMessagingProviderImplForTest(messengers);
			provider.EnableTestModeValidation = true;
			provider.IsInTestMode = true;

			var notifications = provider.RunPreSendValidation(new ActionResult(true)).ToList();

			AssertNotNull("Notifications", notifications);
			AssertEquals("Has 1 Notification", 1, notifications.Count);
			AssertEquals("Is Warning", true, notifications[0].IsWarning);
			AssertEquals("Msg", MessageSendingValidation.WarningWhenInTestModeText, notifications[0].Message);
		}

		public void TestPreSendValidation_JobDeclarationMessagingProviderFactory()
		{
			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();
			jobDecSup.Setup(x => x.RunPreSendValidation(It.IsAny<ActionResult>())).Returns([new MessageSendingError("JobDec Validation Error")]);

			var noSupport = new CustomsMessagingProviderImplForTest(messengers);
			var jobDecSupporter = new CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(messengers, jobDecSup.Object);

			noSupport.PreSendValidationMessagesForTest = [new MessageSendingError("SupplementaryCodeSupporter Validation Error")];
			jobDecSupporter.PreSendValidationMessagesForTest = [new MessageSendingError("SupplementaryCodeSupporter Validation Error")];

			var noSupportResult = noSupport.RunPreSendValidation(new ActionResult(true)).ToList();
			var jobDecSupporterResult = jobDecSupporter.RunPreSendValidation(new ActionResult(true)).ToList();

			CombineAssertions(() =>
			{
				AssertEquals("No Support no notifications", 0, noSupportResult.Count);
				AssertEquals("JodDec 1 notification", 1, jobDecSupporterResult.Count);
				AssertEquals("JobDec error", "JobDec Validation Error", jobDecSupporterResult[0].Message);
			});
		}

		public void TestPreSendValidation_ISupportPreSendValidation()
		{
			var messengers = new[] { customsMessenger };
			var noSupport = new CustomsMessagingProviderImplForTest(messengers);
			var withSupport = new CustomsMessagingProviderAllImplForTest(messengers);

			noSupport.PreSendValidationMessagesForTest = [new MessageSendingError("Not Supported")];
			withSupport.PreSendValidationMessagesForTest = [new MessageSendingError("Is Supported")];

			var noSupportResult = noSupport.RunPreSendValidation(new ActionResult(true)).ToList();
			var withSupportResult = withSupport.RunPreSendValidation(new ActionResult(true)).ToList();

			CombineAssertions(() =>
			{
				AssertEquals("No support", 0, noSupportResult.Count);
				AssertEquals("With support", 1, withSupportResult.Count);
			});
		}

		public void TestGetAdditionalValidationBusinessObjects()
		{
			var messengers = new[] { customsMessenger };
			var noSupport = new CustomsMessagingProviderImplForTest(messengers);
			var withSupport = new CustomsMessagingProviderWithAdditionalValidationImplForTest(Factory, messengers);

			var noSupportResult = noSupport.GetAdditionalValidationBusinessObjects();
			var withSupportResult = withSupport.GetAdditionalValidationBusinessObjects();

			CombineAssertions(() =>
			{
				AssertEquals("No support", 0, noSupportResult.Count);
				AssertEquals("With support", 1, withSupportResult.Count);
			});
		}

		public void TestConfigureProcess()
		{
			var processChain = new ActionChain("RootStep", (ActionResult ar) => ar);
			var messengers = new[] { customsMessenger };

			var provider = new CustomsMessagingProviderImplForTest(messengers);

			provider.ConfigureProcess(processChain);

			var chainStr = processChain.GetChainAsString();

			AssertEquals("No change to chain", "{ RootStep: }", chainStr);
		}

		public void TestConfigureProcess_JobDeclarationMessagingProviderFactory()
		{
			var processChain = new ActionChain("RootStep", (ActionResult ar) => ar);

			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();
			jobDecSup.Setup(x => x.ConfigureProcess(It.IsAny<ActionChain>())).Callback((ActionChain chain) =>
			{
				chain.FindAction("RootStep").InsertActionAfter("JobDecAction", (ActionResult pr) =>
				{
					pr.AppendInformationNotification("Hello JobDec");
					return pr;
				}, ActionLink.Success);
			});

			var provider = new CustomsMessagingProviderWithCommonJobDecProviderImplForTest(messengers, jobDecSup.Object);
			provider.ConfigureProcess(processChain);

			var chainStr = processChain.GetChainAsString();

			AssertEquals("Steps added to chain", "{ RootStep: success: { JobDecAction: } }", chainStr);

			processChain = new ActionChain("RootStep", (ActionResult ar) => ar);
			provider.ConfigProcessForTesting = (chain) => chain.FindAction("RootStep").InsertActionAfter("SupporterAction", null, ActionLink.Success);
			provider.ConfigureProcess(processChain);

			chainStr = processChain.GetChainAsString();

			AssertEquals("Steps added to chain", "{ RootStep: success: { SupporterAction: success: { JobDecAction: } } }", chainStr);
		}

		public void TestConfigureProcess_ISupportConfigureProcess()
		{
			var processChain = new ActionChain("RootStep", (ActionResult ar) => ar);
			var messengers = new[] { customsMessenger };

			var provider = new CustomsMessagingProviderAllImplForTest(messengers);
			provider.ConfigProcessForTesting = (chain) => chain.FindAction("RootStep").InsertActionAfter("SupporterAction", null, ActionLink.Success);

			provider.ConfigureProcess(processChain);

			var chainStr = processChain.GetChainAsString();

			AssertEquals("Step added to chain", "{ RootStep: success: { SupporterAction: } }", chainStr);
		}

		public void TestGetSendMessagesSecurityCheckpoint()
		{
			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();
			jobDecSup.Setup(x => x.SendMessagesSecurityCheckpoint).Returns(Env.Security.SendTestCustomsMessage);

			var noSecurity = new CustomsMessagingProviderImplForTest(new[] { customsMessenger });
			var withSecurity = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			var withSecurityAndJobDec = new CustomsMessagingProviderWithCommonJobDecProviderImplForTest(messengers, jobDecSup.Object);
			var jobdDecOnly = new CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(messengers, jobDecSup.Object);

			noSecurity.SendMessagesSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurity.SendMessagesSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurityAndJobDec.SendMessagesSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			jobdDecOnly.SendMessagesSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;

			CombineAssertions(() =>
			{
				AssertNull("No Security Support", noSecurity.GetSendMessagesSecurityCheckpoint());
				AssertSame("Security only", Env.Security.CustomsDISSendMessage, withSecurity.GetSendMessagesSecurityCheckpoint());
				AssertSame("Security & Job Dec - SupplementaryCodeSupporter preference", Env.Security.CustomsDISSendMessage, withSecurityAndJobDec.GetSendMessagesSecurityCheckpoint());
				AssertSame("Job Dec Only", Env.Security.SendTestCustomsMessage, jobdDecOnly.GetSendMessagesSecurityCheckpoint());
			});
		}

		public void TestGetSendMessagesWithErrorsSecurityCheckpoint()
		{
			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();
			jobDecSup.Setup(x => x.SendMessagesWithErrorsSecurityCheckpoint).Returns(Env.Security.SendTestCustomsMessage);

			var noSecurity = new CustomsMessagingProviderImplForTest(new[] { customsMessenger });
			var withSecurity = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			var withSecurityAndJobDec = new CustomsMessagingProviderWithCommonJobDecProviderImplForTest(messengers, jobDecSup.Object);
			var jobdDecOnly = new CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(messengers, jobDecSup.Object);

			noSecurity.SendMessagesWithErrorsSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurity.SendMessagesWithErrorsSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurityAndJobDec.SendMessagesWithErrorsSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			jobdDecOnly.SendMessagesWithErrorsSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;

			CombineAssertions(() =>
			{
				AssertNull("No Security Support", noSecurity.GetSendMessagesWithErrorsSecurityCheckpoint());
				AssertSame("Security only", Env.Security.CustomsDISSendMessage, withSecurity.GetSendMessagesWithErrorsSecurityCheckpoint());
				AssertSame("Security & Job Dec - SupplementaryCodeSupporter preference", Env.Security.CustomsDISSendMessage, withSecurityAndJobDec.GetSendMessagesWithErrorsSecurityCheckpoint());
				AssertSame("Job Dec Only", Env.Security.SendTestCustomsMessage, jobdDecOnly.GetSendMessagesWithErrorsSecurityCheckpoint());
			});
		}

		public void TestGetSendMessagesWithErrorsOverrideSecurityCheckpoint()
		{
			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();
			jobDecSup.Setup(x => x.SendMessagesWithErrorsOverrideSecurityCheckpoint).Returns(Env.Security.SendTestCustomsMessage);

			var noSecurity = new CustomsMessagingProviderImplForTest(new[] { customsMessenger });
			var withSecurity = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			var withSecurityAndJobDec = new CustomsMessagingProviderWithCommonJobDecProviderImplForTest(messengers, jobDecSup.Object);
			var jobdDecOnly = new CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(messengers, jobDecSup.Object);

			noSecurity.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurity.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			withSecurityAndJobDec.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;
			jobdDecOnly.SendMessagesWithErrorsOverrideSecurityCheckpointForTesting = Env.Security.CustomsDISSendMessage;

			CombineAssertions(() =>
			{
				AssertNull("No Security Support", noSecurity.GetSendMessagesWithErrorsOverrideSecurityCheckpoint());
				AssertSame("Security only", Env.Security.CustomsDISSendMessage, withSecurity.GetSendMessagesWithErrorsOverrideSecurityCheckpoint());
				AssertSame("Security & Job Dec - SupplementaryCodeSupporter preference", Env.Security.CustomsDISSendMessage, withSecurityAndJobDec.GetSendMessagesWithErrorsOverrideSecurityCheckpoint());
				AssertSame("Job Dec Only", Env.Security.SendTestCustomsMessage, jobdDecOnly.GetSendMessagesWithErrorsOverrideSecurityCheckpoint());
			});
		}

		public void TestGetCreditAndDPSCheckSupporter()
		{
			var messengers = new[] { customsMessenger };
			var jobDecSup = new Mock<ICommonJobDeclarationProvider>();

			var noSupport = new CustomsMessagingProviderImplForTest(new[] { customsMessenger });
			var withSupport = new CustomsMessagingProviderAllImplForTest(new[] { customsMessenger });
			var withSupportAndJobDec = new CustomsMessagingProviderWithCommonJobDecProviderImplForTest(messengers, jobDecSup.Object);
			var jobdDecOnly = new CustomsMessagingProviderWithJobDecProviderOnlyImplForTest(messengers, jobDecSup.Object);

			CombineAssertions(() =>
			{
				AssertNull("No CreditCheck Support", noSupport.GetCreditAndDPSCheckSupporter());
				AssertSame("CreditCheck only", withSupport, withSupport.GetCreditAndDPSCheckSupporter());
				AssertSame("CreditCheck & Job Dec - SupplementaryCodeSupporter preference", withSupportAndJobDec, withSupportAndJobDec.GetCreditAndDPSCheckSupporter());
				AssertSame("Job Dec Only", jobDecSup.Object, jobdDecOnly.GetCreditAndDPSCheckSupporter());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			childBO1 = Factory.New<DummyBizObjWithMessages>();
			customsMessenger = new CustomsMessengerImplForTest(childBO1);
		}

		DummyBizObjWithMessages childBO1;
		CustomsMessengerImplForTest customsMessenger;
	}
}
