using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class SetTransportReferenceFromMatchingDetailsProcessorTest_NotInTrasaction : TestCase
	{
		#region TestProcess

		[UseSnapshotProtection]
		public void TestProcess()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess(Factory, Helper);
		}

		#endregion

		#region TestProcessRanking

		[UseSnapshotProtection]
		public void TestProcessRanking()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcessRanking(Factory, Helper);
		}

		#endregion

		#region TestProcess_Error_InvalidOrder

		[UseSnapshotProtection]
		public void TestProcess_Error_InvalidOrder()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess_Error_InvalidOrder(Factory, Helper);
		}

		#endregion

		#region TestProcess_Error_NoNumberFountain

		[UseSnapshotProtection]
		public void TestProcess_Error_NoNumberFountain()
		{
			SetTransportReferenceFromMatchingDetailsProcessorTestCore.TestProcess_Error_NoNumberFountain(Factory, Helper);
		}

		#endregion

		#region Helpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory(TestCaseDbConnection)); }
		}
		BusinessObjectFactory factory;

		#endregion

		#region TestCaseDbConnection

		DbConnection TestCaseDbConnection
		{
			get { return testCaseDbConnection ?? (testCaseDbConnection = Db.NewExtraConnectionToMainDb()); }
		}
		DbConnection testCaseDbConnection;

		#endregion

		#region TearDown

		protected override void TearDown()
		{
			testCaseDbConnection.Dispose();
			testCaseDbConnection = null;
		}

		#endregion
	}
}
