using Enterprise.Rating.Business;

namespace Enterprise.Rating.GUI
{
	partial class RatesServiceSettingsControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleTransportMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo columnStyleContainerMode = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo columnStyleIsSubscriptionEnabled = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.RatesServiceSettingsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RatesServiceSettingsGrid)).BeginInit();
			this.RatesServiceSettingsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RatesServiceRegistrySettingsCollection);
			// 
			// RatesServiceSettingsGrid
			// 
			this.RatesServiceSettingsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RatesServiceSettingsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Rating.Business.RatesServiceRegistrySettings)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RatesServiceRegistrySettings)(null)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Rating.Business.RatesServiceRegistrySettings)(null)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Rating.Business.RatesServiceRegistrySettings)(null)).IsSubscriptionEnabled)));

			columnStyleTransportMode.BindToList = "TransportModeList";
			columnStyleTransportMode.ColumnName = "TransportMode";
			columnStyleTransportMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleContainerMode.BindToList = "ContainerModeList";
			columnStyleContainerMode.ColumnName = "ContainerMode";
			columnStyleContainerMode.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			columnStyleIsSubscriptionEnabled.ColumnName = "IsSubscriptionEnabled";
			columnStyleIsSubscriptionEnabled.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			this.RatesServiceSettingsGrid.CaptionVisible = false;
			this.RatesServiceSettingsGrid.ColumnStyles.Add(columnStyleTransportMode);
			this.RatesServiceSettingsGrid.ColumnStyles.Add(columnStyleContainerMode);
			this.RatesServiceSettingsGrid.ColumnStyles.Add(columnStyleIsSubscriptionEnabled);
			this.RatesServiceSettingsGrid.CopySelectedRowsAllowed = true;
			this.RatesServiceSettingsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RatesServiceSettingsGrid.GridId = "450cccd1-a36f-48f6-9a06-d4a436474edd";
			this.RatesServiceSettingsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RatesServiceSettingsGrid.LayoutKey = "RatesServiceSettingsGrid";
			this.RatesServiceSettingsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RatesServiceSettingsGrid.Name = "RatesServiceSettingsGrid";
			this.RatesServiceSettingsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 200, true);
			this.RatesServiceSettingsGrid.TabIndex = 0;
			// 
			// RatesServiceSettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RatesServiceSettingsGrid);
			this.Name = "RatesServiceSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 212, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RatesServiceSettingsGrid)).EndInit();
			this.RatesServiceSettingsGrid.ResumeLayout(false);
			this.RatesServiceSettingsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid RatesServiceSettingsGrid;

	}
}
