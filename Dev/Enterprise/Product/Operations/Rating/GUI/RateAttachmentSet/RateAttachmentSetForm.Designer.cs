namespace Enterprise.Rating.GUI
{
	partial class RateAttachmentSetForm
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCheckBox mandatoryCheckEdit;
			Enterprise.ZArchitecture.GUI.ZCheckBox isDefaultCheckEdit;
			Enterprise.ZArchitecture.ZTranslatableTextControl attachmentNameTextBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox templateGuidFindBox;
			Enterprise.ZArchitecture.ZCalcEdit sequenceCalcEdit;
			Enterprise.ZArchitecture.GUI.ZDropEdit templateTypeBoundDropEdit;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox companyGuidFindBox;
			Enterprise.ZArchitecture.GUI.ZCheckBox systemDefinedCheckBox;
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			mandatoryCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			isDefaultCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			attachmentNameTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			templateGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			sequenceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			templateTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			companyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			systemDefinedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 195, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 23, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 9;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RateAttachmentSet);
			// 
			// mandatoryCheckEdit
			// 
			mandatoryCheckEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(mandatoryCheckEdit, "TS_IsMandatory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_IsMandatory)));
			mandatoryCheckEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RateAttachmentSetForm|6de1c0ac-8f65-4c71-9ca0-63a0b2edc267", "Mandatory");
			mandatoryCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			mandatoryCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			mandatoryCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 59, true);
			mandatoryCheckEdit.Name = "mandatoryCheckEdit";
			mandatoryCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			mandatoryCheckEdit.TabIndex = 4;
			// 
			// isDefaultCheckEdit
			// 
			isDefaultCheckEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(isDefaultCheckEdit, "TS_IsDefault");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_IsDefault)));
			isDefaultCheckEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RateAttachmentSetForm|ec794379-443e-49ee-b8cf-42e746c23c5f", "Default Page");
			isDefaultCheckEdit.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			isDefaultCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			isDefaultCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(522, 85, true);
			isDefaultCheckEdit.Name = "isDefaultCheckEdit";
			isDefaultCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 22, true);
			isDefaultCheckEdit.TabIndex = 3;
			// 
			// attachmentNameTextBox
			// 
			this.BindingSource.SetBindingMember(attachmentNameTextBox, "TS_AttachmentName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_AttachmentName)));
			attachmentNameTextBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RateAttachmentSetForm|1259e976-56c7-462d-a82b-db1829e66e3c", "Name");
			attachmentNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 32, true);
			attachmentNameTextBox.Name = "attachmentNameTextBox";
			attachmentNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			attachmentNameTextBox.TabIndex = 1;
			// 
			// templateGuidFindBox
			// 
			templateGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(templateGuidFindBox, "TS_SU");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_SU)));
			templateGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 84, true);
			templateGuidFindBox.Name = "templateGuidFindBox";
			templateGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			templateGuidFindBox.TabIndex = 6;
			// 
			// sequenceCalcEdit
			// 
			sequenceCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(sequenceCalcEdit, "TS_Sequence");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_Sequence)));
			sequenceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("RateAttachmentSetForm|f78e8b1e-983e-4a76-9fbe-cc4643b80060", "Sequence");
			sequenceCalcEdit.DecimalPlaces = 0;
			sequenceCalcEdit.Decimals = 0;
			sequenceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 33, true);
			sequenceCalcEdit.Name = "sequenceCalcEdit";
			sequenceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			sequenceCalcEdit.TabIndex = 5;
			sequenceCalcEdit.Text = "0";
			sequenceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// templateTypeBoundDropEdit
			// 
			templateTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(templateTypeBoundDropEdit, "TS_TemplateType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_TemplateType)));
			templateTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 58, true);
			templateTypeBoundDropEdit.Name = "templateTypeBoundDropEdit";
			templateTypeBoundDropEdit.PreBoundMaxLength = 3;
			templateTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			templateTypeBoundDropEdit.TabIndex = 2;
			// 
			// companyGuidFindBox
			// 
			companyGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(companyGuidFindBox, "TS_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_GC)));
			companyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 110, true);
			companyGuidFindBox.Name = "companyGuidFindBox";
			companyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 20, true);
			companyGuidFindBox.TabIndex = 7;
			// 
			// systemDefinedCheckBox
			// 
			this.BindingSource.SetBindingMember(systemDefinedCheckBox, "TS_IsSystemDefined");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.RateAttachmentSet)(null)).TS_IsSystemDefined)));
			systemDefinedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			systemDefinedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			systemDefinedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 4, true);
			systemDefinedCheckBox.Name = "systemDefinedCheckBox";
			systemDefinedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 22, true);
			systemDefinedCheckBox.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(317, 168, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.PostingButtonsUserControl.TabIndex = 8;
			// 
			// RateAttachmentSetForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(631, 218, true);
			this.Controls.Add(companyGuidFindBox);
			this.Controls.Add(templateTypeBoundDropEdit);
			this.Controls.Add(sequenceCalcEdit);
			this.Controls.Add(templateGuidFindBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(mandatoryCheckEdit);
			this.Controls.Add(systemDefinedCheckBox);
			this.Controls.Add(isDefaultCheckEdit);
			this.Controls.Add(attachmentNameTextBox);
			this.DataSourceAssemblyName = "Enterprise.Rating.Business";
			this.DataSourceType = typeof(Enterprise.Rating.Business.RateAttachmentSet);
			this.DataSourceTypeName = "Enterprise.Rating.Business.RateAttachmentSet";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "RateAttachmentSetForm";
			this.Text = "RateAttachmentSetForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(attachmentNameTextBox, 0);
			this.Controls.SetChildIndex(isDefaultCheckEdit, 0);
			this.Controls.SetChildIndex(systemDefinedCheckBox, 0);
			this.Controls.SetChildIndex(mandatoryCheckEdit, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(templateGuidFindBox, 0);
			this.Controls.SetChildIndex(sequenceCalcEdit, 0);
			this.Controls.SetChildIndex(templateTypeBoundDropEdit, 0);
			this.Controls.SetChildIndex(companyGuidFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private System.ComponentModel.Container components = null;
	}
}
