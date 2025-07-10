namespace Enterprise.Freight.GUI
{
	partial class VesselVoyageForm
	{
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBox sea_vesselBoundCodeFindBox;
			Enterprise.ZArchitecture.ZTextBox sea_voyageFlightBoundTextEdit;
			Enterprise.ZArchitecture.ZTextBox rail_voyageFlightBoundTextBox;
			Enterprise.ZArchitecture.ZTextBox rail_vesselBoundTextBox;
			Enterprise.ZArchitecture.ZTextBox air_voyageFlightBoundTextBox;
			Enterprise.ZArchitecture.ZTextBox road_voyageFlightBoundTextBox;
			CargoWise.Windows.UI.KPanel bottomPanel;
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.sea_panel = new CargoWise.Windows.UI.KPanel();
			this.road_panel = new CargoWise.Windows.UI.KPanel();
			this.air_panel = new CargoWise.Windows.UI.KPanel();
			this.rail_panel = new CargoWise.Windows.UI.KPanel();
			sea_vesselBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			sea_voyageFlightBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			sea_voyageCarrierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			rail_voyageFlightBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			rail_vesselBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			air_voyageFlightBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			road_voyageFlightBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			bottomPanel = new CargoWise.Windows.UI.KPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			bottomPanel.SuspendLayout();
			this.sea_panel.SuspendLayout();
			this.road_panel.SuspendLayout();
			this.air_panel.SuspendLayout();
			this.rail_panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.VoyageFinder);
			// 
			// sea_vesselBoundCodeFindBox
			// 
			sea_vesselBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(sea_vesselBoundCodeFindBox, "JV_RV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_RV_NKVessel)));
			sea_vesselBoundCodeFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselVoyageForm|69ea1ce5-ec80-47a1-b3e0-9521ab61a4d1", "Vessel");
			sea_vesselBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			sea_vesselBoundCodeFindBox.Name = "sea_vesselBoundCodeFindBox";
			sea_vesselBoundCodeFindBox.PreBoundMaxLength = 35;
			sea_vesselBoundCodeFindBox.ShowDescriptionBox = false;
			sea_vesselBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			sea_vesselBoundCodeFindBox.TabIndex = 1;
			// 
			// sea_voyageFlightBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(sea_voyageFlightBoundTextEdit, "JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_VoyageFlight)));
			sea_voyageFlightBoundTextEdit.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselVoyageForm|984e3913-44f6-495d-bcf7-7940f00641e3", "Voyage No");
			sea_voyageFlightBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			sea_voyageFlightBoundTextEdit.Name = "sea_voyageFlightBoundTextEdit";
			sea_voyageFlightBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			sea_voyageFlightBoundTextEdit.TabIndex = 2;
			// 
			// sea_voyageCarrierGuidFindBox
			// 
			this.sea_voyageCarrierGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sea_voyageCarrierGuidFindBox, "CarrierPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.VoyageFinder)(null)).CarrierPK)));
			this.sea_voyageCarrierGuidFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("506cbaa9-824c-4141-a3ba-a767f60992c4", "Carrier");
			this.sea_voyageCarrierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 56, true);
			this.sea_voyageCarrierGuidFindBox.Name = "sea_voyageCarrierGuidFindBox";
			this.sea_voyageCarrierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.sea_voyageCarrierGuidFindBox.TabIndex = 3;
			// 
			// rail_voyageFlightBoundTextBox
			// 
			this.BindingSource.SetBindingMember(rail_voyageFlightBoundTextBox, "JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_VoyageFlight)));
			rail_voyageFlightBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("e4630401-25bf-4df6-989c-a4508848274c", "Journey No");
			rail_voyageFlightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 31, true);
			rail_voyageFlightBoundTextBox.Name = "rail_voyageFlightBoundTextBox";
			rail_voyageFlightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			rail_voyageFlightBoundTextBox.TabIndex = 4;
			// 
			// rail_vesselBoundTextBox
			// 
			this.BindingSource.SetBindingMember(rail_vesselBoundTextBox, "JV_RV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_RV_NKVessel)));
			rail_vesselBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("7230c4a0-8dc5-4f13-ad8c-e7c021cc2593", "Journey");
			rail_vesselBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			rail_vesselBoundTextBox.Name = "rail_vesselBoundTextBox";
			rail_vesselBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			rail_vesselBoundTextBox.TabIndex = 5;
			// 
			// air_voyageFlightBoundTextBox
			// 
			this.BindingSource.SetBindingMember(air_voyageFlightBoundTextBox, "JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_VoyageFlight)));
			air_voyageFlightBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("cae43b49-e7c3-4535-8491-1a7f1366c2d1", "Flight No");
			air_voyageFlightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			air_voyageFlightBoundTextBox.Name = "air_voyageFlightBoundTextBox";
			air_voyageFlightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			air_voyageFlightBoundTextBox.TabIndex = 5;
			// 
			// road_voyageFlightBoundTextBox
			// 
			this.BindingSource.SetBindingMember(road_voyageFlightBoundTextBox, "JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageFinder)(null)).JV_VoyageFlight)));
			road_voyageFlightBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("545f6d77-015a-4e0a-8c2e-67a9dcc6c2c2", "Truck Ref.");
			road_voyageFlightBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			road_voyageFlightBoundTextBox.Name = "road_voyageFlightBoundTextBox";
			road_voyageFlightBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			road_voyageFlightBoundTextBox.TabIndex = 5;
			// 
			// bottomPanel
			// 
			bottomPanel.Controls.Add(this.cancelButton);
			bottomPanel.Controls.Add(this.addButton);
			bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			bottomPanel.Name = "bottomPanel";
			bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 24, true);
			bottomPanel.TabIndex = 9;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselVoyageForm|21a4d5ac-2856-497c-acdd-cf5e91fe0464", "Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 0, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.cancelButton.TabIndex = 4;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// addButton
			// 
			this.addButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.addButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselVoyageForm|d07505e9-e534-462e-a728-d02e30e62461", "Add");
			this.addButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(184, 0, true);
			this.addButton.Name = "addButton";
			this.addButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.addButton.TabIndex = 3;
			this.addButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// sea_panel
			// 
			this.sea_panel.Controls.Add(sea_vesselBoundCodeFindBox);
			this.sea_panel.Controls.Add(sea_voyageFlightBoundTextEdit);
			this.sea_panel.Controls.Add(sea_voyageCarrierGuidFindBox);
			this.sea_panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sea_panel.Name = "sea_panel";
			this.sea_panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 79, true);
			this.sea_panel.TabIndex = 5;
			this.sea_panel.Visible = false;
			// 
			// road_panel
			// 
			this.road_panel.Controls.Add(road_voyageFlightBoundTextBox);
			this.road_panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.road_panel.Name = "road_panel";
			this.road_panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 56, true);
			this.road_panel.TabIndex = 6;
			this.road_panel.Visible = false;
			// 
			// air_panel
			// 
			this.air_panel.Controls.Add(air_voyageFlightBoundTextBox);
			this.air_panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.air_panel.Name = "air_panel";
			this.air_panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 56, true);
			this.air_panel.TabIndex = 7;
			this.air_panel.Visible = false;
			// 
			// rail_panel
			// 
			this.rail_panel.Controls.Add(rail_vesselBoundTextBox);
			this.rail_panel.Controls.Add(rail_voyageFlightBoundTextBox);
			this.rail_panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.rail_panel.Name = "rail_panel";
			this.rail_panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 56, true);
			this.rail_panel.TabIndex = 8;
			this.rail_panel.Visible = false;
			// 
			// VesselVoyageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 132, true);
			this.Controls.Add(bottomPanel);
			this.Controls.Add(this.sea_panel);
			this.Controls.Add(this.rail_panel);
			this.Controls.Add(this.air_panel);
			this.Controls.Add(this.road_panel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.VoyageFinder);
			this.DataSourceTypeName = "Enterprise.Freight.Business.VoyageFinder";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "VesselVoyageForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.road_panel, 0);
			this.Controls.SetChildIndex(this.air_panel, 0);
			this.Controls.SetChildIndex(this.rail_panel, 0);
			this.Controls.SetChildIndex(this.sea_panel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(bottomPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			bottomPanel.ResumeLayout(false);
			this.sea_panel.ResumeLayout(false);
			this.sea_panel.PerformLayout();
			this.road_panel.ResumeLayout(false);
			this.road_panel.PerformLayout();
			this.air_panel.ResumeLayout(false);
			this.air_panel.PerformLayout();
			this.rail_panel.ResumeLayout(false);
			this.rail_panel.PerformLayout();
			this.ResumeLayout(false);

		}
		private System.ComponentModel.IContainer components = null;
		private CargoWise.Windows.UI.KPanel sea_panel;
		private CargoWise.Windows.UI.KPanel road_panel;
		private CargoWise.Windows.UI.KPanel air_panel;
		private CargoWise.Windows.UI.KPanel rail_panel;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton addButton;
		private ZArchitecture.GUI.ZGuidFindBox sea_voyageCarrierGuidFindBox;
	}
}
