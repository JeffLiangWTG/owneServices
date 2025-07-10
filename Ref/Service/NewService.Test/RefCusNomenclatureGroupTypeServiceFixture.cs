using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusNomenclatureGroupTypeServiceFixture
	{
		static string TblPrefix => "ZZ9";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusNomenclatureGroupType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusNomenclatureGroupType { ZZ9_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZ9_GroupType = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZ9_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusNomenclatureGroupType { ZZ9_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZ9_GroupType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZ9_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZ9_GroupType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_GroupType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateGroupType(Now, "AA");
			CreateGroupType(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZ9_GroupType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_GroupType_Data()
		{
			var p = repo.Create(() => new RefCusNomenclatureGroupType
			{
				ZZ9_PK = Guid.NewGuid(),
				ZZ9_GroupType = "AA",
				ZZ9_Description = "BB",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZ9_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZ9_GroupType, Is.EqualTo(p.ZZ9_GroupType));
			Assert.That(result.ZZ9_Description, Is.EqualTo(p.ZZ9_Description));
		}

		RefCusNomenclatureGroupType CreateGroupType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusNomenclatureGroupType { ZZ9_PK = Guid.NewGuid(), ZZ9_GroupType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZ9_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusNomenclatureGroupType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusNomenclatureGroupType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusNomenclatureGroupTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZ9_GroupType);
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
