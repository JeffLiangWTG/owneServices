using System;

namespace Enterprise.MasterFiles.GUI
{
	public interface ITaskDetailsMenuItem : IDisposable
	{
		void HandleMenuStripInit(object sender, int evt);
		void HandleMenuStripOpen(object sender, EventArgs eventArgs);
		void AttachToTaskDetailsControl(ITaskDetailsControl taskDetailsControl);
	}
}
