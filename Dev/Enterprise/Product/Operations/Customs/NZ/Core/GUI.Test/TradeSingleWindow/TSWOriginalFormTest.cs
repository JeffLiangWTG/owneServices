using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWOriginalForm))]
	public class TSWOriginalFormTest : ZFormBasherTest
	{
		public void TestCancel_ButtonClick()
		{
			using (TSWOriginalForm form = new TSWOriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
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
			using (TSWOriginalForm form = new TSWOriginalForm(additionalMessageInformation))
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
		}

		public void TestFormVerb()
		{
			using (TSWOriginalForm form = new TSWOriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory)))
			{
				form.Show();
				AssertEquals("Trade Single Window - ", form.FormVerb);
			}
		}

		#region Overrides
		protected override Form GetFormToBashCore()
		{
			var form = new TSWOriginalForm(new AdditionalMessageInformation(TSWTransactionTypes.Original, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.AdditionalInformationGroupBox);
			return form;
		}
		#endregion
	}
}
