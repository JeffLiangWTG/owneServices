using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI;

partial class AlternativeEvidenceControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.AlternativeEvidenceGrid = new Enterprise.ZArchitecture.ZGrid();
            this.OfficeOfExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.EnquiryInformationCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ExitDateDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).BeginInit();
            this.AlternativeEvidenceGrid.SuspendLayout();
            this.OfficeOfExitCodeFindBox.SuspendLayout();
            this.EnquiryInformationCodeDropEdit.SuspendLayout();
            this.ExitDateDateTimeOffsetEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent);
            // 
            // AlternativeEvidenceGrid
            // 
            this.AlternativeEvidenceGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AlternativeEvidenceGrid, "AlternativeEvidences");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AlternativeEvidences)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AlternativeEvidences)).SyncRoot)).EvidenceType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AlternativeEvidences)).SyncRoot)).DocType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).AlternativeEvidences)).SyncRoot)).Reference)));
            this.AlternativeEvidenceGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "EvidenceType";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.ColumnName = "DocType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo1.ColumnName = "Reference";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.AlternativeEvidenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.AlternativeEvidenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.AlternativeEvidenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AlternativeEvidenceGrid.GridId = "9eb34d06-cc61-4a25-a4f9-a0876bd0c252";
            this.AlternativeEvidenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AlternativeEvidenceGrid.LayoutKey = "zGrid1";
            this.AlternativeEvidenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 13, true);
            this.AlternativeEvidenceGrid.Name = "AlternativeEvidenceGrid";
            this.AlternativeEvidenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 103, true);
            this.AlternativeEvidenceGrid.TabIndex = 0;
            // 
            // OfficeOfExitCodeFindBox
            // 
            this.OfficeOfExitCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OfficeOfExitCodeFindBox, "OfficeOfExitActual");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).OfficeOfExitActual)));
            this.OfficeOfExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 91, true);
            this.OfficeOfExitCodeFindBox.Name = "OfficeOfExitCodeFindBox";
            this.OfficeOfExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.OfficeOfExitCodeFindBox.ParentType = null;
            this.OfficeOfExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
            this.OfficeOfExitCodeFindBox.TabIndex = 1;
            // 
            // EnquiryInformationCodeDropEdit
            // 
            this.EnquiryInformationCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EnquiryInformationCodeDropEdit, "EnquiryInformationCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).EnquiryInformationCode)));
            this.EnquiryInformationCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 69, true);
            this.EnquiryInformationCodeDropEdit.Name = "EnquiryInformationCodeDropEdit";
            this.EnquiryInformationCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 25, true);
            this.EnquiryInformationCodeDropEdit.TabIndex = 2;
            // 
            // ExitDateDateTimeOffsetEdit
            // 
            this.ExitDateDateTimeOffsetEdit.AllowDrop = true;
            this.ExitDateDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ExitDateDateTimeOffsetEdit, "ExitDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).ExitDate)));
            this.ExitDateDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.ExitDateDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 112, true);
            this.ExitDateDateTimeOffsetEdit.Name = "ExitDateDateTimeOffsetEdit";
            this.ExitDateDateTimeOffsetEdit.TabIndex = 3;
            // 
            // AlternativeEvidenceControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ExitDateDateTimeOffsetEdit);
            this.Controls.Add(this.EnquiryInformationCodeDropEdit);
            this.Controls.Add(this.OfficeOfExitCodeFindBox);
            this.Controls.Add(this.AlternativeEvidenceGrid);
            this.Name = "AlternativeEvidenceControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 214, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).EndInit();
            this.AlternativeEvidenceGrid.ResumeLayout(false);
            this.AlternativeEvidenceGrid.PerformLayout();
            this.OfficeOfExitCodeFindBox.ResumeLayout(true);
            this.OfficeOfExitCodeFindBox.PerformLayout();
            this.EnquiryInformationCodeDropEdit.ResumeLayout(true);
            this.EnquiryInformationCodeDropEdit.PerformLayout();
            this.ExitDateDateTimeOffsetEdit.ResumeLayout(true);
            this.ExitDateDateTimeOffsetEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.ZGrid AlternativeEvidenceGrid;
	internal ZCodeFindBox OfficeOfExitCodeFindBox;
	internal ZDropEdit EnquiryInformationCodeDropEdit;
	internal ZDateTimeOffsetEdit ExitDateDateTimeOffsetEdit;
}
