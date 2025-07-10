using System.Linq;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class SegregationFromGroupRuleTests : TestCaseWithFactory
	{
		public void TestCheck_ShouldReturnPassedWhenSGCodeNotExistInMap()
		{
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "BBBBBB";
			commonData.DC_Descriptor = "SG94 Nil";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			var attribute1 = Factory.New<ViewUNDGAttribute>();
			attribute1.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute1.DA_Index = "SGG4";
			attribute1.DA_DG = substance1.PK;

			AssertEquals("1 attribute", 1, substance1.SegregationGroups.Length);
			var attribute2 = Factory.New<ViewUNDGAttribute>();
			attribute2.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute2.DA_Index = "BBBBBB";
			attribute2.DA_DG = substance2.PK;

			var segRule = new SegregationFromGroupRule();
			var messages = segRule.Check(substance1, substance2);
			AssertEquals("1 attribute", 1, substance1.SegregationGroups.Length);
			AssertEquals("1 attribute", 1, substance2.SegregationCodes.Length);
			AssertEquals(0, messages.Count());
		}

		public void TestCheck_ShouldReturnPassedWhenSGCodesorGroupIsEmptyorNull()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("0 attribute", 0, substance1.SegregationGroups.Length);

			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			var segRule = new SegregationFromGroupRule();
			var messages = segRule.Check(substance1, substance2);
			AssertEquals(0, messages.Count());
		}

		public void TestCheck_ShouldReturnFailedWhenConflict()
		{
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "AAAAAA";
			commonData.DC_Descriptor = "SG20 Nil";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			var attribute1 = Factory.New<ViewUNDGAttribute>();
			attribute1.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute1.DA_Index = "SGG1";
			attribute1.DA_DG = substance1.PK;

			var attribute2 = Factory.New<ViewUNDGAttribute>();
			attribute2.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute2.DA_Index = "AAAAAA";
			attribute2.DA_DG = substance2.PK;

			var segRule = new SegregationFromGroupRule();
			var messages = segRule.Check(substance1, substance2);
			AssertEquals(1, messages.Count());
			AssertEquals(MessageType.Error, messages.First().Type);
		}

		public void TestCheck_ShouldReturnPassedWhenNoConflicts()
		{
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "AAAAAA";
			commonData.DC_Descriptor = "SG30 Nil";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			var attribute1 = Factory.New<ViewUNDGAttribute>();
			attribute1.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute1.DA_Index = "AAAAAA";
			attribute1.DA_DG = substance1.PK;

			var attribute2 = Factory.New<ViewUNDGAttribute>();
			attribute2.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute2.DA_Index = "SGG2";
			attribute2.DA_DG = substance2.PK;

			var segRule = new SegregationFromGroupRule();
			var messages = segRule.Check(substance1, substance2);
			AssertEquals(0, messages.Count());
		}
	}
}
