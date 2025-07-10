namespace Enterprise.MasterFiles.Module
{
    partial class OrgLocatedWithinModuleFilterControl
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
			this.UnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FilterTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrgzGuidFindBoxWithSelectedEvent = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxWithSelectedEvent();
			this.UNLOCOzCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxWithSelectedEvent();
			this.AddressPKzDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.DistanceTextBox = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnitDropEdit.SuspendLayout();
			this.FilterTypeDropEdit.SuspendLayout();
			this.OrgzGuidFindBoxWithSelectedEvent.SuspendLayout();
			this.UNLOCOzCodeFindBox.SuspendLayout();
			this.AddressPKzDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DistanceTextBox)).BeginInit();
			this.DistanceTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter);
			// 
			// UnitDropEdit
			// 
			this.UnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitDropEdit, "UnitForGeolocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).UnitForGeolocation)));
			this.UnitDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("83d3eb12-c296-4835-b801-ad88a833f0fd", "Unit");
			this.UnitDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(538, 1, true);
			this.UnitDropEdit.Name = "UnitDropEdit";
			this.UnitDropEdit.PreBoundMaxLength = 4;
			this.UnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.UnitDropEdit.TabIndex = 9;
			// 
			// FilterTypeDropEdit
			// 
			this.FilterTypeDropEdit.AllowDrop = true;
			this.FilterTypeDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.FilterTypeDropEdit, "FilterType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).FilterType)));
			this.FilterTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("25d8cb1d-966a-4ca3-b915-75a0ed37fe39", "Type");
			this.FilterTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FilterTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 1, true);
			this.FilterTypeDropEdit.Name = "FilterTypeDropEdit";
			this.FilterTypeDropEdit.PreBoundMaxLength = 10;
			this.FilterTypeDropEdit.ShowDescriptionBox = false;
			this.FilterTypeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.FilterTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.FilterTypeDropEdit.TabIndex = 5;
			this.FilterTypeDropEdit.SelectedIndexChanged += new System.EventHandler(this.FilterTypeDropEdit_SelectedIndexChanged);
			// 
			// OrgzGuidFindBoxWithSelectedEvent
			// 
			this.OrgzGuidFindBoxWithSelectedEvent.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgzGuidFindBoxWithSelectedEvent, "OrgPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).OrgPK)));
			this.OrgzGuidFindBoxWithSelectedEvent.IsPrimaryKeyFromCodeRequired = false;
			this.OrgzGuidFindBoxWithSelectedEvent.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 27, true);
			this.OrgzGuidFindBoxWithSelectedEvent.Name = "OrgzGuidFindBoxWithSelectedEvent";
			this.OrgzGuidFindBoxWithSelectedEvent.ShouldResize = true;
			this.OrgzGuidFindBoxWithSelectedEvent.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(172, 20, true);
			this.OrgzGuidFindBoxWithSelectedEvent.TabIndex = 7;
			// 
			// UNLOCOzCodeFindBox
			// 
			this.UNLOCOzCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNLOCOzCodeFindBox, "UNLOCO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).UNLOCO)));
			this.UNLOCOzCodeFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.UNLOCOzCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 27, true);
			this.UNLOCOzCodeFindBox.Name = "UNLOCOzCodeFindBox";
			this.UNLOCOzCodeFindBox.PreBoundMaxLength = 8;
			this.UNLOCOzCodeFindBox.ShouldResize = true;
			this.UNLOCOzCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 20, true);
			this.UNLOCOzCodeFindBox.TabIndex = 11;
			// 
			// AddressPKzDropEdit
			// 
			this.AddressPKzDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AddressPKzDropEdit, "AddressPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).AddressPK)));
			this.AddressPKzDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(403, 27, true);
			this.AddressPKzDropEdit.Name = "AddressPKzDropEdit";
			this.AddressPKzDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 20, true);
			this.AddressPKzDropEdit.TabIndex = 12;
			// 
			// DistanceTextBox
			// 
			this.BindingSource.SetBindingMember(this.DistanceTextBox, "Distance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.MasterFiles.Module.OrgLocatedWithinModuleFilter)(null)).Distance)));
			this.DistanceTextBox.BindTo = "Distance";
			this.DistanceTextBox.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("4b8ae435-26dd-4866-bf5e-645649468a30", "Distance");
			this.DistanceTextBox.DecimalPlaces = 2;
			this.DistanceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 2, true);
			this.DistanceTextBox.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
			this.DistanceTextBox.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.DistanceTextBox.Name = "DistanceTextBox";
			this.DistanceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.DistanceTextBox.TabIndex = 13;
			this.DistanceTextBox.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// OrgLocatedWithinModuleFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DistanceTextBox);
			this.Controls.Add(this.AddressPKzDropEdit);
			this.Controls.Add(this.UNLOCOzCodeFindBox);
			this.Controls.Add(this.OrgzGuidFindBoxWithSelectedEvent);
			this.Controls.Add(this.FilterTypeDropEdit);
			this.Controls.Add(this.UnitDropEdit);
			this.Name = "OrgLocatedWithinModuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 51, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnitDropEdit.ResumeLayout(true);
			this.UnitDropEdit.PerformLayout();
			this.FilterTypeDropEdit.ResumeLayout(true);
			this.FilterTypeDropEdit.PerformLayout();
			this.OrgzGuidFindBoxWithSelectedEvent.ResumeLayout(true);
			this.OrgzGuidFindBoxWithSelectedEvent.PerformLayout();
			this.UNLOCOzCodeFindBox.ResumeLayout(true);
			this.UNLOCOzCodeFindBox.PerformLayout();
			this.AddressPKzDropEdit.ResumeLayout(true);
			this.AddressPKzDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DistanceTextBox)).EndInit();
			this.DistanceTextBox.ResumeLayout(false);
			this.DistanceTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit UnitDropEdit;
		private ZArchitecture.GUI.ZDropEdit FilterTypeDropEdit;
		private ZArchitecture.GUI.ZGuidFindBoxWithSelectedEvent OrgzGuidFindBoxWithSelectedEvent;
		private ZArchitecture.GUI.ZGuidFindBoxWithSelectedEvent UNLOCOzCodeFindBox;
		private ZArchitecture.GUI.ZGuidDropEdit AddressPKzDropEdit;
		private ZArchitecture.GUI.ZNumericUpDown DistanceTextBox;
	}
}
