using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IDeclarationWarehouseIntegrationSupporter = Enterprise.Customs.Business.WarehouseExtensions.IDeclarationWarehouseIntegrationSupporter;

namespace Enterprise.Customs.GUI
{
	public class BondedWarehouseMenuItemsForGridCreator : BondedWarehouseMenuItemsCreator
	{
		public BondedWarehouseMenuItemsForGridCreator(IBondedWarehouseMenuItemsForGridCreatorSupporter creatorSupporter)
			: base(null, creatorSupporter, ResString.GetMultilingualString("93949F53-B92B-4BC5-8405-1C6FB616A115", "Inventory Management"), supportShortCutMenu: false)
		{
			grid = creatorSupporter.GetGrid();
			grid.AfterBind += Grid_AfterBind;
			if (grid.ContextMenu == null)
			{
				grid.ContextMenu = new ContextMenu();
			}
			grid.ContextMenu.MenuItems.Add(BondedWarehouseMenuItem);
			grid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void Grid_AfterBind(object sender, EventArgs e)
		{
			grid.ListManager.PositionChanged += gridListManager_PositionChanged;
			gridListManager_PositionChanged(null, null);
		}

		void gridListManager_PositionChanged(object sender, EventArgs e)
		{
			IDeclarationWarehouseIntegrationSupporter supporter = null;
			var listManager = grid.ListManager;
			if (listManager != null)
			{
				supporter = creatorSupporter.GetSupporter(listManager.GetCurrent());
				if (supporter != null && supporter.IsDeleted)
				{
					supporter = null;
				}
			}

			if (currentSupporter != supporter)
			{
				currentSupporter = supporter;
			}
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup -= ContextMenu_Popup;
				if (BondedWarehouseMenuItem != null)
				{
					grid.ContextMenu.MenuItems.Remove(BondedWarehouseMenuItem);
				}
			}
			base.Dispose(disposing);
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			BondedWarehouseMenuItem.Visible = creatorSupporter.IsEnabled && currentSupporter is { IsActive: true, IsDeleted: false } &&
				(!currentSupporter.IsBondedWarehousingDisabled || currentSupporter.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails) &&
				(currentSupporter.IsOutwardBondedWarehousingEnabled || currentSupporter.IsInwardBondedWarehousingEnabled || Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.HasWHSTransaction(currentSupporter));
		}

		readonly ZGrid grid;
		protected new IBondedWarehouseMenuItemsForGridCreatorSupporter creatorSupporter
		{
			get { return (IBondedWarehouseMenuItemsForGridCreatorSupporter)base.creatorSupporter; }
		}
	}

	public class BondedWarehouseMenuItemsCreator : IDisposable
	{
		public BondedWarehouseMenuItemsCreator(IDeclarationWarehouseIntegrationSupporter currentSupporter, IBondedWarehouseMenuItemsCreatorSupporter creatorSupporter, string bondedWarehouseMenuItemText, bool supportShortCutMenu = false)
		{
			this.currentSupporter = currentSupporter;
			this.creatorSupporter = Argument.NotNull(creatorSupporter, "creatorSupporter");
			updateBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("{5406E31B-9B1D-42E8-BD4C-C442DFC5982B}", "&Update Inventory"));
			updateBondedWarehouseMenuItem.Click += UpdateBondedWarehouseMenuItem_Click;

			cancelUpdateBondedWarehouseInwardMenuItem = new ZMenuItem(ResString.GetMultilingualString("{29AD010A-0385-4A3C-8F02-8EE174914F9F}", "&Cancel Inventory"));
			cancelUpdateBondedWarehouseInwardMenuItem.Click += CancelUpdateBondedWarehouseInwardMenuItem_Click;

			cancelUpdateBondedWarehouseOutwardMenuItem = new ZMenuItem(ResString.GetMultilingualString("{EE05A56E-D973-4320-B714-857518527A49}", "&Cancel Inventory Stock Release"));
			cancelUpdateBondedWarehouseOutwardMenuItem.Click += CancelUpdateBondedWarehouseOutwardMenuItem_Click;

			cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem = new ZMenuItem(ResString.GetMultilingualString("{43EDD0A5-348C-479F-B0CB-8D32B1F4B7CC}", "&Cancel Bonded Warehouse Change Of Ownership"));
			cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem.Click += CancelUpdateBondedWarehouseChangeOfOwnershipMenuItem_Click;

			cancelUpdateInventoryChangeOfRegimeMenuItem = new ZMenuItem(ResString.GetMultilingualString("{0995C64B-BE80-4319-BEB0-500BFEAC7D22}", "&Cancel Inventory Change Of Regime"));
			cancelUpdateInventoryChangeOfRegimeMenuItem.Click += CancelUpdateInventoryChangeOfRegimeMenuItem_Click;

			disableBondedWarehouseIntegrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("{0E555340-8E9D-4871-9C85-A793026B5F43}", "&Disable Integration"));
			disableBondedWarehouseIntegrationMenuItem.Click += DisableBondedWarehouseIntegrationMenuItem_Click;

			BondedWarehouseMenuItem = new ZMenuItem(bondedWarehouseMenuItemText, new MenuItem[] { updateBondedWarehouseMenuItem, cancelUpdateBondedWarehouseInwardMenuItem, cancelUpdateBondedWarehouseOutwardMenuItem, cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem, cancelUpdateInventoryChangeOfRegimeMenuItem, disableBondedWarehouseIntegrationMenuItem });
			BondedWarehouseMenuItem.Popup += BondedWarehouseMenuItem_Popup;

			if (supportShortCutMenu)
			{
				ShortCutUpdateBondedWarehouseMenuItem = new ZMenuItem(
					MultilingualString.Join(" - ",
						ResString.GetMultilingualString("EAE4BD4D-E2C0-4842-A787-0FF798CDE9E8",
						"&Update Bonded Warehouse"),
						(NoResString)bondedWarehouseMenuItemText));
				ShortCutUpdateBondedWarehouseMenuItem.Click += UpdateBondedWarehouseMenuItem_Click;
			}

#if DEBUG
			System.ComponentModel.TypeDescriptor.AddAttributes(BondedWarehouseMenuItem, new SuppressFormsLocalizedTestAttribute());
			if (ShortCutUpdateBondedWarehouseMenuItem != null)
			{
				System.ComponentModel.TypeDescriptor.AddAttributes(ShortCutUpdateBondedWarehouseMenuItem, new SuppressFormsLocalizedTestAttribute());
			}

#endif
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed")]
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (updateBondedWarehouseMenuItem != null)
				{
					updateBondedWarehouseMenuItem.Dispose();
					updateBondedWarehouseMenuItem = null;
				}
				if (cancelUpdateBondedWarehouseInwardMenuItem != null)
				{
					cancelUpdateBondedWarehouseInwardMenuItem.Dispose();
					cancelUpdateBondedWarehouseInwardMenuItem = null;
				}
				if (cancelUpdateBondedWarehouseOutwardMenuItem != null)
				{
					cancelUpdateBondedWarehouseOutwardMenuItem.Dispose();
					cancelUpdateBondedWarehouseOutwardMenuItem = null;
				}
				if (disableBondedWarehouseIntegrationMenuItem != null)
				{
					disableBondedWarehouseIntegrationMenuItem.Dispose();
					disableBondedWarehouseIntegrationMenuItem = null;
				}
				if (ShortCutUpdateBondedWarehouseMenuItem != null)
				{
					ShortCutUpdateBondedWarehouseMenuItem.Dispose();
					ShortCutUpdateBondedWarehouseMenuItem = null;
				}
				if (BondedWarehouseMenuItem != null)
				{
					BondedWarehouseMenuItem.Popup -= BondedWarehouseMenuItem_Popup;
					BondedWarehouseMenuItem.Dispose();
					BondedWarehouseMenuItem = null;
				}
			}
		}

		protected IDeclarationWarehouseIntegrationSupporter currentSupporter;
		readonly protected IBondedWarehouseMenuItemsCreatorSupporter creatorSupporter;

		bool CheckHasChanges(Func<bool> extraCheck = null)
		{
			bool okToContinue = true;
			if (creatorSupporter.TopLevelBusinessObjectHasChanges())
			{
				Globals.Message.ShowError(Res.GetString("{C9BA3B57-1AAE-42D6-A455-AD157B4C1BD8}", "Please save all changes before proceeding."));
				okToContinue = false;
			}

			if (okToContinue && extraCheck != null)
			{
				okToContinue = extraCheck();
			}
			return okToContinue;
		}

		void DisableBondedWarehouseIntegrationMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsDisable.IsAllowed)
			{
				if (currentSupporter != null && CheckHasChanges())
				{
					currentSupporter.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					if (creatorSupporter.FireSaveButton() == ContinueWithSave.Yes)
					{
						Globals.Message.ShowInformation(Res.GetString("{97D57666-08CA-4582-AB12-F121BDACB859}", "Inventory Management Integration has been disabled."));
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsDisable);
			}
		}

		void CancelUpdateBondedWarehouseChangeOfOwnershipMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (currentSupporter != null && CheckHasChanges(CanCancelBondedWarehouseChangeOfOwnershipCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false) &&
				Globals.Message.Show(Res.GetString("{42F96F9D-CDB9-4B15-8BBF-1355F87324C7}", "Are you sure you want to cancel the Bonded Warehouse Change Of Ownership for this job? \r\nIf you click 'Yes', all stock for this job will be uncommitted in the Bonded Warehouse."),
					Res.GetString("{90E45469-425B-4D22-BE8E-232D2D7796BA}", "Are you sure?"), MessageBoxButtons.YesNo, DialogResult.No)
					== DialogResult.Yes)
				{
					currentSupporter.CancelBondedWarehouseChangeOfOwnership();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void CancelUpdateInventoryChangeOfRegimeMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (currentSupporter != null && CheckHasChanges(CanCancelInventoryChangeOfRegimeCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false) &&
				Globals.Message.Show(Res.GetString("{82240225-0D9E-47C2-ABF1-8F0A47877942}", "Are you sure you want to cancel the Inventory Change Of Regime for this job? \r\nIf you click 'Yes', all stock for this job will be uncommitted in the Inventory."),
					Res.GetString("{25928D77-C962-4C52-BFA9-14934762312B}", "Are you sure?"), MessageBoxButtons.YesNo, DialogResult.No)
					== DialogResult.Yes)
				{
					currentSupporter.CancelInventoryChangeOfRegime();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void CancelUpdateBondedWarehouseOutwardMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (currentSupporter != null && CheckHasChanges(CanCancelBondedWarehouseOutwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false) &&
				Globals.Message.Show(Res.GetString("1fe2e605-934a-4daf-a4ab-df3a98acaecc", "Are you sure you want to cancel the Inventory stock release for this job? \r\nIf you click 'Yes', all stock for this job will be uncommitted in the Inventory Management."),
					Res.GetString("bd12397f-febe-4d9f-8b83-8d8be0384f3c", "Are you sure?"), MessageBoxButtons.YesNo, DialogResult.No)
					== DialogResult.Yes)
				{
					currentSupporter.CancelBondedWarehouseOutward();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void CancelUpdateBondedWarehouseInwardMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (CheckHasChanges(CanCancelUpdateBondedWarehouseInwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false))
				{
					currentSupporter.CancelBondedWarehouseInward();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void UpdateBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (currentSupporter != null)
			{
				if (Env.Security.CustomsBondedWhsUpdate.IsAllowed)
				{
					if (currentSupporter.IsChangeOfRegimeWarehousingEnabled)
					{
						UpdateInventoryChangeOfRegime();
					}
					else if (currentSupporter.IsChangeOfOwnershipBondedWarehousingEnabled)
					{
						UpdateBondedWarehouseChangeOfOwnership();
					}
					else if (currentSupporter.IsOutwardBondedWarehousingEnabled)
					{
						UpdateBondedWarehouseOutward();
					}
					else if (currentSupporter.IsInwardBondedWarehousingEnabled)
					{
						UpdateBondedWarehouseInward();
					}
				}
				else
				{
					Env.Security.ShowError(Env.Security.CustomsBondedWhsUpdate);
				}
			}
		}

		void UpdateBondedWarehouseChangeOfOwnership()
		{
			if (CheckHasChanges(CanUpdateBondedWarehouseChangeOfOwnershipCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				currentSupporter.UpdateBondedWarehouseChangeOfOwnership();
			}
		}

		void UpdateInventoryChangeOfRegime()
		{
			if (CheckHasChanges(CanUpdateInventoryChangeOfRegimeCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				currentSupporter.UpdateInventoryChangeOfRegime();
			}
		}

		void UpdateBondedWarehouseOutward()
		{
			if (CheckHasChanges(CanUpdateBondedWarehouseOutwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				currentSupporter.UpdateBondedWarehouseOutward();
			}
		}

		bool CheckRequiredFieldsForBondedWarehousingAreEntered(bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			var result = true;
			var errorMessages = currentSupporter.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct, checkQuantity, checkEntryDetails);
			if (!errorMessages.IsEmpty)
			{
				result = false;
				currentSupporter.MessageInitiator.NotifyUserOfAnInvalidOperation(errorMessages);
			}
			return result;
		}

		void UpdateBondedWarehouseInward()
		{
			if (CheckHasChanges(CanUpdateBondedWarehouseInwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				currentSupporter.UpdateBondedWarehouseInward();
			}
		}

		bool CanUpdateBondedWarehouseInwardCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanUpdateBondedWarehouseInwardCheck();
		}

		bool CanCancelBondedWarehouseOutwardCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanCancelBondedWarehouseOutwardCheck();
		}

		bool CanCancelUpdateBondedWarehouseInwardCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanCancelUpdateBondedWarehouseInwardCheck();
		}

		bool CanUpdateBondedWarehouseOutwardCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanUpdateBondedWarehouseOutwardCheck();
		}

		bool CanCancelBondedWarehouseChangeOfOwnershipCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanCancelBondedWarehouseChangeOfOwnershipCheck();
		}

		bool CanUpdateBondedWarehouseChangeOfOwnershipCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanUpdateBondedWarehouseChangeOfOwnershipCheck();
		}

		bool CanCancelInventoryChangeOfRegimeCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanCancelInventoryChangeOfRegimeCheck();
		}

		bool CanUpdateInventoryChangeOfRegimeCheck()
		{
			var determiner = creatorSupporter.GetNewBondedWarehouseOperationDeterminer(currentSupporter);
			return determiner != null && determiner.CanUpdateInventoryChangeOfRegimeCheck();
		}

		void BondedWarehouseMenuItem_Popup(object sender, EventArgs e)
		{
			if (currentSupporter != null && currentSupporter.SupportsBondedWarehousing)
			{
				var hasWHSTransaction = Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.HasWHSTransaction(currentSupporter);
				var isInwardBondedWarehousingEnabled = currentSupporter.IsInwardBondedWarehousingEnabled;
				var isOutwardBondedWarehousingEnabled = currentSupporter.IsOutwardBondedWarehousingEnabled;
				updateBondedWarehouseMenuItem.Visible = currentSupporter.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails && (hasWHSTransaction || isOutwardBondedWarehousingEnabled || isInwardBondedWarehousingEnabled);
				var whsStatus = currentSupporter.WarehouseTransactionStatus;
				cancelUpdateBondedWarehouseInwardMenuItem.Visible = hasWHSTransaction && WarehouseTransactionStatusList.IsInwardCode(whsStatus);
				cancelUpdateBondedWarehouseOutwardMenuItem.Visible = hasWHSTransaction && WarehouseTransactionStatusList.IsOutwardCode(whsStatus);
				cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem.Visible = hasWHSTransaction && WarehouseTransactionStatusList.IsChangeOfOwnershipCode(whsStatus);
				cancelUpdateInventoryChangeOfRegimeMenuItem.Visible = hasWHSTransaction && WarehouseTransactionStatusList.IsChangeOfRegimeCode(whsStatus);
				disableBondedWarehouseIntegrationMenuItem.Visible = !currentSupporter.IsBondedWarehousingDisabled && !currentSupporter.HasManualWhsUpdate;
			}
			else
			{
				updateBondedWarehouseMenuItem.Visible = false;
				cancelUpdateBondedWarehouseInwardMenuItem.Visible = false;
				cancelUpdateBondedWarehouseOutwardMenuItem.Visible = false;
				cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem.Visible = false;
				cancelUpdateInventoryChangeOfRegimeMenuItem.Visible = false;
				disableBondedWarehouseIntegrationMenuItem.Visible = false;
			}
		}

		public ZMenuItem ShortCutUpdateBondedWarehouseMenuItem { get; private set; }
		public ZMenuItem BondedWarehouseMenuItem { get; private set; }
		ZMenuItem updateBondedWarehouseMenuItem;
		ZMenuItem cancelUpdateBondedWarehouseInwardMenuItem;
		ZMenuItem cancelUpdateBondedWarehouseOutwardMenuItem;
		readonly ZMenuItem cancelUpdateBondedWarehouseChangeOfOwnershipMenuItem;
		readonly ZMenuItem cancelUpdateInventoryChangeOfRegimeMenuItem;
		ZMenuItem disableBondedWarehouseIntegrationMenuItem;
	}
}
