namespace Enterprise.Customs.TR.GUI
{
	partial class ManualRegistrationNoEntryForm
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
			this.RegistrationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RegistrationDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.ManualRegistrationNoEntry);
			// 
			// btnClose
			// 
			this.btnClose.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("a89f1d28-f102-47d7-94c1-ff10dc9ad7c4", "Cancel");
			this.btnClose.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 80, true);
			this.btnClose.Name = "btnClose";
			this.btnClose.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.btnClose.TabIndex = 4;
			this.btnClose.ToolTipCaption = null;
			this.btnClose.UseVisualStyleBackColor = true;
			this.btnClose.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("7ee685f3-28b1-4814-a8a5-c0f08a39e17d", "Update Registration No");
			this.btnOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 80, true);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 23, true);
			this.btnOk.TabIndex = 3;
			this.btnOk.ToolTipCaption = null;
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// RegistrationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNoTextBox, "RegistrationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.ManualRegistrationNoEntry)(null)).RegistrationNumber)));
			this.RegistrationNoTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("6b57fc98-e6bf-4893-8f4e-b86a95905408", "Registration No");
			this.RegistrationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 24, true);
			this.RegistrationNoTextBox.Name = "RegistrationNoTextBox";
			this.RegistrationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
			this.RegistrationNoTextBox.TabIndex = 1;
			// 
			// RegistrationDateEdit
			// 
			this.RegistrationDateEdit.AllowDrop = true;
			this.RegistrationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RegistrationDateEdit, "RegistrationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.ManualRegistrationNoEntry)(null)).RegistrationDate)));
			this.RegistrationDateEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("567349d5-b62d-489e-a821-1e7a2206c4cb", "Registration Date");
			this.RegistrationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RegistrationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 48, true);
			this.RegistrationDateEdit.Name = "RegistrationDateEdit";
			this.RegistrationDateEdit.TabIndex = 2;
			// 
			// ManualRegistrationNoEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("24a82dda-d0f4-45b3-b599-d22e14da1f06", "Manual Registration No Entry");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 144, true);
			this.Controls.Add(this.RegistrationDateEdit);
			this.Controls.Add(this.RegistrationNoTextBox);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnClose);
			this.DataSourceType = typeof(Enterprise.Customs.TR.Business.ManualRegistrationNoEntry);
			this.Name = "ManualRegistrationNoEntryForm";
			this.Text = "Manual Registration No Entry";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.btnClose, 0);
			this.Controls.SetChildIndex(this.btnOk, 0);
			this.Controls.SetChildIndex(this.RegistrationNoTextBox, 0);
			this.Controls.SetChildIndex(this.RegistrationDateEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RegistrationDateEdit.ResumeLayout(true);
			this.RegistrationDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton btnClose;
		private ZArchitecture.GUI.ZButton btnOk;
		private ZArchitecture.ZTextBox RegistrationNoTextBox;
		private ZArchitecture.GUI.ZDateEdit RegistrationDateEdit;
	}
}
