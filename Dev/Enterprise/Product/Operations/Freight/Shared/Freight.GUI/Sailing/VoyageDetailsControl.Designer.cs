using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	partial class VoyageDetailsControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			PortReferenceColumnStyleInfo portReferenceColumnStyleInfo1 = new PortReferenceColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			PortReferenceColumnStyleInfo portReferenceColumnStyleInfo2 = new PortReferenceColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo14 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo15 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo16 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo17 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo18 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo19 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo20 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo21 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo22 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo23 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo24 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo25 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo serviceStringColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo arrivalPortRouteIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo departurePortRouteIdColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.jobVoyOriginBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.dischargeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.jobVoyDestinationBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.sailingsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.jobSailingBoundGrid = new Enterprise.Freight.GUI.SailingsGrid();
			this.exRatesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TradeLanesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.voyageInfoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsArchivedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isAllCargoCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.VendorDataStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.rg_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.jv_VoyageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.jv_VesselJourneyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.jv_RV_NKVesselBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.jv_VoyageFlightBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.jv_AircraftTypeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.jv_OH_LineBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.detailsExchangeSplitter = new CargoWise.Windows.UI.KSplitContainer();
			this.ExchangeRatesTradeLanesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ExchangeRatesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.TradeLanesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			exRatesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			tradeLanesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			portsSailingsSplitter = new CargoWise.Windows.UI.KSplitContainer();
			loadDischargeSplitter = new CargoWise.Windows.UI.KSplitContainer();
			loadGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.jobVoyOriginBoundGrid)).BeginInit();
			this.jobVoyOriginBoundGrid.SuspendLayout();
			this.dischargeGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.jobVoyDestinationBoundGrid)).BeginInit();
			this.jobVoyDestinationBoundGrid.SuspendLayout();
			this.sailingsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.jobSailingBoundGrid)).BeginInit();
			this.jobSailingBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.exRatesGrid)).BeginInit();
			this.exRatesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradeLanesGrid)).BeginInit();
			this.TradeLanesGrid.SuspendLayout();
			this.voyageInfoGroupBox.SuspendLayout();
			this.jv_VoyageTypeDropEdit.SuspendLayout();
			this.jv_RV_NKVesselBoundCodeFindBox.SuspendLayout();
			this.jv_OH_LineBoundGuidFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsExchangeSplitter)).BeginInit();
			this.detailsExchangeSplitter.Panel1.SuspendLayout();
			this.detailsExchangeSplitter.Panel2.SuspendLayout();
			this.detailsExchangeSplitter.SuspendLayout();
			this.ExchangeRatesTradeLanesTabControl.SuspendLayout();
			this.ExchangeRatesTabPage.SuspendLayout();
			this.exRatesGroupBox.SuspendLayout();
			this.TradeLanesTabPage.SuspendLayout();
			this.tradeLanesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(portsSailingsSplitter)).BeginInit();
			portsSailingsSplitter.Panel1.SuspendLayout();
			portsSailingsSplitter.Panel2.SuspendLayout();
			this.portsSailingsSplitter.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(loadDischargeSplitter)).BeginInit();
			loadDischargeSplitter.Panel1.SuspendLayout();
			loadDischargeSplitter.Panel2.SuspendLayout();
			this.loadDischargeSplitter.SuspendLayout();
			this.loadGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobVoyage);
			// 
			// jobVoyOriginBoundGrid
			// 
			this.jobVoyOriginBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.jobVoyOriginBoundGrid, "Origins");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_E_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_A_DEP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_Berth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_DepartReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_Calc_DepartureCTOAddressOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_Calc_DepartureCTOAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_Calc_DepartureCTOPremiseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_DocumentaryCutoff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_VGMCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_ReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_CutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_DGReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_DGCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_A_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_EmptyReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_EmptyCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_ReeferReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageOrigin)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Origins)).SyncRoot)).JA_ReeferCutOff)));
			this.jobVoyOriginBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JA_RL_NKPortOfLoading";
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.ColumnName = "JA_E_DEP";
			zDateEditColumnStyleInfo1.IsMandatory = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo2.ColumnName = "JA_A_DEP";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo1.ColumnName = "JA_Berth";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			portReferenceColumnStyleInfo1.ColumnName = "JA_DepartReference";
			portReferenceColumnStyleInfo1.IsVisible = false;
			portReferenceColumnStyleInfo1.FieldTypeColumnName = "JA_DepartReferenceFieldType";
			portReferenceColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JA_Calc_DepartureCTOAddressOrg";
			zGuidFindBoxColumnStyleInfo1.GroupName = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|7e5a6433-17f2-4ce0-9971-4aecb3d25000", "Departure CTO");
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "JA_Calc_DepartureCTOAddressCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|7e5a6433-17f2-4ce0-9971-4aecb3d25000", "Departure CTO");
			zDropEditColumnStyleInfo1.IsVisible = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|d39f2802-5390-41ee-8527-a11ef6370cb9", "Departure CTO Premise ID");
			zTextBoxColumnStyleInfo3.ColumnName = "JA_Calc_DepartureCTOPremiseID";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|4e9ef6f0-cbb5-4920-8460-172c5b948fae", "Docs  Due", "Docs  Due Date", "Documentary Due Date");
			zDateEditColumnStyleInfo3.ColumnName = "JA_DocumentaryCutoff";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|0819977e-21b4-4bf3-ae96-579b835ed0a7", "CTO Rec. Start", "CTO Receival Start", "The date when the CTO will start accepting goods for this sailing schedule.");
			zDateEditColumnStyleInfo4.ColumnName = "JA_ReceivalCommences";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|050b6b1b-6e4d-4f74-a9f8-4832f6f7037b", "CTO Cut Off", "CTO Cut Off", "The date when the CTO will stop accepting goods for this sailing schedule.");
			zDateEditColumnStyleInfo5.ColumnName = "JA_CutOff";
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo6.ColumnName = "JA_DGReceivalCommences";
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo7.ColumnName = "JA_DGCutOff";
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo8.ColumnName = "JA_E_ARV";
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo9.ColumnName = "JA_A_ARV";
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo20.ColumnName = "JA_S_ARV";
			zDateEditColumnStyleInfo20.IsVisible = false;
			zDateEditColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo18.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|6d89d947-1f19-4be6-83ec-06135ac210e5", "VGM Cut Off", "VGM Cut Off Date", "The date when the CTO will stop accepting VGM for this sailing schedule.");
			zDateEditColumnStyleInfo18.ColumnName = "JA_VGMCutOff";
			zDateEditColumnStyleInfo18.IsVisible = false;
			zDateEditColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo19.ColumnName = "JA_S_DEP";
			zDateEditColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo19.IsVisible = false;
			zDateEditColumnStyleInfo22.ColumnName = "JA_EmptyReceivalCommences";
			zDateEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo22.IsVisible = false;
			zDateEditColumnStyleInfo23.ColumnName = "JA_EmptyCutOff";
			zDateEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo23.IsVisible = false;
			zDateEditColumnStyleInfo24.ColumnName = "JA_ReeferReceivalCommences";
			zDateEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo24.IsVisible = false;
			zDateEditColumnStyleInfo25.ColumnName = "JA_ReeferCutOff";
			zDateEditColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo25.IsVisible = false;
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(portReferenceColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo18);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo19);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo20);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo22);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo23);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo24);
			this.jobVoyOriginBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo25);
			this.jobVoyOriginBoundGrid.CopySelectedRowsAllowed = true;
			this.jobVoyOriginBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobVoyOriginBoundGrid.GridId = "82987d75-b7de-4df2-8d6f-13aefb3dd9cf";
			this.jobVoyOriginBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.jobVoyOriginBoundGrid.LayoutKey = "JobVoyOriginBoundGrid";
			this.jobVoyOriginBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.jobVoyOriginBoundGrid.Name = "jobVoyOriginBoundGrid";
			this.jobVoyOriginBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 143, true);
			this.jobVoyOriginBoundGrid.TabIndex = 0;
			// 
			// dischargeGroupBox
			// 
			this.dischargeGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|ae610528-04d6-4c18-9352-255c5819955a", "Discharge Ports");
			this.dischargeGroupBox.Controls.Add(this.jobVoyDestinationBoundGrid);
			this.dischargeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dischargeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.dischargeGroupBox.Name = "dischargeGroupBox";
			this.dischargeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 162, true);
			this.dischargeGroupBox.TabIndex = 1;
			this.dischargeGroupBox.TabStop = false;
			// 
			// jobVoyDestinationBoundGrid
			// 
			this.jobVoyDestinationBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.jobVoyDestinationBoundGrid, "Destinations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_E_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_A_ARV)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_Berth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_ArrivalReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_Calc_ArrivalCTOAddressOrg)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_Calc_ArrivalCTOAddressCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_Calc_ArrivalCTOPremiseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_AvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_StorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_RL_NKFirstDischargePort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_FirstDischargePortETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_RL_NKLastForeignPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Freight.Business.VoyageDestination)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Destinations)).SyncRoot)).JB_LastForeignPortETD)));
			this.jobVoyDestinationBoundGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JB_RL_NKPortOfDischarge";
			zCodeFindBoxColumnStyleInfo2.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo10.ColumnName = "JB_E_ARV";
			zDateEditColumnStyleInfo10.IsMandatory = true;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo11.ColumnName = "JB_A_ARV";
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "JB_Berth";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			portReferenceColumnStyleInfo2.ColumnName = "JB_ArrivalReference";
			portReferenceColumnStyleInfo2.IsVisible = false;
			portReferenceColumnStyleInfo2.FieldTypeColumnName = "JB_ArrivalReferenceFieldType";
			portReferenceColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "JB_Calc_ArrivalCTOAddressOrg";
			zGuidFindBoxColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|3e394f7c-0a58-4c87-b546-1d53216707de", "Arrival CTO");
			zGuidFindBoxColumnStyleInfo2.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "JB_Calc_ArrivalCTOAddressCode";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|3e394f7c-0a58-4c87-b546-1d53216707de", "Arrival CTO");
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|4ed3f21a-68ea-48ce-a760-e959561f6ba9", "Arrival CTO Premise ID");
			zTextBoxColumnStyleInfo6.ColumnName = "JB_Calc_ArrivalCTOPremiseID";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo12.ColumnName = "JB_AvailabilityDate";
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo13.ColumnName = "JB_StorageDate";
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo21.ColumnName = "JB_S_ARV";
			zDateEditColumnStyleInfo21.IsVisible = false;
			zDateEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "JB_FirstDischargePortETA";
			zDateTimeOffsetEditColumnStyleInfo1.IsVisible = true;
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "JB_LastForeignPortETD";
			zDateTimeOffsetEditColumnStyleInfo2.IsVisible = true;
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zCodeFindBoxColumnStyleInfo7.ColumnName = "JB_RL_NKFirstDischargePort";
			zCodeFindBoxColumnStyleInfo7.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo8.ColumnName = "JB_RL_NKLastForeignPort";
			zCodeFindBoxColumnStyleInfo8.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(portReferenceColumnStyleInfo2);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo21);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo8);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo7);
			this.jobVoyDestinationBoundGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.jobVoyDestinationBoundGrid.CopySelectedRowsAllowed = true;
			this.jobVoyDestinationBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobVoyDestinationBoundGrid.GridId = "beb0b37f-7c26-45f7-87d0-c10670b5f5b6";
			this.jobVoyDestinationBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.jobVoyDestinationBoundGrid.LayoutKey = "JobVoyDestinationBoundGrid";
			this.jobVoyDestinationBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.jobVoyDestinationBoundGrid.Name = "jobVoyDestinationBoundGrid";
			this.jobVoyDestinationBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 143, true);
			this.jobVoyDestinationBoundGrid.TabIndex = 0;
			// 
			// sailingsGroupBox
			// 
			this.sailingsGroupBox.Controls.Add(this.jobSailingBoundGrid);
			this.sailingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sailingsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sailingsGroupBox.Name = "sailingsGroupBox";
			this.sailingsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 167, true);
			this.sailingsGroupBox.TabIndex = 4;
			this.sailingsGroupBox.TabStop = false;
			// 
			// jobSailingBoundGrid
			// 
			this.jobSailingBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.jobSailingBoundGrid, "Sailings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_UniqueReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_JA_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_JB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_ReservedMasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_DepotReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_DepotCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_DepotAvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_DepotStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_ServiceString)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_ArrivalPortRouteId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).JX_DeparturePortRouteId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobSailing)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).Sailings)).SyncRoot)).CO2ePerTonneInKgForBinding)));
			this.jobSailingBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo7.ColumnName = "JX_UniqueReference";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|7975a4ba-7c2b-4b1c-bb74-c9066fadfe21", "Load", "Port Of Loading", "The load port of the current sailing");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JX_JA_RL_NKPortOfLoading";
			zCodeFindBoxColumnStyleInfo3.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo3.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|00ba36d6-afc2-43e2-a8e0-f6144d55b6f8", "Disch.", "Port Of Discharge", "The discharge port of the current sailing.");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "JX_JB_RL_NKPortOfDischarge";
			zCodeFindBoxColumnStyleInfo4.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo4.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.ColumnName = "JX_IsPublished";
			zCheckBoxColumnStyleInfo1.ToolTip = "Publish this Port Pair.";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.ColumnName = "JX_ReservedMasterBill";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo14.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|a4b273cd-a8db-4281-9256-7cedd6ad0104", "CFS Recv. Start", "CFS Receival Start", "");
			zDateEditColumnStyleInfo14.ColumnName = "JX_DepotReceivalCommences";
			zDateEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|eae19811-6037-485f-8f0b-84a0dd43e12f", "CFS Cut Off", "CFS Cut Off", "");
			zDateEditColumnStyleInfo15.ColumnName = "JX_DepotCutOff";
			zDateEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|f810d9b8-453e-462d-bab7-dac4d77f3fb1", "CFS Avail.", "CFS Available", "");
			zDateEditColumnStyleInfo16.ColumnName = "JX_DepotAvailabilityDate";
			zDateEditColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|5add3a94-93e1-4294-aaf5-e1c50a20d705", "CFS Stor.", "CFS Storage Start", "");
			zDateEditColumnStyleInfo17.ColumnName = "JX_DepotStorageDate";
			zDateEditColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			serviceStringColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|ba1980ec-ca99-fabd-4f1b-37ae3fe11f62", "Service String");
			serviceStringColumn.ColumnName = "JX_ServiceString";
			serviceStringColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			cO2ePerTonneInKgColumnStyleInfo.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|4bee1007-6bb2-4794-85de-e9b91bf7ec5f", "CO2e (kg/t)");
			cO2ePerTonneInKgColumnStyleInfo.ColumnName = "CO2ePerTonneInKgForBinding";
			cO2ePerTonneInKgColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			cO2ePerTonneInKgColumnStyleInfo.IsVisible = true;
			cO2ePerTonneInKgColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);zDateEditColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|f810d9b8-453e-462d-bab7-dac4d77f3fb1", "CFS Avail.", "CFS Available", "");
			arrivalPortRouteIdColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|616FAC28-55E3-4C53-B23A-65BFD553FAD5", "Arrival Port Route ID", "Arrival Port Route ID", "");
			arrivalPortRouteIdColumn.ColumnName = "JX_ArrivalPortRouteId";
			arrivalPortRouteIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			arrivalPortRouteIdColumn.IsVisible = false;
			departurePortRouteIdColumn.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|E250F46D-57D8-4D10-B984-2EAAB251AC64", "Departure Port Route ID", "Departure Port Route ID", "");
			departurePortRouteIdColumn.ColumnName = "JX_DeparturePortRouteId";
			departurePortRouteIdColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			departurePortRouteIdColumn.IsVisible = false;
			this.jobSailingBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.jobSailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.jobSailingBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.jobSailingBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.jobSailingBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.jobSailingBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo14);
			this.jobSailingBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo15);
			this.jobSailingBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo16);
			this.jobSailingBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo17);
			this.jobSailingBoundGrid.ColumnStyles.Add(serviceStringColumn);
			this.jobSailingBoundGrid.ColumnStyles.Add(arrivalPortRouteIdColumn);
			this.jobSailingBoundGrid.ColumnStyles.Add(departurePortRouteIdColumn);

			if (ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled)
			{
				this.jobSailingBoundGrid.ColumnStyles.Add(cO2ePerTonneInKgColumnStyleInfo);
			}

			this.jobSailingBoundGrid.CopySelectedRowsAllowed = true;
			this.jobSailingBoundGrid.CopyToText = "Copy To Sailings With the Same Origin";
			this.jobSailingBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.jobSailingBoundGrid.GridId = "95b32481-fa53-41ec-aaa6-7888c8b39910";
			this.jobSailingBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.jobSailingBoundGrid.LayoutKey = "zGrid2";
			this.jobSailingBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.jobSailingBoundGrid.Name = "jobSailingBoundGrid";
			this.jobSailingBoundGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.jobSailingBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(932, 148, true);
			this.jobSailingBoundGrid.TabIndex = 0;
			// 
			// exRatesGrid
			// 
			this.exRatesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.exRatesGrid, "ExRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).ExRates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageExRate)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).ExRates)).SyncRoot)).E8_RX_NKExCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.VoyageExRate)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).ExRates)).SyncRoot)).E8_VoyageExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.VoyageExRate)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).ExRates)).SyncRoot)).E8_RL_NKPort)));
			this.exRatesGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo5.ColumnName = "E8_RX_NKExCurrency";
			zCodeFindBoxColumnStyleInfo5.IsMandatory = true;
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "E8_VoyageExchangeRate";
			zCalcEditColumnStyleInfo1.Decimals = 6;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCodeFindBoxColumnStyleInfo6.ColumnName = "E8_RL_NKPort";
			zCodeFindBoxColumnStyleInfo6.IsMandatory = false;
			zCodeFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.exRatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.exRatesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.exRatesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo6);
			this.exRatesGrid.CopySelectedRowsAllowed = true;
			this.exRatesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exRatesGrid.GridId = "f0d36c78-d3e0-41fe-8fd6-b2a70d243397";
			this.exRatesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.exRatesGrid.LayoutKey = "ExRatesGrid";
			this.exRatesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.exRatesGrid.Name = "exRatesGrid";
			this.exRatesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 81, true);
			this.exRatesGrid.TabIndex = 0;
			// 
			// TradeLanesGrid
			// 
			this.TradeLanesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TradeLanesGrid, "TradeLanes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).TradeLanes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobTradeLaneVoyage)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).TradeLanes)).SyncRoot)).NB_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobTradeLaneVoyage)(((System.Collections.IList)(((Enterprise.Freight.Business.JobVoyage)(null)).TradeLanes)).SyncRoot)).NB_EJ)));
			this.TradeLanesGrid.CaptionVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "NB_OH";
			zOrganisationFindBoxColumnStyleInfo1.IsMandatory = true;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "NB_EJ";
			zGuidFindBoxColumnStyleInfo3.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TradeLanesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.TradeLanesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.TradeLanesGrid.CopySelectedRowsAllowed = true;
			this.TradeLanesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TradeLanesGrid.GridId = "af93780d-1d75-41b2-aba9-e5a53926333a";
			this.TradeLanesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TradeLanesGrid.LayoutKey = "ExRatesGrid";
			this.TradeLanesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TradeLanesGrid.Name = "TradeLanesGrid";
			this.TradeLanesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 81, true);
			this.TradeLanesGrid.TabIndex = 0;
			// 
			// voyageInfoGroupBox
			// 
			this.voyageInfoGroupBox.Controls.Add(this.IsArchivedCheckBox);
			this.voyageInfoGroupBox.Controls.Add(this.isAllCargoCheckBox);
			this.voyageInfoGroupBox.Controls.Add(this.VendorDataStatusLabel);
			this.voyageInfoGroupBox.Controls.Add(this.rg_CodeBoundTextBox);
			this.voyageInfoGroupBox.Controls.Add(this.jv_VoyageTypeDropEdit);
			this.voyageInfoGroupBox.Controls.Add(this.jv_VesselJourneyNameTextBox);
			this.voyageInfoGroupBox.Controls.Add(this.jv_RV_NKVesselBoundCodeFindBox);
			this.voyageInfoGroupBox.Controls.Add(this.jv_VoyageFlightBoundTextEdit);
			this.voyageInfoGroupBox.Controls.Add(this.jv_AircraftTypeBoundTextEdit);
			this.voyageInfoGroupBox.Controls.Add(this.jv_OH_LineBoundGuidFindBox);
			this.voyageInfoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.voyageInfoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.voyageInfoGroupBox.Name = "voyageInfoGroupBox";
			this.voyageInfoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 133, true);
			this.voyageInfoGroupBox.TabIndex = 1;
			this.voyageInfoGroupBox.TabStop = false;
			// 
			// IsArchivedCheckBox
			// 
			this.IsArchivedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsArchivedCheckBox, "IsArchived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.JobVoyage)(null)).IsArchived)));
			this.IsArchivedCheckBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("fc005e4c-7ae0-421a-9600-847521e4a8f4", "Archived");
			this.IsArchivedCheckBox.Enabled = false;
			this.IsArchivedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsArchivedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 71, true);
			this.IsArchivedCheckBox.Name = "IsArchivedCheckBox";
			this.IsArchivedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsArchivedCheckBox.TabIndex = 11;
			this.IsArchivedCheckBox.UseVisualStyleBackColor = true;
			// 
			// isAllCargoCheckBox
			// 
			this.isAllCargoCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isAllCargoCheckBox, "JV_IsCargoOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_IsCargoOnly)));
			this.isAllCargoCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isAllCargoCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(371, 71, true);
			this.isAllCargoCheckBox.Name = "isAllCargoCheckBox";
			this.isAllCargoCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isAllCargoCheckBox.TabIndex = 10;
			this.isAllCargoCheckBox.UseVisualStyleBackColor = true;
			// 
			// VendorDataStatusLabel
			// 
			this.VendorDataStatusLabel.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VendorDataStatusLabel, false);
			this.VendorDataStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 94, true);
			this.VendorDataStatusLabel.Name = "VendorDataStatusLabel";
			this.VendorDataStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 13, true);
			this.VendorDataStatusLabel.TabIndex = 12;
			this.VendorDataStatusLabel.Text = "<schedule data vendor status>";
			// 
			// rg_CodeBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.rg_CodeBoundTextBox, "JV_Calc_VesselConsortiumCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_Calc_VesselConsortiumCode)));
			this.rg_CodeBoundTextBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|6fc8e4d8-fd25-4fa0-8d5a-e5f9ff59fe6b", "Consortium", "The consortium the current vessel belongs to.");
			this.rg_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 68, true);
			this.rg_CodeBoundTextBox.Name = "rg_CodeBoundTextBox";
			this.rg_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.rg_CodeBoundTextBox.TabIndex = 9;
			this.rg_CodeBoundTextBox.Visible = false;
			// 
			// VendorDataStatusLabel
			// 
			this.VendorDataStatusLabel.AutoSize = true;
			this.VendorDataStatusLabel.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VendorDataStatusLabel, false);
			this.VendorDataStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 94, true);
			this.VendorDataStatusLabel.Name = "VendorDataStatusLabel";
			this.VendorDataStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 13, true);
			this.VendorDataStatusLabel.TabIndex = 12;
			this.VendorDataStatusLabel.Text = "<schedule data vendor status>";
			// 
			// jv_VoyageTypeDropEdit
			// 
			this.jv_VoyageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jv_VoyageTypeDropEdit, "JV_VoyageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_VoyageType)));
			this.jv_VoyageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 42, true);
			this.jv_VoyageTypeDropEdit.Name = "jv_VoyageTypeDropEdit";
			this.jv_VoyageTypeDropEdit.PreBoundMaxLength = 3;
			this.jv_VoyageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.jv_VoyageTypeDropEdit.TabIndex = 7;
			this.jv_VoyageTypeDropEdit.Visible = false;
			// 
			// jv_VesselJourneyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.jv_VesselJourneyNameTextBox, "JV_RV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_RV_NKVessel)));
			this.jv_VesselJourneyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 15, true);
			this.jv_VesselJourneyNameTextBox.Name = "jv_VesselJourneyNameTextBox";
			this.jv_VesselJourneyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.jv_VesselJourneyNameTextBox.TabIndex = 1;
			this.jv_VesselJourneyNameTextBox.Visible = false;
			// 
			// jv_RV_NKVesselBoundCodeFindBox
			// 
			this.jv_RV_NKVesselBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jv_RV_NKVesselBoundCodeFindBox, "JV_RV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_RV_NKVessel)));
			this.jv_RV_NKVesselBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 15, true);
			this.jv_RV_NKVesselBoundCodeFindBox.Name = "jv_RV_NKVesselBoundCodeFindBox";
			this.jv_RV_NKVesselBoundCodeFindBox.PreBoundMaxLength = 35;
			this.jv_RV_NKVesselBoundCodeFindBox.ShowDescriptionBox = false;
			this.jv_RV_NKVesselBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.jv_RV_NKVesselBoundCodeFindBox.TabIndex = 1;
			// 
			// jv_VoyageFlightBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.jv_VoyageFlightBoundTextEdit, "JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_VoyageFlight)));
			this.jv_VoyageFlightBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 15, true);
			this.jv_VoyageFlightBoundTextEdit.Name = "jv_VoyageFlightBoundTextEdit";
			this.jv_VoyageFlightBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.jv_VoyageFlightBoundTextEdit.TabIndex = 3;
			// 
			// jv_AircraftTypeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.jv_AircraftTypeBoundTextEdit, "JV_AircraftTypeForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_AircraftTypeForBinding)));
			this.jv_AircraftTypeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 15, true);
			this.jv_AircraftTypeBoundTextEdit.Name = "jv_AircraftTypeBoundTextEdit";
			this.jv_AircraftTypeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.jv_AircraftTypeBoundTextEdit.TabIndex = 4;
			// 
			// jv_OH_LineBoundGuidFindBox
			// 
			this.jv_OH_LineBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.jv_OH_LineBoundGuidFindBox, "JV_OH_Line");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.JobVoyage)(null)).JV_OH_Line)));
			this.jv_OH_LineBoundGuidFindBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|6eb92f4f-ffb9-4aa3-a236-1db1be62f736", "Carrier");
			this.jv_OH_LineBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 42, true);
			this.jv_OH_LineBoundGuidFindBox.Name = "jv_OH_LineBoundGuidFindBox";
			this.jv_OH_LineBoundGuidFindBox.PreBoundMaxLength = 12;
			this.jv_OH_LineBoundGuidFindBox.ShowDescriptionBox = false;
			this.jv_OH_LineBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.jv_OH_LineBoundGuidFindBox.TabIndex = 5;
			// 
			// detailsExchangeSplitter
			// 
			this.detailsExchangeSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsExchangeSplitter.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.detailsExchangeSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsExchangeSplitter.Name = "detailsExchangeSplitter";
			// 
			// detailsExchangeSplitter.Panel1
			// 
			this.detailsExchangeSplitter.Panel1.Controls.Add(this.voyageInfoGroupBox);
			// 
			// detailsExchangeSplitter.Panel2
			// 
			this.detailsExchangeSplitter.Panel2.Controls.Add(this.ExchangeRatesTradeLanesTabControl);
			this.detailsExchangeSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 133, true);
			this.detailsExchangeSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(610);
			this.detailsExchangeSplitter.TabIndex = 0;
			// 
			// ExchangeRatesTradeLanesTabControl
			// 
			this.ExchangeRatesTradeLanesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ExchangeRatesTradeLanesTabControl.Controls.Add(this.ExchangeRatesTabPage);
			this.ExchangeRatesTradeLanesTabControl.Controls.Add(this.TradeLanesTabPage);
			this.ExchangeRatesTradeLanesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExchangeRatesTradeLanesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExchangeRatesTradeLanesTabControl.Name = "ExchangeRatesTradeLanesTabControl";
			this.ExchangeRatesTradeLanesTabControl.SelectedIndex = 0;
			this.ExchangeRatesTradeLanesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 133, true);
			this.ExchangeRatesTradeLanesTabControl.TabIndex = 4;
			// 
			// ExchangeRatesTabPage
			// 
			this.ExchangeRatesTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|077d309a-bbca-4d66-abfb-fefd51d7868f", "Exchange Rates");
			this.ExchangeRatesTabPage.Controls.Add(exRatesGroupBox);
			this.ExchangeRatesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ExchangeRatesTabPage.Name = "ExchangeRatesTabPage";
			this.ExchangeRatesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ExchangeRatesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 106, true);
			this.ExchangeRatesTabPage.TabIndex = 0;
			this.ExchangeRatesTabPage.UseVisualStyleBackColor = true;
			// 
			// exRatesGroupBox
			// 
			exRatesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|5088f8fd-98b3-4991-9ed3-c3a969407cc1", "Exchange Rates");
			exRatesGroupBox.Controls.Add(this.exRatesGrid);
			exRatesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			exRatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			exRatesGroupBox.Name = "exRatesGroupBox";
			exRatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 100, true);
			exRatesGroupBox.TabIndex = 2;
			exRatesGroupBox.TabStop = false;
			// 
			// TradeLanesTabPage
			// 
			this.TradeLanesTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|8856deb7-102e-4d90-8274-42b2d4bab855", "Trade Lanes");
			this.TradeLanesTabPage.Controls.Add(tradeLanesGroupBox);
			this.TradeLanesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TradeLanesTabPage.Name = "TradeLanesTabPage";
			this.TradeLanesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TradeLanesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 106, true);
			this.TradeLanesTabPage.TabIndex = 1;
			this.TradeLanesTabPage.UseVisualStyleBackColor = true;
			// 
			// tradeLanesGroupBox
			// 
			tradeLanesGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|ededdfcc-069b-44cb-af1e-162b2e2046a1", "Trade Lanes");
			tradeLanesGroupBox.Controls.Add(this.TradeLanesGrid);
			tradeLanesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			tradeLanesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			tradeLanesGroupBox.Name = "tradeLanesGroupBox";
			tradeLanesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 100, true);
			tradeLanesGroupBox.TabIndex = 3;
			tradeLanesGroupBox.TabStop = false;
			// 
			// portsSailingsSplitter
			// 
			portsSailingsSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			portsSailingsSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 133, true);
			portsSailingsSplitter.Name = "portsSailingsSplitter";
			portsSailingsSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// portsSailingsSplitter.Panel1
			// 
			portsSailingsSplitter.Panel1.Controls.Add(loadDischargeSplitter);
			// 
			// portsSailingsSplitter.Panel2
			// 
			portsSailingsSplitter.Panel2.Controls.Add(this.sailingsGroupBox);
			portsSailingsSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 333, true);
			portsSailingsSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(162);
			portsSailingsSplitter.TabIndex = 1;
			// 
			// loadDischargeSplitter
			// 
			loadDischargeSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
			loadDischargeSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			loadDischargeSplitter.Name = "loadDischargeSplitter";
			// 
			// loadDischargeSplitter.Panel1
			// 
			loadDischargeSplitter.Panel1.Controls.Add(loadGroupBox);
			// 
			// loadDischargeSplitter.Panel2
			// 
			loadDischargeSplitter.Panel2.Controls.Add(this.dischargeGroupBox);
			loadDischargeSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 162, true);
			loadDischargeSplitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(475);
			loadDischargeSplitter.TabIndex = 0;
			// 
			// loadGroupBox
			// 
			loadGroupBox.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("VoyageDetailsControl|94299a30-be3b-4ebc-9a66-053e429f2933", "Load Ports");
			loadGroupBox.Controls.Add(this.jobVoyOriginBoundGrid);
			loadGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			loadGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			loadGroupBox.Name = "loadGroupBox";
			loadGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 162, true);
			loadGroupBox.TabIndex = 1;
			loadGroupBox.TabStop = false;
			// 
			// VoyageDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(portsSailingsSplitter);
			this.Controls.Add(this.detailsExchangeSplitter);
			this.Name = "VoyageDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 466, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.jobVoyOriginBoundGrid)).EndInit();
			this.jobVoyOriginBoundGrid.ResumeLayout(false);
			this.jobVoyOriginBoundGrid.PerformLayout();
			this.dischargeGroupBox.ResumeLayout(false);
			this.dischargeGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.jobVoyDestinationBoundGrid)).EndInit();
			this.jobVoyDestinationBoundGrid.ResumeLayout(false);
			this.jobVoyDestinationBoundGrid.PerformLayout();
			this.sailingsGroupBox.ResumeLayout(false);
			this.sailingsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.jobSailingBoundGrid)).EndInit();
			this.jobSailingBoundGrid.ResumeLayout(false);
			this.jobSailingBoundGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.exRatesGrid)).EndInit();
			this.exRatesGrid.ResumeLayout(false);
			this.exRatesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradeLanesGrid)).EndInit();
			this.TradeLanesGrid.ResumeLayout(false);
			this.TradeLanesGrid.PerformLayout();
			this.voyageInfoGroupBox.ResumeLayout(false);
			this.voyageInfoGroupBox.PerformLayout();
			this.jv_VoyageTypeDropEdit.ResumeLayout(true);
			this.jv_VoyageTypeDropEdit.PerformLayout();
			this.jv_RV_NKVesselBoundCodeFindBox.ResumeLayout(true);
			this.jv_RV_NKVesselBoundCodeFindBox.PerformLayout();
			this.jv_OH_LineBoundGuidFindBox.ResumeLayout(true);
			this.jv_OH_LineBoundGuidFindBox.PerformLayout();
			this.detailsExchangeSplitter.Panel1.ResumeLayout(false);
			this.detailsExchangeSplitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.detailsExchangeSplitter)).EndInit();
			this.detailsExchangeSplitter.ResumeLayout(false);
			this.detailsExchangeSplitter.PerformLayout();
			this.ExchangeRatesTradeLanesTabControl.ResumeLayout(false);
			this.ExchangeRatesTradeLanesTabControl.PerformLayout();
			this.ExchangeRatesTabPage.ResumeLayout(false);
			this.ExchangeRatesTabPage.PerformLayout();
			this.exRatesGroupBox.ResumeLayout(false);
			this.exRatesGroupBox.PerformLayout();
			this.TradeLanesTabPage.ResumeLayout(false);
			this.TradeLanesTabPage.PerformLayout();
			this.tradeLanesGroupBox.ResumeLayout(false);
			this.tradeLanesGroupBox.PerformLayout();
			portsSailingsSplitter.Panel1.ResumeLayout(false);
			portsSailingsSplitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(portsSailingsSplitter)).EndInit();
			this.portsSailingsSplitter.ResumeLayout(false);
			this.portsSailingsSplitter.PerformLayout();
			loadDischargeSplitter.Panel1.ResumeLayout(false);
			loadDischargeSplitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(loadDischargeSplitter)).EndInit();
			this.loadDischargeSplitter.ResumeLayout(false);
			this.loadDischargeSplitter.PerformLayout();
			this.loadGroupBox.ResumeLayout(false);
			this.loadGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private CargoWise.Windows.UI.KSplitContainer detailsExchangeSplitter;
		private Enterprise.ZArchitecture.GUI.ZCheckBox isAllCargoCheckBox;
		internal Enterprise.ZArchitecture.ZLabel VendorDataStatusLabel;
		private Enterprise.ZArchitecture.ZTextBox rg_CodeBoundTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit jv_VoyageTypeDropEdit;
		private Enterprise.ZArchitecture.ZTextBox jv_VesselJourneyNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox jv_RV_NKVesselBoundCodeFindBox;
		private Enterprise.ZArchitecture.ZTextBox jv_VoyageFlightBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox jv_AircraftTypeBoundTextEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox jv_OH_LineBoundGuidFindBox;
		private Enterprise.ZArchitecture.ZGrid jobVoyOriginBoundGrid;
		private Enterprise.ZArchitecture.ZGrid jobVoyDestinationBoundGrid;
		private SailingsGrid jobSailingBoundGrid;
		private ZGroupBox voyageInfoGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox dischargeGroupBox;
		private ZGroupBox sailingsGroupBox;
		private Enterprise.ZArchitecture.ZGrid exRatesGrid;
		private ZTabControl ExchangeRatesTradeLanesTabControl;
		private ZTabPage ExchangeRatesTabPage;
		private ZTabPage TradeLanesTabPage;
		private Enterprise.ZArchitecture.ZGrid TradeLanesGrid;
		private ZCheckBox IsArchivedCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox exRatesGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox tradeLanesGroupBox;
		private CargoWise.Windows.UI.KSplitContainer portsSailingsSplitter;
		private CargoWise.Windows.UI.KSplitContainer loadDischargeSplitter;
		private Enterprise.ZArchitecture.GUI.ZGroupBox loadGroupBox;
		private Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo cO2ePerTonneInKgColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
	}
}
