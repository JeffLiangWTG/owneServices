using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class ClassSegregationRuleTests : TestCaseWithFactory
	{
		UNDGSubstance substance1;
		UNDGSubstance substance2;

		protected override void SetUp()
		{
			base.SetUp();

			substance1 = Factory.NewWithValidTestData<UNDGSubstance>();

			substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
		}

		public void TestApplicableStandard()
		{
			var rule = new ClassSegregationRule();
			AssertEquals(UNDGSubstanceStandardTypes.IMO, rule.ApplicableStandard);
		}

		public void TestCheck_WhenSubstancesAreCompatible_ShouldPass()
		{
			substance1.DG_Class = "3";
			substance1.DG_SubLabel1 = "6.1";
			substance1.DG_SubLabel2 = "9";

			substance2.DG_Class = "9";
			substance2.DG_SubLabel1 = "3";
			substance2.DG_SubLabel2 = "8";

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(messages.IsNullOrEmpty(), true);
		}

		public void TestCheck_WhenSubstancesHaveSameClass_ShouldPass()
		{
			substance1.DG_Class = "8";
			substance1.DG_SubLabel1 = "9";
			substance1.DG_SubLabel2 = "3";

			substance2.DG_Class = "8";
			substance2.DG_SubLabel1 = "9";
			substance2.DG_SubLabel2 = "3";

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(messages.IsNullOrEmpty(), true);
		}

		public void TestCheck_WhenSubstancesAreIncompatible_ShouldFail()
		{
			substance1.DG_Class = "4.1";
			substance1.DG_SubLabel1 = "9";
			substance1.DG_SubLabel2 = "2.2";

			substance2.DG_Class = "8";
			substance2.DG_SubLabel1 = "4.1";
			substance2.DG_SubLabel2 = "4.3";

			const string expectedMessage = "These dangerous goods classes require segregation.";

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(expectedMessage, messages.FirstOrDefault()?.Text);
		}

		public void TestCheck_WhenSubLabelsConflict_ShouldFail()
		{
			substance1.DG_Class = "3";
			substance1.DG_SubLabel1 = "7";
			substance1.DG_SubLabel2 = "2.1";

			substance2.DG_Class = "9";
			substance2.DG_SubLabel1 = "7";
			substance2.DG_SubLabel2 = "4.1";

			const string expectedMessage = "These dangerous goods classes require segregation.";

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(expectedMessage, messages.FirstOrDefault()?.Text);
		}

		public void TestCheck_WhenSubLabelsAreNull()
		{
			substance1.DG_Class = "3";
			substance1.DG_SubLabel1 = null;
			substance1.DG_SubLabel2 = null;

			substance2.DG_Class = "9";
			substance2.DG_SubLabel1 = null;
			substance2.DG_SubLabel2 = null;

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(messages.IsNullOrEmpty(), true);
		}

		public void TestCheck_ThrowsExceptionWhenWhenClassIsEmpty()
		{
			substance1.DG_Class = "";
			substance2.DG_Class = "9";
			var expectedMessage = $"Class segregation rule: class is empty for {substance1.PK}: 1";

			var rule = new ClassSegregationRule();

			AssertExceptionThrown<DataCorruptionException>(
				expectedMessage,
				() => rule.Check(substance1, substance2)
			);
		}

		public void TestCheck_SupportsClassOneFormattedWithCompatibilityGroups()
		{
			substance1.DG_Class = "1.2B";
			substance2.DG_Class = "9";

			var rule = new ClassSegregationRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(messages.IsNullOrEmpty(), true);
		}
	}
}
