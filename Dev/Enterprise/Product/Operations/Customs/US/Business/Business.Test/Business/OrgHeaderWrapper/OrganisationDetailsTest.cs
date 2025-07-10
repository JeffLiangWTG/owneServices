using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class OrganisationDetailsTest : TestCaseWithFactory
	{
		public void TestIOrganisationDetails()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MR BOB";
			org.OH_RL_NKClosestPort = "MMAKY";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			org.MainAddress.OA_Address2 = "ADDRESS 2";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2200";
			org.MainAddress.OA_Phone = "98774455";
			org.MainAddress.OA_Fax = "98774466";
			org.MainAddress.OA_Email = "who@what.where";
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Myanmar;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123654");
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.BuyerOrgPK = org.PK;
			var wrapper = OrganisationDetails.New(invoice.BuyerOrgPKInfo, OrgMatchedCustomsRegNoType.EIN, true);

			var orgDetail = (IOrganisationDetails)wrapper;
			AssertEquals("MatchedCustomsRegoNumber", "123654", orgDetail.MatchedCustomsRegoNumber);
			AssertEquals("CompanyName", "MR BOB", orgDetail.CompanyName);
			AssertEquals("AddressLine1", "ADDRESS 1", orgDetail.AddressLine1);
			AssertEquals("AddressLine2", "ADDRESS 2", orgDetail.AddressLine2);
			AssertEquals("City", "SYDNEY", orgDetail.City);
			AssertEquals("State", "NSW", orgDetail.State);
			AssertEquals("PostCode", "2200", orgDetail.PostCode);
			AssertEquals("ContactName", "BOB THE BUILDER", orgDetail.ContactName);
			AssertEquals("Email", "who@what.where", orgDetail.Email);
			AssertEquals("Fax", "98774466", orgDetail.Fax);
			AssertEquals("Phone", "98774455", orgDetail.Phone);
			AssertEquals("UserFriendlyPath", "Invoice > Buyer Org PK", orgDetail.UserFriendlyPath);
			AssertEquals(Core.Constants.CountryCodes.Myanmar, orgDetail.Country);
		}

		public void TestIOrganisationDetailsBU()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MR BOB";
			org.OH_RL_NKClosestPort = "BUAKY";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			org.MainAddress.OA_Address2 = "ADDRESS 2";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2200";
			org.MainAddress.OA_Phone = "98774455";
			org.MainAddress.OA_Fax = "98774466";
			org.MainAddress.OA_Email = "who@what.where";
			org.MainAddress.OA_RN_NKCountryCode = USCCountry.Burma;
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";

			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123654");
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.BuyerOrgPK = org.PK;
			var wrapper = OrganisationDetails.New(invoice.BuyerOrgPKInfo, OrgMatchedCustomsRegNoType.EIN, true);

			var orgDetail = (IOrganisationDetails)wrapper;
			AssertEquals("MatchedCustomsRegoNumber", "123654", orgDetail.MatchedCustomsRegoNumber);
			AssertEquals("CompanyName", "MR BOB", orgDetail.CompanyName);
			AssertEquals("AddressLine1", "ADDRESS 1", orgDetail.AddressLine1);
			AssertEquals("AddressLine2", "ADDRESS 2", orgDetail.AddressLine2);
			AssertEquals("City", "SYDNEY", orgDetail.City);
			AssertEquals("State", "NSW", orgDetail.State);
			AssertEquals("PostCode", "2200", orgDetail.PostCode);
			AssertEquals("ContactName", "BOB THE BUILDER", orgDetail.ContactName);
			AssertEquals("Email", "who@what.where", orgDetail.Email);
			AssertEquals("Fax", "98774466", orgDetail.Fax);
			AssertEquals("Phone", "98774455", orgDetail.Phone);
			AssertEquals("UserFriendlyPath", "Invoice > Buyer Org PK", orgDetail.UserFriendlyPath);
			AssertEquals(USCCountry.Burma, orgDetail.Country);
		}

		public void TestIOrganisationDetailsForAddress()
		{
			var refCountryStates1 = Factory.NewWithValidTestData<RefCountryStates>();
			refCountryStates1.RW_Code = "COL";
			refCountryStates1.RW_RN_NKCountryCode = "CA";
			refCountryStates1.RW_Description = "XYZIMA";
			Factory.Save();

			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MR BOB";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "ADDRESS 1";
			org.MainAddress.OA_Address2 = "ADDRESS 2";
			org.MainAddress.OA_City = "SYDNEY";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2200";
			org.MainAddress.OA_Phone = "98774455";
			org.MainAddress.OA_Fax = "98774466";
			org.MainAddress.OA_Email = "who@what.where";

			var address = org.Addresses.AddNew();
			address.OA_Address1 = "NEW ADDRESS 1";
			address.OA_Address2 = "NEW ADDRESS 2";
			address.OA_City = "NEW SYDNEY CITY";
			address.OA_State = "SA";
			address.OA_PostCode = "2061";
			address.OA_Phone = "45638000";
			address.OA_Fax = "4500001";
			address.OA_Email = "test@test.com";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Miscellaneous.ToString();

			var cusCode = address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "MID123654");
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var wrapper = OrganisationDetails.New(invoice.JZ_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID);
			AssertNull(wrapper);

			invoice.JZ_OA_ManufacturerAddress = address.PK;
			wrapper = OrganisationDetails.New(invoice.JZ_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID);

			var orgDetail = (IOrganisationDetails)wrapper;
			AssertEquals("MatchedCustomsRegoNumber", "MID123654", orgDetail.MatchedCustomsRegoNumber);
			AssertEquals("CompanyName", "MR BOB", orgDetail.CompanyName);
			AssertEquals("AddressLine1", "NEW ADDRESS 1", orgDetail.AddressLine1);
			AssertEquals("AddressLine2", "NEW ADDRESS 2", orgDetail.AddressLine2);
			AssertEquals("City", "NEW SYDNEY CITY", orgDetail.City);
			AssertEquals("State", "SA", orgDetail.State);
			AssertEquals("PostCode", "2061", orgDetail.PostCode);
			AssertEquals("Country", "US", orgDetail.Country);
			AssertEquals("ContactName", "BOB THE BUILDER", orgDetail.ContactName);
			AssertEquals("Email", "test@test.com", orgDetail.Email);
			AssertEquals("Fax", "4500001", orgDetail.Fax);
			AssertEquals("Phone", "45638000", orgDetail.Phone);
			AssertEquals("UserFriendlyPath", "MR BOB > " + address.OA_Code, orgDetail.UserFriendlyPath);

			org.MainAddress.OA_RN_NKCountryCode = "CA";
			org.MainAddress.OA_State = "XYZIMA";
			org.MainAddress.OA_RL_NKRelatedPortCode = "CA2NB";
			invoice.BuyerOrgPK = org.PK;
			wrapper = OrganisationDetails.New(invoice.BuyerOrgPKInfo, OrgMatchedCustomsRegNoType.EIN, true);
			orgDetail = wrapper;
			AssertEquals("COL", orgDetail.State);
		}

		public void TestGlobalBusinessIdentifiers()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MR BOB";
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "AAA");
			var address = org.Addresses.AddNew();
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "1234");
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.GlobalLocationNumber, "ABC");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OA_ManufacturerAddress = address.PK;
			var wrapper = OrganisationDetails.New(invoice.JZ_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID);
			var organizationDetail = (ISimplifiedEntryOrganisationDetails)wrapper;
			var entityCodeList = new EntityCodeList();
			var entitiesForGBIPilot = new List<ZString>()
			{
				EntityCodeList.Codes.ManufacturerSupplier, EntityCodeList.Codes.SellingParty, EntityCodeList.Codes.Shipper,
				EntityCodeList.Codes.Exporter, EntityCodeList.Codes.Distributor, EntityCodeList.Codes.Packager
			};

			foreach (var entityCode in entityCodeList.GetAllCodes())
			{
				organizationDetail.EntityCode = entityCode;
				var globalBusinessIdentifiers = organizationDetail.GlobalBusinessIdentifiers.ToList();
				if (entitiesForGBIPilot.Contains(entityCode))
				{
					AssertEquals($"GBI count for entity {entityCode} should be 3, LEI/GLN/DUNS", 3, globalBusinessIdentifiers.Count);
					AssertEquals("GlobalBusinessIdentifiers[0].IdentifierType", "LEI", globalBusinessIdentifiers[0].IdentifierType);
					AssertEquals("GlobalBusinessIdentifiers[0].Identifier", "1234", globalBusinessIdentifiers[0].Identifier);
					AssertEquals("GlobalBusinessIdentifiers[1].IdentifierType", "GLN", globalBusinessIdentifiers[1].IdentifierType);
					AssertEquals("GlobalBusinessIdentifiers[1].Identifier", "ABC", globalBusinessIdentifiers[1].Identifier);
					AssertEquals("GlobalBusinessIdentifiers[2].IdentifierType", "DUNS", globalBusinessIdentifiers[2].IdentifierType);
					AssertEquals("GlobalBusinessIdentifiers[2].Identifier", "AAA", globalBusinessIdentifiers[2].Identifier);
				}
				else
				{
					AssertEquals($"GBI count for entity {entityCode} should be 0", 0, globalBusinessIdentifiers.Count);
				}
			}
		}
	}
}
