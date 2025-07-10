using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ForwarderDetailsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMasterBillPackageGroupingVisible()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var detailsControl = new ForwarderDetailsUserControlForTest())
			{
				detailsControl.OnLoadForTest();

				var documentaryDefaultsGroupBox = GetDocumentaryDefaultsGroupBox(detailsControl);
				var masterBillPackageGrouping = documentaryDefaultsGroupBox.Controls.Find("MasterBillPackageGrouping", true).FirstOrDefault() as ZGroupBox;

				AssertNull(masterBillPackageGrouping);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var detailsControl = new ForwarderDetailsUserControlForTest())
			{
				detailsControl.OnLoadForTest();

				var documentaryDefaultsGroupBox = GetDocumentaryDefaultsGroupBox(detailsControl);
				var masterBillPackageGrouping = documentaryDefaultsGroupBox.Controls.Find("MasterBillPackageGrouping", true).FirstOrDefault() as ZGroupBox;

				AssertNotNull(masterBillPackageGrouping);
			}
		}

		ZGroupBox GetDocumentaryDefaultsGroupBox(ForwarderDetailsUserControl detailsControl)
		{
			var splitContainer1 = detailsControl.Controls.Find("splitContainer1", true).FirstOrDefault() as KSplitContainer;
			AssertNotNull(splitContainer1);

			var miscDetailsLeftPanel = splitContainer1.Panel2.Controls.Find("MiscDetailsLeftPanel", true).FirstOrDefault() as ZPanel;
			AssertNotNull(miscDetailsLeftPanel);

			var forwarderInfoTabControl = miscDetailsLeftPanel.Controls.Find("ForwarderInfoTabControl", true).FirstOrDefault() as ZTabControl;
			AssertNotNull(forwarderInfoTabControl);

			var documentaryDefaultsTabPage = forwarderInfoTabControl.TabPages["DocumentaryDefaultsTabPage"] as ZTabPage;
			AssertNotNull(documentaryDefaultsTabPage);

			var documentaryDefaultsGroupBox = documentaryDefaultsTabPage.Controls.Find("DocumentaryDefaultsGroupBox", true).FirstOrDefault() as ZGroupBox;
			AssertNotNull(documentaryDefaultsGroupBox);

			return documentaryDefaultsGroupBox;
		}

		public void TestAgentDirectionAndStatusColumnsAreIncludedInGatewayAgentGrid()
		{
			using (var detailsControl = new ForwarderDetailsUserControl())
			{
				var grid = detailsControl.Controls.Find("AppointedGatewayAgentPortsGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(grid);

				foreach (var columnName in new[]
				{
					"O5_AgentDirection",
					"O5_AirAgentStatus",
					"O5_SeaAgentStatus",
					"O5_RailAgentStatus",
					"O5_RoadAgentStatus"
				})
				{
					AssertNotNull(grid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == columnName));
				}
			}
		}
	}
}
