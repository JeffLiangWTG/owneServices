using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManArrivalPortTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetBA_SendersMessageReference_NoDuplicateReference()
		{
			var factory1 = new BusinessObjectFactory();
			var header = factory1.NewWithValidTestData<CusSeaManTranHead>();
			var port = header.Arrivals.AddNew();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var senderMessageReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				((ISendersMessageReferenceProvider)port).PopulateSendersReferenceIfNeeded();
				senderMessageReference = port.BA_SendersMessageReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.NewWithValidTestData<CusSeaManTranHead>();
			header2.BT_VoyageNum = "7042PGE0F2";
			var port2 = header2.Arrivals.AddNew();
			newFactory.Save();
			AssertEquals(senderMessageReference, port2.BA_SendersMessageReference);

			AssertEquals(senderMessageReference, port.BA_SendersMessageReference);
			factory1.Save();
			AssertNotEquals(senderMessageReference, port.BA_SendersMessageReference);
		}
	}
}
