using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceIgnition : AutoGlbDeviceIgnition, IDeviceIgnition
	{
		public GlbDeviceIgnition(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDI_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDI_V3_Device
		{
			get => base.GDI_V3_Device;
			set => base.GDI_V3_Device = value;
		}
	}
}
