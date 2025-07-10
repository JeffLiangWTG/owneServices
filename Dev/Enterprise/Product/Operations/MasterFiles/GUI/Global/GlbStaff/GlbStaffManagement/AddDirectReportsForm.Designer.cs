namespace Enterprise.MasterFiles.GUI
{
	partial class AddDirectReportsForm
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
			if (FilterItemModule != null)
			{
				FilterItemModule.Dispose();
				FilterItemModule = null;
			}

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
		new void InitializeComponent()
		{
			this.reportingRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.effectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.roleAndDatePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.roleAndDateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.staffGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.staffGridGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.reportingRoleDropEdit.SuspendLayout();
			this.effectiveDateEdit.SuspendLayout();
			this.roleAndDatePanel.SuspendLayout();
			this.roleAndDateGroupBox.SuspendLayout();
			this.staffGridGroupBox.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 368, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddDirectReportsBizO);
			// 
			// reportingRoleDropEdit
			// 
			this.reportingRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reportingRoleDropEdit, "ManagerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AddDirectReportsBizO)(null)).ManagerType)));
			this.reportingRoleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddDirectReportsForm|ManagerType", "Reporting Role");
			this.reportingRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.reportingRoleDropEdit.Name = "reportingRoleDropEdit";
			this.reportingRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.reportingRoleDropEdit.TabIndex = 0;
			// 
			// effectiveDateEdit
			// 
			this.effectiveDateEdit.AllowDrop = true;
			this.effectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.effectiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.effectiveDateEdit, "EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AddDirectReportsBizO)(null)).EffectiveDate)));
			this.effectiveDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddDirectReportsForm|EffectiveDate", "Effective Date");
			this.effectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.effectiveDateEdit.Name = "effectiveDateEdit";
			this.effectiveDateEdit.TabIndex = 1;
			// 
			// roleAndDatePanel
			// 
			this.roleAndDatePanel.Controls.Add(this.roleAndDateGroupBox);
			this.roleAndDatePanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.roleAndDatePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.roleAndDatePanel.Name = "roleAndDatePanel";
			this.roleAndDatePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 72, true);
			this.roleAndDatePanel.TabIndex = 0;
			// 
			// roleAndDateGroupBox
			// 
			this.roleAndDateGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddDirectReportsForm|RoleAndDateGroupBox", "Set Role and Effective Date");
			this.roleAndDateGroupBox.Controls.Add(this.reportingRoleDropEdit);
			this.roleAndDateGroupBox.Controls.Add(this.effectiveDateEdit);
			this.roleAndDateGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.roleAndDateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.roleAndDateGroupBox.Name = "roleAndDateGroupBox";
			this.roleAndDateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 72, true);
			this.roleAndDateGroupBox.TabIndex = 0;
			this.roleAndDateGroupBox.TabStop = false;
			// 
			// staffGridPanel
			// 
			this.staffGridPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.staffGridPanel.Controls.Add(this.staffGridGroupBox);
			this.staffGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 72, true);
			this.staffGridPanel.Name = "staffGridPanel";
			this.staffGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 290, true);
			this.staffGridPanel.TabIndex = 1;
			// 
			// staffGridGroupBox
			// 
			this.staffGridGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddDirectReportsForm|StaffGridGroupBox", "Staff Members to Add as Direct Reports");
			this.staffGridGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.staffGridGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.staffGridGroupBox.Name = "staffGridGroupBox";
			this.staffGridGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 290, true);
			this.staffGridGroupBox.TabIndex = 0;
			this.staffGridGroupBox.TabStop = false;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.toolStrip);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 392, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 36, true);
			this.bottomPanel.TabIndex = 2;
			// 
			// toolStrip
			// 
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1429, 0, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 25, true);
			this.toolStrip.TabIndex = 0;
			// 
			// AddDirectReportsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AddDirectReportsForm|Title", "Direct Reports");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1531, 428, true);
			this.Controls.Add(this.roleAndDatePanel);
			this.Controls.Add(this.staffGridPanel);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AddDirectReportsBizO);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 467, true);
			this.Name = "AddDirectReportsForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.staffGridPanel, 0);
			this.Controls.SetChildIndex(this.roleAndDatePanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.reportingRoleDropEdit.ResumeLayout(true);
			this.reportingRoleDropEdit.PerformLayout();
			this.effectiveDateEdit.ResumeLayout(true);
			this.effectiveDateEdit.PerformLayout();
			this.roleAndDatePanel.ResumeLayout(false);
			this.roleAndDatePanel.PerformLayout();
			this.roleAndDateGroupBox.ResumeLayout(false);
			this.roleAndDateGroupBox.PerformLayout();
			this.staffGridGroupBox.ResumeLayout(false);
			this.staffGridGroupBox.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit reportingRoleDropEdit;
		private ZArchitecture.GUI.ZDateEdit effectiveDateEdit;
		private Enterprise.ZArchitecture.GUI.ZPanel roleAndDatePanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox roleAndDateGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel staffGridPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox staffGridGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		private Enterprise.ZArchitecture.GUI.ZToolStrip toolStrip;
	}
}
