using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class USInBondHeaderDetailUserControl : ZUserControl
	{
		public USInBondHeaderDetailUserControl(CusInBondHeader header)
		{
			InitializeComponent();
			movementHeadersGridID = this.MovementHeadersGrid.GridId;
			this.inBondHeader = header;
			if (inBondHeader != null)
			{
				HookToSyncronisation(inBondHeader);
				this.MovementHeadersGrid.GridId = movementHeadersGridID + inBondHeader.BH_ImportTransportMode;
			}
			MovementHeadersGrid.SetAvailability(true, [CusInBondMoveHeader.Schema.BM_WarehouseTransactionStatus, CusInBondMoveHeader.Schema.BM_WarehouseTransactionStatusDesc, CusInBondMoveHeader.Schema.WarehouseAddressOrgPK, CusInBondMoveHeader.Schema.BM_OA_WarehouseAddress]);
			AddInBondClosedItem();
			MovementHeadersGrid.ContextMenu.Popup += new EventHandler(ContextMenu_Popup);
		}

		readonly CusInBondHeader inBondHeader;
		readonly ZString movementHeadersGridID;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			if (inBondHeader != null)
			{
				inBondHeader.BH_ImportTransportModeInfo.ValueChanged -= BH_ImportTransportModeInfo_ValueChanged;
			}

			UnhookCurrentMoveHeaderEvents();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			if (CurrentDataItem != null)
			{
				ChangeInBondNumberAllocationButtonVisibility();
				SetInBondNumberVisibility();
				if (inBondHeader != null)
				{
					inBondHeader.BH_ImportTransportModeInfo.ValueChanged += BH_ImportTransportModeInfo_ValueChanged;
				}
				BH_ImportTransportModeInfo_ValueChanged(this, e);
			}
			if (inventorySelectionMenu != null)
			{
				MovementHeadersGrid.ContextMenu.MenuItems.Remove(inventorySelectionMenu);
				inventorySelectionMenu = null;
			}
			if (disableBondedWarehouseIntegrationMenuItem != null)
			{
				MovementHeadersGrid.ContextMenu.MenuItems.Remove(disableBondedWarehouseIntegrationMenuItem);
				disableBondedWarehouseIntegrationMenuItem = null;
			}

			if (this.DataSource != null)
			{
				inventorySelectionMenu = new ZMenuItem("Select Inventory", InventorySelection_Click);
				MovementHeadersGrid.ContextMenu.MenuItems.Add(0, inventorySelectionMenu);
				disableBondedWarehouseIntegrationMenuItem = new ZMenuItem("&Disable Inventory Management Integration", DisableBondedWarehouseIntegrationMenuItem_Click);
				MovementHeadersGrid.ContextMenu.MenuItems.Add(0, disableBondedWarehouseIntegrationMenuItem);
			}
		}

		MenuItem disableBondedWarehouseIntegrationMenuItem;
		MenuItem inventorySelectionMenu;

		void ChangeUNLOCOPortsComponentType(object sender, EventArgs e)
		{
			ChangeUNLOCOPortsComponentVisibility();
		}

		void ChangeUNLOCOPortsComponentVisibility()
		{
			if (currentMoveHeader != null)
			{
				BM_ForeignDestPortKCodeCodeDropEdit.Visible = currentMoveHeader.BM_ForeignDestPortKCodeType == nameof(ZArchitecture.FieldType.TextDropEdit);
				BM_ForeignDestPortKCodeCodeFindBox.Visible = currentMoveHeader.BM_ForeignDestPortKCodeType == nameof(ZArchitecture.FieldType.TextCodeFindBox);
			}
		}

		void DisableBondedWarehouseIntegrationMenuItem_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null)
			{
				if (Env.Security.CustomsBondedWhsDisable.IsAllowed)
				{
					currentMoveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					Globals.Message.ShowInformation(Res.GetString("{8AC4F524-14F9-481C-B399-00DCE220B100}", "Inventory Management Integration has been disabled for movement."));
				}
				else
				{
					Env.Security.ShowError(Env.Security.CustomsBondedWhsDisable);
				}
			}
		}

		void InventorySelection_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new Customs.GUI.InventorySelectionForm(new MoveHeaderInventorySelectionHeader(currentMoveHeader)));
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			RefreshContextMenu();
		}

		protected void AddInBondClosedItem()
		{
			var inBondClosedItem = new ZMenuItem(HandleMarkAsClosedMenuText, new EventHandler(HandleMarkAsClosed));
			var menuItemContainInBomdClosedDate = MovementHeadersGrid.ContextMenu.MenuItems.Contains(inBondClosedItem);
			if (!menuItemContainInBomdClosedDate)
			{
				MovementHeadersGrid.ContextMenu.MenuItems.Add(inBondClosedItem);
			}
		}
		void HandleMarkAsClosed(object sender, EventArgs e)
		{
			if (MovementHeadersGrid.SelectedElements.Length > 0)
			{
				var existHasBeenClosed = false;
				var closeCount = 0;
				foreach (var selectedItem in MovementHeadersGrid.SelectedElements.Cast<CusInBondMoveHeader>())
				{
					if (selectedItem.BM_InBondClosedDate.IsEmpty)
					{
						selectedItem.CloseInBond();
						closeCount++;
					}
					else
					{
						existHasBeenClosed = true;
					}
				}

				string messageText = Res.GetString("784CB7B3-76CE-42DE-8BA4-BB6E9A467C5F", "{0} In-Bond records closed manually.{1}", closeCount, existHasBeenClosed ? " Other In-Bond records have already been closed" : "");
				Globals.Message.Show(messageText, Res.GetString("F7E6141B-F086-4D20-A19E-016FCEEF81E5", "Warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning, DialogResult.OK);
			}
			else
			{
				Globals.Message.Show(Res.GetString("2670629C-C00A-4F13-A0D3-35387A7A6832", "Please select at least one record"), Res.GetString("C0868CEA-2444-4362-B7F8-FCF9366F63AD", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information, DialogResult.OK);
			}
		}

		protected MultilingualString HandleMarkAsClosedMenuText
		{
			get { return ResString.GetMultilingualString("A42FED19-08BD-4F5B-9EAE-4254E18F944D", "Mark as Closed"); }
		}

		void RefreshContextMenu()
		{
			if (inventorySelectionMenu != null || disableBondedWarehouseIntegrationMenuItem != null)
			{
				var enabled = currentMoveHeader != null && !currentMoveHeader.IsDeleted && currentMoveHeader.IsExBondAutomationEnabled;
				if (enabled)
				{
					var header = inBondHeader;
					enabled = header != null && header.CurrentUserHasBondedWarehouseSecurityAccess;
				}
				if (inventorySelectionMenu != null)
				{
					inventorySelectionMenu.Enabled = enabled;
				}

				if (disableBondedWarehouseIntegrationMenuItem != null)
				{
					disableBondedWarehouseIntegrationMenuItem.Enabled = enabled;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (inBondHeader != null)
			{
				inBondHeader.BH_ImportTransportModeInfo.ValueChanged -= BH_ImportTransportModeInfo_ValueChanged;
				inBondHeader.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}

			base.Dispose(disposing);
		}

		void InBondNumberAllocationButton_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null && inBondHeader != null)
			{
				if (currentMoveHeader.HasBeenDeleted)
				{
					Globals.Message.ShowInformation("This In-Bond Movement Header has been deleted by other user while you have the job open. Please close the job, re-open and try again.", "Allocate In-Bond Number");
				}
				else
				{
					ZString message = currentMoveHeader.DisallowAllocateInBondNumber;

					if (!message.IsEmpty)
					{
						Globals.Message.ShowInformation(message, "Allocate In-Bond Number");
					}
					else
					{
						if (!TopLevelBizObjHasChanges || Globals.Message.Show("The In-Bond Job must be saved before an In-Bond Number is allocated.\r\nDo you want to save and proceed?", "Save In-Bond Job", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							var mainForm = FindForm() as ZForm;
							if (mainForm != null)
							{
								var continueWithSave = ContinueWithSave.Yes;

								if (TopLevelBizObjHasChanges)
								{
									continueWithSave = mainForm.FireSaveButton();
								}

								if (continueWithSave == ContinueWithSave.Yes)
								{
									if (currentMoveHeader.LockInBondNumberAllocationMutex())
									{
										var allocateInBondNumber = new US.Business.AllocateInBondNumber(inBondHeader.Branch, inBondHeader.IsPostDepartureMessageOnly);
										using (var form = new US.GUI.AllocateInBondNumberForm(allocateInBondNumber))
										{
											if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
											{
												currentMoveHeader.AllocateInBondNumber(allocateInBondNumber.AI_InBondNumber);

												try
												{
													inBondHeader.Factory.Save();
												}
												catch (Exception exception) when (!exception.IsCriticalException())
												{
													ZExceptionReporting.HandleSaveException(exception);
												}
												finally
												{
													currentMoveHeader.UnLockInBondNumberAllocationMutex();
												}
											}
										}
									}
									else
									{
										Globals.Message.ShowInformation(CusInBondMoveHeader.InBondNumberAllocationMutexLockText(currentMoveHeader.GetInBondNumberAllocationMutexLockInfo()), "Allocate In-Bond Number");
									}
								}
							}
						}
					}
				}
			}
		}

		bool TopLevelBizObjHasChanges
		{
			get { return inBondHeader.Parent != null ? inBondHeader.Parent.HasChanges : inBondHeader.HasChanges; }
		}

		void MovementHeadersGrid_AfterBind(object sender, EventArgs e)
		{
			MovementHeadersGrid.ListManager.PositionChanged += MovementHeadersGridListManager_PositionChanged;
			MovementHeadersGridListManager_PositionChanged(null, null);
		}

		void MovementHeadersGridListManager_PositionChanged(object sender, EventArgs e)
		{
			CurrencyManager listManager = MovementHeadersGrid.ListManager;
			if (listManager != null)
			{
				if (listManager.Count > 0)
				{
					CusInBondMoveHeader current = (CusInBondMoveHeader)listManager.GetCurrent();
					if (current != null)
					{
						bool isDiff = currentMoveHeader != current;
						if (isDiff)
						{
							UnhookCurrentMoveHeaderEvents();
							currentMoveHeader = current;
							HookCurrentMoveHeaderEvents();
							ChangeInBondNumberAllocationButtonVisibility();
						}
					}
				}
				else
				{
					currentMoveHeader = null;
					ChangeInBondNumberAllocationButtonVisibility();
				}
			}
		}

		CusInBondMoveHeader currentMoveHeader;

		void UnhookCurrentMoveHeaderEvents()
		{
			if (currentMoveHeader != null)
			{
				currentMoveHeader.BM_RL_NKForeignDestPortInfo.ValueChanged -= ChangeUNLOCOPortsComponentType;
			}
		}

		void HookCurrentMoveHeaderEvents()
		{
			if (currentMoveHeader != null)
			{
				currentMoveHeader.BM_RL_NKForeignDestPortInfo.ValueChanged += ChangeUNLOCOPortsComponentType;
			}
		}

		void ChangeInBondNumberAllocationButtonVisibility()
		{
			InBondNumberAllocationButton.ReadOnly = currentMoveHeader == null || inBondHeader == null;
			InBondNumberResetButton.ReadOnly = currentMoveHeader == null || inBondHeader == null;
		}

		void SetInBondNumberVisibility()
		{
			this.BH_JobReferenceTextBox.Visible = inBondHeader == null || inBondHeader.JobReferenceVisible;
		}

		void BH_ImportTransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ControlsVisibility();
			var valueChangedEvent = e as ValueChangedEventArgs;
			if (valueChangedEvent != null)
			{
				this.MovementHeadersGrid.GridId = movementHeadersGridID + valueChangedEvent.OldValue;
				this.MovementHeadersGrid.SaveUserLayoutSettings();
			}

			if (valueChangedEvent != null)
			{
				this.MovementHeadersGrid.GridId = movementHeadersGridID + valueChangedEvent.NewValue;
				this.MovementHeadersGrid.LoadUserLayoutSettings();
			}
		}

		void HookToSyncronisation(CusInBondHeader inBondHeader)
		{
			OverrideValuesCheckBox.Visible = !inBondHeader.IsStandAlone;
			inBondHeader.OnOverrideFreightDefaultsChanging += new CancelEventHandler(CusInBondHeader_OnOverrideFreightDefaultsChanging);
		}

		void CusInBondHeader_OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("7499D412-EA4F-4A01-B731-221396F8A59D", "Removing the override will reset your In-Bond entry data.\r\nYou will lose any changes that you have made to the synchronized In-bond data.\r\n\r\nProceed?"), Res.GetString("32c71384-4eb8-4736-a2ce-04a3d49a600b", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			e.Cancel = result == DialogResult.No;
		}

		void ControlsVisibility()
		{
			var isAir = inBondHeader != null && inBondHeader.IsAir;
			var isTruckOrRail = inBondHeader != null && (inBondHeader.IsTruck || inBondHeader.IsRail);

			this.BH_ImportConveyanceNameCodeFindBox.Visible = !isAir && !isTruckOrRail;
			this.BH_VoyageNumberTextBox.Visible = !isAir;

			this.BH_VoyageNumberAirTextBox.Visible = isAir;
			this.AirCarrier3LetterCodeFindBox.Visible = isAir;
			this.MovementHeaderAirCarrier3LetterCodeFindBox.Visible = isAir;
			this.SplitAirCarrier3LetterCodeFindBox.Visible = isAir;

			this.TruckRailImportConveyanceNameTextBox.Visible = isTruckOrRail;
			if (inBondHeader != null)
			{
				if (inBondHeader.IsRail)
				{
					this.TruckRailImportConveyanceNameTextBox.CaptionResourceString = Res.GetData("D574570F-9440-4949-8122-F1EF25CFFDC3", "Conveyance");
				}
				else if (inBondHeader.IsTruck)
				{
					this.TruckRailImportConveyanceNameTextBox.CaptionResourceString = Res.GetData("e11d86b7-93ed-4ac3-8f5c-304224c38c6d", "Truck Reg. No.");
				}
				this.TruckRailImportConveyanceNameTextBox.UpdateCaption();
			}

			if (!isAir)
			{
				this.MovementHeadersGrid.RemoveFromAvailableColumns(CusInBondMoveHeader.Schema.BM_PedimentoNumber);
				this.MovementHeadersGrid.RemoveFromAvailableColumns(CusInBondMoveHeader.Schema.BM_SplitCarrierSCAC);
				this.MovementHeadersGrid.RemoveFromAvailableColumns(CusInBondMoveHeader.Schema.BM_SplitFlightNo);
				this.MovementHeadersGrid.RemoveFromAvailableColumns(CusInBondMoveHeader.Schema.ThreeLetterSplitAirCarrierCode);
				this.SplitFlightTextBox.Visible = false;
				this.SplitCarrierCodeFindBox.Visible = false;
			}
			else
			{
				this.MovementHeadersGrid.AddToAvailableColumns(CusInBondMoveHeaderSchema.BM_PedimentoNumber.Name);
				this.MovementHeadersGrid.AddToAvailableColumns(CusInBondMoveHeader.Schema.BM_SplitCarrierSCAC);
				this.MovementHeadersGrid.AddToAvailableColumns(CusInBondMoveHeader.Schema.ThreeLetterSplitAirCarrierCode);
				this.MovementHeadersGrid.AddToAvailableColumns(CusInBondMoveHeader.Schema.BM_SplitFlightNo);
				this.SplitFlightTextBox.Visible = true;
				this.SplitCarrierCodeFindBox.Visible = true;
			}
		}

		UserResponseArgument ResponseArgument
		{
			get
			{
				if (fResponseArgument == null)
				{
					fResponseArgument = new UserResponseArgument();
					fResponseArgument.MinimumResponseLength = 1;
					fResponseArgument.MaximumResponseLength = StmALogSchema.SL_Reference.MaxLength - CusInBondMoveHeader.ResetInBondNumberMessage.Length - CusInBondMoveHeader.Schema.InBondNumber.Length;
					fResponseArgument.Message = "Please enter the reason for reseting in-bond number.";
					fResponseArgument.Caption = "Warning";
					fResponseArgument.Buttons = ZMessageBoxButtons.OKCancel;
					fResponseArgument.Icon = ZMessageBoxIcon.Asterisk;
					fResponseArgument.DefaultButton = ZMessageBoxDefaultButton.Button2;
				}
				return fResponseArgument;
			}
		}
		UserResponseArgument fResponseArgument;

		void InBondNumberResetButton_Click(object sender, EventArgs e)
		{
			if (currentMoveHeader != null && !currentMoveHeader.InBondNumber.IsEmpty && inBondHeader != null)
			{
				if (Env.Security.USInBondResetToOriginal.IsAllowed)
				{
					if (currentMoveHeader.IsInBondNumberResetable)
					{
						if (!TopLevelBizObjHasChanges || Globals.Message.Show("The In-Bond Job must be saved before an In-Bond Number is reset.\r\nDo you want to save and proceed?", "Save In-Bond Job", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
						{
							var form = FindForm() as ZForm;
							if (form != null)
							{
								var continueWithSave = ContinueWithSave.Yes;

								if (TopLevelBizObjHasChanges)
								{
									continueWithSave = form.FireSaveButton();
								}

								if (continueWithSave == ContinueWithSave.Yes)
								{
									var inBondResetReason = Globals.Message.QueryUserResponse(ResponseArgument);
									if (inBondResetReason.Length > 0)
									{
										currentMoveHeader.ResetInBondNumber(inBondResetReason);
										try
										{
											inBondHeader.Factory.Save();
										}
										catch (Exception exception) when (!exception.IsCriticalException())
										{
											ZExceptionReporting.HandleSaveException(exception);
										}
									}
								}
							}
						}
					}
					else
					{
						Globals.Message.ShowError(CustomsStatusIsAwaitingOrLodged);
					}
				}
				else
				{
					Env.Security.ShowError(Env.Security.USInBondResetToOriginal);
				}
			}
		}
		internal const string CustomsStatusIsAwaitingOrLodged = "This movement is currently waiting or lodged at customs. Its inBond Number cannot be reset.";
	}
}
