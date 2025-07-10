using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.DataTransfer.TACT;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.DataTransfer.Testing
{
	// Note: If you have problems with tests failing locally but passing in DAT
	// please see these links to possibly fix it:
	// 1. https://stackoverflowteams.com/c/wisetechglobal/questions/8701/8702#8702
	// 2. https://stackoverflowteams.com/c/wisetechglobal/questions/10343/10346#10346
	[UseSnapshotProtection]
	sealed class TACTImporterTest : TestCase
	{
		#region Setting RateEntry creation source

		public void TestImport_RateEntryCreationSourceSet()
		{
			CreateCarrierOrganisation("KL", "KLMAIRLINES");

			var tactRates = @"
KSCAR  NBFSGBORKIE    KL              00001KGBP200000001720071001         KL   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), out TACTImportResult _, out NotificationBuffer _);

			var actual = rates.Select(r => r.TI_CreationSource).ToArray();
			var expected = new[] { (ZString)RateEntryCreator.Sources.FromTACT };

			AssertArrayEqualsByElements(expected, actual);
		}

		#endregion

		#region Carrier

		public void TestImport_CarrierIsUnknown_SkipRates()
		{
			CreateCarrierOrganisation("KL", "KLMAIRLINES");

			var tactRates = @"
KSCAR  NBFSGBORKIE    KL              00001KGBP200000001720071001         KL   A
KSCAR  NBFSGBORKIE    EK              00001KGBP200000001720071001         EK   A
GSCAR  NBFSGBORKIE    QF              00001KGBP200000004820080201         QF   A
GSCAR  NBFSGBORKIE    QF              00100KGBP200000003320070601         QF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), out TACTImportResult result, out NotificationBuffer _);

			AssertEquals(1, rates.Length);
			AssertEquals("Carrier on created rate", "KLMAIRLINES", rates[0].TransportProvider.OH_Code);
			AssertEquals("Number of rates skipped with EK airline", 1, result.UnknownCarriers["EK"]);
			AssertEquals("Number of rates skipped with QF airline", 2, result.UnknownCarriers["QF"]);
		}

		#endregion

		#region Carrier Service Level

		public void TestImport_ServiceLevelMappingExists_MapIt()
		{
			var carrier = CreateCarrierOrganisation("LH", "LUFTHANSA");
			var mapping = carrier.CreatePatternMatchOverrideForTest();
			mapping.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel;
			mapping.OO_ForeignCode = "LH40";
			mapping.OO_LocalCode = "LH1";

			carrier.Factory.Save();

			var tactRates = @"
MC501  ASYDAULAXUSLH40LH              00000KAUD200000800019981001        74083 A
";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			// Rate
			AssertEquals("Number of created RateEntries", 1, rates.Length);
			AssertEquals("Service level code on RateEntry", "LH1", rates[0].TI_PL_NKCarrierServiceLevel);
		}

		public void TestImport_ServiceLevelIsNotMapped_CreateMapping()
		{
			var carrier = CreateCarrierOrganisation("LH", "LUFTHANSA");
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "LH4";
			serviceLevel.PL_CarrierServiceLevelDescription = "LH40";

			carrier.Factory.Save();

			var tactRates = @"
MC501  ASYDAULAXUSLH40LH              00000KAUD200000800019981001        74083 A
";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			// Rate
			AssertEquals("Number of created RateEntries", 1, rates.Length);
			AssertEquals("Service level code on RateEntry", "LH4", rates[0].TI_PL_NKCarrierServiceLevel);

			// Code Mapping
			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, carrier.PK)
			.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel);
			var codeMappings = carrier.Factory.Load<OrgPatternMatchOverride>(filter);

			AssertEquals("Mapping local code", "LH4", codeMappings[0].OO_LocalCode);
			AssertEquals("Mapping foreign code", "LH40", codeMappings[0].OO_ForeignCode);
		}

		public void TestImport_ServiceLevelDoesntExist_CreateAndMapIt()
		{
			var lufthansa = CreateCarrierOrganisation("LH", "LUFTHANSA");
			var emirates = CreateCarrierOrganisation("EK", "EMIRATES");

			var tactRates = @"
MC501  ASYDAULAXUSLH02LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSLH05LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSLH01LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSLH03LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSLH04LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSLH06LH              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK20EK              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK60EK              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK30EK              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK40EK              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK10EK              00000KAUD200000800019981001        74083 A
MC501  ASYDAULAXUSEK50EK              00000KAUD200000800019981001        74083 A
";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			// Rate
			var actualRates = rates.Select(r => (string)r.TI_PL_NKCarrierServiceLevel).ToArray();
			var expectedRates = new[] { "LH1", "LH2", "LH3", "LH4", "LH5", "LH6", "EK1", "EK2", "EK3", "EK4", "EK5", "EK6" };
			AssertContainsExactElementsInAnyOrder(expectedRates, actualRates);

			// Service Level
			lufthansa.MiscServ.CarrierServiceLevels.Load();
			emirates.MiscServ.CarrierServiceLevels.Load();

			var lufthansaServiceLevels = lufthansa.MiscServ.CarrierServiceLevels
				.Cast<OrgCarrierServiceLevel>()
				.Where(s => s.PL_Code != "STD")
				.Select(s => FormattableString.Invariant($"{s.PL_Code} - {s.PL_CarrierServiceLevelDescription}"))
				.ToArray();

			var emiratesServiceLevels = emirates.MiscServ.CarrierServiceLevels
				.Cast<OrgCarrierServiceLevel>()
				.Where(s => s.PL_Code != "STD")
				.Select(s => FormattableString.Invariant($"{s.PL_Code} - {s.PL_CarrierServiceLevelDescription}"))
				.ToArray();

			var expectedLufthansaServiceLevels = new[]
			{
				"LH1 - LH01",
				"LH2 - LH02",
				"LH3 - LH03",
				"LH4 - LH04",
				"LH5 - LH05",
				"LH6 - LH06",
			};

			var expectedEmiratesServiceLevels = new[]
			{
				"EK1 - EK10",
				"EK2 - EK20",
				"EK3 - EK30",
				"EK4 - EK40",
				"EK5 - EK50",
				"EK6 - EK60",
			};

			AssertContainsExactElementsInAnyOrder(expectedLufthansaServiceLevels, lufthansaServiceLevels);
			AssertContainsExactElementsInAnyOrder(expectedEmiratesServiceLevels, emiratesServiceLevels);

			// Code Mapping
			var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.CarrierServiceLevel);
			var codeMappings = lufthansa.Factory.Load<OrgPatternMatchOverride>(filter)
				.Select(m => FormattableString.Invariant($"{m.OO_LocalCode} - {m.OO_ForeignCode}"))
				.ToArray();

			var expectedCodeMappings = new[]
			{
				"LH1 - LH01",
				"LH2 - LH02",
				"LH3 - LH03",
				"LH4 - LH04",
				"LH5 - LH05",
				"LH6 - LH06",
				"EK1 - EK10",
				"EK2 - EK20",
				"EK3 - EK30",
				"EK4 - EK40",
				"EK5 - EK50",
				"EK6 - EK60",
			};

			AssertContainsExactElementsInAnyOrder(expectedCodeMappings, codeMappings);
		}

		public void TestImport_ServiceLevelDoesntExistAndNoFreeIndex_SkipRate()
		{
			var carrier = CreateCarrierOrganisation("LH", "LUFTHANSA");

			for (var i = 0; i < 10; i++)
			{
				var sercieLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
				sercieLevel.PL_Code = "LH" + i;
				sercieLevel.PL_CarrierServiceLevelDescription = "LH4" + i;
			}

			carrier.Factory.Save();

			var tactRates = @"
MC501  ASYDAULAXUSLH4XLH              00000KAUD200000800019981001        74083 A
";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), out TACTImportResult result, out NotificationBuffer _);

			AssertEquals("Number of created RateEntries", 0, rates.Length);
			AssertEquals("Number of rates skipped with LH4X carrier sercice level", 1, result.UnknownCarrierServiceLevel["LH - LH4X"]);
		}

		#endregion

		#region Delete Existing Rates
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_BatchDeleteExistingRates_ExceptionNotified()
		{
			CreateCarrierOrganisation("KL", "KLMAIRLINES");

			string testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Rating\Rating.DataTransfer.Test\TACT\Testing\Test150CharTACT.054";
			Import(testFilePath, TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, null, true, RatingRoundingTypes.Chargeable, true);

			string tactRates = @"
				BS2A   CAR  N28420FLRIT39720IZMTR    AF                 00000K                                 EUR20000039772013010120130101        A          AF  A  
				BS2A   CAR  N28420FLRIT40520JERGB    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  
				BS2A   CAR  N28420FLRIT40520JERGB    OK                 00000K                                 EUR20000036152013090120130901        A          OK  A  
				BS2A   CAR  N28420FLRIT41050JKGSE    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  
				BS2A   CAR  N28420FLRIT41050JKGSE    OK                 00000K                                 EUR20000036152013090120130901        A          OK  A  
				BS2A   CAR  N28420FLRIT42050KLRSE    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  ";

			AssertExceptionThrown("An error occurred while clearing existing TACT rates",
				typeof(TimeoutException),
				"An error occurred while clearing existing TACT rates: SQL Execution Timeout Expired, Try running the process again.",
				() => Import(tactRates, new TACTData150FixedWidthDataFormat(), 4, true, TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, null, true, RatingRoundingTypes.Chargeable, true),
				true
				);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_BatchDeleteExistingRates()
		{
			CreateCarrierOrganisation("KL", "KLMAIRLINES");

			int batchSize = 3;
			string testFilePath = BaseSourcePath + @"Enterprise\Product\Operations\Rating\Rating.DataTransfer.Test\TACT\Testing\Test150CharTACT.054";
			var connection = Db.Connection;

			var factory = new BusinessObjectFactory();
			var costing = factory.LoadTop1<Costing>(new ZQuery()) ?? factory.New<Costing>();
			factory.Save();

			var companyPK = costing.TH_GC.IsEmpty ? Guid.Empty : costing.TH_GC.ToGuid();
			var importOptions = new TACTImportOptions()
			{
				CompanyPK = companyPK,
				RatingHeaderPK = costing.PK.ToGuid(),
				IsJobLevelCharge = true,
				Rounding = RatingRoundingTypes.Chargeable,
				ShouldExcludeFromAutoRating = true
			};

			Import(testFilePath, TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, importOptions);

			var importer = new TACTImporterForTest(GetStream(""), new TACTData150FixedWidthDataFormat(), new NotificationBuffer(), importOptions);
			Guid ratingHeaderPK = importOptions.RatingHeaderPK;

			int backlog = importer.GetExistingRateBacklog(ratingHeaderPK, true, false, connection);
			AssertGreaterThan("Rates imported", backlog, batchSize);

			string tactRates = @"
				BS2A   CAR  N28420FLRIT39720IZMTR    AF                 00000K                                 EUR20000039772013010120130101        A          AF  A  
				BS2A   CAR  N28420FLRIT40520JERGB    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  
				BS2A   CAR  N28420FLRIT40520JERGB    OK                 00000K                                 EUR20000036152013090120130901        A          OK  A  
				BS2A   CAR  N28420FLRIT41050JKGSE    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  
				BS2A   CAR  N28420FLRIT41050JKGSE    OK                 00000K                                 EUR20000036152013090120130901        A          OK  A  
				BS2A   CAR  N28420FLRIT42050KLRSE    AF                 00000K                                 EUR20000036512010040120100401        A          AF  A  ";

			Import(tactRates, new TACTData150FixedWidthDataFormat(), batchSize, false, TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, importOptions);
			backlog = importer.GetExistingRateBacklog(ratingHeaderPK, true, false, connection);
			AssertEquals("Rates batch delete successful !!", backlog, 0);
		}

		#endregion

		#region Location Mapping

		public void TestImport_LocationMapping_ShouldMapToIATACityCodeWhereExists()
		{
			var tactRates = @"
MCCAR  NAJAFRZRHCH                    00000KEUR200000600020100901         AF   A
MCCAR  NSYDAULONGB                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals("SYD", rates[0].TI_OriginLRC);      // RL_IATARegionCode X 2 = "SYD"
			AssertEquals("LON", rates[0].TI_DestinationLRC); // RL_IATARegionCode X 8 = "LON"
			AssertEquals("ZRH", rates[1].TI_DestinationLRC); // RL_IATARegionCode X 1 = "ZRH"
			AssertEquals("AJA", rates[1].TI_OriginLRC);      // RL_IATARegionCode X 1 = "AJA"
		}

		public void TestImport_LocationMapping_ShouldMapToUNLOCOWhereIATACityCodeDoesntExistButIATAAirportCodeExists()
		{
			var tactRates = @"
MCCAR  NSWFUSBSRIQ                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals("USSWF", rates[0].TI_OriginLRC);      // RL_IATA = "SWF" but RL_IATARegionCode = "NYC"
			AssertEquals("IQBSR", rates[0].TI_DestinationLRC); // RL_IATA = "BSR" and RL_IATARegionCode is empty
		}

		public void TestImport_LocationMapping_ShouldKeepLocationCodeAsItIsWhereIsUnknown()
		{
			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A
GSCAR  NAJAFRZZZAU                    00001KEUR200000046920100901         AF   A
GSCAR  NAJAFRZZZAU                    00045KEUR200000037220100901         AF   A
GSCAR  NAJAFRZZZAU                    00100KEUR200000026120100901         AF   A
MCCAR  NEEEFRZRHCH                    00000KEUR200000600020100901         AF   A
GSCAR  NEEEFRZRHCH                    00001KEUR200000033920100901         AF   A
GSCAR  NEEEFRZRHCH                    00045KEUR200000028720100901         AF   A
GSCAR  NEEEFRZRHCH                    00100KEUR200000020920100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals(2, rates.Length);
			AssertEquals("AUZZZ", rates[0].TI_DestinationLRC);
			AssertEquals("FREEE", rates[1].TI_OriginLRC);
		}

		#endregion

		#region Effective Dates

		[TestDate(2025, 1, 1)]
		public void TestImport_RatesAreExpired_Skip()
		{
			var tactRates = @"
MC501  ASYDAULAXUS                    00000KAUD200000800020180101        74083 A
MC501  AIEVIASYDAU                    00000KAUD2000008000201801012030010174083 A
MC501  AIEVIASINSG                    00000KAUD2000008000202801012030010174083 A
MC501  AIEVIANZAKL                    00000KAUD2000008000201801012020010174083 A
";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), out TACTImportResult result, out NotificationBuffer _);

			AssertEquals(3, rates.Length);
			AssertEntry(rates[0], "SYD", "LAX", new ZDate(2018, 1, 1), ZDate.Empty);
			AssertEntry(rates[1], "IEV", "SYD", new ZDate(2018, 1, 1), new ZDate(2030, 1, 1));
			AssertEntry(rates[2], "IEV", "SIN", new ZDate(2028, 1, 1), new ZDate(2030, 1, 1));
			AssertEquals("Number of skipped rates", 1, result.ExpiredRates);
		}

		#endregion

		#region Calculators

		public void TestImport_ImperfectBreaks_TurnIntoPerUnit()
		{
			var tactRates = @"
MCCAR  NABZGBAALDK                    00000KGBP200000420020071201         SK   A
GSCAR  NABZGBAALDK                    00001KGBP200000020920071201         SK   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals(1, rates.Length);
			AssertEquals("Minimum rate", 42m, rates[0].RateLines[0].Calculator[Calculator.Items.Operator.MIN]);
			AssertEquals("Unit rate", 2.09m, rates[0].RateLines[0].Calculator[Calculator.Items.Operator.UNT]);
			AssertNull("Break", rates[0].RateLines[0].Calculator["+1"]);
		}

		public void TestImport_CorrectCalculatorCodeIsAssigned()
		{
			var tactRates = @"
MCCAR  NMELAUAALDK                    00000KGBP200000420020071201         SK   A
GSCAR  NABZGBAALDK                    00001KGBP200000020920071201         SK   A
BSCAR  NPERAULBAGB                    00000KEUR200000454520100401         SK   A
MCCAR  NCWWAUAALDK                    00000KAUD200000420020071201         SK   A
GSCAR  NCWWAUAALDK                    00001KAUD200000020920071201         SK   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());
			AssertEquals(4, rates.Length);

			var rateLine = rates.Where(x => x.TI_OriginLRC == "MEL").Single().RateLines[0];
			AssertEquals("Charge Calculator", MinimumOrPerUnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Minimum rate", 42m, rateLine.Calculator[Calculator.Items.Operator.MIN]);
			AssertEquals("Per Unit rate", 0m, rateLine.Calculator[Calculator.Items.Operator.UNT]);

			rateLine = rates.Where(x => x.TI_OriginLRC == "GBABD").Single().RateLines[0];
			AssertEquals("Charge Calculator", UnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Per Unit rate", 2.09m, rateLine.Calculator[Calculator.Items.Operator.UNT]);

			rateLine = rates.Where(x => x.TI_OriginLRC == "PER").Single().RateLines[0];
			AssertEquals("Charge Calculator", FlatCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Base rate", 45.45m, rateLine.Calculator[Calculator.Items.Operator.BAS]);

			rateLine = rates.Where(x => x.TI_OriginLRC == "CWW").Single().RateLines[0];
			AssertEquals("Charge Calculator", MinimumOrPerUnitCalculator.Code, rateLine.TL_RateCalculator);
			AssertEquals("Minimum rate", 42m, rateLine.Calculator[Calculator.Items.Operator.MIN]);
			AssertEquals("Per Unit rate", 2.09m, rateLine.Calculator[Calculator.Items.Operator.UNT]);
		}

		#endregion

		#region Charge Code Defaulting

		public void TestImport_LocalChargeCodeFromRegistry()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new TestHelper(factory);
			var globalChargeCode = testHelper.ChargeCodes.CreateGlobalCharge("NEWFRT");
			factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			Env.Registry.FreightChargeCode = localChargeCode.PK.ToGuid();
			factory.Save();

			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals(localChargeCode.PK, rates[0].RateLines[0].TL_AC);
		}

		public void TestImport_LocalChargeCode_FRTFallback()
		{
			Env.Registry.FreightChargeCode = Guid.Empty;

			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals("FRT", rates[0].RateLines[0].ChargeCode.AC_Code);
		}

		public void TestImport_GlobalCosting_GlobalChargeCodeFromRegistry()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new TestHelper(factory);
			var globalChargeCode = testHelper.ChargeCodes.CreateGlobalCharge("TACCHG");
			factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			Env.Registry.FreightChargeCode = localChargeCode.PK.ToGuid();

			var costing = factory.New<Costing>();
			costing.TH_GC = ZGuid.Empty;
			factory.Save();

			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals(globalChargeCode.PK, rates[0].RateLines[0].TL_AC);
		}

		public void TestImport_GlobalCosting_GlobalFRTChargeCode()
		{
			var factory = new BusinessObjectFactory();
			var testHelper = new TestHelper(factory);
			var globalChargeCode = testHelper.ChargeCodes.CreateGlobalCharge("FRT");

			Env.Registry.FreightChargeCode = Guid.Empty;

			var costing = factory.New<Costing>();
			costing.TH_GC = ZGuid.Empty;
			factory.Save();

			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals("Should fall back to the global FRT charge code if registry cannot be used", globalChargeCode.PK, rates[0].RateLines[0].TL_AC);
		}

		public void TestImport_RoundingIsSpecified_DefaultToRoundingToSpecifiedOne()
		{
			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), rounding: RatingRoundingTypes.Bankers);
			AssertEquals(RatingRoundingTypes.Bankers, rates[0].RateLines[0].TL_Rounding);

			rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), rounding: RatingRoundingTypes.Chargeable);
			AssertEquals(RatingRoundingTypes.Chargeable, rates[0].RateLines[0].TL_Rounding);
		}

		public void TestImport_RoundingIsNotSpecified_DefaultToNoRounding()
		{
			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), rounding: "");

			AssertEquals(RatingRoundingTypes.NoRounding, rates[0].RateLines[0].TL_Rounding);
		}

		public void TestImport_DefaultJobLevelChargeShouldBeTrue()
		{
			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), isJobLevelCharge: true);
			AssertEquals(true, rates[0].RateLines[0].TL_IsWhsJobLevelCharge);
		}

		public void TestImport_DefaultJobLevelChargeShouldBeFalse()
		{
			var tactRates = @"
MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";

			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), isJobLevelCharge: false);
			AssertEquals(false, rates[0].RateLines[0].TL_IsWhsJobLevelCharge);
		}

		#endregion

		#region Duplicates

		[TestDate(2017, 02, 07)]
		public void TestImport_OverlappingSecondTACTFile_ClearDuplicateEntries()
		{
			var tactRates = @"MC501  ASYDAULAXUS                    00000KAUD200000800020171001        74083 A
MC501  ASYDAUBODFR                    00000KAUD200000850020170901        55073 A
MC501  ASYDAUHKGHK                    00000KAUD200000850020170801        55073 A";

			var rateEntries = Import(tactRates, new TACTData80FixedWidthDataFormat());

			AssertEquals("Should have successfully imported the rates", 3, rateEntries.Length);

			rateEntries[0].Factory.Save();

			var overlappingTACTRates = @"MC501  ASYDAUBODFR                    00000KAUD200000850020170901        55073 A
MC501  ASYDAUHKGHK                    00000KAUD200000850020170801        55073 A
MC501  ASYDAUDUSDE                    00000KAUD200000850020170701        55073 A
MC501  ASYDAUBERDE                    00000KAUD200000850020170601        55073 A";

			Import(overlappingTACTRates, new TACTData80FixedWidthDataFormat());

			AssertNoExceptionThrown("Should be able to save as there are no duplicates", () => rateEntries[0].Factory.Save());

			var actualResults = new BusinessObjectFactory().Load<RateEntry>(new ZQuery());
			AssertEquals("Should not have duplicates", 5, actualResults.Length);

			var expectedOriginDestinations = new[]
			{
				"SYD-DUS",
				"SYD-BOD",
				"SYD-LAX",
				"SYD-HKG",
				"SYD-BER"
			};

			var actualOriginDestinations = actualResults.Select(x => $"{x.TI_OriginLRC}-{x.TI_DestinationLRC}");
			AssertContainsExactElementsInAnyOrder(expectedOriginDestinations, actualOriginDestinations);
		}

		#endregion

		#region Excluded From AutoRating

		public void TestImport_ExcludeFromAutoRatingShouldBeFalse()
		{
			var tactRates = @"MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), shouldExcludeFromAutoRating: false);
			AssertEquals(false, rates[0].TI_IsExcludedFromAutoRating);
		}

		public void TestImport_ExcludeFromAutoRatingShouldBeTrue()
		{
			var tactRates = @"MCCAR  NAJAFRZZZAU                    00000KEUR200000600020100901         AF   A";
			var rates = Import(tactRates, new TACTData80FixedWidthDataFormat(), shouldExcludeFromAutoRating: true);
			AssertEquals(true, rates[0].TI_IsExcludedFromAutoRating);
		}

		#endregion

		#region 80 Width Test

		[TestDate(2010, 4, 10)]
		public void TestImport_TACTLinesAreProperlyOrdered_80()
		{
			var tactRates = @"
MC501  ASYDAULAXUS                    00000KAUD200000800019981001        74083 A
GS550  ASYDAULAXUS                    00001KAUD200000139519990405        74076 A
GS550  ASYDAULAXUS                    00045KAUD200000066019990405        74076 A
GS550  ASYDAULAXUS                    00100KAUD200000040019990405        74076 A
GS550  ASYDAULAXUS                    00300KAUD200000034019990405        74076 A
GS550  ASYDAULAXUS                    00500KAUD200000032019990405        74076 A
GS550  ASYDAULAXUS                    01000KAUD200000029519990405        74080 A
SS590  ASYDAULAXUS        001420      00045KAUD200000047519990405        74076 A
US530  ASYDAULAXUS                   C00000KAUD200000021019991101        74740 A
US530  ASYDAULAXUS               05  B02500KAUD200073400019991101        74740 A
US530  ASYDAULAXUS               08  B00925KAUD200027300019991101        74740 A
MC501  ASYDAUBODFR                    00000KAUD200000850019981001        55073 A
GC550  ASYDAUBODFR                    00001KAUD200000145519990405        55068 A
GC550  ASYDAUBODFR                    00045KAUD200000112519991001        55837 A
GC550  ASYDAUBODFR                    00100KAUD200000077519991001        55837 A
GC550  ASYDAUBODFR                    00250KAUD200000050019991001        55837 A
GC550  ASYDAUBODFR                    00500KAUD200000038519991001        55837 A
GC550  ASYDAUBODFR                    01000KAUD200000035019991001        55837 A
SC590  ASYDAUBODFR        000006      00100KAUD200000041019991001        55837 A
SC590  ASYDAUBODFR        000006      00500KAUD200000038019991001        55837 A
SC590  ASYDAUBODFR        001420      00045KAUD200000050519991001        55837 A
MC501  ASYDAULONGB                    00000KAUD200000850019981001        55073 A
GS550  ASYDAULONGB                    00001KAUD200000153519990405        55068 A
GS550  ASYDAULONGB                    00045KAUD200000118519991001        55837 A
GS550  ASYDAULONGB                    00100KAUD200000077519991001        55837 A
GS550  ASYDAULONGB                    00250KAUD200000050019991001        55837 A
GS550  ASYDAULONGB                    00500KAUD200000038519991001        55837 A
GS550  ASYDAULONGB                    01000KAUD200000035019991001        55837 A
SS590  ASYDAULONGB        000006      00100KAUD200000041019991001        55837 A
SS590  ASYDAULONGB        000006      00500KAUD200000038019991001        55837 A
SS590  ASYDAULONGB        001420      00045KAUD200000050519991001        55837 A
CS590  ASYDAULONGB        000006 05  H00000KAUD200127000019991001        55837 A
CS590  ASYDAULONGB        000006 08  H00000KAUD200042600019991001        55837 A
BSCAR  NSYDAULBAGB                    00000KEUR2000004545201004012010050155177 A
KSCAR  NSYDAULBAGB                    00001KEUR2000000115201004012010050155177 A";

			var originalPartitionSize = Env.Registry.Rating.TACTRateImportPartitionSize;

			try
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = 5;

				var actual = Import(tactRates, new TACTData80FixedWidthDataFormat());
				var expected = Get80WidthExpectedRates();

				AssertEntries(expected, actual);
			}
			finally
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = originalPartitionSize;
			}
		}

		[TestDate(2010, 4, 10)]
		public void TestImport_TACTLinesAreInRandomOrder_80()
		{
			var tactRates = @"
BSCAR  NSYDAULBAGB                    00000KEUR2000004545201004012010050155177 A
CS590  ASYDAULONGB        000006 05  H00000KAUD200127000019991001        55837 A
CS590  ASYDAULONGB        000006 08  H00000KAUD200042600019991001        55837 A
GC550  ASYDAUBODFR                    00001KAUD200000145519990405        55068 A
GC550  ASYDAUBODFR                    00045KAUD200000112519991001        55837 A
GC550  ASYDAUBODFR                    00100KAUD200000077519991001        55837 A
GC550  ASYDAUBODFR                    00250KAUD200000050019991001        55837 A
GC550  ASYDAUBODFR                    00500KAUD200000038519991001        55837 A
GC550  ASYDAUBODFR                    01000KAUD200000035019991001        55837 A
GS550  ASYDAULAXUS                    00001KAUD200000139519990405        74076 A
GS550  ASYDAULAXUS                    00045KAUD200000066019990405        74076 A
GS550  ASYDAULAXUS                    00100KAUD200000040019990405        74076 A
GS550  ASYDAULAXUS                    00300KAUD200000034019990405        74076 A
GS550  ASYDAULAXUS                    00500KAUD200000032019990405        74076 A
GS550  ASYDAULAXUS                    01000KAUD200000029519990405        74080 A
GS550  ASYDAULONGB                    00001KAUD200000153519990405        55068 A
GS550  ASYDAULONGB                    00045KAUD200000118519991001        55837 A
GS550  ASYDAULONGB                    00100KAUD200000077519991001        55837 A
GS550  ASYDAULONGB                    00250KAUD200000050019991001        55837 A
GS550  ASYDAULONGB                    00500KAUD200000038519991001        55837 A
GS550  ASYDAULONGB                    01000KAUD200000035019991001        55837 A
KSCAR  NSYDAULBAGB                    00001KEUR2000000115201004012010050155177 A
MC501  ASYDAUBODFR                    00000KAUD200000850019981001        55073 A
MC501  ASYDAULAXUS                    00000KAUD200000800019981001        74083 A
MC501  ASYDAULONGB                    00000KAUD200000850019981001        55073 A
SC590  ASYDAUBODFR        000006      00100KAUD200000041019991001        55837 A
SC590  ASYDAUBODFR        000006      00500KAUD200000038019991001        55837 A
SC590  ASYDAUBODFR        001420      00045KAUD200000050519991001        55837 A
SS590  ASYDAULAXUS        001420      00045KAUD200000047519990405        74076 A
SS590  ASYDAULONGB        000006      00100KAUD200000041019991001        55837 A
SS590  ASYDAULONGB        000006      00500KAUD200000038019991001        55837 A
SS590  ASYDAULONGB        001420      00045KAUD200000050519991001        55837 A
US530  ASYDAULAXUS                   C00000KAUD200000021019991101        74740 A
US530  ASYDAULAXUS               05  B02500KAUD200073400019991101        74740 A
US530  ASYDAULAXUS               08  B00925KAUD200027300019991101        74740 A";

			var originalPartitionSize = Env.Registry.Rating.TACTRateImportPartitionSize;

			try
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = 5;

				var actual = Import(tactRates, new TACTData80FixedWidthDataFormat());
				var expected = Get80WidthExpectedRates();

				AssertEntries(expected, actual);
			}
			finally
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = originalPartitionSize;
			}
		}

		IEnumerable<IRateEntry> Get80WidthExpectedRates()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<RatingHeader>();

			var rate = header.AddRateEntry("AIR", "LSE", "SYD", "LAX", ZString.Empty, ZString.Empty);
			rate.TI_RateStartDate = new ZDate(1999, 04, 05);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			var line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			var calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 80m;
			calc["-45"] = (ZDecimal)13.95m;
			calc["+45"] = (ZDecimal)6.60;
			calc["+100"] = (ZDecimal)4m;
			calc["+300"] = (ZDecimal)3.4m;
			calc["+500"] = (ZDecimal)3.2m;
			calc["+1000"] = (ZDecimal)2.95m;

			rate = header.AddRateEntry("AIR", "LSE", "SYD", "BOD", ZString.Empty, ZString.Empty);
			rate.TI_RateStartDate = new ZDate(1999, 10, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 85m;
			calc["-45"] = (ZDecimal)14.55m;
			calc["+45"] = (ZDecimal)11.25;
			calc["+100"] = (ZDecimal)7.75m;
			calc["+250"] = (ZDecimal)5.0m;
			calc["+500"] = (ZDecimal)3.85m;
			calc["+1000"] = (ZDecimal)3.50m;

			rate = header.AddRateEntry("AIR", "LSE", "SYD", "LON", ZString.Empty, ZString.Empty);
			rate.TI_RateStartDate = new ZDate(1999, 10, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 85m;
			calc["-45"] = (ZDecimal)15.35m;
			calc["+45"] = (ZDecimal)11.85;
			calc["+100"] = (ZDecimal)7.75m;
			calc["+250"] = (ZDecimal)5.0m;
			calc["+500"] = (ZDecimal)3.85m;
			calc["+1000"] = (ZDecimal)3.50m;

			rate = header.AddRateEntry("AIR", "LSE", "SYD", "LBA", ZString.Empty, ZString.Empty);
			rate.TI_RateStartDate = new ZDate(1999, 10, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", FlatPlusPerUnitCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			var calc2 = line.GetCalculator<FlatPlusPerUnitCalculator>();
			calc2.BaseRate = 45.45m;
			calc2.PerUnit = 1.15m;

			return header.AllEntries;
		}

		#endregion

		#region 150 Width Test

		[TestDate(2009, 4, 10)]
		public void TestImport_TACTLinesAreProperlyOrdered_150()
		{
			var expected = Get150WidthExpectedRates();

			var tactRates = @"
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00001K                                 EUR20000012002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00045K                                 EUR20000009002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00100K                                 EUR20000006002019090920190909        A          AY  A  
MC5B   CAR  N29870FRADE36630SYDAU    AY                 00000K                                 EUR20000130002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00001K                                 EUR20000005602016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00100K                                 EUR20000004352016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00300K                                 EUR20000004182016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00500K                                 EUR20000004022016010120160101        A          BA  A  
MC5B   CAR  N29870FRADE36630IEVUA    BA                 00000K                                 EUR20000077802016010120160101        A          BA  A 
GS5B   CAR  N29870FRADE36630LAXUS    CX                 00001K                                 EUR20000005302015030120150301        A          CX  A  
GS5B   CAR  N29870FRADE36630LAXUS    CX                 00045K                                 EUR20000004802015030120150301        A          CX  A  
MC5B   CAR  N29870FRADE36630LAXUS    CX                 00000K                                 EUR20000081002015030120150301        A          CX  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00001K                                 EUR20000005602016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00100K                                 EUR20000004352016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00300K                                 EUR20000004182016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00500K                                 EUR20000004022016010120160101        A          IB  A  
MC5B   CAR  N29870FRADE36630SINSG    IB                 00000K                                 EUR20000077802016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00001K                                 EUR20000004492011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00100K                                 EUR20000003242011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00300K                                 EUR20000003072011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00500K                                 EUR20000002912011060120110601        A          JL  A  
MC5B   CAR  N29870FRADE36630MELAU    JL                 00000K                                 EUR20000076692011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00001K                                 EUR20000004492014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00100K                                 EUR20000003242014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00300K                                 EUR20000003072014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00500K                                 EUR20000002912014080120140801        A          KE  A  
MC5B   CAR  N29870FRADE36630AKLNZ    KE                 00000K                                 EUR20000076692014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00001K                                 EUR20000004492013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00100K                                 EUR20000003242013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00300K                                 EUR20000003072013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00500K                                 EUR20000002912013060120130601        A          KZ  A  
MC5B   CAR  N29870FRADE36630LVOUA    KZ                 00000K                                 EUR20000100002013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00001K                                 EUR20000004842015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00100K                                 EUR20000003582015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00500K                                 EUR20000003322015110120151101        A          LH  A  
MC5B   CAR  N29870FRADE36630NYCUS    LH                 00000K                                 EUR20000097002015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00001K                                 EUR20000007892015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00100K                                 EUR20000007602015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00500K                                 EUR20000007312015110120151101        A          LH  A  
MC5B   CAR  N29870FRADE36630BNEAULH40LH                 00000K                                 EUR20000097002015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00001K                                 EUR20000004842015120120151201        A          LX  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00100K                                 EUR20000003582015120120151201        A          LX  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00500K                                 EUR20000003322015120120151201        A          LX  A  
MC5B   CAR  N29870FRADE36630LONGB    LX                 00000K                                 EUR20000097002015120120151201        A          LX  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00001K                                 EUR20000004712016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00100K                                 EUR20000003402016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00300K                                 EUR20000003222016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00500K                                 EUR20000003062016100120161001        A          SQ  A  
MC5B   CAR  N29870SYDAU36630HKGHK    SQ                 00000K                                 EUR20000080522016100120161001        A          SQ  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00001K                                 EUR20000004262010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00100K                                 EUR20000003062010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00300K                                 EUR20000002922010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00500K                                 EUR20000002762010100120101001        A          TG  A  
MC5B   CAR  N29870IEVUA36630HKGHK    TG                 00000K                                 EUR20000070552010100120101001        A          TG  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00001K                                 EUR20000005692016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00100K                                 EUR20000004442016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00300K                                 EUR20000004272016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00500K                                 EUR20000004112016060120160601        A          UA  A   
MC5B   CAR  N29870LVOUA36630HKGHK    UA                 00000K                                 EUR20000081692016060120160601        A          UA  A  ";

			var originalPartitionSize = Env.Registry.Rating.TACTRateImportPartitionSize;

			try
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = 5;

				var actual = Import(tactRates, new TACTData150FixedWidthDataFormat());
				AssertEntries(expected, actual);
			}
			finally
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = originalPartitionSize;
			}
		}

		[TestDate(2009, 4, 10)]
		public void TestImport_TACTLinesAreInRandomOrder_150()
		{
			var expected = Get150WidthExpectedRates();

			var tactRates = @"
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00001K                                 EUR20000004492014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00100K                                 EUR20000003242014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00300K                                 EUR20000003072014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630AKLNZ    KE                 00500K                                 EUR20000002912014080120140801        A          KE  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00001K                                 EUR20000007892015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00100K                                 EUR20000007602015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630BNEAULH40LH                 00500K                                 EUR20000007312015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00001K                                 EUR20000005602016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00100K                                 EUR20000004352016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00300K                                 EUR20000004182016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630IEVUA    BA                 00500K                                 EUR20000004022016010120160101        A          BA  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00001K                                 EUR20000004492013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00100K                                 EUR20000003242013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00300K                                 EUR20000003072013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LVOUA    KZ                 00500K                                 EUR20000002912013060120130601        A          KZ  A  
GS5B   CAR  N29870FRADE36630LAXUS    CX                 00001K                                 EUR20000005302015030120150301        A          CX  A  
GS5B   CAR  N29870FRADE36630LAXUS    CX                 00045K                                 EUR20000004802015030120150301        A          CX  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00001K                                 EUR20000004842015120120151201        A          LX  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00100K                                 EUR20000003582015120120151201        A          LX  A  
GS5B   CAR  N29870FRADE36630LONGB    LX                 00500K                                 EUR20000003322015120120151201        A          LX  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00001K                                 EUR20000004492011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00100K                                 EUR20000003242011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00300K                                 EUR20000003072011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630MELAU    JL                 00500K                                 EUR20000002912011060120110601        A          JL  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00001K                                 EUR20000004842015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00100K                                 EUR20000003582015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630NYCUS    LH                 00500K                                 EUR20000003322015110120151101        A          LH  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00001K                                 EUR20000005602016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00100K                                 EUR20000004352016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00300K                                 EUR20000004182016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SINSG    IB                 00500K                                 EUR20000004022016010120160101        A          IB  A  
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00001K                                 EUR20000012002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00045K                                 EUR20000009002019090920190909        A          AY  A  
GS5B   CAR  N29870FRADE36630SYDAU    AY                 00100K                                 EUR20000006002019090920190909        A          AY  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00001K                                 EUR20000004262010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00100K                                 EUR20000003062010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00300K                                 EUR20000002922010100120101001        A          TG  A  
GS5B   CAR  N29870IEVUA36630HKGHK    TG                 00500K                                 EUR20000002762010100120101001        A          TG  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00001K                                 EUR20000005692016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00100K                                 EUR20000004442016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00300K                                 EUR20000004272016060120160601        A          UA  A  
GS5B   CAR  N29870LVOUA36630HKGHK    UA                 00500K                                 EUR20000004112016060120160601        A          UA  A   
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00001K                                 EUR20000004712016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00100K                                 EUR20000003402016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00300K                                 EUR20000003222016100120161001        A          SQ  A  
GS5B   CAR  N29870SYDAU36630HKGHK    SQ                 00500K                                 EUR20000003062016100120161001        A          SQ  A  
MC5B   CAR  N29870FRADE36630AKLNZ    KE                 00000K                                 EUR20000076692014080120140801        A          KE  A  
MC5B   CAR  N29870FRADE36630BNEAULH40LH                 00000K                                 EUR20000097002015110120151101        A          LH  A  
MC5B   CAR  N29870FRADE36630IEVUA    BA                 00000K                                 EUR20000077802016010120160101        A          BA  A 
MC5B   CAR  N29870FRADE36630LVOUA    KZ                 00000K                                 EUR20000100002013060120130601        A          KZ  A  
MC5B   CAR  N29870FRADE36630LAXUS    CX                 00000K                                 EUR20000081002015030120150301        A          CX  A  
MC5B   CAR  N29870FRADE36630LONGB    LX                 00000K                                 EUR20000097002015120120151201        A          LX  A  
MC5B   CAR  N29870FRADE36630MELAU    JL                 00000K                                 EUR20000076692011060120110601        A          JL  A  
MC5B   CAR  N29870FRADE36630NYCUS    LH                 00000K                                 EUR20000097002015110120151101        A          LH  A  
MC5B   CAR  N29870FRADE36630SINSG    IB                 00000K                                 EUR20000077802016010120160101        A          IB  A  
MC5B   CAR  N29870FRADE36630SYDAU    AY                 00000K                                 EUR20000130002019090920190909        A          AY  A  
MC5B   CAR  N29870IEVUA36630HKGHK    TG                 00000K                                 EUR20000070552010100120101001        A          TG  A  
MC5B   CAR  N29870LVOUA36630HKGHK    UA                 00000K                                 EUR20000081692016060120160601        A          UA  A  
MC5B   CAR  N29870SYDAU36630HKGHK    SQ                 00000K                                 EUR20000080522016100120161001        A          SQ  A  ";

			var originalPartitionSize = Env.Registry.Rating.TACTRateImportPartitionSize;

			try
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = 5;

				var actual = Import(tactRates, new TACTData150FixedWidthDataFormat());
				AssertEntries(expected, actual);
			}
			finally
			{
				Env.Registry.Rating.TACTRateImportPartitionSize = originalPartitionSize;
			}
		}

		IEnumerable<IRateEntry> Get150WidthExpectedRates()
		{
			var factory = new BusinessObjectFactory();

			var orgay = CreateCarrierOrganisation("AY", "ORGAY", factory);
			var orgba = CreateCarrierOrganisation("BA", "ORGBA", factory);
			var orgcx = CreateCarrierOrganisation("CX", "ORGCX", factory);
			var orgib = CreateCarrierOrganisation("IB", "ORGIB", factory);
			var orgjl = CreateCarrierOrganisation("JL", "ORGJL", factory);
			var orgke = CreateCarrierOrganisation("KE", "ORGKE", factory);
			var orgkz = CreateCarrierOrganisation("KZ", "ORGKZ", factory);
			var orglh = CreateCarrierOrganisation("LH", "ORGLH", factory);
			var orglx = CreateCarrierOrganisation("LX", "ORGLX", factory);
			var orgsq = CreateCarrierOrganisation("SQ", "ORGSQ", factory);
			var orgtg = CreateCarrierOrganisation("TG", "ORGTG", factory);
			var orgua = CreateCarrierOrganisation("UA", "ORGUA", factory);

			factory.Save();

			var header = factory.New<RatingHeader>();

			// AY
			var rate = header.AddRateEntry("AIR", "LSE", "FRA", "SYD", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgay.PK;
			rate.TI_RateStartDate = new ZDate(2019, 09, 09);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			var line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			var calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 130m;
			calc["-45"] = (ZDecimal)12m;
			calc["+45"] = (ZDecimal)9;
			calc["+100"] = (ZDecimal)6m;

			// BA
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "IEV", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgba.PK;
			rate.TI_RateStartDate = new ZDate(2016, 01, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 77.80m;
			calc["-100"] = (ZDecimal)5.6m;
			calc["+100"] = (ZDecimal)4.35;
			calc["+300"] = (ZDecimal)4.18m;
			calc["+500"] = (ZDecimal)4.02m;

			// CX
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "LAX", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgcx.PK;
			rate.TI_RateStartDate = new ZDate(2015, 03, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 81.00m;
			calc["-45"] = (ZDecimal)5.3m;
			calc["+45"] = (ZDecimal)4.80;

			// IB
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "SIN", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgib.PK;
			rate.TI_RateStartDate = new ZDate(2016, 01, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 77.80m;
			calc["-100"] = (ZDecimal)5.60m;
			calc["+100"] = (ZDecimal)4.35;
			calc["+300"] = (ZDecimal)4.18;
			calc["+500"] = (ZDecimal)4.02;

			// JL
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "MEL", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgjl.PK;
			rate.TI_RateStartDate = new ZDate(2011, 06, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 76.69m;
			calc["-100"] = (ZDecimal)4.49m;
			calc["+100"] = (ZDecimal)3.24;
			calc["+300"] = (ZDecimal)3.07;
			calc["+500"] = (ZDecimal)2.91;

			// KE
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "AKL", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgke.PK;
			rate.TI_RateStartDate = new ZDate(2014, 08, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 76.69m;
			calc["-100"] = (ZDecimal)4.49m;
			calc["+100"] = (ZDecimal)3.24;
			calc["+300"] = (ZDecimal)3.07;
			calc["+500"] = (ZDecimal)2.91;

			// KZ
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "LVO", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgkz.PK;
			rate.TI_RateStartDate = new ZDate(2013, 06, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 100.00m;
			calc["-100"] = (ZDecimal)4.49m;
			calc["+100"] = (ZDecimal)3.24;
			calc["+300"] = (ZDecimal)3.07;
			calc["+500"] = (ZDecimal)2.91;

			// LH
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "NYC", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orglh.PK;
			rate.TI_RateStartDate = new ZDate(2015, 11, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 97.00;
			calc["-100"] = (ZDecimal)4.84m;
			calc["+100"] = (ZDecimal)3.58;
			calc["+500"] = (ZDecimal)3.32;

			// LH LH40
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "BNE", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orglh.PK;
			rate.TI_RateStartDate = new ZDate(2015, 11, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.TI_PL_NKCarrierServiceLevel = "LH4";
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 97.00;
			calc["-100"] = (ZDecimal)7.89m;
			calc["+100"] = (ZDecimal)7.60;
			calc["+500"] = (ZDecimal)7.31;

			// LX
			rate = header.AddRateEntry("AIR", "LSE", "FRA", "LON", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orglx.PK;
			rate.TI_RateStartDate = new ZDate(2015, 12, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 97.00;
			calc["-100"] = (ZDecimal)4.84m;
			calc["+100"] = (ZDecimal)3.58;
			calc["+500"] = (ZDecimal)3.32;

			// SQ
			rate = header.AddRateEntry("AIR", "LSE", "SYD", "HKG", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgsq.PK;
			rate.TI_RateStartDate = new ZDate(2016, 01, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 80.52;
			calc["-100"] = (ZDecimal)4.71m;
			calc["+100"] = (ZDecimal)3.40;
			calc["+300"] = (ZDecimal)3.22;
			calc["+500"] = (ZDecimal)3.06;

			// TG
			rate = header.AddRateEntry("AIR", "LSE", "IEV", "HKG", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgtg.PK;
			rate.TI_RateStartDate = new ZDate(2010, 01, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 70.55;
			calc["-100"] = (ZDecimal)4.26m;
			calc["+100"] = (ZDecimal)3.06;
			calc["+300"] = (ZDecimal)2.92;
			calc["+500"] = (ZDecimal)2.76;

			// UA
			rate = header.AddRateEntry("AIR", "LSE", "LVO", "HKG", ZString.Empty, ZString.Empty);
			rate.TI_OH_TransportProvider = orgua.PK;
			rate.TI_RateStartDate = new ZDate(2016, 06, 01);
			rate.TI_RateEndDate = ZDate.Empty;
			rate.RateLines.RemoveAndDeleteAll();
			line = rate.AddRateLine("FRT", CombinedCalculator.Code, "KG", "EUR");
			line.RateLineItems.RemoveAndDeleteAll();
			calc = line.GetCalculator<CombinedCalculator>();
			calc.Minimum = 81.69;
			calc["-100"] = (ZDecimal)5.69m;
			calc["+100"] = (ZDecimal)4.44;
			calc["+300"] = (ZDecimal)4.27;
			calc["+500"] = (ZDecimal)4.11;

			return header.AllEntries;
		}

		#endregion

		static RateEntry[] Import(string tactRates, FixedWidthFlatFileFormat format, TACTImporter.ExistingRatesHandlingStrategy cleanupStrategy = TACTImporter.ExistingRatesHandlingStrategy.None, bool isJobLevelCharge = true, string rounding = RatingRoundingTypes.Chargeable, bool shouldExcludeFromAutoRating = false)
		{
			return Import(tactRates, format, out TACTImportResult _, out NotificationBuffer _, cleanupStrategy, isJobLevelCharge, rounding, shouldExcludeFromAutoRating);
		}

		static RateEntry[] Import(string tactRates, FixedWidthFlatFileFormat format, out TACTImportResult result, out NotificationBuffer notifications, TACTImporter.ExistingRatesHandlingStrategy cleanupStrategy = TACTImporter.ExistingRatesHandlingStrategy.None, bool isJobLevelCharge = true, string rounding = RatingRoundingTypes.Chargeable, bool shouldExcludeFromAutoRating = false)
		{
			var logs = new NotificationBuffer();
			var factory = new BusinessObjectFactory();
			var costing = factory.LoadTop1<Costing>(new ZQuery()) ?? factory.New<Costing>();
			factory.Save();

			var companyPK = costing.TH_GC.IsEmpty ? Guid.Empty : costing.TH_GC.ToGuid();
			var importOptions = new TACTImportOptions()
			{
				CompanyPK = companyPK,
				RatingHeaderPK = costing.PK.ToGuid(),
				IsJobLevelCharge = isJobLevelCharge,
				Rounding = rounding,
				ShouldExcludeFromAutoRating = shouldExcludeFromAutoRating
			};

			var importer = new TACTImporter(GetStream(tactRates), format, logs, importOptions);
			result = importer.ImportAsync(cleanupStrategy).GetAwaiter().GetResult();

			var reloadedCosting = new BusinessObjectFactory().LoadTop1<Costing>(new ZQuery());
			var airRateEntries = reloadedCosting.EntryCollections[RatingConstants.RateCategory.AIR].LoadedCollection;

			notifications = logs;
			return airRateEntries.Cast<RateEntry>().OrderBy(e => e.TI_RateStartDate).ThenBy(e => e.TI_DestinationLRC).ToArray();
		}

		static void Import(string tactRates, FixedWidthFlatFileFormat format, int batchSize, bool invokeTimeoutException, TACTImporter.ExistingRatesHandlingStrategy cleanupStrategy = TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, TACTImportOptions importOptions = null, bool isJobLevelCharge = true, string rounding = RatingRoundingTypes.Chargeable, bool shouldExcludeFromAutoRating = true)
		{
			var logs = new NotificationBuffer();

			if (importOptions == null)
			{
				var factory = new BusinessObjectFactory();
				var costing = factory.LoadTop1<Costing>(new ZQuery()) ?? factory.New<Costing>();
				factory.Save();

				var companyPK = costing.TH_GC.IsEmpty ? Guid.Empty : costing.TH_GC.ToGuid();
				importOptions = new TACTImportOptions()
				{
					CompanyPK = companyPK,
					RatingHeaderPK = costing.PK.ToGuid(),
					IsJobLevelCharge = isJobLevelCharge,
					Rounding = rounding,
					ShouldExcludeFromAutoRating = shouldExcludeFromAutoRating
				};
			}

			var importer = new TACTImporterForTest(GetStream(tactRates), format, logs, importOptions);
			importer.RowsDeleteBatchSize = batchSize;
			importer.ImportAsyncTest(cleanupStrategy, invokeTimeoutException);
		}

		static void Import(string testFilePath, TACTImporter.ExistingRatesHandlingStrategy cleanupStrategy = TACTImporter.ExistingRatesHandlingStrategy.ClearTACTRates, TACTImportOptions importOptions = null, bool isJobLevelCharge = true, string rounding = RatingRoundingTypes.Chargeable, bool shouldExcludeFromAutoRating = true)
		{
			FileStream stream = new FileStream(testFilePath, FileMode.Open, FileAccess.Read);
			var formatter = GetFormatter(testFilePath, stream);
			if (formatter == null)
			{
				return;
			}

			var logs = new NotificationBuffer();

			if (importOptions == null)
			{
				var factory = new BusinessObjectFactory();
				var costing = factory.LoadTop1<Costing>(new ZQuery()) ?? factory.New<Costing>();
				factory.Save();
				var companyPK = costing.TH_GC.IsEmpty ? Guid.Empty : costing.TH_GC.ToGuid();

				importOptions = new TACTImportOptions()
				{
					CompanyPK = companyPK,
					RatingHeaderPK = costing.PK.ToGuid(),
					IsJobLevelCharge = isJobLevelCharge,
					Rounding = rounding,
					ShouldExcludeFromAutoRating = shouldExcludeFromAutoRating
				};
			}

			stream.Seek(0, SeekOrigin.Begin);
			var importer = new TACTImporter(stream, formatter, logs, importOptions);
			importer.ImportAsync(cleanupStrategy).GetAwaiter().GetResult();
		}

		void AssertEntries(IEnumerable<IRateEntry> expected, IEnumerable<IRateEntry> actual)
		{
			Func<IRateLineItem, string> getComparableRateLineItem = i =>
			{
				return $"{i.TM_Type}|{i.TM_Break:F4}|{i.TM_RelevantValue:F4}|{i.TM_FlatAmount:F4}|{i.TM_Text}";
			};

			Func<IRateLine, string> getComparableRateLine = r =>
			{
				return $"{r.ChargeCode.AC_Code}|{string.Join(",", r.ChildRateLineItems.OrderBy(i => i.TM_Break).ThenBy(i => i.TM_RelevantValue).Select(getComparableRateLineItem))}";
			};

			Func<IRateEntry, string> getComparableObject = r =>
			{
				return $"{r.TI_OriginLRC}|{r.TI_DestinationLRC}|{r.TransportProvider?.OH_Code}|{r.TI_PL_NKCarrierServiceLevel}|{string.Join(",", r.ChildRateLines.OrderBy(l => l.ChargeCode.AC_Code).Select(getComparableRateLine))}";
			};

			var exp = expected.Select(getComparableObject).ToArray();
			var act = actual.Select(getComparableObject).ToArray();

			AssertContainsExactElementsInAnyOrder(exp, act);
		}

		void AssertEntry(RateEntry entry, string origin, string destination, ZDate startDate, ZDate endDate)
		{
			CombineAssertions("Expected Entry should match", () =>
			{
				AssertEquals("Mode", Core.Constants.RateMode.LSE, entry.TI_Mode);
				AssertEquals("Origin", origin, entry.TI_OriginLRC);
				AssertEquals("Destination", destination, entry.TI_DestinationLRC);
				AssertEquals("Start Date", startDate, entry.TI_RateStartDate);
				AssertEquals("End Date", endDate, entry.TI_RateEndDate);
			});
		}

		static Stream GetStream(string s)
		{
			return new MemoryStream(Encoding.UTF8.GetBytes(s));
		}

		OrgHeader CreateCarrierOrganisation(string carrierCode, string orgCode, BusinessObjectFactory factory = null)
		{
			var saveFactory = false;

			if (factory == null)
			{
				factory = new BusinessObjectFactory();
				saveFactory = true;
			}

			RefAirline airline = null;

			if (carrierCode.Length == 2)
			{
				airline = factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, carrierCode));
			}
			else if (carrierCode.Length == 3)
			{
				airline = factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_ThreeLetterCode, carrierCode));
			}

			var organisation = factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = orgCode;
			organisation.MiscServ.OM_RM_Airline = airline.PK;

			if (saveFactory)
			{
				factory.Save();
			}

			return organisation;
		}

		static FixedWidthFlatFileFormat GetFormatter(string fileName, Stream stream)
		{
			var length = GetTACTLinesLength(stream);
			if (length == null)
			{
				return null;
			}

			var fileExtention = Path.GetExtension(fileName);

			if (fileExtention.EndsWith(TACTData150FixedWidthDataFormat.Constants.FileFormat, StringComparison.OrdinalIgnoreCase) ||
				length == TACTData150FixedWidthDataFormat.Constants.RowLength)
			{
				return new TACTData150FixedWidthDataFormat();
			}

			if (fileExtention.EndsWith(TACTData80FixedWidthDataFormat.Constants.FileFormat, StringComparison.OrdinalIgnoreCase) ||
				length == TACTData80FixedWidthDataFormat.Constants.RowLength)
			{
				return new TACTData80FixedWidthDataFormat();
			}

			return null;
		}

		static int? GetTACTLinesLength(Stream stream)
		{
			int? length = null;

			stream.Seek(0, SeekOrigin.Begin);

			using (var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, true))
			{
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					if (string.IsNullOrEmpty(line))
					{
						// We just ingore empty lines
						continue;
					}

					if (length == null)
					{
						length = line.Length;
						continue;
					}

					if (length != line.Length)
					{
						return null;
					}
				}
			}

			return length;
		}

		class TACTImporterForTest : TACTImporter
		{
			protected bool SqlExceptionForTest { get; set; }
			public TACTImporterForTest(Stream stream, FixedWidthFlatFileFormat format, INotifications notifications, TACTImportOptions importOptions)
				: base(stream, format, notifications, importOptions)
			{
			}

			public TACTImportResult ImportAsyncTest(ExistingRatesHandlingStrategy cleanupStrategy, bool invokeTimeoutException)
			{
				SqlExceptionForTest = invokeTimeoutException;
				return base.ImportAsync(cleanupStrategy).GetAwaiter().GetResult();
			}

			public override int GetExistingRateBacklog(Guid ratingHeaderPK, bool clearTACTRates, bool clearStandardRates, DbConnection connection)
			{
				if (SqlExceptionForTest)
				{
					throw (CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired."));
				}
				return base.GetExistingRateBacklog(ratingHeaderPK, clearTACTRates, clearStandardRates, connection);
			}
		}
	}
}