using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCountryDataJPValidationTest : OrgCountryDataValidationTest
	{
		public void TestDocumentValidation()
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
				AssertHasError(OrgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must match the Valid To Date for the required document type KCA before marking the organization as approved.");

				doc.EQ_ValidToDate = testDate1;

				OrgCountryData.RunPreSaveValidation();
				AssertNoErrors("The required document is attached.", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				AssertNoErrors("The approval number matches", OrgCountryData.OV_EXApprovalNumberInfo);
				AssertNoErrors("The expiry date matches", OrgCountryData.OV_EXApprovalExpiryDateInfo);
			}
		}

		public void TestMandatoryValidation()
		{
			var expiryDate = ZDate.Today.AddDays(10);

			using (new RequiredDocument(Organisation, "KCA", "123", "PER", expiryDate))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				OrgCountryData.OV_OA_ApprovedLocation = ZGuid.Empty;
				OrgCountryData.Validation.ValidateAll();

				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
				AssertHasErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);
				AssertHasErrors(OrgCountryData.OV_OA_ApprovedLocationInfo);

				OrgCountryData.OV_EXApprovalNumber = "123";
				OrgCountryData.OV_EXApprovalExpiryDate = expiryDate;

				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
				AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

				AssertEquals("", OrgCountryData.OV_EXApprovalNumber);
				AssertEquals(ZDate.Empty, OrgCountryData.OV_EXApprovalExpiryDate);

				AssertNoErrors(OrgCountryData.OV_EXApprovalNumberInfo);
				AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);
			}

			using (new RequiredDocument(Organisation, "KCA", "123", "PER", expiryDate))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Empty;
				OrgCountryData.OV_EXApprovalNumber = ZString.Empty;
				OrgCountryData.OV_OA_ApprovedLocation = ZGuid.NewZGuid();
				OrgCountryData.Validation.ValidateAll();

				AssertHasErrors(OrgCountryData.OV_EXApprovalNumberInfo);
				AssertNoErrors(OrgCountryData.OV_EXApprovalExpiryDateInfo);
				AssertEquals(true, OrgCountryData.OV_OA_ApprovedLocation_ReadOnly);
			}
		}

		public void TestDuplicationLocation()
		{
			var expiryDate = ZDate.Today.AddDays(10);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			using (new RequiredDocument(Organisation, "KCA", "12345", "PER", expiryDate))
			{
				OrgAddress address1 = Organisation.Addresses.AddNew();
				OrgAddress address2 = Organisation.Addresses.AddNew();

				OrgCountryDataJP data1 = (OrgCountryDataJP)address1.KnownShipperDetails.AddNew();
				OrgCountryDataJP data2 = (OrgCountryDataJP)address2.KnownShipperDetails.AddNew();

				data1.OV_OH_OrgHeader = Organisation.PK;
				data1.OV_EXApprovedOrMajorExporter = "KC";
				data1.OV_OA_ApprovedLocation = address1.PK;
				data1.OV_EXApprovalNumber = "12345";

				data2.OV_OH_OrgHeader = Organisation.PK;
				data2.OV_EXApprovedOrMajorExporter = "KC";
				data2.OV_OA_ApprovedLocation = address1.PK;
				data2.OV_EXApprovalNumber = "12345";

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

		[TestDate]
		public void TestFutureExpiryDate()
		{
			OrgCountryDataJP data = Factory.NewWithValidTestData<OrgCountryDataJP>();

			data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			data.AddedThroughCollection = true;
			data.OV_EXApprovalNumber = "1234";

			AssertNoErrors(data.OV_EXApprovalExpiryDateInfo);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;

			var jobRequiredDocument = orgHeader.RequiredDocuments.AddNew();
			jobRequiredDocument.EQ_DocCategory = "XXX";
			jobRequiredDocument.EQ_DocType = "KCA";
			jobRequiredDocument.EQ_DocNumber = "1234";
			data.OV_OH_OrgHeader = orgHeader.PK;

			data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			data.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(1);

			AssertHasError(data.OV_EXApprovalExpiryDateInfo, "The Expiry Date must match the Valid To Date for the required document type KCA before marking the organization as approved.");

			data.OV_EXApprovalExpiryDate = ZDate.Empty;

			AssertHasError(data.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

			data.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);

			AssertHasError(data.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

			data.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(3);

			AssertHasError(data.OV_EXApprovalExpiryDateInfo, "The Expiry Date cannot be more than 2 year in the future.");

			data.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(-3);
			Factory.Save();

			TestDateAttribute.Date = DateTime.Today.AddDays(2);

			data.Validation.ValidateAll();

			AssertNoErrors(data.OV_EXApprovalExpiryDateInfo);
			AssertHasWarning(data.OV_EXApprovalExpiryDateInfo, "The Approval has expired.");
		}

		public override void TestOV_EXApprovedOrMajorExporterValidation_RequiredDocument()
		{
			OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			AssertHasError(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);

			FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");

			using (new RequiredDocument(Organisation, "XYZ"))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertHasError("Incorrect doc type", OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
			}

			using (new RequiredDocument(Organisation, "ABC"))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				AssertHasError("Registry setting is ignored for JP", OrgCountryData.OV_EXApprovedOrMajorExporterInfo, ExpectedErrorForMissingRequiredDocument);
			}

			using (new RequiredDocument(Organisation, "KCA"))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertNoErrors("Correct doc type", OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
			}
		}

		public void TestOV_OV_EXApprovedOrMajorExporterShouldNotValidate_WhenSupplyChainSecurityDisabled_ForJP()
		{
			var defaultError = "Please enter an Aviation Security Approved.";

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var orgCountryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = ZString.Empty;
				Assert(orgCountryData.OV_OA_ApprovedLocation.IsEmpty);
				AssertNoErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);

				var address = Organisation.Addresses.AddNew();
				orgCountryData.OV_OA_ApprovedLocation = address.PK;
				AssertHasErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
			}

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var orgCountryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = ZString.Empty;
				AssertNoErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
			}
		}

		#region Implementation

		protected override OrgCountryData GetNewBusinessObjectForTest()
		{
			return Factory.New<OrgCountryDataJP>();
		}

		protected override string ExpectedErrorForMissingRequiredDocument
		{
			get { return "Before flagging this organization as approved, attach a document to eDocs using type \"KCA\" and record the details for the document within the Document Tracking grid."; }
		}

		protected override ZString CountryCodeForTest
		{
			get { return "JP"; }
		}

		protected override BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP; }
		}

		public override void TestOV_EXApprovedOrMajorExporterValidation()
		{
			using (var requiredDocument = new RequiredDocument(Organisation, "KCA", "12345"))
			using (new AviationSecurityEnabler(RegistryItemToEnableSupplyChainSecurity))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodeForTest))
			{
				OrgCountryData.OV_RN_NKClientCountryRelation = CountryCodeForTest;
				OrgCountryData.OV_EXApprovedOrMajorExporter = "XX";
				if (OrgCountryData.SupplyChainSecurityConfiguration.IsEnabled)
				{
					AssertHasErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				}
				else
				{
					AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				}
				OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
			}
		}

		public override void TestOV_EXApprovalExpiryDate_SouthAfrica()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "RA";
				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertNoErrors("Expiry Date is valid", OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertHasError(OrgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

				Assert("Expiry date is read/write", !OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				Assert("Expiry date is blanked", OrgCountryData.OV_EXApprovalExpiryDate.IsEmpty);
				Assert("Expiry date is read only", OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);
			}
		}

		#endregion
	}
}
