using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class PackingListDetailsUserControl : ZUserControl
	{
		public PackingListDetailsUserControl()
		{
			InitializeComponent();
			InitializeButtonClick();
			AddPackableItemsGridMenuItem();
		}

		void InitializeButtonClick()
		{
			Split_StripButton.Click += SplitButton_Click;
			Delete_StripButton.Click += DeleteButton_Click;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Split_StripButton.Click -= SplitButton_Click;
				Delete_StripButton.Click -= DeleteButton_Click;
				if (PackagesGrid.ListManager is CurrencyManager listManager)
				{
					listManager.CurrentChanged -= PackagesGridCurrentItemChanged;
				}

				if (fPackagesGridLayoutPersister != null)
				{
					fPackagesGridLayoutPersister.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			PackagesGrid.ListManager.CurrentChanged += PackagesGridCurrentItemChanged;
		}

		protected void PackagesGridCurrentItemChanged(object sender, EventArgs e)
		{
			((CusPackageCusPackableItemRelationCollection)PackableItemsGrid.List).Load();
		}

		#region Split
		protected void SplitButton_Click(object sender, EventArgs e)
		{
			CheckAndSplitPackableItems();
		}

		void CheckAndSplitPackableItems()
		{
			var packageCusPackableItemRelations = PackableItemsGrid.GetSelectedRows().Cast<CusPackageCusPackableItemRelation>();
			var selectedItemsCount = packageCusPackableItemRelations.Count();
			if (selectedItemsCount == 1)
			{
				var packageCusPackableItemRelation = packageCusPackableItemRelations.Single();
				var isPacked = packageCusPackableItemRelation.IsPacked;
				if (isPacked)
				{
					Globals.Message.ShowError(Res.GetString("8353a1ff-15c1-4dd1-8ddf-c2821346b091", "A packed item cannot be split. Please unpack the item first."));
				}
				else
				{
					SplitPackableItems(packageCusPackableItemRelation);
				}
			}
			else if (selectedItemsCount > 1)
			{
				Globals.Message.ShowError(Res.GetString("2384b7bd-78f1-4e11-a9ac-56556a070161", "Up to one item can be split at a time."));
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("aaecd66e-549e-42c5-9cf2-e0a8ac25ff8e", "No item is selected. Please select an item to split."));
			}
		}

		void SplitPackableItems(CusPackageCusPackableItemRelation packageCusPackableItemRelation)
		{
			var mainForm = FindForm() as ZForm;
			var package = packageCusPackableItemRelation.Package;
			if (mainForm != null && package != null)
			{
				var packableItemsSplitter = new PackableItemsSplitter(package, packageCusPackableItemRelation);
				using (var form = new PackableItemSplitPartsForm(packableItemsSplitter))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK && packableItemsSplitter.DoSplit())
					{
						var list = PackableItemsGrid.List as IBusinessObjectCollection;
						if (list.SortInformation == null)
						{
							list.ApplySort(new SortInfo(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending));
						}
						else
						{
							list.ApplySort(list.SortInformation);
						}
						PackableItemsGrid.Refresh();
					}
				}
			}
		}
		#endregion

		#region Delete
		protected void DeleteButton_Click(object sender, EventArgs e)
		{
			if (CheckCanDeletePackableItemRelations())
			{
				DeletePackableItemRelations();
			}
			else
			{
				Globals.Message.ShowWarning(Res.GetString("CA59B582-C651-44F6-9086-E97729127C42", "The selected pack item record cannot be deleted because it is the last item of the corresponding invoice line. An invoice line must have at least one pack item record."));
			}
		}

		bool CheckCanDeletePackableItemRelations()
		{
			var selectedInvoiceLines = GetDataCount(PackableItemsGrid.GetSelectedRows().Cast<CusPackageCusPackableItemRelation>());
			var packableItemsGridCollection = GetDataCount(PackableItemsGrid.List.Cast<CusPackageCusPackableItemRelation>());
			return !selectedInvoiceLines.Any(x => packableItemsGridCollection.TryGetValue(x.Key, out var gridCount) && gridCount == x.Value);
		}

		Dictionary<string, int> GetDataCount(IEnumerable<CusPackageCusPackableItemRelation> itemRelations)
		{
			var result = new Dictionary<string, int>();
			foreach (var itemRelation in itemRelations)
			{
				var key = itemRelation.InvoiceNumber + "|" + itemRelation.InvoiceLineNumber;
				if (result.TryGetValue(key, out int count))
				{
					result[key] = count + 1;
				}
				else
				{
					result.Add(key, 1);
				}
			}
			return result;
		}

		void DeletePackableItemRelations()
		{
			var selectedPackableItemRelations = PackableItemsGrid.GetSelectedRows().Cast<CusPackageCusPackableItemRelation>();
			foreach (var selectedPackableItemRelation in selectedPackableItemRelations)
			{
				selectedPackableItemRelation.Package.PackableItemRelataions.Remove(selectedPackableItemRelation);
				selectedPackableItemRelation.PackableItem.Delete();
			}
		}
		#endregion

		#region PackableItemsGrid Menu Item

		protected MenuItem quickPackMenuItem;
		protected MenuItem resetInvoiceLineValuesMenuItem;

		void AddPackableItemsGridMenuItem()
		{
			quickPackMenuItem = new ZMenuItem(ResString.GetMultilingualString("08D3389C-1426-43AB-94A4-F2F9797CAA46", "Quick Pack"), QuickPackClick) { Name = nameof(quickPackMenuItem) };
			resetInvoiceLineValuesMenuItem = new ZMenuItem(ResString.GetMultilingualString("D3C2887C-1164-4A30-A780-EDE97C4A4A1D", "Reset values from invoice line"), ResetInvoiceLineClick) { Name = nameof(resetInvoiceLineValuesMenuItem) };
			PackableItemsGrid.ContextMenu.MenuItems.Add(quickPackMenuItem);
			PackableItemsGrid.ContextMenu.MenuItems.Add(resetInvoiceLineValuesMenuItem);
		}

		void ResetInvoiceLineClick(object sender, EventArgs args)
		{
			var selectedPackableItemRelations = PackableItemsGrid.GetSelectedRows().Cast<CusPackageCusPackableItemRelation>().ToArray();
			var mainForm = FindForm() as ZForm;
			if (mainForm != null)
			{
				if (!selectedPackableItemRelations.Any())
				{
					Globals.Message.ShowWarning(Res.GetString("AC82851E-C5E6-42A3-89EE-9CB6190950FE", "Please select at least one pack item."));
					return;
				}
				else
				{
					var continueToAction = Globals.Message.Show(ResString.GetMultilingualString("B499B4F2-E959-488D-8FB7-71161F4C54AF", @"This will remove the changes you have made to this packing list item.
The packing list item values, including Goods Description, Packable Quantity, and Packable Quantity Unit, will be reset using the current invoice line values.
Do you want to proceed ?"), resetInvoiceLineValuesMenuItem.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
					if (continueToAction)
					{
						foreach (var selectedPackableItemRelation in selectedPackableItemRelations)
						{
							selectedPackableItemRelation.PackableItem.ResetValuesFromInvoiceLine();
						}
						PackableItemsGrid.Refresh();
					}
				}
			}
		}

		void QuickPackClick(object sender, EventArgs args)
		{
			var mainForm = FindForm() as ZForm;
			if (mainForm != null && CurrentDataItem is CusPackingList packingList)
			{
				var quickPack = new QuickPack(packingList.PackableItems);
				if (quickPack.QuickPackItems.Count > 0)
				{
					using (var form = new QuickPackForm(quickPack))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
						{
							quickPack.DoQuickPackAction();
							PackableItemsGrid.Refresh();
						}
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("37C21914-D395-4162-9229-965C070EFC8D", "There are no items that can be packed."));
				}
			}
		}

		#endregion

		CustomLabelsGridLayoutPersister fPackagesGridLayoutPersister;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (CurrentDataItem != null)
			{
				if (fPackagesGridLayoutPersister != null)
				{
					fPackagesGridLayoutPersister.Dispose();
				}
			}
			base.SetDataBinding(dataSource, dataMember);
			var currentDataItem = CurrentDataItem;
			if (currentDataItem != null)
			{
				fPackagesGridLayoutPersister = new CustomLabelsGridLayoutPersister(PackagesGrid, new CusPackageCustomLabelsProvider((ICustomLabelsConfigOrgProvider)currentDataItem));
			}
		}
	}
}
