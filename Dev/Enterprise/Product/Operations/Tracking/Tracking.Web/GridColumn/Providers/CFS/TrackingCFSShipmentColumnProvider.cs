using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
namespace Enterprise.Tracking.Web
{
	public class TrackingCFSShipmentColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();

			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("0cb57c8d-00bd-4e6b-b0e2-f068b4f86504", "Shipment#"), TrackingCFSShipment.Schema.JS_UniqueConsignRef) { ColumnKey = WebTracker.Grids.CFSShipments.ShipmentNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.CFSShipmentDetailsPage) + (NoResString)"?Ref={0}"; // Partial URL
			shipmentNumberColumn.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionaryAsRequired(shipmentNumberColumn);

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("25d8710f-8ac6-4074-adb5-638f816e8d40", "Bill"), TrackingCFSShipment.Schema.JS_HouseBill) { ColumnKey = WebTracker.Grids.CFSShipments.HouseBill });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("02cc2452-8cab-482d-98cc-68c777dd6d59", "Shipper"), TrackingCFSShipment.Schema.ConsignorName) { ColumnKey = WebTracker.Grids.CFSShipments.Shipper });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneeName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("8228c494-3cdd-4f49-b9f8-352ab1cbedb6", "Consignee"), TrackingCFSShipment.Schema.ConsigneeName) { ColumnKey = WebTracker.Grids.CFSShipments.Consignee });

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			ZCodeFindBoxColumn originColumn = new ZCodeFindBoxColumn(Res.GetString("61515b77-1b84-4935-ab1a-795ba292410f", "Origin"), TrackingCFSShipment.Schema.JS_RL_NKOrigin, (NoResString)"Ports") { ColumnKey = WebTracker.Grids.CFSShipments.Origin };// Data column name
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(originColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).ETDWithSuppression);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("51a455c4-7a33-4d5b-a8f0-e00d9dee6bd6", "ETD"), TrackingCFSShipment.Schema.ETDWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ETD });

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			ZCodeFindBoxColumn destinationColumn = new ZCodeFindBoxColumn(Res.GetString("a45b574a-d3ed-4e84-8254-59e398f6c7d5", "Destination"), TrackingCFSShipment.Schema.JS_RL_NKDestination, (NoResString)"Ports") { ColumnKey = WebTracker.Grids.CFSShipments.Destination };// Data column name
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(destinationColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).ETAWithSuppression);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("b17f96ab-49dd-41ec-9b48-38b085f2bb25", "ETA"), TrackingCFSShipment.Schema.ETAWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ETA });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).CurrentLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("c45f81fd-67fd-4025-9824-5f401cc8d92b", "Current Load Port"), TrackingCFSShipment.Schema.CurrentLoadPort, (NoResString)"Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.CurrentLoadPort });// Data column name

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).CurrentDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("b145c503-00e1-416d-b171-3eaf98354381", "Current Discharge Port"), TrackingCFSShipment.Schema.CurrentDischargePort, (NoResString)"Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.CurrentDischargePort });// Data column name

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).CurrentVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("578ff7b3-2b19-4320-a379-353835c592e7", "Current Vessel"), TrackingCFSShipment.Schema.CurrentVessel) { ColumnKey = WebTracker.Grids.CFSShipments.CurrentVessel });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).CurrentVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cb4892d8-c016-4016-bec0-b0cf4eed8523", "Current Voy./Flight"), TrackingCFSShipment.Schema.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.CFSShipments.CurrentVoyage });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_BookingReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("243259C8-F340-464F-BE59-CF9F98E1A1B0", "Shipper's Ref#"), TrackingCFSShipment.Schema.JS_BookingReference) { ColumnKey = WebTracker.Grids.CFSShipments.BookingReference });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).TransportMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0062dd99-b437-4458-9bf3-8a4b2f7f983e", "Mode"), TrackingCFSShipment.Schema.TransportMode) { ColumnKey = WebTracker.Grids.CFSShipments.Mode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).PacksWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b62d0fbf-93ca-476d-961b-11d583a284c7", "Packs"), TrackingCFSShipment.Schema.PacksWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Packs });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).WeightWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cf4c6248-c435-4b9f-82ec-c35d0364c080", "Weight"), TrackingCFSShipment.Schema.WeightWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Weight });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).VolumeWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("871ef2d4-0e48-49f7-ac26-53dd0da632f7", "Volume"), TrackingCFSShipment.Schema.VolumeWithUnits) { ColumnKey = WebTracker.Grids.CFSShipments.Volume });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_GoodsDescription);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e075ba9d-fe23-4ac6-8db0-868dffee54b9", "Goods Description"), TrackingCFSShipment.Schema.JS_GoodsDescription) { ColumnKey = WebTracker.Grids.CFSShipments.GoodsDescription });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).EstimatedPickupDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("d9e6809d-ff01-4b8b-957a-8fc6c1bafa19", "Estimated Pickup"), TrackingCFSShipment.Schema.EstimatedPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.EstimatedPickup });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).PickupDateRequiredBy);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("2cf3520a-c8e8-4807-bd0a-7a8132e0e6a0", "Pickup Required By"), TrackingCFSShipment.Schema.PickupDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.PickupRequiredBy });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).EstimatedDeliveryDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("806df2ba-b294-4398-98b0-7cb4818556e2", "Estimated Delivery"), TrackingCFSShipment.Schema.EstimatedDeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.EstimatedDelivery });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).DeliveryDateRequiredBy);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("1ead9b17-03f0-4613-b673-6abfd7e8656f", "Delivery Required By"), TrackingCFSShipment.Schema.DeliveryDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryRequiredBy });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).DeliveryDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("3658c8c8-c709-4456-8c19-0ac50f5de8a1", "Delivery Date"), TrackingCFSShipment.Schema.DeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryDate });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_RS_NKServiceLevel);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).ServiceLevels);
			ZCodeFindBoxColumn serviceLevelColumn = new ZCodeFindBoxColumn(Res.GetString("65b3612b-abd2-4dee-9cdf-2cc5f30a22bc", "Service Level"), TrackingCFSShipment.Schema.JS_RS_NKServiceLevel, "ServiceLevels") { ColumnKey = WebTracker.Grids.CFSShipments.ServiceLevel };
			serviceLevelColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(serviceLevelColumn);

			if (WebEnv.AppInstance != null)
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				if (siteUser != null && siteUser.CanViewAccounts)
				{
					ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).Charges);
					AddToDictionary(new ZTextEditColumn(Res.GetString("f46dd478-9165-4fa1-a0b0-88e2620ac91c", "Charges"), TrackingCFSShipment.Schema.Charges) { ColumnKey = WebTracker.Grids.CFSShipments.Charges });
				}
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorFullAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("44c86346-85a6-41a5-a133-77ce70badec2", "Shipper Full Address"), TrackingCFSShipment.Schema.ConsignorFullAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperFullAddress });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a16d759b-f75e-4f1c-b251-14049bc9ac42", "Shipper Address"), TrackingCFSShipment.Schema.ConsignorAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperAddress });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorCity);
			AddToDictionary(new ZTextEditColumn(Res.GetString("f3f59dee-2fdf-422d-8b45-0442c655dcc0", "Shipper City"), TrackingCFSShipment.Schema.ConsignorCity) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperCity });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b9399908-bedb-4d73-9cd2-4018eb079022", "Shipper State"), TrackingCFSShipment.Schema.ConsignorState) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperState });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsignorPostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("76cd388d-c514-40f4-aaf2-9a938b250344", "Shipper Post Code"), TrackingCFSShipment.Schema.ConsignorPostCode) { ColumnKey = WebTracker.Grids.CFSShipments.ShipperPostCode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneeFullAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("407a9731-0c3f-4eb4-85f8-19dc9e81d00e", "Consignee Full Address"), TrackingCFSShipment.Schema.ConsigneeFullAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeFullAddress });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneeAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cfe7cd0a-8600-48d5-975a-bdc0640ed9b7", "Consignee Address"), TrackingCFSShipment.Schema.ConsigneeAddress) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeAddress });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneeCity);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b0258fcc-1265-472a-8654-c5beb99eb66e", "Consignee City"), TrackingCFSShipment.Schema.ConsigneeCity) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeCity });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneeState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cef5df52-026a-4834-bb71-6e71f2dc24cc", "Consignee State"), TrackingCFSShipment.Schema.ConsigneeState) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneeState });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ConsigneePostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cac35afb-be53-4e7f-a3e1-d978d29af3b3", "Consignee Post Code"), TrackingCFSShipment.Schema.ConsigneePostCode) { ColumnKey = WebTracker.Grids.CFSShipments.ConsigneePostCode });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).ReceivedDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("58de6d74-d81e-428a-95b2-4f0bd2df8899", "Received Date"), TrackingCFSShipment.Schema.ReceivedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ReceivedDate });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ReceivedBy);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a5dc0b5c-3add-412a-9673-72f227381c93", "Received By"), TrackingCFSShipment.Schema.ReceivedBy) { ColumnKey = WebTracker.Grids.CFSShipments.ReceivedBy });
			ZBindToChecker.CheckBindTo((ZInt)((TrackingCFSShipment)null).PiecesReceived);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("13fb25e2-2819-4dbc-8ecd-50d7e1146f63", "Pieces Received"), TrackingCFSShipment.Schema.PiecesReceived) { ColumnKey = WebTracker.Grids.CFSShipments.PiecesReceived });

			ZBindToChecker.CheckBindTo((ZBool)((TrackingCFSShipment)null).BookedOnline);
			AddToDictionary(new ZCheckBoxColumn(Res.GetString("3861e772-3ae6-4d1a-a70c-e8d2910627ee", "Booked Online"), TrackingCFSShipment.Schema.BookedOnline) { ColumnKey = WebTracker.Grids.CFSShipments.BookedOnline });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).ActualPickupDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("08faf301-7132-46a9-9d70-54ad2abf9f7e", "Actual Pickup"), TrackingCFSShipment.Schema.ActualPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.ActualPickup });

			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingCFSShipment)null).Milestones);
				foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
				{
					if (column.HeaderText != Res.GetString("96d56a46-5c9a-439c-b214-9f8cd7e30531", "Last Milestone Desc."))
					{
						AddToDictionary(column);
					}
					else
					{
						AddToDictionaryAsDefault(column);
					}
				}
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).MainLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("ae15ec3b-f4e3-4167-a6c2-b114177445fc", "Main Load Port"), TrackingCFSShipment.Schema.MainLoadPort, (NoResString)"Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.MainLoadPort }); // Data column name

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).MainDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("b5631fd1-7048-4a39-a9cd-8ce25e8483bc", "Main Discharge Port"), TrackingCFSShipment.Schema.MainDischargePort, (NoResString)"Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.CFSShipments.MainDischargePort }); // Data column name

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).MainVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e08d2513-4fd2-492c-928b-0eb825c3f181", "Main Vessel"), TrackingCFSShipment.Schema.MainVessel) { ColumnKey = WebTracker.Grids.CFSShipments.MainVessel });
			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).MainVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b5805614-da10-42ac-91b6-814bf13d00f1", "Main Voy./Flight"), TrackingCFSShipment.Schema.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.CFSShipments.MainVoyage });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).ShipmentType);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b6ac5cdd-4d17-450c-93b7-1a07f4de1dee", "Type"), TrackingCFSShipment.Schema.ShipmentType) { ColumnKey = WebTracker.Grids.CFSShipments.Type });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_AdditionalTerms);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cae70c61-afe0-4725-b9f6-db0d5166e0d1", "Additional Terms"), TrackingCFSShipment.Schema.JS_AdditionalTerms) { ColumnKey = WebTracker.Grids.CFSShipments.AdditionalTerms });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_INCO);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).PaymentTerm_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("376a6b91-e51a-41a4-9f87-0dd113aff8ea", "Payment Term"), TrackingCFSShipment.Schema.JS_INCO)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.INCO,
				BindToList = "PaymentTerm_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_HBLAWBChargesDisplay);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).ChargesApply_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("4429a8d0-c621-4195-9f87-512c549d311d", "Charges Apply"), TrackingCFSShipment.Schema.JS_HBLAWBChargesDisplay)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.ChargesApply,
				BindToList = "ChargesApply_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_ReleaseType);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).ReleaseType_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("575d65f6-f305-4709-bbce-5c12019c1213", "Release Type"), TrackingCFSShipment.Schema.JS_ReleaseType)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.ReleaseType,
				BindToList = "ReleaseType_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_ShippedOnBoard);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingCFSShipment)null).OnBoard_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("c5503c1a-744d-4140-971f-6896e6b2cf9b", "On Board"), TrackingCFSShipment.Schema.JS_ShippedOnBoard)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.CFSShipments.OnBoard,
				BindToList = "OnBoard_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).PickupAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("20991196-297c-4150-b971-e1fa898a532b", "Pickup Agent"), TrackingCFSShipment.Schema.PickupAgentFullName) { ColumnKey = WebTracker.Grids.CFSShipments.PickupAgent });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).DeliveryAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4888e080-1652-4177-ac87-743573bcce5d", "Delivery Agent"), TrackingCFSShipment.Schema.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.CFSShipments.DeliveryAgent });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_ConsolReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("43205A6A-CDC4-4D50-B25D-60D65D7B81F8", "Client Ref"), TrackingCFSShipment.Schema.JS_ConsolReference) { ColumnKey = WebTracker.Grids.CFSShipments.ClientRef });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_InterimReceipt);
			AddToDictionary(new ZTextEditColumn(Res.GetString("7B58DF27-FCA9-49AF-8F75-C185681EB1CB", "Interim Receipt"), TrackingCFSShipment.Schema.JS_InterimReceipt) { ColumnKey = WebTracker.Grids.CFSShipments.InterimReceipt });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingCFSShipment)null).JS_A_RCV);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("215F8F18-326E-4E86-9259-61E4BC61AD47", "Whs. Receipt"), TrackingCFSShipment.Schema.JS_A_RCV, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.CFSShipments.WhsReceipt });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).CustomsEntryNumber);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3F06EB19-7A8D-43AD-88DF-931CD0CFE4FE", "Entry No"), TrackingCFSShipment.Schema.CustomsEntryNumber) { ColumnKey = WebTracker.Grids.CFSShipments.EntryNo });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).JS_WarehouseLocation);
			AddToDictionary(new ZTextEditColumn(Res.GetString("BF8493DF-2356-4C33-A36C-40656FCB22FF", "Warehouse Location"), TrackingCFSShipment.Schema.JS_WarehouseLocation) { ColumnKey = WebTracker.Grids.CFSShipments.WhsLocation });

			ZBindToChecker.CheckBindTo((ZString)((TrackingCFSShipment)null).MasterBill);
			AddToDictionary(new ZTextEditColumn(Res.GetString("1F50E096-0C11-49C8-87EA-BFB38E735B44", "Ocean Bill"), TrackingCFSShipment.Schema.MasterBill) { ColumnKey = WebTracker.Grids.CFSShipments.MasterBill });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.CFSShipments.ShipmentNumber);
			result.Add((int)WebTracker.Grids.CFSShipments.HouseBill);
			result.Add((int)WebTracker.Grids.CFSShipments.Shipper);
			result.Add((int)WebTracker.Grids.CFSShipments.Consignee);
			result.Add((int)WebTracker.Grids.CFSShipments.Origin);
			result.Add((int)WebTracker.Grids.CFSShipments.ETD);
			result.Add((int)WebTracker.Grids.CFSShipments.Destination);
			result.Add((int)WebTracker.Grids.CFSShipments.ETA);
			result.Add((int)WebTracker.Grids.CFSShipments.CurrentLoadPort);
			result.Add((int)WebTracker.Grids.CFSShipments.CurrentDischargePort);
			result.Add((int)WebTracker.Grids.CFSShipments.CurrentVessel);
			result.Add((int)WebTracker.Grids.CFSShipments.CurrentVoyage);
			result.Add((int)WebTracker.Grids.CFSShipments.BookingReference);
			result.Add((int)WebTracker.Grids.CFSShipments.Mode);
			result.Add((int)WebTracker.Grids.CFSShipments.Packs);
			result.Add((int)WebTracker.Grids.CFSShipments.Weight);
			result.Add((int)WebTracker.Grids.CFSShipments.Volume);
			result.Add((int)WebTracker.Grids.CFSShipments.GoodsDescription);
			result.Add((int)WebTracker.Grids.CFSShipments.EstimatedPickup);
			result.Add((int)WebTracker.Grids.CFSShipments.PickupRequiredBy);
			result.Add((int)WebTracker.Grids.CFSShipments.EstimatedDelivery);
			result.Add((int)WebTracker.Grids.CFSShipments.DeliveryRequiredBy);
			result.Add((int)WebTracker.Grids.CFSShipments.DeliveryDate);
			result.Add((int)WebTracker.Grids.CFSShipments.ServiceLevel);
			result.Add((int)WebTracker.Grids.CFSShipments.Charges);
			result.Add((int)WebTracker.Grids.CFSShipments.ShipperFullAddress);
			result.Add((int)WebTracker.Grids.CFSShipments.ShipperAddress);
			result.Add((int)WebTracker.Grids.CFSShipments.ShipperCity);
			result.Add((int)WebTracker.Grids.CFSShipments.ShipperState);
			result.Add((int)WebTracker.Grids.CFSShipments.ShipperPostCode);
			result.Add((int)WebTracker.Grids.CFSShipments.ConsigneeFullAddress);
			result.Add((int)WebTracker.Grids.CFSShipments.ConsigneeAddress);
			result.Add((int)WebTracker.Grids.CFSShipments.ConsigneeCity);
			result.Add((int)WebTracker.Grids.CFSShipments.ConsigneeState);
			result.Add((int)WebTracker.Grids.CFSShipments.ConsigneePostCode);
			result.Add((int)WebTracker.Grids.CFSShipments.ReceivedDate);
			result.Add((int)WebTracker.Grids.CFSShipments.ReceivedBy);
			result.Add((int)WebTracker.Grids.CFSShipments.PiecesReceived);
			result.Add((int)WebTracker.Grids.CFSShipments.BookedOnline);
			result.Add((int)WebTracker.Grids.CFSShipments.ActualPickup);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				result.Add(column.UniqueKey);
			}
			result.Add((int)WebTracker.Grids.CFSShipments.MainLoadPort);
			result.Add((int)WebTracker.Grids.CFSShipments.MainDischargePort);
			result.Add((int)WebTracker.Grids.CFSShipments.MainVessel);
			result.Add((int)WebTracker.Grids.CFSShipments.MainVoyage);
			result.Add((int)WebTracker.Grids.CFSShipments.Type);
			return result;
		}
	}
}
