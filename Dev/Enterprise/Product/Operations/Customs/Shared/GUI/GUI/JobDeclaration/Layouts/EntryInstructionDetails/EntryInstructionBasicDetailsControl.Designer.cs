using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class EntryInstructionBasicDetailsControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SubStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AssessmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DetailsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NewOwnerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.RemoverOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.BondHolderOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.OtherPartiesSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StyleDropEdit.SuspendLayout();
			this.SubStyleDropEdit.SuspendLayout();
			this.AssessmentDateEdit.SuspendLayout();
			this.NewOwnerOrganisationControl.SuspendLayout();
			this.RemoverOrganisationControl.SuspendLayout();
			this.BondHolderOrganisationControl.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusEntryInstruction);
			//
			// StyleDropEdit
			//
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_Style)));
			this.StyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 10, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.StyleDropEdit.TabIndex = 1;
			// 
			// SubStyleDropEdit
			// 
			this.SubStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubStyleDropEdit, "CEI_SubStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubStyle)));
			this.SubStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 12, true);
			this.SubStyleDropEdit.Name = "SubStyleDropEdit";
			this.SubStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.SubStyleDropEdit.TabIndex = 2;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "CEI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_Procedure)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 40, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CPCDropEdit.TabIndex = 2;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CEI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 40, true);
			this.DescriptionTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 0, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// AssessmentDateEdit
			// 
			this.AssessmentDateEdit.AllowDrop = true;
			this.AssessmentDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AssessmentDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.AssessmentDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AssessmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 82, true);
			this.AssessmentDateEdit.Name = "AssessmentDateEdit";
			this.AssessmentDateEdit.TabIndex = 3;
			//
			// DetailsLabel
			//
			this.DetailsLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D76D604D-3D31-4349-BA59-06929BB9C6DA", "Details");
			this.DetailsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.DetailsLabel.IsFontBold = true;
			this.DetailsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 100, true);
			this.DetailsLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.DetailsLabel.Name = "DetailsLabel";
			this.DetailsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DetailsLabel.TabIndex = 3;
			// 
			// OtherPartiesSeparatorUserControl
			// 
			this.OtherPartiesSeparatorUserControl.AllowDrop = true;
			this.OtherPartiesSeparatorUserControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5EA7D48A-8821-4537-A932-127544BCF45B", "Other Parties");
			this.OtherPartiesSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 130, true);
			this.OtherPartiesSeparatorUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.OtherPartiesSeparatorUserControl.Name = "OtherPartiesSeparatorUserControl";
			this.OtherPartiesSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.OtherPartiesSeparatorUserControl.TabIndex = 4;
			//
			// NewOwnerOrganisationControl
			//
			this.NewOwnerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_OH_Owner)));
			this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("D623ADEB-C623-4EC5-BF01-01A10D610EAC", "New Owner");
			this.NewOwnerOrganisationControl.Captions = new string[] {
		"New Owner"};
			this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.NewOwnerOrganisationControl.IsCaptionOverridden = false;
			this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 180, true);
			this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
			this.NewOwnerOrganisationControl.OrgAddressFormatter = null;
			this.NewOwnerOrganisationControl.PopupCaption = "";
			this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.NewOwnerOrganisationControl.TabIndex = 8;
			//
			// RemoverOrganisationControl
			//
			this.RemoverOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RemoverOrganisationControl, "CEI_OH_Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_OH_Carrier)));
			this.RemoverOrganisationControl.BindToOrganisations = "Lookups+CarrierOrganisations";
			this.RemoverOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5CED68E6-D229-41B5-9138-E8C200261E91", "Remover");
			this.RemoverOrganisationControl.Captions = new string[] {
		"Remover"};
			this.RemoverOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.RemoverOrganisationControl.IsCaptionOverridden = false;
			this.RemoverOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 210, true);
			this.RemoverOrganisationControl.Name = "RemoverOrganisationControl";
			this.RemoverOrganisationControl.OrgAddressFormatter = null;
			this.RemoverOrganisationControl.PopupCaption = "";
			this.RemoverOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 300, true);
			this.RemoverOrganisationControl.TabIndex = 9;
			//
			// BondHolderOrganisationControl
			//
			this.BondHolderOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondHolderOrganisationControl, "CEI_OH_BondHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.CusEntryInstruction)(null)).CEI_OH_BondHolder)));
			this.BondHolderOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.BondHolderOrganisationControl.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("0A9777AC-AC70-4440-B6AC-0C4D464E6685", "Bond Holder");
			this.BondHolderOrganisationControl.Captions = new string[] {
		"Bond Holder"};
			this.BondHolderOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.BondHolderOrganisationControl.IsCaptionOverridden = false;
			this.BondHolderOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 250, true);
			this.BondHolderOrganisationControl.Name = "BondHolderOrganisationControl";
			this.BondHolderOrganisationControl.OrgAddressFormatter = null;
			this.BondHolderOrganisationControl.PopupCaption = "";
			this.BondHolderOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.BondHolderOrganisationControl.TabIndex = 10;
			//
			// EntryInstructionBasicDetailsControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SubStyleDropEdit);
			this.Controls.Add(this.StyleDropEdit);
			this.Controls.Add(this.CPCDropEdit);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.AssessmentDateEdit);
			this.Controls.Add(this.DetailsLabel);
			this.Controls.Add(this.OtherPartiesSeparatorUserControl);
			this.Controls.Add(this.BondHolderOrganisationControl);
			this.Controls.Add(this.NewOwnerOrganisationControl);
			this.Controls.Add(this.RemoverOrganisationControl);
			this.Name = "EntryInstructionBasicDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.SubStyleDropEdit.ResumeLayout(true);
			this.SubStyleDropEdit.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.AssessmentDateEdit.ResumeLayout(true);
			this.AssessmentDateEdit.PerformLayout();
			this.OtherPartiesSeparatorUserControl.ResumeLayout(true);
			this.OtherPartiesSeparatorUserControl.PerformLayout();
			this.NewOwnerOrganisationControl.ResumeLayout(true);
			this.NewOwnerOrganisationControl.PerformLayout();
			this.RemoverOrganisationControl.ResumeLayout(true);
			this.RemoverOrganisationControl.PerformLayout();
			this.BondHolderOrganisationControl.ResumeLayout(true);
			this.BondHolderOrganisationControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit StyleDropEdit;
		internal ZDropEdit SubStyleDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CPCDropEdit;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
		internal ZArchitecture.GUI.ZDateEdit AssessmentDateEdit;
		internal ZLabel DetailsLabel;
		internal SeparatorUserControl OtherPartiesSeparatorUserControl;
		internal MasterFiles.GUI.ZOrganisationControl BondHolderOrganisationControl;
		internal MasterFiles.GUI.ZOrganisationControl RemoverOrganisationControl;
		internal MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
	}
}
