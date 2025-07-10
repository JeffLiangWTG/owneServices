namespace Enterprise.Customs.TR.GUI
{
	public partial class InvoiceLineVehicleUserControl
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
			if (disposing)
			{
				components?.Dispose();
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.VehicleGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VehicleGrid)).BeginInit();
			this.VehicleGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// VehicleGrid
			// 
			this.VehicleGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VehicleGrid, "FilteredInvoiceLines.Vehicles");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_RegistrationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_BrandName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).BrandValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).BrandValueInTRY)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_SerialNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_ModelYear)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_ModelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).Engine.CEG_CapacityCC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).Engine.CEG_Cylinders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_Color)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).Engine.CEG_EngineType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_VehicleIdentificationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).Engine.CEG_CapacityHP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).Gears)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.CusVehicle)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Vehicles)).SyncRoot)).CVH_IMEINo)));
			this.VehicleGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("29569f74-9adb-4892-ab7d-b1ac64fc7397", "Registration No");
			zTextBoxColumnStyleInfo1.ColumnName = "CVH_RegistrationNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("75679f83-e63e-4daf-8925-0aec155f7156", "Brand Name");
			zTextBoxColumnStyleInfo2.ColumnName = "CVH_BrandName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("0f1e5ee6-eff5-42d6-a5f1-f5d322a878fe", "Brand Value");
			zCalcEditColumnStyleInfo1.ColumnName = "BrandValue";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("13c38da3-002f-4409-baae-204c300c5d57", "Brand Value in TRY");
			zCalcEditColumnStyleInfo2.ColumnName = "BrandValueInTRY";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("c71b7ff5-d980-4309-b906-81da64129ea0", "Reference No");
			zTextBoxColumnStyleInfo3.ColumnName = "CVH_SerialNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("e0e2d984-62a0-4f80-8e8c-7665e6676e2b", "Model Year");
			zTextBoxColumnStyleInfo4.ColumnName = "CVH_ModelYear";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("dd8df2fa-bbc5-4811-9ff7-cdcb13c3cef0", "Model Name");
			zTextBoxColumnStyleInfo5.ColumnName = "CVH_ModelName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("7cf17b21-55bc-4eeb-b159-1bc8f23fb994", "Motor CC");
			zCalcEditColumnStyleInfo3.ColumnName = "Engine+CEG_CapacityCC";
			zCalcEditColumnStyleInfo3.MaxValue = 9999;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("fb1cb37d-bc2e-4b4c-bad2-be7ff10a3ac6", "Cylinder Qty");
			zCalcEditColumnStyleInfo4.ColumnName = "Engine+CEG_Cylinders";
			zCalcEditColumnStyleInfo4.MaxValue = 9;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("d76b72d1-82e2-4666-9996-03d76ac07815", "Color");
			zTextBoxColumnStyleInfo6.ColumnName = "CVH_Color";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("b23b4799-f7d3-4685-a3e4-0f6fb9eac8f7", "Engine Type");
			zDropEditColumnStyleInfo1.ColumnName = "Engine+CEG_EngineType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("5d69e605-ad2a-43c6-9cfa-fd71b32f8f66", "VIN");
			zTextBoxColumnStyleInfo7.ColumnName = "CVH_VehicleIdentificationNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("fe22be4b-40b7-4c74-a230-ae52d55eff34", "Engine HP");
			zCalcEditColumnStyleInfo5.ColumnName = "Engine+CEG_CapacityHP";
			zCalcEditColumnStyleInfo5.MaxValue = 9999;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("8fa91785-46a2-43df-aacf-8af641e93b93", "Gear Type");
			zDropEditColumnStyleInfo2.ColumnName = "Gears";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("e767fe00-c8d1-472d-bf22-44cf10e5b81f", "IMEI No");
			zTextBoxColumnStyleInfo8.ColumnName = "CVH_IMEINo";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VehicleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VehicleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VehicleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.VehicleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.VehicleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.VehicleGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.VehicleGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.VehicleGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.VehicleGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VehicleGrid.GridId = "82ef5be5-70ef-487d-bcf7-d1af6bd48be2";
			this.VehicleGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VehicleGrid.LayoutKey = "VehicleGrid";
			this.VehicleGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VehicleGrid.Name = "VehicleGrid";
			this.VehicleGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 172, true);
			this.VehicleGrid.TabIndex = 1;
			// 
			// InvoiceLineVehicleUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VehicleGrid);
			this.Name = "InvoiceLineVehicleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(673, 172, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VehicleGrid)).EndInit();
			this.VehicleGrid.ResumeLayout(false);
			this.VehicleGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid VehicleGrid;
	}
}
