using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class CreditReportInformationModelTest : TestCaseWithFactory
	{
		CreditReportExtractedInfo extractedInfo;

		public void TestConstructor()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainWebURL.PU_URL = "abc.cn";

			var orgAddress1 = orgHeader.MainAddress;
			orgAddress1.OA_Address1 = "40 Sunset Drive";
			orgAddress1.OA_City = "Oombabeer";
			orgAddress1.OA_RN_NKCountryCode = "AU";
			orgAddress1.OA_PostCode = "4718";
			orgAddress1.OA_State = "QLD";

			var orgAddress2 = orgHeader.Addresses.AddNew();
			orgAddress2.OA_Address1 = "578 Springvale Rd";
			orgAddress2.OA_City = "Springvale South";
			orgAddress2.OA_RN_NKCountryCode = "AU";
			orgAddress2.OA_PostCode = "3172";
			orgAddress2.OA_State = "VIC";

			var customCode1 = orgHeader.CustomsCodes.AddNew();
			customCode1.OK_CodeType = "DUN";
			customCode1.OK_CustomsRegNo = "1111111";

			var customCode2 = orgHeader.CustomsCodes.AddNew();
			customCode2.OK_CodeType = "GCR";
			customCode2.OK_CustomsRegNo = "23 112 936 991";

			orgHeader.Factory.Save();

			var model = new CreditReportInformationModel(orgHeader, extractedInfo);
			var matchAddressCollection = new ObservableCollection<OrgAddress> { orgAddress1, orgAddress2 };

			CombineAssertions(() =>
			{
				AssertNotNull(model.AddressViewModels);
				AssertNotNull(model.RelatedBrandOrCompanyNameViewModels);
				AssertNotNull(model.RegistrationNumberViewModels);
				AssertNotNull(model.WebsiteViewModels);

				AssertEquals(2, model.AddressViewModels.Count);
				Assert(model.AddressViewModels.Any(x => x.Address1 == "40 Sunset Drive" && x.City == "Oombabeer" && x.MatchAddress.PK == orgAddress1.PK && x.SelectedMergeAction == MergeAction.Codes.Update));
				Assert(model.AddressViewModels.Any(x => x.Address1 == "63 Plantation Place" && x.City == "Crudine" && x.MatchAddress == null && x.SelectedMergeAction == MergeAction.Codes.Add));

				AssertEquals(2, model.RelatedBrandOrCompanyNameViewModels.Count);
				Assert(model.RelatedBrandOrCompanyNameViewModels.Any(x => x.RelatedBrandOrCompanyName == "a brand"));
				Assert(model.RelatedBrandOrCompanyNameViewModels.Any(x => x.RelatedBrandOrCompanyName == "another company"));

				AssertEquals(2, model.RegistrationNumberViewModels.Count);
				Assert(model.RegistrationNumberViewModels.Any(x => x.RegistryNumberType == nameof(IdentifierType.DUNS) && x.NewRegistryNumber == "23 112 936 091" && x.CurrentRegistryNumber == "1111111" && x.SelectedMergeAction == MergeAction.Codes.Update));
				Assert(model.RegistrationNumberViewModels.Any(x => x.RegistryNumberType == nameof(IdentifierType.ABN) && x.NewRegistryNumber == "23 112 936 129" && string.IsNullOrEmpty(x.CurrentRegistryNumber) && x.SelectedMergeAction == MergeAction.Codes.Add));

				AssertEquals(1, model.WebsiteViewModels.Count);
				Assert(model.WebsiteViewModels.Any(x => x.OriginalMainUrl == "abc.cn" && x.NewUrl == extractedInfo.WebsiteUrl && x.SelectedMergeAction == MergeAction.Codes.Add));

				AssertEquals("Import From Credit Reports", model.FormTitle);
				AssertEquals("New Organization details have been found in the purchased Credit Report. Please select an action to update the Organization record with the Credit Report data or select Skip.", model.CreditReportInformationCaption);
				AssertEquals("Addresses", model.AddressesCaption);
				AssertEquals("Brands & Company Names", model.BrandsCaption);
				AssertEquals("Registration Numbers / Codes", model.RegistrationNumberCaption);
				AssertEquals("Website", model.WebsiteCaption);
				AssertEquals("Action", model.MergeActionHeader);
				AssertEquals("Address 1", model.Address1Header);
				AssertEquals("City", model.CityHeader);
				AssertEquals("Match Address", model.MatchAddressHeader);
				AssertEquals("New Name", model.NewBrandNameHeader);
				AssertEquals("Type", model.RegistrationNumberType);
				AssertEquals("Registration Number / Code", model.NewRegistrationNumberHeader);
				AssertEquals("Current Registration Number / Code", model.CurrentRegistrationNumberHeader);
				AssertEquals("Primary", model.PrimaryHeader);
				AssertEquals("New Website", model.NewWebsiteHeader);
				AssertEquals("Current Website", model.CurrentWebsiteHeader);
				AssertEquals("Skip", model.SkipCaption);
				AssertEquals("Update Organization", model.UpdateOrganizationCaption);
			});
		}

		public void TestEmptyFindMatchAddress()
		{
			var orgHeaderInvalid = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressEmpty = orgHeaderInvalid.MainAddress;
			orgAddressEmpty.OA_Address1 = string.Empty;

			var extractedAddressInvalid = new CreditReportExtractAddress
			{
				Address = string.Empty
			};

			extractedInfo.AddressInfos = new List<CreditReportExtractAddress> { extractedAddressInvalid };
			var model = new CreditReportInformationModel(orgHeaderInvalid, extractedInfo);

			AssertEquals(0, model.AddressViewModels.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			extractedInfo = new CreditReportExtractedInfo
			{
				OrganizationName = "name",
				Phone = "1324",
				Email = "4@com",
				WebsiteUrl = "abc.new.cn",
				EmployeesCount = 99,
				RegistrationNums = new List<CreditReportExtractRegistrationNumber>
				{
					new CreditReportExtractRegistrationNumber
					{
						Type = IdentifierType.DUNS, Number = "23 112 936 091"
					},
					new CreditReportExtractRegistrationNumber
					{
						Type = IdentifierType.ABN, Number = "23 112 936 129"
					},
					new CreditReportExtractRegistrationNumber
					{
						Type = IdentifierType.ACN, Number = "23 112 936 991"
					}
				},
				AddressInfos = new List<CreditReportExtractAddress>
				{
					new CreditReportExtractAddress
					{
						Address = "40 Sunset Drive",
						City = "Oombabeer",
						State = "QLD",
						Postcode = "4718",
						Country = "AU",
						Capabilities = new List<string> { "OFC" }
					},
					new CreditReportExtractAddress
					{
						Address = "63 Plantation Place",
						City = "Crudine",
						State = "NDW",
						Postcode = "2795",
						Country = "AU",
						Capabilities = new List<string> { "PST", "OFC" }
					}
				},
				RelatedBrandOrCompanyNames = new List<string> { "a brand", "another company" }
			};
		}
	}
}
