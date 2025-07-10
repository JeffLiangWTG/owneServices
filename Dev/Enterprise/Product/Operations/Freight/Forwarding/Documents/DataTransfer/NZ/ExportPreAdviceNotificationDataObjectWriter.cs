using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NZ;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Container = Enterprise.Freight.Forwarding.Documents.DocDataObjects.Container;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NZ
{
	sealed class ExportPreAdviceNotificationDataObjectWriter : DataObjectWriter<ExportPreAdviceNotification, UniversalShipment>
	{
		public ExportPreAdviceNotificationDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(ExportPreAdviceNotification notification)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = notification.CreateUXmlDataContext();

			PopulateAddresses(notification, shipment);
			PopulateAdditionalReferences(notification, shipment);
			PopulateShipment(notification, shipment);
			PopulateContainers(notification, shipment);

			return shipment;
		}

		#region PopulateAddresses

		void PopulateAddresses(ExportPreAdviceNotification notification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!notification.Shipper.IsEmpty())
				{
					addresses.Add(notification.Shipper.ToUXmlOrganizationAddress(
						nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy,
						CreateRegistrationNumbers(Constants.CountryCodes.NewZealand, OrgCusCode.CodeTypes.PortSystemNumber, notification.ShipperCode)));
				}

				if (!notification.Carrier.IsEmpty())
				{
					addresses.Add(notification.Carrier.ToUXmlOrganizationAddress(
						nameof(DocAddressType.Carrier), writeManager.WriterStrategy,
						CreateRegistrationNumbers(Constants.CountryCodes.UnitedStates, OrgCusCode.CodeTypes.CarrierCode, notification.CarrierCode)));
				}

				if (!notification.DepartureCTOAddress.IsEmpty())
				{
					addresses.Add(notification.DepartureCTOAddress.ToUXmlOrganizationAddress(
						nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy,
						CreateRegistrationNumbers(Constants.CountryCodes.NewZealand, OrgCusCode.CodeTypes.PortSystemNumber, notification.LoadPortFacility)));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(ZString country, ZString type, ZString value)
		{
			return new List<RegistrationNumber>
			{
				new RegistrationNumber
				{
					CountryOfIssue = new Country { Code = country },
					Type = new RegistrationNumberType { Code = type },
					Value = value
				}
			};
		}

		#endregion

		#region PopulateAdditionalReferences

		void PopulateAdditionalReferences(ExportPreAdviceNotification notification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences =
					new DataObjectList<AdditionalReference>(CreateAdditionalReferences(notification));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(ExportPreAdviceNotification notification)
		{
			if (!notification.FreightForwardersReference.IsEmpty)
			{
				yield return new AdditionalReference
				{
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					},
					ReferenceNumber = notification.FreightForwardersReference
				};
			}
		}

		#endregion

		#region PopulateShipment

		void PopulateShipment(ExportPreAdviceNotification notification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.BookingConfirmationReference = notification.BookingConfirmationReference;
			uxmlShipment.VesselName = notification.Vessel.Name;
			uxmlShipment.VoyageFlightNo = notification.Voyage;
			uxmlShipment.PortOfLoading = notification.PortOfLoad.ToUXmlUnloco();
			uxmlShipment.PortOfDischarge = notification.PortOfDischarge.ToUXmlUnloco();
			uxmlShipment.PortOfOrigin = notification.Origin.ToUXmlUnloco();

			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>();

				AddAddInfo(addInfoCollection, "Other_TransportMode", notification.PreCarriageMode.Code);
				AddAddInfo(addInfoCollection, "OperationalPort_Code", notification.OperationalPort.Code);
				AddAddInfo(addInfoCollection, "OperationalPort_Name", notification.OperationalPort.Name);

				return addInfoCollection.Any() ? addInfoCollection : null;
			});
		}

		void AddAddInfo(List<AddInfo> addinfos, ZString key, ZString value)
		{
			if (!value.IsEmpty)
			{
				addinfos.Add(new AddInfo { Key = key, Value = value });
			}
		}

		#endregion

		#region PopulateContainers

		void PopulateContainers(ExportPreAdviceNotification notification, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetContainerCollection(() =>
			{
				var containerCollection = new DataObjectList<UniversalContainer>();

				foreach (var container in notification.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					if (!container.HSCode.IsEmpty)
					{
						uxmlContainer.Commodity = new Commodity
						{
							Code = container.HSCode
						};
					}
					PopulateUNDGs(uxmlContainer, container);

					containerCollection.Add(uxmlContainer);
				}

				return containerCollection.Any() ? containerCollection : null;
			});
		}

		void PopulateUNDGs(UniversalContainer uxmlContainer, Container container)
		{
			uxmlContainer.SetUNDGCollection(() =>
			{
				var uxmlUNDGs = new List<UNDG>();
				var dangerousGoods = container.PackingLines?.SelectMany(x => x.DangerousGoods).ToArray();
				if (dangerousGoods != null && dangerousGoods.Any())
				{
					foreach (var dangerousGood in dangerousGoods)
					{
						var undg = new UNDG(writeManager.WriterStrategy)
						{
							UNDGCode = dangerousGood.Code,
							Contact = dangerousGood.Contact.ToUXmlContact(),
							ProperShippingName = dangerousGood.ProperShippingName,
							TechicalName = dangerousGood.TechnicalName,
							PackQty = dangerousGood.Quantity,
							PackedInLimitedQuantity = dangerousGood.PackedInLimitedQuantity,
							IMOClass = dangerousGood.IMOClass,
							PackingGroup = dangerousGood.PackingGroup,
							SubLabel1 = dangerousGood.SubLabel1,
							SubLabel2 = dangerousGood.SubLabel2,
							Weight = dangerousGood.Weight?.Value,
							State = new UNDGStateConverter().ToEnumValue(dangerousGood.State),
							WeightUQ = new UnitOfWeight
							{
								Code = dangerousGood.Weight?.Unit?.Code,
								Description = dangerousGood.Weight?.Unit?.Description
							},
							Volume = dangerousGood.Volume?.Value,
							VolumeUQ = new UnitOfVolume
							{
								Code = dangerousGood.Volume?.Unit?.Code,
								Description = dangerousGood.Volume?.Unit?.Description
							},
							PackType = new PackageType
							{
								Code = dangerousGood.PackageType?.Code,
								Description = dangerousGood.PackageType?.Description
							},
							MarinePollutant = new UNDGMarinePollutant
							{
								Code = dangerousGood.MarinePollutant?.Code,
								Description = dangerousGood.MarinePollutant?.Description
							},
						};

						if (dangerousGood.FlashPoint != null)
						{
							undg.FlashPoint = dangerousGood.FlashPoint.Value.ToString();
						}

						uxmlUNDGs.Add(undg);
					}
				}

				return uxmlUNDGs.Any() ? uxmlUNDGs : null;
			});
		}
		#endregion
	}
}
