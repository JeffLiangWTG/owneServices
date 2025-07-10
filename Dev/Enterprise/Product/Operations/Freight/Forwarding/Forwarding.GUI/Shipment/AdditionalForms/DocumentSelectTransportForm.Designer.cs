using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentSelectTransportForm : ZChildForm
	{
		ZPanel BottomPanel;
		ZButton CancelFormButton;
		ZButton PrintSelectedButton;
		private ZArchitecture.ZGrid TransportsGrid;

		protected new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
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
			MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new ZPanel();
			this.CancelFormButton = new ZButton();
			this.PrintSelectedButton = new ZButton();
			this.TransportsGrid = new ZArchitecture.ZGrid();
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
			this.BindingSource.DataSourceType = typeof(DocumentShipment);
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
			this.CancelFormButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelFormButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|4e25e306-5201-444e-8117-0f549da4c82e", "&Cancel");
			this.CancelFormButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelFormButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(477, 8, true);
			this.CancelFormButton.Name = "CancelFormButton";
			this.CancelFormButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelFormButton.TabIndex = 1;
			// 
			// PrintSelectedButton
			// 
			this.PrintSelectedButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PrintSelectedButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|ca15f067-9199-46c5-8a23-887952456618", "&Print Selected");
			this.PrintSelectedButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(377, 8, true);
			this.PrintSelectedButton.Name = "PrintSelectedButton";
			this.PrintSelectedButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 23, true);
			this.PrintSelectedButton.TabIndex = 0;
			this.PrintSelectedButton.Click += new System.EventHandler(this.PrintSelectedButton_Click);
			// 
			// TransportsGrid
			// 
			this.TransportsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.TransportsGrid, "Shipment+TransportsIncludingRelated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ParentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_LegOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TransportType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_Vessel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_VesselFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_VoyageFlight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_OA_DepartureLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_OA_ArrivalLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_RL_NKLoadPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_RL_NKDiscPort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ETD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ETA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ATD)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_ATA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalAvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DocumentaryCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotAvailabilityDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotCutOff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotReceivalCommences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_DepotStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_TerminalStorageDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).CarrierPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_CarrierBookingReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Transport)(((System.Collections.IList)(((DocumentShipment)(null)).Shipment.TransportsIncludingRelated)).SyncRoot)).JW_JX_JV_RegistrationNo)));
			this.TransportsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|c54fb442-8ad9-454f-b8f5-eb062eb5ac78", "Defined by");
			zTextBoxColumnStyleInfo1.ColumnName = "JW_ParentDescription";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|8022abd4-5a85-49c0-b7c2-807e673e0567", "Leg");
			zCalcEditColumnStyleInfo1.ColumnName = "JW_LegOrder";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|82b6a64c-cdb0-47ed-b653-99813d8950e9", "Mode");
			zDropEditColumnStyleInfo1.ColumnName = "JW_TransportMode";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|e4118772-5669-4296-84bc-99d5a20a7884", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "JW_TransportType";
			zDropEditColumnStyleInfo2.IsReadOnly = true;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zDropEditColumnStyleInfo3.ColumnName = "JW_Status";
			zDropEditColumnStyleInfo3.IsReadOnly = true;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|42d379a2-19ac-4b14-a7a2-f129e78a4845", "Vessel / Journey");
			zMultiControlColumnStyleInfo1.ColumnName = "JW_Vessel";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "JW_VesselFieldType";
			zMultiControlColumnStyleInfo1.IsReadOnly = true;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "JW_VoyageFlight";
			zTextBoxColumnStyleInfo2.GroupName = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|555b52cf-ddaa-433d-84b9-07f6c9231dc4", "Voyage / Flight / Truck Ref. / Journey No.");
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|6f943076-04aa-49b1-bb9f-19fd2cf7078c", "Departure Location");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JW_OA_DepartureLocation";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|e94cd3dc-1b91-4ca9-bc98-a8ae60b6e6cf", "Arrival Location");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JW_OA_ArrivalLocation";
			zOrganisationFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|98956f5f-25f1-411b-a6b4-efc32a692722", "Load");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "JW_RL_NKLoadPort";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|8651e817-66e5-4703-a321-7772c7806b1f", "Discharge");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "JW_RL_NKDiscPort";
			zCodeFindBoxColumnStyleInfo2.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|b8093e7f-2cd0-49f2-925b-817db9b33f45", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JW_ETD";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|175dc67f-c3f8-4bfd-a0b4-b0678bf969a2", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "JW_ETA";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|2de94fbd-9238-41ba-8c70-4fc9fa55db71", "ATD");
			zDateEditColumnStyleInfo3.ColumnName = "JW_ATD";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|826a46ef-1e2c-454f-8088-556a9fb7d7ad", "ATA");
			zDateEditColumnStyleInfo4.ColumnName = "JW_ATA";
			zDateEditColumnStyleInfo4.IsReadOnly = true;
			zDateEditColumnStyleInfo4.IsVisible = false;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|df577743-bf03-4c31-96e0-8aa8c77fe1be", "CTO Available");
			zDateEditColumnStyleInfo5.ColumnName = "JW_TerminalAvailabilityDate";
			zDateEditColumnStyleInfo5.IsReadOnly = true;
			zDateEditColumnStyleInfo5.IsVisible = false;
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|403ec092-ee11-4bcf-8ad5-14002bfecf52", "Docs Cut Off");
			zDateEditColumnStyleInfo6.ColumnName = "JW_DocumentaryCutOff";
			zDateEditColumnStyleInfo6.IsReadOnly = true;
			zDateEditColumnStyleInfo6.IsVisible = false;
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|f4a3ee87-6e8f-40e4-9d80-633fe282e086", "CTO Cut Off");
			zDateEditColumnStyleInfo7.ColumnName = "JW_TerminalCutOff";
			zDateEditColumnStyleInfo7.IsReadOnly = true;
			zDateEditColumnStyleInfo7.IsVisible = false;
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|0f8f2ea8-5321-42c0-be74-be821506544c", "CTO Receival", "CTO Receival Start");
			zDateEditColumnStyleInfo8.ColumnName = "JW_TerminalReceivalCommences";
			zDateEditColumnStyleInfo8.IsReadOnly = true;
			zDateEditColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|4266764e-764a-474e-9575-9caf9444a269", "CFS Available");
			zDateEditColumnStyleInfo9.ColumnName = "JW_DepotAvailabilityDate";
			zDateEditColumnStyleInfo9.IsReadOnly = true;
			zDateEditColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo10.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|dcc03893-69ae-4431-b221-a73364ba0ba8", "CFS Cut Off");
			zDateEditColumnStyleInfo10.ColumnName = "JW_DepotCutOff";
			zDateEditColumnStyleInfo10.IsReadOnly = true;
			zDateEditColumnStyleInfo10.IsVisible = false;
			zDateEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo11.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|70afa6c7-cd9b-4f51-848b-5d71bcda0735", "CFS Receival", "CFS Receival Start");
			zDateEditColumnStyleInfo11.ColumnName = "JW_DepotReceivalCommences";
			zDateEditColumnStyleInfo11.IsReadOnly = true;
			zDateEditColumnStyleInfo11.IsVisible = false;
			zDateEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo12.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|631f6084-b588-4e17-9fb2-308c97e2d796", "CFS Storage", "CFS Storage Start");
			zDateEditColumnStyleInfo12.ColumnName = "JW_DepotStorageDate";
			zDateEditColumnStyleInfo12.IsReadOnly = true;
			zDateEditColumnStyleInfo12.IsVisible = false;
			zDateEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo13.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|1dd25592-6d0c-40ae-86bf-2864c3dab4cf", "CTO Storage", "CTO Storage Start");
			zDateEditColumnStyleInfo13.ColumnName = "JW_TerminalStorageDate";
			zDateEditColumnStyleInfo13.IsReadOnly = true;
			zDateEditColumnStyleInfo13.IsVisible = false;
			zDateEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|7a825d72-8d54-4cea-94de-8d9782aafc39", "Carrier");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CarrierPK";
			zOrganisationFindBoxColumnStyleInfo2.IsReadOnly = true;
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|65db49a1-a221-49b9-890f-72f6a232539c", "Carrier Ref.");
			zTextBoxColumnStyleInfo3.ColumnName = "JW_CarrierBookingReference";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|ee3eda51-653f-465b-aa5b-15d2ee047bb5", "Aircraft Reg.");
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
			this.TransportsGrid.GridId = "fa8a3e59-5e25-43a4-8dfb-2875d4e18d2f";
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
			// DocumentSelectTransportForm
			// 
			this.AcceptButton = this.PrintSelectedButton;
			this.CancelButton = this.CancelFormButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(569, 252, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentSelectTransportForm|21cda80a-b432-4bbb-b1ae-65f8007c26e3", "Select Routing");
			this.Controls.Add(this.TransportsGrid);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(DocumentShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 277, true);
			this.Name = "DocumentSelectTransportForm";
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

		System.ComponentModel.Container components = null;
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
