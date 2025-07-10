using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTariffBRCharacteristicAttribute))]
	class RefCusTariffBRCharacteristicAttributeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var characteristic = RefCusTariffBRCharacteristicTest.CreateRefCusTariffBRCharacteristic(Factory, Core.Constants.CountryCodes.Brazil, "HSN", "56049000");
			return characteristic.Attributes.AddNew();
		}
	}
}
