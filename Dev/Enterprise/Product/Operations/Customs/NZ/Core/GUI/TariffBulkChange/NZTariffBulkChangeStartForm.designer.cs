
namespace Enterprise.Customs.NZ.GUI
{
	partial class NZTariffBulkChangeStartForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NZTariffBulkChangeStartForm));
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffChangeGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.TariffFinalUpdateZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InfoLabel = new CargoWise.Windows.UI.KLabel();
			this.label2 = new CargoWise.Windows.UI.KLabel();
			this.AutomaticConvertZCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TCOChangeDetails = new CargoWise.Windows.UI.KGroupBox();
			this.TCOUpdateUserFileZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffChangeGroupBox.SuspendLayout();
			this.TCOChangeDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 443, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 24, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.CloseButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("148490DD-44B5-4ECC-871A-A24358B0FF93", "&Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(924, 414, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TariffUpdateUserFileZButton
			// 
			this.TariffUpdateUserFileZButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("772D507F-260D-4DEF-AC02-A9E835201F85", "Perform Tariff Bulk Change Using Your/Other Party Concordance File");
			this.TariffUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(155, 117, true);
			this.TariffUpdateUserFileZButton.Name = "TariffUpdateUserFileZButton";
			this.TariffUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffUpdateUserFileZButton.TabIndex = 3;
			this.TariffUpdateUserFileZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffUpdateUserFileZButton.ToolTipCaption = null;
			this.TariffUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TariffUpdateUserFileZButton.Click += new System.EventHandler(this.TariffBulkChangeFile);
			// 
			// TariffChangeGroupBox
			// 
			this.TariffChangeGroupBox.Controls.Add(this.TariffFinalUpdateZButton);
			this.TariffChangeGroupBox.Controls.Add(this.InfoLabel);
			this.TariffChangeGroupBox.Controls.Add(this.label2);
			this.TariffChangeGroupBox.Controls.Add(this.AutomaticConvertZCheckBox);
			this.TariffChangeGroupBox.Controls.Add(this.TariffUpdateUserFileZButton);
			this.TariffChangeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 26, true);
			this.TariffChangeGroupBox.Name = "TariffChangeGroupBox";
			this.TariffChangeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 287, true);
			this.TariffChangeGroupBox.TabIndex = 40;
			this.TariffChangeGroupBox.TabStop = false;
			this.TariffChangeGroupBox.Text = "Tariff Change Details";
			// 
			// TariffFinalUpdateZButton
			// 
			this.TariffFinalUpdateZButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("B5905EB3-A1FF-4858-B372-1549BF1F974B", "Apply All Pending Changes to Data Base");
			this.TariffFinalUpdateZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(512, 117, true);
			this.TariffFinalUpdateZButton.Name = "TariffFinalUpdateZButton";
			this.TariffFinalUpdateZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TariffFinalUpdateZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TariffFinalUpdateZButton.TabIndex = 46;
			this.TariffFinalUpdateZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TariffFinalUpdateZButton.ToolTipCaption = null;
			this.TariffFinalUpdateZButton.UseVisualStyleBackColor = true;
			this.TariffFinalUpdateZButton.Click += new System.EventHandler(this.ApplyTariffChanges);
			// 
			// InfoLabel
			// 
			this.InfoLabel.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.InfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 169, true);
			this.InfoLabel.Name = "InfoLabel";
			this.InfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(939, 108, true);
			this.InfoLabel.TabIndex = 45;
			this.InfoLabel.Text = resources.GetString("InfoLabel.Text");
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 49, true);
			this.label2.Name = "label2";
			this.label2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(942, 66, true);
			this.label2.TabIndex = 44;
			this.label2.Text = resources.GetString("label2.Text");
			// 
			// AutomaticConvertZCheckBox
			// 
			this.AutomaticConvertZCheckBox.AutoSize = true;
			this.AutomaticConvertZCheckBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("73EC8EE9-BAA9-45EA-BB27-6946BB747F4E", "Automatically Convert One Tariff to One Other Tariff");
			this.AutomaticConvertZCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutomaticConvertZCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 20, true);
			this.AutomaticConvertZCheckBox.Name = "AutomaticConvertZCheckBox";
			this.AutomaticConvertZCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 16, true);
			this.AutomaticConvertZCheckBox.TabIndex = 1;
			this.AutomaticConvertZCheckBox.UseVisualStyleBackColor = true;
			// 
			// TCOChangeDetails
			// 
			this.TCOChangeDetails.Controls.Add(this.TCOUpdateUserFileZButton);
			this.TCOChangeDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 319, true);
			this.TCOChangeDetails.Name = "TCOChangeDetails";
			this.TCOChangeDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 76, true);
			this.TCOChangeDetails.TabIndex = 43;
			this.TCOChangeDetails.TabStop = false;
			this.TCOChangeDetails.Text = "Concession Change Details";
			// 
			// TCOUpdateUserFileZButton
			// 
			this.TCOUpdateUserFileZButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("7D1C1E3A-AA28-43FC-AE7C-7CE11687CBF2", "Perform Concession Bulk Change Using Your/Other Party Concordance File");
			this.TCOUpdateUserFileZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 20, true);
			this.TCOUpdateUserFileZButton.Name = "TCOUpdateUserFileZButton";
			this.TCOUpdateUserFileZButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.TCOUpdateUserFileZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 38, true);
			this.TCOUpdateUserFileZButton.TabIndex = 5;
			this.TCOUpdateUserFileZButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.TCOUpdateUserFileZButton.ToolTipCaption = null;
			this.TCOUpdateUserFileZButton.UseVisualStyleBackColor = true;
			this.TCOUpdateUserFileZButton.Click += new System.EventHandler(this.TCOUpdateUserFileZButton_Click);
			// 
			// NZTariffBulkChangeStartForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 467, true);
			this.Controls.Add(this.TCOChangeDetails);
			this.Controls.Add(this.TariffChangeGroupBox);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "NZTariffBulkChange";
			this.Name = "NZTariffBulkChangeStartForm";
			this.Text = "Tariff Bulk Change";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TariffChangeGroupBox, 0);
			this.Controls.SetChildIndex(this.TCOChangeDetails, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffChangeGroupBox.ResumeLayout(false);
			this.TariffChangeGroupBox.PerformLayout();
			this.TCOChangeDetails.ResumeLayout(false);
			this.TCOChangeDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton TariffUpdateUserFileZButton;
		private CargoWise.Windows.UI.KGroupBox TariffChangeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutomaticConvertZCheckBox;
		private CargoWise.Windows.UI.KLabel InfoLabel;
		private CargoWise.Windows.UI.KLabel label2;
		private Enterprise.ZArchitecture.GUI.ZButton TariffFinalUpdateZButton;
		private CargoWise.Windows.UI.KGroupBox TCOChangeDetails;
		internal Enterprise.ZArchitecture.GUI.ZButton TCOUpdateUserFileZButton;
	}
}

