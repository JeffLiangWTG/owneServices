using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	class ActionChainTest : TestCase
	{
		public void TestConstructor()
		{
			var actionName = "NewTestAction";
			ActionStep actionStep = helper.ActionSuccess;
			var common = helper.SingleCustomChain;
			var success = helper.SingleSuccessChain;
			var failure = helper.SingleFailureChain;

			var chain = new ActionChainForTest(actionName, actionStep, common, success, failure);

			CombineAssertions(() =>
			{
				AssertEquals(actionName, chain.ActionName_Exposed);
				AssertSame(actionStep, chain.CurrentAction_Exposed);
				AssertSame(common, chain.CommonChain_Exposed);
				AssertSame(success, chain.SuccessChain_Exposed);
				AssertSame(failure, chain.FailureChain_Exposed);
			});
		}

		public void TestProcessSuccess()
		{
			var result = helper.SingleSuccessChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("1 Message expected", 1, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessActionStr, result.Notifications[0].Message);
		}

		public void TestProcessFailure()
		{
			var result = helper.SingleFailureChain.ProcessChain();
			AssertEquals("Sucess", false, result.Success);
			AssertEquals("1 Message expected", 1, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureActionStr, result.Notifications[0].Message);
		}

		public void TestProcessSuccessSuccess()
		{
			var result = helper.SuccessSuccessChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessChainSuccessStr, result.Notifications[1].Message);
		}

		public void TestProcessSuccessFailure()
		{
			var result = helper.SuccessFailureChain.ProcessChain();
			AssertEquals("Sucess", false, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessChainFailureStr, result.Notifications[1].Message);
		}

		public void TestProcessFailureSuccess()
		{
			var result = helper.FailureSuccessChain.ProcessChain(); // Success Action not triggered after failure
			AssertEquals("Sucess", true, result.Success);
			Assert("2 Messages expected", result.Notifications.Count == 2);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureChainSuccessStr, result.Notifications[1].Message);
		}

		public void TestProcessFailureFailure()
		{
			var result = helper.FailureFailureChain.ProcessChain(); // Failure Action not triggered after failure
			AssertEquals("Sucess", false, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureChainFailureStr, result.Notifications[1].Message);
		}

		public void TestProcessCustomResult()
		{
			var result = helper.SingleCustomChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("1 Message expected", 1, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.CustomCreatedStr, result.Notifications[0].Message);
			AssertType<ActionChainTestHelper.CustomActionResult>(result);
			AssertEquals("First Call Count", 1, ((ActionChainTestHelper.CustomActionResult)result).CallCount);
		}

		public void TestProcessCustomCustom()
		{
			var result = helper.DoubleCustomChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.CustomCreatedStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.CustomUpdatedStr, result.Notifications[1].Message);
			AssertType<ActionChainTestHelper.CustomActionResult>(result);
			AssertEquals("Double Call Count", 2, ((ActionChainTestHelper.CustomActionResult)result).CallCount);
		}

		public void TestProcessCommon()
		{
			var result = helper.CommonSuccessChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.CommonActionStr, result.Notifications[1].Message);

			result = helper.CommonFailureChain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("2 Messages expected", 2, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureActionStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.CommonActionStr, result.Notifications[1].Message);

			var chain = helper.CustomResultChain;

			helper.CustomResult = true;
			result = chain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("3 Messages expected", 3, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.CustomCreatedStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.SuccessActionStr, result.Notifications[1].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.CommonActionStr, result.Notifications[2].Message);

			helper.CustomResult = false;
			result = chain.ProcessChain();
			AssertEquals("Sucess", true, result.Success);
			AssertEquals("3 Messages expected", 3, result.Notifications.Count);
			AssertEquals("Msg Content", ActionChainTestHelper.CustomCreatedStr, result.Notifications[0].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.FailureActionStr, result.Notifications[1].Message);
			AssertEquals("Msg Content", ActionChainTestHelper.CommonActionStr, result.Notifications[2].Message);
		}

		public void TestNullCurrentAction()
		{
			var result = new ActionChain("Test", null).ProcessChain();
			AssertEquals("Sucess", true, result.Success);
		}

		public void TestProcessSubChain()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, false, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, true, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);

			p2.WrapInChain(s1, s1, true, ActionLink.Common);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[S1 In:True Out:True]\r\n[P2 In:True Out:True]\r\n[P3 In:True Out:False]\r\n[S2 In:False Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainAfter()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, false, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, true, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s1, true, ActionLink.Success);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { S1-P2-SubChain: { S1: success: { P2: success: { P3: } } } failure: { S2: success: { S3: } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[S1 In:True Out:True]\r\n[P2 In:True Out:True]\r\n[P3 In:True Out:False]\r\n[S2 In:False Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainAfterLinkage()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, false, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, false, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s1, true, ActionLink.Common);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { S1-P2-SubChain: { S1: common: { P2: success: { P3: } } } failure: { S2: success: { S3: } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[S1 In:True Out:False]\r\n[P2 In:False Out:True]\r\n[P3 In:True Out:False]\r\n[S2 In:False Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainAfter2nd()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, true, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, false, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s2, true, ActionLink.Success);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { S1: failure: { S2-P2-SubChain: { S2: success: { P2: success: { P3: } } } success: { S3: } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[S1 In:True Out:False]\r\n[S2 In:False Out:True]\r\n[P2 In:True Out:True]\r\n[P3 In:True Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainBefore()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, true, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, false, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s1, false, ActionLink.Success);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { P2-S1-SubChain: { P2: success: { P3: } } success: { S1: failure: { S2: success: { S3: } } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[P2 In:True Out:True]\r\n[P3 In:True Out:True]\r\n[S1 In:True Out:False]\r\n[S2 In:False Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainBeforeLinkage()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, false, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, false, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s1, false, ActionLink.Failure);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { P2-S1-SubChain: { P2: success: { P3: } } failure: { S1: failure: { S2: success: { S3: } } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[P2 In:True Out:True]\r\n[P3 In:True Out:False]\r\n[S1 In:False Out:False]\r\n[S2 In:False Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		public void TestWrapInChainBefore2nd()
		{
			var p1 = new ActionChainForTest("P1", (pr) => ActionResultHelper(pr, true, "P1"));
			var p2 = p1.AppendAction("P2", (pr) => ActionResultHelper(pr, true, "P2"));
			var p3 = p2.AppendAction("P3", (pr) => ActionResultHelper(pr, true, "P3"));

			var s1 = new ActionChainForTest("S1", (pr) => ActionResultHelper(pr, false, "S1"));
			var s2 = s1.AppendAction("S2", (pr) => ActionResultHelper(pr, true, "S2"), ActionLink.Failure);
			var s3 = s2.AppendAction("S3", (pr) => ActionResultHelper(pr, true, "S3"));

			p2.WrapInChain(s1, s2, false, ActionLink.Success);

			var result = p1.ProcessChain();
			CombineAssertions(() =>
			{
				AssertEquals("Sub Chain failed but result still true", true, result.Success);
				AssertEquals("Chain-String", "{ P1: success: { S1: failure: { P2-S2-SubChain: { P2: success: { P3: } } success: { S2: success: { S3: } } } } }", p1.GetChainAsString());
				AssertEquals("Notifications", "[P1 In:True Out:True]\r\n[S1 In:True Out:False]\r\n[P2 In:False Out:True]\r\n[P3 In:True Out:True]\r\n[S2 In:True Out:True]\r\n[S3 In:True Out:True]\r\n", result.Notifications.NotificationsAsString());
			});
		}

		ActionResult ActionResultHelper(ActionResult pr, bool success, string step)
		{
			var result = new ActionResult(success, pr.Notifications);
			result.Notifications.AddInformation($"[{step} In:{pr.Success} Out:{success}]");
			return result;
		}

		public void TestAppendAction()
		{
			var chain = new ActionChainForTest(helper.ActionSuccess);
			CombineAssertions("Pre-Condition", () =>
			{
				AssertNull("Common", chain.CommonChain_Exposed);
				AssertNull("Success", chain.SuccessChain_Exposed);
				AssertNull("Failure", chain.FailureChain_Exposed);
			});

			ActionStep commonStep = helper.CommonAction;
			ActionStep successStep = helper.ActionSuccess;
			ActionStep failureStep = helper.ActionFailure;

			var childCommon = chain.AppendAction("Child Action1", commonStep, ActionLink.Common);
			CombineAssertions("Common", () =>
			{
				AssertNotNull("Child Chain", childCommon);
				AssertSame("Child Action", commonStep, childCommon.CurrentAction_Exposed);
				AssertSame("Common", childCommon, chain.CommonChain_Exposed);
				AssertNull("Success", chain.SuccessChain_Exposed);
				AssertNull("Failure", chain.FailureChain_Exposed);
			});

			var childSuccess = chain.AppendAction("Child Action2", successStep);
			CombineAssertions("Success", () =>
			{
				AssertNotNull("Child Chain", childSuccess);
				AssertSame("Child Action", successStep, childSuccess.CurrentAction_Exposed);
				AssertSame("Common", childCommon, chain.CommonChain_Exposed);
				AssertSame("Success", childSuccess, chain.SuccessChain_Exposed);
				AssertNull("Failure", chain.FailureChain_Exposed);
			});

			var childFailure = chain.AppendAction("Child Action3", failureStep, ActionLink.Failure);
			CombineAssertions("Failure", () =>
			{
				AssertNotNull("Child Chain", childFailure);
				AssertSame("Child Action", failureStep, childFailure.CurrentAction_Exposed);
				AssertSame("Common", childCommon, chain.CommonChain_Exposed);
				AssertSame("Success", childSuccess, chain.SuccessChain_Exposed);
				AssertSame("Failure", childFailure, chain.FailureChain_Exposed);
			});
		}

		public void TestInsertActionAfter()
		{
			var chain = new ActionChainForTest(helper.ActionSuccess);
			ActionStep insertedStep = helper.ActionIgnoredChain;

			var successChain = chain.AppendAction("Success1", helper.ActionSuccess);
			var failureChain = chain.AppendAction("Failure1", helper.ActionFailure, ActionLink.Failure);
			var commonChain = chain.AppendAction("Common1", helper.CommonAction, ActionLink.Common);

			CombineAssertions("Pre-Condition", () =>
			{
				AssertNotNull("Common", chain.CommonChain_Exposed);
				AssertNotNull("Success", chain.SuccessChain_Exposed);
				AssertNotNull("Failure", chain.FailureChain_Exposed);
			});

			var inserted = chain.InsertActionAfter("Inserted action", insertedStep, ActionLink.Failure);

			CombineAssertions("After insert", () =>
			{
				AssertNotNull("New Action", inserted);
				AssertSame("New action", insertedStep, inserted.CurrentAction_Exposed);
				AssertSame("Child is new failure step of parent", inserted, chain.FailureChain_Exposed);
				AssertNull("Parent Common", chain.CommonChain_Exposed);
				AssertNull("Parent Success", chain.SuccessChain_Exposed);

				AssertSame("Child common", commonChain, inserted.CommonChain_Exposed);
				AssertSame("Child success", successChain, inserted.SuccessChain_Exposed);
				AssertSame("Child failure", failureChain, inserted.FailureChain_Exposed);
			});
		}

		public void TestInsertActionBefore()
		{
			var root = new ActionChainForTest(helper.ActionSuccess);
			var step1 = root.AppendAction("FinalStep", helper.ActionFailure);

			CombineAssertions("Pre-Condition", () =>
			{
				AssertNull("Common", root.CommonChain_Exposed);
				AssertSame("Success", step1, root.SuccessChain_Exposed);
				AssertNull("Failure", root.FailureChain_Exposed);
			});

			var step2 = step1.InsertActionBefore("InsStep1", helper.ActionSuccess);

			CombineAssertions("InsStep1 inserted before final step", () =>
			{
				AssertSame("Root-Success to InsStep1", step1, root.SuccessChain_Exposed);
				AssertSame("InsStep1-Success to Final", step2, step1.SuccessChain_Exposed);
				AssertNull("InsStep1-Common", step2.CommonChain_Exposed);
				AssertNull("InsStep1-Failure", step2.FailureChain_Exposed);
			});

			var step3 = step2.InsertActionBefore("InsStep2", helper.ActionSuccess, ActionLink.Common);

			CombineAssertions("InsStep2 inserted before final step", () =>
			{
				AssertSame("Root-Success to InsStep1 (no change)", step1, root.SuccessChain_Exposed);
				AssertSame("InsStep1-Success to InsStep2", step2, step1.SuccessChain_Exposed);
				AssertSame("InsStep2-Common to Final", step3, step2.CommonChain_Exposed);
				AssertNull("InsStep2-Success", step2.SuccessChain_Exposed);
				AssertNull("InsStep2-Failure", step2.FailureChain_Exposed);
			});

			var step4 = step3.InsertActionBefore("InsStep3", helper.ActionSuccess, ActionLink.Failure);
		}

		public void TestFindAction()
		{
			var root = new ActionChainForTest("Finder", helper.ActionSuccess);
			var level1 = root.AppendAction("Success@Level1", helper.ActionSuccess);
			var level2 = level1.AppendAction("Failure@Level2", helper.ActionSuccess, ActionLink.Failure);
			var level3 = level2.AppendAction("Common@Level3", helper.ActionSuccess, ActionLink.Common);
			root.AppendAction("Fluff@Level1", helper.ActionSuccess, ActionLink.Common);
			level1.AppendAction("Fluff@Level2", helper.ActionSuccess, ActionLink.Success);
			level2.AppendAction("Fluff@Level3", helper.ActionSuccess, ActionLink.Failure);

			CombineAssertions(() =>
			{
				AssertNull("Not to be found", root.FindAction("DOESNOTEXIST"));
				AssertSame("Level1", level1, root.FindAction("Success@Level1"));
				AssertSame("Level2", level2, root.FindAction("Failure@Level2"));
				AssertSame("Level3", level3, root.FindAction("Common@Level3"));
			});
		}

		public void TestFindActionWithSubChain()
		{
			var p1 = new ActionChainForTest("P1", helper.ActionSuccess);
			var p2 = p1.AppendAction("P2", helper.ActionSuccess);
			var p3 = p2.AppendAction("P3", helper.ActionSuccess);

			var s1 = new ActionChainForTest("S1", helper.ActionSuccess);
			var s2 = s1.AppendAction("S2", helper.ActionSuccess);

			p2.WrapInChain(s1, s1, false, ActionLink.Common);

			CombineAssertions(() =>
			{
				AssertNotNull("S1 not Found", p1.FindAction("S1"));
				AssertNotNull("S2 not Found", p1.FindAction("S2"));
				AssertNotNull("P2-S1-SubChain not Found", p1.FindAction("P2-S1-SubChain"));
			});
		}

		public void TestProcessChainSimulation()
		{
			var p1 = new ActionChainForTest("P1", helper.ActionSuccess);
			var p2 = p1.AppendAction("P2", helper.ActionSuccess);
			var p3 = p2.AppendAction("P3", helper.ActionSuccess);

			var results = ActionChainTestHelper.SimulateChain(p1);
			AssertContainsExactElementsInExactOrder("All successful", new[]
			{
				"Input: True Action: P1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: P2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: P3 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True"
			}, results);

			results = ActionChainTestHelper.SimulateChain(p1, new Dictionary<string, bool?>
			{
				{ "P2", false }
			});
			AssertContainsExactElementsInExactOrder("Failure on P2", new[]
			{
				"Input: True Action: P1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: P2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: False"
			}, results);
		}

		public void TestProcessChainSimulationInsertBefore()
		{
			var p1 = new ActionChainForTest("P1", helper.ActionSuccess);
			var p2 = p1.AppendAction("P2", helper.ActionSuccess);
			var p3 = p2.AppendAction("P3", helper.ActionSuccess);

			var i1 = p3.InsertActionBefore("I1", helper.CommonAction);

			var results = ActionChainTestHelper.SimulateChain(p1);
			AssertContainsExactElementsInExactOrder("Process after insert", new[]
			{
				"Input: True Action: P1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: P2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: I1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.CommonAction Output: True",
				"Input: True Action: P3 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True"
			}, results);
		}

		public void TestProcessChainSimulationInsertAfter()
		{
			var p1 = new ActionChainForTest("P1", helper.ActionSuccess);
			var p2 = p1.AppendAction("P2", helper.ActionSuccess);
			var p3 = p2.AppendAction("P3", helper.ActionSuccess);

			var i1 = p2.InsertActionAfter("I1", helper.CommonAction, ActionLink.Success);

			var results = ActionChainTestHelper.SimulateChain(p1);
			AssertContainsExactElementsInExactOrder("Process after insert", new[]
			{
				"Input: True Action: P1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: P2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
				"Input: True Action: I1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.CommonAction Output: True",
				"Input: True Action: P3 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True"
			}, results);
		}

		public void TestProcessChainSimulationWrapInChain()
		{
			var p1 = new ActionChainForTest("P1", helper.ActionSuccess);
			var p2 = p1.AppendAction("P2", helper.ActionSuccess);
			var p3 = p2.AppendAction("P3", helper.ActionSuccess);

			var s1 = new ActionChainForTest("S1", helper.ActionSuccess);
			var s2 = s1.AppendAction("S2", helper.ActionSuccess, ActionLink.Common);
			var s3 = s2.AppendAction("S3", helper.ActionSuccess, ActionLink.Failure);

			p2.WrapInChain(s1, s1, true, ActionLink.Success);

			CombineAssertions(() =>
			{
				var chainAsString = p1.GetChainAsString();
				AssertEquals("{ P1: success: { S1-P2-SubChain: { S1: success: { P2: success: { P3: } } } common: { S2: failure: { S3: } } } }", chainAsString);

				var results = ActionChainTestHelper.SimulateChain(p1, new Dictionary<string, bool?>
				{
					{ "P2", false }
				});
				AssertContainsExactElementsInExactOrder("Process after insert", new[]
				{
					"Input: True Action: P1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
					"Input: True Action: S1 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: True",
					"Input: True Action: P2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: False",
					"Input: False Action: S2 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: False",
					"Input: False Action: S3 Method: Enterprise.Customs.Business.MessagingProcess.Testing.ActionChainTestHelper.ActionSuccess Output: False",
				}, results);
			});
		}

		protected override void SetUp()
		{
			helper = new ActionChainTestHelper();
		}

		ActionChainTestHelper helper;
	}
}
