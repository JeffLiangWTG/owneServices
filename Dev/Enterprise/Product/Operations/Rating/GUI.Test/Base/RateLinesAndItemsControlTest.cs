using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateLinesAndItemsControlTest : TestCaseWithFactory
	{
		#region Conversion Factor

		public void TestConversionFactor()
		{
			var rate = Factory.New<ClientRate>();
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				var conversionFactorColumnName = "ConversionFactorForBinding+ConversionFactorString";

				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.FCL, shouldExist: false, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.LCL, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.ORG, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.DST, shouldExist: true, shouldVisible: false);

				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.CFC, shouldExist: false, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.CLC, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.COR, shouldExist: true, shouldVisible: false);
				TestColumn(form, conversionFactorColumnName, RatingConstants.RateCategory.CDS, shouldExist: true, shouldVisible: false);
			}
		}

		#endregion

		#region Unit Factor

		public void TestUnitFactor_DefaultVisibleOnlyInAIROrFCLOrDSTCategory()
		{
			var rate = Factory.New<ClientRate>();
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
				TestUnitFactor(form, RatingConstants.RateCategory.FCL, shouldExist: true, shouldVisible: true);
				TestUnitFactor(form, RatingConstants.RateCategory.DST, shouldExist: true, shouldVisible: true);

				TestUnitFactor(form, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: true);
				TestUnitFactor(form, RatingConstants.RateCategory.CFC, shouldExist: true, shouldVisible: true);
				TestUnitFactor(form, RatingConstants.RateCategory.CDS, shouldExist: true, shouldVisible: true);

				TestUnitFactor(form, RatingConstants.RateCategory.WHS, shouldExist: true, shouldVisible: true);

				TestUnitFactor(form, RatingConstants.RateCategory.LCL, shouldExist: true, shouldVisible: false);
				TestUnitFactor(form, RatingConstants.RateCategory.ORG, shouldExist: true, shouldVisible: false);

				TestUnitFactor(form, RatingConstants.RateCategory.CLC, shouldExist: true, shouldVisible: false);
				TestUnitFactor(form, RatingConstants.RateCategory.COR, shouldExist: true, shouldVisible: false);
			}
		}

		public void TestUnitFactor_ExistAndVisible()
		{
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var clientRate = Factory.New<ClientRate>();
				using (var form = new ActiveRatesForm(clientRate))
				{
					form.Show();
					TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
					TestUnitFactor(form, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: true);
				}

				var tariff = Factory.New<CompanyTariff>();
				using (var form = new GlobalTariffsForm(tariff))
				{
					form.Show();
					TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
					TestUnitFactor(form, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: true);
				}

				var intercompanyTariff = Factory.New<IntercompanyTariff>();
				using (var form = new IntercompanyTariffForm(intercompanyTariff))
				{
					form.Show();
					TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
				}

				var costing = Factory.New<Costing>();
				using (var form = new CostingForm(costing))
				{
					form.Show();
					TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
					TestUnitFactor(form, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: true);
				}

				var quote = Factory.New<Quote>();
				using (var form = new QuotationForm(quote))
				{
					form.Show();
					TestUnitFactor(form, RatingConstants.RateCategory.AIR, shouldExist: true, shouldVisible: true);
					TestUnitFactor(form, RatingConstants.RateCategory.CAI, shouldExist: true, shouldVisible: true);
				}
			}
		}

		static void TestUnitFactor(RatingForm form, string category, bool shouldExist, bool shouldVisible, string message = default)
			=> TestColumn(form, RateLine.Schema.TL_UnitFactor, category, shouldExist, shouldVisible, message);

		static void TestColumn(RatingForm form, string columnName, string category, bool shouldExist, bool shouldVisible, string message = default)
		{
			form.BaseTabControl.SelectTabPage(category);

			var categoryTabPage = form.BaseTabControl.FindTabPage(category);
			AssertNotNull($"Tab Page '{category}' should exist", categoryTabPage);

			var rateLinesAndItemsControl = categoryTabPage.RateLinesAndItemsControl;
			var unitFactorColumn = rateLinesAndItemsControl.RateLinesGrid_ForTest.Columns.SingleOrDefault(c => c.ColumnName == columnName);

			CombineAssertions($"{category}: {message}", () =>
			{
				if (shouldExist)
				{
					AssertNotNull($"Column {columnName}", unitFactorColumn);
					AssertEquals("IsVisible", shouldVisible, unitFactorColumn.IsVisible);
				}
				else
				{
					AssertNull($"Column {columnName}", unitFactorColumn);
				}
			});
		}

		#endregion

		#region Description and Local Description

		public void TestDescriptionAndLocalDescription_DisableLocalChargeCodeDescription()
		{
			AssertDescriptionAndLocalDescription
			(
				enableLocalChargeCodeDescription: false,
				expectedShowDescription: true,
				expectedShowLocalDescription: true
			);
		}

		public void TestDescriptionAndLocalDescription_EnableLocalChargeCodeDescription()
		{
			AssertDescriptionAndLocalDescription
			(
				enableLocalChargeCodeDescription: true,
				expectedShowDescription: true,
				expectedShowLocalDescription: true
			);
		}

		void AssertDescriptionAndLocalDescription(bool enableLocalChargeCodeDescription, bool expectedShowDescription, bool expectedShowLocalDescription)
		{
			var rate = Factory.New<ClientRate>();

			var enableLocalChargeCodeDescriptionDefault = (ZArchitecture.Environment.BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableLocalChargeCodeDescription))
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				RateLinesAndItemsControl airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;

				AssertEquals("Description", expectedShowDescription, airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_RateDesc));
				AssertEquals("Local Description", expectedShowLocalDescription, airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_RateDescLocal));
			}
		}

		#endregion

		public void TestContainerOwnershipColumn()
		{
			var rate = Factory.New<ClientRate>();

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;
				Assert(airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.WHS);
				var whsControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.WHS).RateLinesAndItemsControl;
				Assert(!whsControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				var orgControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;
				Assert(orgControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.SCO);
				var scoControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.SCO).RateLinesAndItemsControl;
				Assert(scoControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.CAI);
				var caiControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.CAI).RateLinesAndItemsControl;
				Assert(caiControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.COR);
				var corControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.COR).RateLinesAndItemsControl;
				Assert(corControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ContainerOwnership));

				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TRN, RateLine.Schema.TL_ContainerOwnership, false);
				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TBC, RateLine.Schema.TL_ContainerOwnership, false);
			}
		}

		public void TestConditionalColumns()
		{
			var rate = Factory.New<ClientRate>();

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				var tabControl = form.BaseTabControl.TopLevelTabControl;
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;
				Assert(airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.WHS);
				var whsControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.WHS).RateLinesAndItemsControl;
				Assert(!whsControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(!whsControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				var orgControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.ORG).RateLinesAndItemsControl;
				Assert(orgControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(orgControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.CAI);
				var caiControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.CAI).RateLinesAndItemsControl;
				Assert(airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(airControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.COR);
				var corControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.COR).RateLinesAndItemsControl;
				Assert(orgControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(orgControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.TWU);
				var twuControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.TWU).RateLinesAndItemsControl;
				Assert(twuControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(twuControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.TRW);
				var trwControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.TRW).RateLinesAndItemsControl;
				Assert(trwControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_Condition));
				Assert(trwControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_ConditionalExpression));

				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TRN, RateLine.Schema.TL_Condition, true);
				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TRN, RateLine.Schema.TL_ConditionalExpression, true);

				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TBC, RateLine.Schema.TL_Condition, true);
				AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.TBC, RateLine.Schema.TL_ConditionalExpression, true);
			}
		}

		public void TestFeesAndChargesColumns()
		{
			var tariff = Factory.New<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;

			var airEntry = tariff.AIRRateEntriesForBinding.AddNew();
			var airLine = airEntry.RateLines.AddNew();
			airLine.FillWithValidTestData();

			var fclEntry = tariff.FCLRateEntriesForBinding.AddNew();
			var seaLine = fclEntry.RateLines.AddNew();
			seaLine.FillWithValidTestData();

			var caiEntry = tariff.CAIRateEntriesForBinding.AddNew();
			var caiLine = caiEntry.RateLines.AddNew();
			caiLine.FillWithValidTestData();

			var cfcEntry = tariff.CFCRateEntriesForBinding.AddNew();
			var cfcLine = cfcEntry.RateLines.AddNew();
			cfcLine.FillWithValidTestData();

			Factory.Save();

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new GlobalTariffsForm(tariff))
			{
				form.Show();

				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.AIR);
				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.ORG);
				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.FCL);

				// Customs
				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.CAI);
				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.COR);
				AssertFeesAndChargeLevelColumnsExists(form, RatingConstants.RateCategory.CFC);
			}
		}

		static void AssertFeesAndChargeLevelColumnsExists(GlobalTariffsForm form, string category)
		{
			form.BaseTabControl.SelectTabPage(category);
			var cfcControl = form.BaseTabControl.FindTabPage(category).RateLinesAndItemsControl;
			Assert($"{category} FeeChargeType", cfcControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_FeeChargeType));
			Assert($"{category} FeeChargeLevel", cfcControl.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_FeeChargeLevel));
		}

		public void TestWarehouseColumns()
		{
			var rate = Factory.New<ClientRate>();

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				ZTabControl tabControl = form.BaseTabControl.TopLevelTabControl;
				form.Show();

				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.AIR, isVisible: true, isShowProductNumber: false, isShowIsOnPallets: false, isShowIsWhsJobLevelCharge: false);
				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.CAI, isVisible: true, isShowProductNumber: false, isShowIsOnPallets: false, isShowIsWhsJobLevelCharge: false);
				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.WHS, isVisible: true, isShowProductNumber: true, isShowIsOnPallets: true, isShowIsWhsJobLevelCharge: true);
				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.TRN, isVisible: true, isShowProductNumber: false, isShowIsOnPallets: false, isShowIsWhsJobLevelCharge: false);
				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.TBC, isVisible: true, isShowProductNumber: false, isShowIsOnPallets: false, isShowIsWhsJobLevelCharge: false);
				AssertRateLinesGridColumns(form, RatingConstants.RateCategory.TWU, isVisible: true, isShowProductNumber: false, isShowIsOnPallets: true, isShowIsWhsJobLevelCharge: true);
			}
		}

		void AssertRateLinesGridColumns(ActiveRatesForm form, string category, bool isVisible, bool isShowProductNumber, bool isShowIsOnPallets, bool isShowIsWhsJobLevelCharge)
		{
			form.BaseTabControl.SelectTabPage(category);
			var tab = form.BaseTabControl.FindTabPage(category);

			if (isVisible)
			{
				var control = tab.RateLinesAndItemsControl;
				Assert(control.RateLinesGrid_ForTest.Columns.Count > 0);
				AssertEquals(isShowProductNumber, control.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_OP_ProductNumber));
				AssertEquals(isShowIsOnPallets, control.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_IsOnPallets));
				AssertEquals(isShowIsWhsJobLevelCharge, control.RateLinesGrid_ForTest.Columns.Contains(RateLine.Schema.TL_IsWhsJobLevelCharge));
			}
			else
			{
				AssertNull(tab);
			}
		}

		public void TestForwardingCostingsHaveJobLevelChargeColumn()
		{
			var costing = Factory.New<Costing>();
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new CostingForm(costing))
				{
					var tabControl = form.BaseTabControl.TopLevelTabControl;
					form.Show();

					foreach (var rateCategory in RatingConstants.RateCategory.RateCategories)
					{
						var fieldInfo = typeof(RatingConstants.RateCategory).GetField(rateCategory, BindingFlags.Public | BindingFlags.Static);
						var rateTypeAttribute = (RateTypeAttribute)fieldInfo.GetCustomAttributes(typeof(RateTypeAttribute), false)[0];
						var expectingJobLevelChargeColumn = rateTypeAttribute.RateType == RateType.Forwarding
							|| rateTypeAttribute.RateType == RateType.Customs
							|| rateTypeAttribute.RateType == RateType.Warehouse
							|| rateTypeAttribute.RateType == RateType.TransitWarehouse
							|| rateTypeAttribute.RateType == RateType.TransitWarehouseTransportationUnit;
						AssertRateLinesGridContainsColumn(form, rateCategory, RateLine.Schema.TL_IsWhsJobLevelCharge, expectingJobLevelChargeColumn);
					}
				}

				var rate = Factory.New<ClientRate>();
				using (var form = new ActiveRatesForm(rate))
				{
					var tabControl = form.BaseTabControl.TopLevelTabControl;
					form.Show();

					AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.AIR, RateLine.Schema.TL_IsWhsJobLevelCharge, false);
					AssertRateLinesGridContainsColumn(form, RatingConstants.RateCategory.CAI, RateLine.Schema.TL_IsWhsJobLevelCharge, false);
				}
			}
		}

		void AssertRateLinesGridContainsColumn(RatingForm form, string rateCategory, string columnName, bool shouldContain)
		{
			form.BaseTabControl.SelectTabPage(rateCategory);
			var tab = form.BaseTabControl.FindTabPage(rateCategory);
			if (tab != null)
			{
				var control = tab.RateLinesAndItemsControl;
				AssertNotNull(control);
				AssertEquals(shouldContain, control.RateLinesGrid_ForTest.Columns.Contains(columnName));
			}
		}

		public void TestInsertNewChargeLineMenuItem()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			RateEntry entry = rate.AIRRateEntriesForBinding.AddNew();

			for (int i = 0; i < 3; i++)
			{
				RateLine line = entry.RateLines.AddNew();
				line.FillWithValidTestData();
			}
			Factory.Save();

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			{
				ZTabControl tabControl = form.BaseTabControl.TopLevelTabControl;
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				RateLinesAndItemsControl airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;

				MenuItem insertMenuItem = null;
				InvokePopUp(airControl);
				foreach (MenuItem menuItem in airControl.RateLinesGrid_ForTest.ContextMenu.MenuItems)
				{
					if (menuItem.Text == airControl.InsertNewCharge)
					{
						insertMenuItem = menuItem;
						break;
					}
				}

				AssertNotNull(insertMenuItem);
				insertMenuItem.PerformClick();

				airControl.RateLinesGrid_ForTest.ListManager.Position = 3;
				insertMenuItem.PerformClick();

				AssertEquals("2 new items + 4 existing", 6, entry.RateLines.Count);
				AssertEquals("Correct line order", (byte)0, entry.RateLines[0].TL_LineOrder);
				AssertEquals((byte)1, entry.RateLines[1].TL_LineOrder);
				AssertEquals((byte)2, entry.RateLines[2].TL_LineOrder);
				AssertEquals((byte)3, entry.RateLines[3].TL_LineOrder);
				AssertEquals((byte)4, entry.RateLines[4].TL_LineOrder);
				AssertEquals((byte)5, entry.RateLines[5].TL_LineOrder);

				AssertEquals("New items in correct position", false, entry.RateLines[0].IsInDatabase);
				AssertEquals("New items in correct position", false, entry.RateLines[3].IsInDatabase);

				AssertEquals("Relationships not lost", entry.PK, entry.RateLines[0].TL_TI);
				AssertEquals("Relationships not lost", entry.PK, entry.RateLines[3].TL_TI);
			}
		}

		public void TestInsertNewChargeLineMenuItem_DoesNotAppearOnReadonlyCollections()
		{
			var isAllowed = Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed;
			try
			{
				Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed = false;
				var globalFRTChargeCode = new TestHelper(Factory).ChargeCodes.CreateGlobalCharge("GLBCHRG");
				var globalTariff = Factory.NewWithValidTestData<GlobalTariff>();
				var entry1 = globalTariff.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AU", "");
				entry1.RateLines.RemoveAndDeleteAll();

				var line1 = entry1.AddRateLine(globalFRTChargeCode, UnitCalculator.Code, QuantityUnit.KG);
				line1.GetCalculator<UnitCalculator>().PerUnit = 100;

				var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
				anotherCompany.GC_Code = "ANC";
				var anotherBranch = Factory.NewWithValidTestData<GlbBranch>();
				anotherCompany.Branches.Add(anotherBranch);
				Factory.Save();

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, anotherBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
				{
					var entry2 = globalTariff.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "CN", "");
					var line2 = entry2.AddRateLine(globalFRTChargeCode, FlatCalculator.Code, QuantityUnit.KG);
					line2.GetCalculator<FlatCalculator>().BaseRate = 200;
				}
				Factory.Save();

				using (var form = new GlobalTariffsForm(globalTariff))
				{
					form.Show();

					form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
					var airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;
					InvokePopUp(airControl);

					var menuItem = airControl.RateLinesGrid_ForTest.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(i => i.Text == airControl.InsertNewCharge);
					AssertNotNull(menuItem);
					AssertEquals(true, menuItem.Visible);

					form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.LCL);
					var lclControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.LCL).RateLinesAndItemsControl;
					lclControl.RateLinesGrid_ForTest.Select(0);

					InvokePopUp(lclControl);

					menuItem = lclControl.RateLinesGrid_ForTest.ContextMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(i => i.Text == lclControl.InsertNewCharge);
					AssertNotNull(menuItem);
					AssertEquals(false, menuItem.Visible);
				}
			}
			finally
			{
				Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed = isAllowed;
			}
		}

		public void TestCompanyTariffDeleteMenuItem()
		{
			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_GlobalRateLevel = 1;
			RateEntry entry = tariff.AIRRateEntriesForBinding.AddNew();

			RateLine line = entry.RateLines.AddNew();
			line.FillWithValidTestData();

			Factory.Save();

			using (GlobalTariffsForm form = new GlobalTariffsForm(tariff))
			{
				ZTabControl tabControl = form.BaseTabControl.TopLevelTabControl;
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				RateLinesAndItemsControl airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;

				MenuItem deleteMenuItem = airControl.RateLinesGrid_ForTest.DeleteMenuItem;
				MenuItem deleteOverrideMenuItem = null;
				MenuItem overrideMenuItem = null;

				airControl.RateLinesGrid_ForTest.Select(0);
				InvokePopUp(airControl);

				deleteOverrideMenuItem = FindMenuItemByText(airControl, airControl.DeleteOverrideText);
				overrideMenuItem = FindMenuItemByText(airControl, airControl.OverrideText);

				AssertEquals(true, deleteMenuItem.Visible);
				AssertEquals(false, deleteOverrideMenuItem.Visible);
				AssertEquals(false, overrideMenuItem.Visible);

				int originRatelinesCount = entry.RateLines.Count;
				DataGrid.HitTestInfo mouseInfo = airControl.RateLinesGrid_ForTest.HitTest(0, 0);
				airControl.RateLinesGrid_ForTest.GetType().InvokeMember("MouseUpInfo",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, Type.DefaultBinder, airControl.RateLinesGrid_ForTest, new object[] { mouseInfo });

				deleteMenuItem.PerformClick();

				AssertEquals("Rateline is deleted", originRatelinesCount - 1, entry.RateLines.Count);
			}
		}

		public void TestCompanyTariffDeleteOverrideMenuItem()
		{
			var tariff1 = Factory.New<CompanyTariff>();
			var entry1 = tariff1.AddRateEntry("AIR");
			entry1.RateLines.RemoveAndDeleteAll();
			entry1.AddRateLine("CAF");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var tariff2 = newFactory.New<CompanyTariff>();

			using (var form = new GlobalTariffsForm(tariff2))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;

				var deleteMenuItem = airControl.RateLinesGrid_ForTest.DeleteMenuItem;

				airControl.RateLinesGrid_ForTest.Select(0);
				InvokePopUp(airControl);

				var deleteOverrideMenuItem = FindMenuItemByText(airControl, airControl.DeleteOverrideText);
				var overrideMenuItem = FindMenuItemByText(airControl, airControl.OverrideText);

				AssertEquals(false, deleteMenuItem.Visible);
				AssertEquals(false, deleteOverrideMenuItem.Visible);
				AssertEquals(true, overrideMenuItem.Visible);

				var mouseInfo = airControl.RateLinesGrid_ForTest.HitTest(0, 0);
				airControl.RateLinesGrid_ForTest.GetType().InvokeMember("MouseUpInfo",
					BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetField, Type.DefaultBinder, airControl.RateLinesGrid_ForTest, new object[] { mouseInfo });

				overrideMenuItem.PerformClick();

				airControl.RateLinesGrid_ForTest.Select(0);
				InvokePopUp(airControl);
				AssertEquals(false, deleteMenuItem.Visible);
				AssertEquals(true, deleteOverrideMenuItem.Visible);
				AssertEquals(false, overrideMenuItem.Visible);

				deleteOverrideMenuItem.PerformClick();

				airControl.RateLinesGrid_ForTest.Select(0);
				InvokePopUp(airControl);
				AssertEquals(false, deleteMenuItem.Visible);
				AssertEquals(false, deleteOverrideMenuItem.Visible);
				AssertEquals(true, overrideMenuItem.Visible);
			}
		}

		public void TestWiseRatesViewSpecificColumns()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "SCAC";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.OH_RSL_ShippingLine = shippingLine.PK;

			Factory.Save();

			var invalidCosting = new Rate
			{
				Carrier = carrier.OH_Code,
				Charges = new List<Charge>(),
				ContainerMode = "YYY",
				Destination = "XYYXX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "ABCDE",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "XXX",
				Provider = WRConstants.RateProviders.WTG
			};

			invalidCosting.Charges.Add(new Charge { ChargeCode = "ZZZX", Currency = "BTC", FlatRate = 8m });

			var validCosting = new Rate
			{
				Carrier = carrier.OH_Code,
				Charges = new List<Charge>(),
				ContainerMode = "FCL",
				Destination = "USLAX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "AUSYD",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "SEA",
				Provider = WRConstants.RateProviders.WTG
			};
			validCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { invalidCosting, validCosting },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "ZZZX" }, new RefChargeCode { Code = "FRT" } },
			};

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var tokenProviderMock = new Mock<IAuthTokenProvider>();

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);

			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();
				Application.DoEvents();
				form.FilterControlForTest.Find();

				Assert("Should load rates", wiseRatingHeaderView.WiseEntryViews.Any());

				var panel = form.FilterControlForTest.Controls.Find("WiseRatesViewPanel", true)[0] as WiseRatesViewPanel;
				var grid = panel.RateLinesAndItemsControl.RateLinesGrid_ForTest;

				Assert(grid.Columns.Any(x => x.ColumnName == "Comment"));
				Assert(grid.Columns.Any(x => x.ColumnName == "UniversalChargeCodes"));
				Assert(grid.Columns.Any(x => x.ColumnName == "CarrierChargeCode"));
				Assert(grid.Columns.Any(x => x.ColumnName == "ConversionFactorForBinding+ConversionFactorString"));
				Assert(grid.Columns.Any(x => x.ColumnName == "UnitMultipleAsString"));
			}
		}

		public void TestAgentRatesCheckBoxShouldNotBeVisibleOnWiseRatesForm()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Emirates";
			carrier.SetCustomsCode(
				OrgCusCode.CodeTypes.CarrierCode,
				RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedStates),
				"SCAC");

			Factory.Save();

			var validCosting = new Rate
			{
				Carrier = carrier.OH_Code,
				Charges = new List<Charge>(),
				ContainerMode = "FCL",
				Destination = "USLAX",
				IssueDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				Origin = "AUSYD",
				StartDate = ZDateTime.Now.AddMonths(-3).ToDateTime(),
				TransportMode = "SEA",
				Provider = WRConstants.RateProviders.WTG
			};
			validCosting.Charges.Add(new Charge { ChargeCode = "FRT", Currency = "AUD", FlatRate = 8m });

			var ratesSearchResponse = new RatesSearchResponse
			{
				Rates = new[] { validCosting },
				Carriers = new[]
				{
					new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
				},
				ChargeCodes = new[] { new RefChargeCode { Code = "ZZZX" }, new RefChargeCode { Code = "FRT" } },
			};

			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.SearchAsync(It.IsAny<RatesSearchRequest>(), It.IsAny<string>(),
					It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(ratesSearchResponse));

			var clientFactoryMock = new Mock<IWiseRatesClientFactory>();
			clientFactoryMock
				.Setup(m => m.TryCreate(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, string.Empty));

			var logger = new TestLogger();
			var testWiseRatesProvider = new WiseRatesProvider(Factory, clientFactoryMock.Object, logger);
			var wiseRatingHeaderView = new WiseRatingHeaderView(testWiseRatesProvider, Factory, logger);

			using (var form = new WiseRatesFormForTest(wiseRatingHeaderView))
			{
				form.Show();
				Application.DoEvents();
				form.FilterControlForTest.Find();

				Assert("Should load rates", wiseRatingHeaderView.WiseEntryViews.Any());

				var panel = form.FilterControlForTest.Controls.Find("WiseRatesViewPanel", true)[0] as WiseRatesViewPanel;
				Assert(!panel.RateLinesAndItemsControl.CalculatorPanelAgentRatesCheckBoxVisible);
			}
		}

		public void TestAgentRatesCheckBoxShouldBeVisibleOnRatingForm()
		{
			var rate = Factory.New<ClientRate>();

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;
				Assert(airControl.CalculatorPanelAgentRatesCheckBoxVisible);

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.CAI);
				var caiControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.CAI).RateLinesAndItemsControl;
				Assert(caiControl.CalculatorPanelAgentRatesCheckBoxVisible);
			}
		}

		public void TestColourDeciding()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();
			var line = entry.RateLines.AddNew();
			line.TL_RateEndDate = ZDate.Today.AddDays(5);

			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var lineControl = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateLinesAndItemsControl;

				var args = new ColourDecidingEventArgs(line);
				lineControl.RateLinesGrid_ColourDeciding_ForTest(args);
				AssertEquals(Color.Empty, args.Colour);

				line.TL_RateEndDate = ZDate.Today.AddDays(-2);
				lineControl.RateLinesGrid_ColourDeciding_ForTest(args);
				AssertEquals(Color.PaleGoldenrod, args.Colour);
			}
		}

		MenuItem FindMenuItemByText(RateLinesAndItemsControl airControl, string menuText)
		{
			foreach (MenuItem menuItem in airControl.RateLinesGrid_ForTest.ContextMenu.MenuItems)
			{
				if (menuItem.Text == menuText)
				{
					return menuItem;
				}
			}
			return null;
		}

		void InvokePopUp(RateLinesAndItemsControl control)
		{
			control.RateLinesGrid_ForTest.ContextMenu
				.GetType()
				.GetMethod("OnPopup", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(control.RateLinesGrid_ForTest.ContextMenu, new object[] { EventArgs.Empty });
		}

		[TestClass]
		class WiseRatesFormForTest : WiseRatesForm
		{
			public WiseRatesFormForTest(WiseRatingHeaderView wiseRatingHeaderView) : base(wiseRatingHeaderView)
			{
			}

			public WiseRatesFilterStripControl FilterControlForTest => FilterControl;
		}
	}
}
