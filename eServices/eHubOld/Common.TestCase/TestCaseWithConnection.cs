using System;
using System.Data.EntityClient;
using CargoWise.eServices.eHub.Common.EntityModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eServices.eHub.Common.TestCase
{
	public abstract class TestCaseWithConnection
	{
		[TestInitialize]
		public void SetUp()
		{
			//Connection = new EntityConnection(ConnectionString);
			//Connection.Open();
			//Transaction = Connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted);
			//Factory = new EntityFactory(ConnectionString);
		}
		//EntityConnection Connection;
		//EntityTransaction Transaction;

		[TestCleanup]
		public void TearDown()
		{
			//Transaction.Rollback();
			//Transaction.Dispose();
			//Connection.Close();
			//Connection.Dispose();
			//Factory.Dispose();
		}

		#region Asserts

		public void AssertHasException(Action action)
		{
			try { action(); }
			catch { return; }
			Assert.Fail("No exception occured");
		}

		public void AssertNoException(Action action)
		{
			try { action(); }
			catch (Exception ex) { Assert.Fail(string.Format("Expect no exception, but occured: {0}", ex.Message)); }
			return;
		}

		#endregion

		protected abstract EntityFactory Factory { get; }
		protected abstract string ConnectionString { get; }
	}
}