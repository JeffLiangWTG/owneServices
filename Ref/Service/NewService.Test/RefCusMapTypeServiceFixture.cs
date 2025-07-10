using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefCusMapTypeServiceFixture
	{
		static string TblPrefix => "ZZP";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusMapType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusMapType { ZZP_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZP_MapType = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusMapType { ZZP_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZP_MapType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZP_MapType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Map_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateMap(Now, "AA");
			var p2 = CreateMap(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZP_MapType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Map_Data()
		{
			var p = CreateMap(Now.AddDays(1), "AA");
			p.ZZP_Direction = "IN";
			p.ZZP_Description = "AA";
			p.ZZP_IsReadonly = true;

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZP_Direction, Is.EqualTo("IN"));
			Assert.That(result.ZZP_Description, Is.EqualTo("AA"));
			Assert.True(result.ZZP_IsReadonly);
		}

		RefCusMapType CreateMap(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusMapType { ZZP_PK = Guid.NewGuid(), ZZP_MapType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZP_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusMapType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusMapType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusMapTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZP_MapType);
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
