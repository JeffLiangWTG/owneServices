namespace Enterprise.MasterFiles.GUI
{
	partial class ConfirmOrganizationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ConfirmOrgTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.orgsListKTableLayout = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.buttonKFlowLayout = new CargoWise.Windows.UI.KFlowLayoutPanel();
			this.confirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfirmOrgTableLayout.SuspendLayout();
			this.buttonKFlowLayout.SuspendLayout();
			this.SuspendLayout();
			// 
			// ConfirmOrgTableLayout
			// 
			this.ConfirmOrgTableLayout.AutoSize = true;
			this.ConfirmOrgTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ConfirmOrgTableLayout.ColumnCount = 1;
			this.ConfirmOrgTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ConfirmOrgTableLayout.Controls.Add(this.orgsListKTableLayout, 0, 0);
			this.ConfirmOrgTableLayout.Controls.Add(this.buttonKFlowLayout, 0, 1);
			this.ConfirmOrgTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfirmOrgTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfirmOrgTableLayout.Name = "ConfirmOrgTableLayout";
			this.ConfirmOrgTableLayout.RowCount = 2;
			this.ConfirmOrgTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.ConfirmOrgTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(46)));
			this.ConfirmOrgTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 483, true);
			this.ConfirmOrgTableLayout.TabIndex = 0;
			// 
			// orgsListKTableLayout
			// 
			this.orgsListKTableLayout.AutoScroll = true;
			this.orgsListKTableLayout.AutoSize = true;
			this.orgsListKTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.orgsListKTableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orgsListKTableLayout.ColumnCount = 1;
			this.orgsListKTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.orgsListKTableLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.orgsListKTableLayout.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 433, true);
			this.orgsListKTableLayout.Name = "orgsListKTableLayout";
			this.orgsListKTableLayout.RowCount = 1;
			this.orgsListKTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.orgsListKTableLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 433, true);
			this.orgsListKTableLayout.TabIndex = 0;
			// 
			// buttonKFlowLayout
			// 
			this.buttonKFlowLayout.Controls.Add(this.confirmButton);
			this.buttonKFlowLayout.Controls.Add(this.cancelButton);
			this.buttonKFlowLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this.buttonKFlowLayout.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.buttonKFlowLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 439, true);
			this.buttonKFlowLayout.Name = "buttonKFlowLayout";
			this.buttonKFlowLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(654, 42, true);
			this.buttonKFlowLayout.TabIndex = 0;
			// 
			// confirmButton
			// 
			this.confirmButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.confirmButton.BackColor = System.Drawing.Color.DeepSkyBlue;
			this.confirmButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e554e60b-eee3-4310-a24d-4df8d06af794", "Confirm");
			this.confirmButton.FlatAppearance.BorderSize = 0;
			this.confirmButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.confirmButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.confirmButton.ForeColor = System.Drawing.Color.White;
			this.confirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(529, 7, true);
			this.confirmButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, true);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 29, true);
			this.confirmButton.TabIndex = 2;
			this.confirmButton.ToolTipCaption = null;
			this.confirmButton.UseVisualStyleBackColor = false;
			this.confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.cancelButton.BackColor = System.Drawing.Color.DimGray;
			this.cancelButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8b9d582b-3f3c-406c-b39f-b373aec0be3a", "Cancel");
			this.cancelButton.FlatAppearance.BorderSize = 0;
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 7, true);
			this.cancelButton.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 15, 7, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 29, true);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = false;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ConfirmOrganizationForm
			// 
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 483, true);
			this.Controls.Add(this.ConfirmOrgTableLayout);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 523, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 123, true);
			this.Name = "ConfirmOrganizationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ConfirmOrganizationForm";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfirmOrgTableLayout.ResumeLayout(false);
			this.ConfirmOrgTableLayout.PerformLayout();
			this.buttonKFlowLayout.ResumeLayout(false);
			this.buttonKFlowLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel ConfirmOrgTableLayout;
		private CargoWise.Windows.UI.KTableLayoutPanel orgsListKTableLayout;
		private CargoWise.Windows.UI.KFlowLayoutPanel buttonKFlowLayout;
		private ZArchitecture.GUI.ZButton confirmButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
