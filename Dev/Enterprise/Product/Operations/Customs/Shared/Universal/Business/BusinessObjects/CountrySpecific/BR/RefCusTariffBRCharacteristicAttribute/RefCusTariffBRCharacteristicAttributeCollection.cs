using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffBRCharacteristicAttributeCollection : ActiveBusinessObjectCollection<RefCusTariffBRCharacteristicAttribute>
	{
		public RefCusTariffBRCharacteristicAttributeCollection(RefCusTariffBRCharacteristic master)
				: base(master.Factory, master, new ZQuery(), RefCusTariffBRCharacteristicAttributeSchema.ZB3_ZB1_Characteristic)
		{
		}
	}
}

