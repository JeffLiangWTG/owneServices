using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class CustomsNumberViewStmNumsUserControlTest : ZFormBasherTest
	{
		public void TestColumns()
		{
			using (var formForTesting = new ZForm(GlbCompany.CurrentCompany))
			using (var control = new CustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers))
			{
				formForTesting.Controls.Add(control);
				formForTesting.Show();
				var grid = (ZGrid)control.Controls.Find("NumberRangesGrid", true).First();
				AssertEquals(160, FindColumnByName(grid, CustomsNumberViewStmNumsWrapper.Schema.SN_FountainName).Width);
			}
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
		{
			return grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == columnName);
		}

		SGCustomsNumberViewStmNumsCompanyProvider Provider => (SGCustomsNumberViewStmNumsCompanyProvider)GlbCompany.CurrentCompany.CustomsNumberProvider;

		protected override Form GetFormToBashCore()
		{
			var form = new ZChildForm(GlbCompany.CurrentCompany);
			form.CaptionRenderingEnabled = true;
			form.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 712);
			var control = new CustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers);
			control.Name = "SGCustomsNumberViewStmNumsUserControl";
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			form.SetDataBinding(GlbCompany.CurrentCompany, "");
			return form;
		}
	}
}
