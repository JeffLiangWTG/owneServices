using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eServices.TestHelpers.Database.Common;

namespace CargoWise.eHub.Products.NZCustoms.IntegrationTests
{
	public class BaseNZCustomsTest
	{
		public void InsertMessageReferenceToDatabase(string clientID, string applicationCode, string messageReference)
		{
			using (var connection = SqlServerHelper.OpenAdminSqlConnection("CargoWise.eHub.DataAccess.Sql.eHubTransactions"))
			{
				var command = connection.CreateCommand();
				command.CommandType = System.Data.CommandType.StoredProcedure;
				command.CommandText = "InsertMessageReference";
				command.Parameters.AddWithValue("@ClientID", clientID);
				command.Parameters.AddWithValue("@ApplicationCode", applicationCode);
				command.Parameters.AddWithValue("@MessageReference", messageReference);

				command.ExecuteNonQuery();
			}
		}

		public void ValidateMessageInEHubInboxTable(Guid messageTrackingID)
		{
			using (var connection = SqlServerHelper.OpenAdminSqlConnection("CargoWise.eHub.DataAccess.Sql.eHubTransactions"))
			{
				var command = connection.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = "SELECT COUNT(*) as count from eHubInboxMessage WHERE EI_MessageTrackingID = @MessageTrackingID";
				command.Parameters.AddWithValue("@MessageTrackingID", messageTrackingID);

				var count = (int)command.ExecuteScalar();
				Assert.AreEqual(1, count, "Expected 1 record should be added to eHubInbox table");
			}
		}

		public void ValidateMessageInEHubInboxTable(string messageReference)
		{
			using (var connection = SqlServerHelper.OpenAdminSqlConnection("CargoWise.eHub.DataAccess.Sql.eHubTransactions"))
			{
				var command = connection.CreateCommand();
				command.CommandType = System.Data.CommandType.Text;
				command.CommandText = "SELECT COUNT(*) as count from eHubInboxMessage WHERE dbo.DecodeAndDecompress(EI_Content) like @Reference";
				command.Parameters.AddWithValue("@Reference", String.Format("%{0}%", messageReference));

				var count = (int)command.ExecuteScalar();
				Assert.AreEqual(1, count, "Expected 1 record should be added to eHubInbox table");
			}
		}

		public int GenerateInterchangeNumber()
		{
			Random random = new Random();
			return random.Next(0, 1000);
		}

		public string GenerateMessageReference()
		{
			Random random = new Random();
			return String.Format("B{0}", random.Next(0, 100000000).ToString());
		}

		public int GenerateMessageNumber()
		{
			Random random = new Random();
			return random.Next(0, 1000);
		}

		public void AssertXmlStream(string expected, string actual)
		{
			using (var expectedStream = ConvertToStream(expected))
			{
				using (var actualStream = ConvertToStream(actual))
				{
					var tool = new XmlDiffTool();
					var compareResult = tool.Execute(actualStream, expectedStream);
					try { Assert.IsTrue(compareResult.Success); }
					catch (AssertFailedException) { throw new AssertFailedException(compareResult.OutputUpdateGram); }
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public Stream ConvertToStream(string text)
		{
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(text);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
