namespace Enterprise.Customs.ZA.DataRegistry.GUI
{
    partial class CustomsDSBCreditorOverrideUserControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            this.MainGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainGrid)).BeginInit();
            this.MainGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.DataRegistry.Business.CustomsDSBCreditorOverrideCollection);
            // 
            // MainGrid
            // 
            this.MainGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.MainGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.DataRegistry.Business.CustomsDSBCreditorOverride)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ZA.DataRegistry.Business.CustomsDSBCreditorOverride)(null)).DistrictOfficeCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ZA.DataRegistry.Business.CustomsDSBCreditorOverride)(null)).CreditorPK)));
            this.MainGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.Caption = "";
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("df91badf-13a9-40c0-a9d4-5d903f23648c", "District Office");
            zDropEditColumnStyleInfo1.ColumnName = "DistrictOfficeCode";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zGuidFindBoxColumnStyleInfo1.Caption = "";
            zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("5804c2fe-c8f6-40ae-b37c-cf196ff72e97", "Creditor");
            zGuidFindBoxColumnStyleInfo1.ColumnName = "CreditorPK";
            zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            this.MainGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.MainGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.MainGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainGrid.GridId = "a2ac0a76-4ac2-4c16-aeb8-78c836c786c0";
            this.MainGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.MainGrid.LayoutKey = "MainGrid";
            this.MainGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainGrid.Name = "MainGrid";
            this.MainGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 313, true);
            this.MainGrid.TabIndex = 0;
            // 
            // CustomsDSBCreditorOverrideUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainGrid);
            this.Name = "CustomsDSBCreditorOverrideUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 313, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MainGrid)).EndInit();
            this.MainGrid.ResumeLayout(false);
            this.MainGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal Enterprise.ZArchitecture.ZGrid MainGrid;
    }
}
