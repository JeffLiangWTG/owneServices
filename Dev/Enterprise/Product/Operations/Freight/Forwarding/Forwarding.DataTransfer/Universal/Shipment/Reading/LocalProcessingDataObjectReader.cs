using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.DataTransfer.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class LocalProcessingDataObjectReader : DataObjectReader<LocalProcessing>
	{
		public LocalProcessingDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject.LocalProcessing, logger, factory)
		{
			ParentShipmentDataObject = dataObject;
		}

		readonly TopLevelDataObject ParentShipmentDataObject;

		public void PopulateBusinessObject(JobDocsAndCartage jobDocsAndCartage, Dictionary<string, ValueSetter> delaySetters = null)
		{
			var jobDocsAndCartageRow = GetColumnIndexer(jobDocsAndCartage);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLPickupEquipmentNeeded, dataObject.FCLPickupEquipmentNeeded, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_EstimatedPickup, dataObject.EstimatedPickup, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupCartageAdvised, dataObject.PickupCartageAdvised, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_ArrivalCartageRef, dataObject.ArrivalCartageRef, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupCartageCompleted, dataObject.PickupCartageCompleted, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupLabourTime, dataObject.PickupLabourTime, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupLabourCharge, dataObject.PickupLabourCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupTruckWaitTime, dataObject.PickupTruckWaitTime ?? dataObject.DemurrageOnPickupTime, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupTruckWaitCharge, dataObject.PickupTruckWaitCharge ?? dataObject.DemurrageOnPickupCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PrintOptionForPackagesOnAWB, dataObject.PrintOptionForPackagesOnAWB, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLDeliveryEquipmentNeeded, dataObject.FCLDeliveryEquipmentNeeded, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLAvailable, dataObject.FCLAvailable, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLStorageCommences, dataObject.FCLStorageCommences, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_LCLAvailable, dataObject.LCLAvailable, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_LCLStorageCommences, dataObject.LCLStorageCommences, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_LCLAirStorageDaysOrHours, dataObject.LCLAirStorageDaysOrHours, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_LCLAirStorageCharge, dataObject.LCLAirStorageCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_EstimatedDelivery, dataObject.EstimatedDelivery, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryCartageAdvised, dataObject.DeliveryCartageAdvised, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryCartageCompleted, dataObject.DeliveryCartageCompleted, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryLabourTime, dataObject.DeliveryLabourTime, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryLabourCharge, dataObject.DeliveryLabourCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryTruckWaitTime, dataObject.DeliveryTruckWaitTime ?? dataObject.DemurrageOnDeliveryTime, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryTruckWaitCharge, dataObject.DeliveryTruckWaitCharge ?? dataObject.DemurrageOnDeliveryCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_HasProhibitedPackaging, dataObject.HasProhibitedPackaging, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_InsuranceRequired, dataObject.InsuranceRequired, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_IsContingencyRelease, dataObject.IsContingencyRelease, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_LCLDatesOverrideConsol, dataObject.LCLDatesOverrideConsol, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_ExportStatement, dataObject.ExportStatement, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupRequiredFrom, dataObject.PickupRequiredFrom, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryRequiredFrom, dataObject.DeliveryRequiredFrom, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLPickupDetentionFreeDays, dataObject.FCLPickupDetentionFreeDays, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLPickupDetentionDays, dataObject.FCLPickupDetentionDays, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLPickupDetentionCharge, dataObject.FCLPickupDetentionCharge, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLDeliveryDetentionFreeDays, dataObject.FCLDeliveryDetentionFreeDays, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLDeliveryDetentionDays, dataObject.FCLDeliveryDetentionDays, delaySetters);
			SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_FCLDeliveryDetentionCharge, dataObject.FCLDeliveryDetentionCharge, delaySetters);

			if (IsDataSourceWarehouse)
			{
				SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupRequiredBy, dataObject.DeliveryRequiredBy, delaySetters);
			}
			else
			{
				SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_PickupRequiredBy, dataObject.PickupRequiredBy, delaySetters);
				SetValue(jobDocsAndCartageRow, JobDocsAndCartageSchema.JP_DeliveryRequiredBy, dataObject.DeliveryRequiredBy, delaySetters);
			}

			if (dataObject.OrderNumberCollection != null)
			{
				if (!jobDocsAndCartage.JP_OrderItemsAsStringInfo.ReadOnly)
				{
					var orderItemCollectionReader = new OrderNumberCollectionReader(dataObject.OrderNumberCollection, jobDocsAndCartage, logger, factory);
					orderItemCollectionReader.ReadIntoCollection();
					jobDocsAndCartage.OrderItems.Sort(JobOrderItemSchema.JT_OrderReference.Name);
				}
			}

			if (dataObject.AdditionalServiceCollection != null)
			{
				if (jobDocsAndCartage.Parent is ForwardingShipment || jobDocsAndCartage.Parent is IBaseJobDeclaration)
				{
					var additionalservicesCollectionReader = new AdditionalServiceDataObjectCollectionReader(dataObject.AdditionalServiceCollection, logger, factory, jobDocsAndCartage);
					additionalservicesCollectionReader.ReadIntoCollection();
				}
				else
				{
					UpdateDataObjectToJobDocsServices(dataObject.AdditionalServiceCollection, jobDocsAndCartage);
				}
			}
		}

		void UpdateDataObjectToJobDocsServices(DataObjectList<AdditionalService> additionalServiceCollection, JobDocsAndCartage jobDocsAndCartage)
		{
			var existingServiceItems = jobDocsAndCartage.Services.Cast<JobService>().ToHashSet();
			foreach (var additionalServiceDataObject in additionalServiceCollection)
			{
				var serviceData = new AdditionalServiceDataObjectReader(additionalServiceDataObject, logger, factory, jobDocsAndCartage).ReadIntoBusinessObject();
				if (!existingServiceItems.Remove(serviceData))
				{
					jobDocsAndCartage.Services.Add(serviceData);
				}
			}
			existingServiceItems.DeleteAll();
		}

		bool IsDataSourceWarehouse
		{
			get { return ParentShipmentDataObject.GetMatchingDataSource(DataContextType.WarehouseOrder) != null; }
		}
	}
}
