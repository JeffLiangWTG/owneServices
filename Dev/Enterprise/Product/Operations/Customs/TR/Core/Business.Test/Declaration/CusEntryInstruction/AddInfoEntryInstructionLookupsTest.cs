using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	public class AddInfoEntryInstructionLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionLookupsTest
	{
		public void TestUnionSecretaryCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._1, UnionSecretaryCodesList.Descriptions._1),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._2, UnionSecretaryCodesList.Descriptions._2),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._3, UnionSecretaryCodesList.Descriptions._3),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._4, UnionSecretaryCodesList.Descriptions._4),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._5, UnionSecretaryCodesList.Descriptions._5),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._6, UnionSecretaryCodesList.Descriptions._6),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._7, UnionSecretaryCodesList.Descriptions._7),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._8, UnionSecretaryCodesList.Descriptions._8),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._9, UnionSecretaryCodesList.Descriptions._9),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._10, UnionSecretaryCodesList.Descriptions._10),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._11, UnionSecretaryCodesList.Descriptions._11),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._12, UnionSecretaryCodesList.Descriptions._12),
				new CodeDescriptionPair(UnionSecretaryCodesList.Codes._13, UnionSecretaryCodesList.Descriptions._13),
			}, instruction.AddInfoLookups.UnionSecretaryCodesList);
		}

		public void TestUnionCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(UnionCodesList.Codes._1, UnionCodesList.Descriptions._1),
				new CodeDescriptionPair(UnionCodesList.Codes._2, UnionCodesList.Descriptions._2),
				new CodeDescriptionPair(UnionCodesList.Codes._3, UnionCodesList.Descriptions._3),
				new CodeDescriptionPair(UnionCodesList.Codes._4, UnionCodesList.Descriptions._4),
				new CodeDescriptionPair(UnionCodesList.Codes._5, UnionCodesList.Descriptions._5),
				new CodeDescriptionPair(UnionCodesList.Codes._6, UnionCodesList.Descriptions._6),
				new CodeDescriptionPair(UnionCodesList.Codes._7, UnionCodesList.Descriptions._7),
				new CodeDescriptionPair(UnionCodesList.Codes._8, UnionCodesList.Descriptions._8),
				new CodeDescriptionPair(UnionCodesList.Codes._9, UnionCodesList.Descriptions._9),
				new CodeDescriptionPair(UnionCodesList.Codes._10, UnionCodesList.Descriptions._10),
				new CodeDescriptionPair(UnionCodesList.Codes._11, UnionCodesList.Descriptions._11),
				new CodeDescriptionPair(UnionCodesList.Codes._12, UnionCodesList.Descriptions._12),
				new CodeDescriptionPair(UnionCodesList.Codes._13, UnionCodesList.Descriptions._13),
				new CodeDescriptionPair(UnionCodesList.Codes._14, UnionCodesList.Descriptions._14),
				new CodeDescriptionPair(UnionCodesList.Codes._15, UnionCodesList.Descriptions._15),
				new CodeDescriptionPair(UnionCodesList.Codes._16, UnionCodesList.Descriptions._16),
				new CodeDescriptionPair(UnionCodesList.Codes._17, UnionCodesList.Descriptions._17),
				new CodeDescriptionPair(UnionCodesList.Codes._18, UnionCodesList.Descriptions._18),
				new CodeDescriptionPair(UnionCodesList.Codes._19, UnionCodesList.Descriptions._19),
				new CodeDescriptionPair(UnionCodesList.Codes._20, UnionCodesList.Descriptions._20),
				new CodeDescriptionPair(UnionCodesList.Codes._21, UnionCodesList.Descriptions._21),
				new CodeDescriptionPair(UnionCodesList.Codes._22, UnionCodesList.Descriptions._22),
				new CodeDescriptionPair(UnionCodesList.Codes._23, UnionCodesList.Descriptions._23),
				new CodeDescriptionPair(UnionCodesList.Codes._24, UnionCodesList.Descriptions._24),
				new CodeDescriptionPair(UnionCodesList.Codes._25, UnionCodesList.Descriptions._25),
				new CodeDescriptionPair(UnionCodesList.Codes._26, UnionCodesList.Descriptions._26),
				new CodeDescriptionPair(UnionCodesList.Codes._27, UnionCodesList.Descriptions._27),
				new CodeDescriptionPair(UnionCodesList.Codes._28, UnionCodesList.Descriptions._28),
			}, instruction.AddInfoLookups.UnionCodesList);
		}

		public void TestTransportModeInlandCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair(TRTransportModeInland.Codes._10, TRTransportModeInland.Descriptions._10),
				new CodeDescriptionPair(TRTransportModeInland.Codes._12, TRTransportModeInland.Descriptions._12),
				new CodeDescriptionPair(TRTransportModeInland.Codes._16, TRTransportModeInland.Descriptions._16),
				new CodeDescriptionPair(TRTransportModeInland.Codes._17, TRTransportModeInland.Descriptions._17),
				new CodeDescriptionPair(TRTransportModeInland.Codes._18, TRTransportModeInland.Descriptions._18),
				new CodeDescriptionPair(TRTransportModeInland.Codes._20, TRTransportModeInland.Descriptions._20),
				new CodeDescriptionPair(TRTransportModeInland.Codes._23, TRTransportModeInland.Descriptions._23),
				new CodeDescriptionPair(TRTransportModeInland.Codes._30, TRTransportModeInland.Descriptions._30),
				new CodeDescriptionPair(TRTransportModeInland.Codes._40, TRTransportModeInland.Descriptions._40),
				new CodeDescriptionPair(TRTransportModeInland.Codes._50, TRTransportModeInland.Descriptions._50),
				new CodeDescriptionPair(TRTransportModeInland.Codes._70, TRTransportModeInland.Descriptions._70),
				new CodeDescriptionPair(TRTransportModeInland.Codes._80, TRTransportModeInland.Descriptions._80),
				new CodeDescriptionPair(TRTransportModeInland.Codes._90, TRTransportModeInland.Descriptions._90),
			}, instruction.AddInfoLookups.TransportModeInland);
		}

		public void TestExportUnionCountryCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var trGroup = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);

			var yesterday = ZDateTime.Today.AddDays(-1);
			var today = ZDateTime.Today;
			var tomorrow = ZDateTime.Today.AddDays(1);

			helper.CreateCusCodeType("TRNOB", "E-Trade Nature Of Business");
			helper.CreateCusCodeType("TREUC", "TR Export Union Country Codes");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TREUC", "082", "Valid Item 1", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TREUC", "F11", "Valid Item 2", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TREUC", "070", "Valid Item 3", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRNOB", "006F", "Invalid Code Type", today, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TREUC", "023A", "Invalid Date", yesterday, yesterday);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.SouthAfrica, "TREUC", "046B", "Wrong Country", today, tomorrow);

			Factory.Save();

			var list = instruction.AddInfoLookups.ExportUnionCountryCodeList;

			AssertEquals(3, list.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Code 082 should exist", true, instruction.AddInfoLookups.ExportUnionCountryCodeList.ContainsCode("082"));
				AssertEquals("Description should be Valid Item 1 to ensure the correct type", "Valid Item 1", instruction.AddInfoLookups.ExportUnionCountryCodeList.GetDescriptionFromCode("082"));
				AssertEquals("Description should be Valid Item 2 to ensure the correct type", "Valid Item 2", instruction.AddInfoLookups.ExportUnionCountryCodeList.GetDescriptionFromCode("F11"));
				AssertEquals("First Item", "070", (list[0] as ICodeDescription).Code);
				AssertEquals("Second Item", "082", (list[1] as ICodeDescription).Code);
				AssertEquals("Third Item", "F11", (list[2] as ICodeDescription).Code);
			});

			var list1 = instruction.AddInfoLookups.ExportUnionCountryCodeList;
			var list2 = Factory.NewWithValidTestData<JobDeclaration>().CusEntryInstruction.AddInfoLookups.ExportUnionCountryCodeList;

			AssertSame("Is Cached", list1, list2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			instruction = (CusEntryInstruction)declaration.CustomsEntryInstructions.FirstOrDefault() ?? (CusEntryInstruction)declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}
}
