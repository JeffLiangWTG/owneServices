using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Common.TemplateRecords.Testing
{
	sealed class TemplateRecordLabelControlTest : TestCaseWithFactory
	{
		public void TestTemplateBinding()
		{
			using (var control = new TemplateRecordLabelControl())
			{
				CombineAssertions("No template provided", () =>
				{
					AssertNoExceptionThrown(() => control.Show());
					AssertNull("Should not bind", control.BindingSource.DataSource);
					Assert(
						"If there is no template to bind to, don't show editable textbox",
						!control.Find(c => c.Name == "templateNameTextBox").Any()
					);
				});
			}

			var template = Factory.New<StmTemplateRecord>();

			using (var control = new TemplateRecordLabelControl(template))
			{
				CombineAssertions("Template provided", () =>
				{
					AssertNoExceptionThrown(() => control.Show());
					AssertEquals("Should bind to template", template, control.BindingSource.DataSource);
					Assert("Should show textbox", control.Find(c => c.Name == "templateNameTextBox").Any());
				});
			}
		}
	}
}
