using Enterprise.ZArchitecture.Core;

namespace Enterprise.Telematics.Business
{
	public class TelSubEquipmentLookups : AutoTelSubEquipmentLookups
	{
		public TelSubEquipmentLookups(AutoTelSubEquipment parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList TelSubEquipmentTypeList => GetTelSubEquipmentTypeList();

		ReadOnlyCodeDescriptionPairList GetTelSubEquipmentTypeList()
		{
			return new TelSubEquipmentTypeList();
		}
	}
}
