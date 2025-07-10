namespace Enterprise.MasterFiles.GUI
{
	public partial class SuppressDocsForOrgForm
	{

		#region Windows Form Designer generated code

		protected Enterprise.ZArchitecture.ZGrid OrgDocumentBoundGrid;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.OrgDocumentBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgDocumentBoundGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 246, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// OrgDocumentBoundGrid
			// 
			this.OrgDocumentBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OrgDocumentBoundGrid, "SuppressedDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_DocumentGroup)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_SU_MenuItem)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_FilterShipmentMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_OH_RelatedFilterByParty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_FilterLocalPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_FilterForeignPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgDocument)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SuppressedDocuments)).SyncRoot)).OD_FilterDirection)));
			this.OrgDocumentBoundGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "OD_DocumentGroup";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OD_SU_MenuItem";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "OD_FilterShipmentMode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OD_OH_RelatedFilterByParty";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OD_FilterLocalPort";
			zCodeFindBoxColumnStyleInfo1.PopupCaption = "Local Port for documents.";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "OD_FilterForeignPort";
			zCodeFindBoxColumnStyleInfo2.PopupCaption = "Foreign Port for documents";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "OD_FilterDirection";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.OrgDocumentBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OrgDocumentBoundGrid.GridId = "6f6ffc56-55f6-4f98-a65d-17f41ffb7667";
			this.OrgDocumentBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgDocumentBoundGrid.LayoutKey = "OrgDocumentBoundGrid";
			this.OrgDocumentBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.OrgDocumentBoundGrid.Name = "OrgDocumentBoundGrid";
			this.OrgDocumentBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 200, true);
			this.OrgDocumentBoundGrid.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SuppressDocsForOrgForm|e7636c0b-c43c-4cdb-892c-391cf1850e50", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 214, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SuppressDocsForOrgForm|27e514b0-f276-4830-ab19-4717f0caec46", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 214, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 1;
			// 
			// SuppressDocsForOrgForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 270, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OrgDocumentBoundGrid);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "SuppressDocsForOrgForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OrgDocumentBoundGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgDocumentBoundGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
