using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusOutturnHeaderTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetC6_SendersMessageReference_NoDuplicateReference()
		{
			var factory1 = new BusinessObjectFactory();
			var header = factory1.NewWithValidTestData<CusOutturnHeader>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var senderMessageReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				((ISendersMessageReferenceProvider)(header)).PopulateSendersReferenceIfNeeded();
				senderMessageReference = header.C6_SendersMessageReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = newFactory.NewWithValidTestData<CusOutturnHeader>();
			header2.C6_OutturningPremiseID = "W7DAHW0QM8";
			newFactory.Save();
			AssertEquals(senderMessageReference, header2.C6_SendersMessageReference);

			AssertEquals(senderMessageReference, header.C6_SendersMessageReference);
			factory1.Save();
			AssertNotEquals(senderMessageReference, header.C6_SendersMessageReference);
		}
	}
}
