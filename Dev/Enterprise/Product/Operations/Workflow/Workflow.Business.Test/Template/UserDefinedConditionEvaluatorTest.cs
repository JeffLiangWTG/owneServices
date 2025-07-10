using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;

namespace Enterprise.Workflow.Business.Test
{
	public class UserDefinedConditionEvaluatorTest : TestCaseWithFactory
	{
		public void TestUserDefinedConditionEvaluator_TextMacroCaching()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Code = "C1";

			using (ProcessTask.Loader.WithTextMacroCaching(Factory))
			{
				ZString condition = "\"<Z0_Code>\" == \"C1\"";
				var evaluatableCondition = condition.ToUdfEvaluatableConditionValue();
				var useCache = true;
				AssertEquals("Z0_Code=C1", true, ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(evaluatableCondition, dummy, useTemplateCacheForConditions: useCache));
				dummy.Z0_Code = "C2";
				AssertEquals("useCache is explicitly set to true so it should be used", true, ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(evaluatableCondition, dummy, useTemplateCacheForConditions: useCache));

				useCache = false;
				AssertEquals("useCache is explicitly set to false so it should not be used", false, ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(evaluatableCondition, dummy, useTemplateCacheForConditions: useCache));
				dummy.Z0_Code = "C1";
				AssertEquals("useCache is explicitly set to false so it should not be used", true, ObjectFactory.Get<IUserDefinedConditionEvaluator>().IsTextMacroConditionMet(evaluatableCondition, dummy, useTemplateCacheForConditions: useCache));
			}
		}
	}
}
