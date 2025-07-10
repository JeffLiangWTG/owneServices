using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.NL.GUI.Testing;

class AdditionalDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestUserControls()
	{
		using (var userControl = new AdditionalDocumentsUserControl())
		{
			AssertNotNull("The kind drop box should exist.", userControl.Controls.Find("KindDropBox", true).Single());
			Assert("The kind drop box should be visible", userControl.Controls.Find("KindDropBox", true).Single().Visible);

			AssertNotNull("The type drop box should exist.", userControl.Controls.Find("AddInfoTypeCodeDropEdit", true).Single());
			Assert("The type drop box should be visible", userControl.Controls.Find("AddInfoTypeCodeDropEdit", true).Single().Visible);

			AssertNotNull("The reference text box should exist.", userControl.Controls.Find("ReferenceTextBox", true).Single());
			Assert("The reference text box should be visible", userControl.Controls.Find("ReferenceTextBox", true).Single().Visible);

			AssertNotNull("The description text box should exist.", userControl.Controls.Find("AddiInfoDescriptionTextBox", true).Single());
			Assert("The description text box should be visible", userControl.Controls.Find("AddiInfoDescriptionTextBox", true).Single().Visible);
		}
	}

	public void TestAdditionalDocumentsGrid()
	{
		using (var userControl = new AdditionalDocumentsUserControl())
		{
			var additionalDocumentsGrid = (ZGrid)userControl.Controls.Find("AdditionalInfosGrid", true).Single();
			AssertNotNull("The grid should exist.", additionalDocumentsGrid);
			Assert("The grid should be visible.", additionalDocumentsGrid.Visible);

			AssertNotNull(FindColumnByName(additionalDocumentsGrid, "CSI_SubType"));
			AssertNotNull(FindColumnByName(additionalDocumentsGrid, "CSI_Code"));
			AssertNotNull(FindColumnByName(additionalDocumentsGrid, "CSI_ReferenceNumber"));
			AssertNotNull(FindColumnByName(additionalDocumentsGrid, "CSI_Description"));

			Assert(FindColumnByName(additionalDocumentsGrid, "CSI_SubType").IsVisible);
			Assert(FindColumnByName(additionalDocumentsGrid, "CSI_Code").IsVisible);
			Assert(FindColumnByName(additionalDocumentsGrid, "CSI_ReferenceNumber").IsVisible);
			Assert(FindColumnByName(additionalDocumentsGrid, "CSI_Description").IsVisible);
		}
	}
	ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
}
