namespace Enterprise.Customs.GUI
{
	partial class BaseMiscOptionsUserControl
	{
		private System.ComponentModel.Container components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.MiscOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.MergeByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaidByDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseMiscOptionsUserControl|e24ee687-3b21-4bb4-b3e8-eca38b3a2fa3", "Miscellaneous Options");
			this.MiscOptionsGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.MiscOptionsGroupBox.Controls.Add(this.MergeByDropEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.PaymentPartyDropEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.PaidByDropEdit);
			this.MiscOptionsGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.MiscOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MiscOptionsGroupBox.Name = "MiscOptionsGroupBox";
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 144, true);
			this.MiscOptionsGroupBox.TabIndex = 0;
			this.MiscOptionsGroupBox.TabStop = false;
			// 
			// BrokerCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GS_NKCusAgent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.CusAgents)));
			this.BrokerCodeFindBox.BindToList = "Lookups+CusAgents";
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 49, true);
			this.BrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BrokerCodeFindBox.TabIndex = 3;
			// 
			// MergeByDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MergeByDropEdit, "JE_MergeBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_MergeBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.MergeByList)));
			this.MergeByDropEdit.BindToList = "Lookups.MergeByList";
			this.MergeByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 73, true);
			this.MergeByDropEdit.Name = "MergeByDropEdit";
			this.MergeByDropEdit.PreBoundMaxLength = 3;
			this.MergeByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.MergeByDropEdit.TabIndex = 5;
			// 
			// PaymentPartyDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PaymentMethod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.PaymentPartyList)));
			this.PaymentPartyDropEdit.BindToList = "Lookups.PaymentPartyList";
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 97, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 3;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 7;
			// 
			// PaidByDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidByDropEdit, "JE_PaidBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_PaidBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.PaidByList)));
			this.PaidByDropEdit.BindToList = "Lookups.PaidByList";
			this.PaidByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 121, true);
			this.PaidByDropEdit.Name = "PaidByDropEdit";
			this.PaidByDropEdit.PreBoundMaxLength = 3;
			this.PaidByDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.PaidByDropEdit.TabIndex = 8;
			// 
			// BranchGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).Lookups.BranchCollection)));
			this.BranchGuidFindBox.BindToList = "Lookups.BranchCollection";
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 25, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BranchGuidFindBox.TabIndex = 1;
			// 
			// BaseMiscOptionsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MiscOptionsGroupBox);
			this.Name = "BaseMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

		protected Enterprise.ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit PaidByDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox MiscOptionsGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit MergeByDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
	}
}
