using System;
using System.Collections.Generic;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public class NotifyPropertyChanged
	{
		public string PropertyName { get; set; }
		public List<Action> ChangedActions { get; set; } = new List<Action>();

		bool invoking;

		public void Invoke()
		{
			if (invoking)
			{
				return;
			}

			invoking = true;

			foreach (var action in ChangedActions)
			{
				action?.Invoke();
			}

			invoking = false;
		}
	}
}
