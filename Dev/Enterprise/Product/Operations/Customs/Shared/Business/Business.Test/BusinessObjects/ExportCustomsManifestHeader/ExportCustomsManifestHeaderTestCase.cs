using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ExportCustomsManifestHeaderTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetED_BGMReference_NoDuplicateReferenceException()
		{
			var factory1 = new BusinessObjectFactory();
			var header = factory1.New<ExportCustomsManifestHeader>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var senderMessageReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				header.PopulateED_BGMReferenceIfNeeded();
				senderMessageReference = header.ED_BGMReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.New<ExportCustomsManifestHeader>();
			newFactory.Save();
			AssertEquals(senderMessageReference, header2.ED_BGMReference);

			AssertEquals(senderMessageReference, header.ED_BGMReference);
			factory1.Save();
			AssertNotEquals(senderMessageReference, header.ED_BGMReference);
		}
	}
}
