using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE
{
	sealed class DangerousGoodsNotificationObjectWriter : DataObjectWriter<DangerousGoodsNotification, UniversalShipment>
	{
		public DangerousGoodsNotificationObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(DangerousGoodsNotification dangerousGoodsNotification)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = dangerousGoodsNotification.CreateUXmlDataContext();

			shipment.ContainerMode = dangerousGoodsNotification.ContainerMode?.ToUXmlContainerMode();
			shipment.ShipmentType = dangerousGoodsNotification.ShipmentType?.ToUXmlCodeDescriptionPair();
			shipment.BookingConfirmationReference = dangerousGoodsNotification.CarrierBookingReference;
			shipment.WayBillNumber = dangerousGoodsNotification.BillOfLading;

			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = CreateAdditionalReferences(dangerousGoodsNotification).ToArray();

				return additionalReferences.Length > 0
					? new DataObjectList<AdditionalReference>(additionalReferences)
					: null;
			});

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(dangerousGoodsNotification).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			shipment.SetDateCollection(() =>
			{
				var dates = CreateDates(dangerousGoodsNotification).ToList();
				return dates.Any() ? dates : null;
			});

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(dangerousGoodsNotification).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			PopulateContainersAndPackingLines(dangerousGoodsNotification, shipment);

			PopulateTransportLegs(dangerousGoodsNotification, shipment);

			return shipment;
		}

		#region PopulateTransportLegs

		void PopulateTransportLegs(DangerousGoodsNotification dangerousGoodsNotification, UniversalShipment uxmlShipment)
		{
			var uxmlTransportLegs = new DataObjectList<TransportLeg>();

			if (dangerousGoodsNotification.Transports != null)
			{
				foreach (var transportleg in dangerousGoodsNotification.Transports)
				{
					var uxmlTransportLeg = transportleg.ToUXmlTransportLeg(writeManager.WriterStrategy);

					uxmlTransportLegs.Add(uxmlTransportLeg);
				}
			}

			uxmlShipment.SetTransportLegCollection(() => uxmlTransportLegs);
		}

		#endregion PopulateTransportLegs

		#region PopulateContainersAndPackingLines

		void PopulateContainersAndPackingLines(DangerousGoodsNotification dangerousGoodsNotification, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			var uxmlPackingLines = new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			if (dangerousGoodsNotification.Containers != null)
			{
				foreach (var container in dangerousGoodsNotification.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					foreach (var packingLine in container.PackingLines)
					{
						var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
						uxmlPackingLine.ContainerLink = uxmlContainer.Link;

						uxmlPackingLines.Add(uxmlPackingLine);
					}

					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetPackingLineCollection(() => uxmlPackingLines);
			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion PopulateContainersAndPackingLines

		IEnumerable<AdditionalReference> CreateAdditionalReferences(DangerousGoodsNotification dangerousGoodsNotification)
		{
			if (!dangerousGoodsNotification.CarrierBookingReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = dangerousGoodsNotification.CarrierBookingReference,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(DangerousGoodsNotification dangerousGoodsNotification)
		{
			if (!dangerousGoodsNotification.DgnSecurityNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(dangerousGoodsNotification.DgnSecurityNumber),
					Value = dangerousGoodsNotification.DgnSecurityNumber
				};
			}

			if (!dangerousGoodsNotification.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in dangerousGoodsNotification.OperationalPort.ToUXmlAddInfos(nameof(dangerousGoodsNotification.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			if (!dangerousGoodsNotification.VesselStayReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(dangerousGoodsNotification.VesselStayReference),
					Value = dangerousGoodsNotification.VesselStayReference
				};
			}

			if (!dangerousGoodsNotification.HandlingInstruction.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(dangerousGoodsNotification.HandlingInstruction),
					Value = dangerousGoodsNotification.HandlingInstruction
				};
			}

			var mainVessel = dangerousGoodsNotification.Transports?.Main?.Vessel;
			if (mainVessel != null)
			{
				if (!mainVessel.RadioCallSign.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = "Main_Vessel_RadioCallSign",
						Value = mainVessel.RadioCallSign
					};
				}
				if (!mainVessel.CountryOfRegistration.Code.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = "Main_Vessel_CountryCode",
						Value = mainVessel.CountryOfRegistration.Code
					};
				}
				if (!mainVessel.Type.Code.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = "Main_VesselType_Code",
						Value = mainVessel.Type.Code
					};
				}
			}

			if (dangerousGoodsNotification.PreOrOnTransportMode != null)
			{
				if (!dangerousGoodsNotification.PreOrOnTransportMode.Code.IsEmpty)
				{
					yield return new AddInfo
					{
						Key = "Other_TransportMode",
						Value = dangerousGoodsNotification.PreOrOnTransportMode.Code
					};
				}
			}
			if (!dangerousGoodsNotification.PreOrOnVesselName.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "Other_VesselName",
					Value = dangerousGoodsNotification.PreOrOnVesselName
				};
			}
			if (!dangerousGoodsNotification.PreOrOnVesselENINumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "Other_VesselENINumber",
					Value = dangerousGoodsNotification.PreOrOnVesselENINumber
				};
			}
		}

		IEnumerable<Date> CreateDates(DangerousGoodsNotification dangerousGoodsNotification)
		{
			if (!dangerousGoodsNotification.VesselStayStartDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.VesselStayStartDate,
					Value = dangerousGoodsNotification.VesselStayStartDate
				};
			}
			if (!dangerousGoodsNotification.VesselStayEndDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.VesselStayEndDate,
					Value = dangerousGoodsNotification.VesselStayEndDate
				};
			}
			if (!dangerousGoodsNotification.HandlingDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.HandlingDate,
					Value = dangerousGoodsNotification.HandlingDate
				};
			}
			if (!dangerousGoodsNotification.PickupDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Pickup,
					Value = dangerousGoodsNotification.PickupDate
				};
			}
			if (!dangerousGoodsNotification.DeliveryDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Delivery,
					Value = dangerousGoodsNotification.DeliveryDate
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(DangerousGoodsNotification dangerousGoodsNotification)
		{
			if (!dangerousGoodsNotification.ArrivalCTO.IsEmpty())
			{
				yield return dangerousGoodsNotification.ArrivalCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.ArrivalCTOTerminalId));
			}

			if (!dangerousGoodsNotification.DepartureCTO.IsEmpty())
			{
				yield return dangerousGoodsNotification.DepartureCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.DepartureCTOTerminalId));
			}

			if (!dangerousGoodsNotification.ReceivingForwarder.IsEmpty())
			{
				yield return dangerousGoodsNotification.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.ReceivingForwarderPortId));
			}

			if (!dangerousGoodsNotification.SendingForwarder.IsEmpty())
			{
				yield return dangerousGoodsNotification.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.SendingForwarderPortId));
			}

			if (!dangerousGoodsNotification.Carrier.IsEmpty())
			{
				yield return dangerousGoodsNotification.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.CarrierPortId));
			}

			if (!dangerousGoodsNotification.SendingParty.IsEmpty())
			{
				DocumentVisualizer.DocDataObjects.RegistrationNumber regNumber;
				if (!dangerousGoodsNotification.SendingPartyEori.Value.IsEmpty)
				{
					regNumber = dangerousGoodsNotification.SendingPartyEori;
				}
				else
				{
					regNumber = dangerousGoodsNotification.SendingPartyDuns;
				}
				yield return dangerousGoodsNotification.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(dangerousGoodsNotification.SendingPartyPortId, regNumber));
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}
	}
}
