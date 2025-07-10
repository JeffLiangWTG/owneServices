using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Telematics.Business
{
	public class TelDeviceAlert : AutoTelDeviceAlert
	{
		public TelDeviceAlert(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(TDA_V3_Device);

		[RelatedBusinessObject(nameof(Device))]
		public override ZGuid TDA_V3_Device
		{
			get => base.TDA_V3_Device;
			set => base.TDA_V3_Device = value;
		}
	}
}
