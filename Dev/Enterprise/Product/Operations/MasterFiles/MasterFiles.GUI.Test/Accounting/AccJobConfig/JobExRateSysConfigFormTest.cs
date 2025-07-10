using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(JobExRateSysConfigForm))]
	sealed class JobExRateSysConfigFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var accCFXcollection = new AccExchangeRateConfigurationCollection(Factory);
			return new JobExRateSysConfigForm(accCFXcollection);
		}

		public void TestJobBillExchRateConfigForm_WhenOpenForm_ButtonNewIsDisabled()
		{
			TestCaseHelper.ClearTable(AccJobConfigPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccExchangeRateConfigurationViewSchema.Constants.TableName);

			var configurationCollection = new AccExchangeRateConfigurationCollection(Factory);
			var configuration = Factory.NewWithValidTestData<AccExchangeRateConfiguration>();
			configurationCollection.Add(configuration);

			using (var form = new JobExRateSysConfigForm(configurationCollection))
			{
				AssertNotNull("Form should not be null when open it", form);
				AssertEquals("ODisplayMode of the form should be Browse", ODisplayMode.Browse, form.DisplayMode);
				AssertEquals("Check Whether this button is the button 'new' on this form", form.CommandButtonApply.Text, "&New");
				Assert("The button 'new' should not be visible when the form is initialized", !form.CommandButtonApply.Visible);

				form.DisplayMode = ODisplayMode.Edit;
				AssertEquals("Check Whether this button is the button 'Save' on this form When modifying the data", form.CommandButtonApply.Text, "&Save");
				form.FireSaveButton();
				AssertEquals("Check Whether this button is the button 'new' after saving", form.CommandButtonApply.Text, "&New");
				Assert("The button 'new' should not be visible when the form is changed and saved", !form.CommandButtonApply.Visible);
			}
		}
	}
}
