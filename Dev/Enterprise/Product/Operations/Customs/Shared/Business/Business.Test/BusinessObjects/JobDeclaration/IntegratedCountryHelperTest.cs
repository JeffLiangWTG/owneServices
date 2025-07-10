using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using static Enterprise.Core.Constants.CountryCodes;

namespace Enterprise.Customs.Business.Testing
{
	sealed class IntegratedCountryHelperTest : TestCaseWithFactory
	{
		public void TestIsInterfaceEnabledCompany()
		{
			CombineAssertions(() =>
			{
				var companyPK = GlbCompany.CurrentCompany.PK;
				AssertEquals("Australia", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Australia));
				AssertEquals("Belgium", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Belgium));
				AssertEquals("China", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, China));
				AssertEquals("Denmark", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Denmark));
				AssertEquals("Germany", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Germany));
				AssertEquals("Ireland", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Ireland));
				AssertEquals("Italy", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Italy));
				AssertEquals("Netherlands", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Netherlands));
				AssertEquals("New Zealand", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, NewZealand));
				AssertEquals("Norway", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Norway));
				AssertEquals("Poland", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Poland));
				AssertEquals("South Africa", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, SouthAfrica));
				AssertEquals("Spain", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Spain));
				AssertEquals("Sweden", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Sweden));
				AssertEquals("Switzerland", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Switzerland));
				AssertEquals("Turkey", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, Turkey));
				AssertEquals("United Arab Emirates", true, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, UnitedArabEmirates));
				AssertEquals("United Kingdom", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, UnitedKingdom));
				AssertEquals("United States", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, UnitedStates));
				AssertEquals("Puerto Rico", false, IntegratedCountryHelper.IsInterfaceEnabledCompany(companyPK, PuertoRico));
			});
		}

		public void TestIsCustomsWareInstallations()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Belgium", true, IntegratedCountryHelper.CustomsWareInstallations(Belgium));
				AssertEquals("China", false, IntegratedCountryHelper.CustomsWareInstallations(China));
				AssertEquals("Germany", false, IntegratedCountryHelper.CustomsWareInstallations(Germany));
				AssertEquals("Ireland", true, IntegratedCountryHelper.CustomsWareInstallations(Ireland));
				AssertEquals("Italy", false, IntegratedCountryHelper.CustomsWareInstallations(Italy));
				AssertEquals("Netherlands", false, IntegratedCountryHelper.CustomsWareInstallations(Netherlands));
				AssertEquals("New Zealand", false, IntegratedCountryHelper.CustomsWareInstallations(NewZealand));
				AssertEquals("South Africa", false, IntegratedCountryHelper.CustomsWareInstallations(SouthAfrica));
				AssertEquals("Sweden", false, IntegratedCountryHelper.CustomsWareInstallations(Sweden));
				AssertEquals("Switzerland", false, IntegratedCountryHelper.CustomsWareInstallations(Switzerland));
				AssertEquals("United Arab Emirates", false, IntegratedCountryHelper.CustomsWareInstallations(UnitedArabEmirates));
			});
		}

		public void TestIsUsedToBeBuiltInOnlyCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Australia", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(Australia));
				AssertEquals("Canada", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(Canada));
				AssertEquals("New Zealand", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(NewZealand));
				AssertEquals("United Kingdom", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(UnitedKingdom));
				AssertEquals("Singapore", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(Singapore));
				AssertEquals("_TemplateCountryName_", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(_TemplateCountryName_));
				AssertEquals("Eritrea", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(Eritrea));
				AssertEquals("Latvia", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(Latvia));
				AssertEquals("United States", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(UnitedStates));
				AssertEquals("Puerto Rico", true, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(PuertoRico));
				AssertEquals("India", false, IntegratedCountryHelper.IsUsedToBeBuiltInOnlyCountry(India));
			});
		}

		public void TestUsedToBeBuiltInOnlyCountryCodes()
		{
			var usedToBeBuiltInOnlyCountryCodes = (ImmutableArray<ZString>)typeof(IntegratedCountryHelper).GetField("UsedToBeBuiltInOnlyCountryCodes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			AssertEquals("Do not modify IntegratedCountryHelper.UsedToBeBuiltInOnlyCountryCodes; no new country should be added to this list", "AI, AU, CA, ER, GB, LV, NZ, PR, SG, US", string.Join(", ", usedToBeBuiltInOnlyCountryCodes.OrderBy(x => x)));
		}

		public void TestHasBuiltInDeclarationCountryCodes()
		{
			var actualCodes = string.Join(", ", IntegratedCountryHelper.HasBuiltInDeclarationCountryCodes.OrderBy(x => x));
			var expectedCodes = string.Join(", ", new[]
			{
				_TemplateCountryName_,
				Australia,
				Canada,
				France,
				FrenchGuyana,
				Germany,
				Guadeloupe,
				Martinique,
				Mayotte,
				NewZealand,
				PuertoRico,
				Reunion,
				SaintBarthelemy,
				SaintMartin,
				Singapore,
				SouthAfrica,
				Taiwan,
				UnitedKingdom,
				UnitedStates
			}.OrderBy(x => x));

			AssertEquals(expectedCodes, actualCodes);
		}

		public void TestHasDeclarationInDevelopmentCountryCodes()
		{
			var actualCodes = string.Join(", ", IntegratedCountryHelper.HasDeclarationInDevelopmentCountryCodes.OrderBy(x => x));
			var expectedCodes = string.Join(", ", new[]
			{
				Belgium,
				Brazil,
				China,
				Denmark,
				India,
				Ireland,
				Israel,
				Italy,
				Japan,
				KoreaSouth,
				Mexico,
				Netherlands,
				Norway,
				Poland,
				Spain,
				Sweden,
				Switzerland,
				Turkey,
				UnitedArabEmirates
			}.OrderBy(x => x));

			AssertEquals(expectedCodes, actualCodes);
		}

		public void TestCountryHasDeclarationInDevelopment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("South Africa is not in development", false, IntegratedCountryHelper.CountryHasDeclarationInDevelopment(SouthAfrica));
				AssertEquals("Belgium is in development", true, IntegratedCountryHelper.CountryHasDeclarationInDevelopment(Belgium));
			});
		}

		public void TestIsABMInterfaceActivatedByCompany()
		{
			CombineAssertions("IntegratedCountryHelper.CustomsWareInstallations - true", () =>
			{
				AssertEquals("PreCondition:IntegratedCountryHelper.CustomsWareInstallations(BE)", true, IntegratedCountryHelper.CustomsWareInstallations(Core.Constants.CountryCodes.Belgium));
				var beCompany = Factory.New<GlbCompany>();
				beCompany.GC_Code = "BE1";
				beCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
				Factory.Save();

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(beCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
				AssertEquals("ABMInterface activated", true, IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(Core.Constants.CountryCodes.Belgium, beCompany.PK.ToGuid()));

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(beCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "");
				AssertEquals("ABMInterface not activated", false, IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(Core.Constants.CountryCodes.Belgium, beCompany.PK.ToGuid()));
			});

			CombineAssertions("IntegratedCountryHelper.CustomsWareInstallations - false", () =>
			{
				AssertEquals("PreCondition:IntegratedCountryHelper.CustomsWareInstallations(AZ)", false, IntegratedCountryHelper.CustomsWareInstallations(Core.Constants.CountryCodes.Azerbaijan));
				var azCompany = Factory.New<GlbCompany>();
				azCompany.GC_Code = "AZ1";
				azCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Azerbaijan;
				Factory.Save();

				CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(azCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "TEST");
				AssertEquals("ABMInterface activated", false, IntegratedCountryHelper.IsABMInterfaceActivatedByCompany(Core.Constants.CountryCodes.Azerbaijan, azCompany.PK.ToGuid()));
			});
		}

		public void TestIsCustomsInterfaceActivatedByCompany()
		{
			var cnCompany = Factory.New<GlbCompany>();
			cnCompany.GC_Code = "CN1";
			cnCompany.GC_RN_NKCountryCode = "CN";
			Factory.Save();

			CombineAssertions(() =>
			{
				using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.DataType.SuspendValidation())
				{
					AssertEquals("No CustomsInterface", false, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(cnCompany.PK));

					var customsInterface = new LocalCountryCustomsInterface();
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals("Empty CustomsInterface", false, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(cnCompany.PK));
					}

					customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals("CustomsInterface with SubmissionType", false, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(cnCompany.PK));
					}

					customsInterface.RecipientID = "Test";
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals("CustomsInterface with SubmissionType and RecipientID", true, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(cnCompany.PK));
					}

					customsInterface.SubmissionType = ZString.Empty;
					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals("CustomsInterface with RecipientID", true, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(cnCompany.PK));
					}

					using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(cnCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
					{
						AssertEquals("No CustomsInterface for Company", false, IntegratedCountryHelper.IsCustomsInterfaceActivatedByCompany(Factory.New<GlbCompany>().PK));
					}
				}
			});
		}

		public void TestIsCustomsInterfaceActivated()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No CustomsInterface", false, IntegratedCountryHelper.CustomsInterfaceIsActivated(null));

				var customsInterface = new LocalCountryCustomsInterface();
				AssertEquals("Empty CustomsInterface", false, customsInterface.CustomsInterfaceIsActivated());

				customsInterface.SubmissionType = DeclarationApplicationCodeList.Codes.Interfaced;
				AssertEquals("CustomsInterface with SubmissionType", false, customsInterface.CustomsInterfaceIsActivated());

				customsInterface.RecipientID = "Test";
				AssertEquals("CustomsInterface with SubmissionType and RecipientID: 'Test'", true, customsInterface.CustomsInterfaceIsActivated());

				customsInterface.SubmissionType = ZString.Empty;
				AssertEquals("CustomsInterface with RecipientID: 'Test'", true, customsInterface.CustomsInterfaceIsActivated());
			});
		}

		public void TestCountryHasBuiltInDeclaration()
		{
			var expectedResults = new Dictionary<ZString, bool>()
			{
				{ _EUTemplateCountryName_, false },
				{ Australia, true },
				{ Brazil, false },
				{ Canada, true },
				{ China, false },
				{ France, true },
				{ FrenchGuyana, true },
				{ Guadeloupe, true },
				{ Martinique, true },
				{ Mayotte, true },
				{ Reunion, true },
				{ SaintBarthelemy, true },
				{ SaintMartin, true },
				{ Germany, true },
				{ Ireland, false },
				{ Italy, false },
				{ Japan, false },
				{ KoreaSouth, false },
				{ Malaysia, false },
				{ Poland, false },
				{ SouthAfrica, true },
				{ Spain, false },
				{ Sweden, false },
				{ Taiwan, true },
				{ UnitedArabEmirates, false },
				{ UnitedStates, true },
				{ UnitedKingdom, true },
				{ Singapore, true },
				{ Turkey, false },
				{ Norway, false },
			};

			CombineAssertions(() =>
			{
				foreach (var pair in expectedResults)
				{
					AssertEquals(pair.Key, pair.Value, IntegratedCountryHelper.CountryHasBuiltInDeclaration(pair.Key));
				}
			});
		}

		public void TestCountryHasBuiltInDeclaration_AsycudaCustomsCountryCodes()
		{
			SetupAAAsAsycudaCustomsCountry();
			AssertEquals(true, IntegratedCountryHelper.CountryHasBuiltInDeclaration("AA"));
		}

		void SetupAAAsAsycudaCustomsCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, "Asycuda Customs Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, "AA", "AA Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ObjectFactory.Get<Integration.Customs.Shared.IAsycudaCustomsCountryProvider>().ResetCachingForTest();
		}

		public void TestIsInterfaceOnlySupported()
		{
			var countryCode = "AE";
			var mockProvider = new Mock<Integration.Customs.IDeclarationSubmissionProvider>();
			mockProvider.Setup(x => x.IsInterfaceOnlySupported()).Returns(false);

			var mockHandle = new Mock<ObjectHandle>();
			mockHandle.Setup(x => x.GetObject()).Returns(mockProvider.Object);
			var hashTable = new Hashtable
			{
				{ countryCode, mockHandle.Object }
			};

			CombineAssertions(() =>
			{
				Assert("Interface Only", IntegratedCountryHelper.IsInterfaceOnlySupported(countryCode));
				using var substitute = ObjectFactory.Substitute("DeclarationSubmissionProviders", hashTable);

				Assert("Not Interface Only", !IntegratedCountryHelper.IsInterfaceOnlySupported(countryCode));
			});
		}
	}
}
