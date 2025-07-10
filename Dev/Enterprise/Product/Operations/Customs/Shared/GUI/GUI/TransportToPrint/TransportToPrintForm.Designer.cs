using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class TransportToPrintForm
	{
		#region Windows Form Designer generated code

		ZPanel BottomPanel;
		Enterprise.ZArchitecture.GUI.ZButton CancelFormButton;
		Enterprise.ZArchitecture.GUI.ZButton PrintSelectedButton;
		Enterprise.ZArchitecture.ZGrid TransportsGrid;
		System.ComponentModel.Container components = null;

		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo10 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo11 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo12 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo13 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelFormButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PrintSelectedButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TransportsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CancelFormButton);
			this.BottomPanel.Controls.Add(this.PrintSelectedButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 36, true);
			this.BottomPanel.TabIndex = 2;
			// 
			// CancelFormButton
			// 
			this.CancelFormButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelFormButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|945a7f42-597b-410d-a0ef-93c50c064edd", "&Cancel");
			this.CancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 8, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 1;
			// 
			// PrintSelectedButton
			// 
			this.PrintSelectedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintSelectedButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|7406fd90-f02d-4eb0-a38e-7d079da2031a", "&Print Selected");
			this.PrintSelectedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 8, true);
			this.PrintSelectedButton.Name = "PrintSelectedButton";
			this.PrintSelectedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.PrintSelectedButton.TabIndex = 0;
			this.PrintSelectedButton.Click += new System.EventHandler(this.PrintSelectedButton_Click);
			// 
			// TransportsGrid
			// 
			this.TransportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransportsGrid, "TransportsIncludingRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ParentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_LegOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_Vessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_VesselFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_VoyageFlight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_OA_DepartureLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_OA_ArrivalLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_RL_NKDiscPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ATD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ATA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalAvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DocumentaryCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotAvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).CarrierPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_CarrierBookingReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Business.Transport)(((System.Collections.IList)(((Enterprise.Freight.Business.DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_JX_JV_RegistrationNo)));
			this.TransportsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|0744505b-759d-4453-8d47-3b73b6c71543", "Defined by");
			zTextBoxColumnStyleInfo1.ColumnName = "JW_ParentDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|5ca8c3b7-e79e-42f2-bd6c-6e639480fa0a", "Leg");
			zCalcEditColumnStyleInfo1.ColumnName = "JW_LegOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|165e7382-3010-4b99-8945-7b42ab599e53", "Mode");
			zDropEditColumnStyleInfo1.ColumnName = "JW_TransportMode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|62ab7d5a-21ff-45f2-9e0a-7e18ee489a8e", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "JW_TransportType";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo3.ColumnName = "JW_Status";
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|5efea4b7-42e5-4638-91f5-0e1dbe2e6221", "Vessel / Journey");
			zMultiControlColumnStyleInfo1.ColumnName = "JW_Vessel";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "JW_VesselFieldType";
			zMultiControlColumnStyleInfo1.IsReadOnly = true;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "JW_VoyageFlight";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|3560c9e1-fa4f-49aa-9ef4-a44429ef98a1", "Voyage / Flight / Truck Ref. / Journey No.");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|a102e36e-b7d5-4cba-a96e-38959cbb2532", "Departure Location");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JW_OA_DepartureLocation";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|2fa7fe9e-cd15-4551-b21b-656c9f6d1af7", "Arrival Location");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JW_OA_ArrivalLocation";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|33ad1366-9475-46ed-94bb-efe18676a015", "Load");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JW_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|a2e82810-e7fa-4ee9-8c37-b077a14f9702", "Discharge");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JW_RL_NKDiscPort";
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|90e9d2da-bf4d-4d28-8816-9ce007636d0c", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JW_ETD";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|1dde3a87-e828-401c-9487-ee5b9a32cf57", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "JW_ETA";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|9502012e-b7da-4a3a-82c7-ce507066432b", "ATD");
			zDateEditColumnStyleInfo3.ColumnName = "JW_ATD";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|41c404fc-ae06-4e5e-a4f0-b66099732f0f", "ATA");
			zDateEditColumnStyleInfo4.ColumnName = "JW_ATA";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|a6c3dac2-bd24-4cf1-a2bf-6add8b533797", "CTO Availability");
			zDateEditColumnStyleInfo5.ColumnName = "JW_TerminalAvailabilityDate";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|0e81973f-d2f2-4890-bb8c-760aa9565448", "Docs Cut Off");
			zDateEditColumnStyleInfo6.ColumnName = "JW_DocumentaryCutOff";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|08653cd8-7d7c-4afe-93d1-182feaf23917", "CTO Cut Off");
			zDateEditColumnStyleInfo7.ColumnName = "JW_TerminalCutOff";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|9104615c-8c19-40d0-b0a6-b95f0feea97b", "CTO Receival Start");
			zDateEditColumnStyleInfo8.ColumnName = "JW_TerminalReceivalCommences";
			zDateEditColumnStyleInfo8.IsReadOnly = true;
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|51da1fe3-04d0-41b8-9fd6-053bc67d9c96", "CFS Availability");
			zDateEditColumnStyleInfo9.ColumnName = "JW_DepotAvailabilityDate";
			zDateEditColumnStyleInfo9.IsReadOnly = true;
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo10.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|064f26a0-133a-4aa2-a1d5-15f598a96344", "CFS Cut Off");
			zDateEditColumnStyleInfo10.ColumnName = "JW_DepotCutOff";
			zDateEditColumnStyleInfo10.IsReadOnly = true;
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|d1c89efa-b01a-411e-955d-23b0c3a0e652", "CFS Receival Start");
			zDateEditColumnStyleInfo11.ColumnName = "JW_DepotReceivalCommences";
			zDateEditColumnStyleInfo11.IsReadOnly = true;
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo12.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|fd923a9e-6030-4ee3-b140-e2fa6b5e4411", "CFS Storage Start");
			zDateEditColumnStyleInfo12.ColumnName = "JW_DepotStorageDate";
			zDateEditColumnStyleInfo12.IsReadOnly = true;
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|da89e0ef-5bd2-429d-96f0-cde382cd0752", "CTO Storage Start");
			zDateEditColumnStyleInfo13.ColumnName = "JW_TerminalStorageDate";
			zDateEditColumnStyleInfo13.IsReadOnly = true;
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|75a40886-dc35-4451-b746-cdcdbf78d354", "Carrier");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CarrierPK";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|758c912f-1fe7-4f71-aba6-b93977a56e52", "Carrier Ref.");
			zTextBoxColumnStyleInfo3.ColumnName = "JW_CarrierBookingReference";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|b43bab23-1a68-443a-8831-6b2ebe012a9d", "Aircraft Reg.");
			zTextBoxColumnStyleInfo4.ColumnName = "JW_JX_JV_RegistrationNo";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo9);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo10);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo11);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo12);
			this.TransportsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo13);
			this.TransportsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.TransportsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.TransportsGrid.GridId = "3a211d26-ad76-40e2-a5f6-3e31ed59acd9";
			this.TransportsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransportsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransportsGrid.IsWholeRowSelectedOnClick = true;
			this.TransportsGrid.LayoutKey = "TransportsGrid";
			this.TransportsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportsGrid.Name = "TransportsGrid";
			this.TransportsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.TransportsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 192, true);
			this.TransportsGrid.TabIndex = 4;
			// 
			// TransportToPrintForm
			// 
			this.AcceptButton = this.PrintSelectedButton;
			this.CancelButton = this.CancelFormButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 252, true);
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("TransportToPrintForm|809725ad-42ce-47ca-96ed-113f72bc96fc", "Select Routing");
			this.Controls.Add(this.TransportsGrid);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			this.DataSourceTypeName = "Enterprise.Customs.Business.BaseJobDeclaration";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 277, true);
			this.Name = "TransportToPrintForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TransportsGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TransportsGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
