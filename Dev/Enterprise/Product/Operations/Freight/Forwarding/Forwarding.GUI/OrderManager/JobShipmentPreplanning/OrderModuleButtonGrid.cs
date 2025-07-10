using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	partial class OrderModuleButtonGrid : ZModuleButtonGrid
	{
		public OrderModuleButtonGrid()
		{
			InitializeComponent();
			Detached += new EventHandler<ModuleButtonGridOnDetachedEventArgs>(OrderModuleButtonGrid_Detached);
		}

		#region ShipmentPreplanning

		protected JobShipmentPreplanning ShipmentPreplanning
		{
			get { return ((ZForm)ParentForm).BusinessEntity as JobShipmentPreplanning; }
		}

		#endregion

		#region Detaching

		protected override void DetachButton_Click(object sender, EventArgs e)
		{
			StmALog[] dataExportEvents = ShipmentPreplanning.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.Events.DataExport.Code));

			if (dataExportEvents.Length > 0 &&
				InnerGrid.SelectedElements.Length > 0)
			{
				ZString message = Res.GetString("abc8ed19-2407-400d-8d6b-8ccd5d25580a", "This shipment pre-advice has been exported and you should not detach orders from it as this could cause data consistency problems for your customers. Instead, the order line quantities should be set to 0. Do you wish to set all order lines to 0 on the order(s) you were trying to detach?");
				ZString caption = Res.GetString("1d4d1360-1545-4074-8766-481e7296536a", "Shipment Pre Advice");

				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
				{
					foreach (Order order in InnerGrid.SelectedElements)
					{
						foreach (OrderLine orderLine in order.OrderLines)
						{
							orderLine.JO_Quantity = 0;
							orderLine.JO_QtyInvoiced = 0;
							orderLine.JO_QtyReceived = 0;
						}
					}
				}

				return;
			}

			base.DetachButton_Click(sender, e);
		}

		void OrderModuleButtonGrid_Detached(object sender, ModuleButtonGridOnDetachedEventArgs e)
		{
			IEnumerable<Order> ordersWithShipments = e.DetachedBusinessObjects.Cast<Order>().Where(order => order.IsShipmentAttached);
			if (ordersWithShipments.Any())
			{
				string caption = Res.GetString("9f1e4e19-804b-4830-a9c9-9d2be9ca96ae", "Detach confirmation");
				string message = Res.GetString("20d91dbd-7098-4e57-93d2-24f60b0c6a00", "Do you want to detach order(s) from the shipment as well?");
				if (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					foreach (Order orderToBeDetached in ordersWithShipments)
					{
						orderToBeDetached.JD_JS = ZGuid.Empty;
					}
				}
			}
		}

		#endregion
	}
}
