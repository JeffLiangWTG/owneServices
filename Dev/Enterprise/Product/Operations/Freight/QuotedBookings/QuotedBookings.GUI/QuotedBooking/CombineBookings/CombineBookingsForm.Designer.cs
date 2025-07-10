namespace Enterprise.Freight.QuotedBookings.GUI
{
	partial class CombineBookingsForm
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
		protected new void InitializeComponent()
		{
			this.kPanel1 = new CargoWise.Windows.UI.KPanel();
			this.combineBookingsControl = new Enterprise.Freight.QuotedBookings.GUI.CombineBookingsControl();
			cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.kPanel1.SuspendLayout();
			this.combineBookingsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 590, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.CombineBookings);
			// 
			// cancelButton
			// 
			cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			cancelButton.CaptionResourceString = Res.GetData("2ec0ef09-65dc-4c1b-bda8-96919ecd87a8", "Cancel");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(cancelButton, false);
			cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(601, 6, true);
			cancelButton.Name = "cancelButton";
			cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			cancelButton.TabIndex = 2;
			cancelButton.UseVisualStyleBackColor = true;
			cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// okButton
			// 
			okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			okButton.CaptionResourceString = Res.GetData("2233d903-5cf4-455b-8156-4e82195ef089", "OK");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(okButton, false);
			okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(681, 6, true);
			okButton.Name = "okButton";
			okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			okButton.TabIndex = 3;
			okButton.UseVisualStyleBackColor = true;
			okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// kPanel1
			// 
			this.kPanel1.Controls.Add(cancelButton);
			this.kPanel1.Controls.Add(okButton);
			this.kPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.kPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 555, true);
			this.kPanel1.Name = "kPanel1";
			this.kPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 35, true);
			this.kPanel1.TabIndex = 1;
			// 
			// combineBookingsControl
			// 
			this.combineBookingsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.combineBookingsControl, ".");
			this.combineBookingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.combineBookingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.combineBookingsControl.Name = "combineBookingsControl";
			this.combineBookingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 555, true);
			this.combineBookingsControl.TabIndex = 2;
			// 
			// CombineBookingsForm
			// 
			this.AcceptButton = okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 614, true);
			this.Controls.Add(this.combineBookingsControl);
			this.Controls.Add(this.kPanel1);
			this.DataSourceType = typeof(Enterprise.Freight.QuotedBookings.Business.CombineBookings);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 614, true);
			this.Name = "CombineBookingsForm";
			this.Text = "CombineBookingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.kPanel1, 0);
			this.Controls.SetChildIndex(this.combineBookingsControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.kPanel1.ResumeLayout(false);
			this.kPanel1.PerformLayout();
			this.combineBookingsControl.ResumeLayout(true);
			this.combineBookingsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KPanel kPanel1;
		private CombineBookingsControl combineBookingsControl;
		private Enterprise.ZArchitecture.GUI.ZButton cancelButton;
		private Enterprise.ZArchitecture.GUI.ZButton okButton;
	}
}