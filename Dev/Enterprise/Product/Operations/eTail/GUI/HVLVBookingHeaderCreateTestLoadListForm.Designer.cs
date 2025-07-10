namespace Enterprise.eTail.GUI
{
	partial class HVLVBookingHeaderCreateTestLoadListForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.groupBoxLoadListDataSetting = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checkBoxCreateHVLVOuterPackageDescrition = new Enterprise.ZArchitecture.ZLabel();
			this.checkBoxCreateHVLVOuterPackage = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.textBoxBookingHeaderReference = new Enterprise.ZArchitecture.ZTextBox();
			this.btnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxLoadListDataSetting.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 109, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 6, true);
			this.MainStatusBar.Visible = false;
			// 
			// groupBoxLoadListDataSetting
			// 
			this.groupBoxLoadListDataSetting.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("76839166-a581-49a3-b757-7c11fb14beb8", "Load List Test Data Setting");
			this.groupBoxLoadListDataSetting.Controls.Add(this.checkBoxCreateHVLVOuterPackageDescrition);
			this.groupBoxLoadListDataSetting.Controls.Add(this.checkBoxCreateHVLVOuterPackage);
			this.groupBoxLoadListDataSetting.Controls.Add(this.textBoxBookingHeaderReference);
			this.groupBoxLoadListDataSetting.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.groupBoxLoadListDataSetting.Name = "groupBoxLoadListDataSetting";
			this.groupBoxLoadListDataSetting.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 69, true);
			this.groupBoxLoadListDataSetting.TabIndex = 0;
			this.groupBoxLoadListDataSetting.TabStop = false;
			// 
			// checkBoxCreateHVLVOuterPackageDescrition
			// 
			this.checkBoxCreateHVLVOuterPackageDescrition.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("8a0051ef-723a-4674-b255-1b1eeac2f6ea", "Create Test HVLV Outer Package");
			this.checkBoxCreateHVLVOuterPackageDescrition.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.checkBoxCreateHVLVOuterPackageDescrition.ForeColor = System.Drawing.Color.Black;
			this.checkBoxCreateHVLVOuterPackageDescrition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 45, true);
			this.checkBoxCreateHVLVOuterPackageDescrition.Name = "checkBoxCreateHVLVOuterPackageDescrition";
			this.checkBoxCreateHVLVOuterPackageDescrition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 20, true);
			this.checkBoxCreateHVLVOuterPackageDescrition.TabIndex = 2;
			this.checkBoxCreateHVLVOuterPackageDescrition.UseMnemonic = false;
			// 
			// checkBoxCreateHVLVOuterPackage
			// 
			this.checkBoxCreateHVLVOuterPackage.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.checkBoxCreateHVLVOuterPackage.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("aea6811e-0739-4ac3-ab4e-03bb32dfa4a5", "Create Test HVLV Outer Package");
			this.checkBoxCreateHVLVOuterPackage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 47, true);
			this.checkBoxCreateHVLVOuterPackage.Name = "checkBoxCreateHVLVOuterPackage";
			this.checkBoxCreateHVLVOuterPackage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 18, true);
			this.checkBoxCreateHVLVOuterPackage.TabIndex = 3;
			// 
			// textBoxBookingHeaderReference
			// 
			this.textBoxBookingHeaderReference.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("5ffe5a1f-0c56-4f50-9579-9caaecfacc4a", "Booking Header Reference");
			this.textBoxBookingHeaderReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 22, true);
			this.textBoxBookingHeaderReference.Name = "textBoxBookingHeaderReference";
			this.textBoxBookingHeaderReference.ReadOnly = true;
			this.textBoxBookingHeaderReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.textBoxBookingHeaderReference.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("9863fffa-1929-4dee-9ead-78e638b57412", "OK");
			this.btnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 79, true);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 26, true);
			this.btnOK.TabIndex = 4;
			this.btnOK.ToolTipCaption = null;
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// HVLVBookingHeaderCreateTestLoadListForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("3d92a404-1081-45a1-bde4-7651a0e3fe25", "Load List Test Data Creator");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 114, true);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBoxLoadListDataSetting);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "HVLVBookingHeaderCreateTestLoadListForm";
			this.Controls.SetChildIndex(this.groupBoxLoadListDataSetting, 0);
			this.Controls.SetChildIndex(this.btnOK, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxLoadListDataSetting.ResumeLayout(false);
			this.groupBoxLoadListDataSetting.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBoxLoadListDataSetting;
		private ZArchitecture.ZTextBox textBoxBookingHeaderReference;
		private ZArchitecture.GUI.ZButton btnOK;
		private ZArchitecture.GUI.ZCheckBox checkBoxCreateHVLVOuterPackage;
		private ZArchitecture.ZLabel checkBoxCreateHVLVOuterPackageDescrition;
	}
}
