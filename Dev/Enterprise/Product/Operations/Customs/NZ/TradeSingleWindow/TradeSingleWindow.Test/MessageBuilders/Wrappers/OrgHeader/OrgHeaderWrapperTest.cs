using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class OrgHeaderWrapperTest : TestCaseWithFactory
	{
		public void TestOrgHeaderWrapper()
		{
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Address1 = "100 Main St.";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.MainAddress.OA_Phone = "+61 2 80012201";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			var supplierCode = organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.SupplierCode, "089234T", Core.Constants.CountryCodes.NewZealand);
			IOrganisation orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertNotNull("OrgHeaderWrapper", orgWrapper);
			AssertEquals("CustomsSupplierCode", "00089234T", orgWrapper.CustomsSupplierCode);
			AssertEquals("Name", "Test OrgHeader", orgWrapper.Name);
			AssertEquals("ItemValue", "Sydney", orgWrapper.City);
			AssertEquals("CountryCode", "AU", orgWrapper.CountryCode);
			AssertEquals("CountryRegion", "NSW", orgWrapper.CountryRegion);
			AssertEquals("PostCode", "2000", orgWrapper.PostCode);
			AssertEquals("Street", "100 Main St.", orgWrapper.Address);
			var commsCount = 0;
			foreach (var comm in orgWrapper.Communications)
			{
				commsCount++;
				if (comm.ContactType == CommunicationTypeList.Codes.TE)
				{
					AssertEquals("Comms - phone", "61280012201", comm.ContactDetail);
				}
				else if (comm.ContactType == CommunicationTypeList.Codes.EM)
				{
					AssertEquals("Comms - email", "admin@Organisation.com", comm.ContactDetail);
				}
				else if (comm.ContactType == CommunicationTypeList.Codes.FX)
				{
					AssertEquals("Comms - fax", "61299999999", comm.ContactDetail);
				}
			}

			AssertEquals("Communications count", 3, commsCount);
			supplierCode.OK_CustomsRegNo = "433089234T";
			AssertEquals("CustomsSupplierCode", "433089234", orgWrapper.CustomsSupplierCode);
		}

		public void TestOrgContact()
		{
			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Will Anderson";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "John Smith";
			var personalAtt = contact2.Attributes.AddNew();
			personalAtt.PC_Type = "ARL";
			var personalAlloc = contact2.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			var contact3 = organisation.Contacts.AddNew();
			contact3.OC_ContactName = "James Smith";
			var personalAlloc2 = contact3.Allocations.AddNew();
			personalAlloc2.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;
			IOrganisation orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("ContactPerson should return the contact that has been allocated to NZ Customs", "John Smith", orgWrapper.ContactPerson);
		}

		public void TestStreetFromJobDocAddress()
		{
			var orgAddress = organisation.Addresses.AddNew();
			orgAddress.OA_Address1 = "57 JOHNSON ST.";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			var orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals("57 JOHNSON ST.", ((IOrganisation)orgWrapper).Address);
			AssertEquals("57 JOHNSON ST.", ((IPartyInformation)orgWrapper).Address);
			orgAddress.OA_Address2 = "MARRICKVILLE";
			AssertEquals("Address should concatenate", "57 JOHNSON ST. MARRICKVILLE", ((IOrganisation)orgWrapper).Address);
			AssertEquals("Address should concatenate", "57 JOHNSON ST. MARRICKVILLE", ((IPartyInformation)orgWrapper).Address);
			orgAddress.OA_Address1 = "1234567890123456789012345678901234567890";
			orgAddress.OA_Address2 = "1234567890123456789012345678901234567890";
			AssertEquals("Address should be truncated", "1234567890123456789012345678901234567890 12345678901234567890123456789", ((IOrganisation)orgWrapper).Address);
			AssertEquals("Address should be truncated", "1234567890123456789012345678901234567890 12345678901234567890123456789", ((IPartyInformation)orgWrapper).Address);
		}

		public void TestStreetFromOrgHeader()
		{
			organisation.MainAddress.OA_Address1 = "100 MAIN ST.";
			var orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("100 MAIN ST.", ((IOrganisation)orgWrapper).Address);
			AssertEquals("100 MAIN ST.", ((IPartyInformation)orgWrapper).Address);
			organisation.MainAddress.OA_Address2 = "DULWICH HILL";
			AssertEquals("Address should concatenate", "100 MAIN ST. DULWICH HILL", ((IOrganisation)orgWrapper).Address);
			AssertEquals("Address should concatenate", "100 MAIN ST. DULWICH HILL", ((IPartyInformation)orgWrapper).Address);
			organisation.MainAddress.OA_Address1 = "1234567890123456789012345678901234567890";
			organisation.MainAddress.OA_Address2 = "1234567890123456789012345678901234567890";
			AssertEquals("Address should be truncated", "1234567890123456789012345678901234567890 12345678901234567890123456789", ((IOrganisation)orgWrapper).Address);
			AssertEquals("Address should be truncated", "1234567890123456789012345678901234567890 12345678901234567890123456789", ((IPartyInformation)orgWrapper).Address);
		}

		public void TestPostCodeFromOrgHeader()
		{
			organisation.MainAddress.OA_PostCode = "4001 H9R 5";
			IOrganisation orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("PostCode should have had white space removed if PostCode length is greater than allowed in message", "4001H9R5", orgWrapper.PostCode);
			organisation.MainAddress.OA_PostCode = "45001SWIFT";
			AssertEquals("Long PostCode should have been truncated to the message maximum length", "45001SWIF", orgWrapper.PostCode);
		}

		public void TestPostCodesStripInvalidCharacters()
		{
			organisation.MainAddress.OA_PostCode = "37373-7780";
			IOrganisation orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("PostCode should have had invalid character removed & if PostCode length is greater than allowed in message should be trimmed", "373737780", orgWrapper.PostCode);

			organisation.MainAddress.OA_PostCode = "45001SWIFT";
			AssertEquals("Long PostCode should have been truncated to the message maximum length of 9 characters", "45001SWIF", orgWrapper.PostCode);
		}

		public void TestPostCodeFromJobDocAddress()
		{
			var orgAddress = organisation.Addresses.AddNew();
			orgAddress.OA_PostCode = "220305";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			IOrganisation orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals("220305", orgWrapper.PostCode);

			orgAddress.OA_PostCode = "1234567890";
			AssertEquals("123456789", orgWrapper.PostCode);

			orgAddress.OA_PostCode = "37373-7780";
			AssertEquals("373737780", orgWrapper.PostCode);
		}

		[ExpectNoExceptions]
		public void TestWrapperOrganisationIsNull()
		{
			var orgWrapper = new OrgHeaderWrapper(null);
			var orgInterface = (IOrganisation)orgWrapper;
			AssertEquals(ZString.Empty, orgInterface.Address);
			AssertEquals(ZString.Empty, orgInterface.PostCode);
			AssertEquals(Enumerable.Empty<ICommunication>(), orgInterface.Communications);
			AssertEquals(Enumerable.Empty<ICommunication>(), ((IPartyInformation)orgWrapper).Communications);
		}

		public void TestNewConstructor()
		{
			AssertNull(OrgHeaderWrapper.New(null));
			AssertNotNull(OrgHeaderWrapper.New(Factory.New<OrgHeader>(), null));
		}

		public void TestPartyInformationName()
		{
			organisation.OH_FullName = "COMPANY NAME";
			var orgAddress = organisation.Addresses.AddNew();
			orgAddress.CompanyName = "ADDR COMPANY NAME";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			IPartyInformation orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals("ADDR COMPANY NAME", orgWrapper.Name);
			orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("COMPANY NAME", orgWrapper.Name);
			orgWrapper = new OrgHeaderWrapper(null);
			AssertEquals(ZString.Empty, orgWrapper.Name);
		}

		public void TestPartyInformationCity()
		{
			var orgAddress = organisation.MainAddress;
			orgAddress.OA_City = "ORG CITY";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_City = "ADDR CITY";
			IPartyInformation orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals("ADDR CITY", orgWrapper.City);
			orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("ORG CITY", orgWrapper.City);
			orgWrapper = new OrgHeaderWrapper(null);
			AssertEquals(ZString.Empty, orgWrapper.City);
		}

		public void TestPartyInformationCountryCode()
		{
			var orgAddress = organisation.MainAddress;
			orgAddress.OA_RL_NKRelatedPortCode = "NKAKL";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			IPartyInformation orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals(Core.Constants.CountryCodes.NewZealand, orgWrapper.CountryCode);
			orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("NK", orgWrapper.CountryCode);
			orgWrapper = new OrgHeaderWrapper(null);
			AssertEquals(ZString.Empty, orgWrapper.CountryCode);
		}

		public void TestPartyInformationCountryRegion()
		{
			var orgAddress = organisation.MainAddress;
			orgAddress.OA_State = "ROTARUA";
			var deliveryAddress = Factory.NewWithValidTestData<JobDocAddress>();
			deliveryAddress.E2_OA_Address = orgAddress.PK;
			deliveryAddress.E2_AddressOverride = true;
			deliveryAddress.E2_State = "PHUKATANE";
			IPartyInformation orgWrapper = OrgHeaderWrapper.New(organisation, deliveryAddress);
			AssertEquals("PHUKATANE", orgWrapper.CountryRegion);
			orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("ROTARUA", orgWrapper.CountryRegion);
			orgWrapper = new OrgHeaderWrapper(null);
			AssertEquals(ZString.Empty, orgWrapper.CountryRegion);
		}

		public void TestCustomsClientCode()
		{
			var ccdCode = organisation.CustomsCodes.AddNew();
			ccdCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			ccdCode.OK_CustomsRegNo = "123456789";
			var orgWrapper = OrgHeaderWrapper.New(organisation);
			AssertEquals("123456789", ((IOrganisation)orgWrapper).CustomsClientCode);
			AssertEquals("123456789", ((IPartyInformation)orgWrapper).CustomsClientCode);
		}

		public void TestOrganizationIsEmpty()
		{
			var orgHeaderWrapper = new OrgHeaderWrapper(null);
			AssertEquals("orgHeaderWrapper.IsEmpty", true, orgHeaderWrapper.IsEmpty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
		}

		OrgHeader organisation;
	}
}
