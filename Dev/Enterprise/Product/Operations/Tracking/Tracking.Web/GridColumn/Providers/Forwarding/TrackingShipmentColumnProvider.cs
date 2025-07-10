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
	public class TrackingShipmentColumnProvider : GridColumnProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("0cb57c8d-00bd-4e6b-b0e2-f068b4f86504", "Shipment#"), ShipmentDeclarationSchema.Constants.Number) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipmentNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ShipmentPage) + (NoResString)"?Ref={0}&Table={1}"; // Partial URL
			shipmentNumberColumn.DataNavigateUrlFields = new string[2] { "PersistentBizOPK", "TableName" };
			AddToDictionaryAsRequired(shipmentNumberColumn);

			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("25d8710f-8ac6-4074-adb5-638f816e8d40", "Bill"), ShipmentDeclarationSchema.Constants.HouseBill) { ColumnKey = WebTracker.Grids.TrackingShipments.HouseBill });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("02cc2452-8cab-482d-98cc-68c777dd6d59", "Shipper"), ShipmentDeclarationSchema.Constants.ConsignorName) { ColumnKey = WebTracker.Grids.TrackingShipments.Shipper });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneeName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("8228c494-3cdd-4f49-b9f8-352ab1cbedb6", "Consignee"), ShipmentDeclarationSchema.Constants.ConsigneeName) { ColumnKey = WebTracker.Grids.TrackingShipments.Consignee });

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			ZCodeFindBoxColumn originColumn = new ZCodeFindBoxColumn(Res.GetString("61515b77-1b84-4935-ab1a-795ba292410f", "Origin"), ShipmentDeclarationSchema.Constants.OriginPortCode, "ShipmentDeclarationLookups.Ports") { ColumnKey = WebTracker.Grids.TrackingShipments.Origin };
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(originColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).ETDWithSuppression);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("51a455c4-7a33-4d5b-a8f0-e00d9dee6bd6", "ETD"), ShipmentDeclarationSchema.Constants.ETDWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ETD });

			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			ZCodeFindBoxColumn destinationColumn = new ZCodeFindBoxColumn(Res.GetString("a45b574a-d3ed-4e84-8254-59e398f6c7d5", "Destination"), ShipmentDeclarationSchema.Constants.DestinationPortCode, "ShipmentDeclarationLookups.Ports") { ColumnKey = WebTracker.Grids.TrackingShipments.Destination };
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(destinationColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).ETAWithSuppression);
			AddToDictionaryAsDefault(new ZDateTimeColumn(Res.GetString("b17f96ab-49dd-41ec-9b48-38b085f2bb25", "ETA"), ShipmentDeclarationSchema.Constants.ETAWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ETA });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).CurrentLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("c45f81fd-67fd-4025-9824-5f401cc8d92b", "Current Load Port"), ShipmentDeclarationSchema.Constants.CurrentLoadPort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.CurrentLoadPort });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).CurrentDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("b145c503-00e1-416d-b171-3eaf98354381", "Current Discharge Port"), ShipmentDeclarationSchema.Constants.CurrentDischargePort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.CurrentDischargePort });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).CurrentVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("578ff7b3-2b19-4320-a379-353835c592e7", "Current Vessel"), ShipmentDeclarationSchema.Constants.CurrentVessel) { ColumnKey = WebTracker.Grids.TrackingShipments.CurrentVessel });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).CurrentVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cb4892d8-c016-4016-bec0-b0cf4eed8523", "Current Voy./Flight"), ShipmentDeclarationSchema.Constants.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingShipments.CurrentVoyage });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).BookingReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("243259C8-F340-464F-BE59-CF9F98E1A1B0", "Shipper's Ref#"), ShipmentDeclarationSchema.Constants.BookingReference) { ColumnKey = WebTracker.Grids.TrackingShipments.BookingReference });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).OwnerReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3F60FAB3-AF30-4DA0-BE71-E38D5B688E36", "Owner's Ref#"), ShipmentDeclarationSchema.Constants.OwnerReference) { ColumnKey = WebTracker.Grids.TrackingShipments.OwnerReference });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).TransportMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0062dd99-b437-4458-9bf3-8a4b2f7f983e", "Mode"), ShipmentDeclarationSchema.Constants.TransportMode) { ColumnKey = WebTracker.Grids.TrackingShipments.Mode });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).PacksWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b62d0fbf-93ca-476d-961b-11d583a284c7", "Packs"), ShipmentDeclarationSchema.Constants.PacksWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Packs });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).WeightWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cf4c6248-c435-4b9f-82ec-c35d0364c080", "Weight"), ShipmentDeclarationSchema.Constants.WeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Weight });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).VolumeWithUnits);
			AddToDictionary(new ZTextEditColumn(Res.GetString("871ef2d4-0e48-49f7-ac26-53dd0da632f7", "Volume"), ShipmentDeclarationSchema.Constants.VolumeWithUnits) { ColumnKey = WebTracker.Grids.TrackingShipments.Volume });

			ZBindToChecker.CheckBindTo((ZDecimal)((IShipmentDeclaration)null).GoodsValue);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("ccc3b3c0-65d6-4a87-8177-eab205ee0452", "Goods Value"), ShipmentDeclarationSchema.Constants.GoodsValue) { ColumnKey = WebTracker.Grids.TrackingShipments.GoodsValue });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).GoodsValueCurrency);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Currencies);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("c66efac4-aa59-45c7-ad50-152e6ad6c6b1", "Currency"), ShipmentDeclarationSchema.Constants.GoodsValueCurrency, "ShipmentDeclarationLookups.Currencies") { ColumnKey = WebTracker.Grids.TrackingShipments.Currency });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).GoodsDescription);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e075ba9d-fe23-4ac6-8db0-868dffee54b9", "Goods Description"), ShipmentDeclarationSchema.Constants.GoodsDescription) { ColumnKey = WebTracker.Grids.TrackingShipments.GoodsDescription });

			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).EstimatedPickupDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("d9e6809d-ff01-4b8b-957a-8fc6c1bafa19", "Estimated Pickup"), ShipmentDeclarationSchema.Constants.EstimatedPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.EstimatedPickup });
			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).PickupDateRequiredBy);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("2cf3520a-c8e8-4807-bd0a-7a8132e0e6a0", "Pickup Required By"), ShipmentDeclarationSchema.Constants.PickupDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.PickupRequiredBy });
			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).EstimatedDeliveryDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("806df2ba-b294-4398-98b0-7cb4818556e2", "Estimated Delivery"), ShipmentDeclarationSchema.Constants.EstimatedDeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.EstimatedDelivery });
			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).DeliveryDateRequiredBy);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("1ead9b17-03f0-4613-b673-6abfd7e8656f", "Delivery Required By"), ShipmentDeclarationSchema.Constants.DeliveryDateRequiredBy, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryRequiredBy });
			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).DeliveryDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("3658c8c8-c709-4456-8c19-0ac50f5de8a1", "Delivery Date"), ShipmentDeclarationSchema.Constants.DeliveryDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryDate });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ServiceLevelCode);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.ServiceLevels);
			ZCodeFindBoxColumn serviceLevelColumn = new ZCodeFindBoxColumn(Res.GetString("65b3612b-abd2-4dee-9cdf-2cc5f30a22bc", "Service Level"), ShipmentDeclarationSchema.Constants.ServiceLevelCode, "ShipmentDeclarationLookups.ServiceLevels") { ColumnKey = WebTracker.Grids.TrackingShipments.ServiceLevel };
			serviceLevelColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(serviceLevelColumn);

			if (WebEnv.AppInstance != null)
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				if (siteUser != null && siteUser.CanViewAccounts)
				{
					ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).Charges);
					AddToDictionary(new ZTextEditColumn(Res.GetString("f46dd478-9165-4fa1-a0b0-88e2620ac91c", "Charges"), ShipmentDeclarationSchema.Constants.Charges) { ColumnKey = WebTracker.Grids.TrackingShipments.Charges });
				}
			}

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorFullAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("44c86346-85a6-41a5-a133-77ce70badec2", "Shipper Full Address"), ShipmentDeclarationSchema.Constants.ConsignorFullAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperFullAddress });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a16d759b-f75e-4f1c-b251-14049bc9ac42", "Shipper Address"), ShipmentDeclarationSchema.Constants.ConsignorAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperAddress });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorCity);
			AddToDictionary(new ZTextEditColumn(Res.GetString("f3f59dee-2fdf-422d-8b45-0442c655dcc0", "Shipper City"), ShipmentDeclarationSchema.Constants.ConsignorCity) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperCity });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b9399908-bedb-4d73-9cd2-4018eb079022", "Shipper State"), ShipmentDeclarationSchema.Constants.ConsignorState) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperState });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsignorPostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("76cd388d-c514-40f4-aaf2-9a938b250344", "Shipper Post Code"), ShipmentDeclarationSchema.Constants.ConsignorPostCode) { ColumnKey = WebTracker.Grids.TrackingShipments.ShipperPostCode });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneeFullAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("407a9731-0c3f-4eb4-85f8-19dc9e81d00e", "Consignee Full Address"), ShipmentDeclarationSchema.Constants.ConsigneeFullAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeFullAddress });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneeAddress);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cfe7cd0a-8600-48d5-975a-bdc0640ed9b7", "Consignee Address"), ShipmentDeclarationSchema.Constants.ConsigneeAddress) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeAddress });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneeCity);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b0258fcc-1265-472a-8654-c5beb99eb66e", "Consignee City"), ShipmentDeclarationSchema.Constants.ConsigneeCity) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeCity });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneeState);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cef5df52-026a-4834-bb71-6e71f2dc24cc", "Consignee State"), ShipmentDeclarationSchema.Constants.ConsigneeState) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneeState });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ConsigneePostCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cac35afb-be53-4e7f-a3e1-d978d29af3b3", "Consignee Post Code"), ShipmentDeclarationSchema.Constants.ConsigneePostCode) { ColumnKey = WebTracker.Grids.TrackingShipments.ConsigneePostCode });

			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).ReceivedDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("58de6d74-d81e-428a-95b2-4f0bd2df8899", "Received Date"), ShipmentDeclarationSchema.Constants.ReceivedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ReceivedDate });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ReceivedBy);
			AddToDictionary(new ZTextEditColumn(Res.GetString("a5dc0b5c-3add-412a-9673-72f227381c93", "Received By"), ShipmentDeclarationSchema.Constants.ReceivedBy) { ColumnKey = WebTracker.Grids.TrackingShipments.ReceivedBy });
			ZBindToChecker.CheckBindTo((ZInt)((IShipmentDeclaration)null).PiecesReceived);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("13fb25e2-2819-4dbc-8ecd-50d7e1146f63", "Pieces Received"), ShipmentDeclarationSchema.Constants.PiecesReceived) { ColumnKey = WebTracker.Grids.TrackingShipments.PiecesReceived });

			ZBindToChecker.CheckBindTo((ZBool)((IShipmentDeclaration)null).BookedOnline);
			AddToDictionary(new ZCheckBoxColumn(Res.GetString("3861e772-3ae6-4d1a-a70c-e8d2910627ee", "Booked Online"), ShipmentDeclarationSchema.Constants.BookedOnline) { ColumnKey = WebTracker.Grids.TrackingShipments.BookedOnline });
			ZBindToChecker.CheckBindTo((ZDateTime)((IShipmentDeclaration)null).ActualPickupDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("08faf301-7132-46a9-9d70-54ad2abf9f7e", "Actual Pickup"), ShipmentDeclarationSchema.Constants.ActualPickupDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.TrackingShipments.ActualPickup });

			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((IShipmentDeclaration)null).Milestones);
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

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).DeclarationCountry);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("b37f5935-c38a-4be0-97fc-b954c968b56d", "Declaration Country/Region"), "DeclarationCountry") { ColumnKey = WebTracker.Grids.TrackingShipments.DeclarationCountry });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).Top3Containers);
			AddToDictionary(new ZTextEditColumn(Res.GetString("ef43481e-af8d-486f-996d-504ab9c02d9d", "Containers"), "Top3Containers") { ColumnKey = WebTracker.Grids.TrackingShipments.Containers });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).OrderReference);
			AddToDictionary(new ZTextEditColumn(Res.GetString("869A2B02-F24C-4A4F-8E6E-EC0C0DC0769E", "Order Ref#"), ShipmentDeclarationSchema.Constants.OrderReference) { ColumnKey = WebTracker.Grids.TrackingShipments.OrderReferences });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).MainLoadPort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("ae15ec3b-f4e3-4167-a6c2-b114177445fc", "Main Load Port"), ShipmentDeclarationSchema.Constants.MainLoadPort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.MainLoadPort });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).MainDischargePort);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ShipmentDeclarationLookups.Ports);
			AddToDictionary(new ZCodeFindBoxColumn(Res.GetString("b5631fd1-7048-4a39-a9cd-8ce25e8483bc", "Main Discharge Port"), ShipmentDeclarationSchema.Constants.MainDischargePort, "ShipmentDeclarationLookups.Ports") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly, ColumnKey = WebTracker.Grids.TrackingShipments.MainDischargePort });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).MainVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("e08d2513-4fd2-492c-928b-0eb825c3f181", "Main Vessel"), ShipmentDeclarationSchema.Constants.MainVessel) { ColumnKey = WebTracker.Grids.TrackingShipments.MainVessel });
			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).MainVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b5805614-da10-42ac-91b6-814bf13d00f1", "Main Voy./Flight"), ShipmentDeclarationSchema.Constants.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingShipments.MainVoyage });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ShipmentType);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b6ac5cdd-4d17-450c-93b7-1a07f4de1dee", "Type"), ShipmentDeclarationSchema.Constants.ShipmentType) { ColumnKey = WebTracker.Grids.TrackingShipments.Type });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).InspectionTypeCode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("16d6b155-06db-42c9-9a33-3e2c0f1b1bc5", "Inspection"), ShipmentDeclarationSchema.Constants.InspectionTypeCode) { ColumnKey = WebTracker.Grids.TrackingShipments.Inspection });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).AdditionalTerms);
			AddToDictionary(new ZTextEditColumn(Res.GetString("cae70c61-afe0-4725-b9f6-db0d5166e0d1", "Additional Terms"), ShipmentDeclarationSchema.Constants.AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingShipments.AdditionalTerms });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).PaymentTerm);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).PaymentTerm_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("376a6b91-e51a-41a4-9f87-0dd113aff8ea", "Payment Term"), ShipmentDeclarationSchema.Constants.PaymentTerm)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.INCO,
				BindToList = "PaymentTerm_List"
			});

			ZBindToChecker.CheckBindTo((ZDecimal)((IShipmentDeclaration)null).LoadingMeters);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("b2586f49-cc82-4844-aa95-1449dfcaf676", "Loading Meters"), ShipmentDeclarationSchema.Constants.LoadingMeters) { ColumnKey = WebTracker.Grids.TrackingShipments.LoadingMeters });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ContainerMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9e65d733-f2df-46d8-851d-e8dd17dc4708", "Container Mode"), ShipmentDeclarationSchema.Constants.ContainerMode) { ColumnKey = WebTracker.Grids.TrackingShipments.ContainerMode });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ChargesApply);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ChargesApply_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("4429a8d0-c621-4195-9f87-512c549d311d", "Charges Apply"), ShipmentDeclarationSchema.Constants.ChargesApply)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.ChargesApply,
				BindToList = "ChargesApply_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).ReleaseType);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).ReleaseType_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("575d65f6-f305-4709-bbce-5c12019c1213", "Release Type"), ShipmentDeclarationSchema.Constants.ReleaseType)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.ReleaseType,
				BindToList = "ReleaseType_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).OnBoard);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((IShipmentDeclaration)null).OnBoard_List);
			AddToDictionary(new ZDropEditColumn(Res.GetString("c5503c1a-744d-4140-971f-6896e6b2cf9b", "On Board"), ShipmentDeclarationSchema.Constants.OnBoard)
			{
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly,
				ColumnKey = WebTracker.Grids.TrackingShipments.OnBoard,
				BindToList = "OnBoard_List"
			});

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).PickupAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("20991196-297c-4150-b971-e1fa898a532b", "Pickup Agent"), ShipmentDeclarationSchema.Constants.PickupAgentFullName) { ColumnKey = WebTracker.Grids.TrackingShipments.PickupAgent });

			ZBindToChecker.CheckBindTo((ZString)((IShipmentDeclaration)null).DeliveryAgentFullName);
			AddToDictionary(new ZTextEditColumn(Res.GetString("4888e080-1652-4177-ac87-743573bcce5d", "Delivery Agent"), ShipmentDeclarationSchema.Constants.DeliveryAgentFullName) { ColumnKey = WebTracker.Grids.TrackingShipments.DeliveryAgent });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingShipment)null).StorageDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("D07F04FB-7FAC-460A-ABCF-A58EFFCBF367", "Storage Commences"), ShipmentDeclarationSchema.Constants.StorageDate) { ColumnKey = WebTracker.Grids.TrackingShipments.StorageDate });

			ZBindToChecker.CheckBindTo((ZDecimal)((TrackingShipment)null).TEUCount);
			AddToDictionary(new ZCalcEditColumn(Res.GetString("1d5b757a-6678-4a8d-b237-006c8b0e574f", "TEU"), ShipmentDeclarationSchema.Constants.TEUCount) { ColumnKey = WebTracker.Grids.TrackingShipments.TEUCount });

			ZBindToChecker.CheckBindTo((ZString)((TrackingShipment)null).Top3JobNotes);
			AddToDictionary(new ZTextEditColumn(Res.GetString("1b9110b4-6bb9-4a10-8b81-db6058e03b3b", "Job Notes"), ShipmentDeclarationSchema.Constants.Top3JobNotes) { ColumnKey = WebTracker.Grids.TrackingShipments.JobNotes });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingShipment)null).FirstLegLoadETD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("7E4AA2D0-921B-4B26-997B-2EDC5E829FC6", "First Leg Load ETD"), ShipmentDeclarationSchema.Constants.FirstLegLoadETD) { ColumnKey = WebTracker.Grids.TrackingShipments.FirstLegLoadETD });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingShipment)null).FirstLegLoadATD);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("CA4F4F00-744D-4904-A26B-075C2C8CFAE7", "First Leg Load ATD"), ShipmentDeclarationSchema.Constants.FirstLegLoadATD) { ColumnKey = WebTracker.Grids.TrackingShipments.FirstLegLoadATD });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingShipment)null).LastLegDischargeETA);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("165D1CF9-9D0E-4CAE-9804-841B2FFEFCB4", "Last Leg Discharge ETA"), ShipmentDeclarationSchema.Constants.LastLegDischargeETA) { ColumnKey = WebTracker.Grids.TrackingShipments.LastLegDischargeETA });
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingShipment)null).LastLegDischargeATA);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("B2F3B3B6-0FA7-49D4-865E-7E8C7E64AE1A", "Last Leg Discharge ATA"), ShipmentDeclarationSchema.Constants.LastLegDischargeATA) { ColumnKey = WebTracker.Grids.TrackingShipments.LastLegDischargeATA });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipmentNumber);
			result.Add((int)WebTracker.Grids.TrackingShipments.HouseBill);
			result.Add((int)WebTracker.Grids.TrackingShipments.Shipper);
			result.Add((int)WebTracker.Grids.TrackingShipments.Consignee);
			result.Add((int)WebTracker.Grids.TrackingShipments.Origin);
			result.Add((int)WebTracker.Grids.TrackingShipments.ETD);
			result.Add((int)WebTracker.Grids.TrackingShipments.Destination);
			result.Add((int)WebTracker.Grids.TrackingShipments.ETA);
			result.Add((int)WebTracker.Grids.TrackingShipments.CurrentLoadPort);
			result.Add((int)WebTracker.Grids.TrackingShipments.CurrentDischargePort);
			result.Add((int)WebTracker.Grids.TrackingShipments.CurrentVessel);
			result.Add((int)WebTracker.Grids.TrackingShipments.CurrentVoyage);
			result.Add((int)WebTracker.Grids.TrackingShipments.BookingReference);
			result.Add((int)WebTracker.Grids.TrackingShipments.OwnerReference);
			result.Add((int)WebTracker.Grids.TrackingShipments.Mode);
			result.Add((int)WebTracker.Grids.TrackingShipments.Packs);
			result.Add((int)WebTracker.Grids.TrackingShipments.Weight);
			result.Add((int)WebTracker.Grids.TrackingShipments.Volume);
			result.Add((int)WebTracker.Grids.TrackingShipments.GoodsValue);
			result.Add((int)WebTracker.Grids.TrackingShipments.Currency);
			result.Add((int)WebTracker.Grids.TrackingShipments.GoodsDescription);
			result.Add((int)WebTracker.Grids.TrackingShipments.EstimatedPickup);
			result.Add((int)WebTracker.Grids.TrackingShipments.PickupRequiredBy);
			result.Add((int)WebTracker.Grids.TrackingShipments.EstimatedDelivery);
			result.Add((int)WebTracker.Grids.TrackingShipments.DeliveryRequiredBy);
			result.Add((int)WebTracker.Grids.TrackingShipments.DeliveryDate);
			result.Add((int)WebTracker.Grids.TrackingShipments.ServiceLevel);
			result.Add((int)WebTracker.Grids.TrackingShipments.Charges);
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipperFullAddress);
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipperAddress);
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipperCity);
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipperState);
			result.Add((int)WebTracker.Grids.TrackingShipments.ShipperPostCode);
			result.Add((int)WebTracker.Grids.TrackingShipments.ConsigneeFullAddress);
			result.Add((int)WebTracker.Grids.TrackingShipments.ConsigneeAddress);
			result.Add((int)WebTracker.Grids.TrackingShipments.ConsigneeCity);
			result.Add((int)WebTracker.Grids.TrackingShipments.ConsigneeState);
			result.Add((int)WebTracker.Grids.TrackingShipments.ConsigneePostCode);
			result.Add((int)WebTracker.Grids.TrackingShipments.ReceivedDate);
			result.Add((int)WebTracker.Grids.TrackingShipments.ReceivedBy);
			result.Add((int)WebTracker.Grids.TrackingShipments.PiecesReceived);
			result.Add((int)WebTracker.Grids.TrackingShipments.BookedOnline);
			result.Add((int)WebTracker.Grids.TrackingShipments.ActualPickup);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				result.Add(column.UniqueKey);
			}
			result.Add((int)WebTracker.Grids.TrackingShipments.Containers);
			result.Add((int)WebTracker.Grids.TrackingShipments.OrderReferences);
			result.Add((int)WebTracker.Grids.TrackingShipments.MainLoadPort);
			result.Add((int)WebTracker.Grids.TrackingShipments.MainDischargePort);
			result.Add((int)WebTracker.Grids.TrackingShipments.MainVessel);
			result.Add((int)WebTracker.Grids.TrackingShipments.MainVoyage);
			result.Add((int)WebTracker.Grids.TrackingShipments.Type);
			return result;
		}
	}
}
