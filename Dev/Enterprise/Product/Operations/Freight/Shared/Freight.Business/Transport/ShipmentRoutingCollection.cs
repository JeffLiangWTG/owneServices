using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentRoutingCollection : RoutingCollection
	{
		public ShipmentRoutingCollection(CommonShipment shipment)
			: base(shipment)
		{
			AddConsolTransportCollection(shipment);
			shipment.MasterChanged += MasterShipmentChanged;
			AddMonitoringOfMasterShipment(shipment.CoLoadMasterShipment);
		}

		void AddConsolTransportCollection(CommonShipment shipment)
		{
			foreach (CommonConsol consol in shipment.Consols)
			{
				MonitorCollection(consol.Transports);
			}

			shipment.Consols.CountChanged += new CollectionCountChangedEventHandler(ConsolCountChanged);
		}

		void AddMonitoringOfMasterShipment(CommonShipment masterShipment)
		{
			if (masterShipment != null)
			{
				if (masterShipment.Transports != null)
				{
					MonitorCollection(masterShipment.Transports);
				}

				masterShipment.MasterChanged += MasterShipmentChanged;
				AddMonitoringOfMasterShipment(masterShipment.CoLoadMasterShipment);
			}
		}

		void RemoveMonitoringOfMasterShipment(CommonShipment masterShipment)
		{
			if (masterShipment != null)
			{
				if (masterShipment.Transports != null)
				{
					UnMonitorCollection(masterShipment.Transports);
				}

				masterShipment.MasterChanged -= MasterShipmentChanged;
				RemoveMonitoringOfMasterShipment(masterShipment.CoLoadMasterShipment);
			}
		}

		#region AddTransportFetchHints

		protected override void AddTransportFetchHints(ITransportParent parent)
		{
			CommonShipment shipment = (CommonShipment)parent;
			base.AddTransportFetchHints(shipment);

			foreach (CommonConsol consol in shipment.Consols)
			{
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, consol.PK);
			}

			AddMasterShipmentTransportFetchHint(shipment, shipment.CoLoadMasterShipment);
		}

		void AddMasterShipmentTransportFetchHint(CommonShipment shipment, CommonShipment masterShipment)
		{
			if (masterShipment != null)
			{
				shipment.Factory.AddFetchHint(JobConsolTransportSchema.JW_ParentGUID, masterShipment.PK);
				AddMasterShipmentTransportFetchHint(shipment, masterShipment.CoLoadMasterShipment);
			}
		}

		#endregion

		#region Events

		void ConsolCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			CommonConsol consol = (CommonConsol)e.BizObject;

			if (e.ItemAdded)
			{
				MonitorCollection(consol.Transports);
			}
			else if (e.ItemRemoved)
			{
				UnMonitorCollection(consol.Transports);
			}
		}

		void MasterShipmentChanged(object sender, MasterChangedEventArgs e)
		{
			var shipment = (CommonShipment)sender;
			var oldMasterShipment = shipment.Factory.Load<CommonShipment>(e.OldMasterPK);
			RemoveMonitoringOfMasterShipment(oldMasterShipment);
			var newMasterShipment = shipment.Factory.Load<CommonShipment>(e.NewMasterPK);
			AddMonitoringOfMasterShipment(newMasterShipment);
		}

		#endregion
	}
}
