using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.TransportBookings.Document;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	public sealed class FFMMessageDataObjectWriter : DataObjectWriter<FFMMessage, UniversalShipment>
	{
		public FFMMessageDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(FFMMessage ffmMessage)
		{
			var universalShipment = CreateUniversalShipment();

			universalShipment.PortOfDestination = new UNLOCO()
			{
				Code = ffmMessage.AirportOfDestinationCode.IATACode,
				Name = ffmMessage.AirportOfDestinationCode.Name,
			};

			universalShipment.PortOfLoading = new UNLOCO()
			{
				Code = ffmMessage.PortOfLoading.IATACode,
				Name = ffmMessage.PortOfLoading.Name
			};

			universalShipment.VoyageFlightNo = ffmMessage.VoyageFlightNo;

			universalShipment.CarrierDocumentsOverride = WrapRegulationDetailsInCarrierOverwrite(ffmMessage.RegulatedAgentID, ffmMessage.RegulatedAgentCountry, ffmMessage.SecurityStatusCode);

			universalShipment.SetDateCollection(() => WrapFlightDateInDateCollection(ffmMessage.FlightDate));
			universalShipment.SetSubShipmentCollection(() => CreateSubShipmentFromPackages(ffmMessage.Packages));
			universalShipment.SetInstructionCollection(() => WrapDriverDetailsInInstructions(ffmMessage.DriverDocumentID));

			return universalShipment;
		}

		UniversalShipment CreateUniversalShipment()
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);

			universalShipment.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext
			{
				DataSource = new UniversalDataBuss.DataObjects.Universal._2012_11.DataSource()
			};

			return universalShipment;
		}

		CarrierDocumentsOverride WrapRegulationDetailsInCarrierOverwrite(string regulatedAgentID, string regulatedAgentCountry, string securityStatusCode)
		{
			return new CarrierDocumentsOverride()
			{
				AWBHeader = new()
				{
					AgentName = regulatedAgentID,
					AgentPlace = regulatedAgentCountry,
					SpecialHandlingCode = securityStatusCode
				}
			};
		}

		List<Date> WrapFlightDateInDateCollection(ZDateTime departureDate)
		{
			List<Date> dates = new();

			Date flightDate = new()
			{
				IsEstimate = true,
				Type = DateType.Departure,
				Value = departureDate
			};

			dates.Add(flightDate);

			return dates;
		}

		DataObjectList<UniversalShipment> CreateSubShipmentFromPackages(IEnumerable<FFMMessagePackage> packages)
		{
			DataObjectList<UniversalShipment> subPackages = new();
			foreach (var package in packages)
			{
				var subShipment = new UniversalShipment(writeManager.WriterStrategy);
				subShipment.TotalNoOfPacks = package.Quantity;
				subShipment.TotalWeight = package.Weight;
				subShipment.TotalWeightUnit = new()
				{
					Code = package.WeightMetric
				};

				subShipment.TotalVolume = package.Volume;
				subShipment.TotalVolumeUnit = new()
				{
					Code = package.VolumeMetric
				};
				subShipment.GoodsDescription = package.GoodsDescription;

				subShipment.PortOfDestination = new UNLOCO()
				{
					Code = package.PortOfDestination.IATACode,
					Name = package.PortOfDestination.Name,
				};

				subShipment.PortOfLoading = new UNLOCO()
				{
					Code = package.PortOfLoading.IATACode,
					Name = package.PortOfLoading.Name
				};

				subShipment.WayBillNumber = package.WayBillNumber;

				subShipment.SetContainerCollection(() => new());
				subShipment.ContainerCollection.Add(new()
				{
					ContainerNumber = package.ContainerNumber
				});

				subPackages.Add(subShipment);
			}
			return subPackages;
		}

		DataObjectList<Instruction> WrapDriverDetailsInInstructions(string driverDocumentID)
		{
			DataObjectList<Instruction> instructions = new();

			Instruction instruction = new(writeManager.WriterStrategy);
			instruction.SetInstructionPackingLineLinkCollection(() => WrapDriverDetailsInPackageLineLink(driverDocumentID));

			instructions.Add(instruction);

			return instructions;
		}

		List<InstructionPackingLineLink> WrapDriverDetailsInPackageLineLink(string driverDocumentID)
		{
			List<InstructionPackingLineLink> packageLines = new();

			InstructionPackingLineLink packageLine = new();
			packageLine.ConfirmationCollection = WrapDriverDetailsInConfirmation(driverDocumentID);

			packageLines.Add(packageLine);

			return packageLines;
		}

		List<Confirmation> WrapDriverDetailsInConfirmation(string driverDocumentID)
		{
			List<Confirmation> confirmations = new();

			Confirmation confirmation = new()
			{
				DriverDocumentID = driverDocumentID,
			};
			confirmations.Add(confirmation);

			return confirmations;
		}
	}
}
