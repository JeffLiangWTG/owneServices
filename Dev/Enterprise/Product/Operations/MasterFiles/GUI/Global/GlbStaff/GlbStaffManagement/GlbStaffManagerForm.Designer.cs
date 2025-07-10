namespace Enterprise.MasterFiles.GUI
{
	partial class GlbStaffManagerForm
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
		new void InitializeComponent()
		{
			this.reportingRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.staffManagerFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.effectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.reportingManagerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.reportingRoleDropEdit.SuspendLayout();
			this.staffManagerFindBox.SuspendLayout();
			this.effectiveDateEdit.SuspendLayout();
			this.reportingManagerGroupBox.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffManager);
			// 
			// reportingRoleDropEdit
			// 
			this.reportingRoleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reportingRoleDropEdit, "GSM_ManagerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_ManagerType)));
			this.reportingRoleDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerForm|GSM_ManagerType", "Reporting Role");
			this.reportingRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.reportingRoleDropEdit.Name = "reportingRoleDropEdit";
			this.reportingRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.reportingRoleDropEdit.TabIndex = 0;
			// 
			// staffManagerFindBox
			// 
			this.staffManagerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.staffManagerFindBox, "GSM_GS_Manager");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_GS_Manager)));
			this.staffManagerFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerForm|GSM_GS_Manager", "Reports To");
			this.staffManagerFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.staffManagerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.staffManagerFindBox.Name = "staffManagerFindBox";
			this.staffManagerFindBox.PopupCaption = null;
			this.staffManagerFindBox.ShouldResize = true;
			this.staffManagerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 20, true);
			this.staffManagerFindBox.TabIndex = 1;
			// 
			// effectiveDateEdit
			// 
			this.effectiveDateEdit.AllowDrop = true;
			this.effectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.effectiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.effectiveDateEdit, "GSM_EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaffManager)(null)).GSM_EffectiveDate)));
			this.effectiveDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerForm|GSM_EffectiveDate", "Effective Date");
			this.effectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 71, true);
			this.effectiveDateEdit.Name = "effectiveDateEdit";
			this.effectiveDateEdit.TabIndex = 2;
			// 
			// reportingManagerGroupBox
			// 
			this.reportingManagerGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerForm|ReportingManagerGroupBox", "Reporting Manager");
			this.reportingManagerGroupBox.Controls.Add(this.reportingRoleDropEdit);
			this.reportingManagerGroupBox.Controls.Add(this.staffManagerFindBox);
			this.reportingManagerGroupBox.Controls.Add(this.effectiveDateEdit);
			this.reportingManagerGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.reportingManagerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.reportingManagerGroupBox.Name = "reportingManagerGroupBox";
			this.reportingManagerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 104, true);
			this.reportingManagerGroupBox.TabIndex = 0;
			this.reportingManagerGroupBox.TabStop = false;
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = true;
			this.toolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 0, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.toolStrip.TabIndex = 0;
			this.toolStrip.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStrip.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.bottomPanel.Controls.Add(this.toolStrip);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 36, true);
			this.bottomPanel.TabIndex = 1;
			// 
			// GlbStaffManagerForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffManagerForm|Title", "Reporting Manager");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 130, true);
			this.Controls.Add(this.reportingManagerGroupBox);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaffManager);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 210, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 210, true);
			this.Name = "GlbStaffManagerForm";
			this.ShouldSerializeTabPageMethods = false;
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.reportingRoleDropEdit.ResumeLayout(true);
			this.reportingRoleDropEdit.PerformLayout();
			this.staffManagerFindBox.ResumeLayout(true);
			this.staffManagerFindBox.PerformLayout();
			this.effectiveDateEdit.ResumeLayout(true);
			this.effectiveDateEdit.PerformLayout();
			this.reportingManagerGroupBox.ResumeLayout(false);
			this.reportingManagerGroupBox.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit reportingRoleDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox staffManagerFindBox;
		private ZArchitecture.GUI.ZDateEdit effectiveDateEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox reportingManagerGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		private Enterprise.ZArchitecture.GUI.ZToolStrip toolStrip;
	}
}
