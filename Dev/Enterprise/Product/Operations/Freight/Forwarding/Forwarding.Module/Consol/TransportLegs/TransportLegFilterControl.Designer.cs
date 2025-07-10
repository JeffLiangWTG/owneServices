using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class TransportLegFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
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
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new ZMultiLineTextBoxColumnInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo routeNumberColumn = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|7f49f16e-2fce-4923-a73c-d806adc592c9", "Defined By", "The reference number of the parent of this leg.\r\nIf the leg is defined by a consol then this is the consol number.\r\nIf the leg is defined by a shipment then it is the shipments reference number.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JW_ParentDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|9a24eb9b-5c78-40d0-9f22-988bee039ffc", "Is Linked");
			zCheckBoxColumnStyleInfo1.ColumnName = "JW_IsLinked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|7aba0a47-51d5-425f-9fc4-6baa12c2c5b4", "Published", "Published", "Published", "");
			zCheckBoxColumnStyleInfo2.ColumnName = "JW_JX_IsPublished";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo3.ColumnName = "JW_IsCharter";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JW_LegOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.ColumnName = "JW_TransportMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(37);
			zDropEditColumnStyleInfo2.ColumnName = "JW_TransportType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo3.ColumnName = "JW_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(42);
			zDropEditColumnStyleInfo4.ColumnName = "JW_PL_NKCarrierServiceLevel";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zMultiControlColumnStyleInfo1.ColumnName = "JW_Vessel";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "JW_VesselFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JW_VoyageFlight";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|ceac1158-0be1-4438-a1f1-24c789923099", "Voyage / Flight / Truck Ref. / Journey No.");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JW_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JW_RL_NKDiscPort";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|7d3bb0f6-cfe0-4aad-a09d-352be84b4258", "Is Domestic");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsDomestic";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zDateEditColumnStyleInfo1.ColumnName = "JW_ETD";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.ColumnName = "JW_ETA";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.ColumnName = "JW_ATD";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.ColumnName = "JW_ATA";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|f008f8c1-07a6-4e97-aa45-002ab67233bc", "CTO Available");
			zDateEditColumnStyleInfo5.ColumnName = "JW_TerminalAvailabilityDate";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(112);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|54f0cd51-ad5a-4106-85a6-f970b4296062", "Docs Due", "Docs Due", "The documentation cut off date.");
			zDateEditColumnStyleInfo6.ColumnName = "JW_DocumentaryCutOff";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|b57d76e5-faf7-4797-bf81-2d9e7ea528b3", "CTO Cut Off");
			zDateEditColumnStyleInfo7.ColumnName = "JW_TerminalCutOff";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|b7ea2bbe-fe03-438a-969c-7efa888ee06a", "CTO Receival Start");
			zDateEditColumnStyleInfo8.ColumnName = "JW_TerminalReceivalCommences";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|f9fb3847-02b8-4567-9417-1a92f7525184", "CFS Available");
			zDateEditColumnStyleInfo9.ColumnName = "JW_DepotAvailabilityDate";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|4c09b4ce-aed2-419d-8313-290c3db5f0c8", "CFS Cut Off");
			zDateEditColumnStyleInfo10.ColumnName = "JW_DepotCutOff";
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|10731782-9c61-4c5b-94ae-1dd0cea94af2", "CFS Receival Start");
			zDateEditColumnStyleInfo11.ColumnName = "JW_DepotReceivalCommences";
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|a6c43850-c824-4e6a-a22f-0bfa4da67a59", "CFS Storage Start");
			zDateEditColumnStyleInfo12.ColumnName = "JW_DepotStorageDate";
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|5fc1ab21-aa3f-4627-9720-4ba5bfbcf6d4", "CTO Storage Start");
			zDateEditColumnStyleInfo13.ColumnName = "JW_TerminalStorageDate";
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zDateEditColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|e9c392fc-3fbb-4615-8530-3a5679c68baa", "Load Port ETA");
			zDateEditColumnStyleInfo14.ColumnName = "JW_JX_Load_ETA";
			zDateEditColumnStyleInfo14.IsVisible = false;
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|9f699db6-12db-42bf-b2a0-448608225173", "Load Port ATA");
			zDateEditColumnStyleInfo15.ColumnName = "JW_JX_Load_ATA";
			zDateEditColumnStyleInfo15.IsVisible = false;
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|2ecb6888-ea98-4868-b557-b7e1d3338c7b", "VGM Cut Off");
			zDateEditColumnStyleInfo16.ColumnName = "JW_VGMCutOff";
			zDateEditColumnStyleInfo16.IsVisible = false;
			zDateEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CarrierPK";
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CreditorPK";
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "JW_CarrierBookingReference";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|84330440-650e-4c5b-810f-3d30e6c5e040", "Aircraft Reg.", "Aircraft Registration Number", "The aircraft registration number for a chartered flight.");
			zTextBoxColumnStyleInfo4.ColumnName = "JW_JX_JV_RegistrationNo";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "JW_LegNotes";
			zMultiLineTextBoxColumnInfo1.IsVisible = false;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|ad017760-8a5c-4763-bc3d-7bb1d0a17989", "Distance");
			zCalcEditColumnStyleInfo2.ColumnName = "JW_Distance";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|f29ae480-c373-4dcc-890d-328a72cf1ad8", "Distance Unit");
			zDropEditColumnStyleInfo5.ColumnName = "JW_DistanceUnit";
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|780afa8f-a866-4f93-b293-4d4936cccdea", "Is Cargo Only");
			zCheckBoxColumnStyleInfo5.ColumnName = "JW_IsCargoOnly";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|e5a82193-91fe-4c83-ac62-8d1a67cb1b61", "Voyage Type");
			zTextBoxColumnStyleInfo5.ColumnName = "JW_JX_JV_VoyageType";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			routeNumberColumn.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|a752ec27-d8e3-4507-a0a7-9d23d95a08cb", "Route Set Number");
			routeNumberColumn.ColumnName = "RouteSetNumber";
			routeNumberColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			routeNumberColumn.IsVisible = false;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("TransportLegFilterControl|0F3FEFF9-5B08-487B-88AA-0343242A8AC5", "Flight Status");
			zTextBoxColumnStyleInfo6.ColumnName = "OnlineScheduleStatusDescription";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(routeNumberColumn);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 501, true);
			this.grid.TabIndex = 28;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Freight.Business.TransportNonDependent);
			// 
			// TransportLegFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "TransportLegFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 504, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
