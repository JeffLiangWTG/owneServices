using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class WebURLsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.WebURLsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OrgWebURLsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WebURLsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgWebURLsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// WebURLsGroupBox
			// 
			this.WebURLsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WebURLsUserControl|9b807b9e-933e-4f6a-a9ad-5c81e4d26487", "Organization Web URLs");
			this.WebURLsGroupBox.Controls.Add(this.OrgWebURLsGrid);
			this.WebURLsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WebURLsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WebURLsGroupBox.Name = "WebURLsGroupBox";
			this.WebURLsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 377, true);
			this.WebURLsGroupBox.TabIndex = 0;
			this.WebURLsGroupBox.TabStop = false;
			// 
			// OrgWebURLsGrid
			// 
			this.OrgWebURLsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgWebURLsGrid, "OrgWebURLs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWebURLs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWebURL)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWebURLs)).SyncRoot)).PU_URL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWebURL)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWebURLs)).SyncRoot)).PU_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgWebURL)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWebURLs)).SyncRoot)).PU_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgWebURL)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).OrgWebURLs)).SyncRoot)).PU_IsPrimary)));
			this.OrgWebURLsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "PU_URL";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ColumnName = "PU_Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.ColumnName = "PU_Description";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.ColumnName = "PU_IsPrimary";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.OrgWebURLsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgWebURLsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgWebURLsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgWebURLsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrgWebURLsGrid.GridId = "d526df44-7b83-4a15-afae-281849aecb89";
			this.OrgWebURLsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgWebURLsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgWebURLsGrid.LayoutKey = "OrgWebURLsGrid";
			this.OrgWebURLsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrgWebURLsGrid.Name = "OrgWebURLsGrid";
			this.OrgWebURLsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 358, true);
			this.OrgWebURLsGrid.TabIndex = 11;
			// 
			// WebURLsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.WebURLsGroupBox);
			this.Name = "WebURLsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 377, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WebURLsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrgWebURLsGrid)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox WebURLsGroupBox;
		protected Enterprise.ZArchitecture.ZGrid OrgWebURLsGrid;
    }
}
