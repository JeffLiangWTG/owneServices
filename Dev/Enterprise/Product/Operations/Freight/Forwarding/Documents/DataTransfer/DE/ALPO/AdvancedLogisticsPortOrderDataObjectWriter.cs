using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.DE
{
	sealed class AdvancedLogisticsPortOrderDataObjectWriter : DataObjectWriter<AdvancedLogisticsPortOrder, UniversalShipment>
	{
		public AdvancedLogisticsPortOrderDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = advancedLogisticsPortOrder.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			shipment.BookingConfirmationReference = advancedLogisticsPortOrder.CarrierBookingReference;
			shipment.ContainerMode = advancedLogisticsPortOrder.ContainerMode?.ToUXmlContainerMode();
			shipment.LloydsIMO = advancedLogisticsPortOrder.Vessel?.LloydsIMO;
			shipment.PortOfDestination = advancedLogisticsPortOrder.PortOfDestination?.ToUXmlUnloco();
			shipment.PortOfDischarge = advancedLogisticsPortOrder.PortOfDischarge?.ToUXmlUnloco();
			shipment.PortOfLoading = advancedLogisticsPortOrder.PortOfOrigin?.ToUXmlUnloco();
			shipment.PortOfOrigin = advancedLogisticsPortOrder.PortOfOrigin?.ToUXmlUnloco();
			shipment.VesselName = advancedLogisticsPortOrder.Vessel?.Name;
			shipment.VoyageFlightNo = advancedLogisticsPortOrder.VoyageFlightNo;
			shipment.WayBillNumber = advancedLogisticsPortOrder.BillOfLading;

			PopulateAddresses(advancedLogisticsPortOrder, shipment);
			PopulateContainers(advancedLogisticsPortOrder, shipment);
			PopulateAddInfos(advancedLogisticsPortOrder, shipment);
			PopulateAdditionalReferences(advancedLogisticsPortOrder, shipment);
			PopulateDates(advancedLogisticsPortOrder, shipment);
			PopulateShipments(advancedLogisticsPortOrder, shipment);

			return shipment;
		}

		#region PopulateAddresses

		void PopulateAddresses(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!advancedLogisticsPortOrder.CTO.IsEmpty())
				{
					addresses.Add(advancedLogisticsPortOrder.CTO.ToUXmlOrganizationAddress(nameof(DocAddressType.Warehouse), writeManager.WriterStrategy, CreateRegistrationNumbers(advancedLogisticsPortOrder.CTOWarehouseCode)));
				}

				if (!advancedLogisticsPortOrder.Forwarder.IsEmpty())
				{
					addresses.Add(advancedLogisticsPortOrder.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(advancedLogisticsPortOrder.EoriNumber, advancedLogisticsPortOrder.EoriBranchSuffix)));
				}

				if (!advancedLogisticsPortOrder.Carrier.IsEmpty())
				{
					addresses.Add(advancedLogisticsPortOrder.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(advancedLogisticsPortOrder.CarrierCode)));
				}

				if (!advancedLogisticsPortOrder.SendingParty.IsEmpty())
				{
					addresses.Add(advancedLogisticsPortOrder.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers()));
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

		#endregion

		#region Populate Shipments

		void PopulateShipments(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment shipment)
		{
			var uxmlSubShipments = new DataObjectList<UniversalShipment>();

			foreach (var shipmentDO in advancedLogisticsPortOrder.Shipments)
			{
				var uxmlSubShipment = shipmentDO.ToUXmlShipment(PackingLineContainerLinkMap, writeManager.WriterStrategy);

				uxmlSubShipments.Add(uxmlSubShipment);
			}

			shipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? uxmlSubShipments : null);
		}

		Dictionary<ZGuid, int> PackingLineContainerLinkMap;

		#endregion

		#region Populate Containers

		void PopulateContainers(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment shipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var subShipments = new List<UniversalShipment>();
			PackingLineContainerLinkMap = new Dictionary<ZGuid, int>();

			if (advancedLogisticsPortOrder.Containers != null)
			{
				foreach (var container in advancedLogisticsPortOrder.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					if (container.PackingLines != null)
					{
						foreach (var packingLine in container.PackingLines)
						{
							PackingLineContainerLinkMap.Add((ZGuid)packingLine.Identifier, containerLink);
						}
					}

					uxmlContainers.Add(uxmlContainer);
				}
			}

			shipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				if (!advancedLogisticsPortOrder.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(advancedLogisticsPortOrder.OperationalPort.ToUXmlAddInfos(nameof(advancedLogisticsPortOrder.OperationalPort)));
				}

				if (!advancedLogisticsPortOrder.ALPOReference.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = DocDataConstants.AddinfoTypes.ALPOReference,
						Value = advancedLogisticsPortOrder.ALPOReference
					});
				}

				if (!advancedLogisticsPortOrder.ALPOUserID.IsEmpty)
				{
					addInfos.Add(new AddInfo
					{
						Key = DocDataConstants.AddinfoTypes.ALPOUserID,
						Value = advancedLogisticsPortOrder.ALPOUserID
					});
				}

				AddAddInfo(addInfos, nameof(advancedLogisticsPortOrder.Direction), advancedLogisticsPortOrder.Direction);
				AddAddInfo(addInfos, nameof(advancedLogisticsPortOrder.MarksAndNumbers), advancedLogisticsPortOrder.MarksAndNumbers);
				AddAddInfo(addInfos, "Other_TransportMode", advancedLogisticsPortOrder.TransportModePreCarriageOrOnForwarding.Code);
				AddAddInfo(addInfos, "Other_TransportID", advancedLogisticsPortOrder.PreCarriageOrOnForwardingID);
				AddAddInfo(addInfos, AdvancedLogisticsPortOrder.SisNumberReferenceUXmlName, advancedLogisticsPortOrder.SisNumber);
				AddAddInfo(addInfos, "FormVersion", "2.0.0");

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

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(advancedLogisticsPortOrder));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			if (!advancedLogisticsPortOrder.CarrierBookingReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = advancedLogisticsPortOrder.CarrierBookingReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		#endregion

		#region PopulateDates

		void PopulateDates(AdvancedLogisticsPortOrder advancedLogisticsPortOrder, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetDateCollection(() =>
			{
				var dates = CreateDates(advancedLogisticsPortOrder).ToList();
				return dates.Any() ? dates : null;
			});
		}

		IEnumerable<Date> CreateDates(AdvancedLogisticsPortOrder advancedLogisticsPortOrder)
		{
			if (!advancedLogisticsPortOrder.ETD.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Departure,
					Value = advancedLogisticsPortOrder.ETD
				};
			}

			if (!advancedLogisticsPortOrder.ETA.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = advancedLogisticsPortOrder.ETA
				};
			}
		}

		#endregion
	}
}
