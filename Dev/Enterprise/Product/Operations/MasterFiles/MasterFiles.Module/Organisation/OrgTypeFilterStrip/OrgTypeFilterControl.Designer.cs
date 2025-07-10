
namespace Enterprise.MasterFiles.Module
{
	partial class OrgTypeFilterControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReceivablesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PayablesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsigneeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsignorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CarrierCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ForwarderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TransportAgentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WarehouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BrokerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ServicesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CompetitorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SalesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ANDRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ORRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.ControllingAgentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ControllingCustomerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgTypeModuleFilter);
			// 
			// ReceivablesCheckBox
			// 
			this.ReceivablesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ReceivablesCheckBox, "Property0");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property0)));
			this.ReceivablesCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|ea873fbb-e9cd-4cc0-a7cb-b4ab98bd999f", "Receivables");
			this.ReceivablesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReceivablesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 0, true);
			this.ReceivablesCheckBox.Name = "ReceivablesCheckBox";
			this.ReceivablesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.ReceivablesCheckBox.TabIndex = 0;
			this.ReceivablesCheckBox.UseVisualStyleBackColor = true;
			// 
			// PayablesCheckBox
			// 
			this.PayablesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PayablesCheckBox, "Property1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property1)));
			this.PayablesCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|74182c9e-0dfa-4fb2-9beb-4b24987f8c29", "Payables");
			this.PayablesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PayablesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 0, true);
			this.PayablesCheckBox.Name = "PayablesCheckBox";
			this.PayablesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.PayablesCheckBox.TabIndex = 1;
			this.PayablesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsigneeCheckBox
			// 
			this.ConsigneeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsigneeCheckBox, "Property2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property2)));
			this.ConsigneeCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|30f6485c-3d56-4e85-8d37-19dd1c96d8c4", "Consignee");
			this.ConsigneeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsigneeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 0, true);
			this.ConsigneeCheckBox.Name = "ConsigneeCheckBox";
			this.ConsigneeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.ConsigneeCheckBox.TabIndex = 2;
			this.ConsigneeCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConsignorCheckBox
			// 
			this.ConsignorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ConsignorCheckBox, "Property3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property3)));
			this.ConsignorCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|18673057-b6ca-4b2b-bc6d-7c47d90ea4af", "Consignor");
			this.ConsignorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ConsignorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 0, true);
			this.ConsignorCheckBox.Name = "ConsignorCheckBox";
			this.ConsignorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.ConsignorCheckBox.TabIndex = 3;
			this.ConsignorCheckBox.UseVisualStyleBackColor = true;
			// 
			// CarrierCheckBox
			// 
			this.CarrierCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CarrierCheckBox, "Property4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property4)));
			this.CarrierCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|fb715f77-d77e-40d2-ac8d-03daebd24f36", "Carrier");
			this.CarrierCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CarrierCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 19, true);
			this.CarrierCheckBox.Name = "CarrierCheckBox";
			this.CarrierCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CarrierCheckBox.TabIndex = 4;
			this.CarrierCheckBox.UseVisualStyleBackColor = true;
			// 
			// ForwarderCheckBox
			// 
			this.ForwarderCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ForwarderCheckBox, "Property5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property5)));
			this.ForwarderCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|e44dc9f0-2292-4b01-bf06-8c64b0a933bc", "Forwarder");
			this.ForwarderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ForwarderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 19, true);
			this.ForwarderCheckBox.Name = "ForwarderCheckBox";
			this.ForwarderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.ForwarderCheckBox.TabIndex = 5;
			this.ForwarderCheckBox.UseVisualStyleBackColor = true;
			// 
			// TransportAgentCheckBox
			// 
			this.TransportAgentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.TransportAgentCheckBox, "Property6");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property6)));
			this.TransportAgentCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|6a350101-3365-4745-92a0-68672a1d92ad", "Transport Client");
			this.TransportAgentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TransportAgentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 19, true);
			this.TransportAgentCheckBox.Name = "TransportAgentCheckBox";
			this.TransportAgentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TransportAgentCheckBox.TabIndex = 6;
			this.TransportAgentCheckBox.UseVisualStyleBackColor = true;
			// 
			// WarehouseCheckBox
			// 
			this.WarehouseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.WarehouseCheckBox, "Property7");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property7)));
			this.WarehouseCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|b15b132f-d51d-4ee2-83c3-d4ee01257aeb", "Warehouse");
			this.WarehouseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WarehouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 19, true);
			this.WarehouseCheckBox.Name = "WarehouseCheckBox";
			this.WarehouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			this.WarehouseCheckBox.TabIndex = 7;
			this.WarehouseCheckBox.UseVisualStyleBackColor = true;
			// 
			// BrokerCheckBox
			// 
			this.BrokerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BrokerCheckBox, "Property8");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property8)));
			this.BrokerCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|07dd1193-40f2-4b19-be70-4f677ea47d0f", "Broker");
			this.BrokerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BrokerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 38, true);
			this.BrokerCheckBox.Name = "BrokerCheckBox";
			this.BrokerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.BrokerCheckBox.TabIndex = 8;
			this.BrokerCheckBox.UseVisualStyleBackColor = true;
			// 
			// ServicesCheckBox
			// 
			this.ServicesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ServicesCheckBox, "Property9");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property9)));
			this.ServicesCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|918244a2-0a7a-4181-bfa7-8a8f2d737c05", "Services");
			this.ServicesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ServicesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 38, true);
			this.ServicesCheckBox.Name = "ServicesCheckBox";
			this.ServicesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.ServicesCheckBox.TabIndex = 9;
			this.ServicesCheckBox.UseVisualStyleBackColor = true;
			// 
			// CompetitorCheckBox
			// 
			this.CompetitorCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CompetitorCheckBox, "Property10");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property10)));
			this.CompetitorCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|3161c8a6-627c-4b84-b178-32389c291af5", "Competitor");
			this.CompetitorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompetitorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 38, true);
			this.CompetitorCheckBox.Name = "CompetitorCheckBox";
			this.CompetitorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.CompetitorCheckBox.TabIndex = 10;
			this.CompetitorCheckBox.UseVisualStyleBackColor = true;
			// 
			// SalesCheckBox
			// 
			this.SalesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SalesCheckBox, "Property11");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property11)));
			this.SalesCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|bb240871-482d-4748-9ddf-9f49aef33174", "Sales");
			this.SalesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SalesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 38, true);
			this.SalesCheckBox.Name = "SalesCheckBox";
			this.SalesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.SalesCheckBox.TabIndex = 11;
			this.SalesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ControllingAgentCheckBox
			// 
			this.ControllingAgentCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ControllingAgentCheckBox, "Property12");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property12)));
			this.ControllingAgentCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|5201126c-a5f5-4e0b-8568-e1f644beb96d", "Controlling Agent");
			this.ControllingAgentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ControllingAgentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 57, true);
			this.ControllingAgentCheckBox.Name = "ControllingAgentCheckBox";
			this.ControllingAgentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 17, true);
			this.ControllingAgentCheckBox.TabIndex = 12;
			this.ControllingAgentCheckBox.UseVisualStyleBackColor = true;
			// 
			// ControllingCustomerCheckBox
			// 
			this.ControllingCustomerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ControllingCustomerCheckBox, "Property13");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).Property13)));
			this.ControllingCustomerCheckBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|bb89f43d-c7ff-4222-a84b-54e4539656ef", "Controlling Customer");
			this.ControllingCustomerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ControllingCustomerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 57, true);
			this.ControllingCustomerCheckBox.Name = "ControllingCustomerCheckBox";
			this.ControllingCustomerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 17, true);
			this.ControllingCustomerCheckBox.TabIndex = 13;
			this.ControllingCustomerCheckBox.UseVisualStyleBackColor = true;
			// 
			// ANDRadioButton
			// 
			this.ANDRadioButton.AutoCheck = false;
			this.ANDRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ANDRadioButton, "AndJoinCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).AndJoinCondition)));
			this.ANDRadioButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|14d02b5e-2017-41be-8608-65b9d5d65265", "And");
			this.ANDRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ANDRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(67, 28, true);
			this.ANDRadioButton.Name = "ANDRadioButton";
			this.ANDRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 17, true);
			this.ANDRadioButton.TabIndex = 14;
			this.ANDRadioButton.TabStop = true;
			this.ANDRadioButton.UseVisualStyleBackColor = true;
			// 
			// ORRadioButton
			// 
			this.ORRadioButton.AutoCheck = false;
			this.ORRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ORRadioButton, "OrJoinCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Module.OrgTypeModuleFilter)(null)).OrJoinCondition)));
			this.ORRadioButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgTypeFilterControl|6620524e-52d6-4740-9a83-b0e61ad4e5f9", "Or");
			this.ORRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ORRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 28, true);
			this.ORRadioButton.Name = "ORRadioButton";
			this.ORRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 17, true);
			this.ORRadioButton.TabIndex = 15;
			this.ORRadioButton.TabStop = true;
			this.ORRadioButton.UseVisualStyleBackColor = true;
			// 
			// OrgTypeFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ORRadioButton);
			this.Controls.Add(this.ANDRadioButton);
			this.Controls.Add(this.ControllingCustomerCheckBox);
			this.Controls.Add(this.ControllingAgentCheckBox);
			this.Controls.Add(this.SalesCheckBox);
			this.Controls.Add(this.CompetitorCheckBox);
			this.Controls.Add(this.ServicesCheckBox);
			this.Controls.Add(this.BrokerCheckBox);
			this.Controls.Add(this.WarehouseCheckBox);
			this.Controls.Add(this.TransportAgentCheckBox);
			this.Controls.Add(this.ForwarderCheckBox);
			this.Controls.Add(this.CarrierCheckBox);
			this.Controls.Add(this.ConsignorCheckBox);
			this.Controls.Add(this.ConsigneeCheckBox);
			this.Controls.Add(this.PayablesCheckBox);
			this.Controls.Add(this.ReceivablesCheckBox);
			this.Name = "OrgTypeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 77, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox ReceivablesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox PayablesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsigneeCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ConsignorCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CarrierCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ForwarderCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox TransportAgentCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox WarehouseCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox BrokerCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox ServicesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox CompetitorCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox SalesCheckBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ANDRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton ORRadioButton;
		public Enterprise.ZArchitecture.GUI.ZCheckBox ControllingAgentCheckBox;
		public Enterprise.ZArchitecture.GUI.ZCheckBox ControllingCustomerCheckBox;
	}
}
