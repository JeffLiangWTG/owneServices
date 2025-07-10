using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceRegistrationLog : AutoGlbDeviceRegistrationLog
	{
		public GlbDeviceRegistrationLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(V4_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid V4_V3_Device
		{
			get => base.V4_V3_Device;
			set => base.V4_V3_Device = value;
		}
	}
}
