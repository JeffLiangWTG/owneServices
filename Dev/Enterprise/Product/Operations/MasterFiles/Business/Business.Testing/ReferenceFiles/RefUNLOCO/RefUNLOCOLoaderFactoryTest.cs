using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefUNLOCOLoaderFactoryTest : TestCaseWithFactory
	{
		public void TestLoadFromIATA()
		{
			AssertNull(loader.LoadFromIATA(Factory, "HAHAHHAHAHAHA"));
			AssertEquals("AUSYD", loader.LoadFromIATA(Factory, "SYD").RL_Code);
			AssertEquals("Multiple ports with same IATA", "USDFW", loader.LoadFromIATA(Factory, "DFW").RL_Code);
			AssertEquals("active UNLOCO has precedence", "RSBEG", loader.LoadFromIATA(Factory, "BEG").RL_Code);

			var rsbeg = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "RSBEG");
			var csbeg = Factory.New<RefUNLOCO>();
			csbeg.RL_Code = "CSBEG";
			rsbeg.RL_IsActive = false;
			csbeg.RL_IsActive = true;
			csbeg.RL_RN_NKCountryCode = "CS";
			csbeg.RL_IATA = "BEG";
			Factory.Save();

			AssertEquals("active UNLOCO has precedence", "CSBEG", loader.LoadFromIATA(Factory, "BEG").RL_Code);

			rsbeg.RL_IsActive = true;
			csbeg.RL_IsActive = false;
			Factory.Save();
			AssertEquals("active UNLOCO has precedence", "RSBEG", loader.LoadFromIATA(Factory, "BEG").RL_Code);
		}

		public void TestGetPortFromNameAndCountryCode()
		{
			ZString portName = "Christchurch";
			ZString countryCode = "NZ";
			var unloco = loader.GetPortFromNameAndCountryCode(Factory, portName, countryCode);
			AssertNotNull("Christchurch port should NOT be null", unloco);
			AssertEquals("Christchurch port code", "NZCHC", unloco.RL_Code);

			portName = "InvalidPortName";
			unloco = loader.GetPortFromNameAndCountryCode(Factory, portName, countryCode);
			AssertNull("InvalidPortName", unloco);

			// Wollongong should not be found in NZ, so null should be returned.
			portName = "Wollongong";
			unloco = loader.GetPortFromNameAndCountryCode(Factory, portName, countryCode);
			AssertNull("Wollongong port should be null", unloco);

			countryCode = "AU";
			unloco = loader.GetPortFromNameAndCountryCode(Factory, portName, countryCode);
			AssertNotNull("Wollongong port should NOT be null", unloco);
			AssertEquals("Wollongong port code", "AUWOL", unloco.RL_Code);
		}

		public void TestGetPortFromNameAndCountryName()
		{
			ZString testPortName = "Christchurch";
			ZString testCountryName = "New Zealand";
			var unloco = loader.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Christchurch port should NOT be null", unloco);
			AssertEquals("Christchurch port code", "NZCHC", unloco.RL_Code);

			testPortName = "InvalidPortName";
			unloco = loader.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNull("InvalidPortName", unloco);

			// Wollongong should not be found in NZ, so null should be returned.
			testPortName = "Wollongong";
			unloco = loader.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNull("Wollongong port should be null", unloco);

			testCountryName = "Australia";
			unloco = loader.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Wollongong port should NOT be null", unloco);
			AssertEquals("Wollongong port code", "AUWOL", unloco.RL_Code);

			testPortName = "Hamburg";
			testCountryName = "Germany";
			unloco = loader.GetPortFromNameAndCountryName(Factory, testPortName, testCountryName);
			AssertNotNull("Hamburg port should NOT be null", unloco);
			AssertEquals("Hamburg port code", "DEHAM", unloco.RL_Code);
		}

		public void TestLoadFromLocalMap()
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

			Factory.Save();

			var refUNLOCO = loader.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.UnitedStates, "XYZ");
			AssertEquals("USLAX", refUNLOCO.RL_Code);

			refUNLOCO = loader.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.UnitedStates, "AIR");
			AssertNull(refUNLOCO);

			refUNLOCO = loader.LoadFromLocalMap(Factory, "2795", Core.Constants.CountryCodes.SouthAfrica, "XYZ");
			AssertNull(refUNLOCO);

			refUNLOCO = loader.LoadFromLocalMap(Factory, "2791", Core.Constants.CountryCodes.UnitedStates, "XYZ");
			AssertNull(refUNLOCO);

			refUNLOCO = loader.LoadFromLocalMap(Factory, "9639", Core.Constants.CountryCodes.Australia, "XYZ");
			AssertEquals("AUSYD", refUNLOCO.RL_Code);
		}

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

			var returnedUNLOCO = loader.LoadFromForeignCode(Factory, "AAA", company);
			AssertNotNull(returnedUNLOCO);
			AssertEquals(portWithExCode.RL_Code, returnedUNLOCO.RL_Code);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			loader = ObjectFactory.Get<IRefUNLOCOLoader>();
		}

		IRefUNLOCOLoader loader;

		#endregion
	}
}
