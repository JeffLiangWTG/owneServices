using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class RateOneOffShipmentDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestValidateOrganisationPK()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsActive = false;
			organisation.OH_Code = "XX";
			organisation.OH_IsConsignor = true;
			organisation.OH_IsConsignee = true;

			var oneOff = Factory.New<RateOneOffShipment>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = organisation.PK;
			address.OA_Code = "2";
			Factory.Save();

			oneOff.DeliveryDocAddress.E2_OA_Address = address.PK;
			oneOff.PickUpDocAddress.E2_OA_Address = address.PK;
			oneOff.DeliveryDocAddress.Validation.ValidateAll();
			oneOff.PickUpDocAddress.Validation.ValidateAll();

			AssertEquals("Delivery address should have an error as organisation is inactive", true, oneOff.DeliveryDocAddress.HasErrors);
			AssertEquals("Pickup address should have an error as organisation is inactive", true, oneOff.PickUpDocAddress.HasErrors);

			organisation.OH_IsActive = true;
			Factory.Save();
			oneOff.DeliveryDocAddress.Reload();
			oneOff.PickUpDocAddress.Reload();

			oneOff.DeliveryDocAddress.Validation.ValidateAll();
			oneOff.PickUpDocAddress.Validation.ValidateAll();

			AssertEquals("Delivery address shouldn't have an error as organisation is active", false, oneOff.DeliveryDocAddress.HasErrors);
			AssertEquals("Pickup address shouldn't have an error as organisation is active", false, oneOff.PickUpDocAddress.HasErrors);
		}
	}
}
