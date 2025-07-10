using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusConditionCodeServiceFixture
	{
		static string TblPrefix => "ZY7";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusConditionCode);

		[TestCase("55EED516-68AA-49E8-A26A-3BF42E771DF1", new[] { "BB", "CC" })]
		[TestCase("55EED516-68AA-49E8-A26A-3BF42E771DF4", new[] { "CC" })]
		[TestCase("55EED516-68AA-49E8-A26A-3BF42E771DF8", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusConditionCode { ZY7_PK = new Guid("55EED516-68AA-49E8-A26A-3BF42E771DF3"), ZY7_ConditionCode = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZY7_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusConditionCode { ZY7_PK = new Guid("55EED516-68AA-49E8-A26A-3BF42E771DF6"), ZY7_ConditionCode = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZY7_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZY7_ConditionCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Code_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateCode(Now, "AA");
			var p2 = CreateCode(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZY7_ConditionCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Code_Data()
		{
			var p = CreateCode(Now.AddDays(1), "AA");
			p.ZY7_Description = "B";
			p.ZY7_ZZZ_NKDataGrouping = "ZA";

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZY7_Description, Is.EqualTo("B"));
			Assert.That(result.ZY7_ZZZ_NKDataGrouping, Is.EqualTo("ZA"));
		}

		[Test]
		public void GetLatest_CodeLanguage_Data()
		{
			var p = CreateCode(Now.AddDays(1), "AA");
			p.ZY7_Description = "B";
			p.ZY7_ZZZ_NKDataGrouping = "ZA";

			var lang = repo.Create(() => new RefCusConditionCodeLanguage
			{
				ZY8_Description = "A",
				ZY8_ZY7_ConditionCode = p.ZY7_PK,
				ZY8_ZX6_NKLanguage = "EN"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0)?.RefCusConditionCodeLanguages.FirstOrDefault();
			Assert.IsNotNull(result);

			Assert.That(result.ZY8_Description, Is.EqualTo(lang.ZY8_Description));
			Assert.That(result.ZY8_ZX6_NKLanguage, Is.EqualTo(lang.ZY8_ZX6_NKLanguage));
		}

		RefCusConditionCode CreateCode(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusConditionCode { ZY7_PK = Guid.NewGuid(), ZY7_ConditionCode = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZY7_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusConditionCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusConditionCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusConditionCodeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZY7_ConditionCode);
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
