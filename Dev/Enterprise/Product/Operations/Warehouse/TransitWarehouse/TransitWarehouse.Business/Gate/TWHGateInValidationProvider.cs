using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using static Enterprise.Core.Constants.GateManagementConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transit.Business
{
	public class TWHGateInValidationProvider : IGateFacilityValidationService
	{
		public string[] ValidateMovement(ITopLevelDataObject shipment, string validationType)
		{
			var validationResults = new List<string>();
			if (shipment is UniversalShipment shipmentDataObject && shipmentDataObject.SubShipmentCollection.Count > 0 && validationType == FacilityValidationTypes.GateIn)
			{
				var arrivalCFSAddress = shipmentDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.ArrivalCFSAddress));
				if (arrivalCFSAddress == null || !IsValidZStringProperty(arrivalCFSAddress.AddressShortCode) || !IsValidZStringProperty(arrivalCFSAddress.OrganizationCode))
				{
					validationResults.Add(Res.GetString("AB8A2BF9-A1B7-4D50-A8AD-E5302567CAFD", "Arrival CFS Address must be provided"));
				}
				else
				{
					var arrivalCFS = GateMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, arrivalCFSAddress.OrganizationCode.Value, arrivalCFSAddress.AddressShortCode.Value, WarehouseTypes.Codes.Transit);
					if (arrivalCFS == null)
					{
						validationResults.Add(Res.GetString("BBEED061-66A9-475D-B232-FF3A7292EE5A", "Cannot find arrival warehouse"));
					}
				}
				var departureCFSAddress = shipmentDataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.DepartureCFSAddress));
				if (departureCFSAddress == null || !IsValidZStringProperty(departureCFSAddress.AddressShortCode) || !IsValidZStringProperty(departureCFSAddress.OrganizationCode))
				{
					validationResults.Add(Res.GetString("2199E906-6ED5-4470-8B46-58FEC00D578D", "Departure CFS Address must be provided"));
				}
				else
				{
					var departureCFS = GateMatchingHelper.GetFacilityFromOrgCodeAndAddressCode(Factory, departureCFSAddress.OrganizationCode.Value, departureCFSAddress.AddressShortCode.Value, WarehouseTypes.Codes.Transit);
					if (departureCFS == null)
					{
						validationResults.Add(Res.GetString("107A95E1-56C9-48B9-85A5-E1E01BA0F835", "Cannot find departure warehouse"));
					}
				}

				var subShipmentCollection = shipmentDataObject.SubShipmentCollection;
				var gateBookingDataContextType = nameof(DataContextType.GateBooking);
				var gateMovementBookingDataContextType = nameof(DataContextType.GateMovementBooking);
				foreach (var subShipment in subShipmentCollection)
				{
					if (validationResults.Count > 0)
					{
						break;
					}
					var gateBookingNumber = subShipment.DataContext.GetMatchingDataSource(DataContextType.GateBooking)?.Key;
					var gateMovementBookingNumber = subShipment.DataContext.GetMatchingDataSource(DataContextType.GateMovementBooking)?.Key;
					var direction = subShipment.TransportBookingDirection?.Code;
					if (!IsValidZStringProperty(gateBookingNumber))
					{
						validationResults.Add(Res.GetString("81857758-D123-44AF-A859-5AE885993B77", "Gate booking number must be provided"));
					}

					if (!IsValidZStringProperty(gateMovementBookingNumber))
					{
						validationResults.Add(Res.GetString("272EF8F4-FB15-4D9B-8C52-396389146C25", "Gate movement booking number must be provided"));
					}

					if (!direction.HasValue || (direction.Value != TransitConstants.TransportDirection.Delivery && direction.Value != TransitConstants.TransportDirection.Pickup))
					{
						validationResults.Add(Res.GetString("48E14933-6FD6-4B8E-82B6-9DDAC20D5CBD", "Transport direction must be either 'PIC' or 'DLV'"));
					}
					else if (direction.Value == TransitConstants.TransportDirection.Delivery && IsValidZStringProperty(gateBookingNumber) && IsValidZStringProperty(gateMovementBookingNumber))
					{
						var rtuWithGateMovementBookingReference = GateMatchingHelper.FindMatchingRTUByJobLink(Factory, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber.Value);
						if (rtuWithGateMovementBookingReference == null)
						{
							validationResults.Add(Res.GetString("D0FFA25B-06D8-4F2D-B64C-F525BAC7552B", "Matching RTU by {0} not found", "GateMovementBooking"));
						}
						else if (rtuWithGateMovementBookingReference.WRH_GateInTime != ZDateTimeOffset.Empty)
						{
							validationResults.Add(Res.GetString("7D0C8C2A-B37B-464E-A3B3-BAD710BA62D2", $"RTU - '{rtuWithGateMovementBookingReference.WRH_ReferenceNumber}' is already gated into the warehouse"));
						}
						else
						{
							var packageStates = TWHJobValidationHelper.GetValidPackageStates(rtuWithGateMovementBookingReference.BookedPackagesInPendingASNs);
							var warehouse = Factory.Load<WhsWarehouse>(rtuWithGateMovementBookingReference.WRH_WW_Warehouse);
							if (warehouse == null)
							{
								validationResults.Add(Res.GetString("B53041E1-5C50-4DB9-8E2C-CA5C86B803CD", $"Matching warehouse by RTU - '{rtuWithGateMovementBookingReference.WRH_ReferenceNumber}' not found"));
								break;
							}
							var errorMessage = TWHJobValidationHelper.ValidateDGPackageNotExceedCore(packageStates, warehouse);
							if (errorMessage != null)
							{
								validationResults.Add(errorMessage);
							}
							var rtuWithGateBookingReference = GateMatchingHelper.FindMatchingRTUByJobLink(Factory, nameof(DataContextType.GateBooking), gateBookingNumber.Value);
							if (rtuWithGateBookingReference == null)
							{
								validationResults.Add(Res.GetString("162F8C18-F1D8-4973-B289-DD605B7F9FF2", "Matching RTU by {0} not found", gateBookingDataContextType));
							}
							else if (rtuWithGateBookingReference.PK != rtuWithGateMovementBookingReference.PK)
							{
								if (rtuWithGateBookingReference.WRH_GateInTime != ZDateTimeOffset.Empty)
								{
									validationResults.Add(Res.GetString("9627A6D1-511E-48E1-81F3-7E9255953CC8", $"RTU - '{rtuWithGateBookingReference.WRH_ReferenceNumber}' is already gated into the warehouse"));
								}
								else
								{
									packageStates = TWHJobValidationHelper.GetValidPackageStates(rtuWithGateMovementBookingReference.BookedPackagesInPendingASNs);
									errorMessage = TWHJobValidationHelper.ValidateDGPackageNotExceedCore(packageStates, warehouse);
									if (errorMessage != null)
									{
										validationResults.Add(errorMessage);
									}
								}
							}
						}
					}
					else if (direction.Value == TransitConstants.TransportDirection.Pickup && IsValidZStringProperty(gateMovementBookingNumber) && IsValidZStringProperty(gateBookingNumber))
					{
						var dtuWithGateMovementBookingReference = GateMatchingHelper.FindMatchingDTUByJobLink(Factory, nameof(DataContextType.GateMovementBooking), gateMovementBookingNumber.Value);
						if (dtuWithGateMovementBookingReference == null)
						{
							validationResults.Add(Res.GetString("329A4406-5741-4647-A623-3201C9A6876C", "Matching DTU by {0} not found", gateMovementBookingDataContextType));
						}
						else if (dtuWithGateMovementBookingReference.WDH_GateInTime != ZDateTimeOffset.Empty)
						{
							validationResults.Add(Res.GetString("72185FCD-543E-46F6-852F-F7B18BB9026A", $"DTU - '{dtuWithGateMovementBookingReference.WDH_ReferenceNumber}' is already gated into the warehouse"));
						}
						else
						{
							var dtuWithGateBookingReference = GateMatchingHelper.FindMatchingDTUByJobLink(Factory, nameof(DataContextType.GateBooking), gateBookingNumber.Value);
							if (dtuWithGateBookingReference == null)
							{
								validationResults.Add(Res.GetString("30265932-01FC-42BD-8184-6195E9FCDA95", "Matching DTU by {0} not found", gateBookingDataContextType));
							}
							else if (dtuWithGateBookingReference.PK != dtuWithGateMovementBookingReference.PK)
							{
								if (dtuWithGateBookingReference.WDH_GateInTime != ZDateTimeOffset.Empty)
								{
									validationResults.Add(Res.GetString("F4F90B7A-C089-480F-8F44-FF3C0A08B6C2", $"DTU - '{dtuWithGateBookingReference.WDH_ReferenceNumber}' is already gated into the warehouse"));
								}
							}
						}
					}
				}
			}
			else
			{
				validationResults.Add(Res.GetString("8a6ca909-aa54-48d6-bb95-6f4dd4e72a80", "Invalid shipment or validation type"));
			}

			return validationResults.ToArray();
		}

		bool IsValidZStringProperty(ZString? property) => property.HasValue && property.Value.Length > 0;

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
