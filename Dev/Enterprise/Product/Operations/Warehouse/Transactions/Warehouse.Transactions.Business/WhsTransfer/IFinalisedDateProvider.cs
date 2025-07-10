using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	interface IFinalisedDateProvider
	{
		ZDateTimeOffset GetFinalisationTimeOffset();
	}
}
