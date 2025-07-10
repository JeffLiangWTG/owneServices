using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class HarmonizedNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			var commodity = Factory.New<Trip>().Shipments.AddNew().Commodities.AddNew();
			var cusCode = commodity.HarmonizedNumbers.AddNew();
			var validCode = Factory.LoadTop1<USCTariff>(new USCTariffCollection(Factory).CompleteFilter).UE_Tariff;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusCode.CY_DataInfo);
			var messageError = "The harmonized number has invalid length, it should be either 6, 8 or 10 digits.";
			cusCode.CY_Data = "1234";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			cusCode.CY_Data = "1234567";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			cusCode.CY_Data = "123456789";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			messageError = "The harmonized number is not recognized as a valid tariff. Please check the tariff or, " +
				"if necessary, send a query to customs for the latest tariff information (Customs Declarations->Actions->Reference File Request->Tariff).";
			cusCode.CY_Data = "123456";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			cusCode.CY_Data = "12345678";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			cusCode.CY_Data = "1234567890";
			AssertHasMessageError(cusCode.CY_DataInfo, messageError);
			cusCode.CY_Data = validCode.Left(6);
			AssertNoMessageErrors(cusCode.CY_DataInfo);
			cusCode.CY_Data = validCode.Left(8);
			AssertNoMessageErrors(cusCode.CY_DataInfo);
			cusCode.CY_Data = validCode;
			AssertNoMessageErrors(cusCode.CY_DataInfo);
			messageError = "Some of the harmonized numbers are invalid. Please check Harmonized Numbers tab for more details.";
			cusCode.CY_Data = "8001";
			AssertHasMessageError(commodity.BY_HarmonizedNumbersInfo, messageError);
			cusCode.CY_Data = validCode;
			AssertNoMessageError(commodity.BY_HarmonizedNumbersInfo, messageError);
		}
	}
}
