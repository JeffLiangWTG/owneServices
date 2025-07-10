using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceExternalVoltage : AutoGlbDeviceExternalVoltage, IDeviceExternalVoltage
	{
		public GlbDeviceExternalVoltage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDV_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDV_V3_Device
		{
			get => base.GDV_V3_Device;
			set => base.GDV_V3_Device = value;
		}
	}
}
