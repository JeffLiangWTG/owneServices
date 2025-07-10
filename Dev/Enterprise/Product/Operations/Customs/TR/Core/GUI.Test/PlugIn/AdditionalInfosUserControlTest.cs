using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.TR.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class AdditionalInfosUserControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var control = new AdditionalInfosUserControl())
			{
				var grid = control.FindSingle<ZGrid>("AdditionalInfosGrid");
				var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(x => !x.IsUnavailable).Select(x => x.ColumnName);
				AssertContainsExactElementsInAnyOrder("Only CSI_Description is required in TR.", new[] { AdditionalInfo.Schema.CSI_Description }, columns);
			}
		}

		public void TestVisibilityofAddInfoTypeCodeDropEdit()
		{
			using (var control = new AdditionalInfosUserControl())
			{
				control.Show();

				var additionalInfosGrid = (ZGrid)control.Controls.Find("AdditionalInfosGrid", true).First();
				var addInfoTypeCodeDropEdit = control.Controls.Find("AddInfoTypeCodeDropEdit", true).First();
				AssertEquals(true, additionalInfosGrid.Visible);
				AssertEquals(false, addInfoTypeCodeDropEdit.Visible);
			}
		}
	}
}
