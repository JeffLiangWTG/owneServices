using CargoWise.Types;
using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public interface IConsignmentAction
	{
		ZGuid PK { get; }
		string KeyForCache { get; set; }
		ZInt TotalPackages { get; }
		ZDecimal TotalVolume { get; }
		ZDecimal TotalWeight { get; }
		ZString TotalWeightUnit { get; }
		ZString TotalVolumeUnit { get; }
		ZInt ActionQuantity { get; }
		ZDateTime Estimated { get; }
		ZDateTime RequiredFrom { get; }
		ZDateTime RequiredTo { get; }
		ZString ReferenceNumber { get; }
		ZString ActionType { get; }
		ZString ReceivedBy { get; }
		ZBlob ReceivedBySignature { get; }
		bool IsPickUp { get; }
		bool IsDelivery { get; }
		IConsignmentAddress ConsignmentAddress { get; }
		ZBool IsEmptyContainer { get; }
		AutoDtbBookingInstructionPkgDivot PackageDivot { get; }
		ZString ConsignorOrConsigneeAddress { get; }
	}
}
