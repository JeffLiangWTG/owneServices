using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class RelatedShipmentsControl : ZUserControl
	{
		#region Ctor

		public RelatedShipmentsControl()
		{
			InitializeComponent();

			SetContextMenu();
		}

		#endregion

		#region Properties

		ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)CurrentDataItem; }
		}

		#endregion

		#region Implementation

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				shipment.ValueNotSet -= new EventHandler<ValueNotSetEventArgs>(OnShipmentValueNotSet);
				shipment.MasterChanged -= new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			ForwardingShipment shipment = CurrentDataItem as ForwardingShipment;
			if (shipment != null)
			{
				shipment.ValueNotSet += new EventHandler<ValueNotSetEventArgs>(OnShipmentValueNotSet);
				shipment.MasterChanged += new EventHandler<MasterChangedEventArgs>(OnShipment_MasterChanged);
			}
		}

		void OnShipmentValueNotSet(object sender, ValueNotSetEventArgs e)
		{
			Globals.Message.ShowError(e.Reason);
		}

		void SetContextMenu()
		{
			MenuItem subShipmentAllocMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.MoveSubShipments", "Move sub shipments"));
			ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Count - 1, new ZMenuItem("-"));
			ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Count - 1, subShipmentAllocMenuItem);
			subShipmentAllocMenuItem.Click += (sender, e) =>
			{
				if (!Env.Security.MaintainShipmentAllowDetachingSubShipments.IsAllowed)
				{
					Globals.Message.ShowError(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAllowDetachingSubShipments));
					return;
				}

				SubShipmentMover mover = new SubShipmentMover(this.FindForm(), Shipment.Factory);
				mover.BeforeMoveShipmentsAction = () => { (this.TopLevelControl as ZForm).Close(); };
				mover.Execute(Shipment, ShipmentModuleButtonGrid.InnerGrid.SelectedElements.Cast<ForwardingShipment>().ToList());
			};

			MenuItem packingMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.Packing", "Packing"));
			packingMenuItem.Shortcut = Shortcut.CtrlP;
			packingMenuItem.ShowShortcut = true;
			packingMenuItem.Click += (s, e) => ShowPackingDetails();
			ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(2, packingMenuItem);

			MenuItem createJobHeaderMenuItem = new ZMenuItem(ResString.GetMultilingualString("Freight.Forwarding.CreateJobInvoicingRecord", "Create Job Invoicing Record"));
			createJobHeaderMenuItem.Shortcut = Shortcut.CtrlJ;
			createJobHeaderMenuItem.ShowShortcut = true;
			createJobHeaderMenuItem.Click += (s, e) => ShipmentModuleButtonGrid.CreateJobHeader();
			ShipmentModuleButtonGrid.InnerGrid.ContextMenu.MenuItems.Add(3, createJobHeaderMenuItem);
		}

		void ShowPackingDetails()
		{
			ForwardingShipment shipment = ShipmentModuleButtonGrid.InnerGrid.ListManager != null ?
				ShipmentModuleButtonGrid.InnerGrid.ListManager.GetCurrent() as ForwardingShipment : null;

			if (shipment != null)
			{
				ShipmentPackingDetailForm.Show(shipment, FindForm());
			}
			else
			{
				Globals.Message.Show(Res.GetString("18c872e6-4149-4092-b1e6-14eb403c93bf", "Please select a shipment."));
			}
		}

		void OnShipment_MasterChanged(object sender, MasterChangedEventArgs e)
		{
			ShipmentVsConsolGUIMessageHelper.Instance.OnShipmentMasterChanged(FreightShipmentVsConsolMessageHelper.Instance, (ForwardingShipment)sender, null, e);
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
