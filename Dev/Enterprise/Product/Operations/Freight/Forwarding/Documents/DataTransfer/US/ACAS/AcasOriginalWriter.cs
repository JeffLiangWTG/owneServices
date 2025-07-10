using System.Collections.Generic;
using System.Globalization;
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
	sealed class AcasOriginalWriter : DataObjectWriter<AirCargoAdvanceScreening, UniversalShipment>
	{
		public AcasOriginalWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UniversalShipment PopulateDataObject(AirCargoAdvanceScreening acas)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = acas.CreateUXmlDataContext();

			shipment.VoyageFlightNo = acas.FlightNumber;
			shipment.TotalNoOfPacks = acas.NumberOfPacks;
			shipment.TotalWeight = acas.Weight?.Value;
			shipment.TotalWeightUnit = acas.Weight?.Unit?.ToUXmlUnitOfWeight();
			shipment.OuterPacks = acas.NumberOfPacks;
			shipment.GoodsDescription = acas.GoodsDescription;

			var dates = CreateDates(acas).ToList();
			shipment.SetDateCollection(() => dates.Any()
				? dates
				: null);

			var addresses = CreateAddresses(acas).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);

			var infos = CreateAddInfos(acas).ToList();
			shipment.SetAddInfoCollection(() => infos.Any()
				? infos
				: null);

			PopulatePorts(acas, shipment);

			return shipment;
		}

		void PopulatePorts(AirCargoAdvanceScreening acas, UniversalShipment shipment)
		{
			shipment.PortOfFirstArrival = acas.Arrival?.ToUXmlUnloco();
			shipment.PortOfOrigin = acas.Departure?.ToUXmlUnloco();
		}

		IEnumerable<OrganizationAddress> CreateAddresses(AirCargoAdvanceScreening acas)
		{
			if (!acas.Carrier.IsEmpty())
			{
				yield return acas.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.Carrier), writeManager.WriterStrategy);
			}

			if (!acas.CTO.IsEmpty())
			{
				yield return acas.CTO.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy);
			}

			if (!acas.Shipper.IsEmpty())
			{
				yield return acas.Shipper.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!acas.Consignee.IsEmpty())
			{
				yield return acas.Consignee.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!acas.NotifyParty.IsEmpty())
			{
				yield return acas.NotifyParty.ToUXmlOrganizationAddress(nameof(DocAddressType.NotifyParty), writeManager.WriterStrategy);
			}

			if (!acas.BookingParty.IsEmpty())
			{
				yield return acas.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}
		}

		IEnumerable<Date> CreateDates(AirCargoAdvanceScreening acas)
		{
			if (!acas.ETA.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = acas.ETA
				};
			}
		}

		IEnumerable<AddInfo> CreateAddInfos(AirCargoAdvanceScreening acas)
		{
			if (!acas.MAWB.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(acas.MAWB),
					Value = acas.MAWB
				};
			}

			if (!acas.NotifyPartyType.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(acas.NotifyPartyType), nameof(acas.NotifyPartyType.Code)),
					Value = acas.NotifyPartyType.Code
				};
			}

			if (!acas.PortOfOriginIata.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(acas.PortOfOriginIata),
					Value = acas.PortOfOriginIata
				};
			}

			if (!acas.PortOfFirstArrivalIata.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(acas.PortOfFirstArrivalIata),
					Value = acas.PortOfFirstArrivalIata
				};
			}

			if (!acas.ConsolType.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(acas.ConsolType), nameof(acas.ConsolType.Code)),
					Value = acas.ConsolType.Code
				};
			}

			if (!acas.Arrival.IsEmpty())
			{
				yield return acas.Arrival.ToUXmlAddInfos("OperationalPort").First();
			}
		}
	}
}
