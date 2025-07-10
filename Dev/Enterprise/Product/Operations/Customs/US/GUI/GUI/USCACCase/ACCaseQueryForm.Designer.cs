namespace Enterprise.Customs.US.GUI
{
	partial class ACCaseQueryForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CaseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HTSLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TSUSALabel = new Enterprise.ZArchitecture.ZLabel();
			this.TSUSATextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManufacturerMIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ManMIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ForeignShipperTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignExporterMIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GiveUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HTSTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UpdateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CaseDataGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CaseNoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CaseNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CaseDataGroupBox.SuspendLayout();
			this.CaseNoGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CaseNumbersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 478, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.ACEACCaseQuery);
			// 
			// StatusLabel
			// 
			this.StatusLabel.AutoSize = true;
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 25, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.StatusLabel.TabIndex = 0;
			this.StatusLabel.Text = "Case Status:";
			// 
			// CaseStatusDropEdit
			// 
			this.CaseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CaseStatusDropEdit, "US_CompanyCaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_CompanyCaseStatus)));
			this.CaseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 22, true);
			this.CaseStatusDropEdit.Name = "CaseStatusDropEdit";
			this.CaseStatusDropEdit.PreBoundMaxLength = 1;
			this.CaseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CaseStatusDropEdit.TabIndex = 1;
			// 
			// CountryLabel
			// 
			this.CountryLabel.AutoSize = true;
			this.CountryLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 52, true);
			this.CountryLabel.Name = "CountryLabel";
			this.CountryLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.CountryLabel.TabIndex = 2;
			this.CountryLabel.Text = "Country:";
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "US_CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_CountryCode)));
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 49, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.PreBoundMaxLength = 2;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.CountryCodeFindBox.TabIndex = 3;
			// 
			// HTSLabel
			// 
			this.HTSLabel.AutoSize = true;
			this.HTSLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 79, true);
			this.HTSLabel.Name = "HTSLabel";
			this.HTSLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 13, true);
			this.HTSLabel.TabIndex = 4;
			this.HTSLabel.Text = "HTS:";
			// 
			// TSUSALabel
			// 
			this.TSUSALabel.AutoSize = true;
			this.TSUSALabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 186, true);
			this.TSUSALabel.Name = "TSUSALabel";
			this.TSUSALabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 13, true);
			this.TSUSALabel.TabIndex = 12;
			this.TSUSALabel.Text = "TSUSA:";
			// 
			// TSUSATextBox
			// 
			this.BindingSource.SetBindingMember(this.TSUSATextBox, "US_TSUSA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_TSUSA)));
			this.TSUSATextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 183, true);
			this.TSUSATextBox.Name = "TSUSATextBox";
			this.TSUSATextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TSUSATextBox.TabIndex = 13;
			// 
			// ManufacturerMIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ManufacturerMIDTextBox, "US_ManufacturerMID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_ManufacturerMID)));
			this.ManufacturerMIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 103, true);
			this.ManufacturerMIDTextBox.Name = "ManufacturerMIDTextBox";
			this.ManufacturerMIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ManufacturerMIDTextBox.TabIndex = 7;
			// 
			// ManMIDLabel
			// 
			this.ManMIDLabel.AutoSize = true;
			this.ManMIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 106, true);
			this.ManMIDLabel.Name = "ManMIDLabel";
			this.ManMIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 13, true);
			this.ManMIDLabel.TabIndex = 6;
			this.ManMIDLabel.Text = "Manufacturer MID:";
			// 
			// ForeignShipperTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignShipperTextBox, "US_ForeignShipperMID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_ForeignShipperMID)));
			this.ForeignShipperTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 130, true);
			this.ForeignShipperTextBox.Name = "ForeignShipperTextBox";
			this.ForeignShipperTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ForeignShipperTextBox.TabIndex = 9;
			// 
			// ForeignExporterMIDLabel
			// 
			this.ForeignExporterMIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 128, true);
			this.ForeignExporterMIDLabel.Name = "ForeignExporterMIDLabel";
			this.ForeignExporterMIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 23, true);
			this.ForeignExporterMIDLabel.TabIndex = 8;
			this.ForeignExporterMIDLabel.Text = "Foreign Shipper MID:";
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 449, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "&Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// GiveUpButton
			// 
			this.GiveUpButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.GiveUpButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.GiveUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 449, true);
			this.GiveUpButton.Name = "GiveUpButton";
			this.GiveUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.GiveUpButton.TabIndex = 3;
			this.GiveUpButton.Text = "&Cancel";
			this.GiveUpButton.UseVisualStyleBackColor = true;
			this.GiveUpButton.Click += new System.EventHandler(this.GiveUpButton_Click);
			// 
			// HTSTextBox
			// 
			this.BindingSource.SetBindingMember(this.HTSTextBox, "US_HTSNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_HTSNumber)));
			this.HTSTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 76, true);
			this.HTSTextBox.Name = "HTSTextBox";
			this.HTSTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.HTSTextBox.TabIndex = 5;
			// 
			// UpdateDateEdit
			// 
			this.UpdateDateEdit.AllowDrop = true;
			this.UpdateDateEdit.AutoCompleteMonthThreshold = 1;
			this.UpdateDateEdit.AutoCompleteYear = true;
			this.UpdateDateEdit.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.UpdateDateEdit, "US_DateSinceLastUpdate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).US_DateSinceLastUpdate)));
			this.UpdateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 157, true);
			this.UpdateDateEdit.Name = "UpdateDateEdit";
			this.UpdateDateEdit.TabIndex = 11;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 160, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 13, true);
			this.zLabel1.TabIndex = 10;
			this.zLabel1.Text = "Date Since Last Update:";
			// 
			// CaseDataGroupBox
			// 
			this.CaseDataGroupBox.Controls.Add(this.StatusLabel);
			this.CaseDataGroupBox.Controls.Add(this.CaseStatusDropEdit);
			this.CaseDataGroupBox.Controls.Add(this.zLabel1);
			this.CaseDataGroupBox.Controls.Add(this.CountryLabel);
			this.CaseDataGroupBox.Controls.Add(this.UpdateDateEdit);
			this.CaseDataGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.CaseDataGroupBox.Controls.Add(this.HTSTextBox);
			this.CaseDataGroupBox.Controls.Add(this.HTSLabel);
			this.CaseDataGroupBox.Controls.Add(this.ForeignShipperTextBox);
			this.CaseDataGroupBox.Controls.Add(this.ManMIDLabel);
			this.CaseDataGroupBox.Controls.Add(this.ForeignExporterMIDLabel);
			this.CaseDataGroupBox.Controls.Add(this.TSUSATextBox);
			this.CaseDataGroupBox.Controls.Add(this.ManufacturerMIDTextBox);
			this.CaseDataGroupBox.Controls.Add(this.TSUSALabel);
			this.CaseDataGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.CaseDataGroupBox.Name = "CaseDataGroupBox";
			this.CaseDataGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 218, true);
			this.CaseDataGroupBox.TabIndex = 1;
			this.CaseDataGroupBox.TabStop = false;
			this.CaseDataGroupBox.Text = "Other Criteria";
			// 
			// CaseNoGroupBox
			// 
			this.CaseNoGroupBox.Controls.Add(this.CaseNumbersGrid);
			this.CaseNoGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CaseNoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CaseNoGroupBox.Name = "CaseNoGroupBox";
			this.CaseNoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 208, true);
			this.CaseNoGroupBox.TabIndex = 0;
			this.CaseNoGroupBox.TabStop = false;
			this.CaseNoGroupBox.Text = "Case Numbers";
			// 
			// CaseNumbersGrid
			// 
			this.CaseNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CaseNumbersGrid, "CaseNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).CaseNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ACECaseNumberForQuery)(((System.Collections.IList)(((Enterprise.Customs.US.Business.ACEACCaseQuery)(null)).CaseNumbers)).SyncRoot)).CaseNumber)));
			this.CaseNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo3.Caption = "Case Number";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CaseNumber";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.CaseNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CaseNumbersGrid.GridId = "2365ab4a-6059-4802-b453-11879bc8652a";
			this.CaseNumbersGrid.CopySelectedRowsAllowed = true;
			this.CaseNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CaseNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CaseNumbersGrid.LayoutKey = "TariffsGrid";
			this.CaseNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CaseNumbersGrid.Name = "CaseNumbersGrid";
			this.CaseNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(439, 189, true);
			this.CaseNumbersGrid.TabIndex = 1;
			// 
			// ACCaseQueryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 502, true);
			this.Controls.Add(this.CaseNoGroupBox);
			this.Controls.Add(this.CaseDataGroupBox);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.GiveUpButton);
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.ACEACCaseQuery);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(451, 530, true);
			this.Name = "ACCaseQueryForm";
			this.Text = "ACCaseQueryForm";
			this.Controls.SetChildIndex(this.GiveUpButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CaseDataGroupBox, 0);
			this.Controls.SetChildIndex(this.CaseNoGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CaseDataGroupBox.ResumeLayout(false);
			this.CaseDataGroupBox.PerformLayout();
			this.CaseNoGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CaseNumbersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZLabel StatusLabel;
		private ZArchitecture.GUI.ZDropEdit CaseStatusDropEdit;
		private ZArchitecture.ZLabel CountryLabel;
		private ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		private ZArchitecture.ZLabel HTSLabel;
		private ZArchitecture.ZLabel TSUSALabel;
		private ZArchitecture.ZTextBox TSUSATextBox;
		private ZArchitecture.ZTextBox ManufacturerMIDTextBox;
		private ZArchitecture.ZLabel ManMIDLabel;
		private ZArchitecture.ZTextBox ForeignShipperTextBox;
		private ZArchitecture.ZLabel ForeignExporterMIDLabel;
		public ZArchitecture.GUI.ZButton SendButton;
		public ZArchitecture.GUI.ZButton GiveUpButton;
		private ZArchitecture.ZTextBox HTSTextBox;
		private ZArchitecture.GUI.ZDateEdit UpdateDateEdit;
		private ZArchitecture.ZLabel zLabel1;
		private ZArchitecture.GUI.ZGroupBox CaseDataGroupBox;
		private ZArchitecture.GUI.ZGroupBox CaseNoGroupBox;
		private ZArchitecture.ZGrid CaseNumbersGrid;
	}
}
