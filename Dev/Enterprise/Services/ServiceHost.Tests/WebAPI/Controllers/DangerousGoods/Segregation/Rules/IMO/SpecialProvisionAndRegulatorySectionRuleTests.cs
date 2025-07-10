using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class SpecialProvisionAndRegulatorySectionRuleTests : TestCaseWithFactory
	{
		UNDGSubstance substance1;
		UNDGSubstance substance2;

		const string segregationMessage = "Check the special provision, or regulatory reference attached to this UN number’s class data to ensure appropriate segregation requirements are met.";
		const string classNotFoundMessage = "The class is empty for the dangerous goods substance";

		protected override void SetUp()
		{
			base.SetUp();

			substance1 = Factory.NewWithValidTestData<UNDGSubstance>();

			substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
		}

		public void TestCheck_WhenSubstanceHasSpecialProvisions()
		{
			substance1.DG_Class = "7";
			substance1.DG_SubLabel1 = "SP172";

			substance2.DG_Class = "2.1";
			substance2.DG_SubLabel1 = "1.2";

			var specialProvionAndRegulatorySectionRule = new SpecialProvisionAndRegulatorySectionRule();
			var messages = specialProvionAndRegulatorySectionRule.Check(substance1, substance2);
			AssertEquals(segregationMessage, messages.FirstOrDefault()?.Text);

			substance1.DG_Class = "1.1D";
			messages = specialProvionAndRegulatorySectionRule.Check(substance1, substance2);
			AssertEquals(messages.IsNullOrEmpty(), true);
		}

		public void TestCheck_WhenSubstanceHasRegulatorySections()
		{
			substance1.DG_Class = "1.1D";
			substance1.DG_SubLabel1 = "1.2";

			substance2.DG_Class = "2.1";
			substance2.DG_SubLabel1 = "2.0.6.6";

			var specialProvionAndRegulatorySectionRule = new SpecialProvisionAndRegulatorySectionRule();
			var messages = specialProvionAndRegulatorySectionRule.Check(substance1, substance2);
			AssertEquals(segregationMessage, messages.FirstOrDefault()?.Text);

			substance2.DG_Class = "1.1";
			messages = specialProvionAndRegulatorySectionRule.Check(substance1, substance2);
			AssertEquals(messages.IsNullOrEmpty(), true);
		}

		public void TestCheck_ShouldThrowDataNotFoundExceptionWhenClassIsEmpty()
		{
			substance1.DG_Class = "";
			substance2.DG_Class = "1.2C";

			var specialProvionAndRegulatorySectionRule = new SpecialProvisionAndRegulatorySectionRule();
			AssertExceptionThrown<DataNotFoundException>(classNotFoundMessage,
				() => specialProvionAndRegulatorySectionRule.Check(substance1, substance2));
		}
	}
}
