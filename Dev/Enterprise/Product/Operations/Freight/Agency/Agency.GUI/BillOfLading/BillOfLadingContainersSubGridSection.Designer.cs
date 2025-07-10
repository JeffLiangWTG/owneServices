using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.GUI
{
	[SuppressMessage("Microsoft.Maintainability", "CA1505: Avoid unmaintainable code", Justification = "Auto generated code")]
	partial class BillOfLadingContainersSubGridSection
	{
		protected override void Dispose(bool IsNotFinalizing)
		{
			if (IsNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
					components = null;
				}
			}
			base.Dispose(IsNotFinalizing);
		}

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZTabPage ExportTabPage;
			Enterprise.ZArchitecture.GUI.ZDateEdit exportFullPickupDateEdit;
			Enterprise.ZArchitecture.ZTextBox fclOnBoardVesselPortTextBox;
			Enterprise.ZArchitecture.ZTextBox fclWharfGateInPortTextBox;
			Enterprise.ZArchitecture.ZTextBox containerYardEmptyPickupGateOutPortTextBox;
			Enterprise.ZArchitecture.GUI.ZDateEdit fclOnBoardVesselDateEdit;
			Enterprise.ZArchitecture.GUI.ZAddressControl exportPickupEmptyFromAddressControl;
			Enterprise.ZArchitecture.GUI.ZDateEdit exportEmptyReqByDateEdit;
			Enterprise.ZArchitecture.GUI.ZDateEdit containerYardEmptyPickupGateOutDateEdit;
			Enterprise.ZArchitecture.GUI.ZDateEdit wharfGateInDateEdit;
			Enterprise.ZArchitecture.GUI.ZCheckBox isArrivingAtCTOByRailCheckBox;
			Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
			Enterprise.Messaging.GUI.EDIMessageUserControl messagesControl;
			Enterprise.ZArchitecture.GUI.ZGroupBox detailsGroupBox;
			Enterprise.ZArchitecture.GUI.ZCodeFindBox commodityCodeFindBox;
			Enterprise.ZArchitecture.GUI.ZCheckBox exportIsDamagedCheckBox;
			Enterprise.ZArchitecture.GUI.ZCheckBox exportIsEmptyContainerCheckBox;
			Enterprise.ZArchitecture.GUI.ZGuidFindBox importContainerTypeGuidFindBox;
			Enterprise.ZArchitecture.GUI.ZDropEdit importContainerModeDropEdit;
			Enterprise.ZArchitecture.ZTextBox secondSealTextBox;
			Enterprise.ZArchitecture.ZTextBox exportContainerNumberTextBox;
			Enterprise.ZArchitecture.ZTextBox exportSealNumberTextBox;
			CargoWise.Windows.UI.KPanel detailsPanel;
			Enterprise.ZArchitecture.ZTextBox StowagePositionTextBox;
			Enterprise.ZArchitecture.GUI.ZTabControl containersTabControl;
			Enterprise.ZArchitecture.GUI.ZTabPage ImportTabPage;
			Enterprise.ZArchitecture.ZTextBox containerYardEmptyReturnGateInPortTextBox;
			Enterprise.ZArchitecture.ZTextBox fclWharfGateOutPortTextBox;
			Enterprise.ZArchitecture.ZTextBox fclUnloadFromVesselPortTextBox;
			Enterprise.ZArchitecture.GUI.ZAddressControl importReturnEmptyToAddressControl;
			Enterprise.ZArchitecture.GUI.ZDateEdit fclWharfGateOutDateEdit;
			Enterprise.ZArchitecture.GUI.ZDateEdit importEmptyWasReturnedOnDateEdit;
			Enterprise.ZArchitecture.GUI.ZDateEdit importEmptyReturnByDateEdit;
			Enterprise.ZArchitecture.GUI.ZDateEdit fclUnloadFromVesselDateEdit;
			Enterprise.ZArchitecture.GUI.ZTabPage MovementsTabPage;
			Enterprise.Freight.Agency.GUI.BillOfLadingMovementsControl movementsControl;
			this.DockReceiptTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.slotBookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.slotDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.emptyReleaseNumTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.pickupFromCustDoorDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.isShipperOwnedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.additionalSealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.additional2SealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.sealPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.thirdSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WeightsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TareOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ActualTareWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualNetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualDunnageCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ActualWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalGrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OnFileWeightLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MaxGrossWeightOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReeferTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.reeferUserControl = new Enterprise.Freight.GUI.ReeferUserControl();
			this.MeasuresTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OverhangRightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangLeftCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangBackCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverhangFrontCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentTotalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AddDataLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ShipmentTotalVolumeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StandardDataHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasuresCalculatedLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StandardDataLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasuresLowerOnfileLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StandardWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CalculatedVolumeOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDataHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxVolumeOnFileCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AddDataWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasuresOverhangLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverhangHeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasuresActualLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverhangLengthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MeasuresOnfileLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OverhangWidthCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.estimatedFullDeliveryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.deliverToCustomerDoorDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.releaseNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.impSlotBookingRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.impSlotDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VGMTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.VerifiedByDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.VerifiedWeightGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VerifiedDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.VerifiedMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.NumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.numbersControl1 = new Enterprise.MasterFiles.GUI.NumbersControl();
			ExportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			exportFullPickupDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			fclOnBoardVesselPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			fclWharfGateInPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			containerYardEmptyPickupGateOutPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			fclOnBoardVesselDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			exportPickupEmptyFromAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			exportEmptyReqByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			containerYardEmptyPickupGateOutDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			wharfGateInDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			isArrivingAtCTOByRailCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			messagesControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			commodityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			exportIsDamagedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			exportIsEmptyContainerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			importContainerTypeGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			importContainerModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			secondSealTextBox = new Enterprise.ZArchitecture.ZTextBox();
			exportContainerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			exportSealNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			detailsPanel = new CargoWise.Windows.UI.KPanel();
			StowagePositionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			containersTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			ImportTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			containerYardEmptyReturnGateInPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			fclWharfGateOutPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			fclUnloadFromVesselPortTextBox = new Enterprise.ZArchitecture.ZTextBox();
			importReturnEmptyToAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			fclWharfGateOutDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			importEmptyWasReturnedOnDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			importEmptyReturnByDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			fclUnloadFromVesselDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			MovementsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			movementsControl = new Enterprise.Freight.Agency.GUI.BillOfLadingMovementsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.slotDateEdit.SuspendLayout();
			this.pickupFromCustDoorDateEdit.SuspendLayout();
			this.additionalSealPartyDropEdit.SuspendLayout();
			this.additional2SealPartyDropEdit.SuspendLayout();
			this.sealPartyDropEdit.SuspendLayout();
			this.WeightsGroupBox.SuspendLayout();
			this.GrossWeightUQDropEdit.SuspendLayout();
			this.ReeferTabPage.SuspendLayout();
			this.reeferUserControl.SuspendLayout();
			this.MeasuresTabPage.SuspendLayout();
			this.estimatedFullDeliveryDateEdit.SuspendLayout();
			this.deliverToCustomerDoorDateEdit.SuspendLayout();
			this.impSlotDateEdit.SuspendLayout();
			this.VGMTabPage.SuspendLayout();
			this.VerifiedByDocAddressControl.SuspendLayout();
			this.VerifiedWeightGroupBox.SuspendLayout();
			this.VerifiedDateEdit.SuspendLayout();
			this.VerifiedMethodDropEdit.SuspendLayout();
			ExportTabPage.SuspendLayout();
			exportFullPickupDateEdit.SuspendLayout();
			fclOnBoardVesselDateEdit.SuspendLayout();
			exportPickupEmptyFromAddressControl.SuspendLayout();
			exportEmptyReqByDateEdit.SuspendLayout();
			containerYardEmptyPickupGateOutDateEdit.SuspendLayout();
			wharfGateInDateEdit.SuspendLayout();
			MessagesTabPage.SuspendLayout();
			messagesControl.SuspendLayout();
			detailsGroupBox.SuspendLayout();
			commodityCodeFindBox.SuspendLayout();
			importContainerTypeGuidFindBox.SuspendLayout();
			importContainerModeDropEdit.SuspendLayout();
			detailsPanel.SuspendLayout();
			containersTabControl.SuspendLayout();
			ImportTabPage.SuspendLayout();
			importReturnEmptyToAddressControl.SuspendLayout();
			fclWharfGateOutDateEdit.SuspendLayout();
			importEmptyWasReturnedOnDateEdit.SuspendLayout();
			importEmptyReturnByDateEdit.SuspendLayout();
			fclUnloadFromVesselDateEdit.SuspendLayout();
			MovementsTabPage.SuspendLayout();
			movementsControl.SuspendLayout();
			NumbersTabPage.SuspendLayout();
			numbersControl1.SuspendLayout();
			SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.BillOfLading);
			// 
			// DockReceiptTextBox
			// 
			this.BindingSource.SetBindingMember(this.DockReceiptTextBox, "FCLContainers.JC_DepartureDockReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureDockReceipt)));
			this.DockReceiptTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 187, true);
			this.DockReceiptTextBox.Name = "DockReceiptTextBox";
			this.DockReceiptTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.DockReceiptTextBox.TabIndex = 11;
			// 
			// slotBookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.slotBookingRefTextBox, "FCLContainers.JC_DepartureSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureSlotReference)));
			this.slotBookingRefTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("73f2380e-51eb-4297-8239-ecbf28298c99", "Slot Bkg. Ref", "Slot Booking Ref", "Slot Booking Reference", "");
			this.slotBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 135, true);
			this.slotBookingRefTextBox.Name = "slotBookingRefTextBox";
			this.slotBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.slotBookingRefTextBox.TabIndex = 8;
			// 
			// slotDateEdit
			// 
			this.slotDateEdit.AllowDrop = true;
			this.slotDateEdit.AutoCompleteMonthThreshold = 1;
			this.slotDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.slotDateEdit, "FCLContainers.JC_DepartureSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureSlotDateTime)));
			this.slotDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("a277becc-99e5-4f93-a0e0-2503846506aa", "Slot Date");
			this.slotDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.slotDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 135, true);
			this.slotDateEdit.Name = "slotDateEdit";
			this.slotDateEdit.TabIndex = 7;
			// 
			// emptyReleaseNumTextBox
			// 
			this.BindingSource.SetBindingMember(this.emptyReleaseNumTextBox, "FCLContainers.JC_ReleaseNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ReleaseNum)));
			this.emptyReleaseNumTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7d810ac6-d7a9-466c-9a5a-ce1347c2041d", "Empty Rel. #", "Empty Release Num.", "Empty Release Number", "");
			this.emptyReleaseNumTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 59, true);
			this.emptyReleaseNumTextBox.Name = "emptyReleaseNumTextBox";
			this.emptyReleaseNumTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 17, true);
			this.emptyReleaseNumTextBox.TabIndex = 2;
			// 
			// pickupFromCustDoorDateEdit
			// 
			this.pickupFromCustDoorDateEdit.AllowDrop = true;
			this.pickupFromCustDoorDateEdit.AutoCompleteMonthThreshold = 1;
			this.pickupFromCustDoorDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.pickupFromCustDoorDateEdit, "FCLContainers.JC_DepartureCartageComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureCartageComplete)));
			this.pickupFromCustDoorDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("b839061d-e0e4-4703-86c8-ebb8f8707b4c", "Pickup", "Pickup From Customer", "Pickup From Customer Door", "");
			this.pickupFromCustDoorDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.pickupFromCustDoorDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 109, true);
			this.pickupFromCustDoorDateEdit.Name = "pickupFromCustDoorDateEdit";
			this.pickupFromCustDoorDateEdit.TabIndex = 6;
			// 
			// isShipperOwnedCheckBox
			// 
			this.isShipperOwnedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isShipperOwnedCheckBox, "FCLContainers.JC_IsShipperOwned");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_IsShipperOwned)));
			this.isShipperOwnedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isShipperOwnedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 8, true);
			this.isShipperOwnedCheckBox.Name = "isShipperOwnedCheckBox";
			this.isShipperOwnedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.isShipperOwnedCheckBox.TabIndex = 0;
			// 
			// additionalSealPartyDropEdit
			// 
			this.additionalSealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additionalSealPartyDropEdit, "FCLContainers.JC_AdditionalSealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_AdditionalSealParty)));
			this.additionalSealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 69, true);
			this.additionalSealPartyDropEdit.Name = "additionalSealPartyDropEdit";
			this.additionalSealPartyDropEdit.PreBoundMaxLength = 3;
			this.additionalSealPartyDropEdit.ShowDescriptionBox = false;
			this.additionalSealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.additionalSealPartyDropEdit.TabIndex = 7;
			// 
			// additional2SealPartyDropEdit
			// 
			this.additional2SealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.additional2SealPartyDropEdit, "FCLContainers.JC_Additional2SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Additional2SealParty)));
			this.additional2SealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 91, true);
			this.additional2SealPartyDropEdit.Name = "additional2SealPartyDropEdit";
			this.additional2SealPartyDropEdit.PreBoundMaxLength = 3;
			this.additional2SealPartyDropEdit.ShowDescriptionBox = false;
			this.additional2SealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.additional2SealPartyDropEdit.TabIndex = 9;
			// 
			// sealPartyDropEdit
			// 
			this.sealPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sealPartyDropEdit, "FCLContainers.JC_SealParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_SealParty)));
			this.sealPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 47, true);
			this.sealPartyDropEdit.Name = "sealPartyDropEdit";
			this.sealPartyDropEdit.PreBoundMaxLength = 3;
			this.sealPartyDropEdit.ShowDescriptionBox = false;
			this.sealPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			this.sealPartyDropEdit.TabIndex = 5;
			// 
			// thirdSealTextBox
			// 
			this.BindingSource.SetBindingMember(this.thirdSealTextBox, "FCLContainers.JC_Additional2SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Additional2SealNum)));
			this.thirdSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 91, true);
			this.thirdSealTextBox.Name = "thirdSealTextBox";
			this.thirdSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			this.thirdSealTextBox.TabIndex = 8;
			// 
			// WeightsGroupBox
			// 
			this.WeightsGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|83b3d9f2-440b-496b-a3cd-4de8ee643b65", "Weights");
			this.WeightsGroupBox.Controls.Add(this.TareOnFileCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.GrossWeightUQDropEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualTareWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualNetWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualDunnageCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.ActualWeightLabel);
			this.WeightsGroupBox.Controls.Add(this.TotalGrossWeightCalcEdit);
			this.WeightsGroupBox.Controls.Add(this.OnFileWeightLabel);
			this.WeightsGroupBox.Controls.Add(this.MaxGrossWeightOnFileCalcEdit);
			this.WeightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 113, true);
			this.WeightsGroupBox.Name = "WeightsGroupBox";
			this.WeightsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 129, true);
			this.WeightsGroupBox.TabIndex = 10;
			this.WeightsGroupBox.TabStop = false;
			// 
			// TareOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TareOnFileCalcEdit, "FCLContainers.JC_Calc_TareWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_TareWeight)));
			this.TareOnFileCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|b6754337-a9ea-4776-b37d-dfdfc34967d2", "Tare");
			this.TareOnFileCalcEdit.DecimalPlaces = 2;
			this.TareOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 23, true);
			this.TareOnFileCalcEdit.Name = "TareOnFileCalcEdit";
			this.TareOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.TareOnFileCalcEdit.TabIndex = 8;
			this.TareOnFileCalcEdit.Text = "0.000";
			this.TareOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightUQDropEdit
			// 
			this.GrossWeightUQDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightUQDropEdit, "FCLContainers.WeightUnitForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).WeightUnitForBinding)));
			this.GrossWeightUQDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|52cfa1d0-a071-4aeb-b19f-593092119343", "Weight UQ");
			this.GrossWeightUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 107, true);
			this.GrossWeightUQDropEdit.Name = "GrossWeightUQDropEdit";
			this.GrossWeightUQDropEdit.PreBoundMaxLength = 4;
			this.GrossWeightUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.GrossWeightUQDropEdit.TabIndex = 5;
			// 
			// ActualTareWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualTareWeightCalcEdit, "FCLContainers.JC_TareWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_TareWeight)));
			this.ActualTareWeightCalcEdit.DecimalPlaces = 2;
			this.ActualTareWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 23, true);
			this.ActualTareWeightCalcEdit.Name = "ActualTareWeightCalcEdit";
			this.ActualTareWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualTareWeightCalcEdit.TabIndex = 3;
			this.ActualTareWeightCalcEdit.Text = "0.000";
			this.ActualTareWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualNetWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualNetWeightCalcEdit, "FCLContainers.GoodsWeightForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).GoodsWeightForBinding)));
			this.ActualNetWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|bbc17990-1fe5-4cbc-92a1-92ade6642d73", "Goods Wgt.");
			this.ActualNetWeightCalcEdit.DecimalPlaces = 2;
			this.ActualNetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 44, true);
			this.ActualNetWeightCalcEdit.Name = "ActualNetWeightCalcEdit";
			this.ActualNetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualNetWeightCalcEdit.TabIndex = 4;
			this.ActualNetWeightCalcEdit.Text = "0.000";
			this.ActualNetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualDunnageCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualDunnageCalcEdit, "FCLContainers.JC_DunnageWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DunnageWeight)));
			this.ActualDunnageCalcEdit.DecimalPlaces = 2;
			this.ActualDunnageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 65, true);
			this.ActualDunnageCalcEdit.Name = "ActualDunnageCalcEdit";
			this.ActualDunnageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.ActualDunnageCalcEdit.TabIndex = 6;
			this.ActualDunnageCalcEdit.Text = "0.000";
			this.ActualDunnageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ActualWeightLabel
			// 
			this.BindingSource.SetBindingMember(this.ActualWeightLabel, "FCLContainers.ActualWeightUnitText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).ActualWeightUnitText)));
			this.ActualWeightLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|2bf9a42f-4d69-4e8e-9552-7acdb4e3bc55", "Actual (kg)");
			this.ActualWeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActualWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 8, true);
			this.ActualWeightLabel.Name = "ActualWeightLabel";
			this.ActualWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 13, true);
			this.ActualWeightLabel.TabIndex = 0;
			this.ActualWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// TotalGrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalGrossWeightCalcEdit, "FCLContainers.JC_GrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_GrossWeight)));
			this.TotalGrossWeightCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|aa054742-58da-45d2-ba74-e4b375b2267a", "Gross Wgt.");
			this.TotalGrossWeightCalcEdit.DecimalPlaces = 2;
			this.TotalGrossWeightCalcEdit.IsCalculatorEnabled = false;
			this.TotalGrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 86, true);
			this.TotalGrossWeightCalcEdit.Name = "TotalGrossWeightCalcEdit";
			this.TotalGrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.TotalGrossWeightCalcEdit.TabIndex = 7;
			this.TotalGrossWeightCalcEdit.Text = "0.000";
			this.TotalGrossWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OnFileWeightLabel
			// 
			this.BindingSource.SetBindingMember(this.OnFileWeightLabel, "FCLContainers.OnFileWeightUnitText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).OnFileWeightUnitText)));
			this.OnFileWeightLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|e5025228-9220-40db-a725-45bf70dd6f8b", "On File (kg)");
			this.OnFileWeightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OnFileWeightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 8, true);
			this.OnFileWeightLabel.Name = "OnFileWeightLabel";
			this.OnFileWeightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 13, true);
			this.OnFileWeightLabel.TabIndex = 1;
			this.OnFileWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// MaxGrossWeightOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxGrossWeightOnFileCalcEdit, "FCLContainers.JC_Calc_MaxGrossWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_MaxGrossWeight)));
			this.MaxGrossWeightOnFileCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|ee8f88f8-5f61-4172-959d-b9153d0f6023", "Max");
			this.MaxGrossWeightOnFileCalcEdit.DecimalPlaces = 2;
			this.MaxGrossWeightOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 86, true);
			this.MaxGrossWeightOnFileCalcEdit.Name = "MaxGrossWeightOnFileCalcEdit";
			this.MaxGrossWeightOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 17, true);
			this.MaxGrossWeightOnFileCalcEdit.TabIndex = 8;
			this.MaxGrossWeightOnFileCalcEdit.Text = "0.000";
			this.MaxGrossWeightOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReeferTabPage
			// 
			this.ReeferTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("69ecfb2e-81fc-4725-a51a-eafff09cb8a8", "Refrigeration");
			this.ReeferTabPage.Controls.Add(this.reeferUserControl);
			this.ReeferTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ReeferTabPage.Name = "ReeferTabPage";
			this.ReeferTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ReeferTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			this.ReeferTabPage.TabIndex = 0;
			// 
			// reeferUserControl
			// 
			this.reeferUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reeferUserControl, "FCLContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Business.CommonContainer)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)))));
			this.reeferUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.reeferUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.reeferUserControl.Name = "reeferUserControl";
			this.reeferUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 301, true);
			this.reeferUserControl.TabIndex = 0;
			// 
			// MeasuresTabPage
			// 
			this.MeasuresTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("f03b4898-48b6-40dd-a7b6-bb0eac5cc6e3", "Measures");
			this.MeasuresTabPage.Controls.Add(this.OverhangRightCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.OverhangLeftCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.OverhangBackCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.OverhangFrontCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.ShipmentTotalLabel);
			this.MeasuresTabPage.Controls.Add(this.AddDataLengthCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.ShipmentTotalVolumeCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.StandardDataHeightCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MeasuresCalculatedLabel);
			this.MeasuresTabPage.Controls.Add(this.StandardDataLengthCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MeasuresLowerOnfileLabel);
			this.MeasuresTabPage.Controls.Add(this.StandardWidthCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.CalculatedVolumeOnFileCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.AddDataHeightCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MaxVolumeOnFileCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.AddDataWidthCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MeasuresOverhangLabel);
			this.MeasuresTabPage.Controls.Add(this.OverhangHeightCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MeasuresActualLabel);
			this.MeasuresTabPage.Controls.Add(this.OverhangLengthCalcEdit);
			this.MeasuresTabPage.Controls.Add(this.MeasuresOnfileLabel);
			this.MeasuresTabPage.Controls.Add(this.OverhangWidthCalcEdit);
			this.MeasuresTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MeasuresTabPage.Name = "MeasuresTabPage";
			this.MeasuresTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MeasuresTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			this.MeasuresTabPage.TabIndex = 6;
			// 
			// OverhangRightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangRightCalcEdit, "FCLContainers.JC_OverhangRight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_OverhangRight)));
			this.OverhangRightCalcEdit.DecimalPlaces = 3;
			this.OverhangRightCalcEdit.Decimals = 3;
			this.OverhangRightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 120, true);
			this.OverhangRightCalcEdit.Name = "OverhangRightCalcEdit";
			this.OverhangRightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangRightCalcEdit.TabIndex = 12;
			this.OverhangRightCalcEdit.Text = "0.000";
			this.OverhangRightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangLeftCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangLeftCalcEdit, "FCLContainers.JC_Calc_OverhangLeft");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_OverhangLeft)));
			this.OverhangLeftCalcEdit.DecimalPlaces = 3;
			this.OverhangLeftCalcEdit.Decimals = 3;
			this.OverhangLeftCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 120, true);
			this.OverhangLeftCalcEdit.Name = "OverhangLeftCalcEdit";
			this.OverhangLeftCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangLeftCalcEdit.TabIndex = 11;
			this.OverhangLeftCalcEdit.Text = "0.000";
			this.OverhangLeftCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangBackCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangBackCalcEdit, "FCLContainers.JC_OverhangBack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_OverhangBack)));
			this.OverhangBackCalcEdit.DecimalPlaces = 3;
			this.OverhangBackCalcEdit.Decimals = 3;
			this.OverhangBackCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 72, true);
			this.OverhangBackCalcEdit.Name = "OverhangBackCalcEdit";
			this.OverhangBackCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangBackCalcEdit.TabIndex = 7;
			this.OverhangBackCalcEdit.Text = "0.000";
			this.OverhangBackCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverhangFrontCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangFrontCalcEdit, "FCLContainers.JC_Calc_OverhangFront");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_OverhangFront)));
			this.OverhangFrontCalcEdit.DecimalPlaces = 3;
			this.OverhangFrontCalcEdit.Decimals = 3;
			this.OverhangFrontCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 72, true);
			this.OverhangFrontCalcEdit.Name = "OverhangFrontCalcEdit";
			this.OverhangFrontCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangFrontCalcEdit.TabIndex = 6;
			this.OverhangFrontCalcEdit.Text = "0.000";
			this.OverhangFrontCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalLabel
			// 
			this.ShipmentTotalLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7b969aeb-b80c-4280-9c1c-d009456e43c1", "Shipment Total");
			this.ShipmentTotalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ShipmentTotalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(247, 144, true);
			this.ShipmentTotalLabel.Name = "ShipmentTotalLabel";
			this.ShipmentTotalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 12, true);
			this.ShipmentTotalLabel.TabIndex = 11;
			this.ShipmentTotalLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// AddDataLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataLengthCalcEdit, "FCLContainers.JC_TotalLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_TotalLength)));
			this.AddDataLengthCalcEdit.DecimalPlaces = 3;
			this.AddDataLengthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataLengthCalcEdit, false);
			this.AddDataLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 48, true);
			this.AddDataLengthCalcEdit.Name = "AddDataLengthCalcEdit";
			this.AddDataLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.AddDataLengthCalcEdit.TabIndex = 4;
			this.AddDataLengthCalcEdit.Text = "0.000";
			this.AddDataLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ShipmentTotalVolumeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ShipmentTotalVolumeCalcEdit, "FCLContainers.JC_Calc_TotalVolumeInM3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_TotalVolumeInM3)));
			this.ShipmentTotalVolumeCalcEdit.DecimalPlaces = 2;
			this.ShipmentTotalVolumeCalcEdit.IsCalculatorEnabled = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ShipmentTotalVolumeCalcEdit, false);
			this.ShipmentTotalVolumeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 157, true);
			this.ShipmentTotalVolumeCalcEdit.Name = "ShipmentTotalVolumeCalcEdit";
			this.ShipmentTotalVolumeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.ShipmentTotalVolumeCalcEdit.TabIndex = 15;
			this.ShipmentTotalVolumeCalcEdit.Text = "0.000";
			this.ShipmentTotalVolumeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StandardDataHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardDataHeightCalcEdit, "FCLContainers.JC_Calc_Height");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_Height)));
			this.StandardDataHeightCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("a7b9fb47-a531-4c96-ac67-ea28a586e432", "Height (ft.)");
			this.StandardDataHeightCalcEdit.DecimalPlaces = 3;
			this.StandardDataHeightCalcEdit.Decimals = 3;
			this.StandardDataHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 24, true);
			this.StandardDataHeightCalcEdit.Name = "StandardDataHeightCalcEdit";
			this.StandardDataHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.StandardDataHeightCalcEdit.TabIndex = 0;
			this.StandardDataHeightCalcEdit.Text = "0.000";
			this.StandardDataHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasuresCalculatedLabel
			// 
			this.MeasuresCalculatedLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("4ba4a1e2-7e9d-4e3b-b405-070cfbb1d646", "Calculated");
			this.MeasuresCalculatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MeasuresCalculatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 144, true);
			this.MeasuresCalculatedLabel.Name = "MeasuresCalculatedLabel";
			this.MeasuresCalculatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 12, true);
			this.MeasuresCalculatedLabel.TabIndex = 10;
			this.MeasuresCalculatedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StandardDataLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardDataLengthCalcEdit, "FCLContainers.JC_Calc_Length");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_Length)));
			this.StandardDataLengthCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ca732229-a6fa-423d-945c-b479c5f9b81a", "Length (ft.)");
			this.StandardDataLengthCalcEdit.DecimalPlaces = 3;
			this.StandardDataLengthCalcEdit.Decimals = 3;
			this.StandardDataLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 48, true);
			this.StandardDataLengthCalcEdit.Name = "StandardDataLengthCalcEdit";
			this.StandardDataLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.StandardDataLengthCalcEdit.TabIndex = 3;
			this.StandardDataLengthCalcEdit.Text = "0.000";
			this.StandardDataLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasuresLowerOnfileLabel
			// 
			this.MeasuresLowerOnfileLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7a1d5d9f-c540-44cc-ba50-381f5108a02d", "On File");
			this.MeasuresLowerOnfileLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MeasuresLowerOnfileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 144, true);
			this.MeasuresLowerOnfileLabel.Name = "MeasuresLowerOnfileLabel";
			this.MeasuresLowerOnfileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 12, true);
			this.MeasuresLowerOnfileLabel.TabIndex = 9;
			this.MeasuresLowerOnfileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// StandardWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.StandardWidthCalcEdit, "FCLContainers.JC_Calc_Width");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_Width)));
			this.StandardWidthCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("f61be755-c602-41ea-8b69-9efe8254227c", "Width (ft.)");
			this.StandardWidthCalcEdit.DecimalPlaces = 3;
			this.StandardWidthCalcEdit.Decimals = 3;
			this.StandardWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 96, true);
			this.StandardWidthCalcEdit.Name = "StandardWidthCalcEdit";
			this.StandardWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.StandardWidthCalcEdit.TabIndex = 8;
			this.StandardWidthCalcEdit.Text = "0.000";
			this.StandardWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CalculatedVolumeOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CalculatedVolumeOnFileCalcEdit, "FCLContainers.JC_Calc_ActualCapacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_ActualCapacity)));
			this.CalculatedVolumeOnFileCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CalculatedVolumeOnFileCalcEdit, false);
			this.CalculatedVolumeOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 157, true);
			this.CalculatedVolumeOnFileCalcEdit.Name = "CalculatedVolumeOnFileCalcEdit";
			this.CalculatedVolumeOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.CalculatedVolumeOnFileCalcEdit.TabIndex = 14;
			this.CalculatedVolumeOnFileCalcEdit.Text = "0.000";
			this.CalculatedVolumeOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddDataHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataHeightCalcEdit, "FCLContainers.JC_TotalHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_TotalHeight)));
			this.AddDataHeightCalcEdit.DecimalPlaces = 3;
			this.AddDataHeightCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataHeightCalcEdit, false);
			this.AddDataHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 24, true);
			this.AddDataHeightCalcEdit.Name = "AddDataHeightCalcEdit";
			this.AddDataHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.AddDataHeightCalcEdit.TabIndex = 1;
			this.AddDataHeightCalcEdit.Text = "0.000";
			this.AddDataHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxVolumeOnFileCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxVolumeOnFileCalcEdit, "FCLContainers.JC_Calc_ContainerCapacity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_ContainerCapacity)));
			this.MaxVolumeOnFileCalcEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ff848959-089c-4841-88dc-546832f46980", "Vol (M3)");
			this.MaxVolumeOnFileCalcEdit.DecimalPlaces = 2;
			this.MaxVolumeOnFileCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 157, true);
			this.MaxVolumeOnFileCalcEdit.Name = "MaxVolumeOnFileCalcEdit";
			this.MaxVolumeOnFileCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.MaxVolumeOnFileCalcEdit.TabIndex = 13;
			this.MaxVolumeOnFileCalcEdit.Text = "0.000";
			this.MaxVolumeOnFileCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AddDataWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AddDataWidthCalcEdit, "FCLContainers.JC_TotalWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_TotalWidth)));
			this.AddDataWidthCalcEdit.DecimalPlaces = 3;
			this.AddDataWidthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AddDataWidthCalcEdit, false);
			this.AddDataWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 96, true);
			this.AddDataWidthCalcEdit.Name = "AddDataWidthCalcEdit";
			this.AddDataWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.AddDataWidthCalcEdit.TabIndex = 9;
			this.AddDataWidthCalcEdit.Text = "0.000";
			this.AddDataWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasuresOverhangLabel
			// 
			this.MeasuresOverhangLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("5cba417e-894b-4e94-b374-f1d910998e29", "Overhang");
			this.MeasuresOverhangLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MeasuresOverhangLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 8, true);
			this.MeasuresOverhangLabel.Name = "MeasuresOverhangLabel";
			this.MeasuresOverhangLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.MeasuresOverhangLabel.TabIndex = 2;
			this.MeasuresOverhangLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OverhangHeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangHeightCalcEdit, "FCLContainers.JC_Calc_OverhangHeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_OverhangHeight)));
			this.OverhangHeightCalcEdit.DecimalPlaces = 3;
			this.OverhangHeightCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangHeightCalcEdit, false);
			this.OverhangHeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 24, true);
			this.OverhangHeightCalcEdit.Name = "OverhangHeightCalcEdit";
			this.OverhangHeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangHeightCalcEdit.TabIndex = 2;
			this.OverhangHeightCalcEdit.Text = "0.000";
			this.OverhangHeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasuresActualLabel
			// 
			this.MeasuresActualLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("9bba9942-64df-4054-98c4-28e5b02b0f11", "Actual");
			this.MeasuresActualLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MeasuresActualLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 8, true);
			this.MeasuresActualLabel.Name = "MeasuresActualLabel";
			this.MeasuresActualLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 16, true);
			this.MeasuresActualLabel.TabIndex = 1;
			this.MeasuresActualLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OverhangLengthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangLengthCalcEdit, "FCLContainers.JC_Calc_OverhangLength");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_OverhangLength)));
			this.OverhangLengthCalcEdit.DecimalPlaces = 3;
			this.OverhangLengthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangLengthCalcEdit, false);
			this.OverhangLengthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 48, true);
			this.OverhangLengthCalcEdit.Name = "OverhangLengthCalcEdit";
			this.OverhangLengthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangLengthCalcEdit.TabIndex = 5;
			this.OverhangLengthCalcEdit.Text = "0.000";
			this.OverhangLengthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MeasuresOnfileLabel
			// 
			this.MeasuresOnfileLabel.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("9e4a0b66-a32a-44a8-bef2-e90a93d12bc6", "On File");
			this.MeasuresOnfileLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MeasuresOnfileLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 7, true);
			this.MeasuresOnfileLabel.Name = "MeasuresOnfileLabel";
			this.MeasuresOnfileLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 16, true);
			this.MeasuresOnfileLabel.TabIndex = 0;
			this.MeasuresOnfileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// OverhangWidthCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.OverhangWidthCalcEdit, "FCLContainers.JC_Calc_OverhangWidth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_Calc_OverhangWidth)));
			this.OverhangWidthCalcEdit.DecimalPlaces = 3;
			this.OverhangWidthCalcEdit.Decimals = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OverhangWidthCalcEdit, false);
			this.OverhangWidthCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 96, true);
			this.OverhangWidthCalcEdit.Name = "OverhangWidthCalcEdit";
			this.OverhangWidthCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 17, true);
			this.OverhangWidthCalcEdit.TabIndex = 10;
			this.OverhangWidthCalcEdit.Text = "0.000";
			this.OverhangWidthCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// estimatedFullDeliveryDateEdit
			// 
			this.estimatedFullDeliveryDateEdit.AllowDrop = true;
			this.estimatedFullDeliveryDateEdit.AutoCompleteMonthThreshold = 1;
			this.estimatedFullDeliveryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.estimatedFullDeliveryDateEdit, "FCLContainers.JC_ArrivalEstimatedDelivery");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ArrivalEstimatedDelivery)));
			this.estimatedFullDeliveryDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("4a7b7772-8ef2-4294-9713-49a027e212f4", "Estimated Full Delivery", "Estimated Full Delivery", "Estimated Full Delivery", "");
			this.estimatedFullDeliveryDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.estimatedFullDeliveryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 110, true);
			this.estimatedFullDeliveryDateEdit.Name = "estimatedFullDeliveryDateEdit";
			this.estimatedFullDeliveryDateEdit.TabIndex = 7;
			// 
			// deliverToCustomerDoorDateEdit
			// 
			this.deliverToCustomerDoorDateEdit.AllowDrop = true;
			this.deliverToCustomerDoorDateEdit.AutoCompleteMonthThreshold = 1;
			this.deliverToCustomerDoorDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.deliverToCustomerDoorDateEdit, "FCLContainers.JC_ArrivalCartageComplete");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ArrivalCartageComplete)));
			this.deliverToCustomerDoorDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("2c49fae6-b363-4c3f-a17b-948d61711ac1", "Delivery", "Delivery", "Deliver To Customer Door", "");
			this.deliverToCustomerDoorDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.deliverToCustomerDoorDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 110, true);
			this.deliverToCustomerDoorDateEdit.Name = "deliverToCustomerDoorDateEdit";
			this.deliverToCustomerDoorDateEdit.TabIndex = 8;
			// 
			// releaseNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.releaseNumberTextBox, "FCLContainers.JC_ContainerImportDORelease");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ContainerImportDORelease)));
			this.releaseNumberTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("6e809ee2-9f1b-4420-a910-ff11a90761a7", "", "Release Num.", "Release Number", "");
			this.releaseNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 58, true);
			this.releaseNumberTextBox.Name = "releaseNumberTextBox";
			this.releaseNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.releaseNumberTextBox.TabIndex = 4;
			// 
			// impSlotBookingRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.impSlotBookingRefTextBox, "FCLContainers.JC_ArrivalSlotReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ArrivalSlotReference)));
			this.impSlotBookingRefTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("e7d650b7-2264-40e5-9dee-2b9614a049ec", "Slot Bkg. Ref", "Slot Booking Ref", "Slot Booking Reference", "");
			this.impSlotBookingRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 32, true);
			this.impSlotBookingRefTextBox.Name = "impSlotBookingRefTextBox";
			this.impSlotBookingRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.impSlotBookingRefTextBox.TabIndex = 3;
			// 
			// impSlotDateEdit
			// 
			this.impSlotDateEdit.AllowDrop = true;
			this.impSlotDateEdit.AutoCompleteMonthThreshold = 1;
			this.impSlotDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.impSlotDateEdit, "FCLContainers.JC_ArrivalSlotDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ArrivalSlotDateTime)));
			this.impSlotDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("b925dfd6-0b4d-4062-bf20-a90c69ee9dbf", "Slot Date");
			this.impSlotDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.impSlotDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 32, true);
			this.impSlotDateEdit.Name = "impSlotDateEdit";
			this.impSlotDateEdit.TabIndex = 2;
			// 
			// VGMTabPage
			// 
			this.VGMTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("de44bfb1-2dcc-476c-bf52-0f4119a0c917", "VGM");
			this.VGMTabPage.Controls.Add(this.VerifiedByDocAddressControl);
			this.VGMTabPage.Controls.Add(this.VerifiedWeightGroupBox);
			this.VGMTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.VGMTabPage.Name = "VGMTabPage";
			this.VGMTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.VGMTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			this.VGMTabPage.TabIndex = 7;
			// 
			// VerifiedByDocAddressControl
			// 
			this.VerifiedByDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VerifiedByDocAddressControl, "FCLContainers.JobContainer.GrossWeightVerifiedByAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JobContainer.GrossWeightVerifiedByAddress)));
			this.VerifiedByDocAddressControl.BindToOrganisations = "FCLContainers.JobContainer.Lookups.GrossWeightVerifiedByList";
			this.VerifiedByDocAddressControl.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("2c84db3d-664f-4858-869c-11d09c1366ee", "VGM Verified By");
			this.VerifiedByDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 90, true);
			this.VerifiedByDocAddressControl.Name = "VerifiedByDocAddressControl";
			this.VerifiedByDocAddressControl.ReadOnly = false;
			this.VerifiedByDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.VerifiedByDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.VerifiedByDocAddressControl.TabIndex = 1;
			this.VerifiedByDocAddressControl.ValidationJustForced = false;
			// 
			// VerifiedWeightGroupBox
			// 
			this.VerifiedWeightGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("4d951bc9-6103-415c-90bd-2ec19fdcccd0", "Verified Weight");
			this.VerifiedWeightGroupBox.Controls.Add(this.VerifiedDateEdit);
			this.VerifiedWeightGroupBox.Controls.Add(this.VerifiedMethodDropEdit);
			this.VerifiedWeightGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.VerifiedWeightGroupBox.Name = "VerifiedWeightGroupBox";
			this.VerifiedWeightGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 80, true);
			this.VerifiedWeightGroupBox.TabIndex = 0;
			this.VerifiedWeightGroupBox.TabStop = false;
			// 
			// VerifiedDateEdit
			// 
			this.VerifiedDateEdit.AllowDrop = true;
			this.VerifiedDateEdit.AutoCompleteMonthThreshold = 1;
			this.VerifiedDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.VerifiedDateEdit, "FCLContainers.JobContainer.JC_GrossWeightVerificationDateTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JobContainer.JC_GrossWeightVerificationDateTime)));
			this.VerifiedDateEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ecc0632e-4588-41b7-bfb7-9a525a338139", "Verified Date");
			this.VerifiedDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.VerifiedDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 43, true);
			this.VerifiedDateEdit.Name = "VerifiedDateEdit";
			this.VerifiedDateEdit.TabIndex = 1;
			// 
			// VerifiedMethodDropEdit
			// 
			this.VerifiedMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.VerifiedMethodDropEdit, "FCLContainers.JobContainer.JC_GrossWeightVerificationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JobContainer.JC_GrossWeightVerificationType)));
			this.VerifiedMethodDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("7c29421c-6953-4646-9780-a859e50c6afe", "Verified Method");
			this.VerifiedMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 17, true);
			this.VerifiedMethodDropEdit.Name = "VerifiedMethodDropEdit";
			this.VerifiedMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.VerifiedMethodDropEdit.TabIndex = 0;
			// 
			// ExportTabPage
			// 
			ExportTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|88a21b8c-d04f-43be-b809-7d9fb9b98712", "Export Process");
			ExportTabPage.Controls.Add(this.DockReceiptTextBox);
			ExportTabPage.Controls.Add(this.slotBookingRefTextBox);
			ExportTabPage.Controls.Add(this.slotDateEdit);
			ExportTabPage.Controls.Add(this.emptyReleaseNumTextBox);
			ExportTabPage.Controls.Add(this.pickupFromCustDoorDateEdit);
			ExportTabPage.Controls.Add(exportFullPickupDateEdit);
			ExportTabPage.Controls.Add(fclOnBoardVesselPortTextBox);
			ExportTabPage.Controls.Add(fclWharfGateInPortTextBox);
			ExportTabPage.Controls.Add(containerYardEmptyPickupGateOutPortTextBox);
			ExportTabPage.Controls.Add(fclOnBoardVesselDateEdit);
			ExportTabPage.Controls.Add(exportPickupEmptyFromAddressControl);
			ExportTabPage.Controls.Add(exportEmptyReqByDateEdit);
			ExportTabPage.Controls.Add(containerYardEmptyPickupGateOutDateEdit);
			ExportTabPage.Controls.Add(wharfGateInDateEdit);
			ExportTabPage.Controls.Add(isArrivingAtCTOByRailCheckBox);
			ExportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			ExportTabPage.Name = "ExportTabPage";
			ExportTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			ExportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			ExportTabPage.TabIndex = 0;
			// 
			// exportFullPickupDateEdit
			// 
			exportFullPickupDateEdit.AllowDrop = true;
			exportFullPickupDateEdit.AutoCompleteMonthThreshold = 1;
			exportFullPickupDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(exportFullPickupDateEdit, "FCLContainers.JC_DepartureEstimatedPickup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureEstimatedPickup)));
			exportFullPickupDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			exportFullPickupDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 109, true);
			exportFullPickupDateEdit.Name = "exportFullPickupDateEdit";
			exportFullPickupDateEdit.TabIndex = 5;
			// 
			// fclOnBoardVesselPortTextBox
			// 
			fclOnBoardVesselPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(fclOnBoardVesselPortTextBox, "FCLContainers.JC_RL_NKFCLOnBoardVesselPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKFCLOnBoardVesselPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(fclOnBoardVesselPortTextBox, false);
			fclOnBoardVesselPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 213, true);
			fclOnBoardVesselPortTextBox.Name = "fclOnBoardVesselPortTextBox";
			fclOnBoardVesselPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			fclOnBoardVesselPortTextBox.TabIndex = 13;
			// 
			// fclWharfGateInPortTextBox
			// 
			fclWharfGateInPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(fclWharfGateInPortTextBox, "FCLContainers.JC_RL_NKFCLWharfGateInPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKFCLWharfGateInPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(fclWharfGateInPortTextBox, false);
			fclWharfGateInPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 161, true);
			fclWharfGateInPortTextBox.Name = "fclWharfGateInPortTextBox";
			fclWharfGateInPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			fclWharfGateInPortTextBox.TabIndex = 10;
			// 
			// containerYardEmptyPickupGateOutPortTextBox
			// 
			containerYardEmptyPickupGateOutPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(containerYardEmptyPickupGateOutPortTextBox, "FCLContainers.JC_RL_NKContainerYardEmptyPickupGateOutPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKContainerYardEmptyPickupGateOutPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(containerYardEmptyPickupGateOutPortTextBox, false);
			containerYardEmptyPickupGateOutPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 85, true);
			containerYardEmptyPickupGateOutPortTextBox.Name = "containerYardEmptyPickupGateOutPortTextBox";
			containerYardEmptyPickupGateOutPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			containerYardEmptyPickupGateOutPortTextBox.TabIndex = 4;
			// 
			// fclOnBoardVesselDateEdit
			// 
			fclOnBoardVesselDateEdit.AllowDrop = true;
			fclOnBoardVesselDateEdit.AutoCompleteMonthThreshold = 1;
			fclOnBoardVesselDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(fclOnBoardVesselDateEdit, "FCLContainers.JC_FCLOnBoardVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_FCLOnBoardVessel)));
			fclOnBoardVesselDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			fclOnBoardVesselDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 213, true);
			fclOnBoardVesselDateEdit.Name = "fclOnBoardVesselDateEdit";
			fclOnBoardVesselDateEdit.TabIndex = 12;
			// 
			// exportPickupEmptyFromAddressControl
			// 
			exportPickupEmptyFromAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(exportPickupEmptyFromAddressControl, "FCLContainers.JC_OA_DepartureContainerYardAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_OA_DepartureContainerYardAddress)));
			exportPickupEmptyFromAddressControl.BindToOrgList = "FCLContainers.Lookups+ContainerYard_List";
			this.LabelCaptionRenderProvider.SetLabelTop(exportPickupEmptyFromAddressControl, 4);
			exportPickupEmptyFromAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 32, true);
			exportPickupEmptyFromAddressControl.Name = "exportPickupEmptyFromAddressControl";
			exportPickupEmptyFromAddressControl.PopupCaption = null;
			exportPickupEmptyFromAddressControl.ReadOnly = false;
			exportPickupEmptyFromAddressControl.ShowAddress = false;
			exportPickupEmptyFromAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 17, true);
			exportPickupEmptyFromAddressControl.TabIndex = 1;
			// 
			// exportEmptyReqByDateEdit
			// 
			exportEmptyReqByDateEdit.AllowDrop = true;
			exportEmptyReqByDateEdit.AutoCompleteMonthThreshold = 1;
			exportEmptyReqByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(exportEmptyReqByDateEdit, "FCLContainers.JC_EmptyRequired");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_EmptyRequired)));
			exportEmptyReqByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			exportEmptyReqByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 6, true);
			exportEmptyReqByDateEdit.Name = "exportEmptyReqByDateEdit";
			exportEmptyReqByDateEdit.TabIndex = 0;
			// 
			// containerYardEmptyPickupGateOutDateEdit
			// 
			containerYardEmptyPickupGateOutDateEdit.AllowDrop = true;
			containerYardEmptyPickupGateOutDateEdit.AutoCompleteMonthThreshold = 1;
			containerYardEmptyPickupGateOutDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(containerYardEmptyPickupGateOutDateEdit, "FCLContainers.JC_ContainerYardEmptyPickupGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ContainerYardEmptyPickupGateOut)));
			containerYardEmptyPickupGateOutDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			containerYardEmptyPickupGateOutDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 85, true);
			containerYardEmptyPickupGateOutDateEdit.Name = "containerYardEmptyPickupGateOutDateEdit";
			containerYardEmptyPickupGateOutDateEdit.TabIndex = 3;
			// 
			// wharfGateInDateEdit
			// 
			wharfGateInDateEdit.AllowDrop = true;
			wharfGateInDateEdit.AutoCompleteMonthThreshold = 1;
			wharfGateInDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(wharfGateInDateEdit, "FCLContainers.JC_FCLWharfGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_FCLWharfGateIn)));
			wharfGateInDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			wharfGateInDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 161, true);
			wharfGateInDateEdit.Name = "wharfGateInDateEdit";
			wharfGateInDateEdit.TabIndex = 9;
			// 
			// isArrivingAtCTOByRailCheckBox
			// 
			isArrivingAtCTOByRailCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(isArrivingAtCTOByRailCheckBox, "FCLContainers.JC_DepartureDeliveryByRail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_DepartureDeliveryByRail)));
			isArrivingAtCTOByRailCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			isArrivingAtCTOByRailCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 61, true);
			isArrivingAtCTOByRailCheckBox.Name = "isArrivingAtCTOByRailCheckBox";
			isArrivingAtCTOByRailCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			isArrivingAtCTOByRailCheckBox.TabIndex = 14;
			// 
			// MessagesTabPage
			// 
			MessagesTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|482e5418-9a6f-403c-bc25-3a7dabb77b57", "Container Messages");
			MessagesTabPage.Controls.Add(messagesControl);
			MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			MessagesTabPage.Name = "MessagesTabPage";
			MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			MessagesTabPage.TabIndex = 1;
			// 
			// messagesControl
			// 
			messagesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(messagesControl, "FCLContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)))));
			messagesControl.BindPrepend = "";
			messagesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			messagesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			messagesControl.Name = "messagesControl";
			messagesControl.ShowChangingBlueMessageHeading = false;
			messagesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 301, true);
			messagesControl.TabIndex = 0;
			// 
			// detailsGroupBox
			// 
			detailsGroupBox.Controls.Add(this.isShipperOwnedCheckBox);
			detailsGroupBox.Controls.Add(commodityCodeFindBox);
			detailsGroupBox.Controls.Add(exportIsDamagedCheckBox);
			detailsGroupBox.Controls.Add(exportIsEmptyContainerCheckBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(detailsGroupBox, false);
			detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 241, true);
			detailsGroupBox.Name = "detailsGroupBox";
			detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 64, true);
			detailsGroupBox.TabIndex = 0;
			detailsGroupBox.TabStop = false;
			// 
			// commodityCodeFindBox
			// 
			commodityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(commodityCodeFindBox, "FCLContainers.JC_RH_NKContainerCommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RH_NKContainerCommodityCode)));
			commodityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 32, true);
			commodityCodeFindBox.Name = "commodityCodeFindBox";
			commodityCodeFindBox.ShouldResize = true;
			commodityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 17, true);
			commodityCodeFindBox.TabIndex = 3;
			// 
			// exportIsDamagedCheckBox
			// 
			exportIsDamagedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(exportIsDamagedCheckBox, "FCLContainers.JC_IsDamaged");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_IsDamaged)));
			exportIsDamagedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			exportIsDamagedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(340, 8, true);
			exportIsDamagedCheckBox.Name = "exportIsDamagedCheckBox";
			exportIsDamagedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			exportIsDamagedCheckBox.TabIndex = 2;
			// 
			// exportIsEmptyContainerCheckBox
			// 
			exportIsEmptyContainerCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(exportIsEmptyContainerCheckBox, "FCLContainers.JC_IsEmptyContainer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_IsEmptyContainer)));
			exportIsEmptyContainerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			exportIsEmptyContainerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 8, true);
			exportIsEmptyContainerCheckBox.Name = "exportIsEmptyContainerCheckBox";
			exportIsEmptyContainerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			exportIsEmptyContainerCheckBox.TabIndex = 1;
			// 
			// importContainerTypeGuidFindBox
			// 
			importContainerTypeGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(importContainerTypeGuidFindBox, "FCLContainers.JC_RC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RC)));
			importContainerTypeGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			importContainerTypeGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 25, true);
			importContainerTypeGuidFindBox.Name = "importContainerTypeGuidFindBox";
			importContainerTypeGuidFindBox.ShouldResize = true;
			importContainerTypeGuidFindBox.ShowDescriptionBox = false;
			importContainerTypeGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			importContainerTypeGuidFindBox.TabIndex = 3;
			// 
			// importContainerModeDropEdit
			// 
			importContainerModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(importContainerModeDropEdit, "FCLContainers.JC_ContainerMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ContainerMode)));
			importContainerModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 25, true);
			importContainerModeDropEdit.Name = "importContainerModeDropEdit";
			importContainerModeDropEdit.PreBoundMaxLength = 3;
			importContainerModeDropEdit.ShowDescriptionBox = false;
			importContainerModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 17, true);
			importContainerModeDropEdit.TabIndex = 2;
			// 
			// secondSealTextBox
			// 
			this.BindingSource.SetBindingMember(secondSealTextBox, "FCLContainers.JC_AdditionalSealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_AdditionalSealNum)));
			secondSealTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 69, true);
			secondSealTextBox.Name = "secondSealTextBox";
			secondSealTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			secondSealTextBox.TabIndex = 6;
			// 
			// exportContainerNumberTextBox
			// 
			this.BindingSource.SetBindingMember(exportContainerNumberTextBox, "FCLContainers.JC_ContainerNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ContainerNum)));
			exportContainerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 3, true);
			exportContainerNumberTextBox.Name = "exportContainerNumberTextBox";
			exportContainerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			exportContainerNumberTextBox.TabIndex = 0;
			// 
			// exportSealNumberTextBox
			// 
			this.BindingSource.SetBindingMember(exportSealNumberTextBox, "FCLContainers.JC_SealNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_SealNum)));
			exportSealNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 47, true);
			exportSealNumberTextBox.Name = "exportSealNumberTextBox";
			exportSealNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			exportSealNumberTextBox.TabIndex = 4;
			// 
			// detailsPanel
			// 
			detailsPanel.Controls.Add(this.additionalSealPartyDropEdit);
			detailsPanel.Controls.Add(this.additional2SealPartyDropEdit);
			detailsPanel.Controls.Add(this.sealPartyDropEdit);
			detailsPanel.Controls.Add(StowagePositionTextBox);
			detailsPanel.Controls.Add(exportSealNumberTextBox);
			detailsPanel.Controls.Add(detailsGroupBox);
			detailsPanel.Controls.Add(exportContainerNumberTextBox);
			detailsPanel.Controls.Add(importContainerTypeGuidFindBox);
			detailsPanel.Controls.Add(importContainerModeDropEdit);
			detailsPanel.Controls.Add(secondSealTextBox);
			detailsPanel.Controls.Add(this.thirdSealTextBox);
			detailsPanel.Controls.Add(this.WeightsGroupBox);
			detailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			detailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			detailsPanel.Name = "detailsPanel";
			detailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 330, true);
			detailsPanel.TabIndex = 18;
			// 
			// StowagePositionTextBox
			// 
			this.BindingSource.SetBindingMember(StowagePositionTextBox, "FCLContainers.JC_StowagePosition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_StowagePosition)));
			StowagePositionTextBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("a1077469-579d-4b1b-8ee8-71fbb3601feb", "Stowage Position", "Stowage Position format should be BBBRRTT where BBB is Bay, RR is Row and TT is Tier. All values should be numeric.");
			StowagePositionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(384, 3, true);
			StowagePositionTextBox.Name = "StowagePositionTextBox";
			StowagePositionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 17, true);
			StowagePositionTextBox.TabIndex = 1;
			// 
			// containersTabControl
			// 
			containersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			containersTabControl.Controls.Add(this.ReeferTabPage);
			containersTabControl.Controls.Add(this.MeasuresTabPage);
			containersTabControl.Controls.Add(ExportTabPage);
			containersTabControl.Controls.Add(ImportTabPage);
			containersTabControl.Controls.Add(MovementsTabPage);
			containersTabControl.Controls.Add(MessagesTabPage);
			containersTabControl.Controls.Add(this.VGMTabPage);
			containersTabControl.Controls.Add(this.NumbersTabPage);
			containersTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			containersTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(528, 0, true);
			containersTabControl.Name = "containersTabControl";
			containersTabControl.SelectedIndex = 0;
			containersTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 330, true);
			containersTabControl.TabIndex = 16;
			// 
			// ImportTabPage
			// 
			ImportTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("BillOfLadingContainersSubGridSection|87d1a726-eb0c-4bde-a4e1-8203305b2253", "Import Process");
			ImportTabPage.Controls.Add(this.estimatedFullDeliveryDateEdit);
			ImportTabPage.Controls.Add(this.deliverToCustomerDoorDateEdit);
			ImportTabPage.Controls.Add(this.releaseNumberTextBox);
			ImportTabPage.Controls.Add(this.impSlotBookingRefTextBox);
			ImportTabPage.Controls.Add(this.impSlotDateEdit);
			ImportTabPage.Controls.Add(containerYardEmptyReturnGateInPortTextBox);
			ImportTabPage.Controls.Add(fclWharfGateOutPortTextBox);
			ImportTabPage.Controls.Add(fclUnloadFromVesselPortTextBox);
			ImportTabPage.Controls.Add(importReturnEmptyToAddressControl);
			ImportTabPage.Controls.Add(fclWharfGateOutDateEdit);
			ImportTabPage.Controls.Add(importEmptyWasReturnedOnDateEdit);
			ImportTabPage.Controls.Add(importEmptyReturnByDateEdit);
			ImportTabPage.Controls.Add(fclUnloadFromVesselDateEdit);
			ImportTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			ImportTabPage.Name = "ImportTabPage";
			ImportTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			ImportTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			ImportTabPage.TabIndex = 3;
			// 
			// containerYardEmptyReturnGateInPortTextBox
			// 
			containerYardEmptyReturnGateInPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(containerYardEmptyReturnGateInPortTextBox, "FCLContainers.JC_RL_NKContainerYardEmptyReturnGateInPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKContainerYardEmptyReturnGateInPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(containerYardEmptyReturnGateInPortTextBox, false);
			containerYardEmptyReturnGateInPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 226, true);
			containerYardEmptyReturnGateInPortTextBox.Name = "containerYardEmptyReturnGateInPortTextBox";
			containerYardEmptyReturnGateInPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			containerYardEmptyReturnGateInPortTextBox.TabIndex = 12;
			// 
			// fclWharfGateOutPortTextBox
			// 
			fclWharfGateOutPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(fclWharfGateOutPortTextBox, "FCLContainers.JC_RL_NKFCLWharfGateOutPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKFCLWharfGateOutPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(fclWharfGateOutPortTextBox, false);
			fclWharfGateOutPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 84, true);
			fclWharfGateOutPortTextBox.Name = "fclWharfGateOutPortTextBox";
			fclWharfGateOutPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			fclWharfGateOutPortTextBox.TabIndex = 6;
			// 
			// fclUnloadFromVesselPortTextBox
			// 
			fclUnloadFromVesselPortTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(fclUnloadFromVesselPortTextBox, "FCLContainers.JC_RL_NKFCLUnloadFromVesselPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_RL_NKFCLUnloadFromVesselPort)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(fclUnloadFromVesselPortTextBox, false);
			fclUnloadFromVesselPortTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 6, true);
			fclUnloadFromVesselPortTextBox.Name = "fclUnloadFromVesselPortTextBox";
			fclUnloadFromVesselPortTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			fclUnloadFromVesselPortTextBox.TabIndex = 1;
			// 
			// importReturnEmptyToAddressControl
			// 
			importReturnEmptyToAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(importReturnEmptyToAddressControl, "FCLContainers.JC_OA_ArrivalContainerYardAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_OA_ArrivalContainerYardAddress)));
			importReturnEmptyToAddressControl.BindToOrgList = "FCLContainers.Lookups+ContainerYard_List";
			this.LabelCaptionRenderProvider.SetLabelTop(importReturnEmptyToAddressControl, 4);
			importReturnEmptyToAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 136, true);
			importReturnEmptyToAddressControl.Name = "importReturnEmptyToAddressControl";
			importReturnEmptyToAddressControl.PopupCaption = null;
			importReturnEmptyToAddressControl.ReadOnly = false;
			importReturnEmptyToAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 55, true);
			importReturnEmptyToAddressControl.TabIndex = 9;
			// 
			// fclWharfGateOutDateEdit
			// 
			fclWharfGateOutDateEdit.AllowDrop = true;
			fclWharfGateOutDateEdit.AutoCompleteMonthThreshold = 1;
			fclWharfGateOutDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(fclWharfGateOutDateEdit, "FCLContainers.JC_FCLWharfGateOut");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_FCLWharfGateOut)));
			fclWharfGateOutDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			fclWharfGateOutDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 84, true);
			fclWharfGateOutDateEdit.Name = "fclWharfGateOutDateEdit";
			fclWharfGateOutDateEdit.TabIndex = 5;
			// 
			// importEmptyWasReturnedOnDateEdit
			// 
			importEmptyWasReturnedOnDateEdit.AllowDrop = true;
			importEmptyWasReturnedOnDateEdit.AutoCompleteMonthThreshold = 1;
			importEmptyWasReturnedOnDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(importEmptyWasReturnedOnDateEdit, "FCLContainers.JC_ContainerYardEmptyReturnGateIn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_ContainerYardEmptyReturnGateIn)));
			importEmptyWasReturnedOnDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			importEmptyWasReturnedOnDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 226, true);
			importEmptyWasReturnedOnDateEdit.Name = "importEmptyWasReturnedOnDateEdit";
			importEmptyWasReturnedOnDateEdit.TabIndex = 11;
			// 
			// importEmptyReturnByDateEdit
			// 
			importEmptyReturnByDateEdit.AllowDrop = true;
			importEmptyReturnByDateEdit.AutoCompleteMonthThreshold = 1;
			importEmptyReturnByDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(importEmptyReturnByDateEdit, "FCLContainers.JC_EmptyReturnedBy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_EmptyReturnedBy)));
			importEmptyReturnByDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			importEmptyReturnByDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 200, true);
			importEmptyReturnByDateEdit.Name = "importEmptyReturnByDateEdit";
			importEmptyReturnByDateEdit.TabIndex = 10;
			// 
			// fclUnloadFromVesselDateEdit
			// 
			fclUnloadFromVesselDateEdit.AllowDrop = true;
			fclUnloadFromVesselDateEdit.AutoCompleteMonthThreshold = 1;
			fclUnloadFromVesselDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(fclUnloadFromVesselDateEdit, "FCLContainers.JC_FCLUnloadFromVessel");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).JC_FCLUnloadFromVessel)));
			fclUnloadFromVesselDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			fclUnloadFromVesselDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 6, true);
			fclUnloadFromVesselDateEdit.Name = "fclUnloadFromVesselDateEdit";
			fclUnloadFromVesselDateEdit.TabIndex = 0;
			// 
			// MovementsTabPage
			// 
			MovementsTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("665e2c1a-9b1b-49e4-8d82-18004168414a", "Movements");
			MovementsTabPage.Controls.Add(movementsControl);
			MovementsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			MovementsTabPage.Name = "MovementsTabPage";
			MovementsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			MovementsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			MovementsTabPage.TabIndex = 2;
			// 
			// movementsControl
			// 
			movementsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(movementsControl, ".");
			movementsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			movementsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			movementsControl.Name = "movementsControl";
			movementsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 301, true);
			movementsControl.TabIndex = 0;
			// 
			// NumbersTabPage
			// 
			this.NumbersTabPage.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("1fce536d-fe45-4bd4-b8e9-e2e594832184", "", "Numbers", "", "");
			this.NumbersTabPage.Controls.Add(this.numbersControl1);
			this.NumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NumbersTabPage.Name = "NumbersTabPage";
			this.NumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.NumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(475, 308, true);
			this.NumbersTabPage.TabIndex = 8;
			this.NumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// numbersControl1
			// 
			this.numbersControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.numbersControl1, "FCLContainers.AdditionalReferenceNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection)(((Enterprise.Freight.Agency.Business.AgencyShipmentContainer)(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.BillOfLading)(null)).FCLContainers)).SyncRoot)).AdditionalReferenceNumbers)));
			this.numbersControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.numbersControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.numbersControl1.Name = "numbersControl1";
			this.numbersControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 303, true);
			this.numbersControl1.TabIndex = 0;
			// 
			// BillOfLadingContainersSubGridSection
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(containersTabControl);
			this.Controls.Add(detailsPanel);
			this.Name = "BillOfLadingContainersSubGridSection";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.slotDateEdit.ResumeLayout(true);
			this.slotDateEdit.PerformLayout();
			this.pickupFromCustDoorDateEdit.ResumeLayout(true);
			this.pickupFromCustDoorDateEdit.PerformLayout();
			this.additionalSealPartyDropEdit.ResumeLayout(true);
			this.additionalSealPartyDropEdit.PerformLayout();
			this.additional2SealPartyDropEdit.ResumeLayout(true);
			this.additional2SealPartyDropEdit.PerformLayout();
			this.sealPartyDropEdit.ResumeLayout(true);
			this.sealPartyDropEdit.PerformLayout();
			this.WeightsGroupBox.ResumeLayout(false);
			this.WeightsGroupBox.PerformLayout();
			this.GrossWeightUQDropEdit.ResumeLayout(true);
			this.GrossWeightUQDropEdit.PerformLayout();
			this.ReeferTabPage.ResumeLayout(false);
			this.ReeferTabPage.PerformLayout();
			this.reeferUserControl.ResumeLayout(true);
			this.reeferUserControl.PerformLayout();
			this.MeasuresTabPage.ResumeLayout(false);
			this.MeasuresTabPage.PerformLayout();
			this.estimatedFullDeliveryDateEdit.ResumeLayout(true);
			this.estimatedFullDeliveryDateEdit.PerformLayout();
			this.deliverToCustomerDoorDateEdit.ResumeLayout(true);
			this.deliverToCustomerDoorDateEdit.PerformLayout();
			this.impSlotDateEdit.ResumeLayout(true);
			this.impSlotDateEdit.PerformLayout();
			this.VGMTabPage.ResumeLayout(false);
			this.VGMTabPage.PerformLayout();
			this.VerifiedByDocAddressControl.ResumeLayout(true);
			this.VerifiedByDocAddressControl.PerformLayout();
			this.VerifiedWeightGroupBox.ResumeLayout(false);
			this.VerifiedWeightGroupBox.PerformLayout();
			this.VerifiedDateEdit.ResumeLayout(true);
			this.VerifiedDateEdit.PerformLayout();
			this.VerifiedMethodDropEdit.ResumeLayout(true);
			this.VerifiedMethodDropEdit.PerformLayout();
			ExportTabPage.ResumeLayout(false);
			ExportTabPage.PerformLayout();
			exportFullPickupDateEdit.ResumeLayout(true);
			exportFullPickupDateEdit.PerformLayout();
			fclOnBoardVesselDateEdit.ResumeLayout(true);
			fclOnBoardVesselDateEdit.PerformLayout();
			exportPickupEmptyFromAddressControl.ResumeLayout(true);
			exportPickupEmptyFromAddressControl.PerformLayout();
			exportEmptyReqByDateEdit.ResumeLayout(true);
			exportEmptyReqByDateEdit.PerformLayout();
			containerYardEmptyPickupGateOutDateEdit.ResumeLayout(true);
			containerYardEmptyPickupGateOutDateEdit.PerformLayout();
			wharfGateInDateEdit.ResumeLayout(true);
			wharfGateInDateEdit.PerformLayout();
			MessagesTabPage.ResumeLayout(false);
			MessagesTabPage.PerformLayout();
			messagesControl.ResumeLayout(true);
			messagesControl.PerformLayout();
			detailsGroupBox.ResumeLayout(false);
			detailsGroupBox.PerformLayout();
			commodityCodeFindBox.ResumeLayout(true);
			commodityCodeFindBox.PerformLayout();
			importContainerTypeGuidFindBox.ResumeLayout(true);
			importContainerTypeGuidFindBox.PerformLayout();
			importContainerModeDropEdit.ResumeLayout(true);
			importContainerModeDropEdit.PerformLayout();
			detailsPanel.ResumeLayout(false);
			detailsPanel.PerformLayout();
			containersTabControl.ResumeLayout(false);
			containersTabControl.PerformLayout();
			ImportTabPage.ResumeLayout(false);
			ImportTabPage.PerformLayout();
			importReturnEmptyToAddressControl.ResumeLayout(true);
			importReturnEmptyToAddressControl.PerformLayout();
			fclWharfGateOutDateEdit.ResumeLayout(true);
			fclWharfGateOutDateEdit.PerformLayout();
			importEmptyWasReturnedOnDateEdit.ResumeLayout(true);
			importEmptyWasReturnedOnDateEdit.PerformLayout();
			importEmptyReturnByDateEdit.ResumeLayout(true);
			importEmptyReturnByDateEdit.PerformLayout();
			fclUnloadFromVesselDateEdit.ResumeLayout(true);
			fclUnloadFromVesselDateEdit.PerformLayout();
			MovementsTabPage.ResumeLayout(false);
			MovementsTabPage.PerformLayout();
			movementsControl.ResumeLayout(true);
			movementsControl.PerformLayout();
			this.NumbersTabPage.ResumeLayout(false);
			this.NumbersTabPage.PerformLayout();
			this.numbersControl1.ResumeLayout(true);
			this.numbersControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		System.ComponentModel.IContainer components;
		ZArchitecture.GUI.ZTabPage MeasuresTabPage;
		ZArchitecture.ZCalcEdit OverhangRightCalcEdit;
		ZArchitecture.ZCalcEdit OverhangLeftCalcEdit;
		ZArchitecture.ZCalcEdit OverhangBackCalcEdit;
		ZArchitecture.ZCalcEdit OverhangFrontCalcEdit;
		ZArchitecture.ZLabel ShipmentTotalLabel;
		ZArchitecture.ZCalcEdit AddDataLengthCalcEdit;
		ZArchitecture.ZCalcEdit ShipmentTotalVolumeCalcEdit;
		ZArchitecture.ZCalcEdit StandardDataHeightCalcEdit;
		ZArchitecture.ZLabel MeasuresCalculatedLabel;
		ZArchitecture.ZCalcEdit StandardDataLengthCalcEdit;
		ZArchitecture.ZLabel MeasuresLowerOnfileLabel;
		ZArchitecture.ZCalcEdit StandardWidthCalcEdit;
		ZArchitecture.ZCalcEdit CalculatedVolumeOnFileCalcEdit;
		ZArchitecture.ZCalcEdit AddDataHeightCalcEdit;
		ZArchitecture.ZCalcEdit MaxVolumeOnFileCalcEdit;
		ZArchitecture.ZCalcEdit AddDataWidthCalcEdit;
		ZArchitecture.ZLabel MeasuresOverhangLabel;
		ZArchitecture.ZCalcEdit OverhangHeightCalcEdit;
		ZArchitecture.ZLabel MeasuresActualLabel;
		ZArchitecture.ZCalcEdit OverhangLengthCalcEdit;
		ZArchitecture.ZLabel MeasuresOnfileLabel;
		ZArchitecture.ZCalcEdit OverhangWidthCalcEdit;
		ZArchitecture.GUI.ZDateEdit pickupFromCustDoorDateEdit;
		ZArchitecture.ZTextBox emptyReleaseNumTextBox;
		ZArchitecture.GUI.ZDateEdit slotDateEdit;
		ZArchitecture.ZTextBox slotBookingRefTextBox;
		ZArchitecture.ZTextBox impSlotBookingRefTextBox;
		ZArchitecture.GUI.ZDateEdit impSlotDateEdit;
		ZArchitecture.ZTextBox releaseNumberTextBox;
		ZArchitecture.GUI.ZDateEdit deliverToCustomerDoorDateEdit;
		ZArchitecture.GUI.ZDateEdit estimatedFullDeliveryDateEdit;
		ZArchitecture.GUI.ZTabPage ReeferTabPage;
		Freight.GUI.ReeferUserControl reeferUserControl;
		ZArchitecture.ZTextBox DockReceiptTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox WeightsGroupBox;
		Enterprise.ZArchitecture.ZCalcEdit TareOnFileCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit GrossWeightUQDropEdit;
		Enterprise.ZArchitecture.ZCalcEdit ActualTareWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit ActualNetWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit ActualDunnageCalcEdit;
		Enterprise.ZArchitecture.ZLabel ActualWeightLabel;
		Enterprise.ZArchitecture.ZLabel OnFileWeightLabel;
		Enterprise.ZArchitecture.ZCalcEdit TotalGrossWeightCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit MaxGrossWeightOnFileCalcEdit;
		Enterprise.ZArchitecture.ZTextBox thirdSealTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit sealPartyDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit additionalSealPartyDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit additional2SealPartyDropEdit;
		ZArchitecture.GUI.ZTabPage VGMTabPage;
		ZArchitecture.GUI.ZCheckBox isShipperOwnedCheckBox;
		ZArchitecture.GUI.ZGroupBox VerifiedWeightGroupBox;
		ZArchitecture.GUI.ZDropEdit VerifiedMethodDropEdit;
		ZArchitecture.GUI.ZDateEdit VerifiedDateEdit;
		MasterFiles.GUI.ZDocAddressControl VerifiedByDocAddressControl;
		private ZArchitecture.GUI.ZTabPage NumbersTabPage;
		private MasterFiles.GUI.NumbersControl numbersControl1;
	}
}
