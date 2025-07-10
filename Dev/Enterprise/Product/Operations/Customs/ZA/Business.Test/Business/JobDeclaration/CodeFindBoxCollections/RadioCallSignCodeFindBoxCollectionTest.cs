using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(RadioCallSignCodeFindBoxCollection))]
	sealed class RadioCallSignCodeFindBoxCollectionTest : ActiveBusinessObjectCollectionTestCase<RadioCallSignCodeFindBoxCollection>
	{
	}

	[TestedType(typeof(RefVesselZZForRadioCallSign))]
	sealed class RefVesselZZForRadioCallSignTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new ZAUniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			return base.GetNewBusinessObjectForDeleteTest(factory);
		}
	}
}
