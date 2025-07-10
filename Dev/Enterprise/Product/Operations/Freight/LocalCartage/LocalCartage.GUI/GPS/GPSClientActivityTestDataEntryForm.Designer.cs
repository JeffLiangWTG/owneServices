using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class GPSClientActivityTestDataEntryForm
	{
		System.ComponentModel.Container components = null;

		private ZButton CloseButton;
		private ZButton SaveButton;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZGroupBox zGroupBox1;
		private ZArchitecture.ZGrid GPSClientActivityGrid;
		readonly BusinessObjectFactory Factory;

		new void InitializeComponent()
		{
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GPSClientActivityGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GPSClientActivityGrid)).BeginInit();
			this.GPSClientActivityGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 251, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestDataCollection);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("c7fb6965-8211-48ea-af7c-50bc6ab41beb", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(626, 2, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("2e49ff35-d82c-40e9-a1eb-8ffb723e26f3", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 2, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.Click += new System.EventHandler(this.OnSaveButton_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.zGroupBox1);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.SaveButton);
			this.splitContainer1.Panel2.Controls.Add(this.CloseButton);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 251, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(221);
			this.splitContainer1.TabIndex = 5;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("af252157-5e2d-48e4-8e7e-f63f01e363e7", "Event");
			this.zGroupBox1.Controls.Add(this.GPSClientActivityGrid);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 221, true);
			this.zGroupBox1.TabIndex = 5;
			this.zGroupBox1.TabStop = false;
			// 
			// GPSClientActivityGrid
			// 
			this.GPSClientActivityGrid.AllowNavigation = false;
			this.GPSClientActivityGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.GPSClientActivityGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).EN_RQ_Vehicle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).EN_ActivityID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).EN_ActivityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).EN_ActivityTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).ClientPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestData)(null)).EN_ActivityInformation)));
			this.GPSClientActivityGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EN_RQ_Vehicle";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(88);
			zTextBoxColumnStyleInfo1.ColumnName = "EN_ActivityID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "EN_ActivityType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "EN_ActivityTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "EN_ActivityInformation";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.GPSClientActivityGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.GPSClientActivityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.GPSClientActivityGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.GPSClientActivityGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.GPSClientActivityGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.GPSClientActivityGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.GPSClientActivityGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GPSClientActivityGrid.GridId = "7330f16e-be25-4af9-9d67-92389c4de509";
			this.GPSClientActivityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GPSClientActivityGrid.LayoutKey = "GPSClientActivityGrid";
			this.GPSClientActivityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.GPSClientActivityGrid.Name = "GPSClientActivityGrid";
			this.GPSClientActivityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(701, 204, true);
			this.GPSClientActivityGrid.TabIndex = 1;
			// 
			// GPSClientActivityTestDataEntryForm
			// 
			this.AcceptButton = this.SaveButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("042b973d-a3fd-40c0-8763-540e814a79e7", "GPS Activity Test Data Entry");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 274, true);
			this.Controls.Add(this.splitContainer1);
			this.DataSourceAssemblyName = "Enterprise.Freight.LocalCartage.GUI";
			this.DataSourceType = typeof(Enterprise.Freight.LocalCartage.GUI.GPSSupporterActivityTestDataCollection);
			this.DataSourceTypeName = "Enterprise.Freight.LocalCartage.GUI.GPSClientActivityTestDataCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 312, true);
			this.Name = "GPSClientActivityTestDataEntryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.GPSClientActivityGrid)).EndInit();
			this.GPSClientActivityGrid.ResumeLayout(false);
			this.GPSClientActivityGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
