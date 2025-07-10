using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileType))]
	class RefCusProfileTypeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
