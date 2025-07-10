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
	sealed class AcasAcknowledgementWriter : DataObjectWriter<AirCargoAdvanceScreening, UniversalShipment>
	{
		public AcasAcknowledgementWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override UniversalShipment PopulateDataObject(AirCargoAdvanceScreening acas)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = acas.CreateUXmlDataContext();

			shipment.PortOfFirstArrival = acas.Arrival?.ToUXmlUnloco();

			var addInfos = CreateAddInfoCollection(acas).ToList();
			shipment.SetAddInfoCollection(() => addInfos.Any()
				? addInfos
				: null);

			var addresses = CreateAddresses(acas).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);

			return shipment;
		}

		IEnumerable<AddInfo> CreateAddInfoCollection(AirCargoAdvanceScreening acas)
		{
			if (!acas.MAWB.IsEmpty)
			{
				yield return new AddInfo()
				{
					Key = nameof(acas.MAWB),
					Value = acas.MAWB
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

			yield return new AddInfo()
			{
				Key = "AcknowledgementStatus",
				Value = "Z"
			};

			if (!acas.Arrival.IsEmpty())
			{
				yield return acas.Arrival.ToUXmlAddInfos("OperationalPort").First();
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(AirCargoAdvanceScreening acas)
		{
			if (!acas.BookingParty.IsEmpty())
			{
				yield return acas.BookingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}
		}
	}
}
