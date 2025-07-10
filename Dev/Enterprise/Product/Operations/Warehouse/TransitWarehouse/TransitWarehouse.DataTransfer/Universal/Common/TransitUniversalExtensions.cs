using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transit.Business;
using static Enterprise.Integration.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	/*
	 * Universal XML 2011
	 * Consols are always top level. 
	 * If you export only a shipment (with no consol) then it is top level.
	 * 
	 * Universal XML 2012
	 * If you export from a consol with shipments, the consol is top level. However only consol.SubShipmentCollection is populated with the shipments.
	 * If you export from a shipment attached to a consol, the shipment is top level. However only shipment.ParentSubShipmentcollection is populated with the consol.
	 * If you export from a shipment only, the shipment is top level.
	 */
	public static class TransitUniversalExtensions
	{
		#region Consolidated Job Helper

		public static bool IsSupportedJob(this IEnumerable<IDataSourceDataObject> dataSources)
		{
			return
				(dataSources.Count() == 1 && dataSources.Any(d => d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingConsol) ||
																	d.Type.GetValueOrDefault() == nameof(DataContextType.SeaCargoOutturn) ||
																	d.Type.GetValueOrDefault() == nameof(DataContextType.AirManifest) ||
																	d.Type.GetValueOrDefault() == nameof(DataContextType.GateBooking)))
				|| dataSources.Any(d => d.Type.GetValueOrDefault() == nameof(DataContextType.TransportConsignmentRunSheet) ||
										d.Type.GetValueOrDefault() == nameof(DataContextType.TransportConsignmentRunSheetInstruction) ||
										d.Type.GetValueOrDefault() == nameof(DataContextType.UnderBond));
		}

		public static bool IsConsolDataTarget(this IEnumerable<IDataTargetDataObject> dataTargets)
		{
			var contextType = dataTargets?.FirstOrDefault()?.Type ?? "";
			return IsConsolContextType(contextType);
		}

		#endregion

		#region Data Source Types

		public static bool IsFromForwardingConsol(this IEnumerable<IDataSourceDataObject> dataSources) => dataSources != null && dataSources.Any(d => d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingConsol));

		public static bool IsFromForwardingShipment(this IEnumerable<IDataSourceDataObject> dataSources) => dataSources != null && dataSources.Any(d => d.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment));

		public static bool IsFromLocalTransportRunSheet(this IEnumerable<IDataSourceDataObject> dataSources) => dataSources != null && dataSources.Any(d => d.Type.GetValueOrDefault() == nameof(DataContextType.LocalTransportRunSheet));

		#endregion

		#region Data Object Helpers

		public static UniversalShipment GetTopLevelDataObject(this IXmlImportLogger logger)
		{
			return logger.TopLevelDataObject as UniversalShipment;
		}

		public static UniversalShipment GetConsolDataObject(this IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var topLevelDO = GetTopLevelDataObject(logger);

			if (logger.TopLevelDataObject != null && topLevelDO == null)
			{
				var errorMessageStringBuilder = new ZStringBuilder();
				errorMessageStringBuilder.AppendLine(Res.GetString("811D13A8-4301-4457-AB64-060625A85280", "Transit Warehouse does not support this transaction type."));
				throw new DataObjectReadFailureException(errorMessageStringBuilder.ToString().TrimEnd());
			}

			if (SchemaVersionManager.Current == UniversalXmlSchema.Version_2012_11_DO_NOT_USE)
			{
				return IsConsolDataObject(topLevelDO) ? topLevelDO : GetConsolDataObject_2012(topLevelDO, factory);
			}
			else
			{
				return IsConsolDataObject(topLevelDO) ? topLevelDO : null;
			}
		}

		static UniversalShipment GetConsolDataObject_2012(UniversalShipment shipmentTopLevelDO, UniversalObjectFactory factory)
		{
			if (shipmentTopLevelDO.ParentShipmentCollection != null)
			{
				return shipmentTopLevelDO.ParentShipmentCollection.Count == 1
					? shipmentTopLevelDO.ParentShipmentCollection[0]
					: GetRelatedConsol(shipmentTopLevelDO, factory);
			}
			else if (shipmentTopLevelDO.SubShipmentCollection != null)
			{
				return shipmentTopLevelDO.SubShipmentCollection.Count == 1
					? shipmentTopLevelDO.SubShipmentCollection[0]
					: GetRelatedConsol(shipmentTopLevelDO, factory);
			}

			return null;
		}

		static UniversalShipment GetRelatedConsol(UniversalShipment shipmentTopLevelDO, UniversalObjectFactory factory)
		{
			if (shipmentTopLevelDO.IsArrivalTransitWarehouse())
			{
				var destination = shipmentTopLevelDO.PortOfDestination.GetUNLOCOAsUpperCase(factory.BOFactory);
				return shipmentTopLevelDO.SubShipmentCollection.FirstOrDefault(c => c.PortOfDischarge.GetUNLOCOAsUpperCase(factory.BOFactory) == destination && IsConsolDataObject(c));
			}
			else if (shipmentTopLevelDO.IsDepartureTransitWarehouse())
			{
				var origin = shipmentTopLevelDO.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory);
				return shipmentTopLevelDO.SubShipmentCollection.FirstOrDefault(c => c.PortOfLoading.GetUNLOCOAsUpperCase(factory.BOFactory) == origin && IsConsolDataObject(c));
			}

			return null;
		}

		static bool IsConsolDataObject(this UniversalShipment dataObject)
		{
			var contextType = dataObject?.DataContext?.DataSourceCollection?.FirstOrDefault()?.Type ?? dataObject?.DataContext?.DataTargetCollection?.FirstOrDefault()?.Type ?? "";
			return IsConsolContextType(contextType);
		}

		public static IDataSourceDataObject FirstDataSource(this UniversalShipment dataObject) => dataObject?.DataContext?.DataSourceCollection?.FirstOrDefault();

		public static IDataTargetDataObject FirstDataTarget(this UniversalShipment dataObject) => dataObject?.DataContext?.DataTargetCollection?.FirstOrDefault();

		public static OrganizationAddress GetOrgAddress(this UniversalShipment dataObject, ZString docAddressType)
		{
			return dataObject?.OrganizationAddressCollection?.FirstOrDefault(org => (org.AddressType ?? ZString.Empty) == docAddressType);
		}

		#endregion

		public static bool IsDepartureTransitWarehouse(this UniversalShipment shipmentTopLevelDO)
		{
			return shipmentTopLevelDO.HasRecipientRole(RecipientRoleType.DTW);
		}

		public static bool IsArrivalTransitWarehouse(this UniversalShipment shipmentTopLevelDO)
		{
			return shipmentTopLevelDO.HasRecipientRole(RecipientRoleType.ATW);
		}

		public static bool IsTransitWarehouseReceive(this UniversalShipment shipmentTopLevelDO)
		{
			return shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWR) || shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWR);
		}

		public static bool IsTransitWarehouseDispatch(this UniversalShipment shipmentTopLevelDO)
		{
			return shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWD) || shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWD);
		}

		public static bool IsTransitWarehouseCombined(this UniversalShipment shipmentTopLevelDO) =>
			shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.ATW, ServiceCodeType.TWX) ||
			shipmentTopLevelDO.HasRecipientRoleAndService(RecipientRoleType.DTW, ServiceCodeType.TWX);

		public static void CreateAdditionalReference(this UniversalShipment dataObject, string referenceNumber, string code, string description)
		{
			var transitWarehouseReceiveReferenceType = new EntryType { Code = code, Description = description };
			var transitWarehouseReceiveReference = new AdditionalReference { Type = transitWarehouseReceiveReferenceType, ReferenceNumber = referenceNumber };
			if (dataObject.AdditionalReferenceCollection != null || dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>()))
			{
				dataObject.AdditionalReferenceCollection.Add(transitWarehouseReceiveReference);
			}
		}

		public static ICusEntryNumber First(this ICustomsReferenceCollection references, string entryType, string entryNum = "")
		{
			return FirstICusEntryNum(references.Cast<ICusEntryNumber>(), entryType, entryNum);
		}

		public static ICusEntryNumber First(this IPortReferenceCollection references, string entryType, string entryNum = "")
		{
			return FirstICusEntryNum(references.Cast<ICusEntryNumber>(), entryType, entryNum);
		}

		static ICusEntryNumber FirstICusEntryNum(IEnumerable<ICusEntryNumber> references, string entryType, string entryNum)
		{
			ICusEntryNumber result;

			if (string.IsNullOrEmpty(entryNum))
			{
				result = references.FirstOrDefault(r => r.CE_EntryType == entryType);
			}
			else
			{
				result = references.FirstOrDefault(r => r.CE_EntryType == entryType && r.CE_EntryNum == entryNum);
			}

			return result;
		}

		public static bool IsFromDataSource(this UniversalShipment dataObject, DataContextType dataContextType)
		{
			return dataObject?.GetMatchingDataSource(dataContextType) != null;
		}

		public static ZString? GetMatchingDataSourceValue(this UniversalShipment dataObject, DataContextType dataContextType)
		{
			return dataObject?.GetMatchingDataSource(dataContextType)?.Key;
		}

		public static bool IsFromGateBooking(this UniversalShipment shipment) => shipment.IsFromDataSource(DataContextType.GateBooking) || shipment.IsFromDataSource(DataContextType.GateMovementBooking);

		public static Vehicle GetVehicle(this UniversalShipment dataObject)
		{
			var preCarriageVehicle = dataObject.PreCarriageShipmentCollection?.FirstOrDefault()?.VehicleRun?.Vehicle;
			return preCarriageVehicle ?? dataObject.VehicleRun?.Vehicle;
		}

		public static UniversalShipment GetFirstSubShipmentFromGateBooking(this UniversalShipment dataObject)
		{
			return dataObject.SubShipmentCollection?.FirstOrDefault(subShipment => subShipment.IsFromDataSource(DataContextType.GateMovementBooking));
		}

		public static HashSet<ZString> GetContainerNumbers(this UniversalShipment dataObject)
		{
			var res = dataObject?.ContainerCollection?.Where(c => c.ContainerNumber.HasValue && !c.ContainerNumber.Value.IsEmpty).Select(c => c.ContainerNumber ?? "").ToHashSet();
			if (res == null || res.Count == 0)
			{
				var firstSubShipment = dataObject.SubShipmentCollection.FirstOrDefault();
				res = firstSubShipment?.ContainerCollection?.Where(c => c.ContainerNumber.HasValue && !c.ContainerNumber.Value.IsEmpty).Select(c => c.ContainerNumber ?? "").ToHashSet() ?? new HashSet<ZString>();
			}
			return res;
		}

		public static IEnumerable<Container> GetContainers(this UniversalShipment dataObject, string containerNumberToMatch = "")
		{
			var res = dataObject?.ContainerCollection?.Where(c => c.ContainerNumber.HasValue && !c.ContainerNumber.Value.IsEmpty && (string.IsNullOrEmpty(containerNumberToMatch) || c.ContainerNumber.Value == containerNumberToMatch));
			if (res == null || !res.Any())
			{
				var firstSubShipment = dataObject.SubShipmentCollection.FirstOrDefault();
				res = firstSubShipment?.ContainerCollection?.Where(c => c.ContainerNumber.HasValue && !c.ContainerNumber.Value.IsEmpty && (string.IsNullOrEmpty(containerNumberToMatch) || c.ContainerNumber.Value == containerNumberToMatch));
			}
			return res ?? new List<Container>();
		}

		public static DataObjectList<PackingLine> GetPackingLines(this UniversalShipment dataObject)
		{
			var res = dataObject?.PackingLineCollection;
			if (res == null || res.Count == 0)
			{
				return dataObject.SubShipmentCollection.FirstOrDefault()?.PackingLineCollection;
			}
			return res;
		}

		public static ZString? GetDriverName(this UniversalShipment dataObject)
		{
			return dataObject.VehicleRun?.CrewCollection?.FirstOrDefault(cc => cc.FullName.HasValue)?.FullName;
		}

		public static ZString? GetBookingConfirmationReference(this UniversalShipment dataObject)
		{
			var result = dataObject.BookingConfirmationReference;
			if (!result.HasValue && dataObject.SubShipmentCollection?.Count > 0)
			{
				result = dataObject.SubShipmentCollection?.FirstOrDefault(ss => ss.BookingConfirmationReference.HasValue)?.BookingConfirmationReference;
			}
			return result;
		}

		#region Implementation

		static bool IsConsolContextType(ZString contextType)
		{
			return contextType == nameof(DataContextType.ForwardingConsol)
				|| contextType == nameof(DataContextType.TransportConsignmentRunSheet)
				|| contextType == nameof(DataContextType.TransportConsignmentRunSheetInstruction)
				|| contextType == nameof(DataContextType.SeaCargoOutturn)
				|| contextType == nameof(DataContextType.TransitReceiveConsol)
				|| contextType == nameof(DataContextType.TransitDispatchConsol)
				|| contextType == nameof(DataContextType.UnderBond)
				|| contextType == nameof(DataContextType.AirManifest);
		}

		public static bool IsPackageDeparted(this string status)
		{
			return status == TransitWarehouseStatuses.Codes.Departed || status == TransitWarehouseStatuses.Codes.Finalized;
		}

		#endregion
	}
}
