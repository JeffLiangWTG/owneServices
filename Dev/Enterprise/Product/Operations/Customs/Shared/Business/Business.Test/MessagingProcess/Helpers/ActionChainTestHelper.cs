using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.Business.MessagingProcess.Testing
{
	public class ActionChainForTest : ActionChain
	{
		public ActionChainForTest(ActionStep action, ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null) : this("UnitTest", action, commonChain, successChain, failureChain)
		{
		}
		public ActionChainForTest(string actionName, ActionStep action, ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null) : base(actionName, action, commonChain, successChain, failureChain)
		{
		}

		protected override ActionChain New(string actionName, ActionStep action, ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null)
		{
			return new ActionChainForTest(actionName, action, commonChain, successChain, failureChain);
		}

		public string ActionName_Exposed => actionName;
		public ActionStep CurrentAction_Exposed => currentAction;
		public ActionChainForTest CommonChain_Exposed => commonChain as ActionChainForTest;
		public ActionChainForTest SuccessChain_Exposed => successChain as ActionChainForTest;
		public ActionChainForTest FailureChain_Exposed => failureChain as ActionChainForTest;
		public ActionChainForTest SubChain_Exposed => subChain as ActionChainForTest;

		public new ActionChainForTest AppendAction(string actionName, ActionStep action, ActionLink link = ActionLink.Success) => (ActionChainForTest)base.AppendAction(actionName, action, link);
		public new ActionChainForTest InsertActionAfter(string actionName, ActionStep action, ActionLink replaceLink) => (ActionChainForTest)base.InsertActionAfter(actionName, action, replaceLink);
		public new ActionChainForTest InsertActionBefore(string actionName, ActionStep action, ActionLink link = ActionLink.Success) => (ActionChainForTest)base.InsertActionBefore(actionName, action, link);
		public new ActionChain WrapInChain(ActionChain subChain, ActionChain subChainAnchor, bool insertAfter, ActionLink subChainlink) => (ActionChainForTest)base.WrapInChain(subChain, subChainAnchor, insertAfter, subChainlink);
		public new ActionChainForTest FindAction(string actionName) => base.FindAction(actionName) as ActionChainForTest;
	}

	public class ActionChainTestHelper
	{
		public const string SuccessActionStr = "success action";
		public const string FailureActionStr = "success action";
		public const string CommonActionStr = "success action";
		const string SuccessChainStr = "success chain";
		const string FailureChainStr = "failure chain";
		const string IgnoredChainStr = "ignored chain - should not get this message";
		public const string SuccessChainSuccessStr = SuccessChainStr + " " + SuccessActionStr;
		public const string SuccessChainFailureStr = SuccessChainStr + " " + FailureActionStr;
		public const string FailureChainSuccessStr = FailureChainStr + " " + SuccessActionStr;
		public const string FailureChainFailureStr = FailureChainStr + " " + FailureActionStr;
		public const string CustomCreatedStr = "CustomActionResult created";
		public const string CustomUpdatedStr = "CustomActionResult updated";

		public ActionChainForTest SingleSuccessChain => new ActionChainForTest(ActionSuccess);
		public ActionChainForTest SingleFailureChain => new ActionChainForTest(ActionFailure);
		public ActionChainForTest SuccessSuccessChain => new ActionChainForTest(ActionSuccess, successChain: new ActionChainForTest(ActionSuccessChainSuccess), failureChain: new ActionChainForTest(ActionIgnoredChain));
		public ActionChainForTest SuccessFailureChain => new ActionChainForTest(ActionSuccess, successChain: new ActionChainForTest(ActionSuccessChainFailure), failureChain: new ActionChainForTest(ActionIgnoredChain));
		public ActionChainForTest FailureSuccessChain => new ActionChainForTest(ActionFailure, successChain: new ActionChainForTest(ActionIgnoredChain), failureChain: new ActionChainForTest(ActionFailureChainSuccess));
		public ActionChainForTest FailureFailureChain => new ActionChainForTest(ActionFailure, successChain: new ActionChainForTest(ActionIgnoredChain), failureChain: new ActionChainForTest(ActionFailureChainFailure));
		public ActionChainForTest SingleCustomChain => new ActionChainForTest(CustomAction);
		public ActionChainForTest DoubleCustomChain => new ActionChainForTest(CustomAction, successChain: new ActionChainForTest(CustomAction), failureChain: new ActionChainForTest(ActionFailureChainFailure));
		public ActionChainForTest CommonSuccessChain => new ActionChainForTest(ActionSuccess, commonChain: new ActionChainForTest(CommonAction), successChain: new ActionChainForTest(ActionIgnoredChain), failureChain: new ActionChainForTest(ActionIgnoredChain));
		public ActionChainForTest CommonFailureChain => new ActionChainForTest(ActionFailure, commonChain: new ActionChainForTest(CommonAction), successChain: new ActionChainForTest(ActionIgnoredChain), failureChain: new ActionChainForTest(ActionIgnoredChain));

		public ActionResult ActionSuccess(ActionResult previousResult) => new ActionResult(true, AppendMsg(previousResult.Notifications, SuccessActionStr));
		public ActionResult ActionFailure(ActionResult previousResult) => new ActionResult(false, AppendMsg(previousResult.Notifications, FailureActionStr));
		ActionResult ActionSuccessChainSuccess(ActionResult previousResult) => new ActionResult(true, AppendMsg(previousResult.Notifications, SuccessChainSuccessStr));
		ActionResult ActionSuccessChainFailure(ActionResult previousResult) => new ActionResult(false, AppendMsg(previousResult.Notifications, SuccessChainFailureStr));
		ActionResult ActionFailureChainSuccess(ActionResult previousResult) => new ActionResult(true, AppendMsg(previousResult.Notifications, FailureChainSuccessStr));
		ActionResult ActionFailureChainFailure(ActionResult previousResult) => new ActionResult(false, AppendMsg(previousResult.Notifications, FailureChainFailureStr));
		public ActionResult ActionIgnoredChain(ActionResult previousResult) => new ActionResult(false, AppendMsg(previousResult.Notifications, IgnoredChainStr));

		internal class CustomActionResult : ActionResult
		{
			public int CallCount { get; set; }
		}
		CustomActionResult CustomAction(ActionResult previousResult)
		{
			if (previousResult is CustomActionResult result)
			{
				result.CallCount++;
				result.Notifications = AppendMsg(previousResult.Notifications, CustomUpdatedStr);
			}
			else
			{
				result = new CustomActionResult();
				result.Success = previousResult.Success;
				result.Notifications = AppendMsg(previousResult.Notifications, CustomCreatedStr);
				result.CallCount = 1;
			}

			return result;
		}

		public bool CustomResult { get; set; }
		CustomActionResult CustomResultAction(ActionResult previousResult)
		{
			var result = CustomAction(previousResult);
			result.Success = CustomResult;
			return result;
		}

		public ActionChainForTest CustomResultChain
		{
			get
			{
				var finalStep = new ActionChainForTest(CommonAction);
				return new ActionChainForTest(CustomResultAction, successChain: new ActionChainForTest(ActionSuccess, commonChain: finalStep), failureChain: new ActionChainForTest(ActionFailure, commonChain: finalStep));
			}
		}
		public ActionResult CommonAction(ActionResult previousResult) => new ActionResult(true, AppendMsg(previousResult.Notifications, CommonActionStr));

		internal static MessageSendingNotificationCollection AppendMsg(MessageSendingNotificationCollection msgs, ZString newMsg)
		{
			if (msgs == null)
			{
				msgs = new MessageSendingNotificationCollection();
			}

			msgs.Add(new MessageSendingInformation(newMsg));
			return msgs;
		}

		public static string[] SimulateChain(ActionChain chain, Dictionary<string, bool?> outputResults = null)
		{
			return ConvertToTestChain(chain, outputResults).ProcessChain().Notifications.Select(x => x.Message.ToString()).ToArray();
		}

		static ActionChain ConvertToTestChain(ActionChain chain, Dictionary<string, bool?> outputResults)
		{
			ActionChain newChain = null;

			if (chain != null)
			{
				ActionStep actionStep = null;
				var methodName = string.Empty;

				if (chain.currentAction != null)
				{
					var target = chain.currentAction.Target;
					var field = target.GetType().GetField("methodName");
					if (field != null)
					{
						methodName = field.GetValue(target).ToString();
					}
					else
					{
						methodName = chain.currentAction.Method.Name;
					}

					var decType = chain.currentAction.Method.DeclaringType;
					while (decType.DeclaringType != null)
					{
						decType = decType.DeclaringType;
					}
					methodName = decType.FullName + "." + methodName;

					bool? success = null;
					outputResults?.TryGetValue(chain.actionName, out success);

					actionStep = (pr) => ActionResultHelper(pr, success, chain.actionName, methodName);
				}

				newChain = new ActionChain(chain.actionName, actionStep,
									ConvertToTestChain(chain.commonChain, outputResults),
									ConvertToTestChain(chain.successChain, outputResults),
									ConvertToTestChain(chain.failureChain, outputResults));
				newChain.subChain = ConvertToTestChain(chain.subChain, outputResults);
			}

			return newChain;
		}

		static ActionResult ActionResultHelper(ActionResult pr, bool? success, string actionName, string methodName)
		{
			var output = success ?? pr.Success;
			var result = new ActionResult(output, pr.Notifications);
			result.Notifications.AddInformation($"Input: {pr.Success} Action: {actionName} Method: {methodName} Output: {output}");
			return result;
		}
	}
}
