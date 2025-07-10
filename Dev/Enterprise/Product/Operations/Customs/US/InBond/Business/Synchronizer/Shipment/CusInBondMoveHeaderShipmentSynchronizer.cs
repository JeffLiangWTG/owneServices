using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderShipmentSynchronizer : BusinessObjectSynchroniser
	{
		public CusInBondMoveHeaderShipmentSynchronizer(CusInBondMoveHeader destination)
			: base(destination, destination.Header.Parent as ForwardingShipment)
		{
		}

		protected new CusInBondMoveHeader Destination
		{
			get { return (CusInBondMoveHeader)base.Destination; }
		}

		protected ForwardingShipment Shipment
		{
			get { return (ForwardingShipment)base.Source; }
		}

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();

			destinationPortFieldSynchroniser = new FieldSynchroniser(Destination.BM_DestinationPortCodeInfo, () => DataCalculator.GetDestinationPort(), GetPortRelatedInfos);
			Synchronisers.Add(destinationPortFieldSynchroniser);

			foreignDestinationFieldSynchroniser = new FieldSynchroniser(Destination.BM_ForeignDestPortKCodeInfo, () => DataCalculator.GetForeignDestPort(), GetPortRelatedInfos);
			Synchronisers.Add(foreignDestinationFieldSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.BM_MonetaryValueInfo, GetGoodsValue, GetGoodsValueRelatedInfos));

			var conso = DataCalculator.Consol;
			if (conso != null)
			{
				conso.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
				conso.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
			}
			Shipment.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			Shipment.Transports.CountChanged += new CollectionCountChangedEventHandler(Transports_CountChanged);
		}

		void Transports_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			UpdateInfoEventsAndReSynchronise(destinationPortFieldSynchroniser);
			UpdateInfoEventsAndReSynchronise(foreignDestinationFieldSynchroniser);
		}

		protected override void UnHookSynchronisers()
		{
			var conso = DataCalculator.Consol;
			if (conso != null)
			{
				conso.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);
			}
			Shipment.Transports.CountChanged -= new CollectionCountChangedEventHandler(Transports_CountChanged);

			base.UnHookSynchronisers();
		}

		#region Ports

		FieldSynchroniser destinationPortFieldSynchroniser;
		FieldSynchroniser foreignDestinationFieldSynchroniser;

		IEnumerable<ZPropertyInfo> GetPortRelatedInfos()
		{
			yield return Destination.BM_InBondEntryTypeInfo;

			foreach (var info in GetTransportsInfos())
			{
				yield return info;
			}
		}

		public IEnumerable<ZPropertyInfo> GetTransportsInfos()
		{
			var transports = DataCalculator.GetSortedTransports();

			foreach (Transport transport in transports)
			{
				foreach (var propertyName in RequiredTransportProperties)
				{
					if (transport.ZPropertyInfoHash.ContainsKey(propertyName))
					{
						yield return transport.ZPropertyInfoHash[propertyName];
					}
				}
			}
		}

		string[] RequiredTransportProperties
		{
			get
			{
				if (requiredTransportProperties == null)
				{
					List<string> propertyNames = new List<string>();
					propertyNames.Add(Transport.Schema.JW_TransportMode);
					propertyNames.Add(Transport.Schema.JW_RL_NKLoadPort);
					propertyNames.Add(Transport.Schema.JW_RL_NKDiscPort);
					propertyNames.Add(Transport.Schema.JW_ETD);
					propertyNames.Add(Transport.Schema.JW_ETA);
					requiredTransportProperties = propertyNames.ToArray();
				}
				return requiredTransportProperties;
			}
		}
		string[] requiredTransportProperties;

		#endregion

		#region BM_MonetaryValue

		IZType GetGoodsValue()
		{
			return Shipment.JS_GoodsValue;
		}

		IEnumerable<ZPropertyInfo> GetGoodsValueRelatedInfos()
		{
			yield return Shipment.JS_GoodsValueInfo;
		}

		#endregion

		CusInBondMoveHeaderShipmentDataCalculator DataCalculator
		{
			get { return dataCalculator ?? (dataCalculator = new CusInBondMoveHeaderShipmentDataCalculator(Shipment, Destination)); }
		}
		CusInBondMoveHeaderShipmentDataCalculator dataCalculator;
	}
}
