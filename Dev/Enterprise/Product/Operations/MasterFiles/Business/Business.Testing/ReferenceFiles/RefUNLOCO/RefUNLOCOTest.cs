using System;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefUNLOCO))]
	sealed class RefUNLOCOTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountryCode_MacroIgnoreAndObsolete()
		{
			CombineAssertions(() =>
			{
				var propertyInfo = typeof(RefUNLOCO).GetProperty("CountryCode");
				AssertNotNull("MacroIgnoreAttribute", propertyInfo.GetCustomAttribute<MacroIgnoreAttribute>());
				AssertNotNull("ObsoleteAttribute", propertyInfo.GetCustomAttribute<ObsoleteAttribute>());
			});
		}

		public void TestCountryEconomicGroupDescription()
		{
			var riga = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "LVRIX"));
			var newYork = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USNYC"));
			var toronto = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "CAYTO"));
			var singapore = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "SGSIN"));
			var madeUp = Factory.New<RefUNLOCO>();
			madeUp.RL_Code = "GBXXX";
			AssertEquals(EconomicGroupList.Descriptions.EuropeanUnion, riga.CountryEconomicGroupDescription);
			AssertEquals(EconomicGroupList.Descriptions.NAFTA, newYork.CountryEconomicGroupDescription);
			AssertEquals(EconomicGroupList.Descriptions.NAFTA, toronto.CountryEconomicGroupDescription);
			AssertEquals(EconomicGroupList.Descriptions.EuropeanUnion, madeUp.CountryEconomicGroupDescription);
			AssertEquals(EconomicGroupList.Descriptions.ASEAN, singapore.CountryEconomicGroupDescription);
		}

		public void TestIsInEU()
		{
			var riga = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "LVRIX"));
			var sydney = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var madeUp = Factory.New<RefUNLOCO>();
			madeUp.RL_Code = "GBXXX";
			Assert(riga.IsInEU);
			Assert(!sydney.IsInEU);
			Assert(madeUp.IsInEU);
		}

		public void TestIsInIcs2Zone()
		{
			var riga = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "LVRIX"));
			var sydney = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var london = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLHR"));
			var aadorf = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "CHARF"));
			var abelvar = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "NOABV"));
			var madeUp = Factory.New<RefUNLOCO>();
			madeUp.RL_Code = "GBXXX";

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}

			Assert(riga.IsInIcs2Zone);
			Assert(!sydney.IsInIcs2Zone);
			Assert(!madeUp.IsInIcs2Zone);
			Assert(!london.IsInIcs2Zone);
			Assert(abelvar.IsInIcs2Zone);
			Assert(aadorf.IsInIcs2Zone);
			Assert(belfast.IsInIcs2Zone);
		}

		public void TestIsInNorthernIreland()
		{
			var barcelona = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "ESBCN"));

			var chester = new RefUNLOCO.Loader(Factory).Load("GBCEG");
			if (chester.CountryStates == null || string.Compare(chester.CountryStates.RW_RegionName, "ENGLAND", true) != 0)
			{
				var eng = Factory.New<RefCountryStates>();
				chester.RL_RW = eng.PK;
				eng.RW_RegionName = "EngLanD";
			}

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				belfast.RL_RW = ni.PK;
				ni.RW_RegionName = "nOrThErN IRelAnd";
			}

			Assert(!barcelona.IsInGreatBritain);
			Assert(!barcelona.IsInNorthernIreland);
			Assert(chester.IsInGreatBritain);
			Assert(!chester.IsInNorthernIreland);
			Assert(!belfast.IsInGreatBritain);
			Assert(belfast.IsInNorthernIreland);
		}

		public void TestIsInCurrentCountry()
		{
			var riga = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "LVRIX"));
			var sydney = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var london = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBLON"));

			Assert(!riga.IsInCurrentCompanysCountry);
			Assert(sydney.IsInCurrentCompanysCountry);
			Assert(!london.IsInCurrentCompanysCountry);

			var lvCompany = Factory.New<GlbCompany>();
			lvCompany.GC_Code = "DAN";
			lvCompany.GC_RN_NKCountryCode = "LV";
			var rigaBranch = lvCompany.Branches.AddNew();
			rigaBranch.GB_RL_NKHomePort = "LVRIX";
			rigaBranch.GB_Code = "DAN";
			Factory.Save();
			using (DisposableEnvironment.ForBranch(rigaBranch.PK.ToGuid()))
			{
				Assert(riga.IsInCurrentCompanysCountry);
				Assert(!sydney.IsInCurrentCompanysCountry);
				Assert(!london.IsInCurrentCompanysCountry);
			}
		}

		public void TestCurrentBranchCode()
		{
			var refUNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			refUNLOCO.RL_Code = "AUTST";
			Factory.Save();

			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUTST";
			AssertEquals("AUTST", refUNLOCO.CurrentBranchCode);

			refUNLOCO.RL_Code = "CWTST";
			Factory.Save();
			AssertEquals("", refUNLOCO.CurrentBranchCode);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		#region No 4 code RefUNLOCOs

		public void TestNoInvalidRefUNLOCOsInDatabase()
		{
			string sql = "select count(*) from dbo.RefUNLOCO where len(RL_Code) != 5 and RL_IsActive = 1";
			AssertEquals(0, Db.Connection.ExecuteScalar(sql));
		}

		#endregion

		#region On Loaded
		public void TestOnLoadedNonSystemRecord()
		{
			UNLOCO.RL_IsSystem = false;
			GlbStaff.CurrentUser.GS_IsController = false;
			UNLOCO.OnLoaded();
			CheckUNLOCOInfoForNonSystemRecord();

			GlbStaff.CurrentUser.GS_IsController = true;
			UNLOCO.OnLoaded();
			CheckUNLOCOInfoForNonSystemRecord();
		}

		public void TestOnLoadedSystemRecord()
		{
			UNLOCO.RL_IsSystem = true;
			GlbStaff.CurrentUser.GS_IsController = false;
			UNLOCO.OnLoaded();
			Assert("Is a system UNLOCO, property is readonly", UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is readonly", UNLOCO.RL_PortNameInfo.ReadOnly);
			CheckUNLOCOInfoForSystemRecord();

			GlbStaff.CurrentUser.GS_IsController = true;
			UNLOCO.OnLoaded();
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_PortNameInfo.ReadOnly);
			CheckUNLOCOInfoForSystemRecord();
		}

		#endregion

		#region Static Loaders

		public void TestGetPortFromNameAndCountryCode()
		{
			ZString testPortName = "Christchurch";
			ZString testCountryCode = "NZ";
			RefUNLOCO testPort = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, testPortName, testCountryCode);
			AssertNotNull("Christchurch port should NOT be null", testPort);
			AssertEquals("Christchurch port code", "NZCHC", testPort.Code);

			testPortName = "InvalidPortName";
			testPort = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, testPortName, testCountryCode);
			AssertNull("InvalidPortName", testPort);

			// Wollongong should not be found in NZ, so null should be returned.
			testPortName = "Wollongong";
			testPort = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, testPortName, testCountryCode);
			AssertNull("Wollongong port should be null", testPort);

			testCountryCode = "AU";
			testPort = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, testPortName, testCountryCode);
			AssertNotNull("Wollongong port should NOT be null", testPort);
			AssertEquals("Wollongong port code", "AUWOL", testPort.Code);
		}

		public void TestGetPortFromNameAndCountryName()
		{
			ZString testPortName = "Christchurch";
			ZString testCountryName = "New Zealand";
			RefUNLOCO testPort = RefUNLOCO.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Christchurch port should NOT be null", testPort);
			AssertEquals("Christchurch port code", "NZCHC", testPort.Code);

			testPortName = "InvalidPortName";
			testPort = RefUNLOCO.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNull("InvalidPortName", testPort);

			// Wollongong should not be found in NZ, so null should be returned.
			testPortName = "Wollongong";
			testPort = RefUNLOCO.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNull("Wollongong port should be null", testPort);

			testCountryName = "Australia";
			testPort = RefUNLOCO.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Wollongong port should NOT be null", testPort);
			AssertEquals("Wollongong port code", "AUWOL", testPort.Code);

			testPortName = "Hamburg";
			testCountryName = "Germany";
			testPort = RefUNLOCO.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Hamburg port should NOT be null", testPort);
			AssertEquals("Hamburg port code", "DEHAM", testPort.Code);
		}

		public void TestLoadFromLocalMap()
		{
			PopulateRefLocoMap();

			RefUNLOCO refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.UnitedStates, "XYZ");
			AssertEquals("USLAX", refUNLOCO.RL_Code);

			refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.UnitedStates, "AIR");
			AssertNull(refUNLOCO);

			refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.SouthAfrica, "XYZ");
			AssertNull(refUNLOCO);

			refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, "2791", Core.Constants.CountryCodes.UnitedStates, "XYZ");
			AssertNull(refUNLOCO);

			refUNLOCO = RefUNLOCO.LoadFromLocalMap(Factory, "9639", Core.Constants.CountryCodes.Australia, "XYZ");
			AssertEquals("AUSYD", refUNLOCO.RL_Code);
		}

		void PopulateRefLocoMap()
		{
			RefLocoMap uSLocoMap = Factory.New<RefLocoMap>();
			uSLocoMap.RY_LocalPortCode = "2795";
			uSLocoMap.RY_RL_NKLocoPort = "USLAX";
			uSLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "US").PK;
			uSLocoMap.RY_SystemUsage = "XYZ";

			RefLocoMap sYDLocoMap = Factory.New<RefLocoMap>();
			sYDLocoMap.RY_LocalPortCode = "9639";
			sYDLocoMap.RY_RL_NKLocoPort = "AUSYD";
			sYDLocoMap.RY_RN = RefCountry.LoadFromCountryCode(Factory, "AU").PK;
			sYDLocoMap.RY_SystemUsage = "XYZ";

			// Set mandatory fields so it can be saved
			StartRule.R4_DaylightSavingDate = ZDateTime.Now;
			EndRule.R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();
		}

		#endregion

		#region Location Coordinates

		public void TestLocationCoordinates_SyncedBothWays()
		{
			var testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			testLoco1.RL_GeoLocation = new ZGeography("24.46700 24.46700");
			AssertEquals(24.46700m, testLoco1.Latitude);
			AssertEquals(24.46700m, testLoco1.Longitude);
		}

		public void TestLocationCoordinates_NormalCase()
		{
			RefUNLOCO testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			RefUNLOCO testLoco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			testLoco1.Latitude = 12.34567m;
			testLoco1.Longitude = 123.45678m;
			testLoco1.RunPreSaveValidation();

			Assert("Loco latitude should not have errors", !testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should not have errors", !testLoco1.LongitudeInfo.HasErrors());

			testLoco1.Latitude = -12.34567m;
			testLoco1.Longitude = -123.45678m;
			testLoco1.RunPreSaveValidation();

			Assert("Loco latitude should not have errors", !testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should not have errors", !testLoco1.LongitudeInfo.HasErrors());
		}

		public void TestLocationCoordinates_OutOfRangeCase()
		{
			RefUNLOCO testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();
			RefUNLOCO testLoco2 = Factory.NewWithValidTestData<RefUNLOCO>();

			testLoco1.Latitude = 90.00001m;
			testLoco1.Longitude = 180.00001m;
			testLoco1.RunPreSaveValidation();

			Assert("Loco latitude should have errors", testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should have errors", testLoco1.LongitudeInfo.HasErrors());

			testLoco1.Latitude = -90.00001m;
			testLoco1.Longitude = -180.00001m;
			testLoco1.RunPreSaveValidation();

			Assert("Loco latitude should have errors", testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should have errors", testLoco1.LongitudeInfo.HasErrors());
		}

		public void TestLocationCoordinatesAreBeingSavedToDB()
		{
			var testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();

			var guid = testLoco1.PK;

			testLoco1.Latitude = 12.34567m;
			testLoco1.Longitude = 123.45678m;
			testLoco1.RunPreSaveValidation();
			Assert("Loco latitude should not have errors", !testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should not have errors", !testLoco1.LongitudeInfo.HasErrors());

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadedUnloco = anotherFactory.Load<RefUNLOCO>(guid);
			var geoValue = ZGeography.CreatePoint(123.45678, 12.34567);

			AssertEquals("RL_GeoLocation value.", geoValue, loadedUnloco.RL_GeoLocation);
			AssertEquals("Latitude value", loadedUnloco.Latitude, 12.34567m);
			AssertEquals("Longitude value", loadedUnloco.Longitude, 123.45678m);
		}

		public void TestLocationCoordinatesAreBeingSavedToDB_Empty()
		{
			var testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();

			var guid = testLoco1.PK;

			testLoco1.Latitude = 0;
			testLoco1.Longitude = 0;
			testLoco1.RunPreSaveValidation();
			Assert("Loco latitude should not have errors", !testLoco1.LatitudeInfo.HasErrors());
			Assert("Loco longitude should not have errors", !testLoco1.LongitudeInfo.HasErrors());

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var loadedUnloco = anotherFactory.Load<RefUNLOCO>(guid);

			AssertEquals("RL_GeoLocation value should be empty since we take lat/long 0/0 as empty point.", ZGeography.Empty, loadedUnloco.RL_GeoLocation);
			AssertEquals("Latitude value", loadedUnloco.Latitude, 0m);
			AssertEquals("Longitude value", loadedUnloco.Longitude, 0m);
		}

		public void TestCoordinateText()
		{
			var testLoco1 = Factory.NewWithValidTestData<RefUNLOCO>();

			testLoco1.RL_GeoLocation = ZGeography.Empty;
			AssertEquals("Empty Coordinate text.", string.Empty, testLoco1.CoordinateText);

			testLoco1.RL_GeoLocation = ZGeography.CreatePoint(123.123, 12.12);
			AssertEquals("Valid Coordinate text.", "12.12 123.123", testLoco1.CoordinateText);
		}

		#endregion

		#region RefLOCOMap

		public void TestRefLocoMap()
		{
			RefUNLOCO newUNLOCO = Factory.New<RefUNLOCO>();
			newUNLOCO.RL_Code = "CHIPS";
			newUNLOCO.RL_RN_NKCountryCode = "AU";
			newUNLOCO.RL_IsActive = true;
			newUNLOCO.RL_PortName = "California Highway Patrol";
			newUNLOCO.RL_HasAirport = true;
			newUNLOCO.RL_HasSeaport = true;

			RefLocoMap newLocoMap = newUNLOCO.RefLocoMaps.AddNew();
			AssertEquals(newLocoMap.RY_RL_NKLocoPort, "CHIPS");
			AssertEquals("RefLocoMap count", 1, newUNLOCO.RefLocoMaps.Count);
		}

		public void TestLocalMapWhenUnlocoCodeUpdated()
		{
			var newUnloco = Factory.New<RefUNLOCO>();
			newUnloco.RL_Code = "CHIPS";
			newUnloco.RL_RN_NKCountryCode = "AU";
			newUnloco.RL_IsActive = true;
			newUnloco.RL_PortName = "California Highway Patrol";
			newUnloco.RL_HasAirport = true;
			newUnloco.RL_HasSeaport = true;

			var newLocoMap = newUnloco.RefLocoMaps.AddNew();
			AssertEquals("CHIPS", newLocoMap.RY_RL_NKLocoPort);
			AssertEquals("RefLocoMap count", 1, newUnloco.RefLocoMaps.Count);

			newUnloco.RL_Code = "NUTTT";
			AssertEquals("NUTTT", newLocoMap.RY_RL_NKLocoPort);
			AssertEquals("RefLocoMap count", 1, newUnloco.RefLocoMaps.Count);
		}

		#endregion

		#region GetCountryFromCode

		public void TestGetCountryFromCode()
		{
			RefCountry country = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "AU") as RefCountry;
			AssertEquals("Country should be the same, no error expected", UNLOCO.GetCountryFromCode("AU").PK, country.PK);

			country = Factory.LoadFromNaturalKey(typeof(RefCountry), RefCountrySchema.RN_Code, "GB") as RefCountry;
			AssertEquals("Country is invalid code.. wonder what happens?", UNLOCO.GetCountryFromCode("GB").PK, country.PK);
		}

		#endregion

		#region TestLoadFromForeignCode

		public void TestLoadFromForeignCode()
		{
			OrgHeader company = OrgHeader.New(Factory);
			company.OH_Code = "TESTORG";

			RefUNLOCO portWithExCode = Factory.NewWithValidTestData<RefUNLOCO>(TestBusinessObjectKind.MinimumRequiredToSave);
			portWithExCode.RL_Code = "ABABC";
			portWithExCode.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			OrgPatternMatchOverride orgPatternMatch = company.CreatePatternMatchOverrideForTest();
			orgPatternMatch.OO_ForeignCode = "AAA";
			orgPatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			orgPatternMatch.OO_LocalGuid = portWithExCode.PK;

			RefUNLOCO returnedUNLOCO = RefUNLOCO.LoadFromForeignCode(Factory, "AAA", company);
			AssertNotNull(returnedUNLOCO);
			AssertEquals(portWithExCode.RL_Code, returnedUNLOCO.RL_Code);
		}

		#endregion

		#region Appointed / Published Agents

		public void TestGetPublishedAgent()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgAddress aU__AIR_BTH_APP = GetForwarderAgent(newFactory, AU, transportAir, directionBoth, statusAppointed);
			OrgAddress sYD_AIR_BTH_APP = GetForwarderAgent(newFactory, SYD.Code, transportAir, directionBoth, statusAppointed);
			newFactory.Save();
			AssertEquals(null, SYD.GetPublishedAgent(transportAir, directionExport));
			AssertEquals(null, SYD.GetPublishedAgent(transportAir, directionImport));

			OrgAddress aU__AIR_BTH_PUB = GetForwarderAgent(newFactory, AU, transportAir, directionBoth, statusPublished);
			newFactory.Save();
			AssertEquals(aU__AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionExport).PK);
			AssertEquals(aU__AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionImport).PK);

			OrgAddress aU__AIR_EXP_PUB = GetForwarderAgent(newFactory, AU, transportAir, directionExport, statusPublished);
			newFactory.Save();
			AssertEquals(aU__AIR_EXP_PUB.PK, SYD.GetPublishedAgent(transportAir, directionExport).PK);
			AssertEquals(aU__AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionImport).PK);

			OrgAddress sYD_AIR_BTH_PUB = GetForwarderAgent(newFactory, SYD.Code, transportAir, directionBoth, statusPublished);
			newFactory.Save();
			AssertEquals(sYD_AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionExport).PK);
			AssertEquals(sYD_AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionImport).PK);

			OrgAddress sYD_AIR_EXP_PUB = GetForwarderAgent(newFactory, SYD.Code, transportAir, directionExport, statusPublished);
			newFactory.Save();
			AssertEquals(sYD_AIR_EXP_PUB.PK, SYD.GetPublishedAgent(transportAir, directionExport).PK);
			AssertEquals(sYD_AIR_BTH_PUB.PK, SYD.GetPublishedAgent(transportAir, directionImport).PK);
		}

		void AssertBestAgentReturned(string agentStatus, bool shouldBeNull = false)
		{
			var forwarder = GetForwarder(Factory);

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");

			var ausydAddress = forwarder.Addresses.AddNew("1", "1");
			var auAddress = forwarder.Addresses.AddNew("2", "2");
			var uaievImpAddress = forwarder.Addresses.AddNew("3", "3");
			var uaievExpAddress = forwarder.Addresses.AddNew("4", "4");

			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, agentStatus);
			AddForwarderAgent(forwarder, auAddress, "AU", transportAir, directionBoth, agentStatus);
			AddForwarderAgent(forwarder, uaievImpAddress, "UAIEV", transportAir, directionImport, agentStatus);
			AddForwarderAgent(forwarder, uaievExpAddress, "UAIEV", transportAir, directionExport, agentStatus);

			Factory.Save();

			if (shouldBeNull)
			{
				AssertNull(ausyd.GetBestAgent(forwarder, transportAir, directionImport));
				AssertNull(aumel.GetBestAgent(forwarder, transportAir, directionExport));
				AssertNull(uaiev.GetBestAgent(forwarder, transportAir, directionImport));
				AssertNull(uaiev.GetBestAgent(forwarder, transportAir, directionExport));
			}
			else
			{
				AssertEquals(ausydAddress.PK, ausyd.GetBestAgent(forwarder, transportAir, directionImport).PK);
				AssertEquals(auAddress.PK, aumel.GetBestAgent(forwarder, transportAir, directionExport).PK);
				AssertEquals(uaievImpAddress.PK, uaiev.GetBestAgent(forwarder, transportAir, directionImport).PK);
				AssertEquals(uaievExpAddress.PK, uaiev.GetBestAgent(forwarder, transportAir, directionExport).PK);
			}
		}

		public void TestGetBestAgent()
		{
			AssertBestAgentReturned(statusAppointed);
			AssertBestAgentReturned(statusPublished);
			AssertBestAgentReturned(statusHandles, shouldBeNull: true);
			AssertBestAgentReturned(statusGatewayAgent, shouldBeNull: true);
			AssertBestAgentReturned(statusGatewayAgentWithTariff, shouldBeNull: true);
		}

		public void TestGetBestAgent_BothFreightTypesAndGatewayExist()
		{
			var forwarder = GetForwarder(Factory);
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");

			var uaPUBCountryAddress = forwarder.Addresses.AddNew("UAGCA", "UA Published Country Address");
			AddForwarderAgent(forwarder, uaPUBCountryAddress, "UA", transportAir, directionImport, statusPublished);
			Factory.Save();
			AssertEquals(uaPUBCountryAddress.PK, uaiev.GetBestAgent(null, transportAir, directionImport).PK);

			var uaPUBAddress = forwarder.Addresses.AddNew("UAGA", "UA Published Address");
			AddForwarderAgent(forwarder, uaPUBAddress, "UAIEV", transportAir, directionImport, statusPublished);
			Factory.Save();
			AssertEquals(uaPUBAddress.PK, uaiev.GetBestAgent(null, transportAir, directionImport).PK);

			var uaGTAAddress = forwarder.Addresses.AddNew("UAPA", "UA GatewayAgent Address");
			AddForwarderAgent(forwarder, uaGTAAddress, "UAIEV", transportAir, directionImport, statusGatewayAgent);
			Factory.Save();
			AssertEquals("Should not prioritise gateway", uaPUBAddress.PK, uaiev.GetBestAgent(null, transportAir, directionImport).PK);
		}

		public void TestGetBestAgent_GatewayNeedsFreightHandlingToDefault()
		{
			var forwarder = GetForwarder(Factory);
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");

			var uaGTAAddress = forwarder.Addresses.AddNew("UAPA", "UA GatewayAgent Address");
			AddForwarderAgent(forwarder, uaGTAAddress, "UAIEV", transportAir, directionImport, statusGatewayAgent);
			Factory.Save();

			AssertEquals(
				"Should not prioritise gateway without freight handling details",
				null,
				uaiev.GetBestAgent(null, transportAir, directionImport)
			);

			AddForwarderAgent(forwarder, uaGTAAddress, "UAIEV", transportAir, directionImport, statusPublished);
			Factory.Save();

			AssertEquals(uaGTAAddress.PK, uaiev.GetBestAgent(null, transportAir, directionImport).PK);
		}

		public void TestGetBestAgent_NoOrganization()
		{
			var forwarder = GetForwarder(Factory);

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var ausydAddress = forwarder.Addresses.AddNew("1", "1");

			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, AgentStatusList.Codes.Handles);

			Factory.Save();

			CombineAssertions("No handler agent should be found with or without organisation, only PUB/APP should default", () =>
			{
				AssertNull("Organization specified", ausyd.GetBestAgent(forwarder, transportAir, directionImport));
				AssertNull("No Organization specified", ausyd.GetBestAgent(null, transportAir, directionImport));
			});
		}

		public void TestGetBestAgent_WithHandleStatus()
		{
			var forwarder = GetForwarder(Factory);

			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var ausydAddress = forwarder.Addresses.AddNew("1", "1");

			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, AgentStatusList.Codes.Handles);

			Factory.Save();

			CombineAssertions("No handler agent should be found with or without organisation, only PUB/APP (maybe HAN) should default", () =>
			{
				AssertNull("Best agent should be null", ausyd.GetBestAgent(forwarder, transportAir, directionImport));
				AssertEquals("Best agent should be not null", ausydAddress.PK, ausyd.GetBestAgent(forwarder, transportAir, directionImport, true).PK);
			});
		}

		public void TestGetPublishedAgent_NonForwarderOrNonActiveAgents_ReturnNull()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader forwarder = GetForwarder(newFactory);
			OrgAddress aU__AIR_IMP_PUB = AddForwarderAgent(forwarder, AU, transportAir, directionImport, statusPublished);

			newFactory.Save();
			AssertEquals(aU__AIR_IMP_PUB.PK, SYD.GetPublishedAgent(transportAir, directionImport).PK);

			forwarder.OH_IsForwarder = false;
			newFactory.Save();
			AssertNull(SYD.GetPublishedAgent(transportAir, directionImport));

			forwarder.OH_IsActive = false;
			newFactory.Save();
			AssertNull(SYD.GetPublishedAgent(transportAir, directionImport));

			forwarder.OH_IsActive = true;
			newFactory.Save();
			AssertNull(SYD.GetPublishedAgent(transportAir, directionImport));
		}

		public void TestBestAgent_GatewayAgentPriortyFromAppointedAgent()
		{
			var forwarder = GetForwarder(Factory);
			var originalProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			GlbCompany.CurrentCompany.GC_OH_OrgProxyInfo.Value = forwarder.PK;
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");
			var ausydAddress = forwarder.Addresses.AddNew("1", "1");
			var auAddress = forwarder.Addresses.AddNew("2", "2");
			var uaievImpAddress = forwarder.Addresses.AddNew("3", "3");
			var uaievExpAddress = forwarder.Addresses.AddNew("4", "4");
			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, auAddress, "AU", transportAir, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, uaievImpAddress, "UAIEV", transportAir, directionImport, statusAppointed);
			AddForwarderAgent(forwarder, uaievExpAddress, "UAIEV", transportAir, directionExport, statusAppointed);
			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, statusGatewayAgent);
			AddForwarderAgent(forwarder, auAddress, "AU", transportAir, directionBoth, statusGatewayAgent);
			AddForwarderAgent(forwarder, uaievImpAddress, "UAIEV", transportAir, directionImport, statusGatewayAgent);
			AddForwarderAgent(forwarder, uaievExpAddress, "UAIEV", transportAir, directionExport, statusGatewayAgent);
			Factory.Save();
			AssertEquals(ausydAddress.PK, ausyd.GetBestAgent(null, transportAir, directionImport).PK);
			AssertEquals(auAddress.PK, aumel.GetBestAgent(null, transportAir, directionExport).PK);
			AssertEquals(uaievImpAddress.PK, uaiev.GetBestAgent(null, transportAir, directionImport).PK);
			AssertEquals(uaievExpAddress.PK, uaiev.GetBestAgent(null, transportAir, directionExport).PK);
			GlbCompany.CurrentCompany.GC_OH_OrgProxyInfo.Value = originalProxy;
		}

		public void TestBestAgent_AppointedAgentPriortyFromGatewayAgent()
		{
			var forwarder = GetForwarder(Factory);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var uaiev = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "UAIEV");
			var ausydAddress = forwarder.Addresses.AddNew("1", "1");
			var auAddress = forwarder.Addresses.AddNew("2", "2");
			var uaievImpAddress = forwarder.Addresses.AddNew("3", "3");
			var uaievExpAddress = forwarder.Addresses.AddNew("4", "4");
			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, auAddress, "AU", transportAir, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, uaievImpAddress, "UAIEV", transportAir, directionImport, statusAppointed);
			AddForwarderAgent(forwarder, uaievExpAddress, "UAIEV", transportAir, directionExport, statusAppointed);
			AddForwarderAgent(forwarder, ausydAddress, "AUSYD", transportAir, directionBoth, statusGatewayAgent);
			AddForwarderAgent(forwarder, auAddress, "AU", transportAir, directionBoth, statusGatewayAgent);
			AddForwarderAgent(forwarder, uaievImpAddress, "UAIEV", transportAir, directionImport, statusGatewayAgent);
			AddForwarderAgent(forwarder, uaievExpAddress, "UAIEV", transportAir, directionExport, statusGatewayAgent);
			Factory.Save();
			AssertEquals(ausydAddress.PK, ausyd.GetBestAgent(forwarder, transportAir, directionImport).PK);
			AssertEquals(auAddress.PK, aumel.GetBestAgent(forwarder, transportAir, directionExport).PK);
			AssertEquals(uaievImpAddress.PK, uaiev.GetBestAgent(forwarder, transportAir, directionImport).PK);
			AssertEquals(uaievExpAddress.PK, uaiev.GetBestAgent(forwarder, transportAir, directionExport).PK);
		}

		public void TestBestAgent_Order()
		{
			var forwarder = GetForwarder(Factory);
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			var address1 = forwarder.Addresses.AddNew("address1", "address1");
			var address2 = forwarder.Addresses.AddNew("address2", "address2");
			var address3 = forwarder.Addresses.AddNew("address3", "address3");
			var address4 = forwarder.Addresses.AddNew("address4", "address4");
			var address5 = forwarder.Addresses.AddNew("address5", "address5");
			var address6 = forwarder.Addresses.AddNew("address6", "address6");
			var address7 = forwarder.Addresses.AddNew("address7", "address7");
			var address8 = forwarder.Addresses.AddNew("address8", "address8");
			AddForwarderAgent(forwarder, address1, "AU", transportSea, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, address2, "AU", transportSea, directionBoth, statusPublished);
			AddForwarderAgent(forwarder, address3, "AU", transportSea, directionImport, statusAppointed);
			AddForwarderAgent(forwarder, address4, "AU", transportSea, directionImport, statusPublished);
			AddForwarderAgent(forwarder, address5, "AUSYD", transportSea, directionBoth, statusAppointed);
			AddForwarderAgent(forwarder, address6, "AUSYD", transportSea, directionBoth, statusPublished);
			AddForwarderAgent(forwarder, address7, "AUSYD", transportSea, directionImport, statusAppointed);
			AddForwarderAgent(forwarder, address8, "AUSYD", transportSea, directionImport, statusPublished);
			Factory.Save();

			AssertBestAgentOrder(
				address8,
				address6,
				address4,
				address2,
				address7,
				address5,
				address3,
				address1);

			void AssertBestAgentOrder(params OrgAddress[] addresses)
			{
				addresses.ForEach(address =>
				{
					AssertEquals(address.PK, ausyd.GetBestAgent(forwarder, transportSea, directionImport).PK);
					forwarder.AppointedAgentPorts.Cast<OrgAppointedAgentPorts>().FirstOrDefault(p => p.O5_OA_AgentOfficeAddress == address.PK)?.Delete();
					Factory.Save();
				});
			}
		}

		#endregion

		#region Properties

		#region RL_Code

		public void TestRL_Code()
		{
			UNLOCO.RL_Code = "AUTST";
			AssertEquals("RL_Code changes Country Code, should be a valid country", "AU", UNLOCO.RL_RN_NKCountryCode);
			Assert("UNLOCO code equal", UNLOCO.RL_Code == "AUTST");

			UNLOCO.RL_Code = "UKTST";
			Assert("RL_Code Set block changes Country Code, invalid country, error expected", UNLOCO.RL_RN_NKCountryCodeInfo.HasErrors());
			Assert("UNLOCO code equal", UNLOCO.RL_Code == "UKTST");
		}

		#endregion

		#region RL_PortName

		public void TestRL_PortName()
		{
			UNLOCO.RL_PortName = "Test";
			AssertEquals("PortNamewithDiacriticals equal", UNLOCO.RL_PortName, UNLOCO.RL_NameWithDiacriticals);

			UNLOCO.RL_PortName = "AnotherTest";
			AssertEquals("PortNamewithDiacriticals equal", UNLOCO.RL_PortName, UNLOCO.RL_NameWithDiacriticals);
		}

		#endregion

		#region RL_PortName

		public void TestNameAndCountry()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Desc = "Country";

			UNLOCO.RL_PortName = "Port";
			AssertEquals("Port", UNLOCO.NameAndCountry);

			UNLOCO.RL_RN_NKCountryCode = country.Code;
			AssertEquals("Port, Country", UNLOCO.NameAndCountry);
		}

		#endregion

		#region RL_NameWithDiacriticalsInfo

		public void TestRL_NameWithDiacriticalsInfo()
		{
			UNLOCO.RL_IsSystem = true;
			GlbStaff.CurrentUser.GS_IsController = false;
			Assert("RL_NameWithDiacriticalsInfo is readonly when Controller is false, record is system.", UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			GlbStaff.CurrentUser.GS_IsController = true;
			Assert("RL_NameWithDiacriticalsInfo is not readonly when Controller is true, record is system.", !UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			UNLOCO.RL_IsSystem = false;
			GlbStaff.CurrentUser.GS_IsController = false;
			Assert("RL_NameWithDiacriticalsInfo is not readonly when Controller is false, record is not a system record.", !UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			GlbStaff.CurrentUser.GS_IsController = true;
			Assert("RL_NameWithDiacriticalsInfo is not readonly when Controller is true, record is not a system record..", !UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
		}

		#endregion

		#region StandardZoneUTCOffset
		public void Test_StandardZoneUTCOffsetReturnsValueInDecimalHours()
		{
			Assert("StandardZoneUTCOffset should return offset in decimal hours", UNLOCO.StandardZoneUTCOffset == 10.0);
		}
		#endregion

		#region TimeZoneDescription

		public void TestTimeZoneDescription()
		{
			TimeZoneSet.R3_TimeZoneSetName = "Australia/Sydney";
			UNLOCO.TimeZoneSet.DaylightSavingZone.Delete();
			AssertEquals("Australia/Sydney UTC+10:00", UNLOCO.TimeZoneDescription);

			UNLOCO.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = -120;
			AssertEquals("Australia/Sydney UTC-02:00", UNLOCO.TimeZoneDescription);
		}

		#endregion

		#region Current UNLOCO Time Change

		public void TestCurrentUNLOCOTimeChange()
		{
			AssertCurrentUNLOCOTimeChange(2.50m);
			AssertCurrentUNLOCOTimeChange(-2.50m);
			AssertCurrentUNLOCOTimeChange(1.00m);
			AssertCurrentUNLOCOTimeChange(-1.00m);
			AssertCurrentUNLOCOTimeChange(0.00m);
		}

		void AssertCurrentUNLOCOTimeChange(ZDecimal utcOffsetInHours)
		{
			UNLOCO.TimeZoneSet.StandardZone.R2_OffsetMinutesFromUTC = (ZShort)(utcOffsetInHours * 60m);

			ZDateTime utcTime1 = ZDateTime.UtcNow;
			ZDateTime locationTime = UNLOCO.LocationDateTime;
			TimeSpan locationUtcDiff = locationTime - utcTime1;

			AssertEquals("Location to UTC offset ~= " + utcOffsetInHours.ToString() + " hours", true,
				locationUtcDiff.TotalHours >= (double)(utcOffsetInHours - 0.02m) && locationUtcDiff.TotalHours <= (double)(utcOffsetInHours + 0.02m));
		}

		[TestDate(2021, 7, 14, 17, 0, 0)]
		public void TestLocationDateTimeWithoutHomePort()
		{
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.NewZealand));
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = country.Code;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RL_NKHomePort = null;
			Factory.Save();

			var expected = new ZDateTime(2021, 7, 15, 3, 0, 0);
			AssertEquals("Expected current time in New Zealand", expected, UNLOCO.LocationDateTime);
			AssertEquals("Expected current time in New Zealand", expected, UNLOCO.LocationDateTimeOffset.ToZDateTime());
			AssertEquals("Expected current time in New Zealand", TimeSpan.FromHours(10), UNLOCO.LocationDateTimeOffset.Offset);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Location Date Time should still work regardless of the current environment. Why would it matter?", expected, UNLOCO.LocationDateTime);
			}
		}

		#endregion

		#region Current UNLOCO Time Change With Daylight Savings

		public void TestCurrentUNLOCOTimeWithDaylightSavings()
		{
			ZDateTime utcNow = ZDateTime.UtcNow;
			UNLOCO.TimeZoneSet.DaylightSavingZone.R2_OffsetMinutesFromUTC = 120;
			StartRule.R4_DaylightSavingDate = utcNow.AddMonths(-1);
			StartRule.R4_TypeOfTime = "UTC";
			EndRule.R4_DaylightSavingDate = utcNow.AddMonths(1);
			EndRule.R4_TypeOfTime = "UTC";

			ZDateTime locationTime = UNLOCO.LocationDateTime;
			TimeSpan locationUtcDiff = locationTime - utcNow;
			AssertEquals("Location to UTC offset ~= 2h", true, locationUtcDiff.TotalMinutes >= 119D && locationUtcDiff.TotalMinutes <= 121D);
			AssertEquals(TimeSpan.FromHours(2), UNLOCO.LocationDateTimeOffset.Offset);
		}

		[TestTimeZoneUNLOCO("USERI")]
		[TestDate]
		public void TestUNLOCO_DuringDSTEndingRepeatedHour()
		{
			TestDateAttribute.UseUNLOCO = true;
			{
				var factory = new BusinessObjectFactory();

				//UTCNOW immediately before DST ending in 2014 (going from UTC-4h to UTC-5h)
				TestDateAttribute.Date = new DateTime(2014, 11, 2, 5, 30, 0);

				var uSERIUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USERI"));

				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), uSERIUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), uSERIUnloco.LocationDateTime);

				var aUSYDUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")); //at this time of year, UTC+11h

				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), aUSYDUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 16, 30, 0), aUSYDUnloco.LocationDateTime);
			}

			{
				var factory = new BusinessObjectFactory();

				//UTCNOW immediately after DST ending in 2014 (going from UTC-4h to UTC-5h)
				TestDateAttribute.Date = new DateTime(2014, 11, 2, 6, 30, 0);

				var uSERIUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USERI"));

				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), uSERIUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), uSERIUnloco.LocationDateTime);

				var aUSYDUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")); //at this time of year, UTC+11h

				AssertEquals(new DateTime(2014, 11, 2, 1, 30, 0), aUSYDUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 17, 30, 0), aUSYDUnloco.LocationDateTime);
			}

			{
				var factory = new BusinessObjectFactory();

				//UTCNOW immediately after the looped hour of DST ending in 2014 (going from UTC-4h to UTC-5h)
				TestDateAttribute.Date = new DateTime(2014, 11, 2, 7, 30, 0);

				var uSERIUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USERI"));

				AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0), uSERIUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0), uSERIUnloco.LocationDateTime);

				var aUSYDUnloco = factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")); //at this time of year, UTC+11h

				AssertEquals(new DateTime(2014, 11, 2, 2, 30, 0), aUSYDUnloco.LocalDateTime);
				AssertEquals(new DateTime(2014, 11, 2, 18, 30, 0), aUSYDUnloco.LocationDateTime);

				//After we edit it manually, it's a local date - so ambiguous - so we pick the second hour to break ties.
				aUSYDUnloco.LocalDateTime = new DateTime(2014, 11, 2, 0, 30, 0);
				AssertEquals(new DateTime(2014, 11, 2, 15, 30, 0), aUSYDUnloco.LocationDateTime);
				aUSYDUnloco.LocalDateTime = new DateTime(2014, 11, 2, 1, 30, 0);
				AssertEquals(new DateTime(2014, 11, 2, 17, 30, 0), aUSYDUnloco.LocationDateTime);
				aUSYDUnloco.LocalDateTime = new DateTime(2014, 11, 2, 2, 30, 0);
				AssertEquals(new DateTime(2014, 11, 2, 18, 30, 0), aUSYDUnloco.LocationDateTime);
			}
		}

		#endregion

		#region LocalDateTime

		[TestDate(2005, 7, 18, 12, 13, 10)]
		public void TestLocalDateTime()
		{
			AssertEquals("Location Date Time is set to now", ZDateTime.Now, UNLOCO.LocalDateTime);
			UNLOCO.LocalDateTime = ZDateTime.Invalid;
			AssertEquals("Location Date Time is still set to now", ZDateTime.Now, UNLOCO.LocalDateTime);
		}

		#endregion

		#region HasDaylightSavingZone

		public void TestHasDaylightSavingZone()
		{
			UNLOCO.TimeZoneSet.DaylightSavingZone.Delete();
			AssertEquals(false, UNLOCO.HasDaylightSavingsZone);

			DaylightSavingTimeZone timeZone1 = Factory.NewWithValidTestData<DaylightSavingTimeZone>();
			UNLOCO.TimeZoneSet.R3_R2_DaylightSavingZone = Factory.NewWithValidTestData<StandardTimeZone>().PK;

			AssertEquals(true, UNLOCO.HasDaylightSavingsZone);

			UNLOCO.TimeZoneSet.Delete();
			AssertEquals(false, UNLOCO.HasDaylightSavingsZone);
		}

		#endregion

		#endregion

		#region ILocation Implementation

		public void TestILocationImplementation()
		{
			RefUNLOCO testLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			RefCountry testCountry1 = Factory.NewWithValidTestData<RefCountry>();
			testCountry1.RN_Code = "Z1";
			RefCountry testCountry2 = Factory.NewWithValidTestData<RefCountry>();
			testCountry2.RN_Code = "Z2";
			RefZoneHeader testZone = Factory.NewWithValidTestData<RefZoneHeader>();
			RefCountryStates testState = Factory.NewWithValidTestData<RefCountryStates>();

			AssertEquals("((ILocation)TestLoco).UNLOCO set properly", testLoco, ((ILocation)testLoco).UNLOCO);

			testLoco.RL_RN_NKCountryCode = testCountry1.Code;
			AssertEquals("TestLoco.Country set properly", testCountry1, testLoco.Country);
			AssertEquals("((ILocation)TestLoco).Country set properly", testCountry1, ((ILocation)testLoco).Country);

			AssertEquals("Zone's LOCO collection empty", 0, testZone.UNLOCOs.Count);
			AssertEquals("Zone's Countries collection empty", 0, testZone.Countries.Count);

			testZone.UNLOCOs.Add(testLoco);
			testZone.Countries.Add(testCountry2);
			Assert("Added Loco and Country successfully to zone", testZone.UNLOCOs.Count == 1 && testZone.Countries.Count == 1);

			AssertEquals("((ILocation)TestLoco).Zone set properly", 1, ((ILocation)testLoco).Zones.Length);
			AssertEquals("((ILocation)TestLoco).Zone set properly", testZone, ((ILocation)testLoco).Zones[0]);

			testLoco.RL_RW = testState.PK;
			AssertEquals("((ILocation)TestLoco).State set properly", testState, ((ILocation)testLoco).State);

			((ILocation)testLoco).Zones[0].Delete();
			AssertEquals("((ILocation)TestLoco).Zones not caching deletes", 0, ((ILocation)testLoco).Zones.Length);
		}

		public void TestZoneHeaderForUNLOCOOrCountry()
		{
			RefZoneHeader testZoneHeader = Factory.NewWithValidTestData<RefZoneHeader>();
			RefUNLOCO testLoco = Factory.NewWithValidTestData<RefUNLOCO>();
			RefCountry testCountry = Factory.NewWithValidTestData<RefCountry>();

			testLoco.RL_RN_NKCountryCode = testCountry.Code;
			AssertEquals("TestLoco's Country should be TestCountry)", testCountry.Code, testLoco.RL_RN_NKCountryCode);
			AssertEquals("TestZoneHeaders LOCO count should be 0)", 0, testZoneHeader.UNLOCOs.Count);
			AssertEquals("TestZoneHeaders Country count should be 0)", 0, testZoneHeader.Countries.Count);

			testZoneHeader.UNLOCOs.Add(testLoco);
			Assert("ZoneHeader should contain UNLOCO", testZoneHeader.UNLOCOs.Contains(testLoco.PK));
			testZoneHeader.UNLOCOs.Remove(testLoco.PK);

			testZoneHeader.Countries.Add(testCountry);
			Assert("ZoneHeader should contain Country", testZoneHeader.Countries.Contains(testCountry.PK));
			testZoneHeader.Countries.Remove(testCountry.PK);

			testZoneHeader.Countries.Add(testCountry);
			testZoneHeader.UNLOCOs.Add(testLoco);
			Assert("ZoneHeader should contain UNLOCO", testZoneHeader.UNLOCOs.Contains(testLoco.PK));
			Assert("ZoneHeader should contain Country", testZoneHeader.Countries.Contains(testCountry.PK));
		}

		public void TestILocationCityTown()
		{
			var country = Factory.NewWithValidTestData<RefCountry>();
			country.RN_Code = "JH";
			country.RN_Desc = "Johto";

			var cityTown = Factory.NewWithValidTestData<RefCityTown>();
			cityTown.R9_RN_NKCountry = country.RN_Code;
			cityTown.R9_InternationalName = "Goldenrod City";

			var unloco = Factory.NewWithValidTestData<RefUNLOCO>();
			unloco.RL_RN_NKCountryCode = country.RN_Code;
			unloco.RL_Code = "JHGRC";
			unloco.RL_PortName = "Goldenrod City";

			var iLocation = (ILocation)unloco;

			AssertEquals(country, iLocation.Country);
			AssertEquals(cityTown, iLocation.CityTown);
		}

		public void TestILocationCompletelyCovers()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "GBBYS";
			var abbotsAdrs = org.MainAddress;
			abbotsAdrs.OA_RN_NKCountryCode = Constants.CountryCodes.UnitedKingdom;
			abbotsAdrs.OA_RL_NKRelatedPortCode = "GBBYS";

			var docAddress = Factory.New<JobDocAddress>();
			docAddress.DefaultAddressType = AddressType.DLV;
			docAddress.OrganisationPK = org.PK;

			var newYork = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USNYC"));
			var abbots = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBBYS"));
			var gbUnloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "GBABB"));

			var britain = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedKingdom);
			var america = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);

			AssertEquals("New York doesn't cover America", false, newYork.CompletelyCovers(america));
			AssertEquals("New York completely covers New York", true, newYork.CompletelyCovers(newYork));
			AssertEquals("Abbots cover Address in Abbots", true, abbots.CompletelyCovers(abbotsAdrs));
			AssertEquals("GBUNLOCO covers JobDoc in it.", true, abbots.CompletelyCovers(docAddress));

			var usAbbotsZone = Factory.NewWithValidTestData<RefZoneHeader>();
			usAbbotsZone.Countries.Add(america);
			usAbbotsZone.UNLOCOs.Add(abbots);

			AssertEquals("GBBYS does not cover one country Zone (with US) and UNLOCO", false, abbots.CompletelyCovers(usAbbotsZone));
		}

		#endregion

		#region Test Read Only Factory

		public void TestReadOnlyFactory()
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			AssertEquals("Read Only Factory instance", uNLOCO.ReadOnlyFactory, uNLOCO.ReadOnlyFactory);
		}

		#endregion

		#region Implementation

		RefUNLOCO UNLOCO;

		RefTimeZoneSet TimeZoneSet;
		StandardTimeZone StandardZone;
		DaylightSavingTimeZone DaylightSavingZone;

		RefTimeZoneRule StartRule;
		RefTimeZoneRule EndRule;

		GlbBranch TestBranch;

		#region Create Agents

		const string transportAir = Core.Constants.TransportModes.Air;
		const string transportSea = Core.Constants.TransportModes.Sea;

		const string statusPublished = AgentStatusList.Codes.Published;
		const string statusAppointed = AgentStatusList.Codes.Appointed;
		const string statusHandles = AgentStatusList.Codes.Handles;

		const string statusGatewayAgent = AgentStatusList.Codes.GatewayAgent;
		const string statusGatewayAgentWithTariff = AgentStatusList.Codes.GatewayAgentWithTariff;

		const string directionBoth = AgentDirectionList.Codes.Both;
		const string directionExport = AgentDirectionList.Codes.Export;
		const string directionImport = AgentDirectionList.Codes.Import;

		RefUNLOCO SYD;

		const string AU = "AU";

		int addressCount;

		OrgHeader GetForwarder(BusinessObjectFactory forwarderFactory)
		{
			OrgHeader result = forwarderFactory.New<OrgHeader>();
			result.OH_Code = string.Format("FWD{0}", ++addressCount);
			result.OH_RL_NKClosestPort = UNLOCO.Code;
			result.MainAddress.OA_Address1 = string.Format("{0} main address", result.OH_Code);
			result.OH_IsForwarder = true;

			return result;
		}

		OrgAddress GetForwarderAgent(BusinessObjectFactory forwarderFactory, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgHeader forwarder = GetForwarder(forwarderFactory);
			OrgAddress address = AddForwarderAgent(forwarder, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);

			return address;
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAddress address = forwarder.Addresses.AddNew();
			address.OA_Code = string.Format("Addr{0}", ++addressCount);
			address.OA_Address1 = address.OA_Code;

			return AddForwarderAgent(forwarder, address, agentPortOrCountry, agentTransportMode, agentDirection, agentStatus);
		}

		OrgAddress AddForwarderAgent(OrgHeader forwarder, OrgAddress address, ZString agentPortOrCountry, ZString agentTransportMode, ZString agentDirection, ZString agentStatus)
		{
			OrgAppointedAgentPorts newAppAgent = forwarder.AppointedAgentPorts.AddNew();
			newAppAgent.O5_PortOrCountry = agentPortOrCountry;
			newAppAgent.O5_OA_AgentOfficeAddress = address.PK;
			newAppAgent.O5_AgentDirection = agentDirection;

			switch (agentTransportMode)
			{
				case Core.Constants.TransportModes.Air:
					newAppAgent.O5_AirAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Rail:
					newAppAgent.O5_RailAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Road:
					newAppAgent.O5_RoadAgentStatus = agentStatus;
					break;

				case Core.Constants.TransportModes.Sea:
					newAppAgent.O5_SeaAgentStatus = agentStatus;
					break;
			}

			return address;
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			UNLOCO = Factory.NewWithValidTestData<RefUNLOCO>();
			TestBranch = GlbBranch.CurrentBranch;

			TimeZoneSet = Factory.NewWithValidTestData<RefTimeZoneSet>();
			StandardZone = TimeZoneSet.StandardZone;
			TimeZoneSet.HasDaylightSavings = true;
			DaylightSavingZone = TimeZoneSet.DaylightSavingZone;
			UNLOCO.RL_R3 = TimeZoneSet.PK;

			RefUNLOCO homePort = Factory.Load<RefUNLOCO>(TestBranch.HomePort.PK);
			homePort.RL_R3 = TimeZoneSet.PK;
			StandardZone.R2_OffsetMinutesFromUTC = 600;
			DaylightSavingZone.R2_OffsetMinutesFromUTC = 660;

			SYD = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			Factory.Save();

			StartRule = DaylightSavingZone.StartDateRules.AddNew();
			EndRule = DaylightSavingZone.EndDateRules.AddNew();
			StartRule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
			EndRule.R4_DaylightSavingDayWeekDate = TimeZoneConstants.DstRuleDayOfMonth;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_HasAirport = true;
			return uNLOCO;
		}

		void CheckUNLOCOInfoForNonSystemRecord()
		{
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_CodeInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_RWInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_PortNameInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_NameWithDiacriticalsInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasAirportInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasBorderCrossingInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasCustomsLodgeInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasDischargeInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasOutportInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasPostInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasRailInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasRoadInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasSeaportInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasStoreInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasTerminalInfo.ReadOnly);
			Assert("Not a system UNLOCO, property is not readonly", !UNLOCO.RL_HasUnloadInfo.ReadOnly);
			Assert("Flag Inidcatig the system record property is not readonly", !UNLOCO.RL_IsSystemInfo.ReadOnly);
		}

		void CheckUNLOCOInfoForSystemRecord()
		{
			Assert("Is a system UNLOCO, property is readonly", UNLOCO.RL_CodeInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is readonly", UNLOCO.RL_RN_NKCountryCodeInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_RWInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasAirportInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasBorderCrossingInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasCustomsLodgeInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasDischargeInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasOutportInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasPostInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasRailInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasRoadInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasSeaportInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasStoreInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasTerminalInfo.ReadOnly);
			Assert("Is a system UNLOCO, property is not readonly", !UNLOCO.RL_HasUnloadInfo.ReadOnly);
			Assert("Flag Inidcatig the system record property is not readonly", !UNLOCO.RL_IsSystemInfo.ReadOnly);
		}
		#endregion

		#region IATA Lookup

		public void TestLoadFromIATA()
		{
			AssertNull(RefUNLOCO.LoadFromIATA(Factory, "HAHAHHAHAHAHA"));
			AssertEquals("AUSYD", RefUNLOCO.LoadFromIATA(Factory, "SYD").RL_Code);
			AssertEquals("Multiple ports with same IATA", "USDFW", RefUNLOCO.LoadFromIATA(Factory, "DFW").RL_Code);
		}

		#endregion

		#region Org PortCode Lookup

		public void TestLookupWithNoValues()
		{
			AssertEquals("Default port code expected", "ZZZZZ", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "", "", ""));
		}

		public void TestLookupWithCountry()
		{
			AssertEquals("Default port code for country name expected", "AUZZZ", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "Australia", "", ""));
			AssertEquals("Default port code for country name expected", "SGZZZ", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "Singapore", "", ""));
			AssertEquals("Default port code for country code expected", "MYZZZ", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "MY", "", ""));
		}

		public void TestLookupWithCountryAndCity()
		{
			AssertEquals("Default port code for Perth expected", "AUEUK", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "Australia", "Perth", ""));
			AssertEquals("Default port code for Singapore expected", "SGSIN", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "Singapore", "Singapore", ""));
			AssertEquals("Default port code for Kuala Lumpur expected", "MYKUL", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "MY", "Kuala Lumpur", ""));
		}

		public void TestLookupWithAussieState()
		{
			AssertEquals("Default port code for WA expected", "AUPER", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "", "", "WA"));
			AssertEquals("Default port code for S.A. expected", "AUADL", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "", "", "S.A."));
			AssertEquals("Default port code for Victoria expected", "AUMEL", RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, "", "", "Victoria"));
		}

		#endregion

		#region IsAutoLogged

		public void TestIsAutoLogged()
		{
			// Set mandatory fields so it can be saved
			StartRule.R4_DaylightSavingDate = ZDateTime.Now;
			EndRule.R4_DaylightSavingDate = ZDateTime.Now;

			Factory.Save();

			AssertEquals("Expect that log is added.", true, UNLOCO.Logs.GetAllLogs()[0].IsInDatabase);
			AssertEquals("Expect that only one log exists.", 1, UNLOCO.Logs.GetAllLogs().Count);
		}

		#endregion

		#region ILocationReference

		public void TestILocationReferenceIsLocalInRelationTo()
		{
			ILocationReference locationReference = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));

			AssertEquals("AUSYD local to AUSYD", true, locationReference.IsLocalInRelationTo("AUSYD"));
			AssertEquals("AUMEL local to AUSYD", true, locationReference.IsLocalInRelationTo("AUMEL"));
			AssertEquals("SGSIN not local to AUSYD", false, locationReference.IsLocalInRelationTo("SGSIN"));
		}

		#endregion

		#region HumanReadableName

		public void TestHumanReadableName()
		{
			UNLOCO.RL_Code = "123";
			AssertEquals("UNLOCO (123)", UNLOCO.HumanReadableName);
			UNLOCO.RL_Code = "AUSYD";
			AssertEquals("UNLOCO (AUSYD)", UNLOCO.HumanReadableName);
		}

		#endregion

		#region Zones

		public void TestZonesCacheStatus()
		{
			var unloco = new RefUNLOCO.Loader(Factory).Load("AUSYD") as ILocation;
			var zones1 = unloco.Zones;
			Assert("Cache hit", ReferenceEquals(zones1, unloco.Zones));
			Factory.NewWithValidTestData<RefZoneHeader>();
			Assert("Cache was stale", !ReferenceEquals(zones1, unloco.Zones));
		}

		#endregion
	}
}
