using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLocation : AutoGlbDeviceLocation, IDeviceLocation
	{
		public GlbDeviceLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(V2_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid V2_V3_Device
		{
			get => base.V2_V3_Device;
			set => base.V2_V3_Device = value;
		}

		[ReadOnly(true)]
		public ZDateTime MeasurementTimeLocal => V2_MeasurementTimeUtc.ToLocalBranchTime(Factory);

		[List("Lookups.SpeedLimitStateList")]
		public override ZString V2_SpeedLimitState
		{
			get => base.V2_SpeedLimitState;
			set => base.V2_SpeedLimitState = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (!V2_Location.IsValid)
			{
				V2_Location = ZGeography.Empty;
			}
		}

		public ZDateTime MeasurementTimeUtc => V2_MeasurementTimeUtc;
		public ZGeography Location => V2_Location;
		public ZDecimal Speedkmh => V2_Speedkmh;
		public ZDecimal CompassHeadingDegrees => V2_CompassHeadingDegrees;
		public ZDecimal SpeedLimitKmh => V2_SpeedLimitKmh;
		public ZString SpeedLimitState => V2_SpeedLimitState;
	}
}
