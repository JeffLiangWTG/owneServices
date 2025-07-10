using System;
using System.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.CFS.Business
{
	public class GatePassShipmentCollection : CFSShipmentCollection
	{
		public GatePassShipmentCollection(GatePassLoadListConsol consol) : base(consol)
		{
			Sort(GatePassShipment.Schema.JS_UniqueConsignRef, ListSortDirection.Descending);
		}

		public new GatePassShipment this[int index]
		{
			get { return (GatePassShipment)Elements[index]; }
		}

		public new GatePassShipment AddNew()
		{
			return (GatePassShipment)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(GatePassShipment);
		}

		#region Totals

		public ZInt ReceivedPackages
		{
			get
			{
				ZInt receivedPackages = 0;
				foreach (GatePassShipment shipment in this)
				{
					if (shipment.IsReceived)
					{
						receivedPackages += shipment.JS_OuterPacks;
					}
				}
				return receivedPackages;
			}
		}

		public ZDecimal ReceivedVolume
		{
			get
			{
				ZDecimal receivedVolume = 0;
				foreach (GatePassShipment shipment in this)
				{
					if (shipment.IsReceived)
					{
						receivedVolume += shipment.JS_ActualVolume;
					}
				}
				return receivedVolume;
			}
		}

		public ZDecimal ReceivedWeight
		{
			get
			{
				ZDecimal receivedWeight = 0;
				foreach (GatePassShipment shipment in this)
				{
					if (shipment.IsReceived)
					{
						receivedWeight += shipment.JS_ActualWeight;
					}
				}
				return receivedWeight;
			}
		}

		public ZInt TotalPackages
		{
			get { return (ZInt)TotalCalculation.GetTotal(this, GatePassShipment.Schema.JS_OuterPacks); }
		}

		#endregion
	}
}
