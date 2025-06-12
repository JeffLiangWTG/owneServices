using System;
using System.Collections.Generic;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eServices.USCustoms.IntegrationTests
{
	[TestFixture]
	public abstract class GenericTestBase
	{
		[OneTimeSetUp]
		public void ClassInitialize()
		{
			try
			{
				testDataController = new TestDataController(CommonTestDataLocation, TestDataSchemaLocation, GetType().Name, ConnectionStringPattern, TestClassLocationPattern);
				testDataController.LoadTestData();
			}
			catch (Exception ex1)
			{
				try
				{
					testDataController?.RestoreDatabase();
				}
				catch (Exception ex2)
				{
					throw new AggregateException("There was an exception in setup, then a second exception occured during teardown.", ex1, ex2);
				}
				throw ex1;
			}
		}

		[OneTimeTearDown]
		public void ClassCleanup()
		{
			testDataController?.RestoreDatabase();
		}

		[SetUp]
		public void SetUp()
		{
			TestContext.WriteLine(Deployment.GetLog());

			rollbackList = new List<Action>();
			SetUpCore();
		}

		[TearDown]
		public void TearDown()
		{
			TearDownCore();
			foreach (var action in rollbackList)
			{
				action();
			}
		}

		public virtual void SetUpCore()
		{
		}

		public virtual void TearDownCore()
		{
		}

		protected void AddRollback(Action action)
		{
			rollbackList.Add(action);
		}

		TestDataController testDataController;
		List<Action> rollbackList;

		protected virtual string CommonTestDataLocation => null;
		protected virtual string TestDataSchemaLocation => null;
		protected virtual string ConnectionStringPattern => ".Properties.Settings.";
		protected virtual string TestClassLocationPattern => ".*.";
	}
}
