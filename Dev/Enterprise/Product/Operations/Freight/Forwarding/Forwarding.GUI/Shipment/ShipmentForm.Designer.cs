using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using DataTransfer.Common.GUI.MenuItems;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.Customs.Common;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.HelperClasses;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Common.TemplateRecords;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using Constants = Enterprise.Core.Constants;
using FreightRegistry = Enterprise.Registry.Business.FreightDataRegistry;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentForm : ZForm,
		Integration.Forwarding.IForwardingShipmentForm,
		ITabVisibilityDeciderPersistence,
		INotifications,
		IRequireInactivationPrompt,
		ISupportSwitchTabPage
	{
		protected ZTabPage DeliveryTabPage;
		protected ZTabPage PickupTabPage;
		ZTabPage RelatedShipmentsTabPage;
		ZPanel RelatedShipmentsPanel;
		RelatedShipmentsControl RelatedShipmentsz;
		ShipmentBasicRegistrationControl fShipmentUserControl;
		protected ZTabPage AdditionalTabPage;
		ZTabPage BookingDetailsTabPage;
		BookingDetailsControl BookingDetailsControl;
		protected ShipmentAdditionalDetailsControl shipmentAdditionalDetailsControl;
		protected ZTemplateTabControl MainTabControl;
		protected ZTabPage ShipmentDetailsTabPage;
		ZTabPage ContainerDetailsTabPage;
		ZPanel BottomPanel;
		ZPreviousNextControl PreviousNextControlForDesigner;
		ZPanel PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel;
		ContainersUserControl ContainersUserControl;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		ZStmNoteTabPage NotesTabPage;
		protected ZLogsTabPage EventTabPage;
		ZWorkflowTabPage WorkflowTabPage;
		ZTabPage ElectronicMessagingTabPage;
		protected ZTabControl ElectronicMessagingTabControl;
		internal ZButton ViewShipmentTrackingButton;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
        new internal void InitializeComponent()
        {
			this.components = new Container();
			this.BottomPanel = new ZPanel();
			this.PreviousNextControlForDesigner = new ZPreviousNextControl();
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = new ZPanel();
			this.PostingButtonsUserControl = new Core.Forms.ZPostingButtonsUserControl();
			this.MainTabControl = new ZTemplateTabControl();
			this.ShipmentDetailsTabPage = new ZTabPage();
			this.AdditionalTabPage = new ZTabPage();
			this.BookingDetailsTabPage = new ZTabPage();
			this.RelatedShipmentsTabPage = new ZTabPage();
			this.ContainerDetailsTabPage = new ZTabPage();
			this.PickupTabPage = new ZTabPage();
			this.DeliveryTabPage = new ZTabPage();
			this.WorkflowTabPage = new Enterprise.Freight.GUI.ForwardingShipmentWorkflowTabPage();
			this.ElectronicMessagingTabPage = new ZTabPage();
			this.ElectronicMessagingTabControl = new ZTabControl();
			this.NotesTabPage = new ZStmNoteTabPage();
			this.EventTabPage = new ZLogsTabPage();
			this.ViewShipmentTrackingButton = new ZButton();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.ElectronicMessagingTabPage.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.SuspendLayout();
			this.RememberSplitterLayout = false;
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 653, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(1001);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel);
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Controls.Add(this.PreviousNextControlForDesigner);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 624, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 29, true);
			this.BottomPanel.TabIndex = 25;
			//
			// PreviousNextControlForDesigner
			//
			this.PreviousNextControlForDesigner.AllowDrop = true;
			this.PreviousNextControlForDesigner.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 3, true);
			this.PreviousNextControlForDesigner.Name = "PreviousNextControlForDesigner";
			this.PreviousNextControlForDesigner.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(313, 26, true);
			this.PreviousNextControlForDesigner.TabIndex = 27;
			this.PreviousNextControlForDesigner.Visible = false;
			//
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			//
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.ViewShipmentTrackingButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(311, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Name = "PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel";
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 30, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 28;
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = false;
			//
			// PostingButtonsUserControl
			//
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(753, 3, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 26;
			//
			// MainTabControl
			//
			this.MainTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.ShipmentDetailsTabPage);
			this.MainTabControl.Controls.Add(this.AdditionalTabPage);
			this.MainTabControl.Controls.Add(this.BookingDetailsTabPage);
			this.MainTabControl.Controls.Add(this.RelatedShipmentsTabPage);
			this.MainTabControl.Controls.Add(this.ContainerDetailsTabPage);
			this.MainTabControl.Controls.Add(this.PickupTabPage);
			this.MainTabControl.Controls.Add(this.DeliveryTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.Add(this.ElectronicMessagingTabPage);
			this.MainTabControl.Controls.Add(this.NotesTabPage);
			this.MainTabControl.Controls.Add(this.EventTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.Secured = true;
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 624, true);
			this.MainTabControl.TabIndex = 25;
			//
			// ShipmentDetailsTabPage
			//
			this.ShipmentDetailsTabPage.AutoScroll = true;
			this.ShipmentDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 680, true);
			this.ShipmentDetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|b0000cd2-b6c2-41fb-bdab-b07824806e77", "Basic Registration");
			this.ShipmentDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShipmentDetailsTabPage.Name = "ShipmentDetailsTabPage";
			this.ShipmentDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.ShipmentDetailsTabPage.TabIndex = 0;
			//
			// AdditionalTabPage
			//
			this.AdditionalTabPage.AutoScroll = true;
			this.AdditionalTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 586, true);
			this.AdditionalTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|5dc4cab5-8904-403b-9ee6-e0c564da1bd9", "Additional Detail");
			this.AdditionalTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalTabPage.Name = "AdditionalTabPage";
			this.AdditionalTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.AdditionalTabPage.TabIndex = 12;
			this.AdditionalTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.AdditionalTabPage_InitializeTab));
			//
			// BookingDetailsTabPage
			//
			this.BookingDetailsTabPage.AutoScroll = true;
			this.BookingDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.BookingDetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|3a7ddb97-9a3a-428f-a3a3-f8bfe7a21e7a", "Booking Details");
			this.BookingDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BookingDetailsTabPage.Name = "BookingDetailsTabPage";
			this.BookingDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.BookingDetailsTabPage.TabIndex = 16;
			this.BookingDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.BookingDetailsTabPage_InitializeTab));
			//
			// RelatedShipmentsTabPage
			//
			this.RelatedShipmentsTabPage.AutoScroll = true;
			this.RelatedShipmentsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.RelatedShipmentsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|853a463a-1f61-4477-903d-50aef1273673", "Related Shipments");
			this.RelatedShipmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedShipmentsTabPage.Name = "RelatedShipmentsTabPage";
			this.RelatedShipmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.RelatedShipmentsTabPage.TabIndex = 15;
			this.RelatedShipmentsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.RelatedShipmentsTabPage_InitializeTab));
			//
			// ContainerDetailsTabPage
			//
			this.ContainerDetailsTabPage.AutoScroll = true;
			this.ContainerDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1330, 586, true);
			this.ContainerDetailsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|86b0fb35-c778-41e5-9b7a-09155dfc0429", "Packing");
			this.ContainerDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerDetailsTabPage.Name = "ContainerDetailsTabPage";
			this.ContainerDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 10, 8, 8, true);
			this.ContainerDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.ContainerDetailsTabPage.TabIndex = 5;
			this.ContainerDetailsTabPage.RunWhenBindingOrFirstShown(new EventHandler(this.ContainerDetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CommonShipment)(((ForwardingShipment)(null)))));
			//
			// PickupTabPage
			//
			this.PickupTabPage.AutoScroll = true;
			this.PickupTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.PickupTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|67f353c9-143b-4393-93b0-5a7c21b4195b", "Pickup");
			this.PickupTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PickupTabPage.Name = "PickupTabPage";
			this.PickupTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.PickupTabPage.TabIndex = 13;
			//
			// DeliveryTabPage
			//
			this.DeliveryTabPage.AutoScroll = true;
			this.DeliveryTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.DeliveryTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|0bd7cad9-4c12-4bf6-ad68-9c3a7c7b2a32", "Delivery");
			this.DeliveryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeliveryTabPage.Name = "DeliveryTabPage";
			this.DeliveryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.DeliveryTabPage.TabIndex = 14;
			//
			// WorkflowTabPage
			//
			this.WorkflowTabPage.AutoScroll = true;
			this.WorkflowTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.WorkflowTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|6421b3f2-31a5-4d8e-8239-c7337ee242cc", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.WorkflowTabPage.TabIndex = 11;
			//
			// ElectronicMessagingTabPage
			//
			this.ElectronicMessagingTabPage.AutoScroll = true;
			this.ElectronicMessagingTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.ElectronicMessagingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.ElectronicMessagingTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|3476085d-caec-4248-9f17-b923c04a5c71", "Electronic Messages");
			this.ElectronicMessagingTabPage.Controls.Add(this.ElectronicMessagingTabControl);
			this.ElectronicMessagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ElectronicMessagingTabPage.Name = "ElectronicMessagingTabPage";
			this.ElectronicMessagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ElectronicMessagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 591, true);
			this.ElectronicMessagingTabPage.TabIndex = 9;
			//
			// ElectronicMessagingTabControl
			//
			this.ElectronicMessagingTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ElectronicMessagingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ElectronicMessagingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ElectronicMessagingTabControl.Name = "ElectronicMessagingTabControl";
			this.ElectronicMessagingTabControl.SelectedIndex = 0;
			this.ElectronicMessagingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 585, true);
			this.ElectronicMessagingTabControl.TabIndex = 0;
			//
			// NotesTabPage
			//
			this.NotesTabPage.AutoScroll = true;
			this.NotesTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.NotesTabPage.Name = "NotesTabPage";
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.NotesTabPage.TabIndex = 8;
			//
			// EventTabPage
			//
			this.EventTabPage.AutoScroll = true;
			this.EventTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 586, true);
			this.EventTabPage.ExcludeFromBindingOnSave = true;
			this.EventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EventTabPage.Name = "EventTabPage";
			this.EventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.EventTabPage.TabIndex = 10;
			//
			// ViewShipmentTrackingButton
			//
			this.ViewShipmentTrackingButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentForm|352c0ddb-aea8-429a-a884-0d0e5c6da2e5", "Shipment Visibility");
			this.ViewShipmentTrackingButton.Image = Icons.GetImage(IconTypes.ViewButtonActive);			
			this.ViewShipmentTrackingButton.TextImageRelation = TextImageRelation.ImageBeforeText;
			this.ViewShipmentTrackingButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ViewShipmentTrackingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.ViewShipmentTrackingButton.Name = "ViewShipmentTrackingButton";
			this.ViewShipmentTrackingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.ViewShipmentTrackingButton.TabIndex = 29;
			this.ViewShipmentTrackingButton.AllowDrop = true;
			this.ViewShipmentTrackingButton.Click += new EventHandler(this.ViewShipmentTrackingButton_Click);
			//
			// ShipmentForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 677, true);
			this.Controls.Add(this.MainTabControl);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(ForwardingShipment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1360, 725, true);
			this.Name = "ShipmentForm";
			this.SecurityToken = "ShipmentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "ShipmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();

			this.ShipmentDetailsTabPage.ResumeLayout(false);
			this.ShipmentDetailsTabPage.PerformLayout();
			this.DeliveryTabPage.ResumeLayout(false);
			this.DeliveryTabPage.PerformLayout();
			this.EventTabPage.ResumeLayout(false);
			this.EventTabPage.PerformLayout();

			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.ElectronicMessagingTabPage.ResumeLayout(false);
			this.ElectronicMessagingTabPage.PerformLayout();

			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();

			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void ContainerDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			this.ContainersUserControl = new ContainersUserControl();
			this.ContainersUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainersUserControl, ".");
			this.ContainersUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.ContainersUserControl.Name = "ContainersUserControl";
			this.ContainersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 579, true);
			this.ContainersUserControl.TabIndex = 0;
			this.ContainersUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 18, true);

			this.ContainerDetailsTabPage.ResumeLayout(false);
			this.ContainerDetailsTabPage.PerformLayout();

			this.ContainerDetailsTabPage.Controls.Add(this.ContainersUserControl);
		}

		private void AdditionalTabPage_InitializeTab(object sender, EventArgs e)
		{
			this.shipmentAdditionalDetailsControl = new ShipmentAdditionalDetailsControl();
			this.shipmentAdditionalDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.shipmentAdditionalDetailsControl, ".");
			this.shipmentAdditionalDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.shipmentAdditionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.shipmentAdditionalDetailsControl.Name = "shipmentAdditionalDetailsControl";
			this.shipmentAdditionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 591, true);
			this.shipmentAdditionalDetailsControl.TabIndex = 0;

			this.shipmentAdditionalDetailsControl.ResumeLayout(false);
			this.shipmentAdditionalDetailsControl.PerformLayout();

			this.AdditionalTabPage.Controls.Add(this.shipmentAdditionalDetailsControl);
		}

		private void RelatedShipmentsTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.RelatedShipmentsPanel = new ZPanel();
			this.RelatedShipmentsz = new RelatedShipmentsControl();
			this.RelatedShipmentsPanel.SuspendLayout();
			//
			// RelatedShipmentsPanel
			//
			this.RelatedShipmentsPanel.Controls.Add(this.RelatedShipmentsz);
			this.RelatedShipmentsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedShipmentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedShipmentsPanel.Name = "RelatedShipmentsPanel";
			this.RelatedShipmentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.RelatedShipmentsPanel.TabIndex = 0;
			//
			// RelatedShipmentsz
			//
			this.RelatedShipmentsz.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedShipmentsz, ".");
			this.RelatedShipmentsz.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedShipmentsz.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedShipmentsz.Name = "RelatedShipmentsz";
			this.RelatedShipmentsz.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 597, true);
			this.RelatedShipmentsz.TabIndex = 0;
			this.RelatedShipmentsPanel.ResumeLayout(false);
			this.RelatedShipmentsPanel.PerformLayout();
			this.RelatedShipmentsTabPage.Controls.Add(this.RelatedShipmentsPanel);
		}

		void BookingDetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.BookingDetailsControl = new BookingDetailsControl();
			this.BookingDetailsControl.SuspendLayout();
			//
			// BookingDetailsControl
			//
			this.BookingDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BookingDetailsControl, ".");
			this.BookingDetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BookingDetailsControl.Name = "BookingDetailsControl";
			this.BookingDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.BookingDetailsControl.TabIndex = 0;
			this.BookingDetailsControl.ResumeLayout(true);
			this.BookingDetailsControl.PerformLayout();
			this.BookingDetailsTabPage.Controls.Add(this.BookingDetailsControl);
		}
	}
}
