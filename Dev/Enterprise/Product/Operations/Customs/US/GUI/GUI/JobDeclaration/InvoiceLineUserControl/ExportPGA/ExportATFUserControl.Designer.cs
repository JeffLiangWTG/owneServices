namespace Enterprise.Customs.US.GUI
{
    partial class ExportATFUserControl
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
			this.ATFGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CategoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PermitExemptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PermitNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FFLExemptionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FFLNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProductIrrevelantPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ProductRelevantPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ATFGroupBox.SuspendLayout();
			this.CategoryCodeDropEdit.SuspendLayout();
			this.PermitExemptionDropEdit.SuspendLayout();
			this.FFLExemptionDropEdit.SuspendLayout();
			this.ProductIrrevelantPanel.SuspendLayout();
			this.ProductRelevantPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ATF);
			// 
			// ATFGroupBox
			// 
			this.ATFGroupBox.Controls.Add(this.ProductRelevantPanel);
			this.ATFGroupBox.Controls.Add(this.ProductIrrevelantPanel);
			this.ATFGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ATFGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ATFGroupBox.Name = "ATFGroupBox";
			this.ATFGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 245, true);
			this.ATFGroupBox.TabIndex = 0;
			this.ATFGroupBox.TabStop = false;
			this.ATFGroupBox.Text = "ATF - Bureau of Alcohol, Tobacco, Firearms and Explosives";
			// 
			// CategoryCodeDropEdit
			// 
			this.CategoryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CategoryCodeDropEdit, "US_CategoryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ATF)(null)).US_CategoryCode)));
			this.CategoryCodeDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d0c900bd-52a0-497b-aaa5-7075c0d8e4ba", "Category Code");
			this.CategoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 4, true);
			this.CategoryCodeDropEdit.Name = "CategoryCodeDropEdit";
			this.CategoryCodeDropEdit.PreBoundMaxLength = 4;
			this.CategoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.CategoryCodeDropEdit.TabIndex = 1;
			// 
			// PermitQuantityCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PermitQuantityCalcEdit, "US_Quantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.US.Business.ATF)(null)).US_Quantity)));
			this.PermitQuantityCalcEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c31b0511-8f67-46f8-811e-0af9eb6cd716", "Quantity");
			this.PermitQuantityCalcEdit.DecimalPlaces = 0;
			this.PermitQuantityCalcEdit.Decimals = 0;
			this.PermitQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 5, true);
			this.PermitQuantityCalcEdit.Name = "PermitQuantityCalcEdit";
			this.PermitQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PermitQuantityCalcEdit.TabIndex = 0;
			this.PermitQuantityCalcEdit.Text = "0";
			this.PermitQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PermitExemptionDropEdit
			// 
			this.PermitExemptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitExemptionDropEdit, "US_PermitExemptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ATF)(null)).US_PermitExemptionCode)));
			this.PermitExemptionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("a70777cd-3143-46a9-9af3-d50c92688652", "Permit Exemption Code");
			this.PermitExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 108, true);
			this.PermitExemptionDropEdit.Name = "PermitExemptionDropEdit";
			this.PermitExemptionDropEdit.PreBoundMaxLength = 1;
			this.PermitExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PermitExemptionDropEdit.TabIndex = 5;
			// 
			// PermitNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PermitNumberTextBox, "US_PermitNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_PermitNumber)));
			this.PermitNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("53cc4b19-0921-4f10-ad2d-33f5a31737fa", "Permit Number");
			this.PermitNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 82, true);
			this.PermitNumberTextBox.Name = "PermitNumberTextBox";
			this.PermitNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.PermitNumberTextBox.TabIndex = 4;
			// 
			// FFLExemptionDropEdit
			// 
			this.FFLExemptionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FFLExemptionDropEdit, "US_FFLExemptionCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ATF)(null)).US_FFLExemptionCode)));
			this.FFLExemptionDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("87790561-ac1c-4b2e-811c-2b7533df78ac", "FFL Exemption Code");
			this.FFLExemptionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 56, true);
			this.FFLExemptionDropEdit.Name = "FFLExemptionDropEdit";
			this.FFLExemptionDropEdit.PreBoundMaxLength = 1;
			this.FFLExemptionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.FFLExemptionDropEdit.TabIndex = 3;
			// 
			// FFLNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FFLNumberTextBox, "US_FFLNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ATF)(null)).US_FFLNumber)));
			this.FFLNumberTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d51b20e1-4920-45e1-8194-cf4970592cc8", "FFL Number");
			this.FFLNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 30, true);
			this.FFLNumberTextBox.Name = "FFLNumberTextBox";
			this.FFLNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.FFLNumberTextBox.TabIndex = 2;
			// 
			// ProductIrrevelantPanel
			// 
			this.ProductIrrevelantPanel.Controls.Add(this.PermitQuantityCalcEdit);
			this.ProductIrrevelantPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ProductIrrevelantPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ProductIrrevelantPanel.Name = "ProductIrrevelantPanel";
			this.ProductIrrevelantPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 26, true);
			this.ProductIrrevelantPanel.TabIndex = 6;
			// 
			// ProductRelevantPanel
			// 
			this.ProductRelevantPanel.Controls.Add(this.CategoryCodeDropEdit);
			this.ProductRelevantPanel.Controls.Add(this.FFLNumberTextBox);
			this.ProductRelevantPanel.Controls.Add(this.PermitExemptionDropEdit);
			this.ProductRelevantPanel.Controls.Add(this.FFLExemptionDropEdit);
			this.ProductRelevantPanel.Controls.Add(this.PermitNumberTextBox);
			this.ProductRelevantPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductRelevantPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 42, true);
			this.ProductRelevantPanel.Name = "ProductRelevantPanel";
			this.ProductRelevantPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 200, true);
			this.ProductRelevantPanel.TabIndex = 7;
			// 
			// ExportATFUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ATFGroupBox);
			this.Name = "ExportATFUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ATFGroupBox.ResumeLayout(false);
			this.ATFGroupBox.PerformLayout();
			this.CategoryCodeDropEdit.ResumeLayout(true);
			this.CategoryCodeDropEdit.PerformLayout();
			this.PermitExemptionDropEdit.ResumeLayout(true);
			this.PermitExemptionDropEdit.PerformLayout();
			this.FFLExemptionDropEdit.ResumeLayout(true);
			this.FFLExemptionDropEdit.PerformLayout();
			this.ProductIrrevelantPanel.ResumeLayout(false);
			this.ProductIrrevelantPanel.PerformLayout();
			this.ProductRelevantPanel.ResumeLayout(false);
			this.ProductRelevantPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private ZArchitecture.ZTextBox FFLNumberTextBox;
		private ZArchitecture.GUI.ZGroupBox ATFGroupBox;
		private ZArchitecture.GUI.ZDropEdit CategoryCodeDropEdit;
		private ZArchitecture.ZCalcEdit PermitQuantityCalcEdit;
		private ZArchitecture.GUI.ZDropEdit PermitExemptionDropEdit;
		private ZArchitecture.ZTextBox PermitNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit FFLExemptionDropEdit;
		private ZArchitecture.GUI.ZPanel ProductRelevantPanel;
		private ZArchitecture.GUI.ZPanel ProductIrrevelantPanel;
	}
}
