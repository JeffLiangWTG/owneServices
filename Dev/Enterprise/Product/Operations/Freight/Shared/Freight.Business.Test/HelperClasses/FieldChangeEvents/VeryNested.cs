using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VeryNested
	{
		public List<CommonShipment> Shipments { get; } = new List<CommonShipment>();

		public IDisposable CreateNestedShipments(BusinessObjectFactory factory, ref int count)
		{
			var result = ShipmentFieldStateChange.InitializingShipment(factory);
			CreateWrappedShipment(factory, ref count);
			return result;
		}

		void CreateWrappedShipment(BusinessObjectFactory factory, ref int count)
		{
			using (ShipmentFieldStateChange.InitializingShipment(factory))
			{
				var shipment = factory.New<CommonShipment>();
				shipment.JS_ActualChargeable = count;
				Shipments.Add(shipment);
				--count;
				if (count == 0)
				{
					return;
				}

				CreateUnwrappedShipment(factory, ref count);
			}
		}

		void CreateUnwrappedShipment(BusinessObjectFactory factory, ref int count)
		{
			var shipment = factory.New<CommonShipment>();
			shipment.JS_ActualChargeable = count;
			Shipments.Add(shipment);
			--count;
			if (count == 0)
			{
				return;
			}

			CreateWrappedShipment(factory, ref count);
		}
	}
}
