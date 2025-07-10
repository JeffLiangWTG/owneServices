using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Tools.Telephony;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.MasterFiles.DataTransfer.Universal.Testing
{
	public class OrganizationAddressTestHelper : TestCaseWithFactoryAndMessagingHelpers
	{
		public OrganizationAddressTestHelper(UniversalObjectFactory factory)
		{
			factoryOverride = factory;
		}
		readonly UniversalObjectFactory factoryOverride;

		public OrganizationAddressTestHelper()
			: this(null)
		{
		}

		protected override UniversalObjectFactory NewUniversalObjectFactory()
		{
			return factoryOverride ?? base.NewUniversalObjectFactory();
		}

		protected TestErrorLogger Logger
		{
			get { return logger ?? (logger = new TestErrorLogger()); }
		}

		TestErrorLogger logger;

		protected ZGuid MiscOrgAddressPK
		{
			get { return OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress; }
		}

		public static void AssertAddress(List<OrganizationAddress> addressCollection, int index
			, string addressType, string organizationCode, string companyName, ZBool? addressOverride
			, string address1, string address2, string city, string state, string postcode, string country
			, string contact, string email, string fax, string mobile, string phone)
		{
			Assert("Address Collection needs to have at least " + (index + 1).ToString() + " addresses in it. Cannot find address type [" + addressType + "].", index < addressCollection.Count);

			var addressData = addressCollection[index];
			string message = "Checking addressData[" + index + "]";
			AssertAddress(message, addressData, addressType, organizationCode, companyName, addressOverride, address1, address2, city, state, postcode, country, contact, email, fax, mobile, phone);
		}

		public static void AssertAddressExists(List<OrganizationAddress> addressCollection, string message
			, string addressType, string organizationCode, string companyName, ZBool? addressOverride
			, string address1, string address2, string city, string state, string postcode, string country
			, string contact, string email, string fax, string mobile, string phone)
		{
			var addressData = addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault().Equals(addressType));
			if (addressData == null)
			{
				Fail("addressCollection does not contain element with AddressType: " + addressType);
			}
			else
			{
				AssertAddress(message, addressData, addressType, organizationCode, companyName, addressOverride, address1, address2, city, state, postcode, country, contact, email, fax, mobile, phone);
			}
		}

		static string GetFormattedPhone(string phone, string country)
		{
			if (string.IsNullOrEmpty(country))
			{
				return phone;
			}
			var formattedPhone = PhoneNumberFormatter.Instance.FormatE164(phone, country);
			if (!string.IsNullOrEmpty(formattedPhone))
			{
				return formattedPhone;
			}
			return phone;
		}

		public static void AssertAddress(string message, OrganizationAddress addressData
			, string addressType, string organizationCode, string companyName, ZBool? addressOverride
			, string address1, string address2, string city, string state, string postcode, string country
			, string contact, string email, string fax, string mobile, string phone)
		{
			var addressFormatted = new OrganizationAddressFormatted(addressData);
			CombineAssertions(message, delegate
			{
				AssertEquals(".AddressType", addressType, addressFormatted.AddressType);

				AssertEquals(".OrganizationCode", organizationCode, addressData.OrganizationCode);
				AssertEquals(".CompanyName", companyName, addressFormatted.CompanyName);
				AssertEquals(".AddressOverride", addressOverride, addressFormatted.AddressOverride);

				AssertEquals(".Address1", address1, addressFormatted.Address1);
				AssertEquals(".Address2", address2, addressFormatted.Address2);
				AssertEquals(".City", city, addressFormatted.City);
				AssertEquals(".State", state, addressFormatted.State);
				AssertEquals(".Postcode", postcode, addressFormatted.Postcode);
				if (country == null)
				{
					AssertNull(".Country should be null", addressFormatted.Country);
				}
				else
				{
					AssertEquals(".Country.Code", country, addressFormatted.Country.Code);
				}

				AssertEquals(".Contact", contact, addressFormatted.Contact);
				AssertEquals(".Email", email, addressFormatted.Email);
				AssertEquals(".Fax", fax, addressFormatted.Fax);
				AssertEquals(".Mobile", mobile, addressFormatted.Mobile);
				AssertEquals(".Phone", GetFormattedPhone(phone, country), addressFormatted.Phone);
			});
		}

		public static void AssertOrganizationBO_WUFSHIJNBExists(List<OrganizationAddress> addressCollection, string message, string addressType)
		{
			AssertOrganizationBO_WUFSHIJNBExists(addressCollection, message, addressType, false);
		}

		public static void AssertOrganizationBO_WUFSHIJNBExists(List<OrganizationAddress> addressCollection, string message, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var addressData = addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault().Equals(addressType));
			if (addressData == null)
			{
				Fail("addressCollection does not contain element with AddressType: " + addressType);
			}
			else
			{
				AssertOrganizationBO_WUFSHIJNB(message, addressData, addressType, hasContactInfoFromMatchedContact);
			}
		}

		public static void AssertOrganizationBO_WUFSHIJNB(string message, OrganizationAddress addressData, string addressType)
		{
			AssertOrganizationBO_WUFSHIJNB(message, addressData, addressType, false);
		}

		public static void AssertOrganizationBO_WUFSHIJNB(string message, OrganizationAddress addressData, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var expectedContactName = hasContactInfoFromMatchedContact ? "Benny Banana" : null;
			var expectedEmail = hasContactInfoFromMatchedContact ? "benny.banana@wufu.co.za" : "";
			var expectedFax = hasContactInfoFromMatchedContact ? "0011 54 392 2921" : "";
			var expectedMobile = hasContactInfoFromMatchedContact ? "0011 289 392 2900" : null;
			var expectedPhone = hasContactInfoFromMatchedContact ? "0011 54 392 2900" : "";

			AssertAddress(message, addressData
				, addressType, "WUFSHIJNB", "WUFU SHIPPING LINE", false
				, "Level 2, Building G", "34 Dock Lane", "Johannesburg", "", "12345", "ZA"
				, expectedContactName, expectedEmail, expectedFax, expectedMobile, expectedPhone);
		}

		public static void AssertOrganizationBO_CRAHOLSYD(string message, OrganizationAddress addressData, string addressType)
		{
			AssertOrganizationBO_CRAHOLSYD(message, addressData, addressType, false);
		}

		public static void AssertOrganizationBO_CRAHOLSYD(string message, OrganizationAddress addressData, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var expectedContactName = hasContactInfoFromMatchedContact ? "Rob Anybody" : null;
			var expectedEmail = hasContactInfoFromMatchedContact ? "rob.anybody@cjh.com.au" : "";
			var expectedFax = hasContactInfoFromMatchedContact ? "02 6392 2921" : "";
			var expectedMobile = hasContactInfoFromMatchedContact ? "0421 392 290" : null;
			var expectedPhone = hasContactInfoFromMatchedContact ? "02 6392 2900" : "";

			AssertAddress(message, addressData
				, addressType, "CRAHOLSYD", "CRACKERJACK HOLDINGS", false
				, "1804 Fudrucker Way", "", "BOTANY", "NSW", "2035", "AU"
				, expectedContactName, expectedEmail, expectedFax, expectedMobile, expectedPhone);
		}

		public static void AssertOrganizationBO_CRAHOLSYDExists(List<OrganizationAddress> addressCollection, string message, string addressType)
		{
			AssertOrganizationBO_CRAHOLSYDExists(addressCollection, message, addressType, false);
		}

		public static void AssertOrganizationBO_CRAHOLSYDExists(List<OrganizationAddress> addressCollection, string message, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var addressData = addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault().Equals(addressType));
			if (addressData == null)
			{
				Fail("addressCollection does not contain element with AddressType: " + addressType);
			}
			else
			{
				AssertOrganizationBO_CRAHOLSYD(message, addressData, addressType, hasContactInfoFromMatchedContact);
			}
		}

		public static void AssertOrganizationBO_INTHEMSYDExists(List<OrganizationAddress> addressCollection, string message, string addressType)
		{
			AssertOrganizationBO_INTHEMSYDExists(addressCollection, message, addressType, false);
		}

		public static void AssertOrganizationBO_INTHEMSYDExists(List<OrganizationAddress> addressCollection, string message, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var addressData = addressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault().Equals(addressType));
			if (addressData == null)
			{
				Fail("addressCollection does not contain element with AddressType: " + addressType);
			}
			else
			{
				AssertOrganizationBO_INTHEMSYD(message, addressData, addressType, hasContactInfoFromMatchedContact);
			}
		}

		public static void AssertOrganizationBO_INTHEMSYD(string message, OrganizationAddress addressData, string addressType)
		{
			AssertOrganizationBO_INTHEMSYD(message, addressData, addressType, false);
		}

		public static void AssertOrganizationBO_INTHEMSYD(string message, OrganizationAddress addressData, string addressType, bool hasContactInfoFromMatchedContact)
		{
			var expectedContactName = hasContactInfoFromMatchedContact ? "Starshine Moonbeam" : null;
			var expectedMobile = hasContactInfoFromMatchedContact ? "234098293" : null;

			AssertAddress(message, addressData
				, addressType, "INTHEMSYD", "In The Moment", false
				, "Unit 12, Level 3", "233 Here St", "ThereVille", "OfBliss", "1233", "AU"
				, expectedContactName, "s.m@moment.com.au", "234098234", expectedMobile, "1239813209");
		}

		public static OrgHeader GetOrganizationBO_WUFSHIJNB(BusinessObjectFactory factory)
		{
			var organizationBO = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "WUFSHIJNB");
			if (organizationBO == null)
			{
				organizationBO = factory.New<OrgHeader>();

				organizationBO.OH_FullName = "WUFU SHIPPING LINE";
				organizationBO.OH_RL_NKClosestPort = "ZAJNB";
				organizationBO.OH_Code = "WUFSHIJNB";

				var addressBO = organizationBO.MainAddress;
				addressBO.OA_Address1 = "Level 2, Building G";
				addressBO.OA_Address2 = "34 Dock Lane";
				addressBO.OA_City = "Johannesburg";
				addressBO.OA_State = "";
				addressBO.OA_PostCode = "12345";

				var contactBO = organizationBO.Contacts.AddNew();
				contactBO.OC_ContactName = "Benny Banana";
				contactBO.OC_Email = "benny.banana@wufu.co.za";
				contactBO.OC_Fax = "0011 54 392 2921";
				contactBO.OC_Mobile = "0011 289 392 2900";
				contactBO.OC_Phone = "0011 54 392 2900";
				contactBO.Documents.AddNew().OD_DocumentGroup = "ALL";

				organizationBO.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown; // Unknown

				var govRegNumBO = organizationBO.PrimaryRegistrationNumber.CusCode;
				organizationBO.PrimaryRegistrationNumber.Number = "TAXME";

				organizationBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalNettingCode, "GOFISH");
				organizationBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.UniversalOfficeCode, "AWESOME");
				organizationBO.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "FLOG").OK_RN_NKCodeCountry = "ZA";
			}
			return organizationBO;
		}

		public static OrgHeader GetOrganizationBO_CRAHOLSYD(BusinessObjectFactory factory)
		{
			var organizationBO = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "CRAHOLSYD");
			if (organizationBO == null)
			{
				organizationBO = factory.New<OrgHeader>();

				organizationBO.OH_FullName = "CRACKERJACK HOLDINGS";
				organizationBO.OH_RL_NKClosestPort = "AUSYD";
				organizationBO.OH_Code = "CRAHOLSYD";

				var addressBO = organizationBO.MainAddress;
				addressBO.OA_Address1 = "1804 Fudrucker Way";
				addressBO.OA_City = "BOTANY";
				addressBO.OA_State = "NSW";
				addressBO.OA_PostCode = "2035";

				var contactBO = organizationBO.Contacts.AddNew();
				contactBO.OC_ContactName = "Rob Anybody";
				contactBO.OC_Email = "rob.anybody@cjh.com.au";
				contactBO.OC_Fax = "02 6392 2921";
				contactBO.OC_Mobile = "0421 392 290";
				contactBO.OC_Phone = "+61263922900";
				contactBO.Documents.AddNew().OD_DocumentGroup = "ALL";

				organizationBO.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;

				var govRegNumBO = organizationBO.PrimaryRegistrationNumber.CusCode;
				organizationBO.PrimaryRegistrationNumber.Number = "TAXYOU";
			}
			return organizationBO;
		}

		public static OrgHeader GetOrganizationBO_INTHEMSYD(BusinessObjectFactory factory)
		{
			var organizationBO = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "INTHEMSYD");
			if (organizationBO == null)
			{
				organizationBO = factory.New<OrgHeader>();
				organizationBO.OH_FullName = "In The Moment";
				organizationBO.OH_RL_NKClosestPort = "AUMEL";
				organizationBO.OH_Code = "INTHEMSYD";

				var addressBO = organizationBO.MainAddress;
				addressBO.OA_Address1 = "Unit 12, Level 3";
				addressBO.OA_Address2 = "233 Here St";
				addressBO.OA_Code = "THEMOMENT";
				addressBO.OA_City = "ThereVille";
				addressBO.OA_State = "OfBliss";
				addressBO.OA_PostCode = "1233";
				addressBO.OA_Email = "s.m@moment.com.au";
				addressBO.OA_Fax = "234098234";
				addressBO.OA_Mobile = "234098293";
				addressBO.OA_Phone = "1239813209";

				var contactBO = organizationBO.Contacts.AddNew();
				contactBO.OC_ContactName = "Starshine Moonbeam";
				contactBO.OC_Email = "s.m@moment.com.au";
				contactBO.OC_Fax = "234098234";
				contactBO.OC_Mobile = "234098293";
				contactBO.OC_Phone = "1239813209";
				contactBO.Documents.AddNew().OD_DocumentGroup = "ALL";

				organizationBO.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			}
			return organizationBO;
		}

		public static OrganizationAddress GetLocalClientAddress()
		{
			return GetNewAddressData_CRAHOLSYD("LocalClient");
		}

		public static void AssertLocalClientAddress(OrgAddress addressBO)
		{
			AssertEquals("addressBO.OA_Address1", "1804 Fudrucker Way", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "", addressBO.OA_Address2);
			AssertEquals("addressBO.Header.OH_Code", "CRAHOLSYD", addressBO.Header.OH_Code);
			AssertEquals("addressBO.Header.OH_FullName", "CRACKERJACK HOLDINGS", addressBO.Header.OH_FullName);
			AssertEquals("addressBO.OA_City", "BOTANY", addressBO.OA_City);
			AssertEquals("addressBO.OA_State", "NSW", addressBO.OA_State);
			AssertEquals("addressBO.OA_PostCode", "2035", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.Header.Contacts[0].OC_ContactName", "Rob Anybody", addressBO.Header.Contacts[0].OC_ContactName);
			AssertEquals("addressBO.OA_Phone", GetFormattedPhone("02 6392 2900", "AU"), addressBO.OA_Phone);
			AssertEquals("addressBO.OA_Fax", "02 6392 2921", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Email", "rob.anybody@cjh.com.au", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Mobile", "0421 392 290", addressBO.OA_Mobile);
		}

		public static OrganizationAddress GetNewAddressData_INTHEMSYD(DocAddressType addressType)
		{
			return GetNewAddressData_INTHEMSYD(addressType.ToString());
		}

		public static OrganizationAddress GetNewAddressData_INTHEMSYD(ZString addressType)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressOverride = false,

				OrganizationCode = "INTHEMSYD",
				CompanyName = "In The Moment",
				Address1 = "Unit 12, Level 3",
				Address2 = "233 Here St",
				City = "ThereVille",
				State = "OfBliss",
				Postcode = "1233",
				Country = new Country() { Code = "AU", Name = "Australia" },

				Contact = "Starshine Moonbeam",
				Email = "s.m@moment.com.au",
				Fax = "234098234",
				Mobile = "234098293",
				Phone = "1239813209",

				ScreeningStatus = new CodeDescriptionPair() { Code = ScreeningStatusesList.Codes.Unknown, Description = "Unknown" },

				GovRegNum = "55555",
				GovRegNumType = new RegistrationNumberType() { Code = "GST", Description = "GST Code" },
				UniversalOfficeCode = "454",
				UniversalNettingCode = "545",
			};
			addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "ATF", Description = "Approved Transitional Facility" },
						 CountryOfIssue = new Country() { Code = "NZ", Name = "New Zealand" },
						Value = "1234F",
					},
				}));

			return addressData;
		}

		public static OrganizationAddress GetNewAddressData_CRAHOLSYD(DocAddressType addressType)
		{
			return GetNewAddressData_CRAHOLSYD(addressType.ToString());
		}

		public static OrganizationAddress GetNewAddressData_CRAHOLSYD(ZString addressType)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressOverride = false,

				OrganizationCode = "CRAHOLSYD",
				CompanyName = "CRACKERJACK HOLDINGS",
				Address1 = "1804 Fudrucker Way",
				Address2 = "",
				City = "BOTANY",
				State = "NSW",
				Postcode = "2035",
				Country = new Country() { Code = "AU", Name = "Australia" },

				Contact = "Rob Anybody",
				Email = "rob.anybody@cjh.com.au",
				Fax = "02 6392 2921",
				Mobile = "0421 392 290",
				Phone = "02 6392 2900",

				ScreeningStatus = new CodeDescriptionPair() { Code = ScreeningStatusesList.Codes.Unknown, Description = "Unknown" },

				GovRegNum = "TAXYOU",
				GovRegNumType = new RegistrationNumberType() { Code = "GST", Description = "GST Code" },
				UniversalOfficeCode = "454",
				UniversalNettingCode = "545",
			};
			addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = "ATF", Description = "Approved Transitional Facility" },
						 CountryOfIssue = new Country() { Code = "NZ", Name = "New Zealand" },
						Value = "1234F",
					},
				}));

			return addressData;
		}

		public static OrganizationAddress GetNewAddressData_WUFSHIJNB(DocAddressType addressType)
		{
			return GetNewAddressData_WUFSHIJNB(addressType.ToString());
		}

		public static OrganizationAddress GetNewAddressData_WUFSHIJNB(ZString addressType)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressOverride = false,

				OrganizationCode = "WUFSHIJNB",
				CompanyName = "WUFU SHIPPING LINE",
				Address1 = "Level 2, Building G",
				Address2 = "34 Dock Lane",
				City = "Johannesburg",
				State = "",
				Postcode = "12345",
				Country = new Country() { Code = "ZA", Name = "South Africa" },

				Contact = "Benny Banana",
				Email = "benny.banana@wufu.co.za",
				Fax = "0011 54 392 2921",
				Mobile = "0011 289 392 2900",
				Phone = "0011 54 392 2900",

				GovRegNum = "TAXME",
				GovRegNumType = new RegistrationNumberType() { Code = "SAM", Description = "Uncle Sam" },

				UniversalNettingCode = "GOFISH",
				UniversalOfficeCode = "AWESOME",
			};
			addressData.SetRegistrationNumberCollection(() => new List<RegistrationNumber>(new[]
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType() { Code = OrgCusCode.CodeTypes.CarrierCode, Description = "Carrier Code" },
						 CountryOfIssue = new Country() { Code = "ZA", Name = "South Africa" },
						Value = "FLOG",
					},
				}));

			return addressData;
		}

		public static void AssertJobDocAddressContentMatches_INTHEMSYD(JobDocAddress jobDocAddressBO)
		{
			AssertEquals("jobDocAddressBO.E2_Address1", "Unit 12, Level 3", jobDocAddressBO.E2_Address1);
			AssertEquals("jobDocAddressBO.E2_Address2", "233 Here St", jobDocAddressBO.E2_Address2);
			AssertEquals("jobDocAddressBO.E2_CompanyName", "In The Moment", jobDocAddressBO.E2_CompanyName);
			AssertEquals("jobDocAddressBO.E2_City", "ThereVille", jobDocAddressBO.E2_City);
			AssertEquals("jobDocAddressBO.E2_State", "OfBliss", jobDocAddressBO.E2_State);
			AssertEquals("jobDocAddressBO.E2_Postcode", "1233", jobDocAddressBO.E2_Postcode);
			AssertEquals("jobDocAddressBO.E2_RN_NKCountryCode", "AU", jobDocAddressBO.E2_RN_NKCountryCode);
			AssertEquals("jobDocAddressBO.E2_Contact", "Starshine Moonbeam", jobDocAddressBO.E2_Contact);
			AssertEquals("jobDocAddressBO.E2_Phone", GetFormattedPhone("1239813209", "AU"), jobDocAddressBO.E2_Phone);
			AssertEquals("jobDocAddressBO.E2_Fax", "234098234", jobDocAddressBO.E2_Fax);
			AssertEquals("jobDocAddressBO.E2_Email", "s.m@moment.com.au", jobDocAddressBO.E2_Email);
			AssertEquals("jobDocAddressBO.E2_Mobile", "234098293", jobDocAddressBO.E2_Mobile);
			AssertEquals("jobDocAddressBO.E2_GovRegNum", "55555", jobDocAddressBO.E2_GovRegNum);
			AssertEquals("jobDocAddressBO.E2_GovRegNumType", "GST", jobDocAddressBO.E2_GovRegNumType);
		}

		public static void AssertJobDocAddressContentMatches_CRAHOLSYD(JobDocAddress jobDocAddressBO)
		{
			AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(jobDocAddressBO);
			AssertEquals("jobDocAddressBO.E2_GovRegNum", "TAXYOU", jobDocAddressBO.E2_GovRegNum);
			AssertEquals("jobDocAddressBO.E2_GovRegNumType", "GST", jobDocAddressBO.E2_GovRegNumType);
		}

		public static void AssertJobDocAddressContentMatches_CRAHOLSYDWithoutGovRegNumAndType(JobDocAddress jobDocAddressBO)
		{
			AssertEquals("jobDocAddressBO.E2_Address1", "1804 Fudrucker Way", jobDocAddressBO.E2_Address1);
			AssertEquals("jobDocAddressBO.E2_Address2", "", jobDocAddressBO.E2_Address2);
			AssertEquals("jobDocAddressBO.E2_CompanyName", "CRACKERJACK HOLDINGS", jobDocAddressBO.E2_CompanyName);
			AssertEquals("jobDocAddressBO.E2_City", "BOTANY", jobDocAddressBO.E2_City);
			AssertEquals("jobDocAddressBO.E2_State", "NSW", jobDocAddressBO.E2_State);
			AssertEquals("jobDocAddressBO.E2_Postcode", "2035", jobDocAddressBO.E2_Postcode);
			AssertEquals("jobDocAddressBO.E2_RN_NKCountryCode", "AU", jobDocAddressBO.E2_RN_NKCountryCode);
			AssertEquals("jobDocAddressBO.E2_Contact", "Rob Anybody", jobDocAddressBO.E2_Contact);
			AssertEquals("jobDocAddressBO.E2_Phone", GetFormattedPhone("02 6392 2900", "AU"), jobDocAddressBO.E2_Phone);
			AssertEquals("jobDocAddressBO.E2_Fax", "02 6392 2921", jobDocAddressBO.E2_Fax);
			AssertEquals("jobDocAddressBO.E2_Email", "rob.anybody@cjh.com.au", jobDocAddressBO.E2_Email);
			AssertEquals("jobDocAddressBO.E2_Mobile", "0421 392 290", jobDocAddressBO.E2_Mobile);
		}

		public static void AssertJobDocAddressContentMatches_WUFSHIJNB(JobDocAddress jobDocAddressBO)
		{
			AssertEquals("jobDocAddressBO.E2_Address1", "Level 2, Building G", jobDocAddressBO.E2_Address1);
			AssertEquals("jobDocAddressBO.E2_Address2", "34 Dock Lane", jobDocAddressBO.E2_Address2);
			AssertEquals("jobDocAddressBO.E2_CompanyName", "WUFU SHIPPING LINE", jobDocAddressBO.E2_CompanyName);
			AssertEquals("jobDocAddressBO.E2_City", "Johannesburg", jobDocAddressBO.E2_City);
			AssertEquals("jobDocAddressBO.E2_State", "", jobDocAddressBO.E2_State);
			AssertEquals("jobDocAddressBO.E2_Postcode", "12345", jobDocAddressBO.E2_Postcode);
			AssertEquals("jobDocAddressBO.E2_RN_NKCountryCode", "ZA", jobDocAddressBO.E2_RN_NKCountryCode);
			AssertEquals("jobDocAddressBO.E2_Contact", "Benny Banana", jobDocAddressBO.E2_Contact);
			AssertEquals("jobDocAddressBO.E2_Phone", GetFormattedPhone("0011 54 392 2900", "ZA"), jobDocAddressBO.E2_Phone);
			AssertEquals("jobDocAddressBO.E2_Fax", "0011 54 392 2921", jobDocAddressBO.E2_Fax);
			AssertEquals("jobDocAddressBO.E2_Email", "benny.banana@wufu.co.za", jobDocAddressBO.E2_Email);
			AssertEquals("jobDocAddressBO.E2_Mobile", "0011 289 392 2900", jobDocAddressBO.E2_Mobile);
			AssertEquals("jobDocAddressBO.E2_GovRegNum", "TAXME", jobDocAddressBO.E2_GovRegNum);
			AssertEquals("jobDocAddressBO.E2_GovRegNumType", "SAM", jobDocAddressBO.E2_GovRegNumType);
		}

		public static void AssertAddressContentMatches_INTHEMSYD(OrgAddress addressBO)
		{
			var organizationBO = addressBO.Header;
			AssertEquals("addressBO.OA_Address1", "Unit 12, Level 3", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "233 Here St", addressBO.OA_Address2);
			AssertEquals("addressBO.OA_City", "ThereVille", addressBO.OA_City);
			AssertEquals("addressBO.OA_State", "OfBliss", addressBO.OA_State);
			AssertEquals("addressBO.OA_PostCode", "1233", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.OA_Phone", GetFormattedPhone("1239813209", "AU"), addressBO.OA_Phone);
			AssertEquals("addressBO.OA_Fax", "234098234", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Email", "s.m@moment.com.au", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Mobile", "234098293", addressBO.OA_Mobile);
			AssertOrgHeaderContents_INTHEMSYD(organizationBO);
		}

		public static void AssertAddressContentMatches_CRAHOLSYD(OrgAddress addressBO)
		{
			var organizationBO = addressBO.Header;
			AssertEquals("addressBO.OA_Address1", "1804 Fudrucker Way", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "", addressBO.OA_Address2);
			AssertEquals("addressBO.OA_City", "BOTANY", addressBO.OA_City);
			AssertEquals("addressBO.OA_State", "NSW", addressBO.OA_State);
			AssertEquals("addressBO.OA_PostCode", "2035", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "AU", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.OA_Phone", GetFormattedPhone("02 6392 2900", "AU"), addressBO.OA_Phone);
			AssertEquals("addressBO.OA_Fax", "02 6392 2921", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Email", "rob.anybody@cjh.com.au", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Mobile", "0421 392 290", addressBO.OA_Mobile);
			AssertOrgHeaderContents_CRAHOLSYD(organizationBO);
		}

		public static void AssertAddressContentMatches_WUFSHIJNB(OrgAddress addressBO)
		{
			var organizationBO = addressBO.Header;
			AssertEquals("addressBO.OA_Address1", "Level 2, Building G", addressBO.OA_Address1);
			AssertEquals("addressBO.OA_Address2", "34 Dock Lane", addressBO.OA_Address2);
			AssertEquals("addressBO.OA_City", "Johannesburg", addressBO.OA_City);
			AssertEquals("addressBO.OA_State", "", addressBO.OA_State);
			AssertEquals("addressBO.OA_PostCode", "12345", addressBO.OA_PostCode);
			AssertEquals("addressBO.OA_RL_NKRelatedPortCode", "ZA", addressBO.OA_RL_NKRelatedPortCode);
			AssertEquals("addressBO.OA_Phone", GetFormattedPhone("0011 54 392 2900", "ZA"), addressBO.OA_Phone);
			AssertEquals("addressBO.OA_Fax", "0011 54 392 2921", addressBO.OA_Fax);
			AssertEquals("addressBO.OA_Email", "benny.banana@wufu.co.za", addressBO.OA_Email);
			AssertEquals("addressBO.OA_Mobile", "0011 289 392 2900", addressBO.OA_Mobile);
			AssertOrgHeaderContents_WUFSHIJNB(organizationBO);
		}

		static void AssertOrgHeaderContents_INTHEMSYD(OrgHeader organizationBO)
		{
			AssertEquals("organizationBO.OH_Code", "INTHEMSYD", organizationBO.OH_Code);
			AssertEquals("organizationBO.OH_FullName", "In The Moment", organizationBO.OH_FullName);

			organizationBO.Contacts.Sort(OrgContact.Schema.OC_ContactName);
			var contacts = new List<string>();
			foreach (OrgContact contact in organizationBO.Contacts)
			{
				contacts.Add(string.Format("Name[{0}] Email[{1}] Phone[{2}] Fax[{3}] Mobile[{4}]", contact.OC_ContactName, contact.OC_Email, contact.OC_Phone, contact.OC_Fax, contact.OC_Mobile));
			}
			AssertMultilineASCIIEquals("organizationBO.Contacts", @"
Name[Starshine Moonbeam] Email[s.m@moment.com.au] Phone[1239813209] Fax[234098234] Mobile[234098293]
				".Trim(), string.Join("\r\n", contacts.ToArray()));

			organizationBO.CustomsCodes.Sort(OrgCusCode.Schema.OK_CodeType);
			var registrationNumbers = new List<string>();
			foreach (OrgCusCode customsCode in organizationBO.CustomsCodes)
			{
				registrationNumbers.Add(string.Format("Country[{0}] Type[{1}] Number[{2}] Address[{3}]", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo, customsCode.PremisesAddress == null ? ZString.Empty : customsCode.PremisesAddress.OA_Address2));
			}
			AssertMultilineASCIIEquals("organizationBO.CustomsCodes", @"
Country[NZ] Type[ATF] Number[1234F] Address[233 Here St]
Country[AU] Type[GST] Number[55555] Address[]
Country[AU] Type[UNC] Number[545] Address[]
Country[AU] Type[UOC] Number[454] Address[]
				".Trim(), string.Join("\r\n", registrationNumbers.ToArray()));
		}

		static void AssertOrgHeaderContents_CRAHOLSYD(OrgHeader organizationBO)
		{
			AssertEquals("organizationBO.OH_Code", "CRAHOLSYD", organizationBO.OH_Code);
			AssertEquals("organizationBO.OH_FullName", "CRACKERJACK HOLDINGS", organizationBO.OH_FullName);

			organizationBO.Contacts.Sort(OrgContact.Schema.OC_ContactName);
			var contacts = new List<string>();
			foreach (OrgContact contact in organizationBO.Contacts)
			{
				contacts.Add(string.Format("Name[{0}] Email[{1}] Phone[{2}] Fax[{3}] Mobile[{4}]", contact.OC_ContactName, contact.OC_Email, contact.OC_Phone, contact.OC_Fax, contact.OC_Mobile));
			}
			AssertMultilineASCIIEquals("organizationBO.Contacts", @"
Name[Rob Anybody] Email[rob.anybody@cjh.com.au] Phone[+61263922900] Fax[02 6392 2921] Mobile[0421 392 290]
				".Trim(), string.Join("\r\n", contacts.ToArray()));

			organizationBO.CustomsCodes.Sort(OrgCusCode.Schema.OK_CodeType);
			var registrationNumbers = new List<string>();
			foreach (OrgCusCode customsCode in organizationBO.CustomsCodes)
			{
				registrationNumbers.Add(string.Format("Country[{0}] Type[{1}] Number[{2}] Address[{3}]", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo, customsCode.PremisesAddress == null ? ZString.Empty : customsCode.PremisesAddress.OA_Address2));
			}
			AssertMultilineASCIIEquals("organizationBO.CustomsCodes", @"
Country[NZ] Type[ATF] Number[1234F] Address[]
Country[AU] Type[GST] Number[TAXYOU] Address[]
Country[AU] Type[UNC] Number[545] Address[]
Country[AU] Type[UOC] Number[454] Address[]
				".Trim(), string.Join("\r\n", registrationNumbers.ToArray()));
		}

		static void AssertOrgHeaderContents_WUFSHIJNB(OrgHeader organizationBO)
		{
			AssertEquals("organizationBO.OH_Code", "WUFSHIJNB", organizationBO.OH_Code);
			AssertEquals("organizationBO.OH_FullName", "WUFU SHIPPING LINE", organizationBO.OH_FullName);

			organizationBO.Contacts.Sort(OrgContact.Schema.OC_ContactName);
			var contacts = new List<string>();
			foreach (OrgContact contact in organizationBO.Contacts)
			{
				contacts.Add(string.Format("Name[{0}] Email[{1}] Phone[{2}] Fax[{3}] Mobile[{4}]", contact.OC_ContactName, contact.OC_Email, contact.OC_Phone, contact.OC_Fax, contact.OC_Mobile));
			}
			AssertMultilineASCIIEquals("organizationBO.Contacts", @"
Name[Benny Banana] Email[benny.banana@wufu.co.za] Phone[0011 54 392 2900] Fax[0011 54 392 2921] Mobile[0011 289 392 2900]
				".Trim(), string.Join("\r\n", contacts.ToArray()));

			organizationBO.CustomsCodes.Sort(OrgCusCode.Schema.OK_CodeType);
			var registrationNumbers = new List<string>();
			foreach (OrgCusCode customsCode in organizationBO.CustomsCodes)
			{
				registrationNumbers.Add(string.Format("Country[{0}] Type[{1}] Number[{2}] Address[{3}]", customsCode.OK_RN_NKCodeCountry, customsCode.OK_CodeType, customsCode.OK_CustomsRegNo, customsCode.PremisesAddress == null ? ZString.Empty : customsCode.PremisesAddress.OA_Address2));
			}
			AssertMultilineASCIIEquals("organizationBO.CustomsCodes", @"
Country[ZA] Type[CCC] Number[FLOG] Address[]
Country[ZA] Type[SAM] Number[TAXME] Address[]
Country[ZA] Type[UNC] Number[GOFISH] Address[]
Country[ZA] Type[UOC] Number[AWESOME] Address[]
				".Trim(), string.Join("\r\n", registrationNumbers.ToArray()));
		}

		public static ZGuid SetUseUnmatchedOrganisationForMatchingRegistry(bool isEnabled)
		{
			var unmatchedOrgRegistryItem = OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value;
			unmatchedOrgRegistryItem.IsEnabled = isEnabled;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unmatchedOrgRegistryItem);
			return unmatchedOrgRegistryItem.Organisation;
		}

		public static OrganizationAddress GetAddressData(ZString addressType, ZString baseValue, ZString unloco)
		{
			var addressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = addressType,
				AddressOverride = false,

				OrganizationCode = baseValue.ToUpper() + unloco.SubstringSafe(2),
				CompanyName = baseValue + " Inc.",
				Address1 = "123 " + baseValue + " Street",
				City = baseValue + "Ville",
				Postcode = "1234",

				Country = new Country() { Code = unloco.Left(2) },
				Port = new UNLOCO() { Code = unloco },

				Contact = baseValue + " Smith",
				Email = baseValue.ToLower() + ".smith@" + baseValue + ".com",
			};

			return addressData;
		}
	}
}
