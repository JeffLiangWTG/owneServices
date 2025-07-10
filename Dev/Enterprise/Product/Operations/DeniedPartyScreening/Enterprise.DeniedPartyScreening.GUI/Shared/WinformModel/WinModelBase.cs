using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public abstract class WinModelBase
	{
		List<NotifyPropertyChanged> NotifyPropertyChanges { get; } = new List<NotifyPropertyChanged>();

		protected void NotifyPropertyChanged([CallerMemberName] string property = null)
		{
			if (!string.IsNullOrEmpty(property))
			{
				NotifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property)?.Invoke();
			}
		}

		public void RegisterNotifyPropertyChangeEvent(string property, Action action, bool clearPreviousEvents)
		{
			NotifyPropertyChanges.RegisterNotifyPropertyChangeEvent(property, action, clearPreviousEvents);
		}

		public void ClearNotifyPropertyChangeEvent(string property)
		{
			NotifyPropertyChanges.ClearNotifyPropertyChangeEvent(property);
		}
	}
}
