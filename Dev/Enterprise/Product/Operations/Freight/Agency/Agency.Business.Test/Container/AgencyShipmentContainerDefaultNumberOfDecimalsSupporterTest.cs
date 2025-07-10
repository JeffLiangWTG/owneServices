using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<AgencyShipment>();
			container = shipment.RealContainers.AddNew();
			container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
			container.JC_GrossVolumeUQ = Constants.Volume.CubicMetres;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get
			{
				return container;
			}
		}

		AgencyShipmentContainer container;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_GrossWeight, AgencyShipmentContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_NetWeight, AgencyShipmentContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_TareWeight, AgencyShipmentContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_DunnageWeight, AgencyShipmentContainer.Schema.JC_GrossWeightUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_TotalVolume, AgencyShipmentContainer.Schema.JC_Calc_TotalVolumeUnit);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_TotalWeight, AgencyShipmentContainer.Schema.JC_Calc_TotalWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_MaxGrossWeight, AgencyShipmentContainer.Schema.ContainerWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_TareWeight, AgencyShipmentContainer.Schema.ContainerWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.GoodsWeightForBinding, AgencyShipmentContainer.Schema.ContainerWeightUnit);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_GrossVolume, AgencyShipmentContainer.Schema.JC_GrossVolumeUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_ActualGrossWeightInKgs, Constants.Weight.Kilograms);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_TotalWeightInKgs, Constants.Weight.Kilograms);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_TotalVolumeInM3, Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_ActualCapacity, Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_Calc_ContainerCapacity, Constants.Volume.CubicMetres);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_VolumeCapacity, AgencyShipmentContainer.Schema.JC_VolumeCapacityUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentContainer.Schema.JC_WeightCapacity, AgencyShipmentContainer.Schema.JC_WeightCapacityUQ);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
