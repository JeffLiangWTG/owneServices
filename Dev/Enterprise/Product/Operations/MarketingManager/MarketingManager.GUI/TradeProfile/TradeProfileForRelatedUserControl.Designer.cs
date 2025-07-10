namespace Enterprise.MarketingManager.GUI
{
	partial class TradeProfileForRelatedUserControl
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
			this.salesValueAnalysisControl = new Enterprise.MarketingManager.GUI.OpportunitySalesValueAnalysisControl();
			this.dynamicTradeLanesControl = new Enterprise.MarketingManager.GUI.DynamicTradeLanesControl();
			this.splitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.toolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.UpdateProspectStatusButton = new ZArchitecture.GUI.ZToolStripButton();
			this.editProspectValuesButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.newProspectValueButton = new Enterprise.ZArchitecture.GUI.ZToolStripDropDownButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.salesValueAnalysisControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
			this.splitContainer.Panel1.SuspendLayout();
			this.splitContainer.Panel2.SuspendLayout();
			this.splitContainer.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.EntityFramework.BusinessObject);
			// 
			// salesValueAnalysisControl
			// 
			this.salesValueAnalysisControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.salesValueAnalysisControl, ".");
			this.salesValueAnalysisControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesValueAnalysisControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.salesValueAnalysisControl.Name = "salesValueAnalysisControl";
			this.salesValueAnalysisControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
			this.salesValueAnalysisControl.TabIndex = 2;
			// 
			// dynamicTradeLanesControl
			// 
			this.dynamicTradeLanesControl.AllowDrop = true;
			this.dynamicTradeLanesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dynamicTradeLanesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 0, true);
			this.dynamicTradeLanesControl.Name = "dynamicTradeLanesControl";
			this.dynamicTradeLanesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(490, 129, true);
			this.dynamicTradeLanesControl.TabIndex = 3;
			// 
			// splitContainer
			// 
			this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.splitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer.Name = "splitContainer";
			this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer.Panel1
			// 
			this.splitContainer.Panel1.Controls.Add(this.salesValueAnalysisControl);
			this.splitContainer.Panel1MinSize = 50;
			// 
			// splitContainer.Panel2
			// 
			this.splitContainer.Panel2.Controls.Add(this.dynamicTradeLanesControl);
			this.splitContainer.Panel2.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.splitContainer.Panel2MinSize = 50;
			this.splitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 231, true);
			this.splitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.splitContainer.TabIndex = 4;
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.UpdateProspectStatusButton,
			this.editProspectValuesButton,
			this.newProspectValueButton});
			this.toolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 231, true);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 20, true);
			this.toolStrip.TabIndex = 5;
			this.toolStrip.Text = "zToolStrip1";
			// 
			// updateProspectStatusButton
			// 
			this.UpdateProspectStatusButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.UpdateProspectStatusButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("91154dbb-092b-4674-8973-67ae7f864a3c", "Update Estimate Status");
			this.UpdateProspectStatusButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.UpdateProspectStatusButton.Name = "updateProspectStatusButton";
			this.UpdateProspectStatusButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 36, true);
			this.UpdateProspectStatusButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.UpdateProspectStatusButton.Click += UpdateProspectStatusButton_Click;
			// 
			// editProspectValuesButton
			// 
			this.editProspectValuesButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.editProspectValuesButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("21da8258-621d-44f9-b5f0-798886916657", "Edit Estimate Values");
			this.editProspectValuesButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.editProspectValuesButton.Name = "editProspectValuesButton";
			this.editProspectValuesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 36, true);
			this.editProspectValuesButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.editProspectValuesButton.Click += new System.EventHandler(this.EditProspectValuesButton_Click);
			// 
			// newProspectValueButton
			// 
			this.newProspectValueButton.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.newProspectValueButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("823e5b10-ffbf-4160-80d8-56db2abebcec", "Add Estimate Value");
			this.newProspectValueButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.newProspectValueButton.Name = "newProspectValueButton";
			this.newProspectValueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 36, true);
			this.newProspectValueButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// TradeProfileForRelatedUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer);
			this.Controls.Add(this.toolStrip);
			this.Name = "TradeProfileForRelatedUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.salesValueAnalysisControl.ResumeLayout(true);
			this.salesValueAnalysisControl.PerformLayout();
			this.splitContainer.Panel1.ResumeLayout(false);
			this.splitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
			this.splitContainer.ResumeLayout(false);
			this.splitContainer.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private OpportunitySalesValueAnalysisControl salesValueAnalysisControl;
		private DynamicTradeLanesControl dynamicTradeLanesControl;
		private CargoWise.Windows.UI.KSplitContainer splitContainer;
		private ZArchitecture.GUI.ZToolStrip toolStrip;
		internal ZArchitecture.GUI.ZToolStripButton UpdateProspectStatusButton;
		protected ZArchitecture.GUI.ZToolStripButton editProspectValuesButton;
		protected ZArchitecture.GUI.ZToolStripDropDownButton newProspectValueButton;
	}
}
