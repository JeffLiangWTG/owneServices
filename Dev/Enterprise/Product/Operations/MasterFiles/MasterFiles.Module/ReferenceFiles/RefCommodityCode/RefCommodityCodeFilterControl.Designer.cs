using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Definitions;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.Module
{
	public partial class RefCommodityCodeFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|c2ed5765-a953-4141-99d7-6f099e80c6f3", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "RH_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|79642c7c-c27c-45be-a2ee-245b771ad808", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "RH_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|75642c7c-c21c-45be-a2be-245b771ad909", "IATA Commodity Code");
			zTextBoxColumnStyleInfo3.ColumnName = "RH_IATACommodityItem";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|72652c8a-c27c-35be-a2bb-246b772ad211", "IATA Commodity Description");
			zTextBoxColumnStyleInfo4.ColumnName = "RefIATACommodityCode.RAC_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|81eb8b6e-79ca-452f-907a-b3cb8396b182", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "RH_IsActive";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo10.IsVisible = true;
			zCheckBoxColumnStyleInfo10.ColumnName = "RH_IsSystem";
			zCheckBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|89D24A9F-5E60-448A-B9C7-CD320A5E576E", "Is System");
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|90e4c06a-44ea-4bc8-b23a-d865d13ca3f4", "Is Flammable");
			zCheckBoxColumnStyleInfo2.ColumnName = "RH_IsFlammable";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|f872d60c-b4da-4a14-a7a0-12ee219bed69", "Is Hazardous");
			zCheckBoxColumnStyleInfo3.ColumnName = "RH_IsHazardous";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|8e5f9ec1-6766-4f34-af2c-0f7a331d333a", "Is Perishable");
			zCheckBoxColumnStyleInfo4.ColumnName = "RH_IsPerishable";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|c8b163d9-e7d6-439f-8853-785088f07929", "Is Timber");
			zCheckBoxColumnStyleInfo5.ColumnName = "RH_IsTimber";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|BEA732C4-D2BF-48E1-AD6D-D275DD535C7F", "Is Forwarding");
			zCheckBoxColumnStyleInfo6.ColumnName = "RH_IsForwarding";
			zCheckBoxColumnStyleInfo6.IsVisible = true;
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|A707098D-6382-4E4A-8E22-A1D4C13AED94", "Is Shipping");
			zCheckBoxColumnStyleInfo7.ColumnName = "RH_IsShipping";
			zCheckBoxColumnStyleInfo7.IsVisible = true;
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|F4BC9386-4E67-4577-8C38-077FCCDF9B43", "Is Land Transport");
			zCheckBoxColumnStyleInfo8.ColumnName = "RH_IsLandTransport";
			zCheckBoxColumnStyleInfo8.IsVisible = true;
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|2ADEABAC-5C95-4955-BE9B-FC62BECA3C30", "Is Personal Effects");
			zCheckBoxColumnStyleInfo9.ColumnName = "RH_IsPersonalEffects";
			zCheckBoxColumnStyleInfo9.IsVisible = true;
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|482082A9-D268-4664-B15C-1DE009F2A847", "NMFC");
			zTextBoxColumnStyleInfo11.ColumnName = "RH_FN_NKNMFC";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|B17F235D-620F-4A8E-9F9C-7BB568B96979", "Universal Group");
			zTextBoxColumnStyleInfo12.ColumnName = "RH_UniversalCommodityGroup";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|6F2C5A0C-C89A-4E23-B117-87562BDE6788", "Expiry Date");
			zTextBoxColumnStyleInfo13.ColumnName = "RH_ExpiryDate";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|EFEBF6D4-3D82-45F6-A1A7-60C8E6197E99", "Reefer Min. Temp.");
			zCalcEditColumnStyleInfo14.ColumnName = "RH_ReeferMinTemperature";
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|82DC5D05-305B-4445-ACFC-57E619F50507", "Reefer Max. Temp.");
			zCalcEditColumnStyleInfo15.ColumnName = "RH_ReeferMaxTemperature";
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCheckBoxColumnStyleInfo16.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|E07D4EFA-9168-4ACE-8989-B41F77968C94", "Is Ctn Vent Req.", "Is Container Vent Required", "");
			zCheckBoxColumnStyleInfo16.ColumnName = "RH_ContainerVentRequired";
			zCheckBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|90E2C86C-2C3A-4DD1-B24C-D0C3DFA26B5C", "Local Codes");
			zTextBoxColumnStyleInfo17.ColumnName = "LocalCodesAsString";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("RefCommodityCodeFilterControl|D65D26BD-D3E3-4DFF-BF7F-1296AB967A54", "Rating Codes");
			zTextBoxColumnStyleInfo18.ColumnName = "RatingCodesAsString";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Clear();
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo16);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			if (ReferenceFilesDataRegistry.Instance.ShowPersonalEffects.Value)
			{
				this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			}
			var countries = new HashSet<string> { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Canada, Core.Constants.CountryCodes.Mexico };
			if (countries.Contains(Env.CurrentCompany.Country.Code))
			{
				this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			}
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 296, true);
			this.FilteredGrid.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCommodityCode);
			// 
			// RefCommodityCodeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RefCommodityCodeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(749, 448, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
