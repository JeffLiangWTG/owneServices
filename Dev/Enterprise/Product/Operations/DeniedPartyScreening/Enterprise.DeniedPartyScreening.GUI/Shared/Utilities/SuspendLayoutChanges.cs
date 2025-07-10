using System;
namespace Enterprise.DeniedPartyScreening.GUI
{
	class SuspendLayoutChanges : IDisposable
	{
		readonly Action disposeAction;
		public SuspendLayoutChanges(Action action, Action disposeAction)
		{
			action.Invoke();
			this.disposeAction = disposeAction;
		}
		public void Dispose()
		{
			disposeAction?.Invoke();
		}
	}
}
