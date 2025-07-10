using Enterprise.ZArchitecture;

namespace Enterprise.Rating.Module
{
	public partial class RateTransportProviderFilterControl
	{
		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo countryColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zoneHubColumnStyleInfo = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zoneOwnerCodeColumnStyleInfo = new ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateTransportProvider)(null)).RelatedPartyFullName);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateTransportProvider)(null)).TP_ZoneType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateTransportProvider)(null)).CountryCode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateTransportProvider)(null)).ZoneHubLocation.R9_InternationalName);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateTransportProviderFilterControl|236694f8-939a-4765-ad28-9f833775aae3", "Zone Owner");
			zTextBoxColumnStyleInfo1.ColumnName = "RelatedPartyFullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(310);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("57640A84-B9F7-4C7E-9DC9-64F7D5063A7B", "Zone Type");
			zTextBoxColumnStyleInfo3.ColumnName = "TP_ZoneType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			countryColumnStyleInfo.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("d703bc71-f834-4cd4-b3cc-570e6e3108a5", "Country/Region");
			countryColumnStyleInfo.ColumnName = "CountryCode";
			countryColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zoneHubColumnStyleInfo.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("56a8546b-0fb9-4779-9075-78e6eb5f739c", "Zone Hub");
			zoneHubColumnStyleInfo.ColumnName = "ZoneHubLocation+R9_InternationalName";
			zoneHubColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zoneOwnerCodeColumnStyleInfo.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("f58d35f2-739f-4e54-bdd2-ff071c5bab4a", "Zone Owner Code", "Zone Owner Code", "");
			zoneOwnerCodeColumnStyleInfo.ColumnName = "RelatedPartyCode";
			zoneOwnerCodeColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(countryColumnStyleInfo);
			this.grid.ColumnStyles.Add(zoneHubColumnStyleInfo);
			this.grid.ColumnStyles.Add(zoneOwnerCodeColumnStyleInfo);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 414, true);
			this.grid.TabIndex = 7;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RateTransportProvider);
			// 
			// RateTransportProviderFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RateTransportProviderFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
