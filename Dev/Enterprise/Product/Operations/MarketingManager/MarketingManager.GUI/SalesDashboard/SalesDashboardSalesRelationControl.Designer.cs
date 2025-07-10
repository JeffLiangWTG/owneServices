namespace Enterprise.MarketingManager.GUI
{
	partial class SalesDashboardSalesRelationControl
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
			this.activityNoteRichTextBox = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			this.Tree.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).BeginInit();
			this.mainPanel.Panel1.SuspendLayout();
			this.mainPanel.Panel2.SuspendLayout();
			this.mainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.activityNoteRichTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// ShowCommunicationsCheckBox
			// 
			this.ShowCommunicationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 3, true);
			this.ShowCommunicationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 16, true);
			this.ShowCommunicationsCheckBox.Visible = false;
			// 
			// Tree
			// 
			this.Tree.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.Tree.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 365, true);
			// 
			// mainPanel.Panel2
			// 
			this.mainPanel.Panel2.Controls.Add(this.activityNoteRichTextBox);
			this.mainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 368, true);
			// 
			// activityNoteRichTextBox
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.activityNoteRichTextBox, false);
			this.activityNoteRichTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.activityNoteRichTextBox.IsPopupButtonVisible = false;
			this.activityNoteRichTextBox.IsToolBarVisible = false;
			this.activityNoteRichTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.activityNoteRichTextBox.MaxLength = 10000000;
			this.activityNoteRichTextBox.Name = "activityNoteRichTextBox";
			this.activityNoteRichTextBox.ReadOnly = true;
			this.activityNoteRichTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 31, true);
			this.activityNoteRichTextBox.TabIndex = 9;
			// 
			// SalesDashboardSalesRelationControl
			// 
			this.Name = "SalesDashboardSalesRelationControl";
			this.Tree.ResumeLayout(false);
			this.Tree.PerformLayout();
			this.mainPanel.Panel1.ResumeLayout(false);
			this.mainPanel.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainPanel)).EndInit();
			this.mainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.activityNoteRichTextBox.ResumeLayout(true);
			this.activityNoteRichTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZRichTextBox activityNoteRichTextBox;
	}
}
