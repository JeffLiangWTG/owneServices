using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class ShipmentGatePassFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new ZMultiControlColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.ColumnName = "JS_HouseBill";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "JS_GoodsDescription";
			zMultiControlColumnStyleInfo1.Caption = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			zMultiControlColumnStyleInfo1.ColumnName = "ConsignorNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsignorFieldType";
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|b3bd0bcb-9fea-4687-9b5b-d2c24b7369fb", "Consignee");
			zMultiControlColumnStyleInfo2.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ConsigneeFieldType";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|ca248a8a-9416-4a5c-8dc7-3291fe00b831", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JS_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|8281c127-bd24-493f-8646-60880cc55d1b", "Dest.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JS_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|f6d3eb24-21b2-45c9-bf91-eee9f361dbde", "Delivered", "Fully Delivered", "");
			zCheckBoxColumnStyleInfo1.ColumnName = "JS_IsFullyDelivered";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|02be27f4-65e4-4e3b-8de5-0e3398338829", "Containers", "Container Nums.", "");
			zTextBoxColumnStyleInfo4.ColumnName = "JS_Calc_RelatedContainerNums";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentGatePassFilterControl|556c14ad-cfa2-4b17-b312-3ad20eae9ca0", "Status");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_GatePassStatusShort";
			zTextBoxColumnStyleInfo5.IsSortable = false;
			zTextBoxColumnStyleInfo6.ColumnName = "Job+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo1.ColumnName = "Job+JH_TotalProfitRevenueMargin";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 440, true);
			this.FilteredGrid.TabIndex = 15;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.GatePassShipment);
			// 
			// ShipmentGatePassFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ShipmentGatePassFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 616, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
