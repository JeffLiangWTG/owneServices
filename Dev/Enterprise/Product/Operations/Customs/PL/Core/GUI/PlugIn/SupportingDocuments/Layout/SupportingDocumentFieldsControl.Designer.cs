namespace Enterprise.Customs.PL.GUI.PlugIn
{
	partial class SupportingDocumentFieldsControl
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
            this.ValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
            this.SupDocReference2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupDocDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ValueConvertToLocalCurrencyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.SupportingDocument);
            // 
            // ValueConvertToLocalCurrencyControl
            // 
            this.ValueConvertToLocalCurrencyControl.AllowDrop = true;
            this.ValueConvertToLocalCurrencyControl.BindToAmount = "CSI_Value";
            this.ValueConvertToLocalCurrencyControl.BindToUnit = "CSI_RX_NKCurrency";
            this.ValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
            this.ValueConvertToLocalCurrencyControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 8, true);
            this.ValueConvertToLocalCurrencyControl.Name = "ValueConvertToLocalCurrencyControl";
            this.ValueConvertToLocalCurrencyControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
            this.ValueConvertToLocalCurrencyControl.TabIndex = 13;
            // 
            // SupDocReference2TextBox
            // 
            this.SupDocReference2TextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.SupDocReference2TextBox, "CSI_ReferenceNumber2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.SupportingDocument)(null)).CSI_ReferenceNumber2)));
            this.SupDocReference2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SupDocReference2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 34, true);
            this.SupDocReference2TextBox.Name = "SupDocReference2TextBox";
            this.SupDocReference2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
            this.SupDocReference2TextBox.TabIndex = 6;
            // 
            // SupDocDescriptionTextBox
            // 
            this.SupDocDescriptionTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.SupDocDescriptionTextBox, "CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.Declaration.SupportingDocument)(null)).CSI_Description)));
            this.SupDocDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.SupDocDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 60, true);
            this.SupDocDescriptionTextBox.Name = "SupDocDescriptionTextBox";
            this.SupDocDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
            this.SupDocDescriptionTextBox.TabIndex = 8;
            // 
            // SupportingDocumentFieldsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ValueConvertToLocalCurrencyControl);
            this.Controls.Add(this.SupDocReference2TextBox);
            this.Controls.Add(this.SupDocDescriptionTextBox);
            this.Name = "SupportingDocumentFieldsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 111, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ValueConvertToLocalCurrencyControl.ResumeLayout(true);
            this.ValueConvertToLocalCurrencyControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl ValueConvertToLocalCurrencyControl;
		internal ZArchitecture.ZTextBox SupDocReference2TextBox;
		internal ZArchitecture.ZTextBox SupDocDescriptionTextBox;
	}
}
