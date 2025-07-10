using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ForwardingShipmentModuleButtonGridTest : TestCaseWithFactory
	{
		public void TestIsModuleButtonGrid()
		{
			Assert(typeof(ZModuleButtonGrid).IsAssignableFrom(typeof(ForwardingShipmentModuleButtonGrid)));
		}

		public void TestShipmentAndConsolModuleButtonGridsHaveDifferentColumns()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var shipmentGrid = new ShipmentModuleButtonGrid())
			using (var consolShipmentGrid = new ConsolShipmentModuleButtonGrid())
			{
				var shipmentGridColumnNames = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(col => col.ColumnName).ToList();
				var consolShipmentGridColumnNames = consolShipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(col => col.ColumnName).ToList();
				AssertEquals(155, shipmentGridColumnNames.Count);

				AssertEquals(157, consolShipmentGridColumnNames.Count);

				foreach (string columnName in shipmentGridColumnNames)
				{
					AssertEquals(true, consolShipmentGridColumnNames.Contains(columnName));
				}

				AssertEquals(false, shipmentGridColumnNames.Contains("JS_JS_ColoadMasterShipmentForBinding"));
				AssertEquals(true, consolShipmentGridColumnNames.Contains("JS_JS_ColoadMasterShipmentForBinding"));
				AssertEquals(false, shipmentGridColumnNames.Contains("BKGNumber"));
				AssertEquals(true, consolShipmentGridColumnNames.Contains("BKGNumber"));
			}
		}

		public void TestShipmentAndConsolModuleButtonGridsHaveTheSameColumnCaptions()
		{
			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				List<string> listUnmatched = new List<string>();

				foreach (ZGridColumnInfo columnStyleInfo in buttonGrid.ColumnStyles)
				{
					var shipmentGridCaption = GetCaption(nameof(ShipmentModuleButtonGrid), columnStyleInfo);
					var consolShipmentGridCaption = GetCaption(nameof(ConsolShipmentModuleButtonGrid), columnStyleInfo);

					if (shipmentGridCaption != consolShipmentGridCaption)
					{
						listUnmatched.Add(String.Format("[ColumnName]: {0}, [Shipment] {1}, [Consol] {2",
							columnStyleInfo.ColumnName, shipmentGridCaption, consolShipmentGridCaption));
					}
				}

				AssertEquals(String.Join(System.Environment.NewLine, listUnmatched.ToArray()), 0, listUnmatched.Count);
			}
		}

		public void TestAdditionalReferenceColumn()
		{
			using (var shipmentGrid = new ShipmentModuleButtonGrid())
			{
				Assert("Additional Reference column should exist", shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(col => col.ColumnName == "NumbersAsString"));
			}
		}

		string GetCaption(string controlName, ZGridColumnInfo columnInfo)
		{
			return DataBoundResourceStrings.GetColumnDescriptiveName(JobShipmentSchema.Constants.TableName, columnInfo.ColumnName);
		}

		public void TestSetupColumns()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Nadawca");

			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var columnInfo = FindColumnByName(buttonGrid, CommonShipment.Schema.ConsignorNameOrPK);

				AssertNotNull("ConsignorNameOrPK column is missing", columnInfo);
				AssertEquals("ConsignorNameOrPK column caption", "Nadawca", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, CommonShipment.Schema.JS_Calc_ConsignorCompanyName);

				AssertNotNull("JS_Calc_ConsignorCompanyName column is missing", columnInfo);
				AssertEquals("JS_Calc_ConsignorCompanyName column caption", "Nadawca Full Name", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, "JS_Calc_EstimatedExportClearanceDate");
				AssertNotNull("JS_Calc_EstimatedExportClearanceDate column is missing", columnInfo);
				AssertEquals("JS_Calc_EstimatedExportClearanceDate column caption", "Estimated Export Clearance Date", columnInfo.CaptionResourceString.Caption);
				AssertEquals("JS_Calc_EstimatedExportClearanceDate column default visible is false", false, columnInfo.IsVisible);

				columnInfo = FindColumnByName(buttonGrid, "JS_EFreightStatus");
				AssertNotNull("JS_EFreightStatus column is missing", columnInfo);
				AssertEquals("JS_EFreightStatus column caption", "e-freight Status", columnInfo.CaptionResourceString.Caption);
				AssertEquals("JS_EFreightStatus column default visible is false", false, columnInfo.IsVisible);
				AssertEquals("JS_EFreightStatus column is not readonly", false, columnInfo.IsReadOnly);
			}
		}
		public void TestSetupColumns_CustomFields()
		{
			FreightDataRegistry.Instance.ShipmentCustomText1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Custom 1", "cusom field 1"));
			FreightDataRegistry.Instance.ShipmentCustomDate2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Date 2", "cusom date 2"));
			FreightDataRegistry.Instance.ShipmentCustomFlag1.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Bool 1", "cusom flag 1"));
			FreightDataRegistry.Instance.ShipmentCustomDecimalNo2.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CaptionAndHint("Decimal 2", "cusom decimal 2"));

			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SHP";
			template1.P0_SubType1 = "ROA";
			template1.P0_SubType2 = "DOM";

			var def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUMEL";
			shipment.JS_TransportMode = "ROA";

			var collection = new ForwardingShipmentCollection(Factory) { shipment };

			Factory.Save();

			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid(collection))
			{
				var columnInfo = FindColumnByName(buttonGrid, nameof(ForwardingShipment.DocsAndCartage) + "+" + AutoJobDocsAndCartage.Schema.JP_CustomAttrib1);
				AssertNotNull(columnInfo);
				AssertEquals("Custom 1", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, nameof(ForwardingShipment.DocsAndCartage) + "+" + AutoJobDocsAndCartage.Schema.JP_CustomDate2);
				AssertNotNull(columnInfo);
				AssertEquals("Date 2", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, nameof(ForwardingShipment.DocsAndCartage) + "+" + AutoJobDocsAndCartage.Schema.JP_CustomFlag1);
				AssertNotNull(columnInfo);
				AssertEquals("Bool 1", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, nameof(ForwardingShipment.DocsAndCartage) + "+" + AutoJobDocsAndCartage.Schema.JP_CustomDecimal2);
				AssertNotNull(columnInfo);
				AssertEquals("Decimal 2", columnInfo.Caption);

				columnInfo = FindColumnByName(buttonGrid, "__C11__prop__ZString");
				AssertNotNull(columnInfo);
				AssertEquals("C11", columnInfo.Caption);
			}
		}

		public void TestSetupColumns_TaxBranch()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var columnInfo = FindColumnByName(buttonGrid, "ShipmentJobHeader+JH_GB_TaxBranch");
				AssertNotNull("JH_GB_TaxBranch column is missing", columnInfo);
				AssertEquals("JH_GB_TaxBranch column caption", "Tax Branch", columnInfo.CaptionResourceString.Caption);
				AssertEquals("JH_GB_TaxBranch column default visible is false", false, columnInfo.IsVisible);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var columnInfo = FindColumnByName(buttonGrid, "ShipmentJobHeader+JH_GB_TaxBranch");
				AssertNull("Didn't contain JH_GB_TaxBranch column when EnableTaxBranchReporting is false.", columnInfo);
			}
		}

		public void TestDisplayCTStatusColumn_WhenIsCountryEuOrCtCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Denmark))
			using (var forwardingShipmentModuleButtonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var zDropEditColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNotNull("If country is part of EU, CT Status column should be available", zDropEditColumn);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Switzerland))
			using (var forwardingShipmentModuleButtonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var zDropEditColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNotNull("If country is part of Common Transit not included in EU, CT Status column should be available", zDropEditColumn);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			using (var forwardingShipmentModuleButtonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var zDropEditColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNull("If country is not part of EU nor Common Transit countries, CT Status column should not be available", zDropEditColumn);
			}
		}

		public void TestLastKnownTransitWarehouseStatusColumn()
		{
			using (var forwardingShipmentModuleButtonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var zDropEditColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZTextBoxColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_Calc_LastKnownTransitWarehouseStatus");

				AssertNotNull("Last Known Transit Warehouse Status column should be available", zDropEditColumn);
			}
		}

		public void TestShouldContainOverallPartyLocationCommodityAssessmentComplianceRisk()
		{
			using (var grid = new ForwardingShipmentModuleButtonGrid())
			{
				AssertNull("Column: Job Compliance Status should not exists.", FindColumnByName(grid, "OverallComplianceRisk"));
				AssertNull("Column: Party Compliance Risk should not exists.", FindColumnByName(grid, "PartyComplianceRisk"));
				AssertNull("Column: Location Compliance Risk should not exists.", FindColumnByName(grid, "LocationComplianceRisk"));
				AssertNull("Column: Commodity Compliance Risk should not exists.", FindColumnByName(grid, "CommodityComplianceRisk"));
			}

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var grid = new ForwardingShipmentModuleButtonGrid())
			{
				AssertNotNull("Column: Job Compliance Status should exists.", FindColumnByName(grid, "OverallComplianceRisk"));
				AssertNotNull("Column: Party Compliance Risk should exists.", FindColumnByName(grid, "PartyComplianceRisk"));
				AssertNotNull("Column: Location Compliance Risk should exists.", FindColumnByName(grid, "LocationComplianceRisk"));
				AssertNotNull("Column: Commodity Compliance Risk should exists.", FindColumnByName(grid, "CommodityComplianceRisk"));
			}
		}

		public void TestPickupDeliveryDropModeColumns()
		{
			using (var forwardingShipmentModuleButtonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var pickupDropModeColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "DocsAndCartage+JP_FCLPickupEquipmentNeeded");

				AssertNotNull("Pickup Drop Mode column should be available", pickupDropModeColumn);

				var deliveryDropModeColumn = forwardingShipmentModuleButtonGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "DocsAndCartage+JP_FCLDeliveryEquipmentNeeded");

				AssertNotNull("Delivery Drop Mode column should be available", deliveryDropModeColumn);
			}
		}

		public void TestCO2eColumnExistInShipmentGridWhenGreenhouseGasEmissionCalculationEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var columnInfo = FindColumnByName(buttonGrid, "TotalCO2eForSorting");
				AssertNotNull("TotalCO2e column should exist", columnInfo);
				AssertEquals("TotalCO2e column caption", "CO2e (kg)", columnInfo.CaptionResourceString.Caption);
				AssertEquals("TotalCO2e column default visibility is false", false, columnInfo.IsVisible);
				AssertEquals("TotalCO2e column is readonly", true, columnInfo.IsReadOnly);
				AssertEquals("TotalCO2e column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			using (var buttonGrid = new ForwardingShipmentModuleButtonGrid())
			{
				var columnInfo = FindColumnByName(buttonGrid, "TotalCO2eForSorting");
				AssertNull("TotalCO2e column should not exist", columnInfo);
			}
		}

		ZGridColumnInfo FindColumnByName(ForwardingShipmentModuleButtonGrid buttonGrid, string columnName)
		{
			return (from columnStyleInfo in buttonGrid.ColumnStyles.Cast<ZGridColumnInfo>()
					where columnStyleInfo.ColumnName == columnName
					select columnStyleInfo).FirstOrDefault();
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}
	}
}
