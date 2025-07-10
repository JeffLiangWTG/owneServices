using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	internal class OrgCountryDataAUValidationTest : OrgCountryDataValidationTest
	{
		public void TestMandatoryValidation()
		{
			var expiryDate = ZDate.Today.AddDays(10);

			OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			OrgCountryData.Validation.ValidateAll();

			AssertHasErrors(OrgCountryData.OV_EXApprovalNumberInfo);
			AssertHasErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);

			OrgCountryData.OV_EXApprovalNumber = "12345-67";
			OrgCountryData.OV_EXApprovalExpiryDate = expiryDate;

			AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
			AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);

			OrgCountryData.OV_EXApprovalNumber = "XYZ";
			AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

			OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

			AssertEquals(ZDate.Empty, OrgCountryData.OV_EXApprovalExpiryDate);

			AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);
		}

		public void TestDuplicationLocation()
		{
			var expiryDate = ZDate.Today.AddDays(10);

			var address1 = Organisation.Addresses.AddNew();
			var address2 = Organisation.Addresses.AddNew();

			var data1 = (OrgCountryDataAU)address1.KnownShipperDetails.AddNew();
			var data2 = (OrgCountryDataAU)address2.KnownShipperDetails.AddNew();

			data1.OV_OH_OrgHeader = Organisation.PK;
			data1.OV_EXApprovedOrMajorExporter = "KC";
			data1.OV_OA_ApprovedLocation = address1.PK;
			data1.OV_EXApprovalNumber = "12345-67";

			data2.OV_OH_OrgHeader = Organisation.PK;
			data2.OV_EXApprovedOrMajorExporter = "KC";
			data2.OV_OA_ApprovedLocation = address1.PK;
			data2.OV_EXApprovalNumber = "12345-67";

			data1.Validation.ValidateAll();

			AssertHasError(data1.OV_OA_ApprovedLocationInfo, "Address cannot be duplicated.");
			AssertHasError(data2.OV_OA_ApprovedLocationInfo, "Address cannot be duplicated.");

			data2.OV_OA_ApprovedLocation = address2.PK;

			data1.Validation.ValidateAll();

			AssertNoErrors(data1.OV_OA_ApprovedLocationInfo);
			AssertNoErrors(data2.OV_OA_ApprovedLocationInfo);

			AssertNoErrors("The same approval numbers can apply to multiple addresses", data1.OV_EXApprovalNumberInfo);
			AssertNoErrors("The same approval numbers can apply to multiple addresses", data2.OV_EXApprovalNumberInfo);
		}

		public void TestMainAddressValidation()
		{
			var address1 = Organisation.MainAddress;
			var address2 = Organisation.Addresses.AddNew();
			address2.AddAddressType(OrgAddressType.Pickup);
			address2.Address1 = "Pickup Address";

			OrgCountryData.OV_EXApprovedOrMajorExporter = "AA";
			OrgCountryData.OV_OA_ApprovedLocation = address1.PK;
			AssertNoError(OrgCountryData.OV_OA_ApprovedLocationInfo, "The Main Address must be selected for Aviation Security Approved of 'AA'.");

			OrgCountryData.OV_OA_ApprovedLocation = address2.PK;
			AssertHasError(OrgCountryData.OV_OA_ApprovedLocationInfo, "The Main Address must be selected for Aviation Security Approved of 'AA'.");

			OrgCountryData.OV_EXApprovedOrMajorExporter = "RA";
			OrgCountryData.OV_OA_ApprovedLocation = address1.PK;
			AssertNoErrors("All addresses are valid for RA", OrgCountryData.OV_OA_ApprovedLocationInfo);

			OrgCountryData.OV_OA_ApprovedLocation = address2.PK;
			AssertNoErrors("All addresses are valid for RA", OrgCountryData.OV_OA_ApprovedLocationInfo);

			OrgCountryData.OV_EXApprovedOrMajorExporter = "KC";
			OrgCountryData.OV_OA_ApprovedLocation = address1.PK;
			AssertNoErrors("All addresses are valid for KC", OrgCountryData.OV_OA_ApprovedLocationInfo);

			OrgCountryData.OV_OA_ApprovedLocation = address2.PK;
			AssertNoErrors("All addresses are valid for KC", OrgCountryData.OV_OA_ApprovedLocationInfo);
		}

		public void TestOV_OV_EXApprovedOrMajorExporterShouldNotValidate_WhenSupplyChainSecurityDisabled_ForAU()
		{
			var defaultError = "Please enter an Aviation Security Approved.";

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgCountryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = ZString.Empty;
				Assert(orgCountryData.OV_OA_ApprovedLocation.IsEmpty);
				AssertNoErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);

				var address = Organisation.Addresses.AddNew();
				orgCountryData.OV_OA_ApprovedLocation = address.PK;
				AssertHasErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
			}

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var orgCountryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = ZString.Empty;
				AssertNoErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
			}
		}

		public void TestMainAddressValidation_WhenSupplyChainSecurityDisabled()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var address2 = Organisation.Addresses.AddNew();

				address2.AddAddressType(OrgAddressType.Pickup);
				address2.Address1 = "Pickup Address";

				OrgCountryData.OV_EXApprovedOrMajorExporter = "AA";
				OrgCountryData.OV_OA_ApprovedLocation = address2.PK;
				AssertHasError(OrgCountryData.OV_OA_ApprovedLocationInfo, "The Main Address must be selected for Aviation Security Approved of 'AA'.");
			}

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var address2 = Organisation.Addresses.AddNew();

				address2.AddAddressType(OrgAddressType.Pickup);
				address2.Address1 = "Pickup Address";

				OrgCountryData.OV_EXApprovedOrMajorExporter = "AA";
				OrgCountryData.OV_OA_ApprovedLocation = address2.PK;
				AssertNoError(OrgCountryData.OV_OA_ApprovedLocationInfo, "The Main Address must be selected for Aviation Security Approved of 'AA'.");
			}
		}

		#region Implementation

		protected override OrgCountryData GetNewBusinessObjectForTest()
		{
			return Factory.New<OrgCountryDataAU>();
		}

		protected override ZString CountryCodeForTest
		{
			get { return "AU"; }
		}

		protected override BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU; }
		}

		protected override string ApprovedCodeForTest
		{
			get { return AviationSecuritySchemeMembership.Codes.KnownConsignor; }
		}

		protected override string ExpectedErrorForMissingRequiredDocument
		{
			get { return string.Empty; }
		}

		#endregion
	}
}
