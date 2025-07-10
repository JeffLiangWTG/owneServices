using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common.Test
{
	[TestFixture]
	[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
	[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
	public class OdbcConnectionHelperFixture
	{
		[Test]
		public void TestConnection()
		{
			var emptyMdbFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestFiles\Empty.mdb");

			var connection = OdbcConnectionHelper.GetOdbcConnection(emptyMdbFilePath);
			Assert.IsNotNull(connection);
			connection.Dispose();
		}
	}
}
