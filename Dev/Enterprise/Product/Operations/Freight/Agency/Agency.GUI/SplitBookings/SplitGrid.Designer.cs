using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.GUI
{
	partial class SplitGrid
	{
		void InitializeComponent()
		{
			this.packMidSection = new CargoWise.Windows.UI.KPanel();
			this.splitControl = new CargoWise.Windows.UI.KSplitContainer();
			this.grid1 = new Enterprise.ZArchitecture.ZGrid();
			this.grid2 = new Enterprise.ZArchitecture.ZGrid();
			floatingMidPanel = new CargoWise.Windows.UI.KPanel();
			moveRightButton = new Enterprise.ZArchitecture.GUI.ZButton();
			moveLeftButton = new Enterprise.ZArchitecture.GUI.ZButton();
			gridLabel1 = new Enterprise.ZArchitecture.ZLabel();
			gridLabel2 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			floatingMidPanel.SuspendLayout();
			this.packMidSection.SuspendLayout();
			this.splitControl.Panel1.SuspendLayout();
			this.splitControl.Panel2.SuspendLayout();
			this.splitControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).BeginInit();
			this.SuspendLayout();
			// 
			// floatingMidPanel
			// 
			floatingMidPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
			floatingMidPanel.Controls.Add(moveRightButton);
			floatingMidPanel.Controls.Add(moveLeftButton);
			floatingMidPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 53, true);
			floatingMidPanel.Name = "floatingMidPanel";
			floatingMidPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 56, true);
			floatingMidPanel.TabIndex = 1;
			// 
			// moveRightButton
			// 
			moveRightButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitGrid|6b999161-e3c1-415f-9eab-6eea9f874e8f", "-->");
			moveRightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			moveRightButton.Name = "moveRightButton";
			moveRightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			moveRightButton.TabIndex = 0;
			moveRightButton.UseVisualStyleBackColor = true;
			moveRightButton.Click += new System.EventHandler(this.movePackRightButton_Click);
			// 
			// moveLeftButton
			// 
			moveLeftButton.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("SplitGrid|44791c7f-3427-42d9-902d-c81d06790252", "<--");
			moveLeftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			moveLeftButton.Name = "moveLeftButton";
			moveLeftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			moveLeftButton.TabIndex = 1;
			moveLeftButton.UseVisualStyleBackColor = true;
			moveLeftButton.Click += new System.EventHandler(this.movePackLeftButton_Click);
			// 
			// gridLabel1
			// 
			gridLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			gridLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			gridLabel1.Name = "gridLabel1";
			gridLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 16, true);
			gridLabel1.TabIndex = 0;
			// 
			// gridLabel2
			// 
			gridLabel2.Dock = System.Windows.Forms.DockStyle.Top;
			gridLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			gridLabel2.Name = "gridLabel2";
			gridLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 16, true);
			gridLabel2.TabIndex = 1;
			// 
			// packMidSection
			// 
			this.packMidSection.Controls.Add(floatingMidPanel);
			this.packMidSection.Dock = System.Windows.Forms.DockStyle.Left;
			this.packMidSection.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.packMidSection.Name = "packMidSection";
			this.packMidSection.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 170, true);
			this.packMidSection.TabIndex = 0;
			// 
			// splitControl
			// 
			this.splitControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitControl.Name = "splitControl";
			// 
			// splitControl.Panel1
			// 
			this.splitControl.Panel1.Controls.Add(this.grid1);
			this.splitControl.Panel1.Controls.Add(gridLabel1);
			// 
			// splitControl.Panel2
			// 
			this.splitControl.Panel2.Controls.Add(this.grid2);
			this.splitControl.Panel2.Controls.Add(gridLabel2);
			this.splitControl.Panel2.Controls.Add(this.packMidSection);
			this.splitControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 170, true);
			this.splitControl.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(348);
			this.splitControl.TabIndex = 0;
			// 
			// grid1
			// 
			this.grid1.AllowCopyToNewRowMenuItem = false;
			this.grid1.AllowNavigation = false;
			this.grid1.CaptionVisible = false;
			this.grid1.GridId = "cfccd4ab-4ee3-42a8-8262-f546b3b4e80a";
			this.grid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid1.IsWholeRowSelectedOnClick = true;
			this.grid1.LayoutKey = "grid1";
			this.grid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
			this.grid1.Name = "grid1";
			this.grid1.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid1.ShouldSetErrorsOnTabPage = false;
			this.grid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 154, true);
			this.grid1.TabIndex = 1;
			this.grid1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.grid1_MouseDown);
			// 
			// grid2
			// 
			this.grid2.AllowCopyToNewRowMenuItem = false;
			this.grid2.AllowNavigation = false;
			this.grid2.CaptionVisible = false;
			this.grid2.GridId = "005cafb6-7495-4cf9-9b8e-6ffe2ff2fe65";
			this.grid2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid2.IsWholeRowSelectedOnClick = true;
			this.grid2.LayoutKey = "grid2";
			this.grid2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 16, true);
			this.grid2.Name = "grid2";
			this.grid2.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid2.ShouldSetErrorsOnTabPage = false;
			this.grid2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 154, true);
			this.grid2.TabIndex = 2;
			this.grid2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.grid2_MouseDown);
			// 
			// SplitGrid
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitControl);
			this.Name = "SplitGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			floatingMidPanel.ResumeLayout(false);
			this.packMidSection.ResumeLayout(false);
			this.splitControl.Panel1.ResumeLayout(false);
			this.splitControl.Panel2.ResumeLayout(false);
			this.splitControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid2)).EndInit();
			this.ResumeLayout(false);

		}

		private CargoWise.Windows.UI.KSplitContainer splitControl;
		protected Enterprise.ZArchitecture.ZGrid grid1;
		protected Enterprise.ZArchitecture.ZGrid grid2;
		private CargoWise.Windows.UI.KPanel packMidSection;
		protected Enterprise.ZArchitecture.ZLabel gridLabel1;
		protected Enterprise.ZArchitecture.ZLabel gridLabel2;
		private CargoWise.Windows.UI.KPanel floatingMidPanel;
		private Enterprise.ZArchitecture.GUI.ZButton moveRightButton;
		private Enterprise.ZArchitecture.GUI.ZButton moveLeftButton;
	}
}
