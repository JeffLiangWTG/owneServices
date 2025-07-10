using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonPickupDeliveryConfirmDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public void TestDefaultNumberOfDecimalsAttribute_RelatedBusinessObjectsUnits()
		{
			AssertDefaultNumberOfDecimalsAttribute(pickupDeliveryConfirm, CommonPickupDeliveryConfirm.Schema.TotalBookedWeight, pickupDeliveryConfirm.Container.JC_GrossWeightUQ);
			AssertDefaultNumberOfDecimalsAttribute(pickupDeliveryConfirm, CommonPickupDeliveryConfirm.Schema.TotalBookedVolume, ((IGoods)pickupDeliveryConfirm.Container.PackLines[0]).VolumeUnit);
			AssertDefaultNumberOfDecimalsAttribute(pickupDeliveryConfirm, CommonPickupDeliveryConfirm.Schema.TotalDeliveredWeight, pickupDeliveryConfirm.Container.JC_GrossWeightUQ);
			AssertDefaultNumberOfDecimalsAttribute(pickupDeliveryConfirm, CommonPickupDeliveryConfirm.Schema.TotalDeliveredVolume, pickupDeliveryConfirm.TotalVolumeUnit);
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_PackageCount = 1;
			var container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var pickupDeliveryConfirm = container.OriginConfirm;

			container.JC_GrossWeight = 127.561m;
			packLine.JL_ActualVolume = 3.658m;

			AssertEquals(127.561m, pickupDeliveryConfirm.TotalBookedWeight);
			AssertEquals(127.561m, pickupDeliveryConfirm.TotalDeliveredWeight);
			AssertEquals(3.658m, pickupDeliveryConfirm.TotalBookedVolume);
			AssertEquals(3.658m, pickupDeliveryConfirm.TotalDeliveredVolume);

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

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_TransportMode = Constants.TransportModes.Sea;

			AssertEquals(127.57m, pickupDeliveryConfirm.TotalBookedWeight);
			AssertEquals(127.57m, pickupDeliveryConfirm.TotalDeliveredWeight);
			AssertEquals(3.65m, pickupDeliveryConfirm.TotalBookedVolume);
			AssertEquals(3.65m, pickupDeliveryConfirm.TotalDeliveredVolume);
		}

		public void TestErrorReportedForDivotConsistency()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 1;

			var confirmCollection = packline.PickupConfirms;
			var confirm1 = confirmCollection.AddNew();
			confirm1.EU_JS = shipment.PK;
			confirm1.EU_DriversName = "Fred";

			var confirm2 = confirmCollection.AddNew();
			confirm2.EU_JS = shipment.PK;
			confirm2.EU_DriversName = "George";

			var confirm2Divot = confirm2.Divots.AddNew();
			confirm2Divot.J8_JL = packline.PK;
			confirm2Divot.J8_PackagesDelivered = 1;

			Factory.Save();

			AssertContains("Shipment PackLine count: 1 (1) differs from Divots count: 2 (2)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_PackageCount = 1;
			var container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			pickupDeliveryConfirm = container.OriginConfirm;
		}

		public override BusinessObject BizObj
		{
			get { return pickupDeliveryConfirm; }
		}
		CommonPickupDeliveryConfirm pickupDeliveryConfirm;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get { return new Dictionary<ZString, ZString>(); }
		}

		public override List<ZString> PropertiesWithExternalUnitsToExcludeFromTesting
		{
			get
			{
				if (propertiesWithExternalUnitsToExcludeFromTesting == null)
				{
					propertiesWithExternalUnitsToExcludeFromTesting = new List<ZString>();
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CommonPickupDeliveryConfirm.Schema.TotalBookedWeight);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CommonPickupDeliveryConfirm.Schema.TotalBookedVolume);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CommonPickupDeliveryConfirm.Schema.TotalDeliveredWeight);
					propertiesWithExternalUnitsToExcludeFromTesting.Add(CommonPickupDeliveryConfirm.Schema.TotalDeliveredVolume);
				}

				return propertiesWithExternalUnitsToExcludeFromTesting;
			}
		}
		List<ZString> propertiesWithExternalUnitsToExcludeFromTesting;

		#endregion

	}
}
