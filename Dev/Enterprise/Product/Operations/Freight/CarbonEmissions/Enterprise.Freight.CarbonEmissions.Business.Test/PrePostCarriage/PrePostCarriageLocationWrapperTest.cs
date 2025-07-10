using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Business.Testing;

public class PrePostCarriageLocationWrapperTest : TestCaseWithFactory
{
	public void TestIsEmpty()
	{
		var location = new PrePostCarriageLocationWrapper("");
		Assert(location.IsEmpty);

		location = new PrePostCarriageLocationWrapper("AUSYD");
		Assert(!location.IsEmpty);

		location = new PrePostCarriageLocationWrapper((ISupportWebAddressValidation)null);
		Assert(location.IsEmpty);

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		location = new PrePostCarriageLocationWrapper(orgAddress);
		Assert(!location.IsEmpty);

		var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
		location = new PrePostCarriageLocationWrapper(docAddress);
		Assert(!location.IsEmpty);
	}

	public void TestIsPort()
	{
		var location = new PrePostCarriageLocationWrapper("");
		Assert(!location.IsPort);

		location = new PrePostCarriageLocationWrapper("AUSYD");
		Assert(location.IsPort);

		location = new PrePostCarriageLocationWrapper((ISupportWebAddressValidation)null);
		Assert(!location.IsPort);

		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		location = new PrePostCarriageLocationWrapper(orgAddress);
		Assert(!location.IsPort);

		var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
		location = new PrePostCarriageLocationWrapper(docAddress);
		Assert(!location.IsPort);
	}

	public void TestEquals()
	{
		var emptyLocation = new PrePostCarriageLocationWrapper("");
		DoAssert(emptyLocation, new PrePostCarriageLocationWrapper(""), true);

		var sydLocation = new PrePostCarriageLocationWrapper("AUSYD");
		DoAssert(sydLocation, emptyLocation, false);
		DoAssert(sydLocation, new PrePostCarriageLocationWrapper("AUMEL"), false);
		DoAssert(sydLocation, new PrePostCarriageLocationWrapper("AUSYD"), true);

		var nullLocation = new PrePostCarriageLocationWrapper((ISupportWebAddressValidation)null);
		DoAssert(nullLocation, emptyLocation, true);
		DoAssert(nullLocation, sydLocation, false);
		DoAssert(nullLocation, new PrePostCarriageLocationWrapper((ISupportWebAddressValidation)null), true);

		var addressGeoLocation = new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10)));
		DoAssert(addressGeoLocation, emptyLocation, false);
		DoAssert(addressGeoLocation, sydLocation, false);
		DoAssert(addressGeoLocation, nullLocation, false);
		DoAssert(addressGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(10, 10))), true);
		DoAssert(addressGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(ZGeography.CreatePoint(100, 10))), false);

		var addressNoGeoLocation = new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2000", city: "Sydney", port: "AUSYD", country: "AU"));
		DoAssert(addressNoGeoLocation, emptyLocation, false);
		DoAssert(addressNoGeoLocation, sydLocation, false);
		DoAssert(addressNoGeoLocation, nullLocation, false);
		DoAssert(addressNoGeoLocation, addressGeoLocation, false);
		DoAssert(addressNoGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2000", city: "Sydney", port: "AUSYD", country: "AU")), true);
		DoAssert(addressNoGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2001", city: "Sydney", port: "AUSYD", country: "AU")), false);
		DoAssert(addressNoGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2000", city: "Bondi", port: "AUSYD", country: "AU")), false);
		DoAssert(addressNoGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2000", city: "Sydney", port: "AUMEL", country: "AU")), false);
		DoAssert(addressNoGeoLocation, new PrePostCarriageLocationWrapper(CreateAddress(postcode: "2000", city: "Sydney", port: "AUSYD", country: "NZ")), false);

		void DoAssert(PrePostCarriageLocationWrapper lhs, PrePostCarriageLocationWrapper rhs, bool expectedEquals)
		{
			AssertEquals(expectedEquals, lhs.Equals(rhs));
			if (expectedEquals)
			{
				AssertEquals(lhs.GetHashCode(), rhs.GetHashCode());
			}
			else
			{
				AssertNotEquals(lhs.GetHashCode(), rhs.GetHashCode());
			}
		}
	}

	OrgAddress CreateAddress(ZGeography? geoLocation = null, string postcode = "", string city = "", string port = "", string country = "")
	{
		var address = Factory.New<OrgAddress>();
		address.OA_GeoLocation = geoLocation ?? ZGeography.Empty;
		address.OA_Address1 = "Address";
		address.OA_PostCode = postcode;
		address.OA_City = city;
		address.OA_RL_NKRelatedPortCode = port;
		address.OA_RN_NKCountryCode = country;

		return address;
	}
}
