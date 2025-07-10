using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight));

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			order.JD_UnitOfWeight = Core.Constants.Weight.Kilograms;
			order.JD_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			order.JD_ActualWeight = 136.542m;
			order.JD_ActualVolume = 2.165m;

			AssertEquals(136.542m, order.JD_ActualWeight);
			AssertEquals(2.165m, order.JD_ActualVolume);
			AssertEquals(136.542m, order.Weight);
			AssertEquals(2.165m, order.Volume);

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

			AssertEquals(136.55m, order.JD_ActualWeight);
			AssertEquals(2.16m, order.JD_ActualVolume);
			AssertEquals(136.55m, order.Weight);
			AssertEquals(2.16m, order.Volume);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;
			order.JD_UnitOfWeight = Core.Constants.Weight.Kilograms;
			order.JD_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			var line1 = order.OrderLines.AddNew();
			line1.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;
			line1.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return order; }
		}
		Order order;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(Order.Schema.JD_ActualWeight, Order.Schema.JD_UnitOfWeight);
					measurePropertiesAndUnits.Add(Order.Schema.JD_ActualVolume, Order.Schema.JD_UnitOfVolume);
					measurePropertiesAndUnits.Add(Order.Schema.Weight, Order.Schema.WeightUnit);
					measurePropertiesAndUnits.Add(Order.Schema.Volume, Order.Schema.VolumeUnit);
					measurePropertiesAndUnits.Add(Order.Schema.JD_Calc_TotalWeight, Order.Schema.JD_Calc_TotalWeightUnit);
					measurePropertiesAndUnits.Add(Order.Schema.JD_Calc_TotalVolume, Order.Schema.JD_Calc_TotalVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
