using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;
using OrgCusCode = Enterprise.MasterFiles.Business.OrgCusCode;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public static class WarehouseMatchingHelper
	{
		#region GetWarehouse

		public static IColumnIndexer GetWarehouse(UniversalShipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			IColumnIndexer warehouse = null;
			var hasAddressCollections = false;

			if (sourceDO.OrganizationAddressCollection != null)
			{
				hasAddressCollections = true;
				warehouse = GetTransitWarehouseFromOrgAddress(sourceDO, factory, logger);
			}

			if (warehouse == null && sourceDO.AdditionalReferenceCollection != null)
			{
				hasAddressCollections = true;
				warehouse = GetTransitWarehouseFromPremise(sourceDO, factory, logger);
			}

			if (!hasAddressCollections)
			{
				logger.Log(LogType.Warning, Res.GetString("d6e96ca5-139b-4abb-8895-98dac5a719fc", "Could not find Warehouse. No organization address or premise information is found in UXML."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromOrgAddress(UniversalShipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			IColumnIndexer warehouse = null;
			var hasAnyWarehouseAddress = false;
			var topLevelDO = TransitUniversalExtensions.GetTopLevelDataObject(logger);

			if (topLevelDO.IsDepartureTransitWarehouse() && TryGetDepartureCFSAddressDataObject(sourceDO, out var departureCFSAddressDataObject))
			{
				hasAnyWarehouseAddress = true;
				warehouse = GetTransitWarehouseFromDepartureCFSAddress(departureCFSAddressDataObject, factory, logger);
			}
			else if (topLevelDO.IsArrivalTransitWarehouse() && TryGetArrivalCFSAddressDataObject(sourceDO, out var arrivalCFSAddressDataObject))
			{
				hasAnyWarehouseAddress = true;
				warehouse = GetTransitWarehouseFromArrivalCFSAddress(arrivalCFSAddressDataObject, factory, logger);
			}

			if (warehouse == null && TryGetLocalCartageCFSAddressDataObject(sourceDO, out var cfsAddressDataObject))
			{
				hasAnyWarehouseAddress = true;
				warehouse = GetTransitWarehouseFromLocalCartageCFSAddress(cfsAddressDataObject, factory, logger);
			}

			if (!hasAnyWarehouseAddress)
			{
				logger.Log(LogType.Warning, Res.GetString("6bde1040-7a41-4176-ad24-3cce3c655e21", "Could not find Warehouse. UXML does not have a valid corresponding address."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromDepartureCFSAddress(OrganizationAddress departureCFSAddressDataObject, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(departureCFSAddressDataObject.OrganizationCode?.ToString(), departureCFSAddressDataObject.Address1);
			logger.Log(LogType.Information, Res.GetString("a231d7ac-d2a9-48dc-81ef-3031fbfa86d1",
				"Searching for Transit Warehouse matching Departure CFS Address {0}.", addressString));
			var warehouse = GetTransitWarehouse(factory, departureCFSAddressDataObject, logger);

			if (warehouse != null)
			{
				logger.Log(LogType.Information, Res.GetString("77de4236-2340-4539-9610-f9b3781420cb", "Found Warehouse {0} from Departure CFS Address.", warehouse[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("87cf83af-227f-49ad-b66f-dfbc9c338a03", "Could not find Warehouse from Departure CFS Address."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromArrivalCFSAddress(OrganizationAddress arrivalCFSAddressDataObject, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(arrivalCFSAddressDataObject.OrganizationCode?.ToString(), arrivalCFSAddressDataObject.Address1);
			logger.Log(LogType.Information, Res.GetString("0abaa0cb-c99c-4f19-8821-b07edc238da0",
				"Searching for Transit Warehouse matching Arrival CFS Address {0}.", addressString));
			var warehouse = GetTransitWarehouse(factory, arrivalCFSAddressDataObject, logger);

			if (warehouse != null)
			{
				logger.Log(LogType.Information, Res.GetString("db69a668-73dc-4364-9d60-806e7c081338", "Found Warehouse {0} from Arrival CFS Address.", warehouse[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("ea23cf94-5530-4aa4-aa7f-a964ae11dca7", "Could not find Warehouse from Arrival CFS Address."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromLocalCartageCFSAddress(OrganizationAddress cfsAddressDataObject, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(cfsAddressDataObject.OrganizationCode?.ToString(), cfsAddressDataObject.Address1);
			logger.Log(LogType.Information, Res.GetString("b30faf30-605b-4582-b6f8-61f05887b6d1",
				"Searching for Transit Warehouse matching Local Cartage CFS Address {0}.", addressString));
			var warehouse = GetTransitWarehouse(factory, cfsAddressDataObject, logger);

			if (warehouse != null)
			{
				logger.Log(LogType.Information, Res.GetString("15320723-1193-4d83-a46d-0b7827e8b789", "Found Warehouse {0} from Local Cartage CFS Address.", warehouse[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("7e8c0b10-0225-4cb9-8d05-0d0560942ed5", "Could not find Warehouse from Local Cartage CFS Address."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromPremise(UniversalShipment sourceDO, UniversalObjectFactory factory, IXmlImportLogger logger)
		{
			IColumnIndexer warehouse = null;

			var topLevelDO = TransitUniversalExtensions.GetTopLevelDataObject(logger);
			var typeCode = "";
			if (topLevelDO.IsArrivalTransitWarehouse())
			{
				typeCode = WarehouseAdditionalReferenceTypes.Codes.DestinationPremiseID;
			}
			else if (topLevelDO.IsDepartureTransitWarehouse())
			{
				typeCode = WarehouseAdditionalReferenceTypes.Codes.OriginPremiseID;
			}

			if (!typeCode.IsNullOrEmpty())
			{
				var additionalRef = sourceDO.AdditionalReferenceCollection.FirstOrDefault(a => a.Type.Code.Equals(typeCode));

				if (additionalRef != null && additionalRef.ReferenceNumber.HasValue)
				{
					var referenceNo = additionalRef.ReferenceNumber;

					if (referenceNo.HasValue && !referenceNo.Value.IsEmpty)
					{
						warehouse = GetTransitWarehouseFromPremise(factory, logger, referenceNo.Value);
					}
					else
					{
						var message = topLevelDO.IsArrivalTransitWarehouse()
							? Res.GetString("eafb6b39-6409-4c23-aa82-f6ab7fb7c881", "Could not find Warehouse. Reference Number is empty in Destination Premise.")
							: Res.GetString("2ee745f8-97c4-4d98-914a-9bf68792c8c2", "Could not find Warehouse. Reference Number is empty in Origin Premise.");
						logger.Log(LogType.Warning, message);
					}
				}
				else
				{
					var message = topLevelDO.IsArrivalTransitWarehouse()
						? Res.GetString("1b7a1342-3c9b-462e-bb65-9b19cfd493a8", "Could not find Warehouse. Destination Premise is not found in UXML.")
						: Res.GetString("b02156ea-aa72-4ed1-ac41-2ab7971f3d95", "Could not find Warehouse. Origin Premise is not found in UXML.");
					logger.Log(LogType.Warning, message);
				}
			}
			else
			{
				logger.Log(LogType.Warning, Res.GetString("bebd8d65-af0f-410b-bd51-0012f4ab995b", "Could not find Warehouse. Role type in the UXML is incorrect for Departure Transit Warehouse and Arrival Transit Warehouse."));
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouseFromPremise(UniversalObjectFactory factory, IXmlImportLogger logger, string referenceNo)
		{
			var topLevelDO = TransitUniversalExtensions.GetTopLevelDataObject(logger);

			var orgCusCodeQuery = new ZQuery();
			orgCusCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, referenceNo);
			orgCusCodeQuery.MaximumRows = 2;
			var orgCusCodes = factory.Load<OrgCusCode>(orgCusCodeQuery);

			IColumnIndexer warehouse = null;
			if (orgCusCodes.Length > 1)
			{
				var multipleTransitWarehousesErrorMessage =
					Res.GetString("d9d92baa-87f6-4fc1-867e-93fa7b0905a0", "Multiple Premise addresses were found for premise id '{0}'.", referenceNo);
				throw new DataObjectReadFailureException(multipleTransitWarehousesErrorMessage);
			}
			else
			{
				var orgCusCode = orgCusCodes.Where(c => c.OK_RN_NKCodeCountry == CountryCodes.Australia).SingleOrDefault();
				var orgAddress = orgCusCode?.PremisesAddress;

				warehouse = orgAddress != null ? GetTransitWarehouse(factory, orgAddress) : null;
				if (warehouse != null)
				{
					var message = topLevelDO.IsArrivalTransitWarehouse()
						? Res.GetString("1eeeb43f-2dad-43ec-91f4-0260273d2bea", "Found Warehouse {0} from Destination Premise (Reference Number - '{1}').", warehouse[WhsWarehouseSchema.Constants.WW_WarehouseCode], referenceNo)
						: Res.GetString("9033ef57-b8b0-4f8b-8a4d-fad12740f2f5", "Found Warehouse {0} from Origin Premise (Reference Number - '{1}').", warehouse[WhsWarehouseSchema.Constants.WW_WarehouseCode], referenceNo);
					logger.Log(LogType.Information, message);
				}
				else
				{
					var message = topLevelDO.IsArrivalTransitWarehouse()
						? Res.GetString("51468cd3-aa19-4d7c-8f12-0e4987a4fa9d", "Could not find Warehouse. No matching Warehouse found from Destination Premise (Reference Number - '{0}').", referenceNo)
						: Res.GetString("a0f5a234-a9e5-41e6-9901-86095e81dbe1", "Could not find Warehouse. No matching Warehouse found from Origin Premise (Reference Number - '{0}').", referenceNo);
					logger.Log(LogType.Warning, message);
				}
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouse(UniversalObjectFactory factory, OrganizationAddress cfsAddressDataObject, IXmlImportLogger logger)
		{
			IColumnIndexer warehouse = null;
			var cfsOrgAddress = new OrganisationDataObjectReader(cfsAddressDataObject, logger, factory).GetMatched();
			if (cfsOrgAddress != null)
			{
				return GetTransitWarehouse(factory, cfsOrgAddress);
			}

			return warehouse;
		}

		static IColumnIndexer GetTransitWarehouse(UniversalObjectFactory factory, OrgAddress cfsOrgAddress)
		{
			var warehouseQuery = GetWarehouseQuery(cfsOrgAddress);

			var activeWarehouses = factory.RowFactory.Load(WhsWarehouseSchema.Constants.TableName, warehouseQuery);
			if (activeWarehouses.Length > 1)
			{
				var warehousesWithSameOrgAddress = string.Join(", ", activeWarehouses.Select(w => w[WhsWarehouseSchema.Constants.WW_WarehouseCode]));
				var multipleTransitWarehousesErrorMessage =
					Res.GetString("a5f4e4a4-f43b-4c62-9f42-02e23708392a",
@"Multiple Active Transit Warehouses {0} matching the Address '{1} - {2}' were found.
Ensure only one Active Transit Warehouse exists matching this Address.", warehousesWithSameOrgAddress, cfsOrgAddress.Header.OH_Code, cfsOrgAddress.Address1);
				throw new DataObjectReadFailureException(multipleTransitWarehousesErrorMessage);
			}

			return DataObjectReader.GetColumnIndexerFromRow(activeWarehouses.SingleOrDefault());
		}

		static ZQuery GetWarehouseQuery(OrgAddress cfsOrgAddress)
		{
			var warehouseQuery = new ZQuery();
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_OA_WarehouseAddress, cfsOrgAddress.PK);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.Transit);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
			warehouseQuery.MaximumRows = 2;
			return warehouseQuery;
		}

		#endregion

		#region ThrowForNoMatchingWarehouse

		public static void ThrowForNoMatchingWarehouse(UniversalShipment sourceDO, IXmlImportLogger logger)
		{
			var topLevelDO = TransitUniversalExtensions.GetTopLevelDataObject(logger);

			if (sourceDO.OrganizationAddressCollection != null)
			{
				if (topLevelDO.IsDepartureTransitWarehouse() && TryGetDepartureCFSAddressDataObject(sourceDO, out var departureCFSAddressDataObject))
				{
					var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(departureCFSAddressDataObject.OrganizationCode?.ToString(), departureCFSAddressDataObject.Address1);
					throw new DataObjectReadFailureException(Res.GetString("1e2ef39c-9356-4297-ad86-ea154ac2056e",
						"Cannot import without a valid Warehouse supplied. Please ensure your Departure CFS Address {0} has an active Transit Warehouse.", addressString));
				}
				else if (topLevelDO.IsArrivalTransitWarehouse() && TryGetArrivalCFSAddressDataObject(sourceDO, out var arrivalCFSAddressDataObject))
				{
					var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(arrivalCFSAddressDataObject.OrganizationCode?.ToString(), arrivalCFSAddressDataObject.Address1);
					throw new DataObjectReadFailureException(Res.GetString("1bc15393-d39f-4cc1-b384-46622c1bfc8c",
						"Cannot import without a valid Warehouse supplied. Please ensure your Arrival CFS Address {0} has an active Transit Warehouse.", addressString));
				}

				if (TryGetLocalCartageCFSAddressDataObject(sourceDO, out var cfsAddressDataObject))
				{
					var addressString = WhsTransitLogHelper.FormatCodeDescriptionForLogging(cfsAddressDataObject.OrganizationCode?.ToString(), cfsAddressDataObject.Address1);
					throw new DataObjectReadFailureException(Res.GetString("a16badb2-5467-4f8d-9de4-8c1a88507ace",
						"Cannot import without a valid Warehouse supplied. Please ensure your CFS Address {0} has an active Transit Warehouse.", addressString));
				}
			}

			throw new DataObjectReadFailureException(Res.GetString("3924d44d-1818-4606-bf97-cfe80bdf717d",
				"Cannot import without a valid Warehouse supplied. Please ensure there is a valid departure/arrival CFS address and that you targeted the correct recipient type."));
		}

		#endregion

		#region Addresses

		static bool TryGetDepartureCFSAddressDataObject(UniversalShipment sourceDO, out OrganizationAddress cfsAddress)
		{
			cfsAddress = sourceDO.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress));
			return cfsAddress != null;
		}

		static bool TryGetArrivalCFSAddressDataObject(UniversalShipment sourceDO, out OrganizationAddress cfsAddress)
		{
			cfsAddress = sourceDO.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
			return cfsAddress != null;
		}

		static bool TryGetLocalCartageCFSAddressDataObject(UniversalShipment sourceDO, out OrganizationAddress cfsAddress)
		{
			cfsAddress = sourceDO.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.LocalCartageCFS));
			return cfsAddress != null;
		}

		#endregion
	}
}
