using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusUnderbondTestCase : TestCase
	{
		[UseSnapshotProtection]
		public void TestSavingSetC4_SendersMessageReference_NoDuplicateReferenceException()
		{
			var factory1 = new BusinessObjectFactory();
			var header = (CusUnderbond)factory1.New<Integration.Customs.AU.ICusUnderbond>();
			var dbConnection = ((CargoWise.Data.IDbConnected)factory1).Connection;
			var senderMessageReference = ZString.Empty;
			try
			{
				dbConnection.BeginTransaction();
				header.PopulateC4_SendersMessageReferenceIfNeeded();
				senderMessageReference = header.C4_SendersMessageReference;
			}
			finally
			{
				dbConnection.RollbackTransaction();
			}

			var newFactory = new BusinessObjectFactory();
			var header2 = (CusUnderbond)newFactory.New<Integration.Customs.AU.ICusUnderbond>();
			newFactory.Save();
			AssertEquals(senderMessageReference, header2.C4_SendersMessageReference);

			AssertEquals(senderMessageReference, header.C4_SendersMessageReference);
			factory1.Save();
			AssertNotEquals(senderMessageReference, header.C4_SendersMessageReference);
		}
	}
}
