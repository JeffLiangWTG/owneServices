using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Business.Testing
{
	public class TrackingBookingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestThirdPartyAddressPK()
		{
			Booking.IsDomesticFreight = false;
			Booking.Validation.ValidateAll();
			Assert("should not have errors while domestic freight is false", !Booking.ThirdPartyAddressPKInfo.HasErrors());
			Booking.IsDomesticFreight = true;
			Booking.Booking.JS_INCO = Core.Constants.DomesticPaymentTerms.CollectThirdParty;
			Booking.Validation.ValidateAll();
			Assert("should be entered when domestic freight", Booking.ThirdPartyAddressPKInfo.HasErrors());
		}

		public void TestOrigin()
		{
			bool initialValue = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;

				Booking.Origin = "AAAAA";
				Booking.Validation.ValidateAll();
				AssertHasErrors(Booking.OriginInfo);
				AssertEquals("BizO.HasErrors", true, Booking.HasErrors);

				Booking.Origin = "AUSYD";
				Booking.Validation.ValidateAll();
				AssertNoErrors(Booking.OriginInfo);
				AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestDestination()
		{
			bool initialValue = Globals.IsWeb;

			try
			{
				Globals.IsWeb = true;

				Booking.Destination = "AAAAA";
				Booking.Validation.ValidateAll();
				AssertHasErrors(Booking.DestinationInfo);
				AssertEquals("BizO.HasErrors", true, Booking.HasErrors);

				Booking.Destination = "AUSYD";
				Booking.Validation.ValidateAll();
				AssertNoErrors(Booking.DestinationInfo);
				AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			}
			finally
			{
				Globals.IsWeb = initialValue;
			}
		}

		public void TestIsDomesticFreight()
		{
			Booking.IsDomesticFreight = true;
			Booking.Origin = "AUSYD";
			Booking.Destination = "HKHKG";
			Booking.IsDomesticFreight = true;
			Booking.Validation.ValidateAll();
			AssertHasErrors(Booking.IsDomesticFreightInfo);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
			Booking.Destination = "AUSYD";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.IsDomesticFreightInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
		}

		public void TestMode()
		{
			AssertEquals("The Mode of Booking is LSE", "LSE", Booking.Mode);
			Booking.Mode = "xtr";
			AssertEquals("Only valid mode can be assigned.", "LSE", Booking.Mode);
			Booking.Mode = "ULD";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.ModeInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
		}

		public void TestServiceLevel()
		{
			Booking.ServiceLevel = "XTR";
			Booking.Validation.ValidateAll();
			AssertHasErrors(Booking.ServiceLevelInfo);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
			Booking.ServiceLevel = "DIR";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.ServiceLevelInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
		}

		public void TestCustomsEntryNumber()
		{
			Booking.CustomsEntryNumber = "01234567890123456789";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.CustomsEntryNumberInfo);
			AssertHasMessageErrors(Booking.CustomsEntryNumberInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			AssertEquals("BizO.HasMessageErrors", true, Booking.HasMessageErrors);
			Booking.CustomsEntryNumber = "33333333L";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.CustomsEntryNumberInfo);
			AssertNoMessageErrors(Booking.CustomsEntryNumberInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			AssertEquals("BizO.HasMessageErrors", false, Booking.HasMessageErrors);
		}

		public void TestGoodsValueCurr()
		{
			Booking.GoodsValueCurr = "";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.GoodsValueCurrInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			Booking.GoodsValueCurr = "AUD";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.GoodsValueCurrInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			Booking.GoodsValueCurr = "ZZZ";
			Booking.Validation.ValidateAll();
			AssertHasErrors("Invalid currency", Booking.GoodsValueCurrInfo);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
		}

		public void TestInsuranceCurrency()
		{
			Booking.InsuranceCurrency = "";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.InsuranceCurrencyInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			Booking.InsuranceCurrency = "AUD";
			Booking.Validation.ValidateAll();
			AssertNoErrors(Booking.InsuranceCurrencyInfo);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			Booking.InsuranceCurrency = "ZZZ";
			Booking.Validation.ValidateAll();
			AssertHasErrors("Invalid currency", Booking.InsuranceCurrencyInfo);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
		}

		public void TestConsignorOrganisationPK()
		{
			AssertPickupDeliveryAddressValidation(Booking.ConsignorPickupAddress);
		}

		public void TestConsigneeOrganisationPK()
		{
			AssertPickupDeliveryAddressValidation(Booking.ConsigneeDeliveryAddress);
		}

		#region Implementation

		void AssertPickupDeliveryAddressValidation(JobDocAddress address)
		{
			Booking.Validation.ValidateAll();
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			AssertEquals("BizO.HasRowErrors", false, Booking.HasRowErrors);

			address.E2_AddressOverride = false;
			Booking.Validation.ValidateAll();
			AssertHasErrors(address.OrganisationPKInfo);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
			AssertEquals("BizO.HasRowErrors", false, Booking.HasRowErrors);
			AssertNoErrors(address.E2_Address1Info);

			address.E2_AddressOverride = true;
			address.E2_Address1 = "";
			Booking.Validation.ValidateAll();
			AssertHasErrors(address.OrganisationPKInfo);
			AssertHasErrors(address.E2_Address1Info);
			AssertEquals("BizO.HasErrors", true, Booking.HasErrors);
			AssertEquals("BizO.HasRowErrors", false, Booking.HasRowErrors);

			SetupOverridenAddress(address, "Address");
			Booking.Validation.ValidateAll();
			AssertNoErrors(address.OrganisationPKInfo);
			AssertNoErrors(address.E2_Address1Info);
			AssertEquals("BizO.HasErrors", false, Booking.HasErrors);
			AssertEquals("BizO.HasRowErrors", false, Booking.HasRowErrors);
		}

		TrackingBooking Booking
		{
			get
			{
				if (booking == null)
				{
					booking = new TrackingBooking(Factory, null);
					booking.Mode = "AIR";

					SetupOverridenAddress(booking.ConsignorPickupAddress, "Pickup");

					SetupOverridenAddress(booking.ConsigneeDeliveryAddress, "Delivery");
				}
				return booking;
			}
		}
		TrackingBooking booking;

		void SetupOverridenAddress(JobDocAddress address, string text)
		{
			address.E2_AddressOverride = true;
			address.E2_CompanyName = text;
			address.E2_Address1 = text;
			address.E2_City = text;
			address.E2_RN_NKCountryCode = ZString.Empty;
		}

		#endregion
	}
}
