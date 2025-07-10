using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	public class CommonContainerDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultNumberOfDecimalsCollection(Module.Freight));

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			var container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_WeightCapacityUQ = Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_VolumeCapacityUQ = Constants.Volume.CubicMetres;
			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			container.JC_GrossWeight = 125.237m;
			container.JC_TareWeight = 47.236m;
			container.JC_DunnageWeight = 162.463m;
			container.JC_TotalHeight = 5.23m;
			container.JC_TotalLength = 4.34m;
			container.JC_TotalWidth = 6.18m;
			container.JC_GrossVolume = 26.801m;
			container.JC_VolumeCapacity = 50.705m;
			container.JC_WeightCapacity = 500.472m;

			packLine.JL_ActualVolume = 5.649m;
			packLine.JL_ActualWeight = 52.321m;

			AssertEquals(262.020m, container.JC_GrossWeight);
			AssertEquals(214.784m, container.JC_Calc_NetWeight);
			AssertEquals(47.236m, container.JC_TareWeight);
			AssertEquals(162.463m, container.JC_DunnageWeight);

			AssertEquals(26.801m, container.JC_GrossVolume);
			AssertEquals(50.705m, container.JC_VolumeCapacity);
			AssertEquals(500.472m, container.JC_WeightCapacity);

			AssertEquals(262.020m, container.JC_Calc_ActualGrossWeightInKgs);
			AssertEquals(52.321m, container.JC_Calc_TotalWeightInKgs);

			AssertEquals(5.649m, container.JC_Calc_TotalVolume);

			AssertEquals(5.649m, container.JC_Calc_TotalVolumeInM3);
			AssertEquals(3.972m, container.JC_Calc_ActualCapacity);
			AssertEquals(33.200m, container.JC_Calc_ContainerCapacity);

			AssertEquals(52.321m, container.JC_Calc_TotalWeight);

			AssertEquals(24000.000m, container.JC_Calc_MaxGrossWeight);
			AssertEquals(2280.000m, container.JC_Calc_TareWeight);
			AssertEquals(52.321m, container.GoodsWeightForBinding);

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

			AssertEquals(262.04m, container.JC_GrossWeight);
			AssertEquals(214.80m, container.JC_Calc_NetWeight);
			AssertEquals(47.24m, container.JC_TareWeight);
			AssertEquals(162.47m, container.JC_DunnageWeight);

			AssertEquals(26.80m, container.JC_GrossVolume);
			AssertEquals(50.70m, container.JC_VolumeCapacity);
			AssertEquals(500.48m, container.JC_WeightCapacity);

			AssertEquals(262.04m, container.JC_Calc_ActualGrossWeightInKgs);
			AssertEquals(52.33m, container.JC_Calc_TotalWeightInKgs);

			AssertEquals(5.64m, container.JC_Calc_TotalVolume);

			AssertEquals(5.64m, container.JC_Calc_TotalVolumeInM3);
			AssertEquals(3.97m, container.JC_Calc_ActualCapacity);
			AssertEquals(33.20m, container.JC_Calc_ContainerCapacity);

			AssertEquals(52.33m, container.JC_Calc_TotalWeight);

			AssertEquals(24000.00m, container.JC_Calc_MaxGrossWeight);
			AssertEquals(2280.00m, container.JC_Calc_TareWeight);
			AssertEquals(52.33m, container.GoodsWeightForBinding);
		}

		public void TestRegistryNotAppliedToContainersOnStandAloneDeclarations()
		{
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

			var container = Factory.New<CommonContainer>();
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_TransportMode] = Constants.TransportModes.Sea;

			var cusContainer = (BusinessObject)Factory.New<Enterprise.Integration.Customs.Shared.IBaseCusContainer>();
			cusContainer[CusContainerSchema.CO_JE] = declaration.PK;
			cusContainer[CusContainerSchema.CO_JC] = container.PK;

			AssertEquals("Default of 3 decimals is applied, registry is ignored", 3, (int)MetaData.GetMetaData(container, container.JC_GrossWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false));

			container.JC_GrossWeight = 157.458m;
			AssertEquals("No rounding applied, registry is ignored", 157.458m, container.JC_GrossWeight);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			container = consol.Containers.AddNew();
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_WeightCapacityUQ = Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			container.JC_VolumeCapacityUQ = Constants.Volume.CubicMetres;

			var shipment = consol.Shipments.AddNew();
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return container; }
		}
		CommonContainer container;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_GrossWeight, CommonContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_NetWeight, CommonContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_TareWeight, CommonContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_DunnageWeight, CommonContainer.Schema.JC_GrossWeightUQ);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_TotalVolume, CommonContainer.Schema.JC_Calc_TotalVolumeUnit);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_TotalWeight, CommonContainer.Schema.JC_Calc_TotalWeightUnit);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_MaxGrossWeight, CommonContainer.Schema.ContainerWeightUnit);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_TareWeight, CommonContainer.Schema.ContainerWeightUnit);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.GoodsWeightForBinding, CommonContainer.Schema.ContainerWeightUnit);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_ActualGrossWeightInKgs, Constants.Weight.Kilograms);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_TotalWeightInKgs, Constants.Weight.Kilograms);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_TotalVolumeInM3, Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_ActualCapacity, Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_Calc_ContainerCapacity, Constants.Volume.CubicMetres);

					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_GrossVolume, CommonContainer.Schema.JC_GrossVolumeUQ);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_VolumeCapacity, CommonContainer.Schema.JC_VolumeCapacityUQ);
					measurePropertiesAndUnits.Add(CommonContainer.Schema.JC_WeightCapacity, CommonContainer.Schema.JC_WeightCapacityUQ);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion
	}
}
