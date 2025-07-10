using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTyreReport : AutoGlbDeviceTyreReport, IDeviceTyreReport
	{
		public GlbDeviceTyreReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDR_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDR_V3_Device
		{
			get => base.GDR_V3_Device;
			set => base.GDR_V3_Device = value;
		}
	}
}
