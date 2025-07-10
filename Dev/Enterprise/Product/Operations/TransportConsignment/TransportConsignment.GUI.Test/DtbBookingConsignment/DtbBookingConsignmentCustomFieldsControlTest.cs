using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Layout;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DtbBookingConsignmentCustomFieldsControlTest : TestCaseWithFactory
	{
		#region TestWorkflowCustomFields

		public void TestWorkflowCustomFields()
		{
			var template = Helper.CreateWorkflowTemplate(WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode);
			Helper.AddCustomField(template, "stringField", AddOnColumnDataType.Codes.String);
			Helper.AddCustomField(template, "intField", AddOnColumnDataType.Codes.Integer);
			Helper.AddCustomField(template, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			Helper.AddCustomField(template, "boolField", AddOnColumnDataType.Codes.Boolean);

			Factory.Save();

			var consignment = Helper.CreateBookingConsignment();

			using (var form = new ZForm(consignment))
			{
				using (var control = new DtbBookingConsignmentCustomFieldsControlForTest())
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

		class DtbBookingConsignmentCustomFieldsControlForTest : DtbBookingConsignmentCustomFieldsControl
		{
			public ProcessTemplateCustomFieldsControl GetProcessTemplateCustomFieldsControl()
			{
				return processTemplateCustomFieldsControl1;
			}
		}

		#endregion

		#region Implementation

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
