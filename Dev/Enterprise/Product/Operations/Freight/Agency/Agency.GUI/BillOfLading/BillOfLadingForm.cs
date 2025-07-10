using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.DataTransfer;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.FormExtensions;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingForm : ZTemplateForm, IModuleToModuleForm<BillOfLading>, ISupportSwitchTabPage, IRequireInactivationPrompt, ICustomerServiceMenuSectionCodeOverridable
	{
		public BillOfLadingForm(BillOfLading billOfLading)
			: base(billOfLading)
		{
			InitializeComponent();
			Shipment = billOfLading;

			RegisterMessageValidations(billOfLading);

			PlugIns.Add(ControllerIDs.Routing);
			PlugIns.Add(ControllerIDs.DocAddresses);
			PlugIns.AddJobInvoicing(billOfLading.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.DtbBooking);
			PlugIns.Add(ControllerIDs.DocumentVisualizer);

			workflowTabPage.Initialize(billOfLading);

			InitialiseActionsMenu();

			if (ComplianceRiskHelper.IsLinerAgencyEnabledComplianceWise)
			{
				PlugIns.Add(ControllerIDs.ComplianceRiskPlugin);
			}
		}

		public void SelectAndShowContainer(ZGuid containerPK)
		{
			if (containersTabPage.TabVisible)
			{
				MainTabControl.SelectedTab = containersTabPage;

				var container = (BillOfLadingContainer)Shipment.FCLContainers.FindByPK(containerPK);

				var manager = (CurrencyManager)BindingContext[BusinessEntity, "FCLContainers"];
				var index = manager.List.IndexOf(container);
				if (index >= 0)
				{
					manager.Position = index;
				}
			}
		}

		#region Implementation

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Shipment = null;

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JS_PackingModeInfo_ValueChanged(this, EventArgs.Empty);

			if (FormVerb == FormVerbs.Deactivate)
			{
				PromptForInactivation();
			}
		}

		public override string FormCaption
		{
			get
			{
				if (Shipment == null)
				{
					return base.FormCaption;
				}

				if (Shipment.JS_IsCancelled)
				{
					return Res.GetString("83f3c357-54e8-45ec-911f-3ac34caab48e", "Bill of Lading {0} - Canceled", Shipment.JS_UniqueConsignRef);
				}

				return Res.GetString("30a0a5c8-21d1-47cd-80f7-02412578fe6c", "Bill of Lading {0}", Shipment.JS_UniqueConsignRef);
			}
		}

		protected override void Save(ITransactionParticipant[] factories)
		{
			if (DisplayMode == ODisplayMode.Edit)
			{
				Array.Resize(ref factories, factories.Length + 1);
				factories[factories.Length - 1] = new PackingTransactionParticipant(shipment);
			}

			base.Save(factories);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			try
			{
				var result = ContinueWithSave.No;

				if (Shipment == null)
				{
					return result;
				}

				Shipment.DefaultWeightAndVolumeUnits();
				Shipment.CheckTotalsDiffer();

				if (Shipment.Sailing == null || Shipment.Sailing.Voyage == null)
				{
					result = base.ValidateAndSave();
				}
				else
				{
					using (var mutex = new AgencyAllocationMutex(Shipment.Sailing.Voyage))
					{
						mutex.Lock();
						if (mutex.HasLock)
						{
							try
							{
								result = base.ValidateAndSave();
							}
							finally
							{
								mutex.Unlock();
							}
						}
						else
						{
							var lockInfo = mutex.GetLockInfo();
							var (caption, message, allowRelease) = mutex.GetFriendlyMessage(lockInfo);
							var shouldForceUnlock = Globals.Message.Show(message, caption, allowRelease ? MessageBoxButtons.YesNo : MessageBoxButtons.OK, MessageBoxIcon.Error);

							if (shouldForceUnlock == DialogResult.Yes)
							{
								mutex.ReleaseLocks(lockInfo);
								result = ValidateAndSave();
							}
						}
					}
				}
				return result;
			}
			catch (PackedIntoMultipleContainersException)
			{
				PackedIntoMultipleContainersHandler.Handle(Shipment);
				return ContinueWithSave.No;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (Shipment.BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship && QueryForNewSupplierBuyerRelationship())
			{
				Shipment.BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
			}

			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && Shipment.ShouldEnforceAllocations)
			{
				if (Shipment.Sailing != null)
				{
					var usageBeforeChange = Shipment.LastSavedUsage;
					var usageAfterChange = AllocationUsage.LoadFromShipment(Shipment);

					if (Shipment.JS_JX != Shipment.JS_JX_LastSavedSailing || usageAfterChange.HasAspectExceedingThatFor(usageBeforeChange))
					{
						var set = Shipment.LoadAllocationUsageSet();

						if (set == null)
						{
							Globals.Message.ShowError(Res.GetString("a6bd6868-8c07-4872-ae5e-aae9d586508e", "Allocations are not configured for the current sailing."));
							result = ContinueWithSave.No;
						}
						else if (!set.CanFit(usageAfterChange))
						{
							result = AllocationAdjustmentDialog.ConfirmAdjustAllocations(set, usageAfterChange) ? ContinueWithSave.Yes : ContinueWithSave.No;
						}
					}
				}
			}

			if (result == ContinueWithSave.Yes)
			{
				var containerNumbers = GetContainerNumbersWithChangesAffectingEIDO();

				if (containerNumbers.Count > 5)
				{
					Globals.Message.ShowWarning(Res.GetString("efff939c-5b44-4228-a3c0-2765e3f7963c", "Several containers affected by your changes have previously sent E-IDO messages. You should re-send the E-IDO messages."));
				}
				else if (containerNumbers.Count > 0)
				{
					var builder = new StringBuilder();
					builder.AppendLine(Res.GetString("d7476090-8d90-49f5-816a-b8db7288b3f7", "The following containers affected by your changes have previously sent E-IDO messages. You should re-send the E-IDO messages."));
					builder.AppendLine();

					foreach (var containerNum in containerNumbers)
					{
						builder.Append((NoResString)"\u2022 "); // Character Code
						builder.AppendLine(containerNum);
					}

					Globals.Message.ShowWarning(builder.ToString());
				}
			}

			return result;
		}

		protected override void PopulateDevTools(List<IDevTool> tools)
		{
			base.PopulateDevTools(tools);
			tools.Add(new DetentionAdviceDocumentCustomisationDevTool());
		}

		public override bool IsResizableByTabPageAllowed => true;

		void RegisterMessageValidations(BillOfLading billOfLading)
		{
			var strategies = MessagingValidationStrategyFactory.GetStrategies();

			foreach (var strategy in strategies.Where(x => x.IsEnabled))
			{
				strategy.Register(billOfLading.Factory);
			}
		}

		void HookShipment(BillOfLading bill)
		{
			bill.PackingModeChanging += Shipment_PackingModeChanging;
			bill.JS_PackingModeInfo.ValueChanged += JS_PackingModeInfo_ValueChanged;
			bill.UpdateShipmentTotalsPackQuantityVariation += Shipment_CheckUpdateShipmentTotals;

			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				bill.OnShipmentStatusUpdate += new EventHandler<ShipmentStatusEventArgs>(UpdateShipmentStatus);
			}
		}

		void UnHookShipment(BillOfLading bill)
		{
			bill.PackingModeChanging -= Shipment_PackingModeChanging;
			bill.JS_PackingModeInfo.ValueChanged -= JS_PackingModeInfo_ValueChanged;
			bill.UpdateShipmentTotalsPackQuantityVariation -= Shipment_CheckUpdateShipmentTotals;
			bill.OnShipmentStatusUpdate += new EventHandler<ShipmentStatusEventArgs>(UpdateShipmentStatus);
		}

		void UpdateShipmentStatus(object sender, ShipmentStatusEventArgs e)
		{
			if (e.ShipmentStatus == ShipmentStatusList.Codes.SIRejected)
			{
				var userResponseArgs = new UserResponseArgument
				{
					Caption = Res.GetString("0397F93B-0931-417A-A7F3-20AE246636BC", "Rejection Reason"),
					Message = Res.GetString("51560E3D-78BE-4B85-8FCA-E03439DD9AB1", "Please enter the reason of rejection."),
					Buttons = ZMessageBoxButtons.OKCancel,
					DefaultButton = ZMessageBoxDefaultButton.Button1,
					Icon = ZMessageBoxIcon.Information,
					MinimumResponseLength = 1
				};

				e.StatusUpdatedReason = Globals.Message.QueryUserResponse(userResponseArgs);
			}
		}

		void Shipment_PackingModeChanging(object sender, CancelEventArgsWithMessage e)
		{
			if (!e.Cancel)
			{
				e.Cancel = Globals.Message.Show(e.Message, e.Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No;
			}
		}

		void JS_PackingModeInfo_ValueChanged(object sender, EventArgs e)
		{
			billOfLadingMainPage1.SetForContainerMode(Shipment.JS_PackingMode);

			this.BeginInvoke(new MethodInvoker(() =>
			{
				switch (Shipment.JS_PackingMode)
				{
					case Constants.ContainerModes.FCL:
						VehiclesVisible = false;
						PacksVisible = false;
						ContainersVisible = true;
						break;

					case Constants.ContainerModes.RollOnRollOff:
						PacksVisible = false;
						ContainersVisible = false;
						VehiclesVisible = true;
						break;

					default:
						ContainersVisible = false;
						VehiclesVisible = false;
						PacksVisible = true;
						break;
				}
			}));
		}

		void Shipment_CheckUpdateShipmentTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel && Shipment.JS_PackingMode != Constants.ContainerModes.FCL)
			{
				var caption = Res.GetString("941bb173-8eb5-4257-8811-6c8ba8265961", "Totals do not match");
				string message;
				if (Shipment.JS_PackingMode == Constants.ContainerModes.RollOnRollOff)
				{
					message = Res.GetString("abde675b-83d4-4597-9896-9ca53da9f4b7", "Total vehicles, weight and volume do not match the shipment total. Would you like to update the booking to match the vehicles totals?");
				}
				else
				{
					message = Res.GetString("2440fe56-328e-4c5a-af04-1fbec20f1d4f", "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?");
				}

				var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result == DialogResult.No;
			}
		}

		List<string> GetContainerNumbersWithChangesAffectingEIDO()
		{
			var shipmentChanged = EIDOShipmentMessagingData.HasChangesAffectingEIDO(Shipment);
			var containerNumbers = new List<string>();

			foreach (BillOfLadingContainer container in Shipment.RealContainers)
			{
				if (shipmentChanged || EIDOShipmentMessagingData.HasChangesAffectingEIDO(container))
				{
					bool messageSent;

					switch (container.JC_ImportReleaseOrderStatus)
					{
						case ReleaseImportOrderMessageStatusList.Codes.Withdrawn:
						case ReleaseImportOrderMessageStatusList.Codes.NotSent:
							messageSent = false;
							break;

						default:
							messageSent = true;
							break;
					}

					if (messageSent)
					{
						containerNumbers.Add(container.JC_ContainerNum);
					}
				}
			}
			return containerNumbers;
		}

		void InitialiseActionsMenu()
		{
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItems);

			ZFormMenuStrategy.AddActionsMenuItem(this, new MessagingValidationMenu(this));

			var relatedJobsMenuItem = this.CreateRelatedJobsMainMenuItem<BillOfLadingForm, BillOfLading>();
			relatedJobsMenuItem.Index = 3;

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		bool QueryForNewSupplierBuyerRelationship()
		{
			return Globals.Message.Show(
				Res.GetString("f18cf5b9-ab96-4a90-8344-6815886d45dd", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?"),
				Res.GetString("08c1d62a-56c8-4fba-89f0-acba59f6e749", "Save"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				DialogResult.Yes
				) == DialogResult.Yes;
		}

		List<MenuItem> ExportToXmlMenuItems
		{
			get
			{
				return new ExportToXmlMenuItemSet<BillOfLading>(() => { return Exporter; }, (BillOfLading)BusinessEntity);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				return new XmlDataTransferExporter(new AgencyShipmentValueObjectDataAdapter<BillOfLading>(), true);
			}
		}

		protected BillOfLading Shipment
		{
			[DebuggerStepThrough]
			get { return shipment; }
			set
			{
				if (value != shipment)
				{
					if (shipment != null)
					{
						UnHookShipment(shipment);
					}

					shipment = value;

					if (shipment != null)
					{
						HookShipment(shipment);
					}
				}
			}
		}
		BillOfLading shipment;

		protected bool ContainersVisible
		{
			[DebuggerStepThrough]
			get { return containersVisible; }
			set
			{
				if (containersVisible != value)
				{
					containersVisible = value;
					containersTabPage.TabVisible = ContainersVisible;
					bookedContainersTabPage.TabVisible = ContainersVisible;
				}
			}
		}
		bool containersVisible = true;

		protected bool VehiclesVisible
		{
			[DebuggerStepThrough]
			get { return vehiclesVisible; }
			set
			{
				if (vehiclesVisible != value)
				{
					vehiclesVisible = value;
					vehiclesTabPage.TabVisible = VehiclesVisible;
				}
			}
		}
		bool vehiclesVisible = true;

		protected bool PacksVisible
		{
			[DebuggerStepThrough]
			get { return packsVisible; }
			set
			{
				if (packsVisible != value)
				{
					packsVisible = value;
					packsTabPage.TabVisible = PacksVisible;
				}
			}
		}
		bool packsVisible = true;

		#endregion

		#region IRequireInactivationPrompt Members

		public void PromptForInactivation()
		{
			if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value)
			{
				if (Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.SIRejected && Shipment.IsReceivedElectronicShippingInstruction())
				{
						var dialogContext = new DialogDefaultContext(
						new ZGuid("19C37B57-AD79-4A57-A197-F6C8F40FDCDD"),
						ResString.GetMultilingualString("79D005B1-A895-4884-8596-D907A30BC90C", "Bill Of Lading Rejection Message"),
						ZMessageBoxButtons.YesNo,
						ZMessageBoxIcon.Question,
						null,
						showCheckboxOnly: true
					);

					var dialogResult = Globals.Message.ShowOrDefault(dialogContext,
						ResString.GetMultilingualString("05816EE6-ABFB-470A-B160-706D7A9E7658", "This Bill Of Lading was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?"));

					if (dialogResult == ZDialogResult.Yes)
					{
						Shipment.LogStatusChangedEvent(ShipmentStatusList.Codes.SIRejected, (NoResString)"Bill Of Lading Cancelled"); // Event Parameter Constant.
					}
				}
				else if (Shipment.JS_ShipmentStatus != ShipmentStatusList.Codes.SIRejected)
				{
					Shipment.LogStatusChangedEvent(ShipmentStatusList.Codes.SIRejected, (NoResString)"Bill Of Lading Cancelled"); // Event Parameter Constant.
				}

				using (Shipment.SuspendAutomaticCreationOfStatusChangedLog())
				{
					Shipment.JS_ShipmentStatus = ShipmentStatusList.Codes.SIRejected;
				}

				DeleteUselessShipmentStatusUpdateLogsForInactivation();
			}
		}

		void DeleteUselessShipmentStatusUpdateLogsForInactivation()
		{
			var existingNonBillOfLadingRejectedShipmentStatusUpdateLogsNotInDB = Shipment.Logs.LogsNotInDB.OfType<StmALog>().Where(x => x.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode && !x.SL_IsCancelled
				&& x.Parameters.TryGetValue(Params.Type, out var type) && type == Constants.EventReferenceMessageTypes.ShipmentStatus
				&& x.Parameters.TryGetValue(Params.New, out var newShipmentStatus) && newShipmentStatus != ShipmentStatusList.Codes.SIRejected);

			if (existingNonBillOfLadingRejectedShipmentStatusUpdateLogsNotInDB.Any())
			{
				existingNonBillOfLadingRejectedShipmentStatusUpdateLogsNotInDB.DeleteAll();
			}
		}

		#endregion

		#region IModuleToModuleForm Members

		bool IModuleToModuleForm<BillOfLading>.HasChanges
		{
			get { return Shipment != null && Shipment.HasChanges; }
		}

		ZString IModuleToModuleForm<BillOfLading>.ErrorMessage
		{
			get { return Res.GetString("C66FEB08-C8EC-4624-8733-EEBF8B4C804E", "You must save this Bill Of Lading before creating a related Job."); }
		}

		ZString IModuleToModuleForm<BillOfLading>.RelatedEntityName
		{
			get { return Res.GetString("F2D3CB70-5202-49BF-A89A-6BF3ECCB9EAF", "Declaration"); }
		}

		ZString IModuleToModuleForm<BillOfLading>.RelatedEntityDescription
		{
			get { return ZString.Empty; }
		}

		ResourceString IModuleToModuleForm<BillOfLading>.TopLevelMenuItemCaption
		{
			get { return ResString.GetMultilingualString("569B48D4-5168-4D8F-8CC8-F29E29FB8283", "Related Jobs"); }
		}

		ResourceString IModuleToModuleForm<BillOfLading>.MainMenuItemCaption
		{
			get { return ResString.GetMultilingualString("20CB8861-9484-47F7-95A8-C144298393CB", "Declarations"); }
		}

		BillOfLading IModuleToModuleForm<BillOfLading>.BusinessEntity
		{
			get { return Shipment; }
		}

		ControllerID IModuleToModuleForm<BillOfLading>.ControllerID
		{
			get { return ControllerIDs.Customs.JobDeclaration; }
		}

		ResourceString IModuleToModuleForm<BillOfLading>.WasExportedNotificationMessage
		{
			get { return ResString.GetMultilingualString("7D18EF6B-AC5A-4A17-8C94-38B60B4AC36A", ""); }
		}

		IModuleToModuleSender IModuleToModuleForm<BillOfLading>.GetModuleToModuleSender()
		{
			return ObjectFactory.Get<IDeclarationModuleToModuleSender>();
		}

		#endregion

		#region ISupportSwitchTabPage

		public void SwitchTabPage(string tabPageName)
		{
			var tabPage = MainTabControl.GetTabPage(tabPageName);
			if (tabPage != null)
			{
				MainTabControl.SelectedTab = tabPage;
			}
		}

		#endregion

		#region ICustomerServiceMenuSectionCodeOverridable

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode
			=> MainTabControl.SelectedTab.Name == ComplianceWiseConstants.ComplianceRiskTabPageName
			? ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise
			: ModuleTreeCustomerServiceMenuSectionList.Codes.LinerAndAgency;

		#endregion ICustomerServiceMenuSectionCodeOverridable
	}
}

#region Test
#if DEBUG

#region Test Methods

namespace Enterprise.Freight.Agency.GUI
{
	partial class BillOfLadingForm
	{
		public class TestHelper
		{
			readonly BillOfLadingForm fForm;

			public TestHelper(BillOfLadingForm form)
			{
				fForm = form;
			}

			public bool ContainersVisible
			{
				get { return fForm.ContainersVisible; }
				set { fForm.ContainersVisible = value; }
			}

			public bool VehiclesVisible
			{
				get { return fForm.VehiclesVisible; }
				set { fForm.VehiclesVisible = value; }
			}

			public bool PacksVisible
			{
				get { return fForm.PacksVisible; }
				set { fForm.PacksVisible = value; }
			}

			public bool ContainersTabIsInTheTabController
			{
				get { return fForm.MainTabControl.TabPages.Contains(fForm.containersTabPage); }
			}

			public bool VehiclesTabInTheTabController
			{
				get { return fForm.MainTabControl.TabPages.Contains(fForm.vehiclesTabPage); }
			}

			public bool PacksTabInTheTabController
			{
				get { return fForm.MainTabControl.TabPages.Contains(fForm.packsTabPage); }
			}

			public int ContainerTabIndex
			{
				get { return fForm.MainTabControl.TabPages.IndexOf(fForm.containersTabPage); }
			}

			public int VehicleTabIndex
			{
				get { return fForm.MainTabControl.TabPages.IndexOf(fForm.vehiclesTabPage); }
			}

			public ZTabPage ContainersTabPage
			{
				get { return fForm.containersTabPage; }
			}

			public ZTabPage VehicleTabPage
			{
				get { return fForm.vehiclesTabPage; }
			}

			public ZTabPage PacksTabInTabController
			{
				get { return fForm.packsTabPage; }
			}

			public int PacksTabIndex
			{
				get { return fForm.MainTabControl.TabPages.IndexOf(fForm.packsTabPage); }
			}
		}
	}
}

#endregion



#endif
#endregion
