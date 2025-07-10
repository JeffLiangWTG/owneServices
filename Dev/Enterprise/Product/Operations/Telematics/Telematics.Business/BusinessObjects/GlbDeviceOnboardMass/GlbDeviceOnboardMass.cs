using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Integration;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceOnboardMass : AutoGlbDeviceOnboardMass, IDeviceOnboardMass
	{
		public GlbDeviceOnboardMass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public GlbDevice Device => Factory.Load<GlbDevice>(GDM_V3_Device);
		public TelSubEquipment SubEquipment => Factory.Load<TelSubEquipment>(GDM_TSE_SubEquipment);

		[RelatedBusinessObject(nameof(Device))]
		public override ZGuid GDM_V3_Device
		{
			get => base.GDM_V3_Device;
			set => base.GDM_V3_Device = value;
		}

		[RelatedBusinessObject(nameof(SubEquipment))]
		public override ZGuid GDM_TSE_SubEquipment
		{
			get => base.GDM_TSE_SubEquipment;
			set => base.GDM_TSE_SubEquipment = value;
		}
	}
}
