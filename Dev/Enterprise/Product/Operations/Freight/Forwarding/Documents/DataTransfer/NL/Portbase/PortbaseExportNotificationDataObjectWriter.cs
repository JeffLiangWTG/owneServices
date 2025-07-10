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
	sealed class PortbaseExportNotificationDataObjectWriter : DataObjectWriter<PortbaseExportNotification, UniversalShipment>
	{
		public PortbaseExportNotificationDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(PortbaseExportNotification portbase)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = portbase.CreateUXmlDataContext()
				.AddDataProvider()
				.AddUserBranchAndDepartment();

			shipment.BookingConfirmationReference = portbase.BookingConfirmationReference;

			PopulateAddInfos(portbase, shipment);
			PopulateAdditionalReferences(portbase, shipment);
			PopulateAddresses(portbase, shipment);
			PopulateSubShipments(portbase, shipment);

			return shipment;
		}

		#region PopulateAddInfos

		void PopulateAddInfos(PortbaseExportNotification portbase, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>();

				AddAddInfo(addInfoCollection, "Is_FerryTerminal", portbase.IsFerryTerminal.ToString());

				if (!portbase.OperationalPort.IsEmpty())
				{
					addInfoCollection.AddRange(portbase.OperationalPort.ToUXmlAddInfos(nameof(portbase.OperationalPort)));
				}

				return addInfoCollection.Any() ? addInfoCollection : null;
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

		void PopulateAdditionalReferences(PortbaseExportNotification portbase, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(portbase));
				return additionalReferences.Any() ? additionalReferences : null;
			});
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(PortbaseExportNotification portbase)
		{
			if (!portbase.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					},
					ReferenceNumber = portbase.ConsolNumber
				};
			}
		}

		#endregion

		#region PopulateAddresses

		void PopulateAddresses(PortbaseExportNotification portbase, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!portbase.DepartureCTO.IsEmpty())
				{
					addresses.Add(portbase.DepartureCTO.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy));
				}

				if (!portbase.CurrentUser.IsEmpty())
				{
					addresses.Add(portbase.CurrentUser.ToUXmlOrganizationAddress(nameof(portbase.CurrentUser), writeManager.WriterStrategy));
				}

				return addresses.Any() ? addresses : null;
			});
		}

		#endregion

		#region PopulateSubShipments

		void PopulateSubShipments(PortbaseExportNotification portbase, UniversalShipment uxmlShipment)
		{
			var uxmlSubShipments = portbase.Documents?.Where(d => d.IsSelectedToSend).Select(d => ToUXmlSubShipment(d, writeManager.WriterStrategy)).ToList()
							?? new List<UniversalShipment>();

			uxmlShipment.SetSubShipmentCollection(() => uxmlSubShipments.Any() ? new DataObjectList<UniversalShipment>(uxmlSubShipments) : null);
		}

		UniversalShipment ToUXmlSubShipment(PortbaseDocument portbaseDocument, IDataObjectWriterStrategy writerStrategy)
		{
			if (portbaseDocument == null)
			{
				return null;
			}

			var uxmlSubShipment = new UniversalShipment(writerStrategy);

			uxmlSubShipment.SetAddInfoCollection(() =>
			{
				var addInfoCollection = new List<AddInfo>();

				AddAddInfo(addInfoCollection, nameof(portbaseDocument.ReferenceNumber), portbaseDocument.ReferenceNumber);
				AddAddInfo(addInfoCollection, string.Format(CultureInfo.InvariantCulture, "{0}_{1}", nameof(portbaseDocument.EntryType), nameof(portbaseDocument.EntryType.Code)), portbaseDocument.EntryType.Code);

				return addInfoCollection.Any() ? addInfoCollection : null;
			});

			var seed = 0;

			uxmlSubShipment.SetContainerCollection(() =>
			{
				var containerCollection = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

				foreach (var portbaseContainer in portbaseDocument.Containers)
				{
					var uxmlContainer = new UniversalDataBuss.DataObjects.Universal.Container(writerStrategy);

					uxmlContainer.Link = ++seed;

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
								uxmlPackingLine.WeightUnit = new UnitOfWeight
								{
									Code = portbaseShipment.Weight.Unit.Code,
									Description = portbaseShipment.Weight.Unit.Description
								};
							}

							uxmlPackingLines.Add(uxmlPackingLine);
						}
						return uxmlPackingLines.Any() ? uxmlPackingLines : null;
					});

					containerCollection.Add(uxmlContainer);
				}
				return containerCollection.Any() ? containerCollection : null;
			});

			return uxmlSubShipment;
		}

		#endregion
	}
}
