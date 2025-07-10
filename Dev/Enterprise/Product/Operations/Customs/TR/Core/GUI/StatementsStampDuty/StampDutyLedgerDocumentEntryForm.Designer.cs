namespace Enterprise.Customs.TR.GUI
{
	partial class StampDutyLedgerDocumentEntryForm
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
		new void InitializeComponent()
		{
			this.btnClose = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnOk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrintDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 24, true);
			// 
			// btnClose
			// 
			this.btnClose.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("8F656F5A-1F3B-49C4-989C-99239A8B5D96", "Cancel");
			this.btnClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(186, 60, true);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnClose.TabIndex = 4;
			this.btnClose.ToolTipCaption = null;
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("3A0BB871-8E7D-4064-9085-E1B787076C65", "OK");
			this.btnOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 60, true);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.btnOk.TabIndex = 3;
			this.btnOk.ToolTipCaption = null;
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// PrintDateDateEdit
			// 
			this.PrintDateDateEdit.AllowDrop = true;
			this.PrintDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PrintDateDateEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("5628051F-7EB4-4256-B0FE-7311641CCC6C", "Print Date");
			this.PrintDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 33, true);
			this.PrintDateDateEdit.Name = "PrintDateDateEdit";
			this.PrintDateDateEdit.TabIndex = 2;
			// 
			// StampDutyLedgerDocumentEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("267F0BCD-85D3-4B8A-A59B-D626F286B3FB", "Stamp Duty Ledger Document Entry Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 128, true);
			this.Controls.Add(this.PrintDateDateEdit);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnClose);
			this.Name = "StampDutyLedgerDocumentEntryForm";
			this.Text = "Stamp Duty Ledger Print EntryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnClose, 0);
			this.Controls.SetChildIndex(this.btnOk, 0);
			this.Controls.SetChildIndex(this.PrintDateDateEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrintDateDateEdit.ResumeLayout(true);
			this.PrintDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZButton btnClose;
		internal ZArchitecture.GUI.ZButton btnOk;
		internal ZArchitecture.GUI.ZDateEdit PrintDateDateEdit;
	}
}
