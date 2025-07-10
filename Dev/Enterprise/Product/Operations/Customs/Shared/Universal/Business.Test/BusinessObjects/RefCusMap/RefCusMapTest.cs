using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusMap))]
	class RefCusMapTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var cusMapType = helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", true);
			factory.Save();
			return helper.CreateCusMap("REL", "1", "2", ZDateTime.Today, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.SouthAfrica);
		}
	}
}
