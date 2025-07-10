using System;
using System.Collections;
using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	class GteFacilityJobIntegrationHelper
	{
		internal void CreateAndLinkFacilityJobs(ITopLevelDataObject dataObject, IXmlImportLogger logger, UniversalObjectFactory universalObjectFactory, GteBooking booking)
		{
			if (booking.Facility.WW_WarehouseType == new ZString("CYD") &&
				WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndContainerYard.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				logger.Log(LogType.Information, Res.GetString("62de586a-d9be-4436-9e7f-78e518d76fc7", "Linking of jobs skipped due to registry settings (Warehouse > Gate Management > Enable universal integration between gate and container yard)"));
				return;
			}
			else if (booking.Facility.WW_WarehouseType == new ZString("TRW") &&
						WarehouseDataRegistry.Instance.EnableUniversalIntegrationBetweenGateAndTransitWarehouse.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				logger.Log(LogType.Information, Res.GetString("609018de-bd04-44f3-8e4f-7e6aca99f39a", "Linking of jobs skipped due to registry settings (Warehouse > Gate Management > Enable universal integration between gate and transit warehouse)"));
				return;
			}

			logger.Log(LogType.Information, Res.GetString("829bfa3f-2ca7-8a8a-4bcc-fc8ada2fa45e", "Try link matching facility jobs"));
			var facilityTypeOrServiceCode = GetFacilityTypeOrServiceCode(dataObject, logger);

			if (string.IsNullOrEmpty(facilityTypeOrServiceCode))
			{
				return;
			}

			var facilityJobManager = GetFacilityJobManager(facilityTypeOrServiceCode, logger);
			if (facilityJobManager == null)
			{
				return;
			}

			var originalDataTargets = dataObject.DataContext.DataTargetCollection.ToList();
			var addedDataTarget = AddRequiredDataTarget(dataObject, facilityJobManager);
			AddRequiredDataSources(dataObject);

			logger.Log(LogType.Information, Res.GetString("c56d69b2-6cf6-47e9-bef6-dc30ce712b41", "Begin creation of facility jobs"));

			facilityJobManager.UseIncomingShipmentData(dataObject, logger, universalObjectFactory);

			logger.Log(LogType.Information, Res.GetString("c56d69b2-6cf6-47e9-bef6-dc30ce712b42", "End creation of facility jobs"));

			logger.Log(LogType.Information, Res.GetString("c56d69b2-6cf6-47e9-bef6-dc30ce712b43", "Begin linking of facility jobs"));

			foreach (var gateMovementBooking in booking.GateMovementBookings.Cast<GteGateMovementBooking>())
			{
				var facilityEntity = facilityJobManager.GetLinkedEntity(universalObjectFactory, gateMovementBooking);

				if (facilityEntity != null)
				{
					gateMovementBooking.GBM_FacilityJobId = facilityEntity.PK;
					gateMovementBooking.GBM_FacilityTableCode = facilityEntity.TablePrefix;
				}
				else
				{
					logger.Log(LogType.Information, Res.GetString(
						"52db7820-6c51-f3be-4fd7-e1ad40742a5f",
						"Unable to match booking '{0}' to valid facility job",
						gateMovementBooking.GBM_BookingReferenceNumber));
				}
			}

			logger.Log(LogType.Information, Res.GetString("c56d69b2-6cf6-47e9-bef6-dc30ce712b44", "End linking of facility jobs"));

			if (addedDataTarget)
			{
				RemoveAddedDataTargets(dataObject, originalDataTargets);
			}
		}

		string GetFacilityTypeOrServiceCode(ITopLevelDataObject dataObject, IXmlImportLogger logger)
		{
			var recipientRole = dataObject.DataContext?.RecipientRoleCollection?.FirstOrDefault();
			var facilityTypeCode = recipientRole?.Code;
			var serviceCode = string.Empty;

			if (facilityTypeCode == null)
			{
				logger.Log(LogType.Information, Res.GetString("be67a1bb-8352-0cab-4596-54a9cc694b9d", "Imported UXML does not contain a Facility Type"));
				return null;
			}

			if (facilityTypeCode == RecipientRoleType.ATW || facilityTypeCode == RecipientRoleType.DTW)
			{
				serviceCode = recipientRole.ServiceCode?.ToString();
				return serviceCode;
			}

			return facilityTypeCode?.ToString();
		}

		static bool AddRequiredDataTarget(ITopLevelDataObject dataObject, IGateManagementFacilityDataContextManager facilityJobManager)
		{
			if (dataObject.DataContext.GetMatchingDataTarget(facilityJobManager.DataContextType) != null)
			{
				return false;
			}

			dataObject.DataContext.AddDataTarget(facilityJobManager.DataContextType, null);
			return true;
		}

		void RemoveAddedDataTargets(ITopLevelDataObject dataObject, IEnumerable originalDataTargets)
		{
			dataObject.DataContext.ClearDataTargetCollection();
			foreach (IDataTargetDataObject target in originalDataTargets)
			{
				dataObject.DataContext.AddDataTarget(target);
			}
		}

		void AddRequiredDataSources(ITopLevelDataObject dataObject)
		{
			var shipment = (Shipment)dataObject;

			AddGateDataSourceToShipment(shipment, DataContextType.GateBooking, string.Empty);

			foreach (var subShipment in shipment.SubShipmentCollection)
			{
				var bookingPartySourceRefNumber = subShipment.AdditionalReferenceCollection.FirstOrDefault(x => (x.Type.Code ?? string.Empty) == AdditionalReferenceTypes.Codes.BookingPartyReference)?.ReferenceNumber ?? string.Empty;
				AddGateDataSourceToShipment(subShipment, DataContextType.GateBooking, bookingPartySourceRefNumber); // This data source is for the container yard, in CYDTransportationUnitDataContextManager
				AddGateDataSourceToShipment(subShipment, DataContextType.GateMovementBooking, bookingPartySourceRefNumber); // This data source is for transit warehouse.
			}
		}

		void AddGateDataSourceToShipment(Shipment shipment, DataContextType type, string key)
		{
			if (shipment.DataContext == null)
			{
				shipment.DataContext = DataContextFactory.New();
			}

			if (shipment.DataContext.GetMatchingDataSource(type) == null)
			{
				shipment.DataContext.AddDataSource(type, key);
			}
		}

		IGateManagementFacilityDataContextManager GetFacilityJobManager(string type, IXmlImportLogger logger)
		{
			var providers = (Hashtable)ObjectFactory.Get("GateManagementFacilityManagerList");
			var objectHandle = (ObjectHandle)providers[type];
			var facilityEntityManager = objectHandle?.GetObject() as IGateManagementFacilityDataContextManager;

			if (facilityEntityManager == null)
			{
				logger.Log(LogType.Information, Res.GetString(
					"5f8533b7-6d07-acb0-47c2-4f1fc074700f",
					"No Facility Job Manager defined for Facility type '{0}'",
					type));
			}

			return facilityEntityManager;
		}
	}
}
