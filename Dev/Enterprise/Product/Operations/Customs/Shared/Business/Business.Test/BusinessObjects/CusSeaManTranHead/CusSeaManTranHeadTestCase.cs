using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManTranHeadTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetBT_SendersMessageReference_NoDuplicateReference()
		{
			var factory1 = new BusinessObjectFactory();
			var header = factory1.NewWithValidTestData<CusSeaManTranHead>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var senderMessageReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				((ISendersMessageReferenceProvider)(header)).PopulateSendersReferenceIfNeeded();
				senderMessageReference = header.BT_SendersMessageReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.NewWithValidTestData<CusSeaManTranHead>();
			header2.BT_VoyageNum = "7042PGE0F2";
			newFactory.Save();
			AssertEquals(senderMessageReference, header2.BT_SendersMessageReference);

			AssertEquals(senderMessageReference, header.BT_SendersMessageReference);
			factory1.Save();
			AssertNotEquals(senderMessageReference, header.BT_SendersMessageReference);
		}
	}
}
