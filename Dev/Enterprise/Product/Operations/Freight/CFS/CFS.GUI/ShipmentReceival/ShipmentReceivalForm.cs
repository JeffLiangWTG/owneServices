using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class ShipmentReceivalForm : ZTemplateForm
	{
		public ShipmentReceivalForm(CFSShipment shipment)
			: base(shipment)
		{
			InitializeComponent();

			SetupTabPages();
			Shipment = shipment;

			#region Plug Ins

			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.CA.ICACustomsDataRegistry>().RNSActive.Value)
			{
				PlugIns.Add(ControllerIDs.Customs.CA.RNSCFSShipmentPlugIn);
			}

			PlugIns.AddJobInvoicing(shipment.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);

			PlugIns.Add(ControllerIDs.CartagePlugin);

			#endregion

			if (shipment.JS_IsForwardRegistered)
			{
				WorkflowTabPage.TabVisible = false;
			}
			else
			{
				WorkflowTabPage.Initialize(shipment);
			}

			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("376DE77E-D498-4e4a-9F99-2FAF2B72CB23", "Delivery Order Handed Over"), new EventHandler(this.DeliveryOrderHandedOver_Click));
			ActionsMenuItem.Popup += (s, e) => { ActionsMenuItemsHelper.DisableActionMenuItemsExcludingDefaultsInViewMode(this); };

			ShipmentDetails.DescriptionNotesButton.NoteHasChangesChanged += new ZStmNotePopupForm.NoteHasChangesEventHandler(DescriptionNotesButton_NoteHasChangesChanged);
			AddDataMenuItem();

			SetupPickupDelivery();
			HookShipment();

			ShipmentDocumentSupporterGuiQueryProvider.Register(Shipment.Factory);
			ServicesSelectionGuiProvider.Register(Shipment.Factory);
		}

		void SetupPickupDelivery()
		{
			ShipmentArrivalCFSConfirmControl shipmentArrivalCFSConfirmControl = new ShipmentArrivalCFSConfirmControl();
			shipmentArrivalCFSConfirmControl.Dock = DockStyle.Fill;
			ArrivalTabPage.Controls.Add(shipmentArrivalCFSConfirmControl);
			SetupConfirmationVisibilty();
		}

		#region Events

		protected void DeliveryOrderHandedOver_Click(object sender, EventArgs e)
		{
			ZFormModaliser.Show(NewRaiseEventLogForm(ZArchitecture.Business.Events.DeliveryOrderHandedOver), this);
		}

		protected internal ZStmALogAddForm NewRaiseEventLogForm(Event @event)
		{
			var view = new StmALogCollectionView(Shipment);
			var form = new ZStmALogAddForm(view, Shipment.HasChanges);
			var newLog = (BaseStmALog)form.BusinessEntity;
			newLog.SL_SE_NKEvent = @event.Code;
			return form;
		}

		#endregion

		#region OWinForm Override

		public override string FormCaption
		{
			get { return Res.GetString("e1ace6cb-0b70-45da-8535-8f898bfbba32", "Shipment {0}", Shipment.JS_UniqueConsignRef); }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			Consols_CountChanged(this, new CollectionCountChangedEventArgs(false, null));
			JS_TransportModeInfo_ValueChanged(this, null);
			if (Shipment.Consols.Count > 0)
			{
				ShipmentDetails.SelectSailingButton.Enabled = false;
			}
			Shipment.CheckClientAndSailingAreDefaultedFromLoadList();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			Shipment.CheckTotalsDiffer();

			return base.ValidateAndSave();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			if (DialogResult.Yes == ConstantsAndReusablesGUI.AskInGUIAboutChangingJobID(Shipment))
			{
				return base.ShowPreSaveDialogs();
			}
			else
			{
				return ContinueWithSave.No;
			}
		}

		public override bool IsResizableByTabPageAllowed => true;

		#endregion

		#region Hook / Unhook Shipment

		void HookShipment()
		{
			Shipment.UpdateShipmentTotalsPackQuantityVariation += Shipment_CheckUpdateShipmentTotals;
			Shipment.Consols.CountChanged += Consols_CountChanged;
			Shipment.JS_TransportModeInfo.ValueChanged += JS_TransportModeInfo_ValueChanged;
			Shipment.OnlyShipmentInConsol += new CommonShipment.OnlyShipmentInConsolEventHandler(Shipment_OnlyShipmentInConsol);

			Shipment.JS_RL_NKOriginInfo.ValueChanged += JS_RL_NKOriginInfo_ValueChanged;
			Shipment.JS_RL_NKDestinationInfo.ValueChanged += JS_RL_NKDestinationInfo_ValueChanged;
			Shipment.JS_IsForwardRegisteredInfo.ValueChanged += JS_IsForwardRegisteredInfo_ValueChanged;
			Shipment.ShipmentTypeChanging += Shipment_TypeChanging;

			if (GlbCompany.CurrentCompany.Country.Code == Core.Constants.CountryCodes.Canada)
			{
				ShipmentLinkingMessagesSupporter = ObjectFactory.Get<Enterprise.Integration.Customs.CA.IShipmentLinkingMessagesSupporterProvider>().Create(Shipment);
				ShipmentLinkingMessagesSupporter.HookShipment();
			}
		}

		void JS_RL_NKDestinationInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupConfirmationVisibilty();
		}

		void JS_RL_NKOriginInfo_ValueChanged(object sender, EventArgs e)
		{
			SetupConfirmationVisibilty();
		}

		void JS_IsForwardRegisteredInfo_ValueChanged(object sender, EventArgs e)
		{
			WorkflowTabPage.TabVisible = !Shipment.JS_IsForwardRegistered;
			if (!Shipment.JS_IsForwardRegistered && !((IWorkflowTabPage)WorkflowTabPage).Initialized)
			{
				WorkflowTabPage.Initialize(Shipment);
			}
		}

		void SetupConfirmationVisibilty()
		{
			ArrivalTabPage.TabVisible = Shipment.IsExport() && !Shipment.IsImport();
		}

		#region Shipment_TypeChanging

		void Shipment_TypeChanging(CommonShipment sender, ShipmentTypeChangingCancelEventArgs e)
		{
			ShipmentTypeChangingHandler.Handle(sender, e);
		}

		#endregion

		void UnHookShipment()
		{
			Shipment.UpdateShipmentTotalsPackQuantityVariation -= Shipment_CheckUpdateShipmentTotals;
			Shipment.Consols.CountChanged -= Consols_CountChanged;
			Shipment.JS_TransportModeInfo.ValueChanged -= JS_TransportModeInfo_ValueChanged;
			Shipment.JS_IsForwardRegisteredInfo.ValueChanged -= JS_IsForwardRegisteredInfo_ValueChanged;
			Shipment.ShipmentTypeChanging -= Shipment_TypeChanging;
			Shipment.OnlyShipmentInConsol -= new CommonShipment.OnlyShipmentInConsolEventHandler(Shipment_OnlyShipmentInConsol);

			if (ShipmentLinkingMessagesSupporter != null)
			{
				ShipmentLinkingMessagesSupporter.UnHookShipment();
			}
		}

		#endregion

		ZTabPage ArrivalTabPage;
		Enterprise.Integration.Customs.CA.IShipmentLinkingMessagesSupporter ShipmentLinkingMessagesSupporter;

		#region Implementation

		protected CFSShipment Shipment;

		#region NotesPopupButtons

		void DescriptionNotesButton_NoteHasChangesChanged()
		{
			Shipment.JS_GoodsDescriptionInfo.RefreshBinding();
		}

		#endregion

		#region Events

		#region SelectSailingButton_Click

		void SelectSailingButton_Click(object sender, EventArgs e)
		{
			SailingIFindBox helper = new SailingIFindBox(Shipment, this);
			helper.ShowModuleFromShipment();
		}

		#endregion

		void Shipment_CheckDefaultDetailsFromLoadList(object sender, CancelEventArgs e)
		{
			string message = Res.GetString("28f66b6e-cfe6-4885-962d-643802207407", "The Client details need to be defaulted from the Load List.\r\nDo you want this to be done automatically now?");
			string caption = Res.GetString("8eb3a649-f061-4f2d-ac8f-2a9ea9571049", "Default from Load List");

			DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
			e.Cancel = result == DialogResult.No;
		}

		#region ShowWarningMessage

		void ShowWarningMessage(object sender, EventArgs e)
		{
			if (Shipment.ContinueWithChanging && !Shipment.WarningMessage.IsEmpty)
			{
				DialogResult result = Globals.Message.Show(Shipment.WarningMessage, Shipment.MessageCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				Shipment.ContinueWithChanging = (result == DialogResult.Yes);
			}
		}

		#endregion

		#region ShowErrorMessage

		void ShowErrorMessage(object sender, EventArgs e)
		{
			if (Shipment.ContinueWithChanging && !Shipment.ErrorMessage.IsEmpty)
			{
				Globals.Message.ShowError(Shipment.ErrorMessage, Shipment.MessageCaption);
			}
		}

		#endregion

		#region Shipment_CheckUpdateShipmentTotals

		void Shipment_CheckUpdateShipmentTotals(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				string caption = Res.GetString("81f9ee6b-f26d-493d-ac62-5d09b3b90c9a", "Totals do not match");
				string message = Res.GetString("7fd73f30-b070-43b1-8729-c9bc62926169", "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?");
				DialogResult result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result == DialogResult.No;
			}
		}

		#endregion

		#region Consols_CountChanged

		void Consols_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (Shipment.Consols.Count == 0)
			{
				ShipmentDetails.ShipmentSailingPanel.Visible = true;
				ShipmentDetails.LoadListSailingPanel.Visible = false;
			}
			else
			{
				ShipmentDetails.ShipmentSailingPanel.Visible = false;
				ShipmentDetails.LoadListSailingPanel.Visible = true;
			}
		}

		#endregion

		#region JS_TransportModeInfo_ValueChanged

		void JS_TransportModeInfo_ValueChanged(object sender, EventArgs e)
		{
			ZString journeyScheduleResString = Res.GetString("88502760-779F-4022-8AF4-E2FF9D5C5F2A", "Journey Schedule");
			ZString oceanBillNoResString = Res.GetString("0290BD01-B676-4bbe-A2D6-6E1D33FB22C5", "Ocean Bill No");

			if (Shipment.JS_TransportMode == Constants.TransportModes.Air)
			{
				ShipmentDetails.VoyageNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("1DD7F6A1-78B3-44e8-865A-CBA175B6D0B7", "Flight");
				ShipmentDetails.VesselNameTextBox.Visible = false;
				ShipmentDetails.SailingGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("9F52E46C-7905-4b72-ABCB-2FB9C728C87A", "Flight Schedule");
				ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("D89C36C8-6EF5-45b5-B9D8-9BD846F60AAB", "Master Bill No");
			}
			else if (Shipment.JS_TransportMode == Constants.TransportModes.Rail)
			{
				ShipmentDetails.VoyageNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("47FC94CE-E4EA-4544-ABF0-5BF82B10B9BA", "Jrny. No.");
				ShipmentDetails.VesselNameTextBox.Visible = true;
				ShipmentDetails.VesselNameTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("78BCC88A-218B-4c70-BBEB-6A583DB1C05B", "Journey");
				ShipmentDetails.SailingGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = journeyScheduleResString;
				ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
			else if (Shipment.JS_TransportMode == Constants.TransportModes.Road)
			{
				ShipmentDetails.VoyageNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("0CC9C2F9-8EF3-47e4-B7A2-05D24DD5D149", "Truck");
				ShipmentDetails.VesselNameTextBox.Visible = false;
				ShipmentDetails.SailingGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = journeyScheduleResString;
				ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
			else
			{
				ShipmentDetails.VoyageNoTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("92BB5D82-DE33-48c9-805E-CB6FD680BCCC", "Voyage");
				ShipmentDetails.VesselNameTextBox.Visible = true;
				ShipmentDetails.VesselNameTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("B3908042-8D83-40ff-87B6-23961667AE27", "Vessel");
				ShipmentDetails.SailingGroupBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("3AA2280D-225B-40fd-81D8-30D4ED48BD8A", "Sailing Schedule");
				ShipmentDetails.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption = oceanBillNoResString;
			}
		}

		#endregion

		#region OnlyShipmentInConsol

		void Shipment_OnlyShipmentInConsol(CommonShipment.OnlyShipmentInConsolEventArgs args)
		{
			var response = Globals.Message.Show(args.Message, Res.GetString("d5a8babd-bcc4-4427-9b2f-12290f7deca7", "AutoRating"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (response == DialogResult.No)
			{
				args.Cancel = true;
			}
		}

		#endregion

		#endregion

		#region Additional Menu Item Constructions

		protected void AddDataMenuItem()
		{
			DataMenuItem menuItem = new DataMenuItem();
			if (this.MainMenu.MenuItems.Count > 1)
			{
				this.MainMenu.MenuItems.Add(this.MainMenu.MenuItems.Count - 2, menuItem);
			}
			else
			{
				this.MainMenu.MenuItems.Add(0, menuItem);
			}

			menuItem.Shipment = this.Shipment;
			menuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("337B82D9-8193-44f4-BF9B-271F749FA25D", "Make Visible"), new EventHandler(MakeVisible_Click)));
			menuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("49AEDE06-58B1-476a-B3E8-A22286DDD8A0", "Make Invisible"), new EventHandler(MakeInVisible_Click)));
		}

		void MakeVisible_Click(object sender, EventArgs e)
		{
			ShipmentDetails.ClientOrganisationControl.Visible = true;
			ShipmentDetails.LeftPanel.Visible = true;
		}

		void MakeInVisible_Click(object sender, EventArgs e)
		{
			ShipmentDetails.ClientOrganisationControl.Visible = false;
			ShipmentDetails.LeftPanel.Visible = false;
		}

		#endregion

		public ShipmentDetails ShipmentDetails;
		void SetupTabPages()
		{
			ShipmentDetails = new ShipmentDetails();
			ShipmentDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			MainTabPage.Controls.Add(ShipmentDetails);
			ShipmentDetails.SelectSailingButton.Click += new EventHandler(SelectSailingButton_Click);
		}

		#endregion

		#region Auto

		ZWorkflowTabPage WorkflowTabPage;

		protected override void Dispose(bool disposing)
		{
			if (Shipment != null && !Shipment.IsDeleted)
			{
				UnHookShipment();
			}

			base.Dispose(disposing);
		}
		#endregion
	}
}
