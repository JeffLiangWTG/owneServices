using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderLineDeliverContainerDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight));

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			var line = order.OrderLines.AddNew();
			var delivery = line.Deliveries.AddNew();

			var deliveryContainer = delivery.Containers.AddNew();
			deliveryContainer.J5_WeightUQ = Core.Constants.Weight.Kilograms;
			deliveryContainer.J5_VolumeUQ = Core.Constants.Volume.CubicMetres;

			deliveryContainer.J5_Weight = 153.261m;
			deliveryContainer.J5_Volume = 3.167m;

			AssertEquals(153.261m, deliveryContainer.J5_Weight);
			AssertEquals(3.167m, deliveryContainer.J5_Volume);

			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(153.27m, deliveryContainer.J5_Weight);
			AssertEquals(3.16m, deliveryContainer.J5_Volume);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			var line = order.OrderLines.AddNew();
			var delivery = line.Deliveries.AddNew();

			deliveryContainer = delivery.Containers.AddNew();
			deliveryContainer.J5_WeightUQ = Core.Constants.Weight.Kilograms;
			deliveryContainer.J5_VolumeUQ = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return deliveryContainer; }
		}
		OrderLineDeliverContainer deliveryContainer;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(OrderLineDeliverContainer.Schema.J5_Weight, OrderLineDeliverContainer.Schema.J5_WeightUQ);
					measurePropertiesAndUnits.Add(OrderLineDeliverContainer.Schema.J5_Volume, OrderLineDeliverContainer.Schema.J5_VolumeUQ);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
