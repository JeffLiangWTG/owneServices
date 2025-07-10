namespace Enterprise.MasterFiles.Module
{
	partial class UpdateExpiryNotificationPeriodControl
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
            this.ExpiryNotificationPeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.OverrideNonZeroExpiryNotificationPeriodCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ClientFindBox.SuspendLayout();
            this.WarehouseFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.UpdateExpiryNotificationPeriodControl);
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
            // ExpiryNotificationPeriodCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ExpiryNotificationPeriodCalcEdit, "ExpiryNotificationPeriod");
            this.ExpiryNotificationPeriodCalcEdit.CaptionResourceString = null;
            this.ExpiryNotificationPeriodCalcEdit.DecimalPlaces = 0;
            this.ExpiryNotificationPeriodCalcEdit.Decimals = 0;
            this.ExpiryNotificationPeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 73, true);
            this.ExpiryNotificationPeriodCalcEdit.Name = "ExpiryNotificationPeriodCalcEdit";
            this.ExpiryNotificationPeriodCalcEdit.ShouldEscapeAllSpecialCharacters = false;
            this.ExpiryNotificationPeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.ExpiryNotificationPeriodCalcEdit.TabIndex = 2;
            this.ExpiryNotificationPeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // OverrideNonZeroExpiryNotificationPeriodCheckBox
            // 
            this.BindingSource.SetBindingMember(this.OverrideNonZeroExpiryNotificationPeriodCheckBox, "OverrideNonZeroExpiryNotificationPeriod");
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 99, true);
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.Name = "OverrideNonZeroExpiryNotificationPeriodCheckBox";
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 17, true);
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.AutoSize = true;
			this.OverrideNonZeroExpiryNotificationPeriodCheckBox.TabIndex = 1;
			// 
			// UpdateExpiryNotificationPeriodControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.OverrideNonZeroExpiryNotificationPeriodCheckBox);
            this.Controls.Add(this.ExpiryNotificationPeriodCalcEdit);
            this.Controls.Add(this.WarehouseFindBox);
            this.Controls.Add(this.ClientFindBox);
            this.Name = "UpdateExpiryNotificationPeriodControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 174, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ClientFindBox.ResumeLayout(true);
            this.ClientFindBox.PerformLayout();
            this.WarehouseFindBox.ResumeLayout(true);
            this.WarehouseFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox ClientFindBox;
		private ZArchitecture.GUI.ZGuidFindBox WarehouseFindBox;
		private ZArchitecture.ZCalcEdit ExpiryNotificationPeriodCalcEdit;
		private ZArchitecture.GUI.ZCheckBox OverrideNonZeroExpiryNotificationPeriodCheckBox;
	}
}
