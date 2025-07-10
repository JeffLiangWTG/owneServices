using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccOrgTaxConfigurationTemplateForm))]
	class AccOrgTaxConfigurationTemplateFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				var template = form.BusinessEntity as AccOrgTaxConfigurationTemplate;
				AssertEquals("Precondition", true, template.OCT_IsReceivable);
				AssertEquals("Receivables Organizations Template", form.FormCaption);

				template.OCT_IsReceivable = false;
				AssertEquals("Payables Organizations Template", form.FormCaption);
			}
		}

		public void TestResetStatus()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();
			using (var form = new AccOrgTaxConfigurationTemplateForm(template))
			{
				form.Show();
				var targetControl = form.Find(x => x.Name == "FilterLinkedOrganizationsControl").Cast<FilterLinkedOrganizationsControl>().Single();

				var targetTabPageControl = form.Controls.Find("TaxConfigurationTemplateTabControl", true).Cast<ZTemplateTabControl>().Single();
				var targetTabPage = form.Controls.Find("LinkedOrganizationsTabPage", true).Cast<ZTabPage>().Single();
				targetTabPageControl.SelectTab(targetTabPage);

				var filterControl = targetControl.Find(x => x.Name == "LinkedOrganizationsFilterControl").Cast<LinkedOrganizationsFilterControl>().Single();
				var label = filterControl.Find(x => x.Name == "ToolStripRecordsFoundLabel").Cast<ZLabel>().Single();

				label.Text = "TEST01";
				AssertEquals("Pre-condition", "TEST01", label.Text);

				form.FireSaveButton();

				AssertEquals("Records found label is cleared once saved", "Found record(s): 0 of 0", label.Text.Trim());
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var template = Factory.New<AccOrgTaxConfigurationTemplate>();
			return new AccOrgTaxConfigurationTemplateForm(template);
		}

		#endregion
	}
}
