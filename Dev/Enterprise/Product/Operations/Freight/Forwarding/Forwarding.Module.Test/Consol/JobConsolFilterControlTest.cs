using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobConsolFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestWorkflowFilterStripIsInherited()
		{
			ConsolCollection consols = Factory.New<ForwardingShipment>().Consols;
			JobConsolFilterBusinessObject filterBO = new JobConsolFilterBusinessObject();
			using (ZForm form = new ZForm())
			{
				JobConsolFilterControl filterControl = new JobConsolFilterControl(consols, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				filterControl.AddNewFilterStrip();
				AssertEquals("Must return JobConsolModuleStrip so that workflow filter strips may be selected",
					typeof(JobConsolModuleStrip), filterControl.LastFilterStripType);
				AssertEquals(
					"JobConsolModuleStrip must inherit ZFilterStripControl<WorkflowFilterStripWithRoutingSupport> so that workflow filter strips may be selected",
					true, typeof(JobConsolModuleStrip).IsSubclassOf(typeof(WorkflowFilterStripWithRoutingSupport)));
			}
		}

		[RequiresSTA]
		public void TestScreeningStatusColumn()
		{
			AssertScreeningStatusColumn(enableComplianceRisk: false);
			AssertScreeningStatusColumn(enableComplianceRisk: true);

			void AssertScreeningStatusColumn(bool enableComplianceRisk)
			{
				var consolHasImplementedIComplianceRiskStatusProvider = typeof(IComplianceItemRiskStatusProvider).IsAssignableFrom(typeof(ForwardingConsol));

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				using (var filterControl = new JobConsolFilterControl(new ConsolCollection(Factory.New<CommonShipment>()), new JobShipmentFilterBusinessObject()))
				{
					var screeningStatusColumnStyle = filterControl.FilteredGrid.GetColumnStyle("JK_ScreeningStatus");

					AssertNotNull(screeningStatusColumnStyle);
				}
			}
		}

		[RequiresSTA]
		public void TestColumnsWithUnits_ShouldNotOverriteDefaultDecimals()
		{
			ConsolCollection consols = Factory.New<ForwardingShipment>().Consols;
			JobConsolFilterBusinessObject filterBO = new JobConsolFilterBusinessObject();

			using (JobConsolFilterControl filterControl = new JobConsolFilterControl(consols, filterBO))
			{
				CombineAssertions(() =>
				{
					foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
					{
						if
						(
							column.ColumnName == "JK_TotalShipmentWeight"
							|| column.ColumnName == "JK_TotalShipmentVolume"
							|| column.ColumnName == "JK_TotalShipmentChargeable"
							|| column.ColumnName == "JK_CorrectedConsolWeight"
							|| column.ColumnName == "JK_CorrectedConsolVolume"
							|| column.ColumnName == "JK_ConsolChargeable"
						)
						{
							var calcColumn = (ZArchitecture.ZCalcEditColumnStyleInfo)column;
							bool decimalsOverridden = (bool)calcColumn.GetType().GetProperty("DecimalsOverridden", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(calcColumn, null);

							AssertEquals("Decimals should not be overridden", false, decimalsOverridden);
						}
					}
				});
			}
		}

		[NUnit.Framework.ExpectNoExceptions]
		[RequiresSTA]
		public void TestLoadControl()
		{
			ConsolCollection consols = Factory.New<ForwardingShipment>().Consols;
			JobConsolFilterBusinessObject filterBO = new JobConsolFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				JobConsolFilterControl filterControl = new JobConsolFilterControl(consols, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				var aircraftTypeColumn = filterControl.FilteredGrid.GetColumnStyle("JK_JX_JV_AircraftType");
				AssertNotNull(aircraftTypeColumn);
				Assert(!aircraftTypeColumn.IsVisible);
				AssertEquals("Aircraft Type", aircraftTypeColumn.CaptionResourceString.Caption);

				var securityStatusColumn = filterControl.FilteredGrid.GetColumnStyle("JK_SecurityStatus");
				AssertNotNull(securityStatusColumn);
				Assert(!securityStatusColumn.IsVisible);
				AssertEquals("Security Status", securityStatusColumn.CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestHoldReasonColumn()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;
			var filterBO = new JobConsolFilterBusinessObject();

			using (var filterControl = new JobConsolFilterControl(consols, filterBO))
			{
				var holdReason = nameof(ForwardingConsol.Job) + "+" + nameof(ForwardingConsol.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestAdditionalReferenceColumn()
		{
			var consols = Factory.New<ForwardingShipment>().Consols;
			var filterBO = new JobConsolFilterBusinessObject();

			using (var filterControl = new JobConsolFilterControl(consols, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any(col => col.ColumnName == "NumbersAsString" && !col.IsVisible);

				Assert("Additional Reference column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestCO2eColumnExistInConsolGridWhenGreenhouseGasEmissionCalculationEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var consols = Factory.New<ForwardingShipment>().Consols;
				var filterBO = new JobConsolFilterBusinessObject();

				using (var filterControl = new JobConsolFilterControl(consols, filterBO))
				{
					bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "TotalCO2eForSorting" && !c.IsVisible);

					Assert("CO2e (kg) column should exist and should NOT be visible", columnExistsAndNotVisible);
				}
			}
		}

		[RequiresSTA]
		public void TestCO2eStatusColumnExistInConsolGridWhenGreenhouseGasEmissionCalculationEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var consols = Factory.New<ForwardingShipment>().Consols;
				var filterBO = new JobConsolFilterBusinessObject();

				using (var filterControl = new JobConsolFilterControl(consols, filterBO))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "CO2eStatus");

					AssertNotNull("CO2 Status column should exist", columnInfo);
					AssertEquals("CO2 Status column caption", "CO2e Status", columnInfo.CaptionResourceString.Caption);
					AssertEquals("CO2 Status column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("CO2 Status column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				var consols = Factory.New<ForwardingShipment>().Consols;
				var filterBO = new JobConsolFilterBusinessObject();

				using (var filterControl = new JobConsolFilterControl(consols, filterBO))
				{
					var columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "CO2eStatus");

					Assert("CO2 Status column should not exist", !columnExists);
				}
			}
		}

		[RequiresSTA]
		public void TestElectronicBillDetailsExist()
		{
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consols = Factory.New<ForwardingShipment>().Consols;
				var filterBO = new JobConsolFilterBusinessObject();

				using (var filterControl = new JobConsolFilterControl(consols, filterBO))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "JK_Calc_BillOfLadingBillStatus");

					AssertNotNull("column should exist", columnInfo);
					AssertEquals("column caption", "Electronic Bill Status", columnInfo.CaptionResourceString.Caption);
					AssertEquals("column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("column is readonly", true, columnInfo.IsReadOnly);

					columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "JK_Calc_BillOfLadingBillDate");

					AssertNotNull("column should exist", columnInfo);
					AssertEquals("column caption", "Bill Status Date", columnInfo.CaptionResourceString.Caption);
					AssertEquals("column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("column is readonly", true, columnInfo.IsReadOnly);

					columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "JK_ElectronicBillOfLadingType");

					AssertNotNull("column should exist", columnInfo);
					AssertEquals("column caption", "eBL Type", columnInfo.CaptionResourceString.Caption);
					AssertEquals("column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("column is readonly", true, columnInfo.IsReadOnly);

					columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "JK_ElectronicBillOfLadingTerms");

					AssertNotNull("column should exist", columnInfo);
					AssertEquals("column caption", "eBL Terms", columnInfo.CaptionResourceString.Caption);
					AssertEquals("column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("column is readonly", true, columnInfo.IsReadOnly);

					columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "JK_ElectronicBillOfLadingReference");

					AssertNotNull("column should exist", columnInfo);
					AssertEquals("column caption", "eBL Identifier", columnInfo.CaptionResourceString.Caption);
					AssertEquals("column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("column is readonly", true, columnInfo.IsReadOnly);
				}
			}

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				var consols = Factory.New<ForwardingShipment>().Consols;
				var filterBO = new JobConsolFilterBusinessObject();

				using (var filterControl = new JobConsolFilterControl(consols, filterBO))
				{
					var columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JK_Calc_BillOfLadingBillStatus");

					Assert("column should not exist", !columnExists);

					columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JK_Calc_BillOfLadingBillDate");

					Assert("column should not exist", !columnExists);

					columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JK_ElectronicBillOfLadingType");

					Assert("column should not exist", !columnExists);

					columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JK_ElectronicBillOfLadingTerms");

					Assert("column should not exist", !columnExists);

					columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JK_ElectronicBillOfLadingReference");

					Assert("column should not exist", !columnExists);
				}
			}
		}
	}
}
