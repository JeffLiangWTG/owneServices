namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class TWManifestUserControl
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
			this.GoodsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LoginCompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MailBoxTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeconsolidateVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsLocationCodeFindBox.SuspendLayout();
			this.LoginCompanyGuidFindBox.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader);
			// 
			// GoodsLocationCodeFindBox
			// 
			this.GoodsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationCodeFindBox, "AMA_GoodsLocationFromMasterBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader)(null)).AMA_GoodsLocationFromMasterBill)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.GoodsLocationCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.GoodsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.GoodsLocationCodeFindBox.Name = "GoodsLocationCodeFindBox";
			this.GoodsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GoodsLocationCodeFindBox.ParentType = null;
			this.GoodsLocationCodeFindBox.PreBoundMaxLength = 5;
			this.GoodsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 16, true);
			this.GoodsLocationCodeFindBox.TabIndex = 12;
			// 
			// LoginCompanyGuidFindBox
			// 
			this.LoginCompanyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LoginCompanyGuidFindBox, "AMA_LoginCompanyPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader)(null)).AMA_LoginCompanyPK)));
			this.LoginCompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.LoginCompanyGuidFindBox.Name = "LoginCompanyGuidFindBox";
			this.LoginCompanyGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.LoginCompanyGuidFindBox.ParentType = null;
			this.LoginCompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 16, true);
			this.LoginCompanyGuidFindBox.TabIndex = 23;
			// 
			// MailBoxTextBox
			// 
			this.BindingSource.SetBindingMember(this.MailBoxTextBox, "AMA_MailBox");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader)(null)).AMA_MailBox)));
			this.MailBoxTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MailBoxTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 55, true);
			this.MailBoxTextBox.Name = "MailBoxTextBox";
			this.MailBoxTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 16, true);
			this.MailBoxTextBox.TabIndex = 24;
			// 
			// DeconsolidateVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeconsolidateVATTextBox, "AMA_DeconsolidateVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader)(null)).AMA_DeconsolidateVAT)));
			this.DeconsolidateVATTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DeconsolidateVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 81, true);
			this.DeconsolidateVATTextBox.Name = "DeconsolidateVATTextBox";
			this.DeconsolidateVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 16, true);
			this.DeconsolidateVATTextBox.TabIndex = 25;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "AMA_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Manifest.Business.AsycudaManifestHeader)(null)).AMA_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 107, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 16, true);
			this.MessageStatusDropEdit.TabIndex = 26;
			// 
			// TWManifestUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.DeconsolidateVATTextBox);
			this.Controls.Add(this.MailBoxTextBox);
			this.Controls.Add(this.LoginCompanyGuidFindBox);
			this.Controls.Add(this.GoodsLocationCodeFindBox);
			this.Name = "TWManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsLocationCodeFindBox.ResumeLayout(true);
			this.GoodsLocationCodeFindBox.PerformLayout();
			this.LoginCompanyGuidFindBox.ResumeLayout(true);
			this.LoginCompanyGuidFindBox.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZCodeFindBox GoodsLocationCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox LoginCompanyGuidFindBox;
		internal ZArchitecture.ZTextBox MailBoxTextBox;
		internal ZArchitecture.ZTextBox DeconsolidateVATTextBox;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
	}
}
