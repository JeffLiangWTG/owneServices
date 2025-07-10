
namespace Enterprise.Customs.US.GUI
{
	partial class USCarrierCombinedForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.UI_AirwayBillPrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UI_AddressTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UI_ModeOfTransportationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UI_NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UI_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 178, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.UI_AirwayBillPrefixTextBox);
			this.MainTabPage.Controls.Add(this.UI_AddressTextBox);
			this.MainTabPage.Controls.Add(this.UI_ModeOfTransportationDropEdit);
			this.MainTabPage.Controls.Add(this.UI_NameTextBox);
			this.MainTabPage.Controls.Add(this.UI_CodeTextBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 151, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 151, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 178, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.USCarrierCombined);
			// 
			// UI_AirwayBillPrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.UI_AirwayBillPrefixTextBox, "UI_AirwayBillPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCarrierCombined)(null)).UI_AirwayBillPrefix)));
			this.UI_AirwayBillPrefixTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("85846ae5-be69-4f66-883f-d94ece89ab3f", "Air Waybill Prefix");
			this.UI_AirwayBillPrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 113, true);
			this.UI_AirwayBillPrefixTextBox.Name = "UI_AirwayBillPrefixTextBox";
			this.UI_AirwayBillPrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.UI_AirwayBillPrefixTextBox.TabIndex = 9;
			// 
			// UI_AddressTextBox
			// 
			this.BindingSource.SetBindingMember(this.UI_AddressTextBox, "UI_Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCarrierCombined)(null)).UI_Address)));
			this.UI_AddressTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ddd285b8-03d7-4470-bae8-559f6c5cb813", "Address");
			this.UI_AddressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 61, true);
			this.UI_AddressTextBox.Name = "UI_AddressTextBox";
			this.UI_AddressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.UI_AddressTextBox.TabIndex = 7;
			// 
			// UI_ModeOfTransportationDropEdit
			// 
			this.UI_ModeOfTransportationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UI_ModeOfTransportationDropEdit, "UI_ModeOfTransportation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.USCarrierCombined)(null)).UI_ModeOfTransportation)));
			this.UI_ModeOfTransportationDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("1aed7b3b-b04d-4303-9b2f-5c096a6bb469", "Transport Mode");
			this.UI_ModeOfTransportationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 87, true);
			this.UI_ModeOfTransportationDropEdit.Name = "UI_ModeOfTransportationDropEdit";
			this.UI_ModeOfTransportationDropEdit.PreBoundMaxLength = 2;
			this.UI_ModeOfTransportationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.UI_ModeOfTransportationDropEdit.TabIndex = 8;
			// 
			// UI_NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UI_NameTextBox, "UI_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCarrierCombined)(null)).UI_Name)));
			this.UI_NameTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("5b831eb4-19b0-4006-aa51-0d5fd9ad2f5b", "Name");
			this.UI_NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 35, true);
			this.UI_NameTextBox.Name = "UI_NameTextBox";
			this.UI_NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 20, true);
			this.UI_NameTextBox.TabIndex = 6;
			// 
			// UI_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.UI_CodeTextBox, "UI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.USCarrierCombined)(null)).UI_Code)));
			this.UI_CodeTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("95ff7d18-fde4-40f4-a0d7-f4c7e252bd70", "Code");
			this.UI_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 9, true);
			this.UI_CodeTextBox.Name = "UI_CodeTextBox";
			this.UI_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 20, true);
			this.UI_CodeTextBox.TabIndex = 5;
			// 
			// USCarrierCombinedForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d23a3981-bbb9-4f20-abca-ede4fbae0e7e", "Carrier");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 234, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.USCarrierCombined);
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "USCarrierCombinedForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "USCarrierCombinedForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZTextBox UI_AirwayBillPrefixTextBox;
		private ZArchitecture.ZTextBox UI_AddressTextBox;
		private ZArchitecture.GUI.ZDropEdit UI_ModeOfTransportationDropEdit;
		private ZArchitecture.ZTextBox UI_NameTextBox;
		private ZArchitecture.ZTextBox UI_CodeTextBox;


	}
}
