using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.VGMValidation;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	sealed class VGMValidationExtensionsTest : TestCaseWithFactory
	{
		public void TestAddCarrierStateLengthValidation()
		{
			var context = new CommonContext(Factory);

			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddCarrierStateLengthValidation();

			address.ValidateAll();

			const string errorMessage = "Carrier message state information accepts only 9 characters, extra characters will be truncated when sending VGM to carrier";

			AssertNoWarning(address.StateInfo, errorMessage);

			address.State = "12345678910";

			AssertHasWarning(address.StateInfo, errorMessage);

			address.State = "Valid";

			AssertNoWarning(address.StateInfo, errorMessage);
		}

		public void TestAddVerifiedByAddressValidation()
		{
			var context = new CommonContext(Factory);

			var address = AddressBuilder.Create(context, (OrgAddress)null);
			address.AddVerifiedByAddressValidation(Constants.ContainerGrossWeightVerificationTypes.Codes.Method1Container);

			address.ValidateAll();

			const string errorMessage = "The party designated to ascertain the weight is not mandatory but is recommended to be sent in the VGM message";

			AssertHasWarning(address.CompanyNameInfo, errorMessage);

			address.CompanyName = "Name";

			AssertNoWarning(address.CompanyNameInfo, errorMessage);

			address.CompanyName = string.Empty;

			AssertHasWarning(address.CompanyNameInfo, errorMessage);
		}
	}
}
