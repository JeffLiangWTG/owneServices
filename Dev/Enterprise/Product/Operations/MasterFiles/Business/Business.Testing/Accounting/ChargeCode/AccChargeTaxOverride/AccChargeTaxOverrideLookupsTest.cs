using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTaxOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeTaxOverride taxOverride = testChargeCode.TaxOverrides.AddNew();
			AccChargeTaxOverrideLookups lookups = new AccChargeTaxOverrideLookups(taxOverride);

			AssertEquals("The JobTypeList should contain 'ALL' as the first element", "ALL", lookups.JobTypes[0].Code);
			Assert("The JobType 'ALL' should be JobInvoicingConsumerType", lookups.JobTypes[0] is JobInvoicingConsumerType);
			AssertEquals("The JobTypeList should contain 'SHP'", true, lookups.JobTypes.ContainsCode("SHP"));
			AssertEquals("The JobTypeList should contain 'CLL'", true, lookups.JobTypes.ContainsCode("CLL"));
			AssertEquals("The JobTypeList should contain 'CSH'", true, lookups.JobTypes.ContainsCode("CSH"));
			AssertEquals("The JobTypeList should contain 'BRK'", true, lookups.JobTypes.ContainsCode("BRK"));
			AssertEquals("The JobTypeList should contain 'NJR'", true, lookups.JobTypes.ContainsCode("NJR"));
		}

		public void TestCustomsStatusListMaximumLength_EU()
		{
			var euCountryCodes = ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>()
				.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers();

			var charge = Factory.New<AccChargeCode>();
			Factory.Save();

			foreach (var code in euCountryCodes.Take(2))
			{
				var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, code));
				if (company == null)
				{
					company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_Code = $"D{code}";
					company.GC_RN_NKCountryCode = code;
				}
				var branch = company.Branches.AddNew();
				branch.GB_BranchName = "currentBranch";
				branch.GB_Code = $"T{code}";
				Factory.Save();
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					Assert(euCountryCodes.Contains(Env.CurrentCompany.Country.Code));
					var chargeTaxOverride = charge.TaxOverrides.AddNew();
					var lookups = new AccChargeTaxOverrideLookups(chargeTaxOverride);
					var lookupEntryTypes = lookups.CustomsStatusList.ToArray();
					Assert(lookupEntryTypes.Any());
					AssertEquals(
						"Code length should not exceed max length",
						false,
						lookupEntryTypes.Any(customsStatus => customsStatus.Code.Length > AutoAccChargeTaxOverride.Schema.AO_CustomsStatusMaxLength)
					);
					chargeTaxOverride.Delete();
				}
			}
		}

		public void TestTaxMessagesIsUsingTheCorrectCountryCodeFilter()
		{
			var newFactory = new BusinessObjectFactory();
			var uSCompany = newFactory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			newFactory.Save();

			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "MSG";
			taxMsg.A9_EnglishMsg = "ENG msg";
			taxMsg.A9_LocalMsg = "Local msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = uSCompany.PK;
			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_TransportMode = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_HomeCountryOrZone = Core.Constants.CountryCodes.UnitedStates;
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "COD";
			taxOverride.AO_AT = taxRate.PK;
			Factory.Save();

			var lookup = new AccChargeTaxOverrideLookups(taxOverride);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				Assert("The TaxMessage should be found if the TaxRate country and message country are the same", lookup.DefaultVATClasses.Contains(taxMsg));
				chargeCode.AC_GC = ZGuid.Empty;
				Assert("The TaxMessage should not be found if the chargeCode company is null and the Current Company is different from the message country", !lookup.DefaultVATClasses.Contains(taxMsg));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Assert("The TaxMessage should be found if the chargeCode company is null and the Current Company and message country are the same", lookup.DefaultVATClasses.Contains(taxMsg));
			}
		}

		public void TestLocationsList()
		{
			RefCountryCollection countriesCollection = new RefCountryCollection(Factory);
			EconomicGroupList economicGroupList = new EconomicGroupList();
			AccChargeTaxOverride taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			AssertEquals("The location list should contain all countries codes, European Union = EUN, ALL and tax Zones (at least one default QUBC zone)", countriesCollection.Count + 14, taxOverride.Lookups.Locations.Count);
			AssertEquals("The location list should contain all countries codes, all economic group codes, ALL and tax Zones (at least one default QUBC zone)", countriesCollection.Count + economicGroupList.Count + 10, taxOverride.Lookups.Locations.Count);
			RefZoneHeader newTaxZone = Factory.New<RefZoneHeader>();
			newTaxZone.FZ_Code = "TAXZ";
			newTaxZone.FZ_Description = "Tax Zone";
			newTaxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			RefZoneHeader newRatZone = Factory.New<RefZoneHeader>();
			newRatZone.FZ_Code = "RATZ";
			newRatZone.FZ_Description = "Rat Zone";
			newRatZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Rating;

			Factory.Save();

			AccChargeTaxOverrideLookups lookups = new AccChargeTaxOverrideLookups(taxOverride);

			int expectedCount = countriesCollection.Count + economicGroupList.Count + 11;
			AssertEquals("Unexpected number of locations.", expectedCount, lookups.Locations.Count);

			// Validate expected tax zone codes in sequence
			string[] expectedCodes = { "BCTZ", "HSTC", "ITBL", "ONTZ", "QUBC", "TAXZ" };
			for (int i = 0; i < expectedCodes.Length; i++)
			{
				int index = economicGroupList.Count + 5 + i;
				AssertEquals($"Expected {expectedCodes[i]} at position {index}.", expectedCodes[i], lookups.Locations[index].Code);
			}
		}

		public void TestTaxRegistrationLocations()
		{
			var sortedCountryCodes = new RefCountryCollection(Factory).Select(c => c.RN_Code).OrderBy(c => c).ToArray();
			var expectedLocations = new List<ZString>
			{
				(ZString)AccChargeTaxOverride.ALL,
				(ZString)EconomicGroupList.Codes.ASEAN,
				(ZString)EconomicGroupList.Codes.NAFTA,
				(ZString)AccChargeTaxOverride.EuropeanUnion,
				(ZString)EconomicGroupList.Codes.BLNS,
				(ZString)AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry,
				OrgCusCode.CodeTypes.SpecialEconomicZone
			};
			expectedLocations.AddRange(sortedCountryCodes);

			// Act
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();

			// Assert
			var message = "The location list should contain all Region Codes (i.e. EU), SEZ, ALL and countries";
			var actualLocations = taxOverride.Lookups.TaxRegistrationLocations.GetAllCodes();
			AssertContainsExactElementsInExactOrder(message, expectedLocations, actualLocations);

			// Arrange
			var taxZone = Factory.New<RefZoneHeader>();
			taxZone.FZ_Code = "TAXZ";
			taxZone.FZ_Description = "Tax Zone";
			taxZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Tax;

			Factory.Save();

			// Act
			var lookups = new AccChargeTaxOverrideLookups(taxOverride);

			// Assert
			message = "Tax Registration locations does not take into consideration tax zones";
			actualLocations = lookups.TaxRegistrationLocations.GetAllCodes();
			AssertContainsExactElementsInExactOrder(message, expectedLocations, actualLocations);
		}

		public void TestCustomsStatusList()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeTaxOverride taxOverride = testChargeCode.TaxOverrides.AddNew();
			AccChargeTaxOverrideLookups lookups = new AccChargeTaxOverrideLookups(taxOverride);
			var customsStatusListInAU = new string[] {
				"C","CAN","CCN","EXDC","EXDD","EXLV","EXML","EXPE","EXSP","EXTI","F","T","T1","T2","T2F","T2L","T2LF","T2LSM","T2SM","TD","TF","X"
			};
			var customsStatusListInDE = new string[] {
					"ATA","C","EXP","F","IMP","MRN","MUC","PMT","T","T1","T2","T2F","T2L","T2LF","T2LSM","T2SM","TD","TF","TSN","UCR","X"
				};
			var customsStatusListNotInAU = customsStatusListInDE.Except(customsStatusListInAU);
			var customsStatusListNotInDE = customsStatusListInAU.Except(customsStatusListInDE);

			CombineAssertions(() =>
			{
				foreach (var code in customsStatusListInAU)
				{
					AssertEquals(true, lookups.CustomsStatusList.ContainsCode(code));
				}
			});
			CombineAssertions(() =>
			{
				foreach (var code in customsStatusListNotInAU)
				{
					AssertEquals(false, lookups.CustomsStatusList.ContainsCode(code));
				}
			});

			RefCountry countryDE = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				taxOverride.ChargeCode.Company.GC_RN_NKCountryCode = countryDE.Code;
				lookups = new AccChargeTaxOverrideLookups(taxOverride);
				CombineAssertions(() =>
				{
					foreach (var code in customsStatusListInDE)
					{
						AssertEquals(true, lookups.CustomsStatusList.ContainsCode(code));
					}
				});
				CombineAssertions(() =>
				{
					foreach (var code in customsStatusListNotInDE)
					{
						AssertEquals(false, lookups.CustomsStatusList.ContainsCode(code));
					}
				});
			}
			finally
			{
				taxOverride.ChargeCode.Company.GC_RN_NKCountryCode = originalCountry;
			}
		}

		public void TestOrganisationCategoryList()
		{
			AccChargeCode testChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeTaxOverride taxOverride = testChargeCode.TaxOverrides.AddNew();
			AccChargeTaxOverrideLookups lookups = new AccChargeTaxOverrideLookups(taxOverride);
			AssertEquals(5, lookups.OrganisationCategoryList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ALL", "BUS", "GOV", "NAT", "NGO" }, lookups.OrganisationCategoryList.GetAllCodes());
		}

		public void TestDefaultingRuleList()
		{
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var lookups = new AccChargeTaxOverrideLookups(taxOverride);
			AssertEquals(3, lookups.DefaultingRuleList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ART", "NON", "SUM" }, lookups.DefaultingRuleList.GetAllCodes());
		}

		public void TestTransactionContextList()
		{
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var lookups = new AccChargeTaxOverrideLookups(taxOverride);
			AssertEquals(3, lookups.TransactionContextList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "ALL", "INT", "STD" }, lookups.TransactionContextList.GetAllCodes());
		}

		public void TestDebtorRoleList()
		{
			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var lookups = new AccChargeTaxOverrideLookups(taxOverride);
			AssertEquals(3, lookups.DebtorRoleList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "NOW", "AGT", "NCX" }, lookups.DebtorRoleList.GetAllCodes());
		}

		void CreateAccTaxRate()
		{
			var rate = Factory.NewWithValidTestData<AccTaxRate>();
			rate.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other";

			var rate2 = Factory.NewWithValidTestData<AccTaxRate>();
			rate2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			rate2.AT_Code = "BBCXYZ";
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_TaxSystemCode = "Other1";

			Factory.Save();
		}

		public void TestTaxRates_TaxFrameworkRelated_FilterByTaxConfigurationTaxSystem()
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			var taxRate1 = AccountingTestObjectCreator.CreateTaxRate("AABBCC", "TESTSYS");

			var taxRate2 = AccountingTestObjectCreator.CreateTaxRate("BBCXYZ");

			var taxRate3 = AccountingTestObjectCreator.CreateTaxRate("AABBZZ", "TEST");

			Factory.Save();

			Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);

			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxOverride.AO_ParentID = taxOverrideGroup.PK;
			taxOverride.AO_ParentTableCode = AccTaxOverrideGroupSchema.Constants.Prefix;
			var taxOverrideGroupPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			AssertNull("Precondition:", taxOverrideGroupPivot.TaxConfiguration);

			var taxRatesCollection = taxOverride.Lookups.TaxRates;
			var expectedCollectionItems = new[] { taxRate1, taxRate3 };
			taxRatesCollection.Load();
			AssertEquals(2, taxRatesCollection.Count);
			AssertContainsExactElementsInAnyOrder(expectedCollectionItems, taxRatesCollection);

			taxOverrideGroupPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			AssertNotNull("Precondition:", taxOverrideGroupPivot.TaxConfiguration);

			taxRatesCollection = taxOverride.Lookups.TaxRates;
			expectedCollectionItems = new[] { taxRate3 };
			taxRatesCollection.Load();
			AssertEquals(1, taxRatesCollection.Count);
			AssertContainsExactElementsInAnyOrder(expectedCollectionItems, taxRatesCollection);
		}

		public void TestAccChargeTaxOverrideTaxRate()
		{
			CreateAccTaxRate();

			Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);

			var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxOverride.AO_ParentID = taxOverrideGroup.PK;
			taxOverride.AO_ParentTableCode = AccTaxOverrideGroupSchema.Constants.Prefix;
			var lookupFilter = taxOverride.Lookups.TaxRates.CompleteFilter;
			var taxRates = Factory.Load<AccTaxRate>(lookupFilter);
			Assert(taxRates.Length > 0);
			Assert("Collection should present only the VAT Tax System", taxRates.Cast<AccTaxRate>().All(item => !item.AT_TaxSystemCode.IsEmpty));

			Factory.RemoveContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);

			var taxOverride1 = Factory.NewWithValidTestData<AccChargeTaxOverride>();
			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup1.AX_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxOverride1.AO_ParentID = taxOverrideGroup1.PK;
			taxOverride1.AO_ParentTableCode = AccTaxOverrideGroupSchema.Constants.Prefix;
			var lookupFilter1 = taxOverride1.Lookups.TaxRates.CompleteFilter;
			var taxRates1 = Factory.Load<AccTaxRate>(lookupFilter1);
			Assert(taxRates1.Length > 0);
			Assert("Collection should present only the VAT Tax System", taxRates1.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestAccChargeCodeTaxOverrideTaxRate()
		{
			CreateAccTaxRate();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			chargeCode.AC_GC = newCompany.PK;
			var taxOverride = chargeCode.TaxOverrides.AddNew();

			var lookupFilter = taxOverride.Lookups.TaxRates.CompleteFilter;
			var taxRates = Factory.Load<AccTaxRate>(lookupFilter);

			Assert(taxRates.Length > 0);
			Assert("Collection should present only the VAT Tax System", taxRates.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestAccChargeCodeTaxOverrideTaxRateWhenSetBusinessContext()
		{
			CreateAccTaxRate();

			Factory.SetContext(AccTaxOverrideGroup.BusinessContext.TaxFramework);

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			chargeCode.AC_GC = newCompany.PK;
			var taxOverride = chargeCode.TaxOverrides.AddNew();

			var lookupFilter = taxOverride.Lookups.TaxRates.CompleteFilter;
			var taxRates = Factory.Load<AccTaxRate>(lookupFilter);

			Assert(taxRates.Length > 0);
			Assert("Collection should present only the VAT Tax System", taxRates.Cast<AccTaxRate>().All(item => item.AT_TaxSystemCode.IsEmpty));
		}

		public void TestSupplyTypes()
		{
			AssertAllAreEnabled();
			AssertSomeAreDisabled();
			AssertAllAreDisabled();

			void AssertAllAreEnabled()
			{
				var collection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;
				foreach (CodeDescriptionBool code in collection)
				{
					code.Bool = true;
				}
				AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);

				var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
				AssertContainsExactElementsInAnyOrder(
					new string[] { "LOC", "LOX", "LOA", "INT", "INX", "INA", "DSB" },
					taxOverride.Lookups.SupplyTypes.GetAllCodes()
				);
			}

			void AssertSomeAreDisabled()
			{
				var collection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;
				foreach (CodeDescriptionBool code in collection)
				{
					code.Bool = true;
				}
				((CodeDescriptionBool)collection.FindByCode("LOC")).Bool = false;
				((CodeDescriptionBool)collection.FindByCode("INT")).Bool = false;
				AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);

				var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
				AssertContainsExactElementsInAnyOrder(
					new string[] { "LOX", "LOA", "INX", "INA", "DSB" },
					taxOverride.Lookups.SupplyTypes.GetAllCodes()
				);
			}

			void AssertAllAreDisabled()
			{
				var collection = AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.Value;
				foreach (CodeDescriptionBool code in collection)
				{
					code.Bool = false;
				}
				AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesList.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection);

				var taxOverride = Factory.NewWithValidTestData<AccChargeTaxOverride>();
				AssertContainsExactElementsInAnyOrder(
				Array.Empty<string>(),
					taxOverride.Lookups.SupplyTypes.GetAllCodes()
				);
			}
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
