using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.Testing.Shipment.UserControls
{
	public class ConsolDetailsControlTest : TestCaseWithFactory
	{
		public void TestShouldContainOverallPartyLocationCommodityComplianceRisk()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new ConsolDetailsControl())
			{
				var grid = (ConsolModuleButtonGrid)control.Controls.Find("ConsolModuleButtonGrid", true).First();
				AssertNotNull("ConsolModuleButtonGrid should exists.", grid);
				AssertNotNull("Column: Job Compliance Status should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "OverallComplianceRisk"));
				AssertNotNull("Column: Party Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "PartyComplianceRisk"));
				AssertNotNull("Column: Location Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName ==  "LocationComplianceRisk"));
				AssertNotNull("Column: Commodity Compliance Risk should exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "CommodityComplianceRisk"));
			}

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new ConsolDetailsControl())
			{
				var grid = (ConsolModuleButtonGrid)control.Controls.Find("ConsolModuleButtonGrid", true).First();
				AssertNotNull("ConsolModuleButtonGrid should not exists.", grid);
				AssertNull("Column: Job Compliance Status should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "OverallComplianceRisk"));
				AssertNull("Column: Party Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "PartyComplianceRisk"));
				AssertNull("Column: Location Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "LocationComplianceRisk"));
				AssertNull("Column: Commodity Compliance Risk should not exists.", grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "CommodityComplianceRisk"));
			}
		}
	}
}
