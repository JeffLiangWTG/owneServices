using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWSendFormWithoutAttachments))]
	sealed class TSWSendFormWithoutAttachmentsTest : ZFormBasherTest
	{
		public void TestCancel_ButtonClick()
		{
			using (var form = new TSWSendFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestOKButtonClick()
		{
			var additionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			using (var form = new TSWSendFormWithoutAttachments(additionalMessageInformation))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}

			additionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory);
			using (var form = new TSWSendFormWithoutAttachments(additionalMessageInformation))
			{
				form.Show();
				additionalMessageInformation.AM_AdditionalStatementText = string.Empty;
				form.OKButton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			additionalMessageInformation = new AdditionalMessageInformation(null, null, TSWTransactionTypes.Completion, Factory, "");
			using (var form = new TSWSendFormWithoutAttachments(additionalMessageInformation))
			{
				form.Show();
				additionalMessageInformation.AM_OverrideText = string.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.OKButton.PerformClick();
				AssertEquals("FormResult", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestFormVerb()
		{
			using (var form = new TSWSendFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				form.Show();
				AssertEquals("Trade Single Window -", form.FormVerb);
			}
		}

		public void TestFormCaptions()
		{
			using (var form = new TSWSendFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				AssertEquals("Send", form.FormCaption);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			return new TSWSendFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory));
		}
		#endregion
	}
}
