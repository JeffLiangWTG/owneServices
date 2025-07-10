using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class TG_WhsDocketLine_ConcurrencyTest : TestCase
	{
		#region DisablePageLocks

		protected virtual void DisablePageLocks(DbConnection connection)
		{
			// Page locks have caused interesting locking behaviour in our tests in the past.
			// For peace of mind, I'll disable every page lock (that exists as of writing) possibly related to my test.
			connection.ExecuteNonQuery(@"
				ALTER INDEX ALL ON WhsDocketLine SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsDocket SET(ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsLocation SET (ALLOW_PAGE_LOCKS = OFF)
				ALTER INDEX ALL ON WhsRow SET (ALLOW_PAGE_LOCKS = OFF)");
		}

		#endregion
	}
}
