using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefDocOrgCusCodeServiceFixture
	{
		static string TblPrefix => "DOC";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefDocOrgCusCode);

		[TestCase("7B62B060-FDAD-4CE1-A57B-587B588E5172", new byte[] { 1, 2 })]
		[TestCase("D72776F8-5AE4-4366-89E3-0DB171761375", new byte[] { 2 })]
		[TestCase("DF88FF88-4F59-4E42-A0FD-F62A482DFFF1", new byte[0])]
		public void FilterWithLastDataSet(string checkpointPK, byte[] expected)
		{
			var dataset1 = repo.Create(() => new RefDocOrgCusCode { DOC_PK = new Guid("7B62B060-FDAD-4CE1-A57B-587B588E5173"), DOC_Priority = 1 });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.DOC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefDocOrgCusCode { DOC_PK = new Guid("D72776F8-5AE4-4366-89E3-0DB171761376"), DOC_Priority = 2 });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.DOC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.DOC_Priority).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new byte[] { 1, 2 })]
		[TestCase(1, 3, new byte[] { 2 })]
		[TestCase(3, 3, new byte[0])]
		[TestCase(null, 3, new byte[] { 1, 2 })]
		[TestCase(null, 1, new byte[] { 1 })]
		[TestCase(null, -1, new byte[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, byte[] expected)
		{
			var p1 = CreateData(Now, 1);
			var p2 = CreateData(Now.AddDays(2), 2);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.DOC_Priority).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), 1);
			p.DOC_RN_NKCodeCountry = "CN";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.DOC_Priority, Is.EqualTo(1));
			Assert.That(result.DOC_RN_NKCodeCountry, Is.EqualTo("CN"));
		}

		RefDocOrgCusCode CreateData(DateTime dateTime, byte priority = 0)
		{
			var result = repo.Create(() => new RefDocOrgCusCode { DOC_PK = Guid.NewGuid(), DOC_Priority = priority });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.DOC_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefDocOrgCusCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefDocOrgCusCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefDocOrgCusCodeService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.DOC_Priority);
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
