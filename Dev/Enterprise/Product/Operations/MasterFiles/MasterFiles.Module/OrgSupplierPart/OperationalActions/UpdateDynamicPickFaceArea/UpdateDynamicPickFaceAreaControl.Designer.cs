namespace Enterprise.MasterFiles.Module
{
	partial class UpdateDynamicPickFaceAreaControl
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
            this.ClientFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.WarehouseFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.DynamicPickAreaFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.OverrideNonEmptyDynamicPickAreaCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ClientFindBox.SuspendLayout();
            this.WarehouseFindBox.SuspendLayout();
            this.DynamicPickAreaFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.UpdateDynamicPickFaceAreaControl);
            // 
            // ClientFindBox
            // 
            this.ClientFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ClientFindBox, "ClientPK");
            this.ClientFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 21, true);
            this.ClientFindBox.Name = "ClientFindBox";
            this.ClientFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ClientFindBox.ParentType = null;
            this.ClientFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.ClientFindBox.TabIndex = 0;
            // 
            // WarehouseFindBox
            // 
            this.WarehouseFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.WarehouseFindBox, "WarehousePK");
            this.WarehouseFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 47, true);
            this.WarehouseFindBox.Name = "WarehouseFindBox";
            this.WarehouseFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.WarehouseFindBox.ParentType = null;
            this.WarehouseFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.WarehouseFindBox.TabIndex = 1;
            // 
            // DynamicPickAreaFindBox
            // 
            this.DynamicPickAreaFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DynamicPickAreaFindBox, "DynamicPickAreaPK");
            this.DynamicPickAreaFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 73, true);
            this.DynamicPickAreaFindBox.Name = "DynamicPickAreaFindBox";
            this.DynamicPickAreaFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DynamicPickAreaFindBox.ParentType = null;
            this.DynamicPickAreaFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.DynamicPickAreaFindBox.TabIndex = 2;
            // 
            // OverrideNonEmptyDynamicPickAreaCheckBox
            // 
            this.OverrideNonEmptyDynamicPickAreaCheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.OverrideNonEmptyDynamicPickAreaCheckBox, "OverrideNonEmptyDynamicPickArea");
            this.OverrideNonEmptyDynamicPickAreaCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 99, true);
            this.OverrideNonEmptyDynamicPickAreaCheckBox.Name = "OverrideNonEmptyDynamicPickAreaCheckBox";
            this.OverrideNonEmptyDynamicPickAreaCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
            this.OverrideNonEmptyDynamicPickAreaCheckBox.TabIndex = 3;
            // 
            // UpdateDynamicPickFaceAreaControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.OverrideNonEmptyDynamicPickAreaCheckBox);
            this.Controls.Add(this.DynamicPickAreaFindBox);
            this.Controls.Add(this.WarehouseFindBox);
            this.Controls.Add(this.ClientFindBox);
            this.Name = "UpdateDynamicPickFaceAreaControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 174, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ClientFindBox.ResumeLayout(true);
            this.ClientFindBox.PerformLayout();
            this.WarehouseFindBox.ResumeLayout(true);
            this.WarehouseFindBox.PerformLayout();
            this.DynamicPickAreaFindBox.ResumeLayout(true);
            this.DynamicPickAreaFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox ClientFindBox;
		private ZArchitecture.GUI.ZGuidFindBox WarehouseFindBox;
		private ZArchitecture.GUI.ZGuidFindBox DynamicPickAreaFindBox;
		private ZArchitecture.GUI.ZCheckBox OverrideNonEmptyDynamicPickAreaCheckBox;
	}
}
