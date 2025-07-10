using System.Drawing;
using System.Drawing.Drawing2D;
using CargoWise.Windows.UI;
using Enterprise.Freight.CFS.Business;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.CFS.GUI
{
	public partial class AddConsigneeWizardPage : WizardPage
	{
		ZArchitecture.ZLabel WizardPageInstructionsLabel;
		MasterFiles.GUI.ZOrganisationControl CW_OH_ConsigneeBoundOrganisationControl;
		ZArchitecture.GUI.ZCheckBox MarkTemporaryCheckBox;
		ZArchitecture.GUI.ZCheckBox CreateNewCheckBox;
		ZArchitecture.GUI.ZPictureBox StartWizardPictureBox;
		System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CW_OH_ConsigneeBoundOrganisationControl = new MasterFiles.GUI.ZOrganisationControl();
			this.MarkTemporaryCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.WizardPageInstructionsLabel = new ZArchitecture.ZLabel();
			this.CreateNewCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.StartWizardPictureBox = new ZArchitecture.GUI.ZPictureBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StartWizardPictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CoLoadWizardShipment);
			// 
			// CW_OH_ConsigneeBoundOrganisationControl
			// 
			this.BindingSource.SetBindingMember(this.CW_OH_ConsigneeBoundOrganisationControl, "CW_OH_Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CoLoadWizardShipment)(null)).CW_OH_Consignee)));
			this.CW_OH_ConsigneeBoundOrganisationControl.BindToOrganisations = "Consignees";
			this.CW_OH_ConsigneeBoundOrganisationControl.CaptionResourceString = Res.GetData("AddConsigneeWizardPage|a3be46a8-6e7f-4584-a5ab-2d3a27da6f76", "Consignee");
			this.CW_OH_ConsigneeBoundOrganisationControl.Location = ControlDpiScalingHelper.NewScaledPoint(152, 152, true);
			this.CW_OH_ConsigneeBoundOrganisationControl.Name = "CW_OH_ConsigneeBoundOrganisationControl";
			this.CW_OH_ConsigneeBoundOrganisationControl.PopupCaption = "";
			this.CW_OH_ConsigneeBoundOrganisationControl.Size = ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.CW_OH_ConsigneeBoundOrganisationControl.TabIndex = 1;
			// 
			// MarkTemporaryCheckBox
			// 
			this.MarkTemporaryCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.MarkTemporaryCheckBox, "CW_TempOrgMarkTemporary");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CoLoadWizardShipment)(null)).CW_TempOrgMarkTemporary)));
			this.MarkTemporaryCheckBox.CaptionResourceString = Res.GetData("AddConsigneeWizardPage|3f4a1216-f4af-43aa-9a52-4ca7e9d21571", "Mark Temporary");
			this.MarkTemporaryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MarkTemporaryCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(296, 128, true);
			this.MarkTemporaryCheckBox.Name = "MarkTemporaryCheckBox";
			this.MarkTemporaryCheckBox.Size = ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.MarkTemporaryCheckBox.TabIndex = 2;
			// 
			// WizardPageInstructionsLabel
			// 
			this.BindingSource.SetBindingMember(this.WizardPageInstructionsLabel, "CW_TempOrgCreateActionExplaination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CoLoadWizardShipment)(null)).CW_TempOrgCreateActionExplaination)));
			this.WizardPageInstructionsLabel.CaptionResourceString = Res.GetData("AddConsigneeWizardPage|79a35903-7076-409a-8dc2-c981d9c46785", "The following Consignee already exists in the system and will be used for the consignee");
			this.WizardPageInstructionsLabel.Location = ControlDpiScalingHelper.NewScaledPoint(152, 16, true);
			this.WizardPageInstructionsLabel.Name = "WizardPageInstructionsLabel";
			this.WizardPageInstructionsLabel.Size = ControlDpiScalingHelper.NewScaledSize(256, 72, true);
			this.WizardPageInstructionsLabel.TabIndex = 3;
			this.WizardPageInstructionsLabel.TextAlign = ContentAlignment.TopLeft;
			// 
			// CreateNewCheckBox
			// 
			this.CreateNewCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreateNewCheckBox, "CW_TempOrgCreateNew");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((CoLoadWizardShipment)(null)).CW_TempOrgCreateNew)));
			this.CreateNewCheckBox.CaptionResourceString = Res.GetData("AddConsigneeWizardPage|71878436-545d-4073-92d9-007123277a6a", "Create New");
			this.CreateNewCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreateNewCheckBox.Location = ControlDpiScalingHelper.NewScaledPoint(160, 128, true);
			this.CreateNewCheckBox.Name = "CreateNewCheckBox";
			this.CreateNewCheckBox.Size = ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CreateNewCheckBox.TabIndex = 4;
			// 
			// StartWizardPictureBox
			// 
			this.StartWizardPictureBox.Location = ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.StartWizardPictureBox.Name = "StartWizardPictureBox";
			this.StartWizardPictureBox.Size = ControlDpiScalingHelper.NewScaledSize(136, 304, true);
			this.StartWizardPictureBox.TabIndex = 5;
			this.StartWizardPictureBox.TabStop = false;
			this.StartWizardPictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.StartWizardPictureBox_Paint);
			// 
			// AddConsigneeWizardPage
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StartWizardPictureBox);
			this.Controls.Add(this.CreateNewCheckBox);
			this.Controls.Add(this.WizardPageInstructionsLabel);
			this.Controls.Add(this.MarkTemporaryCheckBox);
			this.Controls.Add(this.CW_OH_ConsigneeBoundOrganisationControl);
			this.Name = "AddConsigneeWizardPage";
			this.Size = ControlDpiScalingHelper.NewScaledSize(424, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StartWizardPictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion
	}
}
