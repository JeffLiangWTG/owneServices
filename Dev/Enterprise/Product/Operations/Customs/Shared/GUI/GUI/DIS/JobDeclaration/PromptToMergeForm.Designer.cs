namespace Enterprise.Customs.GUI
{
	partial class PromptToMergeForm
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
		protected override void InitializeComponent()
		{
			this.NoMergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MergeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 24, true);
			// 
			// NoMergeButton
			// 
			this.NoMergeButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("21487170-b3b6-40b5-a996-63c4de08149f", "Proceed without Merging");
			this.NoMergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 74, true);
			this.NoMergeButton.Name = "NoMergeButton";
			this.NoMergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 23, true);
			this.NoMergeButton.TabIndex = 2;
			this.NoMergeButton.UseVisualStyleBackColor = true;
			this.NoMergeButton.Click += new System.EventHandler(this.NoMergeButton_Click);
			// 
			// MergeButton
			// 
			this.MergeButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("018edce9-f080-45a6-9d8f-fce3949699e2", "Merge (Generate Entries)");
			this.MergeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 74, true);
			this.MergeButton.Name = "MergeButton";
			this.MergeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 23, true);
			this.MergeButton.TabIndex = 1;
			this.MergeButton.UseVisualStyleBackColor = true;
			this.MergeButton.Click += new System.EventHandler(this.MergeButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("53a3a44a-bede-4ad7-a6ce-e1035e1a8646", "Cancel");
			this.CancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 74, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAndCloseButton.TabIndex = 3;
			this.CancelAndCloseButton.UseVisualStyleBackColor = true;
			this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelAndCloseButton_Click);
			// 
			// DescriptionLabel
			// 
			this.DescriptionLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("81219ab6-f0c2-4162-a792-3a596971bb42", "DIS messages make a reference to an entry or entry summary record, but this declaration is not merged and has neither of records.\r\nDo you want system to merge and generate entries now?");
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 52, true);
			this.DescriptionLabel.TabIndex = 0;
			// 
			// PromptToMergeForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAndCloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c889b802-4fa5-4765-9a9c-da3822b81ec0", "Prompt to Merge");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 132, true);
			this.Controls.Add(this.DescriptionLabel);
			this.Controls.Add(this.MergeButton);
			this.Controls.Add(this.CancelAndCloseButton);
			this.Controls.Add(this.NoMergeButton);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 171, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(488, 171, true);
			this.Name = "PromptToMergeForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Prompt to Merge";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NoMergeButton, 0);
			this.Controls.SetChildIndex(this.CancelAndCloseButton, 0);
			this.Controls.SetChildIndex(this.MergeButton, 0);
			this.Controls.SetChildIndex(this.DescriptionLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton NoMergeButton;
		private ZArchitecture.GUI.ZButton MergeButton;
		private ZArchitecture.GUI.ZButton CancelAndCloseButton;
		internal ZArchitecture.ZLabel DescriptionLabel;
	}
}