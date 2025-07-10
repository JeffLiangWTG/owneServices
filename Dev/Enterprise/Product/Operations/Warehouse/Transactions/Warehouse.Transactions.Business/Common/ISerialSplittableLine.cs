using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ISerialSplittableLine
	{
		bool HasErrors { get; }
		bool IsFinalised { get; }
		bool IsSplittableProduct { get; }
		ZDecimal Units { get; }

		void SplitWhenSerialNumberExists();
	}
}
