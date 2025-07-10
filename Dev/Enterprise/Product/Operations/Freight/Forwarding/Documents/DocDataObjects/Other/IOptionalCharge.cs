using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IOptionalCharge
	{
		ZBool IsPrepaid { get; }
		ZBool IsCollect { get; }

		ZBool IsPayableElsewhere { get; }
	}
}
