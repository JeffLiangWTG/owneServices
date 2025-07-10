using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class PackLineInspectionTypeBulkUpdatePresentationManager
	{
		string SetInspectionStatusMenuText => Res.GetString("e012f249-9a34-4ce2-bda9-e081453afe83", "Set Inspection Status");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public void CreateMenus(Form parentForm, MenuItem.MenuItemCollection menuItems, ZGrid shipmentGrid)
		{
			Argument.NotNull(menuItems, nameof(menuItems));

			var setInspectionStatusMenuItem = new ZMenuItem(SetInspectionStatusMenuText, delegate
			{
				var shipments = shipmentGrid.GetSelectedElements<ForwardingShipment>();
				ShowBulkUpdateForm(shipments, parentForm);
			});

			menuItems.Add(setInspectionStatusMenuItem);
		}

		public void CreateMenus(ZFilterGridModule module, IList<MenuItem> menuItems, Func<ZFilterGridModule, ForwardingShipment[]> getShipments = null)
		{
			Argument.NotNull(module, nameof(module));
			Argument.NotNull(menuItems, nameof(menuItems));

			getShipments = getShipments ?? (u => Array.ConvertAll(u.GetSelectedBusinessObjects(), item => item as ForwardingShipment));

			var parentForm = module.LocateMainForm();

			var setInspectionStatusMenuItem = new ZMenuItem(SetInspectionStatusMenuText, delegate
			{
				var shipments = getShipments(module);
				ShowBulkUpdateForm(shipments, parentForm);
			});

			menuItems.Add(setInspectionStatusMenuItem);
		}

		void ShowBulkUpdateForm(ForwardingShipment[] shipments, Form parentForm)
		{
			if (shipments.Any())
			{
				var dataSource = CreateDataSource(shipments);
				var bulkUpdateForm = new PackLineInspectionTypeBulkUpdateForm(dataSource);
				ZFormModaliser.Show(bulkUpdateForm, parentForm);
			}
			else
			{
				Globals.Message.Show(Res.GetString("b27b68ee-02a8-4ed2-8c6c-1d9806e2195f", "Please select at least 1 row to set inspection status."));
			}
		}

		PackLineBulkUpdateDataSource CreateDataSource(ForwardingShipment[] shipments)
		{
			var factory = new BusinessObjectFactory();
			var shipmentCollection = new ForwardingShipmentCollection(factory);
			foreach (var shipment in shipments)
			{
				shipmentCollection.AddFromDatabase(shipment.PK);
			}
			var dataSource = new PackLineBulkUpdateDataSource(shipmentCollection.Factory);
			dataSource.AddAllOuterPackLines(shipmentCollection.OfType<ForwardingShipment>());
			return dataSource;
		}
	}
}
