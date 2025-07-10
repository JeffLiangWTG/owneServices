using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class ContainerAdviceToBookingWriter : DataObjectWriter<ContainerAdviceToBooking, UniversalShipment>
	{
		public ContainerAdviceToBookingWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ContainerAdviceToBooking containerAdviceToBooking)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = containerAdviceToBooking.CreateUXmlDataContext();

			universalShipment.ContainerMode = containerAdviceToBooking.ContainerMode?.ToUXmlContainerMode();
			universalShipment.ShipmentType = containerAdviceToBooking.ShipmentType?.ToUXmlCodeDescriptionPair();
			universalShipment.WayBillNumber = containerAdviceToBooking.BillOfLading;

			universalShipment.PortOfDestination = containerAdviceToBooking.PortOfDestination.ToUXmlUnloco();
			universalShipment.PortOfOrigin = containerAdviceToBooking.PortOfOrigin.ToUXmlUnloco();
			universalShipment.PortFirstForeign = containerAdviceToBooking.PortOfTranshipment.ToUXmlUnloco();

			universalShipment.BookingConfirmationReference = containerAdviceToBooking.CarrierBookingReference;
			universalShipment.RequiresTemperatureControl = containerAdviceToBooking.RequiresTemperatureControl;
			universalShipment.RequiredTemperatureMinimum = containerAdviceToBooking.TemperatureMinimum?.Value;
			universalShipment.RequiredTemperatureMaximum = containerAdviceToBooking.TemperatureMaximum?.Value;
			universalShipment.VoyageFlightNo = containerAdviceToBooking.VoyageNumber;

			var requiredTemperatureUnit = containerAdviceToBooking.TemperatureMinimum?.Unit;
			universalShipment.RequiredTemperatureUnit = requiredTemperatureUnit == null
				? null
				: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };

			PopulateAddresses(containerAdviceToBooking, universalShipment);
			PopulateContainers(containerAdviceToBooking, universalShipment);
			PopulateAddInfos(containerAdviceToBooking, universalShipment);
			PopulateAdditionalReferences(containerAdviceToBooking, universalShipment);

			return universalShipment;
		}

		void PopulateAddresses(ContainerAdviceToBooking containerAdviceToBooking, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!containerAdviceToBooking.SendingForwarder.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.SendingForwarderCI5, containerAdviceToBooking.SendingForwarderSON)));
				}

				if (!containerAdviceToBooking.ReceivingForwarder.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.ReceivingForwarderCI5, containerAdviceToBooking.ReceivingForwarderSON)));
				}

				if (!containerAdviceToBooking.Carrier.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.CarrierCI5, containerAdviceToBooking.CarrierSON)));
				}

				if (!containerAdviceToBooking.CarrierBookingAgent.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.CarrierBookingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.CarrierBookingAgent), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.CarrierBookingAgentCI5, containerAdviceToBooking.CarrierBookingAgentSON)));
				}

				if (!containerAdviceToBooking.CTO.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.CTO.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.CTOCI5, containerAdviceToBooking.CTOSON)));
				}

				if (!containerAdviceToBooking.Transporter.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.Transporter.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSLocalTransportAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.TransporterCI5, containerAdviceToBooking.TransporterSON)));
				}

				if (!containerAdviceToBooking.SendingParty.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.SendingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.SendingPartyCI5, containerAdviceToBooking.SendingPartySOA, containerAdviceToBooking.SendingPartySON, containerAdviceToBooking.SendingPartySOW)));
				}

				if (!containerAdviceToBooking.Forwarder.IsEmpty())
				{
					addresses.Add(containerAdviceToBooking.Forwarder.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(containerAdviceToBooking.ForwarderCI5, containerAdviceToBooking.ForwarderSOA, containerAdviceToBooking.ForwarderSON, containerAdviceToBooking.ForwarderSOW)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		void PopulateContainers(ContainerAdviceToBooking containerAdviceToBooking, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			if (containerAdviceToBooking.Containers != null)
			{
				foreach (var container in containerAdviceToBooking.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					foreach (BookingPackingLine packingLine in container.PackingLines)
					{
						var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
						uxmlPackingLine.ContainerLink = containerLink;

						uxmlPackingLines.Add(uxmlPackingLine);
					}

					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetPackingLineCollection(() => uxmlPackingLines.Any() ? uxmlPackingLines : null);
			uxmlShipment.SetContainerCollection(() => uxmlContainers.Any() ? uxmlContainers : null);
		}

		void PopulateAddInfos(ContainerAdviceToBooking containerAdviceToBooking, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				AddAddInfo(addInfos, nameof(containerAdviceToBooking.PortLocation), containerAdviceToBooking.PortLocation);
				AddAddInfo(addInfos, nameof(containerAdviceToBooking.PortArea), containerAdviceToBooking.PortArea);
				AddAddInfo(addInfos, nameof(containerAdviceToBooking.BookingConfirmationCBK), containerAdviceToBooking.BookingConfirmationCBK);
				AddAddInfo(addInfos, nameof(containerAdviceToBooking.OTC), containerAdviceToBooking.OTC);
				AddAddInfo(addInfos, nameof(containerAdviceToBooking.ATPReference), containerAdviceToBooking.ATPReference);
				AddAddInfo(addInfos, nameof(containerAdviceToBooking.OTCReference), containerAdviceToBooking.OTCReference);

				if (!containerAdviceToBooking.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(containerAdviceToBooking.OperationalPort.ToUXmlAddInfos(nameof(containerAdviceToBooking.OperationalPort)));
				}

				if (addInfos.Any())
				{
					addInfos.Add(new AddInfo { Key = DocDataConstants.AddinfoTypes.FormVersion, Value = "1.0.0" }); // Programmatic version number
				}

				return addInfos.Any() ? addInfos : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo
				{
					Key = key,
					Value = value
				});
			}
		}

		void PopulateAdditionalReferences(ContainerAdviceToBooking containerAdviceToBooking, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(containerAdviceToBooking));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(ContainerAdviceToBooking containerAdviceToBooking)
		{
			if (!containerAdviceToBooking.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = containerAdviceToBooking.ConsolNumber,
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
