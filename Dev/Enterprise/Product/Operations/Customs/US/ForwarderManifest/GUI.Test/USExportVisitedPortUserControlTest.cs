using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	public class USExportVisitedPortUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new USExportVisitedPortUserControl())
			{
				var grid = userControl.FindSingle<ZGrid>("visitedPortsForManifestHeaderUserControlGrid");
				AssertNotNull(grid);
				var codeColumn = grid.GetColumnStyle("CY_Code");
				AssertType<ZMultiControlColumnStyleInfo>(codeColumn);
				AssertEquals("PortCodeFieldType", ((ZMultiControlColumnStyleInfo)codeColumn).FieldTypeColumnName);
			}
		}
	}
}
