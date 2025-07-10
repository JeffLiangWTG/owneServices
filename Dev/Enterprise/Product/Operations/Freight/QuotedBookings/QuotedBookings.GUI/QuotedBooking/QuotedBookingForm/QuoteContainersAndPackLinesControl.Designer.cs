using System;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuoteContainersAndPackLinesControl : ZUserControl
	{
		ZGroupBox LooseCargoGroupBox;
		internal ZGrid LooseCargoGrid;
		ZGroupBox QuoteContainersGroupBox;
		MasterFiles.GUI.NumbersControl quoteNumbersControl;
		ZGrid ContainersGrid;

		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6;
		ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9;
		ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4;
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7;
		ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1;

		void InitializeComponent()
		{
			zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			this.LooseCargoGroupBox = new ZGroupBox();
			this.LooseCargoGrid = new ZGrid();
			this.QuoteContainersGroupBox = new ZGroupBox();
			this.ContainersGrid = new ZGrid();
			this.quoteNumbersControl = new MasterFiles.GUI.NumbersControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LooseCargoGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LooseCargoGrid)).BeginInit();
			this.LooseCargoGrid.SuspendLayout();
			this.QuoteContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.quoteNumbersControl.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(QuotedBooking);
			//
			// LooseCargoGroupBox
			//
			this.LooseCargoGroupBox.CaptionResourceString = Res.GetData("QuoteContainersAndPackLinesControl|c4abcd08-b1a0-4ca4-9699-fbb63d101d1a", "Loose Cargo");
			this.LooseCargoGroupBox.Controls.Add(this.LooseCargoGrid);
			this.LooseCargoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LooseCargoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 0, true);
			this.LooseCargoGroupBox.Name = "LooseCargoGroupBox";
			this.LooseCargoGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.LooseCargoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 150, true);
			this.LooseCargoGroupBox.TabIndex = 51;
			this.LooseCargoGroupBox.TabStop = false;
			//
			// LooseCargoGrid
			//
			this.LooseCargoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LooseCargoGrid, "Quote+CurrentOneOffQuote+LooseCargo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_PackLineCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_F3_NKPackType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_Weight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_WeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_Volume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_VolumeUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_Length);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).LengthImperial);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_Width);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).WidthImperial);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_Height);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).HeightImperial);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).TPL_DimensionUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffPackLine)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.LooseCargo)).SyncRoot)).LooseCargoContainerType);
			this.LooseCargoGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TPL_PackLineCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|70e97bc2-2896-4631-a60d-8c60f4d4a9f4", "Packages");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.ColumnName = "TPL_F3_NKPackType";
			zDropEditColumnStyleInfo1.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|70e97bc2-2896-4631-a60d-8c60f4d4a9f4", "Packages");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "TPL_Weight";
			zCalcEditColumnStyleInfo2.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|72b8f005-1099-49d3-839c-4125ca3f6d47", "Weight");
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.ColumnName = "TPL_WeightUQ";
			zDropEditColumnStyleInfo2.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|72b8f005-1099-49d3-839c-4125ca3f6d47", "Weight");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TPL_Volume";
			zCalcEditColumnStyleInfo3.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|4cfc8929-3138-4e5f-82c1-58b82f8ba8a0", "Volume");
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo3.ColumnName = "TPL_VolumeUQ";
			zDropEditColumnStyleInfo3.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|4cfc8929-3138-4e5f-82c1-58b82f8ba8a0", "Volume");
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TPL_Length";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|070500e0-e8c0-4fab-af97-c55e67fca5a6", "Dimensions");
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("QuoteContainersAndPackLinesControl|34fc712f-40be-4706-976f-ce8305dea86f", "Length (ft/in)", "The Length in feet and inches.");
			zTextBoxColumnStyleInfo1.ColumnName = "LengthImperial";
			zTextBoxColumnStyleInfo1.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|fc1b2165-39ca-4c76-9982-a69cb94eeaf5", "Imperial Dimensions");
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo5.ColumnName = "TPL_VehicleTransmission";
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.ColumnName = "TPL_VehicleMake";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "TPL_VehicleModel";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "TPL_VehicleColor";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "TPL_RefVehicleIdentificationNumber";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.ColumnName = "TPL_VehicleNumberOfDoors";
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zCalcEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.ColumnName = "TPL_VehicleYear";
			zCalcEditColumnStyleInfo9.IsVisible = false;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "TPL_Width";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|070500e0-e8c0-4fab-af97-c55e67fca5a6", "Dimensions");
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("QuoteContainersAndPackLinesControl|a48b3ddb-3e0e-4b7b-ac75-1ef069fcb9c2", "Width (ft/in)", "The Width in feet and inches.");
			zTextBoxColumnStyleInfo2.ColumnName = "WidthImperial";
			zTextBoxColumnStyleInfo2.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|fc1b2165-39ca-4c76-9982-a69cb94eeaf5", "Imperial Dimensions");
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "TPL_Height";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|070500e0-e8c0-4fab-af97-c55e67fca5a6", "Dimensions");
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("QuoteContainersAndPackLinesControl|97b6d60c-3952-4dd9-a277-10a92d028eba", "Height (ft/in)", "The Height in feet and inches.");
			zTextBoxColumnStyleInfo3.ColumnName = "HeightImperial";
			zTextBoxColumnStyleInfo3.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|fc1b2165-39ca-4c76-9982-a69cb94eeaf5", "Imperial Dimensions");
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo4.ColumnName = "TPL_DimensionUQ";
			zDropEditColumnStyleInfo4.GroupName = Res.GetData("QuoteContainersAndPackLinesControl|070500e0-e8c0-4fab-af97-c55e67fca5a6", "Dimensions");
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LooseCargoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LooseCargoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LooseCargoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LooseCargoGrid.GridId = "bc5a9b44-e43d-4523-92e8-7b7726b418f8";
			this.LooseCargoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LooseCargoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LooseCargoGrid.LayoutKey = "ContainersGrid";
			this.LooseCargoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 14, true);
			this.LooseCargoGrid.Name = "LooseCargoGrid";
			this.LooseCargoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 131, true);
			this.LooseCargoGrid.TabIndex = 13;
			//
			// QuoteContainersGroupBox
			//
			this.QuoteContainersGroupBox.CaptionResourceString = Res.GetData("QuoteContainersAndPackLinesControl|b30076a9-d3fd-423a-bf71-bf47937604ea", "Containers");
			this.QuoteContainersGroupBox.Controls.Add(this.ContainersGrid);
			this.QuoteContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.QuoteContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.QuoteContainersGroupBox.Name = "QuoteContainersGroupBox";
			this.QuoteContainersGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(4, true);
			this.QuoteContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 150, true);
			this.QuoteContainersGroupBox.TabIndex = 50;
			this.QuoteContainersGroupBox.TabStop = false;
			//
			// ContainersGrid
			//
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "Quote+CurrentOneOffQuote+Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.Containers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffContainers)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.Containers)).SyncRoot)).TC_ContainerCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateOneOffContainers)(((System.Collections.IList)(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.Containers)).SyncRoot)).TC_RC);
			this.ContainersGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.ColumnName = "TC_ContainerCount";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TC_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.ContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ContainersGrid.GridId = "b1fdd0b4-12ce-4043-9040-29807bf78448";
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 14, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 131, true);
			this.ContainersGrid.TabIndex = 11;
			//
			// quoteNumbersControl
			//
			this.quoteNumbersControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.quoteNumbersControl, "Quote+CurrentOneOffQuote+Numbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((QuotedBooking)(null)).Quote.CurrentOneOffQuote.Numbers);
			this.quoteNumbersControl.DisplayDetailPanel = false;
			this.quoteNumbersControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.quoteNumbersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(555, 0, true);
			this.quoteNumbersControl.Name = "quoteNumbersControl";
			this.quoteNumbersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 150, true);
			this.quoteNumbersControl.TabIndex = 52;
			//
			// QuoteContainersAndPackLinesControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LooseCargoGroupBox);
			this.Controls.Add(this.quoteNumbersControl);
			this.Controls.Add(this.QuoteContainersGroupBox);
			this.Name = "QuoteContainersAndPackLinesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LooseCargoGroupBox.ResumeLayout(false);
			this.LooseCargoGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LooseCargoGrid)).EndInit();
			this.LooseCargoGrid.ResumeLayout(false);
			this.LooseCargoGrid.PerformLayout();
			this.QuoteContainersGroupBox.ResumeLayout(false);
			this.QuoteContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.quoteNumbersControl.ResumeLayout(true);
			this.quoteNumbersControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
