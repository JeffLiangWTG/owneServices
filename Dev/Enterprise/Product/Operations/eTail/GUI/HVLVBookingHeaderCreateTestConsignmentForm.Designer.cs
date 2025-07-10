namespace Enterprise.eTail.GUI
{
	partial class HVLVBookingHeaderCreateTestConsignmentForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.groupBoxConsignmentDataSetting = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.checkBoxDeleteExistingConsignments = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.textBoxWaybillPrefix = new Enterprise.ZArchitecture.ZTextBox();
			this.calcEditConsignmentCount = new Enterprise.ZArchitecture.ZCalcEdit();
			this.textBoxBookingHeaderReference = new Enterprise.ZArchitecture.ZTextBox();
			this.checkBoxDeleteExistingConsignmentsDescrition = new Enterprise.ZArchitecture.ZLabel();
			this.btnOK = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.groupBoxConsignmentDataSetting.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 177, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 24, true);
			// 
			// groupBoxConsignmentDataSetting
			// 
			this.groupBoxConsignmentDataSetting.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("b6db1976-9136-4dd8-88b1-b5e2fde0ab4b", "Consignment Data Setting");
			this.groupBoxConsignmentDataSetting.Controls.Add(this.checkBoxDeleteExistingConsignmentsDescrition);
			this.groupBoxConsignmentDataSetting.Controls.Add(this.checkBoxDeleteExistingConsignments);
			this.groupBoxConsignmentDataSetting.Controls.Add(this.textBoxWaybillPrefix);
			this.groupBoxConsignmentDataSetting.Controls.Add(this.calcEditConsignmentCount);
			this.groupBoxConsignmentDataSetting.Controls.Add(this.textBoxBookingHeaderReference);
			this.groupBoxConsignmentDataSetting.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.groupBoxConsignmentDataSetting.Name = "groupBoxConsignmentDataSetting";
			this.groupBoxConsignmentDataSetting.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 130, true);
			this.groupBoxConsignmentDataSetting.TabIndex = 0;
			this.groupBoxConsignmentDataSetting.TabStop = false;
			//
			// checkBoxDeleteExistingConsignmentsDescrition
			//
			this.checkBoxDeleteExistingConsignmentsDescrition.CaptionResourceString = null;
			this.checkBoxDeleteExistingConsignmentsDescrition.ForeColor = System.Drawing.Color.Gray;
			this.checkBoxDeleteExistingConsignmentsDescrition.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 100, true);
			this.checkBoxDeleteExistingConsignmentsDescrition.Name = "checkBoxRemoveAllExistingConsignmentsDescription";
			this.checkBoxDeleteExistingConsignmentsDescrition.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 20, true);
			this.checkBoxDeleteExistingConsignmentsDescrition.Text = Enterprise.eTail.GUI.Res.GetString("8f8a0c81-c6aa-464a-af1d-083cf5ab0d02", "Delete All Existing Consignments");
			this.checkBoxDeleteExistingConsignmentsDescrition.TabIndex = 6;
			// 
			// checkBoxRemoveAllExistingConsignments
			//
			this.checkBoxDeleteExistingConsignments.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("4d7c2774-db95-48a9-9c58-8ee98ebc54a1", "Delete All Existing Consignments");
			this.checkBoxDeleteExistingConsignments.Enabled = true;
			this.checkBoxDeleteExistingConsignments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(196, 100, true);
			this.checkBoxDeleteExistingConsignments.Name = "checkBoxRemoveAllExistingConsignments";
			this.checkBoxDeleteExistingConsignments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.checkBoxDeleteExistingConsignments.TabIndex = 4;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.checkBoxDeleteExistingConsignments, true);
			// 
			// textBoxWaybillPrefix
			// 
			this.textBoxWaybillPrefix.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("0d9b7c9a-a450-4286-8429-fa7cdc0756e1", "Waybill Prefix");
			this.textBoxWaybillPrefix.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 80, true);
			this.textBoxWaybillPrefix.Name = "textBoxWaybillPrefix";
			this.textBoxWaybillPrefix.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxWaybillPrefix.TabIndex = 3;
			// 
			// calcEditConsignmentCount
			// 
			this.calcEditConsignmentCount.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("bb658925-e298-4868-a525-71a42fa46ba5", "Consignment Count");
			this.calcEditConsignmentCount.DecimalPlaces = 0;
			this.calcEditConsignmentCount.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 50, true);
			this.calcEditConsignmentCount.Name = "calcEditConsignmentCount";
			this.calcEditConsignmentCount.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.calcEditConsignmentCount.TabIndex = 2;
			this.calcEditConsignmentCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxShipmentReference
			// 
			this.textBoxBookingHeaderReference.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("859aeb78-42a4-477a-ace9-78934d94d534", "Booking Header Reference");
			this.textBoxBookingHeaderReference.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 20, true);
			this.textBoxBookingHeaderReference.Name = "textBoxShipmentReference";
			this.textBoxBookingHeaderReference.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.textBoxBookingHeaderReference.TabIndex = 1;
			// 
			// btnOK
			// 
			this.btnOK.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("eff0e847-46a7-48e2-977f-5f2301447171", "OK");
			this.btnOK.IsCaptionOverridden = false;
			this.btnOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 150, true);
			this.btnOK.Name = "btnOK";
			this.btnOK.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.btnOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnOK.TabIndex = 5;
			this.btnOK.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.btnOK.ToolTipCaption = null;
			this.btnOK.UseVisualStyleBackColor = true;
			this.btnOK.Click += new System.EventHandler(this.BtnOK_Click);
			// 
			// HVLVTestConsignmentDataCreatorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.eTail.GUI.Res.GetData("1730f6a7-c7f2-4026-9cf6-4b838b8fb1c3", "Test Consignment Data Creator");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 201, true);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.groupBoxConsignmentDataSetting);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "HVLVTestConsignmentDataCreatorForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.groupBoxConsignmentDataSetting, 0);
			this.Controls.SetChildIndex(this.btnOK, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.groupBoxConsignmentDataSetting.ResumeLayout(false);
			this.groupBoxConsignmentDataSetting.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox groupBoxConsignmentDataSetting;
		private ZArchitecture.ZTextBox textBoxBookingHeaderReference;
		private ZArchitecture.ZTextBox textBoxWaybillPrefix;
		private ZArchitecture.ZCalcEdit calcEditConsignmentCount;
		private ZArchitecture.GUI.ZButton btnOK;
		private ZArchitecture.GUI.ZCheckBox checkBoxDeleteExistingConsignments;
		private ZArchitecture.ZLabel checkBoxDeleteExistingConsignmentsDescrition;
	}
}
