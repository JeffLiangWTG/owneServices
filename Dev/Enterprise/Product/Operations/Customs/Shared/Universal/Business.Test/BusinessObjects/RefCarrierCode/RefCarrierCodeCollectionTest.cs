using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierCodeCollection))]
	internal class RefCarrierCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCarrierCodeCollection>
	{
		public void TestCountrySpecificLoading()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCarrierCode("A", "A", Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCarrierCode("A", "A", Core.Constants.CountryCodes.Italy);
			Factory.Save();
			var testCollection = new RefCarrierCodeCollection(Factory, Core.Constants.CountryCodes.Australia);
			AssertEquals("No RefCarrierCode for AU", 0, testCollection.Count);
			testCollection = new RefCarrierCodeCollection(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("No RefCarrierCode for GB", 1, testCollection.Count);
			testCollection = new RefCarrierCodeCollection(Factory, Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("No RefCarrierCode for US", 0, testCollection.Count);
			testCollection = new RefCarrierCodeCollection(Factory, Core.Constants.CountryCodes.Italy);
			AssertEquals("No RefCarrierCode for IT", 1, testCollection.Count);
		}

		protected override RefCarrierCodeCollection GetCollectionToTest()
		{
			return new RefCarrierCodeCollection(Factory, string.Empty);
		}
	}
}
