using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GUI
{
	public sealed class CustomsControlLockManager : BaseCustomsControlLockManager
	{
		public CustomsControlLockManager(ICustomsFileParent fileParent, Control owner)
			: base(fileParent, owner) { }

		protected override void OnDeclarationTypeChanged(object sender, EventArgs eventArgs)
		{
			var config = CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(fileParent.DeclarationType, fileParent.Logs);

			if (config != null)
			{
				var reference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Lock From Declaration Type - {0}", fileParent.DeclarationType);
				fileParent.LockFile(reference);
			}
		}

		protected override string GetLockFunctionNotAvailableWhenParentIsReadOnly() => Res.GetString("42a40dc6-6854-472b-af35-3f2d4e59e973", "The Declaration is Read-only, the function of lock is not available.");
		protected override string GetUnlockFunctionNotAvailableWhenParentIsReadOnly() => Res.GetString("7c1417a0-7815-4d73-b46f-4044e60df7f7", "The Declaration is Read-only, the function of unlock is not available.");
		protected override DeclarationLockConfig GetConfigForDeclarationType() => CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Find(fileParent.DeclarationType);

		protected override string GetTabPagesLockedForEditMessage(string tabPageNames)
		{
			return Res.GetString("b1b61528-c7fb-4c6b-856f-f1ade301d46d",
				"These tab pages have been locked for edit.\r\n\r\n{0}\r\n\r\nYou can click the {1} to unlock them.\r\nYou can change the lock config in the System Registry under {2}.",
				tabPageNames,
				UnlockCommandDescription,
				CustomsDataRegistry.Instance.DeclarationLockForEdit.HumanReadableRegistryPath());
		}

		public string UnlockCommandDescription { get; set; } = Res.GetString("A57E50BC-6B47-4EC2-AFD3-239E1205E535", "Brokerage - Unlock Customs Declaration");
	}

	public abstract class BaseCustomsControlLockManager : IDisposable
	{
		protected BaseCustomsControlLockManager(ICustomsFileParent fileParent, Control owner)
		{
			this.fileParent = Argument.NotNull(fileParent, nameof(fileParent));
			this.owner = Argument.NotNull(owner, nameof(owner));

			HookEvents();

			ControlLockInfos = new List<LockControlInfo>();
		}

		#region Implement

		bool IsLocked => ControlLockInfos.Any(c => c.IsLocked);

		protected readonly ICustomsFileParent fileParent;

		readonly Control owner;

		internal List<LockControlInfo> ControlLockInfos { get; private set; }

		public bool IsParentReadOnly => ((BusinessObject)fileParent).ReadOnly;

		#endregion

		#region Register

		public void Register(ZString key, Control control, string[] ignoreControlNames = null)
		{
			if (string.IsNullOrWhiteSpace(key) || control == null)
			{
				return;
			}

			var existingLockInfo = new LockControlInfo(key, control, ignoreControlNames);
			ControlLockInfos.Add(existingLockInfo);
		}

		public void Register(ZString key, ZPlugIn plugin, string[] ignoreControlNames = null)
		{
			var control = plugin?.TabPage;
			Register(key, control, ignoreControlNames);
		}

		#endregion

		#region Events

		void HookEvents()
		{
			fileParent.DeclarationTypeInfo.ValueChanged += OnDeclarationTypeChanged;
			fileParent.Logs.GetAllLogs().CountChanged += OnLogsCountChanged;

			if (owner is ZForm form)
			{
				form.Shown += OnParentControlFirstShown;
			}
			else if (owner is ZTabPage tabPage)
			{
				tabPage.TabInitialized += OnParentControlFirstShown;
			}
			else
			{
				if (owner.Parent != null)
				{
					OwnerOnParentChanged(owner, EventArgs.Empty);
				}
				else
				{
					owner.ParentChanged += OwnerOnParentChanged;
				}
			}
		}

		void UnHookEvents()
		{
			fileParent.DeclarationTypeInfo.ValueChanged -= OnDeclarationTypeChanged;
			fileParent.Logs.GetAllLogs().CountChanged -= OnLogsCountChanged;
			owner.ParentChanged -= OwnerOnParentChanged;
		}

		void OwnerOnParentChanged(object sender, EventArgs eventArgs)
		{
			owner.ParentChanged -= OwnerOnParentChanged;

			var form = owner.Parent as ZForm;
			if (form != null)
			{
				form.Shown += OnParentControlFirstShown;
				return;
			}

			var tabPage = owner.Parent as ZTabPage;
			if (tabPage != null)
			{
				tabPage.TabInitialized += OnParentControlFirstShown;
			}
		}

		protected abstract void OnDeclarationTypeChanged(object sender, EventArgs eventArgs);

		void OnLogsCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!((ISingleElementListInternal)fileParent).IsListChangeSuspended)
			{
				var log = e.ItemAdded ? e.BizObject as StmALog : null;

				if (log != null && !log.IsCancelled)
				{
					switch (log.SL_SE_NKEvent)
					{
						case AutoEvents.LockForEditCode:
							{
								CancelBeforeEvents(log);
								LockControls();
								break;
							}

						case AutoEvents.UnlockForEditCode:
							{
								CancelBeforeEvents(log);
								UnlockControls();
								break;
							}
					}
				}
			}
		}

		void CancelBeforeEvents(StmALog log)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.UnlockForEditCode, AutoEvents.LockForEditCode });
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(StmALogSchema.PK, SQLComparisonOperator.NotEqual, log.PK);

			var logsForCancel = fileParent.Logs.Find(query);
			logsForCancel.ForEach(c => c.Cancel());
		}

		void OnParentControlFirstShown(object sender, EventArgs e)
		{
			if (owner.Visible && !owner.IsDisposed && fileParent.IsLocked && !IsParentReadOnly)
			{
				LockControls();
			}
		}

		#endregion

		#region Lock Or Unlock Controls

		protected abstract string GetLockFunctionNotAvailableWhenParentIsReadOnly();
		protected abstract DeclarationLockConfig GetConfigForDeclarationType();

		void LockControls()
		{
			if (IsParentReadOnly)
			{
				Globals.Message.ShowInformation(GetLockFunctionNotAvailableWhenParentIsReadOnly());

				return;
			}

			var config = GetConfigForDeclarationType();

			if (config != null)
			{
				var tabPages = config.TabInfos.Cast<DeclarationTabLockInfo>().ToArray();

				if (tabPages.Any())
				{
					foreach (var lockInfo in ControlLockInfos.ToArray())
					{
						var control = lockInfo.Control;

						if (control == null || control.IsDisposed)
						{
							ControlLockInfos.Remove(lockInfo);
						}
						else
						{
							var locked = tabPages.Any(c => c.TabPage == Core.Constants.Customs.DeclarationTabPages.Codes.All || c.TabPage == lockInfo.Key);

							if (locked != lockInfo.IsLocked)
							{
								control.UpdateEditableIncludingChildren(!locked, lockInfo.IgnoreControlNames);
								lockInfo.IsLocked = locked;
							}
						}
					}

					if (IsLocked)
					{
						var affectedTabPages = tabPages
							.Where(c => c.TabPage == Core.Constants.Customs.DeclarationTabPages.Codes.All || ControlLockInfos.Any(d => d.Key == c.TabPage))
							.Select(c => c.TabPageDescription);

						var tabPageNames = string.Join(System.Environment.NewLine, affectedTabPages);
						Globals.Message.ShowInformation(GetTabPagesLockedForEditMessage(tabPageNames));
					}
				}
			}
		}

		protected abstract string GetTabPagesLockedForEditMessage(string tabPageNames);
		protected abstract string GetUnlockFunctionNotAvailableWhenParentIsReadOnly();

		void UnlockControls()
		{
			if (IsParentReadOnly)
			{
				Globals.Message.ShowInformation(GetUnlockFunctionNotAvailableWhenParentIsReadOnly());

				return;
			}

			foreach (var lockInfo in ControlLockInfos.ToArray())
			{
				var control = lockInfo.Control;

				if (control == null || control.IsDisposed)
				{
					ControlLockInfos.Remove(lockInfo);
				}
				else if (lockInfo.IsLocked)
				{
					control.UpdateEditableIncludingChildren(true, lockInfo.IgnoreControlNames);
					lockInfo.IsLocked = false;
				}
			}

			var message = Res.GetString("585ec74e-7b42-4e22-94a3-a5c301be27a7", "All tab pages are unlocked.");
			Globals.Message.ShowInformation(message);
		}

		#endregion

		#region IDisposable

		public void Dispose()
		{
			ControlLockInfos.Clear();
			UnHookEvents();
		}

		#endregion

		#region LockInfo

		internal sealed class LockControlInfo
		{
			public LockControlInfo(ZString key, Control control, string[] ignoreControlNames)
			{
				Key = key;
				Control = control;
				IsLocked = false;
				IgnoreControlNames = ignoreControlNames;
			}

			public ZString Key { get; }

			public Control Control { get; }

			public ZBool IsLocked { get; set; }

			public string[] IgnoreControlNames { get; }
		}

		#endregion
	}
}
