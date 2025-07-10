namespace Enterprise.Customs.US.GUI
{
	partial class DrawbackMiscOptionsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContractNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ContractNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CertOfManufactureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RulingNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransfereeOrganisationControlWithMiscellaneous = new Enterprise.Customs.GUI.ZOrganisationControlWithMiscellaneous();
			this.DocumentDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DrawbackSameCondCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ACEDrawbackSectionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RejectedReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PetroliumClaimIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PreInspectionIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExporterSummaryIndCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.MergeByDropEdit.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContractNumbersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).BeginInit();
			this.ContractNumbersGrid.SuspendLayout();
			this.TransfereeOrganisationControlWithMiscellaneous.SuspendLayout();
			this.DocumentDetailsGroupBox.SuspendLayout();
			this.RejectedReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 99, true);
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 24, true);
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 73, true);
			this.MiscOptionsGroupBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.JobDeclaration);
			// 
			// ContractNumbersGroupBox
			// 
			this.ContractNumbersGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("960aa6b3-cdb0-4d7e-82a9-3dd1dc5b0213", "Contract Numbers");
			this.ContractNumbersGroupBox.Controls.Add(this.ContractNumbersGrid);
			this.ContractNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 16, true);
			this.ContractNumbersGroupBox.Name = "ContractNumbersGroupBox";
			this.ContractNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 152, true);
			this.ContractNumbersGroupBox.TabIndex = 12;
			this.ContractNumbersGroupBox.TabStop = false;
			this.ContractNumbersGroupBox.Text = "Contract Numbers";
			// 
			// ContractNumbersGrid
			// 
			this.ContractNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContractNumbersGrid, "ContractNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContractNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.ContractNumber)(((System.Collections.IList)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).ContractNumbers)).SyncRoot)).CY_Data)));
			this.ContractNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("037d3bad-c562-4826-a555-74f02b618f05", "Drawback Contract Numbers");
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ContractNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContractNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContractNumbersGrid.GridId = "293173f0-84ce-4913-bce5-1f5c4ee6b3c0";
			this.ContractNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContractNumbersGrid.LayoutKey = "ContractNumbersGrid";
			this.ContractNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.ContractNumbersGrid.Name = "ContractNumbersGrid";
			this.ContractNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 135, true);
			this.ContractNumbersGrid.TabIndex = 0;
			// 
			// CertOfManufactureTextBox
			// 
			this.BindingSource.SetBindingMember(this.CertOfManufactureTextBox, "US_DRWCertOfManufacture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWCertOfManufacture)));
			this.CertOfManufactureTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2d983bf1-dd78-4160-96bf-ba51a7e619da", "Certificate of Manufacture");
			this.CertOfManufactureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 46, true);
			this.CertOfManufactureTextBox.Name = "CertOfManufactureTextBox";
			this.CertOfManufactureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 18, true);
			this.CertOfManufactureTextBox.TabIndex = 1;
			// 
			// RulingNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RulingNoTextBox, "US_DRWRulingNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWRulingNo)));
			this.RulingNoTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("e317c7b9-62ee-4266-89f8-dca6765d9878", "Ruling No.");
			this.RulingNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(385, 46, true);
			this.RulingNoTextBox.Name = "RulingNoTextBox";
			this.RulingNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 18, true);
			this.RulingNoTextBox.TabIndex = 2;
			// 
			// TransfereeOrganisationControlWithMiscellaneous
			// 
			this.TransfereeOrganisationControlWithMiscellaneous.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransfereeOrganisationControlWithMiscellaneous, "US_DRWTransferee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWTransferee)));
			this.TransfereeOrganisationControlWithMiscellaneous.BindToMiscellaneousFields = "JE_TransfereeMiscFields";
			this.TransfereeOrganisationControlWithMiscellaneous.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("c79b493b-0cb9-404e-b76b-63bda04a4d37", "Transferee");
			this.TransfereeOrganisationControlWithMiscellaneous.Captions = new string[] {
        "Transferee"};
			this.TransfereeOrganisationControlWithMiscellaneous.IsCaptionOverridden = false;
			this.TransfereeOrganisationControlWithMiscellaneous.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(767, 16, true);
			this.TransfereeOrganisationControlWithMiscellaneous.Name = "TransfereeOrganisationControlWithMiscellaneous";
			this.TransfereeOrganisationControlWithMiscellaneous.PopupCaption = "";
			this.TransfereeOrganisationControlWithMiscellaneous.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 152, true);
			this.TransfereeOrganisationControlWithMiscellaneous.TabIndex = 13;
			// 
			// DocumentDetailsGroupBox
			// 
			this.DocumentDetailsGroupBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d1259606-bc6f-408d-83b5-430da928db18", "Document Details");
			this.DocumentDetailsGroupBox.Controls.Add(this.DrawbackSameCondCheckBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.ACEDrawbackSectionTextBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.ContractNumbersGroupBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.RejectedReasonDropEdit);
			this.DocumentDetailsGroupBox.Controls.Add(this.PetroliumClaimIndCheckBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.PreInspectionIndCheckBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.ExporterSummaryIndCheckBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.RulingNoTextBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.CertOfManufactureTextBox);
			this.DocumentDetailsGroupBox.Controls.Add(this.TransfereeOrganisationControlWithMiscellaneous);
			this.DocumentDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DocumentDetailsGroupBox.Name = "DocumentDetailsGroupBox";
			this.DocumentDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 195, true);
			this.DocumentDetailsGroupBox.TabIndex = 3;
			this.DocumentDetailsGroupBox.TabStop = false;
			this.DocumentDetailsGroupBox.Text = "Document Details";
			// 
			// DrawbackSameCondCheckBox
			// 
			this.BindingSource.SetBindingMember(this.DrawbackSameCondCheckBox, "US_DRWSameCondNAFTA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWSameCondNAFTA)));
			this.DrawbackSameCondCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("cc1e4250-eeac-4109-8eda-4ab51b6d46b5", "Same Condition Drawback under NAFTA");
			this.DrawbackSameCondCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.DrawbackSameCondCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.DrawbackSameCondCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 116, true);
			this.DrawbackSameCondCheckBox.Name = "DrawbackSameCondCheckBox";
			this.DrawbackSameCondCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 24, true);
			this.DrawbackSameCondCheckBox.TabIndex = 7;
			this.DrawbackSameCondCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DrawbackSameCondCheckBox.UseVisualStyleBackColor = true;
			// 
			// ACEDrawbackSectionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ACEDrawbackSectionTextBox, "US_DRWSection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWSection)));
			this.ACEDrawbackSectionTextBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("d37d6f4e-08b8-490d-8929-ef1d17419eef", "Drawback Section");
			this.ACEDrawbackSectionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 71, true);
			this.ACEDrawbackSectionTextBox.Name = "ACEDrawbackSectionTextBox";
			this.ACEDrawbackSectionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.ACEDrawbackSectionTextBox.TabIndex = 3;
			// 
			// RejectedReasonDropEdit
			// 
			this.RejectedReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RejectedReasonDropEdit, "US_DRWRejectedMerchandiseReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_DRWRejectedMerchandiseReason)));
			this.RejectedReasonDropEdit.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("2553f9ab-2566-4066-b2c8-8d0a6cd23ec4", "Rejected Merchandise Reason");
			this.RejectedReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 21, true);
			this.RejectedReasonDropEdit.Name = "RejectedReasonDropEdit";
			this.RejectedReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 18, true);
			this.RejectedReasonDropEdit.TabIndex = 0;
			// 
			// PetroliumClaimIndCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PetroliumClaimIndCheckBox, "US_PetroleumClaimInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PetroleumClaimInd)));
			this.PetroliumClaimIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ab6631f4-5363-49d3-a541-4292ab90f5ea", "Petroleum Claim Indicator", "Petroleum Claim Indicator", "Tick if petroleum or petroleum product.");
			this.PetroliumClaimIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PetroliumClaimIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PetroliumClaimIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 116, true);
			this.PetroliumClaimIndCheckBox.Name = "PetroliumClaimIndCheckBox";
			this.PetroliumClaimIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 24, true);
			this.PetroliumClaimIndCheckBox.TabIndex = 6;
			this.PetroliumClaimIndCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PetroliumClaimIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// PreInspectionIndCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PreInspectionIndCheckBox, "US_PreInspectionInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_PreInspectionInd)));
			this.PreInspectionIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("b213c287-d836-436d-a67e-33a33bb24744", "Pre Inspection Indicator", "Pre Inspection Indicator", "Tick if pre inspection is indicated.");
			this.PreInspectionIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PreInspectionIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PreInspectionIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 94, true);
			this.PreInspectionIndCheckBox.Name = "PreInspectionIndCheckBox";
			this.PreInspectionIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 24, true);
			this.PreInspectionIndCheckBox.TabIndex = 5;
			this.PreInspectionIndCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PreInspectionIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExporterSummaryIndCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ExporterSummaryIndCheckBox, "US_ExporterSummaryInd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.US.Business.JobDeclaration)(null)).US_ExporterSummaryInd)));
			this.ExporterSummaryIndCheckBox.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("09ce12f4-5f14-4196-b4c0-b490e8120f08", "Exporter Summary Indicator", "Exporter Summary Procedure Indicator", "Tick to indicate that exporter summary procedure is used.");
			this.ExporterSummaryIndCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ExporterSummaryIndCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExporterSummaryIndCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 94, true);
			this.ExporterSummaryIndCheckBox.Name = "ExporterSummaryIndCheckBox";
			this.ExporterSummaryIndCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 24, true);
			this.ExporterSummaryIndCheckBox.TabIndex = 4;
			this.ExporterSummaryIndCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ExporterSummaryIndCheckBox.UseVisualStyleBackColor = true;
			// 
			// DrawbackMiscOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.DocumentDetailsGroupBox);
			this.Name = "DrawbackMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1032, 607, true);
			this.Controls.SetChildIndex(this.MiscOptionsGroupBox, 0);
			this.Controls.SetChildIndex(this.DocumentDetailsGroupBox, 0);
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.MergeByDropEdit.ResumeLayout(true);
			this.MergeByDropEdit.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContractNumbersGroupBox.ResumeLayout(false);
			this.ContractNumbersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).EndInit();
			this.ContractNumbersGrid.ResumeLayout(false);
			this.ContractNumbersGrid.PerformLayout();
			this.TransfereeOrganisationControlWithMiscellaneous.ResumeLayout(true);
			this.TransfereeOrganisationControlWithMiscellaneous.PerformLayout();
			this.DocumentDetailsGroupBox.ResumeLayout(false);
			this.DocumentDetailsGroupBox.PerformLayout();
			this.RejectedReasonDropEdit.ResumeLayout(true);
			this.RejectedReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ContractNumbersGroupBox;
		private ZArchitecture.ZGrid ContractNumbersGrid;
		private Customs.GUI.ZOrganisationControlWithMiscellaneous TransfereeOrganisationControlWithMiscellaneous;
		private ZArchitecture.ZTextBox RulingNoTextBox;
		private ZArchitecture.ZTextBox CertOfManufactureTextBox;
		private ZArchitecture.GUI.ZGroupBox DocumentDetailsGroupBox;
		private ZArchitecture.GUI.ZCheckBox ExporterSummaryIndCheckBox;
		private ZArchitecture.GUI.ZCheckBox PreInspectionIndCheckBox;
		private ZArchitecture.GUI.ZCheckBox PetroliumClaimIndCheckBox;
		private ZArchitecture.GUI.ZDropEdit RejectedReasonDropEdit;
		private ZArchitecture.GUI.ZCheckBox DrawbackSameCondCheckBox;
		private ZArchitecture.ZTextBox ACEDrawbackSectionTextBox;
	}
}
