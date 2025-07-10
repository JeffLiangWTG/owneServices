namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	partial class NewAWBForm
	{
		protected new void InitializeComponent()
		{
			this.JM_GC_CompanyBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JM_GBBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JM_Airline3DigitPrefixBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JM_MAWBBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.JM_OA_FromAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.JM_ServiceLevelBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HyphenLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CnclButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JM_IsPaperBoundRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.JM_Calc_IsNeutralBoundRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.BranchHomePortTextBox = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 24, true);
			this.MainStatusBar.TabIndex = 15;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobMawb);
			// 
			// JM_GC_CompanyBoundGuidFindBox
			// 
			this.JM_GC_CompanyBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_GC_CompanyBoundGuidFindBox, "JM_GC_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobMawb)(null)).JM_GC_Company)));
			this.JM_GC_CompanyBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.JM_GC_CompanyBoundGuidFindBox.Name = "JM_GC_CompanyBoundGuidFindBox";
			this.JM_GC_CompanyBoundGuidFindBox.PreBoundMaxLength = 3;
			this.JM_GC_CompanyBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.JM_GC_CompanyBoundGuidFindBox.TabIndex = 0;
			// 
			// JM_GBBoundGuidFindBox
			// 
			this.JM_GBBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_GBBoundGuidFindBox, "JM_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobMawb)(null)).JM_GB)));
			this.JM_GBBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 38, true);
			this.JM_GBBoundGuidFindBox.Name = "JM_GBBoundGuidFindBox";
			this.JM_GBBoundGuidFindBox.PreBoundMaxLength = 3;
			this.JM_GBBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 21, true);
			this.JM_GBBoundGuidFindBox.TabIndex = 1;
			// 
			// JM_Airline3DigitPrefixBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JM_Airline3DigitPrefixBoundTextBox, "JM_Airline3DigitPrefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobMawb)(null)).JM_Airline3DigitPrefix)));
			this.JM_Airline3DigitPrefixBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 86, true);
			this.JM_Airline3DigitPrefixBoundTextBox.Name = "JM_Airline3DigitPrefixBoundTextBox";
			this.JM_Airline3DigitPrefixBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 20, true);
			this.JM_Airline3DigitPrefixBoundTextBox.TabIndex = 5;
			// 
			// JM_MAWBBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.JM_MAWBBoundTextBox, "JM_MAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobMawb)(null)).JM_MAWB)));
			this.JM_MAWBBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 86, true);
			this.JM_MAWBBoundTextBox.Name = "JM_MAWBBoundTextBox";
			this.JM_MAWBBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.JM_MAWBBoundTextBox.TabIndex = 7;
			// 
			// JM_OA_FromBoundGuidFindBox
			// 
			this.JM_OA_FromAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_OA_FromAddressControl, "JM_OA_From");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobMawb)(null)).JM_OA_From)));
			this.JM_OA_FromAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 110, true);
			this.JM_OA_FromAddressControl.Name = "JM_OA_FromAddressControl";
			this.JM_OA_FromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 21, true);
			this.JM_OA_FromAddressControl.TabIndex = 9;
			this.JM_OA_FromAddressControl.BindToOrgList = "BorrowedFromList";
			this.JM_OA_FromAddressControl.PopupCaption = "Select Borrower Organization";
			this.JM_OA_FromAddressControl.ShowAddress = false;
			this.JM_OA_FromAddressControl.ShowOrganisationName = true;
			// 
			// JM_ServiceLevelBoundDropEdit
			// 
			this.JM_ServiceLevelBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.JM_ServiceLevelBoundDropEdit, "JM_ServiceLevel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobMawb)(null)).JM_ServiceLevel)));
			this.JM_ServiceLevelBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 134, true);
			this.JM_ServiceLevelBoundDropEdit.Name = "JM_ServiceLevelBoundDropEdit";
			this.JM_ServiceLevelBoundDropEdit.PreBoundMaxLength = 3;
			this.JM_ServiceLevelBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.JM_ServiceLevelBoundDropEdit.TabIndex = 11;
			// 
			// HyphenLabel
			// 
			this.HyphenLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 86, true);
			this.HyphenLabel.Name = "HyphenLabel";
			this.HyphenLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 20, true);
			this.HyphenLabel.TabIndex = 6;
			this.HyphenLabel.Text = "-";
			// 
			// CnclButton
			// 
			this.CnclButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CnclButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NewAWBForm|a6840b84-9fdd-4a16-a3d3-9446564b604f", "Cancel");
			this.CnclButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CnclButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 215, true);
			this.CnclButton.Name = "CnclButton";
			this.CnclButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CnclButton.TabIndex = 14;
			this.CnclButton.Click += new System.EventHandler(this.CnclButton_Click);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NewAWBForm|68d549d2-fabb-4896-881b-287c7812a1a9", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 215, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 13;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// TypeGroupBox
			// 
			this.TypeGroupBox.Controls.Add(this.JM_IsPaperBoundRadioButton);
			this.TypeGroupBox.Controls.Add(this.JM_Calc_IsNeutralBoundRadioButton);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TypeGroupBox, false);
			this.TypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 165, true);
			this.TypeGroupBox.Name = "TypeGroupBox";
			this.TypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 30, true);
			this.TypeGroupBox.TabIndex = 12;
			this.TypeGroupBox.TabStop = false;
			// 
			// JM_IsPaperBoundRadioButton
			// 
			this.JM_IsPaperBoundRadioButton.AutoCheck = false;
			this.JM_IsPaperBoundRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JM_IsPaperBoundRadioButton, "JM_IsPaper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.JobMawb)(null)).JM_IsPaper)));
			this.JM_IsPaperBoundRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JM_IsPaperBoundRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 10, true);
			this.JM_IsPaperBoundRadioButton.Name = "JM_IsPaperBoundRadioButton";
			this.JM_IsPaperBoundRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(92, 17, true);
			this.JM_IsPaperBoundRadioButton.TabIndex = 1;
			// 
			// JM_Calc_IsNeutralBoundRadioButton
			// 
			this.JM_Calc_IsNeutralBoundRadioButton.AutoCheck = false;
			this.JM_Calc_IsNeutralBoundRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.JM_Calc_IsNeutralBoundRadioButton, "JM_Calc_IsNeutral");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.JobMawb)(null)).JM_Calc_IsNeutral)));
			this.JM_Calc_IsNeutralBoundRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NewAWBForm|470bfb84-4506-4170-a8e5-f9d6d5807c2a", "Neutral MAWB");
			this.JM_Calc_IsNeutralBoundRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.JM_Calc_IsNeutralBoundRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 10, true);
			this.JM_Calc_IsNeutralBoundRadioButton.Name = "JM_Calc_IsNeutralBoundRadioButton";
			this.JM_Calc_IsNeutralBoundRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.JM_Calc_IsNeutralBoundRadioButton.TabIndex = 0;
			// 
			// BranchHomePortTextBox
			// 
			this.BranchHomePortTextBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BranchHomePortTextBox, "JM_Calc_HomePortText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.RangeJobMawb)(null)).JM_Calc_HomePortText)));
			this.BranchHomePortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 65, true);
			this.BranchHomePortTextBox.Name = "BranchHomePortTextBox";
			this.BranchHomePortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.BranchHomePortTextBox.TabIndex = 16;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NewAWBForm|0bbf7f51-0294-4473-88b4-cf1f2cdce35c", "Home Port:");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 65, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.zLabel1.TabIndex = 17;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// NewAWBForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("NewAWBForm|f4ca325d-617d-4529-8899-19af25fbf0c5", "Create New MAWB");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 270, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.BranchHomePortTextBox);
			this.Controls.Add(this.JM_GC_CompanyBoundGuidFindBox);
			this.Controls.Add(this.JM_GBBoundGuidFindBox);
			this.Controls.Add(this.CnclButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.HyphenLabel);
			this.Controls.Add(this.TypeGroupBox);
			this.Controls.Add(this.JM_Airline3DigitPrefixBoundTextBox);
			this.Controls.Add(this.JM_ServiceLevelBoundDropEdit);
			this.Controls.Add(this.JM_MAWBBoundTextBox);
			this.Controls.Add(this.JM_OA_FromAddressControl);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.JobMawb);
			this.DataSourceTypeName = "Enterprise.Freight.Business.JobMawb";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "NewAWBForm";
			this.Controls.SetChildIndex(this.JM_OA_FromAddressControl, 0);
			this.Controls.SetChildIndex(this.JM_MAWBBoundTextBox, 0);
			this.Controls.SetChildIndex(this.JM_ServiceLevelBoundDropEdit, 0);
			this.Controls.SetChildIndex(this.JM_Airline3DigitPrefixBoundTextBox, 0);
			this.Controls.SetChildIndex(this.TypeGroupBox, 0);
			this.Controls.SetChildIndex(this.HyphenLabel, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CnclButton, 0);
			this.Controls.SetChildIndex(this.JM_GC_CompanyBoundGuidFindBox, 0);
			this.Controls.SetChildIndex(this.JM_GBBoundGuidFindBox, 0);
			this.Controls.SetChildIndex(this.BranchHomePortTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TypeGroupBox.ResumeLayout(false);
			this.TypeGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGuidFindBox JM_GC_CompanyBoundGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox JM_GBBoundGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox JM_Airline3DigitPrefixBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox JM_MAWBBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZAddressControl JM_OA_FromAddressControl;
		private Enterprise.ZArchitecture.GUI.ZDropEdit JM_ServiceLevelBoundDropEdit;
		private Enterprise.ZArchitecture.ZLabel HyphenLabel;
		private Enterprise.ZArchitecture.GUI.ZButton CnclButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OkButton;
		private Enterprise.ZArchitecture.GUI.ZGroupBox TypeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton JM_Calc_IsNeutralBoundRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton JM_IsPaperBoundRadioButton;
		private Enterprise.ZArchitecture.ZLabel BranchHomePortTextBox;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
	}
}
