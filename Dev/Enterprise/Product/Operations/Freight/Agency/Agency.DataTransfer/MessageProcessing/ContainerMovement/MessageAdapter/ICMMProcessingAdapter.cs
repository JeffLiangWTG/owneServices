using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing
{
	public interface ICMMProcessingAdapter
	{
		string CountryCode { get; }
		bool CreateMissingContainers { get; }
		BusinessObjectFactory Factory { get; }
		string LloydsNumber { get; }
		string MessageSenderReference { get; }
		string MessageText { get; }
		string MessageTypeTitle { get; }
		string MovementCode { get; }
		string PortCode { get; }
		OrgHeader Sender { get; }
		OrgAddress SenderAddress { get; }
		bool UpdateBookings { get; }
		RefVessel Vessel { get; }
		JobVoyage Voyage { get; }
		string VoyageNumber { get; }
		string TransportMode { get; }

		string MessageSenderCode { get; }
		CMMOrganisationType MessageSenderCodeType { get; }
		CMMMessageType MessageType { get; }

		List<CMMMessageContainer> Containers { get; }

		void AttachMessage(ContainerMovement movement, string status);
		string GetMovementDescription(string movementType);

		bool HasRelatedJobs(string containerNumber);
		void Load();
	}

	public class CMMMessageContainer
	{
		public virtual string BillOfLading { get; set; }
		public virtual string BookingReference { get; set; }
		public virtual string ContainerNumber { get; set; }
		public virtual string ContainerLeaseNumber { get; set; }
		public virtual CMMEquipmentSupplier CmmEquipmentSupplier { get; set; }
		public virtual string GoodsDeclarationNumber { get; set; }
		public virtual decimal? GrossWeightKG { get; set; }
		public virtual bool IsEmpty { get; set; }
		public virtual string ISOType { get; set; }
		public virtual string OwnerType { get; set; }
		public virtual DateTime PositioningDateTime { get; set; }
		public virtual List<string> SealNumbers { get; set; }

		public bool HasValue()
		{
			if (
				!string.IsNullOrEmpty(BillOfLading) ||
				!string.IsNullOrEmpty(BookingReference) ||
				!string.IsNullOrEmpty(ContainerNumber) ||
				!string.IsNullOrEmpty(ContainerLeaseNumber) ||
				CmmEquipmentSupplier != CMMEquipmentSupplier.Unknown ||
				!string.IsNullOrEmpty(GoodsDeclarationNumber) ||
				GrossWeightKG.HasValue ||
				!string.IsNullOrEmpty(ISOType) ||
				!string.IsNullOrEmpty(OwnerType) ||
				SealNumbers.Count > 0
			)
			{
				return true;
			}
			return false;
		}
	}

	public enum CMMEquipmentSupplier
	{
		Unknown,
		Carrier,
		Shipper,
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	public interface IContainerMovementMessageProcessingAdapterTest
	{
		void TestCantFindSender();
		void TestCountryCode_OrgHeader();
		void TestCountryCode_Address();
		void TestCreateMissingContainers();
		void TestGetMovementDescription();
		void TestHasRelatedJobs();
		void TestPropertiesMapping();
		void TestTooManyMatchingSenderOrgs();
		void TestVesselVoyage_NoVoyage();
		void TestVesselVoyage_NoVessel();
	}
}

#endif
#endregion
