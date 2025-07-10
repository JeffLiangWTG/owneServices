using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Business.MessagingProcess.Testing;

namespace Enterprise.Customs.Business.Testing.MessagingProcess
{
	sealed class CustomsMessagingSupporterExtensionsTest : TestCaseWithFactory
	{
		public void TestNoNessengersValidation()
		{
			var messengers = System.Array.Empty<ICustomsMessenger>();
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(new CustomsMessagingProviderImplForTest(messengers)));

			var notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 1 Notification", 1, notifications.Count);
			AssertEquals("No messenger msg", "Error: There are no messengers to send messages with", notifications[0].MessageIncludingPrefix);

			messengers = new[] { customsMessenger };
			supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(new CustomsMessagingProviderImplForTest(messengers)));

			notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 0 Notification", 0, notifications.Count);
		}

		public void TestBusinessObjectMessageErrorValidation()
		{
			var messengers = new[] { customsMessenger };
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(new CustomsMessagingProviderImplForTest(messengers)));

			var notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 0 Notifications", 0, notifications.Count);

			topLevelBO.Z0_Description = "message error";

			notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 1 Notification", 1, notifications.Count);
			AssertEquals("Is Warning", true, notifications[0].IsWarning);
			AssertEquals("Msg", $"{MessageSendingValidation.MessageErrorsExistHeaderText}\nDescription: Message Error", notifications[0].Message);
		}

		public void TestAdditionalBusinessObjectMessageErrorValidation()
		{
			var messengers = new[] { customsMessenger };
			var provider = new CustomsMessagingProviderWithAdditionalValidationImplForTest(Factory, messengers);
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(provider));

			var notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 0 Notifications", 0, notifications.Count);

			provider.LocalBO.Z0_Description = "message error";

			notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 1 Notification", 1, notifications.Count);
			AssertEquals("Is Warning", true, notifications[0].IsWarning);
			AssertEquals("Msg", $"{MessageSendingValidation.MessageErrorsExistHeaderText}\nDescription: Message Error", notifications[0].Message);
		}

		public void TestValidateChildrenWhichHveNotBeenLoaded()
		{
			var dec = (BaseJobDeclaration)Factory.New<Integration.Customs.TR.IJobDeclaration>();
			var header = dec.ActiveEntryHeaders.AddNew();
			var line = header.AllEntryLines.AddNew();
			var fee = line.Fees.AddNew();
			fee.CF_MethodOfCalculation = "X";

			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };

			var loadedDeclaration = newFactory.Load<Integration.Customs.TR.IJobDeclaration>(dec.PK);

			AssertNotNull("Declaration should be loaded", loadedDeclaration);

			var localChild = newFactory.New<DummyBizObjWithMessages>();
			var messengers = new[] { new CustomsMessengerImplForTest(localChild) };
			var provider = new CustomsMessagingProviderWithAdditionalValidationImplForTest(newFactory, messengers);
			var supporter = new CustomsMessagingSupporter((BusinessObject)loadedDeclaration, new CustomsMessagingProviderFactoryImplForTest(provider));
			var notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();

			var messages = string.Join("\n", notifications.Select(x => x.Message));

			AssertContains("Fee validated", "Method Of Calculation: The code you have selected is not in the list", messages);
		}

		public void TestProviderPreSendValidation()
		{
			var messengers = new[] { customsMessenger };
			var provider = new CustomsMessagingProviderAllImplForTest(messengers);
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(provider));

			var notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 0 Notifications", 0, notifications.Count);

			provider.PreSendValidationMessagesForTest = [new MessageSendingWarning("We are out of dog food")];

			notifications = supporter.RunPreSendValidation(new ActionResult(true)).ToList();
			AssertEquals("Has 1 Notification", 1, notifications.Count);
			AssertEquals("Is Warning", true, notifications[0].IsWarning);
			AssertEquals("Msg", "We are out of dog food", notifications[0].Message);
		}

		public void TestSendMessageBusinessActionProviderType()
		{
			var messengers = new[] { customsMessenger };
			var testClass = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(new CustomsMessagingProviderImplForTest(messengers)));

			AssertType<SendMessagesBusinessActionProvider>(testClass.GetSendMessagesBusinessActionProvider());
		}

		public void TestSendMessages()
		{
			var messengers = new[] { new CustomsMessengerImplForTest(childBO1) };
			var supporter = new CustomsMessagingSupporter(topLevelBO, new CustomsMessagingProviderFactoryImplForTest(new CustomsMessagingProviderImplForTest(messengers)));

			CombineAssertions(() =>
			{
				var result = supporter.SendMessages();
				AssertEquals("Send Successful no GUI", true, result.Success);

				result = supporter.SendMessages(new SendMessagesGuiActionProviderImplForTest());
				AssertEquals("Send Successful with GUI", true, result.Success);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			topLevelBO = Factory.New<DummyBizObjWithMessages>();
			childBO1 = Factory.New<DummyBizObjWithMessages>();
			customsMessenger = new CustomsMessengerImplForTest(childBO1);
		}

		DummyBizObjWithMessages topLevelBO;
		DummyBizObjWithMessages childBO1;
		CustomsMessengerImplForTest customsMessenger;
	}
}
