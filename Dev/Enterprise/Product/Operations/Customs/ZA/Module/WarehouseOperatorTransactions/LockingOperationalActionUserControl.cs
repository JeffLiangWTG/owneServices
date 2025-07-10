using System;
using CargoWise.Windows.UI;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module
{
	public class LockingOperationalActionUserControl : ZUserControl
	{
		protected LockingOperationalActionUserControl()
		{
			Disposed += (s, e) => { ReleaseLockIfNeeded(); };
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			opMutexRef = ((BaseExportApplicator)DataSource).Mutex;
			var isLockedToCurrentOp = opMutexRef?.HasLock ?? false;
			var topRunnerForm = this.GetTopLevelNonParentedControl();

			UpdateGuiForLockStatus(isLockedToCurrentOp);
			if (topRunnerForm?.GetField("okButton") is ZButton okButton)
			{
				okButton.Visible = isLockedToCurrentOp;
			}
		}

		protected virtual void UpdateGuiForLockStatus(bool isLockedToCurrentOp)
		{
		}

		ZGlobalMutex opMutexRef;
		void ReleaseLockIfNeeded()
		{
			if (opMutexRef != null)
			{
				if (opMutexRef.IsLocked && opMutexRef.HasLock)
				{
					opMutexRef.Unlock();
				}
				((IDisposable)opMutexRef).Dispose();
				opMutexRef = null;
			}
		}
	}
}
