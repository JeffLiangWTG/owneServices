using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Integration.Freight;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	partial class RealTimeRoutingFilterControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfoCO2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfoCO2Total = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();

			dayOfWeekColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			dayOfWeekColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RoutingLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.WeeklyTimetableRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SingleDayRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RoutingLinesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|40500184-14ad-41b2-bf64-8125834b434f", "Origin");
			zTextBoxColumnStyleInfo1.ColumnName = "Origin";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|570e091e-52e9-4115-b6a8-0dcdf07499a3", "Origin Port");
			zTextBoxColumnStyleInfo2.ColumnName = "OriginDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|09f58a2e-b1af-4ae1-acc8-f8c10a2128c3", "Dest.");
			zTextBoxColumnStyleInfo3.ColumnName = "Destination";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|e8696d8a-5a7c-41a4-b4e2-7dc18b9c9fff", "Dest. Port Name", "Destination Port Name.");
			zTextBoxColumnStyleInfo4.ColumnName = "DestinationDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|3fcde195-d71e-41d1-a700-de74d3643d0d", "Cnct.");
			zCalcEditColumnStyleInfo1.ColumnName = "Connections";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|42e7367d-1964-4055-95aa-2c9905d77b46", "Stops");
			zCalcEditColumnStyleInfo2.ColumnName = "Stops";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|d722767c-317c-4929-b4c2-31dd5570e79b", "Departure");
			zTextBoxColumnStyleInfo5.ColumnName = "DepartureDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|38e0e12b-bdc2-47fe-9d9a-d50195782108", "Arrival");
			zTextBoxColumnStyleInfo6.ColumnName = "ArrivalDescription";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|1dbd2d9b-31bd-4634-9262-6def972e2877", "Duration");
			zTextBoxColumnStyleInfo7.ColumnName = "Duration";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|c64da81e-7ab8-44d3-9080-f2d50ef78d04", "Carrier 1");
			zTextBoxColumnStyleInfo8.ColumnName = "Carrier1";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|791a9f87-a9d2-4399-9c2c-48f43c15204a", "Carrier 2");
			zTextBoxColumnStyleInfo9.ColumnName = "Carrier2";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|5bdb67bf-378b-4b80-bf33-de2fccabcd96", "Carrier 3");
			zTextBoxColumnStyleInfo10.ColumnName = "Carrier3";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|5ae49a45-d30b-424b-8736-d9c257b6942b", "Carrier 4");
			zTextBoxColumnStyleInfo11.ColumnName = "Carrier4";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo23.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|e9dae42e-28e5-4edd-b25b-ade99f4ccf59", "Flight");
			zTextBoxColumnStyleInfo23.ColumnName = "Flight";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo24.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|f51fb4b3-3648-49bf-b20a-8f7723331120", "Flight Type");
			zTextBoxColumnStyleInfo24.ColumnName = "FlightType";
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo25.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|7be8f3fb-696b-4f94-b086-fd54505fe27b", "Aircraft");
			zTextBoxColumnStyleInfo25.ColumnName = "Aircraft";
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			dayOfWeekColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|fc2c7c18-409c-42e2-a3c9-0afb5861e043", "Days of the Week");
			dayOfWeekColumnStyleInfo1.ColumnName = "OperationDay";
			dayOfWeekColumnStyleInfo1.IsVisible = true;
			dayOfWeekColumnStyleInfo1.IsMandatory = true;
			dayOfWeekColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo29.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|143587e5-8c0f-47aa-b6c8-a703dd0dde26", "Via");
			zTextBoxColumnStyleInfo29.ColumnName = "Via";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfoCO2Total.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|e920b3a6-7760-4ab2-b6e6-5cee3f9db423", "CO2e (kg/t)");
			zCalcEditColumnStyleInfoCO2Total.ColumnName = "CO2EmissionTotal";
			zCalcEditColumnStyleInfoCO2Total.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfoCO2Total.ShowEmptyStringForEmptyValue = true;

			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(dayOfWeekColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled && FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.Value)
			{
				this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfoCO2Total);
			}

			this.FilteredGrid.IsWholeRowSelectedOnClick = true;
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 61, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 377, true);
			this.FilteredGrid.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager);
			// 
			// RoutingLinesGrid
			// 
			this.RoutingLinesGrid.AllowNavigation = false;
			this.RoutingLinesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.RoutingLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|8f233c0d-29ff-4dbd-8a21-70b8bb2ad5fd", "Origin");
			zTextBoxColumnStyleInfo12.ColumnName = "Origin";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|040e86c7-a1dd-4255-9877-9df38815b221", "Term", "Terminal", "The airport terminal at origin where this flight is loaded.");
			zTextBoxColumnStyleInfo13.ColumnName = "OriginTerminal";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|b94186d0-69c2-42ef-a33e-f1953a74212b", "Origin Port Name");
			zTextBoxColumnStyleInfo14.ColumnName = "OriginDescription";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|3975fd09-86d3-4b76-b252-8b02a0611137", "Dest.");
			zTextBoxColumnStyleInfo15.ColumnName = "Destination";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|c40f126f-1512-467a-b4ae-bdee8fe0ccdc", "Term", "Terminal", "The airport terminal at destination where this flight is unloaded.");
			zTextBoxColumnStyleInfo16.ColumnName = "DestinationTerminal";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|1fd69094-7679-4b07-b01e-475136c697e7", "Dest. Port Name");
			zTextBoxColumnStyleInfo17.ColumnName = "DestinationDescription";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo18.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|36bbcd2c-5359-4032-9f32-6146f96ce005", "Carrier", "Carrier", "Carrier", "");
			zTextBoxColumnStyleInfo18.ColumnName = "TicketingCarrier";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo19.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|5db987a5-61bf-4db0-84f5-2077d69f8e35", "Flight");
			zTextBoxColumnStyleInfo19.ColumnName = "FlightNumber";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo20.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|f39a8823-d3b7-4f70-ad67-ad62cd53534f", "Departure");
			zTextBoxColumnStyleInfo20.ColumnName = "DepartureDescription";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo21.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|2b123046-41cf-429b-b4dc-90375e0b8fd9", "Arrival");
			zTextBoxColumnStyleInfo21.ColumnName = "ArrivalDescription";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|d5e2e16b-229e-4b20-8cc0-11a8e5cd528b", "Stops");
			zCalcEditColumnStyleInfo3.ColumnName = "Stops";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo22.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|8cc281b0-da62-49a2-9c03-a5fb36c8d3df", "Aircraft");
			zTextBoxColumnStyleInfo22.ColumnName = "Aircraft";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|26249f20-7b2a-43d6-b689-c6e485377ebf", "Distance (Mi)");
			zCalcEditColumnStyleInfo4.ColumnName = "MilesDistance";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			dayOfWeekColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|aa6142ab-dff4-4ae6-a0f2-17b197ca25a2", "Days of the Week");
			dayOfWeekColumnStyleInfo2.ColumnName = "OperationDay";
			dayOfWeekColumnStyleInfo2.IsMandatory = true;
			dayOfWeekColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo27.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|ea3dd81e-5b28-40f7-a572-c92642d70ed6", "Flight Type");
			zTextBoxColumnStyleInfo27.ColumnName = "FlightType";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|ea82da61-ded0-47d9-8673-dc7fc07ed932", "Effective Date");
			zDateEditColumnStyleInfo1.ColumnName = "EffectiveDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo1, 70, true);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|dfc32010-b7f5-407d-a02d-113480f8dcb2", "Discontinued Date");
			zDateEditColumnStyleInfo2.ColumnName = "DiscontinuedDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref zDateEditColumnStyleInfo2, 70, true);
			zCalcEditColumnStyleInfoCO2.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|b443d89c-98f8-4fd5-9d58-3cb5424d8727", "CO2e (kg/t)");
			zCalcEditColumnStyleInfoCO2.ColumnName = "CO2Emission";
			zCalcEditColumnStyleInfoCO2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfoCO2.ShowEmptyStringForEmptyValue = true;

			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.RoutingLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.RoutingLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RoutingLinesGrid.ColumnStyles.Add(dayOfWeekColumnStyleInfo2);
			this.RoutingLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.RoutingLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RoutingLinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled && FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.Value)
			{
				this.RoutingLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfoCO2);
			}

			this.RoutingLinesGrid.GridId = "151820a6-96b3-4413-ad5c-624eafec2999";
			this.RoutingLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RoutingLinesGrid.IsWholeRowSelectedOnClick = true;
			this.RoutingLinesGrid.LayoutKey = "RoutingLinesGrid";
			this.RoutingLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 254, true);
			this.RoutingLinesGrid.Name = "RoutingLinesGrid";
			this.RoutingLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(787, 181, true);
			this.RoutingLinesGrid.TabIndex = 13;
			//
			//WeeklyTimetableRadioButton
			//
			this.BindingSource.SetBindingMember(this.WeeklyTimetableRadioButton, "IncludeWeeklyTimetable"); 
			this.WeeklyTimetableRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingForm|2db6baf9-2bae-4ead-41de-0b25849873f2", "Weekly Timetable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compithis.WeeklyTimetableRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 3, true);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager)(null)).IncludeWeeklyTimetable)));
			this.WeeklyTimetableRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 3, true);
			this.WeeklyTimetableRadioButton.Name = "WeeklyTimetableRadioButton";
			this.WeeklyTimetableRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.WeeklyTimetableRadioButton.TabIndex = 1;
			//
			//SingleDayRadioButton
			//
			//this.BindingSource.SetBindingMember(this.SingleDayRadioButton, "ExcludeWeeklyTimetableForBinding");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager)(null)).ExcludeWeeklyTimetableForBinding)));
			this.BindingSource.SetBindingMember(this.SingleDayRadioButton, "ExcludeWeeklyTimetableForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager)(null)).ExcludeWeeklyTimetableForBinding)));
			this.SingleDayRadioButton.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingForm|8c6b9947-f6ce-b196-42d8-8a2767f88607", "Single Day");
			this.SingleDayRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 23, true);
			this.SingleDayRadioButton.Name = "SingleDayRadioButton";
			this.SingleDayRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 17, true);
			this.SingleDayRadioButton.TabIndex = 2;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingFilterControl|63f3b633-8284-4973-b002-7410f89df30b", "Connections");
			this.zLabel1.IsFontBold = true;
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 240, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 13, true);
			this.zLabel1.TabIndex = 14;
			// 
			// RealTimeRoutingFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.RoutingLinesGrid);
			this.Controls.Add(this.WeeklyTimetableRadioButton);
			this.Controls.Add(this.SingleDayRadioButton);
			this.Name = "RealTimeRoutingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(789, 438, true);
			this.Controls.SetChildIndex(this.FilterStripsPanel, 0);
			this.Controls.SetChildIndex(this.AddStripButton, 0);
			this.Controls.SetChildIndex(this.ToolStripRecordsFoundLabel, 0);
			this.Controls.SetChildIndex(this.FilteredGrid, 0);
			this.Controls.SetChildIndex(this.WeeklyTimetableRadioButton, 0);
			this.Controls.SetChildIndex(this.SingleDayRadioButton, 0);
			this.Controls.SetChildIndex(this.RoutingLinesGrid, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RoutingLinesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.ZGrid RoutingLinesGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZRadioButton WeeklyTimetableRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton SingleDayRadioButton;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo dayOfWeekColumnStyleInfo1;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo dayOfWeekColumnStyleInfo2;
	}
}
