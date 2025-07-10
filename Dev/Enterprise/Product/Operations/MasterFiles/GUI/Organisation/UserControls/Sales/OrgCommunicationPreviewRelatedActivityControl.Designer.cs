namespace Enterprise.MasterFiles.GUI
{
	partial class OrgCommunicationPreviewRelatedActivityControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RelatedActivityGrid = new Enterprise.MasterFiles.GUI.RelatedActivityButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedActivityGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgSalesCall);
			// 
			// RelatedActivityGrid
			// 
			this.RelatedActivityGrid.AllowDrop = true;
			this.RelatedActivityGrid.AttachButtonText = null;
			this.BindingSource.SetBindingMember(this.RelatedActivityGrid, "RelatedActivityLinkCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.OrgSalesCall)(null)).RelatedActivityLinkCollection)));
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5A579CDE-101E-446F-83D1-4C45DB24B779", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "ToActivityTypeForBinding";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("A56E3771-0EA0-4E70-A7E5-1A639BE4EE57", "ID");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ToActivityIDForBinding";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("854B1024-BA74-44C8-B743-218C183CCC6C", "Information");
			zTextBoxColumnStyleInfo1.ColumnName = "ToActivityForBinding+Summary";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3BB93C3C-1FD8-4710-808C-DC638DE0D4AB", "Created Time");
			zDateEditColumnStyleInfo1.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemCreateTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AEA3F041-FD1E-4ED2-8AF1-8B087C58112F", "Creating User");
			zTextBoxColumnStyleInfo2.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4FBDCB1B-E950-41A7-AC66-E2F1C8F6FD16", "Last Edit Time");
			zDateEditColumnStyleInfo2.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemLastEditTime";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("13AC0B82-421B-4FBA-A014-B117CB50A77E", "Last Edit User");
			zTextBoxColumnStyleInfo3.ColumnName = "ToActivityForBindingLocalAuditDetails+SystemLastEditUser";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.RelatedActivityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RelatedActivityGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.RelatedActivityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedActivityGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RelatedActivityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedActivityGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RelatedActivityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedActivityGrid.DetachButtonText = null;
			this.RelatedActivityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedActivityGrid.GridId = "950a9772-8c0c-4094-9fee-b3a147973b8d";
			// 
			// 
			// 
			this.RelatedActivityGrid.InnerGrid.AllowNavigation = false;
			this.RelatedActivityGrid.InnerGrid.CaptionVisible = false;
			this.RelatedActivityGrid.InnerGrid.CopySelectedRowsAllowed = true;
			this.RelatedActivityGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelatedActivityGrid.InnerGrid.GridId = null;
			this.RelatedActivityGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedActivityGrid.InnerGrid.LayoutKey = "Grid";
			this.RelatedActivityGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RelatedActivityGrid.InnerGrid.Name = "Grid";
			this.RelatedActivityGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 283, true);
			this.RelatedActivityGrid.InnerGrid.TabIndex = 0;
			this.RelatedActivityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedActivityGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.RelatedActivityGrid.Name = "RelatedActivityGrid";
			this.RelatedActivityGrid.NewButtonText = null;
			this.RelatedActivityGrid.ReadOnly = false;
			this.RelatedActivityGrid.ShowNewButton = false;
			this.RelatedActivityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 321, true);
			this.RelatedActivityGrid.TabIndex = 0;
			// 
			// OrgCommunicationPreviewRelatedActivityControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RelatedActivityGrid);
			this.Name = "OrgCommunicationPreviewRelatedActivityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 321, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedActivityGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private RelatedActivityButtonGrid RelatedActivityGrid;
	}
}
