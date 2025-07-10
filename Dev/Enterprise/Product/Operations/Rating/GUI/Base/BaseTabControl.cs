using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public class BaseTabControl : ZUserControl
	{
		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisallowRemoveOnRateLinesAndItemsControl();
		}

		void DisallowRemoveOnRateLinesAndItemsControl()
		{
			if (pageLookup != null)
			{
				if (!InEditMode)
				{
					foreach (EntryTabPage page in pageLookup.Values)
					{
						if (page.RateLinesAndItemsControl != null)
						{
							page.RateLinesAndItemsControl.RemoveAction = RemoveAction.NoRemovePossible;
						}
					}
				}
			}
		}

		#endregion

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				InitializePages((RatingHeader)BindingContext[dataSource, dataMember].GetCurrent());
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (RatingHeader != null)
			{
				if (RatingHeader.IsAdditionalTariff())
				{
					foreach (EntryTabPage page in pageLookup.Values)
					{
						if (page.RateEntryGrid != null)
						{
							page.RateEntryGrid.RemoveAction = RemoveAction.NoRemovePossible;
						}
					}
				}

				EntryTabPage[] pages = new EntryTabPage[pageLookup.Count];
				pageLookup.Values.CopyTo(pages, 0);

				RateEntryContextMenu.Setup(RatingHeader, TopLevelTabControl, pages);
			}
		}

		#endregion

		#region Tab Pages

		void InitializePages(RatingHeader ratingHeader)
		{
			pageLookup = new Dictionary<string, EntryTabPage>();

			int nextTopLevelInsertionPoint = 1;

			TopLevelTabControl.TabIndexChanged += HostTabIndexChanged;

			foreach (var pair in GetGroupedCategories(ratingHeader.GetType(), ratingHeader.IsGlobal(), ratingHeader.IsStandardCostRate()))
			{
				ZTabControl hostTabControl;
				int hostInsertionPoint;

				if (pair.Key == null || pair.Value.Length == 1)
				{
					hostTabControl = TopLevelTabControl;
					hostInsertionPoint = nextTopLevelInsertionPoint++;
				}
				else
				{
					hostTabControl = new ZTabControl();
					hostTabControl.Dock = DockStyle.Fill;
					hostTabControl.SelectedIndexChanged += HostTabIndexChanged;

					hostInsertionPoint = 0;

					ZTabPage productPage = new ZTabPage();
					productPage.CaptionResourceString = pair.Key.Item2;
					productPage.Name = pair.Key.Item1;
					productPage.Controls.Add(hostTabControl);

					TopLevelTabControl.TabPages.Insert(productPage, nextTopLevelInsertionPoint++);
				}

				foreach (RateEntryCollectionGUIInfo info in pair.Value)
				{
					string licenceError;

					EntryTabPage page;

					if (IsLicenceValid(ratingHeader, out licenceError))
					{
						page = CloneEntryTabPage(info, (ZTabPage)TopLevelTabControl.TabPages[0]);
					}
					else
					{
						Label missingLicenceLabel = new Label();
						missingLicenceLabel.Dock = DockStyle.Fill;
						missingLicenceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
						missingLicenceLabel.Text = licenceError;

						EntryTabPage missingLicencePage = new EntryTabPage();
						missingLicencePage.Name = info.Category + "LicenceErrorTabPage";
						missingLicencePage.Text = info.Text;
						missingLicencePage.Controls.Add(missingLicenceLabel);

						page = missingLicencePage;
					}

					page.Tag = info.Category;
					pageLookup.Add(info.Category, page);
					hostTabControl.TabPages.Insert(page, hostInsertionPoint++);
				}
			}

			TabPage templatePage = TopLevelTabControl.TabPages[0];
			TopLevelTabControl.TabPages.RemoveAt(0);
			templatePage.Dispose();

			TopLevelTabControl.SelectedIndexChanged += HostTabIndexChanged;
			ratingHeader.SelectedFilterCategory = SelectedCategory = FindSelectedCategory();
		}

		internal virtual void HostTabIndexChanged(object sender, EventArgs e)
		{
			string category = FindSelectedCategory();

			if (category != SelectedCategory || SelectedCategory == null)
			{
				SelectedCategory = category;
			}
		}

		public IEntryTabPage FindSelectedEntryTabPage(TabControl control)
		{
			var selectedTopTab = control.SelectedTab;
			IEntryTabPage result = null;

			if (selectedTopTab != null)
			{
				if ((result = selectedTopTab as IEntryTabPage) == null && selectedTopTab.Controls.Count == 1)
				{
					var innerControl = selectedTopTab.Controls[0];

					var innerTabControl = innerControl as TabControl;
					if (innerTabControl != null)
					{
						if (innerTabControl.SelectedTab == null && innerTabControl.TabPages.Count > 0)
						{
							innerTabControl.SelectTab(0);
						}

						return innerTabControl.SelectedTab as IEntryTabPage;
					}

					var innerPage = innerControl as IEntryTabPage;

					if (innerPage != null)
					{
						return innerPage;
					}
				}
			}

			return result;
		}

		string FindSelectedCategory()
		{
			var entryTabPage = FindSelectedEntryTabPage(TopLevelTabControl);
			return entryTabPage != null ? entryTabPage.Tag.ToString() : string.Empty;
		}

		public static string GetTabPageName(string category)
		{
			return category + "TabPage";
		}

		EntryTabPage CloneEntryTabPage(RateEntryCollectionGUIInfo info, ZTabPage template)
		{
			EntryTabPage newPage = new EntryTabPage();
			newPage.SuspendLayout();
			newPage.CheckForNotifications = template.CheckForNotifications;
			newPage.Location = template.Location;
			newPage.Size = template.Size;

			newPage.Name = GetTabPageName(info.Category);

			RateEntryCollectionGUIInfo gUIInfo = null;
			foreach (Control ctrl in template.Controls)
			{
				Control newCtrl = (Control)Activator.CreateInstance(ctrl.GetType());
				newCtrl.Location = ctrl.Location;
				newCtrl.Size = ctrl.Size;
				newCtrl.TabIndex = ctrl.TabIndex;
				newCtrl.Name = info.Category + newCtrl.GetType().Name;
				newCtrl.Anchor = ctrl.Anchor;

				if (ctrl is RateEntryPanel)
				{
					RateEntryPanel @new = (RateEntryPanel)newCtrl;
					@new.Category = info.Category;
					newPage.RateEntryGrid = @new.RateEntryGrid;
					newPage.RateLinesAndItemsControl = @new.RateLinesAndItemsControl;
					newPage.CostingRateLineAndItemsControl = @new.CostingRateLineAndItemsControl;
					gUIInfo = info;
				}

				newPage.Controls.Add(newCtrl);
				if (!(newCtrl is ZTabControl))
				{
					BindingSource.SetBindingMember(newCtrl, ".");
				}
			}
			if (gUIInfo != null)
			{
				newPage.Text = gUIInfo.Text;
			}

			newPage.ResumeLayout(false);

			return newPage;
		}

		public EntryTabPage FindTabPage(string category)
		{
			EntryTabPage targetPage;

			if (pageLookup == null)
			{
				targetPage = null;
			}
			else
			{
				pageLookup.TryGetValue(category, out targetPage);
			}

			return targetPage;
		}

		public void SelectTabPage(string category)
		{
			EntryTabPage targetPage;

			if (pageLookup != null && pageLookup.TryGetValue(category, out targetPage))
			{
				Stack<TabPage> pagesToSelect = new Stack<TabPage>();

				Control ctrl = targetPage;
				while (ctrl != null && ctrl != TopLevelControl)
				{
					TabPage page = ctrl as TabPage;

					if (page != null)
					{
						pagesToSelect.Push(page);
					}

					ctrl = ctrl.Parent;
				}

				while (pagesToSelect.Count > 0)
				{
					TabPage page = pagesToSelect.Pop();
					TabControl control = page.Parent as TabControl;

					if (control != null)
					{
						control.SelectedTab = page;
					}
				}
			}
		}

		public void SelectSummaryTabPage()
		{
			// Find a control that has exactly 1 child control of type SummaryEntryPanel. That should be the Summary Tab Page.
			// there should be just one.
			var summaryTabPage = TopLevelTabControl.FindSingle<ZTabPage>(tabPage => tabPage.FindSingleOrDefault<SummaryEntryPanel>() != null);
			TopLevelTabControl.SelectedTab = summaryTabPage;
		}

		#endregion

		#region Properties

		#region RatingHeader

		protected RatingHeader RatingHeader
		{
			get { return CurrentDataItem as RatingHeader; }
		}

		ZForm CurrentForm
		{
			get { return (ZForm)FindForm(); }
		}

		#endregion

		#region EntryCategories

		bool IsLicenceValid(RatingHeader ratingHeader, out string licenceError)
		{
			licenceError = "";
			var result = true;

			if (ratingHeader != null)
			{
				LicenceCheckpoint checkpoint;

				if (ratingHeader.IsClientRate() || ratingHeader.IsTariff() || ratingHeader.IsIntercompanyTariff())
				{
					checkpoint = Env.Licence.RelationshipClientRatesTariffs;
				}
				else if (ratingHeader.IsQuote())
				{
					checkpoint = Env.Licence.RelationshipQuotations;
				}
				else if (ratingHeader.IsCosting())
				{
					checkpoint = Env.Licence.RelationshipCostings;
				}
				else
				{
					throw new NotSupportedException("Unknown RatingHeader type - " + ratingHeader.TH_RateType);
				}

				if (checkpoint.Login(CurrentForm) == LicenceLoginResponse.Denied)
				{
					result = false;
					licenceError = checkpoint.LastReasonForNotAllowing;
				}
			}

			return result;
		}

		#endregion

		public virtual ZTabControl TopLevelTabControl
		{
			get { return null; }
		}

		protected virtual string[] GetInapplicableGroups() => Array.Empty<string>();

		protected virtual string[] GetInapplicableCategories() => Array.Empty<string>();

		public BaseRateLinesAndItemsControl CurrentRateLinesAndItemsControl
		{
			get
			{
				if (!DesignModeFinder.IsDesigning)
				{
					var page = FindSelectedEntryTabPage(TopLevelTabControl);
					if (page != null)
					{
						return page.RateLinesAndItemsControl;
					}
				}
				return null;
			}
		}

		public RateEntry CurrentEntry
		{
			get
			{
				if (!DesignModeFinder.IsDesigning)
				{
					var page = FindSelectedEntryTabPage(TopLevelTabControl);

					if (page != null)
					{
						var currentGrid = page.RateEntryGrid as EntryGrid;
						if (currentGrid != null)
						{
							return currentGrid.CurrentEntry;
						}
					}
				}
				return null;
			}
		}

		protected internal bool InEditMode
		{
			get
			{
				return DesignModeFinder.IsDesigning
					|| CurrentForm.DisplayMode == ODisplayMode.New
					|| CurrentForm.DisplayMode == ODisplayMode.Edit
					|| CurrentForm.DisplayMode == ODisplayMode.Browse;
			}
		}

		Dictionary<string, EntryTabPage> pageLookup;
		string selectedCategory;

		KeyValuePair<Tuple<string, ResourceStringData>, RateEntryCollectionGUIInfo[]>[] GetGroupedCategories(Type dataSourceType, bool isGlobal, bool isStandardCosting)
		{
			var orderList = new List<Tuple<string, ResourceStringData>>();
			var map = new Dictionary<Tuple<string, ResourceStringData>, List<RateEntryCollectionGUIInfo>>();

			var customsCategories = new[] { RatingConstants.RateCategory.CAI, RatingConstants.RateCategory.CFC, RatingConstants.RateCategory.CLC, RatingConstants.RateCategory.COR, RatingConstants.RateCategory.CDS };
			var autorateStandAloneCustomsDeclarationJobWithOwnRatesSetup = RatingDataRegistry.Instance.AutorateStandAloneCustomsDeclarationJobWithOwnRatesSetup.Value;

			foreach (string category in RatingConstants.RateCategory.RateCategories)
			{
				var info = RateEntryCollectionGUIInfo.GetInfo(category, dataSourceType, isGlobal, isStandardCosting);

				if (string.IsNullOrEmpty(info.Text) || GetInapplicableGroups().Contains(info.GroupData.Item1) || GetInapplicableCategories().Contains(category))
				{
					continue;
				}

				if (info.IsIntercompanyTariff && category.In(customsCategories))
				{
					continue;
				}

				if (!map.TryGetValue(info.GroupData, out var list))
				{
					list = new List<RateEntryCollectionGUIInfo>();
					map.Add(info.GroupData, list);
					orderList.Add(info.GroupData);
				}

				list.Add(info);
			}

			var result = new KeyValuePair<Tuple<string, ResourceStringData>, RateEntryCollectionGUIInfo[]>[orderList.Count];

			for (int i = 0; i < orderList.Count; i++)
			{
				result[i] = new KeyValuePair<Tuple<string, ResourceStringData>, RateEntryCollectionGUIInfo[]>(orderList[i], map[orderList[i]].ToArray());
			}

			return result;
		}

		public string SelectedCategory
		{
			get
			{
				return selectedCategory;
			}
			private set
			{
				selectedCategory = value;

				if (RatingHeader != null)
				{
					RatingHeader.SelectedFilterCategory = selectedCategory;
				}
			}
		}

		#endregion
	}
}

