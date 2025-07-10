using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Shared.Module
{
	partial class RefPacksFilterControl
	{
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.BaseRefPacks)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((MasterFiles.Business.BaseRefPacks)(null)).RP_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.BaseRefPacks)(null)).RP_OH_Supplier_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((MasterFiles.Business.BaseRefPacks)(null)).RP_ConversionFactor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.BaseRefPacks)(null)).RP_CustomsPack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.BaseRefPacks)(null)).RP_CustomsPack_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.BaseRefPacks)(null)).RP_CommercialPack)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MasterFiles.Business.BaseRefPacks)(null)).RP_CommercialPack_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MasterFiles.Business.BaseRefPacks)(null)).RP_CustomsCountry)));
			zGuidFindBoxColumnStyleInfo1.BindToList = "RP_OH_Supplier_List";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RP_OH_Supplier";
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zGuidFindBoxColumnStyleInfo1.ToolTip = "This mapping is specific for this supplier. If a supplier is blank, this mapping " +
	"is general.";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCalcEditColumnStyleInfo1.ColumnName = "RP_ConversionFactor";
			zCalcEditColumnStyleInfo1.Decimals = 9;
			zCalcEditColumnStyleInfo1.ToolTip = "Conversion factor from customs pack unit to commercial pack unit. Eg 2 in 2 BC (C" +
	"ustoms Carton) = 1 CTN (Commercial Carton)";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "RP_CustomsPack_List";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("RefPacksFilterControl|609F6551-A609-4da7-BB49-F70CCDB11B6C", "Customs Pack", "Customs Pack Unit", "");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "RP_CustomsPack";
			zDropEditColumnStyleInfo1.ToolTip = "Unit of package accepted by the Customs that needs to be mapped to a commercial p" +
	"ackage unit. Eg BC in 2 CT(Commercial Carton) = 1 BC(Customs Carton)";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.BindToList = "RP_CommercialPack_List";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("RefPacksFilterControl|85BB450B-ACD1-439f-B351-9B7C94E7E69B", "Commercial Pack", "Commercial Pack Unit", "");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "RP_CommercialPack";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.RefPacks;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Commercial unit of quantity. Eg CT in 2 CT(Commercial Carton) = 1 BC(Customs Basi" +
	"c Carton)";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("cafc47f0-4588-4257-845d-e859a83677ee", "Country");
			zTextBoxColumnStyleInfo1.ColumnName = "RP_CustomsCountry";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.Module.Res.GetData("324ca51a-c2a3-448f-a359-2f0177f391e8", "Pack Conversion Type");
			zTextBoxColumnStyleInfo2.ColumnName = "RP_Type";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 320, true);
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(574, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MasterFiles.Business.BaseRefPacks);
			// 
			// RefPacksFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "RefPacksFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 472, true);
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
