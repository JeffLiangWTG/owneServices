namespace Enterprise.MasterFiles.GUI
{
	partial class EPaymentConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.configGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.configGrid)).BeginInit();
			this.configGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EPaymentConfigurationCollection);
			// 
			// configGrid
			// 
			this.configGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.configGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.EPaymentConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EPaymentConfiguration)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EPaymentConfiguration)(null)).CountryDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EPaymentConfiguration)(null)).OFXEPaymentEnabled)));
			this.configGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("0fdbd640-d6c7-476e-b9d9-e0543309d77d", "Country/Region Code");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5330012a-c172-4cd2-9e70-3913e3daf867", "Country/Region Description");
			zTextBoxColumnStyleInfo2.ColumnName = "CountryDescription";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a12b2343-97c0-45af-9d58-d6686ee2154f", "Enable OFX E-Payments");
			zCheckBoxColumnStyleInfo1.ColumnName = "OFXEPaymentEnabled";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.configGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.configGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.configGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.configGrid.GridId = "cfc95096-c3d8-428f-927e-77dbf4f4657c";
			this.configGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.configGrid.LayoutKey = "zGrid1";
			this.configGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.configGrid.Name = "configGrid";
			this.configGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(371, 80, true);
			this.configGrid.TabIndex = 0;
			// 
			// EPaymentConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.configGrid);
			this.Name = "EPaymentConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 87, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.configGrid)).EndInit();
			this.configGrid.ResumeLayout(false);
			this.configGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid configGrid;
	}
}
