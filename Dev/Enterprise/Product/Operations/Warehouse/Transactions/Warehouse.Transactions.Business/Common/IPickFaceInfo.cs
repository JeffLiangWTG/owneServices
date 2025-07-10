using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPickFaceInfo : IFixedPickFaceInfo
	{
		ZBool IsDynamicTransfer { get; }
		ZDate OrderedExpiryDate { get; }
		ZDate OrderedPackingDate { get; }
		ZString OrderedAttribute1 { get; }
		ZString OrderedAttribute2 { get; }
		ZString OrderedAttribute3 { get; }
		ZString OrderedSerialNumber { get; }
	}
}
