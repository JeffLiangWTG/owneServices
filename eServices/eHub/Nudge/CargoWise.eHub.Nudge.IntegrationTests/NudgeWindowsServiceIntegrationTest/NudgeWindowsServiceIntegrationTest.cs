using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Data.SqlClient;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using eServices.eHubDatabase.Tests.Common;
using NUnit.Framework;

namespace CargoWise.eHub.Nudge.IntegrationTests
{
	[TestFixture]
	[Property("DAT:CapabilityRequirements", "VM,SQL,SQLFILESTREAM")]
	public class NudgeWindowsServiceIntegrationTest
	{
		[Test]
		public void TestOutboxMessageTriggersNudgeCall()
		{
			requestReceived = false;

			try
			{
				StartHttpServer();
				StartCCDSServer();
				PrepareDB();

				NudgeSettings.Instance.LoadApplicationSettings();
				new ReloadSystemInfoManagerForTest().RetrieveUpdatedSystemInfo(DateTime.UtcNow);
				new CheckAndNudgeManagerForTest().CheckAndNudge();

				WaitSignaledOrTimeout();
				Assert.IsTrue(requestReceived, "Nudge request not received");
			}
			finally
			{
				// ensures the thread created by StartHttpServer() is closed, otherwise DAT would consider the test still running.
				listener.Close();
				CleanUpDB();
			}
		}

		void WaitSignaledOrTimeout()
		{
			DateTime startTime = DateTime.Now;
			while (!requestReceived && DateTime.Now < startTime + timeout)
			{
				Thread.Sleep(100);
			}
		}

		[SetUp]
		public void Setup()
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
				CreateUser();
			}
			finally
			{
				TestContext.WriteLine(Deployment.GetLog());
			}

			var dic = ConfigurationManager.GetSection("testsettings") as IDictionary;
			url = dic["serverUrl"].ToString();
		}

		void CreateUser()
		{
			RunInTransaction(
				"IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'IntegrationTestUser') DROP LOGIN [IntegrationTestUser]",
				"CREATE LOGIN [IntegrationTestUser] WITH PASSWORD=N'3hubRock$', DEFAULT_DATABASE=[master], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF ",
				"ALTER LOGIN [IntegrationTestUser] ENABLE",
				"ALTER SERVER ROLE [sysadmin] ADD MEMBER [IntegrationTestUser]"
				);
		}

		void StartHttpServer()
		{
			listener = new HttpListener();
			listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			listener.Prefixes.Add(url);
			listener.Start();
			new Thread(new ThreadStart(delegate
				{
					HttpListenerContext context = listener.GetContext();
					context.Response.StatusCode = 200;
					context.Response.StatusDescription = "Request received";
					context.Response.Close();
					requestReceived = true;
				}
				)).Start();
		}

		void StartCCDSServer()
		{
			listener = new HttpListener();
			listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
			listener.Prefixes.Add("http://localhost:9753/mockhttpserver/ScheduleServiceTasksUri/");
			listener.Start();
			new Thread(new ThreadStart(delegate
			{
				while (listener.IsListening)
				{
					HttpListenerContext context = listener.GetContext();
					HttpListenerResponse response = context.Response;
					response.StatusCode = 200;
					response.ContentType = "application/json";

					string jsonResponse = @"
            {
                ""urls"": [
                    ""http://localhost:9753/mockhttpserver/nudge""
                ]
            }";

					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jsonResponse);
					response.ContentLength64 = buffer.Length;
					using (Stream output = response.OutputStream)
					{
						output.Write(buffer, 0, buffer.Length);
					}

					requestReceived = true;
				}
			})).Start();
		}
		void PrepareDB()
		{
			RunInTransaction(
				"insert into eHubClient (CC_PK, CC_ID, CC_Odyssey_OH, CC_EmailAddress, CC_Password, CC_SystemCategory, CC_OwnerCategory)" +
				" values('88703E18-2A15-4D94-98C9-BE15E4300BB6', 'AAABBBCCC', NEWID(), 'aaa@bbb.cc', '', 'Enterprise', 'WiseTech')",
				"insert into eHubClientSystem (EH_PK, EH_ID, EH_URL)" +
				" values ('57C3547F-6A64-4BE4-8313-56EB2E37E518','AAACCC','http://localhost:9753/mockhttpserver/nudge')",
				"insert into eHubOutboxMessage (OI_PK, OI_CC_Sender, OI_CC_Recipient, OI_Status)" +
				" values ('476AFCB7-1A11-4E8F-ABCD-8B1BA0DDC05E','88703E18-2A15-4D94-98C9-BE15E4300BB6','88703E18-2A15-4D94-98C9-BE15E4300BB6', 0)");
		}

		void CleanUpDB()
		{
			RunInTransaction(
				"delete from eHubOutboxMessage where OI_PK = '476AFCB7-1A11-4E8F-ABCD-8B1BA0DDC05E'",
				"delete from eHubClientSystem where EH_PK = '57C3547F-6A64-4BE4-8313-56EB2E37E518'",
				"delete from eHubClient where CC_PK = '88703E18-2A15-4D94-98C9-BE15E4300BB6'");
		}

		void RunInTransaction(params string[] sqlStatements)
		{
			using (var connection = SqlServerHelper.OpenAdminSqlConnection("eHubTransactions"))
			using (var transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead))
			{
				foreach (var sql in sqlStatements)
				{
					var command = new SqlCommand(sql, connection, transaction)
					{
						CommandTimeout = 0
					};
					command.ExecuteNonQuery();
				}
				transaction.Commit();
			}
		}

		string url;
		HttpListener listener;
		bool requestReceived;
		TimeSpan timeout = TimeSpan.FromSeconds(10);
	}

	class ReloadSystemInfoManagerForTest : ReloadSystemInfoManager
	{
		internal override IDbConnection OpenConnection()
		{
			return SqlServerHelper.OpenAdminSqlConnection("eHubTransactions");
		}
	}

	class CheckAndNudgeManagerForTest : CheckAndNudgeManager
	{
		internal override IDbConnection OpenConnection()
		{
			return SqlServerHelper.OpenAdminSqlConnection("eHubTransactions");
		}
	}
}
