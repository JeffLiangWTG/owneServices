using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BR
{
	sealed class
		CargoControlAndTransitHouseManifestDataObjectWriter : DataObjectWriter<CargoControlAndTransitHouseManifest,
			UniversalShipment>
	{
		public CargoControlAndTransitHouseManifestDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = cctHouseManifest.CreateUXmlDataContext();

			universalShipment.WayBillNumber = cctHouseManifest.Mawb;

			universalShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(cctHouseManifest).ToList();
				return addresses.Count > 0 ? addresses : null;
			});

			universalShipment.PortOfDestination = new UNLOCO
			{
				Code = cctHouseManifest.AirportOfDestination.IATACode,
				Name = cctHouseManifest.AirportOfDestination.Name
			};

			universalShipment.PortOfFirstArrival = new UNLOCO
			{
				Code = cctHouseManifest.PortOfFirstArrival.Code,
				Name = cctHouseManifest.PortOfFirstArrival.Name
			};

			universalShipment.PortOfOrigin = new UNLOCO
			{
				Code = cctHouseManifest.PortOfOrigin.IATACode,
				Name = cctHouseManifest.PortOfOrigin.Name
			};

			universalShipment.TotalWeight = cctHouseManifest.Weight.Value;
			universalShipment.TotalWeightUnit = new UnitOfWeight()
			{
				Code = cctHouseManifest.Weight.Unit.Code,
				Description = cctHouseManifest.Weight.Unit.Description
			};

			universalShipment.TotalNoOfPacks = cctHouseManifest.Packs;

			universalShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(cctHouseManifest));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			universalShipment.SetSubShipmentCollection(() =>
			{
				var subShipments = CreateSubShipments(cctHouseManifest);
				return subShipments.Any() ? new DataObjectList<UniversalShipment>(subShipments) : null;
			});

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();
				addInfos.Add(new AddInfo
				{
					Key = "OperationalPort_Code", // XML Add Info key
					Value = cctHouseManifest.PortOfFirstArrival.Code
				});

				return addInfos;
			});

			return universalShipment;
		}

		IEnumerable<OrganizationAddress> CreateAddresses(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			if (!cctHouseManifest.SendingParty.IsEmpty())
			{
				var address = cctHouseManifest.SendingParty.ToUXmlOrganizationAddress(
					nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy);

				address.GovRegNum = cctHouseManifest.SendingParty.TaxNumber;

				yield return address;
			}

			if (!cctHouseManifest.ReceivingAgent.IsEmpty())
			{
				var address = cctHouseManifest.ReceivingAgent.ToUXmlOrganizationAddress(
					nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy);

				address.GovRegNum = cctHouseManifest.ReceivingAgent.TaxNumber;

				yield return address;
			}
		}

		IEnumerable<UniversalShipment> CreateSubShipments(CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			foreach (var cctShipment in cctHouseManifest.Shipments)
			{
				var shipment = new UniversalShipment(writeManager.WriterStrategy);

				shipment.DataContext = cctShipment.CreateUXmlDataContext();

				shipment.WayBillNumber = cctShipment.Hawb;
				shipment.PortOfOrigin = new UNLOCO();
				shipment.PortOfOrigin.Code = cctShipment.Origin?.IATACode;
				shipment.PortOfOrigin.Name = cctShipment.Origin?.Name;
				shipment.PortOfDestination = new UNLOCO();
				shipment.PortOfDestination.Code = cctShipment.Destination?.IATACode;
				shipment.PortOfDestination.Name = cctShipment.Destination?.Name;
				shipment.OuterPacks = cctShipment.Packs;
				shipment.TotalWeight = cctShipment.Weight?.Value;
				shipment.TotalWeightUnit = cctShipment.Weight?.Unit?.ToUXmlUnitOfWeight();
				shipment.MessageStatus = new CodeDescriptionPair();
				shipment.MessageStatus.Description = cctShipment.Status;
				var dates = CreateDates(cctShipment).ToList();
				shipment.SetDateCollection(() => dates.Any()
					? dates
					: null);
				shipment.GoodsDescription = cctShipment.GoodsDescription;

				yield return shipment;
			}
		}

		IEnumerable<Date> CreateDates(CargoControlAndTransitDetail detail)
		{
			if (!detail.EventDateTime.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = detail.EventDateTime
				};
			}
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(
			CargoControlAndTransitHouseManifest cctHouseManifest)
		{
			if (!cctHouseManifest.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = cctHouseManifest.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}
	}
}
