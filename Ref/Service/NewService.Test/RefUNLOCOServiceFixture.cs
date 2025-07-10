using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefUNLOCOServiceFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "RL";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefUNLOCO);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "AUABP", "AUABX" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "AUABX" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = _repo.Create(() => new RefUNLOCO() { RL_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RL_Code = "AUABP" });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = _now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = _repo.Create(() => new RefUNLOCO() { RL_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RL_Code = "AUABX" });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = _now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, _now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RL_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefUNLOCOService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefUNLOCO
				{
					RL_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					RL_Code = "AUABP",
					RL_RN_NKCountryCode = "AU",
					RL_PortName = "Canillo",
					RL_NameWithDiacriticals = "Canillo",
					RL_IATA = string.Empty,
					RL_CoOrdinates = "4234N 00135E",
					RL_IATARegionCode = string.Empty,
					RL_GeoLocation = new Point(10, 20) { SRID = 4326 }
				});
				await repository.SaveChangesAsync();

				var service = new RefUNLOCOService(repository);
				var refUNLOCOEnumerable = service.GetData(null, _now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.RL_Code);
				Assert.DoesNotThrow(() =>
				{
					var refUNLOCOResults = refUNLOCOEnumerable.ToArray();
					Assert.NotNull(refUNLOCOResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AUABP", "AUABX" })]
		[TestCase(1, 3, new[] { "AUABX" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AUABP", "AUABX" })]
		[TestCase(null, 1, new[] { "AUABP" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateData(_now, "AUABP");
			CreateData(_now.AddDays(2), "AUABX");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RL_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(_now.AddDays(1), "AUABP");
			p.RL_PortName = "Abbot Point";
			p.RL_GeoLocation = new Point(-148.083, -19.900) { SRID = 4326 };
			var dataSets = GetDataSets(_now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RL_Code, Is.EqualTo("AUABP"));
			Assert.That(result.RL_PortName, Is.EqualTo("Abbot Point"));
			Assert.That(result.RL_GeoLocation, Is.EqualTo("POINT (-148.083 -19.9)"));
		}

		[Test]
		public void GetLatest_With_LocoMap_Data()
		{
			var p = CreateData(_now.AddDays(1), "AUABP");
			p.RL_PortName = "Abbot Point";
			p.RL_GeoLocation = new Point(-148.083, -19.900) { SRID = 4326 };

			var locoMap = CreateLocoMap(_now.AddDays(1), "Any", "AUABP");

			var dataSets = GetDataSets(_now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RL_Code, Is.EqualTo("AUABP"));
			Assert.That(result.RL_PortName, Is.EqualTo("Abbot Point"));
			Assert.That(result.RL_GeoLocation, Is.EqualTo("POINT (-148.083 -19.9)"));
			Assert.That(result.RefLocoMaps.Length == 1);
		}

		[Test]
		public void GetLatest_With_UtcOffset_Data()
		{
			var p = CreateData(_now.AddDays(1), "AUABP");
			p.RL_PortName = "Abbot Point";
			p.RL_GeoLocation = new Point(-148.083, -19.900) { SRID = 4326 };

			var utcOffset = CreateUtcOffset(_now.AddDays(1), "AUABP");

			var dataSets = GetDataSets(_now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RL_Code, Is.EqualTo("AUABP"));
			Assert.That(result.RL_PortName, Is.EqualTo("Abbot Point"));
			Assert.That(result.RL_GeoLocation, Is.EqualTo("POINT (-148.083 -19.9)"));
			Assert.That(result.RefUNLOCOUtcOffsets.Length == 1);
			Assert.That(result.RefUNLOCOUtcOffsets[0].RLO_RL_NKCode, Is.EqualTo(utcOffset.RLO_RL_NKCode));
			Assert.That(result.RefUNLOCOUtcOffsets[0].RLO_OffsetMinutesFromUtc, Is.EqualTo(utcOffset.RLO_OffsetMinutesFromUtc));
		}

		[Test]
		public void GetLatest_LocoMapsAndUtcOffsets_Count()
		{
			var p = CreateData(_now.AddDays(1), "AUABP");
			var locoMap1 = CreateLocoMap(_now.AddDays(1), "Any1", "AUABP");
			var locoMap2 = CreateLocoMap(_now.AddDays(10), "Any2", "AUABP");
			var utcOffset1 = CreateUtcOffset(_now.AddDays(1), "AUABP");
			var utcOffset2 = CreateUtcOffset(_now.AddDays(10), "AUABP");
			var dataSets = GetDataSets(_now);
			var result = dataSets.ElementAt(0);
			Assert.AreEqual(2, result.RefLocoMaps.Length);
			Assert.AreEqual(2, result.RefUNLOCOUtcOffsets.Length);
		}

		[Test]
		public void GetLatest_With_RelatedPort_Data()
		{
			var portsWithRelatedPorts = new[] {
				(CreateData(_now.AddDays(1), "AUABP"), CreateRelatedPort(_now.AddDays(1), 2, "AUABP")),
				(CreateData(_now.AddDays(1), "AUKUL"), CreateRelatedPort(_now.AddDays(1), 2, "AUKUL")),
				(CreateData(_now.AddDays(1), "MYPEN"), CreateRelatedPort(_now.AddDays(1), 3, "MYPEN"))
			};

			var dataSets = GetDataSets(_now);

			foreach (var (unloco, relatedPort) in portsWithRelatedPorts)
			{
				var unlocoDataSet = dataSets.First(dataSet => dataSet.RL_Code == unloco.RL_Code);
				Assert.NotNull(unlocoDataSet);
				Assert.AreEqual(relatedPort.RLR_GroupNumber, unlocoDataSet.RefUNLOCORelatedPorts[0].RLR_GroupNumber);
				Assert.AreEqual(relatedPort.RLR_RL_NKRelatedPort, unlocoDataSet.RefUNLOCORelatedPorts[0].RLR_RL_NKRelatedPort);
				Assert.AreEqual(1, unlocoDataSet.RefUNLOCORelatedPorts.Length);
			}
		}

		RefUNLOCO CreateData(DateTime dateTime, string code = null)
		{
			var result = _repo.Create(() => new RefUNLOCO() { RL_PK = Guid.NewGuid(), RL_Code = code });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RL_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		RefCountry CreateCountry(DateTime dateTime, string code)
		{
			var country = _repo.Create(() => new RefCountry() { RN_PK = Guid.NewGuid(), RN_Code = code });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = country.RN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return country;
		}

		RefLocoMap CreateLocoMap(DateTime dateTime, string code, string unloco)
		{
			var locoMap = _repo.Create(() => new RefLocoMap() { RY_PK = Guid.NewGuid(), RY_LocalPortCode = code, RY_RN_NKCountryCode = "any", RY_RL_NKLocoPort = unloco });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = locoMap.RY_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return locoMap;
		}

		RefUNLOCOUtcOffset CreateUtcOffset(DateTime dateTime, string unloco)
		{
			var utcOffset = _repo.Create(() => new RefUNLOCOUtcOffset() { RLO_PK = Guid.NewGuid(), RLO_RL_NKCode = unloco, RLO_StartTimeUtc = dateTime, RLO_EndTimeUtc = dateTime.AddMonths(1), RLO_OffsetMinutesFromUtc = 2 });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = utcOffset.RLO_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return utcOffset;
		}

		RefUNLOCORelatedPort CreateRelatedPort(DateTime dateTime, short group, string unloco)
		{
			var relatedPort = _repo.Create(() => new RefUNLOCORelatedPort() { RLR_PK = Guid.NewGuid(), RLR_RL_NKRelatedPort = unloco, RLR_GroupNumber = group });
			_repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = relatedPort.RLR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return relatedPort;
		}

		IEnumerable<Models.RefUNLOCO> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)_now.AddDays(daysOffSet.Value) : null, _now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefUNLOCO> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefUNLOCOService(_repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RL_Code);
		}

		DateTime _now;
		ObjectReferenceDataRepository _repo;
		[SetUp]
		public void SetUp()
		{
			_now = DateTime.UtcNow;
			_repo = new ObjectReferenceDataRepository();
		}
	}
}
