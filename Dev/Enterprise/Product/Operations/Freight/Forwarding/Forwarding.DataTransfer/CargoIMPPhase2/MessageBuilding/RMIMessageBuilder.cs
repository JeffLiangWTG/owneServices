using Enterprise.Edifact.Auto;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class RMIMessageBuilder : MessageBuilder
	{
		public RMIMessageBuilder()
		{
		}

		ShipmentInformationProvider Provider
		{
			get { return this.provider ?? (this.provider = new ShipmentInformationProvider(Shipment)); }
		}
		ShipmentInformationProvider provider;

		protected override SegmentGroup CreateMessage()
		{
			RMIMessage message = new RMIMessage();
			message.RMI[0].MessageVersion = MessageVersion;
			FillMessageDetail(message.MSG[0]);
			FillShipmentDetail(message.SHD[0]);
			FillShipmentRoutingGroups(message.Group1);
			FillShipmentReferences(message.REF);
			FillInterestedParties(message.PAR);
			return message;
		}

		void FillShipmentDetail(RMISHDSegment sHDSegment)
		{
			sHDSegment.Forwarder = ConvertAndCheckValue(Provider.Forwarder, true, DataTypeDefinitions.Parties_ForwarderId);
			sHDSegment.HouseBillId = ConvertAndCheckValue(Provider.HouseBillId, true, DataTypeDefinitions.ShipmentIds_ShipmentId);
			sHDSegment.HouseBillDate = ConvertAndCheckValue(Provider.HouseBillDate, true, DataTypeDefinitions.Date);
			sHDSegment.HouseBillOrigin = ConvertAndCheckValue(Provider.HouseBillOrigin, true, DataTypeDefinitions.Locations_OrgDestCode);
			sHDSegment.HouseBillDestination = ConvertAndCheckValue(Provider.HouseBillDestination, true, DataTypeDefinitions.Locations_OrgDestCode);
			sHDSegment.TotalPieces = ConvertAndCheckValue(Provider.Pieces, true, DataTypeDefinitions.Quantities_NumberOfPieces);
			sHDSegment.TotalWeight = ConvertAndCheckValue(Provider.Weight, true, DataTypeDefinitions.Quantities_Weight);
			sHDSegment.WeightUnit = ConvertAndCheckValue(provider.WeightUnit, true, DataTypeDefinitions.WeightUnitConverter);
			sHDSegment.TotalVolume = ConvertAndCheckValue(Provider.Volume, false, DataTypeDefinitions.Quantities_Volume);
			if (!string.IsNullOrEmpty(sHDSegment.TotalVolume))
			{
				sHDSegment.VolumeUnit = ConvertAndCheckValue(provider.VolumeUnit, true, DataTypeDefinitions.VolumeUnitConverter);
			}

			sHDSegment.ProductCode = ConvertAndCheckValue(Provider.ProductCode, false, DataTypeDefinitions.ShipmentDetail_ProductServiceCode);
			sHDSegment.ServiceCode = ConvertAndCheckValue(Provider.ServiceCode, false, DataTypeDefinitions.ShipmentDetail_ProductServiceCode);
			sHDSegment.HandlingCode1 = ConvertAndCheckValue(Provider.HandlingCode1, false, DataTypeDefinitions.ShipmentDetail_HandlingCode);
			sHDSegment.HandlingCode2 = ConvertAndCheckValue(Provider.HandlingCode2, false, DataTypeDefinitions.ShipmentDetail_HandlingCode);
			sHDSegment.CustomerId = ConvertAndCheckValue(Provider.Consignor, false, DataTypeDefinitions.Parties_InterestedPartyId);
		}

		void FillShipmentRoutingGroups(SegmentGroup1MessageSection segmentGroup1MessageSection)
		{
			SegmentGroup1 group1 = segmentGroup1MessageSection.InstantiateAChildAndAddItToChildrenCollection();
			FillShipmentRouting(group1.RTG[0]);
			//FillPlannedPickup(group1.PPU); //Commented out temporarily. PUP will be supported in future releases
			FillPlannedMovements(group1.PMV);
			FillPlannedDelivery(group1.PDL);
		}

		void FillShipmentRouting(RTGSegment rTG)
		{
			rTG.TimeIndicator = TimeIndicatorList.Local;
			rTG.FirstForwarderLocation = ConvertAndCheckValue(Provider.ExportWarehouse, true, DataTypeDefinitions.Locations_LocationId);
			rTG.LastForwarderLocation = ConvertAndCheckValue(Provider.ImportWarehouse, false, DataTypeDefinitions.Locations_LocationId);
		}

		void FillPlannedMovements(PMVSegmentMessageSection pMVSegmentMessageSection)
		{
			var transports = Provider.Transports;
			if (transports.Count > 0)
			{
				FillExportWarehouseToTerminalMovement(pMVSegmentMessageSection);
			}

			foreach (ShipmentTransportInfoProvider transportProvider in transports)
			{
				PMVSegment pMV = pMVSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
				pMV.DepartureLocation = ConvertAndCheckValue(transportProvider.DepartureLocation, true, DataTypeDefinitions.Locations_LocationId);
				pMV.PlannedDateAndTimeOfDeparture = ConvertAndCheckValue(transportProvider.ETD, false, DataTypeDefinitions.DateTime);
				pMV.ArrivalLocation = ConvertAndCheckValue(transportProvider.ArrivalLocation, true, DataTypeDefinitions.Locations_LocationId);
				pMV.PlannedDateAndTimeOfReceipt = ConvertAndCheckValue(transportProvider.ETA, false, DataTypeDefinitions.DateTime);
				pMV.VoyageMode = ConvertAndCheckValue(transportProvider.VoyageMode, true, DataTypeDefinitions.VoyageModeConverter);
				pMV.MasterBill = ConvertAndCheckValue(transportProvider.MasterBillNumber, false, DataTypeDefinitions.ShipmentIds_ShipmentId);
				pMV.Carrier = ConvertAndCheckValue(transportProvider.CarrierCode, false, DataTypeDefinitions.Parties_CarrierId);
			}

			if (transports.Count > 0)
			{
				FillImportTerminalToWarehouseMovement(pMVSegmentMessageSection);
			}
		}

		void FillExportWarehouseToTerminalMovement(PMVSegmentMessageSection pMVSegmentMessageSection)
		{
			var exportWarehouse = Provider.ExportWarehouse;
			if (!exportWarehouse.Value.IsEmpty)
			{
				PMVSegment pMV = pMVSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
				pMV.DepartureLocation = ConvertAndCheckValue(exportWarehouse, true, DataTypeDefinitions.Locations_LocationId);
				pMV.ArrivalLocation = ConvertAndCheckValue(Provider.ExportingCarriersTerminal, true, DataTypeDefinitions.Locations_LocationId);
				pMV.ArrivalLocationRole = LocationsLocationRoleList.CarriersExportTerminal;
			}
		}

		void FillImportTerminalToWarehouseMovement(PMVSegmentMessageSection pMVSegmentMessageSection)
		{
			var importWarehouse = Provider.ImportWarehouse;
			if (!importWarehouse.Value.IsEmpty)
			{
				PMVSegment pMV = pMVSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
				pMV.DepartureLocation = ConvertAndCheckValue(Provider.ImportingCarriersTerminal, true, DataTypeDefinitions.Locations_LocationId);
				pMV.DepartureLocationRole = LocationsLocationRoleList.CarriersImportTerminal;
				pMV.ArrivalLocation = ConvertAndCheckValue(importWarehouse, true, DataTypeDefinitions.Locations_LocationId);
			}
		}

		void FillPlannedDelivery(PDLSegmentMessageSection pDLSegmentMessageSection)
		{
			PDLSegment pDL = pDLSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
			pDL.ImportDispositionIndicator = MovementDispositionTypeList.NormalDeliveryByForwToDoor;
			pDL.ForwardersDispatchLocation = ConvertAndCheckValue(Provider.ImportWarehouse, false, DataTypeDefinitions.Locations_LocationId);
			pDL.PlannedDateAndTimeOfDispatch = ConvertAndCheckValue(Provider.EstimatedImportWarehousePickupDate, false, DataTypeDefinitions.DateTime);
			pDL.DeliveryLocation = ConvertAndCheckValue(Provider.DeliverTo, false, DataTypeDefinitions.Locations_LocationId);
			pDL.DeliveryDate = ConvertAndCheckValue(Provider.EstimatedDeliveryDate, false, DataTypeDefinitions.Date);
			pDL.DeliveryTime = ConvertAndCheckValue(Provider.EstimatedDeliveryDate, false, DataTypeDefinitions.Time);
			pDL.DeliveryCustomerId = ConvertAndCheckValue(Provider.DeliverToCustomer, false, DataTypeDefinitions.Parties_InterestedPartyId);
			pDL.DeliveringPartyId = ConvertAndCheckValue(Provider.DeliverToDeliveryParty, false, DataTypeDefinitions.Parties_InterestedPartyId);
		}

		void FillShipmentReferences(REFSegmentMessageSection rEFSegmentMessageSection)
		{
			foreach (ShipmentReference reference in Provider.References)
			{
				if (!reference.Reference.Value.IsEmpty)
				{
					bool isFormattedCorrectly;
					string referenceId = ConvertAndCheckValue(reference.Reference, false, DataTypeDefinitions.ShipmentIds_ShipmentId, out isFormattedCorrectly);
					if (isFormattedCorrectly)
					{
						REFSegment rEF = rEFSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
						rEF.ReferenceId = referenceId;
						rEF.ReferenceTypeIdentifier = ConvertAndCheckValue(reference.ReferenceType, true, DataTypeDefinitions.ShipmentReferenceTypeConverter);
					}
				}
			}
		}

		void FillInterestedParties(PARSegmentMessageSection pARSegmentMessageSection)
		{
			foreach (InterestedParty party in Provider.InterestedParties)
			{
				bool isFormattedCorrectly;
				string interestedPartyId = ConvertAndCheckValue(party.Id, false, DataTypeDefinitions.Parties_InterestedPartyId, out isFormattedCorrectly);
				if (isFormattedCorrectly)
				{
					PARSegment pAR = pARSegmentMessageSection.InstantiateAChildAndAddItToChildrenCollection();
					pAR.InterestedPartyId = interestedPartyId;
					pAR.InterestedPartyType = ConvertAndCheckValue(party.Type, true, DataTypeDefinitions.InterestedPartyTypeConverter);
					pAR.InterestedPartyServiceIndicator = ConvertAndCheckValue(party.ServiceIndicator, false, DataTypeDefinitions.Parties_InterestedPartyServiceIndicator);
					pAR.InterestedPartyReference = ConvertAndCheckValue(party.Reference, false, DataTypeDefinitions.Parties_InterestedPartyReference);
					pAR.Remarks = ConvertAndCheckValue(party.Remarks, false, DataTypeDefinitions.OtherInfo_Remarks);
				}
			}
		}
	}
}
