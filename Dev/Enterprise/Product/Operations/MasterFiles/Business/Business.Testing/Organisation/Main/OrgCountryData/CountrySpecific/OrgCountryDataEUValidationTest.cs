using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCountryDataEUValidationTest : OrgCountryDataEUStyleValidationTest<OrgCountryDataEU>
	{
		public void TestValidateRAApprovalNumber()
		{
			var expectedError = "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.";

			foreach (var country in CountriesToTest)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
					OrgCountryData.OV_EXApprovalNumber = "XY12345";
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, expectedError);

					OrgCountryData.OV_EXApprovalNumber = "RA87654";
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, expectedError);

					OrgCountryData.OV_EXApprovalNumber = "A2345-56";
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, expectedError);

					OrgCountryData.OV_EXApprovalNumber = "12345-6X";
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, expectedError);

					OrgCountryData.OV_EXApprovalNumber = "12345-67";
					AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
				}
			}
		}

		public override void TestOV_EXApprovedOrMajorExporterValidation_RequiredDocument()
		{
			using (new AviationSecurityEnabler(RegistryItemToEnableSupplyChainSecurity))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				foreach (var countryCode in CountriesToTest)
				{
					if (countryCode == "DE" || countryCode == "GB")
					{
						Assert("AC is not applicable for DE or GB", true);
						continue;
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
					{
						Organisation.MainAddress.OA_RN_NKCountryCode = countryCode;
						Organisation.MainAddress.KnownShipperDetails.DeleteAll();

						var orgCountryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
						orgCountryData.OV_OH_OrgHeader = Organisation.PK;
						orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
						AssertHasError(orgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);

						FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
						{
							using (new RequiredDocument(Organisation, "XYZ"))
							{
								orgCountryData.OV_EXApprovedOrMajorExporter = "NO";
								orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
								AssertHasError("Incorrect doc type", orgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
							}

							using (new RequiredDocument(Organisation, "ABC"))
							{
								orgCountryData.OV_EXApprovedOrMajorExporter = "NO";
								orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
								AssertHasError("Registry setting is not applicable", orgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
							}

							using (new RequiredDocument(Organisation, "KCA"))
							{
								orgCountryData.OV_EXApprovedOrMajorExporter = "NO";
								orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
								AssertNoErrors("Correct doc type", orgCountryData.OV_EXApprovedOrMajorExporterInfo);
							}
						}
					}
				}
			}
		}

		public void TestOV_EXApprovedOrMajorExporter_RequiredDocumentWithOptionalApprovalNumber()
		{
			const string expectedErrorForMissingRequiredDocument = "Before flagging this organization as approved, attach a document to eDocs using type \"KCA\" and record the details for the document within the Document Tracking grid.";
			const string expectedErrorForRequiredDocumentNumberDoesNotMatch = "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				Organisation.MainAddress.OA_RN_NKCountryCode = "FR";
				OrgCountryData.OV_OH_OrgHeader = Organisation.PK;
				OrgCountryData.OV_EXApprovedOrMajorExporter = "AC";
				AssertHasError(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, expectedErrorForMissingRequiredDocument);

				using (new RequiredDocument(Organisation, "KCA", "12345-67"))
				{
					OrgCountryData.Validation.ValidateAll();
					AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);

					OrgCountryData.OV_EXApprovalNumber = "99999-99";
					AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, expectedErrorForRequiredDocumentNumberDoesNotMatch);

					OrgCountryData.OV_EXApprovalNumber = "12345-67";
					AssertNoErrors("No error as approval number matches", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertNoErrors("No error as approval number matches", OrgCountryData.OV_EXApprovalNumberInfo);
				}
			}
		}

		public void TestDuplicateLocation()
		{
			var validToDate = ZDate.Today.AddDays(1);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("IT"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			using (new RequiredDocument(Organisation, "KCA", "A1234-43"))
			{
				Organisation.Addresses.RemoveAll();

				var address1 = Organisation.Addresses.AddNew();
				var address2 = Organisation.Addresses.AddNew();

				var data1 = address1.KnownShipperDetails.AddNew();
				var data2 = address2.KnownShipperDetails.AddNew();

				data1.OV_OH_OrgHeader = Organisation.PK;
				data1.OV_EXApprovedOrMajorExporter = "AC";
				data1.OV_OA_ApprovedLocation = address1.PK;
				data1.OV_EXApprovalNumber = "A1234-43";
				data1.OV_EXApprovalExpiryDate = validToDate;

				data2.OV_OH_OrgHeader = Organisation.PK;
				data2.OV_EXApprovedOrMajorExporter = "AC";
				data2.OV_EXApprovalNumber = "A1234-43";
				data2.OV_EXApprovalExpiryDate = validToDate;

				data2.OV_OA_ApprovedLocation = address1.PK;

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
		}

		public void TestOV_EXApprovedOrMajorExporter_OrgLevelApprovalDisallowsAddressLevelApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			using (new RequiredDocument(Organisation, "KCA"))
			{
				var mainAddress = Organisation.MainAddress;
				mainAddress.OA_RN_NKCountryCode = "FR";

				var secondaryAddress = Organisation.Addresses.AddNew();
				secondaryAddress.Address1 = "Secondary Address";
				secondaryAddress.OA_RN_NKCountryCode = "FR";

				var approval1 = mainAddress.KnownShipperDetails.AddNew();
				approval1.OV_OH_OrgHeader = Organisation.PK;
				approval1.OV_EXApprovedOrMajorExporter = "AC";

				var approval2 = secondaryAddress.KnownShipperDetails.AddNew();
				approval2.OV_OH_OrgHeader = Organisation.PK;
				approval2.OV_EXApprovedOrMajorExporter = "AC";

				AssertHasError(approval2.OV_EXApprovedOrMajorExporterInfo, "Account Consignor approval status is given to the whole company so the code per address does not need to be defined. Suggest to use single AC type with Main Organization address.");

				approval2.OV_EXApprovedOrMajorExporter = "KC";

				AssertHasError(approval2.OV_EXApprovedOrMajorExporterInfo, "Account Consignor approval status is given to the whole company so other approval types are not possible.");

				approval1.OV_EXApprovedOrMajorExporter = "RA";

				AssertNoErrors(approval1.OV_EXApprovedOrMajorExporterInfo);
				AssertNoErrors(approval2.OV_EXApprovedOrMajorExporterInfo);
			}
		}

		#region Implementation

		protected override IEnumerable<string> CountriesToTest
		{
			get
			{
				return ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().Union(new[]
				{
					Constants.CountryCodes.Norway,
					Constants.CountryCodes.Switzerland
				});
			}
		}

		protected override BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU; }
		}

		protected override int MaximumApprovalValidityInYearsForAccountConsignors
		{
			get { return 2; }
		}

		protected override int MaximumApprovalValidityInYearsForKnownConsignors
		{
			get { return 5; }
		}

		#endregion
	}
}
