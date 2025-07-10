using System;
using System.Collections;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine;

namespace Enterprise.MasterFiles.Business.Testing.Workflow
{
	class TemplateApplicationMacroEvaluationCachingTest : TemplateApplicationTestCase
	{
		public void TestPropertyEvaluationIsCachedAndOnlyDoneOncePerBusinessObject()
		{
			var template = MakeTemplate();
			var task1 = MakeTask(template, udfCondition: "\"<TrackedString>\" == \"ABC\"");
			var task2 = MakeTask(template, udfCondition: "\"<TrackedString>\" == \"EFG\"");

			Factory.Save();

			var dummy1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy1.ApplyWorkflowTemplates();

			var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy2.ApplyWorkflowTemplates();

			AssertEquals("Each property on the workflow provider object used in UDF conditions should only get evaluated once during template application.", 1, dummy1.TrackedStringReadCount);
			AssertEquals("Each property on the workflow provider object used in UDF conditions should only get evaluated once during template application.", 1, dummy2.TrackedStringReadCount);
		}

		public void TestPropertyEvaluationCacheLifetimeIsForASingleTemplateApplicationOnly()
		{
			var template = MakeTemplate();
			var task1 = MakeTask(template, udfCondition: "\"<TrackedString>\" == \"ABC\"");

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.All, true));

			AssertEquals("Each property on the workflow provider object used in UDF conditions should only get evaluated once during template application.", 2, dummy.TrackedStringReadCount);
		}

		public void TestFunctionEvaluationIsCachedAndOnlyDoneOncePerParameters()
		{
			Registry.Business.WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var trackedFunction = new TrackedFunctionValueProvider();
			var extraValueProviders = (ArrayList)ObjectFactory.Get("ExtraValueProviders");
			extraValueProviders.Add(trackedFunction);

			var template = MakeTemplate();
			var task1 = MakeTask(template, udfCondition: "\"<TrackedFunction('', 'A')>\" == \"<NOW><NOW>\"");
			var task2 = MakeTask(template, udfCondition: "\"<TrackedFunction('', 'A')>\" == \"<TrackedFunction('B', 'C')>\"");
			var task3 = MakeTask(template, udfCondition: "\"<TrackedFunction('<TrackedString>', 'A')>\" == \"ABC\"");

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertEquals(2, trackedFunction.EvaluationCount);
		}

		class TrackedFunctionValueProvider : ValueProvider
		{
			int evaluationCount;
			public int EvaluationCount => evaluationCount;

			protected override object GetReplacementCore(string macro, Report report)
			{
				evaluationCount++;
				return ZString.Empty;
			}

			protected override ValueProviderDocumenter GetDocumentation()
			{
				return null;
			}

			public override Regex Regex
			{
				get { return regex; }
			}
			static readonly Regex regex = new Regex(@"^<[\s]*TrackedFunction[\s]*\([\s]*'[^']*'[\s]*,[\s]*'[^']'[\s]*\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		}
	}
}
