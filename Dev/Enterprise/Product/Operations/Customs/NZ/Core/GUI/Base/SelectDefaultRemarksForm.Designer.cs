
namespace Enterprise.Customs.NZ.GUI.Base
{
	partial class SelectDefaultRemarksForm
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
			this.RemarksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RemarksListBox = new CargoWise.Windows.UI.KListBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 132, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// RemarksLabel
			// 
			this.RemarksLabel.AutoSize = true;
			this.RemarksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.RemarksLabel.Name = "RemarksLabel";
			this.RemarksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.RemarksLabel.TabIndex = 0;
			this.RemarksLabel.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("c6da14b2-8ce0-43ae-922e-9521f7c47c53", "Remarks:");
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 102, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.CancelBtn.TabIndex = 3;
			this.CancelBtn.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("fce5dcfc-ed7e-43de-bd7d-fe3d46423ec6", "Cancel");
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 102, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.OkButton.TabIndex = 2;
			this.OkButton.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8aee848b-695e-4d8c-87c8-30c10ce520ca", "OK");
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// RemarksListBox
			// 
			this.RemarksListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RemarksListBox.FormattingEnabled = true;
			this.RemarksListBox.HorizontalScrollbar = true;
			this.RemarksListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 9, true);
			this.RemarksListBox.Name = "RemarksListBox";
			this.RemarksListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 82, true);
			this.RemarksListBox.TabIndex = 1;
			// 
			// SelectDefaultRemarksForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 156, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RemarksListBox);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.RemarksLabel);
			this.DataSourceAssemblyName = "Enterprise.Customs.NZ.Business";
			this.DataSourceType = typeof(Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance);
			this.DataSourceTypeName = "Enterprise.Customs.NZ.Business.MessageBuilders.MessageManagerForClearance";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 183, true);
			this.Name = "SelectDefaultRemarksForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("31552822-721c-44f0-b394-01d08613c75d", "Select Default Remarks");
			this.Controls.SetChildIndex(this.RemarksLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.RemarksListBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZLabel RemarksLabel;
		public Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
		public Enterprise.ZArchitecture.GUI.ZButton OkButton;
		public CargoWise.Windows.UI.KListBox RemarksListBox;
	}
}
