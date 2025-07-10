using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class SegregateFromCertainCompoundsRuleTests : TestCaseWithFactory
	{
		public void TestCheck_ApplicableStandardIsIMO()
		{
			var rule = new SegregateFromCertainCompoundsRule();
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, rule.ApplicableStandard);
		}

		public void Test_SegregationCheck_Between_SG22_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var listOfIncompatibleUUNOsAndVariants = new List<(string, string)>()
			{
				("0004", "A"),
				("0004", "a"),
				("0004", "b"),
				("0004", "c"),
				("0222", "a"),
				("0222", "b"),
				("0402", "a"),
				("0402", "b"),
				("1310", ""),
				("1439", ""),
				("1442", ""),
				("1444", ""),
				("1512", ""),
				("1546", ""),
				("1630", ""),
				("1727", ""),
				("1835", "a"),
				("1835", "b"),
				("1843", ""),
				("1942", "a"),
				("1942", "b"),
				("2067", "a"),
				("2067", "b"),
				("2071", ""),
				("2426", ""),
				("2505", ""),
				("2506", ""),
				("2683", "a"),
				("2683", "b"),
				("2687", ""),
				("2817", "a"),
				("2817", "b"),
				("2818", "a"),
				("2818", "b"),
				("2854", ""),
				("2859", ""),
				("2861", ""),
				("2863", ""),
				("3375", "a"),
				("3375", "b"),
				("3375", "c"),
				("3423", ""),
				("3424", "a"),
				("3424", "b")
			};

			var expectedMessage = "Stow \"away from ammonium salts.\"";
			var substanceWithSG22 = GetUNDGSubstanceWithSegregationCode("SG22");
			foreach (var (uNNO, variant) in listOfIncompatibleUUNOsAndVariants)
			{
				ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG22, GetUNDGSubstance(uNNO, variant), expectedMessage);
			}
		}

		public void Test_SegregationCheck_Between_SG27_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var listOfIncompatibleUUNOsAndVariants = new List<(string, string)>()
			{
				("1481", "a"),
				("1481", "b"),
				("3210", "a"),
				("3210", "b"),
				("3211", "a"),
				("3211", "b")
			};

			var expectedMessage = "Stow \"separated from explosives containing chlorates or perchlorates.\"";
			var substanceWithSG27 = GetUNDGSubstanceWithSegregationCode("SG27");
			foreach (var (uNNO, variant) in listOfIncompatibleUUNOsAndVariants)
			{
				ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG27, GetUNDGSubstance(uNNO, variant), expectedMessage);
			}
		}

		public void Test_SegregationCheck_Between_SG37_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var expectedMessage = "Stow \"separated from ammonia\"";
			var substanceWithSG37 = GetUNDGSubstanceWithSegregationCode("SG37");
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG37, GetUNDGSubstance("1043", ""), expectedMessage);
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG37, GetUNDGSubstance("1841", ""), expectedMessage);
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG37, GetUNDGSubstance("2073", ""), expectedMessage);
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG37, GetUNDGSubstance("2672", ""), expectedMessage);
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG37, GetUNDGSubstance("3318", ""), expectedMessage);
		}

		public void Test_SegregationCheck_Between_SG46_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var listOfIncompatibleUUNOsAndVariants = new List<(string, string)>()
			{
				("1749", ""),
				("2548", ""),
				("2761", "a"),
				("2761", "b"),
				("2761", "c"),
				("2762", "a"),
				("2762", "b"),
				("2995", "a"),
				("2995", "b"),
				("2995", "c"),
				("2996", "a"),
				("2996", "b"),
				("2996", "c"),
				("3520", "")
			};

			var expectedMessage = "Stow \"separated from chlorine.\"";
			var substanceWithSG46 = GetUNDGSubstanceWithSegregationCode("SG46");
			foreach (var (uNNO, variant) in listOfIncompatibleUUNOsAndVariants)
			{
				ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG46, GetUNDGSubstance(uNNO, variant), expectedMessage);
			}
		}

		public void Test_SegregationCheck_Between_SG52_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var expectedMessage = "Stow \"separated from iron oxide.\"";
			var substanceWithSG52 = GetUNDGSubstanceWithSegregationCode("SG52");
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG52, GetUNDGSubstance("1376", "b"), expectedMessage);
			ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG52, GetUNDGSubstance("1994", ""), expectedMessage);
		}

		public void Test_SegregationCheck_Between_SG55_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var listOfIncompatibleUUNOsAndVariants = new List<(string, string)>()
			{
				("1629", ""),
				("1630", ""),
				("1631", ""),
				("1634", ""),
				("1636", ""),
				("1637", ""),
				("1638", ""),
				("1639", ""),
				("1640", ""),
				("1641", ""),
				("1642", ""),
				("1643", ""),
				("1644", ""),
				("1645", ""),
				("1646", ""),
				("2024", "a"),
				("2024", "b"),
				("2024", "c"),
				("2025", "a")
			};

			var expectedMessage = "Stow \"separated from mercury salts.\"";
			var substanceWithSG55 = GetUNDGSubstanceWithSegregationCode("SG55");
			foreach (var (uNNO, variant) in listOfIncompatibleUUNOsAndVariants)
			{
				ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG55, GetUNDGSubstance(uNNO, variant), expectedMessage);
			}
		}

		public void Test_SegregationCheck_Between_SG62_And_IncompatibleSubstances_ShouldFailAndShowError()
		{
			var listOfIncompatibleUUNOsAndVariants = new List<(string, string)>()
			{
				("1079", ""),
				("1080", ""),
				("1350", ""),
				("1786", ""),
				("1817", ""),
				("1828", ""),
				("1829", ""),
				("1830", "a"),
				("1830", "b"),
				("1831", "a"),
				("1831", "b"),
				("1832", "a"),
				("1832", "b"),
				("1833", ""),
				("1834", ""),
				("2191", ""),
				("2240", ""),
				("2308", ""),
				("2418", ""),
				("2448", ""),
				("2571", "a"),
				("2571", "b"),
				("2796", "a"),
				("3456", "")
			};

			var expectedMessage = "Stow \"separated from sulfur.\"";
			var substanceWithSG62 = GetUNDGSubstanceWithSegregationCode("SG62");
			foreach (var (uNNO, variant) in listOfIncompatibleUUNOsAndVariants)
			{
				ExpectTheSegregationCheckToFailWithErrorMessage(substanceWithSG62, GetUNDGSubstance(uNNO, variant), expectedMessage);
			}
		}

		public void Test_SegregationCheck_ShouldPassAndShowWarning_ForCompatibleSubstances()
		{
			var friendlySubstance = GetUNDGSubstance("3000", "a");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG22"), friendlySubstance, "Stow \"away from ammonium salts.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG41"), friendlySubstance, "Stow \"away from & separated from animal or vegetable oils.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG23"), friendlySubstance, "Stow \"away from & separated from animal or vegetable oils.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG27"), friendlySubstance, "Stow \"separated from explosives containing chlorates or perchlorates.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG37"), friendlySubstance, "Stow \"separated from ammonia\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG46"), friendlySubstance, "Stow \"separated from chlorine.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG52"), friendlySubstance, "Stow \"separated from iron oxide.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG55"), friendlySubstance, "Stow \"separated from mercury salts.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG57"), friendlySubstance, "Stow \"separated from odor-absorbing cargoes.\"");
			ExpectTheSegregationCheckToPassWithWarningMessage(GetUNDGSubstanceWithSegregationCode("SG62"), friendlySubstance, "Stow \"separated from sulfur.\"");
		}

		public void Test_SegregationCheck_ShouldPass_AndShowMultipleWarnings_ForCompatibleSubstances_AndMultipleSegregationCode()
		{
			var rule = new SegregateFromCertainCompoundsRule();
			var substance1 = GetUNDGSubstanceWithSegregationCode("SG22", "SG27", "SG62");
			var substance2 = GetUNDGSubstance("3000", "a");

			var messages = rule.Check(substance1, substance2);
			AssertEquals(3, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals("Stow \"away from ammonium salts.\"", messages.First().Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(1).Type);
			AssertEquals("Stow \"separated from explosives containing chlorates or perchlorates.\"", messages.ElementAt(1).Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(2).Type);
			AssertEquals("Stow \"separated from sulfur.\"", messages.ElementAt(2).Text);

			messages = rule.Check(substance2, substance1);
			AssertEquals(3, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals("Stow \"away from ammonium salts.\"", messages.First().Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(1).Type);
			AssertEquals("Stow \"separated from explosives containing chlorates or perchlorates.\"", messages.ElementAt(1).Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(2).Type);
			AssertEquals("Stow \"separated from sulfur.\"", messages.ElementAt(2).Text);
		}

		public void Test_SegregationCheck_ShouldFail_WhenThereAreMultipleSegregationCodes_AndOneOfThemIsIncompatibleWithTheOtherSubstance()
		{
			var rule = new SegregateFromCertainCompoundsRule();
			var substance1 = GetUNDGSubstanceWithSegregationCode("SG22", "SG27", "SG62");
			var substance2 = GetUNDGSubstance("1833", "");

			var messages = rule.Check(substance1, substance2);
			AssertEquals(3, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals("Stow \"away from ammonium salts.\"", messages.First().Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(1).Type);
			AssertEquals("Stow \"separated from explosives containing chlorates or perchlorates.\"", messages.ElementAt(1).Text);
			AssertEquals(MessageType.Error, messages.ElementAt(2).Type);
			AssertEquals("Stow \"separated from sulfur.\"", messages.ElementAt(2).Text);

			messages = rule.Check(substance2, substance1);
			AssertEquals(3, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals("Stow \"away from ammonium salts.\"", messages.First().Text);
			AssertEquals(MessageType.Warning, messages.ElementAt(1).Type);
			AssertEquals("Stow \"separated from explosives containing chlorates or perchlorates.\"", messages.ElementAt(1).Text);
			AssertEquals(MessageType.Error, messages.ElementAt(2).Type);
			AssertEquals("Stow \"separated from sulfur.\"", messages.ElementAt(2).Text);
		}

		void ExpectTheSegregationCheckToFailWithErrorMessage(UNDGSubstance substance1, UNDGSubstance substance2, string expectedMessage)
		{
			var rule = new SegregateFromCertainCompoundsRule();

			var messages = rule.Check(substance1, substance2);
			AssertEquals(1, messages.Count());
			AssertEquals(MessageType.Error, messages.First().Type);
			AssertEquals(expectedMessage, messages.First().Text);

			messages = rule.Check(substance2, substance1);
			AssertEquals(1, messages.Count());
			AssertEquals(MessageType.Error, messages.First().Type);
			AssertEquals(expectedMessage, messages.First().Text);
		}

		void ExpectTheSegregationCheckToPassWithWarningMessage(UNDGSubstance substance1, UNDGSubstance substance2, string expectedMessage)
		{
			var rule = new SegregateFromCertainCompoundsRule();

			var messages = rule.Check(substance1, substance2);
			AssertEquals(1, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals(expectedMessage, messages.First().Text);

			messages = rule.Check(substance2, substance1);
			AssertEquals(1, messages.Count());
			AssertEquals(MessageType.Warning, messages.First().Type);
			AssertEquals(expectedMessage, messages.First().Text);
		}

		UNDGSubstance GetUNDGSubstance(string uNNO, string variant)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = uNNO;
			substance.DG_Variant = variant;
			return substance;
		}

		UNDGSubstance GetUNDGSubstanceWithSegregationCode(params string[] segregationCodes)
		{
			var substance = Factory.New<UNDGSubstance>();
			foreach (var segregationCode in segregationCodes)
			{
				var arbitraryIndexStringForCommonData = segregationCode + "AA";
				var commonData = Factory.New<UNDGCommonData>();
				commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
				commonData.DC_Index = arbitraryIndexStringForCommonData;
				commonData.DC_Descriptor = segregationCode + " Nil";
				var attribute = Factory.New<ViewUNDGAttribute>();
				attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
				attribute.DA_Index = arbitraryIndexStringForCommonData;
				attribute.DA_DG = substance.PK;
			}

			return substance;
		}
	}
}
