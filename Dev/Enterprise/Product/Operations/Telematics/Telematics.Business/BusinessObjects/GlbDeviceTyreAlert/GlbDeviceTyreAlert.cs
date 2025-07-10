using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceTyreAlert : AutoGlbDeviceTyreAlert, IDeviceTyreAlert
	{
		public GlbDeviceTyreAlert(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDA_V3_Device);

		[RelatedBusinessObject("Device")]
		public override ZGuid GDA_V3_Device
		{
			get => base.GDA_V3_Device;
			set => base.GDA_V3_Device = value;
		}
	}
}
