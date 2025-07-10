using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_DateAtCustomsOffice()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_DateAtCustomsOffice = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_DateAtCustomsOffice = ZDateTime.Now;
			AssertNoMessageErrorContaining(header.AMA_DateAtCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var org = Factory.New<OrgHeader>();

			var orgAddress = org.Addresses.AddNew();
			header.AMA_OA_Carrier = orgAddress.PK;

			AssertHasMessageError(header.AMA_OA_CarrierInfo, "The selected Carrier should have RUT");
		}
	}
}
