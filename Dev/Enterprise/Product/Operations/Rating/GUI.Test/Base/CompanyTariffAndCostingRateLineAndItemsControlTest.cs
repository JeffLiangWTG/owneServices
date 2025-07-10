using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class CompanyTariffAndCostingRateLineAndItemsControlTest : RatingTestCase
	{
		public void TestRelatedRateLineAndItems_CostBasedCalculator_RateLineItemsGrid()
		{
			var orgHeader = Helper.NewOrgHeader();
			orgHeader.OH_FullName = "CONSIGNOR";
			orgHeader.OH_Code = "CON";
			orgHeader.OH_IsConsignor = true;

			var costing = Helper.NewCosting(orgHeader);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();
			var costingRateLine = costingEntry.AddRateLine("BAF", CombinedCalculator.Code, QuantityUnit.KG, currencyCode: Core.Constants.CurrencyCodes.UnitedStates);
			var costingCalculator = costingRateLine.GetCalculator<CombinedCalculator>();
			costingCalculator["-4"] = (ZDecimal)50m;
			costingCalculator["+4"] = (ZDecimal)100m;

			var clientRate = Helper.NewClientRate(orgHeader);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			clientRateEntry.TI_OH_Supplier = orgHeader.PK;
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var clientRateLine = clientRateEntry.AddRateLine("BAF", CompanyTariffOrCostBasedCalculator.CostBasedCode);

			var quote = Helper.NewQuote(orgHeader);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			quoteRateEntry.RateLines.RemoveAndDeleteAll();

			Factory.Save();

			using (var form = new QuotationForm(quote))
			{
				form.Show();
				Application.DoEvents();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airTabPage = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR);
				var showRelatedRateLineAndItemsLinkControl = airTabPage.Controls.Find("ShowControlLabel", true).Single() as ZLinkLabel;
				showRelatedRateLineAndItemsLinkControl.PerformClick_ForTest();
				Application.DoEvents();

				var relatedRateLineAndItemsControl = airTabPage.Controls.Find("TemplateCostingRateLineAndItemsControl", true).Single().Controls;
				var relatedRateLineItemsGrid = relatedRateLineAndItemsControl.Find("RateLinesGrid", true)[0] as ZGrid;
				var relatedRateLineItemsGridList = relatedRateLineItemsGrid.ListManager.List;
				var relatedRateLineItemsGridBindingList = (IBindingList)relatedRateLineItemsGridList;
				var rateTypeColumnDescriptor = relatedRateLineItemsGrid.ListManager.GetItemProperties()["RateType"];
				relatedRateLineItemsGridBindingList.ApplySort(rateTypeColumnDescriptor, ListSortDirection.Descending);
				Application.DoEvents();
				AssertSequencesEqual
				(
					"Related RateLines should show Costing and ClientRate in order",
					new[] { "COS", "SAL" },
					relatedRateLineItemsGridList.Cast<RateLine>().Select(rate => $"{rate.ParentRateEntry.ParentRatingHeader.TH_RateType}")
				);

				// Tick off the costing related rate line to show related clientRate rate line
				var costingCheckBox = relatedRateLineAndItemsControl.Find("CostingCheckBox", true).Single() as ZCheckBox;
				AssertEquals("Related RateLines CostingCheckBox Enabled", true, costingCheckBox.Enabled);
				costingCheckBox.Checked = false;
				Application.DoEvents();

				var resultsCheckBox = relatedRateLineAndItemsControl.Find("ResultsCheckBox", true).Single() as ZCheckBox;
				AssertEquals("ClientRate CST-calculator Results-checkbox Enabled", true, resultsCheckBox.Enabled);
				resultsCheckBox.Checked = true; // showing clientRate CostBased calculator results
				Application.DoEvents();

				var combinedControl = relatedRateLineAndItemsControl.Find("CombinedControl", true).Single();
				var rateLineItemsGrid = combinedControl.Controls.Find("RateLineItemsGrid", true).Single();
				AssertEquals("ClientRate CST-calculator results should be shown", true, rateLineItemsGrid.Visible);
				AssertNullOrEmpty
				(
					"GIVEN related Costing with CMB-calculator grid is shown WHEN showing related ClientRate with CST-calculator results grid THEN it should not show exception (Exception occurred while drawing a form: componentType must be sub-class of prop.ComponentType. componentType=Enterprise.Rating.Business.RateLineItem, property.Name=TM_RelevantValue, property.ComponentType=Enterprise.Rating.Business.RelatedRateLineItem Parameter name: componentType.)",
					UnitTestUserNotification.Instance.LastMessage.Text
				);

				var companyTariffOrCostBasedCalculatorControl = relatedRateLineAndItemsControl.Find("CompanyTariffOrCostBasedCalculatorControl", true).SingleOrDefault();
				AssertNull("CompanyTariffOrCostBasedCalculatorControl", companyTariffOrCostBasedCalculatorControl);

				var companyTariffOrCostBasedCalculatorControlRelatedRateLine = relatedRateLineAndItemsControl.Find("CompanyTariffOrCostBasedCalculatorControl-RelatedRateLine", true).SingleOrDefault();
				AssertNotNull("CompanyTariffOrCostBasedCalculatorControl-RelatedRateLine", companyTariffOrCostBasedCalculatorControlRelatedRateLine);
			}
		}

		[ExpectNoExceptions]
		public void TestLoadEmptyCombinedCalculator_CompanyTariffAndCostingRateLineAndItemsControl()
		{
			var costing = Factory.New<Costing>();
			var costEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "ZA", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var rateLineCosting = costEntry.AddRateLine("FRT", CombinedCalculator.Code, QuantityUnit.KG, currencyCode: Core.Constants.CurrencyCodes.UnitedStates);
			rateLineCosting.GetCalculator<CombinedCalculator>().RateLineItems.RemoveAndDeleteAll();

			Factory.Save();

			var clientRate = Factory.New<ClientRate>();
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "ZA", "");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var rateLineClientRate = clientRateEntry.AddRateLine("FRT", FlatCalculator.Code, QuantityUnit.KG, currencyCode: Core.Constants.CurrencyCodes.UnitedStates);
			rateLineClientRate.GetCalculator<FlatCalculator>().BaseRate = 10m;

			using (var form = new ActiveRatesForm(clientRate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var tabAIR = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR);
				var linkControl = tabAIR.Controls.Find("ShowControlLabel", true).Single() as ZLinkLabel;
				linkControl.PerformClick_ForTest();

				var ratelineItemGrid = tabAIR.Controls.Find("RateLineItemsGrid", true).Single() as ZGrid;
				var datasource = ratelineItemGrid.DataSource as ClientRate;
				var rateEntry = datasource.AIRRateEntriesForBinding.First() as RateEntry;
				var rateLine = rateEntry.RelatedRateLines.First() as RateLine;
				AssertEquals(true, rateLine.ReadOnly);
				var ratelineItems = rateLine.ViewCalculator.RateLineItems;
				AssertEquals("Data source for Rate Line Items Grid should be empty", 0, ratelineItems.Count);
			}
		}

		public void TestRelatedRateLineAndItems_AcceptCost_MakeSureAccepted()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			costingEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costingEntry.AddRateLine("BAF", CombinedCalculator.Code, QuantityUnit.KG, currencyCode: Core.Constants.CurrencyCodes.UnitedStates);

			var quote = Helper.NewQuote(TransportProvider1);
			var quoteRateEntry = quote.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			quoteRateEntry.RateLines.RemoveAndDeleteAll();

			Factory.Save();

			using (var form = new QuotationForm(quote))
			{
				form.Show();
				Application.DoEvents();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airTabPage = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR);
				var showRelatedRateLineAndItemsLinkControl = airTabPage.Controls.Find("ShowControlLabel", true).Single() as ZLinkLabel;
				showRelatedRateLineAndItemsLinkControl.PerformClick_ForTest();
				Application.DoEvents();

				var relatedRateLineAndItemsControl = airTabPage.Controls.Find("TemplateCostingRateLineAndItemsControl", true)[0].Controls;
				var relatedRateLineItemsGrid = relatedRateLineAndItemsControl.Find("RateLinesGrid", true)[0] as ZGrid;
				relatedRateLineItemsGrid.SelectSingleElement(costLine);
				var accept = relatedRateLineItemsGrid.ContextMenu.MenuItems.FindByText("Accept");
				accept.PerformClick();

				var quoteLine = quoteRateEntry.RateLines[0];
				var cstCalculator = quoteLine.GetCalculator<CompanyTariffOrCostBasedCalculator>();
				AssertEquals(costLine.PK.ToString(), cstCalculator.ApplyToLine);
			}
		}
	}
}
