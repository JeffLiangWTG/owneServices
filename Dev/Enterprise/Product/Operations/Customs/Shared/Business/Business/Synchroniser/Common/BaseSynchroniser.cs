using System;

namespace Enterprise.Customs.Business
{
	public abstract class BaseSynchroniser : ISynchroniser, IDisposable
	{
		#region Enabled

		public void SetEnabled(bool enabled, bool enableDetection)
		{
			DetectEnabled = enableDetection;
			SetEnabled(enabled);
		}

		void SetEnabled(bool enabled)
		{
			if (fEnabled != enabled)
			{
				fEnabled = enabled;
				OnEnabledChanged();
			}
		}

		public bool IsEnabled
		{
			get { return fEnabled; }
		}

		protected virtual void OnEnabledChanged()
		{
			if (IsEnabled)
			{
				HookEvents();
			}
			else
			{
				UnHookEvents();
			}
		}

		protected abstract void HookEvents();
		protected abstract void UnHookEvents();

		bool fEnabled;

		#endregion

		public bool DetectEnabled
		{
			get { return detectEnabled; }
			set
			{
				if (detectEnabled != value)
				{
					detectEnabled = value;
					OnDetectEnabledChanged();
				}
			}
		}
		bool detectEnabled;

		protected abstract void OnDetectEnabledChanged();

		#region Synchronise

		public void Synchronise()
		{
			Synchronise(IsEnabled);
		}

		public void Synchronise(bool force)
		{
			if (force)
			{
				Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
			}
		}

		///TODO: I really think it's confusing, and should be refactored:
		///Actualy Start means Enable synchroniser, and Stop - Disable.
		///But to disable synchroniser I need to call Synchronise(Stop). And it's weird. Why should I call Synchronise if I need to disable? And what about Enable = false (Enable is public property)?
		///I think somebody made it to be able to override OnSynchronise and to do something like EnableIncludingChildren and DisableIncludingChildren (I think these methods are more meaningful).
		///But anyway these Start/Stop are rarely used in production code and Enabled can be used instead of. I think everything necessary is in ISynchroniser interface.
		///I wanted to get rid of it, but when I started I've got that it will take me few days to make everything work correctly and too many changes for FIX.
		///But I think it should be refactored later for alpha only.
		public void Synchronise(SynchroniseEventArgs e)
		{
			if (!isSynchronisationInProgress)
			{
				SyncChangesDetected = false;
				try
				{
					isSynchronisationInProgress = true;
					OnSynchronise(e);
					OnSynchronised();
				}
				finally
				{
					isSynchronisationInProgress = false;
				}
			}
		}

		protected virtual void OnSynchronise(SynchroniseEventArgs e)
		{
			switch (e.Action)
			{
				case SynchroniseAction.Start:
					SetEnabled(true);
					break;
				case SynchroniseAction.Stop:
					SetEnabled(false);
					break;
				case SynchroniseAction.Force:
					SetEnabled(true);
					ForceSynchronise();
					break;
			}
		}

		protected abstract void ForceSynchronise();

		protected virtual void OnSynchronised()
		{
		}

		bool isSynchronisationInProgress;

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			DisposeCore();
		}

		protected virtual void DisposeCore()
		{
			UnHookEvents();
		}

		#endregion

		#region ISynchroniser Members

		public bool SyncChangesDetected
		{
			get { return syncChangesDetected; }
			protected set
			{
				syncChangesDetected = value;
			}
		}
		bool syncChangesDetected;

		#endregion
	}

	public class SynchroniseEventArgs : EventArgs
	{
		public SynchroniseEventArgs(SynchroniseAction action)
		{
			Action = action;
		}

		public readonly SynchroniseAction Action;
	}

	public enum SynchroniseAction { Start, Force, Stop }
}
