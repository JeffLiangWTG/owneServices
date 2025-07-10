using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVItemsUserControl : ZUserControl
	{
		public HVLVItemsUserControl()
		{
			InitializeComponent();
			SetUpContextMenu();
			new UNDGDataItemFormManager(itemsGrid, "", UNDGDataItemFormManagerConfig.ShowSubstanceProperties()).Initialize();
		}

		void SetUpContextMenu()
		{
			var dividerMenuItem = new ZMenuItem("-");

			toggleItemActiveStatusMenuItem = HVLVMenuItemHelper.ToggleActiveStatus(ToggleSelectedItemsActiveStatus);
			itemsGrid.ContextMenu.MenuItems.Add(itemsGrid.DeleteMenuItem.Index + 1, toggleItemActiveStatusMenuItem);

			itemsGrid.ContextMenu.Popup += ItemContextMenu_Popup;
		}

		void ItemContextMenu_Popup(object sender, EventArgs args)
		{
			HVLVMenuItemHelper.UpdateToggleActiveStatusMenuItemUsabilityAndCaption(itemsGrid.SelectedElements, toggleItemActiveStatusMenuItem);

			var selectedItem = itemsGrid.GetCurrent() as HVLVItem;
			if (selectedItem != null)
			{
				itemsGrid.DeleteMenuItem.Enabled = selectedItem.CanDelete;
			}
		}

		void ToggleSelectedItemsActiveStatus(object sender, EventArgs e)
		{
			itemsGrid.SelectedElements.OfType<HVLVItem>().Where(item => item.IsInDatabase).ForEach(item => item.HVI_IsActive = !item.HVI_IsActive);
		}

		ZMenuItem toggleItemActiveStatusMenuItem;
		internal ZGrid HVLVItemsGrid => itemsGrid;
	}
}
