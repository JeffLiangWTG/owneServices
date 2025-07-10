using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	partial class FallbackSubjectToChargesControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleRatesProviderCode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo columnStyleIsFallbackEnabled = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.FallbackSubjectToChargesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.FallbackSubjectToChargesGrid)).BeginInit();
			this.FallbackSubjectToChargesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.FallbackSubjectToChargesCollection);
			// 
			// FallbackSubjectToChargesGrid
			// 
			this.FallbackSubjectToChargesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.FallbackSubjectToChargesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.FallbackSubjectToCharges)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.FallbackSubjectToCharges)(null)).RatesProviderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.FallbackSubjectToCharges)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.FallbackSubjectToCharges)(null)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.FallbackSubjectToCharges)(null)).IsFallbackEnabled)));

			columnStyleRatesProviderCode.BindToList = "RatesProviderCodeList";
			columnStyleRatesProviderCode.ColumnName = "RatesProviderCode";
			columnStyleRatesProviderCode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleTransportMode.BindToList = "TransportModeList";
			columnStyleTransportMode.ColumnName = "TransportMode";
			columnStyleTransportMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleContainerMode.BindToList = "ContainerModeList";
			columnStyleContainerMode.ColumnName = "ContainerMode";
			columnStyleContainerMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleIsFallbackEnabled.ColumnName = "IsFallbackEnabled";
			columnStyleIsFallbackEnabled.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.FallbackSubjectToChargesGrid.CaptionVisible = false;
			this.FallbackSubjectToChargesGrid.ColumnStyles.Add(columnStyleRatesProviderCode);
			this.FallbackSubjectToChargesGrid.ColumnStyles.Add(columnStyleTransportMode);
			this.FallbackSubjectToChargesGrid.ColumnStyles.Add(columnStyleContainerMode);
			this.FallbackSubjectToChargesGrid.ColumnStyles.Add(columnStyleIsFallbackEnabled);
			this.FallbackSubjectToChargesGrid.CopySelectedRowsAllowed = true;
			this.FallbackSubjectToChargesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FallbackSubjectToChargesGrid.GridId = "27243ffb-75db-4578-9c18-f6ce3292fa7a";
			this.FallbackSubjectToChargesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.FallbackSubjectToChargesGrid.LayoutKey = "FallbackSubjectToChargesGrid";
			this.FallbackSubjectToChargesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FallbackSubjectToChargesGrid.Name = "FallbackSubjectToChargesGrid";
			this.FallbackSubjectToChargesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 200, true);
			this.FallbackSubjectToChargesGrid.TabIndex = 0;
			// 
			// FallbackSubjectToChargesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FallbackSubjectToChargesGrid);
			this.Name = "FallbackSubjectToChargesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.FallbackSubjectToChargesGrid)).EndInit();
			this.FallbackSubjectToChargesGrid.ResumeLayout(false);
			this.FallbackSubjectToChargesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid FallbackSubjectToChargesGrid;

	}
}
