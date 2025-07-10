using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusRateTypeFixture
	{
		static string TblPrefix => "ZZR";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusRateType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusRateType { ZZR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZR_RateType = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusRateType { ZZR_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZR_RateType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCusRateCode>(repo, nameof(RefCusRateCode.ZY1_ZZR_RateType), dataset2.ZZR_PK);
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZR_RateType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_RateType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateRateType(Now, "AA");
			var p2 = CreateRateType(Now.AddDays(2), "BB");
			Helper.CreateChild<RefCusRateCode>(repo, nameof(RefCusRateCode.ZY1_ZZR_RateType), p2.ZZR_PK);
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZR_RateType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RateCode_Data()
		{
			var codeList = CreateRateType(Now.AddDays(1));
			var attr = repo.Create(() => new RefCusRateCode
			{
				ZY1_Description = "AA",
				ZY1_ZZR_RateType = codeList.ZZR_PK,
				ZY1_RateCode = "BB",
				ZY1_InternalUse = false
			});
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRateCodes.ElementAt(0);
			Assert.That(result.ZY1_Description, Is.EqualTo(attr.ZY1_Description));
			Assert.That(result.ZY1_RateCode, Is.EqualTo(attr.ZY1_RateCode));
			Assert.That(result.ZY1_InternalUse, Is.EqualTo(attr.ZY1_InternalUse));
		}

		[Test]
		public void GetLatest_RateCodeLanguage_Data()
		{
			var rateType = CreateRateType(Now.AddDays(1));
			var rateCode = repo.Create(() => new RefCusRateCode
			{
				ZY1_Description = "AA",
				ZY1_ZZR_RateType = rateType.ZZR_PK,
				ZY1_RateCode = "BB"
			});
			var rateCodeLanguage = repo.Create(() => new RefCusRateCodeLanguage
			{
				ZXC_ZY1_RateCode = rateCode.ZY1_PK,
				ZXC_ZX6_NKLanguage = "",
				ZXC_Description = "Test"
			});

			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRateCodes.ElementAt(0).RefCusRateCodeLanguages.ElementAt(0);
			Assert.That(result.ZXC_Description, Is.EqualTo(rateCodeLanguage.ZXC_Description));
		}

		[Test]
		public void GetLatest_RateType_Data()
		{
			var p = repo.Create(() => new RefCusRateType
			{
				ZZR_PK = Guid.NewGuid(),
				ZZR_Description = "HA",
				ZZR_IsPayable = true,
				ZZR_RateType = "AA",
				ZZR_ZZZ_NKDataGrouping = "ZA"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZR_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			Helper.CreateChild<RefCusRateCode>(repo, nameof(RefCusRateCode.ZY1_ZZR_RateType), p.ZZR_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZR_Description, Is.EqualTo(p.ZZR_Description));
			Assert.That(result.ZZR_IsPayable, Is.EqualTo(p.ZZR_IsPayable));
			Assert.That(result.ZZR_RateType, Is.EqualTo(p.ZZR_RateType));
			Assert.That(result.ZZR_ZZZ_NKDataGrouping, Is.EqualTo(p.ZZR_ZZZ_NKDataGrouping));
		}

		[Test]
		public void GetLatest_RateTypeLanguage_Data()
		{
			var rateType = CreateRateType(Now.AddDays(1));
			var typeLanguage = repo.Create(() => new RefCusRateTypeLanguage
			{
				ZXT_ZZR_RateType = rateType.ZZR_PK,
				ZXT_ZX6_NKLanguage = "EN",
				ZXT_Description = "AA",
			});
			Helper.CreateChild<RefCusRateCode>(repo, nameof(RefCusRateCode.ZY1_ZZR_RateType), rateType.ZZR_PK);
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0).RefCusRateTypeLanguages.ElementAt(0);
			Assert.That(result.ZXT_ZX6_NKLanguage, Is.EqualTo(typeLanguage.ZXT_ZX6_NKLanguage));
			Assert.That(result.ZXT_Description, Is.EqualTo(typeLanguage.ZXT_Description));
		}

		RefCusRateType CreateRateType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusRateType { ZZR_PK = Guid.NewGuid(), ZZR_RateType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZR_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusRateType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusRateType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusRateTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZR_RateType);
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
