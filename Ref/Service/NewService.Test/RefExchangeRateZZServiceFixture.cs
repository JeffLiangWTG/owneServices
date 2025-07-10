using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using NUnit.Framework;
using Models = CargoWise.RefDbRepo.Common.Contract_0_9;

namespace CargoWise.RefDbRepo.NewService.Test
{
	class RefExchangeRateZZServiceFixture
	{
		static string TblPrefix => "ZZN";
		static short DataSetId => Helper.GetDataSetId(DataSet.RefExchangeRateZZ);

		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E75", new string[] { "11", "22" })]
		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E78", new string[] { "22" })]
		[TestCase("1746513B-85ED-4030-BFFA-20CADBA34E7F", new string[0])]
		public void FilterWithLastDataSet(string checkpointPK, string[] expected)
		{
			var dataset1 = repo.Create(() => new RefExchangeRateZZ { ZZN_PK = new Guid("1746513B-85ED-4030-BFFA-20CADBA34E77"), ZZN_ExRateType = "11" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset1.ZZN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataset2 = repo.Create(() => new RefExchangeRateZZ { ZZN_PK = new Guid("1746513B-85ED-4030-BFFA-20CADBA34E7E"), ZZN_ExRateType = "22" });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = Now, RVC_DataSetId = DataSetId, RVC_ParentPK = dataset2.ZZN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			var dataSets = GetDataSets(null, Now, CheckpointHelper.Create(new Guid(checkpointPK))).Select(x => x.ZZN_ExRateType).ToArray();
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
			Assert.That(dataSets.Select(x => x.ZZN_ExRateType).ToArray(), Is.EqualTo(expected));
		}

		[Test]
		public void GetLatest_Data()
		{
			var p = CreateData(Now.AddDays(1), "this is the rate type");
			p.ZZN_Rate = 1;
			p.ZZN_Rate = 1;
			p.ZZN_StartDate = Now.AddDays(-1);
			p.ZZN_EndDate = Now;
			p.ZZN_AsPublished = "T";
			var dataSets = GetDataSets(Now);
			var result = dataSets.ElementAt(0);
			Assert.That(result.ZZN_ExRateType, Is.EqualTo("this is the rate type"));
			Assert.That(result.ZZN_Rate, Is.EqualTo(1));
			Assert.That(result.ZZN_StartDate, Is.EqualTo(Now.AddDays(-1)));
			Assert.That(result.ZZN_EndDate, Is.EqualTo(Now));
			Assert.That(result.ZZN_AsPublished, Is.EqualTo("T"));
		}

		RefExchangeRateZZ CreateData(DateTime dateTime, string type = null)
		{
			var result = repo.Create(() => new RefExchangeRateZZ { ZZN_PK = Guid.NewGuid(), ZZN_ExRateType = type });
			repo.Create(() => new RefDbVersionControl { RVC_LastUpdatedUTC = dateTime, RVC_DataSetId = DataSetId, RVC_ParentPK = result.ZZN_PK, RVC_ParentCode = TblPrefix, RVC_IsPublished = true });
			return result;
		}

		IEnumerable<Models.RefExchangeRateZZ> GetDataSets(int? daysOffSet, int runtimeDaysOffSet, ICheckpoint checkpoint = null)
		{
			return GetDataSets(daysOffSet.HasValue ? (DateTime?)Now.AddDays(daysOffSet.Value) : null, Now.AddDays(runtimeDaysOffSet), checkpoint);
		}

		IEnumerable<Models.RefExchangeRateZZ> GetDataSets(DateTime? dateTime, DateTime? runtime = null, ICheckpoint checkpoint = null)
		{
			var service = new RefExchangeRateZZService(repo);
			return service.GetData(dateTime, runtime ?? DateTime.UtcNow.AddDays(100), checkpoint, null, DataSetId).OrderBy(x => x.ZZN_ExRateType);
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
