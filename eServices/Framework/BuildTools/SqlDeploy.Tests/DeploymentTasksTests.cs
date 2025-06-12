using System.Collections.Generic;
using System.IO;
using Microsoft.SqlServer.Dac;
using NUnit.Framework;

namespace eServices.BuildTools.SqlDeploy.Tests
{
	public class DeploymentTasksTests
	{
		[Test]
		public void DeploymentTasks_DeserializeDeploymentInfos()
		{
			var deployments = DeploymentTasks.GetDefaultDeploymentInfos();

			Assert.Multiple(() =>
			{
				Assert.That(deployments.Count, Is.EqualTo(7));
				Assert.That(deployments["ediProd"].DeployOptions.SqlCommandVariableValues,
					Is.EquivalentTo(new Dictionary<string, string>
					{
						{ "ediProdDB", "ediProd" },
						{ "UpdateCacheServer", "localhost" },
					}));
				Assert.That(deployments["ediProd"].DeployOptions.ExcludeObjectTypes, Is.EquivalentTo(new[]
				{
					ObjectType.Permissions,
					ObjectType.RoleMembership,
					ObjectType.Users
				}));
			});
		}

		[Test, Explicit("Will only work in development after building eServices.eHubDatabase solution separately.")]
		[TestCase("eHub")]
		[TestCase("ediProd")]
		[TestCase("ediProdCache")]
		[TestCase("eHubTransactions")]
		[TestCase("eHubArchiveOnline")]
		[TestCase("eHubArchiveOnlineMonthly")]
		[TestCase("eHubArchiveOnlineView")]
		public void DeploymentTasks_DeployDatabase(string databaseName)
		{
			var deployments = DeploymentTasks.GetDefaultDeploymentInfos(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\..\..\..\..\Shared\eHubDatabase\Bin"), TestContext.Progress.WriteLine);

#if !DEBUG
			DeploymentTasks.DropDatabase(deployments[databaseName], false);
#endif

			try
			{
				DeploymentTasks.DeployDatabase(deployments[databaseName]);

			}
			finally
			{
#if !DEBUG
				DeploymentTasks.DropDatabase(deployments[databaseName], false);
#endif
			}
		}
	}
}