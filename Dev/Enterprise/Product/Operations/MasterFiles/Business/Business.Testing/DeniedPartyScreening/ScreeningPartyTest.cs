using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Integration.Customs;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ScreeningPartyTest : TestCaseWithFactory
	{
		public void TestConvertFromComplianceParty()
		{
			AssertExceptionThrown<ArgumentException>(
				() => ScreeningParty.ConvertFromComplianceParty(null, null, null, null));
			AssertExceptionThrown<ArgumentException>(
				() => ScreeningParty.ConvertFromComplianceParty(Parent, null, null, null));

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var result = ScreeningParty.ConvertFromComplianceParty(Parent, "Consignor", org, null);
			AssertComplianceParties(result, "Consignor", org);
			var transport = Factory.New<ITransport>() as BusinessObject;
			result = ScreeningParty.ConvertFromComplianceParty(Parent, "Transport", transport, null);
			AssertComplianceParties(result, "Transport", transport);
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			result = ScreeningParty.ConvertFromComplianceParty(Parent, "Address", address, null);
			AssertComplianceParties(result, "Address", address);
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			result = ScreeningParty.ConvertFromComplianceParty(Parent, "Vessel", vessel, null);
			AssertComplianceParties(result, "Vessel", vessel);
			result = ScreeningParty.ConvertFromComplianceParty(Parent, "Consignee", null, null);
			AssertComplianceParties(result, "Consignee", null);
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			var naturalPerson = new NaturalPerson(
				name: shipperAddress.CompanyName,
				address1: shipperAddress.OA_Address1,
				address2: shipperAddress.OA_Address2,
				city: shipperAddress.OA_City,
				state: shipperAddress.OA_State,
				postCode: shipperAddress.OA_PostCode,
				country: shipperAddress.OA_RN_NKCountryCode,
				additionalAddressLine: shipperAddress.OA_AdditionalAddressInformation,
				businessEntityKey: Parent.PK.ToGuid(),
				shouldBeConsideredAsOrganization: true);
			result = ScreeningParty.ConvertFromComplianceParty(Parent, "Shipper", null, naturalPerson);
			AssertComplianceParties(result, "Shipper", null);
			AssertNotNull($"{nameof(NaturalPerson)} should be instantiated", result.NaturalPerson);
			AssertNotEquals($"{nameof(ScreeningParty.ConvertFromComplianceParty)} should create a new instance of {nameof(NaturalPerson)}", naturalPerson, result.NaturalPerson);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.BusinessEntityKey)}", naturalPerson.BusinessEntityKey, result.NaturalPerson.BusinessEntityKey);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.Name)}", naturalPerson.Name, result.NaturalPerson.Name);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.Address1)}", naturalPerson.Address1, result.NaturalPerson.Address1);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.Address2)}", naturalPerson.Address2, result.NaturalPerson.Address2);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.City)}", naturalPerson.City, result.NaturalPerson.City);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.State)}", naturalPerson.State, result.NaturalPerson.State);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.PostCode)}", naturalPerson.PostCode, result.NaturalPerson.PostCode);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.Country)}", naturalPerson.Country, result.NaturalPerson.Country);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.AdditionalAddressLine)}", naturalPerson.AdditionalAddressLine, result.NaturalPerson.AdditionalAddressLine);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.IsConsideredAsOrganization)}", naturalPerson.IsConsideredAsOrganization, result.NaturalPerson.IsConsideredAsOrganization);
			AssertEquals($"{nameof(NaturalPerson)}.{nameof(NaturalPerson.HashID)}", naturalPerson.HashID, result.NaturalPerson.HashID);
		}

		public void TestScreeningPartyIScreeningPartyForVessel()
		{
			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(ITransportParentCommon);
			transport.JW_Vessel = "123456";
			var screeningParty = new ScreeningParty(Parent, "Not Linked Vessel", transport as IScreeningPartyForVessel);

			CombineAssertions(() =>
			{
				AssertEquals(screeningParty.NotLinkedVessel, transport);
				AssertEquals(screeningParty.Key, (transport as BusinessObject).PK);
				AssertEquals(screeningParty.CurrentScreeningStatus, ((IScreeningPartyForVessel)transport).CurrentScreeningStatus);
				AssertEquals(screeningParty.Code, ((IScreeningPartyForVessel)transport).Code);
			});
		}

		public void TestScreeningPartyHeader()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "hello";
			header.OH_Code = "ORG";
			var screeningParty = new ScreeningParty(Parent, "header", header);

			AssertEquals(screeningParty.Header, header);
			AssertDeniedPartyOrg(screeningParty, Parent, "header", header.PK, "hello");

			AssertEquals("Not Screened : ORG, hello (header)", screeningParty.Summary);
		}

		public void TestScreeningPartyHeaderInactive()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "hello";
			header.OH_Code = "ORG";
			header.OH_IsActive = false;
			var screeningParty = new ScreeningParty(Parent, "header", header);

			AssertEquals(screeningParty.Header, header);
			AssertDeniedPartyOrg(screeningParty, Parent, "header", header.PK, "hello");
			AssertEquals(false, screeningParty.IsActive);

			AssertEquals("Not Screened : ORG, hello (header)", screeningParty.Summary);
		}

		public void TestScreeningPartyJobDocAddress()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "hello";
			header.OH_Code = "JOBDOC";
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_CompanyName = "world";
			address.E2_OA_Address = header.MainAddress.PK;
			address.E2_AddressOverride = false;
			var screeningParty = new ScreeningParty(Parent, "address", address);

			AssertEquals(screeningParty.DocAddress, address);
			AssertDeniedPartyOrg(screeningParty, Parent, "address", header.PK, "hello");

			AssertEquals("Not Screened : JOBDOC, hello (address)", screeningParty.Summary);

			address.E2_AddressOverride = true;
			address.E2_CompanyName = "world";
			screeningParty = new ScreeningParty(Parent, "address", address);

			AssertEquals(screeningParty.DocAddress, address);
			AssertDeniedPartyOrg(screeningParty, Parent, "address", address.PK, "world");

			AssertEquals("Not Screened : MISC, world (address)", screeningParty.Summary);
		}

		public void TestScreeningPartyJobDocAddressInactive()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_FullName = "hello";
			header.OH_Code = "JOBDOC";
			header.OH_IsActive = false;

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_CompanyName = "world";
			address.E2_OA_Address = header.MainAddress.PK;

			address.E2_AddressOverride = false;
			var screeningParty = new ScreeningParty(Parent, "address", address);

			AssertEquals(screeningParty.DocAddress, address);
			AssertDeniedPartyOrg(screeningParty, Parent, "address", header.PK, "hello");
			AssertEquals(false, screeningParty.IsActive);

			AssertEquals("Not Screened : JOBDOC, hello (address)", screeningParty.Summary);

			address.E2_AddressOverride = true;
			address.E2_CompanyName = "world";
			screeningParty = new ScreeningParty(Parent, "address", address);

			AssertEquals(screeningParty.DocAddress, address);
			AssertDeniedPartyOrg(screeningParty, Parent, "address", address.PK, "world");
			AssertEquals(true, screeningParty.IsActive);

			AssertEquals("Not Screened : MISC, world (address)", screeningParty.Summary);
		}

		public void TestScreeningPartyRefCountryNull()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			var screeningParty = new ScreeningParty(vessel, "country", null as RefCountry);
			CombineAssertions(() =>
			{
				AssertEquals(screeningParty.Country, null);
				AssertEquals(screeningParty.Parent, vessel);
				AssertEquals(screeningParty.Parents[0], vessel);
				AssertEquals(screeningParty.Description, "country");
				AssertEquals(" :  (country)", screeningParty.Summary);
			});
		}

		public void TestScreeningPartyRefCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "IR";
			country.RN_Desc = "Iran";
			var screeningParty = new ScreeningParty(country, "country", country);

			CombineAssertions(() =>
			{
				AssertEquals(screeningParty.Country, country);
				AssertEquals(screeningParty.Description, "country");
				AssertEquals(screeningParty.Key, country.PK);
				AssertEquals(screeningParty.Code, "IR");
				AssertEquals("Unknown : IR (country)", screeningParty.Summary);
				AssertEquals(true, screeningParty.IsActive);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, screeningParty.CurrentScreeningStatus);

				AssertEquals(country, ((IComplianceLocation)screeningParty).Country);
				AssertEquals(country.RN_Code, ((IComplianceLocation)screeningParty).Code);
				AssertEquals(country.RN_IsSanctioned, ((IComplianceLocation)screeningParty).IsSanctioned);
				AssertEquals(country.RN_Desc, ((IComplianceLocation)screeningParty).LocationDescription);
				AssertEquals("IR: country", ((IComplianceLocation)screeningParty).ParentsDescription);
				AssertEquals(country.PK, ((IComplianceLocation)screeningParty).Key);
			});
		}

		public void TestScreeningPartyRefCountryInactive()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "IR";
			country.RN_Desc = "Iran";
			country.RN_IsActive = false;

			var screeningParty = new ScreeningParty(country, "country", country);

			AssertEquals(false, screeningParty.IsActive);
		}

		public void TestScreeningEntityRefCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "IR";
			country.RN_Desc = "Iran";

			var party = new ScreeningParty(country, "country", country);

			AssertEquals(country, party.ScreeningEntity);
		}

		public void TestScreeningPartyVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "iddqd";

			var screeningParty = new ScreeningParty(Parent, "vessel", vessel);

			AssertEquals(screeningParty.Vessel, vessel);
			AssertDeniedPartyOrg(screeningParty, Parent, "vessel", vessel.PK, "iddqd");

			AssertEquals("Not Screened : iddqd (vessel)", screeningParty.Summary);
		}

		public void TestScreeningPartyVesselInactive()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "iddqd";
			vessel.RV_IsActive = false;

			ScreeningParty screeningParty = new ScreeningParty(Parent, "vessel", vessel);

			AssertEquals(screeningParty.Vessel, vessel);
			AssertDeniedPartyOrg(screeningParty, Parent, "vessel", vessel.PK, "iddqd");
			AssertEquals(false, screeningParty.IsActive);

			AssertEquals("Not Screened : iddqd (vessel)", screeningParty.Summary);
		}

		public void TestScreeningPartyNaturalPerson()
		{
			var parent = Factory.NewWithValidTestData<RefVessel>();
			Assert("Precondition", parent is IScreeningStatusProvider);

			var description = "Person";
			var name = "Good King Moggle Mog XII";
			var address1 = "Address 1";
			var address2 = "Address 2";
			var city = "City";
			var postCode = "Postcode";
			var state = "State";
			var country = "Country";
			var additionalAddressLine = "Additional Address Line";

			var screeningParty = new ScreeningParty(parent, description, name, address1, address2, city, postCode, state, country, additionalAddressLine);
			var naturalPerson = screeningParty.NaturalPerson;

			CombineAssertions(() =>
			{
				AssertNotNull(naturalPerson);
				AssertEquals("Person", screeningParty.Description);
				AssertEquals("Good King Moggle Mog XII", naturalPerson.Name);
				AssertEquals("Address 1", naturalPerson.Address1);
				AssertEquals("Address 2", naturalPerson.Address2);
				AssertEquals("City", naturalPerson.City);
				AssertEquals("State", naturalPerson.State);
				AssertEquals("Postcode", naturalPerson.PostCode);
				AssertEquals("Country", naturalPerson.Country);
				AssertEquals("Additional Address Line", naturalPerson.AdditionalAddressLine);
				AssertEquals((name + address1 + address2 + city + state + postCode + country + additionalAddressLine).GetHashCode(), naturalPerson.HashID);
			});
		}

		public void TestNullArgument()
		{
			OrgHeader header = null;
			var screeningParty = new ScreeningParty(Parent, "header", header);

			AssertEquals(screeningParty.Header, header);
			AssertDeniedPartyOrg(screeningParty, Parent, "header", ZGuid.Empty, "");

			AssertEquals(" :  (header)", screeningParty.Summary);
		}

		public void TestDocAddressWithNullAddressHeader()
		{
			var orgAddress = Factory.New<OrgAddress>();

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_AddressOverride = false;
			address.E2_AddressType = ZString.Empty;
			address.E2_GovRegNum = ZString.Empty;
			address.E2_ParentID = ZGuid.Empty;
			address.E2_ParentTableCode = ZString.Empty;
			address.E2_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			address.E2_OA_Address = orgAddress.PK;

			AssertEquals("Header", null, address.Address.Header);

			ScreeningParty screeningParty = new ScreeningParty(Parent, "address", address);
			AssertEquals("ScreeningStatus", ScreeningStatusesList.Codes.Unknown, screeningParty.CurrentScreeningStatus);

			AssertEquals("Unknown :  (address)", screeningParty.Summary);
		}

		public void TestCalculateScreeningStatusIsValid()
		{
			CombineAssertions(() =>
			{
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Unknown, false);
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.NotScreened, false);
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Canceled, true);
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Clear, true);
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.Matched, true);
				AssertCurrentScreeningStatusValid(ScreeningStatusesList.Codes.RequiresReview, false);
			});
		}

		public void TestScreeningEntity()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var address = Factory.NewWithValidTestData<JobDocAddress>();
			address.E2_CompanyName = "world";
			address.E2_OA_Address = header.MainAddress.PK;
			address.E2_AddressOverride = false;

			var vessel = Factory.NewWithValidTestData<RefVessel>();

			var transport = Factory.New<ITransport>();
			transport.ParentType = typeof(ITransportParentCommon);
			transport.JW_Vessel = "CODE";

			var party = new ScreeningParty(Parent, "Org", header);
			AssertEquals(header, party.ScreeningEntity);

			party = new ScreeningParty(Parent, "Not Override DocAddress", address);
			AssertEquals(header, party.ScreeningEntity);

			address.E2_AddressOverride = true;
			party = new ScreeningParty(Parent, "Override DocAddress", address);
			AssertEquals(address, party.ScreeningEntity);

			party = new ScreeningParty(Parent, "Vessel", vessel);
			AssertEquals(vessel, party.ScreeningEntity);

			party = new ScreeningParty(Parent, "Not Linked Vessel", transport as IScreeningPartyForVessel);
			AssertEquals(transport, party.ScreeningEntity);
		}

		public void TestScreeningPartyDescriptionIfParentIsDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "CN";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var declaration = Factory.New<IBaseJobDeclaration>() as BusinessObject;
			(declaration as IBaseJobDeclaration).JE_GB = branch.PK;
			Factory.Save();

			var screeningParty = new ScreeningParty(declaration, "Parent is Declaration", org);
			AssertEquals($"{(declaration as IBaseJobDeclaration).JE_DeclarationReference}: CN - Parent is Declaration", screeningParty.ParentsDescription);
		}

		public void TestGetPartyDescriptionWithCountryCodeIfParentIsDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = "CN";

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			AssertEquals("CN - Consignee", ScreeningParty.GetPartyDescriptionWithCountryCodeIfParentIsDeclaration((BusinessObject)declaration, "Consignee"));
		}

		public void TestGetPartyDescriptionWithoutCountryCodeIfParentIsDeclaration()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RN_NKCountryCode = " ";

			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = branch.PK;
			AssertEquals("Consignee", ScreeningParty.GetPartyDescriptionWithCountryCodeIfParentIsDeclaration((BusinessObject)declaration, "Consignee"));
		}

		public void TestGetPartyDescriptionIfParentIsNotDeclaration()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("Consignee", ScreeningParty.GetPartyDescriptionWithCountryCodeIfParentIsDeclaration(org, "Consignee"));
		}

		#region Implementation

		void AssertComplianceParties(ScreeningParty party, string description, BusinessObject originalParty)
		{
			var complianceParty = party as IScreeningParty;
			AssertNotNull(complianceParty);
			AssertEquals(Parent, complianceParty.Parent);
			AssertEquals(description, complianceParty.Description);
			AssertEquals(originalParty, complianceParty.Party);
		}

		void AssertCurrentScreeningStatusValid(string status, bool expectedIsCurrentScreeningStatusValid)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_ScreeningStatus = status;

			var party = new ScreeningParty(Parent, "OrgHeader", orgHeader);
			party.CalculateScreeningStatusIsValid();

			AssertEquals(expectedIsCurrentScreeningStatusValid, party.IsCurrentScreeningStatusValid);
		}

		void AssertDeniedPartyOrg(ScreeningParty screeningParty, BusinessObject parent, string description, ZGuid key, string code)
		{
			CombineAssertions(() =>
			{
				AssertEquals(screeningParty.Parent, parent);
				AssertEquals(screeningParty.Parents[0], parent);
				AssertEquals(screeningParty.Description, description);
				AssertEquals(screeningParty.ParentsDescription, "PARORGSYD: " + description);
				AssertEquals(screeningParty.Key, key);
				AssertEquals(screeningParty.Code, code);
			});
		}

		OrgHeader Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = Factory.New<OrgHeader>();
					fParent.OH_FullName = "PARENT ORG";
					fParent.MainAddress.OA_Address1 = "Parent address";
					fParent.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return fParent;
			}
		}
		OrgHeader fParent;

		#endregion

	}
}
