using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefComplianceListServiceFixture
	{
		static string TblPrefix => "RCL";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefComplianceList);

		[TestCase("7B62B060-FDAD-4CE1-A57B-587B588E5172", new[] { "11", "22" })]
		[TestCase("D72776F8-5AE4-4366-89E3-0DB171761375", new[] { "22" })]
		[TestCase("DF88FF88-4F59-4E42-A0FD-F62A482DFFF1", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefComplianceList { RCL_PK = new Guid("7B62B060-FDAD-4CE1-A57B-587B588E5173"), RCL_ListCode = "11" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_ParentPK = dataset1.RCL_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefComplianceList { RCL_PK = new Guid("D72776F8-5AE4-4366-89E3-0DB171761376"), RCL_ListCode = "22" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_ParentPK = dataset2.RCL_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.RCL_ListCode).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateData(Now, "AA");
			var p2 = CreateData(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.RCL_ListCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "11");
			p.RCL_ListDescription = "Description";
			p.RCL_ListName = "Name";
			p.RCL_ListPublisher = "Publisher";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.RCL_ListCode, Is.EqualTo("11"));
			Assert.That(result.RCL_ListDescription, Is.EqualTo("Description"));
			Assert.That(result.RCL_ListName, Is.EqualTo("Name"));
			Assert.That(result.RCL_ListPublisher, Is.EqualTo("Publisher"));
		}

		RefComplianceList CreateData(DateTime dateTime, string rateType = null)
		{
			var result = repo.Create(() => new RefComplianceList { RCL_PK = Guid.NewGuid(), RCL_ListCode = rateType });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_ParentPK = result.RCL_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefComplianceList> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefComplianceList> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefComplianceListService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.RCL_ListCode);
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
