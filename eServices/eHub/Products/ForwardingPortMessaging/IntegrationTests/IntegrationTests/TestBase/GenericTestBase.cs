using System;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using eServices.eHubDatabase.Tests.Common;
using eServices.eHubDataModel.eHubTransactions;
using NUnit.Framework;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.IntegrationTests
{
    [TestFixture]
	[Property("DAT:CapabilityRequirements", "VM,SQL,SQLFILESTREAM")]
	public abstract class GenericTestBase
	{
		[OneTimeSetUp]
		public void ClassInitialize()
		{
			try
			{
                try
                {
                    Deployment.Deploy(Deployments.EHubTransactions, Deployments.EHubArchiveOnlineSecondary);
                }
                catch
                {
                    Deployment.Recreate(Deployments.EHubTransactions, Deployments.EHubArchiveOnlineSecondary);
                }

				TestDataController = new TestDataController(CommonTestDataLocation, TestDataSchemaLocation, this.GetType().Name, ConnectionStringPattern, TestClassLocationPattern);
				TestDataController.LoadTestData();
			}
			catch (Exception)
			{
				TestDataController?.RestoreDatabase();
				throw;
			}
		}

		[OneTimeTearDown]
		public void ClassCleanup()
		{
			TestDataController?.RestoreDatabase();
		}

		protected virtual void SetUpCore()
		{
		}

		protected virtual void TearDownCore()
		{
		}

		[SetUp]
		public void SetUp()
		{
			TestContext.WriteLine(Deployment.GetLog());

			SetUpCore();
		}

		[TearDown]
		public void TearDown()
		{
			TearDownCore();
		}

		protected eHubTransactionsContext CreateeHubTransactionsContext() =>
			new eHubTransactionsContext(SqlServerHelper.GetAdminConnectionString("eHubTransactions"));

		private TestDataController TestDataController;

		protected virtual string CommonTestDataLocation { get { return ".TestDataBase.TestData."; } }
		protected virtual string TestDataSchemaLocation { get { return ".TestDataBase.Schemas."; } }
		protected virtual string ConnectionStringPattern { get { return ".Properties.Settings."; } }
		protected virtual string TestClassLocationPattern { get { return ".*."; } }
	}
}
