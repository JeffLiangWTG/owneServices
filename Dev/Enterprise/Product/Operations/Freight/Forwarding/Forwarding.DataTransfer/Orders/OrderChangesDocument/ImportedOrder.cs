using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ImportedOrder : ImportedObject
	{
		public ImportedOrder(Order order)
		{
			Populate(order);
		}

		public ImportedOrderState OrderState
		{
			get { return orderState; }
		}
		ImportedOrderState orderState = ImportedOrderState.New;

		#region Relationships

		public ImportedOrderLineCollection OrderLines
		{
			get { return orderLines; }
		}
		ImportedOrderLineCollection orderLines;

		void SetupOrderLines(Order order)
		{
			if (orderLines != null)
			{
				throw new NotSupportedException("OrderLines already setup");
			}

			orderLines = new ImportedOrderLineCollection();
			foreach (OrderLine orderLine in order.OrderLines)
			{
				orderLines.Add(new ImportedOrderLine(this, orderLine));
			}

			if (orderLines.Count == 0)
			{
				orderLines.Add(new ImportedOrderLine(this, order.Factory.GetNull<OrderLine>())); //Dummy Line as the Order Changed Report groups Order Line Deliveries
			}
		}

		#endregion

		#region Populate

		void Populate(Order order)
		{
			orderState = GetImportedOrderState(order);

			OrderNumberAndSplit = order.JD_OrderNumberAndSplit;

			ImportedPropertyState state = ImportedPropertyState.New;
			if (order.IsInDatabase)
			{
				state = order.JD_OrderNumberInfo.HasChanges || order.JD_OrderNumberSplitInfo.HasChanges ? ImportedPropertyState.Modified : ImportedPropertyState.Unchanged;
			}
			JD_OrderNumberAndSplit = new ImportedProperty(order.JD_OrderNumberAndSplitInfo, Res.GetString("6c7a49b6-94d4-4e6e-aab9-38a64c9da57a", "Order Number"), state);

			Buyer = new ImportedProperty(order.BuyerPKInfo, Res.GetString("b7450893-ab9e-4237-b00a-4b10eaceed2d", "Buyer"), delegate
			{ return order.Buyer == null ? ZString.Empty : order.Buyer.OH_Code; }, order.JD_OA_BuyerAddressInfo.HasChanges ? ImportedPropertyState.Modified : ImportedPropertyState.Unchanged);
			Supplier = new ImportedProperty(order.SupplierPKInfo, Res.GetString("a8d3a5e4-aa38-4b0d-8759-875340fb4046", "Supplier"), delegate
			{ return order.Supplier == null ? ZString.Empty : order.Supplier.OH_Code; }, order.JD_OA_SupplierAddressInfo.HasChanges ? ImportedPropertyState.Modified : ImportedPropertyState.Unchanged);
			ShipmentPreadvice = new ImportedProperty(order.JD_EF_ShipmentPrePlanningInfo, Res.GetString("3aca898c-cee5-4d98-b9fb-64d470728f65", "Pre-advice"), delegate
			{ return order.PreAdvice == null ? ZString.Empty : order.PreAdvice.EF_PreshipID; });
			JD_OrderDate = new ImportedProperty(order.JD_OrderDateInfo, Res.GetString("3371410a-c8c2-48b1-81bb-6ffecdb84e6f", "Order Date"));
			JD_BookingConfRef = new ImportedProperty(order.JD_BookingConfRefInfo, Res.GetString("70fd05e5-94a7-4988-af90-590543b33430", "Confirm #"));
			JD_BookingConfDate = new ImportedProperty(order.JD_BookingConfDateInfo, Res.GetString("2002e6a4-c70c-482e-bc0d-f39933690fcc", "Confirm Date"));
			JD_InvoiceNumber = new ImportedProperty(order.JD_InvoiceNumberInfo, Res.GetString("dc1abced-e4bc-4837-abf9-3adbe2c2b372", "Invoice #"));
			JD_InvoiceDate = new ImportedProperty(order.JD_InvoiceDateInfo, Res.GetString("039ff725-94bd-44ce-99cd-412557f30228", "Invoice Date"));
			JD_OrderStatus = new ImportedProperty(order.JD_OrderStatusInfo, Res.GetString("5148f707-1298-4a93-9e1e-3840634a9302", "Order Status"), delegate
			{ return order.JD_OrderStatus_List.GetDescriptionFromCode(order.JD_OrderStatus); });
			JD_OrderGoodsDescription = new ImportedProperty(order.JD_OrderGoodsDescriptionInfo, Res.GetString("d9489e27-8477-4eee-8c4a-8e6415d52e70", "Description"));
			Currency = new ImportedProperty(order.JD_RX_NKOrderCurrencyInfo, Res.GetString("183982e0-cf4e-452b-a705-44e3e54206dd", "Currency"), delegate
			{ return order.OrderCurrency == null ? ZString.Empty : order.JD_RX_NKOrderCurrency; });
			JD_IncoTerm = new ImportedProperty(order.JD_IncoTermInfo, Res.GetString("a28203bc-5f38-4e8f-4b01-b86e60576bb3", "Incoterms"));
			JD_TransportMode = new ImportedProperty(order.JD_TransportModeInfo, Res.GetString("e6206328-26ad-44b0-ba3b-fda970703126", "Transport"));
			JD_ContainerMode = new ImportedProperty(order.JD_ContainerModeInfo, Res.GetString("33efe0be-adc3-424d-8ebf-a3a91864ea73", "Mode"));
			JD_RN_NKCountryOfSupply = new ImportedProperty(order.JD_RN_NKCountryOfSupplyInfo, Res.GetString("63adfbe4-f822-4da4-ba68-14c7ade03c84", "Country/Region Origin"));
			SendingAgent = new ImportedProperty(order.JD_OH_SendingAgentInfo, Res.GetString("c8e19015-8c41-481f-9533-067c18ed9cbb", "Sending Ag."), delegate
			{ return order.SendingAgent == null ? ZString.Empty : order.SendingAgent.OH_Code; });
			ReceivingAgent = new ImportedProperty(order.JD_OH_ReceivingAgentInfo, Res.GetString("07ad7b19-379d-4084-bf6a-018c86bcd962", "Receiving Ag."), delegate
			{ return order.ReceivingAgent == null ? ZString.Empty : order.ReceivingAgent.OH_Code; });
			JD_RL_NKGoodsAvailableAt = new ImportedProperty(order.JD_RL_NKGoodsAvailableAtInfo, Res.GetString("ebeaf4c9-b1f5-4ceb-9bb1-226d84e975bd", "Origin"));
			JD_RL_NKGoodsDeliveredTo = new ImportedProperty(order.JD_RL_NKGoodsDeliveredToInfo, Res.GetString("50ea6ebf-d0bd-415a-9b69-36db80d24c57", "Destination"));
			JD_RL_NKPortOfLoading = new ImportedProperty(order.JD_RL_NKPortOfLoadingInfo, Res.GetString("5f032fcf-1b13-4420-9104-ce3da08d5bc6", "Load Port"));
			JD_RL_NKPortOfDischarge = new ImportedProperty(order.JD_RL_NKPortOfDischargeInfo, Res.GetString("f7549ee2-10a7-41dc-9d73-b13b70053498", "Disch. Port"));
			JD_Waybill = new ImportedProperty(order.JD_WaybillInfo, Res.GetString("98d0bc07-a59a-4938-ab08-3a767d128f8b", "Waybill"));
			JD_MasterWaybill = new ImportedProperty(order.JD_MasterWaybillInfo, Res.GetString("d28a29b0-46f2-4c8e-8f26-3710dc96d3dd", "Master Waybill"));
			JD_Packs = new ImportedProperty(order.JD_PacksInfo, Res.GetString("cb0c3555-671b-4cd2-b308-914b09d55b6c", "Packs"));
			JD_F3_NKPackType = new ImportedProperty(order.JD_F3_NKPackTypeInfo, Res.GetString("3143e39c-4ba4-42fa-803a-55d1a1fa1017", "Packs Unit"));
			JD_ActualWeight = new ImportedProperty(order.JD_ActualWeightInfo, Res.GetString("7d9f69c0-1288-4a28-922e-5e94d7310a79", "Weight"));
			JD_UnitOfWeight = new ImportedProperty(order.JD_UnitOfWeightInfo, Res.GetString("2430ddf1-510a-412b-84ca-56fe65ee747f", "Weight Unit"));
			JD_ActualVolume = new ImportedProperty(order.JD_ActualVolumeInfo, Res.GetString("403d619a-9d12-4112-9488-5781b2b83fd9", "Volume"));
			JD_UnitOfVolume = new ImportedProperty(order.JD_UnitOfVolumeInfo, Res.GetString("dbbf485c-b349-4723-92a7-e947a63f64a8", "Volume Unit"));
			JD_E_EXW = GetEstimatedMilestoneDateProperty(order, Events.ExWorks, Res.GetString("6d640dbd-6017-4595-b84a-985f704b7ee5", "Est. Ex Factory"));
			JD_A_EXW = GetActualMilestoneDateProperty(order, Events.ExWorks, Res.GetString("89ea5c2f-adc6-4025-88c9-dea6d62f7ee2", "Act. Ex Factory"));
			JD_E_IST = GetEstimatedMilestoneDateProperty(order, Events.DeliveryCartageCompleteFinalised, Res.GetString("db59131e-3c62-426d-96ad-d8622986f5dd", "Est. Delivered"));
			JD_A_IST = GetActualMilestoneDateProperty(order, Events.DeliveryCartageCompleteFinalised, Res.GetString("d05f29fa-629c-45db-9a2a-80596ccb08f3", "Act. Delivered"));
			JD_E_RCV = GetEstimatedMilestoneDateProperty(order, Events.GateIn, Res.GetString("2578b8ef-c31a-4b5d-b037-50eee9206569", "Est. Receival"));
			JD_A_RCV = GetActualMilestoneDateProperty(order, Events.GateIn, Res.GetString("c0ef09d3-8c4b-4362-a681-9912763a9b02", "Act. Receival"));
			JD_E_DEP = GetEstimatedMilestoneDateProperty(order, Events.Departure, Res.GetString("f729ce80-0b29-4603-9508-a084b313add5", "Est. Departure"));
			JD_A_DEP = GetActualMilestoneDateProperty(order, Events.Departure, Res.GetString("48ab9464-ae55-4640-916f-c0c18028c419", "Act. Departure"));
			JD_E_ARV = GetEstimatedMilestoneDateProperty(order, Events.Arrival, Res.GetString("98a6f03d-440b-4040-b9e7-85a1fea9f824", "Est. Arrival"));
			JD_A_ARV = GetActualMilestoneDateProperty(order, Events.Arrival, Res.GetString("53785cca-9788-444d-8370-e9566ad628ca", "Act. Arrival"));
			JD_E_UNP = GetEstimatedMilestoneDateProperty(order, Events.CargoAvailable, Res.GetString("049aeb22-c2af-4b51-a682-b990cb18b457", "Est. Unpacked"));
			JD_A_UNP = GetActualMilestoneDateProperty(order, Events.CargoAvailable, Res.GetString("cb828292-c758-4178-b0be-605496ffa392", "Act. Unpacked"));
			JD_E_PUP = GetEstimatedMilestoneDateProperty(order, Events.DeliveryCartageAdvised, Res.GetString("ed66f0ee-3b19-43a0-beb1-60aed3cc1d3d", "Est. Unpacked"));
			JD_A_PUP = GetActualMilestoneDateProperty(order, Events.DeliveryCartageAdvised, Res.GetString("e5b42bba-4d6d-440b-9f80-836a3888ede0", "Act. Unpacked"));
			JD_DeliveryRequiredBy = new ImportedProperty(order.JD_DeliveryRequiredByInfo, Res.GetString("e01eca33-dd0d-42e5-bb29-17ffa0d0e5f9", "Req. In Store"));

			SetupOrderLines(order);
		}

		#endregion

		#region GetAllImportedProperties / AmendedPropertiesToShowAlways

		public override ImportedProperty[] GetAllImportedProperties()
		{
			return new ImportedProperty[]
			{
				JD_OrderNumberAndSplit,
				ShipmentPreadvice,
				Buyer,
				Supplier,
				JD_OrderDate,
				JD_IncoTerm,
				JD_BookingConfRef,
				JD_BookingConfDate,
				JD_InvoiceNumber,
				JD_InvoiceDate,
				JD_OrderStatus,
				JD_OrderGoodsDescription,
				Currency,
				JD_TransportMode,
				JD_ContainerMode,
				JD_RN_NKCountryOfSupply,
				SendingAgent,
				ReceivingAgent,
				JD_RL_NKGoodsAvailableAt,
				JD_RL_NKGoodsDeliveredTo,
				JD_RL_NKPortOfLoading,
				JD_RL_NKPortOfDischarge,
				JD_Waybill,
				JD_MasterWaybill,
				JD_Packs,
				JD_F3_NKPackType,
				JD_ActualWeight,
				JD_UnitOfWeight,
				JD_ActualVolume,
				JD_UnitOfVolume,
				JD_E_EXW,
				JD_A_EXW,
				JD_E_IST,
				JD_A_IST,
				JD_E_RCV,
				JD_A_RCV,
				JD_E_DEP,
				JD_A_DEP,
				JD_E_ARV,
				JD_A_ARV,
				JD_E_UNP,
				JD_A_UNP,
				JD_E_PUP,
				JD_A_PUP,
				JD_DeliveryRequiredBy,
			};
		}

		public override List<string> PropertiesToShowAlways
		{
			get
			{
				if (propertiesToShowAlways == null)
				{
					propertiesToShowAlways = new List<string>();
					propertiesToShowAlways.Add(JD_OrderNumberAndSplit.Name);
					propertiesToShowAlways.Add(Buyer.Name);
					propertiesToShowAlways.Add(Supplier.Name);
					propertiesToShowAlways.Add(JD_OrderDate.Name);
					propertiesToShowAlways.Add(JD_IncoTerm.Name);
				}
				return propertiesToShowAlways;
			}
		}
		List<string> propertiesToShowAlways;

		public override List<string> OtherPropertiesToShow
		{
			get { return new List<string>(); }
		}

		#endregion

		#region Properties

		public ZString OrderNumberAndSplit { get; private set; }
		public ImportedProperty JD_OrderNumberAndSplit { get; private set; }
		public ImportedProperty Buyer { get; private set; }
		public ImportedProperty Supplier { get; private set; }
		public ImportedProperty ShipmentPreadvice { get; private set; }
		public ImportedProperty JD_OrderDate { get; private set; }
		public ImportedProperty JD_BookingConfRef { get; private set; }
		public ImportedProperty JD_BookingConfDate { get; private set; }
		public ImportedProperty JD_InvoiceNumber { get; private set; }
		public ImportedProperty JD_InvoiceDate { get; private set; }
		public ImportedProperty JD_OrderStatus { get; private set; }
		public ImportedProperty JD_OrderGoodsDescription { get; private set; }
		public ImportedProperty Currency { get; private set; }
		public ImportedProperty JD_IncoTerm { get; private set; }
		public ImportedProperty JD_TransportMode { get; private set; }
		public ImportedProperty JD_ContainerMode { get; private set; }
		public ImportedProperty JD_RN_NKCountryOfSupply { get; private set; }
		public ImportedProperty SendingAgent { get; private set; }
		public ImportedProperty ReceivingAgent { get; private set; }
		public ImportedProperty JD_RL_NKGoodsAvailableAt { get; private set; }
		public ImportedProperty JD_RL_NKGoodsDeliveredTo { get; private set; }
		public ImportedProperty JD_RL_NKPortOfLoading { get; private set; }
		public ImportedProperty JD_RL_NKPortOfDischarge { get; private set; }
		public ImportedProperty JD_Waybill { get; private set; }
		public ImportedProperty JD_MasterWaybill { get; private set; }
		public ImportedProperty JD_Packs { get; private set; }
		public ImportedProperty JD_F3_NKPackType { get; private set; }
		public ImportedProperty JD_ActualWeight { get; private set; }
		public ImportedProperty JD_UnitOfWeight { get; private set; }
		public ImportedProperty JD_ActualVolume { get; private set; }
		public ImportedProperty JD_UnitOfVolume { get; private set; }

		#endregion

		#region Milestone Properties

		public ImportedProperty JD_E_EXW { get; private set; }
		public ImportedProperty JD_A_EXW { get; private set; }
		public ImportedProperty JD_E_IST { get; private set; }
		public ImportedProperty JD_A_IST { get; private set; }
		public ImportedProperty JD_E_RCV { get; private set; }
		public ImportedProperty JD_A_RCV { get; private set; }
		public ImportedProperty JD_E_DEP { get; private set; }
		public ImportedProperty JD_A_DEP { get; private set; }
		public ImportedProperty JD_E_ARV { get; private set; }
		public ImportedProperty JD_A_ARV { get; private set; }
		public ImportedProperty JD_E_UNP { get; private set; }
		public ImportedProperty JD_A_UNP { get; private set; }
		public ImportedProperty JD_E_PUP { get; private set; }
		public ImportedProperty JD_A_PUP { get; private set; }
		public ImportedProperty JD_DeliveryRequiredBy { get; private set; }

		ImportedProperty GetEstimatedMilestoneDateProperty(Order order, Event eventType, string caption)
		{
			ProcessTask task = order.WorkflowItems.Milestones[eventType];
			if (task != null)
			{
				return new ImportedProperty(task.P9_ScheduledDateInfo, "E_" + eventType.Code, caption);
			}
			else
			{
				return new ImportedProperty(MilestoneDateInfo, "E_" + eventType.Code, caption, ImportedPropertyState.Unchanged);
			}
		}

		ImportedProperty GetActualMilestoneDateProperty(Order order, Event eventType, string caption)
		{
			ProcessTask task = order.WorkflowItems.Milestones[eventType];
			if (task != null)
			{
				return new ImportedProperty(task.P9_ActualDateInfo, "A_" + eventType.Code, caption);
			}
			else
			{
				return new ImportedProperty(MilestoneDateInfo, "A_" + eventType.Code, caption, ImportedPropertyState.Unchanged);
			}
		}

		public ZDateTime MilestoneDate
		{
			get { return ZDateTime.Empty; }
		}

		public ZPropertyInfo MilestoneDateInfo
		{
			get { return GetZPropertyInfo(nameof(MilestoneDate)); }
		}

		#endregion

		static ImportedOrderState GetImportedOrderState(Order importedOrder)
		{
			ImportedOrderState result = ImportedOrderState.Unchanged;

			if (!importedOrder.IsInDatabase)
			{
				result = ImportedOrderState.New;
			}
			else if (!importedOrder.HasChanges)
			{
				result = ImportedOrderState.Unchanged;
			}
			else if (
				importedOrder.IsInDatabase &&
				importedOrder.JD_OrderStatusInfo.HasChanges &&
				importedOrder.JD_OrderStatus == Core.Constants.OrderStatus.Cancelled)
			{
				result = ImportedOrderState.Cancelled;
			}
			else
			{
				result = ImportedOrderState.Amended;
			}

			if ((ZBool)importedOrder.JD_IsCancelledInfo.OriginalValue && !importedOrder.JD_IsCancelled)
			{
				result = ImportedOrderState.Reactivated;
			}

			if (!importedOrder.CanBeUpdatedByImport)
			{
				result = ImportedOrderState.Attached;
			}

			return result;
		}

		#region ModifiedOrderLineProperties

		internal List<string> ModifiedOrderLinePropertiesToShow
		{
			get
			{
				if (modifiedOrderLinePropertiesToShow == null)
				{
					modifiedOrderLinePropertiesToShow = new List<string>();
					foreach (ImportedOrderLine orderLine in OrderLines)
					{
						foreach (string propertyName in orderLine.ModifiedProperties)
						{
							if (!modifiedOrderLinePropertiesToShow.Contains(propertyName))
							{
								modifiedOrderLinePropertiesToShow.Add(propertyName);
							}
						}
					}
				}
				return modifiedOrderLinePropertiesToShow;
			}
		}
		List<string> modifiedOrderLinePropertiesToShow;

		#endregion
	}
}
