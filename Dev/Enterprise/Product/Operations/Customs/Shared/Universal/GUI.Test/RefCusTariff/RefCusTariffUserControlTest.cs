using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.GUI.Testing
{
	class RefCusTariffUserControlTest : TestCaseWithFactory
	{
		public void TestRateAndUnitTabOrder()
		{
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				var tariffDataTabControl = (ZTabControl)tariffUserControl.Controls.Find("TariffDataTabControl", true)[0];
				AssertEquals("Rates should be tab order 0", "RateTab", tariffDataTabControl.TabPages[0].Name);
				AssertEquals("Units Of Measure should be tab order 1", "UnitsOfMeasureTabPage", tariffDataTabControl.TabPages[1].Name);
			}
		}

		public void TestConditionsTabControl_Captions()
		{
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				var conditionsTabControl = (ZTabControl)tariffUserControl.Controls.Find("ConditionsTabControl", true)[0];
				conditionsTabControl.Show();
				AssertEquals("First tab should have caption Values", "Values", conditionsTabControl.TabPages[0].Text);
				AssertEquals("Second tab should have caption Applies To", "Applies To", conditionsTabControl.TabPages[1].Text);
			}
		}

		[RequiresSTA]
		public void TestZZ1_CRT_NKTariffVersionDropEdit_Visible_IsNotSystem()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				AssertEquals(true, tariffUserControl.FindSingle<ZDropEdit>("ZZ1_CRT_NKTariffVersionDropEdit").Visible);
			}
		}

		public void TestZZ1_CRT_NKTariffVersionDropEdit_Invisible_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				AssertEquals(false, tariffUserControl.FindSingle<ZDropEdit>("ZZ1_CRT_NKTariffVersionDropEdit").Visible);
			}
		}

		public void TestVATApplicabilityGridContainsSpecificColumns()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("VatApplicabilitiesGrid");
				AssertNotNull(grid);
				CombineAssertions("VatApplicabilities Grid should have the following columns:", () =>
				{
					AssertNotNull(VATApplicabilityView.Schema.ZX5_StartDate, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_StartDate) as ZDateEditColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_EndDate, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_EndDate) as ZDateEditColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_AdditionalCode, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_AdditionalCode) as ZTextBoxColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_Description, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_Description) as ZTextBoxColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_ZZF_NKTaxOrFeeCode, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_ZZF_NKTaxOrFeeCode) as ZTextBoxColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_ZZZ_NKDataGrouping, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_ZZZ_NKDataGrouping) as ZTextBoxColumnStyleInfo);
					AssertNotNull(VATApplicabilityView.Schema.ZX5_VATCategory, grid.GetColumnStyle(VATApplicabilityView.Schema.ZX5_VATCategory) as ZTextBoxColumnStyleInfo);
				});
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestNomenclatureData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "Traditional Chinese");
			helper.CreateOrGetLanguage("FR", "French");
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1020A", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), "DESC GROUP 1", "10.20", "NGT");
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10A", new ZDateTime(2019, 09, 13), new ZDateTime(2020, 11, 16), "DESC GROUP 2", "10", "NGT");
			var nomenclatureGroup3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1020B", new ZDateTime(2019, 09, 13), new ZDateTime(2019, 12, 31), "DESC GROUP 3", "10.20", "NGT");
			var nomenclatureGroup4 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10B", new ZDateTime(2020, 2, 1), new ZDateTime(2020, 11, 16), "DESC GROUP 4", "10", "NGT");
			Factory.Save();
			var zHTWLanguage1 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage1.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup1.PK;
			zHTWLanguage1.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage1.ZX8_Description = "描述群組 1";
			var fRLanguage1 = Factory.New<RefCusNomenclatureLanguage>();
			fRLanguage1.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup1.PK;
			fRLanguage1.ZX8_ZX6_NKLanguage = "FR";
			fRLanguage1.ZX8_Description = "GROUPE DE DESCRIPTION 1";
			var zHTWLanguage2 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage2.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup2.PK;
			zHTWLanguage2.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage2.ZX8_Description = "描述群組 2";
			var fRLanguage2 = Factory.New<RefCusNomenclatureLanguage>();
			fRLanguage2.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup2.PK;
			fRLanguage2.ZX8_ZX6_NKLanguage = "FR";
			fRLanguage2.ZX8_Description = "GROUPE DE DESCRIPTION 2";
			var zHTWLanguage3 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage3.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup3.PK;
			zHTWLanguage3.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage3.ZX8_Description = "描述群組 3";
			var fRLanguage3 = Factory.New<RefCusNomenclatureLanguage>();
			fRLanguage3.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup3.PK;
			fRLanguage3.ZX8_ZX6_NKLanguage = "FR";
			fRLanguage3.ZX8_Description = "GROUPE DE DESCRIPTION 3";
			var zHTWLanguage4 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage4.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup4.PK;
			zHTWLanguage4.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage4.ZX8_Description = "描述群組 4";
			var fRLanguage4 = Factory.New<RefCusNomenclatureLanguage>();
			fRLanguage4.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup4.PK;
			fRLanguage4.ZX8_ZX6_NKLanguage = "FR";
			fRLanguage4.ZX8_Description = "GROUPE DE DESCRIPTION 4";
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "TT1", "NGT", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", compositeKey: "10.20.30", ensureDataGroupingExists: false);
			Factory.Save();
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				var nomenclatureGroupBox = tariffUserControl.FindSingle<ZGroupBox>("NomenclatureGroupBox");
				AssertEquals("nomenclatureGroupBox.Visible", true, nomenclatureGroupBox.Visible);
				var nomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("DefaultLanguageNomenclatureTreeView");
				var treeViewStructure = new ZStringBuilder();
				GatherTreeViewStructure(treeViewStructure, nomenclatureTreeView.Nodes, 1);
				AssertMultilineASCIIEquals("TreeView Structure", @"1 - 10A DESC GROUP 2
2 - 1020A DESC GROUP 1", treeViewStructure.ToStringWithNewLineBetweenAppends());
				var alternateLanguageNomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("AlternateLanguageNomenclatureTreeView");
				var alternateLanguageNomenclatureTreeViewStructure = new ZStringBuilder();
				GatherTreeViewStructure(alternateLanguageNomenclatureTreeViewStructure, alternateLanguageNomenclatureTreeView.Nodes, 1);
				AssertMultilineASCIIEquals("AlternateLanguageNomenclatureTreeView Structure", @"1 - 10A 描述群組 2
2 - 1020A 描述群組 1", alternateLanguageNomenclatureTreeViewStructure.ToStringWithNewLineBetweenAppends());
			}

			tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", ensureDataGroupingExists: false);
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				var nomenclatureGroupBox = tariffUserControl.FindSingle<ZGroupBox>("NomenclatureGroupBox");
				AssertEquals("nomenclatureGroupBox.Visible", false, nomenclatureGroupBox.Visible);
				var nomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("DefaultLanguageNomenclatureTreeView");
				AssertEquals("nomenclatureTreeView.Nodes.Count", 0, nomenclatureTreeView.Nodes.Count);
				var alternateLanguageNomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("AlternateLanguageNomenclatureTreeView");
				AssertEquals("alternateLanguageNomenclatureTreeView.Nodes.Count", 0, alternateLanguageNomenclatureTreeView.Nodes.Count);
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestNomenclatureDataShowTextBaseOnSize()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ZHT", "Traditional Chinese");
			var za = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var longString = ZString.AlphanumericCharacters.PadRight(200, '_');
			var nomenclatureGroup1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1020A", new ZDateTime(2019, 09, 14), new ZDateTime(2020, 11, 16), longString + "DESC GROUP 1", "10.20", "NGT");
			var nomenclatureGroup2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10A", new ZDateTime(2019, 09, 13), new ZDateTime(2020, 11, 16), longString + "DESC GROUP 2", "10", "NGT");
			var nomenclatureGroup3 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "1020B", new ZDateTime(2019, 09, 13), new ZDateTime(2019, 12, 31), "DESC GROUP 3", "10.20", "NGT");
			var nomenclatureGroup4 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10B", new ZDateTime(2020, 2, 1), new ZDateTime(2020, 11, 16), "DESC GROUP 4", "10", "NGT");
			Factory.Save();
			var zHTWLanguage1 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage1.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup1.PK;
			zHTWLanguage1.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage1.ZX8_Description = longString + "描述群組 1";
			var zHTWLanguage2 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage2.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup2.PK;
			zHTWLanguage2.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage2.ZX8_Description = longString + "描述群組 2";
			var zHTWLanguage3 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage3.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup3.PK;
			zHTWLanguage3.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage3.ZX8_Description = "描述群組 3";
			var zHTWLanguage4 = Factory.New<RefCusNomenclatureLanguage>();
			zHTWLanguage4.ZX8_ZZ5_NomenclatureGroup = nomenclatureGroup4.PK;
			zHTWLanguage4.ZX8_ZX6_NKLanguage = "ZHT";
			zHTWLanguage4.ZX8_Description = "描述群組 4";
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.SouthAfrica, "TT1", "NGT", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0", compositeKey: "10.20.30", ensureDataGroupingExists: false);
			Factory.Save();
			GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = "ZH-TW";
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				form.Size = form.MinimumSize;
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				var nomenclatureGroupBox = tariffUserControl.FindSingle<ZGroupBox>("NomenclatureGroupBox");
				AssertEquals("nomenclatureGroupBox.Visible", true, nomenclatureGroupBox.Visible);
				var defaultLanguageNomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("DefaultLanguageNomenclatureTreeView");
				AssertEquals(1, defaultLanguageNomenclatureTreeView.Nodes.Count);
				var defaultLanguageParentNode = defaultLanguageNomenclatureTreeView.Nodes[0];
				var defaultLanguageParentNodeTextLength = defaultLanguageParentNode.Text.Length;
				AssertEquals(1, defaultLanguageParentNode.Nodes.Count);
				var defaultLanguageChildNode = defaultLanguageParentNode.Nodes[0];
				var defaultLanguageChildNodeTextLength = defaultLanguageChildNode.Text.Length;
				form.WindowState = FormWindowState.Maximized;
				AssertLessThan("Default Language parentNode.Text", defaultLanguageParentNodeTextLength, defaultLanguageParentNode.Text.Length);
				AssertLessThan("Default Language childNode.Text", defaultLanguageChildNodeTextLength, defaultLanguageChildNode.Text.Length);
				form.WindowState = FormWindowState.Normal;
				var nomenclatureTabControl = tariffUserControl.FindSingle<ZTabControl>("NomenclatureTabControl");
				nomenclatureTabControl.SelectedTab = tariffUserControl.FindSingle<ZTabPage>("AlternateLanguageTabPage");
				var alternateLanguageNomenclatureTreeView = nomenclatureGroupBox.FindSingle<ZTreeView>("AlternateLanguageNomenclatureTreeView");
				AssertEquals(1, alternateLanguageNomenclatureTreeView.Nodes.Count);
				var alternateLanguageParentNode = alternateLanguageNomenclatureTreeView.Nodes[0];
				var alternateLanguageParentNodeTextLength = alternateLanguageParentNode.Text.Length;
				AssertEquals(1, alternateLanguageParentNode.Nodes.Count);
				var alternateLanguageChildNode = alternateLanguageParentNode.Nodes[0];
				var alternateLanguageChildNodeTextLength = alternateLanguageChildNode.Text.Length;
				form.WindowState = FormWindowState.Maximized;
				AssertLessThan("Alternate Language parentNode.Text", alternateLanguageParentNodeTextLength, alternateLanguageParentNode.Text.Length);
				AssertLessThan("Alternate Language childNode.Text", alternateLanguageChildNodeTextLength, alternateLanguageChildNode.Text.Length);
			}
		}

		public void TestDataGroupingIsShowingInGridsForDataGroupingWithMembers_IsSystem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var au = helper.CreateNewOrGetExistingDataGrouping("AU");
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var it = helper.CreateNewOrGetExistingDataGrouping("IT", parent: eun);
			var de = helper.CreateNewOrGetExistingDataGrouping("DE", parent: eun);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, "EUN", "T1T", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff("EUN", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			Factory.Save();
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				AssertGridColumnAvailability(tariffUserControl, "RatesGrid", RateView.Schema.ZZ2_ZZZ_NKDataGrouping, false);
				AssertGridColumnAvailability(tariffUserControl, "RateApplicabilitiesGrid", CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping, false);
				AssertGridColumnAvailability(tariffUserControl, "VatApplicabilitiesGrid", VATApplicabilityView.Schema.ZX5_ZZZ_NKDataGrouping, false);
				AssertGridColumnAvailability(tariffUserControl, "AdditionalCodesGrid", TariffAdditionalCodeView.Schema.ZY2_ZZZ_NKDataGrouping, false);
				AssertGridColumnAvailability(tariffUserControl, "AdditionalCodesApplicabilitiesGrid", CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping, false);
			}

			tariff = helper.CreateTariff("AU", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				AssertGridColumnAvailability(tariffUserControl, "RatesGrid", RateView.Schema.ZZ2_ZZZ_NKDataGrouping, true);
				AssertGridColumnAvailability(tariffUserControl, "RateApplicabilitiesGrid", CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping, true);
				AssertGridColumnAvailability(tariffUserControl, "VatApplicabilitiesGrid", VATApplicabilityView.Schema.ZX5_ZZZ_NKDataGrouping, true);
				AssertGridColumnAvailability(tariffUserControl, "AdditionalCodesGrid", TariffAdditionalCodeView.Schema.ZY2_ZZZ_NKDataGrouping, true);
				AssertGridColumnAvailability(tariffUserControl, "AdditionalCodesApplicabilitiesGrid", CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping, true);
			}
		}

		public void TestRatesGridContainsSpecificColumns()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RatesGrid");
				AssertNotNull(grid);
				CombineAssertions("Rates Grid should have the following columns:", () =>
				{
					var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { AutoRateView.Schema.ZZ2_StartDate, AutoRateView.Schema.ZZ2_EndDate, RateView.Schema.ZZ2_EndDate_ForDisplay, RateView.Schema.ZZ2_ZZR_RateTypeDesc,
						AutoRateView.Schema.ZZ2_RateFormula, AutoRateView.Schema.ZZ2_ZZS_Preference, RateView.Schema.PreferenceCode, RateView.Schema.PreferenceDescription, AutoRateView.Schema.ZZ2_ZY1_RateCode, RateView.Schema.RateCode,
						RateView.Schema.RateDescription, AutoRateView.Schema.ZZ2_RX_NKCurrencyOverride, AutoRateView.Schema.ZZ2_ZZZ_NKDataGrouping, RateView.Schema.RateFormulaDescription, RateView.Schema.ZZ2_ZZR_RateTypeCode }, columnNames);
					AssertGridColumn<ZDateEditColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_StartDate, 90, false, true);
					AssertGridColumn<ZDateEditColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_EndDate, 90, false, true);
					AssertGridColumn<ZDateEditColumnStyleInfo>(grid, RateView.Schema.ZZ2_EndDate_ForDisplay, 90, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.ZZ2_ZZR_RateTypeDesc, 104, false, false);
					AssertGridColumn<ZGuidFindBoxColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_ZZS_Preference, 75, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.PreferenceCode, 75, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.PreferenceDescription, 293, false, false);
					AssertGridColumn<ZGuidFindBoxColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_ZY1_RateCode, 80, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.RateCode, 45, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.RateDescription, 191, false, false);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_RX_NKCurrencyOverride, 92, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_ZZZ_NKDataGrouping, 109, false, true);
					AssertGridColumn<RateFormulaBoxColumnStyleInfo>(grid, AutoRateView.Schema.ZZ2_RateFormula, 110, false, true);
					AssertGridColumn<ZMultiLineTextBoxColumnInfo>(grid, RateView.Schema.RateFormulaDescription, 110, false, true);
					AssertGridColumn<ZTextBoxColumnStyleInfo>(grid, RateView.Schema.ZZ2_ZZR_RateTypeCode, 75, false, true);
					var rateFormulaColumn = grid.GetColumnStyle(AutoRateView.Schema.ZZ2_RateFormula);
					AssertEquals("ZZ2_RateFormula: RateFormulaBoxColumn UnitListProvider", tariffView, ((RateFormulaBoxColumnStyleInfo)rateFormulaColumn).UnitListProvider);
				});
			}
		}

		public void TestRatesGridColumns()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RatesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("ZZ2_EndDate_ForDisplay", true, grid.GetColumnStyle(RateView.Schema.ZZ2_EndDate_ForDisplay).IsUnavailable);
					AssertEquals("PreferenceCode", true, grid.GetColumnStyle(RateView.Schema.PreferenceCode).IsUnavailable);
					AssertEquals("RateCode", true, grid.GetColumnStyle(RateView.Schema.RateCode).IsUnavailable);
					AssertEquals("ZZ2_RX_NKCurrencyOverride", true, grid.GetColumnStyle(AutoRateView.Schema.ZZ2_RX_NKCurrencyOverride).IsUnavailable);
				});
			}
		}

		public void TestRatesGridColumns_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RatesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("ZZ2_EndDate", true, grid.GetColumnStyle(AutoRateView.Schema.ZZ2_EndDate).IsUnavailable);
					AssertEquals("ZZ2_ZZS_Preference", true, grid.GetColumnStyle(AutoRateView.Schema.ZZ2_ZZS_Preference).IsUnavailable);
					AssertEquals("ZZ2_ZY1_RateCode", true, grid.GetColumnStyle(AutoRateView.Schema.ZZ2_ZY1_RateCode).IsUnavailable);
				});
			}
		}

		public void TestChildTiraffGridColumnsCaption()
		{
			CombineAssertions(() =>
			{
				var refCusTariffType = Factory.New<RefCusTariffType>();
				AssertEquals("Tariff Type", DataBoundResourceStrings.GetDataForProperty(refCusTariffType.ZZI_TariffTypeInfo).Caption);
				AssertEquals("Description", DataBoundResourceStrings.GetDataForProperty(refCusTariffType.ZZI_DescriptionInfo).Caption);
				var tariffView = Factory.New<TariffView>();
				AssertEquals("Tariff Code", DataBoundResourceStrings.GetDataForProperty(tariffView.ZZ1_TariffCodeInfo).Caption);
			});
		}

		[RequiresSTA]
		public void TestRatesGridColumns_ZZ2_ZZZ_NKDataGrouping_IsParentDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping("IT", parent: eun);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, "EUN", "T1T", ensureDataGroupingExists: false);
			Factory.Save();
			var tariffView = helper.CreateTariff("EUN", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			Factory.Save();
			tariffView.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RatesGrid");
				AssertEquals("ZZ2_EndDate_ForDisplay", true, grid.GetColumnStyle(AutoRateView.Schema.ZZ2_ZZZ_NKDataGrouping).IsUnavailable);
			}
		}

		public void TestRateApplicabilitiesGridContainsSpecificColumns()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RateApplicabilitiesGrid");
				AssertNotNull(grid);
				CombineAssertions(() =>
				{
					var columnNames = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { AutoCusRefApplicabilityView.Schema.ZZT_StartDate, AutoCusRefApplicabilityView.Schema.ZZT_EndDate, AutoCusRefApplicabilityView.Schema.ZZT_AdditionalCode,
						AutoCusRefApplicabilityView.Schema.ZZT_OrderNumber, AutoCusRefApplicabilityView.Schema.ZZT_ZZA_TradeGroup, CusRefApplicabilityView.Schema.TradeGroupDescription, CusRefApplicabilityView.Schema.ExcludedTradeGroupsConcatenated,
						CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping, AutoCusRefApplicabilityView.Schema.ZZT_ZZA_SecondTradeGroup, CusRefApplicabilityView.Schema.SecondTradeGroupDescription }, columnNames);
					AssertEquals("ZZT_StartDate", 90, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_StartDate).Width);
					AssertEquals("ZZT_EndDate", 90, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_EndDate).Width);
					AssertEquals("ZZT_AdditionalCode", 100, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_AdditionalCode).Width);
					AssertEquals("ZZT_OrderNumber", 100, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_OrderNumber).Width);
					AssertEquals("ZZT_ZZA_TradeGroup", 80, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_ZZA_TradeGroup).Width);
					var tradeGroupDescriptionColumn = grid.GetColumnStyle(CusRefApplicabilityView.Schema.TradeGroupDescription);
					AssertEquals("TradeGroupDescription Width", 130, tradeGroupDescriptionColumn.Width);
					AssertEquals("TradeGroupDescription IsVisible", false, tradeGroupDescriptionColumn.IsVisible);
					AssertEquals("ExcludedTradeGroupsConcatenated", 170, grid.GetColumnStyle(CusRefApplicabilityView.Schema.ExcludedTradeGroupsConcatenated).Width);
					AssertEquals("ZZT_TradeGroupDataGrouping", 110, grid.GetColumnStyle(CusRefApplicabilityView.Schema.ZZT_TradeGroupDataGrouping).Width);
					AssertEquals("ZZT_ZZA_SecondTradeGroup", 80, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_ZZA_SecondTradeGroup).Width);
					var secondTradeGroupDescriptionColumn = grid.GetColumnStyle(CusRefApplicabilityView.Schema.SecondTradeGroupDescription);
					AssertEquals("SecondTradeGroupDescription Width", 130, secondTradeGroupDescriptionColumn.Width);
					AssertEquals("SecondTradeGroupDescription IsVisible", false, secondTradeGroupDescriptionColumn.IsVisible);
				});
			}
		}

		public void TestRateApplicabilitiesGridColumns()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RateApplicabilitiesGrid");
				CombineAssertions(() =>
				{
					AssertEquals("ZZT_AdditionalCode", true, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_AdditionalCode).IsUnavailable);
					AssertEquals("ExcludedTradeGroupsConcatenated", true, grid.GetColumnStyle(CusRefApplicabilityView.Schema.ExcludedTradeGroupsConcatenated).IsUnavailable);
				});
			}
		}

		public void TestRateApplicabilitiesGridColumns_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("RateApplicabilitiesGrid");
				AssertEquals("ZZT_ZZA_TradeGroup", true, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_ZZA_TradeGroup).IsUnavailable);
				AssertEquals("ZZT_ZZA_SecondTradeGroup", true, grid.GetColumnStyle(AutoCusRefApplicabilityView.Schema.ZZT_ZZA_SecondTradeGroup).IsUnavailable);
			}
		}

		public void TestStartAndEndDatesIncludeTime()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var startDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_StartDateDateEdit");
				var endDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_EndDateDateEdit");
				var tariffDataTabControl = tariffUserControl.FindSingle<ZTabControl>("TariffDataTabControl");
				var ratesTabPage = form.FindSingle<ZTabPage>("RateTab");
				tariffDataTabControl.SelectedTab = ratesTabPage;
				var packCountriesGrid = ratesTabPage.FindSingle<ZGrid>("RatesGrid");
				var ratesGridStartDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_StartDate) as ZDateEditColumnStyleInfo;
				var ratesGridEndDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate) as ZDateEditColumnStyleInfo;
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_StartDateDateEdit should have start date with time", ZDateTimePickerFormat.Short, startDateControl.DateTimeFormat);
					AssertEquals("ZZ1_EndDateDateEdit should have end date with time", ZDateTimePickerFormat.Short, endDateControl.DateTimeFormat);
					AssertEquals("RatesGrid ZZ2_StartDate should have start date with time", ZDateTimePickerFormat.Short, ratesGridStartDateStyle.DateTimeFormat);
					AssertEquals("RatesGrid ZZ2_EndDate should have end date with time", ZDateTimePickerFormat.Short, ratesGridEndDateStyle.DateTimeFormat);
				});
			}
		}

		public void TestStartAndEndDatesIncludeTime_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var tariffDataTabControl = tariffUserControl.FindSingle<ZTabControl>("TariffDataTabControl");
				var ratesTabPage = form.FindSingle<ZTabPage>("RateTab");
				tariffDataTabControl.SelectedTab = ratesTabPage;
				var packCountriesGrid = ratesTabPage.FindSingle<ZGrid>("RatesGrid");
				var ratesGridEndDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate_ForDisplay) as ZDateEditColumnStyleInfo;
				AssertEquals("RatesGrid ZZ2_EndDate_ForDisplay should have end date with time", ZDateTimePickerFormat.Short, ratesGridEndDateStyle.DateTimeFormat);
			}
		}

		public void TestStartAndEndDatesIncludeTime_ZA()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var startDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_StartDateDateEdit");
				var endDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_EndDateDateEdit");
				var tariffDataTabControl = tariffUserControl.FindSingle<ZTabControl>("TariffDataTabControl");
				var ratesTabPage = form.FindSingle<ZTabPage>("RateTab");
				tariffDataTabControl.SelectedTab = ratesTabPage;
				var packCountriesGrid = ratesTabPage.FindSingle<ZGrid>("RatesGrid");
				var ratesGridStartDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_StartDate) as ZDateEditColumnStyleInfo;
				var ratesGridEndDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate) as ZDateEditColumnStyleInfo;
				CombineAssertions(() =>
				{
					AssertEquals("ZZ1_StartDateDateEdit should have start date with time", ZDateTimePickerFormat.Long, startDateControl.DateTimeFormat);
					AssertEquals("ZZ1_EndDateDateEdit should have end date with time", ZDateTimePickerFormat.Long, endDateControl.DateTimeFormat);
					AssertEquals("RatesGrid ZZ2_StartDate should have start date with time", ZDateTimePickerFormat.Long, ratesGridStartDateStyle.DateTimeFormat);
					AssertEquals("RatesGrid ZZ2_EndDate should have end date with time", ZDateTimePickerFormat.Long, ratesGridEndDateStyle.DateTimeFormat);
				});
			}
		}

		public void TestStartAndEndDatesIncludeTime_ZA_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			tariffView.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var tariffDataTabControl = tariffUserControl.FindSingle<ZTabControl>("TariffDataTabControl");
				var ratesTabPage = form.FindSingle<ZTabPage>("RateTab");
				tariffDataTabControl.SelectedTab = ratesTabPage;
				var packCountriesGrid = ratesTabPage.FindSingle<ZGrid>("RatesGrid");
				var ratesGridEndDateStyle = packCountriesGrid.GetColumnStyle(RateView.Schema.ZZ2_EndDate_ForDisplay) as ZDateEditColumnStyleInfo;
				AssertEquals("RatesGrid ZZ2_EndDate_ForDisplay should have end date with time", ZDateTimePickerFormat.Long, ratesGridEndDateStyle.DateTimeFormat);
			}
		}

		public void TestEndDatePositionCorrectly()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var descriptionTextBox = tariffUserControl.FindSingle<ZTextBox>("ZZ1_DescriptionTextBox");
				var endDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_EndDateDateEdit");
				AssertEquals("ZZ1_EndDateDateEdit should have end date with time", ZDateTimePickerFormat.Long, endDateControl.DateTimeFormat);
				AssertEquals("ZZ1_EndDateDateEdit.Location.X", descriptionTextBox.Location.X + descriptionTextBox.Width - endDateControl.Width, endDateControl.Location.X);
			}

			tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var descriptionTextBox = tariffUserControl.FindSingle<ZTextBox>("ZZ1_DescriptionTextBox");
				var endDateControl = tariffUserControl.FindSingle<ZDateEdit>("ZZ1_EndDateDateEdit");
				AssertEquals("ZZ1_EndDateDateEdit should have end date with time", ZDateTimePickerFormat.Short, endDateControl.DateTimeFormat);
				AssertEquals("ZZ1_EndDateDateEdit.Location.X", descriptionTextBox.Location.X + descriptionTextBox.Width - endDateControl.Width, endDateControl.Location.X);
			}
		}

		public void TestAttributesGridAvailability()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_ZZZ_NKDataGrouping = "TW";
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				AssertGridColumnAvailability(tariffUserControl, "AttributesGrid", TariffAttributeView.Schema.AdditionalDescription, false);
			}

			tariff.ZZ1_ZZZ_NKDataGrouping = "CN";
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				AssertGridColumnAvailability(tariffUserControl, "AttributesGrid", TariffAttributeView.Schema.AdditionalDescription, false);
			}

			tariff.ZZ1_ZZZ_NKDataGrouping = "US";
			using (var form = new RefCusTariffForm(tariff))
			{
				form.Show();
				var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
				AssertGridColumnAvailability(tariffUserControl, "AttributesGrid", TariffAttributeView.Schema.AdditionalDescription, true);
			}
		}

		public void TestComponentsHiddenIfNonSystemRecord()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariff))
			using (var userControl = new RefCusTariffUserControl())
			{
				void AssertTabPageIsNull(string nameToFind) => AssertNull(userControl.FindSingleOrDefault<ZTabPage>(nameToFind));
				void AssertGroupBoxIsNotVisible(string nameToFind) => AssertEquals(false, userControl.FindSingleOrDefault<ZGroupBox>(nameToFind)?.Visible);
				form.Controls.Add(userControl);
				form.Show();
				AssertGroupBoxIsNotVisible("NomenclatureGroupBox");
				AssertGroupBoxIsNotVisible("AttributesGroupBox");
				AssertTabPageIsNull("ParentTariffTabPage");
				AssertTabPageIsNull("ChildTariffsTabPage");
				AssertTabPageIsNull("AdditionalCodesTabPage");
				AssertTabPageIsNull("ConditionsTabPage");
				//UT
			}
		}

		public void TestUnitsOfMeasureGridColumns() => CombineAssertions(() =>
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				var unitsOfMeasureGridControls = tariffUserControl.Controls.Find("UnitsOfMeasureGrid", true);
				AssertEquals("unitsOfMeasureGridControls count", 1, unitsOfMeasureGridControls.Length);
				var unitsOfMeasureGridControl = unitsOfMeasureGridControls.Single();
				AssertType<ZGrid>("unitsOfMeasureGridControl type", unitsOfMeasureGridControl);
				var unitsOfMeasureGrid = (ZGrid)unitsOfMeasureGridControl;
				AssertEquals("column count", 5, unitsOfMeasureGrid.ColumnStyles.Count);
				AssertColumn<ZDropEditColumnStyleInfo>(0, TariffUOMView.Schema.ZZ8_Type, 50);
				AssertColumn<ZDropEditColumnStyleInfo>(1, TariffUOMView.Schema.ZZ8_UOM, 50);
				AssertColumn<ZTextBoxColumnStyleInfo>(2, $"{nameof(TariffUOMView.CusTradeGroup)}+{CusRefTradeGroupView.Schema.ZZA_TradeGroup}", 50);
				AssertColumn<ZTextBoxColumnStyleInfo>(3, TariffUOMView.Schema.TradeGroupDescription, 120);
				AssertColumn<ZTextBoxColumnStyleInfo>(4, TariffUOMView.Schema.ZZ8_ZZZ_NKDataGrouping, 120);

				void AssertColumn<TColumnType>(int columnIndex, string columnName, int width)
					where TColumnType : ZGridColumnInfo
				{
					var columnInfo = (ZGridColumnInfo)unitsOfMeasureGrid.ColumnStyles[columnIndex];
					AssertType<TColumnType>($"Column {columnIndex} type", columnInfo);
					AssertEquals($"Column {columnIndex} name", columnName, columnInfo.ColumnName);
					AssertEquals($"Column {columnIndex} width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
				}
			}
		});

		[RequiresSTA]
		public void TestRatesApplyToCountryFindBox()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var findBox = form.FindSingle<ZCodeFindBox>("RatesApplyToCountryFindBox");
				AssertEquals(false, findBox.Visible);
			}
		}

		public void TestRatesApplyToCountryFindBox_IsSystem()
		{
			var tariffView = Factory.New<TariffView>();
			tariffView.ZZ1_IsSystem = true;
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var findBox = form.FindSingle<ZCodeFindBox>("RatesApplyToCountryFindBox");
				AssertEquals(true, findBox.Visible);
			}
		}

		public void TestTariffLanguagesGridColumns()
		{
			var tariffView = Factory.New<TariffView>();
			using (var form = new ZForm(tariffView))
			using (var tariffUserControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(tariffUserControl);
				form.Show();
				var grid = form.FindSingle<ZGrid>("TariffLanguagesGrid");
				AssertEquals(CharacterCasing.Upper, grid.GetColumnStyle(AutoCusRefTariffLanguageView.Schema.ZX7_ZX6_NKLanguage).CharacterCasing);
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestVATGridHidden_New()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
				using (var form = (RefCusTariffForm)controller.ShowNewForm())
				{
					form.Show();
					AssertVATGridHidden(form, false);
				}
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestVATGridHidden_Edit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffTypePk = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, "TTX").PK;
				Factory.Save();
				var tariff = CreateTariff(tariffTypePk, "10001");
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
				using (var form = (RefCusTariffForm)controller.ShowEditForm(tariff))
				{
					form.Show();
					AssertVATGridHidden(form, false);
				}
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestVATGridHidden_Delete()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffTypePk = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, "TTX").PK;
				Factory.Save();
				var tariff = CreateTariff(tariffTypePk, "10001");
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
				using (var form = (RefCusTariffForm)controller.ShowDeleteForm(tariff))
				{
					form.Show();
					AssertVATGridHidden(form, true);
				}
			}
		}

		[SelfManagedTariffCountries(Core.Constants.CountryCodes.Congo)]
		public void TestVATGridHidden_View()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Congo))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffTypePk = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, "TTX").PK;
				Factory.Save();
				var tariff = CreateTariff(tariffTypePk, "10001");
				var controller = ZControllerFactory.Create(ControllerIDs.Customs.Universal.RefCusTariff);
				using (var form = (RefCusTariffForm)controller.ShowViewForm(tariff))
				{
					form.Show();
					AssertVATGridHidden(form, true);
				}
			}
		}

		public void TestTariffVersionDropBox()
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_IsSystem = false;
			using (var form = new ZForm(tariff))
			using (var userControl = new RefCusTariffUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				AssertNotNull(userControl.FindSingleOrDefault<ZDropEdit>("ZZ1_CRT_NKTariffVersionDropEdit"));
			}
		}

		public void TestAdditionalCodesTabPageGrid()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var au = helper.CreateNewOrGetExistingDataGrouping("AU");
			var eun = helper.CreateNewOrGetExistingDataGrouping("EUN");
			var it = helper.CreateNewOrGetExistingDataGrouping("IT", parent: eun);
			var de = helper.CreateNewOrGetExistingDataGrouping("DE", parent: eun);
			Factory.Save();

			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, "EUN", "T1T", ensureDataGroupingExists: false);
			Factory.Save();
			var tariff = helper.CreateTariff("AU", tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			Factory.Save();
			using (var form = new RefCusTariffForm(tariff))
			using (var userControl = form.FindSingle<RefCusTariffUserControl>())
			{
				form.Show();
				var additionalCodesTabPage = userControl.FindSingleOrDefault<ZTabPage>("AdditionalCodesTabPage");

				AssertNotNull("prerequisite: additionalCodesTabPage is not null", additionalCodesTabPage);

				var additionalCodesSplitContainer = additionalCodesTabPage.FindSingleOrDefault<KSplitContainer>("AdditionalCodesSplitContainer");

				AssertNotNull("AdditionalCodesSplitContainer is not null", additionalCodesSplitContainer);
				AssertNotNull("AdditionalCodesGrid is not null", additionalCodesSplitContainer.Panel1.FindSingleOrDefault<ZGrid>("AdditionalCodesGrid"));

				var groupBox = additionalCodesSplitContainer.Panel2.FindSingleOrDefault<ZGroupBox>("AdditionalCodesApplicabilitiesGroupBox");
				AssertNotNull("AdditionalCodesAppliesToGrid is not null", groupBox);
				AssertNotNull("AdditionalCodesAppliesToGrid is not null", groupBox.FindSingleOrDefault<ZGrid>("AdditionalCodesApplicabilitiesGrid"));
			}
		}

		void AssertGridColumn<T>(ZGrid grid, string columnName, int width, bool readOnly, bool visible)
			where T : IZColumnStyleInfo
		{
			var columnStyleInfo = grid.GetColumnStyle(columnName);
			AssertNotNull($"{columnName} should exist", columnStyleInfo);
			AssertType<T>($"{columnName}", columnStyleInfo);
			AssertEquals($"{columnName}.Width", width, columnStyleInfo.Width);
			AssertEquals($"{columnName}.IsReadOnly", readOnly, columnStyleInfo.IsReadOnly);
			AssertEquals($"{columnName}.IsVisible", visible, columnStyleInfo.IsVisible);
		}

		void GatherTreeViewStructure(ZStringBuilder treeViewStructure, TreeNodeCollection nodes, int level)
		{
			if (nodes != null)
			{
				foreach (TreeNode node in nodes)
				{
					treeViewStructure.Append($"{level} - {node.Text}");
					GatherTreeViewStructure(treeViewStructure, node.Nodes, level + 1);
				}
			}
		}

		void AssertGridColumnAvailability(RefCusTariffUserControl tariffUserControl, string gridName, string columnName, bool isUnavailable)
		{
			var grid = tariffUserControl.FindSingle<ZGrid>(gridName);
			AssertEquals(gridName + "." + columnName, isUnavailable, grid.GetColumnStyle(columnName).IsUnavailable);
		}

		static void AssertVATGridHidden(RefCusTariffForm form, bool expected)
		{
			var tariffUserControl = form.FindSingle<RefCusTariffUserControl>();
			var tariffDataTabControl = tariffUserControl.FindSingle<ZTabControl>("TariffDataTabControl");
			tariffDataTabControl.SelectedTab = tariffUserControl.FindSingle<ZTabPage>("VATDataTabPage");
			AssertEquals(expected, form.FindSingle<ZGrid>("VatApplicabilitiesGrid").Visible);
		}

		TariffView CreateTariff(ZGuid tariffTypePk, string tariffCode)
		{
			var tariff = Factory.New<TariffView>();
			tariff.ZZ1_IsSystem = false;
			tariff.ZZ1_ZZI_TariffType = tariffTypePk;
			tariff.ZZ1_TariffCode = tariffCode;
			tariff.ZZ1_StartDate = ZDateTime.Today.AddDays(-1);
			tariff.ZZ1_EndDate = ZDateTime.Today.AddDays(1);
			tariff.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Australia;
			tariff.ZZ1_Description = "Default description";
			tariff.ZZ1_CRT_NKTariffVersion = "VER1";
			Factory.Save();
			return Factory.Load<TariffView>(tariff.PK);
		}
	}
}
