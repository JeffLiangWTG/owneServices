
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PackLineConfirmDiscrepanciesDialog
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
		private new void InitializeComponent()
		{
			this.CaptionRenderingEnabled = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.AcceptDiscrepancyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UpdatePacklineButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Label1 = new Enterprise.ZArchitecture.ZLabel();
			this.Label2 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AcceptDiscrepancyButton
			// 
			this.AcceptDiscrepancyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 103, true);
			this.AcceptDiscrepancyButton.Name = "AcceptDiscrepancyButton";
			this.AcceptDiscrepancyButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AcceptDiscrepancyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 24, true);
			this.AcceptDiscrepancyButton.TabIndex = 0;
			this.AcceptDiscrepancyButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackLineConfirmDiscrepanciesDialog|239e13d5-9fd1-02b4-482c-bb3abc4a18fc", "Accept Discrepancy and Confirm");
			this.AcceptDiscrepancyButton.ToolTipCaption = null;
			this.AcceptDiscrepancyButton.UseVisualStyleBackColor = true;
			this.AcceptDiscrepancyButton.Click += new System.EventHandler(this.AcceptDiscrepancyButton_Click);
			// 
			// UpdatePacklineButton
			// 
			this.UpdatePacklineButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 104, true);
			this.UpdatePacklineButton.Name = "UpdatePacklineButton";
			this.UpdatePacklineButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UpdatePacklineButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 23, true);
			this.UpdatePacklineButton.TabIndex = 1;
			this.UpdatePacklineButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackLineConfirmDiscrepanciesDialog|5eab6892-c329-47bf-4bd0-ee1ee151b42b", "Update Pack line and Confirm");
			this.UpdatePacklineButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UpdatePacklineButton.ToolTipCaption = null;
			this.UpdatePacklineButton.UseVisualStyleBackColor = true;
			this.UpdatePacklineButton.Click += new System.EventHandler(this.UpdatePacklineButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(405, 104, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 2;
			this.CancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PackLineConfirmDiscrepanciesDialog|3dfa4751-7eef-9ca1-4875-2337608b7250", "Cancel");
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 26, true);
			this.Label1.Name = "label1";
			this.Label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(489, 13, true);
			this.Label1.TabIndex = 3;
			this.Label1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData(
				"PackLineConfirmDiscrepanciesDialog|60f1ee8d-14e1-6f8a-4b65-b0b6cf7005bc",
				"\"Accept Discrepancy and Confirm\" will change status to CNF and keep warnings on the discrepancies"
			);
			// 
			// Label2
			// 
			this.Label2.AutoSize = true;
			this.Label2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 52, true);
			this.Label2.Name = "label2";
			this.Label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(498, 13, true);
			this.Label2.TabIndex = 4;
			this.Label2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData(
				"PackLineConfirmDiscrepanciesDialog|9a9194f7-6823-e19b-4281-1facde654e23",
				"\"Update Pack line and Confirm\" will split pack line by common attributes and set status on all splits to CNF"
			);
			// 
			// PackLineConfirmDiscrepanciesDialog
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 144, true);
			this.Controls.Add(this.Label2);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.UpdatePacklineButton);
			this.Controls.Add(this.AcceptDiscrepancyButton);
			this.Name = "PackLineConfirmDiscrepanciesDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = Res.GetString("76aecafe-d3c4-1287-4c94-0214e75caa97", "Pack line Discrepancy Dialog");
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton AcceptDiscrepancyButton;
		private ZArchitecture.GUI.ZButton UpdatePacklineButton;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private Enterprise.ZArchitecture.ZLabel Label1;
		private Enterprise.ZArchitecture.ZLabel Label2;
	}
}
