using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Infrastructure.Test;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefTimeZoneSetFixture
	{
		string GetConnectionString(string dbName) => TestConnectionString.GetAdmin(dbName);
		static string TblPrefix => "R3";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefTimeZoneSet);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefTimeZoneSet { R3_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), R3_TimeZoneSetName = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.R3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefTimeZoneSet { R3_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), R3_TimeZoneSetName = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.R3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.R3_TimeZoneSetName).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		[TransactionedTestCase]
		[Property("DAT:CapabilityRequirements", "SQL2019+")]
		public async Task GetDataCoreLinqCanBeTranslated_RefTimeZoneSetService()
		{
			var dbName = TransactionedTestCaseAttribute.GetDbName(DbSchema.RefDbRepoSafe);
			using (var repository = new ReferenceDataRepository(GetConnectionString(dbName)))
			{
				repository.Add(new RefTimeZoneSet
				{
					R3_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"),
					R3_TimeZoneSetName = "BB",
					R3_IsActive = true
				});
				await repository.SaveChangesAsync();

				var service = new RefTimeZoneSetService(repository);
				var refTimeZoneSetEnumerable = service.GetData(null, Now, CheckpointHelper.Create(new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5")), null, DataSetId).OrderBy(x => x.R3_TimeZoneSetName);
				Assert.DoesNotThrow(() =>
				{
					var refTimeZoneSetResults = refTimeZoneSetEnumerable.ToArray();
					Assert.NotNull(refTimeZoneSetResults);
				});
			}
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_CodeType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateTimeZoneSetDaylight(Now, null, "AA");
			CreateTimeZoneSetDaylight(Now.AddDays(2), null, "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset).Select(x => x.R3_TimeZoneSetName).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CodeType_Data()
		{
			var p = repo.Create(() => new RefTimeZoneSet
			{
				R3_PK = Guid.NewGuid(),
				R3_TimeZoneSetName = "AA",
				R3_IsActive = true,
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.R3_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.R3_TimeZoneSetName, Is.EqualTo(p.R3_TimeZoneSetName));
		}

		[Test]
		public void GetTimeZoneRule_Data()
		{
			var timeZone = repo.Create(() => new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_MilitaryTimeZoneCode = "AA",
				R2_OffsetMinutesFromUTC = 3,
				R2_Type = "DLS"
			});
			var setDaylight = CreateTimeZoneSetDaylight(Now.AddDays(1), timeZone.R2_PK, "AA");

			timeZone.R2_R3_TimeZoneSet = setDaylight.R3_PK;
			var rule = repo.Create(() => new RefTimeZoneRule
			{
				R4_PK = Guid.NewGuid(),
				R4_R2 = timeZone.R2_PK,
				R4_StartOrEndRule = "STA",
				R4_DataSetPK = setDaylight.R3_PK,
				R4_DataSetCode = "R3"
			});

			var dataSets = GetDataSets(Now);

			var result = dataSets.ElementAt(0).RefTimeZoneDaylightSavingZone.RefTimeZoneRules.FirstOrDefault();
			Assert.That(result.R4_StartOrEndRule, Is.EqualTo(rule.R4_StartOrEndRule));
		}

		[Test]
		public void GetTimeZoneRule_WithTwoTimeZoneAndTwoRules()
		{
			var timeZoneSet = repo.Create(() => new RefTimeZoneSet
			{
				R3_PK = Guid.NewGuid(),
				R3_TimeZoneSetName = "TMZ1"
			});

			var timeZoneDaylight = repo.Create(() => new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_MilitaryTimeZoneCode = "AA",
				R2_OffsetMinutesFromUTC = 3,
				R2_Type = "DLS"
			});
			var timeZoneStandard = repo.Create(() => new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_MilitaryTimeZoneCode = "BB",
				R2_OffsetMinutesFromUTC = -3,
				R2_Type = "STD"
			});
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now.AddDays(1), RVC_DataSetId = DataSetId, RVC_ParentPK = timeZoneSet.R3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });

			timeZoneDaylight.R2_R3_TimeZoneSet = timeZoneSet.R3_PK;
			timeZoneStandard.R2_R3_TimeZoneSet = timeZoneSet.R3_PK;

			var rule_1 = repo.Create(() => new RefTimeZoneRule
			{
				R4_PK = Guid.NewGuid(),
				R4_R2 = timeZoneDaylight.R2_PK,
				R4_StartOrEndRule = "STA",
				R4_DataSetPK = timeZoneSet.R3_PK,
				R4_DataSetCode = "R3"
			});

			var rule_2 = repo.Create(() => new RefTimeZoneRule
			{
				R4_PK = Guid.NewGuid(),
				R4_R2 = timeZoneStandard.R2_PK,
				R4_StartOrEndRule = "END",
				R4_DataSetPK = timeZoneSet.R3_PK,
				R4_DataSetCode = "R3"
			});

			var dataSets = GetDataSets(Now);

			var result = dataSets.ElementAt(0);

			Assert.That(result.RefTimeZoneDaylightSavingZone != null && result.RefTimeZoneStandardZone != null);
			Assert.That(result.RefTimeZoneDaylightSavingZone.RefTimeZoneRules != null);
			Assert.That(result.RefTimeZoneDaylightSavingZone.RefTimeZoneRules.Length, Is.EqualTo(1));
			Assert.That(result.RefTimeZoneStandardZone.RefTimeZoneRules != null);
			Assert.That(result.RefTimeZoneStandardZone.RefTimeZoneRules.Length, Is.EqualTo(1));
		}

		[Test]
		public void GetTimeZone_Data()
		{
			var timeZone = repo.Create(() => new RefTimeZone
			{
				R2_PK = Guid.NewGuid(),
				R2_MilitaryTimeZoneCode = "AA",
				R2_OffsetMinutesFromUTC = 3,
				R2_Type = "STD"
			});

			var setDaylight = CreateTimeZoneSetStandard(Now.AddDays(1), timeZone.R2_PK, "AA");
			timeZone.R2_R3_TimeZoneSet = setDaylight.R3_PK;

			var dataSets = GetDataSets(Now);

			var result = dataSets.ElementAt(0).RefTimeZoneStandardZone;
			Assert.That(result.R2_MilitaryTimeZoneCode, Is.EqualTo(timeZone.R2_MilitaryTimeZoneCode));
		}

		RefTimeZoneSet CreateTimeZoneSetDaylight(DateTime dateTime, Guid? timeZonePK, string code)
		{
			var result = repo.Create(() => new RefTimeZoneSet { R3_PK = Guid.NewGuid(), R3_TimeZoneSetName = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.R3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		RefTimeZoneSet CreateTimeZoneSetStandard(DateTime dateTime, Guid? timeZonePK, string code)
		{
			var result = repo.Create(() => new RefTimeZoneSet { R3_PK = Guid.NewGuid(), R3_TimeZoneSetName = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.R3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefTimeZoneSet> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefTimeZoneSet> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefTimeZoneSetService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.R3_TimeZoneSetName);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();
		}
	}
}
