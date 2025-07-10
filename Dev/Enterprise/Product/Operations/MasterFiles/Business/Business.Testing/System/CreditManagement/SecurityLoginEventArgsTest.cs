using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SecurityLoginEventArgsTest : TestCase
	{
		#region TestWasCancelled

		public void TestWasCancelled()
		{
			var e = new SecurityLoginEventArgs((NoResString)string.Empty, (NoResString)string.Empty, (s) => s.ReceivablesOnCreditHoldController);
			AssertEquals(false, e.WasCancelled);

			e.MessageToShowWhenNotAllowed = MultilingualString.Join(",", (NoResString)"bla ", SecurityLogin.CancelledText);
			AssertEquals("if the message contains cancelled, then it was cancelled.", true, e.WasCancelled);
		}

		#endregion

		public void TestMessageOnAllowedToProceed()
		{
			var message = "Error Message";
			var args = new SecurityLoginEventArgs((NoResString)"Prompt Message", (NoResString)message, (s) => s.ReceivablesOnCreditHoldController);

			AssertEquals(false, args.IsAllowedToProceed);
			AssertEquals(message, args.MessageToShowWhenNotAllowed);
			AssertEquals(message, args.Message);

			args.IsAllowedToProceed = true;
			AssertEquals(message, args.MessageToShowWhenNotAllowed);
			AssertEquals("", args.Message);
		}

		public void TestHideApprovalRequestButton()
		{
			var e_Show = new SecurityLoginEventArgs((NoResString)string.Empty, (NoResString)string.Empty, (s) => s.ReceivablesOnCreditHoldController);
			AssertEquals(false, e_Show.HideApprovalRequestButton);

			var e_Hide = new SecurityLoginEventArgs((NoResString)string.Empty, (NoResString)string.Empty, (s) => s.ReceivablesOnCreditHoldController)
			{
				HideApprovalRequestButton = true
			};
			AssertEquals(true, e_Hide.HideApprovalRequestButton);
		}
	}
}
