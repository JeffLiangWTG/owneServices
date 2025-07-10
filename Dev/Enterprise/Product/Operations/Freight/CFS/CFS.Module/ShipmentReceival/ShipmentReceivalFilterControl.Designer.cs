using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class ShipmentReceivalFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new ZMultiControlColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			this.TotalsPanel = new ZPanel();
			this.TotalsLabel = new ZLabel();
			((ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TotalsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|3d75d608-0ba4-435f-9d9c-71aadc3ca04e", "Shipment ID");
			zTextBoxColumnStyleInfo1.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|537db895-ef2b-4b24-b49c-5da75bfa5a89", "House Bill");
			zTextBoxColumnStyleInfo2.ColumnName = "JS_HouseBill";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|ff48bf19-d954-4a3f-ab08-6d45829bbde7", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JS_OH_HandledOnBehalfOfForwarder";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|f2735391-9231-4693-9f1f-842dc4a640f5", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JS_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|82c576fb-faf9-4b74-a31a-a9d5196b8338", "Dest.");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JS_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|f2ef9593-d35d-4b93-bddb-ac9109f1498a", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|6bde1be9-728c-4d06-ad00-acf15aa8cdfd", "Weight");
			zCalcEditColumnStyleInfo2.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo2.Decimals = 3;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|e05ec5bf-8483-4492-8209-e65519d91629", "Volume");
			zCalcEditColumnStyleInfo3.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo3.Decimals = 3;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zMultiControlColumnStyleInfo1.Caption = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			zMultiControlColumnStyleInfo1.ColumnName = "ConsignorNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsignorFieldType";
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|5ea31627-4fb6-46f1-b88e-ce8359351332", "Consignee");
			zMultiControlColumnStyleInfo2.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ConsigneeFieldType";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|6ad80443-75a0-46c8-8fd7-470537fde2a5", "Interim Receipt");
			zTextBoxColumnStyleInfo3.ColumnName = "JS_InterimReceipt";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|464528a8-c983-477f-9bf1-f74bb1452f9b", "Vessel Name");
			zTextBoxColumnStyleInfo4.ColumnName = "JS_Calc_CurrentVessel";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|165a9aea-6cf7-4a30-98de-732cf2bdd9cf", "Voyage No");
			zTextBoxColumnStyleInfo5.ColumnName = "JS_Calc_CurrentVoyageFlight";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|c3fb44d8-047b-4d41-a146-c945c05201d2", "Whs. Rec.");
			zDateEditColumnStyleInfo1.ColumnName = "JS_A_RCV";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|9c0fe1f5-4591-4aac-857f-2ec86a953b19", "Client Ref.");
			zTextBoxColumnStyleInfo6.ColumnName = "JS_ConsolReference";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|51e09514-7236-48a4-ab3f-20b5cb44130c", "Con Note");
			zTextBoxColumnStyleInfo7.ColumnName = "JS_CartageWaybill";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|cc1a6055-9352-4162-b8d8-163082ea6520", "Registered Date");
			zDateEditColumnStyleInfo2.ColumnName = "JS_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|5064b7a8-5848-4e14-a4bd-e9dd9dfa5d4b", "Permit Number");
			zTextBoxColumnStyleInfo8.ColumnName = "CustomsEntryNumber";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|0cbce7f7-de01-4563-9530-1f82fd0ed1ef", "Job Status");
			zTextBoxColumnStyleInfo9.ColumnName = "Job+JH_Status";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ShipmentReceivalFilterControl|e1cf1610-2773-4ba8-870b-53f229d9e2fb", "Load List ID", "Comma separated list of related load list job numbers.");
			zTextBoxColumnStyleInfo10.ColumnName = "JS_JK_ConsolID";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "Job+JH_HoldReason";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.ColumnName = "Job+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo4.ColumnName = "Job+JH_TotalProfitRevenueMargin";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 48, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 364, true);
			this.FilteredGrid.TabIndex = 16;
			this.FilteredGrid.KeyDown += new KeyEventHandler(this.FilteredGrid_KeyDown);
			this.FilteredGrid.CurrentCellChanged += new EventHandler(this.FilteredGrid_CurrentCellChanged);
			this.FilteredGrid.Navigate += new NavigateEventHandler(this.FilteredGrid_Navigate);
			this.FilteredGrid.Click += new EventHandler(this.FilteredGrid_Click);
			// 
			// TotalsPanel
			// 
			this.TotalsPanel.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.TotalsPanel.Controls.Add(this.TotalsLabel);
			this.TotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 386, true);
			this.TotalsPanel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.TotalsPanel.Name = "TotalsPanel";
			this.TotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 26, true);
			this.TotalsPanel.TabIndex = 17;
			this.TotalsPanel.Visible = false;
			// 
			// TotalsLabel
			// 
			this.TotalsLabel.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 4, true);
			this.TotalsLabel.Name = "TotalsLabel";
			this.TotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 20, true);
			this.TotalsLabel.TabIndex = 0;
			// 
			// ShipmentReceivalFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TotalsPanel);
			this.Name = "ShipmentReceivalFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(721, 412, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.FilteredGrid, 0);
			this.Controls.SetChildIndex(this.TotalsPanel, 0);
			((ISupportInitialize)(this.FilteredGrid)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.TotalsPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected override void HandleGridSizing()
		{
			if ((FilteredGrid != null) && (TotalsPanel != null))
			{
				var yValue = CargoWise.Windows.UI.ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top);
				FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, yValue);
				ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - (TotalsPanel.Visible ? TotalsPanel.Height : 0), false);
				ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
			}
		}

		void CalcTotals()
		{
			if (TotalsPanel.Visible)
			{
				decimal totalWeight = 0;
				decimal totalVolume = 0;
				int totalPacks = 0;

				foreach (CommonShipment ship in FilteredGrid.SelectedElements)
				{
					totalWeight += ship.JS_ActualWeight;
					totalVolume += ship.JS_ActualVolume;
					totalPacks += ship.JS_OuterPacks;
				}

				TotalsLabel.Text = Res.GetString("dc0ad91f-76d1-4af9-a21a-03cbc9999dd6", "Total Weight: {0:F}; Total Volume: {1:F}; Total Packs: {2}", totalWeight, totalVolume, totalPacks);
			}
		}

		#endregion
	}
}
