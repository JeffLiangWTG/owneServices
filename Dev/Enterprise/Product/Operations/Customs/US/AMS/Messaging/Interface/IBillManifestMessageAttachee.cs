using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.AMS.Messaging.Interface
{
	public interface ICommonBillManifestMessageAttachee : IManifestMessageAttachee
	{
		IPort PortDetails { get; }
		void UpdateOutgoingBillStatus();
	}

	public interface IACEBillManifestMessageAttachee : ICommonBillManifestMessageAttachee
	{
		IACEBillOfLading BillOfLadingDetails { get; }

		ZDateTime EventDateTime { get; }
		ZString ForeignDeparturePort { get; }
		ZString BillPKAsString { get; }
		AMSBillEDIMessageCollection BillMessages { get; }
	}

	public interface IVesselArrivalMessageAttachee
	{
		void UpdateActualArrivalDate(ZString portOfUnlading, ZDateTime dateTime, ZString messageSubType);
	}
}
