namespace Enterprise.Customs.GUI
{
	partial class CommonHeaderDetailsUserControl
	{
		void InitializeComponent()
		{
			this.EntryTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PeriodFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PeriodToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AuthorizationNumberGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DeclarantAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.RepresentativeAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.BuyingAgentAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EntryTypeDropEdit.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.PeriodFromDateEdit.SuspendLayout();
			this.PeriodToDateEdit.SuspendLayout();
			this.AuthorizationNumberGuidDropEdit.SuspendLayout();
			this.DeclarantAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.BuyingAgentAddressControl.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.DeclarantTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusReconDeclaration);
			// 
			// EntryTypeDropEdit
			// 
			this.EntryTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryTypeDropEdit, "CRD_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_ApplicationCode)));
			this.EntryTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 10, true);
			this.EntryTypeDropEdit.Name = "EntryTypeDropEdit";
			this.EntryTypeDropEdit.ShouldResizeByMaxLength = true;
			this.EntryTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.EntryTypeDropEdit.TabIndex = 0;
			// 
			// EntryStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.EntryStatusTextBox, "CRD_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_CustomsStatus)));
			this.EntryStatusTextBox.CaptionResourceString = null;
			this.EntryStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.EntryStatusTextBox.Name = "EntryStatusTextBox";
			this.EntryStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.EntryStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.EntryStatusTextBox.TabIndex = 2;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "CRD_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 35, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeCodeFindBox.ParentType = null;
			this.CustomsOfficeCodeFindBox.PreBoundMaxLength = 2;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// PeriodFromDateEdit
			// 
			this.PeriodFromDateEdit.AllowDrop = true;
			this.PeriodFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PeriodFromDateEdit, "CRD_PeriodFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_PeriodFrom)));
			this.PeriodFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 85, true);
			this.PeriodFromDateEdit.Name = "PeriodFromDateEdit";
			this.PeriodFromDateEdit.TabIndex = 3;
			// 
			// PeriodToDateEdit
			// 
			this.PeriodToDateEdit.AllowDrop = true;
			this.PeriodToDateEdit.AutoCompleteMonthThreshold = 1;
			this.PeriodToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PeriodToDateEdit, "CRD_PeriodTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_PeriodTo)));
			this.PeriodToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 110, true);
			this.PeriodToDateEdit.Name = "PeriodToDateEdit";
			this.PeriodToDateEdit.TabIndex = 4;
			// 
			// AuthorizationNumberGuidDropEdit
			// 
			this.AuthorizationNumberGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberGuidDropEdit, "CRD_CPH_ReconClearanceAuthorisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_CPH_ReconClearanceAuthorisation)));
			this.AuthorizationNumberGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 135, true);
			this.AuthorizationNumberGuidDropEdit.Name = "AuthorizationNumberGuidDropEdit";
			this.AuthorizationNumberGuidDropEdit.PreBoundMaxLength = 15;
			this.AuthorizationNumberGuidDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorizationNumberGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.AuthorizationNumberGuidDropEdit.TabIndex = 5;
			// 
			// DeclarantAddressControl
			// 
			this.DeclarantAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantAddressControl, "CRD_OA_DeclarantAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_OA_DeclarantAddress)));
			this.DeclarantAddressControl.BindToOrgList = "Lookups+DeclarantList";
			this.DeclarantAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 162, true);
			this.DeclarantAddressControl.Name = "DeclarantAddressControl";
			this.DeclarantAddressControl.PopupCaption = "";
			this.DeclarantAddressControl.ReadOnly = false;
			this.DeclarantAddressControl.ShowAddress = false;
			this.DeclarantAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.DeclarantAddressControl.TabIndex = 6;
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeAddressControl, "CRD_OA_RepresentativeAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_OA_RepresentativeAddress)));
			this.RepresentativeAddressControl.BindToOrgList = "Lookups+RepresentativeList";
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 189, true);
			this.RepresentativeAddressControl.Name = "RepresentativeAddressControl";
			this.RepresentativeAddressControl.PopupCaption = "";
			this.RepresentativeAddressControl.ReadOnly = false;
			this.RepresentativeAddressControl.ShowAddress = false;
			this.RepresentativeAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.RepresentativeAddressControl.TabIndex = 7;
			// 
			// BuyingAgentAddressControl
			// 
			this.BuyingAgentAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BuyingAgentAddressControl, "CRD_OA_BuyingAgentAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_OA_BuyingAgentAddress)));
			this.BuyingAgentAddressControl.BindToOrgList = "Lookups+BuyingAgentList";
			this.BuyingAgentAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 216, true);
			this.BuyingAgentAddressControl.Name = "BuyingAgentAddressControl";
			this.BuyingAgentAddressControl.PopupCaption = "";
			this.BuyingAgentAddressControl.ReadOnly = false;
			this.BuyingAgentAddressControl.ShowAddress = false;
			this.BuyingAgentAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.BuyingAgentAddressControl.TabIndex = 8;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "CRD_DeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_DeclarationType)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 243, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.ShouldResizeByMaxLength = true;
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeclarationTypeDropEdit.TabIndex = 12;
			// 
			// DeclarantTypeDropEdit
			// 
			this.DeclarantTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantTypeDropEdit, "CRD_DeclarantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_DeclarantType)));
			this.DeclarantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 270, true);
			this.DeclarantTypeDropEdit.Name = "DeclarantTypeDropEdit";
			this.DeclarantTypeDropEdit.ShouldResizeByMaxLength = true;
			this.DeclarantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DeclarantTypeDropEdit.TabIndex = 13;
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "CRD_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusReconDeclaration)(null)).CRD_MessageStatus)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 297, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 20, true);
			this.MessageStatusTextBox.TabIndex = 14;
			// 
			// CommonHeaderDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageStatusTextBox);
			this.Controls.Add(this.BuyingAgentAddressControl);
			this.Controls.Add(this.RepresentativeAddressControl);
			this.Controls.Add(this.DeclarantAddressControl);
			this.Controls.Add(this.EntryTypeDropEdit);
			this.Controls.Add(this.EntryStatusTextBox);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.PeriodFromDateEdit);
			this.Controls.Add(this.PeriodToDateEdit);
			this.Controls.Add(this.AuthorizationNumberGuidDropEdit);
			this.Controls.Add(this.DeclarationTypeDropEdit);
			this.Controls.Add(this.DeclarantTypeDropEdit);
			this.Name = "CommonHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1165, 525, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EntryTypeDropEdit.ResumeLayout(true);
			this.EntryTypeDropEdit.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.PeriodFromDateEdit.ResumeLayout(true);
			this.PeriodFromDateEdit.PerformLayout();
			this.PeriodToDateEdit.ResumeLayout(true);
			this.PeriodToDateEdit.PerformLayout();
			this.AuthorizationNumberGuidDropEdit.ResumeLayout(true);
			this.AuthorizationNumberGuidDropEdit.PerformLayout();
			this.DeclarantAddressControl.ResumeLayout(true);
			this.DeclarantAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.BuyingAgentAddressControl.ResumeLayout(true);
			this.BuyingAgentAddressControl.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.DeclarantTypeDropEdit.ResumeLayout(true);
			this.DeclarantTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit EntryTypeDropEdit;
		internal ZArchitecture.ZTextBox EntryStatusTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PeriodFromDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PeriodToDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidDropEdit AuthorizationNumberGuidDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl DeclarantAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl RepresentativeAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZAddressControl BuyingAgentAddressControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit DeclarantTypeDropEdit;
		internal ZArchitecture.ZTextBox MessageStatusTextBox;
	}
}
