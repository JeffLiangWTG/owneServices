using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class NatureAndQtyOfGoodsOriginControl
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

		private void InitializeComponent()
		{
			this.countryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.AWB.Business.ExportAWBRateLine);
			// 
			// textCodeFindBox
			// 
			this.countryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryCodeFindBox, "NatureAndQtyOfGoodsOrigin.Country");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.AWB.ExportAWBRateLine)(null)).NatureAndQtyOfGoodsOrigin.Country)));
			this.countryCodeFindBox.CaptionResourceString = Res.GetData("cbe6dedb-7afe-41d9-b5f5-c50afe144a1f", "Country/Region of Origin of Goods");
			this.countryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.countryCodeFindBox.MaxLength = 2;
			this.countryCodeFindBox.Name = "countryCodeFindBox";
			this.countryCodeFindBox.PreBoundMaxLength = 2;
			this.countryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.countryCodeFindBox.TabIndex = 0;
			// 
			// NatureAndQtyOfGoodsTextControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.countryCodeFindBox);
			this.Name = "NatureAndQtyOfGoodsOriginControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZCodeFindBox countryCodeFindBox;
	}
}
