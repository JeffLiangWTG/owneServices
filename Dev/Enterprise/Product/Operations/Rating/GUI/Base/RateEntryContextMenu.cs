using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.GUI
{
	public static class RateEntryContextMenu
	{
		class SecurityCheckInteractor : IRateEntrySecurityUIIntractor
		{
			readonly List<RateEntry> securedEntries = new List<RateEntry>();

			public void ShowMessage(RateEntry entry)
			{
				securedEntries.Add(entry);
			}

			public void ShowMessageForAllEntries()
			{
				if (securedEntries.Any())
				{
					var codes = securedEntries
						.Where(x => x.DeniedSecurityCheckPoint != null)
						.Select(x => x.DeniedSecurityCheckPoint.Code.Replace(Env.Security.RatesSecurity.Code, ""))
						.Distinct()
						.ToArray();

					var messsage = Res.GetString("C3802A50-1F80-4068-8FA8-8DD28F6859C1", "{0} secured rates have been encountered and {1} entry(s) have not been copied.{2}{2}{3}",
							string.Join(", ", codes),
							securedEntries.Count,
							System.Environment.NewLine,
							Env.Security.RatesSecurity.ErrorMessageForNotAllowed);
					Globals.Message.Show(messsage);
				}
			}
		}

		public static void Setup(RatingHeader ratingHeader, ZTabControl tabControl, EntryTabPage[] entryPages)
		{
			new RateEntryCommand(tabControl, entryPages, true, false, null).AddMenuItem((NoResString)"-");

			if (!ratingHeader.IsAdditionalTariff())
			{
				new RateEntryCommand(tabControl, entryPages, true, true, entries => DoActionAndShowMessages(x => ratingHeader.DuplicateEntries(entries, x)))
					.AddMenuItem(RatingConstants.RateEntryContextMenu.Duplicate);
			}

			if (!ratingHeader.IsQuote())
			{
				new RateEntryCommand(tabControl, entryPages, true, true, ChangeValidityDates())
					.AddMenuItem(RatingConstants.RateEntryContextMenu.ChangeValidityDates);
			}

			if (ratingHeader.IsClientRate() || ratingHeader.IsTariff())
			{
				var rateCreator = new RateCreator(ratingHeader);

				new RateEntryCommand(tabControl, entryPages, true, false, CreateQuotes(rateCreator, true))
					.AddMenuItem(RatingConstants.RateEntryContextMenu.QuoteRelatedRatesFromCurrentTab);

				new RateEntryCommand(tabControl, entryPages, false, false, CreateQuotes(rateCreator, false))
					.AddMenuItem(RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromAllTabs);

				new RateEntryCommand(tabControl, entryPages, true, false, CreateQuotes(rateCreator, false))
					.AddMenuItem(RatingConstants.RateEntryContextMenu.QuoteSelectedRatesFromCurrentTab);
			}
		}

		static RateEntryCommand.RateEntryCommandHandler ChangeValidityDates()
		{
			return delegate(BusinessObject[] entries)
			{
				var firstEntry = (RateEntry)entries[0];
				var startEnd = new EntryStartEndDates(firstEntry.TI_RateStartDate, firstEntry.TI_RateEndDate);

				using (var form = new EntryDatesForm(startEnd))
				{
					if (ZFormModaliser.ShowDialogAndDispose(form) == DialogResult.OK)
					{
						foreach (RateEntry entry in entries.Where(e => !e.ReadOnly))
						{
							if (startEnd.UpdateStart)
							{
								entry.TI_RateStartDate = startEnd.Start;
							}
							if (startEnd.UpdateEnd)
							{
								entry.TI_RateEndDate = startEnd.End;
							}
						}
					}
				}
			};
		}

		static RateEntryCommand.RateEntryCommandHandler CreateQuotes(RateCreator rateCreator, bool includeRelatedEntries)
		{
			return delegate(BusinessObject[] entries)
			{
				Quote createdQuote = null;
				DoActionAndShowMessages(x => createdQuote = rateCreator.CreateQuoteFromExistingRateEntries(entries, includeRelatedEntries, x));
				createdQuote.HasChanges = true;
#if DEBUG
				LastFormShownForNewEntity =
#endif
				ZControllerFactory.Create(ControllerIDs.Quotations).ShowFormForNewEntity(createdQuote);
			};
		}

		static void DoActionAndShowMessages(Action<SecurityCheckInteractor> action)
		{
			var interactor = new SecurityCheckInteractor();
			action(interactor);
			interactor.ShowMessageForAllEntries();
		}

#if DEBUG
		[ThreadStatic]
		internal static IZForm LastFormShownForNewEntity;
#endif

		sealed class RateEntryCommand
		{
			internal delegate void RateEntryCommandHandler(BusinessObject[] entries);

			internal RateEntryCommand(ZTabControl tabControl, EntryTabPage[] entryPages, bool useOnlyCurrentEntries, bool allowImpliedSelection, RateEntryCommandHandler handler)
			{
				this.tabControl = tabControl;
				this.entryPages = entryPages;
				this.useOnlyCurrentEntries = useOnlyCurrentEntries;
				this.allowImpliedSelection = allowImpliedSelection;
				this.handler = handler;
				this.menuItems = new Dictionary<ContextMenu, MenuItem>(entryPages.Length);
			}

			internal void AddMenuItem(MultilingualString menuText)
			{
				foreach (EntryTabPage tabPage in entryPages)
				{
					if (tabPage.RateEntryGrid != null)
					{
						if (handler != null)
						{
							var item = new ZMenuItem(menuText, MenuHandler);
							tabPage.RateEntryGrid.ContextMenu.MenuItems.Add(item);
							menuItems.Add(tabPage.RateEntryGrid.ContextMenu, item);
							tabPage.RateEntryGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
						}
						else
						{
							tabPage.RateEntryGrid.ContextMenu.MenuItems.Add(menuText);
						}
					}
				}
			}

			void ContextMenu_Popup(object sender, EventArgs e)
			{
				ContextMenu contextMenu = sender as ContextMenu;
				if (contextMenu != null)
				{
					MenuItem item;
					if (menuItems.TryGetValue(contextMenu, out item))
					{
						BusinessObject[] entries = RelevantEntries;
						item.Enabled = entries != null && entries.Length > 0;
					}
				}
			}

			#region Implementation

			void MenuHandler(object sender, EventArgs e)
			{
				BusinessObject[] entries = RelevantEntries;
				if (entries != null && entries.Length > 0)
				{
					handler(entries);
				}
			}

			bool IsRowAttached(BusinessObject bizO)
			{
				return ((INeedRow)bizO).Row.RowState != DataRowState.Detached;
			}

			BusinessObject[] RelevantEntries
			{
				get { return useOnlyCurrentEntries ? CurrentEntries : SelectedEntries; }
			}

			BusinessObject[] SelectedEntries
			{
				get
				{
					List<BusinessObject> result = new List<BusinessObject>();
					foreach (EntryTabPage tabPage in entryPages)
					{
						EntryGrid grid = (tabPage != null) ? tabPage.RateEntryGrid as EntryGrid : null;
						if (grid != null && grid.SelectedElements.Length > 0)
						{
							result.AddRange(grid.SelectedElements);
						}
					}
					return result.ToArray();
				}
			}

			BusinessObject[] CurrentEntries
			{
				get
				{
					var grid = CurrentEntryGrid;
					if (grid != null)
					{
						if (grid.SelectedElements.Length > 0)
						{
							return grid.SelectedElements.Where(IsRowAttached).ToArray();
						}
						else if (allowImpliedSelection && grid.CurrentRowIndex >= 0 && grid.CurrentRowIndex < grid.EntryCollection.Count)
						{
							var currentRateEntry = grid.EntryCollection[grid.CurrentRowIndex];
							return IsRowAttached(currentRateEntry) ? new BusinessObject[] { currentRateEntry } : null;
						}
					}
					return null;
				}
			}

			EntryGrid CurrentEntryGrid
			{
				get
				{
					EntryTabPage tabPage = tabControl.SelectedTab as EntryTabPage;

					if (tabPage == null && tabControl.SelectedTab != null && tabControl.SelectedTab.Controls.Count > 0)
					{
						ZTabControl nestedTabControl = tabControl.SelectedTab.Controls[0] as ZTabControl;
						tabPage = nestedTabControl != null ? nestedTabControl.SelectedTab as EntryTabPage : null;
					}

					return tabPage != null ? tabPage.RateEntryGrid as EntryGrid : null;
				}
			}

			readonly ZTabControl tabControl;
			readonly EntryTabPage[] entryPages;
			readonly bool useOnlyCurrentEntries;
			readonly RateEntryCommandHandler handler;
			readonly bool allowImpliedSelection;
			readonly Dictionary<ContextMenu, MenuItem> menuItems;

			#endregion
		}
	}
}

