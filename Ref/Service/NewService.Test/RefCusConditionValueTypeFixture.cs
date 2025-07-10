using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	public class RefCusConditionValueTypeFixture
	{
		static string TblPrefix => "ZX4";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusConditionValueType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusConditionValueType { ZX4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZX4_Description = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZX4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusConditionValueType { ZX4_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZX4_Description = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZX4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZX4_Description).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_ConditionValueType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateConditionValueType(Now, "AA");
			var p2 = CreateConditionValueType(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZX4_Description).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_ConditionValueType_Data()
		{
			var p = CreateConditionValueType(Now.AddDays(1), "AA");
			p.ZX4_Description = "BB";
			p.ZX4_ZZZ_NKDataGrouping = "AU";
			p.ZX4_ValueType = "T";
			p.ZX4_IsFormula = true;

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZX4_Description, Is.EqualTo("BB"));
			Assert.That(result.ZX4_ZZZ_NKDataGrouping, Is.EqualTo("AU"));
			Assert.That(result.ZX4_ValueType, Is.EqualTo("T"));
			Assert.That(result.ZX4_IsFormula, Is.EqualTo(true));
		}

		[Test]
		public void GetLatest_ConditionValueTypeLanguage_Data()
		{
			var p = CreateConditionValueType(Now.AddDays(1), "AA");
			p.ZX4_Description = "BB";
			p.ZX4_ZZZ_NKDataGrouping = "AU";
			p.ZX4_ValueType = "T";
			p.ZX4_IsFormula = true;

			var lang = repo.Create(() => new RefCusConditionValueTypeLanguage
			{
				ZXX_Description = "A",
				ZXX_ZX4_ValueType = p.ZX4_PK,
				ZXX_ZX6_NKLanguage = "EN"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0)?.RefCusConditionValueTypeLanguages.FirstOrDefault();
			Assert.IsNotNull(result);
			Assert.That(result.ZXX_Description, Is.EqualTo(lang.ZXX_Description));
			Assert.That(result.ZXX_ZX6_NKLanguage, Is.EqualTo(lang.ZXX_ZX6_NKLanguage));
		}

		RefCusConditionValueType CreateConditionValueType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusConditionValueType { ZX4_PK = Guid.NewGuid(), ZX4_Description = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZX4_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusConditionValueType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusConditionValueType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusConditionValueTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZX4_Description);
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
