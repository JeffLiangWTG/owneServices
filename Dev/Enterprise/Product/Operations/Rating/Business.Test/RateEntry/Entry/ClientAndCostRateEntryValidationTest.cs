using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	internal class ClientAndCostRateEntryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCartagePickupAddressValidation()
		{
			var consignor = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_Consignor = consignor.PK;
			Factory.Save();

			entry.TI_OA_CartagePickupAddressOverride = consignor.MainAddress.PK;
			AssertNoErrors(entry.TI_OA_CartagePickupAddressOverrideInfo);

			entry.TI_OA_CartagePickupAddressOverride = ZGuid.NewZGuid();
			AssertHasErrors(entry.TI_OA_CartagePickupAddressOverrideInfo);

			entry.TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
			AssertNoErrors(entry.TI_OA_CartagePickupAddressOverrideInfo);
		}

		public void TestCartageDeliveryAddressValidation()
		{
			var consignee = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_Consignee = consignee.PK;
			Factory.Save();

			entry.TI_OA_CartageDeliveryAddressOverride = consignee.MainAddress.PK;
			AssertNoErrors(entry.TI_OA_CartageDeliveryAddressOverrideInfo);

			entry.TI_OA_CartageDeliveryAddressOverride = ZGuid.NewZGuid();
			AssertHasErrors(entry.TI_OA_CartageDeliveryAddressOverrideInfo);

			entry.TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
			AssertNoErrors(entry.TI_OA_CartageDeliveryAddressOverrideInfo);
		}

		public void TestTI_RateStartDate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);

			var line = entry.AddFlatRateLine("FRT", 100);
			line.TL_RateStartDate = entry.TI_RateStartDate;

			entry.TI_RateStartDate = entry.TI_RateStartDate.AddDays(1);
			AssertHasError(entry.TI_RateStartDateInfo, "The Start Date of Rate Entry must be the same day or before the Start Date of all Rate Lines under this Rate Entry.");

			entry.TI_RateStartDate = line.TL_RateStartDate;
			AssertNoNotifications(entry.TI_RateStartDateInfo);
		}

		public void TestTI_RateEndDate()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX", removeLines: true);
			entry.TI_RateEndDate = ZDate.Today.AddDays(30);
			var line = entry.AddFlatRateLine("FRT", 100);
			line.TL_RateEndDate = entry.TI_RateEndDate;

			entry.TI_RateEndDate = line.TL_RateEndDate.AddDays(-1);
			AssertHasError(entry.TI_RateEndDateInfo, "The Expiry Date of Rate Entry must be the same day or after the Expiry Date of all Rate Lines under this Rate Entry.");

			entry.TI_RateEndDate = line.TL_RateEndDate;
			AssertNoNotifications(entry.TI_RateEndDateInfo);
		}

		public void TestTI_HBLDeliveryMode()
		{
			var consignor = Helper.NewOrgHeader();
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			var entry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			entry.TI_OH_Consignor = consignor.PK;
			Factory.Save();

			entry.TI_HBLDeliveryMode = string.Empty;
			AssertNoErrors(entry.TI_HBLDeliveryModeInfo);

			entry.TI_HBLDeliveryMode = "something";
			AssertHasErrors(entry.TI_HBLDeliveryModeInfo);

			entry.TI_HBLDeliveryMode = "CFS/CFS";
			AssertNoErrors(entry.TI_HBLDeliveryModeInfo);
		}

		#region Helper

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;

		#endregion
	}
}
