using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class CommercialInvoiceEDIMenu : ZMenuItem
	{
		public CommercialInvoiceEDIMenu()
		{
			Caption = ResString.GetMultilingualString("fe0bab75-8239-44b4-8b86-acf405c6684f", "&Brokerage");

			autoApportionWeightMenuItem = new ZMenuItem(ResString.GetMultilingualString("26b10267-e6e4-4dff-bc27-41f23783ce4b", "Auto Apportion &Weight"), AutoApportionWeight_Click);
			copyPreviousInvoiceLineMenuItem = new ZMenuItem(ResString.GetMultilingualString("90240e52-99c3-46b9-8ea5-f2456bf2caa0", "&Copy Previous Invoice Line"), CopyPreviousInvoiceLine_Click);

			allocateRemainingWeightMenuItem = new ZMenuItem(ResString.GetMultilingualString("2AF7FCC6-F456-4EC6-872B-DB298FD82B47", "Allocate Remaining Weight"));
			allocateRemainingWeightbyPriceMenuItem = new ZMenuItem(ResString.GetMultilingualString("55BC4EDD-8C3A-4F77-9878-759DB8ECEF83", "Allocate Remaining Weight by Price"), new EventHandler(AllocateRemainingWeightbyPriceMenuItem_Click));
			allocateRemainingWeightMenuItem.MenuItems.Add(allocateRemainingWeightbyPriceMenuItem);
			allocateRemainingWeightbyQuantityMenuItem = new ZMenuItem(ResString.GetMultilingualString("9C0621E8-2707-4538-BE11-95F2DEFF1D44", "Allocate Remaining Weight by Quantity"), new EventHandler(AllocateRemainingWeightbyQuantityMenuItem_Click));
			allocateRemainingWeightMenuItem.MenuItems.Add(allocateRemainingWeightbyQuantityMenuItem);

			MenuItems.AddRange(MenuItemsInOrder().ToArray());
		}

		IEnumerable<MenuItem> MenuItemsInOrder()
		{
			yield return autoApportionWeightMenuItem;
			yield return allocateRemainingWeightMenuItem;
			yield return copyPreviousInvoiceLineMenuItem;
		}

		#region Allocate Remaining Weight
		void AllocateRemainingWeightbyPriceMenuItem_Click(object sender, EventArgs e)
		{
			AllocateRemainingWeight(AllocateRemainingWeightWay.ByPrice);
		}

		void AllocateRemainingWeightbyQuantityMenuItem_Click(object sender, EventArgs e)
		{
			AllocateRemainingWeight(AllocateRemainingWeightWay.ByQuantity);
		}

		void AllocateRemainingWeight(AllocateRemainingWeightWay way)
		{
			if (Declaration != null)
			{
				if (Declaration.JE_AutoWeightApportion)
				{
					if (Globals.Message.Show(
						Res.GetString("adfd84f2-3652-4fa8-8dfc-15a8008bcc74", "'Auto Apportion Weight' will be turned off before allocating weight. Do you want to continue?"),
						Res.GetString("969e49a7-586d-4491-9c1e-c3b49d00100a", "Warning - Auto Apportion Weight will be turned off"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					{
						Declaration.JE_AutoWeightApportion = false;
						new AllocateRemainingWeightWrapper(Declaration).ApportionRemaining(NotificationCollector, way);
					}
				}
				else
				{
					new AllocateRemainingWeightWrapper(Declaration).ApportionRemaining(NotificationCollector, way);
				}
			}
		}

		MessageNotificationCollector NotificationCollector => fNotificationCollector ?? (fNotificationCollector = new MessageNotificationCollector());
		MessageNotificationCollector fNotificationCollector;
		#endregion

		void AutoApportionWeight_Click(object sender, EventArgs e)
		{
			autoApportionWeightMenuItem.Checked = !autoApportionWeightMenuItem.Checked;
			Declaration.JE_AutoWeightApportion = autoApportionWeightMenuItem.Checked;
		}

		void CopyPreviousInvoiceLine_Click(object sender, EventArgs e)
		{
			copyPreviousInvoiceLineMenuItem.Checked = !copyPreviousInvoiceLineMenuItem.Checked;
			Declaration.CopyLastLineDetailsToNewLines = copyPreviousInvoiceLineMenuItem.Checked;
		}

		ICommonInvoiceDataProvider fDeclaration;

		public ICommonInvoiceDataProvider Declaration
		{
			get { return fDeclaration; }
			set
			{
				if (fDeclaration != value)
				{
					var oldValue = fDeclaration;
					fDeclaration = value;
					JobDeclarationChanged(oldValue, value);
					AddLockOrUnlockCustomsFileMenus();
				}
			}
		}

		ICustomsFileParent CustomsFileParent => Declaration?.CustomsFileParent;

		protected virtual void JobDeclarationChanged(ICommonInvoiceDataProvider oldValue, ICommonInvoiceDataProvider newValue)
		{
			autoApportionWeightMenuItem.Checked = newValue.JE_AutoWeightApportion;
			copyPreviousInvoiceLineMenuItem.Checked = newValue.JE_CopyLastInvoiceLineDetailsToNewLines;
		}

		protected virtual bool IsLockOrUnlockCustomsFileMenusEnabled => true;

		void AddLockOrUnlockCustomsFileMenus()
		{
			if (lockCustomsFileMenuItem == null && unlockCustomsFileMenuItem == null && Env.Security.LockOrUnlockFileForEdit.IsAllowed && IsLockOrUnlockCustomsFileMenusEnabled)
			{
				lockCustomsFileMenuItem = new ZMenuItem(LockCustomsFileMenuItemCaption, OnLockMenuClick);
				lockCustomsFileMenuItem.Name = nameof(lockCustomsFileMenuItem);

				unlockCustomsFileMenuItem = new ZMenuItem(UnlockCustomsFileMenuItemCaption, OnUnlockMenuClick);
				unlockCustomsFileMenuItem.Name = nameof(unlockCustomsFileMenuItem);

				MenuItems.Add(lockCustomsFileMenuItem);
				MenuItems.Add(unlockCustomsFileMenuItem);

				RefreshLockMenus();
			}
		}

		void OnLockMenuClick(object sender, EventArgs e)
		{
			if (Globals.IsTest)
			{
				WriteLockLog([(BusinessObject)CustomsFileParent], string.Empty);
			}
			else
			{
				using (var frm = new CustomsWriteToLogForm(CustomsFileParent, [(BusinessObject)CustomsFileParent], LockCustomsFileMenuItemCaption, WriteLockLog))
				{
					frm.ShowDialog();
				}
			}
		}

		void WriteLockLog(BusinessObject[] businessObjects, ZString reference)
		{
			foreach (var businessObject in businessObjects)
			{
				var fileParent = businessObject as ICustomsFileParent;
				fileParent?.LockFile(reference);
			}

			RefreshLockMenus();
		}

		void OnUnlockMenuClick(object sender, EventArgs e)
		{
			if (Globals.IsTest)
			{
				WriteUnlockLog([(BusinessObject)CustomsFileParent], string.Empty);
			}
			else
			{
				using (var frm = new CustomsWriteToLogForm(CustomsFileParent, [(BusinessObject)CustomsFileParent], UnlockCustomsFileMenuItemCaption, WriteUnlockLog))
				{
					frm.ShowDialog();
				}
			}
		}

		void WriteUnlockLog(BusinessObject[] businessObjects, ZString reference)
		{
			foreach (var businessObject in businessObjects)
			{
				var fileParent = businessObject as ICustomsFileParent;
				fileParent?.UnlockFile(reference);
			}

			RefreshLockMenus();
		}

		protected virtual MultilingualString LockCustomsFileMenuItemCaption => ResString.GetMultilingualString("5541BF37-238D-41AC-AB4E-3732B0E6F177", "&Lock Commercial Invoice");
		protected virtual MultilingualString UnlockCustomsFileMenuItemCaption => ResString.GetMultilingualString("2E2E2923-A92A-4427-9D81-8AC84DBE4054", "&Unlock Commercial Invoice");
		protected virtual bool IsLockCustomsFileMenuVisible() => true;

		void RefreshLockMenus()
		{
			if (lockCustomsFileMenuItem != null && unlockCustomsFileMenuItem != null)
			{
				if (IsLockCustomsFileMenuVisible() && CustomsFileParent is ICustomsFileParent fileParent)
				{
					var isInvoiceLocked = fileParent.IsLocked;

					lockCustomsFileMenuItem.Visible = !isInvoiceLocked;
					unlockCustomsFileMenuItem.Visible = isInvoiceLocked;
				}
				else
				{
					lockCustomsFileMenuItem.Visible = false;
					unlockCustomsFileMenuItem.Visible = false;
				}
			}
		}

		public bool FireSaveButton() => Form.FireSaveButton() == ContinueWithSave.Yes;

		protected ZForm Form
		{
			get { return (ZForm)GetMainMenu()?.GetForm(); }
		}

		protected bool IsViewOnly => Form?.DisplayMode == ODisplayMode.ReadOnly;

		public virtual void RefreshMenu()
		{
			if (Declaration is ICommonInvoiceDataProvider declaration)
			{
				autoApportionWeightMenuItem.Visible = (declaration as BaseJobDeclaration)?.IsWeightApportionmentSupported ?? true;
				autoApportionWeightMenuItem.Checked = declaration.JE_AutoWeightApportion;
			}
			else
			{
				autoApportionWeightMenuItem.Visible = false;
				autoApportionWeightMenuItem.Checked = false;
			}
			RefreshLockMenus();
		}

		void EnableMenuItemsAfterRefresh()
		{
			var fileParent = CustomsFileParent;
			bool isInvoiceLocked = fileParent?.IsLocked ?? false;
			bool isNotViewOnly = !IsViewOnly;
			if (menuItemEnabledDictionary == null)
			{
				menuItemEnabledDictionary = MenuItems.Cast<MenuItem>().ToDictionary(x => x, y => y.Enabled);
			}
			MenuItems.Cast<MenuItem>().ForEach(x =>
			{
				var enabled = false;
				if (isNotViewOnly)
				{
					if (!menuItemEnabledDictionary.TryGetValue(x, out enabled))
					{
						enabled = x.Enabled;
						menuItemEnabledDictionary.Add(x, enabled);
					}
				}
				x.Enabled = enabled && (object.ReferenceEquals(x, unlockCustomsFileMenuItem) ? isInvoiceLocked : !isInvoiceLocked);
			});
		}
		Dictionary<MenuItem, bool> menuItemEnabledDictionary;

		protected override void OnPopup(EventArgs e)
		{
			RefreshMenu();
			EnableMenuItemsAfterRefresh();
			base.OnPopup(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				MenuItems.Cast<MenuItem>().ForEach(x => x?.Dispose());
			}
			base.Dispose(disposing);
		}

		protected ZMenuItem lockCustomsFileMenuItem;
		protected ZMenuItem unlockCustomsFileMenuItem;
		protected MenuItem autoApportionWeightMenuItem;
		protected MenuItem copyPreviousInvoiceLineMenuItem;
		protected ZMenuItem allocateRemainingWeightMenuItem;
		readonly ZMenuItem allocateRemainingWeightbyPriceMenuItem;
		readonly ZMenuItem allocateRemainingWeightbyQuantityMenuItem;
	}
}
