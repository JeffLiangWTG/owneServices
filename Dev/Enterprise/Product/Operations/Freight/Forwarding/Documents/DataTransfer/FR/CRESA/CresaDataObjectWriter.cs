using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using EZC = Enterprise.ZArchitecture.Core;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class CresaDataObjectWriter : DataObjectWriter<Cresa, UniversalShipment>
	{
		public CresaDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(Cresa cresa)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);

			universalShipment.DataContext = cresa.CreateUXmlDataContext();

			universalShipment.ShipmentType = new CodeDescriptionPair
			{
				Code = cresa.ShipmentType.Code,
				Description = cresa.ShipmentType.Description
			};

			universalShipment.GoodsDescription = cresa.GoodsDescription;
			universalShipment.BookingConfirmationReference = cresa.CarrierBookingReference;
			universalShipment.ContainerMode = cresa.ContainerMode?.ToUXmlContainerMode();
			universalShipment.PortOfDestination = cresa.PortOfArrival.ToUXmlUnloco();
			universalShipment.PortFirstForeign = cresa.PortOfTranshipment.ToUXmlUnloco();
			universalShipment.PortOfOrigin = cresa.OperationalPort.ToUXmlUnloco();
			universalShipment.TotalNoOfPacks = cresa.TotalPackCount;
			universalShipment.TotalNoOfPacksPackageType = new PackageType
			{
				Code = cresa.PackType.Code,
				Description = cresa.PackType.Description
			};

			universalShipment.TotalVolume = cresa.TotalVolume?.Value;
			universalShipment.TotalVolumeUnit = new UnitOfVolume
			{
				Code = cresa.TotalVolume?.Unit?.Code,
				Description = cresa.TotalVolume?.Unit?.Description
			};

			universalShipment.TotalWeight = cresa.TotalWeight?.Value;
			universalShipment.TotalWeightUnit = new UnitOfWeight()
			{
				Code = cresa.TotalWeight?.Unit?.Code,
				Description = cresa.TotalWeight?.Unit?.Description
			};

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(cresa).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			universalShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = CreateAdditionalReferences(cresa).ToArray();

				return additionalReferences.Length > 0
					? new DataObjectList<AdditionalReference>(additionalReferences)
					: null;
			});

			universalShipment.SetDateCollection(() =>
			{
				var dates = CreateDates(cresa).ToList();
				return dates.Any() ? dates : null;
			});

			universalShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(cresa).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			universalShipment.SetNoteCollection(() =>
			{
				var notes = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Note>(CreateNotes(cresa));

				return notes.Count > 0
					? notes
					: null;
			});

			var uxmlPackingLines = new List<UniversalDataBuss.DataObjects.Universal.PackingLine>();

			foreach (var packingLine in cresa.PackingLines)
			{
				var uxmlPackingLine = packingLine.ToUXmlPackingLine(writeManager.WriterStrategy);
				uxmlPackingLines.Add(uxmlPackingLine);
			}
			universalShipment.SetPackingLineCollection(() => new DataObjectList<UniversalDataBuss.DataObjects.Universal.PackingLine>(uxmlPackingLines));

			return universalShipment;
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(Cresa cresa)
		{
			if (!cresa.OperationalPort.IsEmpty())
			{
				foreach (var addInfo in cresa.OperationalPort.ToUXmlAddInfos(nameof(cresa.OperationalPort)))
				{
					yield return addInfo;
				}
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};

			if (!cresa.PortLocation.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.PortLocation),
					Value = cresa.PortLocation
				};
			}

			if (!cresa.PortArea.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.PortArea),
					Value = cresa.PortArea
				};
			}

			if (!cresa.TransportMode.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.TransportMode),
					Value = cresa.TransportMode
				};
			}

			if (!cresa.TransporterID.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.TransporterID),
					Value = cresa.TransporterID
				};
			}

			if (!cresa.PortServiceCodeReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = DocDataConstants.AddinfoTypes.PortServiceReference,
					Value = cresa.PortServiceCodeReference
				};
			}

			if (!cresa.EntryNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.EntryNumber),
					Value = cresa.EntryNumber
				};
			}

			if (!cresa.ECVReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = "AMQReference",
					Value = cresa.ECVReference
				};
			}

			if (!cresa.CommodityReference.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(cresa.CommodityReference),
					Value = cresa.CommodityReference
				};
			}

			yield return new AddInfo
			{
				Key = nameof(cresa.GoodsSealed),
				Value = cresa.GoodsSealed.ToString()
			};
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(Cresa cresa)
		{
			if (!cresa.ShipmentNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = cresa.ShipmentNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(Cresa cresa)
		{
			if (!cresa.Supplier.IsEmpty())
			{
				yield return cresa.Supplier.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsignorDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.SupplierCI5, cresa.SupplierSON));
			}

			if (!cresa.Buyer.IsEmpty())
			{
				yield return cresa.Buyer.ToUXmlOrganizationAddress(nameof(DocAddressType.ConsigneeDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.BuyerCI5, cresa.BuyerSON));
			}

			if (!cresa.Transporter.IsEmpty())
			{
				yield return cresa.Transporter.ToUXmlOrganizationAddress(AddressTypes.PickupLocalCartage, writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.TransporterCI5, cresa.TransporterSON));
			}

			if (!cresa.Agent.IsEmpty())
			{
				yield return cresa.Agent.ToUXmlOrganizationAddress(nameof(DocAddressType.PickupAgent), writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.AgentCI5, cresa.AgentSON, cresa.AgentSOA, cresa.AgentSOW));
			}

			if (!cresa.SendingParty.IsEmpty())
			{
				yield return cresa.SendingParty.ToUXmlOrganizationAddress(nameof(DocAddressType.BookingPartyDocumentaryAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.SendingPartyCI5, cresa.SendingPartySON, cresa.SendingPartySOA, cresa.SendingPartySOW));
			}

			if (!cresa.SendingForwarder.IsEmpty())
			{
				yield return cresa.SendingForwarder.ToUXmlOrganizationAddress(AddressTypes.CurrentUser, writeManager.WriterStrategy, CreateRegistrationNumbers(cresa.SendingForwarderCI5, cresa.SendingForwarderSON, cresa.SendingForwarderSOA, cresa.SendingForwarderSOW));
			}
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => x != null && !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		#region Notes

		IEnumerable<UniversalDataBuss.DataObjects.Universal.Note> CreateNotes(Cresa cresa)
		{
			if (!string.IsNullOrWhiteSpace(cresa.GoodsReceiptNotes))
			{
				yield return new UniversalDataBuss.DataObjects.Universal.Note
				{
					Description = (EZC.NoResString)"Goods Receipt Notes", // hardcoded constant
					NoteText = cresa.GoodsReceiptNotes
				};
			}
		}

		#endregion

		IEnumerable<Date> CreateDates(Cresa cresa)
		{
			if (!cresa.ETA.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = cresa.ETA
				};
			}
			if (!cresa.GoodsInDateTime.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Received,
					Value = cresa.GoodsInDateTime
				};
			}
			if (!cresa.CargoReceiptDate.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.CargoReceiptDate,
					Value = cresa.CargoReceiptDate
				};
			}
		}
	}
}
