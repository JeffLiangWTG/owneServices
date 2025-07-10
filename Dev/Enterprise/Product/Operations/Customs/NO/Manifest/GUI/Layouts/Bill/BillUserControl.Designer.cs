namespace Enterprise.Customs.NO.Manifest.GUI
{
	partial class BillUserControl
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
		void InitializeComponent()
		{
            this.ImportProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExportProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EmailAddressControl = new Enterprise.Customs.NO.Manifest.GUI.EmailAddressesUserControl();
            this.TransportDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PlaceOfAcceptancePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.PlaceOfAcceptanceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PlaceOfAcceptanceCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PlaceOfLoadingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.PlaceOfLoadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PlaceOfLoadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PlaceOfUnloadingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.PlaceOfUnloadingTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PlaceOfUnloadingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.PlaceOfDeliveryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.PlaceOfDeliveryTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PlaceOfDeliveryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ImportProcedureDropEdit.SuspendLayout();
            this.ExportProcedureDropEdit.SuspendLayout();
            this.EmailAddressControl.SuspendLayout();
            this.TransportDocumentTypeDropEdit.SuspendLayout();
            this.PlaceOfAcceptancePanel.SuspendLayout();
            this.PlaceOfAcceptanceCodeFindBox.SuspendLayout();
            this.PlaceOfLoadingPanel.SuspendLayout();
            this.PlaceOfLoadingCodeFindBox.SuspendLayout();
            this.PlaceOfUnloadingPanel.SuspendLayout();
            this.PlaceOfUnloadingCodeFindBox.SuspendLayout();
            this.PlaceOfDeliveryPanel.SuspendLayout();
            this.PlaceOfDeliveryCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Manifest.Business.AsycudaBill);
            // 
            // ImportProcedureDropEdit
            // 
            this.ImportProcedureDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ImportProcedureDropEdit, "ImportProcedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ImportProcedure)));
            this.ImportProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 14, true);
            this.ImportProcedureDropEdit.Name = "ImportProcedureDropEdit";
            this.ImportProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
            this.ImportProcedureDropEdit.TabIndex = 1;
            // 
            // ExportProcedureDropEdit
            // 
            this.ExportProcedureDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ExportProcedureDropEdit, "ExportProcedure");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ExportProcedure)));
            this.ExportProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
            this.ExportProcedureDropEdit.Name = "ExportProcedureDropEdit";
            this.ExportProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
            this.ExportProcedureDropEdit.TabIndex = 2;
            // 
            // EmailAddressControl
            // 
            this.EmailAddressControl.AllowDrop = true;
            this.EmailAddressControl.AutoSize = true;
            this.BindingSource.SetBindingMember(this.EmailAddressControl, ".");
			this.EmailAddressControl.CaptionRenderingEnabled = true;
			this.EmailAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 60, true);
            this.EmailAddressControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 10, true);
            this.EmailAddressControl.Name = "EmailAddressControl";
            this.EmailAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 125, true);
            this.EmailAddressControl.TabIndex = 3;
            // 
            // TransportDocumentTypeDropEdit
            // 
            this.TransportDocumentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportDocumentTypeDropEdit, "TransportDocumentType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).TransportDocumentType)));
            this.TransportDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 182, true);
            this.TransportDocumentTypeDropEdit.Name = "TransportDocumentTypeDropEdit";
            this.TransportDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
            this.TransportDocumentTypeDropEdit.TabIndex = 5;
            // 
            // PlaceOfAcceptancePanel
            // 
            this.PlaceOfAcceptancePanel.Controls.Add(this.PlaceOfAcceptanceTextBox);
            this.PlaceOfAcceptancePanel.Controls.Add(this.PlaceOfAcceptanceCodeFindBox);
            this.PlaceOfAcceptancePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 209, true);
            this.PlaceOfAcceptancePanel.Name = "PlaceOfAcceptancePanel";
            this.PlaceOfAcceptancePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.PlaceOfAcceptancePanel.TabIndex = 6;
            // 
            // PlaceOfAcceptanceTextBox
            // 
            this.BindingSource.SetBindingMember(this.PlaceOfAcceptanceTextBox, "ABL_CustomsOriginPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_CustomsOriginPort)));
            this.PlaceOfAcceptanceTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlaceOfAcceptanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
            this.PlaceOfAcceptanceTextBox.Name = "PlaceOfAcceptanceTextBox";
            this.PlaceOfAcceptanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 15, true);
            this.PlaceOfAcceptanceTextBox.TabIndex = 8;
            // 
            // PlaceOfAcceptanceCodeFindBox
            // 
            this.PlaceOfAcceptanceCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfAcceptanceCodeFindBox, "ABL_RL_NKOrigin");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKOrigin)));
            this.PlaceOfAcceptanceCodeFindBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PlaceOfAcceptanceCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PlaceOfAcceptanceCodeFindBox.Name = "PlaceOfAcceptanceCodeFindBox";
            this.PlaceOfAcceptanceCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfAcceptanceCodeFindBox.ParentType = null;
            this.PlaceOfAcceptanceCodeFindBox.ShowDescriptionBox = false;
            this.PlaceOfAcceptanceCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.PlaceOfAcceptanceCodeFindBox.TabIndex = 7;
            // 
            // PlaceOfLoadingPanel
            // 
            this.PlaceOfLoadingPanel.Controls.Add(this.PlaceOfLoadingTextBox);
            this.PlaceOfLoadingPanel.Controls.Add(this.PlaceOfLoadingCodeFindBox);
            this.PlaceOfLoadingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 264, true);
            this.PlaceOfLoadingPanel.Name = "PlaceOfLoadingPanel";
            this.PlaceOfLoadingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.PlaceOfLoadingPanel.TabIndex = 9;
            // 
            // PlaceOfLoadingTextBox
            // 
            this.BindingSource.SetBindingMember(this.PlaceOfLoadingTextBox, "ABL_CustomsLoadPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_CustomsLoadPort)));
            this.PlaceOfLoadingTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlaceOfLoadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
            this.PlaceOfLoadingTextBox.Name = "PlaceOfLoadingTextBox";
            this.PlaceOfLoadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 15, true);
            this.PlaceOfLoadingTextBox.TabIndex = 11;
            // 
            // PlaceOfLoadingCodeFindBox
            // 
            this.PlaceOfLoadingCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfLoadingCodeFindBox, "ABL_RL_NKPortOfLoading");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKPortOfLoading)));
            this.PlaceOfLoadingCodeFindBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PlaceOfLoadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PlaceOfLoadingCodeFindBox.Name = "PlaceOfLoadingCodeFindBox";
            this.PlaceOfLoadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfLoadingCodeFindBox.ParentType = null;
            this.PlaceOfLoadingCodeFindBox.ShowDescriptionBox = false;
            this.PlaceOfLoadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.PlaceOfLoadingCodeFindBox.TabIndex = 10;
            // 
            // PlaceOfUnloadingPanel
            // 
            this.PlaceOfUnloadingPanel.Controls.Add(this.PlaceOfUnloadingTextBox);
            this.PlaceOfUnloadingPanel.Controls.Add(this.PlaceOfUnloadingCodeFindBox);
            this.PlaceOfUnloadingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 290, true);
            this.PlaceOfUnloadingPanel.Name = "PlaceOfUnloadingPanel";
            this.PlaceOfUnloadingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.PlaceOfUnloadingPanel.TabIndex = 12;
            // 
            // PlaceOfUnloadingTextBox
            // 
            this.BindingSource.SetBindingMember(this.PlaceOfUnloadingTextBox, "ABL_CustomsDischargePort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_CustomsDischargePort)));
            this.PlaceOfUnloadingTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlaceOfUnloadingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
            this.PlaceOfUnloadingTextBox.Name = "PlaceOfUnloadingTextBox";
            this.PlaceOfUnloadingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 15, true);
            this.PlaceOfUnloadingTextBox.TabIndex = 14;
            // 
            // PlaceOfUnloadingCodeFindBox
            // 
            this.PlaceOfUnloadingCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfUnloadingCodeFindBox, "ABL_RL_NKPortOfDischarge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKPortOfDischarge)));
            this.PlaceOfUnloadingCodeFindBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PlaceOfUnloadingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PlaceOfUnloadingCodeFindBox.Name = "PlaceOfUnloadingCodeFindBox";
            this.PlaceOfUnloadingCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfUnloadingCodeFindBox.ParentType = null;
            this.PlaceOfUnloadingCodeFindBox.ShowDescriptionBox = false;
            this.PlaceOfUnloadingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.PlaceOfUnloadingCodeFindBox.TabIndex = 13;
            // 
            // PlaceOfDeliveryPanel
            // 
            this.PlaceOfDeliveryPanel.Controls.Add(this.PlaceOfDeliveryTextBox);
            this.PlaceOfDeliveryPanel.Controls.Add(this.PlaceOfDeliveryCodeFindBox);
            this.PlaceOfDeliveryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 236, true);
            this.PlaceOfDeliveryPanel.Name = "PlaceOfDeliveryPanel";
            this.PlaceOfDeliveryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.PlaceOfDeliveryPanel.TabIndex = 15;
            // 
            // PlaceOfDeliveryTextBox
            // 
            this.BindingSource.SetBindingMember(this.PlaceOfDeliveryTextBox, "ABL_CustomsFinalDestinationPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_CustomsFinalDestinationPort)));
            this.PlaceOfDeliveryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PlaceOfDeliveryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
            this.PlaceOfDeliveryTextBox.Name = "PlaceOfDeliveryTextBox";
            this.PlaceOfDeliveryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 15, true);
            this.PlaceOfDeliveryTextBox.TabIndex = 17;
            // 
            // PlaceOfDeliveryCodeFindBox
            // 
            this.PlaceOfDeliveryCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.PlaceOfDeliveryCodeFindBox, "ABL_RL_NKFinalDestination");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKFinalDestination)));
            this.PlaceOfDeliveryCodeFindBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.PlaceOfDeliveryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PlaceOfDeliveryCodeFindBox.Name = "PlaceOfDeliveryCodeFindBox";
            this.PlaceOfDeliveryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.PlaceOfDeliveryCodeFindBox.ParentType = null;
            this.PlaceOfDeliveryCodeFindBox.ShowDescriptionBox = false;
            this.PlaceOfDeliveryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.PlaceOfDeliveryCodeFindBox.TabIndex = 16;
            // 
            // BillUserControl
            // 
            this.Controls.Add(this.PlaceOfDeliveryPanel);
            this.Controls.Add(this.PlaceOfLoadingPanel);
            this.Controls.Add(this.PlaceOfUnloadingPanel);
            this.Controls.Add(this.PlaceOfAcceptancePanel);
            this.Controls.Add(this.TransportDocumentTypeDropEdit);
            this.Controls.Add(this.ImportProcedureDropEdit);
            this.Controls.Add(this.ExportProcedureDropEdit);
            this.Controls.Add(this.EmailAddressControl);
            this.Name = "BillUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 334, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ImportProcedureDropEdit.ResumeLayout(true);
            this.ImportProcedureDropEdit.PerformLayout();
            this.ExportProcedureDropEdit.ResumeLayout(true);
            this.ExportProcedureDropEdit.PerformLayout();
            this.EmailAddressControl.ResumeLayout(true);
            this.EmailAddressControl.PerformLayout();
            this.TransportDocumentTypeDropEdit.ResumeLayout(true);
            this.TransportDocumentTypeDropEdit.PerformLayout();
            this.PlaceOfAcceptancePanel.ResumeLayout(false);
            this.PlaceOfAcceptancePanel.PerformLayout();
            this.PlaceOfAcceptanceCodeFindBox.ResumeLayout(true);
            this.PlaceOfAcceptanceCodeFindBox.PerformLayout();
            this.PlaceOfLoadingPanel.ResumeLayout(false);
            this.PlaceOfLoadingPanel.PerformLayout();
            this.PlaceOfLoadingCodeFindBox.ResumeLayout(true);
            this.PlaceOfLoadingCodeFindBox.PerformLayout();
            this.PlaceOfUnloadingPanel.ResumeLayout(false);
            this.PlaceOfUnloadingPanel.PerformLayout();
            this.PlaceOfUnloadingCodeFindBox.ResumeLayout(true);
            this.PlaceOfUnloadingCodeFindBox.PerformLayout();
            this.PlaceOfDeliveryPanel.ResumeLayout(false);
            this.PlaceOfDeliveryPanel.PerformLayout();
            this.PlaceOfDeliveryCodeFindBox.ResumeLayout(true);
            this.PlaceOfDeliveryCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit ImportProcedureDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ExportProcedureDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TransportDocumentTypeDropEdit;
		internal ZArchitecture.GUI.ZPanel PlaceOfAcceptancePanel;
		private ZArchitecture.ZTextBox PlaceOfAcceptanceTextBox;
		private ZArchitecture.GUI.ZCodeFindBox PlaceOfAcceptanceCodeFindBox;
		internal ZArchitecture.GUI.ZPanel PlaceOfDeliveryPanel;
		private ZArchitecture.GUI.ZCodeFindBox PlaceOfDeliveryCodeFindBox;
		private ZArchitecture.ZTextBox PlaceOfDeliveryTextBox;
		internal EmailAddressesUserControl EmailAddressControl;
		internal ZArchitecture.GUI.ZPanel PlaceOfLoadingPanel;
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfLoadingCodeFindBox;
		private ZArchitecture.ZTextBox PlaceOfLoadingTextBox;
		internal ZArchitecture.GUI.ZPanel PlaceOfUnloadingPanel;
		internal ZArchitecture.GUI.ZCodeFindBox PlaceOfUnloadingCodeFindBox;
		private ZArchitecture.ZTextBox PlaceOfUnloadingTextBox;
	}
}
