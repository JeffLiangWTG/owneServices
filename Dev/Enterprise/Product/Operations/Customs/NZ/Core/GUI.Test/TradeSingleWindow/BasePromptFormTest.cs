using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWSendFormWithAttachments))]
	public class BasePromptFormTest : ZFormBasherTest
	{
		public void TestCancel_ButtonClick()
		{
			using (TSWSendFormWithAttachments form = new TSWSendFormWithAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestOKButtonClick()
		{
			AdditionalMessageInformation additionalMessageInformation = new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory);
			additionalMessageInformation.AM_FreeText = "OCR Message Testing";
			using (TSWSendFormWithAttachments form = new TSWSendFormWithAttachments(additionalMessageInformation))
			{
				form.Show();
				form.OKButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				((AdditionalMessageInformation)form.BusinessEntity).SupportingDocuments.AddNew();
				form.OKButton.PerformClick();
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			additionalMessageInformation = new AdditionalMessageInformation(null, Factory.New<JobDeclaration>().eDocsForSelection, TSWTransactionTypes.Completion, Factory, "");
			using (TSWSendFormWithAttachments form = new TSWSendFormWithAttachments(additionalMessageInformation))
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
			using (TSWSendFormWithAttachments form = new TSWSendFormWithAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				form.Show();
				AssertEquals("Trade Single Window -", form.FormVerb);
			}
		}

		public virtual void TestFormCaptions()
		{
			using (TSWSendFormWithAttachments form = new TSWSendFormWithAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				AssertEquals("Send", form.FormCaption);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			return new TSWSendFormWithAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory));
		}
		#endregion
	}
}
