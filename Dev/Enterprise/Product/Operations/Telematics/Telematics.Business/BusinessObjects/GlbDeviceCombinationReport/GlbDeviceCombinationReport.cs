using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceCombinationReport : AutoGlbDeviceCombinationReport
	{
		public GlbDeviceCombinationReport(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDC_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDC_V3_Device
		{
			get => base.GDC_V3_Device;
			set => base.GDC_V3_Device = value;
		}
	}
}
