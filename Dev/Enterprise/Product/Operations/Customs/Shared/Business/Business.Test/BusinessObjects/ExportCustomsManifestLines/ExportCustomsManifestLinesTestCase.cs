using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ExportCustomsManifestLinesTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetEL_UserReferenceNum_NoDuplicateReference()
		{
			var factory1 = new BusinessObjectFactory();
			var header = factory1.NewWithValidTestData<ExportCustomsManifestHeader>();
			var line = header.Lines.AddNew();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var referenceNum = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				((ISendersMessageReferenceProvider)(line)).PopulateSendersReferenceIfNeeded();
				referenceNum = line.EL_UserReferenceNum;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.NewWithValidTestData<ExportCustomsManifestHeader>();
			header2.ED_BGMReference = "3S57P2I6C7A7R26";
			var line2 = header2.Lines.AddNew();
			newFactory.Save();
			AssertEquals(referenceNum, line2.EL_UserReferenceNum);

			AssertEquals(referenceNum, line.EL_UserReferenceNum);
			factory1.Save();
			AssertNotEquals(referenceNum, line.EL_UserReferenceNum);
		}
	}
}
