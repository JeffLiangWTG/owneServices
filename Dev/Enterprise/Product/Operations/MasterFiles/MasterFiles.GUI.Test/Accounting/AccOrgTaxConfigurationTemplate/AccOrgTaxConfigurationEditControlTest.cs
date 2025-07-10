using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	class AccOrgTaxConfigurationEditControlTest : TestCaseWithFactory
	{
		public void TestTaxConfigurationGridColumnCaptions()
		{
			using (var userControl = new AccOrgTaxConfigurationEditControl())
			{
				var grid = userControl.Controls.Find("taxConfigGrid", true).Single() as ZGrid;

				AssertEquals(10, grid.ColumnStyles.Count);
				AssertArrayEqualsByElements(
					new string[] { "Tax Code", "Tax Description", "Active", "Threshold is Used", "Recover Tax", "Super Type", "Tax System", "Tax Authority", "Ledger", "Tax System Branch" },
					grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.CaptionResourceString.Caption).ToArray());
			}
		}

		[RequiresSTA]
		public void TestRecoverTaxColumn()
		{
			var template = Factory.NewWithValidTestData<AccOrgTaxConfigurationTemplate>();

			TestTaxConfigurationGridColumnCaptions(true);
			TestTaxConfigurationGridColumnCaptions(false);

			void TestTaxConfigurationGridColumnCaptions(bool isReceivable)
			{
				template.OCT_IsReceivable = isReceivable;
				Factory.Save();

				using (var form = new ZForm(template))
				using (var control = new AccOrgTaxConfigurationEditControl())
				{
					form.Controls.Add(control);
					form.Show();
					var grid = control.Controls.Find("taxConfigGrid", true).Single() as ZGrid;
					AssertEquals(!isReceivable, grid.GetColumnStyle(AccOrgTaxConfigurationSchema.OTC_RecoverTax.Name).IsUnavailable);
				}
			}
		}
	}
}
