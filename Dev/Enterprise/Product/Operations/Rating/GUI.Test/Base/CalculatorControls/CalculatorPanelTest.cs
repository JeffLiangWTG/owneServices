using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Testing
{
	public class CalculatorPanelTest : TestCaseWithFactory
	{
		public void TestGetCalculatorControl_RelatedRateLine()
		{
			using (var panel = new CalculatorPanel())
			{
				CheckHandleCreated(panel);

				var rateLine = GetRateLine();
				var cmbCalculatorType = typeof(CombinedCalculator);
				SetCalculator(cmbCalculatorType, rateLine, panel);
				var rateLineCalculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(cmbCalculatorType), rateLine is RelatedRateLine);
				rateLineCalculatorControl.AccessibleDescription = "rateLineCalculatorControl";

				var cost = Helper.NewCosting(Helper.NewOrgHeader());
				var costEntry = cost.AddRateEntry("FCL", "SEA", "AUSYD", "");
				var costLine = costEntry.AddRateLine("ODOC");
				var relatedRateLine = Factory.Load<RelatedRateLine>(costLine.PK);
				SetCalculator(cmbCalculatorType, relatedRateLine, panel);
				var relatedRateLineCalculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(cmbCalculatorType), relatedRateLine is RelatedRateLine);
				relatedRateLineCalculatorControl.AccessibleDescription = "relatedRateLineCalculatorControl";
				AssertNotEquals
				(
					"WHEN GetCalculatorControl THEN RateLine and RelatedRateLine should return different CalculatorControl",
					rateLineCalculatorControl.AccessibleDescription,
					relatedRateLineCalculatorControl.AccessibleDescription
				);

				SetCalculator(cmbCalculatorType, rateLine, panel);
				var rateLineCalculatorControl2 = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(cmbCalculatorType), rateLine is RelatedRateLine);
				AssertEquals
				(
					"WHEN GetCalculatorControl THEN RateLine and previous RateLine should return same CalculatorControl",
					rateLineCalculatorControl.AccessibleDescription,
					rateLineCalculatorControl2.AccessibleDescription
				);
			}
		}

		TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;

		public void TestCalculatorControls()
		{
			using (var panel = new CalculatorPanel())
			{
				CheckHandleCreated(panel);
				var rateLine = GetRateLine();

				int numberOfCalculators = 0;
				Type[] assemblyTypes = typeof(Calculator).Assembly.GetExportedTypes();
				foreach (Type type in assemblyTypes)
				{
					if (type.IsSubclassOf(typeof(Calculator)) && !type.IsAbstract && type != typeof(NullCalculator) && !type.IsTestCalculator())
					{
						SetCalculator(type, rateLine, panel);

						RateCalculatorUserControl calculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(type));
						AssertNotNull(type.Name + " not null", calculatorControl);
						Assert(type.Name + " is in the Panel's control collection.", panel.Controls.Contains(calculatorControl));
						AssertEquals("All calculators have remove action set to RemoveAndDelete", RemoveAction.RemoveAndDelete, calculatorControl.RemoveAction);
						numberOfCalculators++;
					}
				}
				AssertEquals(30, numberOfCalculators);
			}
		}

		public void TestViewCalculatorIsPlacedOnTop()
		{
			using (var form = new ZForm())
			using (var panel = new CalculatorPanel())
			{
				var rateLine = GetRateLine();
				rateLine.TL_RateCalculator = PercentageCalculator.Code;
				form.Controls.Add(panel);
				form.Show();

				panel.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
				Application.DoEvents();
				var calculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(typeof(PercentageCalculator)));
				AssertEquals(0, panel.Controls.GetChildIndex(calculatorControl));
			}
		}

		public void TestGetRateLineItemsGrid()
		{
			Type[] calculatorsWithoutGrid = new Type[]
			{
					typeof(AgencyCalculator),
					typeof(FirstPlusAdditionalCalculator),
					typeof(FlatCalculator),
					typeof(FlatPlusPerUnitCalculator),
					typeof(PackageCountCalculator),
					typeof(MinimumOrPerUnitCalculator),
					typeof(UnitCalculator),
					typeof(MinimumCalculator),
					typeof(FreightInclusiveCalculator),
					typeof(ExcludeCompanyTariffsCalculator)
			};

			using (var panel = new CalculatorPanel())
			{
				CheckHandleCreated(panel);
				var rateLine = GetRateLine();

				Type[] assemblyTypes = typeof(Calculator).Assembly.GetExportedTypes();
				foreach (Type type in assemblyTypes)
				{
					if (type.IsSubclassOf(typeof(Calculator)) && !type.IsAbstract && type != typeof(NullCalculator) && !type.IsTestCalculator())
					{
						SetCalculator(type, rateLine, panel);
						RateCalculatorUserControl calculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(type));
						AssertEquals(type.Name, ((IList<Type>)calculatorsWithoutGrid).Contains(type), calculatorControl.GetRateLineItemsGrid() == null);
					}
				}
			}
		}

		public void TestWeightBreakDecimals()
		{
			using (var panel = new CalculatorPanel())
			{
				CheckHandleCreated(panel);
				var rateLine = GetRateLine();

				Type[] assemblyTypes = typeof(Calculator).Assembly.GetExportedTypes();
				foreach (Type type in assemblyTypes)
				{
					if (type.IsSubclassOf(typeof(Calculator)) && !type.IsAbstract && type != typeof(NullCalculator) && !type.IsTestCalculator())
					{
						SetCalculator(type, rateLine, panel);
						RateCalculatorUserControl calculatorControl = panel.GetCalculatorControl(new ControlToCalculatorMap().GetControlType(type));
						ZGrid itemsGrid = calculatorControl.GetRateLineItemsGrid();

						if (itemsGrid != null)
						{
							ZCalcEditColumnStyleInfo weightBreak = null;
							foreach (object style in itemsGrid.ColumnStyles)
							{
								ZCalcEditColumnStyleInfo calcEditColumn = style as ZCalcEditColumnStyleInfo;
								if (calcEditColumn != null && calcEditColumn.ColumnName == RateLineItemsSchema.TM_Break.Name)
								{
									weightBreak = calcEditColumn;
									break;
								}
							}

							if (weightBreak != null)
							{
								int expected = type == typeof(ValueRangeCalculator) ? 2 : 1;
								AssertEquals(type.Name, expected, weightBreak.Decimals);
							}
						}
					}
				}
			}
		}

		public void TestCalculatorPanelShouldHaveOnlyOneVisibleCalculatorControl()
		{
			using (CostingForm form = new CostingForm(Factory.New<Costing>()))
			{
				form.Show();

				var tabControl = form.BaseTabControl.TopLevelTabControl.TabPages[0].Controls[0] as ZTabControl;
				if (tabControl != null)
				{
					var entryTab = tabControl.TabPages[0] as EntryTabPage;
					if (entryTab != null)
					{
						var costing = entryTab.RateEntryGrid.DataSource as Costing;
						if (costing != null)
						{
							costing.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").AddRateLine("FRT", FlatCalculator.Code);

							var rateLines = costing.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection[0].RateLines;
							var container = entryTab.RateLinesAndItemsControl.Controls[0] as SplitContainer;

							if (container != null)
							{
								var rateLineGrid = container.Panel1.Controls[0].Controls[0] as ZGrid;
								var calculatorPanel = container.Panel2.Controls[0] as CalculatorPanel;

								if (rateLineGrid != null && calculatorPanel != null)
								{
									Application.DoEvents();

									rateLineGrid.CurrentRowIndex = 0;
									SetCalculator(typeof(UnitCalculator), rateLines[0], calculatorPanel);
									Application.DoEvents();

									rateLineGrid.CurrentRowIndex = 1;
									Application.DoEvents();
									SetCalculator(typeof(TimeCalculator), rateLines[1], calculatorPanel);
									Application.DoEvents();

									rateLineGrid.CurrentRowIndex = 0;
									Application.DoEvents();
									SetCalculator(typeof(TimeCalculator), rateLines[0], calculatorPanel);
									Application.DoEvents();

									SetCalculator(typeof(UnitCalculator), rateLines[0], calculatorPanel);
									rateLineGrid.CurrentRowIndex = 1; //Immediately change rate line while editing TL_RateCalculator
									Application.DoEvents();

									var calculatorControls = calculatorPanel.Controls.Cast<Control>().Where(c => c as RateCalculatorUserControl != null);
									AssertEquals("Calculator panel should have only one visible Calculator control", 1, calculatorControls.Count(c => c as RateCalculatorUserControl != null && c.Visible));
								}
							}
						}
					}
				}
			}
		}

		public void TestResultCheckboxShouldBeVisible_OnlyWhenCalculatorIsCostBasedOrCompanyTariffBasedCalculator()
		{
			var costing = Factory.New<Costing>();
			using (var costingForm = new CostingForm(costing))
			{
				costingForm.Show();
				costingForm.Activate();
				Application.DoEvents();

				var airTab = costingForm.BaseTabControl.FindTabPage("AIR");
				var entryGrid = airTab.RateEntryGrid as EntryGrid;
				var rateLinesAndItemsPanel = airTab.RateLinesAndItemsControl;
				var container = rateLinesAndItemsPanel.Controls[0] as SplitContainer;
				var calculatorPanel = container.Panel2.Controls[0] as CalculatorPanel;
				AssertEquals("Result Check Box should be hidden when we open a Rate tab page without any Rates", false, calculatorPanel.ViewResultsVisible);

				entryGrid.Focus();
				AssertEquals("Result Check Box should still be hidden when we click the Rate Entry Grid", false, calculatorPanel.ViewResultsVisible);
				Application.DoEvents();

				var linkControl = airTab.Controls.Find("ShowControlLabel", true).Single() as ZLinkLabel;
				linkControl.PerformClick_ForTest();
				AssertEquals("When the type of Calculator is not CST/CTB Calculator, Result Check Box should be hidden.", false, calculatorPanel.ViewResultsVisible);

				entryGrid.CurrentEntry.AddFlatRateLine("FRT", 200);
				Application.DoEvents();
				AssertEquals("When the type of Calculator is not CST/CTB Calculator, Result Check Box should be hidden.", false, calculatorPanel.ViewResultsVisible);

				entryGrid.CurrentEntry.RateLines.RemoveAndDeleteAll();
				entryGrid.CurrentEntry.AddRateLine("FRT", CompanyTariffOrCostBasedCalculator.CostBasedCode);

				Application.DoEvents();
				AssertEquals("When the type of Calculator is CST/CTB Calculator, Result Check Box should be appear.", true, calculatorPanel.ViewResultsVisible);
			}
		}

		public void TestBindingToCartageZones_AfterDeleteAndCreateRateLine_StillShowsCartageZonesInList()
		{
			void AssertCTZListCount(CalculatorPanel calculatorPanel, int expectedNumber)
			{
				var calculatorControls = calculatorPanel.Controls.Cast<Control>().Where(c => c as CartageZoneDistanceControl != null);
				var cartageControl = calculatorPanel.Controls.OfType<CartageZoneDistanceControl>().Single();
				var cartageCalculatorPanel = cartageControl.Controls.OfType<CartageCalculatorPanel>().Single();
				var cartageZoneList = cartageCalculatorPanel.CartageZonesGrid.List;
				AssertEquals("CTZ zones should be loaded", expectedNumber, cartageZoneList.Count);
			}

			var helper = new TestHelper(Factory);
			helper.CreateRateTransportZoneSet(null, CountryCodes.Australia, zoneNames: new ZString[] { "ZoneA", "ZoneB" });

			using (CostingForm form = new CostingForm(Factory.New<Costing>()))
			{
				form.Show();

				var tabControl = form.BaseTabControl.TopLevelTabControl.TabPages[0].Controls[0] as ZTabControl;
				var originTab = tabControl.TabPages[3] as EntryTabPage;
				tabControl.SelectedTab = originTab;

				var costing = originTab.RateEntryGrid.DataSource as Costing;
				var rateEntry = costing.AddRateEntry("ORG", "", "AU", "");
				rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, "KG");
				rateEntry.AddRateLine("ODOC", MinimumOrPerUnitCalculator.Code, "KG");
				Application.DoEvents();

				var rateLines = costing.EntryCollections[RatingConstants.RateCategory.ORG].LazyLoadingCollection[0].RateLines;
				var container = originTab.RateLinesAndItemsControl.Controls[0] as SplitContainer;
				var rateLineGrid = container.Panel1.Controls[0].Controls[0] as ZGrid;
				var calculatorPanel = container.Panel2.Controls[0] as CalculatorPanel;

				rateLineGrid.CurrentRowIndex = 0;
				Application.DoEvents();
				AssertCTZListCount(calculatorPanel, 3);

				SetCalculator(typeof(FlatCalculator), rateLines[0], calculatorPanel);
				Application.DoEvents();
				SetCalculator(typeof(CartageZoneDistanceCalculator), rateLines[0], calculatorPanel);
				Application.DoEvents();
				AssertCTZListCount(calculatorPanel, 3);

				rateLineGrid.CurrentRowIndex = 1;
				SetCalculator(typeof(FlatCalculator), rateLines[1], calculatorPanel);
				Application.DoEvents();
				SetCalculator(typeof(CartageZoneDistanceCalculator), rateLines[1], calculatorPanel);
				Application.DoEvents();
				AssertCTZListCount(calculatorPanel, 3);
			}
		}

		void SetCalculator(Type type, RateLine rateLine, CalculatorPanel panel)
		{
			var calculatorCodeField = (type.GetField("Code") ?? type.GetField("CostBasedCode")) ??
																type.GetField("CompanyTariffBasedCode");
			AssertNotNull(calculatorCodeField);

			rateLine.TL_RateCalculator = (string)calculatorCodeField.GetRawConstantValue();
			panel.ViewCalculatorForBinding = rateLine.ViewCalculatorForBinding;
		}

		RateLine GetRateLine()
		{
			var clientRate = Factory.New<ClientRate>();
			clientRate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			var clientRateEntry = clientRate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			var clientRateLine = clientRateEntry.RateLines.AddNew();

			var chargeCode = Factory.New<AccChargeCode>();
			clientRateLine.TL_AC = chargeCode.PK;
			return clientRateLine;
		}

		void CheckHandleCreated(CalculatorPanel panel)
		{
			if (!panel.IsHandleCreated)
			{
				panel.CreateHandle_ForTest();
			}
		}
	}
}
