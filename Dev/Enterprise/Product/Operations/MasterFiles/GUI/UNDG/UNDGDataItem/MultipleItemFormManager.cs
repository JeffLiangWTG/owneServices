using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class MultipleItemFormManager<ParentType> where ParentType : class
	{
		public MultipleItemFormManager(ZGrid grid, string bindingPrefix)
		{
			this.Grid = grid;
			this.BindingPrefix = bindingPrefix;
		}

		readonly ZGrid Grid;
		readonly string BindingPrefix;

		public void Initialize(ZLinkLabel linkLabel, params Control[] individualControls)
		{
			Initialize();
			InitializeLinkLabel(linkLabel);
			this.IndividualControls = individualControls;
		}

		public void Initialize(ZButton button, params Control[] individualControls)
		{
			Initialize();
			InitializeButton(button);
			this.IndividualControls = individualControls;
		}

		public void Initialize()
		{
			AddContextMenu();
			AddColumns();

			if (Grid != null)
			{
				Grid.DataSourceChanged += new EventHandler(MultipleItemFormManager_DataSourceChanged);
				Grid.Disposed += new EventHandler(Grid_Disposed);
			}
		}

		void InitializeLinkLabel(ZLinkLabel linkLabel)
		{
			this.LinkLabel = linkLabel;
			if (LinkLabel != null)
			{
				LinkLabel.Text = LinkLabelText;
				LinkLabel.LinkClicked += delegate
				{ ShowMultipleItemForm(); };
				LinkLabel.Visible = false;
			}
		}

		void InitializeButton(ZButton button)
		{
			this.Button = button;
			if (Button != null)
			{
				Button.Text = ButtonText;
				Button.Click += delegate
				{ ShowMultipleItemForm(); };
			}
		}

		protected virtual ZString LinkLabelText
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString ButtonText
		{
			get { return Res.GetString("0694F94A-CB77-4949-A25E-D3470E1580DE", "More.."); }
		}

		ZLinkLabel LinkLabel;
		ZButton Button;
		Control[] IndividualControls;
		CurrencyManager ListManager;

		void MultipleItemFormManager_DataSourceChanged(object sender, EventArgs e)
		{
			if (Grid.DataSource != null)
			{
				string bindingPrefixWithPlus = !string.IsNullOrEmpty(BindingPrefix) ? BindingPrefix + "+" : "";

				foreach (ZString columnName in ColumnsToUpdate)
				{
					var multiControlColumnStyle = Grid.Columns[bindingPrefixWithPlus + columnName]?.ColumnStyle as ZMultiControlColumnStyle;
					if (multiControlColumnStyle != null)
					{
						multiControlColumnStyle.EditControl.ControlAdded += new ControlEventHandler(EditControl_ControlAdded);
					}
				}

				ListManager = Grid.ListManager;
				ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
				ListManager.ListChanged += new ListChangedEventHandler(MultipleItemFormManager_ListChanged);

				UpdateCurrentItem();

				AddCountChangedEventHandlerToItemsCollection(CurrentItem, new EventHandler(ItemsCollection_CountChanged));
			}
		}

		protected virtual ZString[] ColumnsToUpdate
		{
			get
			{
				return new List<ZString>().ToArray();
			}
		}

		void MultipleItemFormManager_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (e.ListChangedType == ListChangedType.ItemAdded && e.NewIndex >= 0 && e.NewIndex < Grid.List.Count)
			{
				UserIdleWorker.QueueWorkItem(Grid, 200, new MethodInvoker(UpdateCurrentItem), null);
			}
		}

		/// <summary>
		/// Add EventHandler to the Parent.Collection.CountChanged event
		/// </summary>
		protected abstract void AddCountChangedEventHandlerToItemsCollection(ParentType parent, EventHandler eventHandler);

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UpdateCurrentItem();
		}

		void ItemsCollection_CountChanged(object sender, EventArgs e)
		{
			UpdateCurrentItem();
		}

		void UpdateCurrentItem()
		{
			if (CurrentItem != null)
			{
				bool hasMultipleItems = CollectionCount(CurrentItem) > 1;
				if (LinkLabel != null && LinkLabel.Visible != hasMultipleItems)
				{
					LinkLabel.Visible = hasMultipleItems;
				}

				if (IndividualControls != null)
				{
					foreach (Control control in IndividualControls)
					{
						if (control != null && control.Visible != !hasMultipleItems)
						{
							control.Visible = !hasMultipleItems;
						}
					}
				}
			}
		}

		/// <summary>
		/// Override to return Collection.Count
		/// </summary>
		protected abstract int CollectionCount(ParentType parent);

		public void AddColumns()
		{
			if (Grid != null)
			{
				ZGrid gridToAddTo = Grid.Parent as ZGrid
					?? Grid;

				string bindingPrefixWithPlus = !string.IsNullOrEmpty(BindingPrefix) ? BindingPrefix + "+" : "";

				AddColumnsCore(gridToAddTo, bindingPrefixWithPlus);
			}
		}

		/// <summary>
		/// Add columns to the Grid
		/// </summary>
		protected abstract void AddColumnsCore(ZGrid gridToAddTo, string bindingPrefixWithPlus);

		void AddContextMenu()
		{
			if (Grid != null && MenuCaption != null)
			{
				MenuItem menuItem = new ZMenuItem(MenuCaption, delegate
				{ ShowMultipleItemForm(); });
				menuItem.Shortcut = MenuShortcut;
				menuItem.ShowShortcut = MenuShortcut != Shortcut.None;

				//Remove any existing identical shortcuts so that our more specific item takes precedence
				//(for example, Multiple Packing should override Find Previous)
				if (menuItem.ShowShortcut)
				{
					foreach (var oldMenuItem in Grid.ContextMenu.MenuItems.Cast<MenuItem>())
					{
						if (menuItem.Shortcut == oldMenuItem.Shortcut)
						{
							oldMenuItem.Shortcut = Shortcut.None;
							break; //safe to assume there's only one
						}
					}
				}

				Grid.ContextMenu.MenuItems.Add("-");
				Grid.ContextMenu.MenuItems.Add(menuItem);
			}
		}

		protected virtual MultilingualString MenuCaption
		{
			get { return null; }
		}

		protected virtual Shortcut MenuShortcut
		{
			get { return Shortcut.None; }
		}

		protected void EditControl_ControlAdded(object sender, ControlEventArgs e)
		{
			ZLinkLabel linkLabel = e.Control as ZLinkLabel;
			if (linkLabel != null)
			{
				linkLabel.LinkClicked -= new LinkLabelLinkClickedEventHandler(LinkLabel_Clicked);
				linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(LinkLabel_Clicked);
			}
		}

		void LinkLabel_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			ShowMultipleItemForm();
		}

		public void ShowMultipleItemForm()
		{
			if (CurrentItem != null)
			{
				ShowMultipleItemFormCore(CurrentItem, Grid.FindForm());
			}
		}

		/// <summary>
		/// Show the form using Parent and ParentForm
		/// </summary>
		protected abstract void ShowMultipleItemFormCore(ParentType parent, Form parentForm);

		ParentType CurrentItem
		{
			get
			{
				ParentType item = null;
				if (Grid.ListManager != null && Grid.ListManager.Position != -1)
				{
					BusinessObject bizo = (BusinessObject)Grid.ListManager.GetCurrent();
					item = GetParentTypeFromBizo(bizo);
				}

				return item;
			}
		}

		ParentType GetParentTypeFromBizo(BusinessObject bizo)
		{
			ParentType item = bizo as ParentType;
			if (item == null && !string.IsNullOrEmpty(BindingPrefix))
			{
				item = bizo[BindingPrefix] as ParentType;
			}
			return item;
		}

		#region Dispose

		void Grid_Disposed(object sender, EventArgs e)
		{
			Grid.Disposed -= new EventHandler(Grid_Disposed);
			Grid.DataSourceChanged -= new EventHandler(MultipleItemFormManager_DataSourceChanged);
			if (ListManager != null)
			{
				ListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
				ListManager.ListChanged -= new ListChangedEventHandler(MultipleItemFormManager_ListChanged);
				ListManager = null;
			}
		}

		#endregion
	}
}
