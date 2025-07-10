using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class SupportNotifyPropertyChangedExtension
	{
		public static void RegisterNotifyPropertyChangeEvent(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, string property, Action action, bool clearPreviousEvents)
		{
			supportNotifyPropertyChanged.NotifyPropertyChanges.RegisterNotifyPropertyChangeEvent(property, action, clearPreviousEvents);
		}

		public static void RegisterNotifyPropertyChangeEvent(this List<NotifyPropertyChanged> notifyPropertyChanges, string property, Action action, bool clearPreviousEvents)
		{
			var notifyPropertyChange = notifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property);
			if (notifyPropertyChange == null)
			{
				notifyPropertyChange = new NotifyPropertyChanged
				{
					PropertyName = property,
				};

				notifyPropertyChanges.Add(notifyPropertyChange);
			}
			else if (clearPreviousEvents)
			{
				notifyPropertyChange.ChangedActions.Clear();
			}

			notifyPropertyChange.ChangedActions.Add(action);
		}

		public static void ClearNotifyPropertyChangeEvent(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, string property)
		{
			supportNotifyPropertyChanged.NotifyPropertyChanges.ClearNotifyPropertyChangeEvent(property);
		}

		public static void ClearNotifyPropertyChangeEvent(this List<NotifyPropertyChanged> notifyPropertyChanges, string property)
		{
			notifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property)?.ChangedActions.Clear();
		}

		public static void NotifyPropertyChanged(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, [CallerMemberName] string property = null)
		{
			if (!string.IsNullOrEmpty(property))
			{
				supportNotifyPropertyChanged.NotifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property)?.Invoke();
			}
		}
	}
}
