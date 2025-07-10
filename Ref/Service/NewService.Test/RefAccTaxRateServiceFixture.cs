using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefAccTaxRateServiceFixture
	{
		static string TblPrefix => "ZAT";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefAccTaxRate);

		[TestCase("7B62B060-FDAD-4CE1-A57B-587B588E5172", new[] { "11", "22" })]
		[TestCase("D72776F8-5AE4-4366-89E3-0DB171761375", new[] { "22" })]
		[TestCase("DF88FF88-4F59-4E42-A0FD-F62A482DFFF1", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefAccTaxRate { ZAT_PK = new Guid("7B62B060-FDAD-4CE1-A57B-587B588E5173"), ZAT_ReferenceRateType = "11" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_ParentPK = dataset1.ZAT_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefAccTaxRate { ZAT_PK = new Guid("D72776F8-5AE4-4366-89E3-0DB171761376"), ZAT_ReferenceRateType = "22" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_ParentPK = dataset2.ZAT_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.ZAT_ReferenceRateType).ToArray();
			Assert.That(dataSets, Is.EqualTo(expected));
		}

		[TestCase(-1, 3, new[] { "AA", "BB" })]
		[TestCase(1, 3, new[] { "BB" })]
		[TestCase(3, 3, new string[0])]
		[TestCase(null, 3, new[] { "AA", "BB" })]
		[TestCase(null, 1, new[] { "AA" })]
		[TestCase(null, -1, new string[0])]
		public void GetLatest_Data_Filter(int? daysOffset, int rtOffset, string[] expected)
		{
			var p1 = CreateData(Now, "AA");
			var p2 = CreateData(Now.AddDays(2), "BB");
			var dataSets = GetDataSets(daysOffset, rtOffset);
			Assert.That(dataSets.Select(x => x.ZAT_ReferenceRateType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "this is the rate type");
			p.ZAT_RN_NKCountry = "CN";
			p.ZAT_StartDate = Now.AddDays(-1);
			p.ZAT_EndDate = Now;
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZAT_ReferenceRateType, Is.EqualTo("this is the rate type"));
			Assert.That(result.ZAT_RN_NKCountry, Is.EqualTo("CN"));
			Assert.That(result.ZAT_StartDate, Is.EqualTo(Now.AddDays(-1)));
			Assert.That(result.ZAT_EndDate, Is.EqualTo(Now));
		}

		RefAccTaxRate CreateData(DateTime dateTime, string rateType = null)
		{
			var result = repo.Create(() => new RefAccTaxRate { ZAT_PK = Guid.NewGuid(), ZAT_ReferenceRateType = rateType });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_ParentPK = result.ZAT_PK, RVC_ParentCode = TblPrefix, RVC_DataSetId = DataSetId, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefAccTaxRate> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefAccTaxRate> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefAccTaxRateService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZAT_ReferenceRateType);
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
