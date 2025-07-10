using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using AdditionalReferenceCodes = Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants.AdditionalReferences.Codes;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalPackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class BookingConfirmationBuilder
	{
		public BookingConfirmationBuilder(ForwardingConsol consol, IDocDataObjectParameters parameters)
		{
			this.consol = consol ?? throw new ArgumentNullException(nameof(consol));
			this.parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

			context = new ContextWithCarrierUnlocoMapping(consol.Factory.GetCachedReadOnlyFactory(), consol.IsCoLoad ? consol.CreditorPK : consol.ShippingLinePK, false);
		}

		readonly ForwardingConsol consol;
		readonly IDocDataObjectParameters parameters;
		readonly IContext context;

		public BookingConfirmation Build()
		{
			var universalShipment = parameters.Data is UniversalShipment usxml
				? usxml
				: consol.GetBookingConfirmationUniversalXml();

			var resultBC = new BookingConfirmation(consol.PK);
			PopulateAllProperties(resultBC, universalShipment);

			var universalShipmentBR = consol.GetUniversalXmlFromBookingRequestDialog();

			if (universalShipmentBR != null)
			{
				var resultBR = new BookingConfirmation(consol.PK);
				PopulateAllProperties(resultBR, universalShipmentBR);

				CompareAndAddWarningMessage(resultBC, resultBR);
			}

			resultBC.ValidateAllIncludingChildren();

			return resultBC;
		}

		void PopulateAllProperties(BookingConfirmation result, UniversalShipment universalShipment)
		{
			PopulateAddressesAndDependentProperties(result, universalShipment);
			PopulateNotes(result, universalShipment);
			PopulateNumbers(result, universalShipment);
			PopulateTransportsAndDependentProperties(result, universalShipment);
			PopulateUnlocos(result, universalShipment);
			PopulateCodeDescriptions(result, universalShipment);
			PopulateIsDoorProperties(result, universalShipment);
			PopulateContainers(result, universalShipment);
		}

		void AddWarningMessageForAddress(Address addressFromBC, Address addressFromBR)
		{
			AddWarningMessage(addressFromBC.CompanyNameInfo, addressFromBC.CompanyName, addressFromBR.CompanyName);
			AddWarningMessage(addressFromBC.AddressLine1Info, addressFromBC.AddressLine1, addressFromBR.AddressLine1);
			AddWarningMessage(addressFromBC.AddressLine2Info, addressFromBC.AddressLine2, addressFromBR.AddressLine2);
			AddWarningMessage(addressFromBC.CityInfo, addressFromBC.City, addressFromBR.City);
			AddWarningMessage(addressFromBC.Country.NameInfo, addressFromBC.Country.Name, addressFromBR.Country.Name);
			AddWarningMessage(addressFromBC.ContactInfo, addressFromBC.Contact, addressFromBR.Contact);
		}

		void AddWarningMessage(ZPropertyInfo propertyInfo, IZType valueFromBC, IZType valueFromBR)
		{
			propertyInfo.AddWarning(() =>
				!valueFromBR.IsEmpty && valueFromBC.CompareTo(valueFromBR) != 0,
				(NoResString)"Requested Info: " + valueFromBR);  // non-translatable message
		}

		void AddWarningMessageForUnloco(ZPropertyInfo propertyInfo, ZString unlocoFromBC, ZString unlocoFromBR)
		{
			var unlocoMapper = context.Unlocos as ICodeMapper;
			var mappingUnlocoFromBC = unlocoMapper?.GetLocalCode(unlocoFromBC) ?? unlocoFromBC;
			var mappingUnlocoFromBR = unlocoMapper?.GetLocalCode(unlocoFromBR) ?? unlocoFromBR;

			propertyInfo.AddWarning(() =>
				!string.IsNullOrEmpty(mappingUnlocoFromBC) && mappingUnlocoFromBC != mappingUnlocoFromBR,
				(NoResString)"Requested Info: " + mappingUnlocoFromBR);  // non-translatable message
		}

		void AddWarningMessageForGeneralProperties(BookingConfirmation resultBC, BookingConfirmation resultBR)
		{
			AddWarningMessageForAddress(resultBC.Shipper, resultBR.Shipper);
			AddWarningMessageForAddress(resultBC.Carrier, resultBR.Carrier);
			AddWarningMessageForAddress(resultBC.Consignee, resultBR.Consignee);
			AddWarningMessageForAddress(resultBC.NotifyParty, resultBR.NotifyParty);

			AddWarningMessageForAddress(resultBC.ContainerTerminalOperator.ImportableValue, resultBR.ContainerTerminalOperator.ImportableValue);
			resultBC.ContainerTerminalOperator.ImportableValue.ValidateAllIncludingChildren();

			AddWarningMessageForAddress(resultBC.PickUpFrom, resultBR.PickUpFrom);
			AddWarningMessageForAddress(resultBC.DeliverTo, resultBR.DeliverTo);
			AddWarningMessage(resultBC.CarrierBookingOffice.NameInfo, resultBC.CarrierBookingOffice.Name, resultBR.CarrierBookingOffice.Name);
			AddWarningMessage(resultBC.CarrierConfirmationNotes?.ImportableValueInfo, resultBC.CarrierConfirmationNotes?.ImportableValue ?? ZString.Empty, resultBR.CarrierConfirmationNotes?.ImportableValue ?? ZString.Empty);
			AddWarningMessage(resultBC.CarrierBookingReference?.ImportableValueInfo, resultBC.CarrierBookingReference?.ImportableValue ?? ZString.Empty, resultBR.CarrierBookingReference?.ImportableValue ?? ZString.Empty);
			AddWarningMessage(resultBC.BillOfLadingNumber.ImportableValueInfo, resultBC.BillOfLadingNumber.ImportableValue, resultBR.BillOfLadingNumber.ImportableValue);
			AddWarningMessage(resultBC.VesselNameInfo, resultBC.VesselName, resultBR.VesselName);
			AddWarningMessage(resultBC.LloydsIMOInfo, resultBC.LloydsIMO, resultBR.LloydsIMO);
			AddWarningMessage(resultBC.VoyageNumberInfo, resultBC.VoyageNumber, resultBR.VoyageNumber);
			AddWarningMessage(resultBC.ShipperReferenceNumberInfo, resultBC.ShipperReferenceNumber, resultBR.ShipperReferenceNumber);
			AddWarningMessage(resultBC.FreightForwarderReferenceNumberInfo, resultBC.FreightForwarderReferenceNumber, resultBR.FreightForwarderReferenceNumber);
			AddWarningMessage(resultBC.CarrierContractNumber.ImportableValueInfo, resultBC.CarrierContractNumber.ImportableValue, !resultBR.CarrierQuoteNumber.IsEmpty ? resultBR.CarrierQuoteNumber : resultBR.CarrierContractNumber.ImportableValue);
			AddWarningMessage(resultBC.ContractNamedAccountNumberInfo, resultBC.ContractNamedAccountNumber, resultBR.ContractNamedAccountNumber);
			AddWarningMessageForUnloco(resultBC.PortOfLoad.CodeInfo, resultBC.PortOfLoad.Code, resultBR.PortOfLoad.Code);
			AddWarningMessageForUnloco(resultBC.PortOfDischarge.CodeInfo, resultBC.PortOfDischarge.Code, resultBR.PortOfDischarge.Code);
			AddWarningMessageForUnloco(resultBC.PlaceOfReceipt.CodeInfo, resultBC.PlaceOfReceipt.Code, resultBR.PlaceOfReceipt.Code);
			AddWarningMessageForUnloco(resultBC.PlaceOfDelivery.CodeInfo, resultBC.PlaceOfDelivery.Code, resultBR.PlaceOfDelivery.Code);
			AddWarningMessageForUnloco(resultBC.Origin.CodeInfo, resultBC.Origin.Code, resultBR.Origin.Code);
			AddWarningMessageForUnloco(resultBC.Destination.CodeInfo, resultBC.Destination.Code, resultBR.Destination.Code);
			AddWarningMessage(resultBC.EarliestDepartureDateInfo, resultBC.EarliestDepartureDate, resultBR.EarliestDepartureDate);
			AddWarningMessage(resultBC.LatestDeliveryDateInfo, resultBC.LatestDeliveryDate, resultBR.LatestDeliveryDate);
		}

		void AddWarningMessageForTransportsProperties(IReadOnlyCollection<Transport> transportsBC, IReadOnlyCollection<Transport> transportsBR)
		{
			if (transportsBC.Any())
			{
				foreach (var transportBC in transportsBC)
				{
					var transportBR = transportsBR.FirstOrDefault(t => t.LegOrder == transportBC.LegOrder);
					if (transportBR != null)
					{
						AddWarningMessage(((CodeDescription)transportBC.Mode).CodeInfo, transportBC.Mode.Code, transportBR.Mode.Code);
						var typeCode = transportBC.Type.Code;
						AddWarningMessage(((CodeDescription)transportBC.Type).CodeInfo, typeCode, transportBR.Type.Code);
						AddWarningMessage(transportBC.Vessel.NameInfo, transportBC.Vessel.Name, transportBR.Vessel.Name);
						AddWarningMessage(transportBC.VoyageFlightNumberInfo, transportBC.VoyageFlightNumber, transportBR.VoyageFlightNumber);
						AddWarningMessageForUnloco(transportBC.PortOfLoading.CodeInfo, transportBC.PortOfLoading.Code, transportBR.PortOfLoading.Code);
						AddWarningMessageForUnloco(transportBC.PortOfDischarge.CodeInfo, transportBC.PortOfDischarge.Code, transportBR.PortOfDischarge.Code);
						AddWarningMessage(transportBC.ETDInfo, transportBC.ETD, transportBR.ETD);
						AddWarningMessage(transportBC.ETAInfo, transportBC.ETA, transportBR.ETA);

						if (typeCode == Constants.TransportModes.Mail)
						{
							AddWarningMessage(transportBC.VGMCutOffInfo, transportBC.VGMCutOff, transportBR.VGMCutOff);
							AddWarningMessage(transportBC.DocumentCutOffInfo, transportBC.DocumentCutOff, transportBR.DocumentCutOff);
							AddWarningMessage(transportBC.FCLCutOffInfo, transportBC.FCLCutOff, transportBR.FCLCutOff);
						}
						transportBC.ValidateAllIncludingChildren();
					}
				}
			}
		}

		void AddWarningMessageForContainersProperties(IReadOnlyCollection<ImportableWrapper<Container>> containersBC, IReadOnlyCollection<ImportableWrapper<Container>> containersBR)
		{
			if (containersBC != null && containersBC.Any())
			{
				foreach (var containerBC in containersBC)
				{
					var containerBR = containersBR?.FirstOrDefault(t => t.ImportableValue?.Number == containerBC.ImportableValue?.Number);
					if (containerBR != null && containerBR.ImportableValue != null && containerBC.ImportableValue != null)
					{
						var containerBRValue = containerBR.ImportableValue;
						var containerBCValue = containerBC.ImportableValue;

						AddWarningMessage(containerBCValue.ContainerCountInfo, containerBCValue.ContainerCount, containerBRValue.ContainerCount);
						AddWarningMessage(containerBCValue.Type.CodeInfo, containerBCValue.Type.Code, containerBRValue.Type.Code);
						AddWarningMessage(containerBCValue.PackCountInfo, containerBCValue.PackCount, containerBRValue.PackCount);
						AddWarningMessage(((Measurement)containerBCValue.NetWeight).ValueInfo, containerBCValue.NetWeight.Value, containerBRValue.NetWeight.Value);
						AddWarningMessage(((Measurement)containerBCValue.NetWeight).ValueInfo, containerBCValue.NetWeight.Value, containerBRValue.NetWeight.Value);
						AddWarningMessage(containerBCValue.TareWeight.ValueInfo, containerBCValue.TareWeight.Value, containerBRValue.TareWeight.Value);
						AddWarningMessage(((Measurement)containerBCValue.GrossWeight).ValueInfo, containerBCValue.GrossWeight.Value, containerBRValue.GrossWeight.Value);
						AddWarningMessage(((Measurement)containerBCValue.Volume).ValueInfo, containerBCValue.Volume.Value, containerBRValue.Volume.Value);
						AddWarningMessage(((Address)containerBCValue.DepartureContainerYard).CompanyNameInfo, containerBCValue.DepartureContainerYard.CompanyName, containerBRValue.DepartureContainerYard.CompanyName);
						AddWarningMessage(containerBCValue.EmptyRequiredInfo, containerBCValue.EmptyRequired, containerBRValue.EmptyRequired);

						var addressBR = !containerBRValue.DepartureContainerYard.AddressLine1.IsEmpty ? containerBRValue.DepartureContainerYard.AddressLine1 : containerBRValue.DepartureContainerYard.AddressLine2;
						var addressBC = !containerBCValue.DepartureContainerYard.AddressLine1.IsEmpty ? containerBCValue.DepartureContainerYard.AddressLine1 : containerBCValue.DepartureContainerYard.AddressLine2;
						AddWarningMessage(((Address)containerBCValue.DepartureContainerYard).AddressLine1Info, addressBC, addressBR);
						AddWarningMessage(((Address)containerBCValue.DepartureContainerYard).AddressLine2Info, addressBC, addressBR);

						containerBCValue.ValidateAllIncludingChildren();
					}
				}
			}
		}

		void CompareAndAddWarningMessage(BookingConfirmation resultBC, BookingConfirmation resultBR)
		{
			AddWarningMessageForGeneralProperties(resultBC, resultBR);
			AddWarningMessageForTransportsProperties(resultBC.Transports.ImportableValue, resultBR.Transports.ImportableValue);
			AddWarningMessageForContainersProperties(resultBC.Containers, resultBR.Containers);
		}

		void PopulateAddressesAndDependentProperties(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			bookingConfirmation.Shipper = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsignorDocumentaryAddress)));

			var shippingLineAddress = FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ShippingLineAddress));
			bookingConfirmation.Carrier = AddressBuilder.Create(context, shippingLineAddress);
			bookingConfirmation.CarrierBookingOffice = UnlocoExtensions.CreateFromUNLOCO(context, shippingLineAddress?.Port, true);

			bookingConfirmation.Consignee = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsigneeDocumentaryAddress)));

			bookingConfirmation.NotifyParty = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.NotifyParty)));

			var containerTerminalOperatorAddress = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.DepartureCTOAddress)));
			bookingConfirmation.ContainerTerminalOperator = new ImportableWrapper<Address>(containerTerminalOperatorAddress);

			bookingConfirmation.PickUpFrom = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsignorPickupDeliveryAddress)));

			bookingConfirmation.DeliverTo = AddressBuilder.Create(context,
				FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ConsigneePickupDeliveryAddress)));
		}

		OrganizationAddress FindOrReturnDefaultAddress(UniversalShipment universalShipment, string addressType)
		{
			return universalShipment
				?.OrganizationAddressCollection
				?.FirstOrDefault(address => addressType.Equals(address.AddressType, StringComparison.OrdinalIgnoreCase));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		void PopulateNotes(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			if (universalShipment?.NoteCollection != null)
			{
				const string bookingConfirmationNotesDescription = "Booking Confirmation Notes";
				foreach (var note in universalShipment?.NoteCollection)
				{
					if (bookingConfirmationNotesDescription.Equals(note.Description, StringComparison.OrdinalIgnoreCase))
					{
						bookingConfirmation.CarrierConfirmationNotes = new ImportableZTypeWrapper<ZString>(note.NoteText.GetValueOrDefault())
						{
							ShouldImportObject = true
						};
						break;
					}
				}
			}
		}

		void PopulateNumbers(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			ZString? carrierBookingReference;
			ZString? billOfLadingNumber;
			if (IsColoadShipment(universalShipment))
			{
				carrierBookingReference = universalShipment?.CoLoadBookingConfirmationReference;
				billOfLadingNumber = universalShipment?.CoLoadMasterBillNumber;
			}
			else
			{
				carrierBookingReference = universalShipment?.BookingConfirmationReference;
				billOfLadingNumber = universalShipment?.WayBillNumber;
			}
			bookingConfirmation.CarrierBookingReference = new ImportableZTypeWrapper<ZString>(carrierBookingReference.GetValueOrDefault())
			{
				ShouldImportObject = true
			};
			bookingConfirmation.BillOfLadingNumber = new ImportableZTypeWrapper<ZString>(billOfLadingNumber.GetValueOrDefault())
			{
				ShouldImportObject = true
			};

			bookingConfirmation.VesselName = (universalShipment?.VesselName).GetValueOrDefault();
			bookingConfirmation.LloydsIMO = (universalShipment?.LloydsIMO).GetValueOrDefault();
			bookingConfirmation.VoyageNumber = (universalShipment?.VoyageFlightNo).GetValueOrDefault();
			bookingConfirmation.ShipperReferenceNumber = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.ShipperReference);
			bookingConfirmation.FreightForwarderReferenceNumber = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.FreightForwarderReference);
			bookingConfirmation.CarrierContractNumber = new ImportableZTypeWrapper<ZString>(FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.CarrierContractNumber));
			bookingConfirmation.CarrierQuoteNumber = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.CarrierQuoteNumber);
			bookingConfirmation.ContractNamedAccountNumber = FindAdditionalReferenceNumber(universalShipment, AdditionalReferenceCodes.ContractNamedAccount);
		}

		bool IsColoadShipment(UniversalShipment universalShipment)
			=> universalShipment?.ShipmentType?.Code is ZString shipmentType && shipmentType == Constants.AgentType.CoLoad;

		ZString FindAdditionalReferenceNumber(UniversalShipment universalShipment, string additionalReferenceTypeCode)
		{
			if (universalShipment?.AdditionalReferenceCollection != null)
			{
				foreach (var additionalReference in universalShipment?.AdditionalReferenceCollection)
				{
					if (additionalReference.Type is EntryType entryType
						&& entryType.Code is ZString typeCode
						&& string.Equals(typeCode, additionalReferenceTypeCode, StringComparison.OrdinalIgnoreCase))
					{
						return additionalReference.ReferenceNumber ?? ZString.Empty;
					}
				}
			}

			return ZString.Empty;
		}

		void PopulateTransportsAndDependentProperties(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			var transportsOrdered = universalShipment?.TransportLegCollection?.OrderBy(leg => leg.LegOrder);
			var firstTransport = transportsOrdered?.FirstOrDefault();
			var lastTransport = transportsOrdered?.LastOrDefault();

			bookingConfirmation.Transports = new ImportableWrapper<IReadOnlyCollection<Transport>>(CreateTransports(transportsOrdered))
			{
				ShouldImportObject = true
			};

			bookingConfirmation.Origin = UnlocoExtensions.CreateFromUNLOCO(context, firstTransport?.PortOfLoading);
			bookingConfirmation.Destination = UnlocoExtensions.CreateFromUNLOCO(context, lastTransport?.PortOfDischarge);

			bookingConfirmation.EarliestDepartureDate = firstTransport?.EstimatedDeparture ?? ZDateTime.Empty;
			bookingConfirmation.LatestDeliveryDate = lastTransport?.EstimatedArrival ?? ZDateTime.Empty;
		}

		IReadOnlyCollection<Transport> CreateTransports(IEnumerable<TransportLeg> transportLegs)
		{
			var result = new List<Transport>();

			if (transportLegs != null)
			{
				foreach (var transportLeg in transportLegs)
				{
					result.Add(Transport.Create(context, transportLeg));
				}
			}

			return result;
		}

		void PopulateUnlocos(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			var shippingLineAddress = FindOrReturnDefaultAddress(universalShipment, nameof(DocAddressType.ShippingLineAddress));
			bookingConfirmation.CarrierBookingOffice = UnlocoExtensions.CreateFromUNLOCO(context, shippingLineAddress?.Port, true);

			bookingConfirmation.PlaceOfReceipt = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PlaceOfReceipt);
			bookingConfirmation.PlaceOfDelivery = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PlaceOfDelivery);

			bookingConfirmation.PortOfLoad = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PortOfLoading);
			bookingConfirmation.PortOfDischarge = UnlocoExtensions.CreateFromUNLOCO(context, universalShipment?.PortOfDischarge);
		}

		void PopulateCodeDescriptions(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			bookingConfirmation.ShipmentType = new CodeDescription(consol.JK_AgentType_List)
			{
				Code = universalShipment?.ShipmentType?.Code ?? ZString.Empty
			};

			bookingConfirmation.ContainerMode = new CodeDescription(consol.JK_ConsolMode_List)
			{
				Code = universalShipment?.ContainerMode?.Code ?? ZString.Empty
			};
		}

		void PopulateIsDoorProperties(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			if (universalShipment?.ContainerCollection != null)
			{
				var cfs = "CFS"; // programmatic constant
				bookingConfirmation.IsDoorPickup = universalShipment
					?.ContainerCollection
					?.Any(container => container.DeliveryMode.GetValueOrDefault().StartsWith(cfs, StringComparison.OrdinalIgnoreCase))
					?? false;

				bookingConfirmation.IsDoorDelivery = universalShipment
					?.ContainerCollection
					?.Any(container => container.DeliveryMode.GetValueOrDefault().EndsWith(cfs, StringComparison.OrdinalIgnoreCase))
					?? false;
			}
		}

		void PopulateContainers(BookingConfirmation bookingConfirmation, UniversalShipment universalShipment)
		{
			if (universalShipment?.ContainerCollection != null)
			{
				var result = new List<ImportableWrapper<Container>>();
				var packingLinesGrouped = GetPackingLinesGrouped(universalShipment);

				if (packingLinesGrouped != null)
				{
					foreach (var universalContainer in universalShipment.ContainerCollection)
					{
						var containerNumber = universalContainer.ContainerNumber ?? ZString.Empty;
						List<UniversalPackingLine> packingLines = null;
						if (!containerNumber.IsEmpty)
						{
							packingLinesGrouped.TryGetValue(containerNumber, out packingLines);
						}
						if (CreateContainerFromUniversalContainer(universalContainer, packingLines) is Container container)
						{
							result.Add(new ImportableWrapper<Container>(container));
						}
					}
					bookingConfirmation.Containers = result;
				}
			}
		}

		Dictionary<ZString, List<UniversalPackingLine>> GetPackingLinesGrouped(UniversalShipment universalShipment)
		{
			var result = new Dictionary<ZString, List<UniversalPackingLine>>();
			if (universalShipment?.PackingLineCollection != null)
			{
				foreach (var packingLine in universalShipment.PackingLineCollection)
				{
					if (packingLine.ContainerNumber is ZString containerNumber)
					{
						if (result.ContainsKey(containerNumber))
						{
							result[containerNumber].Add(packingLine);
						}
						else
						{
							result[containerNumber] = new List<UniversalPackingLine>() { packingLine };
						}
					}
				}
			}

			return result;
		}

		Container CreateContainerFromUniversalContainer(UniversalContainer universalContainer, IReadOnlyCollection<UniversalPackingLine> packingLines)
		{
			if (universalContainer == null)
			{
				return null;
			}

			var result = new Container()
			{
				Number = universalContainer.ContainerNumber.GetValueOrDefault(),
				ContainerCount = universalContainer.ContainerCount.GetValueOrDefault(),
				PackCount = packingLines?.Count ?? 0
			};

			result.Type = new ContainerType(context.ContainerTypes as IFindBoxListProvider)
			{
				Code = universalContainer.ContainerType?.Code.GetValueOrDefault() ?? "",
				ISOCode = universalContainer.ContainerType?.ISOCode.GetValueOrDefault() ?? ""
			};

			var newWeightUnit = Constants.Weight.Kilograms;
			var oldWeightUnit = universalContainer.WeightUnit?.Code ?? newWeightUnit;

			var netWeight = universalContainer.GrossWeight.GetValueOrDefault() - universalContainer.TareWeight.GetValueOrDefault();
			result.NetWeight = GetNewWeightMeasurement(netWeight, oldWeightUnit, newWeightUnit);
			result.TareWeight = GetNewWeightMeasurement(universalContainer.TareWeight.GetValueOrDefault(), oldWeightUnit, newWeightUnit);
			result.GrossWeight = GetNewWeightMeasurement(universalContainer.GrossWeight.GetValueOrDefault(), oldWeightUnit, newWeightUnit);

			var containerVolume = 0m;
			var newVolumeUnit = Constants.Volume.CubicMetres;

			if (packingLines != null)
			{
				foreach (var packingLine in packingLines)
				{
					containerVolume += Constants.Volume.Convert(
						packingLine.Volume.GetValueOrDefault(),
						packingLine.VolumeUnit?.Code ?? newVolumeUnit,
						newVolumeUnit);
				}
			}

			result.Volume = new Measurement()
			{
				Value = containerVolume,
				Unit = new CodeDescription(context.VolumeUnits)
				{
					Code = newVolumeUnit
				}
			};

			var containerYardAddressType = nameof(DocAddressType.ContainerYardEmptyPickupAddress);
			var containerYardAddress = universalContainer
				.OrganizationAddressCollection?
				.FirstOrDefault(address => containerYardAddressType.Equals(address.AddressType, StringComparison.OrdinalIgnoreCase));
			result.DepartureContainerYard = AddressBuilder.Create(context, containerYardAddress);

			result.EmptyRequired = universalContainer.EmptyRequired.GetValueOrDefault();

			return result;
		}

		Measurement GetNewWeightMeasurement(decimal value, string oldUnitCode, string newUnitCode)
		{
			var convertedValue = Constants.Weight.Convert(value, oldUnitCode, newUnitCode);
			return new Measurement()
			{
				Value = convertedValue,
				Unit = new CodeDescription(context.WeightUnits)
				{
					Code = newUnitCode
				}
			};
		}
	}
}
