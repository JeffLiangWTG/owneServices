using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.JobShipment)]
	public class ShipmentCollection : BusinessObjectCollection<CommonShipment>, Integration.IShipmentCollection
	{
		public ShipmentCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public ShipmentCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		#region Totals

		public ZInt ReceivedPackages
		{
			get
			{
				ZInt receivedPackages = 0;
				foreach (CommonShipment shipment in this)
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
				foreach (CommonShipment shipment in this)
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
				foreach (CommonShipment shipment in this)
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
			get { return (ZInt)TotalCalculation.GetTotal(this, CommonShipment.Schema.JS_OuterPacks); }
		}

		public ZDecimal TotalVolume
		{
			get { return TotalCalculation.GetTotalVolume(this, CommonShipment.Schema.JS_ActualVolume, CommonShipment.Schema.JS_UnitOfVolume, Constants.Volume.CubicMetres); }
		}

		public ZDecimal TotalWeight
		{
			get { return TotalCalculation.GetTotalWeight(this, CommonShipment.Schema.JS_ActualWeight, CommonShipment.Schema.JS_UnitOfWeight, Constants.Weight.Kilograms); }
		}

		#endregion
	}
}
