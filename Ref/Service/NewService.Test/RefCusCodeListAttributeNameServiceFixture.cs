using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusCodeListAttributeNameServiceFixture
	{
		static string TblPrefix => "ZXE";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusCodeListAttributeName);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusCodeListAttributeName { ZXE_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZXE_Name = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZXE_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusCodeListAttributeName { ZXE_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZXE_Name = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZXE_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZXE_Name).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_AttributeName_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			CreateAttributeName(Now, "AA");
			CreateAttributeName(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZXE_Name).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_AttributeName_Data()
		{
			var p = repo.Create(() => new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "AA",
				ZXE_Description = "BB",
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZXE_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZXE_Name, Is.EqualTo(p.ZXE_Name));
			Assert.That(result.ZXE_Description, Is.EqualTo(p.ZXE_Description));
		}

		[Test]
		public void GetLatest_AttributeNames()
		{
			var p = repo.Create(() => new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "AA",
				ZXE_Description = "BB",
			});

			var p2 = repo.Create(() => new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "AAA",
				ZXE_Description = "BB",
			});

			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZXE_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p2.ZXE_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now).ToArray();
			var result = dataSets.ElementAt(1);
			Assert.That(result.ZXE_Name, Is.EqualTo(p2.ZXE_Name));
			Assert.That(result.ZXE_Description, Is.EqualTo(p2.ZXE_Description));

			result = dataSets.ElementAt(0);
			Assert.That(result.ZXE_Name, Is.EqualTo(p.ZXE_Name));
			Assert.That(result.ZXE_Description, Is.EqualTo(p.ZXE_Description));
		}

		[Test]
		public void GetLatest_AttributeName_Language_Data()
		{
			var p = repo.Create(() => new RefCusCodeListAttributeName
			{
				ZXE_PK = Guid.NewGuid(),
				ZXE_Name = "AA",
				ZXE_Description = "BB",
			});

			var typeLanguage = repo.Create(() => new RefCusCodeListAttributeNameLanguage()
			{
				ZXH_PK = Guid.NewGuid(),
				ZXH_ZX6_NKLanguage = "EN",
				ZXH_ZXE_CodeListAttributeName = p.ZXE_PK,
				ZXH_Description = "English"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZXE_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZXE_Name, Is.EqualTo(p.ZXE_Name));
			Assert.That(result.ZXE_Description, Is.EqualTo(p.ZXE_Description));

			Assert.That(result.RefCusCodeListAttributeNameLanguages != null);
			Assert.That(result.RefCusCodeListAttributeNameLanguages[0].ZXH_ZX6_NKLanguage, Is.EqualTo(typeLanguage.ZXH_ZX6_NKLanguage));
			Assert.That(result.RefCusCodeListAttributeNameLanguages[0].ZXH_Description, Is.EqualTo(typeLanguage.ZXH_Description));
		}

		RefCusCodeListAttributeName CreateAttributeName(DateTime dateTime, string code = null)
		{
			var result = repo.Create(() => new RefCusCodeListAttributeName { ZXE_PK = Guid.NewGuid(), ZXE_Name = code });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZXE_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusCodeListAttributeName> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusCodeListAttributeName> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusCodeListAttributeNameService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZXE_Name);
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
