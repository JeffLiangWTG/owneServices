using System.Windows.Forms;

namespace Enterprise.Customs.NO.GUI
{
	partial class PreviousProcedureUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.PreviousProcedureControlsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PreviousProceduresGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PreviousProcedureHeaderDynamicPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PreviousProcedureControlsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousProceduresGrid)).BeginInit();
			this.PreviousProceduresGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusEntryInstruction);
			// 
			// PreviousProcedureControlsPanel
			// 
			this.PreviousProcedureControlsPanel.AutoSize = true;
			this.PreviousProcedureControlsPanel.Controls.Add(this.PreviousProceduresGrid);
			this.PreviousProcedureControlsPanel.Controls.Add(this.PreviousProcedureHeaderDynamicPanel);
			this.PreviousProcedureControlsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousProcedureControlsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousProcedureControlsPanel.Name = "PreviousProcedureControlsPanel";
			this.PreviousProcedureControlsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 500, true);
			this.PreviousProcedureControlsPanel.TabIndex = 0;
			// 
			// PreviousProceduresGrid
			// 
			this.PreviousProceduresGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousProceduresGrid, "PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).PreviousDocuments)));
			this.PreviousProceduresGrid.CaptionVisible = false;
			this.PreviousProceduresGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousProceduresGrid.GridId = "65e7b89f-d31c-4894-9ae9-8e7c9b80bf93";
			this.PreviousProceduresGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousProceduresGrid.LayoutKey = "PreviousProceduresGrid";
			this.PreviousProceduresGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 8, true);
			this.PreviousProceduresGrid.Name = "PreviousProceduresGrid";
			this.PreviousProceduresGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 492, true);
			this.PreviousProceduresGrid.TabIndex = 0;
			// 
			// PreviousProcedureHeaderDynamicPanel
			//
			this.BindingSource.SetBindingMember(this.PreviousProcedureHeaderDynamicPanel, "PreviousDocumentMaster");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).PreviousDocumentMaster)));
			this.PreviousProcedureHeaderDynamicPanel.AllowDrop = true;
			this.PreviousProcedureHeaderDynamicPanel.AutoSize = true;
			this.PreviousProcedureHeaderDynamicPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.PreviousProcedureHeaderDynamicPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousProcedureHeaderDynamicPanel.Name = "PreviousProcedureHeaderDynamicPanel";
			this.PreviousProcedureHeaderDynamicPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, 10, 4, 10, true);
			this.PreviousProcedureHeaderDynamicPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 50, true);
			this.PreviousProcedureHeaderDynamicPanel.TabIndex = 0;
			// 
			// PreviousProcedureUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousProcedureControlsPanel);
			this.Name = "PreviousProcedureUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PreviousProcedureControlsPanel.ResumeLayout(false);
			this.PreviousProcedureControlsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousProceduresGrid)).EndInit();
			this.PreviousProceduresGrid.ResumeLayout(false);
			this.PreviousProceduresGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZGrid PreviousProceduresGrid;
		Enterprise.ZArchitecture.GUI.DynamicLayoutPanel PreviousProcedureHeaderDynamicPanel;
		Enterprise.ZArchitecture.GUI.ZPanel PreviousProcedureControlsPanel;
	}
}
