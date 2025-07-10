using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class OrderLinesSelectionForm : ZChildForm, IEmbeddedModulePopupOKButtonStrategy
	{
		public OrderLinesSelectionForm(IWarehouseIntegrationSupporter parent)
		{
			this.parent = Argument.NotNull(parent, nameof(parent));
			module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsOrderLine);
			embeddedControl = (ZFilterStripControl)module.EmbeddedControl;
			((System.ComponentModel.ISupportInitialize)(embeddedControl.BindingSource)).BeginInit();
			embeddedControl.SuspendLayout();
			embeddedControl.ResumeLayout(true);

			var filterBusinessObject = module.FilterBusinessObject;
			SetFilterDefaults(filterBusinessObject);

			SuspendLayout();
			this.mainModulePanel.Controls.Add(embeddedControl);
			var recentItemPanel = this.Controls.Find("RecentItemsPanel", false).FirstOrDefault();
			if (recentItemPanel != null)
			{
				this.Controls.Remove(recentItemPanel);
			}
			ResumeLayout(true);
		}

		protected ZFilterModule module;
		readonly ZFilterStripControl embeddedControl;
		protected IWarehouseIntegrationSupporter parent { get; }

		void SetFilterDefaults(FilterStripBusinessObject filterBO)
		{
			var result = new FilterBusinessObjectDefaults();
			if (parent.ClientPK.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault("Client", "Property", parent.ClientPK, false));
			}
			var warehouse = WhsWarehouse;
			if (warehouse != null)
			{
				result.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouse.PK, false));
			}
			filterBO.SetExternalDefaults(result);
		}

		void SelectButton_Click(object sender, EventArgs args)
		{
			var selectedOrderLines = (embeddedControl.FilteredGrid.SelectedElements);
			(this as IEmbeddedModulePopupOKButtonStrategy).HandleFindBoxOKButton(selectedOrderLines);
		}

		protected IWhsWarehouse WhsWarehouse => CachedValueHelper.GetValue(ref whsWarehouse, () => parent.WarehouseAddress.GetWhsWarehouse());
		CachedValue<IWhsWarehouse> whsWarehouse;

		void IEmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			if (selectedBusinessObject.Length > 0)
			{
				var selectionHeader = new OrderLinesSelectionHeader(parent, selectedBusinessObject.Select(x => x.PK).ToArray());
				switch (selectionHeader.Status)
				{
					case OrderLinesSelectionHeaderStatus.AllQualified:
						selectionHeader.ImportInventoriesByOrderLines();
						DialogResult = System.Windows.Forms.DialogResult.OK;
						break;
					case OrderLinesSelectionHeaderStatus.PartiallyQualified:
						var queryUserResult = Globals.Message.Show(Res.GetString("CD2AF795-26EF-4DBB-AEB7-E0D4627AB0C9", "Only part of Order Lines out of all the selected ones are picked and allocated to typical inventories\r\n, do you want to import the qualified ones while do nothing to the unqualified rest?"), Res.GetString("A259E56B-9F96-413A-8AA5-2AE5FA2F91C9", "Selected order lines partially qualified"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
						if (queryUserResult == DialogResult.Yes)
						{
							selectionHeader.ImportInventoriesByOrderLines();
							DialogResult = System.Windows.Forms.DialogResult.OK;
						}
						break;
					case OrderLinesSelectionHeaderStatus.NoneQualified:
						Globals.Message.ShowError(Res.GetString("92B18396-13C6-4977-9C49-CDC4A0C45505", "No related Inventories can be loaded for the selected Order Lines, please ensure that at least one of the selected Order Lines is attached to a Pick and allocated to one specific inventory."));
						break;
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("A28A8CB1-8E16-4FD0-AF1D-1864A730C9C7", "No Order Lines are selected."));
			}
		}

		void IEmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters) { }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (module != null)
				{
					module.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
