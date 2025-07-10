namespace Enterprise.Customs.GUI
{ 
	partial class ShipmentTypeUserControl
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
			this.ApplicationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceLevelCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MessageSubTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InlandModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ApplicationCodeDropEdit.SuspendLayout();
			this.ServiceLevelCodeFindBox.SuspendLayout();
			this.MessageSubTypeDropEdit.SuspendLayout();
			this.MessageTypeDropEdit.SuspendLayout();
			this.TransportModeDropEdit.SuspendLayout();
			this.ContainerModeDropEdit.SuspendLayout();
			this.InlandModeOfTransportDropEdit.SuspendLayout();
			this.DeclarantTypeDropEdit.SuspendLayout();
			this.CustomsProfileDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// ApplicationCodeDropEdit
			// 
			this.ApplicationCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ApplicationCodeDropEdit, "JE_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ApplicationCodeList)));
			this.ApplicationCodeDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("aa3961ec-6b26-4807-b492-f77a0ae31f2b", "Submit Type");
			this.ApplicationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 183, true);
			this.ApplicationCodeDropEdit.Name = "ApplicationCodeDropEdit";
			this.ApplicationCodeDropEdit.PreBoundMaxLength = 3;
			this.ApplicationCodeDropEdit.ShouldResizeByMaxLength = true;
			this.ApplicationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.ApplicationCodeDropEdit.TabIndex = 18;
			// 
			// ServiceLevelCodeFindBox
			// 
			this.ServiceLevelCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ServiceLevelCodeFindBox, "JE_RS_NKServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RS_NKServiceLevel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.ServiceLevels)));
			this.ServiceLevelCodeFindBox.BindToList = "Lookups.ServiceLevels";
			this.ServiceLevelCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 158, true);
			this.ServiceLevelCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ServiceLevel;
			this.ServiceLevelCodeFindBox.Name = "ServiceLevelCodeFindBox";
			this.ServiceLevelCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ServiceLevelCodeFindBox.ParentType = null;
			this.ServiceLevelCodeFindBox.PreBoundMaxLength = 3;
			this.ServiceLevelCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.ServiceLevelCodeFindBox.TabIndex = 17;
			// 
			// MessageSubTypeDropEdit
			// 
			this.MessageSubTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageSubTypeDropEdit, "JE_MessageSubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageSubType)));
			this.MessageSubTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 31, true);
			this.MessageSubTypeDropEdit.Name = "MessageSubTypeDropEdit";
			this.MessageSubTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageSubTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.MessageSubTypeDropEdit.TabIndex = 12;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "JE_MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MessageType)));
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 6, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.PreBoundMaxLength = 3;
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.MessageTypeDropEdit.TabIndex = 11;
			// 
			// TransportModeDropEdit
			// 
			this.TransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "JE_TransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportMode)));
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 57, true);
			this.TransportModeDropEdit.Name = "TransportModeDropEdit";
			this.TransportModeDropEdit.PreBoundMaxLength = 3;
			this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.TransportModeDropEdit.TabIndex = 13;
			// 
			// ContainerModeDropEdit
			// 
			this.ContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerModeDropEdit, "JE_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_ContainerMode)));
			this.ContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 133, true);
			this.ContainerModeDropEdit.Name = "ContainerModeDropEdit";
			this.ContainerModeDropEdit.PreBoundMaxLength = 3;
			this.ContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.ContainerModeDropEdit.TabIndex = 16;
			// 
			// InlandModeOfTransportDropEdit
			// 
			this.InlandModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InlandModeOfTransportDropEdit, "JE_TransportModeInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_TransportModeInland)));
			this.InlandModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 107, true);
			this.InlandModeOfTransportDropEdit.Name = "InlandModeOfTransportDropEdit";
			this.InlandModeOfTransportDropEdit.PreBoundMaxLength = 3;
			this.InlandModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.InlandModeOfTransportDropEdit.TabIndex = 15;
			// 
			// DeclarantTypeDropEdit
			// 
			this.DeclarantTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantTypeDropEdit, "JE_DeclarantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeclarantType)));
			this.DeclarantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 209, true);
			this.DeclarantTypeDropEdit.Name = "DeclarantTypeDropEdit";
			this.DeclarantTypeDropEdit.PreBoundMaxLength = 3;
			this.DeclarantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 17, true);
			this.DeclarantTypeDropEdit.TabIndex = 19;
			// 
			// CustomsProfileDropEdit
			// 
			this.CustomsProfileDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsProfileDropEdit, "JE_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_CustomsProfile)));
			this.CustomsProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 82, true);
			this.CustomsProfileDropEdit.Name = "CustomsProfileDropEdit";
			this.CustomsProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 17, true);
			this.CustomsProfileDropEdit.TabIndex = 14;
			// 
			// ShipmentTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CustomsProfileDropEdit);
			this.Controls.Add(this.InlandModeOfTransportDropEdit);
			this.Controls.Add(this.ApplicationCodeDropEdit);
			this.Controls.Add(this.ServiceLevelCodeFindBox);
			this.Controls.Add(this.MessageSubTypeDropEdit);
			this.Controls.Add(this.MessageTypeDropEdit);
			this.Controls.Add(this.TransportModeDropEdit);
			this.Controls.Add(this.ContainerModeDropEdit);
			this.Controls.Add(this.DeclarantTypeDropEdit);
			this.Name = "ShipmentTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ApplicationCodeDropEdit.ResumeLayout(true);
			this.ApplicationCodeDropEdit.PerformLayout();
			this.ServiceLevelCodeFindBox.ResumeLayout(true);
			this.ServiceLevelCodeFindBox.PerformLayout();
			this.MessageSubTypeDropEdit.ResumeLayout(true);
			this.MessageSubTypeDropEdit.PerformLayout();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.TransportModeDropEdit.ResumeLayout(true);
			this.TransportModeDropEdit.PerformLayout();
			this.ContainerModeDropEdit.ResumeLayout(true);
			this.ContainerModeDropEdit.PerformLayout();
			this.InlandModeOfTransportDropEdit.ResumeLayout(true);
			this.InlandModeOfTransportDropEdit.PerformLayout();
			this.DeclarantTypeDropEdit.ResumeLayout(true);
			this.DeclarantTypeDropEdit.PerformLayout();
			this.CustomsProfileDropEdit.ResumeLayout(true);
			this.CustomsProfileDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit ApplicationCodeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ServiceLevelCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit MessageSubTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ContainerModeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit InlandModeOfTransportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DeclarantTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CustomsProfileDropEdit;
	}
}
