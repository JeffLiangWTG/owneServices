using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class Class1CompatibilityRuleTests : TestCaseWithFactory
	{
		UNDGSubstance substance1;
		UNDGSubstance substance2;

		const string segregationMessage = "These dangerous goods require segregation because of incompatible \"Class 1 compatibility groups (letters A to S)\".";
		const string classNotFoundMessage = "The class is empty for the dangerous goods substance";

		protected override void SetUp()
		{
			base.SetUp();

			substance1 = Factory.NewWithValidTestData<UNDGSubstance>();

			substance2 = Factory.NewWithValidTestData<UNDGSubstance>();
		}

		public void TestCheck_ShouldReturnPassWhenClassNot1()
		{
			substance1.DG_Class = "1.1D";
			substance2.DG_Class = "2.1";

			var class1CompatibilityRule = new Class1CompatibilityRule();
			var ruleMessages = class1CompatibilityRule.Check(substance1, substance2);

			AssertEquals(ruleMessages.IsNullOrEmpty(), true);
		}

		public void TestCheck_ShouldReturnFailWhenCompatibilityGroupsInfeasible()
		{
			substance1.DG_Class = "1.1D";
			substance2.DG_Class = "1.2A";

			var class1CompatibilityRule = new Class1CompatibilityRule();
			var ruleMessages = class1CompatibilityRule.Check(substance1, substance2);

			var actualMessage = ruleMessages.First().Text;

			AssertEquals(actualMessage, segregationMessage);
		}

		public void TestCheck_ShouldReturnPassWhenCompatibilityGroupsFeasible()
		{
			substance1.DG_Class = "1.1D";
			substance2.DG_Class = "1.2G";

			var class1CompatibilityRule = new Class1CompatibilityRule();
			var ruleMessages = class1CompatibilityRule.Check(substance1, substance2);

			AssertEquals(ruleMessages.IsNullOrEmpty(), true);
		}

		public void TestCheck_ShouldThrowDataNotFoundExceptionWhenClassIsEmpty()
		{
			substance1.DG_Class = "";
			substance2.DG_Class = "1.2C";

			var class1CompatibilityRule = new Class1CompatibilityRule();
			AssertExceptionThrown<DataNotFoundException>(classNotFoundMessage,
				() => class1CompatibilityRule.Check(substance1, substance2));
		}
	}
}
