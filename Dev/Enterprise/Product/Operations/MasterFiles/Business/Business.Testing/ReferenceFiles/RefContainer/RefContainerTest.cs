using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefContainer))]
	sealed class RefContainerTest : EnterpriseBusinessObjectTestCase
	{
		#region Boolean IsXXX properties

		public void TestDefaultIsISO()
		{
			var container = Factory.New<RefContainer>();
			container.RC_ISOType = "40G0";
			Assert(container.RC_IsIso);
			container.RC_ISOType = "AAAA";
			Assert(!container.RC_IsIso);
		}

		public void TestIs20GP()
		{
			AssertContainerTypeBooleanProperty(Constants.ContainerTypes.DryStorage, 20m, (container) => container.Is20GP);
		}

		public void TestIs40GP()
		{
			AssertContainerTypeBooleanProperty(Constants.ContainerTypes.DryStorage, 40m, (container) => container.Is40GP);
		}

		public void TestIs20RE()
		{
			AssertContainerTypeBooleanProperty(Constants.ContainerTypes.Refrigerated, 20m, (container) => container.Is20RE);
		}

		public void TestIs40RE()
		{
			AssertContainerTypeBooleanProperty(Constants.ContainerTypes.Refrigerated, 40m, (container) => container.Is40RE);
		}

		void AssertContainerTypeBooleanProperty(string containerType, decimal acceptedLength, Predicate<RefContainer> checkProperty)
		{
			RefContainer container = Factory.New<RefContainer>();
			container.RC_Length = acceptedLength;
			container.RC_ContainerType = "XXX";
			AssertEquals(false, checkProperty(container));
			AssertEquals(true, container.IsOtherContainerType);

			container.RC_ContainerType = containerType;
			AssertEquals(true, checkProperty(container));
			AssertEquals(false, container.IsOtherContainerType);

			container.RC_Length = acceptedLength + 2m;
			AssertEquals(false, checkProperty(container));
			AssertEquals(true, container.IsOtherContainerType);

			container.RC_Length = acceptedLength - 2m;
			AssertEquals(false, checkProperty(container));
			AssertEquals(true, container.IsOtherContainerType);
		}

		#endregion

		public void TestRC_Calc_StorageClass()
		{
			var container = Factory.New<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			container.RC_StorageClass = "MD";
			container.RC_ContainerType = Constants.ContainerTypes.Refrigerated;

			AssertEquals("Should be Main Deck / Refrigerated", "MDR", container.RC_Calc_StorageClass);

			container.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			AssertEquals("Should be Main Deck", "MD", container.RC_Calc_StorageClass);

			container.RC_StorageClass = "LD";
			AssertEquals("Should be Lower Deck", "LD", container.RC_Calc_StorageClass);

			container.RC_ContainerType = Constants.ContainerTypes.Refrigerated;
			AssertEquals("Should be Lower Deck / Refrigerated", "LDR", container.RC_Calc_StorageClass);

			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Sea;
			AssertEquals("Air storage class should be empty", ZString.Empty, container.RC_Calc_StorageClass);
		}

		public void TestAnyZUnit()
		{
			var factory = new BusinessObjectFactory();
			var gp20 = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var containerCount = new ZAmount(4, gp20, "S1");
			var rate = ZAmount.CreateConversionFactor(new ZAmount(250m, RefCurrency.LoadFromCurrencyCode(factory, "AUD"), "S2"), ZUnit.RefContainer.Any);

			var total = containerCount * rate;

			AssertAmount(total, "1000 AUD", "4 20GP x 250 AUD/CN", "S1\r\nS2");
		}

		public void TestOneContainer()
		{
			var factory = new BusinessObjectFactory();
			var gp20 = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var containerCount = new ZAmount(1, gp20, "S1");
			var rate = ZAmount.CreateConversionFactor(new ZAmount(250m, RefCurrency.LoadFromCurrencyCode(factory, "AUD"), "S2"), ZUnit.RefContainer.Any);

			var total = containerCount * rate;

			AssertAmount(total, "250 AUD", "1 20GP x 250 AUD/CN", "S1\r\nS2");
		}

		void AssertAmount(ZAmount amount, string expectedStr, string expectedFormula, string expectedSource)
		{
			CombineAssertions(() =>
			{
				AssertEquals("ToString()", expectedStr, amount.ToString());
				AssertEquals("source", expectedSource, amount.Source);
				AssertEquals("formula", expectedFormula, amount.ToFormula());
			});
		}

		public void TestShippingMode()
		{
			RefContainer container = RefContainer.New(Factory);
			AssertEquals("By default, Sea", "SEA", container.RC_ShippingMode);
			Assert(container.IsSeaContainer);
			Assert(!container.IsAirContainer);
			Assert(container.RC_IATARateClassInfo.ReadOnly);

			container.RC_ShippingMode = "AIR";
			container.RC_IATARateClass = "11";
			Assert(!container.IsSeaContainer);
			Assert(container.IsAirContainer);
			Assert(!container.RC_IATARateClassInfo.ReadOnly);

			container.RC_ShippingMode = "SEA";
			AssertEquals(ZString.Empty, container.RC_IATARateClass);
			container.RC_HasTynes = true;
			container.RC_HasVents = true;
			container.RC_IsHighCube = true;
			container.RC_TEU = 5m;
			container.RC_IsIso = true;
			container.RC_ISOType = "AABB";
			container.RC_ContainerType = "ZZZ";

			container.RC_ShippingMode = "AIR";
			Assert(!container.RC_HasTynes);
			Assert(!container.RC_HasVents);
			Assert(!container.RC_IsHighCube);
			Assert(!container.RC_IsIso);
			AssertEquals(0m, container.RC_TEU);
			AssertEquals(ZString.Empty, container.RC_ContainerType);
			AssertEquals(ZString.Empty, container.RC_ISOType);
			AssertEquals(true, container.RC_ISOTypeInfo.ReadOnly);
		}

		public void TestRC_USContainerCode()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_Code = "CNT1";
			AssertNullOrEmpty(container.GetCountrySpecificContainerCode("US"));

			var xxMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			xxMap.RCM_RN_NKCountry = "US";
			xxMap.RCM_RC_Container = ZGuid.NewZGuid();
			xxMap.RCM_Code = "US2";

			var cnMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			cnMap.RCM_RN_NKCountry = "CN";
			cnMap.RCM_RC_Container = container.PK;
			cnMap.RCM_Code = "CN1";

			var usMap = Factory.NewWithValidTestData<RefContainerCodeMap>();
			usMap.RCM_RN_NKCountry = "US";
			usMap.RCM_RC_Container = container.PK;
			usMap.RCM_Code = "US1";

			container.CodeMapCollection.Load();

			AssertEquals("US1", container.GetCountrySpecificContainerCode("US"));

			xxMap.Delete();
			cnMap.Delete();
			usMap.Delete();
			container.CodeMapCollection.Load();

			AssertNullOrEmpty(container.GetCountrySpecificContainerCode("US"));

			container.SetCountrySpecificContainerCode("U2", "US");
			var loadQuery = new ZQuery();
			loadQuery.AddToFilter(RefContainerCodeMapSchema.RCM_RN_NKCountry, "US");
			loadQuery.AddToFilter(RefContainerCodeMapSchema.RCM_RC_Container, container.PK);
			loadQuery.AddToFilter(RefContainerCodeMapSchema.RCM_Code, "U2");

			AssertNotNull(Factory.Load<RefContainerCodeMap>(loadQuery).FirstOrDefault());

			container.SetCountrySpecificContainerCode("", "US");
			AssertNull(Factory.Load<RefContainerCodeMap>(loadQuery).FirstOrDefault());
		}

		public void TestMatchesClass()
		{
			RefContainer container20GP = RefContainer.New(Factory);
			container20GP.RC_Code = "21GP";
			container20GP.RC_FreightRateClass = "FRE2";

			RefContainer container40GP = RefContainer.New(Factory);
			container40GP.RC_Code = "41GP";
			container40GP.RC_FreightRateClass = "FRE1";

			RefContainer container40HC = RefContainer.New(Factory);
			container40HC.RC_Code = "41HC";
			container40HC.RC_FreightRateClass = "FRE1";

			Factory.Save();

			Assert("Freight Class should not match", !container20GP.MatchesClass(container40GP));
			Assert("Freight Class should not match", !container20GP.MatchesClass("41GP"));

			Assert("Freight Class should match", container40GP.MatchesClass(container40HC));
			Assert("Freight Class should match", container40GP.MatchesClass("41HC"));

			Assert("Freight Class should match", container40GP.MatchesClass("41GP"));
		}

		public void TestContainersInSameRateClass()
		{
			RefContainer container1 = RefContainer.New(Factory);
			container1.RC_FreightRateClass = "FRE1";
			container1.RC_HandlingRateClass = "HAN1";
			container1.RC_Code = "Z1";

			RefContainer container2 = RefContainer.New(Factory);
			container2.RC_Code = "Z2";

			RefContainer container3 = RefContainer.New(Factory);
			container3.RC_FreightRateClass = "FRE1";
			container3.RC_HandlingRateClass = "HAN2";
			container3.RC_Code = "Z3";

			RefContainer container4 = RefContainer.New(Factory);
			container4.RC_Code = "Z4";

			AssertEquals("1 container in same freight class as Container 1", 1, container1.ContainersInSameFreightRateClass.Count);
			AssertEquals("Container 3 in same freight class as container 1", container3, container1.ContainersInSameFreightRateClass[0]);
			AssertEquals("1 container in same fregith class as Container 3", 1, container3.ContainersInSameFreightRateClass.Count);
			AssertEquals("Container 1 in same freight class as container 3", container1, container3.ContainersInSameFreightRateClass[0]);

			AssertEquals("No containers in same freight class as Container 2 as class is blank", 0, container2.ContainersInSameFreightRateClass.Count);
			AssertEquals("No containers in same freight class as Container 4 as class is blank", 0, container4.ContainersInSameFreightRateClass.Count);

			AssertEquals("0 containers in same handling class as Container 1", 0, container1.ContainersInSameHandlingRateClass.Count);
			AssertEquals("0 containers in same handling class as Container 3", 0, container3.ContainersInSameHandlingRateClass.Count);
			AssertEquals("0 containers in same handling class as Container 2", 0, container2.ContainersInSameHandlingRateClass.Count);

			container3.RC_HandlingRateClass = "HAN1";
			AssertEquals("1 container in same handling class as Container 3", 1, container3.ContainersInSameHandlingRateClass.Count);
			AssertEquals("1 container in same handling class as Container 1", 1, container1.ContainersInSameHandlingRateClass.Count);
		}

		#region Test Calculated Dimensions Properties

		public void TestLength()
		{
			RefContainer container = RefContainer.New(Factory);
			container.LengthFeetOnly = 8;
			AssertEquals(8m, container.RC_Length);
			AssertEquals(2.438m, Utilities.Round((decimal)container.LengthMetres, 3));
			AssertEquals(243.84m, Utilities.Round((decimal)container.LengthCentimetres, 3));
			AssertEquals(96m, Utilities.Round((decimal)container.LengthInches, 3));

			container.LengthInchesOnly = 6;
			AssertEquals(8.5m, container.RC_Length);
			AssertEquals(2.591m, Utilities.Round((decimal)container.LengthMetres, 3));
			AssertEquals(259.08m, Utilities.Round((decimal)container.LengthCentimetres, 3));
			AssertEquals(102m, Utilities.Round((decimal)container.LengthInches, 3));

			container.LengthMetres = 3;
			AssertEquals(9.843m, Utilities.Round((decimal)container.RC_Length, 3));
			AssertEquals(9, (int)container.LengthFeetOnly);
			AssertEquals(10.12m, Utilities.Round((decimal)container.LengthInchesOnly, 2));
			AssertEquals(300.015m, Utilities.Round((decimal)container.LengthCentimetres, 3));
			AssertEquals(118.116m, Utilities.Round((decimal)container.LengthInches, 3));

			container.RC_Length = new ZDecimal(8.6m);
			AssertEquals(2.621m, Utilities.Round((decimal)container.LengthMetres, 3));
			AssertEquals(8, (int)container.LengthFeetOnly);
			AssertEquals(7.20m, Utilities.Round((decimal)container.LengthInchesOnly, 2));
			AssertEquals(262.128m, Utilities.Round((decimal)container.LengthCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.LengthInches, 3));

			container.LengthCentimetres = 123;
			AssertEquals(4.035m, Utilities.Round((decimal)container.RC_Length, 3));
			AssertEquals(48.42m, Utilities.Round((decimal)container.LengthInches, 3));

			container.LengthInches = 98;
			AssertEquals(8.167m, Utilities.Round((decimal)container.RC_Length, 3));
			AssertEquals(248.93m, Utilities.Round((decimal)container.LengthCentimetres, 3));
		}

		public void TestHeight()
		{
			RefContainer container = RefContainer.New(Factory);
			container.HeightFeetOnly = 8;
			AssertEquals(8m, container.RC_Height);
			AssertEquals(2.438m, Utilities.Round((decimal)container.HeightMetres, 3));
			AssertEquals(243.84m, Utilities.Round((decimal)container.HeightCentimetres, 3));
			AssertEquals(96m, Utilities.Round((decimal)container.HeightInches, 3));

			container.HeightInchesOnly = 6;
			AssertEquals(8.5m, container.RC_Height);
			AssertEquals(2.591m, Utilities.Round((decimal)container.HeightMetres, 3));
			AssertEquals(259.08m, Utilities.Round((decimal)container.HeightCentimetres, 3));
			AssertEquals(102m, Utilities.Round((decimal)container.HeightInches, 3));

			container.HeightMetres = 3;
			AssertEquals(9.843m, Utilities.Round((decimal)container.RC_Height, 3));
			AssertEquals(9, (int)container.HeightFeetOnly);
			AssertEquals(10.12m, Utilities.Round((decimal)container.HeightInchesOnly, 2));
			AssertEquals(300.015m, Utilities.Round((decimal)container.HeightCentimetres, 3));
			AssertEquals(118.116m, Utilities.Round((decimal)container.HeightInches, 3));

			container.RC_Height = new ZDecimal(8.6m);
			AssertEquals(2.621m, Utilities.Round((decimal)container.HeightMetres, 3));
			AssertEquals(8, (int)container.HeightFeetOnly);
			AssertEquals(7.20m, Utilities.Round((decimal)container.HeightInchesOnly, 2));
			AssertEquals(262.128m, Utilities.Round((decimal)container.HeightCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.HeightInches, 3));

			container.HeightCentimetres = 123;
			AssertEquals(4.035m, Utilities.Round((decimal)container.RC_Height, 3));
			AssertEquals(48.42m, Utilities.Round((decimal)container.HeightInches, 3));

			container.HeightInches = 98;
			AssertEquals(8.167m, Utilities.Round((decimal)container.RC_Height, 3));
			AssertEquals(248.93m, Utilities.Round((decimal)container.HeightCentimetres, 3));
		}

		public void TestWidth()
		{
			RefContainer container = RefContainer.New(Factory);
			container.WidthFeetOnly = 8;
			AssertEquals(8m, container.RC_Width);
			AssertEquals(2.438m, Utilities.Round((decimal)container.WidthMetres, 3));
			AssertEquals(243.84m, Utilities.Round((decimal)container.WidthCentimetres, 3));
			AssertEquals(96m, Utilities.Round((decimal)container.WidthInches, 3));

			container.WidthInchesOnly = 6;
			AssertEquals(8.5m, container.RC_Width);
			AssertEquals(2.591m, Utilities.Round((decimal)container.WidthMetres, 3));
			AssertEquals(259.08m, Utilities.Round((decimal)container.WidthCentimetres, 3));
			AssertEquals(102m, Utilities.Round((decimal)container.WidthInches, 3));

			container.WidthMetres = 3;
			AssertEquals(9.843m, Utilities.Round((decimal)container.RC_Width, 3));
			AssertEquals(9, (int)container.WidthFeetOnly);
			AssertEquals(10.12m, Utilities.Round((decimal)container.WidthInchesOnly, 2));
			AssertEquals(300.015m, Utilities.Round((decimal)container.WidthCentimetres, 3));
			AssertEquals(118.116m, Utilities.Round((decimal)container.WidthInches, 3));

			container.RC_Width = new ZDecimal(8.6m);
			AssertEquals(2.621m, Utilities.Round((decimal)container.WidthMetres, 3));
			AssertEquals(8, (int)container.WidthFeetOnly);
			AssertEquals(7.20m, Utilities.Round((decimal)container.WidthInchesOnly, 2));
			AssertEquals(262.128m, Utilities.Round((decimal)container.WidthCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.WidthInches, 3));

			container.WidthCentimetres = 123;
			AssertEquals(4.035m, Utilities.Round((decimal)container.RC_Width, 3));
			AssertEquals(48.42m, Utilities.Round((decimal)container.WidthInches, 3));

			container.WidthInches = 98;
			AssertEquals(8.167m, Utilities.Round((decimal)container.RC_Width, 3));
			AssertEquals(248.93m, Utilities.Round((decimal)container.WidthCentimetres, 3));
		}

		public void TestInsideLength()
		{
			RefContainer container = RefContainer.New(Factory);

			container.InsideLengthInches = 6;
			AssertEquals(0.5m, container.RC_InsideLength);
			AssertEquals(15.24m, Utilities.Round((decimal)container.InsideLengthCentimetres, 3));

			container.InsideLengthCentimetres = new ZDecimal(8.6m);
			AssertEquals(0.282m, container.RC_InsideLength);
			AssertEquals(3.384m, Utilities.Round((decimal)container.InsideLengthInches, 3));

			container.RC_InsideLength = new ZDecimal(8.6m);
			AssertEquals(262.128m, Utilities.Round((decimal)container.InsideLengthCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.InsideLengthInches, 3));
		}

		public void TestInsideHeight()
		{
			RefContainer container = RefContainer.New(Factory);

			container.InsideHeightInches = 6;
			AssertEquals(0.5m, container.RC_InsideHeight);
			AssertEquals(15.24m, Utilities.Round((decimal)container.InsideHeightCentimetres, 3));

			container.InsideHeightCentimetres = new ZDecimal(8.6m);
			AssertEquals(0.282m, container.RC_InsideHeight);
			AssertEquals(3.384m, Utilities.Round((decimal)container.InsideHeightInches, 3));

			container.RC_InsideHeight = new ZDecimal(8.6m);
			AssertEquals(262.128m, Utilities.Round((decimal)container.InsideHeightCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.InsideHeightInches, 3));
		}

		public void TestInsideWidth()
		{
			RefContainer container = RefContainer.New(Factory);

			container.InsideWidthInches = 6;
			AssertEquals(0.5m, container.RC_InsideWidth);
			AssertEquals(15.24m, Utilities.Round((decimal)container.InsideWidthCentimetres, 3));

			container.InsideWidthCentimetres = new ZDecimal(8.6m);
			AssertEquals(0.282m, container.RC_InsideWidth);
			AssertEquals(3.384m, Utilities.Round((decimal)container.InsideWidthInches, 3));

			container.RC_InsideWidth = new ZDecimal(8.6m);
			AssertEquals(262.128m, Utilities.Round((decimal)container.InsideWidthCentimetres, 3));
			AssertEquals(103.2m, Utilities.Round((decimal)container.InsideWidthInches, 3));
		}

		#endregion

		public void TestGrossWeight()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_GrossWeight = 10;
			AssertEquals("Gross Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(10, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3), container.GrossWeightPounds);

			container.GrossWeightPounds = 50;
			AssertEquals("Gross Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(50, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3), container.RC_GrossWeight);
		}

		public void TestTareWeight()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_TareWeight = 10;
			AssertEquals("Tare Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(10, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3), container.TareWeightPounds);

			container.TareWeightPounds = 50;
			AssertEquals("Tare Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(50, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3), container.RC_TareWeight);
		}

		public void TestNetWeight()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_NetWeight = 10;
			AssertEquals("Net Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(10, Constants.Weight.Kilograms, Constants.Weight.Pounds), 3), container.NetWeightPounds);

			container.NetWeightPounds = 50;
			AssertEquals("Tare Weight Pounds should be ", Utilities.Round(Constants.Weight.Convert(50, Constants.Weight.Pounds, Constants.Weight.Kilograms), 3), container.RC_NetWeight);
		}

		public void TestNetWeight_IsCalculatedForNonAir()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_ShippingMode = "SEA";
			container.RC_NetWeight = 0;
			container.RC_GrossWeight = 100;
			container.RC_TareWeight = 50;
			AssertEquals("RC_NetWeight = RC_GrossWeight - RC_TareWeight", container.RC_GrossWeight - container.RC_TareWeight, container.RC_NetWeight);
			AssertEquals("NetWeightPounds = GrossWeightPounds - TareWeightPounds", container.GrossWeightPounds - container.TareWeightPounds, container.NetWeightPounds);
		}

		public void TestNetWeight_Readonly()
		{
			RefContainer container = RefContainer.New(Factory);

			container.RC_ShippingMode = "AIR";
			AssertEquals("Net Weight should not be read-only for AIR container", false, container.RC_NetWeightInfo.ReadOnly);
			AssertEquals("Net Weight should not be read-only for AIR container", false, container.NetWeightPoundsInfo.ReadOnly);

			container.RC_ShippingMode = "SEA";
			AssertEquals("Net Weight should be read-only for non-AIR container", true, container.RC_NetWeightInfo.ReadOnly);
			AssertEquals("Net Weight should be read-only for non-AIR container", true, container.NetWeightPoundsInfo.ReadOnly);
		}

		public void TestCodeMapCollection()
		{
			var container = RefContainer.New(Factory);
			container.RC_Code = "cd1";

			var codeMap1 = Factory.NewWithValidTestData<RefContainerCodeMap>();
			codeMap1.RCM_RN_NKCountry = "CN";
			codeMap1.RCM_RC_Container = container.PK;
			codeMap1.RCM_Code = "CN1";
			codeMap1.RCM_Usage = "CUS";

			AssertEquals(1, container.CodeMapCollection.Count);
			AssertEquals("CN1", ((RefContainerCodeMap)container.CodeMapCollection.FirstOrDefault()).RCM_Code);
		}

		public void TestDelete()
		{
			var container = RefContainer.New(Factory);
			container.RC_Code = "cd1";

			var codeMap1 = Factory.NewWithValidTestData<RefContainerCodeMap>();
			codeMap1.RCM_RN_NKCountry = "CN";
			codeMap1.RCM_RC_Container = container.PK;
			codeMap1.RCM_Code = "CN1";
			codeMap1.RCM_Usage = "CUS";

			AssertEquals(1, container.CodeMapCollection.Count);

			container.Delete();
			AssertEquals(0, container.CodeMapCollection.Count);
		}

		public void TestCubicCapacityVolume()
		{
			RefContainer container = RefContainer.New(Factory);
			container.RC_CubicCapacity = 10;
			AssertEquals("CubicCapacity Volume Pounds should be ", Utilities.Round(Constants.Volume.Convert(10, Constants.Volume.CubicMetres, Constants.Volume.CubicFeet), 3), container.CubicCapacityFeet);

			container.CubicCapacityFeet = 50;
			AssertEquals("CubicCapacity Volume Pounds should be ", Utilities.Round(Constants.Volume.Convert(50, Constants.Volume.CubicFeet, Constants.Volume.CubicMetres), 3), container.RC_CubicCapacity);
		}

		public void TestContainerType()
		{
			var container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_ShippingMode = RefContainerLookups.ShippingModes.Air;
			Assert("Container type should no longer be ReadOnly if it is AirContainer", !container.RC_ContainerTypeInfo.ReadOnly);
		}

		public void TestGetUSContainerCodeFilter()
		{
			var container1 = Factory.NewWithValidTestData<RefContainer>();
			container1.RC_Code = "CNT1";
			container1.SetCountrySpecificContainerCode("U1", "US");

			var container2 = Factory.NewWithValidTestData<RefContainer>();
			container2.RC_Code = "CNT2";
			container2.SetCountrySpecificContainerCode("U2", "US");

			var container3 = Factory.NewWithValidTestData<RefContainer>();
			container3.RC_Code = "CNT3";
			var container3Map = container3.CodeMapCollection.AddNew();
			container3Map.RCM_RN_NKCountry = "CN";
			container3Map.RCM_RC_Container = container3.PK;
			container3Map.RCM_Code = "C1";

			Factory.Save();

			var testQuery = new ZDBOnlyQuery(typeof(RefContainer));
			var testSubQuery = RefContainer.GetContainerCodeFilter(Constants.CountryCodes.UnitedStates, "U1", "U2", "C1");
			testQuery.AddSubQuery(RefContainerSchema.PK, testSubQuery, JoinCondition.And);

			var testQueryResult = Factory.Load<RefContainer>(testQuery);
			AssertEquals(2, testQueryResult.Length);
			Assert(testQueryResult.Any(container => container.PK == container1.PK));
			Assert(testQueryResult.Any(container => container.PK == container2.PK));
		}

		public void TestISOTypeDefaults()
		{
			RefContainer container = Factory.NewWithValidTestData<RefContainer>();
			container.RC_ISOType = "45P1";
			container.SetDefaultForISO();
			AssertEquals((ZDecimal)2, container.RC_TEU);
			AssertEquals(true, container.RC_IsHighCube);
			AssertEquals(Constants.ContainerTypes.FlatRack, container.RC_ContainerType);
			container.RC_ISOType = "20P1";
			container.SetDefaultForISO();
			AssertEquals((ZDecimal)1, container.RC_TEU);
			AssertEquals(false, container.RC_IsHighCube);
			AssertEquals(Constants.ContainerTypes.FlatRack, container.RC_ContainerType);
		}

		protected override void TestBizObjectTranslatableDataField(ZPropertyInfo info)
		{
			if (info != ((RefContainer)info.BizObj).RC_DescriptionInfo)
			{
				base.TestBizObjectTranslatableDataField(info);
			}
		}

		[FrequentlyFailing]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRC_DescriptionTranslatableDataField()
		{
			RefContainer container = (RefContainer)GetNewBusinessObject();
			base.TestBizObjectTranslatableDataField(container.RC_DescriptionInfo);
		}

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(RefContainer)));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
		}

		#endregion
	}
}
