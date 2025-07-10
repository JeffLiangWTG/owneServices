using System;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class ClientRecordFixture
	{
		[Test]
		public async Task InsertNewRecord()
		{
			var repo = new Mock<IReferenceDataRepository>();
			var clientRecord = new ClientRecord(repo.Object, dateTimeProvider.Object);
			var now = DateTime.UtcNow;
			await clientRecord.RecordAsync("TariffDataSet", "GG", "PRO", now.AddDays(-2), null, true);
			repo.Verify(x => x.Add(It.Is<ClientRefDbVersionControl>(y => y.CVC_ClientId == "GG" &&
				y.CVC_DataSet == "TariffDataSet" && y.CVC_IsInUse == true && y.CVC_SystemType == "PRO" && y.CVC_LastUpdatedTimeUTC == new DateTime(2018, 04, 30, 0, 0, 0))));
			repo.Verify(x => x.SaveChangesAsync());
		}

		[Test]
		public async Task UpdateRecord()
		{
			var now = DateTime.UtcNow;
			var repo = new Mock<IReferenceDataRepository>();
			var record = new ClientRefDbVersionControl
			{
				CVC_DataSet = "TariffDataSet",
				CVC_ClientId = "GG",
				CVC_DataSetTimestamp = now.AddDays(-1),
				CVC_DataSetCheckpoint = "Olala",
				CVC_SystemType = "PRO",
				CVC_LastUpdatedTimeUTC = UTCNow.AddMinutes(ClientRecord.LastUpdatedTimeUTCRefreshRateInMinutes * -1),
				CVC_IsInUse = true
			};
			var clientRecord = new ClientRecord(repo.Object, dateTimeProvider.Object);
			repo.Setup(x => x.Get<ClientRefDbVersionControl>()).Returns(new[] { record }.AsQueryable());
			await clientRecord.RecordAsync("TariffDataSet", "GG", null, now.AddHours(-1), "Olala1", true);
			Assert.That(record.CVC_DataSetCheckpoint, Is.EqualTo("Olala1"));
			Assert.That(record.CVC_DataSetTimestamp, Is.EqualTo(now.AddHours(-1)));
			Assert.AreEqual(UTCNow, record.CVC_LastUpdatedTimeUTC);

			record.CVC_LastUpdatedTimeUTC = UTCNow.AddMinutes((ClientRecord.LastUpdatedTimeUTCRefreshRateInMinutes - 1) * -1);
			await clientRecord.RecordAsync("TariffDataSet", "GG", null, now.AddHours(-1), "Olala1", true);
			Assert.AreEqual(UTCNow.AddMinutes((ClientRecord.LastUpdatedTimeUTCRefreshRateInMinutes - 1) * -1), record.CVC_LastUpdatedTimeUTC);

			record.CVC_LastUpdatedTimeUTC = UTCNow.AddMinutes((ClientRecord.LastUpdatedTimeUTCRefreshRateInMinutes + 1) * -1);
			await clientRecord.RecordAsync("TariffDataSet", "GG", null, now.AddHours(-1), "Olala1", true);
			Assert.AreEqual(UTCNow, record.CVC_LastUpdatedTimeUTC);
		}

		DateTime UTCNow;
		Mock<IDateTimeProvider> dateTimeProvider;

		[SetUp]
		public void SetUp()
		{
			UTCNow = new DateTime(2018, 04, 30, 0, 0, 0);
			dateTimeProvider = new Mock<IDateTimeProvider>();
			dateTimeProvider.Setup(x => x.GetUTCNow()).Returns(UTCNow);
		}
	}
}
