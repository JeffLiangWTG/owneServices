using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(CRECancelForm))]
	public class CRECancelFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (CRECancelForm form = new CRECancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory)))
			{
				AssertEquals("CRE Cancel Msg Sending Form", "Cancel ICR/CRE", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CRECancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			return form;
		}
		#endregion
	}
}
