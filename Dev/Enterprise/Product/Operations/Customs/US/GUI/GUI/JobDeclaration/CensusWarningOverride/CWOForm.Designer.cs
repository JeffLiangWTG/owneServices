namespace Enterprise.Customs.US.GUI
{
	partial class CWOForm
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
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.censusWarningOverrideUserControl1 = new Enterprise.Customs.US.GUI.CensusWarningOverrideUserControl();
			this.DefaultButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 391, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 24, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Business.EntryCensusWarningOverrideCollection);
			// 
			// CancelButton1
			// 
			this.CancelButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(641, 362, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton1.TabIndex = 4;
			this.CancelButton1.Text = "Cancel";
			this.CancelButton1.UseVisualStyleBackColor = true;
			this.CancelButton1.Click += new System.EventHandler(this.CancelButton1_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(476, 362, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.Text = "Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(557, 362, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 23, true);
			this.SaveButton.TabIndex = 3;
			this.SaveButton.Text = "Save && Close";
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// censusWarningOverrideUserControl1
			// 
			this.censusWarningOverrideUserControl1.AllowDrop = true;
			this.censusWarningOverrideUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.censusWarningOverrideUserControl1, ".");
			this.censusWarningOverrideUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.censusWarningOverrideUserControl1.Name = "censusWarningOverrideUserControl1";
			this.censusWarningOverrideUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 352, true);
			this.censusWarningOverrideUserControl1.TabIndex = 0;
			// 
			// DefaultButton
			// 
			this.DefaultButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DefaultButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 362, true);
			this.DefaultButton.Name = "DefaultButton";
			this.DefaultButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.DefaultButton.TabIndex = 1;
			this.DefaultButton.Text = "Default CWOs";
			this.DefaultButton.UseVisualStyleBackColor = true;
			this.DefaultButton.Click += new System.EventHandler(this.DefaultButton_Click);
			// 
			// CWOForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 415, true);
			this.Controls.Add(this.DefaultButton);
			this.Controls.Add(this.censusWarningOverrideUserControl1);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.CancelButton1);
			this.Controls.Add(this.SendButton);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceType = typeof(Enterprise.Customs.US.Business.EntryCensusWarningOverrideCollection);
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.EntryCensusWarningOverrideCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 453, true);
			this.Name = "CWOForm";
			this.Text = "CWOForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.CancelButton1, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.censusWarningOverrideUserControl1, 0);
			this.Controls.SetChildIndex(this.DefaultButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.GUI.ZButton CancelButton1;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZButton SaveButton;
		private CensusWarningOverrideUserControl censusWarningOverrideUserControl1;
		internal ZArchitecture.GUI.ZButton DefaultButton;
	}
}
