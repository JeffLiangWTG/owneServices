using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	partial class SameChargeCodeDifferentProviderControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleJobType = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleDirection = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo columnStyleIsEnabled = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.SameChargeCodeDifferentProviderGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SameChargeCodeDifferentProviderGrid)).BeginInit();
			this.SameChargeCodeDifferentProviderGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.SameChargeCodeDifferentProviderCollection);
			//
			// SameChargeCodeDifferentProviderGrid
			//
			this.SameChargeCodeDifferentProviderGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SameChargeCodeDifferentProviderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.SameChargeCodeDifferentProvider)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SameChargeCodeDifferentProvider)(null)).JobType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SameChargeCodeDifferentProvider)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.SameChargeCodeDifferentProvider)(null)).Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.SameChargeCodeDifferentProvider)(null)).IsEnabled)));

			columnStyleJobType.BindToList = "JobTypeList";
			columnStyleJobType.ColumnName = "JobType";
			columnStyleJobType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleTransportMode.BindToList = "TransportModeList";
			columnStyleTransportMode.ColumnName = "TransportMode";
			columnStyleTransportMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleDirection.BindToList = "DirectionList";
			columnStyleDirection.ColumnName = "Direction";
			columnStyleDirection.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleIsEnabled.ColumnName = "IsEnabled";
			columnStyleIsEnabled.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.SameChargeCodeDifferentProviderGrid.CaptionVisible = false;
			this.SameChargeCodeDifferentProviderGrid.ColumnStyles.Add(columnStyleJobType);
			this.SameChargeCodeDifferentProviderGrid.ColumnStyles.Add(columnStyleTransportMode);
			this.SameChargeCodeDifferentProviderGrid.ColumnStyles.Add(columnStyleDirection);
			this.SameChargeCodeDifferentProviderGrid.ColumnStyles.Add(columnStyleIsEnabled);
			this.SameChargeCodeDifferentProviderGrid.CopySelectedRowsAllowed = true;
			this.SameChargeCodeDifferentProviderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SameChargeCodeDifferentProviderGrid.GridId = "27243ffb-75db-4578-9c18-f6ce3292fa7a";
			this.SameChargeCodeDifferentProviderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SameChargeCodeDifferentProviderGrid.LayoutKey = "SameChargeCodeDifferentProviderGrid";
			this.SameChargeCodeDifferentProviderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SameChargeCodeDifferentProviderGrid.Name = "SameChargeCodeDifferentProviderGrid";
			this.SameChargeCodeDifferentProviderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 200, true);
			this.SameChargeCodeDifferentProviderGrid.TabIndex = 0;
			//
			// SameChargeCodeDifferentProviderControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SameChargeCodeDifferentProviderGrid);
			this.Name = "SameChargeCodeDifferentProviderControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SameChargeCodeDifferentProviderGrid)).EndInit();
			this.SameChargeCodeDifferentProviderGrid.ResumeLayout(false);
			this.SameChargeCodeDifferentProviderGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid SameChargeCodeDifferentProviderGrid;

	}
}
