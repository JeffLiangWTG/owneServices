using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GUI.WarehouseExtensions;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public class EDIMenu : CommercialInvoiceEDIMenu, IEDIMenu, IBondedWarehouseMenuItemsCreatorSupporter
	{
		public MenuItem GenerateEntriesMenuItem;
		protected MenuItem apportionmentMenuItem;

		internal List<MenuItem> BOMMenuItem;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public EDIMenu()
		{
			TypeDescriptor.AddAttributes(this, new SuppressFormsLocalizedTestAttribute());

			submitMenuItem = new ZMenuItem(ResString.GetMultilingualString("6D1D42B1-5CBC-46DF-9194-7A155405D606", "Submit"), SubmitMenu_Click);
			submitMenuItem.Visible = Declaration != null && Declaration.ShowSubmitMenuItem;

			attachCommercialInvoiceMenuItem = new ZMenuItem(ResString.GetMultilingualString("6eda95dd-72f6-4701-bc00-5be760d85aa3", "&Attach Commercial Invoices"), new EventHandler(AttachMenu_Click));
			attachCommercialInvoiceMenuItem.Visible = Declaration != null && Declaration.EnableAttachCommercialInvoice;

			copyCommercialInvoiceMenuItem = new ZMenuItem(ResString.GetMultilingualString("401e044c-3d2d-40a9-bd03-07d172e2a4ae", "&Copy Commercial Invoices"), new EventHandler(CopyMenu_Click));
			copyCommercialInvoiceMenuItem.Visible = Declaration != null && Declaration.EnableCopyCommercialInvoice;

			commercialInvoiceMenuItem = new ZMenuItem(ResString.GetMultilingualString("182ae3e8-518e-4189-a60d-17721e32194f", "Commercial &Invoices"), new MenuItem[] { attachCommercialInvoiceMenuItem, copyCommercialInvoiceMenuItem });
			commercialInvoiceMenuItem.Visible = attachCommercialInvoiceMenuItem.Visible || copyCommercialInvoiceMenuItem.Visible;

			importCSVMenuItem = new ZMenuItem(ResString.GetMultilingualString("872fb4f6-6af0-419f-8e64-add5fd1f5a03", "Import Invoices"), ImportInvoicesClick);
			importCSVMenuItem.Visible = Declaration != null && Declaration.EnableImportInvoices;
			MenuItem importOrderLinesMenuItem = new ZMenuItem(ResString.GetMultilingualString("b2e7477a-5138-480e-b2f2-6f1610c3bfd1", "Import Order Lines"), ImportOrderLinesClick);

			updateBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("36422415-55e1-47bf-af1b-27253bb6d2e4", "Update Inventory"));
			updateBondedWarehouseMenuItem.Click += UpdateBondedWarehouseMenuItem_Click;
			updateBondedWarehouseMenuItem.Name = nameof(updateBondedWarehouseMenuItem);

			cancelUpdateBondedWarehouseInwardMenuItem = new ZMenuItem(ResString.GetMultilingualString("ab774be4-a2ce-4aff-8372-9708dab2500d", "Cancel Inventory"));
			cancelUpdateBondedWarehouseInwardMenuItem.Click += CancelUpdateBondedWarehouseInwardMenuItem_Click;
			cancelUpdateBondedWarehouseInwardMenuItem.Name = nameof(cancelUpdateBondedWarehouseInwardMenuItem);

			synchronizeWithBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("bd3c16f0-f14a-4331-83f6-7b95d7bd25c8", "&Synchronize with Inventory"));
			synchronizeWithBondedWarehouseMenuItem.Click += SynchronizeWithBondedWarehouseMenuItem_Click;

			inventoriesSelectionFromBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("1C6D71BA-1EEF-491E-8D36-D7C706E80C94", "S&elect Inventory"));
			inventoriesSelectionFromBondedWarehouseMenuItem.Click += InventoriesSelectionFromBondedWarehouseMenuItem_Click;

			orderLinesSelectionFromBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("14B33396-A803-417C-B1BA-7C7019AD6019", "Select &Order Lines"));
			orderLinesSelectionFromBondedWarehouseMenuItem.Click += OrderLinesSelectionFromBondedWarehouseMenuItem_Click;

			disableBondedWarehouseIntegrationMenuItem = new ZMenuItem(ResString.GetMultilingualString("{60E002A1-E183-4F30-9252-58EA3A4A0617}", "&Disable Integration"));
			disableBondedWarehouseIntegrationMenuItem.Click += DisableBondedWarehouseIntegrationMenuItem_Click;

			cancelBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("92122b4b-fd0b-4cf5-9eda-09e32acd956e", "&Cancel Inventory Stock Release"));
			cancelBondedWarehouseMenuItem.Click += CancelBondedWarehouseMenuItem_Click;
			cancelBondedWarehouseMenuItem.Name = nameof(cancelBondedWarehouseMenuItem);

			synchronizeWithOrdersMenuItem = new ZMenuItem(ResString.GetMultilingualString("1E23E297-F403-46DE-82DA-F5542213EE2D", "Synchronize With Orders"));
			synchronizeWithOrdersMenuItem.Click += SynchronizeWithOrdersMenuItem_Click;

			finaliseStockWithBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("5380f4e1-b484-4e1a-866d-7f8f196862ba", "&Finalize Stock with Inventory"));
			finaliseStockWithBondedWarehouseMenuItem.Click += FinaliseStockWithBondedWarehouseMenuItem_Click;

			MenuItem exportDeclarationToXml = new ZMenuItem(ResString.GetMultilingualString("ac122592-9697-4cc7-b414-f788f03d716b", "Export Declaration to XML"));
			exportDeclarationToXml.Click += ExportDeclarationXmlClick;

			bondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("8065c0aa-8156-4ccd-bf34-dd6907a2db06", "Inventory Management"), new MenuItem[] { inventoriesSelectionFromBondedWarehouseMenuItem, synchronizeWithBondedWarehouseMenuItem, updateBondedWarehouseMenuItem, cancelUpdateBondedWarehouseInwardMenuItem, finaliseStockWithBondedWarehouseMenuItem, cancelBondedWarehouseMenuItem, disableBondedWarehouseIntegrationMenuItem, synchronizeWithOrdersMenuItem, orderLinesSelectionFromBondedWarehouseMenuItem });
			bondedWarehouseMenuItem.Popup += BondedWarehouseMenuItem_Popup;
			bondedWarehouseMenuItem.Name = nameof(bondedWarehouseMenuItem);

			createProductFilesMenuItem = new ZMenuItem(ResString.GetMultilingualString("13173201-2ff5-42c6-aee9-5ad2f705b08c", "Create Product Files"));
			createProductFilesMenuItem.Click += CreateProductFilesMenuItem_Click;

			refreshProductDataMenuItem = new ZMenuItem(ResString.GetMultilingualString("18119B27-F0F1-45DC-AEAB-A99A6D883889", "Refresh Product Data"));
			refreshProductDataMenuItem.Click += RefreshProductDataMenuItem_Click;

			dataMenuItem = new ZMenuItem(ResString.GetMultilingualString("7189c3da-d5a5-4eae-ad57-ed0b21d66aa8", "Data"), new MenuItem[] { importCSVMenuItem, importOrderLinesMenuItem, exportDeclarationToXml });

			GenerateEntriesMenuItem = new ZMenuItem(GenerateEntriesMenuOptionText, PerformGenerateEntriesClick);

			apportionmentMenuItem = new ZMenuItem(ResString.GetMultilingualString("59b86615-a6d6-428c-832c-1a675ce7013c", "Perform Apportionment"), PerformApportionmentClick);

			BOMMenuItem = new List<MenuItem>();
			BOMMenuItem.Add(new ZMenuItem(ResString.GetMultilingualString("e620ed2a-5502-4256-be5b-02c771da1660", "Expand ALL Lines by their Bills Of Materials"), DoExpandAllBOMLinesMenuItem_Click));
			BOMMenuItem.Add(new ZMenuItem(ResString.GetMultilingualString("0cf3e5ec-53db-4566-881c-3fca8d5b1893", "Collapse Bills Of Materials for ALL Lines (Remove Expanded Lines)"), DoCollapseAllBOMLinesMenuItem_Click));

			sendToGlobalManifestMenuItem = new ZMenuItem(ResString.GetMultilingualString("{DE1D8E05-792F-4964-AC90-9A0026D9CFCE}", "Send to Global Manifest"));
			sendToGlobalManifestMenuItem.Click += SendToGlobalManifestMenuItem_Click;

			packingListMenuItem = new ZMenuItem(GetPackingListMenuItemCaption(), CreatePackingListMenuItem_Click);
			amendmentSnapshotManagementMenuItemsCreator = CreateNewAmendmentSnapshotManagementMenuItemsCreator();
			LoadMenuItems();
			AddImportNCTSLinesMenuChild();
		}

		public new BaseJobDeclaration Declaration
		{
			get => (BaseJobDeclaration)base.Declaration;
			set => base.Declaration = value;
		}

		void LoadMenuItems()
		{
			MenuItems.Clear();
			MenuItems.AddRange(MenuItemsInOrder().ToArray());
			AddAdditionalMenuItems();
			SetupTopLevelMenu();
		}

		protected virtual void AddAdditionalMenuItems()
		{
		}

		IDictionary<ZGuid, ZString> GetMenuOptions()
		{
			Dictionary<ZGuid, ZString> options = null;
			if (Declaration is BaseJobDeclaration declaration)
			{
				options = declaration.Factory.GetValue(ref cachedMenuOptions, () => declaration.CustomsEntryInstructions.OrderBy(x => x.CEI_DisplaySequence).ThenBy(x => x.CEI_Style).ThenBy(x => x.CEI_Description)
					.ToDictionary(x => x.PK, y =>
					{
						var captionBuilder = new ZStringBuilder();
						captionBuilder.AppendIfNotEmpty(y.CEI_DisplaySequence.ToString());
						captionBuilder.Append(y.CEI_Style);
						captionBuilder.AppendIfNotEmpty(y.CEI_Description);
						captionBuilder.AppendIfNotEmpty(y.EntryHeader?.EntryNumber ?? ZString.Empty);
						return (ZString)captionBuilder.ToStringWithDelimiterBetweenAppends("_");
					}));
			}
			return options ?? new Dictionary<ZGuid, ZString>();
		}
		CachedProperty<Dictionary<ZGuid, ZString>> cachedMenuOptions;

		protected void AddImportNCTSLinesMenuChild()
		{
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			var count = nctsSettings.IsNctsEnabled ? (Declaration?.CustomsEntryInstructions?.Count ?? 0) : 0;

			if (count > 0)
			{
				if (importNCTSLinesMenuItem == null)
				{
					importNCTSLinesMenuItem = new ZMenuItem(importNCTSLinesMenuCaption, ImportNCTSLinesMenuItem_Click);
					importNCTSLinesMenuItem.Name = "importNCTSLinesMenuItem";
				}
				else
				{
					importNCTSLinesMenuItem.MenuItems.Clear();
				}

				if (count > 1)
				{
					foreach (var menuOption in GetMenuOptions())
					{
						var child = new ZMenuItem(menuOption.Value, ImportNCTSLinesMenuItem_Click);
						child.Tag = menuOption.Key;

						importNCTSLinesMenuItem.MenuItems.Add(child);
					}
				}

				MenuItems.RemoveByKey(importNCTSLinesMenuItem.Name);
				MenuItems.Add(importNCTSLinesMenuItem);
			}
			else if (importNCTSLinesMenuItem != null)
			{
				MenuItems.RemoveByKey(importNCTSLinesMenuItem.Name);
			}
		}

		ZString GetPackingListMenuItemCaption()
		{
			ZString result;
			if (Declaration != null && Declaration.HasCusPackingList(new BusinessObjectFactory()))
			{
				result = ResString.GetMultilingualString("73c04705-bba7-440e-947d-e0270132260e", "Edit Packing List");
			}
			else
			{
				result = ResString.GetMultilingualString("5559117e-8ba5-4a66-8f71-3d19d4eace58", "Create Packing List");
			}
			return result;
		}

		IEnumerable<MenuItem> MenuItemsInOrder()
		{
			yield return submitMenuItem;
			if (openCustomsWareMenuItem != null)
			{
				yield return openCustomsWareMenuItem;
			}
			yield return commercialInvoiceMenuItem;
			yield return autoApportionWeightMenuItem;
			yield return allocateRemainingWeightMenuItem;
			yield return bondedWarehouseMenuItem;
			yield return copyPreviousInvoiceLineMenuItem;
			yield return createProductFilesMenuItem;
			if (amendmentSnapshotManagementMenuItemsCreator != null)
			{
				yield return amendmentSnapshotManagementMenuItemsCreator.CreateMenuItem();
			}
			yield return refreshProductDataMenuItem;
			yield return dataMenuItem;
			yield return GenerateEntriesMenuItem;
			yield return apportionmentMenuItem;
			foreach (var item in BOMMenuItem)
			{
				yield return item;
			}
			yield return sendToGlobalManifestMenuItem;
			if (packingListMenuItem != null)
			{
				yield return packingListMenuItem;
			}
			if (ConsolidatedEntryMenuProvider != null)
			{
				foreach (var item in ConsolidatedEntryMenuProvider.CreateMenuEntries())
				{
					yield return item;
				}
			}
		}

		bool ShouldOpenCustomsWareMenuItemBeVisible()
		{
			return (Declaration?.IsInterface ?? false) && Declaration.EntryHasBeenSubmitted;
		}

		ResourceString GetOpenCustomsWareMenuItemText()
		{
			return ResString.GetMultilingualString("62d9f520-1d68-4a5d-8499-e77414d36a24", "Open {0} in CustomsWare", Declaration != null && Declaration.EntryHasBeenSubmitted ? Declaration.JE_DeclarationReference : (ZString)(NoResString)"Bxxx");
		}

		protected virtual bool SupportsBOMExpander => false;

		protected virtual bool DisplayGenerateEntriesMenuOption => false;

		protected virtual bool DisplayCreatePackingListMenuOption => Declaration?.SupportsCusPackingList ?? false;

		protected virtual string GenerateEntriesMenuOptionText
		{
			get { return ResString.GetMultilingualString("d69caca3-0247-4ec8-a19e-9e3bd0422a2d", "Generate Entries (&Merge)"); }
		}

		IDisposable GetCellSuspender()
		{
			var mainMenu = GetMainMenu();
			if (mainMenu != null)
			{
				var form = mainMenu.GetForm();
				if (form != null)
				{
					return new CellNotificationSuspender(form);
				}
			}
			return new DisposableObject();
		}

		protected void SendToGlobalManifestMenuItem_Click(object sender, EventArgs e)
		{
			using (GetCellSuspender())
			{
				if (Declaration.IsGlobalManifestIntegrationEnabled && (!(!Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge) || PerformMerge()) && PreSaveDeclaration(Declaration))
				{
					try
					{
						using (Declaration.Factory.AddDisposableService())
						{
							var result = ObjectFactory.Get<IDeclaratonToGlobalManifestPublisher>().Publish(Declaration, ZDateTimeOffset.Now);
							ZExceptionReporting.ProcessWithSaveExceptionHandling(Declaration.Factory.Save, () => { });
							if (Declaration.MessageInitiator.IsPublishToUniversalTransactionOK(result))
							{
								Globals.Message.Show(Res.GetString("{6A338750-E49E-40C7-888C-5F7EDBA8FFFA}", "Data sent to Global Manifest '{0}'.", Declaration.GlobalManifestReference), Res.GetString("{4D6D8C72-F7F9-4FD0-83AD-DBCC1FE897FD}", "Sent"), MessageBoxButtons.OK, MessageBoxIcon.Information);
							}
						}
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		protected void CreatePackingListMenuItem_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration) && Declaration.IsInDatabase)
			{
				var factoryForPackingList = new BusinessObjectFactory();
				factoryForPackingList.Saved += FactoryForPackingList_Saved;

				var packingList = Declaration.LoadCusPackingList(factoryForPackingList);

				try
				{
					if (packingList == null)
					{
						packingList = CreateNewPackingList(factoryForPackingList);
					}

					if (packingList != null)
					{
						packingList.AddDefaultPackageIfNeeded();

						var controller = ZControllerFactory.Create(ControllerIDs.Customs.CusPackingList);
						controller.SetFormsModalTo(Form);
						var form = packingList.IsInDatabase ? controller.ShowEditForm(packingList) : controller.ShowFormForNewEntity(packingList);
						if (form != null)
						{
							form.Closed += PackingListForm_Closed;
						}
#if DEBUG
						if (Globals.IsTest)
						{
							LastController = controller;
						}
#endif
					}
				}
				catch
				{
					UnlockMutexIfLockedByThisInstance();
					throw;
				}
			}
		}

		CusPackingList CreateNewPackingList(BusinessObjectFactory factoryForPackingList)
		{
			CusPackingList packingList = null;

			if (!MutexForCreatePackingList.IsLocked)
			{
				if (MutexForCreatePackingList.Lock())
				{
					packingList = Declaration.CreateCusPackingList(factoryForPackingList);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("E8CF9B07-03E5-404A-867B-C95ECF717B6F", "Someone else is already in the process of creating a Packing List.\r\nYou should be able to access the Packing List when the person has saved the record. Please try later."));
			}

			return packingList;
		}

		void PackingListForm_Closed(object sender, EventArgs e)
		{
			UnlockMutexIfLockedByThisInstance();
		}

		void FactoryForPackingList_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= FactoryForPackingList_Saved;
			UnlockMutexIfLockedByThisInstance();
		}

		protected bool CheckConsolidatedDeclarationForOtherUsers_OKToProceed(ZMenuItem sender)
		{
			var continueEvenIfOtherUsersHaveEntryOpen = true;
			if (sender?.ParentControl != null)
			{
				continueEvenIfOtherUsersHaveEntryOpen = CheckOtherUsersCurrentlyAccessingThisEntity(sender);
			}

			return continueEvenIfOtherUsersHaveEntryOpen;
		}

		#region OtherUsersCurrentlyAccessingThisObject

		public bool CheckOtherUsersCurrentlyAccessingThisEntity(ZMenuItem sender)
		{
			var continueWithSend = true;
			var mainForm = (ZForm)(sender).ParentControl;
			var beingEdited = mainForm.ObjectsBeingEdited(false);
			if (beingEdited.Count > 0)
			{
				var usersEditingMsg = string.Empty;
				foreach (var entry in beingEdited)
				{
					if (usersEditingMsg.Length > 0)
					{
						usersEditingMsg += "\r\n";
					}

					usersEditingMsg += entry.Key + ":\r\n" + entry.Value;
				}

				var otherUsersWarning = Res.GetString("927C8F5D-FCA6-4E07-8819-2756717553E9", "These users are currently modifying {0} or one of its dependent declarations:\r\n{1}\r\n(Times are shown in your local time zone).\r\n\r\nDo you want to continue this action?", mainForm.FormCaption, usersEditingMsg) + "\r\n\r\n";
				continueWithSend = UserNotification.Show(otherUsersWarning, Res.GetString("520847FD-E4F4-4CB9-B62D-6693180A9EEE", "Another session is currently editing this Consolidated Entry - Continue?"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.Yes;
			}

			return continueWithSend;
		}

		#region CustomsUserNotification

		public ICustomsUserNotification UserNotification => userNotification ?? (userNotification = GetUserNotificationCore());
		ICustomsUserNotification userNotification;

		protected virtual ICustomsUserNotification GetUserNotificationCore() => new FlexibleUserNotification();

		#endregion

		#endregion

		void UnlockMutexIfLockedByThisInstance()
		{
			if (MutexForCreatePackingList.HasLock)
			{
				MutexForCreatePackingList.Unlock();
			}
		}

		ZGlobalMutex MutexForCreatePackingList
		{
			get { return mutex ?? (mutex = new ZGlobalMutex(MutexIDs.CusPackingListMutex, Declaration.PK.ToString())); }
		}
		ZGlobalMutex mutex;

		protected void PerformGenerateEntriesClick(object sender, EventArgs e)
		{
			using (GetCellSuspender())
			{
				PerformGenerateEntriesCore();
			}
		}

		void SubmitMenu_Click(object sender, EventArgs e)
		{
			var continueWithSubmit = PreSaveDeclaration(Declaration);

			var caption = ResString.GetMultilingualString("BA1A1DF5-2FC8-42ED-9BED-6402A65282C2", "Submit Result");
			if (continueWithSubmit)
			{
				var result = Submit();
				RefreshMenu();
				Globals.Message.Show(result, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				Globals.Message.Show(DeclarationNotSubmittedText, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

		protected virtual ZString Submit()
		{
			var integrationProvider = CusIntegrationTypeDecider.CusIntegration(Declaration);
			return integrationProvider != null ? integrationProvider.Execute(Declaration) : DeclarationNotSubmittedText;
		}

		ZString DeclarationNotSubmittedText
		{
			get { return ResString.GetMultilingualString("E42D94A2-961D-41E7-9051-0BA5590757D3", "Declaration Not Submitted."); }
		}

		protected virtual void PerformGenerateEntriesCore()
		{
			if (Declaration != null)
			{
				PerformMerge();
			}
		}

		ZString lastDeclarationSetCallStack { get; set; }

		protected bool IsDeclarationAvailable
		{
			get
			{
				if (Declaration == null)
				{
					Globals.Message.ShowError(Res.GetString("bfea7e5b-7b61-44f9-9959-56edcbebf691", "Declaration is not available, please close and retry."), Res.GetString("da19b9d1-b32f-405a-ab5a-5ded7d048173", "Declaration not available"));
					ErrorReporter.ReportOnce("Declaration not available", string.Format("Last declaration set call stack: {0}", lastDeclarationSetCallStack));
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		protected virtual void AddAuditMenuItems()
		{
			new WriteToLogMenuItem(
				(IStmALogParent)Declaration.Shipment ?? Declaration,
				delegate
				{ return Declaration; },
				Env.Security.CustomsDeclarationAudit,
				AuditCustomsDeclarationMenuCaption
			).AddTo(this);
		}

		protected string AuditCustomsDeclarationMenuCaption
		{
			get { return ResString.GetMultilingualString("c21fd1b4-d6a2-468c-a691-c42f9445dc1b", "Audit Customs Declaration"); }
		}

		protected override bool IsLockCustomsFileMenuVisible()
		{
			var declarationType = (Declaration as ICustomsFileParent)?.DeclarationType;
			return !string.IsNullOrWhiteSpace(declarationType) && CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Cast<DeclarationLockConfig>().Any(c => c.DeclarationType == declarationType.Value);
		}

		protected override bool IsLockOrUnlockCustomsFileMenusEnabled => CustomsDataRegistry.Instance.DeclarationLockForEdit.Value.Any();
		protected override MultilingualString LockCustomsFileMenuItemCaption => ResString.GetMultilingualString("67E0D378-8B98-4EE7-ADF1-A24D615C696D", "Lock Customs Declaration");
		protected override MultilingualString UnlockCustomsFileMenuItemCaption => ResString.GetMultilingualString("C8013157-0C26-49A5-91F8-19CEB2D03D13", "Unlock Customs Declaration");

		protected sealed override void JobDeclarationChanged(ICommonInvoiceDataProvider oldValue, ICommonInvoiceDataProvider newValue) => JobDeclarationChangedCore((BaseJobDeclaration)oldValue, (BaseJobDeclaration)newValue);

		protected virtual void JobDeclarationChangedCore(BaseJobDeclaration oldValue, BaseJobDeclaration newValue)
		{
			base.JobDeclarationChanged(oldValue, newValue);
			if (newValue != null)
			{
				if (cachedMenuOptions != null && newValue.Factory != oldValue?.Factory)
				{
					cachedMenuOptions = null;
				}
				SetMessageInitiatorIfRequired(newValue);
			}

			AddAuditMenuItems();

			lastDeclarationSetCallStack = string.Format(CultureInfo.InvariantCulture, (NoResString)"Declaration set to: {0}. Callstack: {1}", Declaration == null ? "null" : "PK=" + Declaration.PK.ToString(), new StackTrace().ToString());
			RefreshMenu();
		}

		protected virtual void SetMessageInitiatorIfRequired(BaseJobDeclaration newValue)
		{
			if (!newValue.HasMessageInitiator)
			{
				newValue.MessageInitiator = GetNewMessageInitiator();
			}
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();

			GenerateEntriesMenuItem.Visible = Declaration != null && !Declaration.IsDeclarationIntegrated && DisplayGenerateEntriesMenuOption;
			apportionmentMenuItem.Visible = Declaration != null && Declaration.ShowApportionmentMenuItem;
			BOMMenuItem.SetAllVisible(SupportsBOMExpander);
			submitMenuItem.Visible = Declaration != null && Declaration.ShowSubmitMenuItem;
			importCSVMenuItem.Visible = Declaration != null && Declaration.EnableImportInvoices;
			bondedWarehouseMenuItem.Visible = Declaration != null && ((Declaration.IsWHSUniversalXMLActive ? (Declaration.IsOutwardBondedWarehousingEnabledForSingleOrMultipleEntry || Declaration.IsInwardBondedWarehousingEnabledForSingleOrMultipleEntry) : Declaration.HasLinesForInwardBondedWarehousing) || Declaration.IsWarehousedByExternalAgent || Declaration.HasWHSTransaction || Declaration.IsInventorySelectionEnabled || Declaration.IsBondedWarehousePermitEnabled || Declaration.IsWarehouseOrderFunctionActivated);
			sendToGlobalManifestMenuItem.Visible = Declaration != null && Declaration.IsGlobalManifestIntegrationEnabled;

			attachCommercialInvoiceMenuItem.Visible = Declaration?.EnableAttachCommercialInvoice ?? false;
			copyCommercialInvoiceMenuItem.Visible = Declaration?.EnableCopyCommercialInvoice ?? false;
			commercialInvoiceMenuItem.Visible = attachCommercialInvoiceMenuItem.Visible || copyCommercialInvoiceMenuItem.Visible;
			packingListMenuItem.Text = GetPackingListMenuItemCaption();
			packingListMenuItem.Visible = DisplayCreatePackingListMenuOption;
			if (openCustomsWareMenuItem != null)
			{
				openCustomsWareMenuItem.Text = GetOpenCustomsWareMenuItemText();
				openCustomsWareMenuItem.Visible = ShouldOpenCustomsWareMenuItemBeVisible();
			}
			ConsolidatedEntryMenuProvider?.RefreshMenu(Declaration);
			AddImportNCTSLinesMenuChild();
			amendmentSnapshotManagementMenuItemsCreator?.RefreshMenuItem(Declaration);
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessageInitiator()
		{
			return new SendsMessagesToCustomsGUI();
		}

		protected internal bool CheckHasChanges()
		{
			bool okToContinue = true;
			var topLevelBusinessObject = (BusinessObject)Declaration.Shipment ?? Declaration;
			if (topLevelBusinessObject.HasChanges && Globals.Message.Show(Res.GetString("B52E2D4B-825F-41D2-972E-C5FCAD7B2AF4", "The Job has not yet been saved. Do you want to save and proceed?"), Res.GetString("B4D37DD9-D9DB-4E10-B048-1177C8A4FA40", "Save Job"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes) == DialogResult.No)
			{
				okToContinue = false;
			}

			if (okToContinue && topLevelBusinessObject.HasChanges)
			{
				okToContinue = FireSaveButton();
			}

			return okToContinue;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (Declaration != null)
				{
					Declaration.UnlockDoMergeMutex();
				}
			}
			finally
			{
				if (disposing)
				{
					DisposeBondedWarehouseMenuItemsData();
				}
				base.Dispose(disposing);
			}
		}

		void DisposeBondedWarehouseMenuItemsData()
		{
			if (bondedWarehouseMenuItemsData != null)
			{
				bondedWarehouseMenuItemsData.ForEach(x =>
				{
					bondedWarehouseMenuItem.MenuItems.Remove(x.multipleWarehouseEntryMenuItem);
					if (x.shortCutBondedWarehouseMenuItem != null)
					{
						bondedWarehouseMenuItem.MenuItems.Remove(x.shortCutBondedWarehouseMenuItem);
					}
					x.creator.Dispose();
				});
				bondedWarehouseMenuItemsData.Clear();
				bondedWarehouseMenuItemsData = null;
			}
		}

		#region Implementation

		protected MenuItem dataMenuItem;
		protected internal MenuItem commercialInvoiceMenuItem;
		protected internal MenuItem bondedWarehouseMenuItem;
		protected internal MenuItem updateBondedWarehouseMenuItem;
		protected internal MenuItem cancelUpdateBondedWarehouseInwardMenuItem;
		protected internal MenuItem synchronizeWithBondedWarehouseMenuItem;
		protected internal MenuItem finaliseStockWithBondedWarehouseMenuItem;
		protected internal MenuItem cancelBondedWarehouseMenuItem;
		protected internal MenuItem synchronizeWithOrdersMenuItem;
		protected ZMenuItem sendToGlobalManifestMenuItem;
		protected ZMenuItem packingListMenuItem;
		internal readonly MenuItem inventoriesSelectionFromBondedWarehouseMenuItem;
		internal readonly MenuItem orderLinesSelectionFromBondedWarehouseMenuItem;
		internal readonly MenuItem disableBondedWarehouseIntegrationMenuItem;
		readonly MenuItem createProductFilesMenuItem;
		readonly MenuItem submitMenuItem;
		readonly MenuItem openCustomsWareMenuItem;
		readonly MenuItem attachCommercialInvoiceMenuItem;
		internal readonly MenuItem copyCommercialInvoiceMenuItem;
		readonly MenuItem importCSVMenuItem;
		readonly MenuItem refreshProductDataMenuItem;
		protected MenuItem importNCTSLinesMenuItem;
		readonly ZString importNCTSLinesMenuCaption = ResString.GetMultilingualString("940639B0-F53F-43D1-8948-1CF977A42EFF", "Import NCTS lines");
		readonly AmendmentSnapshotManagementMenuItemsCreator amendmentSnapshotManagementMenuItemsCreator;

		protected virtual AmendmentSnapshotManagementMenuItemsCreator CreateNewAmendmentSnapshotManagementMenuItemsCreator() => null;

		protected virtual void SetupTopLevelMenu()
		{
		}

		protected void ExportDeclarationXmlClick(object sender, EventArgs e)
		{
			var declarations = new[] { Declaration };
			var exporter = new DeclarationXmlDataTransferExporter(DeclarationValueObjectDataAdapter.New(), true);
			exporter.DefaultFileName = Declaration.JE_DeclarationReference + "_" + ZDateTime.Now.ToString("yyyyMMddHHmmss");
			if (!(new ZString(SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value).IsEmpty))
			{
				exporter.InitialDirectory = SystemDataRegistry.Instance.CustomDeclarationExportDirectory.Value;
			}
			exporter.PromptUserAndExport(declarations);
		}

		protected void ImportInvoicesClick(object sender, EventArgs e)
		{
			if (!Declaration.ReadOnly)
			{
				using (GetCellSuspender())
				{
					InvoiceImporter importer = new InvoiceImporter(Declaration, DataTransferImpl);
					importer.ShowInvoiceImporterForm();
				}
			}
			else
			{
				ShowReadOnlyWarning();
			}
		}

		protected void ImportOrderLinesClick(object sender, EventArgs e)
		{
			if (IsDeclarationAvailable)
			{
				if (!Declaration.ReadOnly)
				{
					if (Declaration.HasChanges)
					{
						Globals.Message.ShowError(Res.GetString("8b6f3091-bade-42a2-af10-e4095d911c53", "Please save before importing order lines"), Res.GetString("93301cd3-9a6d-478c-a74a-61dd11545e13", "Please Save Before Importing"));
					}
					else
					{
						OrderInvoiceImporter importer = new OrderInvoiceImporter(Declaration);
						if (importer.OrdersToImport.Count == 0)
						{
							Globals.Message.ShowError(Res.GetString("30822cdf-b3cc-4f86-bfa7-e95f6bf628d0", "Please Link Orders to this Shipment/Declaration before importing Order Lines"), Res.GetString("b04237a0-345e-4eb3-90f8-5d31d646f0c5", "No Orders To Import"));
						}
						else
						{
							using (GetCellSuspender())
							{
								if (!Globals.IsTest)
								{
									ZFormModaliser.ShowDialogAndDispose(new OrderInvoiceImporterForm(importer));
								}
							}
						}
					}
				}
				else
				{
					ShowReadOnlyWarning();
				}
			}
		}

		void ShowReadOnlyWarning()
		{
			Globals.Message.Show(Res.GetString("76977776-1295-40d7-9cbf-96bd8e6164f1", "The Declaration is Read-only, this function is not available"));
		}

		void PerformApportionmentClick(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				using (ApportionmentProgressForm progressForm = new ApportionmentProgressForm(Declaration))
				{
					using (GetCellSuspender())
					{
						progressForm.Show();
						Declaration.ResumeApportionment();
						progressForm.Close();
					}
				}
			}
		}

		void AttachMenu_Click(object sender, EventArgs e)
		{
			InvoiceAttacher attacher = new InvoiceAttacher(Declaration.Invoices, Declaration.Lookups.InvoicesToAttach);
			attacher.NameOfAnElementInDestinationCollection = (NoResString)"invoice";
			attacher.Show((ZForm)GetMainMenu().GetForm());
#if DEBUG
			LastShownAttachPopupForTesting = attacher.LastShownAttachPopupForTesting;
#endif
		}
#if DEBUG
		public IDisposable LastShownAttachPopupForTesting;
#endif

		void CopyMenu_Click(object sender, EventArgs e)
		{
			var bizObjectCollection = (IBusinessObjectCollection)Declaration.Invoices;
			if (bizObjectCollection.HasChanges)
			{
				Globals.Message.ShowInformation(Res.GetString("32fb7814-b483-4080-bfd5-c7f6ea1614ce", "Please save the form before copying."));
			}
			else
			{
				var invoicesToChoose = new CommercialInvoiceCollection(Declaration.Factory);
				((IFilterBusinessObjectDefaultsProvider)invoicesToChoose).FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Attached to a Declaration", "Property", new ZString(AttachedToDeclarationFilterOptions.Codes.AttachedToDeclaration), false));

				var chooser = new InvoiceChooser(invoicesToChoose);
				chooser.ShowModal((ZForm)GetMainMenu().GetForm(), selectedInvoices =>
				{
					foreach (BaseJobComInvoiceHeader header in selectedInvoices)
					{
						var invoice = Declaration.Invoices.AddNew();
						var copyInvoiceBo = new JobComInvoiceHeaderCopyBO(header, invoice);
						copyInvoiceBo.CopyInvoice();
					}
				});
			}
		}

		#region DataTransferImpl
		protected internal DataTransferImpl DataTransferImpl
		{
			get
			{
				if (fDataTransferImpl == null)
				{
					fDataTransferImpl = GetDataTransferImpl();
				}

				return fDataTransferImpl;
			}
		}
		DataTransferImpl fDataTransferImpl;

		protected virtual DataTransferImpl GetDataTransferImpl()
		{
			return new DataTransferImpl();
		}
		#endregion

		internal void UpdateBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsUpdate.IsAllowed)
			{
				if (Declaration.IsOutwardBondedWarehousingEnabled)
				{
					UpdateBondedWarehouseOutward();
				}
				else
				{
					UpdateBondedWarehouseInward();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsUpdate);
			}
		}

		protected virtual BondedWarehouseOperationDeterminer GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return new BondedWarehouseOperationDeterminer(supporter);
		}

		protected virtual string TermName
		{
			get { return Res.GetString("8065c0aa-8156-4ccd-bf34-dd6907a2db06", "Inventory Management"); }
		}

		void UpdateBondedWarehouseOutward()
		{
			if (CheckHasChangesAndMergeIfNeeded(Res.GetString("C2CE816E-A3B3-424A-B4B5-E32FCEDB099D", "Updating {0} stock release when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?", TermName), extraCheck: GetNewBondedWarehouseOperationDeterminer(Declaration).CanUpdateBondedWarehouseOutwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				Declaration.UpdateBondedWarehouseOutward();
			}
		}

		void UpdateBondedWarehouseInward()
		{
			if (CheckHasChangesAndMergeIfNeeded(Res.GetString("2BC30078-9C61-40A2-ADC9-8BB3202FDAC7", "Updating {0} stock levels when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?", TermName), extraCheck: GetNewBondedWarehouseOperationDeterminer(Declaration).CanUpdateBondedWarehouseInwardCheck)
				&& CheckRequiredFieldsForBondedWarehousingAreEntered())
			{
				if (Declaration.IsWHSUniversalXMLActive)
				{
					Declaration.UpdateBondedWarehouseInward();
				}
				else
				{
					((BusinessObject)Form.BusinessEntity).LoadChildEditableObjects();
					Form.BusinessEntity.RunPreSaveValidation();
					TabPageNotificationsExposer.ExposeTabPageNotifications(Form, Form.BusinessEntity);
					if (!Form.BusinessEntity.HasErrors() && !Form.BusinessEntity.HasMessageErrors())
					{
						UpdateBondedWarehouseInwardViaOldIntegration();
					}
					else
					{
						Globals.Message.ShowInformation(Res.GetString("2c9772e7-d81c-4ccf-9334-dd47b54c3aa3", "Please fix all errors and message errors on the form before updating the {0}.", TermName));
					}
				}
			}
		}

		void UpdateBondedWarehouseInwardViaOldIntegration()
		{
			try
			{
				Declaration.CreateOrUpdateBondedWarehouseInward();
				if (FireSaveButton())
				{
					Globals.Message.ShowInformation(Res.GetString("263cde1a-5b85-4879-8462-440f8962fa36", "{0} stock levels have been updated.", TermName));
				}
			}
			catch (CannotUpdateStockException e)
			{
				Globals.Message.ShowError(e.Message);
			}
			catch (MissingDataException e) // this should never happen, as validation on customs side should stop it
			{
				Globals.Message.ShowError(Res.GetString("1fbc36b2-d6ba-405a-9a22-42a7e73bcb36", "Sorry, could not update {0} as the following information is missing: \r\n{1}", TermName, e.Message));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		internal void BondedWarehouseMenuItem_Popup(object sender, EventArgs e)
		{
			DisposeBondedWarehouseMenuItemsData();
			var supportsBondedWarehousing = Declaration?.SupportsBondedWarehousingForSingleOrMultipleEntry ?? false;
			var isInventorySelectionEnabled = Declaration?.IsInventorySelectionEnabled ?? false;
			var isBondedWarehousePermitEnabled = Declaration?.IsBondedWarehousePermitEnabled ?? false;
			if (supportsBondedWarehousing || isInventorySelectionEnabled || isBondedWarehousePermitEnabled)
			{
				var supportMultipleWarehouseEntry = false;
				if (Declaration.SupportMultipleWarehouseEntry)
				{
					supportMultipleWarehouseEntry = supportsBondedWarehousing;
					if (supportMultipleWarehouseEntry && Declaration.IsWHSUniversalXMLActive)
					{
						AddBondedWarehouseMenuItems(supportsBondedWarehousing);
					}
				}

				var hasWHSTransaction = Declaration.HasWHSTransaction;
				updateBondedWarehouseMenuItem.Visible = supportsBondedWarehousing && !supportMultipleWarehouseEntry && ((Declaration.IsWHSUniversalXMLActive && Declaration.IsOutwardBondedWarehousingEnabled) || Declaration.IsWarehousedByExternalAgent || (Declaration.HasLinesForInwardBondedWarehousing && Declaration.EntriesExistAndAllHaveEntryNumbers) || hasWHSTransaction);
				cancelUpdateBondedWarehouseInwardMenuItem.Visible = supportsBondedWarehousing && !supportMultipleWarehouseEntry && Declaration.IsWHSUniversalXMLActive && hasWHSTransaction && !Declaration.IsOutwardBondedWarehousingEnabled;
				synchronizeWithBondedWarehouseMenuItem.Visible = Declaration.ShouldUpdateOutwardLinesWithInventoryDetails;
				inventoriesSelectionFromBondedWarehouseMenuItem.Visible = Declaration.IsInventorySelectionEnabled;
				cancelBondedWarehouseMenuItem.Visible = supportsBondedWarehousing && !supportMultipleWarehouseEntry && (Declaration.IsWHSUniversalXMLActive ? (Declaration.IsOutwardBondedWarehousingEnabled && hasWHSTransaction) : (bool)Declaration.IsExWarehouse);
				finaliseStockWithBondedWarehouseMenuItem.Visible = supportsBondedWarehousing && !Declaration.IsWHSUniversalXMLActive && Declaration.EntriesExistWithExBondAutomationAndAllHaveEntryNumbers;
				disableBondedWarehouseIntegrationMenuItem.Visible = supportsBondedWarehousing && !supportMultipleWarehouseEntry && !isBondedWarehousePermitEnabled && Declaration.IsWHSUniversalXMLActive && !Declaration.IsBondedWarehousingDisabled && !(Declaration.SingleWarehouseEntry?.CH_HasManualWhsUpdate ?? false);
				synchronizeWithOrdersMenuItem.Visible = isBondedWarehousePermitEnabled;
				orderLinesSelectionFromBondedWarehouseMenuItem.Visible = Declaration.IsWarehouseOrderFunctionActivated;
			}
			else
			{
				updateBondedWarehouseMenuItem.Visible = false;
				cancelUpdateBondedWarehouseInwardMenuItem.Visible = false;
				synchronizeWithBondedWarehouseMenuItem.Visible = false;
				inventoriesSelectionFromBondedWarehouseMenuItem.Visible = false;
				finaliseStockWithBondedWarehouseMenuItem.Visible = false;
				cancelBondedWarehouseMenuItem.Visible = false;
				disableBondedWarehouseIntegrationMenuItem.Visible = false;
				synchronizeWithOrdersMenuItem.Visible = false;
				orderLinesSelectionFromBondedWarehouseMenuItem.Visible = false;
			}
		}

		protected virtual bool SupportShortCutBondedWarehouseMenus => false;

		void CreateBondedWarehouseMenuItemsDictionary(bool supportsBondedWarehousing, bool supportShortCutBondedWarehouseMenus)
		{
			foreach (var entry in Declaration.ActiveEntryHeaders.OfType<CusEntryHeader>())
			{
				if (((!entry.IsBondedWarehousingDisabled && entry.HasAnInvoiceLineMarkedForBondedWarehousing) || (entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails)) &&
					(entry.IsInwardBondedWarehousingEnabled || entry.IsOutwardBondedWarehousingEnabled || entry.IsChangeOfOwnershipBondedWarehousingEnabled || entry.IsChangeOfRegimeWarehousingEnabled))
				{
					var creator = new BondedWarehouseMenuItemsCreator(entry, this, entry.EntryHeaderDescriptiveMenuItemText, supportShortCutBondedWarehouseMenus);
					var multipleWarehouseEntryMenuItems = creator.BondedWarehouseMenuItem;
					ZMenuItem shortCutBondedWarehouseMenuItem = null;

					if (supportShortCutBondedWarehouseMenus && supportsBondedWarehousing
						&& entry.HasAnInvoiceLineMarkedForBondedWarehousingWithEntryDetails
						&& (JobDeclarationWarehouseExtensions.HasWHSTransaction(entry)
						|| entry.IsInwardBondedWarehousingEnabled
						|| entry.IsOutwardBondedWarehousingEnabled))
					{
						shortCutBondedWarehouseMenuItem = creator.ShortCutUpdateBondedWarehouseMenuItem;
					}

					BondedWarehouseMenuItemsData.Add((creator, multipleWarehouseEntryMenuItems, shortCutBondedWarehouseMenuItem));
				}
			}
		}

		void AddBondedWarehouseMenuItems(bool supportsBondedWarehousing)
		{
			var supportShortCutBondedWarehouseMenus = SupportShortCutBondedWarehouseMenus;
			CreateBondedWarehouseMenuItemsDictionary(supportsBondedWarehousing, supportShortCutBondedWarehouseMenus);

			if (supportShortCutBondedWarehouseMenus)
			{
				var index = 0;
				foreach ((ZMenuItem multipleWarehouseEntryMenuItem, ZMenuItem shortCutBondedWarehouseMenuItem) in BondedWarehouseMenuItemsData.OrderBy(x => x.multipleWarehouseEntryMenuItem.Text).Select(x => (x.multipleWarehouseEntryMenuItem, x.shortCutBondedWarehouseMenuItem)))
				{
					if (shortCutBondedWarehouseMenuItem != null)
					{
						bondedWarehouseMenuItem.MenuItems.Add(index++, shortCutBondedWarehouseMenuItem);
					}
					bondedWarehouseMenuItem.MenuItems.Add(multipleWarehouseEntryMenuItem);
				}
			}
			else
			{
				foreach (var entryMenuItem in BondedWarehouseMenuItemsData.Select(x => x.multipleWarehouseEntryMenuItem).OrderByDescending(x => x.Text))
				{
					bondedWarehouseMenuItem.MenuItems.Add(0, entryMenuItem);
				}
			}
		}

		protected internal void SynchronizeWithOrdersMenuItem_ClickInternal(object sender, EventArgs e) => SynchronizeWithOrdersMenuItem_Click(sender, e);
		protected virtual void SynchronizeWithOrdersMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				if ((Declaration.Invoices.Count == 0 && Declaration.InvoiceLines.Count == 0) ||
					Globals.Message.Show(
						Res.GetString("0DCA9CCB-ABA1-4B0B-B032-770E152FB54F", "System will replace existing invoice data with the data from Orders. Do you want to continue?"),
						Res.GetString("F82B85FE-1E8A-4276-AF4C-5E26D16E02FA", "Existing Invoice Data Replacement"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					var messageText = Declaration.SynchronizeWithOrders();
					if (messageText.IsEmpty)
					{
						SelectInvoiceLinesTabPage();
					}
					else
					{
						Globals.Message.ShowInformation(messageText);
					}
				}
			}
		}

		List<(BondedWarehouseMenuItemsCreator creator, ZMenuItem multipleWarehouseEntryMenuItem, ZMenuItem shortCutBondedWarehouseMenuItem)> BondedWarehouseMenuItemsData => bondedWarehouseMenuItemsData ?? (bondedWarehouseMenuItemsData = new List<(BondedWarehouseMenuItemsCreator creator, ZMenuItem multipleWarehouseEntryMenuItem, ZMenuItem shortCutBondedWarehouseMenuItem)>());
		List<(BondedWarehouseMenuItemsCreator creator, ZMenuItem multipleWarehouseEntryMenuItem, ZMenuItem shortCutBondedWarehouseMenuItem)> bondedWarehouseMenuItemsData;

		void DisableBondedWarehouseIntegrationMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsDisable.IsAllowed)
			{
				if (CheckHasChangesAndMergeIfNeeded(Res.GetString("{B95558CE-CFF3-409C-9F5F-7A41E7F09CEC}", "Disabling {0} Integration when there is a change which requires Customs amendment messaging might cause the data to be out of sync.\r\nDo you still wish to continue?", TermName)))
				{
					Declaration.WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.AutomationIsDisabled;
					if (FireSaveButton())
					{
						Globals.Message.ShowInformation(Res.GetString("{29183BDA-D860-461D-B460-F08A7E5179A3}", "{0} Integration has been disabled.", TermName));
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsDisable);
			}
		}

		void InventoriesSelectionFromBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new InventorySelectionForm(Declaration.InventorySelectionHeader))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					SelectInvoiceLinesTabPage();
				}
			}
		}

		void OrderLinesSelectionFromBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			using (var form = new OrderLinesSelectionForm(Declaration))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					SelectInvoiceLinesTabPage();
				}
			}
		}

		void SelectInvoiceLinesTabPage()
		{
			var parentForm = Form;
			var shipmentForm = parentForm as ShipmentForm;
			if (shipmentForm != null)
			{
				shipmentForm.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				var plugin = (BaseBrokeragePlugIn)shipmentForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
				plugin.OnGUIShown();
				var userControl = (BaseCustomsBrokerageUserControl)plugin.UserControl;
				userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
			}
			else
			{
				var declarationForm = parentForm as BaseJobDeclarationForm;
				if (declarationForm != null)
				{
					var customsBrokerageUserControl = declarationForm.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
				}
			}
		}

		internal void SynchronizeWithBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration.IsWHSUniversalXMLActive)
			{
				if (Declaration.InvoiceLines.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("360231f1-d55b-4c4a-9548-9a1dabe922c9", "At least one invoice line is required in order for Synchronization to work."));
				}
				else
				{
					var errorMessage = Declaration.UpdateOutwardLinesWithInventoryDetails();
					if (!errorMessage.IsEmpty)
					{
						SelectInvoiceLinesTabPage();
						Globals.Message.ShowError(errorMessage);
					}
				}
			}
			else
			{
				if (!Declaration.HasALineWithExBondAutomation)
				{
					Globals.Message.ShowInformation(Res.GetString("2d43a3bf-3fc4-4539-9d10-ffc1e97cab35", "Please enter at least one invoice line with {0} integration enabled.", TermName));
				}
				else
				{
					Declaration.CreateAndUpdateInvoicesForExBondAutomation();
				}
			}

			if (Declaration.InvoiceLines.Any(x => x.HasRowErrors || x.HasRowMessageErrors))
			{
				SelectInvoiceLinesTabPage();
			}
		}

		internal void FinaliseStockWithBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (CheckHasChangesAndMergeIfNeeded())
			{
				if (Declaration.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms)
				{
					Globals.Message.Show(Res.GetString("655513a2-bcf4-46b8-9b3a-0eb10013eb25", "Sorry, you cannot finalize this declaration in the {0} as it has amendments outstanding which have not yet been cleared by Customs.", TermName));
				}
				else
				{
					Declaration.NotifyBondedWarehouseThatExWarehouseEntryHasCleared();
					if (FireSaveButton())
					{
						Globals.Message.ShowInformation(Res.GetString("9324e27d-4ceb-4610-9a5c-d5bff9b42b4d", "The stock has been successfully finalized in the {0} system.", TermName));
					}
				}
			}
		}

		void CancelBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (Declaration != null && CheckHasChangesAndMergeIfNeeded(Res.GetString("D7278C08-E1B0-4233-B3A7-CA15706EA002", "Canceling {0} stock release when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?", TermName), extraCheck: GetNewBondedWarehouseOperationDeterminer(Declaration).CanCancelBondedWarehouseOutwardCheck)
					&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false) &&
					Globals.Message.Show(Declaration.IsWHSUniversalXMLActive ? ConfirmCancelBondedWarehouseOutwardAutomationMessageNew(TermName) : ConfirmCancelBondedWarehouseOutwardAutomationMessageOld(TermName), Res.GetString("bd12397f-febe-4d9f-8b83-8d8be0384f3c", "Are you sure?"), MessageBoxButtons.YesNo, DialogResult.No)
						== DialogResult.Yes)
				{
					if (Declaration.IsWHSUniversalXMLActive)
					{
						Declaration.CancelBondedWarehouseOutward();
					}
					else
					{
						Declaration.CancelBondedWarehouseIntegration();
						if (!Declaration.HasChanges)
						{
							Declaration.HasChanges = true;
						}
					}
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		protected bool RunPreSaveValidationIfHasNoChanges(BaseJobDeclaration declaration)
		{
			var result = true;

			if (!declaration.HasChanges)
			{
				using (new ZWaitCursorChanger())
				{
					declaration.LoadChildEditableObjects();
					declaration.RunPreSaveValidation();
				}

				if (declaration.HasErrors)
				{
					using (var errorMessageBox = new ZErrorMessageBox(declaration))
					{
						ZFormModaliser.ShowDialogAndDispose(errorMessageBox, Form);
					}
					result = false;
				}
			}

			return result;
		}

		protected virtual JobDeclarationUniversalMessagingHelper GetJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent wrapper)
		{
			return new JobDeclarationUniversalMessagingHelper(wrapper);
		}

		protected bool PreSaveDeclaration(BaseJobDeclaration declaration)
		{
			var topLevelBizObj = declaration.Shipment as BusinessObject ?? declaration;
			return Enterprise.Customs.GUI.PlugIn.CustomsPlugIn.FormPreSaved(topLevelBizObj, Form);
		}

		public static string ConfirmCancelBondedWarehouseOutwardAutomationMessageOld(string term)
		{
			return Res.GetString("4917e909-71f8-4ee3-b65d-1495776d40c7", "Are you sure you want to cancel the {0} stock release for this job? \r\nIf you click 'Yes', all stock for this job will be uncommitted in the {0} and all invoice lines will have the tick in the 'Warehouse Automation?' column removed.", term);
		}

		public static string ConfirmCancelBondedWarehouseOutwardAutomationMessageNew(string term)
		{
			return Res.GetString("699D19C5-0562-47A2-B41A-CA057F09122B", "Are you sure you want to cancel the {0} stock release for this job? \r\nIf you click 'Yes', all stock for this job will be uncommitted in the {0}.", term);
		}

		public static string SaveDeclarationFirstMessage
		{
			get { return Res.GetString("fcaea557-24c2-4658-963d-f73a3a6ece68", "The data must be saved.\r\nDo you want to save and proceed?"); }
		}

		protected BusinessObject TopLevelBusinessObject
		{
			get { return (BusinessObject)Declaration.Shipment ?? Declaration; }
		}

		protected bool CheckRequiredFieldsForBondedWarehousingAreEntered(bool checkProduct = true, bool checkQuantity = true, bool checkEntryDetails = true)
		{
			var result = true;
			if (Declaration != null && Declaration.IsWHSUniversalXMLActive)
			{
				var errorMessages = Declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct, checkQuantity, checkEntryDetails);
				if (!errorMessages.IsEmpty)
				{
					result = false;
					Declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(errorMessages);
				}
			}
			return result;
		}

		protected virtual bool PerformMerge()
		{
			return Declaration.DoMerge();
		}

		protected bool CheckHasChangesAndMergeIfNeeded(string amendmentWarningMessage = null, Func<bool> extraCheck = null)
		{
			bool okToContinue = true;
			var topLevelBusinessObject = TopLevelBusinessObject;
			if (topLevelBusinessObject.HasChanges && Globals.Message.Show(SaveDeclarationFirstMessage, Res.GetString("caab2837-1d6c-4ea9-b11a-36f395e542c3", "Save Data"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
			{
				okToContinue = false;
			}

			if (okToContinue && (!Declaration.IsMergeDone || Declaration.MergeManager.RequiresMerge))
			{
				okToContinue = PerformMerge();
			}

			if (okToContinue && extraCheck != null)
			{
				okToContinue = extraCheck();
			}

			if (okToContinue)
			{
				if (topLevelBusinessObject.HasChanges)
				{
					using (Declaration.SuspendAmendmentDetection())
					{
						if (!Declaration.IsWarehousedByExternalAgent)
						{
							var haveAmendmentsBeenMadeAndNotYetClearedByCustoms = Declaration.HaveAmendmentsBeenMadeAndNotYetClearedByCustoms;
							if (!haveAmendmentsBeenMadeAndNotYetClearedByCustoms)
							{
								IMessageManageableBizObj messageManageableDeclaration = Declaration as IMessageManageableBizObj;
								if (messageManageableDeclaration != null && messageManageableDeclaration.IsInAStatusAmendmentSendable)
								{
									IMessageManager manager = messageManageableDeclaration.GetMessageManagerForAmendmentDetection();
									if (manager != null)//Depending on the message type, amendment might not be supported and thus, manager can be null
									{
										RequiredMessagesInformation detectionResult = null;
										try
										{
											detectionResult = manager.GetRequiredMessagesInformation();
											haveAmendmentsBeenMadeAndNotYetClearedByCustoms = detectionResult.HasMessagesToSend;
											if (haveAmendmentsBeenMadeAndNotYetClearedByCustoms)
											{
												var savingOptions = manager.GetDeferredAmendmentSavingOptions() as DeferredAmendmentSavingOptions;
												if (savingOptions != null)
												{
													savingOptions.SaveWithEntryChanges = true;
													manager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, detectionResult);
												}
											}
										}
										finally
										{
											if (detectionResult != null)
											{
												detectionResult.ClearBridgesAndReInitialise();
												detectionResult.IsInProcessOfDetectingRequiredMessages = false;
											}
										}
									}
								}
							}
							if (haveAmendmentsBeenMadeAndNotYetClearedByCustoms && !string.IsNullOrEmpty(amendmentWarningMessage))
							{
								okToContinue = Globals.Message.Show(amendmentWarningMessage, Res.GetString("70C5F7FD-CF83-477B-ADB9-8EE1A719C59F", "Amendment Detected"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes;
							}
						}
						okToContinue = okToContinue && FireSaveButton();
					}
				}
				else
				{
					topLevelBusinessObject.RunPreSaveValidation();
					if (topLevelBusinessObject.HasErrors)
					{
						okToContinue = false;
						Globals.Message.ShowError(Res.GetString("8ce4da89-8a9e-4bc7-8822-db7ce8486ba6", "Unable to proceed due to some critical errors; please fix all errors before trying again."));
						Form.Refresh();
					}
				}
			}

			return okToContinue;
		}

		protected virtual SendsMessagesToCustomsGUI GetNewMessagingActionsController()
		{
			return new SendsMessagesToCustomsGUI();
		}

		internal void CancelUpdateBondedWarehouseInwardMenuItem_Click(object sender, EventArgs e)
		{
			if (Env.Security.CustomsBondedWhsCancel.IsAllowed)
			{
				if (CheckHasChangesAndMergeIfNeeded(Res.GetString("C3FAD8DE-2303-4391-A068-CF8EB34CC4C3", "Canceling {0} stock levels update when there is a change which requires Customs amendment messaging will cause the data to be out of sync.\r\nDo you still wish to continue?", TermName), extraCheck: GetNewBondedWarehouseOperationDeterminer(Declaration).CanCancelUpdateBondedWarehouseInwardCheck)
					&& CheckRequiredFieldsForBondedWarehousingAreEntered(checkProduct: false, checkQuantity: false, checkEntryDetails: false))
				{
					Declaration.CancelBondedWarehouseInward();
				}
			}
			else
			{
				Env.Security.ShowError(Env.Security.CustomsBondedWhsCancel);
			}
		}

		void CreateProductFilesMenuItem_Click(object sender, EventArgs e)
		{
			if (Declaration.HasChanges)
			{
				Globals.Message.Show(Res.GetString("0f9e8154-a8a3-4204-bc8a-cfdb2b7e04e1", "Please save declaration before saving products."), Res.GetString("305526e6-531c-4e55-b9df-6ee9a98aacc5", "Please Save Declaration"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				if (!Env.Security.CustomsSupplierPartModifyCustoms.IsAllowed)
				{
					Env.Security.CustomsSupplierPartModifyCustoms.ShowError();
				}
				else if (Declaration.InvoicesMentionNewOrInactiveProducts && DeclarationForProductCreationHelperCollection.ShouldShowProductCreationConfirmation(Declaration))
				{
					if (ZFormModaliser.ShowDialogAndDispose(new ProductCreationConfirmationForm(Declaration)) == DialogResult.OK)
					{
						Declaration.Factory.Save();
					}
				}
				else
				{
					Globals.Message.Show(Res.GetString("cceb3a3f-559b-4e1e-95e7-020d96de4a71", "There are no new products that can be saved in this declaration."), Res.GetString("49874153-7c05-45d8-9249-97941738d26c", "New products for save not found"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
		}

		void RefreshProductDataMenuItem_Click(object sender, EventArgs e)
		{
			Declaration.RefreshInvoiceLineProducts();
		}

		void DoExpandAllBOMLinesMenuItem_Click(object sender, EventArgs e)
		{
			if (GetConfirmationFromUsers(Res.GetString("c0dfd210-cbf9-4df3-a3a6-def70a06cb69", "Expand ALL BOM Lines"),
Res.GetString("9a5a531d-bf92-4ed5-9017-8d9c35deb99d", "All Invoice lines that have a Bill Of Materials defined, and are not already expanded, will be expanded; i.e. new invoice lines will be added for all Parts defined in the Bill Of Materials."))
				== DialogResult.OK)
			{
				Declaration.InvoiceLines.ExpandAllBOMProductLines();
			}
		}

		void DoCollapseAllBOMLinesMenuItem_Click(object sender, EventArgs e)
		{
			if (GetConfirmationFromUsers(Res.GetString("aebba869-87eb-4908-90e4-50b24778b0aa", "Collapse ALL BOM Lines"),
Res.GetString("f45a19cb-906e-4e10-bd6d-0cebdd3bdd06", "All Invoice lines that have a Bill Of Materials defined, and are expanded, will be collapsed; i.e. all invoice lines expanded from Parts defined in the Bill Of Materials will be removed."))
				== DialogResult.OK)
			{
				Declaration.InvoiceLines.CollapseAllBOMProductLines();
			}
		}

		void ImportNCTSLinesMenuItem_Click(object sender, EventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				var instructions = Declaration.CustomsEntryInstructions;
				var cei = menuItem.Tag is ZGuid key ? instructions.SingleOrDefault(i => i.PK == key) : instructions.FirstOrDefault();
				if (cei?.EntryHeader is CusEntryHeader entryHeader)
				{
					var integrator = entryHeader.CommonGoodsItemsIntegrator;
					var attacher = new NctsToCusEntryHeaderAttacher(entryHeader, integrator.TheOtherCollectionToAttach());
					attacher.Show((ZForm)menuItem.GetMainMenu().GetForm());
				}
			}
		}

		#region ConsolidatedEntryMenuItems

		protected bool IsSubmitEnabledOnConsolidatedDeclaration => ConsolidatedEntryMenuProvider?.SubmitMessageMenuEnabled ?? true;
		protected bool IsQueuedForConsolidation(ZMenuItem menuItem) => ConsolidatedEntryMenuProvider?.IsQueuedForConsolidation(menuItem) ?? false;

		protected ConsolidatedEntryMenuProvider ConsolidatedEntryMenuProvider => consolidatedEntryMenuProvider ?? (consolidatedEntryMenuProvider = GetConsolidatedEntryMenuProvider());
		protected ConsolidatedEntryMenuProvider consolidatedEntryMenuProvider;

		protected virtual ConsolidatedEntryMenuProvider GetConsolidatedEntryMenuProvider() => null;

		#endregion

		protected DialogResult GetConfirmationFromUsers(string caption, string message)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
		}

		ContinueWithSave IBondedWarehouseMenuItemsCreatorSupporter.FireSaveButton()
		{
			return Form.FireSaveButton();
		}

		bool IBondedWarehouseMenuItemsCreatorSupporter.TopLevelBusinessObjectHasChanges()
		{
			var topLevelBusinessObject = TopLevelBusinessObject;
			return topLevelBusinessObject != null && topLevelBusinessObject.HasChanges;
		}

		BondedWarehouseOperationDeterminer IBondedWarehouseMenuItemsCreatorSupporter.GetNewBondedWarehouseOperationDeterminer(IWarehouseIntegrationSupporter supporter)
		{
			return GetNewBondedWarehouseOperationDeterminer(supporter);
		}

		#endregion

		public ZController LastController;
	}
}
