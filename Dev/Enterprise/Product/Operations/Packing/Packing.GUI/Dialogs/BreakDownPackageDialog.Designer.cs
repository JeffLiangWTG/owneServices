namespace Enterprise.Packing.GUI
{
	partial class BreakDownPackageDialog
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

			splitQuantityCalcEdit.Enter -= OkButton_Click;

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.splitQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 28, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Packing.Business.BreakDownPackage);
			// 
			// splitQuantityCalcEdit
			// 
			this.splitQuantityCalcEdit.AcceptsReturn = true;
			this.splitQuantityCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.splitQuantityCalcEdit, "SplitQuantity");
			this.splitQuantityCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.splitQuantityCalcEdit, false);
			this.splitQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 59, true);
			this.splitQuantityCalcEdit.Name = "splitQuantityCalcEdit";
			this.splitQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(156, 20, true);
			this.splitQuantityCalcEdit.TabIndex = 0;
			this.splitQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.splitQuantityCalcEdit.TextChanged += new System.EventHandler(this.SplitQuantityCalcEdit_TextChanged);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.cancelButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("BreakDownPackageDialog|7ee3d92c-07d3-4eb1-b661-22cddbc6d207", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 85, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.UseCompatibleTextRendering = true;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// messageLabel
			// 
			this.messageLabel.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("BreakDownPackageDialog|3cb533e3-375c-427d-acc5-b655ad0ed3a5", "", "When breaking down packages, only empty Packages will be split.\r\nPlease enter the quantity you wish to split the Package into.");
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 39, true);
			this.messageLabel.TabIndex = 17;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.okButton.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("BreakDownPackageDialog|9d951f4e-2811-4077-94a3-26c2119b3686", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 85, true);
			this.okButton.Name = "okButton";
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 1;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// BreakDownPackageDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 120, true);
			this.Controls.Add(this.splitQuantityCalcEdit);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.messageLabel);
			this.Controls.Add(this.okButton);
			this.DataSourceType = typeof(Enterprise.Packing.Business.BreakDownPackage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "BreakDownPackageDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "SplitPackagesDialog";
			this.Shown += new System.EventHandler(this.BreakDownPackageDialog_Shown);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.messageLabel, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.splitQuantityCalcEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit splitQuantityCalcEdit;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZLabel messageLabel;
		private ZArchitecture.GUI.ZButton okButton;

	}
}
