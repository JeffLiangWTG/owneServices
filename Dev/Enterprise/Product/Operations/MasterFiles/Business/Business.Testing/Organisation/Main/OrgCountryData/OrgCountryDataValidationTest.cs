using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgCountryDataValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestOV_EXApprovedOrMajorExporterValidation()
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
				OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
				AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
			}
		}

		public void TestOV_EXApprovedOrMajorExporterValidation_NotInCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
				data.OV_EXApprovedOrMajorExporter = "BB";
				AssertNoErrors("Validation does not run for Address level scheme where OrgCountryData is not in collection", data.OV_EXApprovedOrMajorExporterInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.HongKong;
				data.OV_EXApprovedOrMajorExporter = "CC";
				AssertNoErrors("Validation does not run for Address level scheme where OrgCountryData is not in collection", data.OV_EXApprovedOrMajorExporterInfo);
			}
		}

		public void TestOV_EXApprovalExpiryDateValidation_NotInCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Japan;
				data.OV_EXApprovedOrMajorExporter = "YES";
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_EXApprovalExpiryDate = ZDate.Empty;
				AssertNoErrors("Validation does not run for ApprovalExpiryDate where OrgCountryData is not in collection", data.OV_EXApprovalExpiryDateInfo);
			}
		}

		public void TestOV_EXApprovalExpiryDateRange()
		{
			var defaultWarning = "more than 5 year";
			var defaultError = "more than 10 years from now and thus is not valid";
			var overriddenError = "For Account Consignors, the Expiry Date cannot be more than 5 years in the future.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var org = Factory.New<OrgHeader>();
				var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = "YES";
				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(6);
				AssertHasWarningContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultWarning);
				AssertNoError(orgCountryData.OV_EXApprovalExpiryDateInfo, overriddenError);

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(11);
				AssertHasErrorContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultError);
				AssertNoError(orgCountryData.OV_EXApprovalExpiryDateInfo, overriddenError);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var org = Factory.New<OrgHeader>();
				var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
				orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(6);
				AssertNoWarningContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultWarning);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, overriddenError);

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(11);
				AssertNoErrorContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultError);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, overriddenError);
			}
		}

		public void TestOV_EXApprovalExpiryDate_Canada()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_CA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "RA";
				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertNoErrors("Expiry Date is allowed", OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Empty;
				AssertNoErrors("Expiry Date is not required", OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertHasError(OrgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

				Assert("Expiry date is read/write", !OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				Assert("Expiry date is blanked", OrgCountryData.OV_EXApprovalExpiryDate.IsEmpty);
				Assert("Expiry date is read only", OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);
			}
		}

		public virtual void TestOV_EXApprovalExpiryDate_SouthAfrica()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = "RA";
				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertNoErrors("Expiry Date is valid", OrgCountryData.OV_EXApprovalExpiryDateInfo);

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Empty;
				AssertHasError("Expiry Date is required", OrgCountryData.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertHasError(OrgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

				OrgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(400);
				AssertHasError(OrgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date cannot be more than 1 year in the future.");

				Assert("Expiry date is read/write", !OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);

				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				Assert("Expiry date is blanked", OrgCountryData.OV_EXApprovalExpiryDate.IsEmpty);
				Assert("Expiry date is read only", OrgCountryData.OV_EXApprovalExpiryDateInfo.ReadOnly);
			}
		}

		public virtual void TestOV_EXApprovedOrMajorExporterValidation_RequiredDocument()
		{
			using (new AviationSecurityEnabler(RegistryItemToEnableSupplyChainSecurity))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodeForTest))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
				AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);

				FreightDataRegistry.Instance.ApprovedOrganisationRequiredDocType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ABC");
				OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
				OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;

				AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);

				using (new RequiredDocument(Organisation, "XYZ"))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
					OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
					AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				}

				using (new RequiredDocument(Organisation, "ABC"))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "NO";
					OrgCountryData.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
					AssertNoErrors(OrgCountryData.OV_EXApprovedOrMajorExporterInfo);
				}
			}
		}

		public void TestOV_EXApprovedOrMajorExporterValidation_UnitedKingdom_AccountConsignor()
		{
			const string expectedError = "Account Consignor cannot be selected for addresses in the UK as it does not participate in the Account Consignor Scheme.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var countryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				countryData.AddedThroughCollection = true;
				var gbAddress = Organisation.Addresses.AddNew();
				gbAddress.OA_Address1 = "GB Address";
				gbAddress.OA_RN_NKCountryCode = "GB";

				countryData.OV_OA_ApprovedLocation = gbAddress.PK;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				AssertNoError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);

				countryData.OV_EXApprovedOrMajorExporter = "AC";
				AssertHasError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);
			}
		}

		public void TestOV_EXApprovedOrMajorExporterValidation_EU_AccountConsignor()
		{
			const string expectedError = "Account Consignor cannot be selected for addresses in Germany as it does not participate in the Account Consignor Scheme.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var countryData = Organisation.CountryDataCollectionForThisCompany.AddNew();
				countryData.AddedThroughCollection = true;
				var deAddress = Organisation.Addresses.AddNew();
				deAddress.OA_Address1 = "DE Address";
				deAddress.OA_RN_NKCountryCode = "DE";
				var itAddress = Organisation.Addresses.AddNew();
				itAddress.OA_Address1 = "IT Address";
				itAddress.OA_RN_NKCountryCode = "IT";

				countryData.OV_EXApprovedOrMajorExporter = "AC";
				countryData.OV_OA_ApprovedLocation = itAddress.PK;
				AssertNoError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);

				countryData.OV_OA_ApprovedLocation = deAddress.PK;
				AssertHasError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);

				countryData.OV_EXApprovedOrMajorExporter = "KC";
				AssertNoError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);
			}
		}

		public void TestOV_OA_ApprovedLocationValidation_ThroughAddressCollection()
		{
			var address = Organisation.Addresses.AddNew();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var data = address.KnownShipperDetails.AddNew();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertHasErrors(data.OV_OA_ApprovedLocationInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			data = address.KnownShipperDetails.AddNew();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertHasErrors(data.OV_OA_ApprovedLocationInfo);

			data.OV_OA_ApprovedLocation = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertNoErrors(data.OV_OA_ApprovedLocationInfo);
		}

		public void TestOV_OA_ApprovedLocationValidation_ThroughOrgCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var data = GetNewBusinessObjectForTest();
			data.OV_OH_OrgHeader = Organisation.PK;
			AssertNoErrors(data.OV_OA_ApprovedLocationInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			data = GetNewBusinessObjectForTest();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertNoErrors(data.OV_OA_ApprovedLocationInfo);

			data = Organisation.CountryDataCollectionForThisCompany.AddNew();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertHasErrors(data.OV_OA_ApprovedLocationInfo);

			data.OV_OA_ApprovedLocation = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			AssertNoErrors(OrgCountryData.OV_OA_ApprovedLocationInfo);
		}

		public void TestOV_OA_ApprovedLocationValidation_NotInCollection()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var data = GetNewBusinessObjectForTest();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertNoErrors(data.OV_OA_ApprovedLocationInfo);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			data = GetNewBusinessObjectForTest();
			data.OV_OA_ApprovedLocation = ZGuid.Empty;
			AssertNoErrors(data.OV_OA_ApprovedLocationInfo);
		}

		public virtual void TestOV_EXApprovalNumberCannotBeDuplicatedForOneOrganizationAddress()
		{
			if (OrgCountryData.SupplyChainSecurityConfiguration.IsAddressLevelScheme)
			{
				Assert(true);
			}
			else
			{
				var countryData1 = Organisation.CountryDataCollectionForThisCompany.AddNew();
				var countryData2 = Organisation.CountryDataCollectionForThisCompany.AddNew();

				countryData1.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;
				countryData2.OV_EXApprovedOrMajorExporter = ApprovedCodeForTest;

				if (countryData1.SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(ApprovedCodeForTest))
				{
					countryData1.OV_EXApprovalNumber = "123";
					countryData2.OV_EXApprovalNumber = "123";

					AssertHasError("countryData should have error", countryData2.OV_EXApprovalNumberInfo, "Approval Number already exists.");
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestOV_EXApprovalNumber_MustBeUniqueWithinCountry_EU()
		{
			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var countryData = org1.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = "KC";
				countryData.OV_EXApprovalNumber = "12345-00";
				countryData.OV_OA_ApprovedLocation = org1.MainAddress.PK;

				Factory.Save();

				var address1 = org1.Addresses.AddNew();
				address1.OA_RL_NKRelatedPortCode = "DEFRA";
				var countryData1 = Factory.New<OrgCountryData>();
				countryData1.OV_EXApprovedOrMajorExporter = "RA";
				countryData1.OV_OH_OrgHeader = org1.PK;
				countryData1.OV_OA_ApprovedLocation = address1.PK;
				countryData1.OV_EXApprovalNumber = "12345-00";

				var expectedWarning = string.Format("The Approval Number must be unique for the same country/region. The following Organizations use this number already:\r\n{0}", org1.HumanReadableName);
				AssertNoWarning("Approval code can be reused for the same company", countryData1.OV_EXApprovalNumberInfo, expectedWarning);

				var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEHAM"));
				var countryData2 = Factory.New<OrgCountryData>();
				countryData2.AddedThroughCollection = true;
				countryData2.OV_EXApprovedOrMajorExporter = "RA";
				countryData2.OV_OH_OrgHeader = org2.PK;
				countryData2.OV_EXApprovalNumber = "12345-00";

				AssertHasWarning("Approval code should not be used for another DE company", countryData2.OV_EXApprovalNumberInfo, expectedWarning);

				countryData2.OV_EXApprovalNumber = "22222-22";

				AssertNoWarning("No warning as approval code differs", countryData2.OV_EXApprovalNumberInfo, expectedWarning);

				var org3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "FRPAR"));
				var countryData3 = Factory.New<OrgCountryData>();
				countryData3.OV_EXApprovedOrMajorExporter = "RA";
				countryData3.OV_OH_OrgHeader = org3.PK;
				countryData3.OV_EXApprovalNumber = "12345-00";

				AssertNoWarning("Approval code can be reused for a FR company", countryData3.OV_EXApprovalNumberInfo, expectedWarning);
			}
		}

		public void TestOV_EXApprovalNumberCannotBeDuplicatedForDifferentApprovalCodes()
		{
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				OrgCountryData.OV_RN_NKClientCountryRelation = "EU";
				OrgCountryData.OV_EXApprovedOrMajorExporter = "RA";
				OrgCountryData.OV_EXApprovalNumber = "12345-67";

				var address2 = Organisation.Addresses.AddNew();
				var orgCountryData2 = address2.KnownShipperDetails.AddNew();
				orgCountryData2.OV_OH_OrgHeader = Organisation.PK;
				orgCountryData2.OV_EXApprovedOrMajorExporter = "RA";
				orgCountryData2.OV_EXApprovalNumber = "12345-67";

				AssertNoError("Duplicate numbers are allowed for RA", orgCountryData2.OV_EXApprovalNumberInfo, "Approval Number already exists.");

				orgCountryData2.OV_EXApprovedOrMajorExporter = "KC";
				orgCountryData2.Validation.ValidateOV_EXApprovalNumber();

				AssertHasError("Number cannot be used for different approval codes", orgCountryData2.OV_EXApprovalNumberInfo, "Approval Number already exists.");
			}
		}

		public void TestOV_EXApprovalNumber_AllowsApprovalNumber()
		{
			var code = "YES";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			using (var requiredDocument = new RequiredDocument(Organisation, "KCA", "12345"))
			{
				OrgCountryData.OV_EXApprovedOrMajorExporter = code;
				Assert("Precondition 1", OrgCountryData.SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(code));
				Assert("Precondition 2", !OrgCountryData.SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(code));

				OrgCountryData.OV_EXApprovalNumber = "12345";
				AssertNoErrors("Code is allowed", OrgCountryData.OV_EXApprovalNumberInfo);

				OrgCountryData.OV_EXApprovalNumber = "";
				AssertNoErrors("Code is not required", OrgCountryData.OV_EXApprovalNumberInfo);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var org = Factory.New<OrgHeader>();

				using (var requiredDocument = new RequiredDocument(org, "KCA", "12345"))
				{
					var countryData = org.CountryDataCollectionForThisCompany.AddNew();
					code = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					countryData.OV_EXApprovedOrMajorExporter = code;
					Assert("Precondition 3", countryData.SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(code));
					Assert("Precondition 4", countryData.SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(code));

					countryData.OV_EXApprovalNumber = "12345";
					AssertNoErrors("Code is allowed", countryData.OV_EXApprovalNumberInfo);

					countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
					countryData.OV_EXApprovalNumber = "";
					AssertHasError(countryData.OV_EXApprovalNumberInfo, "Please enter an Approval Number.");

					countryData.OV_EXApprovedOrMajorExporter = "NO";
					countryData.OV_EXApprovalNumber = "12345";
					AssertHasError(countryData.OV_EXApprovalNumberInfo, "Please do not enter an Approval Number.");
				}
			}
		}

		public void TestOV_EXApprovalNumber_Australia()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var expectedError = "Please enter an Approval Number.";
				var ownAgentExpectedError = @"Enter the number from the Transport Security Programme (TSP) provided by OTS that proves regulated status of this Organization.  
This is a five-digit number given to each regulated agent, followed by a two-digit suffix.

Note that the number in the TSP may be preceded by a letter but ignore that letter – only enter the five digit number and a two digit suffix, for example 12345-00. 

The two-digit suffix indicates the address which is regulated (e.g. 01 or 02, etc.). If this is only one address use 00.";

				var countryData = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertHasError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData = GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				countryData.OV_EXApprovedOrMajorExporter = "KC";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertHasError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData.OV_EXApprovedOrMajorExporter = "RE";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertHasError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData.OV_EXApprovedOrMajorExporter = "AA";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertHasError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData.OV_EXApprovedOrMajorExporter = "NO";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertNoError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData.OV_EXApprovedOrMajorExporter = "RC";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertNoError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData.OV_EXApprovedOrMajorExporter = "RA";
				countryData.OV_EXApprovalNumber = "12345-00";
				countryData.Validation.ValidateOV_EXApprovalNumber();
				AssertNoError(countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);

				countryData = Organisation.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = Organisation.PK;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				countryData.Validation.ValidateOV_EXApprovedOrMajorExporter();
				AssertNoError("Not company or branch Org Proxy", countryData.OV_EXApprovalNumberInfo, ownAgentExpectedError);
				AssertHasError("Falls back to regular error", countryData.OV_EXApprovalNumberInfo, expectedError);
			}
		}

		public void TestOV_EXApprovalNumberValidation_NotInCollection()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Japan))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Japan;
				data.OV_EXApprovedOrMajorExporter = "YES";
				data.OV_OA_ApprovedLocation = ZGuid.Empty;
				data.OV_EXApprovalNumber = ZString.Empty;
				AssertNoErrors("Validation does not run for ApprovalNumber where OrgCountryData is not in collection", data.OV_EXApprovalNumberInfo);
			}
		}

		public void TestOV_RN_NKIssuingAuthorityCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKIssuingAuthorityCountry = "X0";
				AssertHasError("Invalid country code", data.OV_RN_NKIssuingAuthorityCountryInfo, "Enter a valid Issuing Authority’s Country.");
				data.OV_RN_NKIssuingAuthorityCountry = "IT";
				AssertNoErrors("Valid country code", data.OV_RN_NKIssuingAuthorityCountryInfo);
			}
		}

		public void TestOV_RN_NKIssuingAuthorityCountry_ShouldBeEUMember_WhenClientCountryRelationIsEU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var data = GetNewBusinessObjectForTest();
				data.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.EuropeanUnion;

				foreach (var country in Core.Constants.CountryCodes.EuropeanUnionAviationSecurityMembersList)
				{
					data.OV_RN_NKIssuingAuthorityCountry = country;
					AssertNoErrors("Valid country code", data.OV_RN_NKIssuingAuthorityCountryInfo);
				}

				var allCountries = new RefCountryCollection(Factory);
				foreach (var country in allCountries.Where(country => !Core.Constants.CountryCodes.IsInEuropeanUnionAviationSecurityScheme(country.Code)))
				{
					data.OV_RN_NKIssuingAuthorityCountry = country.Code;
					AssertHasError("Issuing Country should be an EU country when Client country Relation is EU", data.OV_RN_NKIssuingAuthorityCountryInfo, "Please enter an EU country code.");
				}
			}
		}

		#region AddEditApprovalErrorForUncertifiedUser

		public void TestAddEditApprovalErrorForUncertifiedUser_EditExistingApprovals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
			{
				var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));
				var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "GBLON"));

				var countryData = org1.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today;
				countryData.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				countryData.OV_EXApprovalNumber = "12345-00";
				countryData.OV_RN_NKIssuingAuthorityCountry = CountryCodes.Germany;
				countryData.OV_EXExportPermissionDetails = ZString.Empty;
				countryData.OV_OH_OrgHeader = org1.PK;

				Factory.Save();

				const string expectedError = "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this approval.";

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				AssertHasError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);

				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertNoErrors(countryData.OV_EXApprovedOrMajorExporterInfo);

				countryData.OV_EXApprovalExpiryDate = countryData.OV_EXApprovalExpiryDate.AddDays(1);
				AssertHasError(countryData.OV_EXApprovalExpiryDateInfo, expectedError);

				countryData.OV_EXApprovalExpiryDate = countryData.OV_EXApprovalExpiryDate.AddDays(-1);
				AssertNoErrors(countryData.OV_EXApprovalExpiryDateInfo);

				countryData.OV_OA_ApprovedLocation = org2.MainAddress.PK;
				AssertHasError(countryData.OV_OA_ApprovedLocationInfo, expectedError);

				countryData.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				AssertNoErrors(countryData.OV_OA_ApprovedLocationInfo);

				countryData.OV_EXApprovalNumber = "12345-11";
				AssertHasError(countryData.OV_EXApprovalNumberInfo, expectedError);

				countryData.OV_EXApprovalNumber = "12345-00";
				AssertNoErrors(countryData.OV_EXApprovalNumberInfo);

				countryData.OV_RN_NKIssuingAuthorityCountry = CountryCodes.Australia;
				AssertHasError(countryData.OV_RN_NKIssuingAuthorityCountryInfo, expectedError);

				countryData.OV_RN_NKIssuingAuthorityCountry = CountryCodes.Germany;
				AssertNoErrors(countryData.OV_RN_NKIssuingAuthorityCountryInfo);

				countryData.OV_EXExportPermissionDetails = "Dummy Details";
				AssertHasError(countryData.OV_EXExportPermissionDetailsInfo, expectedError);

				countryData.OV_EXExportPermissionDetails = ZString.Empty;
				AssertNoErrors(countryData.OV_EXExportPermissionDetailsInfo);
			}
		}

		public void TestAddEditApprovalErrorForUncertifiedUser_AddNewApprovals()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
			{
				var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));
				var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "GBLON"));

				var countryData = org1.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData.OV_EXApprovalExpiryDate = ZDate.Today;
				countryData.OV_OA_ApprovedLocation = org2.MainAddress.PK;
				countryData.OV_EXApprovalNumber = "12345-00";
				countryData.OV_RN_NKIssuingAuthorityCountry = CountryCodes.Germany;
				countryData.OV_EXExportPermissionDetails = ZString.Empty;

				countryData.Validation.ValidateAll();
				AssertNoErrors(countryData.OV_EXApprovedOrMajorExporterInfo);
				AssertNoErrors(countryData.OV_EXApprovalExpiryDateInfo);
				AssertNoErrors(countryData.OV_OA_ApprovedLocationInfo);
				AssertNoErrors(countryData.OV_EXApprovalNumberInfo);
				AssertNoErrors(countryData.OV_RN_NKIssuingAuthorityCountryInfo);
				AssertNoErrors(countryData.OV_EXExportPermissionDetailsInfo);

				countryData.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				countryData.Validation.ValidateAll();
				const string expectedError = "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this approval.";
				AssertHasError(countryData.OV_EXApprovedOrMajorExporterInfo, expectedError);
				AssertHasError(countryData.OV_EXApprovalExpiryDateInfo, expectedError);
				AssertHasError(countryData.OV_OA_ApprovedLocationInfo, expectedError);
				AssertHasError(countryData.OV_EXApprovalNumberInfo, expectedError);
				AssertHasError(countryData.OV_RN_NKIssuingAuthorityCountryInfo, expectedError);
				AssertHasError(countryData.OV_EXExportPermissionDetailsInfo, expectedError);
			}
		}

		#endregion

		#region Should not validate when Supply Chain Security disabled

		public void TestOV_EXApprovalExpiryDateShouldNotValidate_WhenSupplyChainSecurityDisabled()
		{
			var defaultError = "Please do not enter a Supply Chain Security Expiry Date.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var org = Factory.New<OrgHeader>();
					var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
					orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(1);
					AssertHasErrorContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultError);
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var org = Factory.New<OrgHeader>();
					var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
					orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(1);
					AssertNoErrorContaining(orgCountryData.OV_EXApprovalExpiryDateInfo, defaultError);
				}
			}
		}

		public void TestOV_OV_EXApprovalNumberShouldNotValidate_WhenSupplyChainSecurityDisabled()
		{
			var defaultError = "Please do not enter an Approval Number.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					OrgCountryData.OV_EXApprovalNumber = "123";
					AssertHasErrorContaining(OrgCountryData.OV_EXApprovalNumberInfo, defaultError);
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var org = Factory.New<OrgHeader>();
					var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
					orgCountryData.OV_EXApprovalNumber = "123";
					AssertNoErrorContaining(orgCountryData.OV_EXApprovalNumberInfo, defaultError);
				}
			}
		}

		public void TestOV_OV_EXApprovedOrMajorExporterShouldNotValidate_WhenSupplyChainSecurityDisabled()
		{
			var defaultError = "Enter a valid Aviation Security Approved.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = "AA";
					AssertHasErrorContaining(OrgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var org = Factory.New<OrgHeader>();
					var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
					orgCountryData.OV_EXApprovedOrMajorExporter = "AA";
					AssertNoErrorContaining(orgCountryData.OV_EXApprovedOrMajorExporterInfo, defaultError);
				}
			}
		}

		public void TestOV_OA_ApprovedLocationShouldNotValidate_WhenSupplyChainSecurityDisabled()
		{
			var defaultError = "Please enter an Address.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					OrgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
					OrgCountryData.OV_OA_ApprovedLocation = ZGuid.Empty;
					AssertHasErrorContaining(OrgCountryData.OV_OA_ApprovedLocationInfo, defaultError);
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var org = Factory.New<OrgHeader>();
					var orgCountryData = org.CountryDataCollectionForThisCompany.AddNew();
					orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
					orgCountryData.OV_OA_ApprovedLocation = ZGuid.Empty;
					AssertNoErrorContaining(orgCountryData.OV_OA_ApprovedLocationInfo, defaultError);
				}
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(CountryCodeForTest);
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
			base.TearDown();
		}

		ZString originalCountryCode;

		protected OrgHeader Organisation => organisation ?? (organisation = Factory.LoadTop1<OrgHeader>(new ZQuery()));
		OrgHeader organisation;

		protected OrgCountryData OrgCountryData
		{
			get
			{
				if (orgCountryData == null)
				{
					orgCountryData = GetNewBusinessObjectForTest();
					orgCountryData.OV_OH_OrgHeader = Organisation.PK;
					orgCountryData.AddedThroughCollection = true;

					if (orgCountryData.SupplyChainSecurityConfiguration.IsAddressLevelScheme)
					{
						orgCountryData.OV_OA_ApprovedLocation = Organisation.MainAddress.PK;
					}
				}

				return orgCountryData;
			}
		}
		OrgCountryData orgCountryData;

		protected virtual OrgCountryData GetNewBusinessObjectForTest()
		{
			return Factory.New<OrgCountryData>();
		}

		protected virtual string ApprovedCodeForTest
		{
			get { return AviationSecuritySchemeMembershipEx.Codes.Yes; }
		}

		protected virtual string ExpectedErrorForMissingRequiredDocument
		{
			get { return "A document of type ABC with Document Tracking must be attached to this organization before flagging it as approved.\r\nThis document type can be configured in the Registry at Freight > Supply Chain Security > Default Required Document type for Approved Organization."; }
		}

		protected virtual BooleanRegistryItem RegistryItemToEnableSupplyChainSecurity
		{
			get { return null; }
		}

		protected virtual ZString CountryCodeForTest
		{
			get { return "JM"; }
		}

		protected class AviationSecurityEnabler : IDisposable
		{
			public AviationSecurityEnabler(BooleanRegistryItem registryItemToEnable)
			{
				if (registryItemToEnable != null)
				{
					originalValue = registryItemToEnable.Value;
					registryItemToEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					this.registryItemToEnable = registryItemToEnable;
				}
			}

			public void Dispose()
			{
				if (registryItemToEnable != null)
				{
					registryItemToEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}

			readonly BooleanRegistryItem registryItemToEnable;

			readonly bool originalValue;
		}

		protected class RequiredDocument : IDisposable
		{
			public RequiredDocument(OrgHeader organisation, string requiredDocumentType, string docNumber = "", string docPeriod = "", ZDate? docValidToDate = null)
			{
				tempFile = TempFile.New();
				System.IO.File.WriteAllBytes(tempFile.Filename, new byte[] { 1, 2, 3 });
				this.organisation = organisation;

				((IDocManagerSupport)organisation).DocManagerInfo.AddFileOrDocument(tempFile.Filename, requiredDocumentType);

				JobRequiredDocument = organisation.RequiredDocuments.AddNew();
				JobRequiredDocument.EQ_DocType = requiredDocumentType;
				JobRequiredDocument.EQ_DocNumber = docNumber;
				JobRequiredDocument.EQ_DocPeriod = docPeriod;

				if (docValidToDate != null)
				{
					JobRequiredDocument.EQ_ValidToDate = (ZDate)docValidToDate;
				}
			}

			public void Dispose()
			{
				organisation.RequiredDocuments.RemoveAndDelete(JobRequiredDocument);
				tempFile.Dispose();
				tempFile = null;
			}

			public JobRequiredDocument JobRequiredDocument { get; private set; }

			readonly OrgHeader organisation;

			TempFile tempFile;
		}

		#endregion
	}
}
