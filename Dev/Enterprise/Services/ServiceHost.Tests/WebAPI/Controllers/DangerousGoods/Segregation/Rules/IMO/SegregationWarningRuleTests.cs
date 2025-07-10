using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class SegregationWarningRuleTests : TestCaseWithFactory
	{
		public void TestCheck_ApplicationStandardIsIMO()
		{
			var rule = new SegregationWarningRule();
			AssertEquals(UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO, rule.ApplicableStandard);
		}

		public void TestCheck_BothSubstanceHasMatchingCodes()
		{
			var segregationCode1 = "SG23";
			var segregationCode2 = "SG29";
			var warning1 = "Stow \"away from animal or vegetable oils.\"";
			var warning2 = "Segregation from foodstuffs as described in Chapter 7.3.4.2.2. 7.6.3.1.2 or 7.7.3.7 of the IMDG Code.";

			RunTest(segregationCode1, warning1, segregationCode2, warning2);
		}

		public void TestCheck_NoSubstanceHasMatchingCodes()
		{
			var segregationCode1 = "SG10";
			var segregationCode2 = "SG20";
			var warning1 = "Stow \"away from\" class 5.1.";
			var warning2 = "'Stow \"away from\" SGG1 - acids.";

			RunTest(segregationCode1, warning1, segregationCode2, warning2, false);
		}

		public void TestCheck_OneSubstanceHasMatchingCode()
		{
			var segregationCode1 = "SG23";
			var segregationCode2 = "SG20";
			var warning1 = "Stow \"away from animal or vegetable oils.\"";
			var warning2 = "'Stow \"away from\" SGG1 - acids.";

			RunTest(segregationCode1, warning1, segregationCode2, warning2, true, 1);
		}

		public void TestCheck_BothSubstanceHaveSameCodes()
		{
			var segregationCode1 = "SG23";
			var segregationCode2 = "SG23";
			var warning1 = "Stow \"away from animal or vegetable oils.\"";
			var warning2 = "Stow \"away from animal or vegetable oils.\"";

			RunTest(segregationCode1, warning1, segregationCode2, warning2, true, 1);
		}

		public void RunTest(ZString segregationCode1, ZString warning1, ZString segregationCode2, ZString warning2, bool expectedMessages = true, int expectedMessagesCount = 2)
		{
			var commonData1 = Factory.New<UNDGCommonData>();
			commonData1.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData1.DC_Index = "AAAAAA";
			commonData1.DC_Descriptor = $"{segregationCode1} + {warning1}";

			var commonData2 = Factory.New<UNDGCommonData>();
			commonData2.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData2.DC_Index = "BBBBBB";
			commonData2.DC_Descriptor = $"{segregationCode2} + {warning2}";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			var attribute1 = Factory.New<ViewUNDGAttribute>();
			attribute1.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute1.DA_Index = "AAAAAA";
			attribute1.DA_DG = substance1.PK;
			AssertEquals(segregationCode1, substance1.SegregationCodesForBinding);

			var attribute2 = Factory.New<ViewUNDGAttribute>();
			attribute2.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute2.DA_Index = "BBBBBB";
			attribute2.DA_DG = substance2.PK;
			AssertEquals(segregationCode2, substance2.SegregationCodesForBinding);

			var rule = new SegregationWarningRule();
			var messages = rule.Check(substance1, substance2);

			if (expectedMessages)
			{
				AssertEquals(expectedMessagesCount, messages.Count());
				if (expectedMessagesCount > 0)
				{
					AssertEquals(warning1, messages.ElementAt(0).Text);
					if (expectedMessagesCount > 1)
					{
						AssertEquals(warning2, messages.ElementAt(1).Text);
					}
				}
			}
			else
			{
				AssertEquals(0, messages.Count());
			}
		}
	}
}
