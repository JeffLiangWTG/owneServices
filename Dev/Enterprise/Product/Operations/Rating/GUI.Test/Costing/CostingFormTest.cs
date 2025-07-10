using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Costing = Enterprise.Rating.Business.Costing;

namespace Enterprise.Rating.GUI.Testing
{
	public class CostingFormTest : RatingTestCase
	{
		public void TestDelete_GivenNewRateLineItem_ThenShouldAbleToDeleteWithoutError()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			var rateEntry = costing.AddRateEntryWithCMBRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX", "BAF", 1m, QuantityUnit.KG, "", "", "", ("-50", 5m), ("+50", 10m), ("+150", 15m));

			// Simulate old rates that doesn't have BreaksPer RateLineItem
			var breaksPerRateLineItem = rateEntry.RateLines[0].RateLineItems.Cast<RateLineItem>().Single(rateLineItem => rateLineItem.TM_Type == Calculator.Items.BreaksPer);
			breaksPerRateLineItem.Delete();

			Factory.Save();

			var loadedCosting = new BusinessObjectFactory().Load<Costing>(costing.PK);

			using (var form = new CostingForm(loadedCosting))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.AcceptButton.PerformClick();
			}

			CombineAssertions("Should able to delete without error", () =>
			{
				AssertEquals("LastMessage", "You are about to delete this record permanently from the system. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsDeleted", true, loadedCosting.IsDeleted);
			});
		}

		public void TestFiltersShouldBeEnabledInViewMode()
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			using (var form = new CostingForm(costing))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();

				var filterControl = (IReadOnlyToggleControl)form.rateEntryFilterStripControl;
				AssertEquals(false, filterControl.ReadOnly);
			}
		}

		[GuiTest]
		[TestDate(2025, 01, 01)]
		public void TestOnSave_ValidateContractNumberLinked()
		{
			Factory.RefreshEnabled = false;

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;
			var newHelper = new TestHelper(newFactory);

			FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var today = ZDate.Today;
			var someOrg = newHelper.NewOrgHeader("some");

			var costingInNewFactory = newHelper.NewCosting(someOrg);
			costingInNewFactory.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUSYD", "AUBNE", "FRT", 100, commodity: "HAZ");
			costingInNewFactory.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUBNE", "AUMEL", "FRT", 100, commodity: "HAZ");

			var contract = newHelper.NewRatingContract(someOrg, "contract", Core.Constants.RatingContractTypes.Provider, today.AddMonths(-2), today.AddYears(1), transportMode: Core.Constants.TransportModes.Sea);
			contract.RCT_AllowHazardousCommodities = false;

			newFactory.Save();

			var hazCommodity = Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, "HAZ");
			hazCommodity.RH_IsHazardous = false;
			Factory.Save();

			var costing = Factory.Load<Costing>(costingInNewFactory.PK);

			// When the registry value is false, form save runs RatingHeader.RunPreSaveValidationCore() => RatingHeader.GetEntriesToValidate()
			// which loads all the rates from the database for a full on-memory validation.
			// That defeats the purpose of validating only the loaded and changed rates.
			_ = RatingDataRegistry.Instance.ValidateOnlyLoadedRatesUponSaving.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using var form = new TestCostingForm(costing);
			form.Show();
			Application.DoEvents();

			form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
			var entry1 = costing.EntryCollections[RatingConstants.RateCategory.FCL].LazyLoadingCollection[0];
			entry1.TI_ContractNumberLinked = true;
			entry1.TI_ContractNumber = "contract";

			// A new rate is added in the new factory after the costing is loaded
			costingInNewFactory.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Constants.RateMode.SEA, "AUPER", "AUADL", "FRT", 100, commodity: "HAZ");
			newFactory.Save();

			form.FireSaveButton();

			// Initially, both the contract and the commodity aligned on hazardousness.
			// However, now the commodity is Hazardous. But no validation has yet
			// been called as it has changed in the background. The UI form does not know this yet.
			hazCommodity.RH_IsHazardous = true;
			Factory.Save();
			AssertNoErrors("No validation has been called by the commodity changing", entry1);

			// Let's change an unrelated column in the rate and use the factory to save
			entry1.TI_OriginLRC = "AUADL";
			Factory.Save();
			AssertNoErrors("No validation has been called by the commodity changing", entry1);

			// Let's change again and use the form save.
			entry1.TI_OriginLRC = "AUPER";
			form.FireSaveButton();

			var grids = form.Controls.Find("TemplateRateEntryGrid", searchAllChildren: true);
			var entryGrid = grids.OfType<EntryGrid>().Single(grid => grid.EntryCollection is { Count: > 0 });
			var loadedEntries = entryGrid.EntryCollection.Cast<RateEntry>().ToList();

			AssertContainsExactElementsInAnyOrder("Should not load the extra rate entry added in the new factory",
				new[] { "AUPER|AUBNE", "AUBNE|AUMEL" },
				loadedEntries.Select(x => $"{x.TI_OriginLRC}|{x.TI_DestinationLRC}"));

			var theOnlyEntryWithError = loadedEntries.Single(e => e.TI_RH_NKCommodityCodeInfo.HasErrors());
			AssertEquals("The entry with error should be the modified entry", entry1.PK, theOnlyEntryWithError.PK);
			AssertEquals("Commodity (HAZ) is a hazardous commodity but the linked Carrier Contract contract does not allow Hazardous Commodities.", theOnlyEntryWithError.TI_RH_NKCommodityCodeInfo.GetErrors().Single().Message);
		}

		[GuiTest]
		public void TestDefaultTab_GivenTransportModeSEA_ThenShouldShowRateSummaryCategory()
			=> TestDefaultTab(Core.Constants.TransportModes.Sea, RatingConstants.RateCategory.SummaryRatesCategory);

		[GuiTest]
		public void TestDefaultTab_GivenTransportModeAIR_ThenShouldShowRateSummaryCategory()
			=> TestDefaultTab(Core.Constants.TransportModes.Air, RatingConstants.RateCategory.SummaryRatesCategory);

		void TestDefaultTab(string transportMode, string expectedCategory)
		{
			var costing = Helper.NewCosting(Helper.NewOrgHeader());
			Factory.Save();

			RateEntryFilterValueCache.Instance[costing.PK] = new RateEntryFilterValue()
			{
				StartDate = ZDate.Today,
				ExpiryDate = ZDate.Today.AddDays(1),
				ContractNumber = "1",
				TransportMode = transportMode
			};

			using (var form = new TestCostingForm(costing))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals($"The {expectedCategory} category should be selected", expectedCategory, form.BaseTabControl.SelectedCategory);

				// Set to different tab, save filters to cache, activate form.
				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				Application.DoEvents();
				RateEntryFilterValueCache.Instance[costing.PK] = new RateEntryFilterValue()
				{
					StartDate = ZDate.Today,
					ExpiryDate = ZDate.Today.AddDays(1),
					ContractNumber = "2",
					TransportMode = transportMode
				};

				form.DoActivate();
				Application.DoEvents();
				AssertEquals($"The {expectedCategory} category should be selected", expectedCategory, form.BaseTabControl.SelectedCategory);

				// Set to different tab, reactivate.
				AssertNull("Cache values consumed, should be empty now", RateEntryFilterValueCache.Instance[costing.PK]);
				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.ORG);
				Application.DoEvents();

				form.DoActivate();
				Application.DoEvents();
				AssertEquals($"The ORG category should be selected", RatingConstants.RateCategory.ORG, form.BaseTabControl.SelectedCategory);
			}
		}

		[GuiTest]
		[TestDate(2025, 1, 1)]
		public void TestSaveCosting_ShouldNotShowExpiredRates()
		{
			var costing = Helper.NewCosting(null);

			var expiredRate = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "USLAX", "FRT", 100);
			expiredRate.TI_RateStartDate = new ZDate(2024, 1, 1);
			expiredRate.TI_RateEndDate = new ZDate(2024, 2, 1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Constants.RateMode.LSE, "AUSYD", "SGSIN", "FRT", 200);

			Factory.Save();

			using var form = new TestCostingForm(costing);
			form.Show();
			Application.DoEvents();

			var grids = form.Controls.Find("TemplateRateEntryGrid", searchAllChildren: true);
			var entryGrid = grids.OfType<EntryGrid>().Single(grid => grid.EntryCollection is { Count: > 0 });

			var shownEntry = entryGrid.EntryCollection.Single() as RateEntry;
			AssertEquals("Precondition: Should initially show only not-expired rate with the default user filter", "SGSIN", shownEntry.TI_DestinationLRC);

			entryGrid[0, 0] = new ZString("AUBNE");
			Application.DoEvents();

			form.FireSaveButton();

			shownEntry = entryGrid.EntryCollection.Single() as RateEntry;
			AssertEquals("Default user filter should still be honored and the expired rate should not be shown after save", "SGSIN", shownEntry.TI_DestinationLRC);
		}

		// DBCommand.CheckAppTransactionRolledBackInDbServer throws a new TransactionException("Transaction has been rolled back in the server (application transaction count pending reset).") in the test environment
		// because tests start a transaction (which makes fDbConnection.AppTransactionCount > 0 initially) and roll it back at the end of each test.
		// For that reason, UseSnapshotProtection is added to this test to assert the handling duplicate rates error messages.
		[UseSnapshotProtection(skipTransaction: true)]
		[GuiTest]
		[TestDate(2025, 1, 1)]
		public void TestSaveCosting_WithUserFilter_ShouldShowOnlyMatchedRates()
		{
			var costing = Helper.NewCosting(null);

			costing.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "AU", destination: "SG");
			costing.AddRateEntry(RatingConstants.RateCategory.AIR, origin: "AU", destination: "BR");

			Factory.Save();

			using var form = new TestCostingForm(costing);
			form.Show();

			var grids = form.Controls.Find("TemplateRateEntryGrid", searchAllChildren: true);
			var entryGrid = grids.OfType<EntryGrid>().Single(grid => grid.EntryCollection is { Count: > 0 });
			AssertEquals("All rates should be loaded initially", 2, entryGrid.EntryCollection.Count);

			var filterStripControl = form.Controls.Find("rateEntryFilterStripControl", searchAllChildren: false)[0] as RateEntryFilterStripControl;
			filterStripControl.SetRateEntryDefaultFilters(new RateEntryFilterValue { Destination = "SG" });

			// Add a new SG duplicate rate to the being shown one
			var newEntry1 = entryGrid.EntryCollection.AddNew();
			newEntry1.TI_OriginLRC = "AU";
			newEntry1.TI_DestinationLRC = "SG";

			entryGrid.EntryCollection.RefreshBindingIncludingChildren();

			form.FireSaveButton();

			var notifications = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text).ToArray();

			// This message in test is different from the production message. See ZForm.ShowErrorsDialog().
			// The actual message from the ZErrorMessageBox: "There are errors that need to be corrected before this {0} can be {1}."
			// Since the message is in the framework and because the errors are with the visible item, we don't need to warn user with `Duplicate might be hidden due to current filters set up.`
			var expectedMessage = @"There are errors - can't save.";
			AssertCollectionContains($"Saving when the errors are with visible rates, notifications:\n{string.Join("\n", notifications)} should contain \n{expectedMessage}", expectedMessage, notifications);

			UnitTestUserNotification.Instance.ClearMessages();

			filterStripControl.SetRateEntryDefaultFilters(new RateEntryFilterValue { Destination = "BR" });

			// A new SG duplicate rate to the being hidden ones
			var newEntry2 = entryGrid.EntryCollection.AddNew();
			newEntry2.TI_OriginLRC = "AU";
			newEntry2.TI_DestinationLRC = "SG";

			entryGrid.EntryCollection.RefreshBindingIncludingChildren();

			form.FireSaveButton();

			notifications = UnitTestUserNotification.Instance.PreviousMessages.Select(m => m.Text).ToArray();

			// The error message from checking duplicates with hidden rates is different from the previous one.
			expectedMessage = @"Cannot save as there are duplicates / overlapping Rate Entries. Another user has made changes to the Rate Entries that are preventing your changes from being saved.
Duplicate might be hidden due to current filters set up.
Please close and reopen this form in order to continue. Your changes may be lost.";
			AssertCollectionContains($"Saving when the errors are with hidden rates, notifications:\n{string.Join("\n", notifications)} should contain \n{expectedMessage}", expectedMessage, notifications);
		}
	}

	#region Form Basher

	[TestedType(typeof(CostingForm))]
	public class CostingFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Helper.IsMarkAsNeedingValidationSuspended = true;
			Costing cost = Helper.NewFullyPopulatedCosting();
			Helper.IsMarkAsNeedingValidationSuspended = false;

			IDisposable costValidationSuspender = cost.SuspendMarkingAsNeedingValidation();

			Factory.Save();

			costValidationSuspender.Dispose();

			return new CostingForm(cost);
		}

		#region Implementation

		TestHelper Helper
		{
			get { return fHelper ?? (fHelper = new TestHelper(Factory)); }
		}

		TestHelper fHelper;

		#endregion
	}

	#endregion

	internal class TestCostingForm : CostingForm
	{
		public TestCostingForm(Costing costing) : base(costing)
		{
		}

		public void DoActivate()
		{
			OnActivated(EventArgs.Empty);
		}
	}
}
