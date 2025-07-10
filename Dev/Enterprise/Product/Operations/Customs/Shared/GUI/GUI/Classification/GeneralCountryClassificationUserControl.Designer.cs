namespace Enterprise.Customs.GUI
{
	partial class GeneralCountryClassificationUserControl
	{
		private Enterprise.Customs.Universal.GUI.TariffFindBox CC_TariffNumFindBox;
		private System.ComponentModel.Container components = null;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CC_TariffNumFindBox = new Enterprise.Customs.Universal.GUI.TariffFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.CC_TariffNumFindBox);
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 152, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_TariffNumFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			// 
			// DescriptionTextBox
			// 
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 32, true);
			// 
			// CC_TariffNumFindBox
			// 
			this.BindingSource.SetBindingMember(this.CC_TariffNumFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.CC_TariffNumFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("GeneralCountryClassificationUserControl|5dc61254-6323-4abd-a3a6-ebc4d1c82a16", "Tariff");
			this.CC_TariffNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.CC_TariffNumFindBox.Name = "CC_TariffNumFindBox";
			this.CC_TariffNumFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.CC_TariffNumFindBox.TabIndex = 1;
			// 
			// GeneralCountryClassificationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GeneralCountryClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 280, true);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
