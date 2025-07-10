using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class FlexibleUserNotificationTest : TransactionedTestCase
	{
		public void TestShowMessageWithLargeValidationDetails()
		{
			ZFormModaliser.LastFormShownDialogForTest = null;
			using (Globals.SetIsUnitTestingProductionFunctionality())
			{
				var userNotification = new FlexibleUserNotification();
				userNotification.Show(LargeMessageContent, "Large Text Test", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				AssertType<ZMessageBoxWithFixedSize>(@"In ZMessageBox, we are use TextRenderer.MeasureText to calculate the size of the form to provide a good user visual experience.
But it always takes a lot of time to calculate large text.
So we need to balance the design of the display message, it should first focus on reminding rather than delaying the customer's business operations.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestShowMessageInConsoleSession()
		{
			var isConsoleSession = Globals.IsConsoleSession;
			using (Globals.SetIsUnitTestingProductionFunctionality())
			using (new DisposableAction(() => Globals.IsConsoleSession = isConsoleSession))
			{
				Globals.IsConsoleSession = true;
				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertExceptionThrown<NotSupportedException>("We should not show any forms in console session.", "Dialogs asking for user input are not supported on the command line", () =>
				{
					var userNotification = new FlexibleUserNotification();
					userNotification.Show(LargeMessageContent, "Large Text Test", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				});
				AssertNull("We should not show any forms in console session.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestShowMessageInNormalSession()
		{
			var message = "Sometimes, we need to use this messagebox for showing a large number of validation errors.";
			var sb = new StringBuilder();
			for (var i = 0; i < 900; i++)
			{
				sb.AppendLine(message);
			}

			ZFormModaliser.LastFormShownDialogForTest = null;
			using (Globals.SetIsUnitTestingProductionFunctionality())
			{
				var userNotification = new FlexibleUserNotification();
				userNotification.Show(sb.ToString(), "Normal Text Test", MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
				AssertType<ZMessageBox>(@"Should use the default ZMessageBox with a normal text.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		string LargeMessageContent
		{
			get
			{
				if (string.IsNullOrWhiteSpace(largeMessageContent))
				{
					var message = "Sometimes, we need to use this messagebox for showing a large number of validation errors.";
					var sb = new StringBuilder();
					for (var i = 0; i < 1000; i++)
					{
						sb.AppendLine(message);
					}

					largeMessageContent = sb.ToString();
				}

				return largeMessageContent;
			}
		}

		string largeMessageContent;
	}
}
