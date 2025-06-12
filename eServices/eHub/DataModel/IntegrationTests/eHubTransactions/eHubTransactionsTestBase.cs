using System;
using System.Data.Entity;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using eServices.eHubDatabase.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eHub.DataModel.IntegrationTests.eHubTransactions
{
	[Property("DAT:CapabilityRequirements", "VM,SQL,SQLFILESTREAM")]
	public abstract class eHubTransactionsTestBase : DataModelIntegrationTestBase
	{
		DbContextTransaction Transaction { get; set; }

		protected override void TestFixtureSetUpCore()
		{
            try
            {
                Deployment.Deploy(Deployments.EHubTransactions, Deployments.EHubArchiveOnlineView);
            }
            catch
            {
                Deployment.Recreate(Deployments.EHubTransactions, Deployments.EHubArchiveOnlineView);
            }
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
		}

		protected override void TearDownCore()
		{
			base.TearDownCore();
		}

		internal static Func<eHubTransactionsContext> ContextFactory =
			() => new eHubTransactionsContext(SqlServerHelper.GetAdminConnectionString("eHubTransactions"));
		internal static Func<eHubTransactionsContext> EHubArchiveOnlineViewContextFactory =
			() => new eHubTransactionsContext(SqlServerHelper.GetAdminConnectionString("eHubArchiveOnlineView"));
	}
}
