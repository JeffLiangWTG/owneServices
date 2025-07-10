using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test
{
	[TestFixture]
	sealed class DeclarableCodeLeafRefresherFixture
	{
		[Test]
		public void RefreshLeafStatusForDailyUpdateRecords_WhenTariffNotEndWith80()
		{
			var declarableRecords = GetRawDeclarableCodeRecords().ToDictionary(x => x.TariffHeader);
			var dailyNomenclaturesRawRecords = GetRawNomenclatureDailyRecords().ToList();

			var leafRefresher = new DeclarableCodeRefresher(declarableRecords, dailyNomenclaturesRawRecords, now);
			leafRefresher.RefreshLeafStatusForDailyUpdateRecords();

			Assert.Multiple(() =>
			{
				Assert.That(declarableRecords["3800000000 80"].IsLeaf, Is.False, "3800000000 80 Is Leaf");
				Assert.That(declarableRecords["3824000000 80"].IsLeaf, Is.False, "3824000000 80 Is Leaf");
				Assert.That(declarableRecords["3824300000 80"].IsLeaf, Is.False, "3824300000 80 Is Leaf");
				Assert.That(declarableRecords["3824300010 10"].IsLeaf, Is.False, "3824300010 10 Is Leaf");
				Assert.That(declarableRecords["3824300010 80"].IsLeaf, Is.True, "3824300010 80 Is Leaf");
				Assert.That(declarableRecords["3824300020 80"].IsLeaf, Is.True, "3824300020 80 Is Leaf");
				Assert.That(declarableRecords["3824300030 80"].IsLeaf, Is.True, "3824300030 80 Is Leaf");
				Assert.That(declarableRecords["3824300040 80"].IsLeaf, Is.True, "3824300040 80 Is Leaf");
				Assert.That(declarableRecords["3824300090 80"].IsLeaf, Is.True, "3824300090 80 Is Leaf");
			});
		}

		[Test]
		public void RefreshLeafStatusForDailyUpdateRecords_WhenAllChildTariffExpiredInDailyUpdate()
		{
			var declarableRecords = GetRawDeclarableCodeRecords().ToDictionary(x => x.TariffHeader);
			var dailyNomenclaturesRawRecords = GetRawNomenclatureDailyRecordsWithAllChildTariffsExpired().ToList();

			var leafRefresher = new DeclarableCodeRefresher(declarableRecords, dailyNomenclaturesRawRecords, now);
			leafRefresher.RefreshLeafStatusForDailyUpdateRecords();

			Assert.That(declarableRecords["3824300000 80"].IsLeaf, Is.True, "3824300000 80 Is Leaf");

			IEnumerable<IRawNomenclatureDailyRecord> GetRawNomenclatureDailyRecordsWithAllChildTariffsExpired()
			{
				var pastDate = now.AddDays(-1);

				yield return GetRawNomenclatureDailyRecord("3824300010 10", isLeaf: false, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300010 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300020 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300030 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300040 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300090 80", isLeaf: true, endDate: pastDate);
			}
		}

		[Test]
		public void RefreshLeafStatusForDailyUpdateRecords_WhenSomeChildTariffExpiredInDailyUpdate()
		{
			var declarableRecords = GetRawDeclarableCodeRecords().ToDictionary(x => x.TariffHeader);
			var dailyNomenclaturesRawRecords = GetRawNomenclatureDailyRecordsWithSomeChildTariffsExpired().ToList();

			var leafRefresher = new DeclarableCodeRefresher(declarableRecords, dailyNomenclaturesRawRecords, now);
			leafRefresher.RefreshLeafStatusForDailyUpdateRecords();

			Assert.That(declarableRecords["3824300000 80"].IsLeaf, Is.False, "3824300000 80 Is Leaf");

			IEnumerable<IRawNomenclatureDailyRecord> GetRawNomenclatureDailyRecordsWithSomeChildTariffsExpired()
			{
				var pastDate = now.AddDays(-1);

				yield return GetRawNomenclatureDailyRecord("3824300010 10", isLeaf: false, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300010 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300020 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300030 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300040 80", isLeaf: true, endDate: pastDate);
				yield return GetRawNomenclatureDailyRecord("3824300090 80", isLeaf: true);
			}
		}

		IEnumerable<IRawDeclarableCodeRecord> GetRawDeclarableCodeRecords()
		{
			yield return GetRawDeclarableCodeRecord("3800000000 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824000000 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300000 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300010 10", isLeaf: false);
			yield return GetRawDeclarableCodeRecord("3824300010 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300020 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300030 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300040 80", isLeaf: true);
			yield return GetRawDeclarableCodeRecord("3824300090 80", isLeaf: true);
		}

		IEnumerable<IRawNomenclatureDailyRecord> GetRawNomenclatureDailyRecords()
		{
			yield return GetRawNomenclatureDailyRecord("3824300020 80", isLeaf: true);
			yield return GetRawNomenclatureDailyRecord("3824300030 80", isLeaf: true);
			yield return GetRawNomenclatureDailyRecord("3824300040 80", isLeaf: true);
			yield return GetRawNomenclatureDailyRecord("3824300010 10", isLeaf: false);
			yield return GetRawNomenclatureDailyRecord("3824300010 80", isLeaf: true);
		}

		IRawDeclarableCodeRecord GetRawDeclarableCodeRecord(
			string tariffHeader,
			bool isLeaf,
			DateTime? endDate = null)
		{
			endDate = endDate ?? defaultEndDate;

			var mock = new Mock<IRawDeclarableCodeRecord>();
			mock.Setup(x => x.TariffHeader).Returns(tariffHeader);
			mock.Setup(x => x.IsLeaf).Returns(isLeaf);
			mock.Setup(x => x.StartDate).Returns(startDate);
			mock.Setup(x => x.EndDate).Returns(endDate.Value);

			return mock.Object;
		}

		IRawNomenclatureDailyRecord GetRawNomenclatureDailyRecord(
			string tariffHeader,
			bool isLeaf,
			DateTime? endDate = null)
		{
			endDate = endDate ?? defaultEndDate;

			var mock = new Mock<IRawNomenclatureDailyRecord>();
			mock.Setup(x => x.TariffHeader).Returns(tariffHeader);
			mock.Setup(x => x.IsLeaf).Returns(isLeaf);
			mock.Setup(x => x.StartDate).Returns(startDate);
			mock.As<IRawDeclarableCodeRecord>()
				.Setup(x => x.EndDate)
				.Returns(endDate.Value);

			return mock.Object;
		}

		static readonly DateTime startDate = new DateTime(2024, 1, 1);
		static readonly DateTime defaultEndDate = new DateTime(2025, 1, 1);
		static readonly DateTime now = new DateTime(2024, 6, 1);
	}
}
