using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefAirlineServiceFixture
	{
		static string TblPrefix => "RM";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefAirline);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefAirline { RM_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RM_ThreeLetterCode = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefAirline { RM_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RM_ThreeLetterCode = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RM_ThreeLetterCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateRefAirline(Now, "AA");
			var p2 = CreateRefAirline(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RM_ThreeLetterCode).ToArray(), Is.EqualTo(expected));
			Assert.That(dataSets.Select(x => x.RM_EagleAddedAirlinePrefixOrAccountingCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefAirline()
		{
			var p = repo.Create(() => new RefAirline
			{
				RM_PK = Guid.NewGuid(),
				RM_ThreeLetterCode = "ABC",
				RM_EagleAddedAirlinePrefixOrAccountingCode = "123"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.RM_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RM_ThreeLetterCode, Is.EqualTo(p.RM_ThreeLetterCode));
			Assert.That(result.RM_EagleAddedAirlinePrefixOrAccountingCode, Is.EqualTo(p.RM_EagleAddedAirlinePrefixOrAccountingCode));
		}

		[Test]
		public void GetLatest_Data_StmNote()
		{
			var p = CreateRefAirline(Now.AddDays(1), "AA");
			p.RM_AirlineName1 = "airline name";

			repo.Create(() => new StmNote { ST_PK = Guid.NewGuid(), ST_ParentTableCode = "RM", ST_ParentId = p.RM_PK, ST_NoteText = "some note" });

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RM_ThreeLetterCode, Is.EqualTo("AA"));
			Assert.That(result.RM_EagleAddedAirlinePrefixOrAccountingCode, Is.EqualTo("AA"));
			Assert.That(result.RM_AirlineName1, Is.EqualTo("airline name"));
			Assert.That(result.StmNotes != null);
			Assert.AreEqual(result.StmNotes.Length, 1);
			Assert.AreEqual(result.StmNotes.First().ST_Table, nameof(RefAirline));
		}

		RefAirline CreateRefAirline(DateTime dateTime, string code = "XXX")
		{
			var result = repo.Create(() => new RefAirline { RM_PK = Guid.NewGuid(), RM_ThreeLetterCode = code, RM_EagleAddedAirlinePrefixOrAccountingCode = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefAirline> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefAirline> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefAirlineService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RM_ThreeLetterCode);
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
