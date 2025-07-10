using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Freight.Business
{
	public class ConcurrencyChecker : SaveInTransactionActionWithFactory
	{
		protected ConcurrencyChecker(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			SaveInTransactionCore();

			return ChangedTableNames.Empty;
		}

		protected virtual void SaveInTransactionCore()
		{
		}

		protected sealed override bool AllowTransactionWithOtherParticipant => true;
	}
}
