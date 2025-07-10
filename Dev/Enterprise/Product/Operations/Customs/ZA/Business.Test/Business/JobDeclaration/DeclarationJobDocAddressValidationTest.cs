using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DeclarationJobDocAddressValidationTest : TestCaseWithFactory
	{
		public void TestExpectNullDeclaration_Issue00840782()
		{
			var nullDeclaration = Factory.GetNull<JobDeclaration>();
			var address = Factory.New<JobDocAddress>();
			AssertNoExceptionThrown("GenericWrapper creates a null object", delegate
			{
				var validation = new DeclarationJobDocAddressValidation(address, null);
			});
		}

		public void TestDepotBlankWhenRoadExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZAJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declaration.DepotDocAddress.OrganisationPK = orgHeader.PK;
			AssertHasMessageError(declaration.DepotDocAddress.OrganisationPKInfo, DeclarationJobDocAddressValidation.DepotMustBeBlankForRoadExport);
		}

		public void TestCheckE2_OA_Address()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			declaration.DepotDocAddress.E2_AddressType = AutoDocAddressTypes.Codes.CustomsDepotAddress;
			declaration.DepotDocAddress.OrganisationPK = orgHeader.PK;
			declaration.DepotDocAddress.Address.DepotLocalControlledPremisesID = ZString.Empty;
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrors(declaration.DepotDocAddress.E2_OA_AddressInfo);
			declaration.DepotDocAddress.Address.DepotLocalControlledPremisesID = "1";
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(declaration.DepotDocAddress.E2_OA_AddressInfo, "Invalid CPD (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.");
			declaration.DepotDocAddress.Address.DepotLocalControlledPremisesID = "1%";
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(declaration.DepotDocAddress.E2_OA_AddressInfo, "Invalid CPD (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.");
			declaration.DepotDocAddress.Address.DepotLocalControlledPremisesID = "18A";
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(declaration.DepotDocAddress.E2_OA_AddressInfo, "Invalid CPD (Customs Controlled Premises Code - Depot). It has to be two alphanumeric characters.");
			declaration.DepotDocAddress.Address.DepotLocalControlledPremisesID = "1A";
			declaration.DepotDocAddress.Validation.ValidateE2_OA_Address();
			AssertNoMessageErrors(declaration.DepotDocAddress.E2_OA_AddressInfo);
		}
	}
}
