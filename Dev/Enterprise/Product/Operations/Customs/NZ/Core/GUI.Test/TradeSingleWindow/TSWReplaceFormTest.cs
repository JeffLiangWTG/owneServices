using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWReplaceForm))]
	public class TSWReplaceFormTest : ZFormBasherTest
	{
		public void TestCancel_ButtonClick()
		{
			using (TSWReplaceForm form = new TSWReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				form.Show();
				form.Cancel_Button.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}

		public void TestFormVerb()
		{
			using (TSWReplaceForm form = new TSWReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				form.Show();
				AssertEquals("Trade Single Window -", form.FormVerb);
			}
		}

		public virtual void TestFormCaptions()
		{
			using (TSWReplaceForm form = new TSWReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory)))
			{
				AssertEquals("Replace TSW Entry", form.FormCaption);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			return new TSWReplaceForm(new AdditionalMessageInformation(TSWTransactionTypes.Replace, Factory));
		}
		#endregion
	}
}
