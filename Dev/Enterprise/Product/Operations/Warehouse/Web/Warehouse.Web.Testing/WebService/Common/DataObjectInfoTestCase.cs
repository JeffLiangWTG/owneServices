using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	[TestsSubclassesOf(typeof(DataObjectInfo))]
	public abstract class DataObjectInfoTestCase<T> : TestCaseWithFactory
		where T : class, new()
	{
		public DataObjectInfoTestCase()
		{
		}

		#region Test Cases

		public void TestConstructors()
		{
			AssertNotNull(Parent);
			AssertNotEquals(Parent, GetNewObjectInfo());
		}

		#endregion

		public void TestParameterlessConstructor_ShouldNotUseDbConnection()
		{
			var lastError = string.Empty;
			var thread = new Thread(() =>
			{
				try
				{
					ErrorReporter.Clear();

					new T();

					lastError = ErrorReporter.LastMessageReported;
					ErrorReporter.Clear();
				}
				catch (Exception ex)
				{
					lastError = ex.Message.IsNullOrEmpty() ? "Exception thrown" : ex.Message;
				}
			});
			thread.Start();
			thread.Join(1000);

			// Should not be "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()"
			AssertEquals("Should be empty.", string.Empty, lastError);
		}

		public void TestParameterlessConstructor_ShouldNotDemandResourceString()
		{
			var resourceDemanded = false;
			Res.ResourceDemanded += (sender, e) => { resourceDemanded = true; };

			new T();

			AssertEquals("Should not be doing Res.GetString in parameterless constructors.", false, resourceDemanded);
		}

		#region Implementation

		#region Helper

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion

		#region Data

		protected TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory)); }
		}

		TestDataSimpleEnvironment data;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			parent = GetNewObjectInfo();
		}

		protected abstract DataObjectInfo GetNewObjectInfo();

		protected DataObjectInfo Parent
		{
			get { return parent; }
		}

		DataObjectInfo parent;

		#endregion
	}
}
