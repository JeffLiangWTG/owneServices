using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceHeaderCustomFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldsDisplayControl()
		{
			using (var control = new InvoiceHeaderCustomFieldsUserControl())
			{
				AssertEquals("To make use of this tab, please setup commercial invoice custom fields in Workflow Manager.", ((ProcessTemplateCustomFieldsControl)control.Controls.Find("InvCustomFieldsDisplayControl", true)[0]).NothingSetupMessageLabelText);
			}
		}
	}
}
