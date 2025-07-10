using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceCFRLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPrimaryClassList_IsSameAsUNDGSubstance()
		{
			var item = Factory.New<UNDGSubstance>();
			AssertExpectedCodes(LookupsForTest.PrimaryClassList, new UNDGSubstanceLookups(item).DGClassList.GetAllCodes());
		}

		public void TestSecondaryClassList()
		{
			var expectedCodes = new[] { "1", "2.1", "3", "4.1", "4.2", "5.1", "6.1", "7", "8" };
			AssertExpectedCodes(LookupsForTest.SecondaryClassList, expectedCodes);
		}

		public void TestTertiaryClassList()
		{
			var expectedCodes = new[] { "3", "6", "8" };
			AssertExpectedCodes(LookupsForTest.TertiaryClassList, expectedCodes);
		}

		public void TestTechnicalNameList()
		{
			var expectedCodes = new[]
			{
				UNDGSubstanceLookups.TechNameTypes.Code.NotRequired,
				UNDGSubstanceLookups.TechNameTypes.Code.Required,
				UNDGSubstanceLookups.TechNameTypes.Code.Other,
			};

			AssertExpectedCodes(LookupsForTest.TechnicalNameList, expectedCodes);
		}

		public void TestExceptedQuantityList()
		{
			var expectedCodes = new[]
			{
				UNDGSubstanceLookups.ExceptedQuantity.Code.E0,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E1,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E2,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E3,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E4,
				UNDGSubstanceLookups.ExceptedQuantity.Code.E5
			};

			AssertExpectedCodes(LookupsForTest.ExceptedQuantityList, expectedCodes);
		}
		public void TestAirRailLimitTypeList()
		{
			var expectedList = new Dictionary<string, string>()
			{
				{ UNDGSubstanceCFRLookups.AirRailLimitTypes.Codes.Forbidden, "Forbidden" },
				{ UNDGSubstanceCFRLookups.AirRailLimitTypes.Codes.NoLimit, "No Limit" },
				{ UNDGSubstanceCFRLookups.AirRailLimitTypes.Codes.NetWeightLimit, "Net Weight Limit" },
				{ UNDGSubstanceCFRLookups.AirRailLimitTypes.Codes.GrossWeightLimit, "Gross Weight Limit" }
			};
			AssertEquals(expectedList.Count, LookupsForTest.AirRailLimitTypeList.Count);
			foreach (var item in expectedList)
			{
				AssertEquals(item.Value, LookupsForTest.AirRailLimitTypeList.GetDescriptionFromCode(item.Key));
			}
		}

		public void TestPackingGroupList()
		{
			var expectedCodes = new[]
			{
				UNDGSubstanceLookups.PackingGroupTypes.LowDangerCode,
				UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode,
				UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode
			};

			AssertExpectedCodes(LookupsForTest.PackingGroupList, expectedCodes);
		}

		public void TestMarinePollutantList()
		{
			var expectedCodes = new[]
			{
				UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code,
				UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code,
				UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code
			};

			AssertExpectedCodes(LookupsForTest.MarinePollutantList, expectedCodes);
		}

		public void TestStateList()
		{
			var expectedCodes = new[]
			{
				UNDGSubstanceLookups.StateTypes.Code.Solid,
				UNDGSubstanceLookups.StateTypes.Code.Liquid,
				UNDGSubstanceLookups.StateTypes.Code.Gas,
				UNDGSubstanceLookups.StateTypes.Code.ExplosiveArticle,
				UNDGSubstanceLookups.StateTypes.Code.ExplosiveSubstance,
			};

			AssertExpectedCodes(LookupsForTest.StateList, expectedCodes);
		}

		public void TestPoisonInhalationHazardProvisionsList()
		{
			var expectedList = new Dictionary<string, string>()
			{
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP1Readable, "Hazard Zone A" },
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP2Readable, "Hazard Zone B" },
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP3Readable, "Hazard Zone C" },
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP4Readable, "Hazard Zone D" },
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP5Readable, "Special Provision SP5" },
				{ UNDGSubstanceCFRLookups.PoisonInhalationHazardProvisions.Codes.SP6Readable, "Special Provision SP6" }
			};
			AssertEquals(expectedList.Count, LookupsForTest.PoisonInhalationHazardList.Count);
			foreach (var item in expectedList)
			{
				AssertEquals(item.Value, LookupsForTest.PoisonInhalationHazardList.GetDescriptionFromCode(item.Key));
			}
		}

		void AssertExpectedCodes(CodeDescriptionPairList lookupList, string[] expectedCodes)
		{
			AssertEquals("Pre-Condition", expectedCodes.Length, lookupList.Count);
			Assert("Should only contain the expected codes", lookupList.ContainsOnly(expectedCodes));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var item = Factory.New<UNDGSubstanceCFR>();
			LookupsForTest = new UNDGSubstanceCFRLookups(item);
		}

		UNDGSubstanceCFRLookups LookupsForTest;
	}
}
