using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Workflow.Business.Test
{
	class UserDefinedConditionSplitterTest : TestCaseWithFactory
	{
		public void TestConditionsSplitByAnds()
		{
			AssertSplit("", new ZString[] { "" });
			AssertSplit("A", new ZString[] { "A" });
			AssertSplit("A&&B", new ZString[] { "A", "B" });
			AssertSplit(" A && B ", new ZString[] { "A", "B" });
			AssertSplit("A&&B&&C", new ZString[] { "A", "B", "C" });
			AssertSplit("A&&B&&(C||D)", new ZString[] { "A", "B", "(C||D)" });
			AssertSplit("(A&&B)&&(C||D)", new ZString[] { "(A&&B)", "(C||D)" });
		}

		public void TestRealUdfExamples()
		{
			AssertSplit("\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import && ||\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\"&&\"<JK_Phase>\"==\"IMC\"&&\"<GetCustomField(Imported from Legacy)>\"==\"Y\"&&\"<Shipments.JS_TransportMode>\"!=\"AIR\"",
				new ZString[] {
					"\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import && ||\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\"",
					"\"<JK_Phase>\"==\"IMC\"",
					"\"<GetCustomField(Imported from Legacy)>\"==\"Y\"",
					"\"<Shipments.JS_TransportMode>\"!=\"AIR\"",
				});

			AssertSplit("\"<JK_TransportMode>\"==\"SEA\"&&(\"<JK_Phase>\"==\"DRF\"||\"<JK_Phase>\"==\"EXP\")&&(\"<JK_ReleaseType>\"==\"BRR\"||\"<JK_ReleaseType>\"==\"BSD\"||\"<JK_ReleaseType>\"==\"BTD\"||\"<JK_ReleaseType>\"==\"CAD\"||\"<JK_ReleaseType>\"==\"CSH\"||\"<JK_ReleaseType>\"==\"LOI\"||\"<JK_ReleaseType>\"==\"NON\"||\"<JK_ReleaseType>\"==\"OBO\"||\"<JK_ReleaseType>\"==\"OBR\"||\"<JK_ReleaseType>\"==\"OBX\")&&\"<WorkflowItems.Find(\"{P9_Description}\"==\"Original MBL Collection\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\"",
				new ZString[]
				{
					"\"<JK_TransportMode>\"==\"SEA\"",
					"(\"<JK_Phase>\"==\"DRF\"||\"<JK_Phase>\"==\"EXP\")",
					"(\"<JK_ReleaseType>\"==\"BRR\"||\"<JK_ReleaseType>\"==\"BSD\"||\"<JK_ReleaseType>\"==\"BTD\"||\"<JK_ReleaseType>\"==\"CAD\"||\"<JK_ReleaseType>\"==\"CSH\"||\"<JK_ReleaseType>\"==\"LOI\"||\"<JK_ReleaseType>\"==\"NON\"||\"<JK_ReleaseType>\"==\"OBO\"||\"<JK_ReleaseType>\"==\"OBR\"||\"<JK_ReleaseType>\"==\"OBX\")",
					"\"<WorkflowItems.Find(\"{P9_Description}\"==\"Original MBL Collection\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\""
				});

			AssertSplit("\"<Consols.GetCustomField(Imported from Legacy)>\"==\"Y\"&&(\"<Job.Branch.ExtraPorts.Find(\"{GY_RL_NKAdditionalBranchRelatedPort}\"==\"<JS_RL_NKDestination>\").GY_RL_NKAdditionalBranchRelatedPort>\"==\"<JS_RL_NKDestination>\"||\"<JobDirection>\"==\"Import\")",
				new ZString[]
				{
					"\"<Consols.GetCustomField(Imported from Legacy)>\"==\"Y\"",
					"(\"<Job.Branch.ExtraPorts.Find(\"{GY_RL_NKAdditionalBranchRelatedPort}\"==\"<JS_RL_NKDestination>\").GY_RL_NKAdditionalBranchRelatedPort>\"==\"<JS_RL_NKDestination>\"||\"<JobDirection>\"==\"Import\")",
				});

			//AssertNoSplit("((\"<JS_Phase>\"==\"DRF\"||\"<JS_Phase>\"==\"EXP\")&&\"<JS_ReleaseType>\"==\"OBO\")||(\"<JS_Phase>\"==\"IMP\"&&(\"<JS_ReleaseType>\"==\"BRR\"||\"<JS_ReleaseType>\"==\"BSD\"||\"<JS_ReleaseType>\"==\"BTD\"||\"<JS_ReleaseType>\"==\"OBR\"||\"<JS_ReleaseType>\"==\"OBX\"||\"<JS_ReleaseType>\"==\"OBD\"||\"<JS_ReleaseType>\"==\"NON\"||\"<JS_ReleaseType>\"==\"LOI\"||\"<JS_ReleaseType>\"==\"CAD\"||\"<JS_ReleaseType>\"==\"CSH\"))&&\"<WorkflowItems.Find(\"{P9_Description}\"==\"Receive OBL\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\"&&\"<WorkflowItems.Find(\"{P9_Description}\"==\"Receive Original HB\"&&\"{IsTask}\"==\"Y\").CreatedTimeLocal>\"==\"\"");

			AssertSplit("\"<ReceivingForwarder.OH_Code>\"==\"HKG\"&&\"<WorkflowItems.Find(\"{P9_Description}\"==\"Send MSG to HK - ISC\"&&\"{IsWorkflowTrigger}\"==\"Y\").CreatedTimeLocal>\"==\"\"&&\"<GetCustomField(Imported from Legacy)>\"!=\"Y\" && \"<HasEvent (Z00)>\"==\"N\" && ((\"<WorkflowItems.Find(\"{P9_Description}\"==\"Confirmed on Board Departure\").P9_ActualDateForBinding>\"!=\"\"||\"<WorkflowItems.Find(\"{P9_Description}\"==\"Consolidation Arrival\").P9_ActualDateForBinding>\"!=\"\") ||( (\"<WorkflowItems.Find(\"{P9_Description}\"==\"Depart from Warehouse\").P9_ActualDateForBinding>\"!=\"\"||\"<WorkflowItems.Find(\"{P9_Description}\"==\"Freight Physically Checked in at Departure Airline\").P9_ActualDateForBinding>\"!=\"\") &&\"<SendingForwarder.GetCustomField(Manual Air Phasing)>\"==\"N\"))",
				new ZString[]
				{
					"\"<ReceivingForwarder.OH_Code>\"==\"HKG\"",
					"\"<WorkflowItems.Find(\"{P9_Description}\"==\"Send MSG to HK - ISC\"&&\"{IsWorkflowTrigger}\"==\"Y\").CreatedTimeLocal>\"==\"\"",
					"\"<GetCustomField(Imported from Legacy)>\"!=\"Y\"",
					"\"<HasEvent (Z00)>\"==\"N\"",
					"((\"<WorkflowItems.Find(\"{P9_Description}\"==\"Confirmed on Board Departure\").P9_ActualDateForBinding>\"!=\"\"||\"<WorkflowItems.Find(\"{P9_Description}\"==\"Consolidation Arrival\").P9_ActualDateForBinding>\"!=\"\") ||( (\"<WorkflowItems.Find(\"{P9_Description}\"==\"Depart from Warehouse\").P9_ActualDateForBinding>\"!=\"\"||\"<WorkflowItems.Find(\"{P9_Description}\"==\"Freight Physically Checked in at Departure Airline\").P9_ActualDateForBinding>\"!=\"\") &&\"<SendingForwarder.GetCustomField(Manual Air Phasing)>\"==\"N\"))"
				});
		}

		public void TestSpecialCharsInsideMacro()
		{
			AssertNoSplit("\"<WhenIGrowUpMacro(\"I wanna be an astronut && be gooder at spelling\")>");
			AssertNoSplit("\"<WhenIGrowUpMacro(\"RICH || !!RICH\")>");
			AssertNoSplit("\"<WhenIGrowUpMacro(\"Happy :-)\")>");
			AssertNoSplit("\"<WhenIGrowUpMacro(\"!Sad :-(\")>");
			AssertNoSplit("\"<WhenIGrowUpMacro(\"Money > Jeff Bezos\")>");
			AssertNoSplit("\"<WhenIGrowUpMacro(\"JLo < Attractive\")>");

			AssertNoSplit("\"&&\"==\"&&\"");
			AssertNoSplit("\'&&\'=='&&'");
			// this won't split as the parentheses are unmatched and at this point in the evaluation of the UDF condition we cannot
			// differentiate "string(" from "macro(" without calling GetValueProvider 
			AssertNoSplit("\")\"==\")\"&&\"(\"==\"(\"");
			AssertNoSplit("\"\\\"\"==\"\"&&\"\"==\"\"");
			AssertNoSplit("\"(\"==\"(\"&&\")\"==\")\"");
			AssertSplit("\"||\"==\"||\"&&\"1\"==\"1\"", new ZString[] { "\"||\"==\"||\"", "\"1\"==\"1\"" });
		}

		public void TestCacheSmallReusableConditions()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_Description = "Import";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"&&\"2\"==\"2\"&&\"3\"==\"3\"";
			dummy.Z0_Code = "ABC";

			Assert("Condition is met", dummy.WorkflowItems.IsCondition2Met(milestone));
			AssertEquals(true, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"1\"==\"1\"", true));
			AssertEquals(true, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"2\"==\"2\"", true));
			AssertEquals(true, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"3\"==\"3\"", true));
		}

		public void TestSlowConditionsNotEvaluatedWhenFalse()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_Description = "Import";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			milestone.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"XYZ\"&&\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import\"&&\"{IsTask}\"==\"N\").CreatedTimeLocal>\"==\"\"";
			dummy.Z0_Code = "ABC";

			Assert("Condition not met", !dummy.WorkflowItems.IsCondition2Met(milestone));
			AssertEquals("This should be the first condition evaluated and cached", true, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"<Z0_Code>\" == \"XYZ\"", false));
			AssertEquals("First condition is false, this should not be evaluated", false, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import\"&&\"{IsTask}\"==\"N\").CreatedTimeLocal>\"==\"\"", false));

			UserDefinedConditionEvaluatorImpl.ClearCache(dummy);
			dummy.Z0_Code = "XYZ";

			Assert("Condition met", dummy.WorkflowItems.IsCondition2Met(milestone));
			AssertEquals("First condition is now true, this should be evaluated and cached", true, UserDefinedConditionEvaluatorImpl.IsCached(dummy, "\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import\"&&\"{IsTask}\"==\"N\").CreatedTimeLocal>\"==\"\"", true));
		}

		public void TestFindMacro()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			ProcessTask milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.IsMilestone = true;
			milestone.P9_Description = "Import";
			milestone.P9_Sequence = 1;
			milestone.TriggerConditions.TriggerEventCode = "Z00";
			milestone.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			milestone.TemplateConditions.TemplateCondition2Value = "\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import\"&&\"{IsMilestone}\"==\"Y\"&&\"{P9_Sequence}\"==\"1\").P9_SE_NKMilestoneEvent>\"==\"Z00\"";
			Assert("Condition met", dummy.WorkflowItems.IsCondition2Met(milestone));

			milestone.TemplateConditions.TemplateCondition2Value = "\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import1\"||\"{IsMilestone}\"==\"Y\"&&\"{P9_Sequence}\"==\"1\").P9_SE_NKMilestoneEvent>\"==\"Z00\"";
			Assert("Condition met", dummy.WorkflowItems.IsCondition2Met(milestone));

			milestone.TemplateConditions.TemplateCondition2Value = "\"<WorkflowItems.Find(\"{P9_Description}\"==\"Import\"&&\"{IsMilestone}\"==\"Y\"&&\"{P9_Sequence}\"==\"2\").P9_SE_NKMilestoneEvent>\"==\"Z00\"";
			Assert("Condition not met", !dummy.WorkflowItems.IsCondition2Met(milestone));
		}

		public void TestIsUserDefinedConditionMet_WithAdditionalJobs()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Code = "C1";

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			shipment.JS_HouseBill = "GUD";
			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_HouseBill = "GUY";
			var additionalJobs = new BusinessObject[] { (BusinessObject)shipment, (BusinessObject)declaration };

			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			CombineAssertions(() =>
			{
				task.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"C1\"";
				AssertEquals("Z0_Code=C1", true, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task }));

				task.TemplateConditions.TemplateCondition2Value = "\"<JS_HouseBill>\" == \"GUD\"";
				AssertEquals("JS_HouseBill=GUD", true, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task, (BusinessObject)shipment }));

				task.TemplateConditions.TemplateCondition2Value = "\"<JE_HouseBill>\" == \"GUY\"";
				AssertEquals("JE_HouseBill=GUY", true, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task, (BusinessObject)declaration }));

				task.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"xx\"";
				AssertEquals("Z0_Code=xx", false, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task }));

				task.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\" == \"\"";
				AssertEquals("Z0_Code=", false, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task }));

				task.TemplateConditions.TemplateCondition2Value = "\"<XX>\" == \"xx\"";
				AssertEquals("Invalid <XX> return empty value when replacing macro so false when compare with xx", false, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task }));

				task.TemplateConditions.TemplateCondition2Value = "\"<XX>\" == \"\"";
				AssertEquals("Invalid <XX> return empty value when replacing macro so return true when compare with empty", true, UserDefinedConditionEvaluatorImpl.IsTextMacroConditionMet(task.TemplateConditions.TemplateCondition2Value.ToUdfEvaluatableConditionValue(), dummy, dataContext: new[] { task }));
			});
		}

		public void TestRegistryItem()
		{
			using (WorkflowDataRegistry.Instance.EnableUserDefinedConditionFastFailing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertSplit("\"1\"==\"1\"&&\"2\"==\"2\"&&\"3\"==\"3\"", new ZString[] { "\"1\"==\"1\"", "\"2\"==\"2\"", "\"3\"==\"3\"" });
			}

			using (WorkflowDataRegistry.Instance.EnableUserDefinedConditionFastFailing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertNoSplit("\"1\"==\"1\"&&\"2\"==\"2\"&&\"3\"==\"3\"");
			}
		}

		public void TestSplitDoesntRaiseErrorReportOnSyntaxErrors()
		{
			var result = UserDefinedConditionSplitterImpl.Split("\"<JK_ReleaseType>\"!=\"EXP\"||\"<JK_ReleaseType>\"!=\"SWB\"||");
			Assert("No Split Should be Done Due to Syntax Error", !result.Children.Any());
			AssertEquals("No Unhandled Errors", 0, ErrorReporter.TotalErrorCount);

			result = UserDefinedConditionSplitterImpl.Split("||\"<JK_ReleaseType>\"!=\"EXP\"||\"<JK_ReleaseType>\"!=\"SWB\"");
			Assert("No Split Should be Done Due to Syntax Error", !result.Children.Any());
			AssertEquals("No Unhandled Errors", 0, ErrorReporter.TotalErrorCount);
		}

		void VerifyConditionClause(ZString condition, Action<IMacroBooleanExpressionClause, List<IMacroBooleanExpressionClause>> verificationAction)
		{
			VerifyConditionClause(UserDefinedConditionSplitterImpl.Split(condition), verificationAction);
		}

		void VerifyConditionClause(IMacroBooleanExpressionClause clause, Action<IMacroBooleanExpressionClause, List<IMacroBooleanExpressionClause>> verificationAction)
		{
			var children = clause.Children.ToList();

			verificationAction(clause, children);
		}

		public void TestSplitWorksOnHierarchicalConditions_FirstCase()
		{
			VerifyConditionClause("A&&B&&C||D", (result, children) =>
			{
				Assert(!result.IsConjunction);
				AssertEquals(2, children.Count);
				AssertEquals("D", children[1].AsString());

				VerifyConditionClause(children[0], (left, leftChildren) =>
				{
					Assert(left.IsConjunction);

					AssertEquals(3, leftChildren.Count);
					AssertEquals("A", leftChildren[0].AsString());
					AssertEquals("B", leftChildren[1].AsString());
					AssertEquals("C", leftChildren[2].AsString());
				});
			});
		}

		public void TestSplitWorksOnHierarchicalConditions_SecondCase()
		{
			VerifyConditionClause("(A&&B)||(C||D)", (result, children) =>
			{
				Assert(!result.IsConjunction);
				AssertEquals(2, children.Count);

				VerifyConditionClause(children[0], (left, leftChildren) =>
				{
					Assert(left.IsConjunction);
					AssertEquals(2, leftChildren.Count);

					AssertEquals("A", leftChildren[0].AsString());
					AssertEquals("B", leftChildren[1].AsString());
				});

				VerifyConditionClause(children[1], (right, rightChildren) =>
				{
					Assert(!right.IsConjunction);
					AssertEquals(2, rightChildren.Count);

					AssertEquals("C", rightChildren[0].AsString());
					AssertEquals("D", rightChildren[1].AsString());
				});
			});
		}

		public void TestSplitWorksOnHierarchicalConditions_ThirdCase()
		{
			var result = UserDefinedConditionSplitterImpl.Split("(A&&B)&&(C||D)||((A&&B)&&(C||D))");

			VerifyConditionClause(result, (clause, children) =>
			{
				Assert(!clause.IsConjunction);
				AssertEquals(2, children.Count);

				VerifyConditionClause(children[0], (left, leftChildren) =>
				{
					Assert(left.IsConjunction);
					AssertEquals(2, leftChildren.Count);

					VerifyConditionClause(leftChildren[0], (ll, llChildren) =>
					{
						Assert(ll.IsConjunction);
						AssertEquals(2, llChildren.Count);
						AssertEquals("A", llChildren[0].AsString());
						AssertEquals("B", llChildren[1].AsString());
					});

					VerifyConditionClause(leftChildren[1], (lr, lrChildren) =>
					{
						Assert(!lr.IsConjunction);
						AssertEquals(2, lrChildren.Count);
						AssertEquals("C", lrChildren[0].AsString());
						AssertEquals("D", lrChildren[1].AsString());
					});
				});

				VerifyConditionClause(children[1], (right, rightChildren) =>
				{
					Assert(right.IsConjunction);
					AssertEquals(2, rightChildren.Count);

					VerifyConditionClause(rightChildren[0], (rl, rlChildren) =>
					{
						Assert(rl.IsConjunction);
						AssertEquals(2, rlChildren.Count);
						AssertEquals("A", rlChildren[0].AsString());
						AssertEquals("B", rlChildren[1].AsString());
					});

					VerifyConditionClause(rightChildren[1], (rr, rrChildren) =>
					{
						Assert(!rr.IsConjunction);
						AssertEquals(2, rrChildren.Count);
						AssertEquals("C", rrChildren[0].AsString());
						AssertEquals("D", rrChildren[1].AsString());
					});
				});
			});
		}

		Func<string, Func<bool>, bool> MakeEvaluator(Func<string, bool> coreEvaluator)
		{
			return (string condition, Func<bool> evaluator) => evaluator == null ? coreEvaluator(condition) : evaluator();
		}

		public void TestShortCircuitingWorksWithConjunctions_Simple()
		{
			var condition = UserDefinedConditionSplitterImpl.Split("A&&B");
			var evaluatedClauses = new HashSet<ZString>();

			Func<string, Func<bool>, bool> evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause != "A";
			});

			AssertEquals(false, condition.EvaluateBy(evaluator));
			AssertEquals(1, evaluatedClauses.Count);
			AssertEquals("A", evaluatedClauses.First());

			evaluatedClauses.Clear();
			evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return true;
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(2, evaluatedClauses.Count);
			Assert("A is evaluated", evaluatedClauses.Contains("A"));
			Assert("B is evaluated", evaluatedClauses.Contains("B"));
		}

		public void TestShortCircuitingWorksWithConjunctions_Complex()
		{
			var condition = UserDefinedConditionSplitterImpl.Split("(A&&B)&&(C||D)||((A&&B)&&(C||D))");
			var evaluatedClauses = new HashSet<ZString>();

			Func<string, Func<bool>, bool> evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause != "A";
			});

			AssertEquals(false, condition.EvaluateBy(evaluator));
			AssertEquals(1, evaluatedClauses.Count);
			AssertEquals("A", evaluatedClauses.First());
		}

		public void TestShortCircuitingWorksWithDisjunctions_Simple()
		{
			var condition = UserDefinedConditionSplitterImpl.Split("A||B");
			var evaluatedClauses = new HashSet<ZString>();

			Func<string, Func<bool>, bool> evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause != "A";
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(2, evaluatedClauses.Count);
			Assert("A is evaluated", evaluatedClauses.Contains("A"));
			Assert("B is evaluated", evaluatedClauses.Contains("B"));

			evaluatedClauses.Clear();
			evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return true;
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(1, evaluatedClauses.Count);
		}

		public void TestShortCircuitingWorksWithDisjunctions_Complex()
		{
			var condition = UserDefinedConditionSplitterImpl.Split("(A||B)||(C&&D)||((A&&B)&&(C||D))");
			var evaluatedClauses = new HashSet<ZString>();

			Func<string, Func<bool>, bool> evaluator = MakeEvaluator((string clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause == "A";
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(1, evaluatedClauses.Count);
			Assert("A", evaluatedClauses.Contains("A"));

			evaluatedClauses.Clear();

			evaluator = MakeEvaluator((clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause == "B";
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(2, evaluatedClauses.Count);
			Assert("A", evaluatedClauses.Contains("A"));
			Assert("B", evaluatedClauses.Contains("B"));

			evaluatedClauses.Clear();

			evaluator = MakeEvaluator((clause) =>
			{
				evaluatedClauses.Add(clause);
				return clause != "A" && clause != "B";
			});

			AssertEquals(true, condition.EvaluateBy(evaluator));
			AssertEquals(4, evaluatedClauses.Count);
			Assert("A", evaluatedClauses.Contains("A"));
			Assert("B", evaluatedClauses.Contains("B"));
			Assert("C", evaluatedClauses.Contains("C"));
			Assert("D", evaluatedClauses.Contains("D"));
		}

		void AssertSplit(ZString condition, ZString[] subConditions)
		{
			var result = UserDefinedConditionSplitterImpl.Split(condition);
			var resultChildren = result.Children.Select(c => c.AsString()).ToArray();

			if (subConditions.Length == 1)
			{
				AssertEquals(0, resultChildren.Length);
				AssertEquals(result.AsString(), subConditions[0]);
				return;
			}

			AssertEquals(subConditions.Length, resultChildren.Length);
			for (var i = 0; i < subConditions.Length; i++)
			{
				AssertEquals(subConditions[i], resultChildren[i]);
			}
		}

		void AssertNoSplit(ZString condition)
		{
			var result = UserDefinedConditionSplitterImpl.Split(condition);

			AssertEquals(0, result.Children.Count());
			AssertEquals(condition, result.AsString());
		}
	}
}
