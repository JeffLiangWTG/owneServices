using System.ComponentModel;
using System.Drawing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateEntryPanelTest : RatingTestCase
	{
		public void TestIsTactRateColumn()
		{
			var rate = Factory.New<ClientRate>();
			using (var form = new ActiveRatesForm(rate))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airRateEntryGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateEntryGrid;
				Assert(!airRateEntryGrid.Columns.Contains("TI_IsTact"));
			}

			var specificCosting = Factory.New<Costing>();
			specificCosting.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			using (var form = new CostingForm(specificCosting))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airRateEntryGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateEntryGrid;
				Assert(!airRateEntryGrid.Columns.Contains("TI_IsTact"));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
				var fclRateEntryGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.FCL).RateEntryGrid;
				Assert(!fclRateEntryGrid.Columns.Contains("TI_IsTact"));
			}

			var standardCosting = Factory.New<Costing>();
			using (var form = new CostingForm(standardCosting))
			{
				form.Show();

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.AIR);
				var airRateEntryGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.AIR).RateEntryGrid;
				Assert(airRateEntryGrid.Columns.Contains("TI_IsTact"));

				form.BaseTabControl.SelectTabPage(RatingConstants.RateCategory.FCL);
				var fclRateEntryGrid = form.BaseTabControl.FindTabPage(RatingConstants.RateCategory.FCL).RateEntryGrid;
				Assert(!fclRateEntryGrid.Columns.Contains("TI_IsTact"));
			}
		}

		public void TestCategory()
		{
			using (TestRateEntryPanel panel = new TestRateEntryPanel())
			{
				panel.Category = RatingConstants.RateCategory.AIR;
				AssertEquals(RatingConstants.RateCategory.AIR, panel.RateLinesAndItemsControl.Category);

				panel.Category = RatingConstants.RateCategory.FCL;
				AssertEquals(RatingConstants.RateCategory.FCL, panel.RateLinesAndItemsControl.Category);
			}
		}

		public void TestColorDecidingWhenClientRateIsAccepting()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "NK");
			rateEntry.TI_RateStartDate = ZDate.Today.AddDays(-20);
			rateEntry.TI_RateEndDate = ZDate.Empty;

			var quote = Helper.NewQuote(rate.Header);
			quote.TH_QuoteDate = ZDate.Today.AddDays(-15);
			quote.TH_QuoteEndDate = ZDate.Today.AddDays(5);
			quote.AddRateEntry("AIR", "LSE", "AUSYD", "NK");

			quote.TryAcceptQuote(out rate);

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			using (TestRateEntryPanel panel = new TestRateEntryPanel())
			{
				form.Controls.Add(panel);
				var args = new ColourDecidingEventArgs(rateEntry);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.LightGreen, args.Colour);
			}
		}

		public void TestColourDeciding()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			using (TestRateEntryPanel panel = new TestRateEntryPanel())
			{
				var args = new ColourDecidingEventArgs(entry);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.TI_RateEndDate = ZDate.Today.AddDays(-2);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.PaleGoldenrod, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.TI_RateEndDate = ZDate.Today.AddDays(2);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.IsAccepting = true;
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				form.Controls.Add(panel);
				args = new ColourDecidingEventArgs(entry);
				entry.IsAccepting = true;
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.LightGreen, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				Factory.Save();
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);
			}
		}

		public void TestColourDecidingWhenIsPublished()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AIRRateEntriesForBinding.AddNew();

			using (ActiveRatesForm form = new ActiveRatesForm(rate))
			using (TestRateEntryPanel panel = new TestRateEntryPanel())
			{
				var args = new ColourDecidingEventArgs(entry);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.IsPublished = true;
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Lavender, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.IsPublished = false;
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(entry);
				entry.IsPublished = true;
				entry.TI_RateStartDate = ZDate.Today.AddDays(-8);
				entry.TI_RateEndDate = ZDate.Today.AddDays(-2);

				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals("Expired colour should override is published", Color.PaleGoldenrod, args.Colour);
			}
		}

		public void TestColourDecidingOnDeletedEntry()
		{
			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry = rate.AIRRateEntriesForBinding.AddNew();
			entry.Delete();

			using (TestRateEntryPanel panel = new TestRateEntryPanel())
			{
				var args = new ColourDecidingEventArgs(entry);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);
			}
		}

		public void TestVisibilityOfRateLinesAndSecurityMessage()
		{
			var setupResult = RateSecurityTestHelper.GetTwoRateSecurityGroups(Factory);

			var rate = Helper.NewClientRate(Helper.NewOrgHeader());

			var entry1 = rate.AddRateEntry("AIR");
			entry1.TI_OH_Consignee = setupResult.DeniedOrg.PK;

			var entry2 = rate.AddRateEntry("AIR");
			entry2.TI_OH_Consignee = setupResult.AllowedOrg.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(setupResult.Staff.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.CachingEnabled = false;

				using (var form = new ActiveRatesForm(rate))
				using (var dummyRateEntryPanel = new DummyRateEntryPanel())
				{
					form.Controls.Add(dummyRateEntryPanel);
					form.Show();

					rate.SelectedFilterCategory = ZString.Empty;
					rate.SummaryRateEntries.Load();

					AssertEquals(2, dummyRateEntryPanel.RateEntryGrid.List.Count);

					dummyRateEntryPanel.RateEntryGrid.ListManager.Position = 1;
					Assert(!dummyRateEntryPanel.IsSecurityMessageLabelVisible);
					Assert(dummyRateEntryPanel.IsPanelVisible);

					dummyRateEntryPanel.RateEntryGrid.ListManager.Position = 0;
					Assert(dummyRateEntryPanel.IsSecurityMessageLabelVisible);
					Assert(!dummyRateEntryPanel.IsPanelVisible);

					var entry3 = rate.AddRateEntry("AIR", "LSE", "AUSYD", "AUBNE");
					entry3.TI_OH_Consignee = setupResult.DeniedOrg.PK;

					rate.SummaryRateEntries.Load();

					AssertEquals(3, dummyRateEntryPanel.RateEntryGrid.List.Count);

					dummyRateEntryPanel.RateEntryGrid.ListManager.Position = 2;
					Assert(!dummyRateEntryPanel.IsSecurityMessageLabelVisible);
					Assert(dummyRateEntryPanel.IsPanelVisible);

					dummyRateEntryPanel.RateEntryGrid.ListManager.Position = 1;
					Factory.Save();

					dummyRateEntryPanel.RateEntryGrid.ListManager.Position = 2;
					Assert(dummyRateEntryPanel.IsSecurityMessageLabelVisible);
					Assert(!dummyRateEntryPanel.IsPanelVisible);
				}
			}
		}

		public class DummyRateEntryPanel : RateEntryPanel
		{
			public DummyRateEntryPanel()
			{
				summaryRateEntryGrid = new ZGrid();
				((ISupportInitialize)(BindingSource)).BeginInit();
				((ISupportInitialize)(summaryRateEntryGrid)).BeginInit();
				SuspendLayout();

				BindingSource.DataSourceType = typeof(RatingHeader);

				EntryGridPanel.Controls.Add(summaryRateEntryGrid);

				BindingSource.SetBindingMember(summaryRateEntryGrid, "SummaryRateEntries");
				summaryRateEntryGrid.GridId = "8F8A49E2-E9CA-43AE-B41D-197FD69B08E3";
				summaryRateEntryGrid.LayoutKey = "SummaryRateEntryGrid";
				summaryRateEntryGrid.Name = "SummaryRateEntryGrid";

				CaptionRenderingEnabled = true;
				Name = "DummyRateEntryPanel";
				((ISupportInitialize)(BindingSource)).EndInit();
				((ISupportInitialize)(summaryRateEntryGrid)).EndInit();
				ResumeLayout(false);
			}

			public bool IsPanelVisible
			{
				get { return CurrentEntryRelatedControlsPanel.Visible; }
			}

			public override ZGrid RateEntryGrid
			{
				get { return summaryRateEntryGrid; }
			}

			readonly ZGrid summaryRateEntryGrid;
		}
	}
}
