using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE.BelgianPortsConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer.BE
{
	sealed class CertifiedPickupDataObjectWriter : DataObjectWriter<CertifiedPickup, UniversalShipment>
	{
		public CertifiedPickupDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(CertifiedPickup certifiedPickup)
		{
			if (certifiedPickup.SelectedContainers.All(c => c.CurrentStatus == CertifiedPickupConstants.Status.TransferSentAwaitingResponse))
			{
				return null;
			}

			var shipment = CreateUniveralShipment(certifiedPickup);

			shipment.ContainerMode = certifiedPickup.ContainerMode?.ToUXmlContainerMode();
			shipment.WayBillNumber = certifiedPickup.BillOfLading;

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(certifiedPickup).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			PopulateContainers(certifiedPickup, shipment);

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(certifiedPickup).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			return shipment;
		}

		UniversalShipment CreateUniveralShipment(CertifiedPickup certifiedPickup)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = certifiedPickup
				.CreateUXmlDataContext()
				.AddUserBranchAndDepartment();

			return shipment;
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(CertifiedPickup certifiedPickup)
		{
			if (!certifiedPickup.OperationalPort.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = AddInfoCollectionTypes.OperationalPort_Code,
					Value = certifiedPickup.OperationalPort.Code
				};
			}

			if (!certifiedPickup.Terminal.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = AddInfoCollectionTypes.Terminal_Code,
					Value = certifiedPickup.Terminal
				};
			}
		}

		void PopulateContainers(CertifiedPickup certifiedPickup, UniversalShipment uxmlShipment)
		{
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			if (certifiedPickup.SelectedContainers != null)
			{
				foreach (var container in certifiedPickup.SelectedContainers.Where(c
					=> !certifiedPickup.IsTransferMode || c.CurrentStatus != CertifiedPickupConstants.Status.TransferSentAwaitingResponse))
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		IEnumerable<OrganizationAddress> CreateAddresses(CertifiedPickup certifiedPickup)
		{
			if (certifiedPickup.IsTransferMode || certifiedPickup.IsRevokeMode)
			{
				if (!certifiedPickup.TransportCompany.IsEmpty())
				{
					yield return certifiedPickup.TransportCompany.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCFSLocalTransportAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(certifiedPickup.TransportCompanyId));
				}

				if (!certifiedPickup.Forwarder.IsEmpty())
				{
					yield return certifiedPickup.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(certifiedPickup.ForwarderId));
				}
			}

			if (!certifiedPickup.Carrier.IsEmpty())
			{
				yield return certifiedPickup.Carrier.ToUXmlOrganizationAddress(nameof(DocAddressType.ShippingLineAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(certifiedPickup.CarrierIdentificationId));
			}

			if (!certifiedPickup.SendingParty.IsEmpty())
			{
				yield return certifiedPickup.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(certifiedPickup.SendingPartyId));
			}
		}

		IEnumerable<UniversalDataBuss.DataObjects.Universal.RegistrationNumber> CreateRegistrationNumbers(params DocumentVisualizer.DocDataObjects.RegistrationNumber[] registrationNumbers)
		{
			foreach (var number in registrationNumbers.Where(x => !x.Value.IsEmpty))
			{
				yield return number.ToUXmlRegistrationNumber();
			}
		}
	}
}
