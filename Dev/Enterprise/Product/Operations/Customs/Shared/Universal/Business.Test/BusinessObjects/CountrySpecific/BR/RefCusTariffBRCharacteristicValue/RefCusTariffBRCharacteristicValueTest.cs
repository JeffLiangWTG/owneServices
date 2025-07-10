using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffBRCharacteristicValue))]
	class RefCusTariffBRCharacteristicValueTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var characteristic = RefCusTariffBRCharacteristicTest.CreateRefCusTariffBRCharacteristic(Factory, Core.Constants.CountryCodes.Brazil, "HSN", "56049000");
			var value = characteristic.Values.AddNew();
			value.ZB2_Value = "01";
			value.ZB2_Description = "FOR USE IN AGRICULTURE";
			return value;
		}
	}
}
