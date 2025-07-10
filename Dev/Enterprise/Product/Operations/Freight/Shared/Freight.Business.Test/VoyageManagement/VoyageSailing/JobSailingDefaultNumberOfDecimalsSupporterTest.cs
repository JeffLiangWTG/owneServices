using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobSailingDefaultNumberOfDecimalsSupporterTest : DefaultNumberOfDecimalsSupporterForFreightTest
	{
		public override void TestRoundingWhenDefaultNumberOfDecimalsChange()
		{
			// add test to check container values
			Assert("No persistent Weight/Volume properties", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;

			sailing = voyage.Sailings[0];

			Env.Registry.FreightWeightUnit = Constants.Weight.Kilograms;
			Env.Registry.FreightVolumeUnit = Constants.Volume.CubicMetres;
		}

		public override BusinessObject BizObj
		{
			get { return sailing; }
		}
		JobSailing sailing;

		public override Dictionary<ZString, ZString> MeasurePropertiesAndUnits
		{
			get
			{
				if (measurePropertiesAndUnits == null)
				{
					measurePropertiesAndUnits = new Dictionary<ZString, ZString>();
					measurePropertiesAndUnits.Add(JobSailing.Schema.TotalWeight, JobSailing.Schema.TotalWeightUnit);
					measurePropertiesAndUnits.Add(JobSailing.Schema.TotalVolume, JobSailing.Schema.TotalVolumeUnit);
					measurePropertiesAndUnits.Add(JobSailing.Schema.ReceivedWeight, JobSailing.Schema.ReceivedWeightUnit);
					measurePropertiesAndUnits.Add(JobSailing.Schema.ReceivedVolume, JobSailing.Schema.ReceivedVolumeUnit);
				}

				return measurePropertiesAndUnits;
			}
		}
		Dictionary<ZString, ZString> measurePropertiesAndUnits;

		#endregion

	}
}
