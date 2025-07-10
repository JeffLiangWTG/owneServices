using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Registry;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ShipmentInformationProvider : InformationProvider
	{
		public ShipmentInformationProvider(ForwardingShipment shipment) : base(shipment.Factory)
		{
			this.shipment = shipment;
		}

		readonly ForwardingShipment shipment;

		public InformationResult<ZString> Forwarder
		{
			get
			{
				return new InformationResult<ZString>(new ZString(ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.Value),
				Res.GetString("e9dd1203-36cd-44d0-af1b-3de9b91af944", "'{0}' registry item", ForwardingConfigurationRegistry.Instance.CargoIMPPhase2ForwarderId.Caption));
			}
		}

		public InformationResult<ZString> HouseBillId
		{
			get { return new InformationResult<ZString>(shipment.JS_HouseBillInfo); }
		}

		public InformationResult<ZDateTime> HouseBillDate
		{
			get
			{
				if (ForwardingConfigurationRegistry.Instance.CargoIMPPhase2UseETDForHouseBillDate.Value)
				{
					return new InformationResult<ZDateTime>(shipment.JS_E_DEPInfo);
				}
				else
				{
					return new InformationResult<ZDateTime>(shipment.JS_SystemCreateTimeUtcInfo);
				}
			}
		}

		public InformationResult<ZString> HouseBillOrigin
		{
			get { return new InformationResult<ZString>(GetLocationCode(shipment.JS_RL_NKOrigin), shipment.JS_RL_NKOriginInfo); }
		}

		public InformationResult<ZString> HouseBillDestination
		{
			get { return new InformationResult<ZString>(GetLocationCode(shipment.JS_RL_NKDestination), shipment.JS_RL_NKDestinationInfo); }
		}

		public InformationResult<ZInt> Pieces
		{
			get { return new InformationResult<ZInt>(shipment.JS_OuterPacksInfo); }
		}

		public InformationResult<ZDecimal> Weight
		{
			get
			{
				ZString unit = shipment.JS_UnitOfWeight;
				ZDecimal value = shipment.JS_ActualWeight;
				if (Constants.Weight.Kilograms != unit &&
					Constants.Weight.Pounds != unit)
				{
					value = Constants.Weight.Convert(value, unit, Constants.Weight.Kilograms);
				}

				return new InformationResult<ZDecimal>(value, shipment, shipment.JS_ActualWeightInfo);
			}
		}

		public InformationResult<WeightUnits> WeightUnit
		{
			get
			{
				WeightUnits unit = WeightUnits.None;
				if (Constants.Weight.Pounds == shipment.JS_UnitOfWeight)
				{
					unit = WeightUnits.Pounds;
				}
				else
				{
					unit = WeightUnits.Kilograms;
				}

				return new InformationResult<WeightUnits>(unit, shipment.JS_UnitOfWeightInfo);
			}
		}

		public InformationResult<ZDecimal> Volume
		{
			get
			{
				ZString unit = shipment.JS_UnitOfVolume;
				ZDecimal value = shipment.JS_ActualVolume;
				if (Constants.Volume.CubicMetres != unit &&
					Constants.Volume.CubicFeet != unit)
				{
					value = Constants.Volume.Convert(value, unit, Constants.Volume.CubicMetres);
				}

				return new InformationResult<ZDecimal>(value, shipment, shipment.JS_ActualVolumeInfo);
			}
		}

		public InformationResult<VolumeUnits> VolumeUnit
		{
			get
			{
				VolumeUnits unit = VolumeUnits.None;
				if (Constants.Volume.CubicFeet == shipment.JS_UnitOfVolume)
				{
					unit = VolumeUnits.CubicFeet;
				}
				else
				{
					unit = VolumeUnits.CubicMetres;
				}

				return new InformationResult<VolumeUnits>(unit, shipment.JS_UnitOfVolumeInfo);
			}
		}

		public InformationResult<ZString> ProductCode
		{
			get { return InformationResult<ZString>.Empty; }
		}

		public InformationResult<ZString> ServiceCode
		{
			get { return InformationResult<ZString>.Empty; }
		}

		public InformationResult<ZString> HandlingCode1
		{
			get { return InformationResult<ZString>.Empty; }
		}

		public InformationResult<ZString> HandlingCode2
		{
			get { return InformationResult<ZString>.Empty; }
		}

		public List<ShipmentTransportInfoProvider> Transports
		{
			get
			{
				List<ShipmentTransportInfoProvider> transports = new List<ShipmentTransportInfoProvider>();
				foreach (Transport transport in shipment.TransportsIncludingRelated)
				{
					transports.Add(new ShipmentTransportInfoProvider(shipment, transport));
				}

				return transports;
			}
		}

		public List<ShipmentReference> References
		{
			get
			{
				List<ShipmentReference> references = new List<ShipmentReference>();

				ShipmentReference jobNumber;
				jobNumber.Reference = new InformationResult<ZString>(shipment.JS_UniqueConsignRefInfo);
				jobNumber.ReferenceType = new InformationResult<ShipmentReferenceTypes>(ShipmentReferenceTypes.JobNumber, Res.GetString("9a67dfef-63ff-4e27-a24d-d08e878a94ea", "Reference Number Type"));
				references.Add(jobNumber);

				return references;
			}
		}

		public List<InterestedParty> InterestedParties
		{
			get
			{
				List<InterestedParty> interestedParties = new List<InterestedParty>();

				AddInterestedParty(Consignor, InterestedPartyTypes.Consignor, interestedParties);
				AddInterestedParty(Consignee, InterestedPartyTypes.Consignee, interestedParties);
				AddInterestedParty(NotifyParty, InterestedPartyTypes.NotifyParty, interestedParties);

				return interestedParties;
			}
		}

		void AddInterestedParty(InformationResult<ZString> partyResult, InterestedPartyTypes partyType, List<InterestedParty> parties)
		{
			if (!partyResult.Value.IsEmpty)
			{
				InterestedParty party = new InterestedParty();
				party.Id = partyResult;
				party.Type = new InformationResult<InterestedPartyTypes>(partyType, Res.GetString("4c938ae3-ef7a-4217-b63a-cb5961116d95", "Party Type"));
				party.ServiceIndicator = InformationResult<ZString>.Empty;
				party.Reference = InformationResult<ZString>.Empty;
				party.Remarks = InformationResult<ZString>.Empty;
				parties.Add(party);
			}
		}

		public InformationResult<ZString> Consignor
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.ConsignorDocumentaryAddress), shipment, Res.GetString("7f3838b6-af24-46d7-ad83-7b65aaf3e6a7", "Consignor"));
			}
		}

		public InformationResult<ZString> Consignee
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.ConsigneeDocumentaryAddress), shipment, Res.GetString("dbde2e91-994d-4ce0-86ee-2eb57d7fa72d", "Consignee"));
			}
		}

		public InformationResult<ZString> NotifyParty
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.NotifyPartyDocumentaryAddress), shipment, Res.GetString("8d0a8d7a-28fc-4278-a2b7-2ec2337f2f0e", "Notify Party"));
			}
		}

		public InformationResult<ZString> PickupFrom
		{
			get
			{
				return new InformationResult<ZString>(GetLocationCode(shipment.ConsignorPickupAddress),
						shipment, Res.GetString("25be69b3-068a-4672-a0f9-edd6eaffbb2e", "Consignor Pickup Address"));
			}
		}

		public InformationResult<ZDateTime> EstimatedPickupDate
		{
			get
			{
				return new InformationResult<ZDateTime>(shipment.DocsAndCartage.JP_EstimatedPickup,
						shipment, shipment.DocsAndCartage.JP_EstimatedPickupInfo);
			}
		}

		public InformationResult<ZString> PickupCustomer
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ConsignorPickupAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ConsignorPickupAddress), shipment, Res.GetString("25be69b3-068a-4672-a0f9-edd6eaffbb2e", "Consignor Pickup Address"));
				}
				else if (IsOrganisationValid(shipment.ConsignorDocumentaryAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ConsignorDocumentaryAddress),
							shipment, Res.GetString("7f3838b6-af24-46d7-ad83-7b65aaf3e6a7", "Consignor"));
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("a95a39af-b9ef-4987-9484-b807266a272e", "Pickup From"));
				}

				return result;
			}
		}

		public InformationResult<ZString> PickupParty
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.DocsAndCartage.PickupCartageCo),
						shipment, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
			}
		}

		public InformationResult<ZString> ExportWarehouse
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ExportReceivingDepot))
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.ExportReceivingDepot), shipment.JS_OA_ExportReceivingDepotInfo);
				}
				else if (shipment.DepartureConsol != null && IsOrganisationValid(shipment.DepartureConsol.PackDepotAddress))
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.DepartureConsol.PackDepotAddress),
							shipment.DepartureConsol.JK_OA_PackDepotAddressInfo);
				}
				else
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.JS_RL_NKOrigin), shipment.JS_RL_NKOriginInfo);
				}

				return result;
			}
		}

		public InformationResult<ZDateTime> EstimatedExportWarehouseDeliveryDate
		{
			get
			{
				return new InformationResult<ZDateTime>(Res.GetString("03456bea-ef51-417d-9d84-8e453d9d447f", "Planned Date and Time Of Receipt"));
			}
		}

		public InformationResult<ZString> ExportWarehouseCustomer
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ExportReceivingDepot))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ExportReceivingDepot), shipment.JS_OA_ExportReceivingDepotInfo);
				}
				else if (shipment.DepartureConsol != null && IsOrganisationValid(shipment.DepartureConsol.PackDepotAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.DepartureConsol.PackDepotAddress),
							shipment.DepartureConsol.JK_OA_PackDepotAddressInfo);
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("6e4daeb8-2478-4063-ab3a-c7c7786697da", "Export Warehouse Customer"));
				}

				return result;
			}
		}

		public InformationResult<ZString> ExportWarehouseDeliveryParty
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.DocsAndCartage.PickupCartageCo),
						shipment, shipment.DocsAndCartage.JP_OA_PickupCartageCoAddrInfo);
			}
		}

		public InformationResult<ZString> ExportWarehousePickupParty
		{
			get { return new InformationResult<ZString>(Res.GetString("e37d3f5c-db1d-42a2-bd37-669e4b9d1157", "Export Warehouse Pickup Party")); }
		}

		public InformationResult<ZString> ExportingCarriersTerminal
		{
			get
			{
				InformationResult<ZString> result;
				if (shipment.DepartureConsol != null && IsOrganisationValid(shipment.DepartureConsol.DepartureCTOAddress))
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.DepartureConsol.DepartureCTOAddress), shipment.DepartureConsol.JK_OA_DepartureCTOAddressInfo);
				}
				else if (shipment.DepartureConsol != null && DepartureMAWB != null)
				{
					result = new InformationResult<ZString>(DepartureMAWB.EH_AWBOriginCode, DepartureMAWB.EH_AWBOriginCodeInfo);
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("47e465a2-822d-4aa8-a40b-494db226a97d", "Exporting Carrier's Terminal Customer (Departure Consol CTO Address or MAWB Origin Code)"));
				}

				return result;
			}
		}

		public InformationResult<ZString> ExportingCarriersTerminalCustomer
		{
			get
			{
				if (shipment.DepartureConsol != null)
				{
					return new InformationResult<ZString>(GetPartyId(shipment.DepartureConsol.DepartureCTOAddress), shipment.DepartureConsol.JK_OA_DepartureCTOAddressInfo);
				}

				return new InformationResult<ZString>(Res.GetString("f3b0ced7-98de-46fd-8dea-937dde711266", "Consol Departure CTO Address"));
			}
		}

		public InformationResult<ZString> ExportingCarriersTerminalDeliveryParty
		{
			get { return new InformationResult<ZString>(Res.GetString("639c7c72-32fe-43d1-9210-73b2a2a479a4", "Exporting Carrier's Terminal Delivery Party")); }
		}

		public InformationResult<ZString> ImportingCarriersTerminal
		{
			get
			{
				InformationResult<ZString> result;
				if (ArrivalConsol != null && IsOrganisationValid(ArrivalConsol.ArrivalCTOAddress))
				{
					result = new InformationResult<ZString>(GetLocationCode(ArrivalConsol.ArrivalCTOAddress), ArrivalConsol.JK_OA_ArrivalCTOAddressInfo);
				}
				else if (ArrivalMAWB != null)
				{
					result = new InformationResult<ZString>(ArrivalMAWB.EH_AirportOfDestinationCode, ArrivalMAWB.EH_AirportOfDestinationCodeInfo);
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("5e761c8d-ba18-459c-8b98-995aa340074c", "Importing Carrier's Terminal (Arrival Consol CTO Address or MAWB Destination Code)"));
				}

				return result;
			}
		}

		public InformationResult<ZString> ImportWarehouse
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ImportReleaseDepot))
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.ImportReleaseDepot), shipment.JS_OA_ImportReleaseDepotInfo);
				}
				else if (ArrivalConsol != null && IsOrganisationValid(ArrivalConsol.UnpackDepotAddress))
				{
					result = new InformationResult<ZString>(GetLocationCode(ArrivalConsol.UnpackDepotAddress),
							ArrivalConsol.JK_OA_UnpackDepotAddressInfo);
				}
				else
				{
					result = new InformationResult<ZString>(GetLocationCode(shipment.JS_RL_NKDestination), shipment.JS_RL_NKDestinationInfo);
				}

				return result;
			}
		}

		public InformationResult<ZDateTime> EstimatedImportWarehousePickupDate
		{
			get { return new InformationResult<ZDateTime>(Res.GetString("13ac1205-7aa1-4c59-b7cd-fdb954a0a46e", "Estimated Import Warehouse Pickup Date")); }
		}

		public InformationResult<ZString> ImportWarehouseCustomer
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ImportReleaseDepot))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ImportReleaseDepot), shipment.JS_OA_ImportReleaseDepotInfo);
				}
				else if (ArrivalConsol != null && IsOrganisationValid(ArrivalConsol.UnpackDepotAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(ArrivalConsol.UnpackDepotAddress),
							ArrivalConsol.JK_OA_UnpackDepotAddressInfo);
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("affad7ed-fcf6-4794-872e-aa1dec6992e3", "Import Warehouse Customer"));
				}

				return result;
			}
		}

		public InformationResult<ZString> ImportWarehouseDeliveryParty
		{
			get { return new InformationResult<ZString>(Res.GetString("dce83ac4-212b-4da5-ad12-fc8e4977c8cc", "Import Warehouse Delivery Party")); }
		}

		public InformationResult<ZString> ImportWarehousePickupParty
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.DocsAndCartage.DeliveryCartageCo),
						shipment, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			}
		}

		public InformationResult<ZString> DeliverTo
		{
			get
			{
				return (!shipment.JS_RL_NKDestination.IsEmpty)
					? new InformationResult<ZString>(GetLocationCode(shipment.JS_RL_NKDestination), shipment, shipment.JS_RL_NKDestinationInfo)
					: new InformationResult<ZString>(GetLocationCode(shipment.ConsigneeDeliveryAddress), shipment, Res.GetString("9a17d246-8c5a-49a9-911a-29141f8e15d3", "Consignee Delivery Address"));
			}
		}

		public InformationResult<ZDateTime> EstimatedDeliveryDate
		{
			get
			{
				return new InformationResult<ZDateTime>(shipment.DocsAndCartage.JP_EstimatedDelivery,
						shipment, shipment.DocsAndCartage.JP_EstimatedDeliveryInfo);
			}
		}

		public InformationResult<ZString> DeliverToCustomer
		{
			get
			{
				InformationResult<ZString> result;
				if (IsOrganisationValid(shipment.ConsigneeDeliveryAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ConsigneeDeliveryAddress), shipment, Res.GetString("9a17d246-8c5a-49a9-911a-29141f8e15d3", "Consignee Delivery Address"));
				}
				else if (IsOrganisationValid(shipment.ConsigneeDocumentaryAddress))
				{
					result = new InformationResult<ZString>(GetPartyId(shipment.ConsigneeDocumentaryAddress),
							shipment, Res.GetString("dbde2e91-994d-4ce0-86ee-2eb57d7fa72d", "Consignee"));
				}
				else
				{
					result = new InformationResult<ZString>(Res.GetString("076524c6-a574-4ad8-b62d-1b3ad7836e19", "Deliver To"));
				}

				return result;
			}
		}

		public InformationResult<ZString> DeliverToDeliveryParty
		{
			get
			{
				return new InformationResult<ZString>(GetPartyId(shipment.DocsAndCartage.DeliveryCartageCo),
						shipment, shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddrInfo);
			}
		}

		ConsolExportAWBHeader DepartureMAWB
		{
			get
			{
				if (!departureMAWBPopulated)
				{
					departureMAWBPopulated = true;
					if (shipment.DepartureConsol != null && shipment.DepartureConsol.IsAWBHeaderAccessible)
					{
						shipment.DepartureConsol.PopulateAWB();
						this.departureMAWB = shipment.DepartureConsol.AWBHeader as ConsolExportAWBHeader;
					}
				}

				return this.departureMAWB;
			}
		}
		ConsolExportAWBHeader departureMAWB;
		bool departureMAWBPopulated;

		ConsolExportAWBHeader ArrivalMAWB
		{
			get
			{
				if (!arrivalMAWBPopulated)
				{
					arrivalMAWBPopulated = true;
					if (ArrivalConsol != null && ArrivalConsol.IsAWBHeaderAccessible)
					{
						ArrivalConsol.PopulateAWB();
						this.arrivalMAWB = ArrivalConsol.AWBHeader as ConsolExportAWBHeader;
					}
				}

				return this.arrivalMAWB;
			}
		}
		ConsolExportAWBHeader arrivalMAWB;
		bool arrivalMAWBPopulated;

		ForwardingConsol ArrivalConsol
		{
			get
			{
				var result = shipment.ArrivalConsol;
				if (result == null && shipment.Consols.Count == 1)
				{
					result = shipment.Consols[0];
				}

				return result;
			}
		}
	}
}
