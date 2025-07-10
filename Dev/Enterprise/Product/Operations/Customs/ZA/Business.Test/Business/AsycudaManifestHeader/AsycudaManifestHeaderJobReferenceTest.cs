using CargoWise.Data.Testing;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class AsycudaManifestHeaderJobReferenceTest : NUnit.Framework.TestCase
	{
		[UseSnapshotProtection]
		public void TestOnSavingSetAMA_JobReference_NoDuplicateReference()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<AsycudaManifestHeader>();
			var jobReference = string.Empty;

			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				header.OnSaving();
				jobReference = header.AMA_JobReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<AsycudaManifestHeader>();
			newFactory.Save();
			AssertEquals("The jobReference which header get first is set to header2", jobReference, header2.AMA_JobReference);

			AssertEquals("header.AMA_JobReference is still jobReference under the same transaction before successfully saved", jobReference, header.AMA_JobReference);
			factory.Save();
			AssertNotEquals("Saved with no duplicate number error and header get another jobReference", jobReference, header.AMA_JobReference);
		}
	}
}
