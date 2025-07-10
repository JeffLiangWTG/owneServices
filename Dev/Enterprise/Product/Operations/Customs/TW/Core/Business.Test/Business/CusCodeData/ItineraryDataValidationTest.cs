using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ItineraryDataValidationTest : CusCodeDataValidationTest
	{
		public void TestCheckCY_CodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itineraryData = declaration.Itineraries.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(itineraryData.CY_CodeInfo, "C1", Core.Constants.CountryCodes.UnitedStates);
		}
	}
}
