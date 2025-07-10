using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefLocoMapLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRY_SystemUsage_List()
		{
			var unloco = Factory.New<RefUNLOCO>();
			var locoMap = unloco.RefLocoMaps.AddNew();

			locoMap.RY_RN = Core.Constants.CountryGuids.Australia;
			AssertEquals(typeof(AirSeaMailSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedKingdom;
			AssertEquals(typeof(GBLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.UnitedStates;
			AssertEquals(typeof(USLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Canada;
			AssertEquals(typeof(CALocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.SouthAfrica;
			AssertEquals(0, locoMap.Lookups.RY_SystemUsage_List.Count);

			locoMap.RY_RN = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iceland)).PK;
			AssertEquals(typeof(ISLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Singapore;
			AssertEquals(typeof(SGLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Turkey;
			AssertEquals(typeof(TRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Mexico;
			AssertEquals(typeof(MXLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Argentina;
			AssertEquals(typeof(ARLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.France;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.FrenchGuiana;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.FrenchGuyana;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.FrenchPolynesia;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.FrenchPolynesia;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Guadeloupe;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Guadeloupe;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Martinique;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Martinique;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Mayotte;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Mayotte;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.NewCaledonia;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.NewCaledonia;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.Reunion;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Reunion;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.SaintBarthelemy;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SaintBarthelemy;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.SaintMartin;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.SaintMartin;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.SaintPierreandMiquelon;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.StPierreEtMiquelon;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.WallisandFutuna;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.WallisAndFutunaIslands;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());

			locoMap.RY_RN = Core.Constants.CountryGuids.FrenchSouthernTerritories;
			unloco.RL_RN_NKCountryCode = Core.Constants.CountryCodes.FrenchSouthernTerritories;
			AssertEquals(typeof(FRLocoMapSystemUsageList), locoMap.Lookups.RY_SystemUsage_List.GetType());
		}

		public void TestRY_LocalPortCode_List()
		{
			var locoMap = Factory.New<RefLocoMap>();

			AssertEquals(typeof(FrenchPortSystemCodeList), locoMap.Lookups.RY_LocalPortCode_List.GetType());
		}
	}
}
