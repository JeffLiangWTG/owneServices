using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	[DependentBusinessObject(typeof(RefCusTariffBRCharacteristic), "Attributes")]
	public partial class RefCusTariffBRCharacteristicAttribute : AutoRefCusTariffBRCharacteristicAttribute
	{
		public RefCusTariffBRCharacteristicAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
