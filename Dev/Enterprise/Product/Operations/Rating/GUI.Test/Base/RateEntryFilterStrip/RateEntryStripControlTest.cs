using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateEntryStripControlTest : TestCaseWithFactory
	{
		#region Last Used Layout

		public void TestLastUsedLayout_Quotation()
			=> TestLastUsedLayout
			(
				ratingHeader: TestHelper.NewQuote(TestHelper.NewOrgHeader()),
				getForm: (ratingHeader) => new QuotationForm((Quote)ratingHeader)
			);

		public void TestLastUsedLayout_IntercompanyTariff()
			=> TestLastUsedLayout
			(
				ratingHeader: TestHelper.NewIntercompanyTariff(TestHelper.NewOrgHeader()),
				getForm: (ratingHeader) => new IntercompanyTariffForm((IntercompanyTariff)ratingHeader)
			);

		public void TestLastUsedLayout_CompanyTariff()
			=> TestLastUsedLayout
			(
				ratingHeader: TestHelper.NewCompanyTariff(),
				getForm: (ratingHeader) => new GlobalTariffsForm((CompanyTariff)ratingHeader)
			);

		public void TestLastUsedLayout_ClientRate()
			=> TestLastUsedLayout
			(
				ratingHeader: TestHelper.NewClientRate(TestHelper.NewOrgHeader()),
				getForm: (ratingHeader) => new ActiveRatesForm((ClientRate)ratingHeader)
			);

		public void TestLastUsedLayout_Costing()
			=> TestLastUsedLayout
			(
				ratingHeader: TestHelper.NewCosting(TestHelper.NewOrgHeader()),
				getForm: (ratingHeader) => new CostingForm((Costing)ratingHeader)
			);

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		void TestLastUsedLayout(RatingHeader ratingHeader, Func<RatingHeader, ZForm> getForm)
		{
			using (var form = getForm(ratingHeader))
			{
				form.Show();
				Application.DoEvents();

				var stripControl = GetStripControl(form);

				var filterBusinessObject = stripControl.FilterBusinessObject;
				var filterLayout1 = filterBusinessObject.SaveLayout("FilterLayout1");
				filterBusinessObject.SaveLastUsedLayout(filterLayout1.PK);

				var filterLayout2 = filterBusinessObject.SaveLayout("FilterLayout2");
				filterBusinessObject.SaveLastUsedLayout(filterLayout2.PK);

				filterBusinessObject.LoadLayout(filterLayout1);
			}

			using (var form = getForm(ratingHeader))
			{
				form.Show();
				Application.DoEvents();

				var toolStripSplitButton = GetToolStripSplitButton(form);
				AssertEquals
				(
					"GIVEN FilterLayout1 was LoadLayout last WHEN re-show the form THEN should show FilterLayout1",
					"Find (FilterLayout1)",
					toolStripSplitButton.Text
				);
			}
		}

		static RateEntryStripControl GetStripControl(ZForm form)
		{
			var filterStripControl = form.Controls.Find("rateEntryFilterStripControl", searchAllChildren: false).Single() as RateEntryFilterStripControl;
			return filterStripControl.Controls.OfType<RateEntryStripControl>().Single();
		}

		static ToolStripSplitButton GetToolStripSplitButton(ZForm form)
		{
			var stripControl = GetStripControl(form);
			var toolStrip = stripControl.FindAll<ZToolStrip>().Single(t => t.Name == "ToolStrip");
			return toolStrip.Items.OfType<ToolStripSplitButton>().Single(n => n.Name == "ToolStripFindDropButton");
		}

		#endregion

		public void TestFilteredEntriesMustBeValidated()
		{
			var clientRate = Factory.New<ClientRate>();
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_IsConsignee = true;
			clientRate.TH_OH = client.PK;

			var entry1 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2013, 1, 1);
			entry1.TI_RateEndDate = new ZDate(2013, 12, 31);

			var entry2 = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry2.TI_RateStartDate = new ZDate(2014, 1, 1);
			entry2.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var clientRateReloaded = newFactory.Load<ClientRate>(clientRate.PK);

			AssertEquals("precondition", 0, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Count);

			using (var form = new ActiveRatesForm(clientRateReloaded))
			{
				form.Show();

				AssertEquals(1, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Count);

				// Since nothing has been edited, Save wouldn't be enabled.
				// The only way the user can cause validation is using File > ValidateAll,
				// which first calls MarkAsNeedingValidationIncludingChildren
				clientRateReloaded.MarkAsNeedingValidationIncludingChildren();
				clientRateReloaded.RunPreSaveValidation();

				foreach (var entry in clientRateReloaded.AllEntries)
				{
					AssertHasError(entry.TI_RCInfo, MandatoryValidation.MustBeEnteredMessage(entry.TI_RCInfo.HumanReadableName));
				}

				Assert(clientRateReloaded.HasErrors);

				var filterStripControl = form.Controls.Find("rateEntryFilterStripControl", false)[0] as RateEntryFilterStripControl;
				var stripControl = filterStripControl.Controls.OfType<RateEntryStripControl>().First();

				(stripControl.FilterBusinessObject as IRateEntryFilterStripBusinessObject).ClearRateEntryFilterStrips();
				// Trigger a search action to ensure the collection is loaded with the new filter.
				stripControl.FirePerformSearch();

				AssertEquals(2, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Count);

				foreach (var entry in clientRateReloaded.AllEntries)
				{
					entry.TI_Mode = Core.Constants.RateMode.LSE;
				}

				clientRateReloaded.RunPreSaveValidation();

				foreach (var entry in clientRateReloaded.AllEntries)
				{
					AssertNoErrors(entry.TI_RCInfo);
				}
			}
		}

		public void TestFilterShouldBeAppliedOnCategoryChange()
		{
			var costing = Factory.New<Costing>();

			var entry1 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USLAX");
			entry1.TI_RateStartDate = new ZDate(2013, 1, 1);
			entry1.TI_RateEndDate = ZDate.Empty;

			var entry2 = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUSYD", "USCHI");
			entry2.TI_RateStartDate = new ZDate(2014, 1, 1);
			entry2.TI_RateEndDate = ZDate.Empty;

			var entry3 = costing.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ULD, "AUSYD", "SGSIN");
			entry3.TI_RateStartDate = new ZDate(2014, 1, 1);
			entry3.TI_RateEndDate = ZDate.Empty;

			var entry4 = costing.AddRateEntry(RatingConstants.RateCategory.DST, Core.Constants.RateMode.ULD, "AUSYD", "UAODS");
			entry4.TI_RateStartDate = new ZDate(2014, 1, 1);
			entry4.TI_RateEndDate = ZDate.Empty;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var clientRateReloaded = newFactory.Load<Costing>(costing.PK);

			AssertEquals("precondition", 0, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Count);
			AssertEquals("precondition", 0, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.DST].LazyLoadingCollection.Count);

			using (var form = new CostingForm(clientRateReloaded))
			{
				form.Show();

				AssertEquals(2, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.AIR].LazyLoadingCollection.Count);

				var tabControl = form.BaseTabControl.TopLevelTabControl.TabPages[0].Controls[0] as TabControl;
				if (tabControl != null)
				{
					var dstTabControl = tabControl.TabPages[4];
					if (dstTabControl != null)
					{
						tabControl.SelectedTab = dstTabControl;
						Application.DoEvents();
					}
				}

				AssertEquals(2, clientRateReloaded.EntryCollections[RatingConstants.RateCategory.DST].LazyLoadingCollection.Count);
			}
		}

		public void TestFilterStripControlsAreVisible()
		{
			using (var stripControl = new RateEntryStripControlForTest(Factory.New<Costing>()))
			{
				Assert(stripControl.ManageButtonExposed.Visible);
				Assert(stripControl.SaveButtonExposed.Visible);
				Assert(stripControl.GroupButtonExposed.Visible);
				Assert(stripControl.FindButtonExposed.Visible);
				Assert(!stripControl.ExposedColorPicker.Visible);
				Assert(!stripControl.ExposedGrid.Visible);
				AssertEquals(92, stripControl.MaxFilterStripPanelHeightExposed);
			}
		}

		class RateEntryStripControlForTest : RateEntryStripControl
		{
			public RateEntryStripControlForTest(RatingHeader header)
				: base(header.AIRRateEntriesForBinding)
			{ }

			public ZToolStripSplitButton ManageButtonExposed
			{
				get { return this.ToolStripManageDropButton; }
			}

			public ZToolStripButton SaveButtonExposed
			{
				get { return this.ToolStripSaveLayoutButton; }
			}

			public ZToolStripSplitButton FindButtonExposed
			{
				get { return this.ToolStripFindDropButton; }
			}

			public ZToolStripButton GroupButtonExposed
			{
				get { return this.ToolStripAddGroupButton; }
			}

			public ZFilterGrid ExposedGrid
			{
				get { return this.Grid; }
			}

			public ZToolStrip ExposedColorPicker
			{
				get { return this.ToolStripColourPicker; }
			}

			public int MaxFilterStripPanelHeightExposed
			{
				get { return MaxFilterStripPanelHeight; }
			}
		}
	}
}
