namespace Enterprise.MasterFiles.GUI
{
	partial class RefShippingLineEBLProviderControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoIsAvailable = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1RseName = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoIsDefault = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.EBLProviderGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.EBLProviderGrid)).BeginInit();
			this.EBLProviderGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefShippingLine);
			// 
			// EBLProviderGrid
			//
			this.EBLProviderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.EBLProviderGrid, "ShippingLineEBLProviders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineEBLProviders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineEBLProvider)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineEBLProviders)).SyncRoot)).RSE_IsAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefShippingLineEBLProvider)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineEBLProviders)).SyncRoot)).RSE_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefShippingLineEBLProvider)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefShippingLine)(null)).ShippingLineEBLProviders)).SyncRoot)).RSE_IsDefault)));
			this.EBLProviderGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfoIsAvailable.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e796884f-8615-47c3-8654-5bfa89a7b415", "Is Available", "Is Available", "Is eBL Provider Available");
			zCheckBoxColumnStyleInfoIsAvailable.ColumnName = "RSE_IsAvailable";
			zCheckBoxColumnStyleInfoIsAvailable.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1RseName.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d76264bc-b97e-4275-a445-9298e255e019", "eBL Provider Name", "eBL Provider Name", "eBL Documentation Provider Name");
			zTextBoxColumnStyleInfo1RseName.ColumnName = "RSE_Name";
			zTextBoxColumnStyleInfo1RseName.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfoIsDefault.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("da25d75a-e569-4732-953e-dbd40323ca03", "Default", "Default", "Is eBL Provider Default");
			zCheckBoxColumnStyleInfoIsDefault.ColumnName = "RSE_IsDefault";
			zCheckBoxColumnStyleInfoIsDefault.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.EBLProviderGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfoIsAvailable);
			this.EBLProviderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1RseName);
			this.EBLProviderGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfoIsDefault);

			this.EBLProviderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EBLProviderGrid.GridId = "d4e697f2-cb19-487d-b10c-0f5237453cba";
			this.EBLProviderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.EBLProviderGrid.LayoutKey = "EBLProviderGrid";
			this.EBLProviderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EBLProviderGrid.Name = "EBLProviderGrid";
			this.EBLProviderGrid.ReadOnly = true;
			this.EBLProviderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			this.EBLProviderGrid.TabIndex = 0;
			// 
			// RefShippingLineEBLProviderControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EBLProviderGrid);
			this.Name = "RefShippingLineEBLProviderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 81, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.EBLProviderGrid)).EndInit();
			this.EBLProviderGrid.ResumeLayout(false);
			this.EBLProviderGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZGrid EBLProviderGrid;

		#endregion
	}
}
