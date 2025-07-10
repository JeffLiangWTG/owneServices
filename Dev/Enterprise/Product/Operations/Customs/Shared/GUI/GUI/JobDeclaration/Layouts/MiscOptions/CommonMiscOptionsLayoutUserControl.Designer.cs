namespace Enterprise.Customs.GUI
{
	partial class CommonMiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MiscellaneousOptionsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MergeByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaidByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RelatedDeclarationsUserControl = new Enterprise.Customs.GUI.BaseRelatedDeclarationsUserControl();
			this.EntryAuthorisationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RepresentationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefermentAccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MiscellaneousOptionsSeparatorUserControl.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.PaidByDropEdit.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.RelatedDeclarationsUserControl.SuspendLayout();
			this.EntryAuthorisationDateEdit.SuspendLayout();
			this.RepresentationDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// MiscellaneousOptionsSeparatorUserControl
			// 
			this.MiscellaneousOptionsSeparatorUserControl.AllowDrop = true;
			this.MiscellaneousOptionsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5BD660FE-7179-49CF-B679-E39217E79D19", "Miscellaneous Options");
			this.MiscellaneousOptionsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 160, true);
			this.MiscellaneousOptionsSeparatorUserControl.Name = "MiscellaneousOptionsSeparatorUserControl";
			this.MiscellaneousOptionsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.MiscellaneousOptionsSeparatorUserControl.TabIndex = 0;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GS_NKCusAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.CusAgents)));
			this.BrokerCodeFindBox.BindToList = "Lookups+CusAgents";
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 49, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// MergeByDropEdit
			// 
			this.MergeByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MergeByDropEdit, "JE_MergeBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MergeBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.MergeByList)));
			this.MergeByDropEdit.BindToList = "Lookups.MergeByList";
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 73, true);
			this.MergeByDropEdit.Name = "MergeByDropEdit";
			this.MergeByDropEdit.PreBoundMaxLength = 2;
			this.MergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MergeByDropEdit.TabIndex = 2;
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.PaymentPartyList)));
			this.PaymentPartyDropEdit.BindToList = "Lookups.PaymentPartyList";
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 3;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 3;
			// 
			// PaidByDropEdit
			// 
			this.PaidByDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaidByDropEdit, "JE_PaidBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PaidBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.PaidByList)));
			this.PaidByDropEdit.BindToList = "Lookups.PaidByList";
			this.PaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 121, true);
			this.PaidByDropEdit.Name = "PaidByDropEdit";
			this.PaidByDropEdit.PreBoundMaxLength = 3;
			this.PaidByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PaidByDropEdit.TabIndex = 4;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.BranchCollection)));
			this.BranchGuidFindBox.BindToList = "Lookups.BranchCollection";
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 25, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchGuidFindBox.ParentType = null;
			this.BranchGuidFindBox.PreBoundMaxLength = 4;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BranchGuidFindBox.TabIndex = 0;
			// 
			// RelatedDeclarationsUserControl
			// 
			this.RelatedDeclarationsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RelatedDeclarationsUserControl, ".");
			this.RelatedDeclarationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 193, true);
			this.RelatedDeclarationsUserControl.Name = "RelatedDeclarationsUserControl";
			this.RelatedDeclarationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 271, true);
			this.RelatedDeclarationsUserControl.TabIndex = 5;
			// 
			// EntryAuthorisationDateEdit
			// 
			this.EntryAuthorisationDateEdit.AllowDrop = true;
			this.EntryAuthorisationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EntryAuthorisationDateEdit, "JE_EntryAuthorisationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_EntryAuthorisationDate)));
			this.EntryAuthorisationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EntryAuthorisationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(697, 25, true);
			this.EntryAuthorisationDateEdit.Name = "EntryAuthorisationDateEdit";
			this.EntryAuthorisationDateEdit.TabIndex = 13;
			// 
			// RepresentationDropEdit
			// 
			this.RepresentationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentationDropEdit, "JE_DeclarantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DeclarantType)));
			this.RepresentationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 49, true);
			this.RepresentationDropEdit.Name = "RepresentationDropEdit";
			this.RepresentationDropEdit.PreBoundMaxLength = 6;
			this.RepresentationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.RepresentationDropEdit.TabIndex = 17;
			// 
			// DefermentAccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DefermentAccountNumberTextBox, "JE_DefermentAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_DefermentAccountNumber)));
			this.DefermentAccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 73, true);
			this.DefermentAccountNumberTextBox.Name = "DefermentAccountNumberTextBox";
			this.DefermentAccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DefermentAccountNumberTextBox.TabIndex = 9;
			// 
			// CommonMiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RepresentationDropEdit);
			this.Controls.Add(this.EntryAuthorisationDateEdit);
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.MergeByDropEdit);
			this.Controls.Add(this.PaidByDropEdit);
			this.Controls.Add(this.PaymentPartyDropEdit);
			this.Controls.Add(this.BranchGuidFindBox);
			this.Controls.Add(this.MiscellaneousOptionsSeparatorUserControl);
			this.Controls.Add(this.RelatedDeclarationsUserControl);
			this.Controls.Add(this.DefermentAccountNumberTextBox);
			this.Name = "CommonMiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(816, 510, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MiscellaneousOptionsSeparatorUserControl.ResumeLayout(true);
			this.MiscellaneousOptionsSeparatorUserControl.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.PaidByDropEdit.ResumeLayout(true);
			this.PaidByDropEdit.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.RelatedDeclarationsUserControl.ResumeLayout(true);
			this.RelatedDeclarationsUserControl.PerformLayout();
			this.EntryAuthorisationDateEdit.ResumeLayout(true);
			this.EntryAuthorisationDateEdit.PerformLayout();
			this.RepresentationDropEdit.ResumeLayout(true);
			this.RepresentationDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit PaidByDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MergeByDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		internal BaseRelatedDeclarationsUserControl RelatedDeclarationsUserControl;
		internal ZArchitecture.GUI.SeparatorUserControl MiscellaneousOptionsSeparatorUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit EntryAuthorisationDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit RepresentationDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox DefermentAccountNumberTextBox;
	}
}
