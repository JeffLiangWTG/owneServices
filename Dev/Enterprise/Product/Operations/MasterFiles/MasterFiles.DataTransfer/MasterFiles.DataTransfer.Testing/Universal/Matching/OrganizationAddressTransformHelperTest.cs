using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.MasterFiles.DataTransfer.OrgMatching.Testing
{
	class OrganizationAddressTransformHelperTest : TestCaseWithFactory
	{
		public void TestGetDeduplicationOrgHeader()
		{
			var organizationAddress = GetOrganizationAddress();
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);

			var orgAddresses = dedupOrgHeader.OrgAddresses.ToList();
			var orgAddress = orgAddresses.FirstOrDefault();
			var cusCodes = dedupOrgHeader.CusCodes.ToList();
			var orgContacts = dedupOrgHeader.OrgContacts.ToList();
			var contact = orgContacts.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals(organizationAddress.OrganizationCode, dedupOrgHeader.OH_Code);
				AssertEquals(organizationAddress.CompanyName, dedupOrgHeader.OH_FullName);
				AssertEquals(organizationAddress.CompanyName, dedupOrgHeader.RawName);
				AssertNotEquals(Guid.Empty, dedupOrgHeader.OH_PK);

				AssertEquals(1, orgAddresses.Count);
				AssertEquals(organizationAddress.Address1, orgAddress.OA_Address1);
				AssertEquals(organizationAddress.Address2, orgAddress.OA_Address2);
				AssertEquals(organizationAddress.AdditionalAddressInformation, orgAddress.OA_AdditionalAddressInformation);
				AssertEquals(organizationAddress.City, orgAddress.OA_City);
				AssertEquals(organizationAddress.AddressShortCode, orgAddress.OA_Code);
				AssertEquals(organizationAddress.Postcode, orgAddress.OA_PostCode);
				AssertEquals(organizationAddress.Port.Code, orgAddress.OA_RL_NKRelatedPortCode);
				AssertEquals(organizationAddress.Country.Code, orgAddress.OA_RN_NKCountryCode);
				AssertEquals(organizationAddress.State, orgAddress.OA_State);
				AssertEquals(organizationAddress.Email, orgAddress.OA_Email);
				AssertEquals(organizationAddress.Fax, orgAddress.OA_Fax);
				AssertEquals(string.Empty, orgAddress.OA_Mobile);
				AssertEquals(organizationAddress.Phone, orgAddress.OA_Phone);
				AssertEquals("NTC", orgAddress.OA_ValidationStatus);
				AssertPKInfo(dedupOrgHeader.OH_PK, orgAddress.OA_OH, orgAddress.OA_PK);

				AssertEquals(4, cusCodes.Count);
				AssertContainCusCode(cusCodes, "55555", "GST", "AU", dedupOrgHeader.OH_PK);
				AssertContainCusCode(cusCodes, "545", "UNC", "AU", dedupOrgHeader.OH_PK);
				AssertContainCusCode(cusCodes, "454", "UOC", "AU", dedupOrgHeader.OH_PK);
				AssertContainCusCode(cusCodes, "1234F", "ATF", "NZ", dedupOrgHeader.OH_PK);

				AssertEquals(1, orgContacts.Count);
				AssertEquals(organizationAddress.Contact, contact.OC_ContactName);
				AssertEquals(organizationAddress.Contact, (contact as DeduplicationOrgContact).RawName);
				AssertEquals(organizationAddress.Email, contact.OC_Email);
				AssertEquals(organizationAddress.Fax, contact.OC_Fax);
				AssertEquals(organizationAddress.Mobile, contact.OC_Mobile);
				AssertEquals(organizationAddress.Phone, contact.OC_Phone);
				AssertPKInfo(dedupOrgHeader.OH_PK, contact.OC_OH, contact.OC_PK);
			});
		}

		void AssertContainCusCode(List<IOrgCusCode> orgCusCodes, string customsRegNo, string codeType, string country, Guid headerPk)
		{
			var cusCode = orgCusCodes.FirstOrDefault(x => x.OK_CustomsRegNo == customsRegNo && x.OK_CodeType == codeType && x.OK_RN_NKCodeCountry == country);
			AssertNotNull(cusCode);
			AssertPKInfo(headerPk, cusCode.OK_OH, cusCode.OK_PK);
		}

		void AssertPKInfo(Guid masterPK, Guid foreignKey, Guid pk)
		{
			AssertEquals(masterPK, foreignKey);
			AssertNotEquals(Guid.Empty, pk);
		}

		public void TestGetDeduplicationOrgHeaderWithoutPort()
		{
			var organizationAddress = GetOrganizationAddress();
			organizationAddress.Port = null;
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);

			var orgAddresses = dedupOrgHeader.OrgAddresses.ToArray();
			AssertEquals(1, orgAddresses.Length);
			AssertEquals("AU", orgAddresses[0].OA_RL_NKRelatedPortCode);
		}

		public void TestGetDeduplicationOrgHeaderWithoutCountry()
		{
			var organizationAddress = GetOrganizationAddress();
			organizationAddress.Country = null;
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);

			var orgAddresses = dedupOrgHeader.OrgAddresses.ToArray();
			AssertEquals(1, orgAddresses.Length);
			AssertEquals(string.Empty, orgAddresses[0].OA_RN_NKCountryCode);
		}

		public void TestGetDeduplicationOrgHeaderWithoutContact()
		{
			var organizationAddress = GetOrganizationAddress();
			organizationAddress.Contact = null;
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			Assert(!dedupOrgHeader.OrgContacts.Any());
		}

		public void TestGetDeduplicationOrgHeaderWithoutCusCode()
		{
			var organizationAddress = GetOrganizationAddress();
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(4, dedupOrgHeader.CusCodes.Count);

			organizationAddress.GovRegNum = null;
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(3, dedupOrgHeader.CusCodes.Count);

			organizationAddress.GovRegNum = "55555";
			organizationAddress.GovRegNumType = null;
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(3, dedupOrgHeader.CusCodes.Count);

			organizationAddress.GovRegNumType = new RegistrationNumberType() { Code = ZString.Empty };
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(3, dedupOrgHeader.CusCodes.Count);

			organizationAddress.UniversalNettingCode = ZString.Empty;
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(2, dedupOrgHeader.CusCodes.Count);

			organizationAddress.UniversalOfficeCode = null;
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			AssertEquals(1, dedupOrgHeader.CusCodes.Count);

			organizationAddress.SetRegistrationNumberCollection(() => new List<RegistrationNumber>() { new RegistrationNumber() { Type = null, Value = "12345" } });
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			Assert(!dedupOrgHeader.CusCodes.Any());

			organizationAddress.SetRegistrationNumberCollection(() => null);
			dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			Assert(!dedupOrgHeader.CusCodes.Any());
		}

		public void TestGetDeduplicationOrgHeaderWithoutContactName()
		{
			var organizationAddress = GetOrganizationAddress();
			organizationAddress.Contact = null;
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(organizationAddress, Factory);
			Assert(!dedupOrgHeader.OrgContacts.Any());

			var orgAddresses = dedupOrgHeader.OrgAddresses.ToArray();
			AssertEquals(1, orgAddresses.Length);
			AssertEquals(organizationAddress.Email, orgAddresses[0].OA_Email);
			AssertEquals(organizationAddress.Fax, orgAddresses[0].OA_Fax);
			AssertEquals(string.Empty, orgAddresses[0].OA_Mobile);
			AssertEquals(organizationAddress.Phone, orgAddresses[0].OA_Phone);
		}

		public void TestGetDeduplicationOrgHeaderWithCountry()
		{
			var orgHeaderForMatching = new Business.OrgHeaderForMatching(Factory);
			orgHeaderForMatching.OH_RL_NKClosestPort = "AUSYD";
			var dedupOrgHeader = OrganizationAddressTransformHelper.GetDeduplicationOrgHeader(orgHeaderForMatching);
			AssertEquals("AU", dedupOrgHeader.CountryCode);
		}

		OrganizationAddress GetOrganizationAddress()
		{
			var result = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "TestType",
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,

				OrganizationCode = "INTHEMSYD",
				CompanyName = "In The Moment",
				Address1 = "Unit 12, Level 3",
				Address2 = "233 Here St",
				City = "ThereVille",
				State = "OfBliss",
				Postcode = "1233",
				Port = new UNLOCO { Code = "AUMEL", Name = "Melbourne" },
				Country = new Country { Code = "AU", Name = "Australia" },

				Contact = "Starshine Moonbeam",
				Email = "s.m@moment.com.au",
				Fax = "234098234",
				Mobile = "234098293",
				Phone = "1239813209",

				ScreeningStatus = new CodeDescriptionPair { Code = "Unk", Description = "Unknown" },

				GovRegNum = "55555",
				GovRegNumType = new RegistrationNumberType { Code = "GST", Description = "GST Code" },
				UniversalOfficeCode = "454",
				UniversalNettingCode = "545",

				AdditionalAddressInformation = "AdditionalAddressInformation"
			};
			result.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber
					{
						Type = new RegistrationNumberType { Code = "ATF", Description = "Approved Transitional Facility" },
						CountryOfIssue = new Country { Code = "NZ", Name = "New Zealand" },
						Value = "1234F",
					},
				}));
			return result;
		}
	}
}
