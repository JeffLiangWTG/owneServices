using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.TransportCommon.GUI.Registry
{
	public partial class PackageVersionOverrideRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.packageVersionOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.packageVersionOverrideGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportCommon.Registry.PackageVersionOverrideCollection);
			// 
			// packageVersionOverrideGrid
			// 
			this.packageVersionOverrideGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.packageVersionOverrideGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.TransportCommon.Registry.PackageVersionOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportCommon.Registry.PackageVersionOverride)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportCommon.Registry.PackageVersionOverride)(null)).AccountNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportCommon.Registry.PackageVersionOverride)(null)).PackageName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportCommon.Registry.PackageVersionOverride)(null)).PackageVersion)));
			this.packageVersionOverrideGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("PackageVersionOverrideRegistryControl|Code", "Carrier Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("PackageVersionOverrideRegistryControl|AccountNumber", "Account Number");
			zTextBoxColumnStyleInfo2.ColumnName = "AccountNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("PackageVersionOverrideRegistryControl|PackageName", "Package Name");
			zTextBoxColumnStyleInfo3.ColumnName = "PackageName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.TransportCommon.GUI.Res.GetData("PackageVersionOverrideRegistryControl|PackageVersion", "Package Version");
			zTextBoxColumnStyleInfo4.ColumnName = "PackageVersion";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.packageVersionOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.packageVersionOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.packageVersionOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.packageVersionOverrideGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.packageVersionOverrideGrid.GridId = "D800AC35-D78F-4F35-9F13-A06F86C84D64";
			this.packageVersionOverrideGrid.CopySelectedRowsAllowed = true;
			this.packageVersionOverrideGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.packageVersionOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.packageVersionOverrideGrid.LayoutKey = "packageVersionOverrideGrid";
			this.packageVersionOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.packageVersionOverrideGrid.Name = "packageVersionOverrideGrid";
			this.packageVersionOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			this.packageVersionOverrideGrid.TabIndex = 0;
			// 
			// PackageVersionOverrideRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.packageVersionOverrideGrid);
			this.Name = "PackageVersionOverrideRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.packageVersionOverrideGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid packageVersionOverrideGrid;
	}
}
