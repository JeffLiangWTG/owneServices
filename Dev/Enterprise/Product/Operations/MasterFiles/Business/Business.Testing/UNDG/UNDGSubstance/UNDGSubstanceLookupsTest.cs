using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class UNDGSubstanceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMarinePollutantList()
		{
			var item = Factory.New<UNDGSubstance>();
			var lookup = new UNDGSubstanceLookups(item);

			AssertEquals("should have 3", 3, lookup.MarinePollutantList.Count);

			CombineAssertions(() =>
			{
				AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.MarinePollutant_Code));
				AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant_Code));
				AssertEquals(UNDGSubstanceLookups.MarinePollutantTypes.Depends, lookup.MarinePollutantList.GetDescriptionFromCode(UNDGSubstanceLookups.MarinePollutantTypes.Depends_Code));
			});
		}

		public void TestPackingGroupList()
		{
			var item = Factory.New<UNDGSubstance>();
			var lookup = new UNDGSubstanceLookups(item);

			AssertEquals("should have 3", 3, lookup.PackingGroupList.Count);

			CombineAssertions(() =>
			{
				AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.HighDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.HighDangerCode));
				AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.MediumDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.MediumDangerCode));
				AssertEquals(UNDGSubstanceLookups.PackingGroupTypes.LowDanger, lookup.PackingGroupList.GetDescriptionFromCode(UNDGSubstanceLookups.PackingGroupTypes.LowDangerCode));
			});
		}

		public void TestExceptedQuantityList()
		{
			var item = Factory.New<UNDGSubstance>();
			var lookup = new UNDGSubstanceLookups(item);

			AssertEquals("should have 6", 6, lookup.ExceptedQuantityList.Count);

			CombineAssertions(() =>
			{
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E0));
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E1));
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E2));
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E3));
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E4));
				Assert(lookup.ExceptedQuantityList.ContainsCode(UNDGSubstanceLookups.ExceptedQuantity.Code.E5));
			});
		}

		public void TestLimitedQuantityTypes()
		{
			var item = Factory.New<UNDGSubstance>();
			var lookup = new UNDGSubstanceLookups(item);

			AssertEquals("should have 6", 6, lookup.LimitedQuantityTypesList.Count);

			CombineAssertions(() =>
			{
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.FOB, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.FOBCode));
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.GLM, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.GLMCode));
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.NAP, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.NAPCode));
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.NLM, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.NLMCode));
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.NLT, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.NLTCode));
				AssertEquals(UNDGSubstanceLookups.LimitedQuantityTypes.NRE, lookup.LimitedQuantityTypesList.GetDescriptionFromCode(UNDGSubstanceLookups.LimitedQuantityTypes.NRECode));
			});
		}

		public void TestStandardTypes()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1230";
			substance.DG_Variant = "A";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "1230", "A", "IMO").First();
			var lookup = subs.Lookups;

			AssertEquals("Standard Types Count", 7, lookup.StandardTypes.Count);

			CombineAssertions(() =>
			{
				Assert("StandardTypes lookup contains ADN", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADN));
				Assert("StandardTypes lookup contains ADR", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.ADR));
				Assert("StandardTypes lookup contains IMO", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
				Assert("StandardTypes lookup contains IATA", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA));
				Assert("StandardTypes lookup contains RID", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.RID));
				Assert("StandardTypes lookup contains CFR", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR));
				Assert("StandardTypes lookup contains JTT", lookup.StandardTypes.ContainsCode(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.JTT));
			});
		}

		public void TestTechNameList()
		{
			var item = Factory.New<UNDGSubstance>();
			var lookup = new UNDGSubstanceLookups(item);

			AssertEquals("should have 3", 3, lookup.TechNameList.Count);

			CombineAssertions(() =>
			{
				AssertEquals(UNDGSubstanceLookups.TechNameTypes.Description.Required, lookup.TechNameList.GetDescriptionFromCode(UNDGSubstanceLookups.TechNameTypes.Code.Required));
				AssertEquals(UNDGSubstanceLookups.TechNameTypes.Description.NotRequired, lookup.TechNameList.GetDescriptionFromCode(UNDGSubstanceLookups.TechNameTypes.Code.NotRequired));
				AssertEquals(UNDGSubstanceLookups.TechNameTypes.Description.Other, lookup.TechNameList.GetDescriptionFromCode(UNDGSubstanceLookups.TechNameTypes.Code.Other));
			});
		}
	}
}
