namespace Enterprise.MasterFiles.GUI
{
	partial class AccPlaceOfSupplyConfigurationControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PlaceOfSupplyConfigurationGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PlaceOfSupplyConfigurationGrid)).BeginInit();
			this.PlaceOfSupplyConfigurationGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccPOSConfigurationCollection);
			// 
			// PlaceOfSupplyConfigurationGrid
			// 
			this.PlaceOfSupplyConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PlaceOfSupplyConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).LevelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_ChargeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_IncoTerm)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_ServiceDirection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_TaxRegistrationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_NK_Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_SupplyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccPOSConfiguration)(null)).PSC_PlaceOfSupplyRule)));
			this.PlaceOfSupplyConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("600323e1-667c-4bfd-a6f1-9edd48ac3f60", "Source");
			zTextBoxColumnStyleInfo1.ColumnName = "LevelName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "PSC_JobType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "PSC_ChargeType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo7.ColumnName = "PSC_IncoTerm";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "PSC_ServiceDirection";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo8.ColumnName = "PSC_TransportMode";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo4.ColumnName = "PSC_TaxRegistrationType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.ColumnName = "PSC_NK_Branch";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo9.ColumnName = "PSC_SupplyType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo6.ColumnName = "PSC_PlaceOfSupplyRule";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.PlaceOfSupplyConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.PlaceOfSupplyConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PlaceOfSupplyConfigurationGrid.GridId = "BA268486-6A1D-48EB-A542-98E799B262C1";
			this.PlaceOfSupplyConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PlaceOfSupplyConfigurationGrid.LayoutKey = "PlaceOfSupplyConfigurationGrid";
			this.PlaceOfSupplyConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PlaceOfSupplyConfigurationGrid.Name = "PlaceOfSupplyConfigurationGrid";
			this.PlaceOfSupplyConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
			this.PlaceOfSupplyConfigurationGrid.TabIndex = 1;
			// 
			// AccPlaceOfSupplyConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = this.Enabled;
			this.Controls.Add(this.PlaceOfSupplyConfigurationGrid);
			this.Name = "AccPlaceOfSupplyConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(699, 245, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PlaceOfSupplyConfigurationGrid)).EndInit();
			this.PlaceOfSupplyConfigurationGrid.ResumeLayout(false);
			this.PlaceOfSupplyConfigurationGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZGrid PlaceOfSupplyConfigurationGrid;
	}
}
