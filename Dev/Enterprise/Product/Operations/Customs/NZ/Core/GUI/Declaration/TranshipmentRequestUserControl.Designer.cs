namespace Enterprise.Customs.NZ.GUI.Declaration
{
	partial class TranshipmentRequestUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private Enterprise.ZArchitecture.GUI.ZDropEdit TransferTransportModeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit InOutTransportModeDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox VesselCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox LloydsIMOTextBox;
		protected Enterprise.ZArchitecture.ZTextBox VoyageTextBox;
		protected Enterprise.ZArchitecture.ZTextBox FlightTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit DepartureDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel VesselAndFlightLinkLabel;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox InOutCraftDetailsGroupBox;
		protected ZArchitecture.ZLabel CoveringLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel MainPanel;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TransferTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InOutTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InOutCraftDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VoyageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LloydsIMOTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VesselAndFlightLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.VesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MovementStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CoveringLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MovementReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransitDestinationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransitDestinationPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DestinationAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransferTransportModeDropEdit.SuspendLayout();
			this.InOutTransportModeDropEdit.SuspendLayout();
			this.InOutCraftDetailsGroupBox.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.VesselCodeFindBox.SuspendLayout();
			this.DepartureDateEdit.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.MovementReasonDropEdit.SuspendLayout();
			this.TransitDestinationGroupBox.SuspendLayout();
			this.TransitDestinationPortFindBox.SuspendLayout();
			this.DestinationAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.ITranshipmentRequestParent);
			// 
			// TransferTransportModeDropEdit
			// 
			this.TransferTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransferTransportModeDropEdit, "TranshipmentRequest+C4_ModeOfMovement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_ModeOfMovement)));
			this.TransferTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 25, true);
			this.TransferTransportModeDropEdit.Name = "TransferTransportModeDropEdit";
			this.TransferTransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.TransferTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.TransferTransportModeDropEdit.TabIndex = 1;
			// 
			// InOutTransportModeDropEdit
			// 
			this.InOutTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InOutTransportModeDropEdit, "TranshipmentRequest+C4_TranshipModeOfMovement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_TranshipModeOfMovement)));
			this.InOutTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 46, true);
			this.InOutTransportModeDropEdit.Name = "InOutTransportModeDropEdit";
			this.InOutTransportModeDropEdit.ShouldResizeByMaxLength = true;
			this.InOutTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.InOutTransportModeDropEdit.TabIndex = 2;
			// 
			// InOutCraftDetailsGroupBox
			// 
			this.InOutCraftDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("10DBDD30-7A2A-4B1B-964E-E3717C8EBC80", "Incoming Craft Details");
			this.InOutCraftDetailsGroupBox.Controls.Add(this.VoyageTextBox);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.LloydsIMOTextBox);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.FlightTextBox);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.ArrivalDateEdit);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.VesselAndFlightLinkLabel);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.VesselCodeFindBox);
			this.InOutCraftDetailsGroupBox.Controls.Add(this.DepartureDateEdit);
			this.InOutCraftDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 76, true);
			this.InOutCraftDetailsGroupBox.Name = "InOutCraftDetailsGroupBox";
			this.InOutCraftDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 137, true);
			this.InOutCraftDetailsGroupBox.TabIndex = 3;
			this.InOutCraftDetailsGroupBox.TabStop = false;
			// 
			// VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.VoyageTextBox, "TranshipmentRequest+C4_TranshipBySeaVoyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_TranshipBySeaVoyage)));
			this.VoyageTextBox.CaptionResourceString = null;
			this.VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 71, true);
			this.VoyageTextBox.Name = "VoyageTextBox";
			this.VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.VoyageTextBox.TabIndex = 3;
			// 
			// LloydsIMOTextBox
			// 
			this.BindingSource.SetBindingMember(this.LloydsIMOTextBox, "TranshipmentRequest+C4_TranshipBySeaLloydsIMONum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_TranshipBySeaLloydsIMONum)));
			this.LloydsIMOTextBox.CaptionResourceString = null;
			this.LloydsIMOTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 49, true);
			this.LloydsIMOTextBox.Name = "LloydsIMOTextBox";
			this.LloydsIMOTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.LloydsIMOTextBox.TabIndex = 2;
			// 
			// FlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightTextBox, "TranshipmentRequest+C4_FlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_FlightNo)));
			this.FlightTextBox.CaptionResourceString = null;
			this.FlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 26, true);
			this.FlightTextBox.Name = "FlightTextBox";
			this.FlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.FlightTextBox.TabIndex = 0;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "TranshipmentRequest+C4_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_ArrivalDate)));
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 5;
			this.ArrivalDateEdit.UseWaitCursor = true;
			// 
			// VesselAndFlightLinkLabel
			// 
			this.VesselAndFlightLinkLabel.AutoSize = true;
			this.VesselAndFlightLinkLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("696825B3-F9E6-4318-8656-6A2663018B28", "Find Vessel/Flight No (Customs Website)");
			this.VesselAndFlightLinkLabel.IsFontBold = false;
			this.VesselAndFlightLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 73, true);
			this.VesselAndFlightLinkLabel.Name = "VesselAndFlightLinkLabel";
			this.VesselAndFlightLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 13, true);
			this.VesselAndFlightLinkLabel.TabIndex = 4;
			this.VesselAndFlightLinkLabel.Click += new System.EventHandler(this.VesselAndFlightLinkLabel_Click);
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "TranshipmentRequest+C4_TranshipBySeaVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_TranshipBySeaVessel)));
			this.VesselCodeFindBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 26, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.ShouldResize = true;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselCodeFindBox.TabIndex = 1;
			// 
			// DepartureDateEdit
			// 
			this.DepartureDateEdit.AllowDrop = true;
			this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateEdit, "TranshipmentRequest+C4_TranshipDepartureDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_TranshipDepartureDate)));
			this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 93, true);
			this.DepartureDateEdit.Name = "DepartureDateEdit";
			this.DepartureDateEdit.TabIndex = 6;
			this.DepartureDateEdit.UseWaitCursor = true;
			// 
			// MovementStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MovementStatusTextBox, "TranshipmentRequest+MovementStatusDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.MovementStatusDesc)));
			this.MovementStatusTextBox.CaptionResourceString = null;
			this.MovementStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MovementStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 103, true);
			this.MovementStatusTextBox.Name = "MovementStatusTextBox";
			this.MovementStatusTextBox.ReadOnly = true;
			this.MovementStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(325, 20, true);
			this.MovementStatusTextBox.TabIndex = 2;
			// 
			// CoveringLabel
			// 
			this.CoveringLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CoveringLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.CoveringLabel.IsFontBold = true;
			this.CoveringLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CoveringLabel.Name = "CoveringLabel";
			this.CoveringLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 375, true);
			this.CoveringLabel.TabIndex = 0;
			this.CoveringLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.CoveringLabel.Visible = false;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MovementReasonDropEdit);
			this.MainPanel.Controls.Add(this.TransitDestinationGroupBox);
			this.MainPanel.Controls.Add(this.TransferTransportModeDropEdit);
			this.MainPanel.Controls.Add(this.InOutTransportModeDropEdit);
			this.MainPanel.Controls.Add(this.InOutCraftDetailsGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 375, true);
			this.MainPanel.TabIndex = 0;
			// 
			// MovementReasonDropEdit
			// 
			this.MovementReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MovementReasonDropEdit, "TranshipmentRequest+C4_MovementReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_MovementReason)));
			this.MovementReasonDropEdit.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("42e74255-282f-4d67-ab57-2c1923035f22", "Movement Reason");
			this.MovementReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 5, true);
			this.MovementReasonDropEdit.Name = "MovementReasonDropEdit";
			this.MovementReasonDropEdit.ShouldResizeByMaxLength = true;
			this.MovementReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MovementReasonDropEdit.TabIndex = 0;
			// 
			// TransitDestinationGroupBox
			// 
			this.TransitDestinationGroupBox.Controls.Add(this.TransitDestinationPortFindBox);
			this.TransitDestinationGroupBox.Controls.Add(this.DestinationAddressControl);
			this.TransitDestinationGroupBox.Controls.Add(this.MovementStatusTextBox);
			this.TransitDestinationGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("2C8CCD12-345F-4753-B7A3-990C96DB5D98", "Transit Destination");
			this.TransitDestinationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 223, true);
			this.TransitDestinationGroupBox.Name = "TransitDestinationGroupBox";
			this.TransitDestinationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 137, true);
			this.TransitDestinationGroupBox.TabIndex = 4;
			this.TransitDestinationGroupBox.TabStop = false;
			// 
			// TransitDestinationPortFindBox
			// 
			this.TransitDestinationPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransitDestinationPortFindBox, "TranshipmentRequest+C4_RL_NKTranshipDestPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_RL_NKTranshipDestPort)));
			this.TransitDestinationPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 69, true);
			this.TransitDestinationPortFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.TransitDestinationPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.TransitDestinationPortFindBox.Name = "TransitDestinationPortFindBox";
			this.TransitDestinationPortFindBox.PreBoundMaxLength = 5;
			this.TransitDestinationPortFindBox.ShouldResize = true;
			this.TransitDestinationPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 20, true);
			this.TransitDestinationPortFindBox.TabIndex = 1;
			// 
			// DestinationAddressControl
			// 
			this.DestinationAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DestinationAddressControl, "TranshipmentRequest+C4_OA_DestinationAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.ITranshipmentRequestParent)(null)).TranshipmentRequest.C4_OA_DestinationAddress)));
			this.DestinationAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("79963c77-35ec-4eec-8e5e-c232aa66a27f", "Premise", "State the destination location of the Domestic or International Transhipment movement.");
			this.DestinationAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 17, true);
			this.DestinationAddressControl.Name = "DestinationAddressControl";
			this.DestinationAddressControl.PopupCaption = null;
			this.DestinationAddressControl.ReadOnly = false;
			this.DestinationAddressControl.ShowAddress = false;
			this.DestinationAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 41, true);
			this.DestinationAddressControl.StackControls = true;
			this.DestinationAddressControl.TabIndex = 0;
			// 
			// TranshipmentRequestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.CoveringLabel);
			this.Name = "TranshipmentRequestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 375, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransferTransportModeDropEdit.ResumeLayout(true);
			this.TransferTransportModeDropEdit.PerformLayout();
			this.InOutTransportModeDropEdit.ResumeLayout(true);
			this.InOutTransportModeDropEdit.PerformLayout();
			this.InOutCraftDetailsGroupBox.ResumeLayout(false);
			this.InOutCraftDetailsGroupBox.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.VesselCodeFindBox.ResumeLayout(true);
			this.VesselCodeFindBox.PerformLayout();
			this.DepartureDateEdit.ResumeLayout(true);
			this.DepartureDateEdit.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.MovementReasonDropEdit.ResumeLayout(true);
			this.MovementReasonDropEdit.PerformLayout();
			this.TransitDestinationGroupBox.ResumeLayout(false);
			this.TransitDestinationGroupBox.PerformLayout();
			this.TransitDestinationPortFindBox.ResumeLayout(true);
			this.TransitDestinationPortFindBox.PerformLayout();
			this.DestinationAddressControl.ResumeLayout(true);
			this.DestinationAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox TransitDestinationGroupBox;
		private ZArchitecture.GUI.ZAddressControl DestinationAddressControl;
		protected ZArchitecture.GUI.ZCodeFindBox TransitDestinationPortFindBox;
		private ZArchitecture.GUI.ZDropEdit MovementReasonDropEdit;
		protected ZArchitecture.ZTextBox MovementStatusTextBox;
	}
}
