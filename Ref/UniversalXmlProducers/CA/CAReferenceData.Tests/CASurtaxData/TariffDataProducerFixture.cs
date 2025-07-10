using System;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.CAReferenceData.Business.CASurtax;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CAReferenceData.Tests.CASurtaxData
{
	[TestFixture]
	class TariffDataProducerFixture
	{
		[Test]
		[Platform("64-bit", Reason = "Only support 64 bit ODBC driver")]
		[Property("DAT:CapabilityRequirements", (int)RefDbRepoMachineCapabilityRequirements.CanConnectToOdbc)]
		public void GetTariff_Integration()
		{
			var mdbFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"CASIMAData\TestFiles\CASIMASmall.mdb");
			using (var obdbConnection = OdbcConnectionHelper.GetOdbcConnection(mdbFile))
			{
				obdbConnection.Open();
				var producer = new TariffDataProducer(obdbConnection);
				var result = producer.GetTariff(new[] { "0103.91.00" }, 10).ToArray();
				AssertTariff(result);
				result = producer.GetTariff(new[] { "0103.91.00.00" }, 13).ToArray();
				AssertTariff(result);
			}
		}

		void AssertTariff(RefCusTariff[] tariff)
		{
			Assert.AreEqual(1, tariff.Length);
			Assert.AreEqual("0103910000", tariff[0].ZZ1_TariffCode);
			Assert.AreEqual("Weighing less than 50 kg", tariff[0].ZZ1_Description);
			Assert.AreEqual(new DateTime(1998, 01, 01), tariff[0].ZZ1_StartDate);
		}

		[Test]
		public void GetTariff()
		{
			var conn = new Mock<IDbConnection>();
			var cmd = new Mock<IDbCommand>();
			conn.Setup(x => x.CreateCommand()).Returns(cmd.Object);
			var reader = new Mock<IDataReader>();
			cmd.Setup(x => x.ExecuteReader()).Returns(reader.Object);
			var count = 0;
			reader.Setup(x => x.Read()).Returns(() => { count++; return count == 1; });
			reader.Setup(x => x[0]).Returns("7324.10.00.10");
			reader.Setup(x => x[1]).Returns(new DateTime(1998, 01, 01));
			reader.Setup(x => x[2]).Returns("Horses:");

			var producer = new TariffDataProducer(conn.Object);
			var tariff = producer.GetTariff(new[] { "7324.10.00.10" }, 13).FirstOrDefault();
			Assert.AreEqual("7324100010", tariff.ZZ1_TariffCode);
			Assert.AreEqual("Horses:", tariff.ZZ1_Description);
			Assert.AreEqual(new DateTime(1998, 01, 01), tariff.ZZ1_StartDate);
			Assert.AreEqual("HSN", tariff.ZZ1_ZZI_NKTariffType);
		}
	}
}
