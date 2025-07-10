using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class StandaloneConsolModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestIsModuleButtonGrid()
		{
			Assert(typeof(ZModuleButtonGrid).IsAssignableFrom(typeof(StandaloneConsolModuleButtonGrid)));
		}

		public void TestNewButtonIsNotShown()
		{
			using (var moduleButtonGrid = new StandaloneConsolModuleButtonGrid())
			{
				Assert(!moduleButtonGrid.ShowNewButton);
			}
		}

		public void TestEditButtonIsNotShown()
		{
			using (var moduleButtonGrid = new StandaloneConsolModuleButtonGrid())
			{
				Assert(!moduleButtonGrid.ShowEditButton);
			}
		}

		public void TestGridContainsEssentialColumns()
		{
			var essentialColumnNames = new string[]
			{
				JobConsolSchema.Constants.JK_UniqueConsignRef,
				JobConsolSchema.Constants.JK_TransportMode,
				JobConsolSchema.Constants.JK_ConsolMode,
				JobConsolSchema.Constants.JK_MasterBillNum,
				JobConsolSchema.Constants.JK_RL_NKLoadPort,
				JobConsolSchema.Constants.JK_RL_NKDischargePort,
				ForwardingConsol.Schema.JK_JX_JA_E_DEP,
				ForwardingConsol.Schema.JK_JX_JB_E_ARV,
				ForwardingConsol.Schema.JK_JX_JV_VoyageFlight
			};

			using (var moduleButtonGrid = new StandaloneConsolModuleButtonGrid())
			{
				var allColumnNames = moduleButtonGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(column => column.ColumnName);
				AssertContainsExactElementsInAnyOrder(essentialColumnNames, allColumnNames.Intersect(essentialColumnNames));
			}
		}

		public void TestShouldContainOverallPartyLocationCommodityComplianceRisk()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var grid = new StandaloneConsolModuleButtonGrid())
			{
				AssertNotNull("Column: Job Compliance Status should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "OverallComplianceRisk"));
				AssertNotNull("Column: Party Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "PartyComplianceRisk"));
				AssertNotNull("Column: Location Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "LocationComplianceRisk"));
				AssertNotNull("Column: Commodity Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "CommodityComplianceRisk"));
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var grid = new StandaloneConsolModuleButtonGrid())
			{
				AssertNull("Column: Job Compliance Status should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "OverallComplianceRisk"));
				AssertNull("Column: Party Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "PartyComplianceRisk"));
				AssertNull("Column: Location Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "LocationComplianceRisk"));
				AssertNull("Column: Commodity Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "CommodityComplianceRisk"));
			}
		}
	}
}
