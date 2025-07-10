using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
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
	public class EDIMenu : KMenuItem
	{
		public EDIMenu()
		{
			this.Text = Res.GetString("94756165-C001-4f74-A6AD-3A1B929349E1", "&Messaging");
			SetupMenuItems();
		}

		public CusInBondHeader Header
		{
			get { return fHeader; }
			set
			{
				fHeader = value;
				if (fHeader != null && !fHeader.HasMessageInitiator)
				{
					fHeader.MessageInitiator = new Customs.GUI.SendsMessagesToCustomsGUI();
				}
			}
		}
		CusInBondHeader fHeader;

		protected override void Dispose(bool disposing)
		{
			if (Header != null)
			{
				Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
			}

			base.Dispose(disposing);
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			MenuItemsVisibility();
		}

		MenuItem updateBondedWarehouseMenuItem;
		MenuItem cancelBondedWarehouseMenuItem;
		MenuItem bondedWarehouseMenuItem;
		MenuItem disableBondedWarehouseIntegrationMenuItem;
		MenuItem addMessagingMenuItem;
		MenuItem deleteMessagingMenuItem;
		MenuItem deleteInBondMessagingMenuItem;
		MenuItem deleteBillMessagingMenuItem;
		MenuItem amendMessagingMenuItem;
		MenuItem arrivalMessagingMenuItem;
		MenuItem arrivalInbondMessagingMenuItem;
		MenuItem arrivalBillMessagingMenuItem;
		MenuItem arrivalContainerMessagingMenuItem;
		MenuItem exportationMessagingMenuItem;
		MenuItem exportationInbondMessagingMenuItem;
		MenuItem exportationBillMessagingMenuItem;
		MenuItem exportationContainerMessagingMenuItem;
		MenuItem transferOfLiabilityMenuItem;
		MenuItem resetToOriginal;
		MenuItem aCEInBondQuery;
		MenuItem diversionRequestMenuItem;

		void SetupMenuItems()
		{
			addMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("B0014993-0924-4da6-AF45-127E1DF02B96", "Send Departure &Add"), SendDepartureAddMessageClick);
			deleteMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("776AB0C5-CD90-42b4-BE3B-FFC016705F07", "Send Departure &Delete"));
			deleteInBondMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("FD7938E6-CEAF-42F9-A506-756695AB712A", "Send Departure Delete (Entire In-Bond)"), SendDepartureDeleteInBondMessageClick);
			deleteBillMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("FF74B050-6222-4758-B292-372E4690E974", "Send Departure Delete (Bill of Lading)"), SendDepartureDeleteBillOfLadingMessageClick);
			amendMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("1E20331F-A8C8-4DA5-B8C5-7BF3D49A277A", "Send Departure &Amend (Delete/Re-Add)"), SendDepartureAmendMessageClick);
			deleteMessagingMenuItem.MenuItems.Add(deleteInBondMessagingMenuItem);
			deleteMessagingMenuItem.MenuItems.Add(deleteBillMessagingMenuItem);

			arrivalMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("E3B053DF-B4AD-43e8-98EF-1F318A997212", "Send Arri&val"));
			arrivalInbondMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("dbb3e745-d47a-4172-93f8-52e3ecd7d110", "Send Arri&val (Entire In-Bond)"), SendInBondLevelArrivalMessageClick);
			arrivalBillMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("a30ddc8a-434d-4906-a3d5-2fe36d2518dc", "Send Arri&val (Bill of Lading)"), SendBillLevelArrivalMessageClick);
			arrivalContainerMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("e0c35149-8f4b-4bd1-83e9-94c6951c331f", "Send Arri&val (Container/Equipment)"), SendContainerLevelArrivalMessageClick);
			arrivalMessagingMenuItem.MenuItems.Add(arrivalInbondMessagingMenuItem);
			arrivalMessagingMenuItem.MenuItems.Add(arrivalBillMessagingMenuItem);
			arrivalMessagingMenuItem.MenuItems.Add(arrivalContainerMessagingMenuItem);

			exportationMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("0D432589-A9B1-4c24-B724-79E193D1D1A2", "Send E&xportation"));
			exportationInbondMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("517a5d86-af77-4cb5-b65e-f2950c2d864c", "Send E&xportation (Entire In-Bond)"), SendInBondLevelExportationMessageClick);
			exportationBillMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("f8c05236-1762-4b3a-b422-eeaaa82be414", "Send E&xportation (Bill of Lading)"), SendBillLevelExportationMessageClick);
			exportationContainerMessagingMenuItem = new ZMenuItem(ResString.GetMultilingualString("16a1c3c0-e1bd-471d-8d73-5587c7ce3c9f", "Send E&xportation (Container/Equipment)"), SendContainerLevelExportationMessageClick);
			exportationMessagingMenuItem.MenuItems.Add(exportationInbondMessagingMenuItem);
			exportationMessagingMenuItem.MenuItems.Add(exportationBillMessagingMenuItem);
			exportationMessagingMenuItem.MenuItems.Add(exportationContainerMessagingMenuItem);

			transferOfLiabilityMenuItem = new ZMenuItem(ResString.GetMultilingualString("01D16CA4-4586-46d3-9A50-B56EFF40998D", "Send &Transfer Of Liability"), SendInBondLevelTransferOfLiabilityMessageClick);
			resetToOriginal = new ZMenuItem(ResString.GetMultilingualString("EBE5E50A-4B9B-46B8-8321-43724E7D9C12", "Reset to Original"), SendInBondLevelResetToOriginalMessageClick);
			aCEInBondQuery = new ZMenuItem(ResString.GetMultilingualString("8FBC3347-F254-4389-ABB4-8A71E189D07E", "Query Cargo/Manifest Status"), SendACEInBondQueryClick);

			updateBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("77E43865-0A57-43AD-A0F5-CF5F7D695F0E", "&Update Inventory"));
			updateBondedWarehouseMenuItem.Click += UpdateBondedWarehouseMenuItem_Click;

			cancelBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("52A77B69-AE95-4CD2-91D3-973895E3138D", "&Cancel Inventory Stock Release"));
			cancelBondedWarehouseMenuItem.Click += CancelBondedWarehouseMenuItem_Click;

			disableBondedWarehouseIntegrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("{C777B337-6F37-4E3E-86D0-B94F1F82FA21}", "Disable &Integration"));
			disableBondedWarehouseIntegrationMenuItem.Click += DisableBondedWarehouseIntegrationMenuItem_Click;

			bondedWarehouseMenuItem = new ZMenuItem(Res.GetString("6ED8E567-0140-4466-B9C9-6766BED12D89", "Inventory Management"), new MenuItem[] { updateBondedWarehouseMenuItem, cancelBondedWarehouseMenuItem, disableBondedWarehouseIntegrationMenuItem });
			bondedWarehouseMenuItem.Popup += BondedWarehouseMenuItem_Popup;
			MenuItems.Add(bondedWarehouseMenuItem);

			diversionRequestMenuItem = new ZMenuItem(ResString.GetMultilingualString("E98237C8-8FFC-40B1-8ADE-EB08F2583D4A", "Send &Diversion Request"));
			diversionRequestMenuItem.Click += DiversionRequestMenuItem_Click;

			this.MenuItems.Add("-");
			this.MenuItems.Add(addMessagingMenuItem);
			this.MenuItems.Add(deleteMessagingMenuItem);
			this.MenuItems.Add(amendMessagingMenuItem);
			this.MenuItems.Add("-");
			this.MenuItems.Add(arrivalMessagingMenuItem);
			this.MenuItems.Add(exportationMessagingMenuItem);
			this.MenuItems.Add(transferOfLiabilityMenuItem);
			this.MenuItems.Add(diversionRequestMenuItem);
			this.MenuItems.Add("-");
			this.MenuItems.Add(aCEInBondQuery);
			this.MenuItems.Add("-");
			this.MenuItems.Add(resetToOriginal);
		}

		void BondedWarehouseMenuItem_Popup(object sender, EventArgs e)
		{
			cancelBondedWarehouseMenuItem.Visible = Header != null && Header.HasAtLeastOneMovementWithWHSTransaction;
		}

		void DisableBondedWarehouseIntegrationMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsDisable.IsAllowed)
			{
				if (CheckHasChanges())
				{
					foreach (var moveHeader in Header.MovementHeaders.OfType<CusInBondMoveHeader>().Where(x => !x.IsBondedWarehousingDisabled))
					{
						moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					}
					if (MainForm.FireSaveButton() == ContinueWithSave.Yes)
					{
						Globals.Message.ShowInformation(Res.GetString("{23E73AB1-AD1A-45F7-99F3-97FD107D13C0}", "Inventory Management Integration has been disabled."));
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsDisable);
			}
		}

		void CancelBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				SendWHSTransaction(InBondMessageType.BondedWarehouseCancel);
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void UpdateBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsUpdate.IsAllowed)
			{
				SendWHSTransaction(InBondMessageType.BondedWarehouseUpdate);
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsUpdate);
			}
		}

		void SendWHSTransaction(InBondMessageType messageType)
		{
			if (CheckHasChanges())
			{
				var sendingHeader = new InBondMessageSendingHeaderObject(Header.PK, messageType, Header.MessageInitiator);
				if (sendingHeader.SendingObjects.Count == 1)
				{
					try
					{
						sendingHeader.SendingObjects[0].US_ShouldSend = true;
						sendingHeader.SendData();
					}
					finally
					{
						sendingHeader.Header.MovementHeaders.UnLockAllInBondNumberAllocationMutex();
					}
				}
				else
				{
					ZFormModaliser.Show(new MessagingForm(sendingHeader), MainForm);
				}
				Header.RefreshBindingIncludingChildren();
			}
		}

		protected BusinessObject TopLevelBusinessObject
		{
			get { return (BusinessObject)Header.Parent ?? Header; }
		}

		public static string SaveDataFirstMessage
		{
			get { return ResString.GetMultilingualString("F2076ADC-7DE0-4b40-AD00-A9E3D643D41E", "The data has not yet been saved. Do you want to save and proceed?"); }
		}

		protected bool CheckHasChanges()
		{
			bool okToContinue = true;
			var topLevelBusinessObject = TopLevelBusinessObject;
			var shouldSaveData = !topLevelBusinessObject.IsInDatabase || topLevelBusinessObject.HasChanges;
			if (shouldSaveData && Globals.Message.Show(SaveDataFirstMessage, Res.GetString("2DC8F743-40AB-4832-8237-4A36A47D4388", "Save Data"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
			{
				okToContinue = false;
			}

			if (okToContinue)
			{
				if (shouldSaveData)
				{
					okToContinue = okToContinue && MainForm.FireSaveButton() == ContinueWithSave.Yes;
				}
				else
				{
					topLevelBusinessObject.RunPreSaveValidation();
					if (topLevelBusinessObject.HasErrors)
					{
						okToContinue = false;
						Globals.Message.ShowError(Res.GetString("528099BF-3C11-4878-AC9E-49C34ACF6D59", "Unable to proceed due to some critical errors; please fix all errors before trying again."));
						MainForm.Refresh();
					}
				}
			}

			return okToContinue;
		}

		void MenuItemsVisibility()
		{
			var isAir = Header != null && Header.IsAir;
			if (isAir)
			{
				addMessagingMenuItem.Click -= new EventHandler(SendDepartureAddMessageClick);
				deleteInBondMessagingMenuItem.Click -= new EventHandler(SendDepartureDeleteInBondMessageClick);
				deleteBillMessagingMenuItem.Click -= new EventHandler(SendDepartureDeleteBillOfLadingMessageClick);
				amendMessagingMenuItem.Click -= new EventHandler(SendDepartureAmendMessageClick);
				arrivalInbondMessagingMenuItem.Click -= new EventHandler(SendInBondLevelArrivalMessageClick);
				exportationInbondMessagingMenuItem.Click -= new EventHandler(SendInBondLevelExportationMessageClick);

				addMessagingMenuItem.Click -= new EventHandler(SendAirInitiationMessageClick);
				addMessagingMenuItem.Click += new EventHandler(SendAirInitiationMessageClick);
				deleteInBondMessagingMenuItem.Click -= new EventHandler(SendAirInBondDeletionMessageClick);
				deleteInBondMessagingMenuItem.Click += new EventHandler(SendAirInBondDeletionMessageClick);
				deleteBillMessagingMenuItem.Click -= new EventHandler(SendAirBillOfLadingDeletionMessageClick);
				deleteBillMessagingMenuItem.Click += new EventHandler(SendAirBillOfLadingDeletionMessageClick);
				amendMessagingMenuItem.Click -= new EventHandler(SendingAirAmendMessageClick);
				amendMessagingMenuItem.Click += new EventHandler(SendingAirAmendMessageClick);
				arrivalInbondMessagingMenuItem.Click -= new EventHandler(SendEntireInBondArrivalMessageClick);
				arrivalInbondMessagingMenuItem.Click += new EventHandler(SendEntireInBondArrivalMessageClick);
				exportationInbondMessagingMenuItem.Click -= new EventHandler(SendEntireInBondExportationMessageClick);
				exportationInbondMessagingMenuItem.Click += new EventHandler(SendEntireInBondExportationMessageClick);
			}
			else
			{
				addMessagingMenuItem.Click -= new EventHandler(SendDepartureAddMessageClick);
				addMessagingMenuItem.Click += new EventHandler(SendDepartureAddMessageClick);
				deleteInBondMessagingMenuItem.Click -= new EventHandler(SendDepartureDeleteInBondMessageClick);
				deleteInBondMessagingMenuItem.Click += new EventHandler(SendDepartureDeleteInBondMessageClick);
				deleteBillMessagingMenuItem.Click -= new EventHandler(SendDepartureDeleteBillOfLadingMessageClick);
				deleteBillMessagingMenuItem.Click += new EventHandler(SendDepartureDeleteBillOfLadingMessageClick);
				amendMessagingMenuItem.Click -= new EventHandler(SendDepartureAmendMessageClick);
				amendMessagingMenuItem.Click += new EventHandler(SendDepartureAmendMessageClick);
				arrivalInbondMessagingMenuItem.Click -= new EventHandler(SendInBondLevelArrivalMessageClick);
				arrivalInbondMessagingMenuItem.Click += new EventHandler(SendInBondLevelArrivalMessageClick);
				exportationInbondMessagingMenuItem.Click -= new EventHandler(SendInBondLevelExportationMessageClick);
				exportationInbondMessagingMenuItem.Click += new EventHandler(SendInBondLevelExportationMessageClick);

				addMessagingMenuItem.Click -= new EventHandler(SendAirInitiationMessageClick);
				deleteInBondMessagingMenuItem.Click -= new EventHandler(SendAirInBondDeletionMessageClick);
				deleteBillMessagingMenuItem.Click -= new EventHandler(SendAirBillOfLadingDeletionMessageClick);
				amendMessagingMenuItem.Click -= new EventHandler(SendingAirAmendMessageClick);
				arrivalInbondMessagingMenuItem.Click -= new EventHandler(SendEntireInBondArrivalMessageClick);
				exportationInbondMessagingMenuItem.Click -= new EventHandler(SendEntireInBondExportationMessageClick);
			}
			diversionRequestMenuItem.Click -= new EventHandler(DiversionRequestMenuItem_Click);
			diversionRequestMenuItem.Click += new EventHandler(DiversionRequestMenuItem_Click);

			transferOfLiabilityMenuItem.Visible = !isAir;
			arrivalContainerMessagingMenuItem.Visible = !isAir;
			exportationContainerMessagingMenuItem.Visible = !isAir;
			bondedWarehouseMenuItem.Visible = Header != null && Header.HasAtLeastOneMovementMarkedForBondedWarhousing;
			addMessagingMenuItem.Enabled = Header != null && !Header.IsPostDepartureMessageOnly;
			deleteMessagingMenuItem.Enabled = Header != null && !Header.IsPostDepartureMessageOnly;
			amendMessagingMenuItem.Enabled = Header != null && !Header.IsPostDepartureMessageOnly;
		}

		void DiversionRequestMenuItem_Click(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.DiversionRequest);
		}

		void SendInBondLevelArrivalMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.InBondLevelArrival);
		}

		void SendBillLevelArrivalMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.BillOfLadingLevelArrival);
		}

		void SendContainerLevelArrivalMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.ContainerLevelArrival);
		}

		void SendInBondLevelExportationMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.InBondLevelExportation);
		}

		void SendBillLevelExportationMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.BillOfLadingLevelExportation);
		}

		void SendContainerLevelExportationMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.ContainerLevelExportation);
		}

		void SendInBondLevelTransferOfLiabilityMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.InBondLevelTransferOfLiability);
		}

		void SendInBondLevelResetToOriginalMessageClick(object sender, EventArgs e)
		{
			if (Env.Security.USInBondResetToOriginal.IsAllowed)
			{
				var continueWithSave = ContinueWithSave.Yes;
				if (MainForm.BusinessEntityForHasChanges.HasChanges && Globals.Message.Show("There are changes in this form. Do you want to save and proceed?", "Save", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					continueWithSave = MainForm.FireSaveButton();
				}
				if (!MainForm.BusinessEntityForHasChanges.HasChanges && continueWithSave == ContinueWithSave.Yes)
				{
					var movementHeadersForResetting = new USMovementHeaderResetCollection(Header);
					if (movementHeadersForResetting.Count > 0)
					{
						if (ZFormModaliser.ShowDialogAndDispose(new MovementHeaderResetToOriginalForm(movementHeadersForResetting)) == DialogResult.OK)
						{
							try
							{
								Header.Factory.Save();
								Globals.Message.Show("Successfully reset.");
							}
							catch (ZSaveException e1)
							{
								ZExceptionReporting.HandleSaveException(e1);
							}
						}
					}
					else
					{
						Globals.Message.Show("There are no Movement Headers that need to be reset to original. No movements have been lodged at Customs or are waiting for responses.");
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.USInBondResetToOriginal);
			}
		}

		void SendDepartureAddMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.DepartureAdd);
		}

		void SendDepartureDeleteInBondMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.DepartureDelete);
		}

		void SendDepartureDeleteBillOfLadingMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.DepartureBillDelete);
		}

		void SendDepartureAmendMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.DepartureAmend);
		}

		#region Air InBond

		void SendAirInitiationMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirInBondAdd);
		}

		void SendAirInBondDeletionMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirInBondDelete);
		}

		void SendAirBillOfLadingDeletionMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirBillDelete);
		}

		void SendingAirAmendMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirInBondAmend);
		}

		void SendEntireInBondArrivalMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirEntireInBondArrival);
		}

		void SendEntireInBondExportationMessageClick(object sender, EventArgs e)
		{
			SendMessage(InBondMessageType.AirEntireInBondExportation);
		}

		#endregion

		void SendACEInBondQueryClick(object sender, EventArgs e)
		{
			if (Header == null)
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("ACFD62DC-293F-46E2-89D2-A00B91F0A2D1", "An In-Bond Movement has not yet been created on this shipment.\r\nNo messages are therefore available to be sent at this time."), ResString.GetMultilingualString("FAE470C4-3362-4ffb-B974-3FFF49C5108B", "IN-BOND MOVEMENT MESSAGING"));
			}
			else if (Header.IsDocumentOnly)
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("59C03E09-8B04-4658-87CA-3C6C28FB3916", "Messages can not be sent in Document Only mode."));
			}
			else if (IsMessagingAllowed() && CheckHasChanges())
			{
				var sendingHeader = new US.Business.CargoManifestStatusQueryHeaderObject(Header);
				if (sendingHeader.SendingObjects.Count > 0)
				{
					cargoManifestStatusQueryForm = new US.GUI.CargoManifestStatusQueryActionForm(sendingHeader);

					ZFormModaliser.ShowDialogAndDispose(cargoManifestStatusQueryForm);

					if (sendingHeader.ShouldSendMessage)
					{
						var messagesCount = sendingHeader.SendQueryMessage();
						if (messagesCount > 0)
						{
							try
							{
								Header.Factory.Save();
								Globals.Message.ShowInformation(ResString.GetMultilingualString("E323DAFA-82F0-4790-B6E0-7B1728E5CD8C", "Cargo Manifest Status Query sent"));
							}
							catch (ZSaveException e1)
							{
								ZExceptionReporting.HandleSaveException(e1);
							}
						}
					}
				}
				else
				{
					Globals.Message.ShowInformation(ResString.GetMultilingualString("008C539F-8E5A-47C3-B9E0-DD431E4A72AC", "There is no In-Bond Movement/Bills available for sending ACE Cargo/Manifest query message to Customs."), ResString.GetMultilingualString("0E5061D2-F05D-45EF-B10A-EE6E0DAD94C5", "NO IN-BOND MOVEMENT/BILLS"));
				}
			}
		}
		US.GUI.CargoManifestStatusQueryActionForm cargoManifestStatusQueryForm;

		bool IsMessagingAllowed()
		{
			bool result = true;
			if (!Env.Security.USInBondMessaging.IsAllowed)
			{
				Env.Security.ShowError(Env.Security.USInBondMessaging);
				result = false;
			}

			return result;
		}

		ZForm MainForm
		{
			get { return (ZForm)GetMainMenu().GetForm(); }
		}

		void SendMessage(InBondMessageType messageType)
		{
			if (Header == null)
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("ACFD62DC-293F-46E2-89D2-A00B91F0A2D1", "An In-Bond Movement has not yet been created on this shipment.\r\nNo messages are therefore available to be sent at this time."), ResString.GetMultilingualString("FAE470C4-3362-4ffb-B974-3FFF49C5108B", "IN-BOND MOVEMENT MESSAGING"));
			}
			else if (Header.IsDocumentOnly)
			{
				Globals.Message.ShowInformation(ResString.GetMultilingualString("59C03E09-8B04-4658-87CA-3C6C28FB3916", "Messages can not be sent in Document Only mode."));
			}
			else if (Header.IsSendCustomsMessageMutexLocked)
			{
				Globals.Message.ShowInformation(Header.CannotSendCustomsMessageWhenMutexIsLocked());
			}
			else if (IsMessagingAllowed() && CheckHasChanges())
			{
				var sendingObject = new InBondMessageSendingHeaderObject(Header.PK, messageType, Header.MessageInitiator);
				if (sendingObject.SendingObjects.Count > 0)
				{
					if (Header.LockSendCustomsMessageMutex())
					{
						try
						{
							ZFormModaliser.ShowDialogAndDispose(new MessagingForm(sendingObject), MainForm);
						}
						finally
						{
							Header.UnlockSendCustomsMessageMutex();
						}
					}
				}
				else
				{
					ShowNoDataToSend(messageType);
				}
			}
		}

		void ShowNoDataToSend(InBondMessageType messageType)
		{
			switch (messageType)
			{
				case InBondMessageType.AirInBondAdd:
				case InBondMessageType.DepartureAdd:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("37BEB192-0C14-45d7-A773-2C16F5797383", "There is no In-Bond Movement available for sending a 'Departure Add' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or have already been accepted by Customs."), ResString.GetMultilingualString("bed32b49-bf0b-4cc0-b254-c57f747bf842", "NO IN-BOND MOVEMENT FOR DEPARTURE ADD MESSAGING"));
					break;
				case InBondMessageType.AirInBondDelete:
				case InBondMessageType.DepartureDelete:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("D54068BC-1D66-4917-A210-41F7F6E2EA41", "There is no In-Bond Movement available for sending a 'Departure Delete' message to Customs.\r\nPossibly all In-Bond Movements are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number."), ResString.GetMultilingualString("687009AC-9C3A-4ecf-93F6-4BD9ADD6532E", "NO IN-BOND MOVEMENT FOR DEPARTURE DELETE MESSAGING"));
					break;
				case InBondMessageType.AirBillDelete:
				case InBondMessageType.DepartureBillDelete:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("B1DC8C2B-DAE7-4E3C-89D1-F9E147CDBEFC", "There is no Bill of Lading available for sending a 'Departure Delete' message to Customs.\r\nPossibly all Bill are awaiting Customs response, or they have been already accepted by Customs, or they have been created in a legacy system, but they have not been allocated the original In-Bond Number."), ResString.GetMultilingualString("7275C85B-01C2-4FD3-85EB-B1B980A22B0C", "NO BILL OF LADING FOR DEPARTURE DELETE MESSAGING"));
					break;
				case InBondMessageType.AirInBondAmend:
				case InBondMessageType.DepartureAmend:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("A127D0CC-B4C7-476E-8B4A-1EF1EF2A688A", "Departure Amend (Delete/Re-add) message(s) cannot be sent to Customs.\r\nAt least one In-Bond should have a cleared status."), ResString.GetMultilingualString("b2afdc8d-a7b5-446b-aeac-4f4209f523de", "NO IN-BOND MOVEMENT FOR DEPARTURE AMEND MESSAGING"));
					break;
				case InBondMessageType.AirEntireInBondArrival:
				case InBondMessageType.InBondLevelArrival:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("67C723FE-B9BF-4edb-8984-CA6F1A0C78C9", "There is no In-Bond Movement available for sending a 'Arrival' message to Customs.\r\nPossibly all In-Bond Movements are pending Customs response."), ResString.GetMultilingualString("5CC2D6B4-45CF-4792-ACC3-139ED148DFD7", "NO IN-BOND MOVEMENT FOR ARRIVAL MESSAGING"));
					break;

				case InBondMessageType.BillOfLadingLevelArrival:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("ad69a623-4fc1-43e4-8534-698977d18361", "There is no Bill of Lading available for sending a 'Arrival' message to Customs.\r\nPossibly all Bill of Ladings are pending Customs response or multiple In-bonds exist against the Bill."), ResString.GetMultilingualString("5ac67833-1a09-4cfc-ab7a-00616700df54", "NO BILL OF LADING FOR ARRIVAL MESSAGING"));
					break;
				case InBondMessageType.ContainerLevelArrival:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("eb816414-77fc-4886-a8ae-41f3c8a2f342", "There is no Container available for sending a 'Arrival' message to Customs.\r\nPossibly all Containers are either pending Customs response or Number being 'NC' or multiple In-bonds exist against the Bill."), ResString.GetMultilingualString("edbbf260-5b17-45f6-9023-55fa69306441", "NO CONTAINER FOR ARRIVAL MESSAGING"));
					break;

				case InBondMessageType.AirEntireInBondExportation:
				case InBondMessageType.InBondLevelExportation:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("2117DE0D-1D94-4e50-A4BF-60046FE280E8", "There is no In-Bond Movement available for sending a 'Exportation' message to Customs.\r\nPossibly all In-Bond Movements are either pending Customs response or do not have Entry Type '62' or '63'."), ResString.GetMultilingualString("9E466A11-2C55-4670-AB70-917E1B600913", "NO IN-BOND MOVEMENT FOR EXPORTATION MESSAGING"));
					break;

				case InBondMessageType.BillOfLadingLevelExportation:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("05268096-41f6-480d-a5c7-d4600126ff85", "There is no Bill of Lading available for sending a 'Exportation' message to Customs.\r\nPossibly all Bill of Ladings are pending Customs response or multiple In-bonds exist against the Bill."), ResString.GetMultilingualString("02dd7edb-04e4-49a6-97fe-02d7a3941ea2", "NO BILL OF LADING FOR EXPORTATION MESSAGING"));
					break;
				case InBondMessageType.ContainerLevelExportation:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("c2ef9b01-294c-4a32-9b1e-4b7d96cf674e", "There is no Container available for sending a 'Exportation' message to Customs.\r\nPossibly all Containers are either pending Customs response or Number being 'NC' or multiple In-bonds exist against the Bill."), ResString.GetMultilingualString("8ae03bc6-4654-475f-ae77-381c5e106078", "NO CONTAINER FOR EXPORTATION MESSAGING"));
					break;

				case InBondMessageType.InBondLevelTransferOfLiability:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("1653744B-2EE8-4be5-B3AA-467716889595", "There is no In-Bond Movement available for sending a 'Transfer of Liability' message to Customs.\r\nPossibly all In-Bond Movements are pending Customs response."), ResString.GetMultilingualString("25BD8077-C96E-4fe2-BDD9-0D9DB978B857", "NO IN-BOND MOVEMENT FOR TRANSFER OF LIABILITY MESSAGING"));
					break;
				case InBondMessageType.DiversionRequest:
					Globals.Message.ShowInformation(ResString.GetMultilingualString("66D93F82-E020-4580-B3DF-D056F5C2855F", "There is no In-Bond Movement available for sending a 'Diversion request' message to Customs.\r\nAt least one In-Bond Movement have already been accepted by Customs."), ResString.GetMultilingualString("22A16DCB-4868-4502-B2D4-A71E6534DCA7", "NO IN-BOND MOVEMENT FOR DIVERSION REQUEST MESSAGING"));
					break;
			}
		}

		internal ImportMessageStatusList StatusList
		{
			get { return statusList ?? (statusList = Header.Factory.GetCachedValue<ImportMessageStatusList>()); }
		}
		ImportMessageStatusList statusList;
	}
}
