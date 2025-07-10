namespace Enterprise.Customs.ZA.Module
{
	partial class ExpiryDateConfirmationForm
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 122, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Module.NonPersistentExpiryDateObject);
            // 
            // DateEdit
            // 
            this.DateEdit.AllowDrop = true;
            this.DateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DateEdit, "Date");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ZA.Module.NonPersistentExpiryDateObject)(null)).Date)));
            this.DateEdit.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("0a63fa1c-5ef2-468d-b44a-e95665b42240", "Entry Date before which entries must be cleared");
            this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 43, true);
            this.DateEdit.Name = "DateEdit";
            this.DateEdit.TabIndex = 1;
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("18FBBBB8-B51F-42CF-A407-370F4F195A23", "Cancel");
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 95, true);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.ToolTipCaption = null;
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("7D81016F-2E27-463C-84D9-51CCE1E97142", "OK");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(208, 95, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
            this.OKButton.TabIndex = 2;
            this.OKButton.ToolTipCaption = null;
            // 
            // ExpiryDateConfirmationForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.ZA.Module.Res.GetData("F4B6F23C-A5D5-4FC8-A26B-D50F1F4D4B9C", "Clear Expired Stock");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(367, 146, true);
            this.ControlBox = false;
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.DateEdit);
            this.DataSourceType = typeof(Enterprise.Customs.ZA.Business.JobComInvoiceLine);
            this.Name = "ExpiryDateConfirmationForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Controls.SetChildIndex(this.DateEdit, 0);
            this.Controls.SetChildIndex(this.OKButton, 0);
            this.Controls.SetChildIndex(this.CancelButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DateEdit.ResumeLayout(true);
            this.DateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit DateEdit;
		internal new ZArchitecture.GUI.ZButton CancelButton;
		internal ZArchitecture.GUI.ZButton OKButton;
	}
}
