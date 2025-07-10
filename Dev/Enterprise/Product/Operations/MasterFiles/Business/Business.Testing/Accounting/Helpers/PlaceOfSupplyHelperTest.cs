using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PlaceOfSupplyHelperTest : TestCaseWithFactory
	{
		public void TestTryConvertToLocation()
		{
			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				var query = new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var state = Factory.LoadTop1<RefCountryStates>(query);

				var location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, state.RW_Code);
				AssertNotNull("State location", location);
				AssertEquals(state.RW_Code, location.Code);
				AssertNotNull("Location State", location.State);
				AssertEquals("Is same State", state.PK, location.State.PK);
				AssertNotNull("Location Country", location.Country);
				AssertEquals("Is Canada", Core.Constants.CountryCodes.Canada, location.Country.Code);
				Assert("Not a Rule", !location.IsLocationRule());

				location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "ONTZ");
				AssertNotNull("Tax Zone location", location);
				AssertEquals("ONTZ", location.Code);
				AssertNotNull(location.Zones);
				AssertNotNull("Tax Zone in Zones", location.Zones.FirstOrDefault(x => x.Code == "ONTZ"));
				AssertNull(location.State);
				AssertNotNull("Location Country", location.Country);
				AssertEquals("Is Canada", Core.Constants.CountryCodes.Canada, location.Country.Code);
				Assert("Not a Rule", !location.IsLocationRule());

				location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "ITBL");
				AssertNotNull("Location Tax Zone not Canada", location);
				AssertNotNull("Tax Zone location", location);
				AssertEquals("ITBL", location.Code);
				AssertNotNull(location.Zones);
				AssertNotNull("Tax Zone in Zones", location.Zones.FirstOrDefault(x => x.Code == "ITBL"));
				AssertNull(location.State);
				AssertNull(location.Country);
				Assert("Not a Rule", !location.IsLocationRule());

				var country = Factory.LoadTop1<RefCountry>(new ZQuery());
				location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, country.RN_Code);
				AssertNotNull("Country location", location);
				AssertEquals(country.RN_Code, location.Code);
				AssertNotNull("Location Country", location.Country);
				AssertNull("Location State", location.State);
				Assert("Not a Rule", !location.IsLocationRule());

				location = PlaceOfSupplyHelper.TryConvertToLocation(GlbCompany.CurrentCompany, "ALX");
				AssertNotNull(location);
				AssertEquals("ALX", location.Code);
				AssertNull(location.State);
				Assert("Is Rule", location.IsLocationRule());
			}
		}

		#region TestIsPlaceOfSupplyEnabled

		public void TestIsPlaceOfSupplyEnabled_ReturnsFalse_ForRegistryDefault()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			Assert("When using current company, place of supply should be disabled by default", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());
			Assert("When using specific company, place of supply should be disabled by default", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company));
		}

		public void TestIsPlaceOfSupplyEnabled_ReturnsFalse_WhenNoTypesEnabledInRegistry()
		{
			var allDisabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in allDisabled)
			{
				item.Bool = false;
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, allDisabled))
			{
				Assert("When using current company, place of supply should be disabled when all registry items are false", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());
			}

			var company = Factory.NewWithValidTestData<GlbCompany>();
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, allDisabled))
			{
				Assert("When using specific company, place of supply should be disabled when all registry items are false", !PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company));
			}
		}

		public void TestIsPlaceOfSupplyEnabled_ReturnsTrue_WhenAnyTypeEnabledInRegistry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var someEnabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in someEnabled)
			{
				item.Bool = false;
			}
			someEnabled[someEnabled.Count - 1].Bool = true;

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, someEnabled))
			{
				Assert("When using current company, place of supply should be enabled when any registry item is true", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());
			}
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, someEnabled))
			{
				Assert("When using specific company, place of supply should be enabled when any registry item is true", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company));
			}

			someEnabled[0].Bool = true;
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, someEnabled))
			{
				Assert("When using current company, place of supply should be enabled when any registry item is true", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled());
			}
			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, someEnabled))
			{
				Assert("When using specific company, place of supply should be enabled when any registry item is true", PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(company));
			}
		}

		#endregion

		#region TestGetEnabledPlaceOfSupplyCodes

		public void TestGetEnabledPlaceOfSupplyCodes()
		{
			var otherCompany = Factory.NewWithValidTestData<GlbCompany>();

			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = false);

			var combinedRegistryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var combinedRegistryValueEnumerable = combinedRegistryValue.OfType<CodeDescriptionBool>();
			combinedRegistryValueEnumerable.ForEach(x => x.Bool = false);

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals("Fallback to the current company. No types enabled", 0, PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes().Length);
			}

			for (int i = 0; i < registryValue.Count; i++)
			{
				registryValue[i].Bool = true;
				combinedRegistryValue[i].Bool = true;

				using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
				{
					var enabledTypes = PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes();
					AssertEquals("Fallback to the current company. Only one type enabled", 1, enabledTypes.Length);
					Assert("Fallback to the current company. Contains enabled type.", enabledTypes.Contains<string>(registryValue[i].Code));
					AssertEquals("Other company has no types enabled", 0, PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(otherCompany).Length);
				}

				using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, combinedRegistryValue))
				{
					var enabledTypes = PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes();
					AssertEquals("Fallback to the current company. Number of type enabled", i + 1, enabledTypes.Length);
					Assert("Fallback to the current company. Contains all enabled type.", combinedRegistryValueEnumerable.Where(x => x.Bool).All(x => enabledTypes.Contains<string>(x.Code)));
					AssertEquals("Other company has no types enabled", 0, PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(otherCompany).Length);
				}

				registryValue[i].Bool = false;
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(otherCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, combinedRegistryValue))
			{
				AssertEquals("Other company has all types enabled", combinedRegistryValue.Count, PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(otherCompany).Length);
				AssertEquals("Current company has no types enabled", 0, PlaceOfSupplyHelper.GetEnabledPlaceOfSupplyCodes(GlbCompany.CurrentCompany).Length);
			}
		}

		#endregion
	}
}
