
namespace Enterprise.Rating.GUI.AutoRating
{
	partial class AutoRatingExplorerForm
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
		private void InitializeComponent()
		{
			this.SplitPanel = new CargoWise.Windows.UI.KSplitContainer();
			this.PropertiesTreeView = new CargoWise.Windows.UI.KTreeView();
			this.DescriptionBox = new CargoWise.Windows.UI.KTextBox();
			this.BottomPanel = new CargoWise.Windows.UI.KPanel();
			this.RecipientEmailTextBox = new CargoWise.Windows.UI.KTextBox();
			this.SendButton = new CargoWise.Windows.UI.KButton();
			this.OpenButton = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.SplitPanel)).BeginInit();
			this.SplitPanel.Panel1.SuspendLayout();
			this.SplitPanel.Panel2.SuspendLayout();
			this.SplitPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// SplitPanel
			// 
			this.SplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitPanel.Name = "SplitPanel";
			// 
			// SplitPanel.Panel1
			// 
			this.SplitPanel.Panel1.Controls.Add(this.PropertiesTreeView);
			// 
			// SplitPanel.Panel2
			// 
			this.SplitPanel.Panel2.Controls.Add(this.DescriptionBox);
			this.SplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 378, true);
			this.SplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(227);
			this.SplitPanel.TabIndex = 0;
			// 
			// PropertiesTreeView
			// 
			this.PropertiesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PropertiesTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PropertiesTreeView.Name = "PropertiesTreeView";
			this.PropertiesTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 378, true);
			this.PropertiesTreeView.TabIndex = 0;
			this.PropertiesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.PropertiesTreeView_AfterSelect);
			// 
			// DescriptionBox
			// 
			this.DescriptionBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DescriptionBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DescriptionBox.Multiline = true;
			this.DescriptionBox.Name = "DescriptionBox";
			this.DescriptionBox.ReadOnly = true;
			this.DescriptionBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 378, true);
			this.DescriptionBox.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.RecipientEmailTextBox);
			this.BottomPanel.Controls.Add(this.SendButton);
			this.BottomPanel.Controls.Add(this.OpenButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 28, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// RecipientEmailTextBox
			// 
			this.RecipientEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(55, 5, true);
			this.RecipientEmailTextBox.Name = "RecipientEmailTextBox";
			this.RecipientEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(226, 20, true);
			this.RecipientEmailTextBox.TabIndex = 4;
			this.RecipientEmailTextBox.TextChanged += new System.EventHandler(this.emailTextBox_TextChanged);
			// 
			// SendButton
			// 
			this.SendButton.Enabled = false;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// OpenButton
			// 
			this.OpenButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(636, 2, true);
			this.OpenButton.Name = "OpenButton";
			this.OpenButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 23, true);
			this.OpenButton.TabIndex = 5;
			this.OpenButton.Text = "Open";
			this.OpenButton.UseVisualStyleBackColor = true;
			this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
			// 
			// AutoRatingExplorerForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 305, true);
			this.Controls.Add(this.SplitPanel);
			this.Controls.Add(this.BottomPanel);
			this.Name = "AutoRatingExplorerForm";
			this.Text = "Autorating Explorer";
			this.SplitPanel.Panel1.ResumeLayout(false);
			this.SplitPanel.Panel2.ResumeLayout(false);
			this.SplitPanel.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitPanel)).EndInit();
			this.SplitPanel.ResumeLayout(false);
			this.SplitPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitPanel;
		private CargoWise.Windows.UI.KPanel BottomPanel;
		private CargoWise.Windows.UI.KTreeView PropertiesTreeView;
		private CargoWise.Windows.UI.KTextBox DescriptionBox;
		private CargoWise.Windows.UI.KButton SendButton;
		private CargoWise.Windows.UI.KTextBox RecipientEmailTextBox;
		private CargoWise.Windows.UI.KButton OpenButton;
	}
}