using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Layout;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class CartageCustomFieldsControlTest : TestCaseWithFactory
	{
		public void TestWorkflowCustomFields()
		{
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var cartage = Factory.New<CommonCartage>();
			using (var form = new ZForm(cartage))
			{
				using (var control = new CartageCustomFieldsControlForTest())
				{
					form.Controls.Add(control);
					control.Dock = DockStyle.Fill;
					form.Show();
					var customFieldsControl = control.GetProcessTemplateCustomFieldsControl();
					AssertEquals("Ensure has tests from control sub class CustomPropertiesControl", true, customFieldsControl.GetType().IsSubclassOf(typeof(CustomPropertiesControl)));
					var rowLayoutPanel = (RowLayoutPanel)customFieldsControl.Controls["rowLayoutPanel"];
					AssertEquals(4, rowLayoutPanel.Controls.Count);
				}
			}
		}

		class CartageCustomFieldsControlForTest : CartageCustomFieldsControl
		{
			public ProcessTemplateCustomFieldsControl GetProcessTemplateCustomFieldsControl()
			{
				return processTemplateCustomFieldsControl1;
			}
		}

		protected virtual string WorkflowDescriptorCode
		{
			get
			{
				return JobInvoicingConsumerTypes.LocalCartage.Code;
			}
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
