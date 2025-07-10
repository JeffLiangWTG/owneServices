using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusConditionTypeServiceFixture
	{
		static string TblPrefix => "ZX2";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusConditionType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusConditionType { ZX2_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZX2_ConditionType = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZX2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusConditionType { ZX2_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZX2_ConditionType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZX2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZX2_ConditionType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Type_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateType(Now, "AA");
			var p2 = CreateType(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZX2_ConditionType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Type_Data()
		{
			var p = CreateType(Now.AddDays(1), "AA");
			p.ZX2_ConditionClass = "A";
			p.ZX2_Description = "B";
			p.ZX2_ZZZ_NKDataGrouping = "ZA";

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZX2_ConditionClass, Is.EqualTo("A"));
			Assert.That(result.ZX2_Description, Is.EqualTo("B"));
			Assert.That(result.ZX2_ZZZ_NKDataGrouping, Is.EqualTo("ZA"));
		}

		[Test]
		public void GetLatest_TypeLanguage_Data()
		{
			var p = CreateType(Now.AddDays(1), "AA");
			p.ZX2_ConditionClass = "A";
			p.ZX2_Description = "B";
			p.ZX2_ZZZ_NKDataGrouping = "ZA";

			var lang = repo.Create(() => new RefCusConditionTypeLanguage
			{
				ZXW_Description = "A",
				ZXW_ZX2_ConditionType = p.ZX2_PK,
				ZXW_ZX6_NKLanguage = "EN"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0)?.RefCusConditionTypeLanguages.FirstOrDefault();
			Assert.IsNotNull(result);

			Assert.That(result.ZXW_Description, Is.EqualTo(lang.ZXW_Description));
			Assert.That(result.ZXW_ZX6_NKLanguage, Is.EqualTo(lang.ZXW_ZX6_NKLanguage));
		}

		RefCusConditionType CreateType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusConditionType { ZX2_PK = Guid.NewGuid(), ZX2_ConditionType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZX2_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusConditionType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusConditionType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusConditionTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZX2_ConditionType);
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
