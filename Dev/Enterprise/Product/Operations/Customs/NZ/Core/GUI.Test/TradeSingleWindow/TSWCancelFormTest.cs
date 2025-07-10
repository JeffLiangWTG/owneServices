using System.Windows.Forms;
using Enterprise.Customs.NZ.Business.TradeSingleWindow;
using Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.TradeSingleWindow.Testing
{
	[TestedType(typeof(TSWCancelForm))]
	public class TSWCancelFormTest : BasePromptFormTest
	{
		#region Overrides
		public override void TestFormCaptions()
		{
			using (TSWCancelForm form = new TSWCancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory)))
			{
				AssertEquals("IM1 Cancel Msg Sending Form", "Cancel TSW Entry", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new TSWCancelForm(new AdditionalMessageInformation(TSWTransactionTypes.Cancel, Factory));
			MissingResourceStringChecker.ExcludeFromTest(form.ChangeCancelReasonTextBox);
			return form;
		}
		#endregion
	}
}
