using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	public class OrderLineDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight));

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			var line = order.OrderLines.AddNew();
			line.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;
			line.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			line.JO_ActualWeight = 451.211m;
			line.JO_ActualVolume = 5.129m;

			AssertEquals(451.211m, line.JO_ActualWeight);
			AssertEquals(5.129m, line.JO_ActualVolume);

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

			AssertEquals(451.22m, line.JO_ActualWeight);
			AssertEquals(5.12m, line.JO_ActualVolume);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			line = order.OrderLines.AddNew();
			line.JO_UnitOfWeight = Core.Constants.Weight.Kilograms;
			line.JO_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return line; }
		}
		OrderLine line;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(OrderLine.Schema.JO_ActualWeight, OrderLine.Schema.JO_UnitOfWeight);
					measurePropertiesAndUnits.Add(OrderLine.Schema.JO_ActualVolume, OrderLine.Schema.JO_UnitOfVolume);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
