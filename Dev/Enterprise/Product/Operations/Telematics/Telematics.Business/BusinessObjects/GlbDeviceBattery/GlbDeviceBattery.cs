using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceBattery : AutoGlbDeviceBattery, IDeviceBattery
	{
		public GlbDeviceBattery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDB_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDB_V3_Device
		{
			get => base.GDB_V3_Device;
			set => base.GDB_V3_Device = value;
		}
	}
}
