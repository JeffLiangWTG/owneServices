using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class DocumentShipmentDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("No change in transport mode expected during the lifecycle of this bizObj", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			documentShipment = new DocumentShipment(shipment, Constants.DataContext.FreightLabels);
			documentShipment.OldWeightUnit = Core.Constants.Weight.Kilograms;
			documentShipment.NewWeightUnit = Core.Constants.Weight.Kilograms;
			documentShipment.OldVolumeUnit = Core.Constants.Volume.CubicMetres;
			documentShipment.NewVolumeUnit = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return documentShipment; }
		}
		DocumentShipment documentShipment;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(DocumentShipment.Schema.OldWeight, DocumentShipment.Schema.OldWeightUnit);
					measurePropertiesAndUnits.Add(DocumentShipment.Schema.OldVolume, DocumentShipment.Schema.OldVolumeUnit);
					measurePropertiesAndUnits.Add(DocumentShipment.Schema.NewWeight, DocumentShipment.Schema.NewWeightUnit);
					measurePropertiesAndUnits.Add(DocumentShipment.Schema.NewVolume, DocumentShipment.Schema.NewVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
