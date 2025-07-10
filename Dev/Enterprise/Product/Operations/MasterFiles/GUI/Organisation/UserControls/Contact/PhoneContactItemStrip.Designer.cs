namespace Enterprise.MasterFiles.GUI
{
	partial class PhoneContactItemStrip
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
		private void InitializeComponent()
		{
			this.PhoneNumberControl = new Enterprise.MasterFiles.GUI.PhoneNumberUserControl();
			this.DescriptionDropDownList.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PhoneNumberControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// deleteButton
			// 
			this.deleteButton.FlatAppearance.BorderSize = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.PhoneContactItem);
			// 
			// PhoneNumberControl
			// 
			this.PhoneNumberControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhoneNumberControl, "Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.PhoneNumber)(((Enterprise.MasterFiles.Business.PhoneContactItem)(null)).Number)));
			this.PhoneNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 2, true);
			this.PhoneNumberControl.Name = "PhoneNumberControl";
			this.PhoneNumberControl.EnableValidStateColor = true;
			this.PhoneNumberControl.ShowLocalNumberLabel = false;
			this.PhoneNumberControl.ShowPublishedCheckBox = false;
			this.PhoneNumberControl.UseNumberTypeCaption = false;
			this.PhoneNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.PhoneNumberControl.TabIndex = 1;
			this.PhoneNumberControl.UnscaledLeftPadding = -72;
			// 
			// PhoneContactItemStrip
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PhoneNumberControl);
			this.Name = "PhoneContactItemStrip";
			this.Controls.SetChildIndex(this.PhoneNumberControl, 0);
			this.Controls.SetChildIndex(this.DescriptionDropDownList, 0);
			this.Controls.SetChildIndex(this.deleteButton, 0);
			this.DescriptionDropDownList.ResumeLayout(true);
			this.DescriptionDropDownList.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PhoneNumberControl.ResumeLayout(true);
			this.PhoneNumberControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected PhoneNumberUserControl PhoneNumberControl;

	}
}
