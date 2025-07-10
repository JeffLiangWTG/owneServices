using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusCodeTypeServiceFixture
	{
		static string TblPrefix => "ZZK";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusCodeType);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusCodeType { ZZK_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZZK_CodeType = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZK_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusCodeType { ZZK_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZZK_CodeType = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZK_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZZK_CodeType).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_CodeType_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateCodeType(Now, "AA");
			CreateCodeType(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZZK_CodeType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_CodeType_Data()
		{
			var p = repo.Create(() => new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "AA",
				ZZK_Description = "BB",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZK_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZK_CodeType, Is.EqualTo(p.ZZK_CodeType));
			Assert.That(result.ZZK_Description, Is.EqualTo(p.ZZK_Description));
		}

		[Test]
		public void GetLatest_CodeTypes()
		{
			var p = repo.Create(() => new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "AA",
				ZZK_Description = "BB",
			});

			var p2 = repo.Create(() => new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "AAA",
				ZZK_Description = "BB",
				ZZK_ZZZ_NKDataGrouping = "AU"
			});

			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZK_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p2.ZZK_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now).ToArray();
			var result = dataSets.ElementAt(1);
			Assert.That(result.ZZK_CodeType, Is.EqualTo(p2.ZZK_CodeType));
			Assert.That(result.ZZK_Description, Is.EqualTo(p2.ZZK_Description));

			result = dataSets.ElementAt(0);
			Assert.That(result.ZZK_CodeType, Is.EqualTo(p.ZZK_CodeType));
			Assert.That(result.ZZK_Description, Is.EqualTo(p.ZZK_Description));
		}

		[Test]
		public void GetLatest_CodeType_TypeLanguage_Data()
		{
			var p = repo.Create(() => new RefCusCodeType
			{
				ZZK_PK = Guid.NewGuid(),
				ZZK_CodeType = "AA",
				ZZK_Description = "BB",
			});

			var typeLanguage = repo.Create(() => new RefCusCodeTypeLanguage()
			{
				ZXI_PK = Guid.NewGuid(),
				ZXI_ZX6_NKLanguage = "EN",
				ZXI_ZZK_CodeType = p.ZZK_PK,
				ZXI_Description = "English"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZZK_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZK_CodeType, Is.EqualTo(p.ZZK_CodeType));
			Assert.That(result.ZZK_Description, Is.EqualTo(p.ZZK_Description));

			Assert.That(result.RefCusCodeTypeLanguages != null);
			Assert.That(result.RefCusCodeTypeLanguages[0].ZXI_ZX6_NKLanguage, Is.EqualTo(typeLanguage.ZXI_ZX6_NKLanguage));
			Assert.That(result.RefCusCodeTypeLanguages[0].ZXI_Description, Is.EqualTo(typeLanguage.ZXI_Description));
		}

		RefCusCodeType CreateCodeType(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusCodeType { ZZK_PK = Guid.NewGuid(), ZZK_CodeType = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZK_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusCodeType> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusCodeType> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusCodeTypeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZK_CodeType);
		}

		DateTime Now;
		ObjectReferenceDataRepository repo;
		[SetUp]
		public void SetUp()
		{
			Now = DateTime.UtcNow;
			repo = new ObjectReferenceDataRepository();

			var dataGrouping = repo.Create(() => new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_Description = "Australia",
				ZZZ_DataGrouping = "AU"
			});

			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = dataGrouping.ZZZ_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });

			dataGrouping = repo.Create(() => new RefDataGrouping
			{
				ZZZ_PK = Guid.NewGuid(),
				ZZZ_Description = "New Zealand",
				ZZZ_DataGrouping = "NZ"
			});

			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = dataGrouping.ZZZ_PK, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
		}
	}
}
