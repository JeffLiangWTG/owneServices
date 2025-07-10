using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI.MenuItems;
using Enterprise.DataTransfer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class CFSLoadListConsolForm : ZTemplateForm
	{
		#region Controls

		ZWorkflowTabPage WorkflowTabPage;
		ZTabPage ArrivalDispatchTabPage;

		#endregion

		public CFSLoadListConsolForm(CFSLoadListConsol loadList)
			: base(loadList)
		{
			InitializeComponent();

			SetupTabPages();
			LoadList = loadList;
			loadList.AutomaticallyUpdatePackLineContainers = CFSDataRegistry.Instance.AutopackContainers.Value;

			HookEvents(loadList);
			SetupConfirmationVisibilty();

			#region Plug Ins

			//			PlugIns.Add(ControllerIDs.ComplexAnyLegs); //will determine state from parent

			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICACustomsDataRegistry>().RNSActive.Value)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.RNSMFLoadListPlugIn);
			}

			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.LoadListContainersPacking, 1);
			PlugIns.Add(ControllerIDs.Routing);
			PlugIns.AddJobInvoicing(loadList.InvoicingSupporter);

			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			PlugIns.Add(ControllerIDs.CartagePlugin);

			#endregion

			if (loadList.JK_IsForwarding)
			{
				WorkflowTabPage.TabVisible = false;
			}
			else
			{
				WorkflowTabPage.Initialize(loadList);
			}

			SetupActionsMenu();

			ConsolDocumentSupporterGuiQueryProvider.Register(loadList.Factory);
			ShipmentDocumentSupporterGuiQueryProvider.Register(loadList.Factory);
			ServicesSelectionGuiProvider.Register(loadList.Factory);
		}

		void SetupActionsMenu()
		{
			ZFormMenuStrategy.AddInterfaceConnectorMenuItems(this, ExportToXmlMenuItem);
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("546B8643-40E8-49f2-94EE-1094D237456B", "Co-Load Wizard"), CoLoadWizardMenuItem_Click);

			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };
		}

		#region Export to XML

		List<MenuItem> ExportToXmlMenuItem
		{
			get
			{
				return new ExportToXmlMenuItemSet<CFSLoadListConsol>(() => Exporter, (CFSLoadListConsol)BusinessEntity);
			}
		}

		IXmlDataTransferExporter Exporter
		{
			get
			{
				var exportor = GetNewXmlDataTransferExporter(new LoadListValueObjectDataAdapter(), true);
				exportor.DefaultFileName = LoadList.JK_UniqueConsignRef + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture);
				if (!(new ZString(SystemDataRegistry.Instance.CFSLoadListConsolDirectory.Value).IsEmpty))
				{
					exportor.InitialDirectory = SystemDataRegistry.Instance.CFSLoadListConsolDirectory.Value;
				}
				return exportor;
			}
		}

#if DEBUG
		protected virtual
#endif
 XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
		{
			return new XmlDataTransferExporter(adapter, checkForLicence);
		}
		#endregion

		void CoLoadWizardMenuItem_Click(object sender, EventArgs e)
		{
			var shipments = GetSelectedShipments();
			if (shipments.Length == 0)
			{
				Globals.Message.ShowInformation(Res.GetString("a5c3fb99-244b-4cbb-82db-355451ed751f", "Please select a Co-Load Master Shipment to run this wizard"), Res.GetString("239b94c0-adc8-4916-a865-4c37efeb397a", "Select a shipment"));
			}
			else if (shipments.Length == 1)
			{
				if (shipments[0].HasChanges)
				{
					Globals.Message.ShowInformation(Res.GetString("cb3356a2-8705-47f1-bc69-0173b0abe1e7", "Please save the current records before running the wizard"), Res.GetString("456359a0-70e4-4362-b221-2ebde2249f3a", "Select a shipment"));
				}
				else if (!shipments[0].IsCoLoadMaster && !shipments[0].IsBlindCoLoadMaster)
				{
					Globals.Message.ShowInformation(Res.GetString("572a4a26-6978-40e4-a24b-51e8450156d7", "The selected shipment is not a co-load Master. Please tick Coload Master on this record to be able to add sub-shipments"),
						Res.GetString("2deb5d9a-3d85-4342-8a0b-57b197c6fc5a", "Select a shipment"));
				}
				else
				{
					RunCoLoadWizard(shipments[0].PK);
				}
			}
			else if (shipments.Length > 1)
			{
				Globals.Message.ShowInformation(Res.GetString("c78a8444-33d6-4f22-9bb9-15a292b40e78", "Please select only one Co-Load shipment to run this wizard"), Res.GetString("b1d51e62-f07a-4214-8e05-9d5a04f4a63c", "Select a shipment"));
			}
		}

#if DEBUG
		internal void CoLoadWizardMenuItem_ClickInternal(object sender, EventArgs e) => CoLoadWizardMenuItem_Click(sender, e);
#endif

		void RunCoLoadWizard(ZGuid shipmentPK)
		{
			var factory2 = new BusinessObjectFactory();
			var coLoadMaster = factory2.Load<PackUnpackShipment>(shipmentPK);
			var coloadShipments = new CoLoadWizardShipment(new BusinessObjectFactory(), coLoadMaster);
			using (var wizardForm = new CoLoadWizardForm(coloadShipments))
			{
#if DEBUG
				if (Globals.IsTest)
				{
					Globals.Message.ShowInformation("CoLoad Wizard would be hit", "CoLoad");
					wizardForm.Show();
				}
				else
#endif
				{
					ZFormModaliser.ShowDialogWithoutDispose(wizardForm);
				}
				if (wizardForm.DialogResult == DialogResult.OK)
				{
					coloadShipments.Factory.Save();
				}
			}
		}

		public CommonShipment[] GetSelectedShipments()
		{
			var result = new ArrayList();
			if (BusinessEntity != null)
			{
				result.AddRange(LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.SelectedElements);
				if (result.Count == 0)
				{
					if (LoadList != null && LoadList.Shipments.Count == 1)
					{
						result.Add(LoadList.Shipments[0]);
					}
				}
			}
			return (CommonShipment[])result.ToArray(typeof(CommonShipment));
		}

		#region Business Object Overrides

		#region FormCaption

		public override string FormCaption
		{
			get { return Res.GetString("735018ac-c37c-457b-baa0-0d3d3676d4a1", "Load List {0}", LoadList.JK_UniqueConsignRef); }
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region OnLoad

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (LoadList != null)
			{
				SetupBasedOnTransportMode(this, EventArgs.Empty);
				LoadList.JK_OA_CTOAddressInfo.RefreshBinding();
				LoadList.SetWarnNotErrorOnLocationTotalsOnPackLines(true);
			}
		}

		#endregion

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (DialogResult.Yes == ConstantsAndReusablesGUI.AskInGUIAboutChangingJobID(LoadList))
			{
				return base.ShowPreSaveDialogs();
			}
			else
			{
				return ContinueWithSave.No;
			}
		}

		#endregion

		#region Implementation

		protected CFSLoadListConsol LoadList;
		protected internal LoadListDetailsUserControl LoadListDetailsUserControl;
		protected internal PickupConfirmControl PickupConfirmControl;
		protected internal DeliveryConfirmControl DeliveryConfirmControl;

		protected void SetupTabPages()
		{
			LoadListDetailsUserControl = new LoadListDetailsUserControl();
			LoadListDetailsUserControl.Dock = DockStyle.Fill;
			MainTabPage.Controls.Add(LoadListDetailsUserControl);

			PickupConfirmControl = new PickupConfirmControl();
			PickupConfirmControl.Dock = DockStyle.Fill;
			ArrivalDispatchTabPage.Controls.Add(PickupConfirmControl);

			DeliveryConfirmControl = new DeliveryConfirmControl();
			DeliveryConfirmControl.Dock = DockStyle.Fill;
			ArrivalDispatchTabPage.Controls.Add(DeliveryConfirmControl);
		}

		#region Event Handlers

		#region Internal Transport Jobs

		//		private void InternalCartageManager_CheckCreateInternalCartageJob(object sender, CancelEventArgs e)
		//		{
		//			if (!e.Cancel)
		//			{
		//				string Caption = FreightConstants.InternalCartageJobCaption);
		//				string Message = FreightConstants.InternalCartageJobQuestion);
		//				DialogResult Result = Globals.Message.Show(Message, Caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		//				e.Cancel = Result == DialogResult.Yes;
		//			}
		//		}
		//
		//		private void InternalCartageManager_CreateCartageError(object sender, CancelEventArgs e)
		//		{
		//			if (!LoadList.InternalCartageManager.CartageCreateError.IsEmpty)
		//			{
		//				Globals.Message.ShowError(LoadList.InternalCartageManager.CartageCreateError, FreightConstants.InternalCartageErrorCaption);
		//			}
		//		}
		//
		//		private void InternalCartageManager_OrgWithRelatedCartageChanged(object sender, CancelEventArgs e)
		//		{
		//			string Message = FreightConstants.InternalCartageJobDelete + System.Environment.NewLine + FreightConstants.InternalCartageJobConfirmCartageChange);
		//			DialogResult Result = InternalCartageQuestion(Message);
		//			e.Cancel = Result == DialogResult.Yes;
		//		}
		//
		//		private void InternalCartageManager_CartageChangedError(object sender, CancelEventArgs e)
		//		{
		//			string Message = FreightConstants.InternalCartageJobDelete + System.Environment.NewLine + FreightConstants.InternalCartageJobDeleteDisAllowed);
		//			Globals.Message.ShowError(Message, FreightConstants.InternalCartageErrorCaption);
		//		}
		//
		//		private DialogResult InternalCartageQuestion(string Message)
		//		{
		//			return Globals.Message.Show(Message, FreightConstants.InternalCartageJobCaption), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
		//		}
		//
		//		private void ViewCartageButton_Click(object sender, System.EventArgs e)
		//		{
		//		
		//			if (LoadList.InternalCartageManager != null)
		//			{
		//				if (!LoadList.HasChanges)
		//				{
		//					JobCartage Cartage = null;
		//					if (LoadList.IsPackLoadList)
		//					{
		//						Cartage = LoadList.InternalCartageManager.GetPickupCartage();
		//					}
		//					else
		//					{
		//						Cartage = LoadList.InternalCartageManager.GetDeliveryCartage();
		//					}
		//					if (Cartage != null)
		//					{
		//						ZController Controller = ZControllerFactory.Create(ControllerIDs.JobCartage);
		//						Controller.ShowEditForm(Cartage);
		//					}
		//					else
		//					{
		//						Globals.Message.ShowError("Cartage Job doesn't exist.");
		//					}
		//				}
		//				else
		//				{
		//					Globals.Message.ShowError("Please save before viewing Cartage Job."));
		//				}
		//			}
		//		}
		//
		//		private void CreateCartageJobButton_Click(object sender, System.EventArgs e)
		//		{
		//			if (LoadList.CartageCo != null)
		//			{
		//				if (LoadList.InternalCartageManager != null)
		//				{
		//					if (LoadList.IsPackLoadList)
		//					{
		//						LoadList.InternalCartageManager.FindOrCreateInternalPickupCartage();
		//					}
		//					else
		//					{
		//						LoadList.InternalCartageManager.FindOrCreateInternalDeliveryCartage();
		//					}
		//				}
		//			}
		//			else
		//			{
		//				Globals.Message.ShowError(FreightConstants.InternalCartageErrorNoOrg);
		//			}
		//		}
		//
		#endregion

		#region SetupBasedOnTransportMode

		void SetupBasedOnTransportMode(object sender, EventArgs e)
		{
			if (LoadList.IsAir)
			{
				LoadListDetailsUserControl.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("BAD16AE9-0936-4ef5-894D-4B9A03736AC0", "Master Bill");
			}
			else
			{
				LoadListDetailsUserControl.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("1E9F9FA9-A8D8-4dfa-B718-9EE2E4542458", "Ocean Bill");
			}
		}

		#endregion

		#region ShowWarningMessage

		void ShowWarningMessage(object sender, EventArgs e)
		{
			if (LoadList.ContinueWithChanging && !LoadList.WarningMessage.IsEmpty)
			{
				var result = Globals.Message.Show(LoadList.WarningMessage, LoadList.MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				LoadList.ContinueWithChanging = (result == DialogResult.Yes);
			}
		}

		#endregion

		#region ShowErrorMessage

		void ShowErrorMessage(object sender, EventArgs e)
		{
			if (LoadList.ContinueWithChanging && !LoadList.ErrorMessage.IsEmpty)
			{
				Globals.Message.ShowError(LoadList.ErrorMessage, LoadList.MessageCaption);
			}
		}

		#endregion

		#endregion

		#endregion

		#region Auto

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (LoadList != null)
				{
					UnHookEvents(LoadList);
					if (LoadList.Job != null)
					{
						LoadList.Job.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}

		#endregion

		#region Hook / Unhook events

		void HookEvents(CFSLoadListConsol loadList)
		{
			loadList.JK_TransportModeInfo.ValueChanged += SetupBasedOnTransportMode;
			loadList.WarningMessageInfo.ValueChanged += ShowWarningMessage;
			loadList.ErrorMessageInfo.ValueChanged += ShowErrorMessage;
			loadList.JK_RL_NKDischargePortInfo.ValueChanged += JK_RL_NKDischargePortInfo_ValueChanged;
			loadList.JK_RL_NKLoadPortInfo.ValueChanged += JK_RL_NKLoadPortInfo_ValueChanged;
			loadList.JK_OA_DepotAddressInfo.ValueChanged += JK_OA_DepotAddressInfo_ValueChanged;
			loadList.JK_IsForwardingInfo.ValueChanged += JK_IsForwardingInfo_ValueChanged;
			loadList.OnRemovingContainerWithPackLines += Containers_RemoveWithPackLines;
		}

		void Containers_RemoveWithPackLines(object sender, CancelEventArgs e)
		{
			string message = Res.GetString("f5833326-29d7-4590-a480-89fd0e1103be", "There are shipments packed into this container.\r\nDetaching the container will unpack the shipments from it.\r\nDo you want to proceed with Detach?");
			string caption = Res.GetString("91b3ceb6-976a-410b-adee-bedffffcc6a5", "Detach Container");
			e.Cancel = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes;
		}

		void JK_RL_NKLoadPortInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupConfirmationVisibilty();
		}

		void JK_RL_NKDischargePortInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupConfirmationVisibilty();
		}

		void JK_OA_DepotAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupConfirmationVisibilty();
		}

		void JK_IsForwardingInfo_ValueChanged(object sender, EventArgs e)
		{
			if (!LoadList.JK_IsForwarding && !((IWorkflowTabPage)WorkflowTabPage).Initialized)
			{
				WorkflowTabPage.Initialize(LoadList);
			}

			WorkflowTabPage.SetVisibility(!LoadList.JK_IsForwarding);
		}

		void SetupConfirmationVisibilty()
		{
			if (LoadList.IsDomestic())
			{
				var depot = LoadList.DepotAddress != null ? LoadList.DepotAddress.Header : null;
				if (depot != null)
				{
					PickupConfirmControl.Visible = depot.OH_RL_NKClosestPort == LoadList.JK_RL_NKLoadPort;
					DeliveryConfirmControl.Visible = depot.OH_RL_NKClosestPort == LoadList.JK_RL_NKDischargePort;
				}
			}
			else
			{
				PickupConfirmControl.Visible = LoadList.IsExport();
				DeliveryConfirmControl.Visible = LoadList.IsImport();
			}
		}

		void UnHookEvents(CFSLoadListConsol loadList)
		{
			loadList.JK_TransportModeInfo.ValueChanged -= new EventHandler(SetupBasedOnTransportMode);
			loadList.WarningMessageInfo.ValueChanged -= new EventHandler(ShowWarningMessage);
			loadList.ErrorMessageInfo.ValueChanged -= new EventHandler(ShowErrorMessage);
			loadList.JK_RL_NKDischargePortInfo.ValueChanged -= new EventHandler(JK_RL_NKDischargePortInfo_ValueChanged);
			loadList.JK_RL_NKLoadPortInfo.ValueChanged -= new EventHandler(JK_RL_NKLoadPortInfo_ValueChanged);
			loadList.JK_OA_DepotAddressInfo.ValueChanged -= new EventHandler(JK_OA_DepotAddressInfo_ValueChanged);
			loadList.JK_IsForwardingInfo.ValueChanged -= JK_IsForwardingInfo_ValueChanged;
			loadList.OnRemovingContainerWithPackLines -= Containers_RemoveWithPackLines;
		}

		#endregion
	}
}
