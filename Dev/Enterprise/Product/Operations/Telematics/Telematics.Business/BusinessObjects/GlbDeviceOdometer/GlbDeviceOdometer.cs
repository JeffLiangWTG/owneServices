using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceOdometer : AutoGlbDeviceOdometer, IDeviceOdometer
	{
		public GlbDeviceOdometer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDO_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDO_V3_Device
		{
			get => base.GDO_V3_Device;
			set => base.GDO_V3_Device = value;
		}
	}
}
