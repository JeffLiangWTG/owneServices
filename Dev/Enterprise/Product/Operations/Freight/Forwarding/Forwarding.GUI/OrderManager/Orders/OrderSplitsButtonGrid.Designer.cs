using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrderSplitsButtonGrid : ZModuleButtonGridWithoutColumnStylesSerialisation
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo16 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo17 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo18 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo19 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo20 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo21 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo22 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo23 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo24 = new ZArchitecture.ZDateEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrderSplitsButtonGrid
			// 
			this.CaptionRenderingEnabled = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JD_OrderNumberSplit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "JD_OrderDate";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ColumnName = "JD_BookingConfRef";
			zTextBoxColumnStyleInfo2.ColumnName = "JD_InvoiceNumber";
			zDateEditColumnStyleInfo2.ColumnName = "JD_InvoiceDate";
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JD_ActualWeight";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JD_ActualVolume";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JD_Packs";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "JD_Waybill";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.ColumnName = "JD_Milestone_E_EXW";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo4.ColumnName = "JD_Milestone_A_EXW";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo5.ColumnName = "JD_Milestone_E_DCF";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo6.ColumnName = "JD_Milestone_A_DCF";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo7.ColumnName = "JD_Milestone_E_GIW";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo8.ColumnName = "JD_Milestone_A_GIW";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo9.ColumnName = "JD_Milestone_E_DEP";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo10.ColumnName = "JD_Milestone_A_DEP";
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo11.ColumnName = "JD_Milestone_E_ARV";
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo12.ColumnName = "JD_Milestone_A_ARV";
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo13.ColumnName = "JD_Milestone_E_CCC";
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo14.ColumnName = "JD_Milestone_A_CCC";
			zDateEditColumnStyleInfo14.IsVisible = false;
			zDateEditColumnStyleInfo15.ColumnName = "JD_Milestone_E_CLR";
			zDateEditColumnStyleInfo15.IsVisible = false;
			zDateEditColumnStyleInfo16.ColumnName = "JD_Milestone_A_CLR";
			zDateEditColumnStyleInfo16.IsVisible = false;
			zDateEditColumnStyleInfo17.ColumnName = "JD_Milestone_E_CAV";
			zDateEditColumnStyleInfo17.IsVisible = false;
			zDateEditColumnStyleInfo18.ColumnName = "JD_Milestone_A_CAV";
			zDateEditColumnStyleInfo18.IsVisible = false;
			zDateEditColumnStyleInfo19.ColumnName = "JD_Milestone_E_DCA";
			zDateEditColumnStyleInfo19.IsVisible = false;
			zDateEditColumnStyleInfo20.ColumnName = "JD_Milestone_A_DCA";
			zDateEditColumnStyleInfo20.IsVisible = false;
			zDateEditColumnStyleInfo21.ColumnName = "JD_EstimateUserDate1";
			zDateEditColumnStyleInfo21.IsVisible = false;
			zDateEditColumnStyleInfo22.ColumnName = "JD_EstimateUserDate2";
			zDateEditColumnStyleInfo22.IsVisible = false;
			zDateEditColumnStyleInfo23.ColumnName = "JD_ActualUserDate1";
			zDateEditColumnStyleInfo23.IsVisible = false;
			zDateEditColumnStyleInfo24.ColumnName = "JD_ActualUserDate2";
			zDateEditColumnStyleInfo24.IsVisible = false;
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo17);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo18);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo19);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo20);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo21);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo22);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo23);
			this.ColumnStyles.Add(zDateEditColumnStyleInfo24);
			this.Name = "OrderSplitsButtonGrid";
			this.NameOfAGridElement = Enterprise.Freight.Forwarding.GUI.Res.GetData("F8F5A5AE-3E7B-4F94-B440-AAD6F18F016F", "Order Split");
			this.ShowAttachButton = false;
			this.ShowDetachButton = false;
			this.ShowEditButton = false;
			this.ShowNewButton = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 352, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
