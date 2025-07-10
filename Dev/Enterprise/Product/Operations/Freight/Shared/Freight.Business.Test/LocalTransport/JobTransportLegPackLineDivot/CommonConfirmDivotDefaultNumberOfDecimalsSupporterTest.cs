using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConfirmDivotDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public void TestDefaultNumberOfDecimalsAttribute_ConstantUnits()
		{
			AssertDefaultNumberOfDecimalsAttribute(divot, CommonConfirmDivot.Schema.J8_DeliveryWeight, Core.Constants.Weight.Kilograms);
			AssertDefaultNumberOfDecimalsAttribute(divot, CommonConfirmDivot.Schema.J8_DeliveryVolume, Core.Constants.Volume.CubicMetres);
		}

		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];

			divot.J8_DeliveryWeight = 125.561m;
			divot.J8_DeliveryVolume = 5.329m;
			packLine.JL_ActualWeight = 136.592m;
			packLine.JL_ActualVolume = 4.127m;

			AssertEquals(125.561m, divot.J8_DeliveryWeight);
			AssertEquals(5.329m, divot.J8_DeliveryVolume);
			AssertEquals(136.592m, divot.PackLineWeight);
			AssertEquals(4.127m, divot.PackLineVolume);

			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
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

			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(125.57m, divot.J8_DeliveryWeight);
			AssertEquals(5.32m, divot.J8_DeliveryVolume);
			AssertEquals(136.60m, divot.PackLineWeight);
			AssertEquals(4.12m, divot.PackLineVolume);
		}

		// WI00748370 - Remove error logging to resolve performance issues in CS01594069
		//public void TestReportWhenInsertRepeatedRow()
		//{
		//	var shipment = Factory.New<CommonShipment>();
		//	var packline = shipment.OuterPackLines.AddNew();

		//	var confirm = shipment.PickupConfirms.AddNew();
		//	confirm.EU_PickupDeliveryTime = new ZDateTime(2016, 10, 25);
		//	var divot = packline.ConfirmDivots.First();

		//	Factory.Save();

		//	var newFactory = new BusinessObjectFactory();
		//	var duplicateDivot = newFactory.New<CommonConfirmDivot>();
		//	duplicateDivot.J8_JL = divot.J8_JL;
		//	duplicateDivot.J8_EU_PickupDeliverConfirm = divot.J8_EU_PickupDeliverConfirm;
		//	newFactory.Save();

		//	AssertContains("The combination of J8_JL and J8_EU_PickupDeliverConfirm repeatedly save.", ErrorReporter.LastKeyReported);
		//	ErrorReporter.Instance.Clear();
		//}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			var confirm = shipment.DeliveryConfirms.AddNew();

			divot = confirm.Divots[0];
		}

		public override BusinessObject BizObj
		{
			get { return divot; }
		}
		CommonConfirmDivot divot;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(CommonConfirmDivot.Schema.PackLineWeight, CommonConfirmDivot.Schema.PackLineWeightUnit);
					measurePropertiesAndUnits.Add(CommonConfirmDivot.Schema.PackLineVolume, CommonConfirmDivot.Schema.PackLineVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
