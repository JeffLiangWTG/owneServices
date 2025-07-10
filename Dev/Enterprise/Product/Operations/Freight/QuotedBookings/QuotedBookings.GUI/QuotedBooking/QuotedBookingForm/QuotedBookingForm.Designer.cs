using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class QuotedBookingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ApproveOneOffButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintQuoteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ConvertQuoteToQuotedBookingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AdditionalDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QuotedBookingAdditionalDetailsControl = new Enterprise.Freight.QuotedBookings.GUI.ForwarderAdditionalDetailsControl();
			this.CustomFieldsTabPage = new Enterprise.Freight.QuotedBookings.GUI.CustomFieldsTabPage();
			this.IsConsolidatedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DocumentSelectionTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.QuotedBookingDocumentSelection = new Enterprise.Freight.QuotedBookings.GUI.QuotedBookingDocumentSelection();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.QuotedBookingDetailsControl = new Enterprise.Freight.QuotedBookings.GUI.ForwarderMainControl();
			this.NVOCCModeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.NVOCCQuotedBookingDetailsControl = new Enterprise.Freight.QuotedBookings.GUI.NVOCCMainControl();
			this.NVOCCQuotedBookingAdditionalDetailsControl = new Enterprise.Freight.QuotedBookings.GUI.NVOCCAdditionalDetailsControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalDetailsTabPage.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.DocumentSelectionTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 600, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.AdditionalDetailsTabPage);
			this.MainTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.MainTabControl.Controls.Add(this.DocumentSelectionTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 627, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DocumentSelectionTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomFieldsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AdditionalDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.NVOCCQuotedBookingDetailsControl);
			this.MainTabPage.Controls.Add(this.QuotedBookingDetailsControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 620, true);
			this.MainTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1343, 700, true);
			this.MainTabPage.AutoScroll = true;
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.PrintQuoteButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.ConvertQuoteToQuotedBookingButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Controls.Add(this.ApproveOneOffButton);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 1, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 31, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 31, true);
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible = true;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			// 
			// ApproveOneOffButton
			// 
			this.ApproveOneOffButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApproveOneOffButton.CaptionResourceString = Res.GetData("QuotedBookingForm|f47cd510-5065-4178-a9ff-f9aa5933fc2d", "Approve");
			this.ApproveOneOffButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 5, true);
			this.ApproveOneOffButton.Name = "ApproveOneOffButton";
			this.ApproveOneOffButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.ApproveOneOffButton.TabIndex = 1;
			this.ApproveOneOffButton.Click += new System.EventHandler(this.ApproveOneOffButton_Click);
			// 
			// PrintQuoteButton
			// 
			this.PrintQuoteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintQuoteButton.CaptionResourceString = Res.GetData("QuotedBookingForm|4022f162-00e0-4f04-a4cd-3532ea08239f", "Print");
			this.PrintQuoteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.PrintQuoteButton.Name = "PrintQuoteButton";
			this.PrintQuoteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.PrintQuoteButton.TabIndex = 2;
			this.PrintQuoteButton.Click += new System.EventHandler(this.PrintQuoteButton_Click);
			// 
			// ConvertQuoteToQuotedBookingButton
			// 
			this.ConvertQuoteToQuotedBookingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConvertQuoteToQuotedBookingButton.CaptionResourceString = Res.GetData("QuotedBookingForm|b120a341-05c0-497e-87c0-2d759ffa1860", "Convert to Booking with Quote");
			this.ConvertQuoteToQuotedBookingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 5, true);
			this.ConvertQuoteToQuotedBookingButton.Name = "ConvertQuoteToQuotedBookingButton";
			this.ConvertQuoteToQuotedBookingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 23, true);
			this.ConvertQuoteToQuotedBookingButton.TabIndex = 0;
			this.ConvertQuoteToQuotedBookingButton.Click += new System.EventHandler(this.ConvertQuoteToQuotedBookingButton_Click);
			// 
			// AdditionalDetailsTabPage
			// 
			this.AdditionalDetailsTabPage.CaptionResourceString = Res.GetData("QuotedBookingForm|c09501ed-07fc-4e93-9108-a78c11ec5a3d", "Additional Details");
			this.AdditionalDetailsTabPage.Controls.Add(this.NVOCCQuotedBookingAdditionalDetailsControl);
			this.AdditionalDetailsTabPage.Controls.Add(this.QuotedBookingAdditionalDetailsControl);
			this.AdditionalDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDetailsTabPage.Name = "AdditionalDetailsTabPage";
			this.AdditionalDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 695, true);
			this.AdditionalDetailsTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 705, true);
			this.AdditionalDetailsTabPage.AutoScroll = true;
			this.AdditionalDetailsTabPage.TabIndex = 3;
			// 
			// QuotedBookingAdditionalDetailsControl
			// 
			this.QuotedBookingAdditionalDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotedBookingAdditionalDetailsControl, ".");
			this.QuotedBookingAdditionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 6, true);
			this.QuotedBookingAdditionalDetailsControl.Name = "QuotedBookingAdditionalDetailsControl";
			this.QuotedBookingAdditionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 625, true);
			this.QuotedBookingAdditionalDetailsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 620, true);
			this.QuotedBookingAdditionalDetailsControl.TabIndex = 0;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Res.GetData("QuotedBookingForm|f37b9a9d-2817-4200-83fc-7e3ec8d82063", "Custom Fields");
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 600, true);
			this.CustomFieldsTabPage.TabIndex = 6;
			this.CustomFieldsTabPage.UseVisualStyleBackColor = true;
			// 
			// IsConsolidatedCheckBox
			// 
			this.IsConsolidatedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.IsConsolidatedCheckBox, "IsForwardRegistered");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.QuotedBookings.Business.QuotedBooking)(null)).IsForwardRegistered)));
			this.IsConsolidatedCheckBox.CaptionResourceString = Res.GetData("QuotedBookingForm|e320b788-93f1-40b2-9337-e0e2ae2bbb43", "Is Consolidated");
			this.IsConsolidatedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsConsolidatedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(775, 3, true);
			this.IsConsolidatedCheckBox.Name = "IsConsolidatedCheckBox";
			this.IsConsolidatedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 18, true);
			this.IsConsolidatedCheckBox.TabIndex = 3;
			this.IsConsolidatedCheckBox.UseVisualStyleBackColor = true;
			this.IsConsolidatedCheckBox.Visible = false;
			// 
			// DocumentSelectionTabPage
			// 
			this.DocumentSelectionTabPage.CaptionResourceString = Res.GetData("QuotedBookingForm|03b7d5a2-5ce7-46dc-bf77-883ca7934976", "Document Selection");
			this.DocumentSelectionTabPage.Controls.Add(this.QuotedBookingDocumentSelection);
			this.DocumentSelectionTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocumentSelectionTabPage.Name = "DocumentSelectionTabPage";
			this.DocumentSelectionTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 600, true);
			this.DocumentSelectionTabPage.TabIndex = 4;
			// 
			// QuotedBookingDocumentSelection
			// 
			this.QuotedBookingDocumentSelection.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotedBookingDocumentSelection, ".");
			this.QuotedBookingDocumentSelection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QuotedBookingDocumentSelection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuotedBookingDocumentSelection.Name = "QuotedBookingDocumentSelection";
			this.QuotedBookingDocumentSelection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 600, true);
			this.QuotedBookingDocumentSelection.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.CaptionResourceString = Res.GetData("QuotedBookingForm|69002cce-bb3b-4c7a-9ecb-3bc92575c9a3", "Workflow");
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 600, true);
			this.WorkflowTabPage.TabIndex = 5;
			// 
			// QuotedBookingDetailsControl
			// 
			this.QuotedBookingDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuotedBookingDetailsControl, ".");
			this.QuotedBookingDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(513, 56, true);
			this.QuotedBookingDetailsControl.Name = "QuotedBookingDetailsControl";
			this.QuotedBookingDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 460, true);
			this.QuotedBookingDetailsControl.TabIndex = 0;
			// 
			// NVOCCModeCheckBox
			// 
			this.NVOCCModeCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.NVOCCModeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.NVOCCModeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(911, 0, true);
			this.NVOCCModeCheckBox.Name = "NVOCCModeCheckBox";
			this.NVOCCModeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 18, true);
			this.NVOCCModeCheckBox.TabIndex = 4;
			this.NVOCCModeCheckBox.UseVisualStyleBackColor = true;
			// 
			// NVOCCQuotedBookingDetailsControl
			// 
			this.NVOCCQuotedBookingDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NVOCCQuotedBookingDetailsControl, ".");
			this.NVOCCQuotedBookingDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NVOCCQuotedBookingDetailsControl.Name = "NVOCCQuotedBookingDetailsControl";
			this.NVOCCQuotedBookingDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(434, 477, true);
			this.NVOCCQuotedBookingDetailsControl.TabIndex = 1;
			// 
			// NVOCCQuotedBookingAdditionalDetailsControl
			// 
			this.NVOCCQuotedBookingAdditionalDetailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NVOCCQuotedBookingAdditionalDetailsControl, ".");
			this.NVOCCQuotedBookingAdditionalDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 7, true);
			this.NVOCCQuotedBookingAdditionalDetailsControl.Name = "NVOCCQuotedBookingAdditionalDetailsControl";
			this.NVOCCQuotedBookingAdditionalDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 447, true);
			this.NVOCCQuotedBookingAdditionalDetailsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 447, true);
			this.NVOCCQuotedBookingAdditionalDetailsControl.TabIndex = 1;
			// 
			// QuotedBookingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 683, true);
			this.Controls.Add(this.NVOCCModeCheckBox);
			this.Controls.Add(this.IsConsolidatedCheckBox);
			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBookings.Business";
			this.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.QuotedBooking);
			this.DataSourceTypeName = "Enterprise.Freight.QuotedBookings.Business.QuotedBooking";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 725, true);
			this.Name = "QuotedBookingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "QuotedBookingForm";
			this.Controls.SetChildIndex(this.NVOCCModeCheckBox, 0);
			this.Controls.SetChildIndex(this.IsConsolidatedCheckBox, 0);
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDetailsTabPage.ResumeLayout(false);
			this.AdditionalDetailsTabPage.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.DocumentSelectionTabPage.ResumeLayout(false);
			this.DocumentSelectionTabPage.PerformLayout();
			this.ConvertQuoteToQuotedBookingButton.ResumeLayout(false);
			this.ConvertQuoteToQuotedBookingButton.PerformLayout();
			this.QuotedBookingAdditionalDetailsControl.ResumeLayout(false);
			this.QuotedBookingAdditionalDetailsControl.PerformLayout();
			this.QuotedBookingDocumentSelection.ResumeLayout(false);
			this.QuotedBookingDocumentSelection.PerformLayout();
			this.QuotedBookingDetailsControl.ResumeLayout(false);
			this.QuotedBookingDetailsControl.PerformLayout();
			this.NVOCCQuotedBookingDetailsControl.ResumeLayout(false);
			this.NVOCCQuotedBookingDetailsControl.PerformLayout();
			this.NVOCCQuotedBookingAdditionalDetailsControl.ResumeLayout(false);
			this.NVOCCQuotedBookingAdditionalDetailsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton ApproveOneOffButton;
		internal Enterprise.ZArchitecture.GUI.ZButton PrintQuoteButton;
		internal Enterprise.ZArchitecture.GUI.ZButton ConvertQuoteToQuotedBookingButton;
		internal Enterprise.ZArchitecture.GUI.ZTabPage AdditionalDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage DocumentSelectionTabPage;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private Enterprise.Freight.QuotedBookings.GUI.QuotedBookingDocumentSelection QuotedBookingDocumentSelection;
		internal Enterprise.Freight.QuotedBookings.GUI.ForwarderAdditionalDetailsControl QuotedBookingAdditionalDetailsControl;
		internal Enterprise.Freight.QuotedBookings.GUI.ForwarderMainControl QuotedBookingDetailsControl;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox NVOCCModeCheckBox;
		internal Enterprise.Freight.QuotedBookings.GUI.NVOCCMainControl NVOCCQuotedBookingDetailsControl;
		internal Enterprise.Freight.QuotedBookings.GUI.NVOCCAdditionalDetailsControl NVOCCQuotedBookingAdditionalDetailsControl;
		internal Enterprise.Freight.QuotedBookings.GUI.CustomFieldsTabPage CustomFieldsTabPage;
		internal ZArchitecture.GUI.ZCheckBox IsConsolidatedCheckBox;
	}
}
