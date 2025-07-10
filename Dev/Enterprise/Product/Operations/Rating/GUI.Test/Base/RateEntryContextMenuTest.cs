using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateEntryContextMenuTest : RatingTestCase
	{
		#region Duplicate

		public void TestContextMenu()
		{
			var cost = Factory.New<Costing>();
			RateEntry entry = cost.AddRateEntry("ORG", "LCL", "AUSYD", "");
			using (CostingForm form = new CostingForm(cost))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				ZGrid orgGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.ORG).RateEntryGrid;

				MenuItem duplicateMenuItem = FindMenuItem(orgGrid, RatingConstants.RateEntryContextMenu.Duplicate);
				AssertNotNull(duplicateMenuItem);

				AssertEquals(1, cost.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
				duplicateMenuItem.PerformClick();
				AssertEquals(2, cost.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.ORG).Count);
			}
		}

		#endregion

		#region CreateQuoteFromTradeLanes

		public void TestCreateQuoteFromTradeLanes()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			RateEntry entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			RateEntry entry2 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "");
			RateEntry entry3a = rate.AddRateEntry("DST", "FCL", "", "NZAKL");
			RateEntry entry3b = rate.AddRateEntry("DST", "FCL", "US", "NZAKL");

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
				ZGrid fclGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.FCL).RateEntryGrid;
				fclGrid.Select(0);

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.DST);
				ZGrid destinationGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.DST).RateEntryGrid;
				destinationGrid.Select(0);
				destinationGrid.Select(1);

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				ZGrid originGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.ORG).RateEntryGrid;
				originGrid.Select(0);

				MenuItem quoteFromCurrentRelatedMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentRelatedMenuItem);
				quoteFromCurrentRelatedMenuItem.PerformClick();
				QuotationForm newForm = RateEntryContextMenu.LastFormShownForNewEntity as QuotationForm;
				AssertNotNull(newForm);
				Quote newQuote = newForm.BusinessEntity as Quote;
				AssertEquals("Related freight entry is copied", 1, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.FCL));
				AssertEquals("Selected origin entry is copied", 1, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.ORG));
				AssertEquals("Unrelated destination entries are NOT copied", 0, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.DST));
				newForm.Dispose();
				RateEntryContextMenu.LastFormShownForNewEntity = null;

				MenuItem quoteFromAllSelectionMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromAllTabs);
				AssertNotNull(quoteFromAllSelectionMenuItem);
				quoteFromAllSelectionMenuItem.PerformClick();
				newForm = RateEntryContextMenu.LastFormShownForNewEntity as QuotationForm;
				AssertNotNull(newForm);
				newQuote = newForm.BusinessEntity as Quote;
				AssertEquals("Selected freight entry is copied", 1, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.FCL));
				AssertEquals("Selected origin entry is copied", 1, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.ORG));
				AssertEquals("Selected destination entries are copied", 2, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.DST));
				newForm.Dispose();
				RateEntryContextMenu.LastFormShownForNewEntity = null;

				MenuItem quoteFromCurrentSelectionMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentSelectionMenuItem);
				quoteFromCurrentSelectionMenuItem.PerformClick();
				newForm = RateEntryContextMenu.LastFormShownForNewEntity as QuotationForm;
				AssertNotNull(newForm);
				newQuote = newForm.BusinessEntity as Quote;
				AssertEquals("Selected freight entry is NOT copied", 0, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.FCL));
				AssertEquals("Selected origin entry is copied", 1, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.ORG));
				AssertEquals("Selected destination entries are NOT copied", 0, newQuote.AllEntries.Count(x => x.TI_RateCategory == RatingConstants.RateCategory.DST));
				newForm.Dispose();
				RateEntryContextMenu.LastFormShownForNewEntity = null;
			}
		}

		public void TestCreateQuoteFromTradeLanes_NotNullRateStartDateAdded()
		{
			int originalQuoteFollowUpDays = RatingDataRegistry.Instance.QuoteFollowUpDays.Value;
			RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 0);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsConsignor = true;

			var rate = Factory.New<ClientRate>();
			rate.TH_OH = orgHeader.PK;
			rate.AddRateEntry(RatingConstants.RateCategory.AIR, "LSE", "AUSYD", "NZAKL");
			Factory.Save();

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				ZGrid grid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateEntryGrid;
				AssertNotNull(grid);

				grid.Select(0);

				MenuItem quoteFromCurrentRelatedMenuItem = FindMenuItem(grid, RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentRelatedMenuItem);

				quoteFromCurrentRelatedMenuItem.PerformClick();
				QuotationForm formWithNullStartDate = RateEntryContextMenu.LastFormShownForNewEntity as QuotationForm;
				AssertNotNull(formWithNullStartDate);

				Quote firstQuote = formWithNullStartDate.BusinessEntity as Quote;
				AssertNotNull(firstQuote);

				quoteFromCurrentRelatedMenuItem.PerformClick();
				QuotationForm formWithNotNullStartDate = RateEntryContextMenu.LastFormShownForNewEntity as QuotationForm;
				AssertNotNull(formWithNotNullStartDate);

				Quote secondQuote = formWithNotNullStartDate.BusinessEntity as Quote;
				AssertNotNull(secondQuote);

				secondQuote.TH_QuoteDate = ZDate.Today;

				try
				{
					AssertNoExceptionThrown(() =>
					{
						formWithNotNullStartDate.FireSaveButton();
					});
					AssertNotEquals("Quotes are in differnt factories", firstQuote.Factory, secondQuote.Factory);
					Assert("TI_QuoteDate of quote are not empty", firstQuote.TH_QuoteDate != ZDate.Empty);
					Assert("TH_QuoteEndDate of quote are not empty", firstQuote.TH_QuoteEndDate != ZDate.Empty);
					Assert("TI_RateStartDate of all entries in the quote are not empty", firstQuote.AllEntries.All(x => x.TI_RateStartDate != ZDate.Empty));
					Assert("TI_RateEndDate of all entries in the quote are not empty", firstQuote.AllEntries.All(x => x.TI_RateEndDate != ZDate.Empty));
				}
				finally
				{
					formWithNotNullStartDate.Dispose();
					formWithNullStartDate.Dispose();
					quoteFromCurrentRelatedMenuItem.Dispose();
					grid.Dispose();
					RateEntryContextMenu.LastFormShownForNewEntity = null;
					RatingDataRegistry.Instance.QuoteFollowUpDays.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, originalQuoteFollowUpDays);
				}
			}
		}

		public void TestMenuItemEnabledAndDisabled()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			RateEntry entry1 = rate.AddRateEntry("FCL", "SEA", "AUSYD", "NZAKL");
			RateEntry entry2 = rate.AddRateEntry("ORG", "FCL", "AUSYD", "");

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
				ZGrid fclGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.FCL).RateEntryGrid;
				fclGrid.Select(0);

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				ZGrid originGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.ORG).RateEntryGrid;

				MenuItem quoteFromCurrentRelatedMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentRelatedMenuItem);
				AssertEquals("Menu item is enabled because there is no selection", false, quoteFromCurrentRelatedMenuItem.Enabled);
				MenuItem quoteFromAllSelectionMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromAllTabs);
				AssertNotNull(quoteFromAllSelectionMenuItem);
				AssertEquals("Menu item is enabled because selection on FCL tab", true, quoteFromAllSelectionMenuItem.Enabled);
				MenuItem quoteFromCurrentSelectionMenuItem = FindMenuItem(originGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentSelectionMenuItem);
				AssertEquals("Menu item is disabled because there is no selection", false, quoteFromCurrentSelectionMenuItem.Enabled);

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
				fclGrid.Select(0);
				quoteFromCurrentRelatedMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentRelatedMenuItem);
				AssertEquals("Menu item is enabled because selection on FCL tab", true, quoteFromCurrentRelatedMenuItem.Enabled);
				quoteFromAllSelectionMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromAllTabs);
				AssertNotNull(quoteFromAllSelectionMenuItem);
				AssertEquals("Menu item is enabled because selection on FCL tab", true, quoteFromAllSelectionMenuItem.Enabled);
				quoteFromCurrentSelectionMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentSelectionMenuItem);
				AssertEquals("Menu item is enabled because selection on FCL tab", true, quoteFromCurrentSelectionMenuItem.Enabled);

				fclGrid.UnSelect(0);
				quoteFromCurrentRelatedMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentRelatedMenuItem);
				AssertEquals("Menu item is enabled because there is no selection", false, quoteFromCurrentRelatedMenuItem.Enabled);
				quoteFromAllSelectionMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromAllTabs);
				AssertNotNull(quoteFromAllSelectionMenuItem);
				AssertEquals("Menu item is disabled because there is no selection", false, quoteFromAllSelectionMenuItem.Enabled);
				quoteFromCurrentSelectionMenuItem = FindMenuItem(fclGrid, RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromCurrentTab);
				AssertNotNull(quoteFromCurrentSelectionMenuItem);
				AssertEquals("Menu item is disabled because there is no selection", false, quoteFromCurrentSelectionMenuItem.Enabled);
			}
		}

		#endregion

		#region Implementation

		MenuItem FindMenuItem(ZGrid currentGrid, string menuText)
		{
			MenuItem result = null;
			currentGrid.ContextMenu.GetType().GetMethod("OnPopup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(currentGrid.ContextMenu, new object[] { EventArgs.Empty });
			foreach (MenuItem menuItem in currentGrid.ContextMenu.MenuItems)
			{
				if (menuItem.Text == menuText)
				{
					result = menuItem;
					break;
				}
			}
			return result;
		}

		#endregion
	}
}
