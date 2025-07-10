using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class PackContainerForm : ZForm
	{
		private ZCodeFindBox JX_JB_RL_NKPortOfDischargeBoundCodeFindBox;
		private ZCodeFindBox JX_JA_RL_NKPortOfLoadingBoundCodeFindBox;
		private ZGroupBox LoadListGroupBox;
		private ZGroupBox AllocatedPackLinesGroupBox;
		private ZGrid LoadListGrid;
		private ZCalcDropEdit TotalPackagesUnitCalcDropEdit;
		private ZCalcDropEdit TotalWeightCalcDropEdit;
		private ZCalcDropEdit TotalVolumeCalcDropEdit;
		private ZGrid ContainerDetailsGrid;
		private ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZLabel ContainerVoyageNoLabel;
		private ZLabel ContainerVesselLabel;
		private ZLabel VesselLabel;
		private ZLabel VoyageLabel;
		private ZCheckBox ReceivedShipmentsCheckBox;
		private ZCheckBox ThisSailingCheckBox;
		private ZMenuItem Pack;
		private ZMenuItem PackAll;
		private ZMenuItem Split;
		private ZMenuItem Unpack;
		private ZMenuItem UnpackAll;
		private ZPanel VesselPanel;
		private ZPanel VoyageNoPanel;
		private ZButton BuildConsolButton;
		private ZDateEdit ETDDateEdit;
		private ZDateEdit ETADateEdit;
		private ZCheckBox ShowTranshipCheckBox;
		private KPanel TopPanel;
		private KSplitter splitter1;
		private KPanel BottomPanel;
		private ZGroupBox ContainerDetailsGroupBox;
		private KSplitter splitter2;
		private ZGrid BookingContainersGrid;
		private Container components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo10 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo12 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo13 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo14 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo15 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo16 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox = new ZCodeFindBox();
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox = new ZCodeFindBox();
			this.LoadListGroupBox = new ZGroupBox();
			this.LoadListGrid = new ZGrid();
			this.Pack = new ZMenuItem();
			this.PackAll = new ZMenuItem();
			this.Split = new ZMenuItem();
			this.AllocatedPackLinesGroupBox = new ZGroupBox();
			this.ContainerDetailsGrid = new ZGrid();
			this.BuildConsolButton = new ZButton();
			this.VoyageNoPanel = new ZPanel();
			this.VoyageLabel = new ZLabel();
			this.ContainerVoyageNoLabel = new ZLabel();
			this.VesselPanel = new ZPanel();
			this.ContainerVesselLabel = new ZLabel();
			this.VesselLabel = new ZLabel();
			this.Unpack = new ZMenuItem();
			this.UnpackAll = new ZMenuItem();
			this.TotalVolumeCalcDropEdit = new ZCalcDropEdit();
			this.TotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.TotalPackagesUnitCalcDropEdit = new ZCalcDropEdit();
			this.PostingButtonsUserControl = new ZPostingButtonsUserControl();
			this.ReceivedShipmentsCheckBox = new ZCheckBox();
			this.ThisSailingCheckBox = new ZCheckBox();
			this.ETDDateEdit = new ZDateEdit();
			this.ETADateEdit = new ZDateEdit();
			this.ShowTranshipCheckBox = new ZCheckBox();
			this.TopPanel = new KPanel();
			this.splitter1 = new KSplitter();
			this.BottomPanel = new KPanel();
			this.ContainerDetailsGroupBox = new ZGroupBox();
			this.BookingContainersGrid = new ZGrid();
			this.splitter2 = new KSplitter();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadListGroupBox.SuspendLayout();
			((ISupportInitialize)(this.LoadListGrid)).BeginInit();
			this.AllocatedPackLinesGroupBox.SuspendLayout();
			((ISupportInitialize)(this.ContainerDetailsGrid)).BeginInit();
			this.VoyageNoPanel.SuspendLayout();
			this.VesselPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.ContainerDetailsGroupBox.SuspendLayout();
			((ISupportInitialize)(this.BookingContainersGrid)).BeginInit();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 648, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 26, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			//
			// MessageStatusBarPanel
			//
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(996);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(PackContainerHelper);
			//
			// JX_JB_RL_NKPortOfDischargeBoundCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox, "Sailing+JX_JB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JB_RL_NKPortOfDischarge);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.CaptionResourceString = Res.GetData("PackContainerForm|723cbe43-6e70-466c-9087-d5ea6cde532e", "Disch.", "Discharge");
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 8, true);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Name = "JX_JB_RL_NKPortOfDischargeBoundCodeFindBox";
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.ShowDescriptionBox = false;
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox.TabIndex = 5;
			//
			// JX_JA_RL_NKPortOfLoadingBoundCodeFindBox
			//
			this.BindingSource.SetBindingMember(this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox, "Sailing+JX_JA_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JA_RL_NKPortOfLoading);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.CaptionResourceString = Res.GetData("PackContainerForm|d2dcd8eb-14c2-43a3-85ab-48139b221f2c", "Load");
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 8, true);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Name = "JX_JA_RL_NKPortOfLoadingBoundCodeFindBox";
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.ShowDescriptionBox = false;
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 21, true);
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.TabIndex = 1;
			this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox.Load += new EventHandler(this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox_Load);
			//
			// LoadListGroupBox
			//
			this.LoadListGroupBox.CaptionResourceString = Res.GetData("PackContainerForm|ae1f6bcc-3a1f-4995-8f58-80ad5e61683f", "Load List");
			this.LoadListGroupBox.Controls.Add(this.LoadListGrid);
			this.LoadListGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LoadListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 64, true);
			this.LoadListGroupBox.Name = "LoadListGroupBox";
			this.LoadListGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 3, 8, 8, true);
			this.LoadListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 175, true);
			this.LoadListGroupBox.TabIndex = 1;
			this.LoadListGroupBox.TabStop = false;
			//
			// LoadListGrid
			//
			this.LoadListGrid.AllowDrop = true;
			this.LoadListGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LoadListGrid, "Sailing+UnAllocatedPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JS_UniqueConsignRef);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JV_Vessel);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JV_VoyageNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JX_ETD);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_PackageCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_ActualVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_ActualWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JS_GoodsDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_IsReceived);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_ActualVolumeUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_ActualWeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Length);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Height);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Width);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_ItemNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_EndItemNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_RefNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_F3_NKPackType);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_UnitOfDimension);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JX_NKLoadPort);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((PackContainerHelper)(null)).Sailing.UnAllocatedPackLines)).SyncRoot)).JL_Calc_JX_NKDischPort);
			this.LoadListGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|86e2b353-3fbc-4d92-86d7-31ca11880c81", "Booking ID");
			zTextBoxColumnStyleInfo1.ColumnName = "JL_Calc_JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("PackContainerForm|088453df-81b0-48b2-98d2-8a23540769c4", "Vessel");
			zTextBoxColumnStyleInfo2.ColumnName = "JL_Calc_JV_Vessel";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("PackContainerForm|544b5888-1491-4643-b1c7-df8fc3042c25", "Voyage");
			zTextBoxColumnStyleInfo3.ColumnName = "JL_Calc_JV_VoyageNo";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|a30ed33b-8771-4d8e-a642-61eb56301e22", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JL_Calc_JX_ETD";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|06ce578b-b394-48a9-97d0-6f7ac9d3030b", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Res.GetData("PackContainerForm|0824074d-a0de-4b38-a32c-5aca126223f4", "Goods Description");
			zTextBoxColumnStyleInfo4.ColumnName = "JL_Calc_JS_GoodsDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|e23e7448-1353-4e93-ba3e-5f69b40d4937", "Rec.");
			zCheckBoxColumnStyleInfo1.ColumnName = "JL_Calc_IsReceived";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|4877c141-6263-4f53-a025-7b9ac306c61c", "UV");
			zDropEditColumnStyleInfo1.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo2.CaptionResourceString = Res.GetData("PackContainerForm|2057303b-6180-4410-8fb0-c75194bc1023", "UW");
			zDropEditColumnStyleInfo2.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo4.Decimals = 3;
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo5.Decimals = 3;
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo6.Decimals = 3;
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.IsVisible = false;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Res.GetData("PackContainerForm|5fc8b801-70da-4b56-ae97-baa01308cad8", "Item #");
			zCalcEditColumnStyleInfo7.ColumnName = "JL_ItemNo";
			zCalcEditColumnStyleInfo7.Decimals = 0;
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.IsVisible = false;
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Res.GetData("PackContainerForm|5078d8cb-4cdc-4154-bea3-ebf666b14188", "End #");
			zCalcEditColumnStyleInfo8.ColumnName = "JL_EndItemNo";
			zCalcEditColumnStyleInfo8.Decimals = 0;
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Res.GetData("PackContainerForm|2b25a427-56f6-4607-a5c4-f21f51d24bc0", "Ref #");
			zTextBoxColumnStyleInfo5.ColumnName = "JL_RefNumber";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "JL_F3_NKPackType";
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.IsVisible = false;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo4.CaptionResourceString = Res.GetData("PackContainerForm|7d49e20e-8703-40b2-b164-92f5221aac38", "UD");
			zDropEditColumnStyleInfo4.ColumnName = "JL_UnitOfDimension";
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Res.GetData("PackContainerForm|211929bc-a24a-4068-b877-993438378000", "Load");
			zTextBoxColumnStyleInfo6.ColumnName = "JL_Calc_JX_NKLoadPort";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Res.GetData("PackContainerForm|85d4b0aa-1147-4fb8-8bca-09dccf8bcb17", "Disch.", "Discharge");
			zTextBoxColumnStyleInfo7.ColumnName = "JL_Calc_JX_NKDischPort";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LoadListGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LoadListGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LoadListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LoadListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.LoadListGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LoadListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LoadListGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LoadListGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LoadListGrid.GridId = "c4b8e5dc-21da-481c-aa69-3117ea8e51d2";
			this.LoadListGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LoadListGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LoadListGrid.IsWholeRowSelectedOnClick = true;
			this.LoadListGrid.LayoutKey = "LoadListGrid";
			this.LoadListGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.LoadListGrid.Name = "LoadListGrid";
			this.LoadListGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.LoadListGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 151, true);
			this.LoadListGrid.TabIndex = 0;
			this.LoadListGrid.DragEnter += new DragEventHandler(this.LoadListGrid_DragEnter);
			this.LoadListGrid.DragDrop += new DragEventHandler(this.LoadListGrid_DragDrop);
			//
			// Pack
			//
			this.Pack.Index = -1;
			this.Pack.Text = "";
			this.Pack.Click += new EventHandler(this.Pack_Click);
			//
			// PackAll
			//
			this.PackAll.Index = -1;
			this.PackAll.Text = "";
			this.PackAll.Click += new EventHandler(this.PackAll_Click);
			//
			// Split
			//
			this.Split.Index = -1;
			this.Split.Text = "";
			this.Split.Click += new EventHandler(this.Split_Click);
			//
			// AllocatedPackLinesGroupBox
			//
			this.AllocatedPackLinesGroupBox.CaptionResourceString = Res.GetData("PackContainerForm|a5b77ec6-c7f7-4442-9fb9-d68284e3a716", "Allocated Pack Lines");
			this.AllocatedPackLinesGroupBox.Controls.Add(this.ContainerDetailsGrid);
			this.AllocatedPackLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocatedPackLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 361, true);
			this.AllocatedPackLinesGroupBox.Name = "AllocatedPackLinesGroupBox";
			this.AllocatedPackLinesGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(8, 3, 8, 8, true);
			this.AllocatedPackLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 219, true);
			this.AllocatedPackLinesGroupBox.TabIndex = 2;
			this.AllocatedPackLinesGroupBox.TabStop = false;
			//
			// ContainerDetailsGrid
			//
			this.ContainerDetailsGrid.AllowDrop = true;
			this.ContainerDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainerDetailsGrid, "Containers.PackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Calc_JS_UniqueConsignRef);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_PackageCount);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_ActualVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_ActualWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Calc_JS_GoodsDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Calc_IsReceived);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_ActualVolumeUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_ActualWeightUQ);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Height);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Length);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_Width);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_EndItemNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_ItemNo);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_RefNumber);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackLine)(((IList)(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).PackLines)).SyncRoot)).JL_F3_NKPackType);
			this.ContainerDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Res.GetData("PackContainerForm|dfc3c731-eb69-4a48-a6c0-64846edd987e", "Booking ID");
			zTextBoxColumnStyleInfo8.ColumnName = "JL_Calc_JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo8.IsMandatory = true;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Res.GetData("PackContainerForm|2eccf54e-a33a-4b8e-b626-8dec8ed3fd4e", "Packs");
			zCalcEditColumnStyleInfo9.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.IsMandatory = true;
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo10.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo10.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo10.IsMandatory = true;
			zCalcEditColumnStyleInfo10.IsReadOnly = true;
			zCalcEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo11.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo11.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo11.IsMandatory = true;
			zCalcEditColumnStyleInfo11.IsReadOnly = true;
			zCalcEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Res.GetData("PackContainerForm|f5787c26-7b15-4dfc-9a80-6fb7a7a2b7de", "Goods Description");
			zTextBoxColumnStyleInfo9.ColumnName = "JL_Calc_JS_GoodsDescription";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("PackContainerForm|3f4bda92-5be5-47a8-b5af-e0a99b4797a0", "Rec.");
			zCheckBoxColumnStyleInfo2.ColumnName = "JL_Calc_IsReceived";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo5.CaptionResourceString = Res.GetData("PackContainerForm|f8bf0994-b0a5-404a-8e51-c9d70da787d0", "UV");
			zDropEditColumnStyleInfo5.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo5.IsReadOnly = true;
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo6.CaptionResourceString = Res.GetData("PackContainerForm|8bb204a9-5f1d-4d28-918b-66dcd7fb471a", "UW");
			zDropEditColumnStyleInfo6.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo6.IsReadOnly = true;
			zDropEditColumnStyleInfo6.IsVisible = false;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo12.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo12.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo12.Decimals = 3;
			zCalcEditColumnStyleInfo12.IsReadOnly = true;
			zCalcEditColumnStyleInfo12.IsVisible = false;
			zCalcEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo13.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo13.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo13.Decimals = 3;
			zCalcEditColumnStyleInfo13.IsReadOnly = true;
			zCalcEditColumnStyleInfo13.IsVisible = false;
			zCalcEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo14.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo14.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo14.Decimals = 3;
			zCalcEditColumnStyleInfo14.IsReadOnly = true;
			zCalcEditColumnStyleInfo14.IsVisible = false;
			zCalcEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo15.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo15.CaptionResourceString = Res.GetData("PackContainerForm|0c17dcef-902b-4530-a22d-ca094444534f", "End #");
			zCalcEditColumnStyleInfo15.ColumnName = "JL_EndItemNo";
			zCalcEditColumnStyleInfo15.Decimals = 0;
			zCalcEditColumnStyleInfo15.IsReadOnly = true;
			zCalcEditColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo16.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo16.CaptionResourceString = Res.GetData("PackContainerForm|28e2742f-a613-4b5a-9f39-ef7818486ef3", "Item #");
			zCalcEditColumnStyleInfo16.ColumnName = "JL_ItemNo";
			zCalcEditColumnStyleInfo16.Decimals = 0;
			zCalcEditColumnStyleInfo16.IsReadOnly = true;
			zCalcEditColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Res.GetData("PackContainerForm|acda8023-3015-42bd-95b5-ccbbef8c7a51", "Ref #");
			zTextBoxColumnStyleInfo10.ColumnName = "JL_RefNumber";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "JL_F3_NKPackType";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			this.ContainerDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo10);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo11);
			this.ContainerDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ContainerDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ContainerDetailsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo12);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo13);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo14);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo15);
			this.ContainerDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo16);
			this.ContainerDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ContainerDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ContainerDetailsGrid.GridId = "f265eb4e-9cf1-4e54-bc02-7e22931cc654";
			this.ContainerDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainerDetailsGrid.IsWholeRowSelectedOnClick = true;
			this.ContainerDetailsGrid.LayoutKey = "ContainerDetailsGrid";
			this.ContainerDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 16, true);
			this.ContainerDetailsGrid.Name = "ContainerDetailsGrid";
			this.ContainerDetailsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.ContainerDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(980, 195, true);
			this.ContainerDetailsGrid.TabIndex = 1;
			this.ContainerDetailsGrid.DragEnter += new DragEventHandler(this.ContainerDetailsGrid_DragEnter);
			this.ContainerDetailsGrid.DragDrop += new DragEventHandler(this.ContainerDetailsGrid_DragDrop);
			//
			// BuildConsolButton
			//
			this.BuildConsolButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BuildConsolButton.CaptionResourceString = Res.GetData("PackContainerForm|5bd6dd87-53cc-42b3-b01d-ab78115e7596", "Build Consol");
			this.BuildConsolButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(880, 8, true);
			this.BuildConsolButton.Name = "BuildConsolButton";
			this.BuildConsolButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.BuildConsolButton.TabIndex = 6;
			this.BuildConsolButton.Click += new EventHandler(this.BuildConsol_Click);
			//
			// VoyageNoPanel
			//
			this.VoyageNoPanel.Controls.Add(this.VoyageLabel);
			this.VoyageNoPanel.Controls.Add(this.ContainerVoyageNoLabel);
			this.VoyageNoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 32, true);
			this.VoyageNoPanel.Name = "VoyageNoPanel";
			this.VoyageNoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 24, true);
			this.VoyageNoPanel.TabIndex = 9;
			//
			// VoyageLabel
			//
			this.VoyageLabel.CaptionResourceString = Res.GetData("PackContainerForm|c85c10c8-2c55-4aa1-8d09-846edd7d9324", "Voyage No.:");
			this.VoyageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VoyageLabel.Name = "VoyageLabel";
			this.VoyageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.VoyageLabel.TabIndex = 0;
			//
			// ContainerVoyageNoLabel
			//
			this.ContainerVoyageNoLabel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerVoyageNoLabel, "Sailing+JX_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JV_VoyageFlight);
			this.ContainerVoyageNoLabel.CaptionResourceString = Res.GetData("PackContainerForm|ffd5b285-20d2-4438-a84b-3fa6dc5982e6", "Voyage");
			this.ContainerVoyageNoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 0, true);
			this.ContainerVoyageNoLabel.Name = "ContainerVoyageNoLabel";
			this.ContainerVoyageNoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 24, true);
			this.ContainerVoyageNoLabel.TabIndex = 1;
			//
			// VesselPanel
			//
			this.VesselPanel.Controls.Add(this.ContainerVesselLabel);
			this.VesselPanel.Controls.Add(this.VesselLabel);
			this.VesselPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 32, true);
			this.VesselPanel.Name = "VesselPanel";
			this.VesselPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 24, true);
			this.VesselPanel.TabIndex = 8;
			//
			// ContainerVesselLabel
			//
			this.BindingSource.SetBindingMember(this.ContainerVesselLabel, "Sailing+JX_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JV_NKVessel);
			this.ContainerVesselLabel.CaptionResourceString = Res.GetData("PackContainerForm|733c3530-2277-483a-b121-af025fe28b40", "Vessel");
			this.ContainerVesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 0, true);
			this.ContainerVesselLabel.Name = "ContainerVesselLabel";
			this.ContainerVesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 24, true);
			this.ContainerVesselLabel.TabIndex = 1;
			//
			// VesselLabel
			//
			this.VesselLabel.CaptionResourceString = Res.GetData("PackContainerForm|38cfb119-009c-473c-b048-7f879d117b78", "Vessel:");
			this.VesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VesselLabel.Name = "VesselLabel";
			this.VesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.VesselLabel.TabIndex = 0;
			//
			// Unpack
			//
			this.Unpack.Index = -1;
			this.Unpack.Text = "";
			this.Unpack.Click += new EventHandler(this.Unpack_Click);
			//
			// UnpackAll
			//
			this.UnpackAll.Index = -1;
			this.UnpackAll.Text = "";
			this.UnpackAll.Click += new EventHandler(this.UnpackAll_Click);
			//
			// TotalVolumeCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalVolume);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalVolumeUnit);
			this.TotalVolumeCalcDropEdit.BindToAmount = "Containers.JC_Calc_TotalVolume";
			this.TotalVolumeCalcDropEdit.BindToUnit = "Containers.JC_Calc_TotalVolumeUnit";
			this.TotalVolumeCalcDropEdit.CaptionResourceString = Res.GetData("PackContainerForm|2f02afb7-7dc3-4344-be0a-3140518ac187", "Volume");
			this.TotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 8, true);
			this.TotalVolumeCalcDropEdit.Name = "TotalVolumeCalcDropEdit";
			this.TotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalVolumeCalcDropEdit.TabIndex = 5;
			this.TotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// TotalWeightCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalWeight);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalWeightUnit);
			this.TotalWeightCalcDropEdit.BindToAmount = "Containers.JC_Calc_TotalWeight";
			this.TotalWeightCalcDropEdit.BindToUnit = "Containers.JC_Calc_TotalWeightUnit";
			this.TotalWeightCalcDropEdit.CaptionResourceString = Res.GetData("PackContainerForm|2e382a62-8c87-4194-87bb-4009bc42c3a3", "Weight");
			this.TotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 8, true);
			this.TotalWeightCalcDropEdit.Name = "TotalWeightCalcDropEdit";
			this.TotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalWeightCalcDropEdit.TabIndex = 3;
			this.TotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			//
			// TotalPackagesUnitCalcDropEdit
			//
			this.BindingSource.SetBindingMember(this.TotalPackagesUnitCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalPackages);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_TotalPackagesUnit);
			this.TotalPackagesUnitCalcDropEdit.BindToAmount = "Containers.JC_Calc_TotalPackages";
			this.TotalPackagesUnitCalcDropEdit.BindToUnit = "Containers.JC_Calc_TotalPackagesUnit";
			this.TotalPackagesUnitCalcDropEdit.CaptionResourceString = Res.GetData("PackContainerForm|75709de2-b671-4d58-9000-64cdac917c47", "Packages");
			this.TotalPackagesUnitCalcDropEdit.Decimals = 0;
			this.TotalPackagesUnitCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 8, true);
			this.TotalPackagesUnitCalcDropEdit.Name = "TotalPackagesUnitCalcDropEdit";
			this.TotalPackagesUnitCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.TotalPackagesUnitCalcDropEdit.TabIndex = 1;
			this.TotalPackagesUnitCalcDropEdit.UnitPreBoundMaxLength = 3;
			//
			// PostingButtonsUserControl
			//
			this.PostingButtonsUserControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 40, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 7;
			//
			// ReceivedShipmentsCheckBox
			//
			this.BindingSource.SetBindingMember(this.ReceivedShipmentsCheckBox, "Sailing+JX_ShowOnlyReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_ShowOnlyReceived);
			this.ReceivedShipmentsCheckBox.CaptionResourceString = Res.GetData("PackContainerForm|5fa9002e-60c1-4b55-a806-7862ba2f042a", "Only Received Shipments");
			this.ReceivedShipmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReceivedShipmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(738, 8, true);
			this.ReceivedShipmentsCheckBox.Name = "ReceivedShipmentsCheckBox";
			this.ReceivedShipmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 24, true);
			this.ReceivedShipmentsCheckBox.TabIndex = 11;
			//
			// ThisSailingCheckBox
			//
			this.BindingSource.SetBindingMember(this.ThisSailingCheckBox, "Sailing+JX_ShowOnlyThisSailing");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_ShowOnlyThisSailing);
			this.ThisSailingCheckBox.CaptionResourceString = Res.GetData("PackContainerForm|79b6f256-5865-4bf5-a810-04cf7b14b647", "Only This Vessel/Voyage");
			this.ThisSailingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ThisSailingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 32, true);
			this.ThisSailingCheckBox.Name = "ThisSailingCheckBox";
			this.ThisSailingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.ThisSailingCheckBox.TabIndex = 12;
			//
			// ETDDateEdit
			//
			this.ETDDateEdit.AutoCompleteMonthThreshold = 1;
			this.ETDDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETDDateEdit, "Sailing+JX_JA_E_DEP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JA_E_DEP);
			this.ETDDateEdit.CaptionResourceString = Res.GetData("PackContainerForm|9b9b9ccf-160c-48c6-8682-c9e0fbac16bb", "ETD");
			this.ETDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 8, true);
			this.ETDDateEdit.Name = "ETDDateEdit";
			this.ETDDateEdit.TabIndex = 3;
			//
			// ETADateEdit
			//
			this.ETADateEdit.AutoCompleteMonthThreshold = 1;
			this.ETADateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ETADateEdit, "Sailing+JX_JB_E_ARV");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_JB_E_ARV);
			this.ETADateEdit.CaptionResourceString = Res.GetData("PackContainerForm|e8f07043-6fbd-4deb-b2c7-a4c6ebcd1984", "ETA");
			this.ETADateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(490, 8, true);
			this.ETADateEdit.Name = "ETADateEdit";
			this.ETADateEdit.TabIndex = 7;
			//
			// ShowTranshipCheckBox
			//
			this.BindingSource.SetBindingMember(this.ShowTranshipCheckBox, "Sailing+JX_ShowOnlyNonTranship");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Sailing.JX_ShowOnlyNonTranship);
			this.ShowTranshipCheckBox.CaptionResourceString = Res.GetData("PackContainerForm|76e0266c-8caf-489f-8958-9164ccaa24ef", "Only Non-Tranship.", "Only Non-Transhipment");
			this.ShowTranshipCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowTranshipCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 8, true);
			this.ShowTranshipCheckBox.Name = "ShowTranshipCheckBox";
			this.ShowTranshipCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(144, 24, true);
			this.ShowTranshipCheckBox.TabIndex = 10;
			//
			// TopPanel
			//
			this.TopPanel.Controls.Add(this.ShowTranshipCheckBox);
			this.TopPanel.Controls.Add(this.ETADateEdit);
			this.TopPanel.Controls.Add(this.ETDDateEdit);
			this.TopPanel.Controls.Add(this.JX_JA_RL_NKPortOfLoadingBoundCodeFindBox);
			this.TopPanel.Controls.Add(this.JX_JB_RL_NKPortOfDischargeBoundCodeFindBox);
			this.TopPanel.Controls.Add(this.ThisSailingCheckBox);
			this.TopPanel.Controls.Add(this.ReceivedShipmentsCheckBox);
			this.TopPanel.Controls.Add(this.VoyageNoPanel);
			this.TopPanel.Controls.Add(this.VesselPanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 64, true);
			this.TopPanel.TabIndex = 0;
			//
			// splitter1
			//
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 239, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 3, true);
			this.splitter1.TabIndex = 47;
			this.splitter1.TabStop = false;
			//
			// BottomPanel
			//
			this.BottomPanel.Controls.Add(this.TotalVolumeCalcDropEdit);
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.BottomPanel.Controls.Add(this.TotalPackagesUnitCalcDropEdit);
			this.BottomPanel.Controls.Add(this.BuildConsolButton);
			this.BottomPanel.Controls.Add(this.TotalWeightCalcDropEdit);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 580, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 68, true);
			this.BottomPanel.TabIndex = 3;
			//
			// ContainerDetailsGroupBox
			//
			this.ContainerDetailsGroupBox.CaptionResourceString = Res.GetData("PackContainerForm|8a481d95-2a13-4a1e-ac4b-67d2f7cd623d", "Containers");
			this.ContainerDetailsGroupBox.Controls.Add(this.BookingContainersGrid);
			this.ContainerDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ContainerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 242, true);
			this.ContainerDetailsGroupBox.Name = "ContainerDetailsGroupBox";
			this.ContainerDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 116, true);
			this.ContainerDetailsGroupBox.TabIndex = 48;
			this.ContainerDetailsGroupBox.TabStop = false;
			//
			// BookingContainersGrid
			//
			this.BookingContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BookingContainersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((PackContainerHelper)(null)).Containers);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_ContainerNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_RC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_ContainerMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_ReleaseNum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_DepartureContainerYardAddressOrg);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ForwardingContainer)(((IList)(((PackContainerHelper)(null)).Containers)).SyncRoot)).JC_Calc_DepartureContainerYardAddressCode);
			this.BookingContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Res.GetData("PackContainerForm|50b59468-7a59-4222-9c79-f45ddaa2c8b8", "Container No.");
			zTextBoxColumnStyleInfo12.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|31898f83-adf3-4460-a294-19d0e0fa1a47", "Type");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_RC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo7.ColumnName = "JC_ContainerMode";
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo13.ColumnName = "JC_ReleaseNum";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PackContainerForm|a71dbe36-8815-4310-b968-5d8c18179941", "Container Yard");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JC_Calc_DepartureContainerYardAddressOrg";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Res.GetData("PackContainerForm|4b384a74-8976-4e36-a7c1-8c44dc89b259", "Container Yard");
			zDropEditColumnStyleInfo8.CaptionResourceString = Res.GetData("PackContainerForm|4237b620-95e1-4589-8043-0209b67890df", "Container Yard");
			zDropEditColumnStyleInfo8.ColumnName = "JC_Calc_DepartureContainerYardAddressCode";
			zDropEditColumnStyleInfo8.GroupName = Res.GetData("PackContainerForm|4b384a74-8976-4e36-a7c1-8c44dc89b259", "Container Yard");
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.BookingContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.BookingContainersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.BookingContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.BookingContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.BookingContainersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.BookingContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.BookingContainersGrid.GridId = "f2d889c9-c454-4331-8899-82679bf0338b";
			this.BookingContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BookingContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BookingContainersGrid.LayoutKey = "BookingContainersGrid";
			this.BookingContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BookingContainersGrid.Name = "BookingContainersGrid";
			this.BookingContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(990, 97, true);
			this.BookingContainersGrid.TabIndex = 0;
			//
			// splitter2
			//
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 358, true);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 3, true);
			this.splitter2.TabIndex = 49;
			this.splitter2.TabStop = false;
			//
			// PackContainerForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1006, 674, true);
			this.CaptionResourceString = Res.GetData("PackContainerForm|a08dd659-0aa8-4c40-9166-680a15c45455", "Pack Containers");
			this.Controls.Add(this.AllocatedPackLinesGroupBox);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.ContainerDetailsGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.LoadListGroupBox);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight.QuotedBookings.Business";
			this.DataSourceType = typeof(PackContainerHelper);
			this.DataSourceTypeName = "Enterprise.Freight.QuotedBookings.Business.PackContainerHelper";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 725, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 725, true);
			this.Name = "PackContainerForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 5, 0, true);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.LoadListGroupBox, 0);
			this.Controls.SetChildIndex(this.splitter1, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.ContainerDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.splitter2, 0);
			this.Controls.SetChildIndex(this.AllocatedPackLinesGroupBox, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadListGroupBox.ResumeLayout(false);
			((ISupportInitialize)(this.LoadListGrid)).EndInit();
			this.AllocatedPackLinesGroupBox.ResumeLayout(false);
			((ISupportInitialize)(this.ContainerDetailsGrid)).EndInit();
			this.VoyageNoPanel.ResumeLayout(false);
			this.VesselPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.ContainerDetailsGroupBox.ResumeLayout(false);
			((ISupportInitialize)(this.BookingContainersGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
