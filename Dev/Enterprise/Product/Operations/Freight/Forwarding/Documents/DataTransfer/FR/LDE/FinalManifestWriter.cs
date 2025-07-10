using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class FinalManifestWriter : DataObjectWriter<FinalManifest, UniversalShipment>
	{
		public FinalManifestWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(FinalManifest finalManifest)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = finalManifest.CreateUXmlDataContext();

			universalShipment.PortOfDestination = finalManifest.PortOfDestination.ToUXmlUnloco();
			universalShipment.PortOfOrigin = finalManifest.PortOfOrigin.ToUXmlUnloco();
			universalShipment.VesselName = finalManifest.Vessel;
			universalShipment.RequiredTemperatureMaximum = finalManifest.TemperatureMaximum?.Value;
			universalShipment.RequiredTemperatureMinimum = finalManifest.TemperatureMinimum?.Value;
			universalShipment.RequiresTemperatureControl = universalShipment.RequiredTemperatureMaximum != null;
			universalShipment.BookingConfirmationReference = finalManifest.CarrierBookingRef;

			universalShipment.ShipmentType = finalManifest.ShipmentType?.ToUXmlCodeDescriptionPair();
			universalShipment.VoyageFlightNo = finalManifest.VoyageFlightNo;
			universalShipment.ContainerMode = finalManifest.ContainerMode?.ToUXmlContainerMode();

			var requiredTemperatureUnit = finalManifest.TemperatureMinimum?.Unit;
			universalShipment.RequiredTemperatureUnit = requiredTemperatureUnit == null
				? null
				: new CodeDescriptionPair1Char { Code = requiredTemperatureUnit.Code, Description = requiredTemperatureUnit.Description };

			PopulateAddresses(finalManifest, universalShipment);
			PopulateContainers(finalManifest, universalShipment);
			PopulateShipments(finalManifest, universalShipment);
			PopulateAddInfos(finalManifest, universalShipment);
			PopulateAdditionalReferences(finalManifest, universalShipment);

			return universalShipment;
		}

		#region PopulateAddresses

		void PopulateAddresses(FinalManifest finalManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!finalManifest.SendingParty.IsEmpty())
				{
					addresses.Add(finalManifest.SendingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(finalManifest.SendingPartyCI5, finalManifest.SendingPartySOA, finalManifest.SendingPartySON, finalManifest.SendingPartySOW)));
				}

				if (!finalManifest.ReceivingForwarder.IsEmpty())
				{
					addresses.Add(finalManifest.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(finalManifest.ReceivingForwarderCI5, finalManifest.ReceivingForwarderSON)));
				}

				if (!finalManifest.Transporter.IsEmpty())
				{
					addresses.Add(finalManifest.Transporter.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCFSLocalTransportAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(finalManifest.TransporterCI5, finalManifest.TransporterSON)));
				}

				if (!finalManifest.Carrier.IsEmpty())
				{
					addresses.Add(finalManifest.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(finalManifest.CarrierCI5, finalManifest.CarrierSON)));
				}

				if (!finalManifest.CurrentUser.IsEmpty())
				{
					addresses.Add(finalManifest.CurrentUser.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(finalManifest.CurrentUserCI5, finalManifest.CurrentUserSOA, finalManifest.CurrentUserSON, finalManifest.CurrentUserSOW)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		#endregion

		#region PopulateAddInfos

		void PopulateAddInfos(FinalManifest finalManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfos = new List<AddInfo>();

				if (!finalManifest.OperationalPort.IsEmpty())
				{
					addInfos.AddRange(finalManifest.OperationalPort.ToUXmlAddInfos(nameof(finalManifest.OperationalPort)));
				}

				AddAddInfo(addInfos, nameof(finalManifest.PortLocation), finalManifest.PortLocation);
				AddAddInfo(addInfos, nameof(finalManifest.PortArea), finalManifest.PortArea);
				AddAddInfo(addInfos, FinalManifest.ATPReferenceUXmlName, finalManifest.ATPReference);
				AddAddInfo(addInfos, FinalManifest.OTCReferenceUXmlName, finalManifest.OTCReference);
				AddAddInfo(addInfos, FinalManifest.CBKReferenceUXmlNameUXmlName, finalManifest.CBKReference);
				AddAddInfo(addInfos, nameof(finalManifest.VoyageServiceCode), finalManifest.VoyageServiceCode);
				AddAddInfo(addInfos, "BookingConfirmationCBK", finalManifest.BookingConfirmation);

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

		#endregion

		#region PopulateContainers
		Dictionary<ZGuid, int> PackingLineContainerLinkMap;

		void PopulateContainers(FinalManifest finalManifest, UniversalShipment uxmlShipment)
		{
			var containerLink = 0;
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			PackingLineContainerLinkMap = new Dictionary<ZGuid, int>();

			if (finalManifest.Containers != null)
			{
				foreach (var container in finalManifest.Containers)
				{
					container.LDEStatus = container.LDEIsFinal ? BookingContainer.LDEIsFinalCode : BookingContainer.LDEIsProvisionalCode;
					container.DeliveryArea = finalManifest.DeliveryArea;
					container.DeliveryLocation = finalManifest.DeliveryLocation;
					//container.PortDuesPortCode = finalManifest.PortDuesPortCode;
					//container.PortDuesAmount = finalManifest.PortDuesAmount;
					//container.PortDuesCurrency = finalManifest.PortDuesCurrency;
					//container.PortDuesPayingParty = finalManifest.PortDuesPayingParty;
					container.DateOfArrival = finalManifest.ExpectedArrivalAtPort;
					container.VehicleRegistration = finalManifest.VehicleRegistration;
					container.TransportMode.Code = finalManifest.TransportMode;
					//container.PackingReference = finalManifest.PackingFunctionalReference;
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainer.Link = ++containerLink;

					foreach (BookingPackingLine packingLine in container.PackingLines)
					{
						PackingLineContainerLinkMap.Add((ZGuid)packingLine.Identifier, containerLink);
					}
					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(FinalManifest finalManifest, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(finalManifest));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(FinalManifest finalManifest)
		{
			if (!finalManifest.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = finalManifest.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		#endregion

		#region PopulateShipments

		void PopulateShipments(FinalManifest finalManifest, UniversalShipment uxmlShipment)
		{
			var uxmlSubShipments = finalManifest.GoodsDetails?.Select(x => x.ToUXmlShipment(PackingLineContainerLinkMap, writeManager.WriterStrategy)).ToList() ?? new List<UniversalShipment>();
			foreach (var subShipment in uxmlSubShipments)
			{
			}

			uxmlShipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? new DataObjectList<UniversalShipment>(uxmlSubShipments) : null);
		}

		#endregion
	}
}
