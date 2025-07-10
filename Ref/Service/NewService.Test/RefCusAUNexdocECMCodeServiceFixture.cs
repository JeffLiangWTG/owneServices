using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	[TestFixture]
	class RefCusAUNexdocECMCodeServiceFixture
	{
		static string TblPrefix => "ZY5";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefCusAUNexdocECMCode);

		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD5", new[] { "BB", "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFD8", new[] { "CC" })]
		[TestCase("8F88FF88-4F59-4E42-A0FD-F62A482DFFDF", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefCusAUNexdocECMCode { ZY5_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFD7"), ZY5_CommodityCode = "BB" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZY5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefCusAUNexdocECMCode { ZY5_PK = new Guid("8F88FF88-4F59-4E42-A0FD-F62A482DFFDE"), ZY5_CommodityCode = "CC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZY5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK)));
			Assert.That(dataSets.Select(x => x.ZY5_CommodityCode).ToArray(), Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_RefCusAUNexdocECMCode_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateCountry(Now, "AA");
			var p2 = CreateCountry(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZY5_CommodityCode).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_RefCusAUNexdocECMCode_Data()
		{
			var p = repo.Create(() => new RefCusAUNexdocECMCode
			{
				ZY5_PK = Guid.NewGuid(),
				ZY5_CommodityCode = "CM1",
				ZY5_PreservationCode = "PC"
			});
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = p.ZY5_PK, RVC_DataSetId = DataSetId, RVC_LastUpdatedUTC = Now.AddDays(1), RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZY5_CommodityCode, Is.EqualTo(p.ZY5_CommodityCode));
			Assert.That(result.ZY5_PreservationCode, Is.EqualTo(p.ZY5_PreservationCode));
		}

		RefCusAUNexdocECMCode CreateCountry(DateTime dateTime, string code = "XX")
		{
			var result = repo.Create(() => new RefCusAUNexdocECMCode { ZY5_PK = Guid.NewGuid(), ZY5_CommodityCode = code, ZY5_PreservationCode = "PC" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZY5_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefCusAUNexdocECMCode> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefCusAUNexdocECMCode> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefCusAUNexdocECMCodeService(repo);
			return service.GetData(dateTime, runtime.HasValue ? runtime.Value : DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZY5_CommodityCode);
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
