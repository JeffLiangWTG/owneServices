namespace Enterprise.Customs.Business.MessagingProcess
{
	public delegate ActionResult ActionStep(ActionResult previousResult);

	public enum ActionLink
	{
		Common,
		Success,
		Failure
	}

	public class ActionChain
	{
		protected internal string actionName;
		protected internal ActionStep currentAction;
		protected internal ActionChain commonChain;
		protected internal ActionChain successChain;
		protected internal ActionChain failureChain;
		protected internal ActionChain subChain;

		public ActionChain(string actionName, ActionStep action, ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null)
		{
			this.actionName = actionName;
			currentAction = action;
			ConfigureChains(commonChain, successChain, failureChain);
		}

		public void ConfigureChains(ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null)
		{
			this.commonChain = commonChain;
			this.successChain = successChain;
			this.failureChain = failureChain;
		}

		public ActionResult ProcessChain()
		{
			var initialResult = new ActionResult(true);

			return ProcessChain(initialResult);
		}

		public ActionResult ProcessChain(ActionResult previousResult)
		{
			var result = previousResult;
			var chainToProcess = this;

			while (chainToProcess != null)
			{
				result = chainToProcess.currentAction != null ? chainToProcess.currentAction.Invoke(result) :
						chainToProcess.subChain != null ? chainToProcess.subChain.ProcessChain(result) :
						result;

				chainToProcess = GetNextChain(chainToProcess, result.Success);
			}

			return result;
		}

		static ActionChain GetNextChain(ActionChain chain, bool previousSuccess)
		{
			if (chain.commonChain != null)
			{
				return chain.commonChain;
			}
			else if (previousSuccess)
			{
				if (chain.successChain != null)
				{
					return chain.successChain;
				}
			}
			else
			{
				if (chain.failureChain != null)
				{
					return chain.failureChain;
				}
			}

			return null;
		}

		protected virtual ActionChain New(string actionName, ActionStep action, ActionChain commonChain = null, ActionChain successChain = null, ActionChain failureChain = null) => new ActionChain(actionName, action, commonChain, successChain, failureChain);

		public ActionChain AppendAction(string actionName, ActionStep action, ActionLink link = ActionLink.Success)
		{
			var newAction = New(actionName, action);

			switch (link)
			{
				case ActionLink.Common:
					commonChain = newAction;
					break;
				case ActionLink.Success:
					successChain = newAction;
					break;
				case ActionLink.Failure:
					failureChain = newAction;
					break;
			}

			return newAction;
		}

		public ActionChain InsertActionAfter(string actionName, ActionStep action, ActionLink replaceLink)
		{
			var newAction = New(actionName, action, commonChain, successChain, failureChain);

			switch (replaceLink)
			{
				case ActionLink.Common:
					commonChain = newAction;
					successChain = null;
					failureChain = null;
					break;
				case ActionLink.Success:
					successChain = newAction;
					commonChain = null;
					failureChain = null;
					break;
				case ActionLink.Failure:
					failureChain = newAction;
					successChain = null;
					commonChain = null;
					break;
			}

			return newAction;
		}

		public ActionChain InsertActionBefore(string actionName, ActionStep action, ActionLink link = ActionLink.Success)
		{
			var newAction = New(this.actionName, this.currentAction, this.commonChain, this.successChain, this.failureChain);

			this.actionName = actionName;
			this.currentAction = action;

			switch (link)
			{
				case ActionLink.Common:
					commonChain = newAction;
					successChain = null;
					failureChain = null;
					break;
				case ActionLink.Success:
					successChain = newAction;
					commonChain = null;
					failureChain = null;
					break;
				case ActionLink.Failure:
					failureChain = newAction;
					successChain = null;
					commonChain = null;
					break;
			}

			return newAction;
		}

		ActionChain InsertSubChainAfter(ActionChain newSubChain, ActionLink subChainlink)
		{
			var holdCommon = commonChain;
			var holdSuccess = successChain;
			var holdFailure = failureChain;

			var chain = New(this.actionName, currentAction,
				subChainlink == ActionLink.Common ? newSubChain : null,
				subChainlink == ActionLink.Success ? newSubChain : null,
				subChainlink == ActionLink.Failure ? newSubChain : null);

			this.actionName = $"{this.actionName}-{newSubChain.actionName}-SubChain";
			this.currentAction = null;
			this.subChain = chain;
			commonChain = holdCommon;
			successChain = holdSuccess;
			failureChain = holdFailure;

			return chain;
		}

		ActionChain InsertSubChainBefore(ActionChain newSubChain, ActionLink subChainlink)
		{
			var copyOfAnchor = New(this.actionName, this.currentAction, this.commonChain, this.successChain, this.failureChain);

			this.actionName = $"{newSubChain.actionName}-{copyOfAnchor.actionName}-SubChain";
			this.currentAction = null;
			this.subChain = newSubChain;

			switch (subChainlink)
			{
				case ActionLink.Common:
					commonChain = copyOfAnchor;
					successChain = null;
					failureChain = null;
					break;
				case ActionLink.Success:
					successChain = copyOfAnchor;
					commonChain = null;
					failureChain = null;
					break;
				case ActionLink.Failure:
					failureChain = copyOfAnchor;
					successChain = null;
					commonChain = null;
					break;
			}

			return copyOfAnchor;
		}

		public ActionChain WrapInChain(ActionChain secondaryChain, ActionChain secondaryChainAnchor, bool insertAfter, ActionLink subChainlink)
		{
			var copyOfCurrent = New(this.actionName, this.currentAction, this.commonChain, this.successChain, this.failureChain);
			ActionChain newChain = secondaryChain;

			if (insertAfter)
			{
				_ = secondaryChainAnchor.InsertSubChainAfter(copyOfCurrent, subChainlink);
			}
			else
			{
				_ = secondaryChainAnchor.InsertSubChainBefore(copyOfCurrent, subChainlink);
			}

			if (newChain != null)
			{
				this.actionName = secondaryChain.actionName;
				this.currentAction = secondaryChain.currentAction;
				this.subChain = secondaryChain.subChain;
				this.commonChain = secondaryChain.commonChain;
				this.successChain = secondaryChain.successChain;
				this.failureChain = secondaryChain.failureChain;
			}

			return this;
		}

		public ActionChain FindAction(string actionName) => FindAction(this, actionName);
		ActionChain FindAction(ActionChain root, string actionName)
		{
			if (root == null)
			{
				return null;
			}

			if (root.actionName == actionName)
			{
				return root;
			}

			return FindAction(root.subChain, actionName) ??
				   FindAction(root.commonChain, actionName) ??
				   FindAction(root.successChain, actionName) ??
				   FindAction(root.failureChain, actionName);
		}
	}
}
