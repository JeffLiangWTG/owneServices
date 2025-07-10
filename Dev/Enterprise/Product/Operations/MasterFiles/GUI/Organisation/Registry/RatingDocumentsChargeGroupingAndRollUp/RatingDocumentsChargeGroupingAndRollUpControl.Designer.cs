using System.ComponentModel;

namespace Enterprise.MasterFiles.GUI
{
    partial class RatingDocumentsChargeGroupingAndRollUpControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.GroupChargesBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DisplayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GroupChargesGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.GroupChargesBox.SuspendLayout();
            this.ModuleDropEdit.SuspendLayout();
            this.TransportModeDropEdit.SuspendLayout();
            this.StyleDropEdit.SuspendLayout();
            this.DisplayDropEdit.SuspendLayout();
            this.JobTypeDropEdit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).BeginInit();
            this.GroupChargesGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistryCollection);
            // 
            // GroupChargesBox
            // 
            this.GroupChargesBox.AutoSize = true;
            this.GroupChargesBox.Controls.Add(this.ModuleDropEdit);
            this.GroupChargesBox.Controls.Add(this.TransportModeDropEdit);
            this.GroupChargesBox.Controls.Add(this.StyleDropEdit);
            this.GroupChargesBox.Controls.Add(this.DisplayDropEdit);
            this.GroupChargesBox.Controls.Add(this.JobTypeDropEdit);
            this.GroupChargesBox.Controls.Add(this.GroupChargesGrid);
            this.GroupChargesBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GroupChargesBox, false);
            this.GroupChargesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GroupChargesBox.Name = "GroupChargesBox";
            this.GroupChargesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 274, true);
            this.GroupChargesBox.TabIndex = 6;
            this.GroupChargesBox.TabStop = false;
            // 
            // ModuleDropEdit
            // 
            this.ModuleDropEdit.AllowDrop = true;
            this.ModuleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ModuleDropEdit, "Module");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Module)));
			this.ModuleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|8d0b035d-633c-4183-9f55-6d1cb009518d", "Module");
			this.ModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 88, true);
            this.ModuleDropEdit.Name = "ModuleDropEdit";
            this.ModuleDropEdit.PreBoundMaxLength = 3;
            this.ModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
            this.ModuleDropEdit.TabIndex = 1;
            // 
            // TransportModeDropEdit
            // 
            this.TransportModeDropEdit.AllowDrop = true;
            this.TransportModeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.TransportModeDropEdit, "TransportMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).TransportMode)));
			this.TransportModeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|0942afa7-7202-43d5-8425-4bc1a1dee2a9", "Mode");
			this.TransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 141, true);
            this.TransportModeDropEdit.Name = "TransportModeDropEdit";
            this.TransportModeDropEdit.PreBoundMaxLength = 3;
            this.TransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
            this.TransportModeDropEdit.TabIndex = 3;
            // 
            // StyleDropEdit
            // 
            this.StyleDropEdit.AllowDrop = true;
            this.StyleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.StyleDropEdit, "Style");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Style)));
			this.StyleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|6523891a-e469-4631-bf18-7bfc9fa92270", "Style");
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 191, true);
            this.StyleDropEdit.Name = "StyleDropEdit";
            this.StyleDropEdit.PreBoundMaxLength = 3;
            this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
            this.StyleDropEdit.TabIndex = 5;
            // 
            // DisplayDropEdit
            // 
            this.DisplayDropEdit.AllowDrop = true;
            this.DisplayDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.DisplayDropEdit, "Display");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Display)));
			this.DisplayDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|05285a42-e8c1-43e0-b26e-9fe6ab4d272e", "Display");
			this.DisplayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 167, true);
            this.DisplayDropEdit.Name = "DisplayDropEdit";
            this.DisplayDropEdit.PreBoundMaxLength = 3;
            this.DisplayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
            this.DisplayDropEdit.TabIndex = 4;
            // 
            // JobTypeDropEdit
            // 
            this.JobTypeDropEdit.AllowDrop = true;
            this.JobTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.JobTypeDropEdit, "JobType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).JobType)));
			this.JobTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|d485ddd9-1e52-4014-9077-ca4c8c67258c", "Job Type");
			this.JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 114, true);
            this.JobTypeDropEdit.Name = "JobTypeDropEdit";
            this.JobTypeDropEdit.PreBoundMaxLength = 3;
            this.JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
            this.JobTypeDropEdit.TabIndex = 2;
            // 
            // GroupChargesGrid
            // 
            this.GroupChargesGrid.AllowNavigation = false;
            this.GroupChargesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.GroupChargesGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Module)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).JobType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).TransportMode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Display)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RatingDocRollupOrGroupRegistry)(null)).Style)));
            this.GroupChargesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|f924d1a0-9215-4367-8451-1d04ffa2e6e1", "Module");
			zDropEditColumnStyleInfo1.ColumnName = "Module";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|8d5b16e1-b472-4762-820e-8559d527ab7a", "Job Type");
			zDropEditColumnStyleInfo2.ColumnName = "JobType";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|f5732fb5-3993-4eba-9187-5502b93b0cf5", "Mode");
			zDropEditColumnStyleInfo3.ColumnName = "TransportMode";
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|4415e20c-b599-4703-8867-8f1e92e61521", "Display");
			zDropEditColumnStyleInfo4.ColumnName = "Display";
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RatingDocumentsChargeGroupingAndRollUpControl|e24403bb-8c3f-427a-af7f-929730514c8f", "Style");
			zDropEditColumnStyleInfo5.ColumnName = "Style";
            zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.GroupChargesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
            this.GroupChargesGrid.GridId = "83e70023-a6b1-4ef0-b5c7-c13aa81ec46a";
            this.GroupChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GroupChargesGrid.LayoutKey = "GroupChargesGrid";
            this.GroupChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.GroupChargesGrid.Name = "GroupChargesGrid";
            this.GroupChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 66, true);
            this.GroupChargesGrid.TabIndex = 0;
            // 
            // RatingDocumentsChargeGroupingAndRollUpControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.GroupChargesBox);
            this.Name = "RatingDocumentsChargeGroupingAndRollUpControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 274, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.GroupChargesBox.ResumeLayout(false);
            this.GroupChargesBox.PerformLayout();
            this.ModuleDropEdit.ResumeLayout(true);
            this.ModuleDropEdit.PerformLayout();
            this.TransportModeDropEdit.ResumeLayout(true);
            this.TransportModeDropEdit.PerformLayout();
            this.StyleDropEdit.ResumeLayout(true);
            this.StyleDropEdit.PerformLayout();
            this.DisplayDropEdit.ResumeLayout(true);
            this.DisplayDropEdit.PerformLayout();
            this.JobTypeDropEdit.ResumeLayout(true);
            ((System.ComponentModel.ISupportInitialize)(this.GroupChargesGrid)).EndInit();
            this.GroupChargesGrid.ResumeLayout(false);
            this.GroupChargesGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Enterprise.ZArchitecture.GUI.ZGroupBox GroupChargesBox;
        private Enterprise.ZArchitecture.GUI.ZDropEdit ModuleDropEdit;
        private Enterprise.ZArchitecture.GUI.ZDropEdit JobTypeDropEdit;
        private Enterprise.ZArchitecture.GUI.ZDropEdit TransportModeDropEdit;
        private Enterprise.ZArchitecture.GUI.ZDropEdit DisplayDropEdit;
        private Enterprise.ZArchitecture.GUI.ZDropEdit StyleDropEdit;
        private Enterprise.ZArchitecture.ZGrid GroupChargesGrid;
    }
}
