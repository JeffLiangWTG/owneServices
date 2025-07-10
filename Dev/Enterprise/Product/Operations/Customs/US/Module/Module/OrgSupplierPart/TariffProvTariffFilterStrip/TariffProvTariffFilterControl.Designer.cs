namespace Enterprise.Customs.US.Module
{
	partial class TariffProvTariffFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProvTariffTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.Module.TariffProvTariffModuleFilter);
			// 
			// TariffTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffTextBox, "Property");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.TariffProvTariffModuleFilter)(null)).Property)));
			this.TariffTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("c0ff33ea-52a7-40fb-b3a8-3e9b1c71a198", "Tariff");
			this.TariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 2, true);
			this.TariffTextBox.Name = "TariffTextBox";
			this.TariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.TariffTextBox.TabIndex = 0;
			this.TariffTextBox.Tag = "Tariff";
			// 
			// ProvTariffTextBox
			// 
			this.BindingSource.SetBindingMember(this.ProvTariffTextBox, "ProvTariff");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Module.TariffProvTariffModuleFilter)(null)).ProvTariff)));
			this.ProvTariffTextBox.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("2f744768-20a4-4246-8f4b-18165f4137a2", "Prov. Tariff");
			this.ProvTariffTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 2, true);
			this.ProvTariffTextBox.Name = "ProvTariffTextBox";
			this.ProvTariffTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.ProvTariffTextBox.TabIndex = 2;
			this.ProvTariffTextBox.Tag = "Prov. Tariff";
			// 
			// TariffProvTariffFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ProvTariffTextBox);
			this.Controls.Add(this.TariffTextBox);
			this.Name = "TariffProvTariffFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 25, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox TariffTextBox;
		private ZArchitecture.ZTextBox ProvTariffTextBox;
	}
}
