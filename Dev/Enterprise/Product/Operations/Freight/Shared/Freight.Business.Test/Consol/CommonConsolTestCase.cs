using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestOnSavingSetJK_UniqueConsignRef_NoDuplicateRef()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<CommonConsol>();

			var uniqueConsignRef = ZString.Empty;
			var dbConnection = ((IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				consol.OnSaving();
				uniqueConsignRef = consol.JK_UniqueConsignRef;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var consol2 = newFactory.New<CommonConsol>();
			newFactory.Save();
			AssertEquals("The consol2 will get the consol.UniqueConsignRef due to number fountain rollback", uniqueConsignRef, consol2.JK_UniqueConsignRef);

			AssertEquals("consol.JK_UniqueConsignRef is still UniqueConsignRef under the same transaction before successfully saved", uniqueConsignRef, consol.JK_UniqueConsignRef);
			factory.Save();
			AssertNotEquals("Saved with no duplicate error and consol get another UniqueConsignRef", uniqueConsignRef, consol.JK_UniqueConsignRef);
		}
	}
}
