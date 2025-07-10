using System;
using System.Collections.Generic;

namespace Enterprise.MasterData.Business
{
	public class PropertyChangedNotify
	{
		public string PropertyName { get; set; }
		public Dictionary<string, Action> ChangedActions { get; } = new Dictionary<string, Action>();
		public bool SuspendNotify { get; internal set; }
		public bool DelayNotify { get; private set; }

		bool invoking;

		public void Invoke()
		{
			if (invoking)
			{
				return;
			}

			if (SuspendNotify)
			{
				DelayNotify = true;
				return;
			}

			invoking = true;

			foreach (var action in ChangedActions.Values)
			{
				action?.Invoke();
			}

			DelayNotify = false;

			invoking = false;
		}
	}
}
