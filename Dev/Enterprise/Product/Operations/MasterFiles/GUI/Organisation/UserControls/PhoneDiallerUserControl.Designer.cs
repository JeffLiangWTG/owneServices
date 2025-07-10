namespace Enterprise.MasterFiles.GUI
{
	partial class PhoneDiallerUserControl
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
			this.components = new System.ComponentModel.Container();
			this.DropButton = new Enterprise.MasterFiles.GUI.DropDownImageButton();
			this.CallButton = new Enterprise.ZArchitecture.GUI.ZImageButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.Types.ZString);
			// 
			// DropButton
			// 
			this.DropButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DropButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Dropdown_Normal;
			this.DropButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.DropButton.DisplayFocusCues = false;
			this.DropButton.DownBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Dropdown_Down;
			this.DropButton.HotBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Dropdown_Active;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DropButton, false);
			this.DropButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 0, true);
			this.DropButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 1, 2, 1, true);
			this.DropButton.Name = "DropButton";
			this.DropButton.NormalBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Dropdown_Normal;
			this.DropButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(17, 17, true);
			this.DropButton.TabIndex = 3;
			this.DropButton.TabStop = false;
			this.DropButton.UseVisualStyleBackColor = true;
			// 
			// CallButton
			// 
			this.CallButton.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CallButton.BackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Short_Normal;
			this.CallButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CallButton.DisplayFocusCues = false;
			this.CallButton.DownBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Short_Down;
			this.CallButton.HotBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Short_Active;
			this.CallButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CallButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CallButton.Name = "CallButton";
			this.CallButton.NormalBackgroundImage = global::Enterprise.MasterFiles.GUI.Properties.Resources.PhoneDialler_Short_Normal;
			this.CallButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(18, 18, true);
			this.CallButton.TabIndex = 2;
			this.CallButton.TabStop = false;
			this.CallButton.UseVisualStyleBackColor = true;
			this.CallButton.Click += new System.EventHandler(this.CallButton_Click);
			// 
			// PhoneDiallerUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DropButton);
			this.Controls.Add(this.CallButton);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			this.Name = "PhoneDiallerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(36, 18, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public ZArchitecture.GUI.ZImageButton CallButton;
		protected Enterprise.MasterFiles.GUI.DropDownImageButton DropButton;
	}
}
