using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	public class DilutionExemptionRuleTests : TestCaseWithFactory
	{
		UNDGSubstance substance1;
		UNDGSubstance substance2;

		protected override void SetUp()
		{
			base.SetUp();

			substance1 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance1.DG_Standard = UNDGSubstanceStandardTypes.IMO;

			substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
			substance2.DG_Standard = UNDGSubstanceStandardTypes.IMO;
		}

		public void TestApplicableStandard()
		{
			var rule = new DilutionExemptionRule();
			AssertEquals(UNDGSubstanceStandardTypes.IMO, rule.ApplicableStandard);
		}

		public void TestCheck_WhenSubstancesAreCompatibleWithVariation_ShouldPass()
		{
			substance1.DG_UNNO = "3101";
			substance1.DG_PSN = "ORGANIC PEROXIDE TYPE B, LIQUID";
			substance1.DG_PG = string.Empty;
			substance1.DG_Variation = "Bearing both corrosive and explosive labels.";
			substance2.DG_UNNO = "3102";
			substance2.DG_PSN = "ORGANIC PEROXIDE TYPE B, SOLID";
			substance2.DG_PG = string.Empty;
			substance2.DG_Variation = "Not bearing an explosive label.";

			var rule = new DilutionExemptionRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(0, messages.Count());
		}

		public void TestCheck_WhenSubstancesAreCompatibleWithoutVariation_ShouldPass()
		{
			substance1.DG_UNNO = "2014";
			substance1.DG_PSN = "HYDROGEN PEROXIDE, AQUEOUS SOLUTION";
			substance1.DG_PG = "II";
			substance1.DG_Variation = null;
			substance2.DG_UNNO = "2984";
			substance2.DG_PSN = "HYDROGEN PEROXIDE, AQUEOUS SOLUTION";
			substance2.DG_PG = "III";
			substance2.DG_Variation = null;

			var rule = new DilutionExemptionRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(0, messages.Count());
		}

		public void TestCheck_WhenSubstancesAreIncompatible_ShouldFail()
		{
			substance1.DG_UNNO = "2014";
			substance1.DG_PSN = "HYDROGEN PEROXIDE, AQUEOUS SOLUTION";
			substance1.DG_PG = "II";
			substance1.DG_Variation = null;
			substance2.DG_UNNO = "1295";
			substance2.DG_PSN = "TRICHLOROSILANE";
			substance2.DG_PG = "I";
			substance2.DG_Variation = null;

			var expectedMessage = "These dangerous goods substances require segregation";
			var rule = new DilutionExemptionRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(expectedMessage, messages.FirstOrDefault()?.Text);
		}

		public void TestCheck_NullInitializedAsEmptyZString_ShouldFail()
		{
			substance1.DG_UNNO = null;
			substance1.DG_PSN = null;
			substance1.DG_PG = null;
			substance1.DG_Variation = null;
			substance2.DG_UNNO = null;
			substance2.DG_PSN = null;
			substance2.DG_PG = null;
			substance2.DG_Variation = null;

			var expectedMessage = "These dangerous goods substances require segregation";
			var rule = new DilutionExemptionRule();
			var messages = rule.Check(substance1, substance2);

			AssertEquals(expectedMessage, messages.FirstOrDefault()?.Text);
		}
	}
}
