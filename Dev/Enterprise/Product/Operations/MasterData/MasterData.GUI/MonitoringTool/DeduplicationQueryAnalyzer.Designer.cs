using Enterprise.ZArchitecture;

namespace Enterprise.MasterData.GUI
{
	partial class DeduplicationQueryAnalyzer
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new void InitializeComponent()
		{
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.QueryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GridViewResult = new Enterprise.ZArchitecture.GUI.ZDisplayGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GridViewResult)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 555, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 24, true);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.QueryTextBox);
			this.splitContainer1.Panel1MinSize = 50;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.GridViewResult);
			this.splitContainer1.Panel2MinSize = 410;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 579, true);
			this.splitContainer1.SplitterDistance = 140;
			this.splitContainer1.TabIndex = 1;
			// 
			// QueryTextBox
			// 
			this.QueryTextBox.AcceptsReturn = true;
			this.QueryTextBox.AcceptsTab = true;
			this.QueryTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.QueryTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.QueryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QueryTextBox.Multiline = true;
			this.QueryTextBox.Name = "QueryTextBox";
			this.QueryTextBox.ReadOnly = true;
			this.QueryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.QueryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 319, true);
			this.QueryTextBox.TabIndex = 0;
			this.QueryTextBox.WordWrap = false;
			this.QueryTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.QueryTextBox_KeyUp);
			// 
			// GridViewResult
			// 
			this.GridViewResult.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GridViewResult.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GridViewResult.Name = "GridViewResult";
			this.GridViewResult.AllowNavigation = false;
			this.GridViewResult.AllowBeginDrag = false;
			this.GridViewResult.AllowSorting = false;
			this.GridViewResult.ReadOnly = true;
			this.GridViewResult.DisableImportDataMenuItem = true;
			this.GridViewResult.IsCustomiseMenuVisible = false;
			this.GridViewResult.IsWholeRowSelectedOnClick = true;
			this.GridViewResult.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 579, true);
			this.GridViewResult.TabIndex = 1;
			// 
			// DeduplicationQueryAnalyzer
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(819, 579, true);
			this.Controls.Add(this.splitContainer1);
			this.KeyPreview = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "DeduplicationQueryAnalyzer";
			this.Text = "De-duplication Query Analyzer";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GridViewResult)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private ZTextBox QueryTextBox;
		private Enterprise.ZArchitecture.GUI.ZDisplayGrid GridViewResult;
	}
}
