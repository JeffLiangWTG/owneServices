using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentPackLinerDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForShippingTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			var shipment = Factory.New<AgencyShipment>();
			packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public override BusinessObject BizObj
		{
			get
			{
				return packLine;
			}
		}

		AgencyShipmentPackLine packLine;
		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.JL_ActualVolume, AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.JL_Calc_VolumeToDeliver, AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.CalculatedVolume, AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.CalculatedOutturnedVolume, AgencyShipmentPackLine.Schema.JL_ActualVolumeUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.JL_ActualWeight, AgencyShipmentPackLine.Schema.JL_ActualWeightUQ);
					measurePropertiesAndUnits.Add(AgencyShipmentPackLine.Schema.JL_Calc_WeightToDeliver, AgencyShipmentPackLine.Schema.JL_ActualWeightUQ);
				}

				return measurePropertiesAndUnits;
			}
		}

		Dictionary<ZString, ZString> measurePropertiesAndUnits;
	}
}
