using System;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Internal.Testing
{
	sealed class ZDocAddressOrganisationControlTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestContactLabelText()
		{
			var emailLabelFieldInfo = typeof(ZOrganisationControl).GetField("EmailLabel", BindingFlags.NonPublic | BindingFlags.Instance);
			var emailLabel = (ZLabel)emailLabelFieldInfo.GetValue(AddressControl);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var cnt1 = org.Contacts.AddNew();
			var cnt2 = org.Contacts.AddNew();
			cnt1.OC_Email = "111@fotoforge.net";
			cnt1.OC_ContactName = "111";
			cnt2.OC_Email = "222@fotoforge.net";
			cnt2.OC_ContactName = "222";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OverrideRequirement = new JobDocAddressRequirement();
			docAddress.OrganisationPK = org.PK;

			AddressControl.DocAddress = docAddress;
			AssertEquals(true, docAddress.Requirement.CanOverride);
			AddressControl.Parent = new ZDocAddressControl();
			AssertEquals(true, AddressControl.ParentAddressControl.OverrideAddressCheckbox.Visible);

			AddressControl.OrganisationForBinding = org;
			AddressControl.Details = OrganisationDetails.All;

			docAddress.E2_Contact = "111";
			AssertEquals("EmailLabel.Text", "Em: 111@fotoforge.net", emailLabel.Text);
			docAddress.E2_Contact = "222";
			AssertEquals("EmailLabel.Text", "Em: 222@fotoforge.net", emailLabel.Text);
			AddressControl.Parent.Dispose();
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsDEF()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "DEF";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City RW 2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is DEF", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsPBC()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "PBC";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "2222 City RW", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is PBC", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsPBS()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "PBS";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			var addressInfo = Factory.New<OrgAddressAdditionalInfo>();
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "2222 City (RW)", "Country").ToUpper();
			AssertMultilineASCIIEquals("Address Label iss formatted correctly when formatting rule is PBS", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsUCU()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "UCU";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "CITY", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is UCU", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCAP()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CAP";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "State";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "City", "Additional info", "Address1", "Address2", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CAP", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCPK()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CPK";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "State";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2, CITY", "2222 Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CPK", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCPL()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CPL";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "State";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "CITY 2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CPL", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsPCK()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "PCK";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "State";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "2222 CITY", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is PCK", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsNSP()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "NSP";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "RW", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is NSP", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCCS()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CCS";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "2222 CITY, RW", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CCS", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsSIN()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Singapore";
			country.RN_AddressFormattingRule = "SIN";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "CITY 2222", "REP. OF SINGAPORE").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is SIN", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsACP()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "ACP";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "ADDRESS2", "CITY", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is ACP", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsPCS()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "PCS";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "2222 City", "State", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is PCS", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCPO()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CPO";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CPO", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCSP()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CSP";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City", "State", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CSP", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCSN()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CSN";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City", "State", "2222").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CSN", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCST()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CST";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City, State", "2222", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CST", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestOrgAddressFormattedIsCorrect_WhenFormattingRuleIsCCO()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "RN";
			country.RN_Desc = "Country";
			country.RN_AddressFormattingRule = "CCO";

			var state = Factory.NewWithValidTestData<RefCountryStates>();
			state.RW_Code = "RW";
			state.RW_RN_NKCountryCode = "RN";
			state.RW_Description = "State";

			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var orgAddress = docAddress.Address;
			orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "Additional info";
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "RW";
			orgAddress.OA_PostCode = "2222";
			orgAddress.OA_RN_NKCountryCode = "RN";

			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = docAddress.Organisation;

			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Additional info", "Address1", "Address2", "City", "Country").ToUpper();
			AssertMultilineASCIIEquals("OrgAddressFormatted is formatted correctly when formatting rule is CCO", expectedOrgAddressFormatted, AddressControl.OrgAddressFormatted);
		}

		public void TestAddressLabelText()
		{
			var addressLabelFieldInfo = typeof(ZOrganisationControl).GetField("AddressLabel", BindingFlags.NonPublic | BindingFlags.Instance);
			var addressLabel = (ZLabel)addressLabelFieldInfo.GetValue(AddressControl);
			AssertNotNull("The label named AddressLabel has not be found.", addressLabel);

			addressLabel.Text = "";
			AssertEquals("No Text Expected to be in AddressLabel", string.Empty, addressLabel.Text);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = organisation.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Address1 = "Address1";
			address.OA_Address2 = "Address2";
			address.OA_CompanyNameOverride = "CompanyName";
			address.OA_City = "City";
			address.OA_State = "State";
			address.OA_PostCode = "2222";
			address.OA_Fax = "Fax";
			address.OA_Phone = "Phone";
			address.OA_Email = "Email@edi.com.au";
			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = organisation;
			AddressControl.Details = OrganisationDetails.All;
			AddressControl.UpdateAddressLabelsIfRequired();
			var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Address1", "Address2", "City State 2222", "Australia").ToUpper();
			AssertMultilineASCIIEquals("AddressLabel.Text", expectedOrgAddressFormatted, addressLabel.Text);
		}

		public void TestAddressPhonesAreFormatted()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var address = organisation.MainAddress;
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = organisation.PK;
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Phone = "0425000001";
			address.OA_Mobile = "+33425000002";
			address.OA_Fax = "+61250000003";
			AddressControl.DocAddress = docAddress;
			AddressControl.OrganisationForBinding = organisation;
			AddressControl.Details = OrganisationDetails.All;

			AssertPhoneIsFormatted("PhoneLabel", "Ph: +61 425 000 001");
			AssertPhoneIsFormatted("MobileLabel", "Mob: +33 4 25 00 00 02");
			AssertPhoneIsFormatted("FaxLabel", "Fax: +61 2 5000 0003");
		}

		public void TestValidationButtonVisible()
		{
			using (var form = new ZForm())
			using (var addressControl = new ZDocAddressControl())
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				form.Controls.Add(addressControl);
				form.Show();
				var address = Factory.New<JobDocAddress>();
				address.E2_AddressOverride = true;
				address.E2_RN_NKCountryCode = "";
				addressControl.SetDataBinding(address, "");
				Assert(!addressControl.ValidateButton.Visible);
				address.E2_RN_NKCountryCode = "US";
				Assert(addressControl.ValidateButton.Visible);
			}
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOrgUNLOCOCoreReturnsRelatedPortCodeIfAnAddressHasBeenAssociated()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			var docAddress = Factory.New<JobDocAddress>();
			using (var addressControl = new ZDocAddressOrganisationControlForTest())
			{
				addressControl.DocAddress = docAddress;
				AssertNull("Precondition - the DocaDdress.Address should be null", docAddress.Address);
				AssertEquals("Precondition - addressOrgControl.OrgUNLOCOCore should be empty", true, addressControl.OrgUNLOCOCore.IsEmpty);

				docAddress.E2_OA_Address = address.PK;
				AssertEquals("DocAddress.Address.RelatedPortCode", "AUSYD", docAddress.Address.OA_RL_NKRelatedPortCode);
				AssertEquals("addressOrgControl.OrgUNLOCOCore", "AUSYD", addressControl.OrgUNLOCOCore);
			}
		}

		public void TestIZAddressParentParseCode()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var docAddress = Factory.New<JobDocAddress>();
			docAddress.OrganisationPK = org.PK;
			AddressControl.DocAddress = docAddress;

			var address1 = org.AddressesNoAutoCreate.AddNew();
			address1.OA_Code = "AAA";

			var address2 = org.AddressesNoAutoCreate.AddNew();
			address2.OA_Code = "BBB";

			var iZAddressParent = (IZAddressParent)AddressControl;

			AssertEquals(address1.PK, iZAddressParent.ParseCode("AAA"));
			AssertEquals(address2.PK, iZAddressParent.ParseCode("BBB"));
			Assert(!iZAddressParent.ParseCode("CCC").IsValid);
		}

		public void TestAdditionalAddressInfoControl_RegistryDisabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var addressControl = new ZDocAddressControl())
			{
				form.Controls.Add(addressControl);
				form.Show();
				AssertEquals(0, addressControl.Controls.Find("AdditionalAddressInfoTab", true).Length);
			}
		}

		public void TestAdditionalAddressInfoControl_RegistryEnabled()
		{
			using (OrganisationRegistry.Instance.AllowOverrideAddressAdditionalInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var addressControl = new ZDocAddressControl())
			{
				form.Controls.Add(addressControl);
				form.Show();

				var additionalInfoTab = (ZTabPage)addressControl.Controls.Find("AdditionalAddressInfoTab", true)[0];
				AssertNotNull("Should render ZTabPage", additionalInfoTab);
				AssertEquals(true, additionalInfoTab.TabVisible);

				var additionalInfoEdit = (ZDropEdit)additionalInfoTab.Controls.Find("AdditionalAddressInfoDropEdit", true)[0];
				AssertNotNull("Should render ZDropEdit", additionalInfoEdit);
				AssertEquals("AdditionalAddressInfoDropEdit show drop down list should be 'OnlyShowCode'", ZDropEdit.ShowInDropDownList.OnlyShowCode, additionalInfoEdit.ShowInDropDown);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();
			addressControl?.Dispose();
		}

		ZDocAddressOrganisationControl addressControl;
		ZDocAddressOrganisationControl AddressControl => addressControl ?? (addressControl = new ZDocAddressOrganisationControl());

		void AssertPhoneIsFormatted(string phoneLabelName, string expectedPhoneLable)
		{
			var phoneLabelFieldInfo = typeof(ZOrganisationControl).GetField(phoneLabelName, BindingFlags.NonPublic | BindingFlags.Instance);
			var phoneLabel = (ZLabel)phoneLabelFieldInfo.GetValue(AddressControl);
			AssertEquals(expectedPhoneLable, phoneLabel.Text);
		}

		sealed class ZDocAddressOrganisationControlForTest : ZDocAddressOrganisationControl
		{
			public ZDocAddressOrganisationControlForTest()
				: base()
			{ }

			internal new ZString OrgUNLOCOCore => base.OrgUNLOCOCore;
		}
	}
}
