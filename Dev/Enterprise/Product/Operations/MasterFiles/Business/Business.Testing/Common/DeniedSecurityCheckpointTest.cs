using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DeniedSecurityCheckpointTest : TestCase
	{
		public void TestIsAllowed_IsAlwaysFalse()
		{
			var checkpoint = new DeniedSecurityCheckpoint();
			AssertEquals("IsAllowed should always be false", false, checkpoint.IsAllowed);
			checkpoint.IsAllowed = true;
			AssertEquals("IsAllowed should always be false, even when its set to true!", false, checkpoint.IsAllowed);
		}

		public void TestConstructorProperties()
		{
			var checkpoint = new DeniedSecurityCheckpoint();
			AssertEquals(checkpoint.Code, "GLOBALAccessDenied");
			AssertEquals(checkpoint.DisplayText, "Access Denied");

			var checkpoint2 = new DeniedSecurityCheckpoint(code: "DifferentCode", displayText: (NoResString)"Different Display Text");
			AssertEquals(checkpoint2.Code, "DifferentCode");
			AssertEquals(checkpoint2.DisplayText, "Different Display Text");
		}

		public void TestShowError_IsBasedOnDisplayText()
		{
			var checkpoint = new DeniedSecurityCheckpoint();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			checkpoint.ShowError();

			AssertEquals("Access Denied", UnitTestUserNotification.Instance.LastMessage.Text);

			var checkpoint2 = new DeniedSecurityCheckpoint(displayText: (NoResString)"Different Display Text");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			checkpoint2.ShowError();

			AssertEquals("Different Display Text", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
