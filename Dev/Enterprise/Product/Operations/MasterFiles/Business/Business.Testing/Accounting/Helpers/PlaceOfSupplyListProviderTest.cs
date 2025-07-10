using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PlaceOfSupplyListProviderTest : TestCaseWithFactory
	{
		#region PlaceOfSupply

		public void TestIsPlaceOfSupplyApplicable()
		{
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue;
			var posTypeCodes = posTypes.OfType<CodeDescriptionBool>().Select(x => x.Code).ToArray();
			AssertEquals(4, posTypeCodes.Length);

			var posTypesToSet = new CodeDescriptionBoolCollection(posTypes);
			var posTypesToSetEnumerable = posTypesToSet.OfType<CodeDescriptionBool>();

			foreach (var typeCode in posTypeCodes)
			{
				posTypesToSetEnumerable.ForEach(x => x.Bool = false);
				assertPlaceOfSupplyApplicable(false);

				posTypesToSetEnumerable.First(x => x.Code == typeCode).Bool = true;
				assertPlaceOfSupplyApplicable(true);
			}

			posTypesToSetEnumerable.ForEach(x => x.Bool = false);
			foreach (var typeCode in posTypeCodes)
			{
				posTypesToSetEnumerable.First(x => x.Code == typeCode).Bool = true;
				assertPlaceOfSupplyApplicable(true);
			}

			void assertPlaceOfSupplyApplicable(bool expected)
			{
				using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, posTypesToSet))
				{
					AssertEquals($"Place of Supply is {(expected ? "applicable" : "not applicable")} for the current company passed explicitly", expected, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany));
					AssertEquals($"Place of Supply is {(expected ? "applicable" : "not applicable")} for the current company when null is passed", expected, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(null));
				}
			}

			var otherCompany = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, Env.CurrentCompanyPK)).FirstOrDefault(x => x.GC_IsActive);
			AssertNotNull(otherCompany);

			posTypesToSetEnumerable.ForEach(x => x.Bool = false);
			var posTypesToSetOtherCompany = new CodeDescriptionBoolCollection(posTypes);
			posTypesToSetOtherCompany[0].Bool = true;

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, posTypesToSet))
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSetOtherCompany))
			{
				AssertEquals("Not applicable to Current company", false, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(null));
				AssertEquals("Applicable to other company", true, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(otherCompany));
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, posTypesToSetOtherCompany))
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
			{
				AssertEquals("Applicable to Current company", true, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(null));
				AssertEquals("Not applicable to other company", false, PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(otherCompany));
			}
		}

		public void TestGetCurrentCompanyPlaceOfSupplyList()
		{
			var currentCompanyPlacesOfSupply = PlaceOfSupplyListProvider.GetCurrentCompanyPlaceOfSupplyList();
			AssertNotNull("GetCurrentCompanyPlaceOfSupplyList should not return null", currentCompanyPlacesOfSupply);
			AssertEquals("No elements returned for default company", 0, currentCompanyPlacesOfSupply.Count);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				AssertPlaceOfSupplyListForCompany(GlbCompany.CurrentCompany, () => PlaceOfSupplyListProvider.GetCurrentCompanyPlaceOfSupplyList());
			}
		}

		public void TestGetPlaceOfSupplyList()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			AssertPlaceOfSupplyListForCompany(company, () => PlaceOfSupplyListProvider.GetPlaceOfSupplyList(company));
		}

		void AssertPlaceOfSupplyListForCompany(GlbCompany company, Func<ReadOnlyCodeDescriptionPairList> listGetter)
		{
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue;
			var posTypesToSet = new CodeDescriptionBoolCollection(posTypes);
			var posTypesToSetEnumerable = posTypesToSet.OfType<CodeDescriptionBool>();

			posTypesToSetEnumerable.ForEach(x => x.Bool = false);
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
			{
				AssertEquals("No places of supply", 0, listGetter().Count);
			}

			posTypesToSetEnumerable.ForEach(x => x.Bool = true);
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
			{
				Assert("Plenty of places of supply", listGetter().Count > 0);

				var miscellaneousCodes = new[]
				{
					PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry,
					PlaceOfSupplyListProvider.Codes.OtherTerritories,
				};

				var expectedCount = miscellaneousCodes.Length;

				var states = Factory.Load<RefCountryStates>(new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, Core.Constants.CountryCodes.Canada));
				expectedCount += states.Length;
				var stateCodes = new HashSet<ZString>(states.Select(state => state.RW_Code));

				var zoneFilter = new ZQuery(RefZoneHeaderSchema.FZ_ZoneType, RefZoneHeaderLookups.ZoneTypeCodes.Tax);
				zoneFilter.AddToFilter(RefZoneHeaderSchema.FZ_IsActive, true);
				var zones = new RefZoneHeaderCollection(Factory, zoneFilter);
				expectedCount += zones.Count;

				var countries = Factory.Load<RefCountry>(new ZQuery());
				var duplicateCodes = countries.Where(country => stateCodes.Contains(country.Code)).Select(x => x.Code).ToHashSet();
				Assert("Has some duplicates", duplicateCodes.Any());
				expectedCount += countries.Length - duplicateCodes.Count;

				company.Factory.ResetDatabaseLoadCount();
				AssertEquals(expectedCount, listGetter().Count);

				foreach (var code in duplicateCodes)
				{
					var stateDescription = states.First(x => x.RW_Code == code).RW_DescriptionMultilingual;
					var posDescription = listGetter().GetDescriptionFromCode(code);
					AssertEquals("Must be description from State", stateDescription, posDescription);
				}

				AssertEquals("PlaceOfSupplyList is cached", 0, company.Factory.DatabaseLoadCount);
			}
		}

		#endregion

		#region PlaceOfSupplyType

		public void TestGetCurrentCompanyPlaceOfSupplyTypeList()
		{
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue;
			var posTypeCodes = posTypes.OfType<CodeDescriptionBool>().Select(x => x.Code).ToArray();
			AssertEquals(4, posTypeCodes.Length);

			var posTypesToSet = new CodeDescriptionBoolCollection(posTypes);
			var posTypesToSetEnumerable = posTypesToSet.OfType<CodeDescriptionBool>();
			var expectedCodes = new List<string>();

			foreach (var typeCode in posTypeCodes)
			{
				posTypesToSetEnumerable.First(x => x.Code == typeCode).Bool = true;
				expectedCodes.Add(typeCode);
				assertPlaceOfSupplyTypeCodesForCurrentCompany();
			}

			void assertPlaceOfSupplyTypeCodesForCurrentCompany()
			{
				using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, posTypesToSet))
				{
					var returnedTypes = PlaceOfSupplyListProvider.GetCurrentCompanyPlaceOfSupplyTypeList();
					AssertArrayEqualsByElements("Expected POS Type codes should be returned", expectedCodes.ToArray(), returnedTypes.OfType<CodeDescriptionPair>().Select(x => x.Code).ToArray());
				}
			}
		}

		public void TestGetPlaceOfSupplyTypeList()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue;
			var posTypeCodes = posTypes.OfType<CodeDescriptionBool>().Select(x => x.Code).ToArray();
			AssertEquals(4, posTypeCodes.Length);

			var posTypesToSet = new CodeDescriptionBoolCollection(posTypes);
			var posTypesToSetEnumerable = posTypesToSet.OfType<CodeDescriptionBool>();
			var expectedCodes = new List<string>();

			foreach (var typeCode in posTypeCodes)
			{
				posTypesToSetEnumerable.First(x => x.Code == typeCode).Bool = true;
				expectedCodes.Add(typeCode);
				assertPlaceOfSupplyTypeCodesForCurrentCompany();
			}

			void assertPlaceOfSupplyTypeCodesForCurrentCompany()
			{
				using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
				{
					var returnedTypes = PlaceOfSupplyListProvider.GetPlaceOfSupplyTypeList(company);
					AssertArrayEqualsByElements("Expected POS Type codes should be returned", expectedCodes.ToArray(), returnedTypes.OfType<CodeDescriptionPair>().Select(x => x.Code).ToArray());
				}
			}
		}

		public void TestGetPlaceTypeFromPlaceCode()
		{
			var posTypes = AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue;
			var posTypesToSet = new CodeDescriptionBoolCollection(posTypes);
			var posTypesToSetEnumerable = posTypesToSet.OfType<CodeDescriptionBool>();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;

			posTypesToSetEnumerable.ForEach(x => x.Bool = false);
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
			{
				AssertEquals("GetPlaceTypeFromPlaceCode for empty place", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, ""));
				AssertEquals("GetPlaceTypeFromPlaceCode for ON", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "ON"));
				AssertEquals("GetPlaceTypeFromPlaceCode for ONTZ", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "ONTZ"));
				AssertEquals("GetPlaceTypeFromPlaceCode for US", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "US"));
				AssertEquals("GetPlaceTypeFromPlaceCode for OutsideTheLoginCountry", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry));
				AssertEquals("GetPlaceTypeFromPlaceCode for NL, which is code for State and Country both", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "NL"));
			}

			posTypesToSetEnumerable.ForEach(x => x.Bool = true);
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, posTypesToSet))
			{
				AssertEquals("GetPlaceTypeFromPlaceCode for empty place", "", PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, ""));
				AssertEquals("GetPlaceTypeFromPlaceCode for ON", PlaceOfSupplyTypes.State.Code, PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "ON"));
				AssertEquals("GetPlaceTypeFromPlaceCode for ONTZ", PlaceOfSupplyTypes.TaxZone.Code, PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "ONTZ"));
				AssertEquals("GetPlaceTypeFromPlaceCode for US", PlaceOfSupplyTypes.Country.Code, PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "US"));
				AssertEquals("GetPlaceTypeFromPlaceCode for OutsideTheLoginCountry", PlaceOfSupplyTypes.PredefinedRule.Code, PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry));
				AssertEquals("GetPlaceTypeFromPlaceCode for NL, which is code for State and Country both. State takes priority.", PlaceOfSupplyTypes.State.Code, PlaceOfSupplyListProvider.GetPlaceTypeFromPlaceCode(company, "NL"));
			}
		}

		#endregion
	}
}
