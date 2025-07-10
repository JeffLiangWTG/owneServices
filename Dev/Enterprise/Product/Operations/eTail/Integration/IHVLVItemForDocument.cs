using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVItemForDocument : IHVLVItem
	{
		ZBool HasItemLines { get; }
		ZString HVI_Status_Description { get; }
		ZBool IsLastMileCarrierBooked { get; }
		ZBool IsScannedCleared { get; }
		ZBool IsScannedHeld { get; }
		ZBool IsScannedSurplus { get; }
		ZBool IsScannedNotReported { get; }

		IDocManagerInfo DocManagerInfo { get; }
		IOrgHeader ETailer { get; }
		Enterprise.Integration.TransportBooking.IDtbBooking LastMileTransportBooking { get; }
		Enterprise.Integration.Forwarding.IForwardingShipment Shipment { get; }
		IHVLVOuterPackage OuterPackage { get; }
		IHVLVOriginLoadList LoadList { get; }
		IHVLVConsignment Consignment { get; }
		IHVLVItemLineCollection Lines { get; }

		Logs Logs { get; }
		Notes Notes { get; }
	}
}
