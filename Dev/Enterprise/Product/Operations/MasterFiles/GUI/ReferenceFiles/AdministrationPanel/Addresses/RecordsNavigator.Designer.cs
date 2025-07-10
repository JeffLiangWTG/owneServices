namespace Enterprise.MasterFiles.GUI
{
	partial class RecordsNavigator
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecordsNavigator));
			this.CurrentNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TotalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OfLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MoveToFirstButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveToLastButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveRightButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MoveLeftButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// CurrentNumberTextBox
			// 
			this.CurrentNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 0, true);
			this.CurrentNumberTextBox.Name = "CurrentNumberTextBox";
			this.CurrentNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.CurrentNumberTextBox.TabIndex = 4;
			this.CurrentNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TotalNumberTextBox
			// 
			this.TotalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 0, true);
			this.TotalNumberTextBox.Name = "TotalNumberTextBox";
			this.TotalNumberTextBox.ReadOnly = true;
			this.TotalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 20, true);
			this.TotalNumberTextBox.TabIndex = 9;
			this.TotalNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OfLabel
			// 
			this.OfLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("34e966ce-a9a7-4821-9f76-eb7a6374868d", "of");
			this.OfLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OfLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 0, true);
			this.OfLabel.Name = "OfLabel";
			this.OfLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.OfLabel.TabIndex = 11;
			// 
			// MoveToFirstButton
			// 
			this.MoveToFirstButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MoveToFirstButton.BackgroundImage")));
			this.MoveToFirstButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.MoveToFirstButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, -1, true);
			this.MoveToFirstButton.Name = "MoveToFirstButton";
			this.MoveToFirstButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveToFirstButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.MoveToFirstButton.TabIndex = 13;
			this.MoveToFirstButton.Text = " ";
			this.MoveToFirstButton.ToolTipCaption = null;
			this.MoveToFirstButton.UseVisualStyleBackColor = true;
			this.MoveToFirstButton.Click += new System.EventHandler(this.MoveToFirstButton_Click);
			// 
			// MoveToLastButton
			// 
			this.MoveToLastButton.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MoveToLastButton.BackgroundImage")));
			this.MoveToLastButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.MoveToLastButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(229, 0, true);
			this.MoveToLastButton.Name = "MoveToLastButton";
			this.MoveToLastButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveToLastButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.MoveToLastButton.TabIndex = 12;
			this.MoveToLastButton.Text = " ";
			this.MoveToLastButton.ToolTipCaption = null;
			this.MoveToLastButton.UseVisualStyleBackColor = true;
			this.MoveToLastButton.Click += new System.EventHandler(this.MoveToLastButton_Click);
			// 
			// MoveRightButton
			// 
			this.MoveRightButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.next;
			this.MoveRightButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.MoveRightButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 0, true);
			this.MoveRightButton.Name = "MoveRightButton";
			this.MoveRightButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveRightButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.MoveRightButton.TabIndex = 10;
			this.MoveRightButton.Text = " ";
			this.MoveRightButton.ToolTipCaption = null;
			this.MoveRightButton.UseVisualStyleBackColor = false;
			this.MoveRightButton.Click += new System.EventHandler(this.MoveRightButton_Click);
			// 
			// MoveLeftButton
			// 
			this.MoveLeftButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.previous;
			this.MoveLeftButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
			this.MoveLeftButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 0, true);
			this.MoveLeftButton.Name = "MoveLeftButton";
			this.MoveLeftButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.MoveLeftButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(34, 20, true);
			this.MoveLeftButton.TabIndex = 8;
			this.MoveLeftButton.Text = " ";
			this.MoveLeftButton.ToolTipCaption = null;
			this.MoveLeftButton.UseVisualStyleBackColor = false;
			this.MoveLeftButton.Click += new System.EventHandler(this.MoveLeftButton_Click);
			// 
			// RecordsNavigator
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoveToFirstButton);
			this.Controls.Add(this.MoveToLastButton);
			this.Controls.Add(this.OfLabel);
			this.Controls.Add(this.MoveRightButton);
			this.Controls.Add(this.TotalNumberTextBox);
			this.Controls.Add(this.MoveLeftButton);
			this.Controls.Add(this.CurrentNumberTextBox);
			this.Name = "RecordsNavigator";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZTextBox CurrentNumberTextBox;
		protected ZArchitecture.GUI.ZButton MoveLeftButton;
		protected ZArchitecture.GUI.ZButton MoveRightButton;
		protected ZArchitecture.ZTextBox TotalNumberTextBox;
		private ZArchitecture.ZLabel OfLabel;
		protected ZArchitecture.GUI.ZButton MoveToLastButton;
		protected ZArchitecture.GUI.ZButton MoveToFirstButton;
	}
}
