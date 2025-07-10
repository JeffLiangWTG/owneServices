using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Schema;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Integration.Freight;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DpsCandidateCreatorTest : TestCaseWithFactory
	{
		public void TestNameCandidateNameTypeSetToORGExplicitly()
		{
			var creator = new DpsCandidateCreator();
			var requestHeader = creator.NewRequestHeader(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			var nameCandidate = requestHeader.DpsNameCandidates.Single();
			AssertEquals("Name candidate name type should be ORG when specified", DeniedPartyConstants.ScreeningNameTypes.Organization, nameCandidate.NameType);
		}

		public void TestBizoCandidateMaxLengthMapping()
		{
			var creator = new DpsCandidateCreator();
			var orgHeader = InitBizoWithMaxLengthColumns<OrgHeader>(OrgHeaderSchema.Instance);
			var orgBrandOrRelatedName = InitBizoWithMaxLengthColumns<OrgBrandOrRelatedName>(OrgBrandOrRelatedNameSchema.Instance);
			var address = InitBizoWithMaxLengthColumns<OrgAddress>(OrgAddressSchema.Instance);
			var orgCusCode = InitBizoWithMaxLengthColumns<OrgCusCode>(OrgCusCodeSchema.Instance);

			orgBrandOrRelatedName.P1_OH = orgHeader.PK;
			address.OA_OH = orgHeader.PK;
			orgCusCode.OK_OH = orgHeader.PK;

			orgHeader.CustomsCodes.Reload(true);
			var header = creator.NewRequestHeader(orgHeader);
			var errorMessage = ValidateDpsRequestHeader(header);
			AssertNullOrEmpty(errorMessage);

			var refCountry = InitBizoWithMaxLengthColumns<RefCountry>(RefCountrySchema.Instance);
			header = creator.NewRequestHeader(refCountry);
			errorMessage = ValidateDpsRequestHeader(header);
			AssertNullOrEmpty(errorMessage);

			var refVessel = InitBizoWithMaxLengthColumns<RefVessel>(RefVesselSchema.Instance);
			header = creator.NewRequestHeader(refVessel);
			errorMessage = ValidateDpsRequestHeader(header);
			AssertNullOrEmpty(errorMessage);
		}

		string ValidateDpsRequestHeader(DpsRequestHeaderWithAddressMatching header)
		{
			var result = new StringBuilder();
			if (header != null)
			{
				var addresses = header.DpsAddressCandidates;
				if (addresses != null)
				{
					foreach (var address in addresses)
					{
						result.AppendLine(Validate(address));
					}
				}

				var names = header.DpsNameCandidates;
				if (names != null)
				{
					foreach (var name in names)
					{
						result.AppendLine(Validate(name));
					}
				}

				var regCodes = header.DpsRegistrationCodeCandidates;
				if (names != null)
				{
					foreach (var regCode in regCodes)
					{
						result.AppendLine(Validate(regCode));
					}
				}
			}

			return result.ToString().Trim();
		}

		T InitBizoWithMaxLengthColumns<T>(Schema schema) where T : BusinessObject
		{
			var bizo = Factory.NewWithValidTestData<T>();
			var bizoRow = (bizo as IBusinessObjectInternals).Row;

			var properties = schema.GetType().GetFields();
			properties.ForEach(o =>
			{
				if (o.GetValue(schema) is SchemaStringColumn stringColumn && !stringColumn.IsComputed)
				{
					bizoRow[o.Name] = InitMaxLengthString(stringColumn);
				}
			});

			return bizo;
		}

		string InitMaxLengthString(SchemaStringColumn stringColumn)
		{
			var result = new StringBuilder();
			var length = stringColumn.MaxLength;
			if (length != int.MaxValue)
			{
				for (var i = 0; i < length; i++)
				{
					result.Append("*");
				}
			}

			return result.ToString();
		}

		string Validate(object model)
		{
			var result = new StringBuilder();
			var validationContext = new ValidationContext(model, null, null);
			var validationResults = new List<ValidationResult>();
			Validator.TryValidateObject(model, validationContext, validationResults, true);
			foreach (var validationResult in validationResults)
			{
				result.AppendLine(validationResult.ErrorMessage);
			}

			return result.ToString().Trim();
		}

		public void TestNewCandidates_OrgHeader()
		{
			var creator = new DpsCandidateCreator();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "This is a name with more than 50 characters in length";
			orgHeader.MainAddress.OA_Address1 = "A";
			orgHeader.MainAddress.OA_Address2 = "B";
			orgHeader.MainAddress.OA_City = "C";
			orgHeader.MainAddress.OA_State = "D";
			orgHeader.MainAddress.OA_PostCode = "E";
			orgHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			orgHeader.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "F";
			var brand1 = orgHeader.BrandsOrRelatedNames.AddNew();
			brand1.P1_RelatedName = "ABC";
			var brand2 = orgHeader.BrandsOrRelatedNames.AddNew();
			brand2.P1_RelatedName = "BBB";
			var address3 = orgHeader.Addresses.AddNew();
			address3.Address1 = "AB";
			address3.OA_CompanyNameOverride = "CCC";
			var address4 = orgHeader.Addresses.AddNew();
			address4.Address1 = "AB";
			address4.OA_CompanyNameOverride = "This is another name with more than 50 characters in length";
			var regCode1 = orgHeader.CustomsCodes.AddNew();
			regCode1.OK_RN_NKCodeCountry = "AU";
			regCode1.OK_CodeType = "CCC";
			regCode1.OK_CustomsRegNo = "123";
			var regCode2 = orgHeader.CustomsCodes.AddNew();
			regCode2.OK_CustomsRegNo = "456";
			var regCode3 = orgHeader.CustomsCodes.AddNew();
			regCode3.OK_CustomsRegNo = "456";
			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Peter";

			var dpsRequestHeader = creator.NewRequestHeader(orgHeader);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					("This is a name with more than 50 characters in length", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("ABC", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("BBB", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("CCC", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("This is another name with more than 50 characters in length", DeniedPartyConstants.ScreeningNameTypes.Organization),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					"ABCDEAUF",
					"AB" + address3.OA_RN_NKCountryCode
				},
				dpsRequestHeader.DpsAddressCandidates.Select(u => u.Address1 + u.Address2 + u.City + u.State + u.PostCode + u.Country + u.AdditionalAddressLine));

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					"123AUCCC",
					"456" + regCode2.OK_RN_NKCodeCountry
				},
				dpsRequestHeader.DpsRegistrationCodeCandidates.Select(u => u.RegCodeValue + u.RegCountryCode + u.RegCodeType));
		}

		public void TestNewCandidate_ScreeningNameType_BasedOn_OrganizationCategory()
		{
			var creator = new DpsCandidateCreator();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Company Name";
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			var brand1 = orgHeader.BrandsOrRelatedNames.AddNew();
			brand1.P1_RelatedName = "ABC";
			var brand2 = orgHeader.BrandsOrRelatedNames.AddNew();
			brand2.P1_RelatedName = "BBB";

			var dpsRequestHeader = creator.NewRequestHeader(orgHeader);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					("Company Name", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson),
					("ABC", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson),
					("BBB", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

			orgHeader.OH_Category = OrgConstants.Category.Business;
			dpsRequestHeader = creator.NewRequestHeader(orgHeader);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					("Company Name", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("ABC", DeniedPartyConstants.ScreeningNameTypes.Organization),
					("BBB", DeniedPartyConstants.ScreeningNameTypes.Organization),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
		}

		public void TestNewCandidate_RefCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "IR";
			country.RN_Desc = "Iran";

			var creator = new DpsCandidateCreator();
			var dpsRequestHeader = creator.NewRequestHeader(country);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					"IR"
				},
				dpsRequestHeader.DpsCountryCandidates.Select(u => (u.CountryCode)));
		}

		public void TestNewCandidates_Vessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel Name";
			vessel.RV_RN_NKCountryOfReg = "AU";
			vessel.RV_LloydsNumber = "121212";

			var creator = new DpsCandidateCreator();
			var dpsRequestHeader = creator.NewRequestHeader(vessel);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					("Vessel Name", DeniedPartyConstants.ScreeningNameTypes.Vessel),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

			AssertNull(dpsRequestHeader.DpsAddressCandidates);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					(vessel.RV_LloydsNumber.ToString(), vessel.RV_RN_NKCountryOfReg.ToString(), "IMO")
				},
				dpsRequestHeader.DpsRegistrationCodeCandidates.Select(u => (u.RegCodeValue, u.RegCountryCode, u.RegCodeType)));
		}

		public void TestNewCandidates_NotLinkedVessel()
		{
			var notLinkedVessel = Factory.New<ITransport>();
			var creator = new DpsCandidateCreator();
			var dpsRequestHeader = creator.NewRequestHeader(notLinkedVessel as IScreeningPartyForVessel);

			AssertEquals(1, dpsRequestHeader.DpsNameCandidates.Count());
			AssertNull(dpsRequestHeader.DpsAddressCandidates);
			AssertNull(dpsRequestHeader.DpsRegistrationCodeCandidates);
		}

		public void TestNewCandidates_NaturalPerson()
		{
			var name = "Good King Moggle Mog XII";
			var address1 = "Address 1";
			var address2 = "Address 2";
			var city = "City";
			var postCode = "Postcode";
			var state = "State";
			var country = "Country";
			var additionalAddressLine = "Additional Address Line";

			var creator = new DpsCandidateCreator();
			var dpsRequestHeader = creator.NewRequestHeader(name, address1, address2, city, state, postCode, country, additionalAddressLine);
			var dpsAddressCandidate = dpsRequestHeader.DpsAddressCandidates.Single();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(
				new[]
				{
					("Good King Moggle Mog XII", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

				AssertEquals("Address 1", dpsAddressCandidate.Address1);
				AssertEquals("Address 2", dpsAddressCandidate.Address2);
				AssertEquals("City", dpsAddressCandidate.City);
				AssertEquals("State", dpsAddressCandidate.State);
				AssertEquals("Postcode", dpsAddressCandidate.PostCode);
				AssertEquals("Country", dpsAddressCandidate.Country);
				AssertEquals("Additional Address Line", dpsAddressCandidate.AdditionalAddressLine);
			});
		}

		public void TestNewCandidates_JobDocAddress()
		{
			var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_Contact = "Contact Name";
			docAddress.E2_CompanyName = "Company Name";
			docAddress.E2_Address1 = "Addr1";
			docAddress.E2_Address2 = "Addr2";
			docAddress.E2_City = "Perth";
			docAddress.E2_State = "WA";
			docAddress.E2_Postcode = "6006";
			docAddress.E2_RN_NKCountryCode = "AU";

			var creator = new DpsCandidateCreator();
			var dpsRequestHeader = creator.NewRequestHeader(docAddress);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					("Company Name", DeniedPartyConstants.ScreeningNameTypes.Organization),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

			AssertEquals(1, dpsRequestHeader.DpsAddressCandidates.Count());
			AssertNull(dpsRequestHeader.DpsRegistrationCodeCandidates);

			docAddress.E2_IsResidential = true;
			dpsRequestHeader = creator.NewRequestHeader(docAddress);

			AssertContainsExactElementsInAnyOrder(
				"Distinct Results",
				new[]
				{
					("Company Name", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson),
				},
				dpsRequestHeader.DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
		}

		public void TestNewNameCandidate()
		{
			var candidate = new DpsCandidateCreator().NewNameCandidate("ABC", "PER");
			AssertEquals("ABC", candidate.FullName);
			AssertEquals("PER", candidate.NameType);
		}

		public void TestNewAddressCandidate()
		{
			var candidate = new DpsCandidateCreator().NewAddressCandidate("A", "B", "C", "D", "E", "F", "G");
			AssertEquals("A", candidate.Address1);
			AssertEquals("B", candidate.Address2);
			AssertEquals("C", candidate.City);
			AssertEquals("D", candidate.State);
			AssertEquals("E", candidate.PostCode);
			AssertEquals("F", candidate.Country);
			AssertEquals("G", candidate.AdditionalAddressLine);
		}

		public void TestNewRegistrationCodeCandidate()
		{
			var candidate = new DpsCandidateCreator().NewRegistrationCodeCandidate("AU", "CCC", "123");
			AssertEquals("AU", candidate.RegCountryCode);
			AssertEquals("CCC", candidate.RegCodeType);
			AssertEquals("123", candidate.RegCodeValue);
		}

		public void TestBrandOrRelatedNamesSeparation()
		{
			var separator = new[] { "DBA" };
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, separator))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = "Full DBA Name";

				var name1 = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
				name1.P1_RelatedName = "Brand DBA Name";
				var name2 = Factory.NewWithValidTestData<OrgBrandOrRelatedName>();
				name2.P1_RelatedName = "Brand DBAName";
				orgHeader.BrandsOrRelatedNames.Add(name1);
				orgHeader.BrandsOrRelatedNames.Add(name2);

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						("Full", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("Brand", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("Name", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("Brand DBAName", DeniedPartyConstants.ScreeningNameTypes.Organization),
					},
					new DpsCandidateCreator().NewRequestHeader(orgHeader).DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
			}
		}

		public void TestAddressCompanyDetailsOverrideNamesSeparation()
		{
			var separator = new[] { "DBA" };
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, separator))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = "Full DBA Name";

				var address1 = Factory.New<OrgAddress>();
				address1.OA_CompanyNameOverride = "address DBA 1";
				address1.OA_OH = orgHeader.PK;

				var address2 = Factory.New<OrgAddress>();
				address2.OA_CompanyNameOverride = "address DBA2";
				address2.OA_OH = orgHeader.PK;

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						("Full", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("Name", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("address", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("1", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("address DBA2", DeniedPartyConstants.ScreeningNameTypes.Organization),
					},
					new DpsCandidateCreator().NewRequestHeader(orgHeader).DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
			}
		}

		public void TestGetDpsRequestHeader_WhenFeatureControlsAreSet_ShouldReturnCorrectValues()
		{
			var mockFeatureData = new Mock<IFeatureData>();
			var isAllAddressIncluded = new DpsAddressMatchingProfilesFeatureControlData() { IsAllAddressesIncluded = true };
			var mockFeatureManager = new Mock<IFeatureControlManager>();

			ObjectFactory.Substitute(mockFeatureManager.Object);

			mockFeatureManager.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.DpsAddressMatchingLevelFeature, CancellationToken.None)).Returns(Task.FromResult(mockFeatureData.Object));
			mockFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out isAllAddressIncluded)).Returns(true);
			var candidateCreator = new DpsCandidateCreator();
			var candidateObject = candidateCreator.GetDpsRequestHeader(null, null, null, null);
			AssertEquals(candidateObject.AddressMatchingLevel, "Balanced");
			AssertEquals(candidateObject.IsAddressOnlyScreeningIncluded, true);
			AssertEquals(candidateObject.IsAllAddressesIncluded, true);
		}

		public void TestJobDocAddressNamesSeparation()
		{
			var separator = new[] { "DBA" };
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, separator))
			{
				var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
				docAddress.E2_CompanyName = "Company Name";

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						("Company Name", DeniedPartyConstants.ScreeningNameTypes.Organization)
					},
					new DpsCandidateCreator().NewRequestHeader(docAddress).DpsNameCandidates.Select(u => (u.FullName, u.NameType)));

				docAddress = Factory.NewWithValidTestData<JobDocAddress>();
				docAddress.E2_CompanyName = "Company DBA Name";

				AssertContainsExactElementsInAnyOrder(
					new[]
					{
						("Company", DeniedPartyConstants.ScreeningNameTypes.Organization),
						("Name", DeniedPartyConstants.ScreeningNameTypes.Organization),
					},
					new DpsCandidateCreator().NewRequestHeader(docAddress).DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
			}
		}

		public void TestOrgNameSeparation()
		{
			var defaultRegistryValue = new[] { "C/O", "T/A", "DBA", "O/A", "C/", " A B C " };
			AssertNameSeparation(defaultRegistryValue, "Full Name", OrgConstants.Category.NaturalPersonIndividual, new[] { ("Full Name", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson) });
			AssertNameSeparation(defaultRegistryValue, "Full C/O Name", OrgConstants.Category.NaturalPersonIndividual, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson), ("Name", DeniedPartyConstants.ScreeningNameTypes.NaturalPerson) });

			AssertNameSeparation(defaultRegistryValue, "Full Name", OrgConstants.Category.Business, new[] { ("Full Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full C/O Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full T/A Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full DBA Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full O/A Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full A B C Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full  C/ Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });

			AssertNameSeparation(defaultRegistryValue, "Full C/ NameA DBA NameB", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("NameA", DeniedPartyConstants.ScreeningNameTypes.Organization), ("NameB", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full C/ NameA C/ NameB", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("NameA", DeniedPartyConstants.ScreeningNameTypes.Organization), ("NameB", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "C/", OrgConstants.Category.Business, new[] { ("C/", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full C/ C/O Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("C/O Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full C/    C/O Name", OrgConstants.Category.Business, new[] { ("Full", DeniedPartyConstants.ScreeningNameTypes.Organization), ("Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "  C/ C/O", OrgConstants.Category.Business, new[] { ("C/ C/O", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "  C/  C/O ", OrgConstants.Category.Business, new[] { ("C/  C/O", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, " C/ Full Name", OrgConstants.Category.Business, new[] { ("C/ Full Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, " C/ Full Name C/O ", OrgConstants.Category.Business, new[] { ("C/ Full Name C/O", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full Name C/ ", OrgConstants.Category.Business, new[] { ("Full Name C/", DeniedPartyConstants.ScreeningNameTypes.Organization) });
			AssertNameSeparation(defaultRegistryValue, "Full C/Name", OrgConstants.Category.Business, new[] { ("Full C/Name", DeniedPartyConstants.ScreeningNameTypes.Organization) });

			AssertNameSeparation(Array.Empty<string>(), "Full C/ NameA DBA NameB", OrgConstants.Category.Business, new[] { ("Full C/ NameA DBA NameB", DeniedPartyConstants.ScreeningNameTypes.Organization) });
		}

		#region Implementation

		void AssertNameSeparation(string[] registryValue, string orgName, string orgCategory, (string, string)[] expectedSeparatedNames)
		{
			using (OrganisationsDataRegistry.Instance.NameSeparators.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_FullName = orgName;
				orgHeader.OH_Category = orgCategory;

				AssertContainsExactElementsInAnyOrder(expectedSeparatedNames, new DpsCandidateCreator().NewRequestHeader(orgHeader).DpsNameCandidates.Select(u => (u.FullName, u.NameType)));
			}
		}

		#endregion
	}
}
