using System;
using System.Data.SqlClient;
using System.IO;
using CargoWise.eHub.Integration;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Snapshot;
using CargoWise.eServices.USCustoms.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.USCustoms.IntegrationTests.GatewayServices
{
	[UseSnapshotProtection("eHubTransactions")]
	public class ReferenceFileTests : GenericTestBase
	{
		protected override string CommonTestDataLocation => ".GatewayServices.Data.";
		protected override string TestDataSchemaLocation => ".TestBases.Schemas.";

		[Test]
		[RestoreSnapshot]
		public void TestSendReferenceFileRequest()
		{
			var mockQueuer = new Mock<IMessageQueuer>();
			mockQueuer.Setup(q => q.EnqueueMessage(It.IsAny<SqlTransaction>(), "USC_REF", It.IsAny<Guid>(), "USI", "HB", It.IsAny<Stream>()));

			var utcNow = DateTime.UtcNow.ToString("MMddyy");
			var expectedContent = $@"<USCustoms><Header><![CDATA[A3910SV9      {utcNow}     HB                                          USC_ATF2304]]></Header><Body><![CDATA[B  3910SV9HB                                               USC_REF              F1102304                                                                        Y  3910SV9HB                                                                    ]]></Body><Footer><![CDATA[Z3910SV9      {utcNow}]]></Footer></USCustoms>";

			using (var connection = SqlServerHelper.GetAdminSqlConnection("CargoWise.eServices.USCustoms.IntegrationTests.Properties.Settings.eHubTransactions"))
			{
				ReferenceFile.SendReferenceFileRequest(connection, "USC_ATF2304", mockQueuer.Object);

				connection.Open();
				using (var command = new SqlCommand("SELECT dbo.DecodeAndDecompress(EI_Content) FROM eHubInboxMessage", connection))
				{
					using (var reader = command.ExecuteReader())
					{
						Assert.That(reader.Read(), Is.True);
						Assert.That(reader.GetString(0), Is.EqualTo(expectedContent));
						Assert.That(reader.Read(), Is.False);
					}
				}
			}

			mockQueuer.Verify(q => q.EnqueueMessage(It.IsAny<SqlTransaction>(), "USC_REF", It.IsAny<Guid>(), "USI", "HB", It.IsAny<Stream>()),
				Times.Once());
		}
	}
}
