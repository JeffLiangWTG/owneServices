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
	public sealed class DemandeDeTracingDataObjectWriter : DataObjectWriter<DemandeDeTracing, UniversalShipment>
	{
		public DemandeDeTracingDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(DemandeDeTracing demandeDeTracing)
		{
			var universalShipment = CreateUniveralShipment(demandeDeTracing);

			universalShipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfos(demandeDeTracing).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			if (demandeDeTracing.PortOfDestination != null)
			{
				universalShipment.PortOfDestination = new UNLOCO
				{
					Code = demandeDeTracing.PortOfDestination.Code,
					Name = demandeDeTracing.PortOfDestination.Name
				};
			}

			if (demandeDeTracing.PortOfOrigin != null)
			{
				universalShipment.PortOfOrigin = new UNLOCO
				{
					Code = demandeDeTracing.PortOfOrigin.Code,
					Name = demandeDeTracing.PortOfOrigin.Name
				};
			}

			if (demandeDeTracing.ContainerMode != null)
			{
				universalShipment.ContainerMode = new ContainerMode()
				{
					Code = demandeDeTracing.ContainerMode.Code,
					Description = demandeDeTracing.ContainerMode.Description
				};
			}

			if (demandeDeTracing.ShipmentType != null)
			{
				universalShipment.ShipmentType = new CodeDescriptionPair()
				{
					Code = demandeDeTracing.ShipmentType.Code,
					Description = demandeDeTracing.ShipmentType.Description
				};
			}

			universalShipment.BookingConfirmationReference = demandeDeTracing.BookingConfirmationReference;
			universalShipment.WayBillNumber = demandeDeTracing.WaybillNumber;

			universalShipment.SetAdditionalReferenceCollection(() =>
			{
				var additionalReferences = new DataObjectList<AdditionalReference>(CreateAdditionalReferences(demandeDeTracing));
				return additionalReferences.Any() ? additionalReferences : null;
			});

			universalShipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(demandeDeTracing).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			universalShipment.SetContainerCollection(() => CreateContainers(demandeDeTracing));

			universalShipment.SetDateCollection(() =>
			{
				var dates = CreateDates(demandeDeTracing).ToList();

				return dates.Count > 0
					? dates
					: null;
			});

			return universalShipment;
		}

		UniversalShipment CreateUniveralShipment(DemandeDeTracing demandeDeTracing)
		{
			var actualDate = ZDateTimeOffset.Now;

			var universalShipment = new UniversalShipment(writeManager.WriterStrategy);

			universalShipment.DataContext = demandeDeTracing
				.CreateUXmlDataContext()
				.AddUserBranchAndDepartment();

			universalShipment.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

			var dataContext = (UniversalDataBuss.DataObjects.Universal._2012_11.DataContext)universalShipment.DataContext;

			dataContext.DocumentaryOverride = new DocumentaryOverride();
			dataContext.DocumentaryOverride.DataVersion = 1;
			dataContext.DocumentaryOverride.IsSystemDefined = true;
			dataContext.DocumentaryOverride.Purpose = new CodeDescriptionPair
			{
				Code = "ORG"
			};
			dataContext.DocumentaryOverride.SubmissionVersion = 1;
			dataContext.Workflow.ActionPurpose = new CodeDescriptionPair
			{
				Code = DocDataConstants.ActionPurpuses.Codes.AsPerPayload,
				Description = DocDataConstants.ActionPurpuses.Descriptions.AsPerPayload
			};

			dataContext.Workflow.TriggerCount = 1;
			dataContext.Workflow.TriggerDate = actualDate;
			dataContext.Workflow.TriggerType = TriggerType.Manual;

			return universalShipment;
		}

		IEnumerable<AddInfo> CreateAddInfos(DemandeDeTracing demandeDeTracing)
		{
			var addInfos = new List<AddInfo>();

			if (!demandeDeTracing.OperationalPort.IsEmpty())
			{
				var operationalPortCode = new AddInfo();
				var operationalPortName = new AddInfo();

				operationalPortCode.Key = "OperationalPort_Code"; // Programatic constant
				operationalPortCode.Value = demandeDeTracing.OperationalPort.Code;

				operationalPortName.Key = "OperationalPort_Name"; // Programatic constant
				operationalPortName.Value = demandeDeTracing.OperationalPort.Name;

				addInfos.Add(operationalPortCode);
				addInfos.Add(operationalPortName);
			}

			return addInfos;
		}

		IEnumerable<AdditionalReference> CreateAdditionalReferences(
			DemandeDeTracing demandeDeTracing)
		{
			if (!demandeDeTracing.ConsolNumber.IsEmpty)
			{
				yield return new AdditionalReference
				{
					ReferenceNumber = demandeDeTracing.ConsolNumber,
					Type = new EntryType
					{
						Code = DocDataConstants.AdditionalReferences.Codes.FreightForwarderReference,
						Description = DocDataConstants.AdditionalReferences.Descriptions.FreightForwarderReference
					}
				};
			}
		}

		IEnumerable<OrganizationAddress> CreateAddresses(DemandeDeTracing demandeDeTracing)
		{
			if (!demandeDeTracing.SendingForwarder.IsEmpty())
			{
				yield return demandeDeTracing.SendingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.SendingForwarderAddress), writeManager.WriterStrategy);
			}

			if (!demandeDeTracing.ReceivingForwarder.IsEmpty())
			{
				yield return demandeDeTracing.ReceivingForwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy);
			}

			if (!demandeDeTracing.Carrier.IsEmpty())
			{
				yield return demandeDeTracing.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy);
			}

			if (!demandeDeTracing.SendingParty.IsEmpty())
			{
				yield return demandeDeTracing.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(demandeDeTracing.SendingPartyCI5, demandeDeTracing.SendingPartySON));
			}
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(number => number != null && !number.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}

		DataObjectList<UniversalDataBuss.DataObjects.Universal.Container> CreateContainers(DemandeDeTracing demandeDeTracing)
		{
			var containers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();

			foreach (var demandeDeTracingContainer in demandeDeTracing.Containers)
			{
				var container =
					new UniversalDataBuss.DataObjects.Universal.Container(writeManager.WriterStrategy)
					{
						ContainerNumber = demandeDeTracingContainer.ContainerNumber,
						NonOperatingReefer = demandeDeTracingContainer.IsNonOperativeReefer
					};

				containers.Add(container);
			}

			return containers;
		}

		IEnumerable<Date> CreateDates(DemandeDeTracing demandeDeTracing)
		{
			if (!demandeDeTracing.ETA.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Arrival,
					Value = demandeDeTracing.ETA
				};
			}

			if (!demandeDeTracing.ETD.IsEmpty)
			{
				yield return new Date
				{
					Type = DateType.Departure,
					Value = demandeDeTracing.ETD
				};
			}
		}
	}
}
