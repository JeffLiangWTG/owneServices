using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	class InterchangeQuerierTest : TestCaseWithFactory
	{
		public void TestCheckMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

			var querier = new InterchangeQuerier(null);
			querier.Query();

			AssertEquals("Please choose a valid message with interchange.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

			var message = Factory.New<EDIMessage>();
			querier = new InterchangeQuerier(message);
			querier.Query();

			AssertEquals("Please choose a valid message with interchange.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);

			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_SessionGUID = ZGuid.Invalid;

			message = Factory.New<EDIMessage>();
			message.EM_EI = interchange.PK;

			querier = new InterchangeQuerier(message);
			querier.Query();

			AssertEquals("The current interchange does not have a valid eHub tracking id for query.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLaunchUrlLocally()
		{
			var guid = Guid.NewGuid();
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_SessionGUID = guid;

			var message = Factory.New<EDIMessage>();
			message.EM_EI = interchange.PK;

			new InterchangeQuerier(message).Query();

			CombineAssertions(() =>
			{
				AssertEquals("contains specific address", true, WebUrlLauncher.LastUrlLaunched.StartsWith("https://ehubadmin.wtg.zone/Messages?"));
				AssertEquals("contains specific guid", true, WebUrlLauncher.LastUrlLaunched.Contains(guid.ToString()));
			});
		}
	}
}
