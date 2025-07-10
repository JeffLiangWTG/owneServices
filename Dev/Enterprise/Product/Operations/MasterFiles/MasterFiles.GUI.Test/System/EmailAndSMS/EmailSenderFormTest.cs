using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EmailSenderForm))]
	sealed class EmailSenderFormTest : ZFormBasherTest
	{
		public void TestSendPerformsPreSendCheckerOnAllRecipients()
		{
			var source = Factory.New<OrgHeader>();
			GlbStaff.CurrentUser.GS_EmailAddress = "from@email.com";
			var senderConfiguration = new EmailSenderConfiguration(source);
			senderConfiguration.HtmlEmail.FromEmailAddress = "from@email.com";
			senderConfiguration.HtmlEmail.ToEmailAddress = "to.1@email.com; to.2@email.com";
			senderConfiguration.HtmlEmail.Cc = "cc.1@email.com;cc.2@email.com";
			senderConfiguration.HtmlEmail.Bcc = "bcc.1@email.com; bcc.2@email.com";
			senderConfiguration.HtmlEmail.Body = "My Body";

			IEnumerable<ZString> actuals = Array.Empty<ZString>();
			var mockPreSendChecker = new Mock<IEmailPreSendChecker>();
			mockPreSendChecker
				.Setup(m => m.PromptUserIfSendingToNdrRecipients(It.IsAny<IEnumerable<ZString>>()))
				.Callback<IEnumerable<ZString>>(x => actuals = x)
				.Returns(true);

			using (ObjectFactory.Substitute(mockPreSendChecker.Object))
			using (var form = new EmailSenderForm(senderConfiguration))
			{
				form.Show();
				form.SendButton.PerformClick();

				var expectedEmailsToCheck = new ZString[]
				{
					"to.1@email.com",
					"to.2@email.com",
					"cc.1@email.com",
					"cc.2@email.com",
					"bcc.1@email.com",
					"bcc.2@email.com"
				};

				AssertContainsExactElementsInAnyOrder(expectedEmailsToCheck, actuals);
			}
		}

		public void TestPreviewAndSendButtonWithErrorBody()
		{
			var source = Factory.New<OrgHeader>();
			GlbStaff.CurrentUser.GS_EmailAddress = "from@email.com";
			var senderConfiguration = new EmailSenderConfiguration(source);
			senderConfiguration.HtmlEmail.FromEmailAddress = "from@email.com";
			senderConfiguration.HtmlEmail.ToEmailAddress = "to.1@email.com; to.2@email.com";
			senderConfiguration.HtmlEmail.Cc = "cc.1@email.com;cc.2@email.com";
			senderConfiguration.HtmlEmail.Body = "(*ABC*";

			using (var form = new EmailSenderForm(senderConfiguration))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();
				form.SendButton.PerformClick();

				Assert(senderConfiguration.HtmlEmail.ShouldCheckFormatOfEmailBody);
				AssertHasError(senderConfiguration.HtmlEmail.BodyInfo, "The attached document is malformed, a start tag (* should always be followed by an end tag *)");

				var previewButton = form.Controls.Find("PreviewButton", true)[0] as ZButton;
				previewButton.PerformClick();
				AssertEquals("The attached document is malformed, a start tag (* should always be followed by an end tag *)", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ISendEmailSource sup = Factory.New<OrgHeader>();
			EmailSenderConfiguration senderConfiguration = new EmailSenderConfiguration(sup);

			return new EmailSenderForm(senderConfiguration);
		}
	}
}
