namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class MultiSelectModuleForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.SelectedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ButtonsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AddToSelectionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemoveFromSelectionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterControlPanel.SuspendLayout();
			this.ButtonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ButtonsSplitContainer)).BeginInit();
			this.ButtonsSplitContainer.Panel1.SuspendLayout();
			this.ButtonsSplitContainer.Panel2.SuspendLayout();
			this.ButtonsSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Controls.Add(this.SplitContainer);
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 241, true);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 273, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.SplitContainer.Panel1MinSize = 150;
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.Controls.Add(this.SelectedGroupBox);
			this.SplitContainer.Panel2.Controls.Add(this.ButtonsSplitContainer);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 241, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.SplitContainer.TabIndex = 0;
			// 
			// SelectedGroupBox
			// 
			this.SelectedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
			this.SelectedGroupBox.Name = "SelectedGroupBox";
			this.SelectedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 62, true);
			this.SelectedGroupBox.TabIndex = 3;
			this.SelectedGroupBox.TabStop = false;
			// 
			// ButtonsSplitContainer
			// 
			this.ButtonsSplitContainer.Dock = System.Windows.Forms.DockStyle.Top;
			this.ButtonsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ButtonsSplitContainer.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 25, true);
			this.ButtonsSplitContainer.Name = "ButtonsSplitContainer";
			// 
			// ButtonsSplitContainer.Panel1
			// 
			this.ButtonsSplitContainer.Panel1.Controls.Add(this.AddToSelectionButton);
			// 
			// ButtonsSplitContainer.Panel2
			// 
			this.ButtonsSplitContainer.Panel2.Controls.Add(this.RemoveFromSelectionButton);
			this.ButtonsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 25, true);
			this.ButtonsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ButtonsSplitContainer.TabIndex = 2;
			// 
			// AddToSelectionButton
			// 
			this.AddToSelectionButton.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("bfcbd77e-3e13-4163-9cbe-d6c48987ace8", "Add To Selection");
			this.AddToSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddToSelectionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddToSelectionButton.Name = "AddToSelectionButton";
			this.AddToSelectionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 25, true);
			this.AddToSelectionButton.TabIndex = 0;
			this.AddToSelectionButton.UseVisualStyleBackColor = true;
			this.AddToSelectionButton.Click += new System.EventHandler(this.AddToSelectionButton_Click);
			// 
			// RemoveFromSelectionButton
			// 
			this.RemoveFromSelectionButton.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("60758ead-c6e0-4554-85f7-a27b77ebe27e", "Remove From Selection");
			this.RemoveFromSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RemoveFromSelectionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RemoveFromSelectionButton.Name = "RemoveFromSelectionButton";
			this.RemoveFromSelectionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 25, true);
			this.RemoveFromSelectionButton.TabIndex = 0;
			this.RemoveFromSelectionButton.UseVisualStyleBackColor = true;
			this.RemoveFromSelectionButton.Click += new System.EventHandler(this.RemoveFromSelectionButton_Click);
			// 
			// MultiSelectModuleForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 332, true);
			this.Name = "MultiSelectModuleForm";
			this.FilterControlPanel.ResumeLayout(false);
			this.FilterControlPanel.PerformLayout();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.ButtonsSplitContainer.Panel1.ResumeLayout(false);
			this.ButtonsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ButtonsSplitContainer)).EndInit();
			this.ButtonsSplitContainer.ResumeLayout(false);
			this.ButtonsSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer SplitContainer;
		private ZArchitecture.GUI.ZGroupBox SelectedGroupBox;
		private CargoWise.Windows.UI.KSplitContainer ButtonsSplitContainer;
		private ZArchitecture.GUI.ZButton AddToSelectionButton;
		private ZArchitecture.GUI.ZButton RemoveFromSelectionButton;
	}
}