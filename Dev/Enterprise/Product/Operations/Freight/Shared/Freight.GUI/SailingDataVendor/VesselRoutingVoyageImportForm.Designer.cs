using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.SailingDataVendor.GUI
{
	partial class VesselRoutingVoyageImportForm
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

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ProgressTextBox = new CargoWise.Windows.UI.KRichTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ProgressBar = new CargoWise.Windows.UI.KProgressBar();
			this.ImportStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.KeepTickedPortPairsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 562, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 24, true);
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 41, true);
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 486, true);
			this.ProgressTextBox.TabIndex = 1;
			this.ProgressTextBox.Text = "";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyageImportForm|b41e1665-d3e4-42b7-92d4-755eb39f7410", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 533, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.OnClose_Click);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 23, true);
			this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.ProgressBar.TabIndex = 0;
			// 
			// ImportStatusLabel
			// 
			this.ImportStatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ImportStatusLabel.AutoSize = true;
			this.ImportStatusLabel.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyageImportForm|1e1d3915-0442-408f-861a-5745355d4f49", "Importing Sailing Schedules...");
			this.ImportStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 535, true);
			this.ImportStatusLabel.Name = "ImportStatusLabel";
			this.ImportStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 13, true);
			this.ImportStatusLabel.TabIndex = 2;
			// 
			// KeepTickedPortPairsCheckBox
			// 
			this.KeepTickedPortPairsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.KeepTickedPortPairsCheckBox.AutoSize = true;
			this.KeepTickedPortPairsCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyageImportForm|f83ded06-abd8-449c-80fa-2699907deebc", "Keep port pairs ticked");
			this.KeepTickedPortPairsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.KeepTickedPortPairsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 537, true);
			this.KeepTickedPortPairsCheckBox.Name = "KeepTickedPortPairsCheckBox";
			this.KeepTickedPortPairsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 17, true);
			this.KeepTickedPortPairsCheckBox.TabIndex = 3;
			this.KeepTickedPortPairsCheckBox.UseVisualStyleBackColor = true;
			// 
			// VesselRoutingVoyageImportForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 586, true);
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VesselRoutingVoyageImportForm|6b192dce-0482-486b-bea9-a960f73206d0", "Sailing Schedule Import");
			this.Controls.Add(this.KeepTickedPortPairsCheckBox);
			this.Controls.Add(this.ImportStatusLabel);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ProgressBar);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(580, 580, true);
			this.Name = "VesselRoutingVoyageImportForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.ImportStatusLabel, 0);
			this.Controls.SetChildIndex(this.KeepTickedPortPairsCheckBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected CargoWise.Windows.UI.KRichTextBox ProgressTextBox;
		protected Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		protected CargoWise.Windows.UI.KProgressBar ProgressBar;
		protected Enterprise.ZArchitecture.ZLabel ImportStatusLabel;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox KeepTickedPortPairsCheckBox;
	}
}
