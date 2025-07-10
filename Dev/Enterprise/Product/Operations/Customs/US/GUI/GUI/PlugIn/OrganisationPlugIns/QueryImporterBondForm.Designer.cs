namespace Enterprise.Customs.US.GUI
{
	partial class QueryImporterBondForm
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
			this.NumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.btnCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.btnSend = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Label1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 24, true);
			// 
			// NumberTextBox
			// 
			this.NumberTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.NumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 9, true);
			this.NumberTextBox.Name = "NumberTextBox";
			this.NumberTextBox.ReadOnly = true;
			this.NumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 20, true);
			this.NumberTextBox.TabIndex = 16;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 35, true);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnCancel.TabIndex = 18;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnSend
			// 
			this.btnSend.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnSend.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 35, true);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnSend.TabIndex = 17;
			this.btnSend.Text = "&Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// Label1
			// 
			this.Label1.AutoSize = true;
			this.Label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.Label1.Name = "Label1";
			this.Label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 13, true);
			this.Label1.TabIndex = 19;
			this.Label1.Text = "EIN/SSN/CBP to query:";
			// 
			// QueryImporterBondForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.btnCancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 89, true);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.NumberTextBox);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnSend);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "QueryImporterBondForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Query Importer Bond";
			this.Load += new System.EventHandler(this.QueryImporterBondForm_Load);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnSend, 0);
			this.Controls.SetChildIndex(this.btnCancel, 0);
			this.Controls.SetChildIndex(this.NumberTextBox, 0);
			this.Controls.SetChildIndex(this.Label1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton btnCancel;
		internal Enterprise.ZArchitecture.GUI.ZButton btnSend;
		private Enterprise.ZArchitecture.ZLabel Label1;
		public Enterprise.ZArchitecture.ZTextBox NumberTextBox;
	}
}
