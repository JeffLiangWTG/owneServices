using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusTariffTypeServiceFixture
	{
		static string TblPrefix => "ZZI";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTariffType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var rateType = repo.Create(() => new RefCusRateType { ZZR_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = rateType.ZZR_PK, RVC_DataSetId = DataSetId });
			var dataset1 = repo.Create(() => new RefCusTariffType { ZZI_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZI_TariffType = "BB", ZZI_ZZR_RateType = rateType.ZZR_PK });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusTariffType { ZZI_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZI_TariffType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZI_TariffType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_TariffType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var rateType = repo.Create(() => new RefCusRateType { ZZR_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = rateType.ZZR_PK, RVC_DataSetId = DataSetId });
			var p1 = CreateTariffType(Now, "AA");
			p1.ZZI_ZZR_RateType = rateType.ZZR_PK;
			var p2 = CreateTariffType(Now.AddDays(2), "BB");
			p2.ZZI_ZZR_RateType = rateType.ZZR_PK;
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZI_TariffType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_TariffType_Data()
		{
			var rateType = repo.Create(() => new RefCusRateType { ZZR_PK = Guid.NewGuid() });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = rateType.ZZR_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var p = repo.Create(() => new RefCusTariffType
			{
				ZZI_PK = Guid.NewGuid(),
				ZZI_TariffType = "AA",
				ZZI_Description = "BB",
				ZZI_ZZZ_NKDataGrouping = "AU",
				ZZI_ZZR_RateType = rateType.ZZR_PK
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZI_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZI_TariffType, Is.EqualTo(p.ZZI_TariffType));
			Assert.That(result.ZZI_Description, Is.EqualTo(p.ZZI_Description));
			Assert.That(result.ZZI_ZZZ_NKDataGrouping, Is.EqualTo(p.ZZI_ZZZ_NKDataGrouping));
		}

		[Test]
		public void GetLatest_TariffTypeLanguage_Data()
		{
			var rateType = repo.Create(() => new RefCusRateType { ZZR_PK = Guid.NewGuid(), ZZR_RateType = "TST" });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = rateType.ZZR_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var tariffType1 = CreateTariffType(Now.AddDays(1), "AA");
			var tariffType2 = CreateTariffType(Now.AddDays(2), "BB");
			tariffType2.ZZI_ZZR_RateType = rateType.ZZR_PK;
			var typeLanguage1 = repo.Create(() => new RefCusTariffTypeLanguage
			{
				ZXK_PK = Guid.NewGuid(),
				ZXK_ZZI_TariffType = tariffType1.ZZI_PK,
				ZXK_ZX6_NKLanguage = "EN",
				ZXK_Description = "Test1",
			});
			var typeLanguage2 = repo.Create(() => new RefCusTariffTypeLanguage
			{
				ZXK_PK = Guid.NewGuid(),
				ZXK_ZZI_TariffType = tariffType2.ZZI_PK,
				ZXK_ZX6_NKLanguage = "CN",
				ZXK_Description = "Test2",
			});
			var dataSets = GetDataSets(Now);
			Assert.AreEqual(2, dataSets.Count());
			var result = dataSets.ElementAt(0);
			Assert.Null(result.RefCusRateType);
			Assert.That(result.RefCusTariffTypeLanguages[0].ZXK_ZX6_NKLanguage, Is.EqualTo(typeLanguage1.ZXK_ZX6_NKLanguage));
			Assert.That(result.RefCusTariffTypeLanguages[0].ZXK_Description, Is.EqualTo(typeLanguage1.ZXK_Description));
			result = dataSets.ElementAt(1);
			Assert.That(result.RefCusRateType.ZZR_RateType, Is.EqualTo(rateType.ZZR_RateType));
			Assert.That(result.RefCusTariffTypeLanguages[0].ZXK_ZX6_NKLanguage, Is.EqualTo(typeLanguage2.ZXK_ZX6_NKLanguage));
			Assert.That(result.RefCusTariffTypeLanguages[0].ZXK_Description, Is.EqualTo(typeLanguage2.ZXK_Description));
		}

		RefCusTariffType CreateTariffType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusTariffType { ZZI_PK = Guid.NewGuid(), ZZI_TariffType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZI_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusTariffType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusTariffType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusTariffTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZI_TariffType);
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
