using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Freight.Forwarding.Business.TMiningConstants;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.TMiningConstants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.DataTransfer
{
	sealed class SecureContainerReleaseDataObjectWriter : DataObjectWriter<SecureContainerRelease, UniversalShipment>
	{
		public SecureContainerReleaseDataObjectWriter(IDataWritingManager writeManager)
			: base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(SecureContainerRelease scr)
		{
			if (scr.SelectedContainers.All(c => c.CurrentStatus == SecureContainerReleaseStatus.TransferSentAwaitingResponse))
			{
				return null;
			}

			var shipment = CreateUniveralShipment(scr);

			shipment.ContainerMode = scr.ContainerMode?.ToUXmlContainerMode();
			shipment.WayBillNumber = scr.BillOfLading;

			shipment.SetAddInfoCollection(() =>
			{
				var addInfos = CreateAddInfosRoot(scr).ToList();

				return addInfos.Count > 0
					? addInfos
					: null;
			});

			PopulateContainers(scr, shipment);

			shipment.SetOrganizationAddressCollection(() =>
			{
				var addresses = CreateAddresses(scr).ToList();

				return addresses.Count > 0
					? addresses
					: null;
			});

			return shipment;
		}

		UniversalShipment CreateUniveralShipment(SecureContainerRelease scr)
		{
			var shipment = new UniversalShipment(writeManager.WriterStrategy);
			shipment.DataContext = scr
				.CreateUXmlDataContext()
				.AddUserBranchAndDepartment();

			return shipment;
		}

		IEnumerable<AddInfo> CreateAddInfosRoot(SecureContainerRelease scr)
		{
			if (!scr.OperationalPort.Code.IsEmpty)
			{
				yield return new AddInfo
				{
					Key = AddInfoCollectionTypes.OperationalPort_Code,
					Value = scr.OperationalPort.Code
				};
			}
		}

		void PopulateContainers(SecureContainerRelease scr, UniversalShipment uxmlShipment)
		{
			var uxmlContainers = new DataObjectList<UniversalDataBuss.DataObjects.Universal.Container>();
			if (scr.SelectedContainers != null)
			{
				foreach (var container in scr.SelectedContainers.Where(c => !scr.IsTransferMode || c.CurrentStatus != SecureContainerReleaseStatus.TransferSentAwaitingResponse))
				{
					var uxmlContainer = container.ToUXmlContainer(writeManager.WriterStrategy);
					uxmlContainers.Add(uxmlContainer);
				}
			}

			uxmlShipment.SetContainerCollection(() => uxmlContainers);
		}

		IEnumerable<OrganizationAddress> CreateAddresses(SecureContainerRelease scr)
		{
			if (scr.IsTransferMode || scr.IsRevokeMode)
			{
				if (!scr.TransportCompany.IsEmpty())
				{
					yield return scr.TransportCompany.ToUXmlOrganizationAddress(nameof(DocAddressType.ArrivalCFSLocalTransportAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(scr.TransportCompanyIdEOR, scr.TransportCompanyIdDUN));
				}

				if (!scr.Forwarder.IsEmpty())
				{
					yield return scr.Forwarder.ToUXmlOrganizationAddress(nameof(DocAddressType.ReceivingForwarderAddress), writeManager.WriterStrategy, CreateRegistrationNumbers(scr.ForwarderIdEOR, scr.ForwarderIdDUN));
				}
			}

			if (!scr.SendingParty.IsEmpty())
			{
				yield return scr.SendingParty.ToUXmlOrganizationAddress("CurrentUser", writeManager.WriterStrategy, CreateRegistrationNumbers(scr.SendingPartyIdEOR, scr.SendingPartyIdDUN));
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
