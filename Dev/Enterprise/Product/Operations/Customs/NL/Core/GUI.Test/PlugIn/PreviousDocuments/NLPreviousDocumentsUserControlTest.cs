using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

public class NLPreviousDocumentsUserControlTest : TestCaseWithFactory
{
	class PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestColumnCount()
		{
			using (var control = new NLPreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				AssertEquals("Count", 3, grid.ColumnStyles.Count);
			}
		}

		public void TestColumnVisibility()
		{
			using (var control = new NLPreviousDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				Assert("The type should be visible.", FindColumnByName(grid, "CSI_Code").IsVisible);
				AssertNull("The class should not be visible.", FindColumnByName(grid, "CSI_SubType"));
				Assert("The reference should be visible.", FindColumnByName(grid, "CSI_ReferenceNumber").IsVisible);
				AssertNull("The date of issue should not be visible.", FindColumnByName(grid, "CSI_DateOfIssue"));
				Assert("The line number should be visible.", FindColumnByName(grid, "CSI_LineNo").IsVisible);
			}
		}

		public void TestControls_PrevDocsGroupBox()
		{
			using (var control = new NLPreviousDocumentsUserControl())
			{
				var prevDocsGroupBox = control.FindSingle<ZGroupBox>("PrevDocsGroupBox");
				AssertEquals("[UCC 2/1] Previous documents", prevDocsGroupBox.CaptionResourceString.Caption);
			}
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName) => grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
	}
}
