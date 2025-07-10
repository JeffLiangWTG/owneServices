using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSendingValidationTest : TestCaseWithFactory
	{
		public void TestConstruction()
		{
			var messageErrorCollector = new CustomsNotificationCollector(declaration, true, true).GetMessageErrors();
			var validation = MessageSendingValidation.New(declaration, messageErrorCollector);
			AssertEquals(declaration, validation.TopLevelBusinessObjectForValidation);
			AssertEquals(messageErrorCollector, validation.MessageErrors);
			AssertEquals(Env.Security.CustomsDeclarationSendWithMessageErrors, validation.SendMessageWithErrorsSecurityCheckpoint);

			var validation2 = new MessageSendingValidation(declaration, messageErrorCollector, Env.Security.CustomsDeclarationAudit);
			AssertEquals(declaration, validation2.TopLevelBusinessObjectForValidation);
			AssertEquals(messageErrorCollector, validation2.MessageErrors);
			AssertEquals(Env.Security.CustomsDeclarationAudit, validation2.SendMessageWithErrorsSecurityCheckpoint);
		}

		public void TestWarningInTestMode()
		{
			var collection = validation.CheckBusinessObjectLevelValidation(true);
			Assert(collection.NotificationsAsString().Contains(MessageSendingValidation.WarningWhenInTestModeText));
			Assert(!collection.NotificationsAsString().Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
			declaration.JE_TransportMode = declaration.TransportModeMailCodeForTesting;
			declaration.Transports.AddNew();
			declaration.Transports[0].JW_IsLinked = false;
			collection = validation.CheckBusinessObjectLevelValidation(true);
			Assert(collection.NotificationsAsString().Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
			collection = validation.CheckBusinessObjectLevelValidation(false);
			Assert(!collection.NotificationsAsString().Contains(MessageSendingValidation.WarningWhenInTestModeText));
			Assert(!collection.NotificationsAsString().Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
			declaration.JE_OH_Importer = ZGuid.Invalid;
			collection = validation.CheckBusinessObjectLevelValidation(true);
			Assert(!collection.NotificationsAsString().Contains(MessageSendingValidation.WarningWhenInTestModeText));
			Assert(!collection.NotificationsAsString().Contains(MessageSendingValidation.WarningAndConfirmationWhenInTestModeText));
		}

		public void TestErrors()
		{
			declaration.JE_OH_Importer = ZGuid.Invalid;
			AssertEquals(false, validation.CheckBusinessObjectLevelValidation(notifier));
			Assert(notifier.InvalidOperationText.Contains("Importer"));
			Assert(notifier.InvalidOperationText.Contains(MessageSendingValidation.ErrorExistHeaderText));

			var collection = validation.CheckBusinessObjectLevelValidation();
			AssertEquals(true, collection.NotificationsAsString().Contains(MessageSendingValidation.ErrorExistHeaderText));
			AssertEquals(true, collection.ContainsError());
		}

		public void TestMessageErrorsNoContinue()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				declaration.JE_MessageType = "ZZZ";
				notifier.AnswerToContinueWithAction = false;
				AssertEquals(false, validation.CheckBusinessObjectLevelValidation(notifier));
				Assert(notifier.ContinueWithActionMessage.Contains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors"));
				Assert(notifier.ContinueWithActionMessage.Contains("Type: The code you have selected is not in the list."));
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		public void TestMessageErrorsWithContinue()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				declaration.JE_MessageType = "YYY";
				notifier.AnswerToContinueWithAction = true;
				AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier));
				Assert(notifier.ContinueWithActionMessage.Contains(MessageSendingValidation.MessageErrorsExistHeaderText));
				Assert(notifier.ContinueWithActionMessage.Contains("Type: The code you have selected is not in the list."));

				var collection = validation.CheckBusinessObjectLevelValidation();
				AssertEquals(true, collection.NotificationsAsString().Contains(MessageSendingValidation.MessageErrorsExistHeaderText));
				AssertEquals(false, collection.ContainsError());
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		public void TestMessageErrorsWithNoSecurityToSend()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				declaration.JE_MessageType = "YYY";
				notifier.AnswerToContinueWithAction = true;
				AssertEquals(false, validation.CheckBusinessObjectLevelValidation(notifier));
				Assert(notifier.InvalidOperationText.Contains($"{MessageSendingValidation.MessageErrorsExistWithNoSecurityRight} {string.Format(MessageSendingValidation.InformationForGetSecurityRight, Env.Security.CustomsDeclarationSendWithMessageErrors.DisplayTextPathToSecurityRight)}"));

				var collection = validation.CheckBusinessObjectLevelValidation();
				AssertEquals(true, collection.ContainsError($"{MessageSendingValidation.MessageErrorsExistWithNoSecurityRight} {string.Format(MessageSendingValidation.InformationForGetSecurityRight, Env.Security.CustomsDeclarationSendWithMessageErrors.DisplayTextPathToSecurityRight)}"));
				AssertEquals(true, collection.ContainsError(MessageSendingValidation.ErrorExistHeaderText));
				AssertContains("Shipment Type: The code you have selected is not in the list.", collection.ErrorNotificationsAsString());
				Assert(validation.IsErrorsExistWithNoSecurityRight);
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		public void TestMessageErrorsWithSupervisorOverrides()
		{
			var oldAllowed = Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed;
			try
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
				declaration.JE_MessageType = "YYY";
				notifier.AnswerToContinueWithAction = true;
				AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier));
				Assert(notifier.ContinueWithActionMessage.Contains(MessageSendingValidation.MessageErrorsExistHeaderText));
				Assert(notifier.ContinueWithActionMessage.Contains("Type: The code you have selected is not in the list."));
			}
			finally
			{
				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = oldAllowed;
			}
		}

		public void TestNoMessageErrorsOrErrors()
		{
			var validation = MessageSendingValidation.New(Factory.New<DummyBusinessObject>(), null);
			AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier));
			Assert(string.IsNullOrEmpty(notifier.InvalidOperationText));
			Assert(string.IsNullOrEmpty(notifier.ContinueWithActionMessage));
			Assert(!validation.IsErrorsExistWithNoSecurityRight);
		}

		public void TestShouldSendMessagesInTestMode()
		{
			var validation = MessageSendingValidation.New(Factory.New<DummyBusinessObject>(), null);
			AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier, true));
			Assert(notifier.ContinueWithActionMessage.Contains("This message(s) will be sent in test mode. i.e. this is for testing or training purposes only and no production data will be registered with Customs. Do you wish to continue?"));
		}

		public void TestGetMessageErrorWithoutValidation()
		{
			using (declaration.GetValidationSuspender())
			{
				declaration.JE_OH_Importer = ZGuid.Invalid;
			}

			var validation = MessageSendingValidation.New(declaration, null, false);
			AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier));

			declaration.Validation.ValidateAll();
			AssertEquals(false, validation.CheckBusinessObjectLevelValidation(notifier));
			Assert(notifier.InvalidOperationText.Contains("Importer"));
			Assert(notifier.InvalidOperationText.Contains(MessageSendingValidation.ErrorExistHeaderText));
		}

		public void TestOrderLinesAreNotLoadedDuringValidation()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TESTORG";
			org.MainAddress.City = "Sydney";

			declaration.JE_OH_Importer = org.PK;

			var order = Factory.NewWithValidTestData<Order>();
			order.OrderLines.AddNew().JO_Description = "TEST1";
			order.OrderLines.AddNew().JO_Description = "TEST2";

			declaration.AttachedOrders.Add(order);
			Factory.Save();

			var factory2 = NewFactory();
			BaseJobDeclaration declaration2;

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 1000, expectedHitCounts: new Dictionary<string, int>
				{
					{ JobOrderLineSchema.Constants.TableName, 0 }
				}))
			{
				declaration2 = factory2.Load<BaseJobDeclaration>(declaration.PK);
				// we need to touch orders for the collection to be registered as a child-collection
				_ = declaration2.AttachedOrders;
				var validation = MessageSendingValidation.New(declaration2, null, refreshValidation: true);
				validation.CheckBusinessObjectLevelValidation(notifier);
			}

			using (AssertDbHitsForAllFactories(ignoreUnspecified: true, thresholdForUnspecified: 1000, expectedHitCounts: new Dictionary<string, int>
				{
					{ JobOrderLineSchema.Constants.TableName, 1 }
				}))
			{
				declaration2.LoadChildEditableObjects();
			}
		}

		public void TestValidationOfConsolidatedDeclaration()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
				var declaration1 = consolidatedDeclaration.JobDeclarations[0];
				var declaration2 = consolidatedDeclaration.JobDeclarations[1];

				using (consolidatedDeclaration.SuspendValidationTesting())
				using (declaration1.SuspendValidationTesting())
				using (declaration2.SuspendValidationTesting())
				{
					validation = MessageSendingValidation.New(consolidatedDeclaration, null, false);
					declaration1.JE_DeclarationReference = "Dec1";
					declaration2.JE_DeclarationReference = "Dec2";

					declaration1.JE_AgentsReferenceInfo.AddError("Error1");
					declaration2.JE_DateOfFirstArrivalInfo.AddError("Error2");

					AssertEquals(false, validation.CheckBusinessObjectLevelValidation(notifier));
					AssertContainsInOrder("Invalid text", notifier.InvalidOperationText, "Dec1", "Error1", "Dec2", "Error2");

					consolidatedDeclaration.ClearAllNotifications();
					declaration1.ClearAllNotifications();
					declaration2.ClearAllNotifications();
					consolidatedDeclaration.CRD_PeriodToInfo.AddMessageError("MeessageError0");
					declaration1.JE_DateOfFirstArrivalInfo.AddMessageError("MessageError1");
					declaration2.JE_EFTModeInfo.AddMessageError("MessageError2");
					AssertEquals(true, validation.CheckBusinessObjectLevelValidation(notifier));
					AssertContainsInOrder("Continue text", notifier.ContinueWithActionMessage, "MeessageError0", "Dec1", "MessageError1", "Dec2", "MessageError2");
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = BaseJobDeclaration.New(Factory);
			validation = MessageSendingValidation.New(declaration, null);
			notifier = new SendsMessagesToCustomsShutterUpperer(false);
		}

		BaseJobDeclaration declaration;
		MessageSendingValidation validation;
		SendsMessagesToCustomsShutterUpperer notifier;
	}
}
