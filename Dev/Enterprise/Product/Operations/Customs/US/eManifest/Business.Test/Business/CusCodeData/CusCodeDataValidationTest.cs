using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class CusCodeDataValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			CusCodeData cusCode = Factory.New<Trip>().Shipments.AddNew().Commodities.AddNew().HarmonizedNumbers.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusCode.CY_DataInfo);
			cusCode = Factory.New<Trip>().Shipments.AddNew().Commodities.AddNew().VehicleIdentificationNumbers.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusCode.CY_DataInfo);
			cusCode = Factory.New<Trip>().Shipments.AddNew().Commodities.AddNew().C4Codes.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(cusCode.CY_DataInfo);
		}

		public void TestCheckCY_DataForSeals()
		{
			var trip = Factory.New<Trip>();
			var cusCode = trip.Conveyance.SealNumbers.AddNew();
			AssertNoNotifications(cusCode.CY_DataInfo);
			cusCode.CY_Data = "X";
			AssertNoNotifications(cusCode.CY_DataInfo);
			cusCode.CY_Data = "";
			AssertHasNotifications(cusCode.CY_DataInfo);
		}
	}
}
