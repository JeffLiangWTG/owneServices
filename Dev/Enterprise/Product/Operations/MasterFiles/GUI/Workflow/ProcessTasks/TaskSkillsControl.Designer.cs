namespace Enterprise.MasterFiles.GUI
{
	partial class TaskSkillsControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TaskSkillsControl));
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LeftButtonsToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
			this.OpenLearningCentreButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.CreateLearningTaskButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
			this.JobSkillsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobSkillsGrid)).BeginInit();
			this.JobSkillsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.LeftButtonsToolStrip);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 30, true);
			this.BottomPanel.TabIndex = 4;
			// 
			// LeftButtonsToolStrip
			// 
			this.LeftButtonsToolStrip.BackColor = System.Drawing.Color.Transparent;
			this.LeftButtonsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this.LeftButtonsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.LeftButtonsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this.OpenLearningCentreButton,
			this.CreateLearningTaskButton});
			this.LeftButtonsToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.LeftButtonsToolStrip.Name = "LeftButtonsToolStrip";
			this.LeftButtonsToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 25, true);
			this.LeftButtonsToolStrip.TabIndex = 7;
			this.LeftButtonsToolStrip.Text = "zToolStrip1";
			// 
			// OpenLearningCentreButton
			// 
			this.OpenLearningCentreButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5b6a0599-131b-41c4-8aca-72cd42d8a64f", "Open Internal Learning Center");
			this.OpenLearningCentreButton.Image = ((System.Drawing.Image)(resources.GetObject("OpenLearningCentreButton.Image")));
			this.OpenLearningCentreButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.OpenLearningCentreButton.Name = "OpenLearningCentreButton";
			this.OpenLearningCentreButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(186, 22, true);
			this.OpenLearningCentreButton.Click += new System.EventHandler(this.OpenLearningCentreButton_Click);
			// 
			// CreateLearningTaskButton
			// 
			this.CreateLearningTaskButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0C278B7E-91BC-46F3-BE76-A107906A6CF0", "New Learning Task");
			this.CreateLearningTaskButton.Image = ((System.Drawing.Image)(resources.GetObject("CreateLearningTaskButton.Image")));
			this.CreateLearningTaskButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CreateLearningTaskButton.Name = "CreateLearningTaskButton";
			this.CreateLearningTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 22, true);
			this.CreateLearningTaskButton.Click += new System.EventHandler(this.CreateLearningTaskButton_Click);
			// 
			// JobSkillsGrid
			// 
			this.JobSkillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobSkillsGrid, "SkillsPivots");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SkillsPivots)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskRequiredSkill)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SkillsPivots)).SyncRoot)).P9S_WiseTechAcademySubjectCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ProcessTaskRequiredSkill)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SkillsPivots)).SyncRoot)).HasCompletedLearningUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTaskRequiredSkill)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).SkillsPivots)).SyncRoot)).LearningUnitName)));
			this.JobSkillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "P9S_WiseTechAcademySubjectCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "HasCompletedLearningUnit";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo2.ColumnName = "LearningUnitName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.JobSkillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobSkillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.JobSkillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.JobSkillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobSkillsGrid.GridId = "29ba5656-ba13-4ebd-8759-9b40875b0fa4";
			this.JobSkillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobSkillsGrid.IsWholeRowSelectedOnClick = true;
			this.JobSkillsGrid.LayoutKey = "zGrid1";
			this.JobSkillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobSkillsGrid.Name = "JobSkillsGrid";
			this.JobSkillsGrid.ReadOnly = true;
			this.JobSkillsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.JobSkillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 140, true);
			this.JobSkillsGrid.TabIndex = 5;
			this.JobSkillsGrid.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.JobSkillsGrid_MouseDoubleClick);
			// 
			// TaskSkillsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.JobSkillsGrid);
			this.Controls.Add(this.BottomPanel);
			this.Name = "TaskSkillsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobSkillsGrid)).EndInit();
			this.JobSkillsGrid.ResumeLayout(false);
			this.JobSkillsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid JobSkillsGrid;
		internal Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZToolStrip LeftButtonsToolStrip;
		private ZArchitecture.GUI.ZToolStripButton OpenLearningCentreButton;
		private ZArchitecture.GUI.ZToolStripButton CreateLearningTaskButton;
	}
}
