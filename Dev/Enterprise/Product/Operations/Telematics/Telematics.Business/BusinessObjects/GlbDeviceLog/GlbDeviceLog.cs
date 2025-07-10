using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLog : AutoGlbDeviceLog, IDeviceLog
	{
		public GlbDeviceLog(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDL_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDL_V3_Device
		{
			get => base.GDL_V3_Device;
			set => base.GDL_V3_Device = value;
		}
	}
}
