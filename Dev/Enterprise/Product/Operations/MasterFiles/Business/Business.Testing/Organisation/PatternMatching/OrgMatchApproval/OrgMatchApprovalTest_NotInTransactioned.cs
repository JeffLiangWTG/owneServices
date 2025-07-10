using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[UseSnapshotProtection]
	sealed class OrgMatchApprovalTest_NotInTransactioned : TestCase
	{
		public void TestAddressToBeMatched_DuringSaveTransaction()
		{
			AssertEquals("This test cannot run in a transaction otherwise it will deadlock", false, Db.Connection.IsInTransaction);

			ZGuid addressPK = DummyMatchApproval.AddressToBeMatched.PK;
			Thread threadForOtherConnection = new Thread(new ThreadStart(SimulateDelayedCompletingSaveOfAddress_InOtherConnectionAndTransactionContext));
			threadForOtherConnection.Start();

			// wait for the other thread to finish renaming OrgPatternMatchAddress.P3_PK
			ThreadWaitMutex.WaitOne();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgMatchApproval loadedMatchApproval = (OrgMatchApproval)newFactory.Load(typeof(OrgMatchApproval), DummyMatchApproval.PK);
			OrgPatternMatchAddress address = loadedMatchApproval.AddressToBeMatched;
			AssertEquals("Should load the address correctly, even though the concurrent transaction hasn't completed", false, address.IsNull);
			AssertEquals("Should load the address correctly, even though the concurrent transaction hasn't completed", addressPK, address.PK);

			threadForOtherConnection.Join();
		}
		readonly AutoResetEvent ThreadWaitMutex = new AutoResetEvent(false);

		void SimulateDelayedCompletingSaveOfAddress_InOtherConnectionAndTransactionContext()
		{
			using (DbConnection extraConnection = Db.NewExtraConnectionToMainDb())
			{
				ZGuid addressPK = DummyMatchApproval.AddressToBeMatched.PK;
				ZGuid renamedAddressPK = ZGuid.NewZGuid();

				extraConnection.BeginTransaction();
				SetP3_PKInDB(extraConnection, addressPK, renamedAddressPK);

				// notify the unit test thread OrgPatternMatchAddress.P3_PK has been renamed and give it time to complete the test
				ThreadWaitMutex.Set();
				Thread.Sleep(1000);

				SetP3_PKInDB(extraConnection, renamedAddressPK, addressPK);
				extraConnection.CommitTransaction();
			}
		}

		void SetP3_PKInDB(DbConnection extraConnection, ZGuid oldAddressPK, ZGuid newAddressPK)
		{
			DbCommand command = extraConnection.Command("UPDATE dbo.OrgPatternMatchAddress SET P3_PK='" + newAddressPK + "' WHERE P3_PK='" + oldAddressPK + "'");
			command.ExecuteNonQuery();
		}

		#region Implementation
		BusinessObjectFactory Factory;
		DummyEnterpriseBusinessObject DummyParent;
		DummyOrgMatchApproval DummyMatchApproval;
		OrgMatchApproval.Loader Loader;

		protected override void SetUp()
		{
			base.SetUp();

			Factory = new BusinessObjectFactory();
			DummyParent = Factory.New<DummyEnterpriseBusinessObject>();
			Loader = new OrgMatchApproval.Loader(Factory);
			DummyMatchApproval = (DummyOrgMatchApproval)Loader.LoadOrCreate(DummyParent.PK, OrgMatchApprovalType.DummyType);
			Factory.Save();
		}

		#endregion
	}
}
