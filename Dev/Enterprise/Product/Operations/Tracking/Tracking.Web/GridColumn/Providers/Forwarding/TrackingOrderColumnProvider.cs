using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public class TrackingOrderColumnProvider : GridColumnProvider
	{
		public TrackingOrderColumnProvider(bool isUsedAsLookup)
		{
			this.isUsedAsLookup = isUsedAsLookup;
		}
		readonly bool isUsedAsLookup;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript HTML")]
		public const string ShipmentQuickViewUserLoginRequest = @"javascript:return confirm('Error: You need to be logged in with a full account to see this information.\n\nPress Ok to Login now ? Cancel to continue');";

		protected override List<int> PopulateRestrictedColumns()
		{
			var restricted = new List<int>();

			if (true.Equals((WebEnv.AppInstance.SiteUser as TrackingSiteUser)?.IsShipmentQuickViewUser))
			{
				restricted.AddRange(new[]
				{
					(int)WebTracker.Grids.TrackingOrders.SplitNumber,
					(int)WebTracker.Grids.TrackingOrders.Supplier,
					(int)WebTracker.Grids.TrackingOrders.Buyer,
					(int)WebTracker.Grids.TrackingOrders.ControllingCustomer,
					(int)WebTracker.Grids.TrackingOrders.Origin,
					(int)WebTracker.Grids.TrackingOrders.Destination,
					(int)WebTracker.Grids.TrackingOrders.Packs,
					(int)WebTracker.Grids.TrackingOrders.Volume,
					(int)WebTracker.Grids.TrackingOrders.Weight
				});
			}

			return restricted;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "ZBindToChecker requires a redundant cast")]
		protected override void CustomizeDictionaryCore()
		{
			base.CustomizeDictionaryCore();
			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_OrderNumberAndSplit);
			if (isUsedAsLookup)
			{
				AddButtonColumn(Res.GetString("f5bc13b5-d028-4dad-b064-861926cd2561", "Order #"), TrackingOrder.Schema.JD_OrderNumberAndSplit, WebTracker.Grids.TrackingOrders.OrderNumber);
			}
			else
			{
				TrackingSiteUser siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;
				if (siteUser != null && siteUser.IsShipmentQuickViewUser)
				{
					ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_OrderNumberAndSplit);
					AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("f5bc13b5-d028-4dad-b064-861926cd2561", "Order #"), TrackingOrder.Schema.JD_OrderNumberAndSplit) { ColumnKey = WebTracker.Grids.TrackingOrders.OrderNumber });
				}
				else
				{
					ZHyperLinkColumn orderNumberColumn = new ZHyperLinkColumn(Res.GetString("f5bc13b5-d028-4dad-b064-861926cd2561", "Order #"), TrackingOrder.Schema.JD_OrderNumberAndSplit) { ColumnKey = WebTracker.Grids.TrackingOrders.OrderNumber };
					orderNumberColumn.DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.OrderDetailsPage) + (NoResString)"?Ref={0}"; // Redirection path
					orderNumberColumn.DataNavigateUrlFields = new string[1] { "PK" };
					AddToDictionaryAsRequired(orderNumberColumn);
				}
			}

			ZBindToChecker.CheckBindTo((ZByte)((TrackingOrder)null).JD_OrderNumberSplit);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("071AC6EB-6690-4118-AF3E-3E729C2AED22", "Split Number"), TrackingOrder.Schema.JD_OrderNumberSplit) { ColumnKey = WebTracker.Grids.TrackingOrders.SplitNumber });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_TransportMode);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e4cb1971-5645-4373-9a44-47c5a3e64646", "Transport Mode"), TrackingOrder.Schema.JD_TransportMode) { ColumnKey = WebTracker.Grids.TrackingOrders.TransportMode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).SupplierName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("a98afb4c-5b10-4dc8-8e29-15ae4786bba3", "Supplier"), TrackingOrder.Schema.SupplierName) { ColumnKey = WebTracker.Grids.TrackingOrders.Supplier });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).BuyerName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("aeedfc34-8dbd-447d-8f26-5fd278a0a95d", "Buyer"), TrackingOrder.Schema.BuyerName) { ColumnKey = WebTracker.Grids.TrackingOrders.Buyer });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).ControllingCustomerDocAddress.E2_CompanyName);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("e227be31-84fc-43dc-913a-db4efbb34349", "Controlling Customer"), "ControllingCustomerDocAddress+E2_CompanyName") { ColumnKey = WebTracker.Grids.TrackingOrders.ControllingCustomer });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_OrderStatusDesc);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("6acb906d-dcd3-445d-ad39-155a401bb8f9", "Status"), TrackingOrder.Schema.JD_OrderStatusDesc) { ColumnKey = WebTracker.Grids.TrackingOrders.Status });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_OrderDate);
			ZDateTimeColumn orderDateColumn = new ZDateTimeColumn(Res.GetString("aedf16ea-5d2d-479c-b5d9-70274627bb90", "Order Date"), TrackingOrder.Schema.JD_OrderDate) { ColumnKey = WebTracker.Grids.TrackingOrders.OrderDate };
			orderDateColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AddToDictionaryAsDefault(orderDateColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_RL_NKGoodsAvailableAt);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.GoodsAvailableAts);
			ZCodeFindBoxColumn originColumn = new ZCodeFindBoxColumn(Res.GetString("d87e2768-891d-417d-94aa-73d2eaa0b8ec", "Origin"), TrackingOrder.Schema.JD_RL_NKGoodsAvailableAt, "Lookups+GoodsAvailableAts", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.Origin };
			originColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(originColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_RL_NKGoodsDeliveredTo);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.GoodsDeliveredTos);
			ZCodeFindBoxColumn destinationColumn = new ZCodeFindBoxColumn(Res.GetString("6beba745-8983-4474-be34-9f837505a8d5", "Destination"), TrackingOrder.Schema.JD_RL_NKGoodsDeliveredTo, "Lookups+GoodsDeliveredTos", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.Destination };
			destinationColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionaryAsDefault(destinationColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).CurrentVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("d2a0672c-529a-410b-8b8a-f5458f8e62ee", "Current Vessel"), TrackingOrder.Schema.CurrentVessel) { ColumnKey = WebTracker.Grids.TrackingOrders.CurrentVessel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).CurrentVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("fcd56927-0348-456e-a763-e436afcdfdca", "Current Voyage/Flight"), TrackingOrder.Schema.CurrentVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingOrders.CurrentVoyage });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_PacksWithUnits);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("4bc6f1c1-fee8-4670-8020-b47036084880", "Packs"), TrackingOrder.Schema.JD_PacksWithUnits) { ColumnKey = WebTracker.Grids.TrackingOrders.Packs });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_ActualVolumeWithUnits);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("5a5d70ee-1ebb-42ec-bc9f-f2dbd740f084", "Volume"), TrackingOrder.Schema.JD_ActualVolumeWithUnits) { ColumnKey = WebTracker.Grids.TrackingOrders.Volume });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_ActualWeightWithUnits);
			AddToDictionaryAsDefault(new ZTextEditColumn(Res.GetString("130332fa-0f36-4f5a-97b7-4f1dad3c5e4c", "Weight"), TrackingOrder.Schema.JD_ActualWeightWithUnits) { ColumnKey = WebTracker.Grids.TrackingOrders.Weight });

			ZBindToChecker.CheckBindTo((TrackingMilestoneCollection)((TrackingOrder)null).Milestones);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				AddToDictionary(column);
			}

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_ExWorksRequiredBy);
			ZDateTimeColumn reqExWorksColumn = new ZDateTimeColumn(Res.GetString("d581829a-ddb3-4323-a2b7-d3fafb46d87d", "Req. Ex Works"), TrackingOrder.Schema.JD_ExWorksRequiredBy) { ColumnKey = WebTracker.Grids.TrackingOrders.RequiredExWorks };
			reqExWorksColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AddToDictionary(reqExWorksColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_DeliveryRequiredBy);
			ZDateTimeColumn reqInStoreColumn = new ZDateTimeColumn(Res.GetString("705b11fd-fe25-4624-ab26-19feed28528c", "Req. In Store"), TrackingOrder.Schema.JD_DeliveryRequiredBy) { ColumnKey = WebTracker.Grids.TrackingOrders.RequiredInStore };
			reqInStoreColumn.DateTimeFormat = ZDateTimePickerFormat.Short;
			AddToDictionary(reqInStoreColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_EXW);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_EXW);
			AddToDictionary(new ZTimelineColumn(Res.GetString("159d3e8a-a009-425b-bb3b-4c942d94ab57", "Ex-Factory"), TrackingOrder.Schema.JD_Milestone_A_EXW, TrackingOrder.Schema.JD_Milestone_E_EXW, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ExFactory });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_GIW);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_GIW);
			AddToDictionary(new ZTimelineColumn(Res.GetString("d90c977e-8b18-48ab-a3a0-6614da214020", "Origin Receival"), TrackingOrder.Schema.JD_Milestone_A_GIW, TrackingOrder.Schema.JD_Milestone_E_GIW, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.OriginReceival });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).ATDWithSuppression);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).ETDWithSuppression);
			AddToDictionary(new ZTimelineColumn(Res.GetString("a735fdfd-ffd5-4bb0-bec4-007091c6923c", "Departure"), TrackingOrder.Schema.ATDWithSuppression, TrackingOrder.Schema.ETDWithSuppression, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Departure });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).ATAWithSuppression);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).ETAWithSuppression);
			AddToDictionary(new ZTimelineColumn(Res.GetString("1796570d-a137-471c-b528-35a086850d29", "Arrival"), TrackingOrder.Schema.ATAWithSuppression, TrackingOrder.Schema.ETAWithSuppression, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Arrival });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_CCC);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_CCC);
			AddToDictionary(new ZTimelineColumn(Res.GetString("b5117cfa-a84f-4b2d-a793-88e9e28c50d5", "Clearance Commenced"), TrackingOrder.Schema.JD_Milestone_A_CCC, TrackingOrder.Schema.JD_Milestone_E_CCC, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ClearanceCommenced });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_CLR);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_CLR);
			AddToDictionary(new ZTimelineColumn(Res.GetString("17a4d0be-4b2d-452e-a602-e8842314c4a8", "Clearance Finalized"), TrackingOrder.Schema.JD_Milestone_A_CLR, TrackingOrder.Schema.JD_Milestone_E_CLR, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.ClearanceFinalized });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_CAV);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_CAV);
			AddToDictionary(new ZTimelineColumn(Res.GetString("76394cd5-4e63-46ef-a1f5-cf2e576c8c6e", "Unpacked"), TrackingOrder.Schema.JD_Milestone_A_CAV, TrackingOrder.Schema.JD_Milestone_E_CAV, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Unpacked });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_DCA);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_DCA);
			AddToDictionary(new ZTimelineColumn(Res.GetString("d4702c13-35c5-408a-83ff-a77e048ed1e3", "Port Transport Advised"), TrackingOrder.Schema.JD_Milestone_A_DCA, TrackingOrder.Schema.JD_Milestone_E_DCA, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.LocalTransportAdvised });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_A_DCF);
			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_Milestone_E_DCF);
			AddToDictionary(new ZTimelineColumn(Res.GetString("50cfc75b-283c-4a34-acf1-c192c2ce51ab", "Delivered"), TrackingOrder.Schema.JD_Milestone_A_DCF, TrackingOrder.Schema.JD_Milestone_E_DCF, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.TrackingOrders.Delivered });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_Waybill);
			ZTextEditColumn houseBillColumn = new ZTextEditColumn(Res.GetString("4381ad62-802a-4c67-a0e6-a79251c1acfd", "House Bill"), TrackingOrder.Schema.JD_Waybill) { ColumnKey = WebTracker.Grids.TrackingOrders.HouseBill };
			AddToDictionary(houseBillColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_MasterWaybill);
			ZTextEditColumn masterBillColumn = new ZTextEditColumn(Res.GetString("1ba8b205-df61-4489-9c4d-0170d398b9a9", "Master Bill"), TrackingOrder.Schema.JD_MasterWaybill) { ColumnKey = WebTracker.Grids.TrackingOrders.MasterBill };
			AddToDictionary(masterBillColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_RL_NKPortOfLoading);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.PortOfLoadings);
			ZCodeFindBoxColumn loadingColumn = new ZCodeFindBoxColumn(Res.GetString("67ee491f-1ff1-42fb-b6c6-73b84b77e390", "Load"), TrackingOrder.Schema.JD_RL_NKPortOfLoading, "Lookups+PortOfLoadings", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.Load };
			loadingColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(loadingColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_RL_NKPortOfDischarge);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.PortOfDischarges);
			ZCodeFindBoxColumn dischargeColumn = new ZCodeFindBoxColumn(Res.GetString("63a004ca-a4de-4ec2-a5ac-c167628ce5c9", "Discharge"), TrackingOrder.Schema.JD_RL_NKPortOfDischarge, "Lookups+PortOfDischarges", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.Discharge };
			dischargeColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(dischargeColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).PickupAddressAsString);
			AddToDictionary(new ZTextEditColumn(Res.GetString("b713f3b0-26c0-4ef5-9492-247ad528349e", "Pickup Address"), TrackingOrder.Schema.PickupAddressAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.PickupAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).DeliveryAddressAsString);
			AddToDictionary(new ZTextEditColumn(Res.GetString("3075f51b-bfd4-453e-884f-cb1bae575783", "Delivery Address"), TrackingOrder.Schema.DeliveryAddressAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.DeliveryAddress });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).ConsolsAsString);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9c93d473-59de-4dc6-bbaf-406fe4e2b6c5", "Consol #"), TrackingOrder.Schema.ConsolsAsString) { ColumnKey = WebTracker.Grids.TrackingOrders.ConsolNumber });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_BookingConfRef);
			AddToDictionary(new ZTextEditColumn(Res.GetString("1b2b8a06-c5a6-44db-9da3-f38e7e6bde4d", "Booking Conf. Ref. #"), JobOrderHeaderSchema.JD_BookingConfRef.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.BookingConfRef });

			ZBindToChecker.CheckBindTo((PKDescriptionCollection)((TrackingOrder)null).Containers);
			ZHyperLinksColumn columnContainers = new ZHyperLinksColumn(Res.GetString("d9859d09-f4f0-4cbe-b64e-f13f23fb79f8", "Container #"), (NoResString)"Containers", PKDescription.Schema.Description) { ColumnKey = WebTracker.Grids.TrackingOrders.ContainerNumber };
			columnContainers.DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ContainerDetailsPage) + (NoResString)"?Ref={0}"; // Redirection path
			columnContainers.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionary(columnContainers);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_InvoiceNumber);
			AddToDictionary(new ZTextEditColumn(Res.GetString("08df43fc-987b-4875-b0f0-364204666e40", "Invoice #"), JobOrderHeaderSchema.JD_InvoiceNumber.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.InvoiceNumber });

			ZBindToChecker.CheckBindTo((OrgSupplierPartCollection)((TrackingOrder)null).Products);
			ZHyperLinksColumn columnProducts = new ZHyperLinksColumn(Res.GetString("46109dcf-2427-4065-bb3f-5417d0df4cd9", "Product #"), (NoResString)"Products", OrgSupplierPartSchema.OP_PartNum.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.ProductNumber };
			columnProducts.DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ProductProfileDetailsPage) + (NoResString)"?Ref={0}"; // Redirection path
			columnProducts.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionary(columnProducts);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).ShipOrDecNumber);
			ZBindToChecker.CheckBindTo((ZGuid)((TrackingOrder)null).ShipOrDecPK);
			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).ShipOrDecTableName);
			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("e77cc8cd-49fe-4285-af34-5650c8f9f794", "Shipment #"), TrackingOrder.Schema.ShipOrDecNumber) { ColumnKey = WebTracker.Grids.TrackingOrders.ShipmentNumber };
			shipmentNumberColumn.DataNavigateUrlFormatString = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ShipmentPage) + (NoResString)"?Ref={0}&Table={1}"; // Redirection path
			shipmentNumberColumn.DataNavigateUrlFields = new string[] { TrackingOrder.Schema.ShipOrDecPK, TrackingOrder.Schema.ShipOrDecTableName };
			AddToDictionary(shipmentNumberColumn);

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_InvoiceDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("78c83bf8-eede-49ba-a376-59f44e922204", "Confirmed Date"), JobOrderHeaderSchema.JD_BookingConfDate.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.ConfirmedDate });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_FollowUpDate);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("08061277-8398-410d-85ac-5db214617261", "Follow Up Date"), JobOrderHeaderSchema.JD_FollowUpDate.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.FollowUpDate });

			ZBindToChecker.CheckBindTo((ZGuid)((TrackingOrder)null).JD_OH_SendingAgent);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.SendingAgents);
			ZFindBoxColumn sendingAgentColumn = new ZFindBoxColumn(Res.GetString("0ad412d0-490b-4c77-b950-97bf81011ce8", "Sending Agent"), TrackingOrder.Schema.JD_OH_SendingAgent, "Lookups+SendingAgents", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.SendingAgent };
			sendingAgentColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(sendingAgentColumn);

			ZBindToChecker.CheckBindTo((ZGuid)((TrackingOrder)null).JD_OH_ReceivingAgent);
			ZBindToChecker.CheckBindTo((ICodeDescriptionPairList)((TrackingOrder)null).Lookups.ReceivingAgents);
			ZFindBoxColumn receivingAgentColumn = new ZFindBoxColumn(Res.GetString("2e2f8759-bbb3-41bd-a92d-af65415b5534", "Receiving Agent"), TrackingOrder.Schema.JD_OH_ReceivingAgent, "Lookups+ReceivingAgents", typeof(TrackingOrder)) { ColumnKey = WebTracker.Grids.TrackingOrders.ReceivingAgent };
			receivingAgentColumn.DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly;
			AddToDictionary(receivingAgentColumn);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).ServiceLevel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("421dc5a5-86cc-439a-9ae2-a1eb699a9cdc", "Service Level"), TrackingOrder.Schema.ServiceLevel) { ColumnKey = WebTracker.Grids.TrackingOrders.ServiceLevel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_TransportMode);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9e65d733-f2df-46d8-851d-e8dd17dc4708", "Container Mode"), JobOrderHeaderSchema.JD_ContainerMode.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.ContainerMode });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).CreatedOn);
			AddToDictionary(new ZTextEditColumn(Res.GetString("0e0b2fa6-c6e9-4d3a-86fe-9b24b8728504", "Created On"), TrackingOrder.Schema.CreatedOn) { ColumnKey = WebTracker.Grids.TrackingOrders.CreatedOn });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_SystemCreateTimeUtc);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("710d4794-c3db-4213-89cb-4318aab10dd1", "Created Time"), JobOrderHeaderSchema.JD_SystemCreateTimeUtc.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.CreatedTime });

			ZBindToChecker.CheckBindTo((ZDateTime)((TrackingOrder)null).JD_SystemLastEditTimeUtc);
			AddToDictionary(new ZDateTimeColumn(Res.GetString("11a53e1f-0ad2-4117-b982-47181eed3be9", "Last Edit Time"), JobOrderHeaderSchema.JD_SystemLastEditTimeUtc.Name) { ColumnKey = WebTracker.Grids.TrackingOrders.LastEditTime });

			if (WebEnv.AppInstance != null)
			{
				var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
				if (siteUser != null)
				{
					var loggedInOrg = siteUser.LoggedInOrganisation;
					if (loggedInOrg != null)
					{
						int customColumnKey = 1500;
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on TrackingOrder
						foreach (CustomLabelInfo field in new TrackingOrder.CustomLabelsProvider(null, true).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
#pragma warning restore IDE0001 // Simplify Names
						{
							if (field.LabelName.StartsWith("OrderHeader."))
							{
								if (AddToDictionary(field, customColumnKey))
								{
									customColumnKey++;
								}
							}
						}
					}
				}
			}

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).MainVessel);
			AddToDictionary(new ZTextEditColumn(Res.GetString("583f6a56-2956-4789-9fa8-498dca8c9380", "Main Vessel"), TrackingOrder.Schema.MainVessel) { ColumnKey = WebTracker.Grids.TrackingOrders.MainVessel });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).MainVoyageWithSuppression);
			AddToDictionary(new ZTextEditColumn(Res.GetString("9ee5bc3c-0302-4c10-8b31-e8c945ae6beb", "Main Voyage/Flight"), TrackingOrder.Schema.MainVoyageWithSuppression) { ColumnKey = WebTracker.Grids.TrackingOrders.MainVoyage });

			ZBindToChecker.CheckBindTo((PKDescriptionCollection)((TrackingOrder)null).PlannedContainerNumbers);
			ZHyperLinksColumn columnPlannedContainers = new ZHyperLinksColumn(Res.GetString("c7f4f05a-f51e-4207-8fc5-c82d517afa09", "Planned Container #"), "PlannedContainerNumbers", PKDescription.Schema.Description) { ColumnKey = WebTracker.Grids.TrackingOrders.PlannedContainers };
			columnPlannedContainers.DataNavigateUrlFormatString = UrlFormatWithAppRoot(TrackingConstants.RelativePath.ContainerDetailsPage) + (NoResString)"?Ref={0}"; // Redirection path
			columnPlannedContainers.DataNavigateUrlFields = new string[] { "PK" };
			AddToDictionary(columnPlannedContainers);

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_IncoTerm);
			AddToDictionary(new ZDropEditColumn(Res.GetString("dee85523-ea99-75a2-4642-72b2a3c56dfc", "Incoterm"), TrackingOrder.Schema.JD_IncoTerm) { ColumnKey = WebTracker.Grids.TrackingOrders.IncoTerm });

			ZBindToChecker.CheckBindTo((ZString)((TrackingOrder)null).JD_AdditionalTerms);
			AddToDictionary(new ZTextEditColumn(Res.GetString("fb3015ff-4ebb-4b2c-9b08-4d6dd5951f04", "Additional Terms"), TrackingOrder.Schema.JD_AdditionalTerms) { ColumnKey = WebTracker.Grids.TrackingOrders.AdditionalTerms });
		}

		protected override List<int> GetOldColumnsOrder()
		{
			List<int> result = new List<int>();
			result.Add((int)WebTracker.Grids.TrackingOrders.OrderNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.SplitNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.TransportMode);
			result.Add((int)WebTracker.Grids.TrackingOrders.Supplier);
			result.Add((int)WebTracker.Grids.TrackingOrders.Buyer);
			result.Add((int)WebTracker.Grids.TrackingOrders.ControllingCustomer);
			result.Add((int)WebTracker.Grids.TrackingOrders.Status);
			result.Add((int)WebTracker.Grids.TrackingOrders.OrderDate);
			result.Add((int)WebTracker.Grids.TrackingOrders.Origin);
			result.Add((int)WebTracker.Grids.TrackingOrders.Destination);
			result.Add((int)WebTracker.Grids.TrackingOrders.CurrentVessel);
			result.Add((int)WebTracker.Grids.TrackingOrders.CurrentVoyage);
			result.Add((int)WebTracker.Grids.TrackingOrders.Packs);
			result.Add((int)WebTracker.Grids.TrackingOrders.Volume);
			result.Add((int)WebTracker.Grids.TrackingOrders.Weight);
			foreach (ZTemplateColumn column in ConfigurationHelper.GetMilestonesColumns((NoResString)"Milestones")) // Data column name
			{
				result.Add(column.UniqueKey);
			}
			result.Add((int)WebTracker.Grids.TrackingOrders.RequiredExWorks);
			result.Add((int)WebTracker.Grids.TrackingOrders.RequiredInStore);
			result.Add((int)WebTracker.Grids.TrackingOrders.ExFactory);
			result.Add((int)WebTracker.Grids.TrackingOrders.OriginReceival);
			result.Add((int)WebTracker.Grids.TrackingOrders.Departure);
			result.Add((int)WebTracker.Grids.TrackingOrders.Arrival);
			result.Add((int)WebTracker.Grids.TrackingOrders.ClearanceCommenced);
			result.Add((int)WebTracker.Grids.TrackingOrders.ClearanceFinalized);
			result.Add((int)WebTracker.Grids.TrackingOrders.Unpacked);
			result.Add((int)WebTracker.Grids.TrackingOrders.LocalTransportAdvised);
			result.Add((int)WebTracker.Grids.TrackingOrders.Delivered);
			result.Add((int)WebTracker.Grids.TrackingOrders.HouseBill);
			result.Add((int)WebTracker.Grids.TrackingOrders.MasterBill);
			result.Add((int)WebTracker.Grids.TrackingOrders.Load);
			result.Add((int)WebTracker.Grids.TrackingOrders.Discharge);
			result.Add((int)WebTracker.Grids.TrackingOrders.PickupAddress);
			result.Add((int)WebTracker.Grids.TrackingOrders.DeliveryAddress);
			result.Add((int)WebTracker.Grids.TrackingOrders.ConsolNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.BookingConfRef);
			result.Add((int)WebTracker.Grids.TrackingOrders.ContainerNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.InvoiceNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.ProductNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.ShipmentNumber);
			result.Add((int)WebTracker.Grids.TrackingOrders.ConfirmedDate);
			result.Add((int)WebTracker.Grids.TrackingOrders.FollowUpDate);
			result.Add((int)WebTracker.Grids.TrackingOrders.SendingAgent);
			result.Add((int)WebTracker.Grids.TrackingOrders.ReceivingAgent);
			result.Add((int)WebTracker.Grids.TrackingOrders.ServiceLevel);
			result.Add((int)WebTracker.Grids.TrackingOrders.ContainerMode);
			result.Add((int)WebTracker.Grids.TrackingOrders.CreatedOn);
			result.Add((int)WebTracker.Grids.TrackingOrders.CreatedTime);
			result.Add((int)WebTracker.Grids.TrackingOrders.LastEditTime);
			var siteUser = WebEnv.AppInstance.SiteUser as OrgContactWebUser;
			if (siteUser != null)
			{
				var loggedInOrg = siteUser.LoggedInOrganisation;
				if (loggedInOrg != null)
				{
					int customColumnKey = 1500;
#pragma warning disable IDE0001 // Simplify Names - Reason = All logic in this file has its scope on TrackingOrder
					foreach (CustomLabelInfo field in new TrackingOrder.CustomLabelsProvider(null, true).GetCustomFields(loggedInOrg, loggedInOrg.Factory))
#pragma warning restore IDE0001 // Simplify Names
					{
						if (field.LabelName.StartsWith("OrderHeader.") && field.IsEnabled)
						{
							result.Add(customColumnKey);
							customColumnKey++;
						}
					}
				}
			}
			result.Add((int)WebTracker.Grids.TrackingOrders.MainVessel);
			result.Add((int)WebTracker.Grids.TrackingOrders.MainVoyage);
			return result;
		}
	}
}
