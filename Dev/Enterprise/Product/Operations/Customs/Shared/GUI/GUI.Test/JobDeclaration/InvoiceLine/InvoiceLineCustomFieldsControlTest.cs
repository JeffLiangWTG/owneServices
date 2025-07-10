using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class InvoiceLineCustomFieldsControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldsDisplayControl()
		{
			using (var control = new InvoiceLineCustomFieldsControl())
			{
				AssertEquals("To make use of this tab, please setup Commercial Invoice Lines custom fields in Workflow Manager", ((ProcessTemplateCustomFieldsControl)control.Controls.Find("CustomFieldsControl", true)[0]).NothingSetupMessageLabelText);
			}
		}
	}
}
