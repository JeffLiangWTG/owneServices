using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusTaxOrFeeServiceFixture
	{
		static string TblPrefix => "ZX0";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusTaxOrFeeType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZX0_TaxOrFeeType = "TST" });
			var dataset1Fee = repo.Create(() => new RefCusTaxOrFee { ZZF_PK = new Guid("C4C66048-129C-4525-8256-1EAF1260E0ED"), ZZF_Code = "BB", ZZF_ZX0_NKTaxOrFeeType = "TST" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });

			var dataset2 = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZX0_TaxOrFeeType = "ANY" });
			var dataset2Fee = repo.Create(() => new RefCusTaxOrFee { ZZF_PK = new Guid("66F325F9-813F-4D80-BF08-938655E56C9F"), ZZF_Code = "CC", ZZF_ZX0_NKTaxOrFeeType = "ANY" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });

			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZF_Code).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_TaxOrFee_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var taxOrFeeType = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = Guid.NewGuid(), ZX0_TaxOrFeeType = "TST" });
			var taxOrFeeType2 = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = Guid.NewGuid(), ZX0_TaxOrFeeType = "ANY" });
			var p1 = CreateTaxCode(Now, taxOrFeeType, "AA");
			var p2 = CreateTaxCode(Now.AddDays(2), taxOrFeeType2, "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZF_Code).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_TaxOrFee_Data()
		{
			var taxOrFeeType = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = Guid.NewGuid(), ZX0_TaxOrFeeType = "TST" });
			var p = repo.Create(() => new RefCusTaxOrFee
			{
				ZZF_PK = Guid.NewGuid(),
				ZZF_Code = "AA",
				ZZF_Description = "BB",
				ZZF_StartDate = Now.AddDays(-1),
				ZZF_EndDate = Now.AddDays(1),
				ZZF_ZZZ_NKDataGrouping = "AU",
				ZZF_Value = 1,
				ZZF_ZX0_NKTaxOrFeeType = "TST"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = taxOrFeeType.ZX0_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZF_Code, Is.EqualTo(p.ZZF_Code));
			Assert.That(result.ZZF_Description, Is.EqualTo(p.ZZF_Description));
			Assert.That(result.ZZF_StartDate, Is.EqualTo(p.ZZF_StartDate));
			Assert.That(result.ZZF_EndDate, Is.EqualTo(p.ZZF_EndDate));
			Assert.That(result.ZZF_ZZZ_NKDataGrouping, Is.EqualTo(p.ZZF_ZZZ_NKDataGrouping));
			Assert.That(result.ZZF_Value, Is.EqualTo(p.ZZF_Value));
		}

		[Test]
		public void GetLatest_TaxOrFeeLanguage_Data()
		{
			var taxOrFeeType = repo.Create(() => new RefCusTaxOrFeeType { ZX0_PK = Guid.NewGuid(), ZX0_TaxOrFeeType = "TST" });
			var p = repo.Create(() => new RefCusTaxOrFee
			{
				ZZF_PK = Guid.NewGuid(),
				ZZF_Code = "AA",
				ZZF_Description = "BB",
				ZZF_StartDate = Now.AddDays(-1),
				ZZF_EndDate = Now.AddDays(1),
				ZZF_ZZZ_NKDataGrouping = "AU",
				ZZF_Value = 1,
				ZZF_ZX0_NKTaxOrFeeType = "TST"
			});
			var lang = repo.Create(() => new RefCusTaxOrFeeLanguage
			{
				ZXU_Description = "CC",
				ZXU_PK = Guid.NewGuid(),
				ZXU_ZX6_NKLanguage = "EN",
				ZXU_ZZF_TaxOrFee = p.ZZF_PK
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = taxOrFeeType.ZX0_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0)?.RefCusTaxOrFeeLanguages.FirstOrDefault();
			Assert.IsNotNull(result);

			Assert.That(result.ZXU_Description, Is.EqualTo(lang.ZXU_Description));
			Assert.That(result.ZXU_ZX6_NKLanguage, Is.EqualTo(lang.ZXU_ZX6_NKLanguage));
		}

		RefCusTaxOrFee CreateTaxCode(DateTime dateTime, RefCusTaxOrFeeType taxOrFeeType, string code = null)
		{
			var result = repo.Create(() => new RefCusTaxOrFee { ZZF_PK = Guid.NewGuid(), ZZF_Code = code, ZZF_ZX0_NKTaxOrFeeType = taxOrFeeType.ZX0_TaxOrFeeType });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = taxOrFeeType.ZX0_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusTaxOrFee> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusTaxOrFee> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var typeService = new RefCusTaxOrFeeTypeService(repo);
			var service = new RefCusTaxOrFeeService(typeService);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZF_Code);
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
