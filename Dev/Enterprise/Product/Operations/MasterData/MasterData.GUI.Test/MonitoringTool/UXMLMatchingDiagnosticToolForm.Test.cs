using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	public class UXMLMatchingDiagnosticToolFormTest : TestCaseWithFactory
	{
		public void TestRegisterAndRemoveParticipant()
		{
			AssertNull("Precondition", DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));
			using (var form = new UXMLMatchingDiagnosticToolForm())
			{
				form.Show();
				AssertEquals(form, DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));

				form.Close();
				AssertNull(DeduplicationUtils.DebuggerHubInstance.FindParticipant(DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName));
			}
		}

		public void TestXMLInputField_ContainsDefaultValuesOnFormLoad()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();

				AssertMultilineASCIIEquals("<OrganizationAddress> Paste your OrganizationAddress element from your UXML into this section </OrganizationAddress>", form.XMLValuesTextBoxForTest.Text);
			}
		}

		public void TestMatchingLogTextBoxReadOnly()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				var logBox = form.Controls.Find("MatchingLogTextBox", true).Single() as ZTextBox;
				AssertEquals("MatchingLogTextBox is read only when initialized", true, logBox.ReadOnly);
				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals("MatchingLogTextBox still read only after load log", true, logBox.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestOrganizationAddressInfo()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.MatchingLogTextBoxForTest.Text = string.Empty;
				form.XMLValuesTextBoxForTest.Text = string.Empty;

				UnitTestUserNotification.Instance.ClearMessages();
				AssertNull(form.OrganizationAddressInfoForTest);
				AssertEquals("Please fix the XML values error before doing UXML matching.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNullOrEmpty(form.MatchingLogTextBoxForTest.Text);

				form.MatchingLogTextBoxForTest.Text = string.Empty;
				form.XMLValuesTextBoxForTest.Text = "Dummy";
				UnitTestUserNotification.Instance.ClearMessages();
				AssertNull(form.OrganizationAddressInfoForTest);
				AssertEquals("Data at the root level is invalid. Line 1, position 1.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(string.Empty, form.MatchingLogTextBoxForTest.Text);

				form.MatchingLogTextBoxForTest.Text = string.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				form.XMLValuesTextBoxForTest.Text = UXMLMatchingDiagosticUtilsTest.ValidOrganizationAddressStr;
				AssertNotNull(form.OrganizationAddressInfoForTest);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNullOrEmpty(form.MatchingLogTextBoxForTest.Text);

				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;
				AssertNotNull(form.OrganizationAddressInfoForTest);

				form.OrganizationNameTextBoxForTest.Text = "1";
				form.Address1TextBoxForTest.Text = "2";
				form.Address2TestBoxForTest.Text = "3";
				form.OrganizationCodeTextBoxForTest.Text = "4";
				form.StateTextBoxForTest.Text = "5";
				form.PostCodeTextBoxForTest.Text = "6";
				form.CityTextBoxForTest.Text = "7";
				form.CountryTextBoxForTest.Text = "8";
				form.AdditionalAddressTextBoxForTest.Text = "9";
				form.FaxTextBoxForTest.Text = "10";
				form.EmailTextBoxForTest.Text = "11";
				form.PortTextBoxForTest.Text = "12";
				form.PhoneTextBoxForTest.Text = "13";
				form.RegNumberTypeForTest.Text = "14";
				form.RegNumberCodeForTest.Text = "15";
				form.ContactNameTextBoxForTest.Text = "16";
				form.UniversalOfficeCodeTextBoxForTest.Text = "17";
				form.UniversalNettingCodeTextBoxForTest.Text = "18";
				form.AddressCodeTextBoxForTest.Text = "19";

				var address = form.OrganizationAddressInfoForTest;
				CombineAssertions(() =>
				{
					AssertEquals("1", address.CompanyName);
					AssertEquals("2", address.Address1);
					AssertEquals("3", address.Address2);
					AssertEquals("4", address.OrganizationCode);
					AssertEquals("5", address.State);
					AssertEquals("6", address.Postcode);
					AssertEquals("7", address.City);
					AssertEquals("8", address.Country.Code);
					AssertEquals("9", address.AdditionalAddressInformation);
					AssertEquals("10", address.Fax);
					AssertEquals("11", address.Email);
					AssertEquals("12", address.Port.Code);
					AssertEquals("13", address.Phone);
					AssertEquals("14", address.GovRegNumType.Code);
					AssertEquals("15", address.GovRegNum);
					AssertEquals("16", address.Contact);
					AssertEquals("17", address.UniversalOfficeCode);
					AssertEquals("18", address.UniversalNettingCode);
					AssertEquals("19", address.AddressShortCode);
				});
			}
		}

		public void TestMatchOrgHeaderAndOrgAddress()
		{
			var targetOrg = Factory.New<OrgHeader>();
			targetOrg.OH_Code = "ANORG1";
			targetOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			targetOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			targetOrg.MainAddress.OA_State = "ABC";
			targetOrg.MainAddress.OA_PostCode = "123123";
			targetOrg.MainAddress.OA_City = "SYDNEY";
			targetOrg.MainAddress.OA_RN_NKCountryCode = "AU";
			targetOrg.MainAddress.OA_Fax = "12300001111";
			targetOrg.MainAddress.OA_Email = "test@test.com";
			targetOrg.OH_RL_NKClosestPort = "AUSYD";

			var contact = targetOrg.Contacts.AddNew();
			contact.OC_ContactName = "Peter Lewis";

			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(targetOrg.OH_FullName, "AU"));
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			Factory.Save();
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();

				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;
				AssertNotNull(form.OrganizationAddressInfoForTest);

				// the Match Organization button is hidden for now. It may be reintroduced in the future.
				// form.MatchOrgButtonForTest.PerformClick();
				// AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);
				// AssertContains("No matched result", form.MatchingLogTextBoxForTest.Text);

				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);
				AssertContains("No matched result", form.MatchingLogTextBoxForTest.Text);

				form.OrganizationNameTextBoxForTest.Text = "A UXML AWESOME ORGANIZATION";
				form.Address1TextBoxForTest.Text = "1300 N SAM HOUSTON";
				form.StateTextBoxForTest.Text = "ABC";
				form.PostCodeTextBoxForTest.Text = "123123";
				form.CityTextBoxForTest.Text = "SYDNEY";
				form.CountryTextBoxForTest.Text = "AU";
				form.FaxTextBoxForTest.Text = "12300001111";
				form.EmailTextBoxForTest.Text = "test@test.com";
				form.PortTextBoxForTest.Text = "AUSYD";
				form.RegNumberTypeForTest.Text = "ABC";
				form.RegNumberCodeForTest.Text = "123";
				form.ContactNameTextBoxForTest.Text = "Peter Lewis";

				// the Match Organization button is hidden for now. It may be reintroduced in the future.
				// form.MatchOrgButtonForTest.PerformClick();
				// CombineAssertions(() =>
				// {
				// 	AssertEquals(1, form.UXMLMatchingDiagnosticModelsForTest.Count);
				// 	AssertEquals(targetOrg.PK, form.UXMLMatchingDiagnosticModelsForTest.Single().OrgPK);
				// 	AssertEquals(targetOrg.OH_Code, form.UXMLMatchingDiagnosticModelsForTest.Single().MatchTo);
				// 	AssertContains("Found matched result", form.MatchingLogTextBoxForTest.Text);
				// });

				form.UXMLMatchingDiagnosticModelsForTest.Clear();
				form.MatchAddressButtonForTest.PerformClick();

				AssertEquals(1, form.UXMLMatchingDiagnosticModelsForTest.Count);
				CombineAssertions(() =>
				{
					var model = form.UXMLMatchingDiagnosticModelsForTest.Single();
					AssertEquals(targetOrg.PK, model.OrgPK);
					AssertEquals(targetOrg.OH_Code, model.MatchedOrgCode);
					AssertEquals(targetOrg.MainAddress.OA_Code, model.MatchedAddressCode);

					var logText = form.MatchingLogTextBoxForTest.Text;
					AssertContains("Found matched result", logText);
					AssertContains(targetOrg.OH_Code, logText);
				});
			}
		}

		public void TestMatchOrgHeaderAndOrgAddress_IsSameSystem_OrgCodeMatched_AddressCodeNotFoundOnOrg_AddressMatchFoundWithinOrg()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "ANORG1";
			targetOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			targetOrg.MainAddress.OA_Code = "1300 N SAM HOUSTON";
			targetOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			targetOrg.MainAddress.OA_State = "ABC";
			targetOrg.MainAddress.OA_PostCode = "123123";
			targetOrg.MainAddress.OA_City = "SYDNEY";
			targetOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "OTHERORG";
			otherOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			otherOrg.MainAddress.OA_Code = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_State = "ABC";
			otherOrg.MainAddress.OA_PostCode = "123123";
			otherOrg.MainAddress.OA_City = "SYDNEY";
			otherOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var targetPatternMatchingName = Factory.New<PatternMatchingName>();
			targetPatternMatchingName.PMN_OH = targetOrg.PK;
			targetPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(targetOrg.OH_FullName, "AU"));
			targetPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			targetPatternMatchingName.PMN_ParentId = targetOrg.PK;
			targetPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var otherPatternMatchingName = Factory.New<PatternMatchingName>();
			otherPatternMatchingName.PMN_OH = otherOrg.PK;
			otherPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(otherOrg.OH_FullName, "AU"));
			otherPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			otherPatternMatchingName.PMN_ParentId = otherOrg.PK;
			otherPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var targetPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			targetPatternMatchingAddress.PMA_OH = targetOrg.PK;
			targetPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.MainAddress.GetFullAddressString());
			targetPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			targetPatternMatchingAddress.PMA_ParentId = targetOrg.MainAddress.PK;
			targetPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var otherPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			otherPatternMatchingAddress.PMA_OH = otherOrg.PK;
			otherPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(otherOrg.MainAddress.GetFullAddressString());
			otherPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			otherPatternMatchingAddress.PMA_ParentId = otherOrg.MainAddress.PK;
			otherPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;
				AssertNotNull(form.OrganizationAddressInfoForTest);

				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);
				AssertContains("No matched result", form.MatchingLogTextBoxForTest.Text);

				form.OrganizationCodeTextBoxForTest.Text = "ANORG1";
				form.AddressCodeTextBoxForTest.Text = "NOT SAM HOUSTON";
				form.OrganizationNameTextBoxForTest.Text = "A UXML AWESOME ORGANIZATION";
				form.Address1TextBoxForTest.Text = "1300 N SAM HOUSTON";
				form.StateTextBoxForTest.Text = "ABC";
				form.PostCodeTextBoxForTest.Text = "123123";
				form.CityTextBoxForTest.Text = "SYDNEY";
				form.CountryTextBoxForTest.Text = "AU";
				form.FaxTextBoxForTest.Text = "12300001111";
				form.EmailTextBoxForTest.Text = "test@test.com";
				form.PortTextBoxForTest.Text = "AUSYD";
				form.RegNumberTypeForTest.Text = "ABC";
				form.RegNumberCodeForTest.Text = "123";
				form.ContactNameTextBoxForTest.Text = "Peter Lewis";

				// Comment since we temporarily disabled the IsFromSystemCheck, we will add it back later
				//form.UXMLMatchingDiagnosticModelsForTest.Clear();
				//form.IsFromSameSystemCheckBoxForTest.Checked = false;
				//form.MatchAddressButtonForTest.PerformClick();

				//CombineAssertions("Not from same system, so no filtering based on org", () =>
				//{
				//	AssertContainsExactElementsInAnyOrder(new[] { "ANORG1", "OTHERORG" }, form.UXMLMatchingDiagnosticModelsForTest.Select(m => m.MatchedOrgCode));

				//	var logText = form.MatchingLogTextBoxForTest.Text;
				//	AssertContains("Found matched result", logText);
				//	AssertNotContains("UXML from same system detected.", logText);
				//	AssertNotContains("Organization Code 'ANORG1' located within database. Restricting potential address matches to this Organization.", logText);
				//	AssertNotContains("No match was found within Organization 'ANORG1'. Retrying matching to find a better match within other Organizations.", logText);
				//	AssertNotContains("Address Code 'NOT SAM HOUSTON' belonging to Organization Code 'ANORG1' located within database. Organization Address from database will be used.", logText);
				//});

				form.UXMLMatchingDiagnosticModelsForTest.Clear();
				form.IsFromSameSystemCheckBoxForTest.Checked = true;
				form.MatchAddressButtonForTest.PerformClick();

				CombineAssertions("Now from same system, so only show results with matching org code", () =>
				{
					AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);

					var logText = form.MatchingLogTextBoxForTest.Text;
					AssertContains("Found matched result", logText);
					AssertContains("Information - Matched to 'ANORG1' by code, address '1300 N SAM HOUSTON' (only address).", logText);
				});
			}
		}

		public void TestMatchOrgHeaderAndOrgAddress_IsSameSystem_OrgCodeMatched_AddressCodeNotFoundOnOrg_NoAddressMatchFoundWithinOrg()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "ANORG1";
			targetOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			targetOrg.MainAddress.OA_Code = "THE WRONG ADDRESS";
			targetOrg.MainAddress.OA_Address1 = "789 NOT A REAL STREET";
			targetOrg.MainAddress.OA_State = "ABC";
			targetOrg.MainAddress.OA_PostCode = "456456";
			targetOrg.MainAddress.OA_City = "SYDNEY";
			targetOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "OTHERORG";
			otherOrg.OH_FullName = "B UXML AWESOME ORGANIZATION";
			otherOrg.MainAddress.OA_Code = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_State = "ABC";
			otherOrg.MainAddress.OA_PostCode = "123123";
			otherOrg.MainAddress.OA_City = "SYDNEY";
			otherOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var targetPatternMatchingName = Factory.New<PatternMatchingName>();
			targetPatternMatchingName.PMN_OH = targetOrg.PK;
			targetPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(targetOrg.OH_FullName, "AU"));
			targetPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			targetPatternMatchingName.PMN_ParentId = targetOrg.PK;
			targetPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var otherPatternMatchingName = Factory.New<PatternMatchingName>();
			otherPatternMatchingName.PMN_OH = otherOrg.PK;
			otherPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(otherOrg.OH_FullName, "AU"));
			otherPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			otherPatternMatchingName.PMN_ParentId = otherOrg.PK;
			otherPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var targetPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			targetPatternMatchingAddress.PMA_OH = targetOrg.PK;
			targetPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.MainAddress.GetFullAddressString());
			targetPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			targetPatternMatchingAddress.PMA_ParentId = targetOrg.MainAddress.PK;
			targetPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var otherPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			otherPatternMatchingAddress.PMA_OH = otherOrg.PK;
			otherPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(otherOrg.MainAddress.GetFullAddressString());
			otherPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			otherPatternMatchingAddress.PMA_ParentId = otherOrg.MainAddress.PK;
			otherPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;
				AssertNotNull(form.OrganizationAddressInfoForTest);

				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);
				AssertContains("No matched result", form.MatchingLogTextBoxForTest.Text);

				form.OrganizationCodeTextBoxForTest.Text = "ANORG1";
				form.AddressCodeTextBoxForTest.Text = "NOT SAM HOUSTON";
				form.OrganizationNameTextBoxForTest.Text = "B UXML AWESOME ORGANIZATION";
				form.Address1TextBoxForTest.Text = "1300 N SAM HOUSTON";
				form.StateTextBoxForTest.Text = "ABC";
				form.PostCodeTextBoxForTest.Text = "123123";
				form.CityTextBoxForTest.Text = "SYDNEY";
				form.CountryTextBoxForTest.Text = "AU";
				form.FaxTextBoxForTest.Text = "12300001111";
				form.EmailTextBoxForTest.Text = "test@test.com";
				form.PortTextBoxForTest.Text = "AUSYD";
				form.RegNumberTypeForTest.Text = "ABC";
				form.RegNumberCodeForTest.Text = "123";
				form.ContactNameTextBoxForTest.Text = "Peter Lewis";

				// Comment since we temporarily disabled the IsFromSystemCheck, we will add it back later
				//form.UXMLMatchingDiagnosticModelsForTest.Clear();
				//form.IsFromSameSystemCheckBoxForTest.Checked = false;
				//form.MatchAddressButtonForTest.PerformClick();

				//CombineAssertions("Not from same system, so no filtering based on org", () =>
				//{
				//	AssertContainsExactElementsInAnyOrder(new[] { "OTHERORG" }, form.UXMLMatchingDiagnosticModelsForTest.Select(m => m.MatchedOrgCode));

				//	var logText = form.MatchingLogTextBoxForTest.Text;
				//	AssertContains("Found matched result", logText);
				//	AssertNotContains("UXML from same system detected.", logText);
				//	AssertNotContains("Organization Code 'ANORG1' located within database. Restricting potential address matches to this Organization.", logText);
				//	AssertNotContains("No match was found within Organization 'ANORG1'. Retrying matching to find a better match within other Organizations.", logText);
				//	AssertNotContains("Address Code 'NOT SAM HOUSTON' belonging to Organization Code 'ANORG1' located within database. Organization Address from database will be used.", logText);
				//});

				form.UXMLMatchingDiagnosticModelsForTest.Clear();
				form.IsFromSameSystemCheckBoxForTest.Checked = true;
				form.MatchAddressButtonForTest.PerformClick();

				CombineAssertions("Now from same system, so try to find results within org, but fallback when no suitable match", () =>
				{
					AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);

					var logText = form.MatchingLogTextBoxForTest.Text;
					AssertContains("Found matched result", logText);
					AssertContains("Information - Matched to 'ANORG1' by code, address '789 NOT A REAL STREET' (only address).", logText);
				});
			}
		}

		public void TestMatchOrgHeaderAndOrgAddress_IsSameSystem_OrgCodeAndAddressCodeMatched()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "ANORG1";
			targetOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			targetOrg.MainAddress.OA_Code = "1300 N SAM HOUSTON";
			targetOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			targetOrg.MainAddress.OA_State = "ABC";
			targetOrg.MainAddress.OA_PostCode = "123123";
			targetOrg.MainAddress.OA_City = "SYDNEY";
			targetOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();
			otherOrg.OH_Code = "OTHERORG";
			otherOrg.OH_FullName = "A UXML AWESOME ORGANIZATION";
			otherOrg.MainAddress.OA_Code = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_Address1 = "1300 N SAM HOUSTON";
			otherOrg.MainAddress.OA_State = "ABC";
			otherOrg.MainAddress.OA_PostCode = "123123";
			otherOrg.MainAddress.OA_City = "SYDNEY";
			otherOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var targetPatternMatchingName = Factory.New<PatternMatchingName>();
			targetPatternMatchingName.PMN_OH = targetOrg.PK;
			targetPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(targetOrg.OH_FullName, "AU"));
			targetPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			targetPatternMatchingName.PMN_ParentId = targetOrg.PK;
			targetPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var otherPatternMatchingName = Factory.New<PatternMatchingName>();
			otherPatternMatchingName.PMN_OH = otherOrg.PK;
			otherPatternMatchingName.PMN_HashedValue = TextStandardizerHelper.ComputeStringHashFast(TextStandardizerHelper.StandardizeCompanyName(otherOrg.OH_FullName, "AU"));
			otherPatternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			otherPatternMatchingName.PMN_ParentId = otherOrg.PK;
			otherPatternMatchingName.PMN_RN_NKCountryCode = "AU";

			var targetPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			targetPatternMatchingAddress.PMA_OH = targetOrg.PK;
			targetPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(targetOrg.MainAddress.GetFullAddressString());
			targetPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			targetPatternMatchingAddress.PMA_ParentId = targetOrg.MainAddress.PK;
			targetPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var otherPatternMatchingAddress = Factory.New<PatternMatchingAddress>();
			otherPatternMatchingAddress.PMA_OH = otherOrg.PK;
			otherPatternMatchingAddress.PMA_HashedValue = TextStandardizerHelper.ComputeStringHashFast(otherOrg.MainAddress.GetFullAddressString());
			otherPatternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			otherPatternMatchingAddress.PMA_ParentId = otherOrg.MainAddress.PK;
			otherPatternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;
				AssertNotNull(form.OrganizationAddressInfoForTest);

				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);
				AssertContains("No matched result", form.MatchingLogTextBoxForTest.Text);

				form.OrganizationCodeTextBoxForTest.Text = "ANORG1";
				form.AddressCodeTextBoxForTest.Text = "1300 N SAM HOUSTON";
				form.OrganizationNameTextBoxForTest.Text = "A UXML AWESOME ORGANIZATION";
				form.Address1TextBoxForTest.Text = "1300 N SAM HOUSTON";
				form.StateTextBoxForTest.Text = "ABC";
				form.PostCodeTextBoxForTest.Text = "123123";
				form.CityTextBoxForTest.Text = "SYDNEY";
				form.CountryTextBoxForTest.Text = "AU";
				form.FaxTextBoxForTest.Text = "12300001111";
				form.EmailTextBoxForTest.Text = "test@test.com";
				form.PortTextBoxForTest.Text = "AUSYD";
				form.RegNumberTypeForTest.Text = "ABC";
				form.RegNumberCodeForTest.Text = "123";
				form.ContactNameTextBoxForTest.Text = "Peter Lewis";

				// Comment since we temporarily disabled the IsFromSystemCheck, we will add it back later
				//form.UXMLMatchingDiagnosticModelsForTest.Clear();
				//form.IsFromSameSystemCheckBoxForTest.Checked = false;
				//form.MatchAddressButtonForTest.PerformClick();

				//CombineAssertions("Not from same system, so no filtering based on org", () =>
				//{
				//	AssertContainsExactElementsInAnyOrder(new[] { "ANORG1", "OTHERORG" }, form.UXMLMatchingDiagnosticModelsForTest.Select(m => m.MatchedOrgCode));

				//	var logText = form.MatchingLogTextBoxForTest.Text;
				//	AssertContains("Found matched result", logText);
				//	AssertNotContains("UXML from same system detected.", logText);
				//	AssertNotContains("Organization Code 'ANORG1' located within database. Restricting potential address matches to this Organization.", logText);
				//	AssertNotContains("No match was found within Organization 'ANORG1'. Retrying matching to find a better match within other Organizations.", logText);
				//	AssertNotContains("Address Code '1300 N SAM HOUSTON' belonging to Organization Code 'ANORG1' located within database. Organization Address from database will be used.", logText);
				//});

				form.UXMLMatchingDiagnosticModelsForTest.Clear();
				form.IsFromSameSystemCheckBoxForTest.Checked = true;
				form.MatchAddressButtonForTest.PerformClick();

				CombineAssertions("Now from same system, so only show direct match from org and address codes", () =>
				{
					AssertEquals(0, form.UXMLMatchingDiagnosticModelsForTest.Count);

					var logText = form.MatchingLogTextBoxForTest.Text;
					AssertContains("Found matched result", logText);
					AssertContains("Information - Matched to 'ANORG1' by code, address '1300 N SAM HOUSTON' (only address).", logText);
				});
			}
		}

		public void TestClearResults()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();

				form.MatchingLogTextBoxForTest.Text = "ASASASAS";
				form.UXMLMatchedListUserControlForTest.PopulatePanel(new[] { new UXMLMatchingDiagnosticModel() });
				form.DebuggerScoringResultsForTest.Add(new ScoringResult());
				UXMLMatchingDiagnosticToolForm.MonitoringObjects.TryAdd("ABC", new MonitoringObjectValue
				{
					Value = "123"
				});

				form.DeduplicationMonitoringUserControl.SetDataContext(UXMLMatchingDiagnosticToolForm.MonitoringObjects);
				var model = form.DeduplicationMonitoringUserControl.model;
				var tableLayOut = form.UXMLMatchedListUserControlForTest.Controls.Find("controlTableLayout", true)[0];
				form.DebuggerHub.Register(new DummyDeduplicationDebuggerParticipant());

				CombineAssertions(() =>
				{
					AssertNotNullOrEmpty(form.MatchingLogTextBoxForTest.Text);
					AssertEquals(1, form.DebuggerScoringResultsForTest.Count);
					AssertEquals(1, UXMLMatchingDiagnosticToolForm.MonitoringObjects.Count);
					AssertNotNull(model);
					AssertEquals(1, model.monitoringObject.Count);
					AssertEquals(1, tableLayOut.Controls.Count);
					Assert(!form.DebuggerHub.FindParticipant(form.DebuggerName).Equals(form));
				});

				form.ClearResultsAndSetToParticipantForTest();
				model = form.DeduplicationMonitoringUserControl.model;
				tableLayOut = form.UXMLMatchedListUserControlForTest.Controls.Find("controlTableLayout", true)[0];
				CombineAssertions(() =>
				{
					AssertNullOrEmpty(form.MatchingLogTextBoxForTest.Text);
					AssertEquals(0, form.DebuggerScoringResultsForTest.Count);
					AssertEquals(0, UXMLMatchingDiagnosticToolForm.MonitoringObjects.Count);
					AssertNotNull(model);
					AssertEquals(0, model.monitoringObject.Count);
					AssertEquals(0, tableLayOut.Controls.Count);
					Assert(form.DebuggerHub.FindParticipant(form.DebuggerName).Equals(form));
				});
			}
		}

		public void TestEnterValuesTextBoxMaxLength()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.InputValueTabControlForTest.SelectedTab = form.EnterValuesTabPageForTest;

				var organizationProperties = typeof(OrganizationAddress).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var countryProperties = typeof(UniversalDataBuss.DataObjects.Universal.Country).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var portProperties = typeof(UNLOCO).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var regNumberTypeProperties = typeof(UNLOCO).GetProperties(BindingFlags.Public | BindingFlags.Instance);

				CombineAssertions(() =>
				{
					AssertTextBoxMaxLength(form.OrganizationCodeTextBoxForTest, organizationProperties, nameof(OrganizationAddress.OrganizationCode));
					AssertTextBoxMaxLength(form.OrganizationNameTextBoxForTest, organizationProperties, nameof(OrganizationAddress.CompanyName));
					AssertTextBoxMaxLength(form.AddressCodeTextBoxForTest, organizationProperties, nameof(OrganizationAddress.AddressShortCode));
					AssertTextBoxMaxLength(form.AdditionalAddressTextBoxForTest, organizationProperties, nameof(OrganizationAddress.AdditionalAddressInformation));
					AssertTextBoxMaxLength(form.Address1TextBoxForTest, organizationProperties, nameof(OrganizationAddress.Address1));
					AssertTextBoxMaxLength(form.Address2TestBoxForTest, organizationProperties, nameof(OrganizationAddress.Address2));
					AssertTextBoxMaxLength(form.CountryTextBoxForTest, countryProperties, nameof(OrganizationAddress.Country.Code));
					AssertTextBoxMaxLength(form.CityTextBoxForTest, organizationProperties, nameof(OrganizationAddress.City));
					AssertTextBoxMaxLength(form.PostCodeTextBoxForTest, organizationProperties, nameof(OrganizationAddress.Postcode));
					AssertTextBoxMaxLength(form.StateTextBoxForTest, organizationProperties, nameof(OrganizationAddress.State));
					AssertTextBoxMaxLength(form.EmailTextBoxForTest, organizationProperties, nameof(OrganizationAddress.Email));
					AssertTextBoxMaxLength(form.FaxTextBoxForTest, organizationProperties, nameof(OrganizationAddress.Fax));
					AssertTextBoxMaxLength(form.PhoneTextBoxForTest, organizationProperties, nameof(OrganizationAddress.Phone));
					AssertTextBoxMaxLength(form.PortTextBoxForTest, portProperties, nameof(OrganizationAddress.Port.Code));
					AssertTextBoxMaxLength(form.RegNumberTypeForTest, regNumberTypeProperties, nameof(OrganizationAddress.GovRegNumType.Code));
					AssertTextBoxMaxLength(form.RegNumberCodeForTest, organizationProperties, nameof(OrganizationAddress.GovRegNum));
					AssertTextBoxMaxLength(form.UniversalNettingCodeTextBoxForTest, organizationProperties, nameof(OrganizationAddress.UniversalNettingCode));
					AssertTextBoxMaxLength(form.UniversalOfficeCodeTextBoxForTest, organizationProperties, nameof(OrganizationAddress.UniversalOfficeCode));
					AssertTextBoxMaxLength(form.ContactNameTextBoxForTest, organizationProperties, nameof(OrganizationAddress.Contact));
				});
			}
		}

		#region FormatXml
#if !WINZOR
		public void TestFormatXml_WithValidValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();

				form.XMLValuesTextBoxForTest.Text = @"
<EXDefaultCntryOfOrigin TableName=""RefCountry"">
<Code> AD </Code>
<PK> 4a039a4c - dcf4 - 472a - 872d - ca545b12ac79 </PK>
</EXDefaultCntryOfOrigin>";

				form.MatchAddressButtonForTest.PerformClick();

				form.XMLValuesTextBoxForTest.SelectAll();

				var expectedRichTextExcerpt = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}
{\colortbl ;\red0\green0\blue255;\red178\green34\blue34;\red255\green0\blue0;\red0\green0\blue0;}
\viewkind4\uc1\pard\cf1\f0\fs16 <\cf2 EXDefaultCntryOfOrigin\cf1  \cf3 TableName\cf1 =""RefCountry"">\cf4\par
  \cf1 <\cf2 Code\cf1 >\cf4  AD \cf1 </\cf2 Code\cf1 >\cf4\par
  \cf1 <\cf2 PK\cf1 >\cf4  4a039a4c - dcf4 - 472a - 872d - ca545b12ac79 \cf1 </\cf2 PK\cf1 >\cf4\par
\cf1 </\cf2 EXDefaultCntryOfOrigin\cf1 >}
";

				var expectedRTFExcerptIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(expectedRichTextExcerpt);
				var actualRtfIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(form.XMLValuesTextBoxForTest.SelectedRtf);

				AssertContains(expectedRTFExcerptIndependentOfMachineSetting, actualRtfIndependentOfMachineSetting);
			}
		}

		public void TestFormatXml_WithInValidValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();

				form.XMLValuesTextBoxForTest.Text = @"
<EXDefaultCntryOfOrigin TableName=""RefCountry"">
<Code> AD </Code>
<PK> 4a039a4c - dcf4 - 472a - 872d - ca545b12ac79 </PK>
</EXDefaultCntryOfOrigin";

				var expectedMessage = "Unexpected end of file has occurred. The following elements are not closed: EXDefaultCntryOfOrigin. Line 4, position 25.";
				var expectedRichTextExcerpt = @"<EXDefaultCntryOfOrigin TableName=""RefCountry"">\par
<Code> AD </Code>\par
<PK> 4a039a4c - dcf4 - 472a - 872d - ca545b12ac79 </PK>\par
</EXDefaultCntryOfOrigin}
";
				form.MatchAddressButtonForTest.PerformClick();
				form.XMLValuesTextBoxForTest.SelectAll();

				CombineAssertions(() =>
				{
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					// We don't want to assert the rich text is exactly equal, as there are certain elements such as version numbers which can change
					AssertContains("Rich text didn't change.", expectedRichTextExcerpt, form.XMLValuesTextBoxForTest.SelectedRtf);
				});
			}
		}
#endif
		public void TestFormatXml_OnTabOut()
		{
			const string UnformattedXML = @"<XML><BadlyFormatted>  <ButStillValid>FooBar </ButStillValid>		</BadlyFormatted>
</XML>";

			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.XMLValuesTextBoxForTest.Select();
				form.XMLValuesTextBoxForTest.Text = UnformattedXML;

				form.ProcessTabKeyForTest(true);

				var expectedRTFExcerptIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(expectedRTFExcerpt);
#if !WINZOR
				var actualRtfIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(form.XMLValuesTextBoxForTest.Rtf);
#else
				var actualRtfIndependentOfMachineSetting = form.XMLValuesTextBoxForTest.Html;
#endif
				AssertContains(expectedRTFExcerptIndependentOfMachineSetting, actualRtfIndependentOfMachineSetting);
			}
		}

		public void TestFormatXml_OnClickOut()
		{
			const string UnformattedXML = @"<XML><BadlyFormatted>  <ButStillValid>FooBar </ButStillValid>		</BadlyFormatted>
</XML>";

			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				form.XMLValuesTextBoxForTest.Select();
				form.XMLValuesTextBoxForTest.Text = UnformattedXML;

				form.MatchingLogTextBoxForTest.Select();
				var expectedRTFExcerptIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(expectedRTFExcerpt);
#if !WINZOR
				var actualRtfIndependentOfMachineSetting = ORtfTextUtilTest.GetRtfStringIndependentOfMachineSettings(form.XMLValuesTextBoxForTest.Rtf);
#else
				var actualRtfIndependentOfMachineSetting = form.XMLValuesTextBoxForTest.Html;
#endif
				AssertContains(expectedRTFExcerptIndependentOfMachineSetting, actualRtfIndependentOfMachineSetting);
			}
		}

		const string expectedRTFExcerpt = @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Tahoma;}}
{\colortbl ;\red0\green0\blue255;\red178\green34\blue34;\red0\green0\blue0;}
\viewkind4\uc1\pard\cf1\f0\fs16 <\cf2 XML\cf1 >\cf3\par
  \cf1 <\cf2 BadlyFormatted\cf1 >\cf3\par
    \cf1 <\cf2 ButStillValid\cf1 >\cf3 FooBar \cf1 </\cf2 ButStillValid\cf1 >\cf3\par
  \cf1 </\cf2 BadlyFormatted\cf1 >\cf3\par
\cf1 </\cf2 XML\cf1 >\par
}";

		#endregion

		#region Threshold Labels

		public void TestThresholdLabelsDisplayedCorrectly_MatchAddress()
		{
			var orgExcludeThresholdPercentage = 67;
			var addressExcludeThresholdPercentage = 56;

			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.UXMLOrganisationMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, orgExcludeThresholdPercentage))
			using (OrganisationsDataRegistry.Instance.OrgAddressMinimumConfidence.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, addressExcludeThresholdPercentage))
			{
				form.Show();

				form.MatchAddressButtonForTest.PerformClick();
				AssertThresholdsLabelsAddedCorretly_MatchAddress(form, orgExcludeThresholdPercentage, addressExcludeThresholdPercentage);
			}
		}

		void AssertThresholdsLabelsAddedCorretly_MatchAddress(UXMLMatchingDiagnosticToolFormForTest form, int orgExcludeThresholdPercentage, int addressExcludeThresholdPercentage)
		{
			var controlNames = form.ThresholdsLayoutForTest.Controls.Cast<Control>().Select(control => control.Name);

			CombineAssertions("Both orgMatchThresholdLabel and addressMatchThresholdLabel should be added to the thresholdsLayout.", () =>
			{
				AssertCollectionContains("OrgMatchThresholdLabel", controlNames);
				AssertCollectionContains("AddressMatchThresholdLabel", controlNames);
			});

			var orgMatchThresholdLabelText = $"Organization Match Threshold: {orgExcludeThresholdPercentage}%";
			AssertEquals(orgMatchThresholdLabelText, form.OrgMatchThresholdLabelForTest.Text);

			var addressMatchThresholdLabelText = $"Address Match Threshold: {addressExcludeThresholdPercentage}%";
			AssertEquals(addressMatchThresholdLabelText, form.AddressMatchThresholdLabelForTest.Text);
		}

		#endregion

		#region Receive

		public void TestReceiveWithValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				form.Show();

				var value = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						Score = 0.9
					},
					new ScoringResult()
					{
						Score = 0.3
					},
					new ScoringResult()
					{
						Score = 0.5
					}
				};

				var methodName = "Org_ScoringResults";
				form.Receive(value, methodName, TimeSpan.MaxValue);
				var result = form.DebuggerScoringResultsForTest;
				var model = form.DeduplicationMonitoringUserControl.model;
				CombineAssertions(() =>
				{
					AssertEquals(3, result.Count);
					Assert(result.Any(o => o.Score == 0.9));
					Assert(result.Any(o => o.Score == 0.3));
					Assert(result.Any(o => o.Score == 0.5));
					AssertEquals(1, UXMLMatchingDiagnosticToolForm.MonitoringObjects.Count);
					AssertEquals(1, model.monitoringObject.Count);
				});
			}
		}

		public void TestReceiveWithValueNotEmptyThePreScoreResults()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				form.Show();

				var value = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						Score = 0.9
					}
				};

				var methodName = "Org_ScoringResults";
				form.Receive(value, methodName, TimeSpan.MaxValue);
				var result = form.DebuggerScoringResultsForTest;
				var model = form.DeduplicationMonitoringUserControl.model;
				AssertEquals(1, result.Count);
				AssertEquals(0.9, result[0].Score);

				value = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						Score = 0.7
					}
				};

				form.Receive(value, methodName, TimeSpan.MaxValue);
				AssertEquals(2, result.Count);
				AssertEquals(0.9, result[0].Score);
				AssertEquals(0.7, result[1].Score);
			}
		}

		public void TestReceiveWithNoValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				var value = new List<ScoringResult>();
				var methodName = "Org_ScoringResults";
				form.Receive(value, methodName, TimeSpan.MaxValue);
				var result = form.DebuggerScoringResultsForTest;
				AssertEquals(0, result.Count);
			}
		}

		public void TestReceiveNotTheSpecificMethod()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				var value = new List<ScoringResult>()
				{
					new ScoringResult()
					{
						Score = 0.9
					},
					new ScoringResult()
					{
						Score = 0.3
					},
					new ScoringResult()
					{
						Score = 0.5
					}
				};

				var methodName = "NotTheSpecificMethod";
				form.Receive(value, methodName, TimeSpan.MaxValue);
				var result = form.DebuggerScoringResultsForTest;
				AssertEquals(0, result.Count);
			}
		}

		public void TestReceiveWithNullValue()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				var methodName = "Org_ScoringResults";
				form.Receive(null, methodName, TimeSpan.MaxValue);
				var result = form.DebuggerScoringResultsForTest;
				AssertEquals(0, result.Count);
			}
		}

		public void TestReceiveEnsureScoringMethodNameNotChanged()
		{
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			targetOrg.OH_Code = "TESTYO2";
			targetOrg.OH_FullName = "ORGANISATION";
			targetOrg.CustomsCodes.AddNew("ATF", "1234F", "AU");

			var targetAddress = targetOrg.MainAddress;
			targetAddress.Address1 = "PADDINGTON NSW";
			targetAddress.OA_Email = "ABCD@TEST.COM";
			targetAddress.OA_Phone = "1504444444";

			var targetContact = targetOrg.Contacts.AddNew();
			targetContact.OC_ContactName = "Jimmy Yang";
			targetContact.OC_Email = "ABCD@TEST.COM";
			targetContact.OC_Phone = "1504444444";

			var hashedAddress = TextStandardizerHelper.ComputeStringHashFast(targetAddress.Address1);
			var hashedEmail = TextStandardizerHelper.ComputeStringHashFast(targetAddress.OA_Email);
			var hashedName = TextStandardizerHelper.ComputeStringHashFast(targetOrg.OH_FullName);
			var hashedRegCode = TextStandardizerHelper.ComputeStringHashFast("1234F");

			var patternMatchingName = Factory.New<PatternMatchingName>();
			patternMatchingName.PMN_OH = targetOrg.PK;
			patternMatchingName.PMN_HashedValue = hashedName;
			patternMatchingName.PMN_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			patternMatchingName.PMN_ParentId = targetOrg.PK;
			patternMatchingName.PMN_RN_NKCountryCode = "AU";

			var patternMathcingEmail = Factory.New<PatternMatchingEmail>();
			patternMathcingEmail.PME_OH = targetOrg.PK;
			patternMathcingEmail.PME_HashedValue = hashedEmail;
			patternMathcingEmail.PME_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMathcingEmail.PME_ParentId = targetAddress.PK;
			patternMathcingEmail.PME_RN_NKCountryCode = "AU";

			var patternMatchingAddress = Factory.New<PatternMatchingAddress>();
			patternMatchingAddress.PMA_OH = targetOrg.PK;
			patternMatchingAddress.PMA_HashedValue = hashedAddress;
			patternMatchingAddress.PMA_ParentTableCode = OrgAddressSchema.Constants.Prefix;
			patternMatchingAddress.PMA_ParentId = targetAddress.PK;
			patternMatchingAddress.PMA_RN_NKCountryCode = "AU";

			var patternMathchingRegCode = Factory.New<PatternMatchingRegCode>();
			patternMathchingRegCode.PMR_OH = targetOrg.PK;
			patternMathchingRegCode.PMR_HashedValue = hashedRegCode;
			patternMathchingRegCode.PMR_ParentTableCode = OrgCusCodeSchema.Constants.Prefix;
			patternMathchingRegCode.PMR_ParentId = targetAddress.PK;
			patternMathchingRegCode.PMR_RN_NKCountryCode = "AU";

			Factory.Save();

			var organizationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "TestType",
				AddressShortCode = "THEMOMENT",
				AddressOverride = false,
				OrganizationCode = "TESTYO2",
				CompanyName = "ORGANISATION",
				Address1 = "PADDINGTON NSW",
				Port = new UNLOCO { Code = "AUMEL", Name = "Melbourne" },
				Country = new UniversalDataBuss.DataObjects.Universal.Country { Code = "AU", Name = "Australia" },
			};

			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				UXMLMatchingDiagnosticUtils.GetMatchedOrgHeader(organizationAddress, Factory);
				Assert("Matched by local code no debug info.", form.DebuggerScoringResultsForTest.Count == 0);
			}

			organizationAddress.OrganizationCode = "TESTYO3";
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			using (OrganisationsDataRegistry.Instance.OrgMatchUseDeduplication.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Medium))
			{
				UXMLMatchingDiagnosticUtils.GetMatchedOrgHeader(organizationAddress, Factory);
				Assert("Matched by dedup engine has debug info.", form.DebuggerScoringResultsForTest.Count > 0);
			}
		}

		public void TestReceive_ShouldPersistsAllScoringResult()
		{
			var masterPK = Guid.NewGuid();
			var targetPK1 = Guid.NewGuid();
			var targetPK2 = Guid.NewGuid();
			var targetPK3 = Guid.NewGuid();

			var result1 = new ScoringResult()
			{
				MasterPK = masterPK,
				TargetPK = targetPK1,
			};

			var result2 = new ScoringResult()
			{
				MasterPK = masterPK,
				TargetPK = targetPK2,
			};

			var result3 = new ScoringResult()
			{
				MasterPK = masterPK,
				TargetPK = targetPK3,
			};

			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				form.Show();
				var methodName = "Org_ScoringResults";
				form.Receive(new List<ScoringResult> { result1 }, methodName, TimeSpan.MaxValue);
				AssertContainsExactElementsInAnyOrder("First receive, should only contain 1 PK", new[] { targetPK1 }, form.DebuggerScoringResultsForTest.Select(result => result.TargetPK));

				form.Receive(new List<ScoringResult> { result2, result3 }, methodName, TimeSpan.MaxValue);
				AssertContainsExactElementsInAnyOrder("New receive should not overwrite old contents, due to the matched organization can be in the first match scoring results", new[] { targetPK1, targetPK2, targetPK3 }, form.DebuggerScoringResultsForTest.Select(result => result.TargetPK));

				form.MatchAddressButtonForTest.PerformClick();
				AssertEquals("Scoring Results Will Be Clear When User Click The Match Button", 0, form.DebuggerScoringResultsForTest.Count);
			}
		}

		public void TestDisableIsSameSystemTemporarily()
		{
			using (var form = new UXMLMatchingDiagnosticToolFormForTest())
			{
				AssertEquals(true, form.IsFromSameSystemCheckBoxForTest.ReadOnly);
				AssertEquals(true, form.IsFromSameSystemCheckBoxForTest.Checked);
			}
		}

		#endregion

		#region Implementation

		class UXMLMatchingDiagnosticToolFormForTest : UXMLMatchingDiagnosticToolForm
		{
			public List<ScoringResult> DebuggerScoringResultsForTest => DebuggerScoringResults;
			public OrganizationAddress OrganizationAddressInfoForTest => OrganizationAddressInfo;
			public RichTextBox XMLValuesTextBoxForTest => XMLValuesTextBox;
			public ZTabControl InputValueTabControlForTest => InputValueTabControl;
			public ZTabPage EnterValuesTabPageForTest => EnterValuesTabPage;
			public ZCheckBox IsFromSameSystemCheckBoxForTest => IsFromSameSystemCheckBox;
			public ZTextBox OrganizationNameTextBoxForTest => OrganizationNameTextBox;
			public ZTextBox Address1TextBoxForTest => Address1TextBox;
			public ZTextBox Address2TestBoxForTest => Address2TestBox;
			public ZTextBox OrganizationCodeTextBoxForTest => OrganizationCodeTextBox;
			public ZTextBox AddressCodeTextBoxForTest => AddressCodeTextBox;
			public ZTextBox StateTextBoxForTest => StateTextBox;
			public ZTextBox PostCodeTextBoxForTest => PostCodeTextBox;
			public ZTextBox CityTextBoxForTest => CityTextBox;
			public ZTextBox CountryTextBoxForTest => CountryTextBox;
			public ZTextBox AdditionalAddressTextBoxForTest => AdditionalAddressTextBox;
			public ZTextBox FaxTextBoxForTest => FaxTextBox;
			public ZTextBox EmailTextBoxForTest => EmailTextBox;
			public ZTextBox PortTextBoxForTest => PortTextBox;
			public ZTextBox PhoneTextBoxForTest => PhoneTextBox;
			public ZTextBox RegNumberTypeForTest => RegNumberType;
			public ZTextBox RegNumberCodeForTest => RegNumberCode;
			public ZTextBox ContactNameTextBoxForTest => ContactNameTextBox;
			public ZTextBox UniversalOfficeCodeTextBoxForTest => UniversalOfficeCodeTextBox;
			public ZTextBox UniversalNettingCodeTextBoxForTest => UniversalNettingCodeTextBox;
			public ZButton MatchOrgButtonForTest => MatchOrgButton;
			public ZButton MatchAddressButtonForTest => MatchAddressButton;
			public void ClearResultsAndSetToParticipantForTest() => ClearResultsAndSetToParticipant();
			public ZTextBox MatchingLogTextBoxForTest => MatchingLogTextBox;
			public UXMLMatchedListUserControl UXMLMatchedListUserControlForTest => uxmlMatchedListUserControl;
			public void ProcessTabKeyForTest(bool forward) => ProcessTabKey(forward);
			public KTableLayoutPanel ThresholdsLayoutForTest => ThresholdsLayout;
			public ZLabel OrgMatchThresholdLabelForTest => OrgMatchThresholdLabel;
			public ZLabel AddressMatchThresholdLabelForTest => AddressMatchThresholdLabel;
		}

		class DummyDeduplicationDebuggerParticipant : IDeduplicationDebuggerParticipant
		{
			public string DebuggerName => DeduplicationDebuggerParticipant.DeduplicationDebuggerMonitoringWindowName;

			public IDeduplicationDebuggerHub DebuggerHub { get; set; }

			public void Receive(object value, string methodName, TimeSpan executionTime)
			{
			}

			public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, object glowBizO)
			{
			}

			public void Send(string recipientName, object value, string methodName, TimeSpan executionTime, string prefix)
			{
			}
		}

		void AssertTextBoxMaxLength(ZTextBox textBox, PropertyInfo[] properties, string propertyName)
		{
			var maxLength = 1000;

			if (properties.FirstOrDefault(u => u.Name == propertyName)?.GetCustomAttribute(typeof(MaxLengthAttribute)) is MaxLengthAttribute attribute && attribute.MaxLength > 0)
			{
				maxLength = attribute.MaxLength;
			}

			AssertEquals(maxLength, textBox.MaxLength);
		}

		#endregion

	}
}
