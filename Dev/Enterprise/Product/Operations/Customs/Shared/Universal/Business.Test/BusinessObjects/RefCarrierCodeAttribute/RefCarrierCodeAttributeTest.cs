using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierCodeAttribute))]
	internal class RefCarrierCodeAttributeTest : EnterpriseBusinessObjectTestCase
	{
		RefCarrierCode CarrierCode => carrierCode ?? (carrierCode = Factory.NewWithValidTestData<RefCarrierCode>());
		RefCarrierCode carrierCode;
		protected override BusinessObject GetNewBusinessObject()
		{
			return CarrierCode.Attributes.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return new UniversalReferenceTestDataHelper(factory).CreateCarrierCode("CDE", "DESC", Core.Constants.CountryCodes.Italy).Attributes.AddNew("NAME", "VALUE");
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
