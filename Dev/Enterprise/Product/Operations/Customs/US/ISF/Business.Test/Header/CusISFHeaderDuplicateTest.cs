using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class CusISFHeaderDuplicateTest : NUnit.Framework.TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetsJobReference_NoDuplicateReferenceException()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<CusISFHeader>();
			var jobReference = ZString.Empty;
			var dbConnection = ((CargoWise.Data.IDbConnected)factory).Connection;
			try
			{
				dbConnection.BeginTransaction();
				header.PopulateJobReferenceIfNeeded();
				jobReference = header.BF_JobReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<CusISFHeader>();
			newFactory.Save();
			AssertEquals(jobReference, header2.BF_JobReference);
			AssertEquals(jobReference, header.BF_JobReference);
			factory.Save();
			AssertNotEquals(jobReference, header.BF_JobReference);
		}
	}
}
