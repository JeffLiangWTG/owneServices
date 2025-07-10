using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.US
{
	sealed class ACASHouseChecklistDataObjectWriter : DataObjectWriter<ACASHouseChecklist, UniversalShipment>
	{
		public ACASHouseChecklistDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ACASHouseChecklist acas)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);

			shipment.DataContext = acas.CreateUXmlDataContext();

			shipment.WayBillNumber = acas.WayBillNumber;

			var infos = CreateAddInfoCollection(acas).ToList();
			shipment.SetAddInfoCollection(() => infos.Any()
				? infos
				: null);

			var addresses = CreateAddresses(acas).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);
			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(acas));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			shipment.SetSubShipmentCollection(() =>
			{
				var subShipments = CreateSubShipments(acas).ToList();
				return subShipments.Any() ? new DataObjectList<UniversalShipment>(subShipments) : null;
			});

			PopulatePorts(acas, shipment);
			PopulateMeasures(acas, shipment);

			return shipment;
		}

		void PopulatePorts(ACASHouseChecklist acas, UniversalShipment shipment)
		{
			shipment.PortOfOrigin = acas.PortOfOrigin?.ToUXmlUnloco();
			shipment.PortOfFirstArrival = acas.PortOfFirstArrival?.ToUXmlUnloco();
			shipment.PortOfDestination = acas.PortOfDestination?.ToUXmlUnloco();
		}

		void PopulateMeasures(ACASHouseChecklist acas, UniversalShipment shipment)
		{
			shipment.TotalWeight = acas.Weight?.Value;
			shipment.TotalWeightUnit = acas.Weight?.Unit?.ToUXmlUnitOfWeight();

			shipment.TotalNoOfPacks = acas.TotalNoOfPacks;
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(ACASHouseChecklist acas)
		{
			if (!acas.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = acas.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(ACASHouseChecklist acas)
		{
			if (!acas.Carrier.IsEmpty())
			{
				yield return acas.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.Carrier), writeManager.WriterStrategy);
			}

			if (!acas.BookingParty.IsEmpty())
			{
				yield return acas.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}
		}

		IEnumerable<UniversalShipment> CreateSubShipments(ACASHouseChecklist acas)
		{
			foreach (var acasShipment in acas.Shipments)
			{
				var shipment = new UniversalShipment(writeManager.WriterStrategy);

				shipment.DataContext = acasShipment.CreateUXmlDataContext();

				shipment.WayBillNumber = acasShipment.WayBillNumber;
				shipment.GoodsDescription = acasShipment.GoodsDescription;
				shipment.OuterPacks = acasShipment.TotalNoOfPacks;
				shipment.TotalWeight = acasShipment.Weight?.Value;
				shipment.TotalWeightUnit = acas.Weight?.Unit?.ToUXmlUnitOfWeight();

				shipment.PortOfOrigin = acasShipment.PortOfOrigin?.ToUXmlUnloco();
				shipment.PortOfDestination = acasShipment.PortOfDestination?.ToUXmlUnloco();

				yield return shipment;
			}
		}

		IEnumerable<AddInfo> CreateAddInfoCollection(ACASHouseChecklist acas)
		{
			if (!acas.PortOfFirstArrival.IsEmpty())
			{
				yield return acas.PortOfFirstArrival.ToUXmlAddInfos("OperationalPort").First();
			}
		}
	}
}
