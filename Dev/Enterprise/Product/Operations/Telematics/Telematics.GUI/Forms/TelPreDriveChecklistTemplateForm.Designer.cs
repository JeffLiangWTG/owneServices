
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.GUI.Forms
{
	partial class TelPreDriveChecklistTemplateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 577, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 585, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Telematics.Business.TelPreDriveChecklistHeader);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).Entries)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).Entries)).SyncRoot)).TTE_Index)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateEntry)(((System.Collections.IList)(((Enterprise.Telematics.Business.TelPreDriveChecklistTemplateHeader)(null)).Entries)).SyncRoot)).TTE_Description)));
			// 
			// TelPreDriveChecklistTemplateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 641, true);
			this.DataSourceType = typeof(Enterprise.Telematics.Business.TelPreDriveChecklistHeader);
			this.Name = "TelPreDriveChecklistTemplateForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.descriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid)).BeginInit();
			this.zGrid.SuspendLayout();
			this.MainTabPage.Controls.Add(this.descriptionTextBox);
			this.MainTabPage.Controls.Add(this.zGrid);
			// 
			// descriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.descriptionTextBox, "TTH_Description");
			this.descriptionTextBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("097233F0-52A4-42F1-ADF3-EF3B2851BCB8", "Description", "Description", "");
			this.descriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 12, true);
			this.descriptionTextBox.Name = "descriptionTextBox";
			this.descriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 20, true);
			this.descriptionTextBox.TabIndex = 1;
			this.descriptionTextBox.ReadOnly = true;
			// 
			// zGrid
			// 
			this.zGrid.AllowNavigation = false;
			this.zGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zGrid, "Entries");
			this.zGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("21B3C690-952A-4336-8800-9714E7A19399", "Index");
			zTextBoxColumnStyleInfo1.ColumnName = "TTE_Index";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("348B0110-CE21-440A-BDA5-0E2E7352A8EE", "Checkbox Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TTE_Description";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGrid.GridId = "1D7F96AE-16B1-4330-9964-16A524918D52";
			this.zGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid.LayoutKey = "zGrid";
			this.zGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 38, true);
			this.zGrid.Name = "zGrid";
			this.zGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 509, true);
			this.zGrid.TabIndex = 3;
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid)).EndInit();
			this.zGrid.ResumeLayout(false);
			this.zGrid.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		private void NotesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);
		}

		#endregion

		private ZArchitecture.ZTextBox descriptionTextBox;
		private ZArchitecture.ZGrid zGrid;
	}
}
