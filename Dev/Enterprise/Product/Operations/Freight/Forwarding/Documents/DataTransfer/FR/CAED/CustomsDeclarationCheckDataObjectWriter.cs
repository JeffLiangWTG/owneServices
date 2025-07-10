using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.FR
{
	sealed class CustomsDeclarationCheckDataObjectWriter : DataObjectWriter<CustomsDeclarationCheck, UniversalShipment>
	{
		public CustomsDeclarationCheckDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CustomsDeclarationCheck customsDeclarationCheck)
		{
			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);
			universalShipment.DataContext = customsDeclarationCheck.CreateUXmlDataContext();

			universalShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = CreateAdditionalReferences(customsDeclarationCheck).ToArray();

				return additionalReferences.Length > 0
					? new DataObjectList<AdditionalReference>(additionalReferences)
					: null;
			});

			PopulateAddresses(customsDeclarationCheck, universalShipment);
			PopulateContainers(customsDeclarationCheck, universalShipment);

			universalShipment.ContainerMode = customsDeclarationCheck.ContainerMode?.ToUXmlContainerMode();
			universalShipment.ShipmentType = customsDeclarationCheck.ShipmentType?.ToUXmlCodeDescriptionPair();
			universalShipment.PortOfDestination = customsDeclarationCheck.PortOfDestination.ToUXmlUnloco();
			universalShipment.PortOfOrigin = customsDeclarationCheck.PortOfOrigin.ToUXmlUnloco();

			universalShipment.TotalNoOfPacks = customsDeclarationCheck.TotalNumberOfPacks;
			universalShipment.TotalNoOfPacksPackageType = new PackageType
			{
				Code = customsDeclarationCheck.PackageType.Code,
				Description = customsDeclarationCheck.PackageType.Description
			};

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(customsDeclarationCheck).ToList();
				return addInfos.Any() ? addInfos : null;
			});

			return universalShipment;
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(CustomsDeclarationCheck customsDeclarationCheck)
		{
			if (!customsDeclarationCheck.ShipmentNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = customsDeclarationCheck.ShipmentNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		void PopulateAddresses(CustomsDeclarationCheck customsDeclarationCheck, UniversalShipment uxmlShipment)
		{
			uxmlShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = new List<OrganizationAddress>();

				if (!customsDeclarationCheck.CurrentUser.IsEmpty())
				{
					addresses.Add(customsDeclarationCheck.CurrentUser.ToUXmlOrganizationAddress(nameof(customsDeclarationCheck.CurrentUser), writeManager.WriterStrategy, CreateRegistrationNumbers(customsDeclarationCheck.SendingPartyCI5Code, customsDeclarationCheck.SendingPartySONCode)));
				}

				if (!customsDeclarationCheck.SendingForwarderAddress.IsEmpty())
				{
					addresses.Add(customsDeclarationCheck.SendingForwarderAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers()));
				}

				if (!customsDeclarationCheck.ReceivingForwarderAddress.IsEmpty())
				{
					addresses.Add(customsDeclarationCheck.ReceivingForwarderAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers()));
				}

				if (!customsDeclarationCheck.ExportBrokerAddress.IsEmpty())
				{
					if (!customsDeclarationCheck.IsImport)
					{
						addresses.Add(customsDeclarationCheck.ExportBrokerAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ExportBroker), writeManager.WriterStrategy, CreateRegistrationNumbers(customsDeclarationCheck.DeclarantsSIRETNumber)));
					}
					else
					{
						addresses.Add(customsDeclarationCheck.ExportBrokerAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ExportBroker), writeManager.WriterStrategy, CreateRegistrationNumbers(new DocumentVisualizer.DocDataObjects.RegistrationNumber())));
					}
				}

				if (!customsDeclarationCheck.ImportBrokerAddress.IsEmpty())
				{
					if (customsDeclarationCheck.IsImport)
					{
						addresses.Add(customsDeclarationCheck.ImportBrokerAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ImportBroker), writeManager.WriterStrategy, CreateRegistrationNumbers(customsDeclarationCheck.DeclarantsSIRETNumber)));
					}
					else
					{
						addresses.Add(customsDeclarationCheck.ExportBrokerAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ImportBroker), writeManager.WriterStrategy, CreateRegistrationNumbers(new DocumentVisualizer.DocDataObjects.RegistrationNumber())));
					}
				}

				if (!customsDeclarationCheck.DepartureCTOAddress.IsEmpty())
				{
					if (!customsDeclarationCheck.IsImport)
					{
						addresses.Add(customsDeclarationCheck.DepartureCTOAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(customsDeclarationCheck.CTOCI5Code, customsDeclarationCheck.CTOSONCode)));
					}
					else
					{
						addresses.Add(customsDeclarationCheck.DepartureCTOAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.DepartureCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(new DocumentVisualizer.DocDataObjects.RegistrationNumber())));
					}
				}

				if (!customsDeclarationCheck.ArrivalCTOAddress.IsEmpty())
				{
					if (customsDeclarationCheck.IsImport)
					{
						addresses.Add(customsDeclarationCheck.ArrivalCTOAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(customsDeclarationCheck.CTOCI5Code, customsDeclarationCheck.CTOSONCode)));
					}
					else
					{
						addresses.Add(customsDeclarationCheck.ArrivalCTOAddress.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCTOAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(new DocumentVisualizer.DocDataObjects.RegistrationNumber())));
					}
				}

				return addresses.Any() ? addresses : null;
			});
		}

		IEnumerable<RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		void PopulateContainers(CustomsDeclarationCheck customsDeclarationCheck, UniversalShipment uxmlShipment)
		{
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

			if (customsDeclarationCheck.Containers != null)
			{
				foreach (var container in customsDeclarationCheck.Containers)
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);

					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers.Any() ? uxmlContainers : null);
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(CustomsDeclarationCheck customsDeclarationCheck)
		{
			if (customsDeclarationCheck.Port != null)
			{
				yield return new AddInfo
				{
					Key = "OperationalPort_Code", // XML Add Info key
					Value = customsDeclarationCheck.Port.Code
				};

				yield return new AddInfo
				{
					Key = "OperationalPort_Name", // XML Add Info key
					Value = customsDeclarationCheck.Port.Name
				};
			}

			yield return new AddInfo
			{
				Key = DocDataConstants.AddinfoTypes.FormVersion,
				Value = "1.0.0" // Programatic version number
			};

			if (!customsDeclarationCheck.DeclarationType.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.DeclarationType),
					Value = customsDeclarationCheck.DeclarationType
				};
			}

			if (!customsDeclarationCheck.CustomsOfficeCode.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.CustomsOfficeCode),
					Value = customsDeclarationCheck.CustomsOfficeCode.Code
				};
			}

			if (!customsDeclarationCheck.DeclarationFileNumber.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.DeclarationFileNumber),
					Value = customsDeclarationCheck.DeclarationFileNumber
				};
			}

			if (!customsDeclarationCheck.CommonAccessRef.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.CommonAccessRef),
					Value = customsDeclarationCheck.CommonAccessRef
				};
			}

			if (!customsDeclarationCheck.AppliesToAllPacks.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.AppliesToAllPacks),
					Value = customsDeclarationCheck.AppliesToAllPacks.ToString()
				};
			}

			if (!customsDeclarationCheck.PortDuesAmount.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.PortDuesAmount),
					Value = customsDeclarationCheck.PortDuesAmount.ToString()
				};
			}

			if (!customsDeclarationCheck.PortDuesCurrency.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = nameof(customsDeclarationCheck.PortDuesCurrency),
					Value = customsDeclarationCheck.PortDuesCurrency.Code
				};
			}
		}
	}
}
