namespace Enterprise.Rating.GUI
{
	partial class DataImportForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ChooseFileButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilePathLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ClearTACTCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ClearStandardCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ImportProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.IsJobLevelChargeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RoundingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExcludeFromAutocostingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.DataTransfer.RateDataImporter);
			// 
			// ChooseFileButton
			// 
			this.ChooseFileButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("95dbe8ca-ed55-4eda-a0b5-5f32ec1e4c75", "Choose File");
			this.ChooseFileButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ChooseFileButton.Name = "ChooseFileButton";
			this.ChooseFileButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.ChooseFileButton.TabIndex = 1;
			this.ChooseFileButton.UseVisualStyleBackColor = true;
			this.ChooseFileButton.Click += new System.EventHandler(this.ChooseFileButton_Click);
			// 
			// FilePathLabel
			// 
			this.FilePathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FilePathLabel.AutoSize = true;
			this.FilePathLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("23d63c57-8dfd-4557-9739-ff3394b203a4", "No file selected.");
			this.FilePathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 17, true);
			this.FilePathLabel.Name = "FilePathLabel";
			this.FilePathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 13, true);
			this.FilePathLabel.TabIndex = 2;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("9da39e09-0a2b-4cb4-876a-853d19302862", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 260, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 22, true);
			this.ImportButton.TabIndex = 9;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// InfoTextBox
			// 
			this.InfoTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.InfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.InfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 115, true);
			this.InfoTextBox.Multiline = true;
			this.InfoTextBox.Name = "InfoTextBox";
			this.InfoTextBox.ReadOnly = true;
			this.InfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 139, true);
			this.InfoTextBox.TabIndex = 8;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("06a39a18-5931-4477-8d4c-5e0d47792cad", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(519, 260, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 22, true);
			this.CloseButton.TabIndex = 11;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// ClearTACTCheckBox
			// 
			this.ClearTACTCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ClearTACTCheckBox, "ShouldClearOldTACTRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.DataTransfer.RateDataImporter)(null)).ShouldClearOldTACTRates)));
			this.ClearTACTCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("523a6e90-9c45-45ef-9e04-33ecd9c28a6c", "Clear Existing TACT Rates");
			this.ClearTACTCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClearTACTCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 42, true);
			this.ClearTACTCheckBox.Name = "ClearTACTCheckBox";
			this.ClearTACTCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(157, 17, true);
			this.ClearTACTCheckBox.TabIndex = 3;
			this.ClearTACTCheckBox.UseVisualStyleBackColor = true;
			// 
			// ClearStandardCheckBox
			// 
			this.ClearStandardCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ClearStandardCheckBox, "ShouldClearOldStandardRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.DataTransfer.RateDataImporter)(null)).ShouldClearOldStandardRates)));
			this.ClearStandardCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("1721972e-905d-4a77-94ae-97008081885a", "Clear Existing Standard Air Freight Rates");
			this.ClearStandardCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ClearStandardCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 42, true);
			this.ClearStandardCheckBox.Name = "ClearStandardCheckBox";
			this.ClearStandardCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.ClearStandardCheckBox.TabIndex = 4;
			this.ClearStandardCheckBox.UseVisualStyleBackColor = true;
			// 
			// ImportProgressBar
			// 
			this.ImportProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ImportProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 88, true);
			this.ImportProgressBar.Name = "ImportProgressBar";
			this.ImportProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(611, 23, true);
			this.ImportProgressBar.TabIndex = 7;
			// 
			//
			// IsJobLevelChargeCheckBox
			// 
			this.IsJobLevelChargeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsJobLevelChargeCheckBox, "IsJobLevelCharge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.DataTransfer.RateDataImporter)(null)).IsJobLevelCharge)));
			this.IsJobLevelChargeCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("27b5f373-4cb5-407b-b2b8-ee2bba6242da", "Job Level");
			this.IsJobLevelChargeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsJobLevelChargeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 66, true);
			this.IsJobLevelChargeCheckBox.Name = "IsJobLevelChargeCheckBox";
			this.IsJobLevelChargeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.IsJobLevelChargeCheckBox.TabIndex = 5;
			this.IsJobLevelChargeCheckBox.UseVisualStyleBackColor = true;
			// 
			// RoundingDropEdit
			// 
			this.RoundingDropEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RoundingDropEdit, "Rounding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.DataTransfer.RateDataImporter)(null)).Rounding)));
			this.RoundingDropEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("c95a273d-7fe1-4322-8f48-75fe727a8313", "Rounding");
			this.RoundingDropEdit.PreBoundMaxLength = 3;
			this.RoundingDropEdit.ShowDescriptionBox = false;
			this.RoundingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(338, 63, true);
			this.RoundingDropEdit.Name = "RoundingDropEdit";
			this.RoundingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.RoundingDropEdit.TabIndex = 6;
			this.RoundingDropEdit.EditableInViewMode = false;
			// 
			// ExcludeFromAutocostingCheckBox
			// 
			this.ExcludeFromAutocostingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExcludeFromAutocostingCheckBox, "ShouldExcludeFromAutoRating");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.DataTransfer.RateDataImporter)(null)).ShouldExcludeFromAutoRating)));
			this.ExcludeFromAutocostingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExcludeFromAutocostingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 65, true);
			this.ExcludeFromAutocostingCheckBox.Name = "ExcludeFromAutocostingCheckBox";
			this.ExcludeFromAutocostingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 17, true);
			this.ExcludeFromAutocostingCheckBox.TabIndex = 10;
			this.ExcludeFromAutocostingCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("2923E99B-B9A1-45C3-94CD-F39E1B3520BD", "Exclude from Autocosting");
			this.ExcludeFromAutocostingCheckBox.UseVisualStyleBackColor = true;
			// 
			// DataImportForm
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("5d8a22be-9c2b-4129-ac7a-abc0fac95663", "Rate Data Import");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(634, 312, true);
			this.Controls.Add(this.IsJobLevelChargeCheckBox);
			this.Controls.Add(this.RoundingDropEdit);
			this.Controls.Add(this.FilePathLabel);
			this.Controls.Add(this.InfoTextBox);
			this.Controls.Add(this.ImportProgressBar);
			this.Controls.Add(this.ChooseFileButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ClearTACTCheckBox);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.ClearStandardCheckBox);
			this.Controls.Add(this.ExcludeFromAutocostingCheckBox);
			this.DataSourceType = typeof(Enterprise.Rating.DataTransfer.RateDataImporter);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 350, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 312, true);
			this.Name = "DataImportForm";
			this.Controls.SetChildIndex(this.ClearStandardCheckBox, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.ClearTACTCheckBox, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ChooseFileButton, 0);
			this.Controls.SetChildIndex(this.ImportProgressBar, 0);
			this.Controls.SetChildIndex(this.InfoTextBox, 0);
			this.Controls.SetChildIndex(this.FilePathLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.IsJobLevelChargeCheckBox, 0);
			this.Controls.SetChildIndex(this.RoundingDropEdit, 0);
			this.Controls.SetChildIndex(this.ExcludeFromAutocostingCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZButton ChooseFileButton;
		protected ZArchitecture.ZLabel FilePathLabel;
		protected ZArchitecture.GUI.ZButton ImportButton;
		internal protected ZArchitecture.ZTextBox InfoTextBox;
		protected ZArchitecture.GUI.ZButton CloseButton;
		protected ZArchitecture.GUI.ZCheckBox ClearTACTCheckBox;
		protected ZArchitecture.GUI.ZCheckBox ClearStandardCheckBox;
		protected CargoWise.Windows.UI.KProgressBar ImportProgressBar;
		protected ZArchitecture.GUI.ZCheckBox IsJobLevelChargeCheckBox;
		protected ZArchitecture.GUI.ZDropEdit RoundingDropEdit;
		protected ZArchitecture.GUI.ZCheckBox ExcludeFromAutocostingCheckBox;
	}
}
