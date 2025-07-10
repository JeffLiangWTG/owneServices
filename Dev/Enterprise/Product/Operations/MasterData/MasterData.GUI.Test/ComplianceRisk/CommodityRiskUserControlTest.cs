using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Business;
using Enterprise.ComplianceRisk.GUI.Test;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test
{
	public class CommodityRiskUserControlTest : ComplianceRiskHelperTest
	{
		public void TestCommodityRiskGridColumnNames_WhenConsolidation()
		{
			AssertCommodityRiskGridColumnsWithJobType((BusinessObject)CreateNewConsolidation, new[]
			{
				"CCD_RiskStatusDescription", // Risk Status
				"NomenclatureCondition", // Nomenclature Alerts
				"SpecificCondition", // Tariff Alerts
				"CCD_HarmonizedCode", // Harmonized Code
				"CCD_Description", // Goods Description
				"CCD_RN_NKOrigin", // Origin Of Goods
				"CommoditySource", // Commodity Source
				"Source", // Job Number
				"CCD_SystemCreateTimeUtc" // Create Time Utc
			},
			true);
		}

		public void TestCommodityRiskGridColumnNames_WhenShipment()
		{
			AssertCommodityRiskGridColumnsWithJobType((BusinessObject)CreateNewShipment, DefaultGridColumnsBorderWiseIntegration);
		}

		public void TestCommodityRiskGridColumnNames_WhenQuickBooking()
		{
			AssertCommodityRiskGridColumnsWithJobType((BusinessObject)CreateNewBookingQuick, DefaultGridColumnsBorderWiseIntegration);
		}

		public void TestCommodityRiskGridColumnNames_WhenBookingWithQuote()
		{
			AssertCommodityRiskGridColumnsWithJobType((BusinessObject)CreateNewBookingWithQuote, DefaultGridColumnsBorderWiseIntegration);
		}

		public void TestCommodityRiskGridColumnNames_WhenBookingSpotQuote()
		{
			AssertCommodityRiskGridColumnsWithJobType((BusinessObject)CreateNewBookingSpotQuote, DefaultGridColumnsBorderWiseIntegration);
		}

		string[] DefaultGridColumnsBorderWiseIntegration => new[]
		{
				"CCD_RiskStatusDescription", // Risk Status
				"NomenclatureCondition", // Nomenclature Alerts
				"SpecificCondition", // Tariff Alerts
				"CCD_HarmonizedCode", // Harmonized Code
				"CCD_Description", // Goods Description
				"CCD_RN_NKOrigin", // Origin Of Goods
				"LegalBookLink", // Compliance Alerts
				"CommoditySource", // Commodity Source
				"Source", // Job Number
				"CCD_SystemCreateTimeUtc" // Create Time Utc
		};

		void AssertCommodityRiskGridColumnsWithJobType(BusinessObject hostBusinessObject, string[] expectedColumns, bool isConsolidation = false)
		{
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ZForm())
			{
				var commodityRiskUserControl = new CommodityRiskUserControl(new ComplianceRiskPlugInBusinessObject(hostBusinessObject));
				var commodityRiskGrid = commodityRiskUserControl.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				form.Controls.Add(commodityRiskGrid);
				form.Show();

				CombineAssertions("CommondityRiskGrid columns border wise API integration:", () =>
				{
					AssertEquals(expectedColumns.Length, commodityRiskGrid.ColumnStyles.Count);
					AssertContainsExactElementsInExactOrder(expectedColumns, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(a => a.ColumnName).ToList());
					AssertEquals(commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(a => a.ColumnName == "Source").IsVisible, isConsolidation);
					AssertEquals(false, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(c => c.ColumnName == "CCD_SystemCreateTimeUtc").IsVisible);
					AssertEquals(false, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(a => a.ColumnName == "NomenclatureCondition").IsVisible);
					AssertEquals(false, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(a => a.ColumnName == "SpecificCondition").IsVisible);
				});

				commodityRiskUserControl.Dispose();
				form.Close();
			}
		}

		public void TestTabStopIsFalseForCommodityRiskGrid()
		{
			using (var form = new ZForm())
			using (var commodityRiskUserControl = new CommodityRiskUserControl(new ComplianceRiskPlugInBusinessObject((BusinessObject)CreateNewShipment)))
			{
				var commodityRiskGrid = commodityRiskUserControl.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
				form.Controls.Add(commodityRiskUserControl);
				form.Show();

				AssertEquals(false, commodityRiskGrid.TabStop);
			}
		}

		public void TestCommodityRiskGridColumnImportAlertsForExportJob()
		{
			AssertCommodityRiskGridColumnImportAlertsForExportJob(true);
			AssertCommodityRiskGridColumnImportAlertsForExportJob(false);

			void AssertCommodityRiskGridColumnImportAlertsForExportJob(bool needImportAlerts)
			{
				using (var form = new ZForm())
				using (var commodityRiskUserControl = new CommodityRiskUserControl(new ComplianceRiskPlugInBusinessObject((BusinessObject)CreateNewShipment)))
				{
					var commodityRiskGrid = commodityRiskUserControl.Controls.Find("CommodityRiskGrid", searchAllChildren: true).Single() as ZGrid;
					form.Controls.Add(commodityRiskUserControl);
					form.Show();

					commodityRiskUserControl.AddOrRemoveComplianceAlertsForExportDeclaration(needImportAlerts);
					var importAlertsColumn = commodityRiskGrid.ColumnStyles.OfType<ZTextBoxColumnStyleInfo>().FirstOrDefault(c => c.ColumnName == "ImportAlertsForExportJobDescription");
					AssertEquals(needImportAlerts, importAlertsColumn != null);
				}
			}
		}
	}
}
