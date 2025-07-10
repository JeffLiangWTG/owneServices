using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class LocalProcessingDataObjectWriter : DataObjectWriter<JobDocsAndCartage, LocalProcessing>
	{
		public LocalProcessingDataObjectWriter(IDataWritingManager manager) : base(manager) { }

		protected override LocalProcessing PopulateDataObject(JobDocsAndCartage docsBO)
		{
			if (docsBO == null)
			{
				return null;
			}

			var processingData = new LocalProcessing(writeManager.WriterStrategy);
			processingData.FCLPickupEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_FCLPickupEquipmentNeeded, docsBO.Lookups.PickupEquipmentNeededList);
			processingData.EstimatedPickup = docsBO.JP_EstimatedPickup;
			processingData.PickupRequiredBy = docsBO.JP_PickupRequiredBy;
			processingData.PickupRequiredFrom = docsBO.JP_PickupRequiredFrom;
			processingData.PickupCartageAdvised = docsBO.JP_PickupCartageAdvised;
			processingData.ArrivalCartageRef = docsBO.JP_ArrivalCartageRef;
			processingData.PickupCartageCompleted = docsBO.JP_PickupCartageCompleted;
			processingData.PickupLabourTime = docsBO.JP_PickupLabourTime;
			processingData.PickupLabourCharge = docsBO.JP_PickupLabourCharge;
			processingData.DemurrageOnPickupTime = docsBO.JP_PickupTruckWaitTime;
			processingData.PickupTruckWaitTime = docsBO.JP_PickupTruckWaitTime;
			processingData.DemurrageOnPickupCharge = docsBO.JP_PickupTruckWaitCharge;
			processingData.PickupTruckWaitCharge = docsBO.JP_PickupTruckWaitCharge;
			processingData.PrintOptionForPackagesOnAWB = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_PrintOptionForPackagesOnAWB, new CodeDescriptionPairList(OLookUpEditType.AWBDimensions));
			processingData.FCLDeliveryEquipmentNeeded = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_FCLDeliveryEquipmentNeeded, docsBO.Lookups.DeliveryEquipmentNeededList);
			processingData.FCLAvailable = docsBO.JP_FCLAvailable;
			processingData.FCLStorageCommences = docsBO.JP_FCLStorageCommences;
			processingData.LCLAvailable = docsBO.JP_LCLAvailable;
			processingData.LCLStorageCommences = docsBO.JP_LCLStorageCommences;
			processingData.LCLAirStorageDaysOrHours = docsBO.JP_LCLAirStorageDaysOrHours;
			processingData.LCLAirStorageCharge = docsBO.JP_LCLAirStorageCharge;
			processingData.EstimatedDelivery = docsBO.JP_EstimatedDelivery;
			processingData.DeliveryRequiredBy = docsBO.JP_DeliveryRequiredBy;
			processingData.DeliveryRequiredFrom = docsBO.JP_DeliveryRequiredFrom;
			processingData.DeliveryCartageAdvised = docsBO.JP_DeliveryCartageAdvised;
			processingData.DeliveryCartageCompleted = docsBO.JP_DeliveryCartageCompleted;
			processingData.DeliveryLabourTime = docsBO.JP_DeliveryLabourTime;
			processingData.DeliveryLabourCharge = docsBO.JP_DeliveryLabourCharge;
			processingData.DemurrageOnDeliveryTime = docsBO.JP_DeliveryTruckWaitTime;
			processingData.DeliveryTruckWaitTime = docsBO.JP_DeliveryTruckWaitTime;
			processingData.DemurrageOnDeliveryCharge = docsBO.JP_DeliveryTruckWaitCharge;
			processingData.DeliveryTruckWaitCharge = docsBO.JP_DeliveryTruckWaitCharge;
			processingData.HasProhibitedPackaging = docsBO.JP_HasProhibitedPackaging;
			processingData.InsuranceRequired = docsBO.JP_InsuranceRequired;
			processingData.IsContingencyRelease = docsBO.JP_IsContingencyRelease;
			processingData.LCLDatesOverrideConsol = docsBO.JP_LCLDatesOverrideConsol;
			processingData.FCLPickupDetentionFreeDays = docsBO.JP_FCLPickupDetentionFreeDays;
			processingData.FCLPickupDetentionDays = docsBO.JP_FCLPickupDetentionDays;
			processingData.FCLPickupDetentionCharge = docsBO.JP_FCLPickupDetentionCharge;
			processingData.FCLDeliveryDetentionFreeDays = docsBO.JP_FCLDeliveryDetentionFreeDays;
			processingData.FCLDeliveryDetentionDays = docsBO.JP_FCLDeliveryDetentionDays;
			processingData.FCLDeliveryDetentionCharge = docsBO.JP_FCLDeliveryDetentionCharge;
			processingData.ExportStatement = ListHelper.GetWithDescription<CodeDescriptionPair>(docsBO.JP_ExportStatement, docsBO.Lookups.JP_ExportStatementList);

			WriteOrderNumberCollection(docsBO, processingData);

			processingData.SetAdditionalServiceCollection(() => ProcessCollection(docsBO.Services, new AdditionalServiceDataObjectWriter(writeManager), CollectionContent.Complete));

			return processingData;
		}

		void WriteOrderNumberCollection(JobDocsAndCartage docsBO, LocalProcessing processingData)
		{
			var orderItems = docsBO.OrderItems;
			var orderItemsCollection = new List<OrderNumber>();
			var attachedOrdersCollection = new List<OrderNumber>();
			var sequenceNo = (ZShort)1;

			if (orderItems.Count > 0)
			{
				orderItemsCollection = ProcessCollection(orderItems, new OrderNumberDataObjectWriter(writeManager));
				sequenceNo = orderItemsCollection.Max(o => o.Sequence).Value + 1;
			}

			var parent = docsBO.Parent;

			if (parent != null)
			{
				var iAttachOrders = parent as IAttachOrders;

				if (iAttachOrders != null)
				{
					var attachOrders = iAttachOrders.AttachedOrders;

					if (attachOrders.Count > 0)
					{
						var writer = new LinkedOrder_OrderNumberDataObjectWriter(writeManager);
						writer.SequenceNo = sequenceNo;

						attachedOrdersCollection = ProcessCollection(attachOrders, writer);
					}
				}
			}

			if (orderItemsCollection.Any() || attachedOrdersCollection.Any())
			{
				processingData.SetOrderNumberCollection(() =>
				{
					var list = new DataObjectList<OrderNumber>();
					list.AddRange(orderItemsCollection);
					list.AddRange(attachedOrdersCollection);
					return list;
				});
			}
		}
	}
}
