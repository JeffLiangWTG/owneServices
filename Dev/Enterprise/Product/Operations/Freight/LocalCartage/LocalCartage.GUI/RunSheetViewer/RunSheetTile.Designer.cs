namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class RunSheetTile
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
			this.BodyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.StatusBarPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OpenRunSheetButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatusBarPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.CommonWorkSheet);
			// 
			// BodyPanel
			// 
			this.BodyPanel.AutoScroll = true;
			this.BodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.BodyPanel.Name = "BodyPanel";
			this.BodyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 154, true);
			this.BodyPanel.TabIndex = 1;
			// 
			// StatusBarPanel
			// 
			this.StatusBarPanel.BackColor = System.Drawing.SystemColors.Control;
			this.StatusBarPanel.Controls.Add(this.StatusLabel);
			this.StatusBarPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.StatusBarPanel.ForeColor = System.Drawing.Color.Black;
			this.StatusBarPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 176, true);
			this.StatusBarPanel.Name = "StatusBarPanel";
			this.StatusBarPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 16, true);
			this.StatusBarPanel.TabIndex = 0;
			// 
			// StatusLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusLabel, "StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.CommonWorkSheet)(null)).StatusDescription)));
			this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusLabel, false);
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 16, true);
			this.StatusLabel.TabIndex = 0;
			// 
			// OpenRunSheetButton
			// 
			this.OpenRunSheetButton.ForeColor = System.Drawing.Color.Black;
			this.OpenRunSheetButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 0, true);
			this.OpenRunSheetButton.Name = "OpenRunSheetButton";
			this.OpenRunSheetButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 20, true);
			this.OpenRunSheetButton.TabIndex = 2;
			this.OpenRunSheetButton.Text = "…";
			this.OpenRunSheetButton.UseVisualStyleBackColor = true;
			this.OpenRunSheetButton.Click += new System.EventHandler(this.OpenRunSheetButton_Click);
			// 
			// RunSheetTile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.BorderColor = System.Drawing.Color.Fuchsia;
			this.Controls.Add(this.OpenRunSheetButton);
			this.Controls.Add(this.BodyPanel);
			this.Controls.Add(this.StatusBarPanel);
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "RunSheetTile";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 22, 11, 11, true);
			this.PanelBorderColor = System.Drawing.Color.Navy;
			this.PanelColor = System.Drawing.Color.White;
			this.PanelColorGradient = System.Drawing.Color.White;
			this.ShowTileShadow = true;
			this.ShowTitleBar = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 203, true);
			this.TileColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
			this.TileColorGradient = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
			this.TileGradient = Enterprise.Freight.LocalCartage.GUI.ZGroupBoxTile.TilesGradient.Vertical;
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatusBarPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZPanel BodyPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel StatusBarPanel;
		private Enterprise.ZArchitecture.ZLabel StatusLabel;
		private Enterprise.ZArchitecture.GUI.ZButton OpenRunSheetButton;

	}
}
