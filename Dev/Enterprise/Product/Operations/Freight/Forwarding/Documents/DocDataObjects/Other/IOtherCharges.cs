using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IOtherCharges
	{
		ZBool IsPrepaid { get; }
		ZBool IsCollect { get; }
		ZBool IsFree { get; }
		ZBool IsPayableElsewhere { get; }
		ZBool IsFirstLinePrepaidLineSecondCollect { get; }
		ZString Remarks { get; }
	}
}
