using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.CN
{
	sealed class ContainerLoadPlanDataObjectWriter : DataObjectWriter<ContainerLoadPlan, UniversalShipment>
	{
		public ContainerLoadPlanDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ContainerLoadPlan containerLoadPlan)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = containerLoadPlan.CreateUXmlDataContext();
			shipment.ShipmentType = containerLoadPlan.ShipmentType.ToUXmlCodeDescriptionPair();
			shipment.PortOfLoading = containerLoadPlan.PortOfLoading.ToUXmlUnloco();
			shipment.PortOfDischarge = containerLoadPlan.PortOfDischarge.ToUXmlUnloco();
			shipment.PlaceOfDelivery = containerLoadPlan.PlaceOfDelivery.ToUXmlUnloco();

			var portOfTranship = new List<AddInfo>();

			portOfTranship.Add(new AddInfo()
			{
				Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(containerLoadPlan.PortOfTranship), (NoResString)"Code"), // non-translatable xml constant
				Value = containerLoadPlan.TransitBerthCode
			});

			portOfTranship.Add(new AddInfo()
			{
				Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(containerLoadPlan.PortOfTranship), (NoResString)"Name"), // non-translatable xml constant
				Value = containerLoadPlan.PortOfTranship?.Name ?? string.Empty
			});

			shipment.SetAddInfoCollection(() =>
			{
				var addinfos = portOfTranship.Concat(containerLoadPlan.OperationalPort.ToUXmlAddInfos(nameof(containerLoadPlan.OperationalPort))).ToList();
				return addinfos.Any() ? addinfos : null;
			});

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(containerLoadPlan);
				return addresses.Any() ? addresses.ToList() : null;
			});

			shipment.VoyageFlightNo = containerLoadPlan.VoyageFlightNumber;
			shipment.LloydsIMO = containerLoadPlan.Vessel?.LloydsIMO;
			shipment.VesselName = containerLoadPlan.Vessel?.Name;

			shipment.ContainerMode = containerLoadPlan.ContainerMode != null ? new ContainerMode()
			{
				Code = containerLoadPlan.ContainerMode.Code,
				Description = containerLoadPlan.ContainerMode.Description
			} : null;

			PopulateBookings(containerLoadPlan, shipment);

			return shipment;
		}

		void PopulateBookings(ContainerLoadPlan containerLoadPlan, UniversalShipment shipment)
		{
			int seed = 0;
			var bookings = new List<UniversalShipment>();

			var uxmlContainers = containerLoadPlan
				.Containers
				.Select(x =>
				{
					var uContainer = x.ToUXmlContainer(writeManager.WriterStrategy);
					uContainer.Link = ++seed;

					foreach (var group in x.Groups ?? Enumerable.Empty<ContainerLoadPlanSOGrouping>())
					{
						var uGrouping = group.ToUXmlShipment(writeManager.WriterStrategy);

						if (uGrouping != null)
						{
							foreach (var l in uGrouping.PackingLineCollection)
							{
								l.ContainerLink = uContainer.Link;
							}

							bookings.Add(uGrouping);
						}
					}

					return uContainer;
				}).Where(x => x != null);

			shipment.SetContainerCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>(uxmlContainers));
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>(bookings));
		}

		IEnumerable<OrganizationAddress> CreateAddresses(ContainerLoadPlan containerLoadPlan)
		{
			if (!containerLoadPlan.SendingAgent.IsEmpty())
			{
				yield return containerLoadPlan.SendingAgent.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy);
			}

			if (!containerLoadPlan.CurrentUser.IsEmpty())
			{
				yield return containerLoadPlan.CurrentUser.ToUXmlOrganizationAddress(nameof(containerLoadPlan.CurrentUser), writeManager.WriterStrategy);
			}

			if (!containerLoadPlan.Carrier.IsEmpty())
			{
				yield return containerLoadPlan.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy);
			}

			if (!containerLoadPlan.DepartureCFSAddress.IsEmpty())
			{
				yield return containerLoadPlan.DepartureCFSAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSAddress), writeManager.WriterStrategy);
			}
		}
	}
}
