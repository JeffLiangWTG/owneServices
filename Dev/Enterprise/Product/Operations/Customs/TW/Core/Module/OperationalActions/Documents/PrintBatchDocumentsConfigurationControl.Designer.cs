namespace Enterprise.Customs.TW.Module.OperationalActions
{
	partial class PrintBatchDocumentsConfigurationControl
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
			this.ReMergeAndCalculateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuppressNotificationDialogsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IgnoreMessageWarningsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ReMergeAndCalculateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReMergeAndCalculateCheckBox, "ReMergeAndCalculate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.PrintBatchDocumentsOperationalActionMethodApplicator)(null)).ReMergeAndCalculate)));
			this.ReMergeAndCalculateCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("51149345-d644-4ed0-aa93-e642c130e71b", "Re-merge and re-calculate before printing");
			this.ReMergeAndCalculateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 88, true);
			this.ReMergeAndCalculateCheckBox.Name = "ReMergeAndCalculateCheckBox";
			this.ReMergeAndCalculateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.ReMergeAndCalculateCheckBox.TabIndex = 9;
			this.ReMergeAndCalculateCheckBox.UseVisualStyleBackColor = true;
			// 
			// SuppressNotificationDialogsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SuppressNotificationDialogsCheckBox, "SuppressNotificationPopout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.PrintBatchDocumentsOperationalActionMethodApplicator)(null)).SuppressNotificationPopout)));
			this.SuppressNotificationDialogsCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("786dbde5-5db1-44d4-a79b-0e4aff7d81ba", "Suppress pop-up notification dialogs");
			this.SuppressNotificationDialogsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 54, true);
			this.SuppressNotificationDialogsCheckBox.Name = "SuppressNotificationDialogsCheckBox";
			this.SuppressNotificationDialogsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.SuppressNotificationDialogsCheckBox.TabIndex = 8;
			this.SuppressNotificationDialogsCheckBox.UseVisualStyleBackColor = true;
			// 
			// IgnoreMessageWarningsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IgnoreMessageWarningsCheckBox, "IgnoreMessageWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.PrintBatchDocumentsOperationalActionMethodApplicator)(null)).IgnoreMessageWarnings)));
			this.IgnoreMessageWarningsCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("db5a6bae-261a-4fde-be27-bc6e7b13ca06", "Ignore message warnings and continue printing");
			this.IgnoreMessageWarningsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 20, true);
			this.IgnoreMessageWarningsCheckBox.Name = "IgnoreMessageWarningsCheckBox";
			this.IgnoreMessageWarningsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.IgnoreMessageWarningsCheckBox.TabIndex = 7;
			this.IgnoreMessageWarningsCheckBox.UseVisualStyleBackColor = true;
			// 
			// PrintBatchDocumentsConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ReMergeAndCalculateCheckBox);
			this.Controls.Add(this.SuppressNotificationDialogsCheckBox);
			this.Controls.Add(this.IgnoreMessageWarningsCheckBox);
			this.Name = "PrintBatchDocumentsConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZCheckBox ReMergeAndCalculateCheckBox;
		private ZArchitecture.GUI.ZCheckBox SuppressNotificationDialogsCheckBox;
		private ZArchitecture.GUI.ZCheckBox IgnoreMessageWarningsCheckBox;
	}
}
