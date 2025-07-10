using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWReplaceFormWithoutAttachments))]
	sealed class TSWReplaceFormWithoutAttachmentsWithoutAttachmentsTest : ZFormBasherTest
	{
		public void TestCancel_ButtonClick()
		{
			using (var form = new TSWReplaceFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestFormVerb()
		{
			using (var form = new TSWReplaceFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				form.Show();
				AssertEquals("Trade Single Window -", form.FormVerb);
			}
		}

		public void TestFormCaptions()
		{
			using (var form = new TSWReplaceFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("Replace TSW Entry", form.FormCaption);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			return new TSWReplaceFormWithoutAttachments(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
		}
		#endregion
	}
}
