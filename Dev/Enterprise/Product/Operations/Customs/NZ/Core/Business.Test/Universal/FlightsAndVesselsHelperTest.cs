using CargoWise.Types;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.Universal.Testing;

	public class FlightsAndVesselsHelperTest : TestCaseWithFactory
	{
		public void TestIsValidFlightOrVessel()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "HAPLOYD", "HAPLOYD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			Assert(!FlightsAndVesselsHelper.IsValidFlightOrVessel(Factory, "INVALID"));
			Assert(FlightsAndVesselsHelper.IsValidFlightOrVessel(Factory, "HAPLOYD"));
		}
	}
}
