using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal
{
	public class RefCusTariffBRCharacteristicValueCollection : ActiveBusinessObjectCollection<RefCusTariffBRCharacteristicValue>
	{
		public RefCusTariffBRCharacteristicValueCollection(RefCusTariffBRCharacteristic master)
				: base(master.Factory, master, new ZQuery(), RefCusTariffBRCharacteristicValueSchema.ZB2_ZB1_Characteristic)
		{
		}
	}
}


