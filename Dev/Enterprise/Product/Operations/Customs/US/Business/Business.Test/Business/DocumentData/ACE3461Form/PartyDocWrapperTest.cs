using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PartyDocWrapper))]
	sealed class PartyDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIOrganisationDetails()
		{
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
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY");
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.BuyerOrgPK = org.PK;
			var wrapper = OrganisationDetails.New(invoice.BuyerOrgPKInfo, OrgMatchedCustomsRegNoType.EIN, true);
			var partyDocWrapper = new PartyDocWrapper(wrapper, false);
			AssertEquals("MatchedCustomsRegoNumber", "12-3456789XY", partyDocWrapper.PartyIdentifier);
			AssertEquals("EIN", "X", partyDocWrapper.IsIRS);
			AssertEquals("CompanyName", "MR BOB", partyDocWrapper.Name);
			AssertEquals("AddressLine1", "ADDRESS 1", partyDocWrapper.AddressLine1);
			AssertEquals("AddressLine2", "ADDRESS 2", partyDocWrapper.AddressLine2);

			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			cusCode.OK_CustomsRegNo = "123-12-1234";
			AssertEquals("PartyIdentifier", "123-12-1234", partyDocWrapper.PartyIdentifier);
			AssertEquals("PartyIdentifierForDocument", string.Empty, partyDocWrapper.PartyIdentifierForDocument);

			partyDocWrapper = new PartyDocWrapper(wrapper, true);
			AssertEquals("PartyIdentifierForDocument", "123-12-1234", partyDocWrapper.PartyIdentifierForDocument);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
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
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			var cusCode = org.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "123654");
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoice.BuyerOrgPK = org.PK;
			var wrapper = OrganisationDetails.New(invoice.BuyerOrgPKInfo, OrgMatchedCustomsRegNoType.EIN, true);
			return new PartyDocWrapper(wrapper, false);
		}
	}
}
