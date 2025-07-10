using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class EBondMessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestWarningInTestMode()
		{
			var expectedMessage = MessageSendingValidation.WarningWhenInTestModeText;

			var declaration = Factory.New<JobDeclaration>();
			var validation = new EBondMessageSendingValidation(declaration);

			var collection = validation.CheckBusinessObjectLevelValidation(true);
			AssertContains(expectedMessage, collection.NotificationsAsString());

			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			declaration.Transports.AddNew();
			declaration.Transports[0].JW_IsLinked = false;

			collection = validation.CheckBusinessObjectLevelValidation(true);
			AssertContains(expectedMessage, collection.NotificationsAsString());

			collection = validation.CheckBusinessObjectLevelValidation(false);
			AssertNotContains(expectedMessage, collection.NotificationsAsString());

			declaration.JE_OH_Importer = CargoWise.Types.ZGuid.Invalid;
			collection = validation.CheckBusinessObjectLevelValidation(true);
			AssertNotContains(expectedMessage, collection.NotificationsAsString());
		}

		public void TestMessageErrorsNoContinue()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;

			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "###";

				var notifier = new SendsMessagesToCustomsShutterUpperer(false)
				{
					AnswerToContinueWithAction = false
				};

				var validation = new EBondMessageSendingValidation(declaration);

				Assert(!validation.CheckBusinessObjectLevelValidation(notifier));
				AssertContains("It is likely that your message(s) will be rejected by Surety Agent, as they have the following message errors", notifier.ContinueWithActionMessage);
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}
	}
}
