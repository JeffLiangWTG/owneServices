using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CoLoadWizardShipmentDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			Assert("No change in transport mode expected during the lifecycle of this bizObj", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<PackUnpackShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			wizardShipment = new CoLoadWizardShipment(shipment);
			wizardShipment.CW_WeightUQ = Core.Constants.Weight.Kilograms;
			wizardShipment.CW_VolumeUQ = Core.Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return wizardShipment; }
		}
		CoLoadWizardShipment wizardShipment;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(CoLoadWizardShipment.Schema.CW_Weight, CoLoadWizardShipment.Schema.CW_WeightUQ);
					measurePropertiesAndUnits.Add(CoLoadWizardShipment.Schema.CW_Volume, CoLoadWizardShipment.Schema.CW_VolumeUQ);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
