using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class JobShipmentPreplanningDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("Not required as Shipment Pre Advice does not have its own transport mode and will use defaults", true);
		}

		public void TestDefaultNumberOfDecimalsAttribute_NoTransportMode()
		{
			AssertDefaultNumberOfDecimalsAttribute_NoTransportMode(preplanning, preplanning.EF_ActualWeightInfo.PropertyDescriptor, preplanning.EF_UnitOfWeight);
			AssertDefaultNumberOfDecimalsAttribute_NoTransportMode(preplanning, preplanning.EF_ActualVolumeInfo.PropertyDescriptor, preplanning.EF_UnitOfVolume);
		}

		void AssertDefaultNumberOfDecimalsAttribute_NoTransportMode(JobShipmentPreplanning preplanning, PropertyDescriptor propertyDescriptor, ZString unitOfMeasure)
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var decimalsAttribute = MetaData.GetMetaData(preplanning, propertyDescriptor, MetaDataTypes.DecimalPlaces);

			AssertEquals("Weight/Volume properties should have 3 decimals by default.", 3, decimalsAttribute);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_Air = collection.AddNew();
			defaultNumberOfDecimals_Air.UnitOfMeasure = unitOfMeasure;
			defaultNumberOfDecimals_Air.TransportMode = Core.Constants.TransportModes.Air;
			defaultNumberOfDecimals_Air.NumberOfDecimals = 1;
			defaultNumberOfDecimals_Air.RoundingMode = RoundingModes.Up;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			decimalsAttribute = MetaData.GetMetaData(preplanning, propertyDescriptor, MetaDataTypes.DecimalPlaces);

			AssertNotEquals("Weight/Volume property should NOT take value from registry.", 1, decimalsAttribute);
			AssertEquals("Weight/Volume properties should have 3 decimals by default, as JobShipmentPreplanning does NOT have a transport mode.", 3, decimalsAttribute);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			preplanning = Factory.New<JobShipmentPreplanning>();
			preplanning.EF_UnitOfWeight = Core.Constants.Weight.Kilograms;
			preplanning.EF_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return preplanning; }
		}
		JobShipmentPreplanning preplanning;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get { return new Dictionary<ZString, ZString>(); }
		}

		#endregion

	}
}
