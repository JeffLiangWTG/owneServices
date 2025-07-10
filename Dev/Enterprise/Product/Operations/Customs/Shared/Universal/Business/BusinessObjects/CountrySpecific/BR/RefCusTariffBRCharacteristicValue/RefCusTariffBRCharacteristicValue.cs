using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	[DependentBusinessObject(typeof(RefCusTariffBRCharacteristic), "Values")]
	public partial class RefCusTariffBRCharacteristicValue : AutoRefCusTariffBRCharacteristicValue
	{
		public RefCusTariffBRCharacteristicValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
