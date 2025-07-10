using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTemperature : AutoGlbDeviceTemperature, IDeviceTemperature
	{
		public GlbDeviceTemperature(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDT_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDT_V3_Device
		{
			get => base.GDT_V3_Device;
			set => base.GDT_V3_Device = value;
		}
	}
}
