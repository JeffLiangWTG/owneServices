using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefUNLOCOPortMappingServiceFixture
	{
		static string TblPrefix => "RLM";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefUNLOCOPortMapping);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefUNLOCOPortMapping { RLM_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), RLM_RL_NKCode = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.RLM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefUNLOCOPortMapping { RLM_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), RLM_RL_NKCode = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.RLM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.RLM_RL_NKCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateRefUNLOCOPortMapping(Now, "AA");
			var p2 = CreateRefUNLOCOPortMapping(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RLM_RL_NKCode).ToArray(), Is.EqualTo(expected));
			Assert.That(dataSets.Select(x => x.RLM_RL_NKCodeRelated).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefUNLOCOPortMapping()
		{
			var p = repo.Create(() => new RefUNLOCOPortMapping
			{
				RLM_PK = Guid.NewGuid(),
				RLM_RL_NKCode = "ZA",
				RLM_RL_NKCodeRelated = "AA"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.RLM_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RLM_RL_NKCode, Is.EqualTo(p.RLM_RL_NKCode));
			Assert.That(result.RLM_RL_NKCodeRelated, Is.EqualTo(p.RLM_RL_NKCodeRelated));
		}

		RefUNLOCOPortMapping CreateRefUNLOCOPortMapping(DateTime dateTime, string code = "XX")
		{
			var result = repo.Create(() => new RefUNLOCOPortMapping { RLM_PK = Guid.NewGuid(), RLM_RL_NKCode = code, RLM_RL_NKCodeRelated = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.RLM_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefUNLOCOPortMapping> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefUNLOCOPortMapping> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefUNLOCOPortMappingService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RLM_RL_NKCode);
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
