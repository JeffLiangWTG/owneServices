using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCountryDataHKValidationTest : OrgCountryDataEUStyleValidationTest<OrgCountryDataHK>
	{
		public void TestApprovalNumberValidation()
		{
			var expiryDate = ZDate.Today.AddDays(100);

			using (var requiredDocument = new RequiredDocument(Organisation, "KCA", "ABCXYZ", "PER", expiryDate))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				OrgCountryData.OV_EXApprovalNumber = "XY12345";
				AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.");

				OrgCountryData.OV_EXApprovalNumber = "RA87654";
				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				OrgCountryData.OV_EXApprovalNumber = "ABCXYZ";
				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);

				var doc = requiredDocument.JobRequiredDocument;
				doc.EQ_DocNumber = "KC-H0643-13A";

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				OrgCountryData.OV_EXApprovalNumber = "KC-H0643-13A";
				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);

				doc.EQ_DocNumber = "KC-H0643-13A-1";

				OrgCountryData.OV_EXApprovalNumber = "KC-H0643-13A-1";
				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
			}
		}

		public void TestDocumentValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				ZDate testDate1 = ZDate.Today.AddDays(25);
				ZDate testDate2 = ZDate.Today.AddDays(50);

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				OrgCountryData.OV_EXApprovalNumber = "KC-H0643-13A";
				OrgCountryData.OV_EXApprovalExpiryDate = testDate1;

				OrgCountryData.Validation.ValidateAll();
				AssertHasError(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
				AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");
				AssertNoErrors("Date validation can't yet run", OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				AssertNoErrors("Document is not required for Regulated Agent", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				OrgCountryData.OV_EXApprovalExpiryDate = testDate1;

				using (new RequiredDocument(Organisation, "XYZ"))
				{
					OrgCountryData.RunPreSaveValidation();
					AssertHasError(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");
					AssertNoErrors("Date validation can't yet run", OrgCountryData.OV_EXApprovalExpiryDateInfo);
				}

				using (var requiredDocument = new RequiredDocument(Organisation, "KCA", "22222", "PER", testDate2))
				{
					OrgCountryData.RunPreSaveValidation();
					AssertNoErrors("The required document is attached.", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");
					AssertNoErrors("Date validation can't yet run", OrgCountryData.OV_EXApprovalExpiryDateInfo);

					var doc = requiredDocument.JobRequiredDocument;
					doc.EQ_DocNumber = "KC-H0643-13A";

					OrgCountryData.RunPreSaveValidation();
					AssertNoErrors("The required document is attached.", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertNoErrors("The approval number matches", OrgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);

					doc.EQ_ValidToDate = testDate1;

					OrgCountryData.RunPreSaveValidation();
					AssertNoErrors("The required document is attached.", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
					AssertNoErrors("The approval number matches", OrgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors("The expiry date matches", OrgCountryData.OV_EXApprovalExpiryDateInfo);
				}
			}
		}

		public override void TestOV_EXApprovedOrMajorExporterValidation_RequiredDocument()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
				AssertHasError(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);

				FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");

				using (new RequiredDocument(Organisation, "XYZ"))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
					OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
					AssertHasError("Incorrect doc type", OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
				}

				using (new RequiredDocument(Organisation, "ABC"))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
					OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
					AssertHasError("Registry setting is ignored for HK", OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
				}

				using (new RequiredDocument(Organisation, "KCA"))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
					OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
					AssertNoErrors("Correct doc type", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				}
			}
		}

		public void TestValidateRAApprovalNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (RegistryItemToEnableSupplyChainSecurity.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				OrgCountryData.OV_EXApprovalNumber = "XY12345";
				AssertHasError(OrgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.");

				OrgCountryData.OV_EXApprovalNumber = "RA87654";
				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
			}
		}

		#region Implementation

		protected override IEnumerable<string> CountriesToTest
		{
			get { return new[] { "HK" }; }
		}

		protected override BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK; }
		}

		protected override int MaximumApprovalValidityInYearsForAccountConsignors
		{
			get { return 5; }
		}

		protected override int MaximumApprovalValidityInYearsForKnownConsignors
		{
			get { return 3; }
		}

		protected override OrgCountryData GetNewBusinessObjectForTest()
		{
			return Factory.New<OrgCountryDataHK>();
		}

		protected override string ApprovedCodeForTest
		{
			get { return AviationSecuritySchemeMembership.Codes.KnownConsignor; }
		}

		#endregion
	}
}
