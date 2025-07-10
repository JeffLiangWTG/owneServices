using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusTariffAdditionalCodeCategoryServiceFixture
	{
		static string TblPrefix => "ZY3";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTariffAdditionalCodeCategory);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusTariffAdditionalCodeCategory { ZY3_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZY3_Category = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZY3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusTariffAdditionalCodeCategory { ZY3_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZY3_Category = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZY3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZY3_Category).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatestAdditionalCodeCategoryFilter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateAdditionalCodeCategory(Now, "AA");
			var p2 = CreateAdditionalCodeCategory(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZY3_Category).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatestAdditionalCodeCategoryData()
		{
			var p = repo.Create(() => new RefCusTariffAdditionalCodeCategory
			{
				ZY3_PK = Guid.NewGuid(),
				ZY3_Category = "BRL",
				ZY3_Description = "Brazilian Real"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZY3_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZY3_Category, Is.EqualTo(p.ZY3_Category));
			Assert.That(result.ZY3_Description, Is.EqualTo(p.ZY3_Description));
		}

		RefCusTariffAdditionalCodeCategory CreateAdditionalCodeCategory(DateTime dateTime, string code = "XX")
		{
			var result = repo.Create(() => new RefCusTariffAdditionalCodeCategory { ZY3_PK = Guid.NewGuid(), ZY3_Category = code, ZY3_Description = "Description" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZY3_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusTariffAdditionalCodeCategory> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusTariffAdditionalCodeCategory> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusTariffAdditionalCodeCategoryService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZY3_Category);
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
