using System.Drawing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbAccreditationGroupTreeControlBase
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
			this.descriptionColumn = new Aga.Controls.Tree.TreeColumn();
			this.descriptionTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.commenceDateColumn = new Aga.Controls.Tree.TreeColumn();
			this.commenceDateTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.completionDateColumn = new Aga.Controls.Tree.TreeColumn();
			this.completionDateTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.scoreColumn = new Aga.Controls.Tree.TreeColumn();
			this.scoreTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.progressColumn = new Aga.Controls.Tree.TreeColumn();
			this.progressTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.competencyRulesColumn = new Aga.Controls.Tree.TreeColumn();
			this.competencyRulesTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.applicantEmailColumn = new Aga.Controls.Tree.TreeColumn();
			this.applicantEmailTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.commentColumn = new Aga.Controls.Tree.TreeColumn();
			this.commentTextBox = new Aga.Controls.Tree.NodeControls.NodeTextBox();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// Tree
			// 
			this.Tree.Columns.Add(this.descriptionColumn);
			this.Tree.Columns.Add(this.commenceDateColumn);
			this.Tree.Columns.Add(this.completionDateColumn);
			this.Tree.Columns.Add(this.scoreColumn);
			this.Tree.Columns.Add(this.progressColumn);
			this.Tree.Columns.Add(this.competencyRulesColumn);
			this.Tree.Columns.Add(this.applicantEmailColumn);
			this.Tree.Columns.Add(this.commentColumn);
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Tree.NodeControls.Add(this.descriptionTextBox);
			this.Tree.NodeControls.Add(this.commenceDateTextBox);
			this.Tree.NodeControls.Add(this.completionDateTextBox);
			this.Tree.NodeControls.Add(this.scoreTextBox);
			this.Tree.NodeControls.Add(this.progressTextBox);
			this.Tree.NodeControls.Add(this.competencyRulesTextBox);
			this.Tree.NodeControls.Add(this.applicantEmailTextBox);
			this.Tree.NodeControls.Add(this.commentTextBox);
			this.Tree.AllowColumnReorder = false;
			// 
			// descriptionColumn
			// 
			this.descriptionColumn.Header = "Description";
			this.descriptionColumn.MinColumnWidth = 10;
			this.descriptionColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.descriptionColumn.TooltipText = null;
			this.descriptionColumn.Width = 350;
			// 
			// descriptionTextBox
			// 
			this.descriptionTextBox.DataPropertyName = "Description";
			this.descriptionTextBox.IncrementalSearchEnabled = true;
			this.descriptionTextBox.LeftMargin = 3;
			this.descriptionTextBox.ParentColumn = this.descriptionColumn;
			this.descriptionTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// commenceDateColumn
			// 
			this.commenceDateColumn.Header = "Commence Date";
			this.commenceDateColumn.MinColumnWidth = 10;
			this.commenceDateColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.commenceDateColumn.TooltipText = null;
			this.commenceDateColumn.Width = 150;
			// 
			// commenceDateTextBox
			// 
			this.commenceDateTextBox.DataPropertyName = "CommenceDate";
			this.commenceDateTextBox.IncrementalSearchEnabled = true;
			this.commenceDateTextBox.LeftMargin = 3;
			this.commenceDateTextBox.ParentColumn = this.commenceDateColumn;
			this.commenceDateTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// completionDateColumn
			// 
			this.completionDateColumn.Header = "Completion Date";
			this.completionDateColumn.MinColumnWidth = 10;
			this.completionDateColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.completionDateColumn.TooltipText = null;
			this.completionDateColumn.Width = 150;
			// 
			// completionDateTextBox
			// 
			this.completionDateTextBox.DataPropertyName = "CompletionDate";
			this.completionDateTextBox.IncrementalSearchEnabled = true;
			this.completionDateTextBox.LeftMargin = 3;
			this.completionDateTextBox.ParentColumn = this.completionDateColumn;
			this.completionDateTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// scoreColumn
			// 
			this.scoreColumn.Header = "Score";
			this.scoreColumn.MinColumnWidth = 10;
			this.scoreColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.scoreColumn.TooltipText = null;
			this.scoreColumn.Width = 80;
			// 
			// scoreTextBox
			// 
			this.scoreTextBox.DataPropertyName = "Score";
			this.scoreTextBox.IncrementalSearchEnabled = true;
			this.scoreTextBox.LeftMargin = 3;
			this.scoreTextBox.ParentColumn = this.scoreColumn;
			this.scoreTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// progressColumn
			// 
			this.progressColumn.Header = "Progress";
			this.progressColumn.MinColumnWidth = 10;
			this.progressColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.progressColumn.TooltipText = null;
			this.progressColumn.Width = 80;
			// 
			// progressTextBox
			// 
			this.progressTextBox.DataPropertyName = "Progress";
			this.progressTextBox.IncrementalSearchEnabled = true;
			this.progressTextBox.LeftMargin = 3;
			this.progressTextBox.ParentColumn = this.progressColumn;
			this.progressTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// competencyRulesColumn
			// 
			this.competencyRulesColumn.Header = "Competency Rules";
			this.competencyRulesColumn.MinColumnWidth = 10;
			this.competencyRulesColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.competencyRulesColumn.TooltipText = null;
			this.competencyRulesColumn.Width = 170;
			// 
			// competencyRulesTextBox
			// 
			this.competencyRulesTextBox.DataPropertyName = "CompetencyRules";
			this.competencyRulesTextBox.IncrementalSearchEnabled = true;
			this.competencyRulesTextBox.LeftMargin = 3;
			this.competencyRulesTextBox.ParentColumn = this.competencyRulesColumn;
			this.competencyRulesTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// competencyRulesColumn
			// 
			this.applicantEmailColumn.Header = "Applicant Email";
			this.applicantEmailColumn.MinColumnWidth = 10;
			this.applicantEmailColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.applicantEmailColumn.TooltipText = null;
			this.applicantEmailColumn.Width = 230;
			// 
			// competencyRulesTextBox
			// 
			this.applicantEmailTextBox.DataPropertyName = "ApplicantEmail";
			this.applicantEmailTextBox.IncrementalSearchEnabled = true;
			this.applicantEmailTextBox.LeftMargin = 3;
			this.applicantEmailTextBox.ParentColumn = this.applicantEmailColumn;
			this.applicantEmailTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// competencyRulesColumn
			// 
			this.commentColumn.Header = "Comment";
			this.commentColumn.MinColumnWidth = 10;
			this.commentColumn.SortOrder = System.Windows.Forms.SortOrder.None;
			this.commentColumn.TooltipText = null;
			this.commentColumn.Width = 230;
			// 
			// competencyRulesTextBox
			// 
			this.commentTextBox.DataPropertyName = "Comment";
			this.commentTextBox.IncrementalSearchEnabled = true;
			this.commentTextBox.LeftMargin = 3;
			this.commentTextBox.ParentColumn = this.commentColumn;
			this.commentTextBox.Trimming = System.Drawing.StringTrimming.EllipsisCharacter;
			// 
			// GlbAccreditationGroupTreeControlBase
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "GlbAccreditationGroupTreeControlBase";
			this.NameOfATreeElement = Res.GetData("B7679602-5484-40EA-A468-C0174AF75584", "item");
			this.Tree.ElementType = typeof(GlbAccreditationTreeBizObjWrapperBase);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 400, true);
			this.Tree.ResumeLayout(false);
			this.Tree.PerformLayout();
			this.mainPanel.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
			this.mainPanel.ResumeLayout(false);
			this.mainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Aga.Controls.Tree.TreeColumn descriptionColumn;
		private Aga.Controls.Tree.TreeColumn commenceDateColumn;
		private Aga.Controls.Tree.TreeColumn completionDateColumn;
		private Aga.Controls.Tree.TreeColumn scoreColumn;
		private Aga.Controls.Tree.TreeColumn progressColumn;
		private Aga.Controls.Tree.TreeColumn competencyRulesColumn;
		private Aga.Controls.Tree.TreeColumn applicantEmailColumn;
		private Aga.Controls.Tree.TreeColumn commentColumn;

		private Aga.Controls.Tree.NodeControls.NodeTextBox descriptionTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox commenceDateTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox completionDateTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox scoreTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox progressTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox competencyRulesTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox applicantEmailTextBox;
		private Aga.Controls.Tree.NodeControls.NodeTextBox commentTextBox;
	}
}
