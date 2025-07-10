using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CustomsNumberViewStmNumsTabPageUserControl : ZUserControl
	{
		public CustomsNumberViewStmNumsTabPageUserControl()
		{
			InitializeComponent();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			if (IsVisibleForBinding)
			{
				LoadOrReloadUserControl();
			}
		}

		ICustomsNumberViewStmNumsParent StmNumsParent => (ICustomsNumberViewStmNumsParent)CurrentDataItem;
		CustomsNumberViewStmNumsBusinessProvider currentProvider;
		ICustomsNumberViewStmNumsGuiProvider CurrentGuiProvider => (ICustomsNumberViewStmNumsGuiProvider)currentProvider;

		public void LoadOrReloadUserControl()
		{
			var provider = StmNumsParent?.CustomsNumberProvider;
			if (currentProvider != provider)
			{
				UnloadUserControl();
			}
			ClearRelatedDataAndNotification();
			currentProvider = provider;
			if (currentProvider != null)
			{
				var mutex = CustomsNumberRangeEditMutex;
				if (LockCustomsNumberRangeEditMutex(mutex))
				{
					label1.Visible = false;
					currentProvider.SetupRelatedDataAndNotification();
					if (Controls.Count <= 1)
					{
						var userControl = (ZUserControl)CurrentGuiProvider.GetUserControl();
						userControl.Name = "NumberRangesControl";
						userControl.Dock = DockStyle.Fill;
						Controls.Add(userControl);
						userControl.SetDataBinding(StmNumsParent, "");
					}
				}
				else if (mutex != null)
				{
					label1.Text = GetCustomsNumberRangeEditMutexLockInfo(mutex);
					label1.Visible = true;
				}
			}
		}

		void UnloadUserControl()
		{
			var controls = Controls.OfType<Control>().Where(x => x != label1).ToList();
			controls.ForEach(c =>
			{
				Controls.Remove(c);
				c.Dispose();
			});
		}

		bool LockCustomsNumberRangeEditMutex(ZGlobalMutex mutex)
		{
			return mutex != null && (mutex.IsLocked ? (bool)mutex.HasLock : mutex.Lock());
		}

		string GetCustomsNumberRangeEditMutexLockInfo(ZGlobalMutex mutex)
		{
			var lockInfo = mutex.GetLockInfo();
			return Res.GetString("{DDB0BD5A-E3F1-413A-98F2-9CDED52195F2}", "{0} is in the process of updating Number Ranges.\r\nOnly one person is allowed to update Number Range at a time.",
				lockInfo == null ?
				Res.GetString("{D3849640-1504-4BA8-A674-5149AA58243F}", "Someone else") :
				Res.GetString("{DDA653B1-1061-4B66-B6F2-FED11DF83C15}", "{0} ({1} since {2})", lockInfo.UserWithLock.GS_FullName, lockInfo.HostName, lockInfo.LockStartTime));
		}

		ZGlobalMutex CustomsNumberRangeEditMutex
		{
			get
			{
				if (customsNumberRangeEditMutex == null && StmNumsParent != null)
				{
					customsNumberRangeEditMutex = new ZGlobalMutex(MutexIDs.DataProcessing, "CustomsNumberRangeEdit" + StmNumsParent.PK.ToString());
				}
				return customsNumberRangeEditMutex;
			}
		}
		ZGlobalMutex customsNumberRangeEditMutex;

		void ClearRelatedDataAndNotification()
		{
			currentProvider?.ClearRelatedDataAndNotification();
			currentProvider = null;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (customsNumberRangeEditMutex != null)
			{
				if (customsNumberRangeEditMutex != null && customsNumberRangeEditMutex.IsLocked && customsNumberRangeEditMutex.HasLock)
				{
					customsNumberRangeEditMutex.Unlock();
				}
				((IDisposable)customsNumberRangeEditMutex).Dispose();
				customsNumberRangeEditMutex = null;
			}
			if (disposing)
			{
				ClearRelatedDataAndNotification();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
