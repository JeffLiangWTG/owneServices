using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Core.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI
{
	public partial class PackagesUserControl
	{
		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo21 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo22 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo23 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new ZGuidFindBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new ZDateEditColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo24 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo25 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo26 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo27 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo28 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo29 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo30 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo31 = new ZCalcEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo32 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo33 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo34 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo35 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo36 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo37 = new ZTextBoxColumnStyleInfo();
			this.ShipmentDetailsTabControl = new ZTemplateTabControl();
			this.PackLineTabPage = new ZTabPage();
			this.ReceiptDateEdit = new ZDateEdit();
			this.zOrganisationFindBox1 = new MasterFiles.GUI.ZOrganisationFindBox();
			this.ImportLoadListNoTextBox = new ZTextBox();
			this.LoadingPortTextBox = new ZTextBox();
			this.JC_ContainerNumTextBox = new ZTextBox();
			this.DischargePortTextBox = new ZTextBox();
			this.JC_LCLStorageCommencesDateEdit = new ZDateEdit();
			this.JC_LCLAvailableDateEdit = new ZDateEdit();
			this.VesselCodeFindBox = new ZCodeFindBox();
			this.JC_UnpackGangTextBox = new ZTextBox();
			this.JX_VoyageTextBox = new ZTextBox();
			this.JC_LCLUnpackDateEdit = new ZDateEdit();
			this.UnpackShedTextBox = new ZTextBox();
			this.ShipmentPanel = new CargoWise.Windows.UI.KPanel();
			this.ShipmentsGrid = new ManifestTallyShipmentsGrid();
			this.PacklineTotalsPanel = new CargoWise.Windows.UI.KPanel();
			this.NilOutturnButton = new ZButton();
			this.LineCountTotalCalcEdit = new ZCalcEdit();
			this.ShipmentsTotalVolumeCalcDropEdit = new ZCalcDropEdit();
			this.ShipmentsTotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.ShipmmentsTotalPacksCalcDropEdit = new ZCalcDropEdit();
			this.BottomPanel = new CargoWise.Windows.UI.KPanel();
			this.zPanel1 = new ZPanel();
			this.PackLineNotesTabControl = new ZTemplateTabControl();
			this.WarehouseTabPage = new ZTabPage();
			this.JobPackLocGrid = new ZGrid();
			this.OutturnNotesTabPage = new ZTabPage();
			this.JL_OutturnCommentTextBox = new ZTextBox();
			this.MarksAndNumbersTabPage = new ZTabPage();
			this.MarksAndNumbersTextBox = new ZTextBox();
			this.PacklinesPackLocatiosnSplitter = new CargoWise.Windows.UI.KSplitter();
			this.PackLinesPanel = new CargoWise.Windows.UI.KPanel();
			this.OutturnTotalsPanel = new CargoWise.Windows.UI.KPanel();
			this.PacksTotalVolumeCalcDropEdit = new ZCalcDropEdit();
			this.PacksTotalWeightCalcDropEdit = new ZCalcDropEdit();
			this.PacksTotalCountCalcDropEdit = new ZCalcDropEdit();
			this.PackLinesGrid = new ZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.zPanel2 = new ZPanel();
			this.SealIntactCheckBox = new ZCheckBox();
			this.SealNumTextBox = new ZTextBox();
			this.PCNNumTextBox = new ZTextBox();
			this.CCNNumTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).BeginInit();
			this.PacklineTotalsPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.ShipmentDetailsTabControl.SuspendLayout();
			this.PackLineTabPage.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.PackLineNotesTabControl.SuspendLayout();
			this.WarehouseTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.JobPackLocGrid)).BeginInit();
			this.OutturnNotesTabPage.SuspendLayout();
			this.MarksAndNumbersTabPage.SuspendLayout();
			this.PackLinesPanel.SuspendLayout();
			this.OutturnTotalsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).BeginInit();
			this.zPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TallyContainer);
			// 
			// ReceiptDateEdit
			// 
			this.ReceiptDateEdit.AllowDrop = true;
			this.ReceiptDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceiptDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceiptDateEdit, "ReceiptDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TallyContainer)(null)).ReceiptDate)));
			this.ReceiptDateEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|0f0cd393-5a8f-4598-81d7-15d202fcfb0a", "Receipt");
			this.ReceiptDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceiptDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 80, true);
			this.ReceiptDateEdit.Name = "ReceiptDateEdit";
			this.ReceiptDateEdit.TabIndex = 16;
			// 
			// zOrganisationFindBox1
			// 
			this.zOrganisationFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zOrganisationFindBox1, "JC_OH_CFSClient");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((TallyContainer)(null)).JC_OH_CFSClient)));
			this.zOrganisationFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(697, 54, true);
			this.zOrganisationFindBox1.ModuleID = ((OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.zOrganisationFindBox1.Name = "zOrganisationFindBox1";
			this.zOrganisationFindBox1.ShowDescriptionBox = false;
			this.zOrganisationFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.zOrganisationFindBox1.TabIndex = 11;
			// 
			// ImportLoadListNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportLoadListNoTextBox, "JC_JK_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_JK_UniqueConsignRef)));
			this.ImportLoadListNoTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|8a0bcd96-ebc4-4dd7-91ff-dcdb92687bcf", "Load List No", "Consol Unique Consign Ref", "Arrival Consolidations Load List Number.");
			this.ImportLoadListNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 28, true);
			this.ImportLoadListNoTextBox.Name = "ImportLoadListNoTextBox";
			this.ImportLoadListNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.ImportLoadListNoTextBox.TabIndex = 1;
			// 
			// LoadingPortTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingPortTextBox, "JC_JA_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_JA_NKPortOfLoading)));
			this.LoadingPortTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|1c33fd6e-a08e-40cd-87aa-8003c77bb69f", "Loading", "Port Of Loading", "Arriving consolidations port of loading.");
			this.LoadingPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 54, true);
			this.LoadingPortTextBox.Name = "LoadingPortTextBox";
			this.LoadingPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.LoadingPortTextBox.TabIndex = 2;
			// 
			// JC_ContainerNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_ContainerNumTextBox, "JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_ContainerNum)));
			this.JC_ContainerNumTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|9fa4da6a-184d-4796-aff2-9a1256a16277", "Container No", "Container Number", "");
			this.JC_ContainerNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 2, true);
			this.JC_ContainerNumTextBox.Name = "JC_ContainerNumTextBox";
			this.JC_ContainerNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.JC_ContainerNumTextBox.TabIndex = 0;
			// 
			// DischargePortTextBox
			// 
			this.BindingSource.SetBindingMember(this.DischargePortTextBox, "JC_JB_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_JB_NKPortOfDischarge)));
			this.DischargePortTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|7763c927-606b-420a-8974-d80a8cc40674", "Discharge", "Port Of Discharge", "Arriving consolidations port of discharge.");
			this.DischargePortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 80, true);
			this.DischargePortTextBox.Name = "DischargePortTextBox";
			this.DischargePortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.DischargePortTextBox.TabIndex = 3;
			// 
			// JC_LCLStorageCommencesDateEdit
			// 
			this.JC_LCLStorageCommencesDateEdit.AllowDrop = true;
			this.JC_LCLStorageCommencesDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_LCLStorageCommencesDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_LCLStorageCommencesDateEdit, "JC_LCLStorageCommences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TallyContainer)(null)).JC_LCLStorageCommences)));
			this.JC_LCLStorageCommencesDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_LCLStorageCommencesDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 54, true);
			this.JC_LCLStorageCommencesDateEdit.Name = "JC_LCLStorageCommencesDateEdit";
			this.JC_LCLStorageCommencesDateEdit.TabIndex = 15;
			// 
			// JC_LCLAvailableDateEdit
			// 
			this.JC_LCLAvailableDateEdit.AllowDrop = true;
			this.JC_LCLAvailableDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_LCLAvailableDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_LCLAvailableDateEdit, "JC_LCLAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TallyContainer)(null)).JC_LCLAvailable)));
			this.JC_LCLAvailableDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_LCLAvailableDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 28, true);
			this.JC_LCLAvailableDateEdit.Name = "JC_LCLAvailableDateEdit";
			this.JC_LCLAvailableDateEdit.TabIndex = 14;
			// 
			// VesselCodeFindBox
			// 
			this.VesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VesselCodeFindBox, "JC_JV_NKVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_JV_NKVessel)));
			this.VesselCodeFindBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|a8b1f82b-08bd-483c-8511-46d54f039fd8", "Vessel", "Vessel in which the container arrived");
			this.VesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 28, true);
			this.VesselCodeFindBox.Name = "VesselCodeFindBox";
			this.VesselCodeFindBox.PreBoundMaxLength = 35;
			this.VesselCodeFindBox.ShowDescriptionBox = false;
			this.VesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.VesselCodeFindBox.TabIndex = 5;
			// 
			// JC_UnpackGangTextBox
			// 
			this.BindingSource.SetBindingMember(this.JC_UnpackGangTextBox, "JC_UnpackGang");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_UnpackGang)));
			this.JC_UnpackGangTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 2, true);
			this.JC_UnpackGangTextBox.Name = "JC_UnpackGangTextBox";
			this.JC_UnpackGangTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.JC_UnpackGangTextBox.TabIndex = 4;
			// 
			// JX_VoyageTextBox
			// 
			this.BindingSource.SetBindingMember(this.JX_VoyageTextBox, "JC_JV_VoyageFlight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_JV_VoyageFlight)));
			this.JX_VoyageTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|48192fef-ace4-41f7-8356-6d2ab6ceaef7", "Voyage", "Voyage number.");
			this.JX_VoyageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 28, true);
			this.JX_VoyageTextBox.Name = "JX_VoyageTextBox";
			this.JX_VoyageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.JX_VoyageTextBox.TabIndex = 10;
			// 
			// JC_LCLUnpackDateEdit
			// 
			this.JC_LCLUnpackDateEdit.AllowDrop = true;
			this.JC_LCLUnpackDateEdit.AutoCompleteMonthThreshold = 1;
			this.JC_LCLUnpackDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.JC_LCLUnpackDateEdit, "JC_LCLUnpack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TallyContainer)(null)).JC_LCLUnpack)));
			this.JC_LCLUnpackDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.JC_LCLUnpackDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 2, true);
			this.JC_LCLUnpackDateEdit.Name = "JC_LCLUnpackDateEdit";
			this.JC_LCLUnpackDateEdit.TabIndex = 13;
			// 
			// UnpackShedTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnpackShedTextBox, "JC_UnpackShed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_UnpackShed)));
			this.UnpackShedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 2, true);
			this.UnpackShedTextBox.Name = "UnpackShedTextBox";
			this.UnpackShedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.UnpackShedTextBox.TabIndex = 9;
			// 
			// ShipmentPanel
			// 
			this.ShipmentPanel.Controls.Add(this.ShipmentsGrid);
			this.ShipmentPanel.Controls.Add(this.PacklineTotalsPanel);
			this.ShipmentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 103, true);
			this.ShipmentPanel.Name = "ShipmentPanel";
			this.ShipmentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 281, true);
			this.ShipmentPanel.TabIndex = 48;
			// 
			// ShipmentsGrid
			// 
			this.ShipmentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ShipmentsGrid, "PackUnpackShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_UnitOfWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_UnitOfVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).ConsigneePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_TranshipToOtherCFS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_RL_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_RL_NKDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).CustomsEntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).ConsignorPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_ShipmentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).JS_JS_ColoadMasterShipment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).ColoadMasterShipmentHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).DocsAndCartage.JP_LCLStorageCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).DocsAndCartage.JP_LCLAvailable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).UnpackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).LinkedToCustoms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).CanadaHouseCCN)));
			this.ShipmentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "JS_UniqueConsignRef";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo13.ColumnName = "JS_HouseBill";
			zTextBoxColumnStyleInfo13.IsMandatory = true;
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "JS_GoodsDescription";
			zCalcEditColumnStyleInfo21.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo21.ColumnName = "JS_OuterPacks";
			zCalcEditColumnStyleInfo21.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo9.ColumnName = "JS_F3_NKPackType";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zCalcEditColumnStyleInfo22.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo22.ColumnName = "JS_ActualWeight";
			zCalcEditColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|cd0b0cfb-9e4c-40a2-8452-db0c30fa738c", "UW");
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo10.ColumnName = "JS_UnitOfWeight";
			zDropEditColumnStyleInfo10.IsVisible = false;
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo23.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo23.ColumnName = "JS_ActualVolume";
			zCalcEditColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zDropEditColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|21aea150-a8d0-40b0-9381-ed94e90fb5da", "UV");
			zDropEditColumnStyleInfo11.ColumnName = "JS_UnitOfVolume";
			zDropEditColumnStyleInfo11.IsVisible = false;
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|9ac612c5-e963-4ffb-997f-11478440e44c", "Consignee");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "ConsigneePK";
			zCheckBoxColumnStyleInfo3.ColumnName = "JS_TranshipToOtherCFS";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "JS_RL_NKOrigin";
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|99005240-5ef2-47d6-bbff-9261dd17d72d", "Dest.", "Destination Port", "The the port where the shipment is intended to go.");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "JS_RL_NKDestination";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|8088c591-8b67-4230-acb3-9b7394d64089", "Customs Number");
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.ColumnName = "CustomsEntryNumber";
			zGuidFindBoxColumnStyleInfo5.Caption = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			zGuidFindBoxColumnStyleInfo5.ColumnName = "ConsignorPK";
			zTextBoxColumnStyleInfo16.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|882e9d06-73fe-49f0-b94a-c9db5a9b594a", "Marks And Numbers");
			zTextBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo16.ColumnName = "JS_MarksAndNumbers";
			zDropEditColumnStyleInfo12.ColumnName = "JS_ShipmentType";
			zDropEditColumnStyleInfo12.IsVisible = false;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|b5298574-c079-4e7e-bdb9-0d8e28c22f4c", "Coload Master Shipment");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "JS_JS_ColoadMasterShipment";
			zGuidFindBoxColumnStyleInfo6.IsVisible = false;
			zGuidFindBoxColumnStyleInfo6.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ShipmentReceival;
			zTextBoxColumnStyleInfo17.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|2e181653-6b37-4a3e-94ec-cbbe2f01fd77", "Coload Master Shipment House Bill");
			zTextBoxColumnStyleInfo17.ColumnName = "ColoadMasterShipmentHouseBill";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|3fe581eb-2754-4db2-9433-809cdad7f797", "CFS Storage", "Storage Start Date", "The actual date/time the goods have been delivered to the Consignee.");
			zDateEditColumnStyleInfo4.ColumnName = "DocsAndCartage+JP_LCLStorageCommences";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|354756ac-7286-4f35-a53f-de8ff05e5c22", "CFS Available", "Available Date", "The actual date/time the goods have been delivered to the Consignee.");
			zDateEditColumnStyleInfo5.ColumnName = "DocsAndCartage+JP_LCLAvailable";
			zDateEditColumnStyleInfo5.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|55b6581b-e16e-4423-91df-065d871ee9c7", "Unpack Date");
			zDateEditColumnStyleInfo6.ColumnName = "UnpackDate";
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|2dbdab60-279a-4804-a43a-22ec2aab36ff", "Linked To Customs");
			zCheckBoxColumnStyleInfo4.ColumnName = "LinkedToCustoms";
			zTextBoxColumnStyleInfo37.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("LoadListShipmentsModuleButtonGrid|20d571c8-de0e-416f-b4b4-38eafd41bfff", "House CCN");
			zTextBoxColumnStyleInfo37.ColumnName = "CanadaHouseCCN";
			zTextBoxColumnStyleInfo37.IsVisible = false;
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo21);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo22);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.ShipmentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo23);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.ShipmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.ShipmentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.ShipmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.ShipmentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.ShipmentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.ShipmentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.ShipmentsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.ShipmentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo37);
			this.ShipmentsGrid.CopySelectedRowsAllowed = true;
			this.ShipmentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentsGrid.GridId = "c2bb3d47-b3d8-40b8-9717-3536eb43a071";
			this.ShipmentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShipmentsGrid.LayoutKey = "zGrid1";
			this.ShipmentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentsGrid.Name = "ShipmentsGrid";
			this.ShipmentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 248, true);
			this.ShipmentsGrid.TabIndex = 0;
			// 
			// PacklineTotalsPanel
			// 
			this.PacklineTotalsPanel.Controls.Add(this.NilOutturnButton);
			this.PacklineTotalsPanel.Controls.Add(this.LineCountTotalCalcEdit);
			this.PacklineTotalsPanel.Controls.Add(this.ShipmentsTotalVolumeCalcDropEdit);
			this.PacklineTotalsPanel.Controls.Add(this.ShipmentsTotalWeightCalcDropEdit);
			this.PacklineTotalsPanel.Controls.Add(this.ShipmmentsTotalPacksCalcDropEdit);
			this.PacklineTotalsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PacklineTotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 248, true);
			this.PacklineTotalsPanel.Name = "PacklineTotalsPanel";
			this.PacklineTotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 33, true);
			this.PacklineTotalsPanel.TabIndex = 15;
			// 
			// NilOutturnButton
			// 
			this.NilOutturnButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|bba3d6a0-382b-472c-bfa5-0b7217ad06c2", "Outturn", "Nil Outturn", "");
			this.NilOutturnButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(878, 5, true);
			this.NilOutturnButton.Name = "NilOutturnButton";
			this.NilOutturnButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.NilOutturnButton.TabIndex = 31;
			this.NilOutturnButton.UseVisualStyleBackColor = true;
			this.NilOutturnButton.Click += new EventHandler(this.NilOutturnButton_Click);
			// 
			// LineCountTotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineCountTotalCalcEdit, "TotalShipments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyContainer)(null)).TotalShipments)));
			this.LineCountTotalCalcEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|806adfe6-d15c-4282-83a0-4f73c83b61cb", "Line Count Total");
			this.LineCountTotalCalcEdit.DecimalPlaces = 0;
			this.LineCountTotalCalcEdit.Decimals = 0;
			this.LineCountTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(632, 7, true);
			this.LineCountTotalCalcEdit.Name = "LineCountTotalCalcEdit";
			this.LineCountTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LineCountTotalCalcEdit.TabIndex = 3;
			this.LineCountTotalCalcEdit.Text = "0";
			this.LineCountTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentsTotalVolumeCalcDropEdit
			// 
			this.ShipmentsTotalVolumeCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentsTotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyContainer)(null)).TotalShipmentVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_Calc_TotalVolumeUnit)));
			this.ShipmentsTotalVolumeCalcDropEdit.BindToAmount = "TotalShipmentVolume";
			this.ShipmentsTotalVolumeCalcDropEdit.BindToUnit = "JC_Calc_TotalVolumeUnit";
			this.ShipmentsTotalVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|86322dc1-8222-488a-a332-7258aaedceb9", "Volume", "Total Shipment Volume", "");
			this.ShipmentsTotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 5, true);
			this.ShipmentsTotalVolumeCalcDropEdit.Name = "ShipmentsTotalVolumeCalcDropEdit";
			this.ShipmentsTotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentsTotalVolumeCalcDropEdit.TabIndex = 2;
			this.ShipmentsTotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmentsTotalWeightCalcDropEdit
			// 
			this.ShipmentsTotalWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentsTotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyContainer)(null)).TotalShipmentWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_Calc_TotalWeightUnit)));
			this.ShipmentsTotalWeightCalcDropEdit.BindToAmount = "TotalShipmentWeight";
			this.ShipmentsTotalWeightCalcDropEdit.BindToUnit = "JC_Calc_TotalWeightUnit";
			this.ShipmentsTotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|63c7bd88-e7a7-4de5-a80f-41d50aeda1a1", "Weight", "Total Shipment Weight", "");
			this.ShipmentsTotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 5, true);
			this.ShipmentsTotalWeightCalcDropEdit.Name = "ShipmentsTotalWeightCalcDropEdit";
			this.ShipmentsTotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmentsTotalWeightCalcDropEdit.TabIndex = 1;
			this.ShipmentsTotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// ShipmmentsTotalPacksCalcDropEdit
			// 
			this.ShipmmentsTotalPacksCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmmentsTotalPacksCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyContainer)(null)).TotalShipmentPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_Calc_TotalPackagesUnit)));
			this.ShipmmentsTotalPacksCalcDropEdit.BindToAmount = "TotalShipmentPacks";
			this.ShipmmentsTotalPacksCalcDropEdit.BindToUnit = "JC_Calc_TotalPackagesUnit";
			this.ShipmmentsTotalPacksCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|fd7bb576-b4bf-45b9-bb64-e62264d283da", "Packs", "Total Shipment Packs", "");
			this.ShipmmentsTotalPacksCalcDropEdit.Decimals = 0;
			this.ShipmmentsTotalPacksCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 5, true);
			this.ShipmmentsTotalPacksCalcDropEdit.Name = "ShipmmentsTotalPacksCalcDropEdit";
			this.ShipmmentsTotalPacksCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.ShipmmentsTotalPacksCalcDropEdit.TabIndex = 0;
			this.ShipmmentsTotalPacksCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ShipmentDetailsTabControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 392, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 184, true);
			this.BottomPanel.TabIndex = 50;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.PackLineNotesTabControl);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(749, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 0, 0, 0, true);
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 184, true);
			this.zPanel1.TabIndex = 19;
			// 
			// PackLineNotesTabControl
			// 
			this.PackLineNotesTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackLineNotesTabControl.Controls.Add(this.WarehouseTabPage);
			this.PackLineNotesTabControl.Controls.Add(this.OutturnNotesTabPage);
			this.PackLineNotesTabControl.Controls.Add(this.MarksAndNumbersTabPage);
			this.PackLineNotesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackLineNotesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.PackLineNotesTabControl.Name = "PackLineNotesTabControl";
			this.PackLineNotesTabControl.SelectedIndex = 0;
			this.PackLineNotesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 184, true);
			this.PackLineNotesTabControl.TabIndex = 0;
			// 
			// WarehouseTabPage
			// 
			this.WarehouseTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|290bdc5d-954a-4e77-a570-d9a6a8abbee6", "Warehouse", "The Warehouse tab.");
			this.WarehouseTabPage.Controls.Add(this.JobPackLocGrid);
			this.WarehouseTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WarehouseTabPage.Name = "WarehouseTabPage";
			this.WarehouseTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 157, true);
			this.WarehouseTabPage.TabIndex = 2;
			// 
			// JobPackLocGrid
			// 
			this.JobPackLocGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.JobPackLocGrid, "PackUnpackShipments.OuterPackLines.PackLocations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).PackLocations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLocation)(((System.Collections.IList)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).JQ_NoPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLocation)(((System.Collections.IList)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).PackLocations)).SyncRoot)).JQ_WarehouseLocation)));
			this.JobPackLocGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JQ_NoPackages";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "JQ_WarehouseLocation";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.JobPackLocGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.JobPackLocGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.JobPackLocGrid.CopySelectedRowsAllowed = true;
			this.JobPackLocGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobPackLocGrid.GridId = "436051fd-1dd8-4f42-bc6d-5859860b2869";
			this.JobPackLocGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.JobPackLocGrid.LayoutKey = "JobPackLocGrid";
			this.JobPackLocGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobPackLocGrid.Name = "JobPackLocGrid";
			this.JobPackLocGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 157, true);
			this.JobPackLocGrid.TabIndex = 0;
			// 
			// OutturnNotesTabPage
			// 
			this.OutturnNotesTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e5a0d780-5f4f-4470-a6bd-20af06cfecc6", "Outturn Notes", "The Outturn Notes tab.");
			this.OutturnNotesTabPage.Controls.Add(this.JL_OutturnCommentTextBox);
			this.OutturnNotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OutturnNotesTabPage.Name = "OutturnNotesTabPage";
			this.OutturnNotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 157, true);
			this.OutturnNotesTabPage.TabIndex = 0;
			// 
			// JL_OutturnCommentTextBox
			// 
			this.BindingSource.SetBindingMember(this.JL_OutturnCommentTextBox, "PackUnpackShipments.OuterPackLines.JL_OutturnComment");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnComment)));
			this.JL_OutturnCommentTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JL_OutturnCommentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JL_OutturnCommentTextBox.Multiline = true;
			this.JL_OutturnCommentTextBox.Name = "JL_OutturnCommentTextBox";
			this.JL_OutturnCommentTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.JL_OutturnCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 157, true);
			this.JL_OutturnCommentTextBox.TabIndex = 0;
			// 
			// MarksAndNumbersTabPage
			// 
			this.MarksAndNumbersTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|ab5ffe73-7d9b-48d3-bed2-c6261412a619", "Marks & Numbers", "The Marks & Numbers tab.");
			this.MarksAndNumbersTabPage.Controls.Add(this.MarksAndNumbersTextBox);
			this.MarksAndNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MarksAndNumbersTabPage.Name = "MarksAndNumbersTabPage";
			this.MarksAndNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MarksAndNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(257, 157, true);
			this.MarksAndNumbersTabPage.TabIndex = 3;
			this.MarksAndNumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// MarksAndNumbersTextBox
			// 
			this.BindingSource.SetBindingMember(this.MarksAndNumbersTextBox, "PackUnpackShipments.OuterPackLines.JL_MarksAndNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_MarksAndNumbers)));
			this.MarksAndNumbersTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MarksAndNumbersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MarksAndNumbersTextBox.Multiline = true;
			this.MarksAndNumbersTextBox.Name = "MarksAndNumbersTextBox";
			this.MarksAndNumbersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 151, true);
			this.MarksAndNumbersTextBox.TabIndex = 1;
			// 
			// PacklinesPackLocatiosnSplitter
			// 
			this.PacklinesPackLocatiosnSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 0, true);
			this.PacklinesPackLocatiosnSplitter.Name = "PacklinesPackLocatiosnSplitter";
			this.PacklinesPackLocatiosnSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 184, true);
			this.PacklinesPackLocatiosnSplitter.TabIndex = 18;
			this.PacklinesPackLocatiosnSplitter.TabStop = false;
			// 
			// ShipmentDetailsTabControl
			// 
			this.ShipmentDetailsTabControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentDetailsTabControl.Controls.Add(this.PackLineTabPage);
			this.ShipmentDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentDetailsTabControl.Name = "ShipmentDetailsTabControl";
			this.ShipmentDetailsTabControl.SelectedIndex = 0;
			this.ShipmentDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 184, true);
			this.ShipmentDetailsTabControl.TabIndex = 0;
			// 
			// PackLineTabPage
			// 
			this.PackLineTabPage.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|d3e3f90a-f43d-48a3-b6a5-f06693d2e480", "Pack Lines");
			this.PackLineTabPage.Controls.Add(this.zPanel1);
			this.PackLineTabPage.Controls.Add(this.PacklinesPackLocatiosnSplitter);
			this.PackLineTabPage.Controls.Add(this.PackLinesPanel);
			this.PackLineTabPage.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackLineTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackLineTabPage.Name = "PackLineTabPage";
			this.PackLineTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 157, true);
			this.PackLineTabPage.TabIndex = 0;
			// 
			// PackLinesPanel
			// 
			this.PackLinesPanel.Controls.Add(this.OutturnTotalsPanel);
			this.PackLinesPanel.Controls.Add(this.PackLinesGrid);
			this.PackLinesPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.PackLinesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackLinesPanel.Name = "PackLinesPanel";
			this.PackLinesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 157, true);
			this.PackLinesPanel.TabIndex = 0;
			this.PackLinesPanel.TabStop = false;
			// 
			// OutturnTotalsPanel
			// 
			this.OutturnTotalsPanel.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OutturnTotalsPanel.Controls.Add(this.PacksTotalVolumeCalcDropEdit);
			this.OutturnTotalsPanel.Controls.Add(this.PacksTotalWeightCalcDropEdit);
			this.OutturnTotalsPanel.Controls.Add(this.PacksTotalCountCalcDropEdit);
			this.OutturnTotalsPanel.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.OutturnTotalsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 131, true);
			this.OutturnTotalsPanel.Name = "OutturnTotalsPanel";
			this.OutturnTotalsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 24, true);
			this.OutturnTotalsPanel.TabIndex = 16;
			// 
			// PacksTotalVolumeCalcDropEdit
			// 
			this.PacksTotalVolumeCalcDropEdit.AllowDrop = true;
			this.PacksTotalVolumeCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PacksTotalVolumeCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalOuterPacksVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalPackLineVolumeUnit)));
			this.PacksTotalVolumeCalcDropEdit.BindToAmount = "PackUnpackShipments.TotalOuterPacksVolume";
			this.PacksTotalVolumeCalcDropEdit.BindToUnit = "PackUnpackShipments.TotalPackLineVolumeUnit";
			this.PacksTotalVolumeCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|2ad0441f-1e43-4833-a84c-3822aba6e40b", "Volume", "Total Outer Packs Volume", "");
			this.PacksTotalVolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 3, true);
			this.PacksTotalVolumeCalcDropEdit.Name = "PacksTotalVolumeCalcDropEdit";
			this.PacksTotalVolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PacksTotalVolumeCalcDropEdit.TabIndex = 2;
			this.PacksTotalVolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// PacksTotalWeightCalcDropEdit
			// 
			this.PacksTotalWeightCalcDropEdit.AllowDrop = true;
			this.PacksTotalWeightCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PacksTotalWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalOuterPacksWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalPackLineWeightUnit)));
			this.PacksTotalWeightCalcDropEdit.BindToAmount = "PackUnpackShipments.TotalOuterPacksWeight";
			this.PacksTotalWeightCalcDropEdit.BindToUnit = "PackUnpackShipments.TotalPackLineWeightUnit";
			this.PacksTotalWeightCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|417a4a5e-0946-4a2c-98c1-09c5efaf305c", "Weight", "Total Outer Packs Weight", "");
			this.PacksTotalWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 3, true);
			this.PacksTotalWeightCalcDropEdit.Name = "PacksTotalWeightCalcDropEdit";
			this.PacksTotalWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PacksTotalWeightCalcDropEdit.TabIndex = 1;
			this.PacksTotalWeightCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// PacksTotalCountCalcDropEdit
			// 
			this.PacksTotalCountCalcDropEdit.AllowDrop = true;
			this.PacksTotalCountCalcDropEdit.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PacksTotalCountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalOuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).TotalOuterPacksUnit)));
			this.PacksTotalCountCalcDropEdit.BindToAmount = "PackUnpackShipments.TotalOuterPacks";
			this.PacksTotalCountCalcDropEdit.BindToUnit = "PackUnpackShipments.TotalOuterPacksUnit";
			this.PacksTotalCountCalcDropEdit.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|8daf2033-7335-444b-80b9-9299929cf358", "Packs", "Total Outer Packs", "");
			this.PacksTotalCountCalcDropEdit.Decimals = 0;
			this.PacksTotalCountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 3, true);
			this.PacksTotalCountCalcDropEdit.Name = "PacksTotalCountCalcDropEdit";
			this.PacksTotalCountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.PacksTotalCountCalcDropEdit.TabIndex = 0;
			this.PacksTotalCountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// PackLinesGrid
			// 
			this.PackLinesGrid.AllowNavigation = false;
			this.PackLinesGrid.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackLinesGrid, "PackUnpackShipments.OuterPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Calc_FirstImportContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_F3_NKPackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Outturn)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Pillaged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Damaged)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Calc_Surplus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Calc_Shortlanded)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnedVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnedLength)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnedWidth)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnedHeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_OutturnUD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_ActualWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Length)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Height)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_Width)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyPackLine)(((System.Collections.IList)(((PackUnpackShipment)(((System.Collections.IList)(((TallyContainer)(null)).PackUnpackShipments)).SyncRoot)).OuterPackLines)).SyncRoot)).JL_UnitOfDimension)));
			this.PackLinesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|2ae3a628-766d-40f8-9be7-1507716082f6", "Container Num.");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "JL_Calc_FirstImportContainerNum";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "JL_F3_NKPackType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "JL_PackageCount";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "JL_Outturn";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsMandatory = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "JL_Pillaged";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo24.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo24.ColumnName = "JL_Damaged";
			zCalcEditColumnStyleInfo24.Decimals = 0;
			zCalcEditColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo25.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo25.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|ebfc9183-6e01-4a22-aaba-29b8678a5666", "Surplus");
			zCalcEditColumnStyleInfo25.ColumnName = "JL_Calc_Surplus";
			zCalcEditColumnStyleInfo25.Decimals = 0;
			zCalcEditColumnStyleInfo26.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo26.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e91f2f29-b7d5-4610-b5f6-c9970d25e9f2", "Short");
			zCalcEditColumnStyleInfo26.ColumnName = "JL_Calc_Shortlanded";
			zCalcEditColumnStyleInfo26.Decimals = 0;
			zCalcEditColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo27.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo27.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e5d1b22a-8228-4fb1-b63a-31f1545c31f8", "OT Weight", "Outturned Weight", "The total weight of the outturned packages.");
			zCalcEditColumnStyleInfo27.ColumnName = "JL_OutturnedWeight";
			zCalcEditColumnStyleInfo27.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e648bb69-46ff-4519-80db-7136b57c921d", "Outturned Weight");
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|c2b53185-cb21-41ee-91f5-70a7886eec38", "UW", "Unit Of Weight", "");
			zTextBoxColumnStyleInfo3.ColumnName = "JL_OutturnWeightUQ";
			zTextBoxColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e648bb69-46ff-4519-80db-7136b57c921d", "Outturned Weight");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo28.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo28.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|13148ac1-43da-4b14-99f4-0780b83f0168", "OT Volume", "Outturned Volume", "The total volume of the outturned packages.");
			zCalcEditColumnStyleInfo28.ColumnName = "JL_OutturnedVolume";
			zCalcEditColumnStyleInfo28.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e2c23548-ee00-46aa-ab46-655aeba070ec", "Outturned Volume");
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|39c7c988-9656-4d30-9535-4d2080f77870", "UV");
			zTextBoxColumnStyleInfo4.ColumnName = "JL_OutturnVolumeUQ";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|e2c23548-ee00-46aa-ab46-655aeba070ec", "Outturned Volume");
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo29.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo29.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|c8d692a5-9263-4c18-9693-6e0951ad7330", "OT Length");
			zCalcEditColumnStyleInfo29.ColumnName = "JL_OutturnedLength";
			zCalcEditColumnStyleInfo29.Decimals = 3;
			zCalcEditColumnStyleInfo29.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|000edc21-dd9f-4eda-b9a6-af7c8a581931", "Outturned Dimensions");
			zCalcEditColumnStyleInfo29.IsVisible = false;
			zCalcEditColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo30.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo30.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|3bc7c87a-a0b8-4e73-8496-ee422b6e575d", "OT Width");
			zCalcEditColumnStyleInfo30.ColumnName = "JL_OutturnedWidth";
			zCalcEditColumnStyleInfo30.Decimals = 3;
			zCalcEditColumnStyleInfo30.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|000edc21-dd9f-4eda-b9a6-af7c8a581931", "Outturned Dimensions");
			zCalcEditColumnStyleInfo30.IsVisible = false;
			zCalcEditColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo31.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo31.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|0e4cf44f-a389-48e6-b0e9-64ab740ca0d0", "OT Height");
			zCalcEditColumnStyleInfo31.ColumnName = "JL_OutturnedHeight";
			zCalcEditColumnStyleInfo31.Decimals = 3;
			zCalcEditColumnStyleInfo31.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|000edc21-dd9f-4eda-b9a6-af7c8a581931", "Outturned Dimensions");
			zCalcEditColumnStyleInfo31.IsVisible = false;
			zCalcEditColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|a81d92fb-b804-418f-b641-680c1d203bc4", "UD");
			zTextBoxColumnStyleInfo5.ColumnName = "JL_OutturnUD";
			zTextBoxColumnStyleInfo5.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|000edc21-dd9f-4eda-b9a6-af7c8a581931", "Outturned Dimensions");
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo32.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo32.ColumnName = "JL_ActualVolume";
			zCalcEditColumnStyleInfo32.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|ccf24517-fdf7-461b-8436-a5033ba9623b", "Volume");
			zCalcEditColumnStyleInfo32.IsReadOnly = true;
			zCalcEditColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "JL_ActualVolumeUQ";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|ccf24517-fdf7-461b-8436-a5033ba9623b", "Volume");
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo33.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo33.ColumnName = "JL_ActualWeight";
			zCalcEditColumnStyleInfo33.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|9fb1ef55-2458-46f6-bac3-60f515ce6582", "Weight");
			zCalcEditColumnStyleInfo33.IsReadOnly = true;
			zCalcEditColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "JL_ActualWeightUQ";
			zDropEditColumnStyleInfo3.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|9fb1ef55-2458-46f6-bac3-60f515ce6582", "Weight");
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zCalcEditColumnStyleInfo34.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo34.ColumnName = "JL_Length";
			zCalcEditColumnStyleInfo34.Decimals = 3;
			zCalcEditColumnStyleInfo34.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|64e22ae3-1c2f-434c-b5ac-e441b7d8747b", "Dimensions");
			zCalcEditColumnStyleInfo34.IsReadOnly = true;
			zCalcEditColumnStyleInfo34.IsVisible = false;
			zCalcEditColumnStyleInfo35.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo35.ColumnName = "JL_Height";
			zCalcEditColumnStyleInfo35.Decimals = 3;
			zCalcEditColumnStyleInfo35.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|64e22ae3-1c2f-434c-b5ac-e441b7d8747b", "Dimensions");
			zCalcEditColumnStyleInfo35.IsReadOnly = true;
			zCalcEditColumnStyleInfo35.IsVisible = false;
			zCalcEditColumnStyleInfo35.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo36.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo36.ColumnName = "JL_Width";
			zCalcEditColumnStyleInfo36.Decimals = 3;
			zCalcEditColumnStyleInfo36.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|64e22ae3-1c2f-434c-b5ac-e441b7d8747b", "Dimensions");
			zCalcEditColumnStyleInfo36.IsReadOnly = true;
			zCalcEditColumnStyleInfo36.IsVisible = false;
			zCalcEditColumnStyleInfo36.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|dbc4c0a5-032b-4d4c-aded-928369ef470a", "UD");
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "JL_UnitOfDimension";
			zDropEditColumnStyleInfo4.GroupName = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|64e22ae3-1c2f-434c-b5ac-e441b7d8747b", "Dimensions");
			zDropEditColumnStyleInfo4.IsReadOnly = true;
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo24);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo25);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo26);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo27);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo28);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo29);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo30);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo31);
			this.PackLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo32);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo33);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo34);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo35);
			this.PackLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo36);
			this.PackLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.PackLinesGrid.CopySelectedRowsAllowed = true;
			this.PackLinesGrid.GridId = "b3f97e00-c040-461a-aab2-72e01043b28d";
			this.PackLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLinesGrid.LayoutKey = "PackLinesGrid";
			this.PackLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.PackLinesGrid.Name = "PackLinesGrid";
			this.PackLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(730, 130, true);
			this.PackLinesGrid.TabIndex = 0;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 384, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 8, true);
			this.splitter1.TabIndex = 51;
			this.splitter1.TabStop = false;
			// 
			// zPanel2
			// 
			this.zPanel2.Controls.Add(this.SealIntactCheckBox);
			this.zPanel2.Controls.Add(this.SealNumTextBox);
			this.zPanel2.Controls.Add(this.JC_LCLStorageCommencesDateEdit);
			this.zPanel2.Controls.Add(this.JC_LCLAvailableDateEdit);
			this.zPanel2.Controls.Add(this.JC_LCLUnpackDateEdit);
			this.zPanel2.Controls.Add(this.JX_VoyageTextBox);
			this.zPanel2.Controls.Add(this.VesselCodeFindBox);
			this.zPanel2.Controls.Add(this.UnpackShedTextBox);
			this.zPanel2.Controls.Add(this.JC_UnpackGangTextBox);
			this.zPanel2.Controls.Add(this.JC_ContainerNumTextBox);
			this.zPanel2.Controls.Add(this.PCNNumTextBox);
			this.zPanel2.Controls.Add(this.CCNNumTextBox);
			this.zPanel2.Controls.Add(this.ImportLoadListNoTextBox);
			this.zPanel2.Controls.Add(this.LoadingPortTextBox);
			this.zPanel2.Controls.Add(this.DischargePortTextBox);
			this.zPanel2.Controls.Add(this.zOrganisationFindBox1);
			this.zPanel2.Controls.Add(this.ReceiptDateEdit);
			this.zPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel2.Name = "zPanel2";
			this.zPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 103, true);
			this.zPanel2.TabIndex = 0;
			// 
			// SealIntactCheckBox
			// 
			this.SealIntactCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SealIntactCheckBox, "JC_IsSealOk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((TallyContainer)(null)).JC_IsSealOk)));
			this.SealIntactCheckBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|c89cfb65-a3d3-4815-8312-29b1e027e369", "Seal Intact");
			this.SealIntactCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SealIntactCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 57, true);
			this.SealIntactCheckBox.Name = "SealIntactCheckBox";
			this.SealIntactCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 17, true);
			this.SealIntactCheckBox.TabIndex = 7;
			this.SealIntactCheckBox.UseVisualStyleBackColor = true;
			// 
			// SealNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.SealNumTextBox, "JC_SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).JC_SealNum)));
			this.SealNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 54, true);
			this.SealNumTextBox.Name = "SealNumTextBox";
			this.SealNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 20, true);
			this.SealNumTextBox.TabIndex = 6;
			// 
			// PCNNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.PCNNumTextBox, "CanadaPCNNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).CanadaPCNNumber)));
			this.PCNNumTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|f63a8f61-8b7a-4416-af01-a58527aae05a", "PCN");
			this.PCNNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(696, 80, true);
			this.PCNNumTextBox.Name = "PCNNumTextBox";
			this.PCNNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.PCNNumTextBox.TabIndex = 12;
			// 
			// CCNNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.CCNNumTextBox, "CanadaCCNNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TallyContainer)(null)).CanadaCCNNumber)));
			this.CCNNumTextBox.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("PackagesUserControl|f7b68070-7ff9-478b-a4c7-dbff079d22db", "CCN");
			this.CCNNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 80, true);
			this.CCNNumTextBox.Name = "CCNNumTextBox";
			this.CCNNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 20, true);
			this.CCNNumTextBox.TabIndex = 8;
			// 
			// PackagesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShipmentPanel);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.zPanel2);
			this.Name = "PackagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 576, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ShipmentsGrid)).EndInit();
			this.PacklineTotalsPanel.ResumeLayout(false);
			this.PacklineTotalsPanel.PerformLayout();
			this.PackLineTabPage.ResumeLayout(false);
			this.ShipmentDetailsTabControl.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.zPanel1.ResumeLayout(false);
			this.PackLineNotesTabControl.ResumeLayout(false);
			this.WarehouseTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.JobPackLocGrid)).EndInit();
			this.OutturnNotesTabPage.ResumeLayout(false);
			this.OutturnNotesTabPage.PerformLayout();
			this.MarksAndNumbersTabPage.ResumeLayout(false);
			this.MarksAndNumbersTabPage.PerformLayout();
			this.PackLinesPanel.ResumeLayout(false);
			this.OutturnTotalsPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackLinesGrid)).EndInit();
			this.zPanel2.ResumeLayout(false);
			this.zPanel2.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion
	}
}
