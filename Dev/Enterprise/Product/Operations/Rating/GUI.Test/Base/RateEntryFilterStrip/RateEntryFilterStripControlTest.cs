using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateEntryFilterStripControlTest : TestCaseWithFactory
	{
		public void TestStrips()
		{
			var clientRate = Factory.New<ClientRate>();
			using (var form = new ActiveRatesForm(clientRate))
			{
				form.Show();

				var filterStripControl = form.Controls.Find("rateEntryFilterStripControl", false)[0] as RateEntryFilterStripControl;
				AssertEquals(1, filterStripControl.Controls.Count);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 2;

				AssertEquals(2, filterStripControl.Controls.Count);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 3;

				AssertEquals(3, filterStripControl.Controls.Count);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 1;
				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 2;

				AssertEquals(1, filterStripControl.Controls.OfType<RateEntryStripControl>().Count(x => x.Visible));
				AssertEquals(3, filterStripControl.Controls.OfType<RateEntryStripControl>().Count(x => !x.Visible));
			}
		}

		/// <summary>
		/// Switch tabs in a rate form should show validation errors for the current tab.
		/// Setup:
		///  - Create a costing having an air TACT rate entry with a validation error, and having a destination rate entry without a validation error.
		///  - When opening the costing form, the AIR tab is selected by default, but the air TACT rate entry is not visible in the grid.
		///  Validation errors are not visible.
		///  - Switch to the destination tab, make a change and run Validate All.
		///  - Switch back to the air tab. The air TACT rate entry is not visible in the grid, hence the validation error is still hidden.
		///  - Change user filter to show the air TACT rate entry.
		/// Assertion:
		///  - The air TACT rate entry is now visible in the grid, and the validation error should be visible.
		/// </summary>
		public void TestCategoryChangeDoesNotHideValidationErrors()
		{
			// Setup: create a costing having an air TACT rate entry with a validation error, and having a destination rate entry without a validation error.
			var costing = Factory.New<Costing>();
			var invalidCarrier = Factory.NewWithValidTestData<OrgHeader>();

			var airFreightEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			airFreightEntry.TI_IsTact = true;
			airFreightEntry.TI_OH_TransportProvider = invalidCarrier.PK;

			var destEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ALL, "AUSYD", "USLAX", "DDOC", 100m, "AUD");
			destEntry.RunPreSaveValidation();
			AssertNoErrors(destEntry);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var ratingHeaderReloaded = newFactory.Load<Costing>(costing.PK);

			// When opening the form, the AIR tab is selected by default
			using var form = new CostingForm(ratingHeaderReloaded);
			form.Show();
			var airCollection = ratingHeaderReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection;
			AssertEquals("entry not loaded since it doesn't meet default filter criteria", 0, airCollection.Count);

			var tabControl = form.BaseTabControl.TopLevelTabControl.TabPages[0].Controls[0] as TabControl;
			var dstTabControl = tabControl.TabPages[4];
			var airTabControl = tabControl.TabPages[0];

			// Switch to the destination tab, make a change and run Validate All.
			tabControl.SelectedTab = dstTabControl;
			Application.DoEvents();
			var dstCollection = ratingHeaderReloaded.EntryCollections[RatingConstants.RateCategory.DST].LazyLoadingCollection;
			AssertEquals(1, dstCollection.Count);
			// Need an edit to trigger full validation
			dstCollection[0].TI_RateStartDate = dstCollection[0].TI_RateStartDate.AddMonths(-1);

			ratingHeaderReloaded.RunPreSaveValidation();

			// Switch back to the air tab. The air TACT rate entry is not visible in the grid, hence the validation error is still hidden.
			tabControl.SelectedTab = airTabControl;
			Application.DoEvents();

			AssertEquals("The AIR rate should not be visible because of the user filter, so the validation error", 0, airCollection.Count);

			// Change the filter to show the TACT rate
			var filterStripControl = form.Controls.Find("rateEntryFilterStripControl", searchAllChildren: false)[0];
			var stripControl = filterStripControl.Controls.Find("RateEntryStripControl", searchAllChildren: false).Single(x => x.Visible) as RateEntryStripControl;
			var filterBizO = stripControl.FilterBusinessObject;
			var includeTactRatesFilter = (ModuleTextFilter)filterBizO[RateEntryFilterUtility.Constants.Codes.IncludeTACTRates];
			includeTactRatesFilter.Property = RateEntryFilterStripBusinessObject.IncludeTACTRatesConstants.Code.TAC;
			stripControl.FirePerformSearch();

			AssertEquals("The AIR TACT rate should be visible now", 1, airCollection.Count);
			AssertHasErrors("The AIR rate should still have errors", airCollection[0].TI_OH_TransportProviderInfo);
		}
	}
}
