using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	/// <summary>
	/// Summary description for IHaveInternalCartage.
	/// </summary>
	public interface IHaveInternalCartage : ICartageExportSupport
	{
		//InternalCartageManager InternalCartageManager { get; }

		ZBool DeliveryAndPickupCartageApplicable { get; }
		ZBool InternalCartageEnabled { get; }
		ZBool PackedAtDepot { get; }
		bool IsForPickupCartage { get; }

		//JobCartage CreateInternalJobCartage(ZString ExpectedJobNumber);
		//JobCartage GetInternalJobCartage(ZString ExpectedJobNumber);

		IContainer[] GetContainers();
		IPackLineInfo[] GetPackLines();

		OrgHeader PickupCartageOrg { get; }
		OrgHeader DeliveryCartageOrg { get; }

		ZString ContainerMode { get; }
		ZString TransportMode { get; }
		ZString ServiceLevel { get; }
		/// <summary>
		/// If this is an empty string, the InternalCartageManager will determine 
		/// the cartage type from the transport and container modes.
		/// </summary>
		ZString CartageTypeOverride { get; }
		ZString OwnerRef { get; }

		ZGuid PK { get; }
		ZGuid CartagePickupDepotAddress { get; }
		ZGuid CartageDeliveryDepotAddress { get; }
		ZGuid CartagePickupCTOAddress { get; }
		ZGuid CartageDeliveryCTOAddress { get; }
		ZGuid CartagePickupContainerYardAddress { get; }
		ZGuid CartageDeliveryContainerYardAddress { get; }
		ZGuid BranchPK { get; }

		JobDocAddress CartageExporterDocAddress { get; }
		JobDocAddress CartageImporterDocAddress { get; }

		ZPropertyInfo CartageDeliveryDepotAddressInfo { get; }
		ZPropertyInfo CartagePickupCTOAddressInfo { get; }
		ZPropertyInfo CartageDeliveryCTOAddressInfo { get; }
		ZPropertyInfo CartagePickupContainerYardAddressInfo { get; }
		ZPropertyInfo CartageDeliveryContainerYardAddressInfo { get; }
		ZPropertyInfo CartagePickupDepotAddressInfo { get; }

		ZPropertyInfo DeliveryCartageAdvisedInfo { get; }
		ZPropertyInfo PickupCartageAdvisedInfo { get; }

		JobDocAddressDependentCollection DocAddresses { get; }
		bool IsAllowedToUpdateAdviseDates { get; }

		/// <summary>
		/// Anything specific to the implementing class
		/// </summary>
		//void TypeSpecificCartageSetup(JobCartage Cartage);
		bool TypeSpecificPreCreationCheck();
	}
}
