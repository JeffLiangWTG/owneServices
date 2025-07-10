using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class BaseTabControlTest : TestCaseWithFactory
	{
		public void TestTabPageGrouping()
		{
			TestTabPageGrouping
				(
					enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: false,
					expectedTabPageGrouping: "Forwarding\r\n" +
						"	Air Freight\r\n" +
						"	FCL Freight\r\n" +
						"	LCL/FTL/LTL/COU Freight\r\n" +
						"	Origin Charges\r\n" +
						"	Destination Charges\r\n" +
						"Liner && Agency\r\n" +
						"	Containerized Freight\r\n" +
						"	Non-Containerized Freight\r\n" +
						"	Origin Charges\r\n" +
						"	Destination Charges\r\n" +
						"	Export Container Detention\r\n" +
						"	Import Container Detention\r\n" +
						"CFS\r\n" +
						"	Packing Charges\r\n" +
						"	Unpacking Charges\r\n" +
						"	Container Storage\r\n" +
						"Warehouse\r\n" +
						"	Product Warehouse\r\n" +
						"	Transit Warehouse\r\n" +
						"	Transit Warehouse Transportation Unit\r\n" +
						"Transport\r\n" +
						"	Port Transport\r\n" +
						"	Land Transport\r\n" +
						"Customs\r\n" +
						"	Air Freight\r\n" +
						"	FCL Freight\r\n" +
						"	LCL/FTL/LTL Freight\r\n" +
						"	Origin Charges\r\n" +
						"	Destination Charges\r\n" +
						"Yard\r\n" +
						"	Storage && Handling\r\n" +
						"	Transportation Unit\r\n" +
						"	Maintenance && Repair\r\n" +
						"",
					assertMessage: "Disable registry"
				);

			TestTabPageGrouping
			(
				enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true,
				expectedTabPageGrouping: "Forwarding\r\n" +
					"	Air Freight\r\n" +
					"	FCL Freight\r\n" +
					"	LCL/FTL/LTL/COU Freight\r\n" +
					"	Origin Charges\r\n" +
					"	Destination Charges\r\n" +
					"Liner && Agency\r\n" +
					"	Containerized Freight\r\n" +
					"	Non-Containerized Freight\r\n" +
					"	Origin Charges\r\n" +
					"	Destination Charges\r\n" +
					"	Export Container Detention\r\n" +
					"	Import Container Detention\r\n" +
					"CFS\r\n" +
					"	Packing Charges\r\n" +
					"	Unpacking Charges\r\n" +
					"	Container Storage\r\n" +
					"Warehouse\r\n" +
					"	Product Warehouse\r\n" +
					"	Transit Warehouse\r\n" +
					"	Transit Warehouse Transportation Unit\r\n" +
					"Transport\r\n" +
					"	Port Transport\r\n" +
					"	Land Transport\r\n" +
					"Customs\r\n" +
					"	Air Freight\r\n" +
					"	FCL Freight\r\n" +
					"	LCL/FTL/LTL Freight\r\n" +
					"	Origin Charges\r\n" +
					"	Destination Charges\r\n" +
					"Yard\r\n" +
					"	Storage && Handling\r\n" +
					"	Transportation Unit\r\n" +
					"	Maintenance && Repair\r\n" +
					"",
				assertMessage: "Enable registry"
			);
		}

		void TestTabPageGrouping(bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup, string expectedTabPageGrouping, string assertMessage)
		{
			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup))
			{
				using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
				{
					form.Show();

					var builder = new StringBuilder();

					foreach (TabPage page in form.BaseTabControl.TopLevelTabControl.TabPages)
					{
						if (page is EntryTabPage)
						{
							builder.AppendLine(page.Text);
						}
						else
						{
							var control = page.Controls.Count == 1 ? page.Controls[0] as TabControl : null;

							if (control != null)
							{
								builder.AppendLine(page.Text);

								foreach (TabPage innerPage in control.TabPages)
								{
									if (!(innerPage is EntryTabPage))
									{
										continue;
									}

									builder.Append("\t");
									builder.AppendLine(innerPage.Text);
								}
							}
						}
					}

					AssertMultilineASCIIEquals(assertMessage, expectedTabPageGrouping, builder.ToString());
				}
			}
		}

		public void TestListOfTabs_SupportUser()
		{
			var supportUser = Factory.New<GlbStaff>();
			supportUser.GS_LoginName = User.SupportUserName;
			AssertEquals("Precondition", true, supportUser.IsSupportUser);

			AssertClientRatesTabList(supportUser);
			AssertCompanyTariffTabList(supportUser);
			AssertQuotationTabList(supportUser);
			AssertCostingTabList(supportUser);
			AssertInterCompanyTariffTabList(supportUser);
		}

		public void TestListOfTabs_SupportUser_EnableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup()
		{
			var supportUser = Factory.New<GlbStaff>();
			supportUser.GS_LoginName = User.SupportUserName;
			AssertEquals("Precondition", true, supportUser.IsSupportUser);

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertClientRatesTabList(supportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertCompanyTariffTabList(supportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertQuotationTabList(supportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertCostingTabList(supportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertInterCompanyTariffTabList(supportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
			}
		}

		public void TestListOfTabs_NonSupportUser()
		{
			var nonSupportUser = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, nonSupportUser.IsSupportUser);

			AssertClientRatesTabList(nonSupportUser);
			AssertCompanyTariffTabList(nonSupportUser);
			AssertQuotationTabList(nonSupportUser);
			AssertCostingTabList(nonSupportUser);
			AssertInterCompanyTariffTabList(nonSupportUser);
		}

		public void TestListOfTabs_NonSupportUser_EnableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup()
		{
			var nonSupportUser = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, nonSupportUser.IsSupportUser);

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertClientRatesTabList(nonSupportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertCompanyTariffTabList(nonSupportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertQuotationTabList(nonSupportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertCostingTabList(nonSupportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
				AssertInterCompanyTariffTabList(nonSupportUser, enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup: true);
			}
		}

		void AssertClientRatesTabList(GlbStaff user, bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new ActiveRatesForm(Factory.New<ClientRate>()))
				{
					var expected =
						"Forwarding\r\n" +
						"Liner && Agency\r\n" +
						"CFS\r\n" +
						"Warehouse\r\n" +
						"Transport\r\n" +
						"Customs\r\n" +
						"Yard\r\n" +
						"Rate Summary\r\n" +
						"Document Format\r\n" +
						"Workflow && Tracking\r\n" +
						"Doc Data\r\n" +
						"eDocs\r\n" +
						"Notes\r\n" +
						"Logs\r\n";
					AssertListOfTabs(form, expected);
				}
			}
		}

		void AssertCompanyTariffTabList(GlbStaff user, bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new GlobalTariffsForm(Factory.New<CompanyTariff>()))
				{
					var expected =
						"Forwarding\r\n" +
						"Liner && Agency\r\n" +
						"CFS\r\n" +
						"Warehouse\r\n" +
						"Transport\r\n" +
						"Customs\r\n" +
						"Yard\r\n" +
						"Rate Summary\r\n" +
						"Workflow && Tracking\r\n" +
						"Doc Data\r\n" +
						"eDocs\r\n" +
						"Notes\r\n" +
						"Logs\r\n";
					AssertListOfTabs(form, expected);
				}
			}
		}

		void AssertQuotationTabList(GlbStaff user, bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new QuotationForm(Factory.New<Quote>()))
				{
					var expected =
						"Forwarding\r\n" +
						"Liner && Agency\r\n" +
						"CFS\r\n" +
						"Warehouse\r\n" +
						"Transport\r\n" +
						"Customs\r\n" +
						"Maintenance && Repair\r\n" +
						"Document Format\r\n" +
						"Document Selection\r\n" +
						"Custom Fields\r\n" +
						"Summary\r\n" +
						"Workflow && Tracking\r\n" +
						"Doc Data\r\n" +
						"eDocs\r\n" +
						"Notes\r\n" +
						"Logs\r\n";
					AssertListOfTabs(form, expected);
				}
			}
		}

		void AssertCostingTabList(GlbStaff user, bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new CostingForm(Factory.New<Costing>()))
				{
					var expected =
						"Forwarding\r\n" +
						"Liner && Agency\r\n" +
						"CFS\r\n" +
						"Warehouse\r\n" +
						"Transport\r\n" +
						"Customs\r\n" +
						"Maintenance && Repair\r\n" +
						"Rate Summary\r\n" +
						"Doc Data\r\n" +
						"eDocs\r\n" +
						"Notes\r\n" +
						"Logs\r\n";
					AssertListOfTabs(form, expected);
				}
			}
		}

		void AssertInterCompanyTariffTabList(GlbStaff user, bool enableRegistryAutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = false)
		{
			using (Env.SetTemporaryUserContext(new UserContext(user, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var form = new IntercompanyTariffForm(Factory.New<IntercompanyTariff>()))
				{
					var expected =
						"Forwarding\r\n" +
						"Yard\r\n" +
						"Rate Summary\r\n" +
						"eDocs\r\n" +
						"Notes\r\n" +
						"Logs\r\n";
					AssertListOfTabs(form, expected);
				}
			}
		}

		void AssertListOfTabs(RatingForm form, string expected)
		{
			form.Show();

			StringBuilder builder = new StringBuilder();

			foreach (TabPage page in form.BaseTabControl.TopLevelTabControl.TabPages)
			{
				if (page is TabPage)
				{
					builder.AppendLine(page.Text);
				}
			}

			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestEntryTabPage_NoEmptyCaption()
		{
			var nonSupportUser = Factory.New<GlbStaff>();
			AssertEquals("Precondition", false, nonSupportUser.IsSupportUser);

			using (Env.SetTemporaryUserContext(new UserContext(nonSupportUser, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
			{
				form.Show();

				foreach (TabPage page in form.BaseTabControl.TopLevelTabControl.TabPages)
				{
					EntryTabPage entryPage = page as EntryTabPage;
					if (entryPage != null)
					{
						Assert(!string.IsNullOrEmpty(entryPage.Text));
					}
				}
			}
		}

		public void TestGridIdsUnique()
		{
			List<string> gridIDs = new List<string>();

			using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
			{
				form.Show();

				foreach (TabPage page in form.BaseTabControl.TopLevelTabControl.TabPages)
				{
					EntryTabPage entryPage = page as EntryTabPage;
					if (entryPage != null)
					{
						AssertGridIDsUnique(entryPage, gridIDs);
					}
					else
					{
						TabControl control = page.Controls.Count == 1 ? page.Controls[0] as TabControl : null;

						if (control != null)
						{
							foreach (TabPage innerPage in control.TabPages)
							{
								entryPage = innerPage as EntryTabPage;
								if (entryPage != null)
								{
									AssertGridIDsUnique(entryPage, gridIDs);
								}
							}
						}
					}
				}
			}
		}

		void AssertGridIDsUnique(EntryTabPage tabPage, List<string> gridIDs)
		{
			string gridId = tabPage.RateEntryGrid.GridId;
			AssertEquals("Grid Control ID should not be duplicated across tab pages - " + gridId, false, gridIDs.Contains(gridId));
		}

		public void TestEntryCategories()
		{
			using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
			{
				form.Show();
				AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC", "CYD");
			}

			using (CostingForm form = new CostingForm(Factory.New<Costing>()))
			{
				form.Show();
				AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC");
			}

			using (QuotationForm form = new QuotationForm(Factory.New<Quote>()))
			{
				form.Show();
				AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC");
			}

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
				{
					form.Show();
					AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC", "CYD", "CAI", "CFC", "CLC", "COR", "CDS");
				}

				using (CostingForm form = new CostingForm(Factory.New<Costing>()))
				{
					form.Show();
					AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC", "CAI", "CFC", "CLC", "COR", "CDS");
				}

				using (QuotationForm form = new QuotationForm(Factory.New<Quote>()))
				{
					form.Show();
					AssertContainsRatingTabPage(form, "AIR", "FCL", "LCL", "ORG", "DST", "SCO", "SNC", "SOR", "SED", "SID", "PAC", "UNP", "CST", "WHS", "TRN", "TBC", "CAI", "CFC", "CLC", "COR", "CDS");
				}
			}
		}

		public void TestSelectedTabChanged()
		{
			var clientRate = Factory.New<ClientRate>();
			using (var form = new ActiveRatesForm(clientRate))
			{
				form.Show();

				AssertEquals(RatingConstants.RateCategory.AIR, clientRate.SelectedFilterCategory);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 1;
				AssertEquals(RatingConstants.RateCategory.SCO, clientRate.SelectedFilterCategory);

				((TabControl)form.BaseTabControl.TopLevelTabControl.SelectedTab.Controls[0]).SelectedIndex = 1;
				AssertEquals(RatingConstants.RateCategory.SNC, clientRate.SelectedFilterCategory);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 0;
				AssertEquals(RatingConstants.RateCategory.AIR, clientRate.SelectedFilterCategory);

				form.BaseTabControl.TopLevelTabControl.SelectedIndex = 7;
				AssertEquals(RatingConstants.RateCategory.SummaryRatesCategory, clientRate.SelectedFilterCategory);
			}

			using (RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ActiveRatesForm(clientRate))
				{
					form.Show();

					AssertEquals(RatingConstants.RateCategory.AIR, clientRate.SelectedFilterCategory);

					form.BaseTabControl.TopLevelTabControl.SelectedIndex = 1;
					AssertEquals(RatingConstants.RateCategory.SCO, clientRate.SelectedFilterCategory);

					((TabControl)form.BaseTabControl.TopLevelTabControl.SelectedTab.Controls[0]).SelectedIndex = 1;
					AssertEquals(RatingConstants.RateCategory.SNC, clientRate.SelectedFilterCategory);

					form.BaseTabControl.TopLevelTabControl.SelectedIndex = 0;
					AssertEquals(RatingConstants.RateCategory.AIR, clientRate.SelectedFilterCategory);

					form.BaseTabControl.TopLevelTabControl.SelectedIndex = 7;
					AssertEquals(RatingConstants.RateCategory.SummaryRatesCategory, clientRate.SelectedFilterCategory);
				}
			}
		}

		public void TestRateEntryGridTabStopShouldBeFalse_ForTabsAndSubTabs()
		{
			using (ActiveRatesForm form = new ActiveRatesForm(Factory.New<ClientRate>()))
			{
				AssertEntryPageTabStop(form);
			}

			using (CostingForm form = new CostingForm(Factory.New<Costing>()))
			{
				AssertEntryPageTabStop(form);
			}

			using (QuotationForm form = new QuotationForm(Factory.New<Quote>()))
			{
				AssertEntryPageTabStop(form);
			}

			using (GlobalTariffsForm form = new GlobalTariffsForm(Factory.New<CompanyTariff>()))
			{
				AssertEntryPageTabStop(form);
			}
		}

		void AssertEntryPageTabStop(RatingForm form)
		{
			form.Show();
			foreach (TabPage page in form.BaseTabControl.TopLevelTabControl.TabPages)
			{
				if (page is EntryTabPage entryPage)
				{
					Assert(!entryPage.RateEntryGrid.TabStop);
				}
				else
				{
					TabControl control = page.Controls.Count == 1 ? page.Controls[0] as TabControl : null;

					if (control != null)
					{
						foreach (TabPage innerPage in control.TabPages)
						{
							if (innerPage is EntryTabPage innerEntryPage)
							{
								Assert(!innerEntryPage.RateEntryGrid.TabStop);
							}
						}
					}
				}
			}
		}

		#region Implementation

		void AssertContainsRatingTabPage(RatingForm form, params string[] categories)
		{
			ZTabControl.TabPageCollection tabs = form.BaseTabControl.TopLevelTabControl.TabPages;
			foreach (string category in categories)
			{
				EntryTabPage page = form.BaseTabControl.FindTabPage(category);

				Assert("Contains " + category, page.Name == (category + "TabPage"));
			}
		}

		#endregion
	}
}
