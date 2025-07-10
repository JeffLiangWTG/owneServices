using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Enterprise.MasterData.Business
{
	public static class SupportNotifyPropertyChangedExtension
	{
		public static void NotifyPropertyChanged(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, [CallerMemberName] string property = null)
		{
			if (!string.IsNullOrEmpty(property))
			{
				supportNotifyPropertyChanged.NotifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property)?.Invoke();
			}
		}

		public static void RegisterNotifyPropertyChangeEvent(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, string property, string actionName, Action action)
		{
			var notifyPropertyChange = supportNotifyPropertyChanged?.NotifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property);
			if (notifyPropertyChange == null)
			{
				notifyPropertyChange = new PropertyChangedNotify
				{
					PropertyName = property,
				};

				supportNotifyPropertyChanged?.NotifyPropertyChanges.Add(notifyPropertyChange);
			}

			if (notifyPropertyChange.ChangedActions.ContainsKey(actionName))
			{
				notifyPropertyChange.ChangedActions.Remove(actionName);
			}

			notifyPropertyChange.ChangedActions.Add(actionName, action);
		}

		public static void ClearNotifyPropertyChangeEvent(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged, string property, string actionName = null)
		{
			var notifyPropertyChange = supportNotifyPropertyChanged.NotifyPropertyChanges.FirstOrDefault(u => u.PropertyName == property);
			if (actionName != null)
			{
				notifyPropertyChange?.ChangedActions.Remove(actionName);
			}
			else
			{
				notifyPropertyChange?.ChangedActions.Clear();
			}
		}

		public static IDisposable SuspendNotifyPropertyChanges(this ISupportNotifyPropertyChanged supportNotifyPropertyChanged)
		{
			return new NotifyPropertyChangesSuspender(supportNotifyPropertyChanged);
		}

		class NotifyPropertyChangesSuspender : IDisposable
		{
			readonly ISupportNotifyPropertyChanged supportNotifyPropertyChanged;

			public NotifyPropertyChangesSuspender(ISupportNotifyPropertyChanged supportNotifyPropertyChanged)
			{
				this.supportNotifyPropertyChanged = supportNotifyPropertyChanged;
				supportNotifyPropertyChanged.NotifyPropertyChanges.ForEach(u => u.SuspendNotify = true);
			}

			public void Dispose()
			{
				supportNotifyPropertyChanged.NotifyPropertyChanges.ForEach(u =>
				{
					u.SuspendNotify = false;

					if (u.DelayNotify)
					{
						u.Invoke();
					}
				});
			}
		}
	}
}
