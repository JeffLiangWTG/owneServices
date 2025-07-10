using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Registry.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSContainerDefaultNumberOfDecimalsSupporterTest : CommonContainerDefaultNumberOfDecimalsSupporterTest
	{
		public void TestDefaultNumberOfDecimalsAttribute_RegistrylUnits()
		{
			AssertDefaultNumberOfDecimalsAttribute(container, CFSContainer.Schema.TotalShipmentWeight, container.PackUnpackShipments[0].JS_UnitOfWeight);
			AssertDefaultNumberOfDecimalsAttribute(container, CFSContainer.Schema.TotalShipmentVolume, container.PackUnpackShipments[0].JS_UnitOfVolume);
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var consol = Factory.New<CFSLoadListConsol>();
			var container = consol.Containers.AddNew();
			container.JC_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = container.PackUnpackShipments.AddNew();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;

			shipment.JS_ActualWeight = 235.581m;
			shipment.JS_ActualVolume = 3.519m;

			AssertEquals(235.581m, container.TotalShipmentWeight);
			AssertEquals(3.519m, container.TotalShipmentVolume);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.TransportMode = Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			container.JC_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(235.59m, container.TotalShipmentWeight);
			AssertEquals(3.51m, container.TotalShipmentVolume);
		}

		#region Default Number of Decimals

		public void TestGetDefaultNumberOfDecimals()
		{
			var testContainer = Factory.New<CFSContainer>();
			testContainer.JC_JS_FCLBookingOnlyLink = ZGuid.NewZGuid();

			var heightproperty = testContainer.JC_Calc_HeightInfo.PropertyDescriptor;
			var descriptionProperty = testContainer.JC_DescriptionInfo.PropertyDescriptor;

			AssertEquals(3, testContainer.GetDefaultNumberOfDecimals(heightproperty));
			AssertEquals(-1, testContainer.GetDefaultNumberOfDecimals(descriptionProperty));

			testContainer.JC_JS_FCLBookingOnlyLink = Guid.Empty;

			AssertEquals(3, testContainer.GetDefaultNumberOfDecimals(heightproperty));
			AssertEquals(-1, testContainer.GetDefaultNumberOfDecimals(descriptionProperty));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<CFSLoadListConsol>();
			container = consol.Containers.AddNew();
			container.JC_TransportMode = Core.Constants.TransportModes.Air;

			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_WeightCapacityUQ = Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_VolumeCapacityUQ = Constants.Volume.CubicMetres;

			var shipment = container.PackUnpackShipments.AddNew();
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
		}

		public override List<ZString> PropertiesWithExternalUnitsToExcludeFromTesting
		{
			get
			{
				if (propertiesWithExternalUnitsToExcludeFromTesting == null)
				{
					propertiesWithExternalUnitsToExcludeFromTesting = new List<ZString>();
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CFSContainer.Schema.TotalShipmentWeight);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CFSContainer.Schema.TotalShipmentVolume);
				}

				return propertiesWithExternalUnitsToExcludeFromTesting;
			}
		}
		List<ZString> propertiesWithExternalUnitsToExcludeFromTesting;

		public override BusinessObject BizObj
		{
			get { return container; }
		}
		CFSContainer container;

		#endregion
	}
}
