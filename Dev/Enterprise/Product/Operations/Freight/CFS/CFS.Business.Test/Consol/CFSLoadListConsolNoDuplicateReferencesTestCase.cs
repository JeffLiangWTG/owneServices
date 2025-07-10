using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	class CFSLoadListConsolNoDuplicateReferencesTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestOnSavingSetJK_UniqueConsignRef_NoDuplicateRef()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<CFSLoadListConsol>();
			AssertNull(consol.Job);
			Assert(!consol.OnSaveWillChangeFromCFSJobNumberToForwardingJobNumber);

			var uniqueConsignRef = ZString.Empty;
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
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
			var consol2 = newFactory.New<CFSLoadListConsol>();
			newFactory.Save();
			AssertEquals("The UniqueConsignRef which consol get first is set to consol2", uniqueConsignRef, consol2.JK_UniqueConsignRef);

			AssertEquals("consol.JK_UniqueConsignRef is still UniqueConsignRef under the same transaction before successfully saved", uniqueConsignRef, consol.JK_UniqueConsignRef);
			factory.Save();
			AssertNotEquals("Saved with no duplicate error and consol get another UniqueConsignRef", uniqueConsignRef, consol.JK_UniqueConsignRef);
		}
	}
}
