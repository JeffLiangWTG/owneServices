using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.NL
{
	sealed class PortbaseImportNotificationDataObjectWriter : DataObjectWriter<PortbaseImportNotification, UniversalShipment>
	{
		public PortbaseImportNotificationDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(PortbaseImportNotification portbase)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = portbase.CreateUXmlDataContext();
			shipment.TransportMode = portbase.TransportMode?.ToUXmlCodeDescriptionPair();
			if (portbase.IsFerryTeminal)
			{
				shipment.BookingConfirmationReference = portbase.CarrierBookingRef;
			}
			shipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(portbase));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			var addInfos = CreateAddInfosRoot(portbase).ToList();
			shipment.SetAddInfoCollection(() => addInfos.Any()
				? addInfos
				: null);

			shipment.SetSubShipmentCollection(() =>
			{
				var subShipments = CreateSubShipments(portbase).ToList();
				return subShipments.Any() ? new DataObjectList<UniversalShipment>(subShipments) : null;
			});

			var addresses = CreateAddresses(portbase).ToList();
			shipment.SetOrganizationAddressCollection(() => addresses.Any()
				? addresses
				: null);

			return shipment;
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(PortbaseImportNotification portbase)
		{
			if (!portbase.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = portbase.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(PortbaseImportNotification portbase)
		{
			if (portbase.ReceivingPort != null)
			{
				yield return new AddInfo
				{
					Key = "OperationalPort_Code",
					Value = portbase.ReceivingPort.Code
				};

				yield return new AddInfo
				{
					Key = "OperationalPort_Name",
					Value = portbase.ReceivingPort.Name
				};
			}

			yield return new AddInfo
			{
				Key = "Is_FerryTerminal",   // constant
				Value = portbase.IsFerryTeminal.ToString()
			};
		}

		#region Documents-Containers-Shipments

		IEnumerable<UniversalShipment> CreateSubShipments(PortbaseImportNotification portbase)
		{
			foreach (var portbaseDocument in portbase.Documents)
			{
				var shipment = new UniversalShipment(writeManager.WriterStrategy);

				var addInfos = CreateAddInfosDocument(portbaseDocument).ToList();
				shipment.SetAddInfoCollection(() => addInfos.Any()
					? addInfos
					: null);

				PopulateContainersPacklines(shipment, portbaseDocument, portbase);

				yield return shipment;
			}
		}

		IEnumerable<AddInfo> CreateAddInfosDocument(PortbaseDocument portbaseDocument)
		{
			if (!portbaseDocument.ReferenceNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(portbaseDocument.ReferenceNumber),
					Value = portbaseDocument.ReferenceNumber
				};
			}

			if (!portbaseDocument.EntryType.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(portbaseDocument.EntryType), nameof(portbaseDocument.EntryType.Code)),
					Value = portbaseDocument.EntryType.Code
				};
			}
		}

		void PopulateContainersPacklines(UniversalShipment shipment, PortbaseDocument portbaseDocument, PortbaseImportNotification portbase)
		{
			var seed = 0;

			shipment.SetContainerCollection(() =>
			{
				var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

				foreach (var portbaseContainer in portbaseDocument.Containers)
				{
					var uxmlContainer = new UniversalDataBuss.DataObjects.Universal.Container(writeManager.WriterStrategy);

					uxmlContainer.Link = ++seed;

					if (!portbase.IsFerryTeminal)
					{
						uxmlContainer.ContainerType = new UniversalDataBuss.DataObjects.Universal.ContainerType
						{
							Code = "CN",
							Description = "CN"
						};
					}

					if (!portbaseContainer.Number.IsEmpty)
					{
						uxmlContainer.ContainerNumber = portbaseContainer.Number;
					}
					if (!portbaseContainer.GrossWeight.Value.IsEmpty)
					{
						uxmlContainer.GrossWeight = portbaseContainer.GrossWeight.Value;
						uxmlContainer.WeightUnit = new UnitOfWeight
						{
							Code = portbaseContainer.GrossWeight.Unit.Code,
							Description = portbaseContainer.GrossWeight.Unit.Description
						};
					}

					uxmlContainer.NonOperatingReefer = portbaseContainer.IsNonOperativeReefer;

					uxmlContainer.SetPackingLineCollection(() =>
					{
						var uxmlPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();

						foreach (var portbaseShipment in portbaseContainer.Shipments)
						{
							var uxmlPackingLine = new UniversalDataBuss.DataObjects.Universal.PackingLine(writeManager.WriterStrategy);

							uxmlPackingLine.ContainerLink = uxmlContainer.Link;
							if (!portbaseShipment.Number.IsEmpty)
							{
								uxmlPackingLine.ReferenceNumber = portbaseShipment.Number;
							}
							if (!portbaseShipment.Quantity.IsEmpty)
							{
								uxmlPackingLine.PackQty = new ZLong(portbaseShipment.Quantity);
							}
							if (!portbaseShipment.PackageType.Code.IsEmpty)
							{
								uxmlPackingLine.PackType = new PackageType
								{
									Code = portbaseShipment.PackageType.Code,
									Description = portbaseShipment.PackageType.Description
								};
							}
							if (!portbaseShipment.Weight.Value.IsEmpty)
							{
								uxmlPackingLine.Weight = portbaseShipment.Weight.Value;
							}

							uxmlPackingLines.Add(uxmlPackingLine);
						}
						return uxmlPackingLines;
					});

					uxmlContainers.Add(uxmlContainer);
				}
				return uxmlContainers;
			});
		}

		#endregion

		IEnumerable<OrganizationAddress> CreateAddresses(PortbaseImportNotification portbase)
		{
			if (!portbase.CTO.IsEmpty())
			{
				yield return portbase.CTO.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy);
			}
			if (!portbase.CurrentUser.IsEmpty())
			{
				yield return portbase.CurrentUser.ToUXmlOrganizationAddress(nameof(portbase.CurrentUser), writeManager.WriterStrategy);
			}
		}
	}
}
